using System.Linq;
using AlphaPrefabs;
using Blueprints;
using HarmonyLib;
using KCSG;
using RimWorld;
using UnityEngine;
using Verse;

namespace Alpha_Prefabs_Blueprints
{
    [StaticConstructorOnStartup]
    public static class Startup
    {
        static Startup() => new Harmony("localghost.alpharefabsblueprint").PatchAll();
    }

    [HarmonyPatch(typeof(Window_Prefab), nameof(Window_Prefab.DoWindowContents))]
    public static class HarmonyPatches
    {
        static void Postfix(Window_Prefab __instance, ref Rect inRect)
        {
            var outRect = new Rect(inRect);
            outRect.yMin += 20f;
            outRect.yMax -= 40f;
            outRect.width -= 16f;
            outRect.yMin += 20f;
            Rect oderButtonRect = new Rect(
                outRect.width / 2f + Window.CloseButSize.x,
                outRect.height + 30,
                Window.CloseButSize.x,
                Window.CloseButSize.y
            );
            if (!__instance.prefab.variations.NullOrEmpty())
                AlphaPrefabs.Utils.DrawButton(
                    oderButtonRect,
                    "AP_Export".Translate(),
                    () =>
                        Find.WindowStack.Add(
                            new FloatMenu(
                                __instance
                                    .prefab.variations.Select(
                                        variation =>
                                            new FloatMenuOption(
                                                variation.name.CapitalizeFirst(),
                                                () =>
                                                    __instance.Export(
                                                        variation.layoutVariation,
                                                        variation.name
                                                    )
                                            )
                                    )
                                    .Prepend(
                                        new FloatMenuOption(
                                            __instance.prefab.labelForDefaultVariation.CapitalizeFirst(),
                                            () => __instance.Export()
                                        )
                                    )
                                    .ToList()
                            )
                        )
                );
            else if (Widgets.ButtonText(oderButtonRect, "AP_ExportWithVariation".Translate()))
                __instance.Export();
        }

        static void Export(
            this Window_Prefab __instance,
            StructureLayoutDef layoutVariation = null,
            string variationString = null
        ) =>
            BlueprintController.Add(
                Utils.ToBluePrint(
                    layoutVariation ?? __instance.prefab.layout,
                    $"{__instance.prefab.LabelCap}_{variationString ?? __instance.prefab.labelForDefaultVariation}"
                )
            );
    }
}
