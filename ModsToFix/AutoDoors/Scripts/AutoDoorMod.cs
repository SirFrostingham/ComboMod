using PugMod;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class AutoDoorMod : IMod
{
    public const string MOD_NAME = "AutoDoors";
    public const string MOD_VERSION = "1.1.9";
    private LoadedMod _modInfo;
    public void EarlyInit()
    {
        Debug.Log($"Loading mod {MOD_NAME} version {MOD_VERSION}...");

        Debug.Log($"Finished loading mod {MOD_NAME} v{MOD_VERSION}");
    }

    public void Init()
    {
        //throw new System.NotImplementedException();
    }

    public void Shutdown()
    {
        //throw new System.NotImplementedException();
    }

    public void ModObjectLoaded(Object obj)
    {
        //throw new System.NotImplementedException();
    }

    public void Update()
    {
        //throw new System.NotImplementedException();
    }
}

[UpdateInGroup(typeof(SimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
public partial class DoorGateAutoOpenSystem : PugSimulationSystemBase
{
    private const float TriggerDistance = 1.5f;
    private const float TriggerDistanceSq = TriggerDistance * TriggerDistance;
    private EntityQuery _doorGateQuery;

    protected override void OnCreate()
    {
        base.OnCreate();
        Debug.Log($"[{AutoDoorMod.MOD_NAME}] DoorGateAutoOpenSystem active (v{AutoDoorMod.MOD_VERSION})");

        _doorGateQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new[]
            {
                ComponentType.ReadWrite<ObjectDataCD>(),
                ComponentType.ReadOnly<LocalTransform>()
            },
            Any = new[]
            {
                ComponentType.ReadOnly<DoorCD>(),
                ComponentType.ReadOnly<GateCD>()
            },
            None = new[]
            {
                ComponentType.ReadOnly<EntityDestroyedCD>()
            },
            Options = EntityQueryOptions.IncludeDisabledEntities
        });

        RequireForUpdate(_doorGateQuery);
    }

    static void SetOpen(ref ObjectDataCD objectData, bool open)
    {
        // Most door/gate variants use even=closed and odd=open pairings.
        var variation = objectData.variation;
        var isOpen = (variation & 1) == 1;

        if (open && !isOpen)
            objectData.variation = variation + 1;
        else if (!open && isOpen)
            objectData.variation = variation - 1;
    }

    protected override void OnUpdate()
    {
        var hasManagerPlayer = Manager.main != null && Manager.main.player != null;
        float3 managerPlayerPosition = float3.zero;
        if (hasManagerPlayer)
        {
            managerPlayerPosition = Manager.main.player.WorldPosition;
        }

        using var entities = _doorGateQuery.ToEntityArray(Allocator.Temp);
        using var doorTransforms = _doorGateQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);
        using var doorObjectDatas = _doorGateQuery.ToComponentDataArray<ObjectDataCD>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            var anyPlayerNearby = false;

            if (EntityManager.HasComponent<DistanceToPlayerCD>(entities[i]))
            {
                var distanceToPlayer = EntityManager.GetComponentData<DistanceToPlayerCD>(entities[i]).minDistanceSq;
                anyPlayerNearby = distanceToPlayer > 0f && distanceToPlayer <= TriggerDistanceSq;
            }
            else if (hasManagerPlayer)
            {
                var distance = math.distancesq(doorTransforms[i].Position, managerPlayerPosition);
                anyPlayerNearby = distance <= TriggerDistanceSq;
            }

            var objectData = doorObjectDatas[i];
            var oldVariation = objectData.variation;
            SetOpen(ref objectData, anyPlayerNearby);

            if (objectData.variation != oldVariation)
            {
                EntityManager.SetComponentData(entities[i], objectData);
            }
        }

        base.OnUpdate();
    }
}
