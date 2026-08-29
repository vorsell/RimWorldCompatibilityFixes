using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace RigorMortisFloatMenuFix
{
    [HarmonyPatch]
    internal static class WorkGiverTargetPrefilterPatch
    {
        private const string RecoverWorkGiverDefName = "RM_Recover";
        private const string AxolotlReadWorkGiverDefName = "Axolotl_ReadMoeLotlQiSkillBooks";
        private const string AxolotlSkillBookTypeName = "Axolotl.MoeLotlSkillBook";

        private static MethodBase TargetMethod()
        {
            return AccessTools.Method(
                typeof(FloatMenuOptionProvider_WorkGivers),
                "GetWorkGiverOption");
        }

        private static bool Prefix(
            WorkGiverDef workGiver,
            LocalTargetInfo target,
            ref FloatMenuOption __result)
        {
            string defName = workGiver?.defName;
            Thing thing = target.Thing;

            if (defName == RecoverWorkGiverDefName
                && !(thing is RigorMortis.Building_ZombieCasket))
            {
                __result = null;
                return false;
            }

            if (defName == AxolotlReadWorkGiverDefName
                && !IsTypeOrSubclass(thing, AxolotlSkillBookTypeName))
            {
                __result = null;
                return false;
            }

            return true;
        }

        private static bool IsTypeOrSubclass(Thing thing, string expectedFullName)
        {
            Type type = thing?.GetType();
            while (type != null)
            {
                if (type.FullName == expectedFullName)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
