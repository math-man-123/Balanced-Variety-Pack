using System.Reflection;
using BaseLib.Abstracts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Powers;

    
public class HighPower : CustomPowerModel
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private const int MinAmount = 0;
    private const int MaxAmount = 100;
    
    public override Task AfterApplied(
        Creature? applier, CardModel? cardSource)
    {
        // ensure correct range on initial application
        ClampAmount();
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal change,
        Creature? applier,
        CardModel? cardSource)
    {
        // ensure changing amount stays in correct range
        if (power == this) ClampAmount();
        return Task.CompletedTask;
    }

    private void ClampAmount()
    {
        int clamped = Math.Clamp(Amount, MinAmount, MaxAmount);
        if (clamped != Amount) SetAmount(clamped);
    }
    
    // this is needed to change cardPlay.Target, since it is init only
    private static readonly MethodInfo TargetSetter =
        AccessTools.PropertySetter(typeof(CardPlay), nameof(CardPlay.Target));
    
    public override Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature != Owner) return Task.CompletedTask;
        
        // roll rng if target should be randomized or not
        int pivot = CombatState.RunState.Rng.CombatTargets.NextInt(100);
        if (pivot >= Amount) return Task.CompletedTask;
        
        // get all possible targets (alive enemies and allies)
        List<Creature> targets = CombatState.Creatures
            .Where(creature => creature.IsAlive).ToList();
        if (targets.Count == 0) return Task.CompletedTask;
        
        // select random target or fallback to original one on fail
        Creature? randomTarget = 
            CombatState.RunState.Rng.CombatTargets.NextItem(targets) ?? cardPlay.Target;
        if (randomTarget == null) return Task.CompletedTask;
        
        TargetSetter.Invoke(cardPlay, [randomTarget]);
        return Task.CompletedTask;
    }
}
