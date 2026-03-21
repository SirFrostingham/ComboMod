using System.Collections.Generic;
using Inventory;
using PugMod;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Scripting;

namespace FXCPDS.Items.Solarite_Shovel {
  [Preserve]
  public class SolariteShovelMod: IMod {
    private const string Version = "1.0.8";
    private const string ModName = "solarite-shovel";
    private const int UpdateIntervalFrames = 60;

    private readonly HashSet<ObjectID> solariteWorkbenchIds = new() { ObjectID.SolariteWorkbench };
    private readonly HashSet<ObjectID> solariteShovelIds = new();
    private readonly HashSet<ObjectID> solariteWorkbenchRecipeAnchorIds = new();
    private readonly HashSet<EntityManager> trackedManagers = new();
    private int updateFrameCounter;

    public static void Log(string message) {
      Debug.Log($"[{ModName}] {message}");
    }

    public void EarlyInit() {
      Log($"init v{Version}");
    }

    public void Init() {
      ResolveSolariteWorkbenchIds();
      ResolveSolariteWorkbenchRecipeAnchorIds();

      API.Authoring.OnObjectTypeAdded += (entity, _, manager) => {
        if (trackedManagers.Add(manager))
          Log($"Tracking new EntityManager for recipe injection. Total tracked managers: {trackedManagers.Count}");

        TryInjectIntoManager(manager);
      };

      Log($"Tracking {solariteWorkbenchIds.Count} Solarite workbench IDs for recipe injection.");
    }

    public void Update() {
      updateFrameCounter++;
      if (updateFrameCounter < UpdateIntervalFrames)
        return;

      updateFrameCounter = 0;

      if (API.Server?.World != null)
        trackedManagers.Add(API.Server.World.EntityManager);

      if (trackedManagers.Count == 0)
        return;

      List<EntityManager> invalidManagers = null;

      foreach (var manager in trackedManagers) {
        try {
          TryInjectIntoManager(manager);
        }
        catch {
          invalidManagers ??= new List<EntityManager>();
          invalidManagers.Add(manager);
        }
      }

      if (invalidManagers == null)
        return;

      foreach (var invalidManager in invalidManagers)
        trackedManagers.Remove(invalidManager);
    }

    private void TryInjectIntoManager(EntityManager manager) {
      ResolveSolariteShovelIds();
      ResolveSolariteWorkbenchRecipeAnchorIds();

      if (solariteShovelIds.Count == 0)
        return;

      foreach (var shovelId in solariteShovelIds)
        TryAddRecipeToAllSolariteWorkbenches(manager, shovelId);
    }

    private void TryAddRecipeToAllSolariteWorkbenches(EntityManager manager, ObjectID id) {
      using var entities = manager.GetAllEntities(Allocator.Temp);
      var added = 0;

      foreach (var entity in entities) {
        if (!manager.HasComponent<ObjectDataCD>(entity) || !manager.HasBuffer<CanCraftObjectsBuffer>(entity))
          continue;

        var objectId = manager.GetComponentData<ObjectDataCD>(entity).objectID;
        var buffer = manager.GetBuffer<CanCraftObjectsBuffer>(entity);
        if (!IsSolariteWorkbenchCraftingBuffer(objectId, buffer))
          continue;

        if (TryAddRecipe(buffer, id))
          added++;
      }

      if (added > 0)
        Log($"Added Solarite Shovel recipe to {added} Solarite workbench entries.");
    }

    private void ResolveSolariteWorkbenchIds() {
      var candidates = new[] {
        "SolariteWorkbench:SolariteWorkbench",
        "Items/SolariteWorkbench:SolariteWorkbench",
        "Objects/SolariteWorkbench:SolariteWorkbench"
      };

      foreach (var candidate in candidates) {
        var id = API.Authoring.GetObjectID(candidate);
        if (id == ObjectID.None)
          continue;

        solariteWorkbenchIds.Add(id);
      }
    }

    private void ResolveSolariteWorkbenchRecipeAnchorIds() {
      var candidates = new[] {
        "Items/SolaritePickaxe:SolaritePickaxe",
        "SolaritePickaxe:SolaritePickaxe",
        "Items/SolariteSword:SolariteSword",
        "SolariteSword:SolariteSword",
        "Items/SolariteBow:SolariteBow",
        "SolariteBow:SolariteBow"
      };

      foreach (var candidate in candidates) {
        var id = API.Authoring.GetObjectID(candidate);
        if (id == ObjectID.None)
          continue;

        if (solariteWorkbenchRecipeAnchorIds.Add(id))
          Log($"Resolved Solarite workbench anchor recipe using '{candidate}': {(int)id}");
      }
    }

    private bool IsSolariteWorkbenchCraftingBuffer(ObjectID objectId, DynamicBuffer<CanCraftObjectsBuffer> buffer) {
      if (solariteWorkbenchIds.Contains(objectId))
        return true;

      if (solariteWorkbenchRecipeAnchorIds.Count == 0)
        return false;

      for (var i = 0; i < buffer.Length; i++) {
        if (solariteWorkbenchRecipeAnchorIds.Contains(buffer[i].objectID))
          return true;
      }

      return false;
    }

    private void ResolveSolariteShovelIds() {
      var candidates = new[] {
        "Items/SolariteShovel:SolariteShovel",
        "SolariteShovel:SolariteShovel",
        "Solarite Shovel:SolariteShovel",
        "Items/SolariteShovel:SolariteShovelEntity",
        "SolariteShovel:SolariteShovelEntity",
        "Solarite Shovel:SolariteShovelEntity",
        "Items/SolariteShovelEntity:SolariteShovelEntity",
        "SolariteShovelEntity:SolariteShovelEntity",
        "Items/SolariteShovelEntity:SolariteShovel",
        "SolariteShovelEntity:SolariteShovel"
      };

      foreach (var candidate in candidates) {
        var id = API.Authoring.GetObjectID(candidate);
        if (id == ObjectID.None)
          continue;

        if (solariteShovelIds.Add(id))
          Log($"Resolved Solarite Shovel object ID using '{candidate}': {(int)id}");
      }

      if (solariteShovelIds.Count == 0)
        Log("Could not resolve any Solarite Shovel object IDs yet.");
    }

    private static bool TryAddRecipe(DynamicBuffer<CanCraftObjectsBuffer> buffer, ObjectID id) {
      for (var i = 0; i < buffer.Length; i++) {
        if (buffer[i].objectID == id)
          return false;
      }

      var recipeRef = new CanCraftObjectsBuffer { objectID = id, amount = 1 };

      for (var i = 0; i < buffer.Length; i++) {
        if (buffer[i].objectID != ObjectID.None)
          continue;

        buffer[i] = recipeRef;
        return true;
      }

      buffer.Add(recipeRef);
      return true;
    }

    public void Shutdown() {}

    public void ModObjectLoaded(Object obj) {}
  }
}