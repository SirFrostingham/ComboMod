using System.Linq;
using PugMod;
using UnityEngine;

namespace BetterTextInput
{
    public class BetterTextMod : IMod
    {
        public const string VERSION = "2.3.5";
        public const string NAME = "BetterTextInput";
        internal const string KoreanFontAssetPath = "Assets/Mods/BetterTextInput/Fonts/Galmuri9.asset";
        private const bool ENABLE_KOREAN_CUSTOM_FONT_OVERRIDE = false;
        private static LoadedMod modInfo;
        public static bool useKoreanCustomFont = false;

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

        internal static bool TryLoadAssetFromAnyBundle<T>(string assetPath, out T asset) where T : Object
        {
            asset = null;

            if (modInfo == null || modInfo.AssetBundles == null || !modInfo.AssetBundles.Any())
            {
                Log($"No asset bundles available while trying to load '{assetPath}'.");
                return false;
            }

            foreach (var bundle in modInfo.AssetBundles)
            {
                if (bundle == null)
                {
                    continue;
                }

                try
                {
                    if (!bundle.Contains(assetPath))
                    {
                        continue;
                    }

                    asset = bundle.LoadAsset<T>(assetPath);
                    if (asset != null)
                    {
                        return true;
                    }
                }
                catch
                {
                    // Ignore invalid bundle lookups and continue scanning.
                }
            }

            Log($"Asset not found in loaded bundles: '{assetPath}'.");
            return false;
        }

        private static void LoadConfigs()
        {
            string ID = NAME.Replace(" ", "");

            bool configRequestedCustomFont = false;
            if (API.Config.TryGet(ID, "Font", "useKoreanCustomFont", out bool value))
            {
                configRequestedCustomFont = value;
            }
            else
            {
                API.Config.Set(ID, "Font", "useKoreanCustomFont", false);
            }

            useKoreanCustomFont = configRequestedCustomFont && ENABLE_KOREAN_CUSTOM_FONT_OVERRIDE;
            if (configRequestedCustomFont && !useKoreanCustomFont)
            {
                Log("Korean custom font override was requested in config, but is currently disabled for startup stability.");
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