using System;
using HarmonyLib;
using UnityEngine;

namespace BetterTextInput
{
    /// <summary>
    /// Compatibility safety net for third-party UI patches.
    ///
    /// Some external mods patch UIManager.Init and can throw during startup,
    /// which causes manager initialization to abort and results in a black screen.
    ///
    /// We only suppress the known ExpandedChestUI startup exception signature.
    /// All other exceptions are rethrown so real issues remain visible.
    /// </summary>
    [HarmonyPatch(typeof(UIManager), "Init")]
    internal static class UIManager_InitSafety_Patch
    {
        [HarmonyFinalizer]
        private static Exception Finalizer(Exception __exception)
        {
            if (__exception == null)
            {
                return null;
            }

            try
            {
                string details = __exception.ToString();
                bool isExpandedChestUIIssue =
                    details.IndexOf("ExpandedChestUI", StringComparison.OrdinalIgnoreCase) >= 0;

                if (isExpandedChestUIIssue)
                {
                    BetterTextMod.Log("Suppressed ExpandedChestUI UIManager.Init exception to prevent black-screen startup failure. Update or disable ExpandedChestUI for a permanent fix.");
                    Debug.LogException(__exception);
                    return null;
                }
            }
            catch
            {
                // Fall through and rethrow the original exception.
            }

            return __exception;
        }
    }
}
