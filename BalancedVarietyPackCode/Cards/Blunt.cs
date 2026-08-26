using BalancedVarietyPack.BalancedVarietyPackCode.Powers;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Cards;


[Pool(typeof(ColorlessCardPool))]
public class Blunt() : CustomCardModel(
    baseCost: 0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [ CardKeyword.Exhaust ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [ HoverTipFactory.FromPower<HighPower>() ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
        [ new EnergyVar(2), new PowerVar<HighPower>(20) ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars["Energy"].IntValue, Owner);
        
        await PowerCmd.Apply<HighPower>(
            choiceContext,
            target: cardPlay.Player.Creature,
            amount: DynamicVars["HighPower"].BaseValue,
            applier: null,
            cardSource: this);
    }
    
    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}
