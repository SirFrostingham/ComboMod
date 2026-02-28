using HarmonyLib;
using System;

namespace BetterTextInput
{
    [HarmonyPatch(typeof(TextManager))]
    internal static class TextManager_Patch
    {
        [HarmonyPatch("Init")]
        [HarmonyPrefix]
        public static void Init(TextManager __instance)
        {
            try
            {
                if (BetterTextMod.useKoreanCustomFont)
                {
                    __instance.koreanFont = BetterTextMod.AssetBundle.LoadAsset<PugFont>("Assets/Mods/BetterTextInput/Fonts/Galmuri9.asset");
                }
            }
            catch (Exception e)
            {
                BetterTextMod.Log($"Error loading font: {e.Message}");
            }
        }
    }
}