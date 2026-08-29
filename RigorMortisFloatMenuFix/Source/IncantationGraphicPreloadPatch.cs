using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RigorMortisFloatMenuFix
{
    internal static class IncantationGraphicCache
    {
        private static readonly Dictionary<string, Graphic> Graphics =
            new Dictionary<string, Graphic>();

        private static bool initialized;

        internal static void Initialize()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            List<ThingDef> defs = DefDatabase<ThingDef>.AllDefsListForReading;
            for (int defIndex = 0; defIndex < defs.Count; defIndex++)
            {
                List<CompProperties> comps = defs[defIndex].comps;
                if (comps == null)
                {
                    continue;
                }

                for (int compIndex = 0; compIndex < comps.Count; compIndex++)
                {
                    RigorMortis.CompProperties_Incantation incantation =
                        comps[compIndex] as RigorMortis.CompProperties_Incantation;
                    if (incantation == null
                        || string.IsNullOrEmpty(incantation.texPath)
                        || Graphics.ContainsKey(incantation.texPath))
                    {
                        continue;
                    }

                    Graphics.Add(
                        incantation.texPath,
                        GraphicDatabase.Get<Graphic_Multi>(
                            incantation.texPath,
                            ShaderDatabase.Cutout,
                            Vector2.one,
                            Color.white));
                }
            }
        }

        internal static bool TryGet(string path, out Graphic graphic)
        {
            if (string.IsNullOrEmpty(path))
            {
                graphic = null;
                return false;
            }

            return Graphics.TryGetValue(path, out graphic);
        }
    }

    [HarmonyPatch]
    internal static class IncantationRenderGraphicPatch
    {
        private static IEnumerable<MethodBase> TargetMethods()
        {
            yield return AccessTools.Method(
                typeof(RigorMortis.PawnRenderNodeWorker_IncantationPut),
                "GetGraphic");
            yield return AccessTools.Method(
                typeof(RigorMortis.PawnRenderNodeWorker_IncantationExecute),
                "GetGraphic");
        }

        private static bool Prefix(PawnRenderNode node, ref Graphic __result)
        {
            string path = null;
            RigorMortis.HediffAbility_IncantationPut putting =
                node.hediff as RigorMortis.HediffAbility_IncantationPut;
            if (putting != null)
            {
                path = putting.graphicPath;
            }
            else
            {
                RigorMortis.HediffAbility_IncantationExecute executing =
                    node.hediff as RigorMortis.HediffAbility_IncantationExecute;
                if (executing != null)
                {
                    path = executing.graphicPath;
                }
            }

            Graphic graphic;
            if (!IncantationGraphicCache.TryGet(path, out graphic))
            {
                return true;
            }

            __result = graphic;
            return false;
        }
    }
}
