using HarmonyLib;
using Inventory;
using PugMod;
using UnityEngine;

namespace FXCPDS.Items.Solarite_Shovel {
  public class SolariteShovelMod: IMod {
    private const string Version = "1.0.0";
    private const string ModName = "solarite-shovel";

    private GameObject prefab;

    public static void Log(string message) {
      Debug.Log($"[{ModName}] {message}");
    }

    public void EarlyInit() {
      Log($"init v{Version}");
    }

    public void Init() {
      var id = API.Authoring.GetObjectID("SolariteShovel:SolariteShovel");

      API.Authoring.OnObjectTypeAdded += (entity, _, manager) => {
        if (manager.GetComponentData<ObjectDataCD>(entity).objectID != ObjectID.SolariteWorkbench)
          return;

        var recipeRef = new CanCraftObjectsBuffer { objectID = id, amount = 1 };

        var buffer = manager.GetBuffer<CanCraftObjectsBuffer>(entity);

        for (var i = 0; i < buffer.Length; i++) {
          if (buffer[i].objectID != ObjectID.None)
            continue;

          buffer[i] = recipeRef;
          return;
        }

        buffer.Add(recipeRef);
      };
    }

    public void Shutdown() {}

    public void ModObjectLoaded(Object obj) {}

    public void Update() {}

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerController), nameof(PlayerController.QueueInputAction))]
    static bool prefix(ref UIInputActionData inputActionData) {
      if (inputActionData.action == UIInputAction.Craft) {
        Log($"{inputActionData.craftActionData.objectId}");
      }

      return true;
    }
  }
}