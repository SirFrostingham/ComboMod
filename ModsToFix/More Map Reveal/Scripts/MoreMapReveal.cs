using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using PugMod;
using System.Linq;
using Unity.Mathematics;

[HarmonyPatch]
public class MoreMapReveal : IMod
{
    public const string VERSION = "1.0";
    public const string NAME = "MoreMapReveal";
    public const string Author = "Ninakoru";

    private LoadedMod modInfo;

    public static LoadedMod GetModInfo(IMod mod)
    {
        return API.ModLoader.LoadedMods.FirstOrDefault(modInfo => modInfo.Handlers.Contains(mod));
    }

    public void EarlyInit()
    {
        Debug.Log($"[{NAME}]: Mod version: {VERSION}");
        modInfo = GetModInfo(this);
        if (modInfo == null)
        {
            Debug.Log($"[{NAME}]: Failed to load {NAME}: mod metadata not found!");
            return;
        }

        Debug.Log($"[{NAME}]: Mod loaded successfully");
    }

    public void Init()
    {
    }

    public void ModObjectLoaded(Object obj)
    {
    }

    public void Shutdown()
    {
    }

    public void Update()
    {
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MapUpdateSystem), "UpdateTiles")]
    public static bool UpdateTiles_PreRun(ref float revealDistance)
    {
        revealDistance = revealDistance == 7f ? 10f : 15f;
        return true;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(MapUpdateSystem), "RefreshLightBufferData")]
    public static bool RefreshLightBufferData_PreRun(ref float revealDistance)
    {
        revealDistance = revealDistance == 7f ? 10f : 15f;
        return true;
    }
}
