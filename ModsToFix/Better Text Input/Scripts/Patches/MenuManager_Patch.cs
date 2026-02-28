using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BetterTextInput
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManager_Patch
    {
        public static BaseInput inputSystem
        {
            get { return EventSystem.current?.currentInputModule?.input; }
        }
        private static string compositionString
        {
            get { return inputSystem != null ? inputSystem.compositionString : Input.compositionString; }
        }

        public static TextInputController activedController;

        [HarmonyPatch(MethodType.Constructor)]
        public static void Postfix(MenuManager __instance)
        {
            InputManager_Patch.OnActiveInputFieldChanged += (activeInputField) =>
            {
                if (inputSystem != null)
                {
                    inputSystem.imeCompositionMode = IMECompositionMode.Off;
                }

                if (activedController != null)
                {
                    activedController.Reset();
                    activedController = null;
                }

                if (activeInputField != null)
                {
                    activedController = ((Component)activeInputField).gameObject.GetComponent<TextInputController>();
                }
            };
        }

        [HarmonyPatch("HandleTypingInput")]
        [HarmonyPrefix]
        private static bool BeforeHandleTypingInput(MenuManager __instance)
        {
            if (activedController == null || Manager.input?.activeInputField == null)
            {
                return true;
            }

            if (!Manager.input.SystemPrefersKeyboardAndMouse())
            {
                return true;
            }

            if (inputSystem != null && inputSystem.imeCompositionMode != IMECompositionMode.On)
            {
                inputSystem.imeCompositionMode = IMECompositionMode.On;
            }

            if (HandleSpecialKeys(__instance))
            {
                return false;
            }

            if (HandleSelectionKeys(__instance))
            {
                return false;
            }

            bool hasInputString = !string.IsNullOrEmpty(Input.inputString);
            bool hasCompositionString = !string.IsNullOrEmpty(compositionString);
            bool isComposing = activedController.state == CompositionState.Composing;

            if (hasInputString && isComposing)
            {
                activedController.UpdateCompositionState(CompositionState.Completed);
                activedController.AppendString(Input.inputString);
            }
            else if (hasCompositionString)
            {
                activedController.AppendString(compositionString);
            }
            else if (!hasInputString && !hasCompositionString && isComposing)
            {
                activedController.UpdateCompositionState(CompositionState.Completed);
                activedController.AppendString("");
            }
            else
            {
                return true;
            }

            Manager.input.activeInputField.WasAutoActivated = false;
            return false;

        }

        private static bool HandleSpecialKeys(MenuManager __instance)
        {
            if (IsKeyDown(__instance, KeyCode.Escape, false))
            {
                Manager.input.activeInputField.Deactivate(false);
                return true;
            }
            if (IsKeyDown(__instance, KeyCode.Return, true) ||
                IsKeyDown(__instance, KeyCode.LeftArrow, true) ||
                IsKeyDown(__instance, KeyCode.RightArrow, true) ||
                IsKeyDown(__instance, KeyCode.UpArrow, true) ||
                IsKeyDown(__instance, KeyCode.DownArrow, true) ||
                IsKeyDown(__instance, KeyCode.Delete, true) ||
                IsKeyDown(__instance, KeyCode.Home, true) ||
                IsKeyDown(__instance, KeyCode.End, true) ||
                (activedController.isSelectionKeyPressed && (IsKeyDown(__instance, KeyCode.LeftArrow, true) || IsKeyDown(__instance, KeyCode.RightArrow, true))))
            {
                activedController.UpdateCompositionState(CompositionState.None);
                if (inputSystem != null)
                {
                    inputSystem.imeCompositionMode = IMECompositionMode.Off;
                }
            }

            return false;
        }

        private static bool HandleSelectionKeys(MenuManager __instance)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                activedController.InitiateSelection();
            }

            if (activedController.isSelected)
            {
                if (IsKeyDown(__instance, KeyCode.Delete, false) || IsKeyDown(__instance, KeyCode.Backspace, false))
                {
                    activedController.DeleteSelectedText();
                    return true;
                }
                else if (!string.IsNullOrEmpty(Input.inputString))
                {
                    activedController.DeleteSelectedText();
                }
                else if (Manager.input.IsPasteButtonDown())
                {
                    activedController.DeleteSelectedText();
                }
                else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
                {
                    if (IsKeyDown(__instance, KeyCode.C, false))
                    {
                        activedController.CopySelectedText();
                        return true;
                    }
                    else if (IsKeyDown(__instance, KeyCode.X, false))
                    {
                        activedController.CopySelectedText();
                        activedController.DeleteSelectedText();
                        return true;
                    }
                }
            }

            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (IsKeyDown(__instance, KeyCode.A, false))
                {
                    activedController.SelectAllText();
                    return true;
                }

                if (IsKeyDown(__instance, KeyCode.LeftArrow, true))
                {
                    activedController.MoveToWord(-1);
                }
                else if (IsKeyDown(__instance, KeyCode.RightArrow, true))
                {
                    activedController.MoveToWord(1);
                }
            }
            else if (IsKeyDown(__instance, KeyCode.Home, false))
            {
                activedController.MoveToStart();
                return true;
            }
            else if (IsKeyDown(__instance, KeyCode.End, false))
            {
                activedController.MoveToEnd();
                return true;
            }
            else if (IsKeyDown(__instance, KeyCode.LeftArrow, true) || IsKeyDown(__instance, KeyCode.RightArrow, true))
            {
                if (!activedController.isSelectionKeyPressed) activedController.DeactivateSelection();
            }

            return false;
        }

        [HarmonyPatch("HandleTypingInput")]
        [HarmonyPostfix]
        private static void AfterHandleTypingInput(MenuManager __instance, ref bool __result)
        {
            __result = true;
        }

        private static bool IsKeyDown(MenuManager instance, KeyCode key, bool allowRepeat)
        {
            return allowRepeat ? (Input.GetKeyDown(key) || Input.GetKey(key)) : Input.GetKeyDown(key);
        }

    }
}