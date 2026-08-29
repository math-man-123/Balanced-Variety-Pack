using BalancedVarietyPack.BalancedVarietyPackCode.Enchantments;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Vfx;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Relics;


[Pool(typeof(EventRelicPool))]
public class BloodyCrown : CustomRelicModel
{
    private const string CombinedIconPath =
        "res://BalancedVarietyPack/images/relics/bloody_crown.png";
    
    public override string PackedIconPath => CombinedIconPath;
    protected override string BigIconPath => CombinedIconPath;
    protected override string PackedIconOutlinePath => CombinedIconPath;

    public override RelicRarity Rarity => RelicRarity.Ancient;
    public override bool HasUponPickupEffect => true;
    
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [ 
        ..HoverTipFactory.FromEnchantment<Leech>(),
        HoverTipFactory.FromPower<ThornsPower>()
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
    [ 
        new CardsVar(2),
        new PowerVar<ThornsPower>(2)
    ];

    // ensure minimum number of Attacks
    public bool CanBePickedUp(Player player) =>
        player.Deck.Cards.Count(card => card.Type == CardType.Attack) 
        >= DynamicVars["Cards"].IntValue;
    
    // enchant Attacks with Leech
    public override async Task AfterObtained()
    {
        CardSelectorPrefs enchantPrompt =
            new (CardSelectorPrefs.EnchantSelectionPrompt, DynamicVars["Cards"].IntValue);
        
        IEnumerable<CardModel> cardSelection = await CardSelectCmd.FromDeckForEnchantment(
            prefs: enchantPrompt, player: Owner, 
            enchantment: ModelDb.Enchantment<Leech>(), amount: 1);
        
        foreach (CardModel card in cardSelection)
        {
            CardCmd.Enchant<Leech>(card, amount: 1);
            NCardEnchantVfx? nCardEnchantVfx = NCardEnchantVfx.Create(card);
            
            if (nCardEnchantVfx is null) continue;
            NRun.Instance?.GlobalUi.CardPreviewContainer.AddChildSafely(nCardEnchantVfx);
        }
    }
    
    // at the start of combat apply Thorns
    public override async Task BeforeSideTurnStart(
        PlayerChoiceContext choiceContext, 
        CombatSide side, 
        IReadOnlyList<Creature> participants, 
        ICombatState combatState)
    {
        if (Owner.PlayerCombatState is null) return;
        if (participants.Contains(Owner.Creature) && Owner.PlayerCombatState.TurnNumber <= 1)
        {
            Flash();
            await PowerCmd.Apply<ThornsPower>(
                choiceContext, 
                targets: combatState.HittableEnemies, 
                amount: DynamicVars["ThornsPower"].BaseValue, 
                applier: Owner.Creature, 
                cardSource: null);
        }
    }
}
