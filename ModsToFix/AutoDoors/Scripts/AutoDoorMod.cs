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
    public const string MOD_VERSION = "1.1.2";
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
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class AutoDoorSystem : PugSimulationSystemBase
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
        var playerPositions = new NativeList<float3>(World.UpdateAllocator.ToAllocator);

        Entities
            .WithAll<PlayerGhost>()
            .ForEach((in LocalTransform translation) =>
            {
                playerPositions.Add(translation.Position);
            })
            .Schedule();

        Entities
            .WithAll<Simulate>()
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
            .WithDisposeOnCompletion(playerPositions)
            .Schedule();

        base.OnUpdate();
    }
}
