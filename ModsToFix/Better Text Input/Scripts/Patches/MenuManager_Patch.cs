using HarmonyLib;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Reflection;

namespace BetterTextInput
{
    [HarmonyPatch(typeof(MenuManager))]
    internal class MenuManager_Patch
    {
        private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly MethodInfo isKeyDownMethod =
            AccessTools.Method(typeof(MenuManager), "IsKeyDown", new[] { typeof(KeyCode), typeof(bool) }) ??
            AccessTools.Method(typeof(MenuManager), "__IsKeyDown", new[] { typeof(KeyCode), typeof(bool) });

        private static readonly FieldInfo typingActionWasClickedField = ResolveTypingActionWasClickedField();
        private static readonly PropertyInfo typingActionWasClickedProperty = ResolveTypingActionWasClickedProperty();
        private static readonly MethodInfo getTypingActionWasClickedMethod =
            AccessTools.Method(typeof(MenuManager), "GetTypingActionWasClicked") ??
            AccessTools.Method(typeof(MenuManager), "__GetTypingActionWasClicked");
        private static readonly MethodInfo setTypingActionWasClickedMethod =
            AccessTools.Method(typeof(MenuManager), "SetTypingActionWasClicked", new[] { typeof(bool) }) ??
            AccessTools.Method(typeof(MenuManager), "__SetTypingActionWasClicked", new[] { typeof(bool) });

        private static FieldInfo ResolveTypingActionWasClickedField()
        {
            var type = typeof(MenuManager);
            var direct = type.GetField("typingActionWasClicked", Flags)
                         ?? type.GetField("_typingActionWasClicked", Flags)
                         ?? type.GetField("m_typingActionWasClicked", Flags);
            if (direct != null && direct.FieldType == typeof(bool)) return direct;

            foreach (var field in type.GetFields(Flags))
            {
                if (field.FieldType != typeof(bool)) continue;
                var n = field.Name;
                if (n.IndexOf("typing", System.StringComparison.OrdinalIgnoreCase) >= 0
                    && n.IndexOf("clicked", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return field;
                }
            }

            return null;
        }

        private static PropertyInfo ResolveTypingActionWasClickedProperty()
        {
            var type = typeof(MenuManager);
            var direct = type.GetProperty("TypingActionWasClicked", Flags)
                         ?? type.GetProperty("typingActionWasClicked", Flags)
                         ?? type.GetProperty("IsTypingActionClicked", Flags);
            if (direct != null && direct.PropertyType == typeof(bool)) return direct;

            foreach (var property in type.GetProperties(Flags))
            {
                if (property.PropertyType != typeof(bool)) continue;
                var n = property.Name;
                if (n.IndexOf("typing", System.StringComparison.OrdinalIgnoreCase) >= 0
                    && n.IndexOf("clicked", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return property;
                }
            }

            return null;
        }

        private static void SetTypingActionWasClicked(MenuManager instance, bool value)
        {
            if (instance == null) return;

            if (setTypingActionWasClickedMethod != null)
            {
                setTypingActionWasClickedMethod.Invoke(instance, new object[] { value });
                return;
            }

            if (typingActionWasClickedProperty != null && typingActionWasClickedProperty.CanWrite)
            {
                typingActionWasClickedProperty.SetValue(instance, value, null);
                return;
            }

            if (typingActionWasClickedField != null)
            {
                typingActionWasClickedField.SetValue(instance, value);
            }
        }

        private static bool GetTypingActionWasClicked(MenuManager instance)
        {
            if (instance == null) return false;

            if (getTypingActionWasClickedMethod != null)
            {
                return (bool)getTypingActionWasClickedMethod.Invoke(instance, null);
            }

            if (typingActionWasClickedProperty != null && typingActionWasClickedProperty.CanRead)
            {
                return (bool)typingActionWasClickedProperty.GetValue(instance, null);
            }

            if (typingActionWasClickedField != null)
            {
                return (bool)typingActionWasClickedField.GetValue(instance);
            }

            return false;
        }

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

            SetTypingActionWasClicked(__instance, false);

            if (HandleSpecialKeys(__instance))
            {
                return false;
            }

            if (HandleSelectionKeys(__instance))
            {
                return false;
            }

            if (!GetTypingActionWasClicked(__instance))
            {
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

            return true;
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
                else if (Input.GetKey(KeyCode.LeftControl))
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

            if (Input.GetKey(KeyCode.LeftControl))
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
            if (isKeyDownMethod != null)
            {
                return (bool)isKeyDownMethod.Invoke(instance, new object[] { key, allowRepeat });
            }

            // Fallback if method name/signature changed in the game API.
            return allowRepeat ? Input.GetKey(key) : Input.GetKeyDown(key);
        }

    }
}