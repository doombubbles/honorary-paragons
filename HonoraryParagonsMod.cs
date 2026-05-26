using MelonLoader;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using HonoraryParagons;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Profile;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Newtonsoft.Json;

[assembly: MelonInfo(typeof(HonoraryParagonsMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace HonoraryParagons;

public class HonoraryParagonsMod : BloonsTD6Mod
{
    public static readonly ModSettingFloat InternalCostFactor = new(1)
    {
        description =
            "The internal multiplier that Honorary Paragons use for determining what their pretend paragon upgrade cost is for the purposes of calculating power investments. " +
            "Higher numbers mean it takes more money to add additional power."
    };

    public override void OnTowerSaved(Tower tower, TowerSaveDataModel saveData)
    {
        if (tower.IsMutatedBy(nameof(HonoraryParagon)))
        {
            var investment = tower.GetTowerBehavior<ParagonTower>().investmentInfo;
            saveData.metaData["HonoraryParagonDegree"] = JsonConvert.SerializeObject(investment);
        }
    }

    public override void OnTowerLoaded(Tower tower, TowerSaveDataModel saveData)
    {
        if (saveData.metaData.TryGetValue("HonoraryParagonDegree", out var text))
        {
            if (tower.GetTowerBehavior<ParagonTower>().Is(out var paragon))
            {
                var investment = JsonConvert.DeserializeObject<ParagonTower.InvestmentInfo>(text);

                paragon.investmentInfo = investment;
                paragon.UpdateDegree();
            }
            else
            {
                TaskScheduler.ScheduleTask(() => OnTowerLoaded(tower, saveData),
                    waitCondition: () => tower.GetTowerBehavior<ParagonTower>() != null,
                    stopCondition: () => tower.IsDestroyed);
            }
        }
    }

    public override object Call(string operation, params object[] parameters) => operation switch
    {
        "GetParagonUpgradeId" when parameters.CheckTypes(out TowerModel towerModel) =>
            HonoraryParagon.GetParagonUpgradeId(towerModel),
        "GetParagonUpgrade" when parameters.CheckTypes(out GameModel gameModel, out string id) =>
            HonoraryParagon.GetParagonUpgrade(gameModel, id),
        "GetParagonUpgrade" when parameters.CheckTypes(out GameModel gameModel, out TowerModel towerModel) =>
            HonoraryParagon.GetParagonUpgrade(gameModel, HonoraryParagon.GetParagonUpgradeId(towerModel)),
        _ => base.Call(operation, parameters)
    };
}