using PugMod;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class ExperienceTweaks : IMod
{
    public const string VERSION = "1.0";
    public const string NAME = "ExperienceTweaks";
    public const string Author = "Ninakoru";

    private LoadedMod modInfo;

    private Dictionary<Unity.Entities.Hash128, Dictionary<SkillID, int>> xpCache = new();

    private Dictionary<Unity.Entities.Hash128, EquippedItemInfo> lastEquippedObjects = new();

    private HashSet<Unity.Entities.Hash128> activeGuids;

    private const int UPDATE_INTERVAL = 15;
    private const float INVALID_COOLDOWN = -1f;
    private const float XP_FORMULA_DIVISOR = 0.3f;
    private const int XP_FORMULA_CAP = 6;

    private int updateCounter = 0;

    private struct EquippedItemInfo
    {
        public ObjectID EquippedObjectId;
        public float Cooldown;
        public int SkillMultiplier;
        public Dictionary<SkillID, bool> ValidForXp;
        public bool HasChanged;
    }

    private static readonly SkillID[] SkillsHandled =
    {
        SkillID.Melee,
        SkillID.Range,
        SkillID.Magic,
        SkillID.Mining
    };


    public static LoadedMod GetModInfo(IMod mod)
    {
        return API.ModLoader.LoadedMods.FirstOrDefault(modInfo => modInfo.Handlers.Contains(mod));
    }

    public void EarlyInit()
    {
        Debug.Log($"[{NAME}]: Mod version: {VERSION}");
        modInfo = GetModInfo(this);
        if (modInfo == null)
        {
            Debug.Log($"[{NAME}]: Failed to load {NAME}: mod metadata not found!");
            return;
        }

        Debug.Log($"[{NAME}]: Mod loaded successfully");
    }

    public void Init()
    {
    }

    public void ModObjectLoaded(Object obj)
    {
    }

    public void Shutdown()
    {
    }

    public void Update()
    {
        if (API.Server == null || API.Server.World == null)
        {
            return; // no world yet, skip update
        }

        var topMenu = Manager.menu.GetTopMenu();
        if ((topMenu != null && topMenu.pausesGame) || Manager.load.IsLoading())
        {
            return; // in pause
        }

        updateCounter++;
        if (updateCounter % UPDATE_INTERVAL != 0)
        {
            return;
        }
        updateCounter = 0; // reset safeguard

        EntityManager em = API.Server.World.EntityManager;

        // Build query for all player entities that have a GUID and skill buffer
        using (EntityQuery entityQuery = em.CreateEntityQuery(new EntityQueryBuilder(Allocator.Temp).WithAll<PlayerGuidCD>().WithAllRW<SkillBuffer>()))
        {
            var players = entityQuery.ToEntityArray(Allocator.Temp);

            // Important to reset each pass.
            activeGuids = new();

            foreach (var playerEntity in players)
            {
                var currentPlayer = em.GetComponentData<PlayerGuidCD>(playerEntity);

                if (!currentPlayer.IsCreated)
                {
                    continue;
                }

                Unity.Entities.Hash128 playerGuid = currentPlayer.Value;

                BuildOrCheckForSkillData(playerGuid);

                EquippedItemInfo currentEquippedItemInfo = GetEquippedItemInfo(em, playerEntity, playerGuid);

                var skillBuffer = em.GetBuffer<SkillBuffer>(playerEntity);

                foreach (SkillID handledSkill in SkillsHandled)
                {
                    var bonusXp = ManageExperienceUpdate(playerGuid, skillBuffer, currentEquippedItemInfo, handledSkill);
                    if (bonusXp > 0)
                    {
                        GrantExtraXP(em, playerEntity, handledSkill, bonusXp);
                    }
                }
            }

            CleanNotActivePlayers();

            players.Dispose();
        }
    }

    private int ManageExperienceUpdate(Unity.Entities.Hash128 playerGuid, DynamicBuffer<SkillBuffer> skillBuffer, EquippedItemInfo currentEquippedItemInfo, SkillID skillTocheck)
    {
        int bonusXp = 0;
        var skillXp = skillBuffer.ElementAt((int)skillTocheck).Value;
        var storedSkillXp = xpCache[playerGuid][skillTocheck];

        if (storedSkillXp == -1)
        {
            // First time setup
            xpCache[playerGuid][skillTocheck] = skillXp;
        }
        else if (skillXp > storedSkillXp)
        {
            // skill XP has changed.
            int effectiveHits = (skillXp - storedSkillXp) / currentEquippedItemInfo.SkillMultiplier;
            int newXp = skillXp; // base update

            // Only grant bonus if equipped item didn't change and is valid for XP, and later the multiplier is greater than 1.
            if (!currentEquippedItemInfo.HasChanged && currentEquippedItemInfo.ValidForXp[skillTocheck])
            {
                int multiplier = GetXpMultiplier(currentEquippedItemInfo.Cooldown);
                if (multiplier > 1)
                {
                    // Delta Bonus to apply
                    bonusXp = effectiveHits * (multiplier - currentEquippedItemInfo.SkillMultiplier);
                    newXp += bonusXp; // apply bonus once
                }
            }


            // Single cache update
            xpCache[playerGuid][skillTocheck] = newXp;
        }

        return bonusXp;
    }

    // Method to add xp through bursted system.
    private void GrantExtraXP(EntityManager em, Entity playerEntity, SkillID skillID, int amount)
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        AddSkill(playerEntity, skillID, amount, ecb, true);
        ecb.Playback(em);
        ecb.Dispose();
    }

    // Setups the initial dictionaty data, to avoid checking in the rest of the program.
    private void BuildOrCheckForSkillData(Unity.Entities.Hash128 playerGuid)
    {
        if (!playerGuid.IsValid)
            return;

        if (!xpCache.ContainsKey(playerGuid))
        {
            var skillMap = new Dictionary<SkillID, int>();
            foreach (SkillID skill in SkillsHandled)
            {
                skillMap[skill] = -1; // initialize with sentinel
            }
            xpCache.Add(playerGuid, skillMap);
        }

        if (!lastEquippedObjects.ContainsKey(playerGuid))
        {
            lastEquippedObjects[playerGuid] = DefaultEquippedItemInfo();
        }

        activeGuids.Add(playerGuid);
    }

    // Prunes not active players to avoid coming back and getting huge xp boosts
    private void CleanNotActivePlayers()
    {
        var staleGuids = xpCache.Keys
            .Concat(lastEquippedObjects.Keys)
            .Except(activeGuids)
            .Distinct()
            .ToList();

        foreach (var guid in staleGuids)
        {
            xpCache.Remove(guid);
            lastEquippedObjects.Remove(guid);
        }
    }

    private EquippedItemInfo DefaultEquippedItemInfo()
    {
        var validForXp = new Dictionary<SkillID, bool>();
        foreach (SkillID skill in SkillsHandled)
        {
            validForXp[skill] = false;
        }

        return new EquippedItemInfo
        {
            HasChanged = true,
            EquippedObjectId = ObjectID.None,
            Cooldown = INVALID_COOLDOWN,
            SkillMultiplier = 1,
            ValidForXp = validForXp
        };
    }

    // Gets all relevant information from equipped item
    private EquippedItemInfo GetEquippedItemInfo(EntityManager em, Entity playerEntity, Unity.Entities.Hash128 playerGuid)
    {
        if (!em.HasComponent<EquippedObjectCD>(playerEntity))
        {
            return DefaultEquippedItemInfo();
        }

        var equipped = em.GetComponentData<EquippedObjectCD>(playerEntity);
        var equippedPrefab = equipped.equipmentPrefab;
        var equippedObjectId = equipped.containedObject.objectID;

        // If we already have info and object hasn't changed → reuse
        if (lastEquippedObjects[playerGuid].EquippedObjectId == equippedObjectId)
        {
            EquippedItemInfo sameEquippedItemInfo = lastEquippedObjects[playerGuid];
            sameEquippedItemInfo.HasChanged = false;
            lastEquippedObjects[playerGuid] = sameEquippedItemInfo;
            return sameEquippedItemInfo;
        }
        // Otherwise rebuild
        var info = DefaultEquippedItemInfo();
        info.EquippedObjectId = equippedObjectId;

        if (em.HasComponent<CooldownCD>(equippedPrefab))
        {
            info.Cooldown = em.GetComponentData<CooldownCD>(equippedPrefab).cooldown;
        }

        if (info.Cooldown != INVALID_COOLDOWN)
        {
            ObjectID projectileObject = ObjectID.None;
            if (em.HasComponent<RangeWeaponCD>(equippedPrefab))
            { 
                projectileObject = em.GetComponentData<RangeWeaponCD>(equippedPrefab).projectileID;
                if (projectileObject != ObjectID.None)
                {
                    if (PugDatabase.TryGetComponent<WeaponSkillMultiplierCD>(projectileObject, out WeaponSkillMultiplierCD projectileMulti))
                    {
                        if (projectileMulti.skillMultiplier > 1)
                        {
                            info.SkillMultiplier = (int)projectileMulti.skillMultiplier;
                        }
                    }
                }
            }

            if (projectileObject == ObjectID.None && PugDatabase.TryGetComponent<WeaponSkillMultiplierCD>(equippedObjectId, out WeaponSkillMultiplierCD weaponMulti))
            {
                if (weaponMulti.skillMultiplier > 1)
                {
                    info.SkillMultiplier = (int)weaponMulti.skillMultiplier;
                }
            }

            ObjectInfo objectInfo = PugDatabase.GetObjectInfo(equippedObjectId, 0);
            if (objectInfo != null)
            {
                info.ValidForXp[SkillID.Mining] =
                    objectInfo.objectType == ObjectType.MiningPick ||
                    objectInfo.objectType == ObjectType.DrillTool ||
                    objectInfo.objectType == ObjectType.Sledge ||
                    objectInfo.objectType == ObjectType.BeamWeapon;
            }

            if (em.HasComponent<HasWeaponDamageCD>(equippedPrefab))
            {
                var damage = em.GetComponentData<HasWeaponDamageCD>(equippedPrefab);
                info.ValidForXp[SkillID.Melee] = !damage.isRange && !damage.isMagic;
                info.ValidForXp[SkillID.Range] = damage.isRange && !damage.isMagic;
                info.ValidForXp[SkillID.Magic] = damage.isMagic;
            }
        }

        // Cache new info
        lastEquippedObjects[playerGuid] = info;
        return info;
    }

    // Simple function to determine bonus xp based on cooldown
    private int GetXpMultiplier(float cooldown)
    {
        // Safety check: invalid or non-positive input
        if (float.IsNaN(cooldown) || cooldown <= 0f)
        {
            return 1;
        }

        // Compute ratio and round up
        int rounded = (int)math.ceil(cooldown / XP_FORMULA_DIVISOR);

        // Cap at max multiplier
        return math.min(rounded, XP_FORMULA_CAP);
    }


    // Mimic the game’s AddSkill method
    public static void AddSkill(Entity entity, SkillID skillID, int amount, EntityCommandBuffer ecb, bool isServer)
    {
        if (!isServer) return;

        Entity e = ecb.CreateEntity();
        ecb.AddComponent(e, new AddSkillValueCD
        {
            entity = entity,
            skillID = skillID,
            amount = amount
        });
    }
}
