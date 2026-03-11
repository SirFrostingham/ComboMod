using PugMod;
using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class AutoGatesAndDoorsMod : IMod
{
    public const string MOD_NAME = "AutoGateAndDoors";
    public const string MOD_VERSION = "1.1.11";
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
    public void ModObjectLoaded(Object obj) { }
    public void Update() { }
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

        for (var i = 0; i < entities.Length; i++)
        {
            if (ghostInstances[i].ghostType < 0)
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
    private EntityQuery _doorGateQuery;

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

        _doorGateQuery = GetEntityQuery(new EntityQueryDesc
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
                ComponentType.ReadOnly<DoorCD>(),
                ComponentType.ReadOnly<GateCD>()
            }
        });

        RequireForUpdate(_playerQuery);
        RequireForUpdate(_doorGateQuery);
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
        using var entities = _doorGateQuery.ToEntityArray(Allocator.Temp);
        using var objectDatas = _doorGateQuery.ToComponentDataArray<ObjectDataCD>(Allocator.Temp);
        using var doorTransforms = _doorGateQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp);

        for (var i = 0; i < entities.Length; i++)
        {
            if (EntityManager.HasComponent<ElectricalDoor>(entities[i])
                || EntityManager.HasComponent<ElectricalDropGate>(entities[i]))
                continue;

            var anyPlayerNearby = false;

            for (var p = 0; p < playerTransforms.Length; p++)
            {
                if (math.distancesq(doorTransforms[i].Position, playerTransforms[p].Position) <= TriggerDistanceSq)
                {
                    anyPlayerNearby = true;
                    break;
                }
            }

            var objectData = objectDatas[i];
            var oldVariation = objectData.variation;
            SetOpen(ref objectData, anyPlayerNearby);

            if (objectData.variation != oldVariation)
                EntityManager.SetComponentData(entities[i], objectData);
        }

        base.OnUpdate();
    }
}