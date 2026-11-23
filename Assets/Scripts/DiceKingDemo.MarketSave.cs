using System;
using UnityEngine;

public sealed partial class DiceKingDemo
{
    [Serializable]
    private sealed class SavedMarket
    {
        public bool Chapter;
        public bool HalfPriceUsed;
        public int TemporaryBonus;
        public int RefreshCount;
        public int RefreshesWithoutHighTier;
        public MarketRoute RecentRoute;
        public SavedShelf[] Shelves;
    }

    [Serializable]
    private sealed class SavedShelf
    {
        public MarketOfferKind Kind;
        public string Key;
        public int Price;
        public int[] Faces;
        public int[] PurchasedFaces;
        public DiceMaterial Material;
        public string Prefixes;
        public string Suffixes;
    }

    private void SaveMarketCheckpoint()
    {
        if (mode != GameMode.InterStageMarket && mode != GameMode.ChapterShop)
        {
            PlayerPrefs.DeleteKey(SavePrefix + "Market");
            return;
        }
        var market = new SavedMarket
        {
            Chapter = currentMarketIsChapter,
            HalfPriceUsed = pirateDiscountUsed,
            TemporaryBonus = marketTemporaryMinimumBonus,
            RefreshCount = currentMarketPaidRefreshCount,
            RefreshesWithoutHighTier = marketRefreshesWithoutHighTier,
            RecentRoute = marketRecentPurchaseRoute,
            Shelves = new SavedShelf[marketOffers.Count]
        };
        for (int i = 0; i < marketOffers.Count; i++)
        {
            MarketOffer offer = marketOffers[i];
            var shelf = new SavedShelf { Kind = offer.Kind, Price = offer.Price };
            if (offer.Kind == MarketOfferKind.Die && offer.Die != null)
            {
                Die die = offer.Die;
                shelf.Key = die.Type.ToString();
                shelf.Faces = die.Faces;
                shelf.PurchasedFaces = die.MarketPurchaseFaces;
                shelf.Material = die.Material;
                shelf.Prefixes = SerializeAffixes(die.PrefixAffixes);
                shelf.Suffixes = SerializeAffixes(die.SuffixAffixes);
            }
            else if (offer.Kind == MarketOfferKind.Spell && offer.Spell != null) shelf.Key = offer.Spell.Type.ToString();
            else if (offer.Kind == MarketOfferKind.CraftingItem && offer.CraftingItem != null) shelf.Key = offer.CraftingItem.Key;
            market.Shelves[i] = shelf;
        }
        PlayerPrefs.SetString(SavePrefix + "Market", JsonUtility.ToJson(market));
    }

    private bool RestoreMarketCheckpoint()
    {
        SavedMarket market;
        try { market = JsonUtility.FromJson<SavedMarket>(PlayerPrefs.GetString(SavePrefix + "Market")); }
        catch (ArgumentException) { return false; }
        if (market == null || market.Shelves == null || market.Shelves.Length != 3) return false;
        marketOffers.Clear();
        foreach (SavedShelf shelf in market.Shelves)
        {
            var offer = EmptyMarketOffer();
            if (shelf != null)
            {
                DieType type;
                SpellType spell;
                if (shelf.Kind == MarketOfferKind.Die && TryParseDieType(shelf.Key, out type)
                    && type != DieType.Basic && shelf.Faces != null && shelf.Faces.Length == 6)
                {
                    offer.Kind = MarketOfferKind.Die;
                    offer.Die = NewDie(TypeName(type), type, shelf.Faces);
                    offer.Die.MarketPurchaseFaces = shelf.PurchasedFaces != null && shelf.PurchasedFaces.Length == 6
                        ? shelf.PurchasedFaces : null;
                    offer.Die.Material = shelf.Material;
                    offer.Die.PrefixAffixes = ParseAffixes(shelf.Prefixes, AffixSlot.Prefix);
                    offer.Die.SuffixAffixes = ParseAffixes(shelf.Suffixes, AffixSlot.Suffix);
                }
                else if (shelf.Kind == MarketOfferKind.Spell && Enum.TryParse(shelf.Key, out spell)
                    && spellDefinitions.TryGetValue(spell, out offer.Spell)) offer.Kind = MarketOfferKind.Spell;
                else if (shelf.Kind == MarketOfferKind.CraftingItem && !string.IsNullOrEmpty(shelf.Key)
                    && craftingItemDefinitions.TryGetValue(shelf.Key, out offer.CraftingItem)) offer.Kind = MarketOfferKind.CraftingItem;
                offer.Price = Mathf.Max(0, shelf.Price);
            }
            marketOffers.Add(offer);
        }
        currentMarketIsChapter = market.Chapter;
        currentMarketPaidRefreshCount = Mathf.Max(0, market.RefreshCount);
        marketRefreshesWithoutHighTier = Mathf.Max(0, market.RefreshesWithoutHighTier);
        marketRecentPurchaseRoute = market.RecentRoute;
        pirateDiscountUsed = market.HalfPriceUsed;
        marketTemporaryMinimumBonus = Mathf.Max(0, market.TemporaryBonus);
        selectedMarketDieIndex = -1;
        mode = market.Chapter ? GameMode.ChapterShop : GameMode.InterStageMarket;
        ResetMarketShelfPresentation();
        EnsureMarketShelfPresentation();
        return true;
    }
}
