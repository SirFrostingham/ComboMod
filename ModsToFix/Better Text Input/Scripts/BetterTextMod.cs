using System.Linq;
using PugMod;
using UnityEngine;

namespace BetterTextInput
{
    public class BetterTextMod : IMod
    {
        public const string VERSION = "2.3.4";
        public const string NAME = "BetterTextInput";
        private static LoadedMod modInfo;
        public static bool useKoreanCustomFont = true;
        internal static AssetBundle AssetBundle => modInfo.AssetBundles[0];

        internal static GameObject characterMarkBlinker;
        internal static GameObject characterMark;


        public void EarlyInit()
        {
            Log($"Mod version: {VERSION}");
            modInfo = GetModInfo(this);
            if (modInfo == null)
            {
                Log($"Failed to load {NAME}: mod metadata not found!");
                return;
            }
            Log("Mod loaded successfully");
            LoadConfigs();
        }

        public static LoadedMod GetModInfo(IMod mod)
        {
            return API.ModLoader.LoadedMods.FirstOrDefault(modInfo => modInfo.Handlers.Contains(mod));
        }

        private static void LoadConfigs()
        {
            string ID = NAME.Replace(" ", "");
            if (API.Config.TryGet(ID, "Font", "useKoreanCustomFont", out bool value))
            {
                useKoreanCustomFont = value;
            }
            else
            {
                API.Config.Set(ID, "Font", "useKoreanCustomFont", useKoreanCustomFont);
            }
        }

        public void Init()
        {
        }

        public void Shutdown()
        {
        }

        public void ModObjectLoaded(Object obj)
        {
            var gameObject = obj as GameObject;
            if (gameObject == null) return;
            if (gameObject.name == "characterMarkBlinker") {
                characterMarkBlinker = gameObject;
            } else if (gameObject.name == "characterMark") {
                characterMark = gameObject;
            }
        }

        public void Update()
        {
        }

        public static void Log(string msg)
        {
            Debug.Log($"[{NAME}]: {msg}");
        }
    }
}