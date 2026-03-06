using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace InfiniteOreBoulder
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class InfiniteBoulderSystem : PugSimulationSystemBase
    {
        private EntityQuery _boulderQuery;

        protected override void OnCreate()
        {
            base.OnCreate();

            _boulderQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadWrite<HealthCD>(),
                    ComponentType.ReadOnly<DropsLootWhenDamagedCD>()
                },
                Options = EntityQueryOptions.IncludeDisabledEntities
            });

            RequireForUpdate(_boulderQuery);
        }

        protected override void OnUpdate()
        {
            using var entities = _boulderQuery.ToEntityArray(Allocator.Temp);
            using var healths = _boulderQuery.ToComponentDataArray<HealthCD>(Allocator.Temp);

            for (var i = 0; i < entities.Length; i++)
            {
                var healthCd = healths[i];
                if (healthCd.health >= healthCd.maxHealth / 2) continue;

                healthCd.health = healthCd.maxHealth;
                EntityManager.SetComponentData(entities[i], healthCd);
            }

            base.OnUpdate();
        }
    }
}