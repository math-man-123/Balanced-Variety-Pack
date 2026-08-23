using BalancedVarietyPack.BalancedVarietyPackCode.Keywords;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Enchantments;


public class Leech : CustomEnchantmentModel
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        [ HoverTipFactory.FromKeyword(ModKeywords.Lifesteal) ];

    
    protected override void OnEnchant()
    {
        CardCmd.ApplyKeyword(Card, ModKeywords.Lifesteal);
    }

    
    public override Task AfterCardPlayed(
        PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // ensure this exact card instance with active enchantment
        if (cardPlay.Card != Card || Status == EnchantmentStatus.Disabled) 
        { return Task.CompletedTask; }
        
        // Leech grants Lifesteel once per combat (like Glam)
        CardCmd.RemoveKeyword(Card, ModKeywords.Lifesteal);
        Status = EnchantmentStatus.Disabled;
        
        return Task.CompletedTask;
    }
}
