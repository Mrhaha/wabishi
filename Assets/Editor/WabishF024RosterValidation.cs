using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public static class WabishF024RosterValidation
{
    private static readonly string[][] Families =
    {
        "PigFarmer MeatPig TradePig SowPig GreedyPig FeedWholesaler".Split(' '),
        "MoneyTurtle TinyTurtle DoubleTurtle LeaderTurtle CaravanTurtle BasicTurtle".Split(' '),
        "Imp DemonBat Demon AbyssSummon Devourer ProgenitorDemon".Split(' '),
        "RefreshPirate RobberyPirate TreasurePirate PirateKing PirateCaptain CrewPirate".Split(' '),
        "Lightfang Crown Enchanter TaxCollector Rewind Duet".Split(' ')
    };
    private static readonly string[] PoolFlags =
    {
        "v02PigFamilyMarketOnly", "v02TurtleAttachmentMarketOnly", "v02DevilFamilyMarketOnly",
        "v02PirateFamilyMarketOnly", "v02NeutralFamilyMarketOnly"
    };
    private const BindingFlags Flags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
    private static readonly Type DemoType = typeof(DiceKingDemo);

    public static void Run()
    {
        var oldRandom = UnityEngine.Random.state;
        var host = new GameObject("F024 isolated roster validation");
        // An inactive component allows the normal config readers to run without loading or clearing a player's save in Awake.
        host.SetActive(false);
        int exitCode = 0;
        try
        {
            UnityEngine.Random.InitState(2430);
            var demo = host.AddComponent<DiceKingDemo>();
            Set(demo, "suppressSave", true);
            foreach (string load in new[] { "LoadGlobalConfigs", "LoadMarketConfigs", "LoadSpellConfigs", "LoadDiceTypeTooltipConfigs", "LoadKeywordConfigs", "BuildEncounters" })
            {
                Call(demo, load);
            }
            var roster = new HashSet<string>(Families.SelectMany(ids => ids));
            var configs = (IDictionary)Get(demo, "marketDieConfigs");
            var systemAndRoster = new HashSet<string>(roster) { "Basic", "Tribute" };
            Check(systemAndRoster.SetEquals(configs.Keys.Cast<object>().Select(k => k.ToString())), "market config IDs");
            var tooltips = (IDictionary)Get(demo, "diceTypeTooltipConfigs");
            Check(systemAndRoster.SetEquals(tooltips.Keys.Cast<object>().Select(k => k.ToString())), "tooltip IDs");
            Check((bool)Call(demo, "ValidateKeywordTooltipCoverage", Get(demo, "keywordGlossaryConfigs"), Get(demo, "dieKeywordBindings"), Get(demo, "stateKeywordBindings")), "keyword coverage");
            Check(((IDictionary)Get(demo, "keywordGlossaryConfigs")).Count > 0, "keyword glossary loaded");
            Call(demo, "AddStarterDiceBag");
            IList bag = (IList)Get(demo, "dice");
            Check(bag.Count == 6 && bag.Cast<object>().All(d => Get(d, "Type").ToString() == "Basic"), "six starting basics");
            bag.Clear();

            ValidatePools(demo, roster);
            ValidateTributes(demo);
            ValidatePrices(demo);
            ValidateFeedChain(demo);
            Debug.Log("F024 roster, shelf generation, fallback, special dice, purchase/sale and feed-chain checks passed.");
        }
        catch (Exception exception)
        {
            exitCode = 1;
            Debug.LogException(exception);
            if (!Application.isBatchMode) throw;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(host);
            UnityEngine.Random.state = oldRandom;
        }
        if (Application.isBatchMode) EditorApplication.Exit(exitCode);
    }

    private static void ValidatePools(DiceKingDemo demo, HashSet<string> roster)
    {
        Set(demo, "spellMarketMinimumShelves", 0);
        Set(demo, "spellMarketMaximumShelves", 0);
        Set(demo, "marketTestRandomRefresh", false);
        IList offers = (IList)Get(demo, "marketOffers");
        for (int mode = -1; mode < Families.Length; mode++)
        {
            for (int i = 0; i < PoolFlags.Length; i++) Set(demo, PoolFlags[i], i == mode);
            var allowed = mode < 0 ? roster : new HashSet<string>(Families[mode]);
            IEnumerable all = (IEnumerable)Call(demo, "TestRandomMarketDieCandidates", (object)null);
            Check(allowed.SetEquals(all.Cast<object>().Select(config => Get(config, "Type").ToString())), "test-random candidates in mode " + mode);
            Check(allowed.Contains(Call(demo, "FallbackMarketOfferType").ToString()), "empty-pool fallback in mode " + mode);
            // Ordinary refreshes exercise weight, tier pity and the active family restrictions.
            for (int sample = 0; sample < 100; sample++)
            {
                Call(demo, "BuildMarketOffers", sample > 0);
                Check(offers.Count == 3, "three dice shelves");
                foreach (object offer in offers)
                {
                    Check(allowed.Contains(OfferType(offer)), "retired/system dice entered a random shelf");
                    if (OfferType(offer) == "BasicTurtle") Check(AllFaces(Get(offer, "Die"), 1), "basic turtle fixed template");
                }
            }
        }
        foreach (string flag in PoolFlags) Set(demo, flag, false);
        foreach (string family in new[]{"Pig", "Turtle", "Devil", "Pirate"})
        {
            object value = EnumValue("MainFamily", family);
            object offer = Call(demo, "MakeFamilyRestockOffer", value, 0);
            Check(offer != null && roster.Contains(OfferType(offer)) && (bool)Call(demo, "HasMainFamily", Get(Get(offer, "Die"), "Type"), value), "target restock " + family);
        }
        var configs = (IDictionary)Get(demo, "marketDieConfigs");
        configs.Clear();
        Call(demo, "BuildMarketOffers", false);
        Check(offers.Cast<object>().All(offer => roster.Contains(OfferType(offer))), "empty config fallback");
        Call(demo, "BuildFallbackMarketDieConfigs");
        Check(configs.Count == 32, "complete fallback catalog");
        Check(((int[])Call(demo, "DefaultFaces", EnumValue("DieType", "BasicTurtle"))).SequenceEqual(new[]{1,1,1,1,1,1}), "fixed default faces");
    }

    private static void ValidateTributes(DiceKingDemo demo)
    {
        Add(demo, "AbyssSummon");
        for (int i = 0; i < 5; i++) Add(demo, "Basic");
        Set(demo, "chapterGold", 20);
        IList offers = (IList)Get(demo, "marketOffers");
        for (int sample = 0; sample < 12; sample++)
        {
            Call(demo, "EnterMarket", false);
            var tributes = offers.Cast<object>().Where(offer => OfferType(offer) == "Tribute").ToList();
            Check(tributes.Count >= 1 && tributes.Count <= 3, "abyss entry count");
            Check(tributes.All(offer => AllFaces(Get(offer, "Die"), 1)), "unbuffed tribute faces");
            Check((bool)Call(demo, "CanBuyMarketOffer", tributes[0]) &&
                ((string)Call(demo, "MarketBuyButtonText", tributes[0])).Contains("奉献"), "full bag may feed a demon");
            Call(demo, "BuildMarketOffers", true);
            Check(offers.Cast<object>().All(offer => OfferType(offer) != "Tribute"), "paid refresh generated tribute");
        }
        ((IList)Get(demo, "dice")).Clear();
    }

    private static void ValidatePrices(DiceKingDemo demo)
    {
        IList offers = (IList)Get(demo, "marketOffers");
        IList bag = (IList)Get(demo, "dice");
        Add(demo, "TaxCollector");
        for (int i = 0; i < 3; i++) ReplaceOffer(demo, offers, i, "BasicTurtle", 4);
        Check(offers.Cast<object>().Sum(item => (int)Call(demo, "CurrentOfferPrice", item)) == 9, "three individual tax discounts");
        Add(demo, "PirateCaptain");
        ReplaceOffer(demo, offers, 0, "RefreshPirate", 7);
        ReplaceOffer(demo, offers, 1, "RefreshPirate", 7);
        Set(demo, "chapterGold", 20);
        Check((int)Call(demo, "CurrentOfferPrice", offers[0]) == 3, "tax before pirate half price");
        Call(demo, "BuyOffer", 0);
        Check((int)Get(demo, "chapterGold") == 17 && bag.Count == 3, "actual purchase cost");
        Check((int)Call(demo, "CurrentOfferPrice", offers[1]) == 6, "one half-price credit");
        Set(demo, "selectedMarketDieIndex", 2);
        Call(demo, "SellSelectedMarketDie");
        Check((int)Get(demo, "chapterGold") == 20 && bag.Count == 2, "pirate sale restores paid price");
        Set(demo, "tavernBaseMinimumBonus", 1);
        Set(demo, "marketTemporaryMinimumBonus", 2);
        object turtle = Get(offers[2], "Die");
        Call(demo, "ApplyCurrentTavernMinimum", turtle);
        Check(AllFaces(turtle, 4), "basic turtle shows both market buffs");
        Call(demo, "BuyOffer", 2);
        Check(AllFaces(bag[2], 2), "bought basic turtle retains only the permanent market buff");
        Set(demo, "selectedMarketDieIndex", 2);
        Call(demo, "SellSelectedMarketDie");
        Check((int)Get(demo, "chapterGold") == 18, "pay three for basic turtle, sell for one despite buffs");
        Set(demo, "tavernBaseMinimumBonus", 0);
        Set(demo, "marketTemporaryMinimumBonus", 0);
        bag.Clear();
    }

    private static void ValidateFeedChain(DiceKingDemo demo)
    {
        Set(demo, "feedQuality", 1);
        object meat = Add(demo, "MeatPig");
        object greedy = Add(demo, "GreedyPig");
        object secondGreedy = Add(demo, "GreedyPig");
        object neutral = Add(demo, "Crown");
        Call(demo, "BeginGameplayChain", "F024 feed");
        Call(demo, "BeginGameplayChainRoot", meat, 0, "饲料", -1);
        Call(demo, "GrantFeed", meat, 1, "饲料", null, true, null, false, -1);
        Call(demo, "DrainGameplayChainResponseWaves");
        Call(demo, "EndGameplayChainRoot", "complete");
        Call(demo, "EndGameplayChain", "complete");
        Check(AllFaces(meat, 3) && (int)Get(meat, "FeedCount") == 2, "meat pig doubles real faces and portions");
        Check(AllFaces(greedy, 2) && AllFaces(secondGreedy, 2), "two greedy pigs each follow once");
        Call(demo, "GrantFeed", neutral, 1, "饲料", null, true, null, false, -1);
        Check(AllFaces(neutral, 2) && AllFaces(greedy, 2) && AllFaces(secondGreedy, 2), "neutral feeding does not trigger greedy pigs");
    }

    private static bool AllFaces(object die, int expected)
    {
        int[] faces = (int[])Get(die, "Faces");
        return faces.Length == 6 && faces.All(value => value == expected);
    }

    private static string OfferType(object offer) { return Get(Get(offer, "Die"), "Type").ToString(); }
    private static object EnumValue(string type, string key) { return Enum.Parse(DemoType.GetNestedType(type, BindingFlags.NonPublic), key); }
    private static object NewDie(DiceKingDemo demo, string key)
    {
        return Call(demo, "NewDie", key, EnumValue("DieType", key), new[]{1,1,1,1,1,1});
    }
    private static object Add(DiceKingDemo demo, string key)
    {
        object die = NewDie(demo, key);
        Call(demo, "AddDie", die);
        return die;
    }
    private static void ReplaceOffer(DiceKingDemo demo, IList offers, int index, string type, int price)
    {
        object offer = Call(demo, "EmptyMarketOffer");
        Set(offer, "Die", NewDie(demo, type));
        Set(offer, "Price", price);
        Set(offer, "Kind", EnumValue("MarketOfferKind", "Die"));
        offers[index] = offer;
    }
    private static object Get(object source, string name) { return source.GetType().GetField(name, Flags).GetValue(source); }
    private static void Set(object source, string name, object value) { source.GetType().GetField(name, Flags).SetValue(source, value); }
    private static object Call(DiceKingDemo demo, string name, params object[] args)
    {
        var method = DemoType.GetMethods(Flags).Single(m => m.Name == name && m.GetParameters().Length == args.Length);
        try { return method.Invoke(demo, args); }
        catch (TargetInvocationException e) { throw new InvalidOperationException(name, e.InnerException); }
    }
    private static void Check(bool passed, string detail) { if (!passed) throw new InvalidOperationException("F024 failed: " + detail); }
}
