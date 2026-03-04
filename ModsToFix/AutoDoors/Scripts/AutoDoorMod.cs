using PugMod;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

public class AutoDoorMod : IMod
{
    public const string MOD_NAME = "AutoDoors";
    public const string MOD_VERSION = "1.1.3";
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
public partial class DoorSwitchSystem : PugSimulationSystemBase
{
    private const float TriggerDistance = 1.5f;
    private const float TriggerDistanceSq = TriggerDistance * TriggerDistance;

    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<GhostPredictionSwitchingQueues>();
    }

    protected override void OnUpdate()
    {
        if (Manager.main.player == null)
        {
            base.OnUpdate();
            return;
        }

        var playerPosition = Manager.main.player.WorldPosition;
        var switchingQueues = SystemAPI.GetSingletonRW<GhostPredictionSwitchingQueues>().ValueRW;

        Entities
            .WithAny<DoorCD, GateCD>()
            .WithNone<SwitchPredictionSmoothing, EntityDestroyedCD>()
            .ForEach((Entity entity, in LocalTransform translation, in GhostInstance ghostInstance) =>
            {
                if (ghostInstance.ghostType < 0) return;

                var distance = math.distancesq(translation.Position, playerPosition);
                var playerNearby = distance <= TriggerDistanceSq;
                var isPredicted = SystemAPI.HasComponent<PredictedGhost>(entity);

                if (playerNearby && !isPredicted)
                {
                    switchingQueues.ConvertToPredictedQueue.Enqueue(new ConvertPredictionEntry
                    {
                        TargetEntity = entity,
                        TransitionDurationSeconds = 1f,
                    });
                    return;
                }

                if (!playerNearby && isPredicted)
                {
                    switchingQueues.ConvertToInterpolatedQueue.Enqueue(new ConvertPredictionEntry
                    {
                        TargetEntity = entity,
                        TransitionDurationSeconds = 1f,
                    });
                }
            })
            .WithoutBurst()
            .Run();

        base.OnUpdate();
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial class DoorGateStateChecker : PugSimulationSystemBase
{
    private const float TriggerDistance = 1.5f;
    private const float TriggerDistanceSq = TriggerDistance * TriggerDistance;

    static void SetOpen(ref ObjectDataCD objectData, bool open)
    {
        if (open)
        {
            objectData.variation = objectData.variation switch
            {
                0 => 1,
                2 => 3,
                _ => objectData.variation
            };
        }
        else
        {
            objectData.variation = objectData.variation switch
            {
                1 => 0,
                3 => 2,
                _ => objectData.variation
            };
        }
    }

    protected override void OnUpdate()
    {
        var playerPositions = new NativeList<float3>(Allocator.Temp);

        Entities
            .WithAll<PlayerGhost>()
            .ForEach((in LocalTransform translation) =>
            {
                playerPositions.Add(translation.Position);
            })
            .Run();

        Entities
            .WithAll<PredictedGhost, Simulate>()
            .WithAny<DoorCD, GateCD>()
            .WithNone<EntityDestroyedCD>()
            .ForEach((ref ObjectDataCD objectData, in LocalTransform translation) =>
            {
                var anyPlayerNearby = false;
                foreach (var playerPos in playerPositions)
                {
                    var distance = math.distancesq(translation.Position, playerPos);
                    if (distance > TriggerDistanceSq) continue;
                    anyPlayerNearby = true;
                    break;
                }

                SetOpen(ref objectData, anyPlayerNearby);
            })
            .WithoutBurst()
            .Run();

        playerPositions.Dispose();

        base.OnUpdate();
    }
}
