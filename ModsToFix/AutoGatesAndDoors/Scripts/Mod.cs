using PugMod;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class AutoGatesAndDoorsMod : IMod
{
    public const string MOD_NAME = "AutoGatesAndDoors";
    public const string MOD_VERSION = "0.0.57";
    private LoadedMod _modInfo;

    public void EarlyInit()
    {
        Debug.Log($"[{MOD_NAME}]: Mod version: {MOD_VERSION}");
        _modInfo = GetModInfo(this);
        if (_modInfo == null)
        {
            Debug.Log($"[{MOD_NAME}]: Failed to load {MOD_NAME}: mod metadata not found!");
            return;
        }

        Debug.Log($"[{MOD_NAME}]: Mod loaded successfully");
    }

    public static LoadedMod GetModInfo(IMod mod)
    {
        return API.ModLoader.LoadedMods.FirstOrDefault(modInfo => modInfo.Handlers.Contains(mod));
    }

    public void Init() { }
    public void Shutdown() { }
    public void ModObjectLoaded(UnityEngine.Object obj) { }
    public void Update() { }
}

internal static class AutoGateDoorFilters
{
    private static readonly HashSet<ObjectID> ExcludedAutoToggleObjectIds = BuildExcludedAutoToggleObjectIds();

    private static HashSet<ObjectID> BuildExcludedAutoToggleObjectIds()
    {
        var excluded = new HashSet<ObjectID>();

        foreach (ObjectID objectId in Enum.GetValues(typeof(ObjectID)))
        {
            if (objectId == ObjectID.None)
                continue;

            var name = objectId.ToString();
            var isElectric = name.IndexOf("electric", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             name.IndexOf("electrical", StringComparison.OrdinalIgnoreCase) >= 0;
            var isDoorOrGate = name.IndexOf("door", StringComparison.OrdinalIgnoreCase) >= 0 ||
                               name.IndexOf("gate", StringComparison.OrdinalIgnoreCase) >= 0;

            if (isElectric && isDoorOrGate)
                excluded.Add(objectId);
        }

        return excluded;
    }

    public static bool ShouldSkipAutoToggle(ObjectDataCD objectData)
    {
        return ExcludedAutoToggleObjectIds.Contains(objectData.objectID);
    }
}

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.LocalSimulation)]
public partial class ThresholdGhostRelay : PugSimulationSystemBase
{
    private const float TriggerDistanceSq = 0.95f;
    private EntityQuery _switchableDoorsQuery;
    private EntityQuery _switchingQueuesQuery;

    protected override void OnCreate()
    {
        base.OnCreate();

        _switchableDoorsQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadWrite<ObjectDataCD>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<GhostInstance>(),
                ComponentType.ReadOnly<DoorCD>()
            },
            None = new[]
            {
                ComponentType.ReadOnly<SwitchPredictionSmoothing>()
            }
        });

        _switchingQueuesQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadWrite<GhostPredictionSwitchingQueues>()
            },
            Options = EntityQueryOptions.IncludeSystems
        });

        RequireForUpdate(_switchableDoorsQuery);
        RequireForUpdate(_switchingQueuesQuery);
    }

    protected override void OnUpdate()
    {
        if (Manager.main?.player == null)
        {
            base.OnUpdate();
            return;
        }

        var playerPosition = (float3)Manager.main.player.WorldPosition;
        var switchingQueues = _switchingQueuesQuery.GetSingletonRW<GhostPredictionSwitchingQueues>().ValueRW;

        using var entities = _switchableDoorsQuery.ToEntityArray(Allocator.Temp);
        using var transforms = _switchableDoorsQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        using var ghostInstances = _switchableDoorsQuery.ToComponentDataArray<GhostInstance>(Allocator.Temp);
        using var objectDatas = _switchableDoorsQuery.ToComponentDataArray<ObjectDataCD>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            if (ghostInstances[i].ghostType < 0)
                continue;

            if (AutoGateDoorFilters.ShouldSkipAutoToggle(objectDatas[i]))
                continue;

            var playerNearby = math.distancesq(transforms[i].Position, playerPosition) <= TriggerDistanceSq;
            var isPredicted = EntityManager.HasComponent<PredictedGhost>(entities[i]);

            if (playerNearby && !isPredicted)
            {
                switchingQueues.ConvertToPredictedQueue.Enqueue(new ConvertPredictionEntry
                {
                    TargetEntity = entities[i],
                    TransitionDurationSeconds = 1f,
                });
            }
            else if (!playerNearby && isPredicted)
            {
                switchingQueues.ConvertToInterpolatedQueue.Enqueue(new ConvertPredictionEntry
                {
                    TargetEntity = entities[i],
                    TransitionDurationSeconds = 1f,
                });
            }
        }

        base.OnUpdate();
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation | WorldSystemFilterFlags.LocalSimulation)]
public partial class ProximityLatchCoordinator : PugSimulationSystemBase
{
    private const float TriggerDistanceSq = 0.95f;
    private EntityQuery _playerQuery;
    private EntityQuery _gateQuery;
    private EntityQuery _nonElectricDoorQuery;

    protected override void OnCreate()
    {
        base.OnCreate();
        Debug.Log($"[{AutoGatesAndDoorsMod.MOD_NAME}] AutoGatesAndDoors systems active (v{AutoGatesAndDoorsMod.MOD_VERSION})");

        _playerQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadOnly<PlayerGhost>(),
                ComponentType.ReadOnly<LocalTransform>()
            }
        });

        _gateQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadWrite<ObjectDataCD>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<PredictedGhost>(),
                ComponentType.ReadOnly<Simulate>()
            },
            Any = new[]
            {
                ComponentType.ReadOnly<GateCD>()
            }
        });

        _nonElectricDoorQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadWrite<ObjectDataCD>(),
                ComponentType.ReadOnly<LocalTransform>(),
                ComponentType.ReadOnly<PredictedGhost>(),
                ComponentType.ReadOnly<Simulate>(),
                ComponentType.ReadOnly<DoorCD>()
            },
            None = new[]
            {
                ComponentType.ReadOnly<GateCD>()
            }
        });

        RequireForUpdate(_playerQuery);
        RequireForUpdate(GetEntityQuery(new EntityQueryDesc
        {
            Any = new[]
            {
                ComponentType.ReadOnly<GateCD>(),
                ComponentType.ReadOnly<DoorCD>()
            }
        }));
    }

    private static void SetOpen(ref ObjectDataCD objectData, bool open)
    {
        var variation = objectData.variation;
        var isOpen = (variation & 1) == 1;

        if (open && !isOpen)
            objectData.variation = variation + 1;
        else if (!open && isOpen)
            objectData.variation = variation - 1;
    }

    protected override void OnUpdate()
    {
        using var playerTransforms = _playerQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        ApplyAutoToggle(_gateQuery, playerTransforms);
        ApplyAutoToggle(_nonElectricDoorQuery, playerTransforms);

        base.OnUpdate();
    }

    private void ApplyAutoToggle(EntityQuery query, NativeArray<LocalTransform> playerTransforms)
    {
        using var entities = query.ToEntityArray(Allocator.Temp);
        using var objectDatas = query.ToComponentDataArray<ObjectDataCD>(Allocator.Temp);
        using var transforms = query.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            var anyPlayerNearby = false;

            for (var p = 0; p < playerTransforms.Length; p++)
            {
                if (math.distancesq(transforms[i].Position, playerTransforms[p].Position) <= TriggerDistanceSq)
                {
                    anyPlayerNearby = true;
                    break;
                }
            }

            var objectData = objectDatas[i];
            if (AutoGateDoorFilters.ShouldSkipAutoToggle(objectData))
                continue;

            var oldVariation = objectData.variation;
            SetOpen(ref objectData, anyPlayerNearby);

            if (objectData.variation != oldVariation)
                EntityManager.SetComponentData(entities[i], objectData);
        }
    }
}

























