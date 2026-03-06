using System.Linq;
using PugMod;
using UnityEngine;

namespace InstantPortalCharge
{
    public class InstantPortalChargeMod : IMod
    {
        public const string VERSION = "1.4.5";
        public const string NAME = "Instant Portal Charge";
        private LoadedMod modInfo;
        
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
        
        public static LoadedMod GetModInfo(IMod mod)
        {
            return API.ModLoader.LoadedMods.FirstOrDefault(modInfo => modInfo.Handlers.Contains(mod));
        }


        public void Init()
        {
        }

        public void Shutdown()
        {
        }

        public void ModObjectLoaded(Object obj)
        {
        }

        public void Update()
        {
        }
    }
}