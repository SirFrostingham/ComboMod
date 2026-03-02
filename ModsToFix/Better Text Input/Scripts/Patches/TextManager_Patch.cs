using HarmonyLib;
using System;
using UnityEngine;

namespace BetterTextInput
{
    [HarmonyPatch(typeof(TextManager))]
    internal static class TextManager_Patch
    {
        private static bool triedLoadCustomFont;

        [HarmonyPatch("Init")]
        [HarmonyPrefix]
        public static void Init(TextManager __instance)
        {
            try
            {
                if (!BetterTextMod.useKoreanCustomFont || __instance == null)
                {
                    return;
                }

                if (!triedLoadCustomFont)
                {
                    triedLoadCustomFont = true;
                    BetterTextMod.Log("Attempting to load Korean custom font...");
                }

                if (BetterTextMod.TryLoadAssetFromAnyBundle(BetterTextMod.KoreanFontAssetPath, out PugFont customFont)
                    && customFont != null)
                {
                    __instance.koreanFont = customFont;
                }
                else
                {
                    BetterTextMod.useKoreanCustomFont = false;
                    BetterTextMod.Log("Custom Korean font not found. Keeping game's default font to avoid TextManager init failure.");
                }
            }
            catch (Exception e)
            {
                BetterTextMod.useKoreanCustomFont = false;
                BetterTextMod.Log($"Error loading font: {e.Message}");
            }
        }

        [HarmonyPatch("Init")]
        [HarmonyFinalizer]
        public static Exception InitFinalizer(Exception __exception)
        {
            if (__exception == null)
            {
                return null;
            }

            try
            {
                string details = __exception.ToString();
                bool looksLikeFontIssue =
                    details.IndexOf("Galmuri9", StringComparison.OrdinalIgnoreCase) >= 0 ||
                    details.IndexOf("koreanFont", StringComparison.OrdinalIgnoreCase) >= 0;

                if (looksLikeFontIssue)
                {
                    BetterTextMod.useKoreanCustomFont = false;
                    BetterTextMod.Log("Suppressed TextManager.Init font exception and disabled custom Korean font for this session.");
                    Debug.LogException(__exception);
                    return null;
                }
            }
            catch
            {
                // Fall through and keep original exception.
            }

            return __exception;
        }
    }
}