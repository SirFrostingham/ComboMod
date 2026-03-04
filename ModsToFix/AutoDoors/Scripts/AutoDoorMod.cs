using System.Collections;
using System.Collections.Generic;
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
    public const string MOD_VERSION = "1.1.4";
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

public partial class DoorSwitchSystem : PugSimulationSystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        RequireForUpdate<GhostPredictionSwitchingQueues>();
    }

    protected override void OnUpdate()
    {
        if (Manager.main.player == null) return;

        const float triggerDistance = 1.5f;
        var triggerDistanceSq = triggerDistance * triggerDistance;

        var playerPosition = Manager.main.player.WorldPosition;

        var switchingQueues = SystemAPI.GetSingletonRW<GhostPredictionSwitchingQueues>().ValueRW;

        Entities.WithAll<DoorCD>().ForEach((
                Entity entity,
                ref ObjectDataCD objectData,
                in LocalTransform translation,
                in GhostInstance ghostInstance) =>
            {
                if (ghostInstance.ghostType < 0) return;
                var distance = math.distancesq(translation.Position, playerPosition);
                var playerNearby = distance <= triggerDistanceSq;

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
                    return;
                }
            })
            .WithNone<SwitchPredictionSmoothing>()
            .WithoutBurst()
            .Schedule();

        base.OnUpdate();
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial class DoorGateStateChecker : PugSimulationSystemBase
{
    static void SetOpen(ref ObjectDataCD objectData, bool open)
    {
        // if we should open the door
        if (open)
        {
            objectData.variation = objectData.variation switch
            {
                0 => 1,
                2 => 3,
                _ => objectData.variation
            };
        }
        // if we should close the door
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
        const float triggerDistance = 1.5f;
        var triggerDistanceSq = triggerDistance * triggerDistance;

        // get and store player positions
        var playerPositions = new NativeList<float3>(World.UpdateAllocator.ToAllocator);
        Entities
            .WithAll<PlayerGhost>().ForEach((in LocalTransform translation) =>
            {
                playerPositions.Add(translation.Position);
            })
            .Schedule();

        Entities
            .WithAll<PredictedGhost, Simulate, DoorCD>()
            .ForEach((ref ObjectDataCD objectData, in LocalTransform translation) =>
            {
                var anyPlayerNearby = false;
                foreach (var playerPos in playerPositions)
                {
                    var distance = math.distancesq(translation.Position, playerPos);
                    if (distance > triggerDistanceSq) continue;
                    anyPlayerNearby = true;
                    break;
                }

                SetOpen(ref objectData, anyPlayerNearby);
            })
            .WithoutBurst()
            .WithDisposeOnCompletion(playerPositions)
            .Schedule();

        base.OnUpdate();
    }
}
