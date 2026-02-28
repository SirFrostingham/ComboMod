using HarmonyLib;
using System;

namespace BetterTextInput
{
    [HarmonyPatch(typeof(InputManager))]
    internal class InputManager_Patch
    {
        public static event Action<InputManager.TextInputInterface> OnActiveInputFieldChanged;

        [HarmonyPatch("SetActiveInputField")]
        [HarmonyPostfix]
        private static void SetActiveInputField(InputManager __instance, InputManager.TextInputInterface _activeInputField)
        {
            OnActiveInputFieldChanged?.Invoke(_activeInputField);
        }

    }
}