using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace InstantPortalCharge
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class PortalChargeSystem : PugSimulationSystemBase
    {
        protected override void OnUpdate()
        {
            using var waypointQuery = SystemAPI.QueryBuilder()
                .WithAllRW<ObjectDataCD>()
                .WithAll<WayPointCD, DistanceToPlayerCD>()
                .WithNone<EntityDestroyedCD, PortalCD>()
                .Build();

            using (var waypointEntities = waypointQuery.ToEntityArray(Allocator.Temp))
            using (var waypointObjectData = waypointQuery.ToComponentDataArray<ObjectDataCD>(Allocator.Temp))
            using (var waypointData = waypointQuery.ToComponentDataArray<WayPointCD>(Allocator.Temp))
            using (var waypointDistance = waypointQuery.ToComponentDataArray<DistanceToPlayerCD>(Allocator.Temp))
            {
                for (var i = 0; i < waypointEntities.Length; i++)
                {
                    var objectDataCd = waypointObjectData[i];
                    if (objectDataCd.amount >= 600) continue;

                    var wayPoint = waypointData[i];
                    var distance = waypointDistance[i];
                    var minDis = distance.minDistanceSq;
                    if (!(minDis > 0) || !(minDis <= wayPoint.distanceToActivateSQ)) continue;

                    objectDataCd.amount = 600;
                    EntityManager.SetComponentData(waypointEntities[i], objectDataCd);
                }
            }

            base.OnUpdate();
        }
    }
}