using BTD_Mod_Helper.Api.Display;
using BTD_Mod_Helper.Api.Enums;
using BTD_Mod_Helper.Api.Towers;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Towers;
using Newtonsoft.Json.Linq;

namespace HonoraryParagons;

public class HonoraryParagonMutator : ModMutator
{
    public override BuffIndicatorModel BuffIcon => GetInstance<HonoraryParagonIcon>();

    public override int Priority => 50; // Before other stuff, but after Paths++

    public override string MutatorId => nameof(HonoraryParagon);

    public override bool Mutate(Model baseModel, Model model, JToken data)
    {
        HonoraryParagon.Paragonify(model.Cast<TowerModel>());
        return true;
    }
}

public class HonoraryParagonIcon : ModBuffIcon
{
    public override string Icon => VanillaSprites.ParagonPip;
}