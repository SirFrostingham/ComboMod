using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Inventory;
using UnityEngine;

namespace InfiniteOreBoulder
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class InfiniteBoulderSystem : PugSimulationSystemBase
    {
        private const int MaxSkippedTypeLogs = 25;

        private EntityQuery _boulderQuery;
        private readonly HashSet<ObjectID> _boulderObjectIds = new();
        private readonly HashSet<ObjectID> _loggedSkippedObjectIds = new();
        private int _skippedTypeLogCount;

        protected override void OnCreate()
        {
            base.OnCreate();

            foreach (ObjectID objectId in Enum.GetValues(typeof(ObjectID)))
            {
                var objectName = objectId.ToString();
                if (objectName.IndexOf("OreBoulder", StringComparison.OrdinalIgnoreCase) < 0) continue;
                _boulderObjectIds.Add(objectId);
            }

            if (_boulderObjectIds.Count == 0)
            {
                foreach (ObjectID objectId in Enum.GetValues(typeof(ObjectID)))
                {
                    var objectName = objectId.ToString();
                    if (objectName.IndexOf("Boulder", StringComparison.OrdinalIgnoreCase) < 0) continue;
                    _boulderObjectIds.Add(objectId);
                }

                Debug.LogWarning("[Infinite Ore Boulder] No ObjectID names matched 'OreBoulder'. Fell back to broader 'Boulder' matching.");
            }

            Debug.Log($"[Infinite Ore Boulder] Tracking {_boulderObjectIds.Count} boulder object IDs.");

            _boulderQuery = GetEntityQuery(new EntityQueryDesc
            {
                All = new[]
                {
                    ComponentType.ReadWrite<HealthCD>(),
                    ComponentType.ReadOnly<DropsLootWhenDamagedCD>(),
                    ComponentType.ReadOnly<ObjectDataCD>()
                },
                Options = EntityQueryOptions.IncludeDisabledEntities
            });

            RequireForUpdate(_boulderQuery);
        }

        protected override void OnUpdate()
        {
            using var entities = _boulderQuery.ToEntityArray(Allocator.Temp);
            using var healths = _boulderQuery.ToComponentDataArray<HealthCD>(Allocator.Temp);
            using var objectDatas = _boulderQuery.ToComponentDataArray<ObjectDataCD>(Allocator.Temp);

            for (var i = 0; i < entities.Length; i++)
            {
                var objectId = objectDatas[i].objectID;
                if (!_boulderObjectIds.Contains(objectId))
                {
                    LogSkippedType(entities[i], objectId);
                    continue;
                }

                var healthCd = healths[i];
                if (healthCd.health >= healthCd.maxHealth / 2) continue;

                healthCd.health = healthCd.maxHealth;
                EntityManager.SetComponentData(entities[i], healthCd);
            }

            base.OnUpdate();
        }

        private void LogSkippedType(Entity entity, ObjectID objectId)
        {
            if (_skippedTypeLogCount >= MaxSkippedTypeLogs) return;
            if (!_loggedSkippedObjectIds.Add(objectId)) return;

            _skippedTypeLogCount++;

            Debug.Log($"[Infinite Ore Boulder] Skipping non-boulder entity. objectID={objectId} ({(int)objectId}).");

            using var componentTypes = EntityManager.GetComponentTypes(entity, Allocator.Temp);
            var componentNames = new string[componentTypes.Length];

            for (var i = 0; i < componentTypes.Length; i++)
            {
                componentNames[i] = componentTypes[i].ToString();
            }

            Debug.Log($"[Infinite Ore Boulder] Components for skipped objectID={objectId}: {string.Join(", ", componentNames)}");
        }
    }
}