using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Upgrades;
using Il2CppAssets.Scripts.Models.Towers.Weapons.Behaviors;
using Il2CppAssets.Scripts.Simulation.SMath;

namespace HonoraryParagons;

/// <summary>
/// Return a custom upgrade for visuals and to modify the paragon cost scaling
/// </summary>
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


[HarmonyPatch(typeof(ParagonTowerModel.PowerDegreeMutator), nameof(ParagonTowerModel.PowerDegreeMutator.MutateTower))]
internal static class PowerDegreeMutator_MutateTower
{
    [HarmonyPostfix]
    internal static void Postfix(ParagonTowerModel.PowerDegreeMutator __instance, TowerModel tower)
    {
        tower.GetDescendants<CashModel>().ForEach(cash =>
        {
            cash.bonusMultiplier += __instance.percentDamageUp / 100f;
        });

        tower.GetDescendants<EmissionsPerRoundFilterModel>().ForEach(filter =>
        {
            filter.count += Math.CeilToInt(filter.count * __instance.attackCooldownReductionPercent / 100f);
        });

        tower.GetDescendants<PerRoundCashBonusTowerModel>().ForEach(bonus =>
        {
            bonus.cashRoundBonusMultiplier += __instance.percentDamageUp / 100f;
        });

        tower.GetDescendants<BankModel>().ForEach(bank =>
        {
            bank.interest *= 1 + __instance.attackCooldownReductionPercent / 100f;
            bank.capacity += Math.RoundToNearestInt(bank.capacity * __instance.percentPierceUp / 100f, 500);
        });
    }
}