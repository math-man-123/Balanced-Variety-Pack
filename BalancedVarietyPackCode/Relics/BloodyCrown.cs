using BalancedVarietyPack.BalancedVarietyPackCode.Cards;
using BalancedVarietyPack.BalancedVarietyPackCode.Enchantments;
using BaseLib.Abstracts;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Runs.History;

namespace BalancedVarietyPack.BalancedVarietyPackCode.Relics;


[Pool(typeof(EventRelicPool))]
public class BloodyCrown : CustomRelicModel
{
    public override RelicRarity Rarity => RelicRarity.Ancient;
    public override bool HasUponPickupEffect => true;
    
    // hide some basic hovertips to not cover the full screen
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [ 
        ..HoverTipFactory.FromCardWithCardHoverTips<Despair>()
            .Except([
                HoverTipFactory.FromKeyword(CardKeyword.Unplayable),
                HoverTipFactory.FromKeyword(CardKeyword.Ethereal),
                HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
            ]),
        ..HoverTipFactory.FromEnchantment<Leech>()
    ];
    
    protected override IEnumerable<DynamicVar> CanonicalVars => 
        [ new CardsVar(3) ];

    // 1. custom card reward (3 Leech Attacks)
    // 2. adds one Despair to player Deck
    public override async Task AfterObtained()
    {
        Func<CardModel, bool> isCommonAttack =
            card => card is { Rarity: CardRarity.Common, Type: CardType.Attack };
        
        // not upgraded common attacks of player character
        CardCreationOptions options = new CardCreationOptions(
            cardPools: [ Owner.Character.CardPool ],
            source: CardCreationSource.Other,
            rarityOdds: CardRarityOddsType.Uniform,
            cardPoolFilter: isCommonAttack
        ).WithFlags(CardCreationFlags.NoUpgradeRoll);
        
        List<CardModel> cardModels = CardFactory.CreateForReward(
            Owner, DynamicVars.Cards.IntValue, options)
            .Select(result => result.Card).ToList();

        // check if all generated cards can be enchanted with Leech
        Leech leech = ModelDb.Enchantment<Leech>();
        if (cardModels.Any(card => !leech.CanEnchant(card)))
        {
            const string msg = "Bloody Crown relic generated invalid targets for Leech enchantment!";
            MainFile.Logger.Error(msg); throw new InvalidOperationException(msg);
        }
        
        // enchant all generated attacks and let player choose or skip
        cardModels.ForEach(card => CardCmd.Enchant<Leech>(card, 1));
        CardModel? chosenCard = await CardSelectCmd.FromChooseACardScreen(
            new BlockingPlayerChoiceContext(), cardModels, Owner, canSkip: true);

        // selected card and Despair is added to player Deck
        List<CardModel> cardList = [Owner.RunState.CreateCard<Despair>(Owner)];
        if (chosenCard != null) cardList.Insert(0, chosenCard);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(cardList, PileType.Deck));
        
        // add each skipped card on map node comment
        foreach (CardModel card in cardModels.Where(card => card != chosenCard))
        {
            Owner.RunState.CurrentMapPointHistoryEntry?
                .GetEntry(Owner.NetId).CardChoices
                .Add(new CardChoiceHistoryEntry(card, wasPicked: false));
        }
    }
}
