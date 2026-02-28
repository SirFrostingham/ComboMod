using System;
using System.Linq;
using HarmonyLib;

[HarmonyPatch]
public class AllSkillPerks
{
    // 1-50  - 1 point per 5 levels  = 10
    // 51-95 - 2 points per 5 levels = 18
    // 100   - 12 points             = 12
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.GetAvailableTalentPoints))]
    private static bool GetSkillPoints(SkillID skillTreeID, ref int __result)
    {
        var sValue = Manager.saves.GetSkillValue(skillTreeID);
        var sLevel = SkillExtensions.GetLevelFromSkill(skillTreeID, sValue);

        sLevel -= sLevel % 5;
        
        if (sLevel <= 50)
        {
            __result = sLevel / 5;
        }
        else if (sLevel < 100)
        {
            __result = (sLevel - 50) / 5 * 2 + 10;
        }
        else
        {
            __result = 40;
        }

        __result -= Manager.saves.GetSkillTalentTreesPoints(skillTreeID)?.Sum() ?? 0;

        return false;
    }

    // lvl 10 -> 9 points
    [HarmonyPrefix]
    [HarmonyPatch(typeof(PetExtensions), nameof(PetExtensions.GetTotalTalentPoints))]
    private static bool GetPetPoints(int currentXp, ref int __result)
    {
        __result = Math.Max(0, PetExtensions.GetLevelFromXP(currentXp) - 1);
        return false;
    }
}
