using HarmonyLib;

namespace BetterTextInput
{
    /// <summary>
    /// Disabled compatibility shim.
    ///
    /// The previous implementation relied on reflection APIs that are blocked by
    /// Core Keeper's mod security verifier in the combined mod context.
    /// Keeping this file/script present (but inert) preserves manifest compatibility
    /// while avoiding startup compile failures.
    /// </summary>
    [HarmonyPatch]
    internal static class TextInput_Patches
    {
    }
}