using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Upgrades;

namespace HonoraryParagons;

[HarmonyPatch(typeof(GameModel), nameof(GameModel.GetUpgrade))]
internal static class GameModel_GetUpgrade
{
    [HarmonyPrefix]
    internal static bool Prefix(GameModel __instance, string id, ref UpgradeModel __result)
    {
        if (!id.StartsWith(nameof(HonoraryParagon))) return true;

        __result = HonoraryParagon.GetParagonUpgrade(__instance, id);
        return false;
    }
}

[HarmonyPatch(typeof(RateSupportModel.RateSupportMutator), nameof(RateSupportModel.RateSupportMutator.Mutate))]
internal static class RateSupportMutator_Mutate
{
    [HarmonyPrefix]
    internal static bool Prefix(RateSupportModel.RateSupportMutator __instance, Model model, ref bool __result)
    {
        if (__instance.id != nameof(HonoraryParagon) || !model.Is(out TowerModel towerModel)) return true;

        HonoraryParagon.Paragonify(towerModel);

        __result = true;

        return false;
    }
}