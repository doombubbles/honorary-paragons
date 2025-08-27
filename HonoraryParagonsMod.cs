using MelonLoader;
using BTD_Mod_Helper;
using BTD_Mod_Helper.Api.ModOptions;
using HonoraryParagons;

[assembly: MelonInfo(typeof(HonoraryParagonsMod), ModHelperData.Name, ModHelperData.Version, ModHelperData.RepoOwner)]
[assembly: MelonGame("Ninja Kiwi", "BloonsTD6")]

namespace HonoraryParagons;

public class HonoraryParagonsMod : BloonsTD6Mod
{
    public static readonly ModSettingFloat InternalCostFactor = new(2)
    {
        description =
            "The internal multiplier that Honorary Paragons use for determining what their pretend paragon upgrade cost is for the purposes of calculating power investments. " +
            "Higher numbers mean it takes more money to add additional power."
    };
}