using System.Collections.Generic;
using System.Linq;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api;
using BTD_Mod_Helper.Api.Display;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.ModOptions;
using BTD_Mod_Helper.Extensions;
using BuffsInShop;
using Il2Cpp;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Effects;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Filters;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Models.Towers.Projectiles.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Upgrades;
using Il2CppAssets.Scripts.Simulation.Input;
using Il2CppAssets.Scripts.Simulation.Objects;
using Il2CppAssets.Scripts.Simulation.SimulationBehaviors;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Towers.Behaviors;
using Il2CppAssets.Scripts.Unity;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using UnityEngine;

namespace HonoraryParagons;

public class HonoraryParagon : ModBuffInShop
{
    public override string OriginTower => null!;

    public override string BaseDescription =>
        "Converts a Tier 3 or higher tower into an Honorary Paragon. " +
        "You can no longer upgrade or buff it, only invest into its Paragon Degree. " +
        "Honorary Paragons can see Camo and pop all Bloon types, but don't do as much bonus damage to Elite Bosses.";

    public override float BaseCost => 5000;
    public override KeyCode KeyCode => KeyCode.Backslash;

    public override bool IsBlocked(TowerInventory ti) => false;

    public override EffectModel? PlacementEffect => null;
    public override AudioClipReference? PlacementSound => null;

    public override bool CanApplyTo(Tower tower, ref string helperMessage)
    {
        if (tower.towerModel.IsHero() && tower.towerModel.tier < 20)
        {
            helperMessage = "Hero must be max level";
            return false;
        }

        if (tower.towerModel.tier < 3)
        {
            helperMessage = "Tower must be at least tier 3";
            return false;
        }

        if (tower.towerModel.isParagon)
        {
            helperMessage = "Already a paragon";
            return false;
        }

        return base.CanApplyTo(tower, ref helperMessage);
    }

    public override void Register()
    {
        base.Register();
        Cache[Name] = this;
    }

    public override BehaviorMutator GetMutator(Tower? tower) => new RateSupportModel.RateSupportMutator(true, Name, 0,
        1000, GetInstance<HonoraryParagonIcon>().CreateBuffIndicatorModel());

    public override void Apply(Tower tower, float purchaseCost = -1, bool sideEffects = false)
    {
        base.Apply(tower, purchaseCost, sideEffects);

        if (!(ModHelper.HasMod("TacticalTweaks", out var tacticalTweaks) &&
              tacticalTweaks.ModSettings.TryGetValue("BuffableParagons", out var buffableParagons) &&
              buffableParagons is ModSettingBool modSettingBool && modSettingBool))
        {
            var exclude = new List<string>();

            if (ModHelper.HasMod("PathsPlusPlus", out var pathsPlusPlus))
            {
                exclude.AddRange((IEnumerable<string>) pathsPlusPlus.Call("GetPathIds"));
            }

            foreach (var mutator in tower.mutators.ToArray().Reverse())
            {
                if (mutator.mutator.id == Name || exclude.Contains(mutator.mutator.id))
                {
                    mutator.isParagonMutator = true;
                }

                if (!mutator.isParagonMutator && !mutator.mutator.isArtifactMutator)
                {
                    tower.RemoveMutatorIncludeSubTowers(mutator.mutator);
                }
            }
        }

        tower.Sim.factory.GetUncast<ParagonOfPower>()
            .ForEach(power => power.CheckBuffs(tower, tower.towerModel.baseId, tower.owner, false));
    }

    public override void OnPlace(Vector2 at, TowerModel towerModelFake, Tower? tower, float towerCost)
    {
        if (tower == null) return;

        base.OnPlace(at, towerModelFake, tower, towerCost);

        var paragonTower = tower.GetTowerBehavior<ParagonTower>();

        if (paragonTower.paragonTowerModel.effectDuringModel.Is(out var effect))
        {
            paragonTower.effectDuring = paragonTower.Sim.SpawnEffect(effect, tower.Position, tower.Transform);
        }
        paragonTower.SetActive(false);
        paragonTower.activeAt = paragonTower.Sim.time.elapsed + paragonTower.paragonTowerModel.inactiveDurationFrames;
        paragonTower.PlayParagonUpgradeSound();


        /*ParagonomicsMod.StartPopup();
        TowerSelectionMenu.instance.SelectTower(hoveredTower.GetTowerToSim());

        var cashBefore = InGame.Bridge.GetCash();

        var upgradeCost = ParagonomicsMod.GetParagonUpgrade(hoveredTower).cost;

        // Dummy mutator to make it not sacrifice itself
        hoveredTower.AddMutator(
            new RateSupportModel.RateSupportMutator(true, "DoorGunnerMutator", 1, 0, null),
            isParagonMutator: true);

        PopupScreen.instance.ShowParagonConfirmationPopup(PopupScreen.Placement.inGameCenter, "Sacrifice Towers?",
            $"Do you want to sacrifice other {(hoveredTower.towerModel.baseId + "s").Localize()} and make an initial investment?",
            new Action<double>(d =>
            {
                var paragonTower = CompletePlacement();

                paragonTower.nonUpgradeCashInvestment = d;
                paragonTower.doSacrifice = true;
                paragonTower.cashBefore = cashBefore;
                paragonTower.cashAfter = InGame.Bridge.GetCash();
                paragonTower.upgradeCost = upgradeCost;
                paragonTower.PlayParagonUpgradeSound();
            }), "Yes", new Action(() =>
            {
                var paragonTower = CompletePlacement();

                ParagonomicsMod.OnDegreeChanged(paragonTower);

            }), "No", Popup.TransitionAnim.Scale, (int) cashBefore, int.MaxValue, (int) towerCost);

        return;

        ParagonTower CompletePlacement()
        {
            TowerSelectionMenu.instance.DeselectTower();
            ParagonomicsMod.FinishPopup();
            base.OnPlace(at, towerModelFake, hoveredTower, towerCost);

            return hoveredTower.GetTowerBehavior<ParagonTower>();
        }*/
    }

    public static UpgradeModel GetParagonUpgrade(GameModel gameModel, string id)
    {
        var towerId = id.Split("_").Last();

        var tower = gameModel.GetTowerWithName(towerId);

        var honoraryParagonCost = gameModel.GetTowerWithName(TowerID<HonoraryParagon>()).cost;

        if (tower.IsHero())
        {
            var upgrades = gameModel
                .GetHeroWithNameAndLevel(tower.baseId, 20)
                .appliedUpgrades
                .Select(gameModel.GetUpgrade)
                .ToArray();

            return new UpgradeModel(tower.baseId,
                (int) (honoraryParagonCost +
                       HonoraryParagonsMod.InternalCostFactor * upgrades.Sum(model => model.xpCost) / 2), 0,
                tower.icon, 0, 0, 0, "", "");
        }
        else
        {
            var upgrades = tower.appliedUpgrades.Select(gameModel.GetUpgrade).ToArray();

            var result = upgrades.MaxBy(upgrade => upgrade.tier)!.Duplicate();
            result.cost = (int) (HonoraryParagonsMod.InternalCostFactor *
                                 upgrades.Aggregate(tower.cost + honoraryParagonCost,
                                     (cost, model) => cost + model.cost));

            return result;
        }
    }

    public static void Paragonify(TowerModel towerModel)
    {
        towerModel.isParagon = true;

        var paragonTower = Game.instance.model
            .GetParagonTower(TowerType.DartMonkey)
            .GetBehavior<ParagonTowerModel>()
            .Duplicate();

        foreach (var displayDegreePath in paragonTower.displayDegreePaths)
        {
            displayDegreePath.SetName(towerModel.name);
            displayDegreePath.assetPath = towerModel.display;
        }

        towerModel.AddBehavior(paragonTower);

        var paragonUpgrade = nameof(HonoraryParagon) + "_" + towerModel.name;

        var upgrades = towerModel.appliedUpgrades.ToList();

        while (upgrades.Count < 6)
        {
            upgrades.Add(paragonUpgrade);
        }
        upgrades[5] = paragonUpgrade;

        towerModel.appliedUpgrades = upgrades.ToArray();

        towerModel.GetDescendants<DamageModel>().ForEach(model =>
        {
            model.immuneBloonProperties = BloonProperties.None;
        });

        towerModel.GetDescendants<ProjectileModel>().ForEach(projectile =>
        {
            if (projectile.HasBehavior("BonusBossDamage", out DamageModifierForTagModel normalBoss) &&
                projectile.HasBehavior("FinalEliteBossDamageBonus", out DamageModifierForTagModel eliteBoss))
            {
                eliteBoss.damageMultiplier -= 1 / normalBoss.damageMultiplier;
            }
        });

        towerModel.GetDescendants<FilterInvisibleModel>().ForEach(invis =>
        {
            invis.isActive = false;
        });
    }
}

public class HonoraryParagonIcon : ModBuffIcon
{
    public override string Icon => VanillaSprites.ParagonPip;
}