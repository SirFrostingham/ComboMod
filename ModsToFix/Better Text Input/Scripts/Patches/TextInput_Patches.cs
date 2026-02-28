using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using Pug.UnityExtensions;

namespace BetterTextInput
{
    internal static class TextInputReflectionCompat
    {
        private static readonly BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static FieldInfo ResolveIntField(Type type, params string[] candidateNames)
        {
            foreach (var name in candidateNames)
            {
                var field = type.GetField(name, Flags);
                if (field != null && field.FieldType == typeof(int)) return field;
            }

            foreach (var field in type.GetFields(Flags))
            {
                if (field.FieldType != typeof(int)) continue;
                var n = field.Name;
                if ((n.IndexOf("char", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("cursor", StringComparison.OrdinalIgnoreCase) >= 0)
                    && (n.IndexOf("index", StringComparison.OrdinalIgnoreCase) >= 0 || n.IndexOf("marker", StringComparison.OrdinalIgnoreCase) >= 0))
                {
                    return field;
                }
            }

            return null;
        }

        private static FieldInfo ResolveBoolField(Type type, params string[] candidateNames)
        {
            foreach (var name in candidateNames)
            {
                var field = type.GetField(name, Flags);
                if (field != null && field.FieldType == typeof(bool)) return field;
            }

            foreach (var field in type.GetFields(Flags))
            {
                if (field.FieldType != typeof(bool)) continue;
                var n = field.Name;
                if (n.IndexOf("text", StringComparison.OrdinalIgnoreCase) >= 0
                    && n.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return field;
                }
            }

            return null;
        }

        private static PropertyInfo ResolveBoolProperty(Type type, params string[] candidateNames)
        {
            foreach (var name in candidateNames)
            {
                var property = type.GetProperty(name, Flags);
                if (property != null && property.PropertyType == typeof(bool)) return property;
            }

            foreach (var property in type.GetProperties(Flags))
            {
                if (property.PropertyType != typeof(bool)) continue;
                var n = property.Name;
                if (n.IndexOf("text", StringComparison.OrdinalIgnoreCase) >= 0
                    && n.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return property;
                }
            }

            return null;
        }

        private static MethodInfo ResolveBoolMethod(Type type, params string[] candidateNames)
        {
            foreach (var name in candidateNames)
            {
                var method = type.GetMethod(name, Flags, null, Type.EmptyTypes, null);
                if (method != null && method.ReturnType == typeof(bool)) return method;
            }

            foreach (var method in type.GetMethods(Flags))
            {
                if (method.ReturnType != typeof(bool) || method.GetParameters().Length != 0) continue;
                var n = method.Name;
                if (n.IndexOf("text", StringComparison.OrdinalIgnoreCase) >= 0
                    && n.IndexOf("active", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return method;
                }
            }

            return null;
        }

        private static MethodInfo ResolveVoidMethod(Type type, params string[] candidateNames)
        {
            foreach (var name in candidateNames)
            {
                var method = type.GetMethod(name, Flags, null, Type.EmptyTypes, null);
                if (method != null && method.ReturnType == typeof(void)) return method;
            }

            return null;
        }

        private static readonly FieldInfo textInputFieldCurrentCharIndexField = ResolveIntField(
            typeof(TextInputField),
            "currentCharIndex", "_currentCharIndex", "m_currentCharIndex", "inputFieldMarker", "_inputFieldMarker", "m_inputFieldMarker");

        private static readonly FieldInfo radialCurrentCharIndexField = ResolveIntField(
            typeof(RadicalMenuOptionTextInput),
            "currentCharIndex", "_currentCharIndex", "m_currentCharIndex", "inputFieldMarker", "_inputFieldMarker", "m_inputFieldMarker");

        private static readonly FieldInfo chatWindowTextInputActiveField = ResolveBoolField(
            typeof(ChatWindow),
            "textInputActive", "_textInputActive", "m_textInputActive");

        private static readonly PropertyInfo chatWindowTextInputActiveProperty = ResolveBoolProperty(
            typeof(ChatWindow),
            "TextInputActive", "IsTextInputActive");

        private static readonly MethodInfo chatWindowTextInputActiveMethod = ResolveBoolMethod(
            typeof(ChatWindow),
            "GetTextInputActive", "IsTextInputActive", "__GetTextInputActive");

        private static readonly MethodInfo adjustInputFieldPositionMethod = ResolveVoidMethod(
            typeof(ChatWindow),
            "AdjustInputFieldPosition", "__AdjustInputFieldPosition");

        public static int GetCurrentCharIndex(TextInputField instance)
        {
            if (instance == null || textInputFieldCurrentCharIndexField == null) return 0;
            return (int)textInputFieldCurrentCharIndexField.GetValue(instance);
        }

        public static void SetCurrentCharIndex(TextInputField instance, int value)
        {
            if (instance == null || textInputFieldCurrentCharIndexField == null) return;
            textInputFieldCurrentCharIndexField.SetValue(instance, value);
        }

        public static int GetCurrentCharIndex(RadicalMenuOptionTextInput instance)
        {
            if (instance == null || radialCurrentCharIndexField == null) return 0;
            return (int)radialCurrentCharIndexField.GetValue(instance);
        }

        public static void SetCurrentCharIndex(RadicalMenuOptionTextInput instance, int value)
        {
            if (instance == null || radialCurrentCharIndexField == null) return;
            radialCurrentCharIndexField.SetValue(instance, value);
        }

        public static bool GetTextInputActive(ChatWindow instance)
        {
            if (instance == null) return false;

            if (chatWindowTextInputActiveField != null)
            {
                return (bool)chatWindowTextInputActiveField.GetValue(instance);
            }

            if (chatWindowTextInputActiveProperty != null)
            {
                return (bool)chatWindowTextInputActiveProperty.GetValue(instance, null);
            }

            if (chatWindowTextInputActiveMethod != null)
            {
                return (bool)chatWindowTextInputActiveMethod.Invoke(instance, null);
            }

            return Manager.input != null && Manager.input.activeInputField != null;
        }

        public static void AdjustInputFieldPosition(ChatWindow instance)
        {
            if (instance == null || adjustInputFieldPositionMethod == null) return;
            adjustInputFieldPositionMethod.Invoke(instance, null);
        }
    }

    
    [HarmonyPatch(typeof(TextInputField))]
    internal class TextInputField_Patch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void Awake(TextInputField __instance)
        {
            var sourceSprite = __instance.characterMarkBlinker.gameObject.GetComponent<SpriteRenderer>();
            var controller = __instance.gameObject.AddComponent<TextInputController>();
            controller.Init(
                __instance.pugText,
                __instance.maxWidth,
                () => TextInputReflectionCompat.GetCurrentCharIndex(__instance),
                (int index) => TextInputReflectionCompat.SetCurrentCharIndex(__instance, index),
                sourceSprite?.sortingOrder,
                sourceSprite?.maskInteraction
            );
        }
    }

    [HarmonyPatch(typeof(RadicalMenuOptionTextInput))]
    internal class RadicalMenuOptionTextInput_Patch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void Awake(RadicalMenuOptionTextInput __instance)
        {
            var sourceSprite = __instance.characterMarkBlinker.gameObject.GetComponent<SpriteRenderer>();
            var controller = __instance.gameObject.AddComponent<TextInputController>();
            controller.Init(
                __instance.pugText,
                __instance.maxWidth,
                () => TextInputReflectionCompat.GetCurrentCharIndex(__instance),
                (int index) => TextInputReflectionCompat.SetCurrentCharIndex(__instance, index),
                sourceSprite?.sortingOrder,
                sourceSprite?.maskInteraction
            );
        }
    }

    [HarmonyPatch(typeof(ChatWindow))]
    internal class ChatWindow_Patch
    {
        private static CharacterMarkBlinker characterMarkBlinker;
        private static TextInputController textInputController;
        private static readonly FieldInfo inputFieldMarkerField = ResolveInputFieldMarkerField();

        private static FieldInfo ResolveInputFieldMarkerField()
        {
            var type = typeof(ChatWindow);
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var direct = type.GetField("inputFieldMarker", flags)
                         ?? type.GetField("_inputFieldMarker", flags)
                         ?? type.GetField("m_inputFieldMarker", flags);
            if (direct != null && direct.FieldType == typeof(int))
            {
                return direct;
            }

            foreach (var field in type.GetFields(flags))
            {
                if (field.FieldType != typeof(int)) continue;
                if (field.Name.IndexOf("input", StringComparison.OrdinalIgnoreCase) >= 0 &&
                    field.Name.IndexOf("marker", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return field;
                }
            }

            return null;
        }

        private static int GetInputFieldMarker(ChatWindow instance)
        {
            if (instance == null || inputFieldMarkerField == null) return 0;
            return (int)inputFieldMarkerField.GetValue(instance);
        }

        private static void SetInputFieldMarker(ChatWindow instance, int value)
        {
            if (instance == null || inputFieldMarkerField == null) return;
            inputFieldMarkerField.SetValue(instance, value);
        }

        [HarmonyPatch("Awake")]
        [HarmonyPrefix]
        public static void Awake(ChatWindow __instance)
        {
            if (BetterTextMod.characterMarkBlinker == null)
            {
                BetterTextMod.Log("characterMarkBlinker prefab was not loaded, skipping ChatWindow patch setup.");
                return;
            }

            var blinkerGameObject = UnityEngine.Object.Instantiate(BetterTextMod.characterMarkBlinker, __instance.transform);
            var sourceSprite = blinkerGameObject.gameObject.GetComponent<SpriteRenderer>();

            characterMarkBlinker = blinkerGameObject.GetComponent<CharacterMarkBlinker>();
            characterMarkBlinker.transform.position = __instance.inputField.transform.position;
            characterMarkBlinker.gameObject.SetActive(false);

            textInputController = __instance.gameObject.AddComponent<TextInputController>();
            textInputController.Init(
                __instance.inputField,
                1000,
                () => GetInputFieldMarker(__instance),
                (int index) => SetInputFieldMarker(__instance, index),
                sourceSprite.sortingOrder,
                sourceSprite.maskInteraction
            );
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        private static void Update(ChatWindow __instance)
        {
            if (!TextInputReflectionCompat.GetTextInputActive(__instance)) return;
            if (characterMarkBlinker == null || textInputController == null) return;

            if (Manager.input.inputFieldWasSetThisFrame && Manager.main.player.inputModule.WasButtonPressedDownThisFrame(PlayerInput.InputType.OPEN_CHAT, true))
            {
                characterMarkBlinker.EnableAndResetBlink();
            }

            TextInputReflectionCompat.AdjustInputFieldPosition(__instance);
            
            characterMarkBlinker.transform.SetPositionX(textInputController.GetCharPositionOf(GetInputFieldMarker(__instance)));
        }

        [HarmonyPatch("MoveCharMarker")]
        [HarmonyPrefix]
        private static void MoveCharMarker(ChatWindow __instance, int n)
        {
            var pos = GetInputFieldMarker(__instance) + n;
            SetInputFieldMarker(__instance, Mathf.Clamp(pos, 0, __instance.inputField.GetTextLength()));
        }

        [HarmonyPatch("AppendString")]
        [HarmonyPrefix]
        private static bool AppendString(ChatWindow __instance, string input)
        {
            var pugText = __instance.inputField;
            string textString = pugText.GetText();
            if (GetInputFieldMarker(__instance) > textString.Length)
            {
                SetInputFieldMarker(__instance, textString.Length);
            }
            if (GetInputFieldMarker(__instance) == textString.Length)
            {
                pugText.SetText(pugText.GetText() + input);
            }
            else
            {
                pugText.SetText(pugText.GetText().Insert(GetInputFieldMarker(__instance), input));
            }
            SetInputFieldMarker(__instance, GetInputFieldMarker(__instance) + input.Length);
            pugText.Render(false, true);
            if (pugText.dimensions.width + input.Length > 1000)
            {
                pugText.SetText(textString);
                SetInputFieldMarker(__instance, GetInputFieldMarker(__instance) - input.Length);
                pugText.Render(false, true);
            }
            __instance.WasAutoActivated = false;

            return false;
        }

        [HarmonyPatch("AdjustInputFieldPosition")]
        [HarmonyPrefix]
        private static bool AdjustInputFieldPosition(ChatWindow __instance)
        {
            var currentCharIndex = GetInputFieldMarker(__instance);
            if (currentCharIndex <= 0)
            {
                __instance.inputField.transform.localPosition = new Vector3(0f, __instance.inputField.transform.localPosition.y, __instance.inputField.transform.localPosition.z);
                return false;
            }
            else if (currentCharIndex > __instance.inputField.GetTextLength())
            {
                currentCharIndex = __instance.inputField.GetTextLength();
            }

            var num = __instance.inputFieldMask.localScale.x / 16f;
            var offset = num / 5f;
            var charPos = __instance.inputField.localCharacterEndPositions[currentCharIndex - 1].x - __instance.inputFieldPrompt.transform.localPosition.x;
            Transform transform = __instance.inputField.transform;
            Vector3 localPosition = transform.localPosition;
            localPosition.x = -1f * Mathf.Max(0f, charPos - num + offset);
            transform.localPosition = localPosition;

            return false;
        }
    }

}