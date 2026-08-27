using System.Reflection;
using BalancedVarietyPack.BalancedVarietyPackCode.Effects;
using BaseLib.Abstracts;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Powers;

    
public class HighPower : CustomPowerModel
{
    private const string CombinedIconPath = 
        "res://BalancedVarietyPack/images/powers/high.png";

    public override string? CustomPackedIconPath => CombinedIconPath;
    public override string? CustomBigIconPath => CombinedIconPath;
    
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    private const int MinAmount = 0;
    private const int MaxAmount = 100;
    
    // clamps amount and updates VFX
    private void UpdateAmountAndVfx()
    {
        SetAmount(Math.Clamp(Amount, MinAmount, MaxAmount));

        if (!LocalContext.IsMe(Owner)) return;
        if (Amount > 0) TrippyVfx.Show(Amount);
        else TrippyVfx.Hide();
    }
    
    public override Task AfterApplied(
        Creature? applier, CardModel? cardSource)
    {
        UpdateAmountAndVfx();
        return Task.CompletedTask;
    }

    public override Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext,
        PowerModel power,
        decimal change,
        Creature? applier,
        CardModel? cardSource)
    {
        if (power == this) UpdateAmountAndVfx();
        return Task.CompletedTask;
    }
    
    public override Task AfterRemoved(Creature oldOwner)
    {
        if (LocalContext.IsMe(oldOwner)) TrippyVfx.Hide();
        return Task.CompletedTask;
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
