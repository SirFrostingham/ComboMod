using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using UnityEngine;

namespace KeepInventoryOnDeath
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(UpdateHealthSystemGroup))]
    [UpdateBefore(typeof(InitMoveInventorySystem))]
    public partial class KeepInventorySystem : SystemBase
    {
        private EntityQuery graveQuery;
        private int preventedCount;
        private float logTimer;
        private const float LogInterval = 300.0f;

        protected override void OnCreate()
        {
            base.OnCreate();
            preventedCount = 0;

            graveQuery = GetEntityQuery(
                ComponentType.ReadOnly<PlayerGraveCD>(),
                ComponentType.ReadWrite<InitialMoveInventoryFromCD>()
            );

            Debug.Log("[KeepInventory] Mod active. Inventory will be kept on death.");
        }

        protected override void OnUpdate()
        {
            if (graveQuery.IsEmptyIgnoreFilter) return;

            float dt = World.Time.DeltaTime;
            logTimer += dt;

            PreventInventoryDrop();

            if (logTimer >= LogInterval)
            {
                logTimer = 0f;
                if (preventedCount > 0)
                {
                    Debug.Log($"[KeepInventory] {preventedCount} inventory drops prevented since start.");
                }
            }
        }

        private void PreventInventoryDrop()
        {
            var entities = graveQuery.ToEntityArray(Allocator.Temp);
            var moveData = graveQuery.ToComponentDataArray<InitialMoveInventoryFromCD>(Allocator.Temp);

            for (int i = 0; i < entities.Length; i++)
            {
                var entity = entities[i];
                var data = moveData[i];

                // Only process if not yet neutralized
                if (data.entityFrom != Entity.Null)
                {
                    data.entityFrom = Entity.Null;
                    EntityManager.SetComponentData(entity, data);
                    EntityManager.SetComponentEnabled<InitialMoveInventoryFromCD>(entity, false);

                    preventedCount++;
                    Debug.Log($"[KeepInventory] Inventory drop prevented for grave [ID: {entity.Index}].");
                }
            }

            entities.Dispose();
            moveData.Dispose();
        }
    }
}
