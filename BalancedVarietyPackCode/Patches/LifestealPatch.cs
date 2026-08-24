using BalancedVarietyPack.BalancedVarietyPackCode.Keywords;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Monsters;

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
        
        // only apply if damage was dealt by the player or by Osty
        if (dealer?.Player is null && dealer?.Monster is not Osty) return;

        // only apply if an attack that has it dealt damage
        if (cardSource is not { Type: CardType.Attack }) return;
        if (!cardSource.Keywords.Contains(ModKeywords.Lifesteal)) return;

        // only apply if an enemy took damage
        // only heal if the dealer is still alive
        if (target.Side == dealer.Side || dealer.IsDead) return;
        
        // heal unblocked damage only and prevent overheal
        int missingHealth = dealer.MaxHp - dealer.CurrentHp;
        int healing = Math.Clamp(
            results.UnblockedDamage, min: 0, max: missingHealth);

        if (healing <= 0) return;
        await CreatureCmd.Heal(dealer, healing);
    }
}
