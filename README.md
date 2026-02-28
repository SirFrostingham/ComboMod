# ComboMod — QoL + Fix Pack for Core Keeper

A combined **Core Keeper** mod pack that bundles multiple quality-of-life tweaks and gameplay fixes into one install.

Instead of installing and maintaining each small mod separately, `ComboMod` ships them together under a single `ModManifest.json`, with scripts and asset bundles organized in `ModsToFix/`.

## What’s included

`ComboMod` currently packages the following modules:

1. **All Skill Perks**
   - Rebalances talent point gain for skill trees.
   - Updates pet talent points to scale with pet level.

2. **AutoDoors**
   - Automatically opens/closes doors and gates based on nearby players.

3. **Better Text Input**
   - Improves text input behavior in multiple UI fields.
   - Includes IME-friendly handling and optional Korean custom font support.
   - Adds better cursor movement/selection behavior (copy, cut, select-all, word jumps, etc.).

4. **Experience Tweaks**
   - Adjusts XP behavior for Melee/Range/Magic/Mining using equipped weapon/tool properties.
   - Helps normalize progression for slower cooldown weapons.

5. **Infinite Ore Boulder**
   - Keeps ore boulders regenerating so they remain mineable.

6. **Instant Portal Charge**
   - Keeps portals and waypoints charged (no manual charge grind).

7. **Keep Inventory On Death (Dedicated Servers)**
   - Prevents inventory drop-on-death while still allowing grave spawn behavior.

8. **More Map Reveal**
   - Increases map reveal radius beyond vanilla values.

9. **Quick Unlock**
   - Right-click a locked chest with the matching key to quickly unlock/use it.

10. **Solarite Shovel**
    - Adds Solarite Shovel crafting integration (including localization/asset content).

## Compatibility

| Target | Status |
|---|---|
| Core Keeper (current ECS-era versions) | ✅ Supported |
| Dedicated servers | ✅ Supported (includes dedicated-server-specific inventory behavior) |

> This repository targets the modern Core Keeper modding API (`PugMod` / ECS-style systems).

## Installation

### Manual

1. Download `ComboMod.zip` from this repo’s releases (or build it locally with `deploy.ps1`).
2. Extract to:

```text
%USERPROFILE%\AppData\LocalLow\Pugstorm\Core Keeper\Steam\<SteamID>\mods\ComboMod\
```

3. Ensure the extracted folder contains:

```text
ComboMod\
  ModManifest.json
  ModsToFix\...
```

4. Launch Core Keeper.

## Building / deploying from source

This project is designed around runtime script compilation by Core Keeper and a PowerShell deployment workflow.

Run:

```powershell
.\deploy.ps1
```

The deploy script will:

1. Bump `deploy.version.txt` patch version.
2. Stage files listed in root `ModManifest.json`.
3. Install staged output into your local Core Keeper mods folder.
4. Clear local mod compile cache so changes recompile cleanly.
5. Create a fresh `ComboMod.zip`.
6. Upload to mod.io (if credentials are configured in `secrets.ps1`).

### Secrets / publishing

- `secrets.ps1` is used for private mod.io credentials (OAuth token).
- Keep it local and out of source control.

## Repository layout

```text
.
├─ ModManifest.json              # Root manifest for the combined package
├─ deploy.ps1                    # Local install + zip + optional mod.io publish
├─ deploy.version.txt            # Deployment version used by the publish step
└─ ModsToFix/
   ├─ <Mod Name>/
   │  ├─ ModManifest.json
   │  ├─ Scripts/
   │  ├─ Bundles/
   │  └─ (optional) Localization/, Libraries/
   └─ ...
```

## Notes

- This is a **bundle repository**: each subfolder under `ModsToFix/` may have different origins/history and implementation style.
- Root `ModManifest.json` defines exactly what gets shipped in the combined mod.
