using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Keywords;


public static class ModKeywords
{
    // an attack that heals unblocked damage
    // should only be added to attack cards!
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Lifesteal;
}
