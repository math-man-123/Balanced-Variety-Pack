using BalancedVarietyPack.BalancedVarietyPackCode.Powers;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Cards;


[Pool(typeof(ColorlessCardPool))]
public class Blunt() : CustomCardModel(
    baseCost: 1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override string? CustomPortraitPath =>
        "res://BalancedVarietyPack/images/card_portraits/blunt.png";
    
    public override IEnumerable<CardKeyword> CanonicalKeywords =>
        [ CardKeyword.Exhaust ];
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips => 
    [ 
        HoverTipFactory.FromPower<ExtraTurnPower>(),
        HoverTipFactory.FromPower<HighPower>()
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [
        new PowerVar<ExtraTurnPower>(1),
        new PowerVar<HighPower>(20)
    ];
    
    protected override async Task OnPlay(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ExtraTurnPower>(
            choiceContext,
            target: cardPlay.Player.Creature,
            amount: DynamicVars["ExtraTurnPower"].BaseValue,
            applier: null,
            cardSource: this);
        
        await PowerCmd.Apply<HighPower>(
            choiceContext,
            target: cardPlay.Player.Creature,
            amount: DynamicVars["HighPower"].IntValue,
            applier: null,
            cardSource: this);
    }
    
    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
