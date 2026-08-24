using BalancedVarietyPack.BalancedVarietyPackCode.Keywords;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Enchantments;


public class Leech : CustomEnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [ HoverTipFactory.FromKeyword(ModKeywords.Lifesteal) ];
    
    // glow gold when Lifesteal is available
    public override bool ShouldGlowGold => Status == EnchantmentStatus.Normal;
    
    public override bool CanEnchant(CardModel card)
    {
        // since Leech applies Lifesteal it only works for attacks
        return base.CanEnchant(card) && card.Type == CardType.Attack;
    }
    
    protected override void OnEnchant()
    {
        CardCmd.ApplyKeyword(Card, ModKeywords.Lifesteal);
    }
    
    // disable Leech (i.e. remove Lifesteal keyword) after play
    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, 
        CardPlay cardPlay)
    {
        // ensure this exact card instance with active enchantment
        if (cardPlay.Card == Card && Status != EnchantmentStatus.Disabled) 
        { DisableEnchantment(); }
        
        return Task.CompletedTask;
    }
    
    // disable Leech (i.e. remove Lifesteal keyword) after turn end
    public override Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext, 
        CombatSide side, 
        IEnumerable<Creature> participants)
    {
        // only process player turn belonging to card owner
        if (side != CombatSide.Player || !participants.Contains(Card.Owner.Creature))
        { return Task.CompletedTask; }
        
        // ensure card instance is in hand with active enchantment
        if (Card.Pile?.Type == PileType.Hand && Status != EnchantmentStatus.Disabled)
        { DisableEnchantment(); }

        return Task.CompletedTask;
    }
    
    private void DisableEnchantment()
    {
        CardCmd.RemoveKeyword(Card, ModKeywords.Lifesteal);
        Status = EnchantmentStatus.Disabled;
    }
}
