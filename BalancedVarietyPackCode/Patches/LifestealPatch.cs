using BalancedVarietyPack.BalancedVarietyPackCode.Keywords;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Patches;


// patching a hook is needed to ensure the Lifesteal 
// keyword can be added to any cards (including vanilla)
[HarmonyPatch(typeof(Hook), nameof(Hook.AfterDamageGiven))]
internal static class LifestealPatch
{
    [HarmonyPostfix]
    private static void HookPostfix(
        Creature? dealer,
        DamageResult results,
        Creature target,
        CardModel? cardSource,
        ref Task __result)
    {
        __result = ApplyLifesteal(
            dealer, results, target, cardSource, __result);
    }

    
    private static async Task ApplyLifesteal(
        Creature? dealer,
        DamageResult results,
        Creature target,
        CardModel? cardSource,
        Task original)
    {
        await original;
        
        // Lifesteal should only apply if damage was dealt by the player
        if (dealer?.Player is null) return;

        // Lifesteal should only apply if an attack that has it dealt damage
        if (cardSource is not { Type: CardType.Attack }) return;
        if (!cardSource.Keywords.Contains(ModKeywords.Lifesteal)) return;

        // Lifesteal should only apply if an enemy took damage
        if (target.Side == dealer.Side) return;

        // Lifesteal should only heal for unblocked damage
        // dealt and only if the player is still alive
        int healing = results.UnblockedDamage;
        if (healing <= 0 || dealer.IsDead) return;

        await CreatureCmd.Heal(dealer, healing);
    }
}
