using Inventory;
using PugMod;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Scripting;

namespace FXCPDS.Items.Solarite_Shovel {
  [Preserve]
  public class SolariteShovelMod: IMod {
    private const string Version = "1.0.1";
    private const string ModName = "solarite-shovel";

    public static void Log(string message) {
      Debug.Log($"[{ModName}] {message}");
    }

    public void EarlyInit() {
      Log($"init v{Version}");
    }

    public void Init() {
      var id = ResolveSolariteShovelId();
      if (id == ObjectID.None) {
        Log("Failed to resolve Solarite Shovel object ID. Recipe injection skipped.");
        return;
      }

      API.Authoring.OnObjectTypeAdded += (entity, _, manager) => {
        if (manager.GetComponentData<ObjectDataCD>(entity).objectID != ObjectID.SolariteWorkbench)
          return;

        var buffer = manager.GetBuffer<CanCraftObjectsBuffer>(entity);
        if (TryAddRecipe(buffer, id)) {
          Log($"Added Solarite Shovel recipe to {ObjectID.SolariteWorkbench}.");
        }
      };
    }

    private static ObjectID ResolveSolariteShovelId() {
      var candidates = new[] {
        "SolariteShovel:SolariteShovel",
        "Items/SolariteShovel:SolariteShovel",
        "Solarite Shovel:SolariteShovel"
      };

      foreach (var candidate in candidates) {
        var id = API.Authoring.GetObjectID(candidate);
        if (id == ObjectID.None)
          continue;

        Log($"Resolved Solarite Shovel object ID using '{candidate}': {(int)id}");
        return id;
      }

      return ObjectID.None;
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

    public void Update() {}
  }
}