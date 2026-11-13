using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WabishHeadlessPlaytestRunner
{
    private enum PlayStrategy
    {
        RandomBaseline,
        ImmediateScore,
        LongTermGrowth,
        Economy,
        RouteBuilder
    }

    private sealed class MarketCandidate
    {
        public int OfferIndex;
        public int SellIndex = -1;
        public double Utility;
        public string Description;
        public string Reason;
    }

    private sealed class RunSummary
    {
        public string RunId;
        public string Strategy;
        public string RouteFocus;
        public int Seed;
        public string Result;
        public int StageReached;
        public int StagesCleared;
        public int Rolls;
        public int Markets;
        public int Decisions;
        public int DiceBought;
        public int DiceSold;
        public int SpellsBought;
        public int SpellsCast;
        public int Refreshes;
        public int LivesRemaining;
        public int Gold;
        public int FinalScore;
        public int ChainWarnings;
        public bool ActionLimitReached;
        public string DiceSummary;
        public string SpellSummary;
        public string Error;
    }

    private const string ActiveKey = "Wabish.HeadlessPlaytest.Active";
    private const string OutputPrefixKey = "Wabish.HeadlessPlaytest.OutputPrefix";
    private const string SeedCountKey = "Wabish.HeadlessPlaytest.SeedCount";
    private const string BaseSeedKey = "Wabish.HeadlessPlaytest.BaseSeed";
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const int MaximumRunActions = 2000;
    private const int MaximumMarketActions = 12;
    private static readonly BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    private static DiceKingDemo demo;
    private static Type demoType;
    private static StreamWriter decisionWriter;
    private static StreamWriter chainWriter;
    private static readonly List<RunSummary> summaries = new List<RunSummary>();
    private static string outputPrefix;
    private static string currentRunId = string.Empty;
    private static string currentStrategy = string.Empty;
    private static int currentSeed;
    private static int currentDecisionIndex;
    private static int currentChainWarnings;
    private static double waitStartedAt;
    private static bool batchStarted;

    [InitializeOnLoadMethod]
    private static void ResumeAfterDomainReload()
    {
        if (!SessionState.GetBool(ActiveKey, false))
        {
            return;
        }

        HookPlayModeState();
        EditorApplication.update -= WaitForPlayableDemo;
        EditorApplication.update += WaitForPlayableDemo;
        waitStartedAt = EditorApplication.timeSinceStartup;
    }

    public static void RunFromCommandLine()
    {
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            outputPrefix = CommandLineValue(args, "-wabishPlaytestOutputPrefix");
            if (string.IsNullOrEmpty(outputPrefix))
            {
                throw new InvalidOperationException("缺少 -wabishPlaytestOutputPrefix 参数。");
            }

            int seedCount = CommandLineInt(args, "-wabishPlaytestSeedCount", 50, 1, 200);
            int baseSeed = CommandLineInt(args, "-wabishPlaytestBaseSeed", 20260828, int.MinValue, int.MaxValue);
            string directory = Path.GetDirectoryName(outputPrefix);
            if (string.IsNullOrEmpty(directory))
            {
                throw new InvalidOperationException("输出路径无效：" + outputPrefix);
            }

            Directory.CreateDirectory(directory);
            SessionState.SetString(OutputPrefixKey, outputPrefix);
            SessionState.SetInt(SeedCountKey, seedCount);
            SessionState.SetInt(BaseSeedKey, baseSeed);
            SessionState.SetBool(ActiveKey, true);
            WriteStatus("状态：后台 Unity 已启动，正在进入无窗口 Play Mode。\n固定种子：" + seedCount + "\n策略数量：5");

            HookPlayModeState();
            if (EditorSceneManager.GetActiveScene().path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            if (!EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = true;
            }
            else
            {
                EditorApplication.update -= WaitForPlayableDemo;
                EditorApplication.update += WaitForPlayableDemo;
                waitStartedAt = EditorApplication.timeSinceStartup;
            }
        }
        catch (Exception exception)
        {
            FailBeforePlayMode(exception);
        }
    }

    private static void HookPlayModeState()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!SessionState.GetBool(ActiveKey, false) || state != PlayModeStateChange.EnteredPlayMode)
        {
            return;
        }

        EditorApplication.update -= WaitForPlayableDemo;
        EditorApplication.update += WaitForPlayableDemo;
        waitStartedAt = EditorApplication.timeSinceStartup;
    }

    private static void WaitForPlayableDemo()
    {
        if (batchStarted || !EditorApplication.isPlaying)
        {
            return;
        }

        demo = UnityEngine.Object.FindObjectOfType<DiceKingDemo>();
        if (demo == null)
        {
            if (EditorApplication.timeSinceStartup - waitStartedAt > 30d)
            {
                EditorApplication.update -= WaitForPlayableDemo;
                FailInPlayMode(new TimeoutException("进入 Play Mode 后 30 秒内未找到 DiceKingDemo。"));
            }
            return;
        }

        EditorApplication.update -= WaitForPlayableDemo;
        batchStarted = true;
        EditorApplication.delayCall += RunBatch;
    }

    private static void RunBatch()
    {
        int exitCode = 0;
        try
        {
            outputPrefix = SessionState.GetString(OutputPrefixKey, outputPrefix);
            int seedCount = Mathf.Clamp(SessionState.GetInt(SeedCountKey, 50), 1, 200);
            int baseSeed = SessionState.GetInt(BaseSeedKey, 20260828);
            demoType = typeof(DiceKingDemo);
            ValidateReflectionContract();
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);

            decisionWriter = new StreamWriter(outputPrefix + "_playtest_decisions.jsonl", false, Utf8NoBom);
            decisionWriter.AutoFlush = true;
            chainWriter = new StreamWriter(outputPrefix + "_playtest_chain.log", false, Utf8NoBom);
            chainWriter.AutoFlush = true;
            Application.logMessageReceived += CaptureRuntimeLog;
            summaries.Clear();

            PlayStrategy[] strategies =
            {
                PlayStrategy.RandomBaseline,
                PlayStrategy.ImmediateScore,
                PlayStrategy.LongTermGrowth,
                PlayStrategy.Economy,
                PlayStrategy.RouteBuilder
            };

            int totalRuns = seedCount * strategies.Length;
            int completedRuns = 0;
            for (int seedIndex = 0; seedIndex < seedCount; seedIndex++)
            {
                int seed = unchecked(baseSeed + seedIndex);
                for (int strategyIndex = 0; strategyIndex < strategies.Length; strategyIndex++)
                {
                    RunSummary summary;
                    try
                    {
                        summary = ExecuteRun(strategies[strategyIndex], seed, seedIndex + 1);
                    }
                    catch (Exception exception)
                    {
                        summary = BuildExceptionSummary(strategies[strategyIndex], seed, seedIndex + 1, exception);
                        exitCode = 1;
                    }

                    summaries.Add(summary);
                    completedRuns++;
                    if (completedRuns == totalRuns || completedRuns % 5 == 0)
                    {
                        WriteStatus(
                            "状态：后台试玩运行中。\n" +
                            "进度：" + completedRuns + " / " + totalRuns + "\n" +
                            "当前：" + summary.RunId + "，" + summary.Result);
                    }
                }
            }

            WriteRunCsv();
            WriteSummaryMarkdown(seedCount, baseSeed);
            WriteStatus(
                "状态：后台试玩完成。\n" +
                "样本：" + summaries.Count + " 局\n" +
                "报告：" + outputPrefix + "_playtest_summary.md");
        }
        catch (Exception exception)
        {
            exitCode = 1;
            WriteFailureReport(exception);
        }
        finally
        {
            Application.logMessageReceived -= CaptureRuntimeLog;
            CloseWriter(decisionWriter);
            CloseWriter(chainWriter);
            decisionWriter = null;
            chainWriter = null;
            SessionState.SetBool(ActiveKey, false);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.update -= WaitForPlayableDemo;
            EditorApplication.Exit(exitCode);
        }
    }

    private static RunSummary ExecuteRun(PlayStrategy strategy, int seed, int seedOrdinal)
    {
        string strategyName = StrategyName(strategy);
        string routeFocus = strategy == PlayStrategy.RouteBuilder ? RouteFocusForSeed(seed) : string.Empty;
        currentRunId = "S" + seedOrdinal.ToString("D4", CultureInfo.InvariantCulture) + "-" + strategyName;
        currentStrategy = strategyName;
        currentSeed = seed;
        currentDecisionIndex = 0;
        currentChainWarnings = 0;

        RunSummary summary = new RunSummary
        {
            RunId = currentRunId,
            Strategy = strategyName,
            RouteFocus = routeFocus,
            Seed = seed,
            Result = "运行中"
        };

        SetDemoField("suppressSave", true);
        UnityEngine.Random.InitState(seed);
        InvokeDemo("StartDefaultBuild");
        SetDemoField("suppressSave", true);
        System.Random decisionRandom = new System.Random(unchecked(seed * 397 ^ ((int)strategy + 1) * 7919));

        int actionGuard = 0;
        while (actionGuard++ < MaximumRunActions)
        {
            summary.StageReached = Mathf.Max(summary.StageReached, DemoInt("stageIndex") + 1);
            string mode = EnumText(DemoField("mode"));
            if (mode == "Win")
            {
                summary.Result = "通关";
                break;
            }
            if (mode == "GameOver" || mode == "MainMenu")
            {
                summary.Result = "失败";
                break;
            }

            if (mode == "Run")
            {
                string phase = EnumText(DemoField("rollPhase"));
                if (phase == "Ready")
                {
                    CastEligibleSpells(strategy, routeFocus, decisionRandom, false, summary);
                    ReorderDice(strategy, routeFocus, decisionRandom, "投掷前", summary);
                    ExecuteFastRoll(strategy, routeFocus, summary);
                    continue;
                }

                if (phase == "StageClear")
                {
                    summary.StagesCleared++;
                    object encounter = InvokeDemo("CurrentEncounter");
                    string before = StateSnapshot();
                    string encounterName = ObjectString(encounter, "Name");
                    WriteDecision(
                        "关卡推进",
                        "进入市场 / 完成本轮",
                        string.IsNullOrEmpty(encounterName) ? "继续" : encounterName + " 已通过",
                        "本关已达标，领取收入后进入既有市场流程。",
                        "",
                        before,
                        before);
                    summary.Decisions++;
                    InvokeDemo("ContinueAfterStageClear", encounter);
                    continue;
                }

                if (phase == "StageFailed")
                {
                    bool retryAvailable = DemoBool("stageFailureRetryAvailable");
                    string before = StateSnapshot();
                    if (retryAvailable)
                    {
                        InvokeDemo("CompleteStageFailureRetry");
                        WriteDecision(
                            "失败续投",
                            "消耗生命后继续本关",
                            "再次出手",
                            "规则要求保留累计分与已提交成长；当前仍有生命。",
                            "返回 Ready。",
                            before,
                            StateSnapshot());
                        summary.Decisions++;
                        continue;
                    }

                    WriteDecision(
                        "本轮结束",
                        "生命归零",
                        "结束本轮",
                        "没有剩余生命，停止继续决策。",
                        "失败。",
                        before,
                        before);
                    summary.Decisions++;
                    summary.Result = "失败";
                    break;
                }

                throw new InvalidOperationException("后台试玩遇到未处理的 Run 阶段：" + phase);
            }

            if (mode == "InterStageMarket" || mode == "ChapterShop")
            {
                HandleMarket(strategy, routeFocus, decisionRandom, summary, mode == "ChapterShop");
                continue;
            }

            throw new InvalidOperationException("后台试玩遇到未处理的模式：" + mode);
        }

        if (actionGuard >= MaximumRunActions)
        {
            summary.ActionLimitReached = true;
            summary.Result = "动作上限";
            WriteDecision(
                "保护停止",
                "动作上限 " + MaximumRunActions,
                "停止本局",
                "防止自动玩家因未知状态无限循环。",
                "需要检查决策日志末尾。",
                StateSnapshot(),
                StateSnapshot());
        }

        if (summary.Result == "运行中")
        {
            summary.Result = "异常结束";
        }

        summary.LivesRemaining = DemoInt("remainingLives");
        summary.Gold = DemoInt("chapterGold");
        summary.FinalScore = DemoInt("currentScore");
        summary.ChainWarnings = currentChainWarnings;
        summary.DiceSummary = DiceSummary();
        summary.SpellSummary = SpellSummary();
        return summary;
    }

    private static void ExecuteFastRoll(PlayStrategy strategy, string routeFocus, RunSummary summary)
    {
        string before = StateSnapshot();
        int scoreBefore = DemoInt("currentScore");
        int livesBefore = DemoInt("remainingLives");
        string order = DiceOrderText();
        WriteDecision(
            "投骰",
            "当前六槽顺序：" + order,
            "按当前顺序投骰",
            RollReason(strategy, routeFocus),
            "等待真实规则结算。",
            before,
            before);
        summary.Decisions++;

        InvokeDemo("BeginShakeRoll");
        InvokeDemo("BeginStopRoll");
        InvokeDemo("FinishShakeRoll");
        InvokeDemo("BeginSettle");
        int resolvedScore = DemoInt("resolvedScore");
        InvokeDemo("CompleteScoreReveal");
        summary.Rolls++;

        int livesAfter = DemoInt("remainingLives");
        string resultPhase = EnumText(DemoField("rollPhase"));
        WriteDecision(
            "结算结果",
            "本手开始累计 " + scoreBefore + "，生命 " + livesBefore,
            "本手新增 " + (resolvedScore - scoreBefore) + " 分",
            "调用真实 ScoreDice、左到右主游标与局部连锁队列；只跳过表现等待。",
            "阶段 " + resultPhase + "，生命 " + livesAfter,
            before,
            StateSnapshot());
        summary.Decisions++;
    }

    private static void HandleMarket(
        PlayStrategy strategy,
        string routeFocus,
        System.Random decisionRandom,
        RunSummary summary,
        bool chapterMarket)
    {
        summary.Markets++;
        CastEligibleSpells(strategy, routeFocus, decisionRandom, true, summary);
        bool refreshed = false;
        int marketActions = 0;
        while (marketActions++ < MaximumMarketActions)
        {
            List<MarketCandidate> candidates = BuildMarketCandidates(strategy, routeFocus, decisionRandom);
            MarketCandidate chosen = ChooseMarketCandidate(candidates, strategy, decisionRandom);
            if (chosen != null && ShouldTakeMarketCandidate(chosen, strategy, decisionRandom))
            {
                string before = StateSnapshot();
                string options = CandidateOptionsText(candidates);
                if (chosen.SellIndex >= 0)
                {
                    object soldDie = DiceList()[chosen.SellIndex];
                    string soldName = DieTypeName(soldDie);
                    SetDemoField("selectedMarketDieIndex", chosen.SellIndex);
                    InvokeDemo("SellSelectedMarketDie");
                    summary.DiceSold++;
                    WriteDecision(
                        "市场卖出",
                        options,
                        "卖出 " + soldName,
                        "为更高估值商品腾出槽位并补足购买预算。",
                        "卖出完成。",
                        before,
                        StateSnapshot());
                    summary.Decisions++;
                    before = StateSnapshot();
                }

                IList offers = MarketOfferList();
                if (chosen.OfferIndex >= 0 && chosen.OfferIndex < offers.Count)
                {
                    object offer = offers[chosen.OfferIndex];
                    string kind = EnumText(ObjectField(offer, "Kind"));
                    bool canBuy = Convert.ToBoolean(InvokeDemo("CanBuyMarketOffer", offer), CultureInfo.InvariantCulture);
                    if (canBuy)
                    {
                        InvokeDemo("BuyOffer", chosen.OfferIndex);
                        if (kind == "Spell")
                        {
                            summary.SpellsBought++;
                        }
                        else if (kind == "Die")
                        {
                            summary.DiceBought++;
                        }

                        WriteDecision(
                            "市场购买",
                            options,
                            chosen.Description,
                            chosen.Reason,
                            "购买完成，原货架留空。",
                            before,
                            StateSnapshot());
                        summary.Decisions++;
                        CastEligibleSpells(strategy, routeFocus, decisionRandom, true, summary);
                        continue;
                    }
                }
            }

            if (!refreshed && ShouldRefreshMarket(strategy, decisionRandom, chapterMarket))
            {
                string before = StateSnapshot();
                int refreshCost = Convert.ToInt32(InvokeDemo("CurrentMarketRefreshNetCost", chapterMarket), CultureInfo.InvariantCulture);
                InvokeDemo("RefreshMarketOffers", chapterMarket);
                summary.Refreshes++;
                summary.Decisions++;
                refreshed = true;
                WriteDecision(
                    "市场刷新",
                    "购买当前商品 / 支付刷新 / 离开",
                    "刷新货架",
                    RefreshReason(strategy, refreshCost),
                    "生成一批新货架。",
                    before,
                    StateSnapshot());
                CastEligibleSpells(strategy, routeFocus, decisionRandom, true, summary);
                continue;
            }

            break;
        }

        CastEligibleSpells(strategy, routeFocus, decisionRandom, true, summary);
        ReorderDice(strategy, routeFocus, decisionRandom, "离开市场前", summary);
        string leaveBefore = StateSnapshot();
        WriteDecision(
            "离开市场",
            "继续购买 / 刷新 / 离开",
            "离开市场",
            "没有剩余高于阈值的购买或刷新决策，保留金币进入下一关。",
            "开始真实离场队列。",
            leaveBefore,
            leaveBefore);
        summary.Decisions++;
        FastForwardMarketLeave();
    }

    private static List<MarketCandidate> BuildMarketCandidates(
        PlayStrategy strategy,
        string routeFocus,
        System.Random decisionRandom)
    {
        List<MarketCandidate> candidates = new List<MarketCandidate>();
        IList offers = MarketOfferList();
        IList dice = DiceList();
        int currentGold = DemoInt("chapterGold");
        int worstDieIndex = dice.Count > 1 ? WorstDieIndex(strategy, routeFocus) : -1;
        double worstDieUtility = worstDieIndex >= 0 ? DieUtility(dice[worstDieIndex], strategy, routeFocus) : 0d;
        int worstSellPrice = worstDieIndex >= 0
            ? Convert.ToInt32(InvokeDemo("SellPrice", dice[worstDieIndex]), CultureInfo.InvariantCulture)
            : 0;

        for (int offerIndex = 0; offerIndex < offers.Count; offerIndex++)
        {
            object offer = offers[offerIndex];
            if (offer == null)
            {
                continue;
            }

            string kind = EnumText(ObjectField(offer, "Kind"));
            if (kind == "Empty")
            {
                continue;
            }

            int price = Convert.ToInt32(InvokeDemo("CurrentOfferPrice", offer), CultureInfo.InvariantCulture);
            bool canBuyNow = Convert.ToBoolean(InvokeDemo("CanBuyMarketOffer", offer), CultureInfo.InvariantCulture);
            if (kind == "Spell")
            {
                object spell = ObjectField(offer, "Spell");
                if (spell == null || !canBuyNow)
                {
                    continue;
                }

                string spellType = EnumText(ObjectField(spell, "Type"));
                double utility = SpellUtility(spellType, strategy, routeFocus) - price * GoldCostWeight(strategy);
                candidates.Add(new MarketCandidate
                {
                    OfferIndex = offerIndex,
                    Utility = strategy == PlayStrategy.RandomBaseline ? decisionRandom.NextDouble() * 10d : utility,
                    Description = "购买法术「" + ObjectString(spell, "DisplayName") + "」(" + price + " 金)",
                    Reason = SpellPurchaseReason(spellType, strategy, routeFocus, price)
                });
                continue;
            }

            if (kind != "Die")
            {
                continue;
            }

            object offeredDie = ObjectField(offer, "Die");
            if (offeredDie == null)
            {
                continue;
            }

            double offerUtility = DieUtility(offeredDie, strategy, routeFocus);
            int sellIndex = -1;
            double netUtility = offerUtility - price * GoldCostWeight(strategy);
            if (!canBuyNow)
            {
                if (worstDieIndex < 0 || currentGold + worstSellPrice < price)
                {
                    continue;
                }

                sellIndex = worstDieIndex;
                netUtility = offerUtility - worstDieUtility - Math.Max(0, price - worstSellPrice) * GoldCostWeight(strategy);
            }

            candidates.Add(new MarketCandidate
            {
                OfferIndex = offerIndex,
                SellIndex = sellIndex,
                Utility = strategy == PlayStrategy.RandomBaseline ? decisionRandom.NextDouble() * 10d : netUtility,
                Description = "购买骰子「" + DieTypeName(offeredDie) + "」(" + price + " 金)",
                Reason = DiePurchaseReason(offeredDie, strategy, routeFocus, price, sellIndex >= 0)
            });
        }

        candidates.Sort(delegate(MarketCandidate left, MarketCandidate right)
        {
            return right.Utility.CompareTo(left.Utility);
        });
        return candidates;
    }

    private static MarketCandidate ChooseMarketCandidate(
        List<MarketCandidate> candidates,
        PlayStrategy strategy,
        System.Random decisionRandom)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return null;
        }

        if (strategy == PlayStrategy.RandomBaseline)
        {
            return candidates[decisionRandom.Next(candidates.Count)];
        }
        return candidates[0];
    }

    private static bool ShouldTakeMarketCandidate(
        MarketCandidate candidate,
        PlayStrategy strategy,
        System.Random decisionRandom)
    {
        if (candidate == null)
        {
            return false;
        }
        if (strategy == PlayStrategy.RandomBaseline)
        {
            return decisionRandom.NextDouble() >= 0.25d;
        }
        return candidate.Utility >= 1.0d;
    }

    private static bool ShouldRefreshMarket(PlayStrategy strategy, System.Random decisionRandom, bool chapterMarket)
    {
        int cost = Convert.ToInt32(InvokeDemo("CurrentMarketRefreshNetCost", chapterMarket), CultureInfo.InvariantCulture);
        int gold = DemoInt("chapterGold");
        if (cost > gold)
        {
            return false;
        }
        if (strategy == PlayStrategy.Economy && cost > 0 && gold - cost < 8)
        {
            return false;
        }
        if (strategy == PlayStrategy.RandomBaseline)
        {
            return decisionRandom.NextDouble() < 0.45d;
        }
        return cost <= 0 || gold - cost >= (strategy == PlayStrategy.LongTermGrowth ? 2 : 4);
    }

    private static void FastForwardMarketLeave()
    {
        InvokeDemo("BeginMarketLeaveEffects");
        int guard = 0;
        while (DemoBool("marketLeaveEffectSequenceActive") && guard++ < 64)
        {
            bool processed = Convert.ToBoolean(InvokeDemo("ProcessNextMarketLeaveEffect"), CultureInfo.InvariantCulture);
            if (!processed)
            {
                InvokeDemo("CompleteMarketLeave");
            }
        }

        if (DemoBool("marketLeaveEffectSequenceActive"))
        {
            throw new InvalidOperationException("离场效果超过后台试玩保护上限。");
        }
    }

    private static void CastEligibleSpells(
        PlayStrategy strategy,
        string routeFocus,
        System.Random decisionRandom,
        bool marketPhase,
        RunSummary summary)
    {
        IList spellBag = SpellBagList();
        int guard = 0;
        int index = 0;
        while (index < spellBag.Count && guard++ < 16)
        {
            object spellType = spellBag[index];
            object definition = InvokeDemo("SpellDefinitionFor", spellType);
            if (definition == null || !Convert.ToBoolean(InvokeDemo("CanCastSpellNow", definition), CultureInfo.InvariantCulture))
            {
                index++;
                continue;
            }

            string typeName = EnumText(spellType);
            bool marketOnly = typeName == "TargetedRestock" || typeName == "PeakCashout";
            if (marketPhase != marketOnly)
            {
                index++;
                continue;
            }
            if (typeName == "PeakCashout" && !ShouldCastPeakCashout(strategy, decisionRandom))
            {
                index++;
                continue;
            }

            string before = StateSnapshot();
            int beforeCount = spellBag.Count;
            string targetText = string.Empty;
            InvokeDemo("BeginSpellCast", index);
            if (typeName == "EchoCommand" || typeName == "PrecisionCalibration" || typeName == "FeedBag")
            {
                int targetIndex = BestSpellTargetIndex(strategy, routeFocus);
                if (targetIndex < 0)
                {
                    InvokeDemo("CancelSpellCastSelection");
                    index++;
                    continue;
                }
                targetText = "，目标 " + DieTypeName(DiceList()[targetIndex]);
                InvokeDemo("CommitSpellDieTarget", targetIndex);
            }
            else if (typeName == "TargetedRestock")
            {
                object family = BestRestockFamily(strategy, routeFocus, decisionRandom);
                int shelfIndex = WorstNonEmptyShelfIndex(strategy, routeFocus);
                if (family == null || shelfIndex < 0)
                {
                    InvokeDemo("CancelSpellCastSelection");
                    index++;
                    continue;
                }
                targetText = "，目标家族 " + EnumText(family) + "、货架 " + (shelfIndex + 1);
                InvokeDemo("CommitSpellFamily", family);
                InvokeDemo("CommitSpellShelf", shelfIndex);
            }

            bool consumed = spellBag.Count < beforeCount;
            if (!consumed)
            {
                InvokeDemo("CancelSpellCastSelection");
                index++;
                continue;
            }

            summary.SpellsCast++;
            summary.Decisions++;
            WriteDecision(
                "施放法术",
                "法术袋：" + SpellSummary(),
                "施放「" + ObjectString(definition, "DisplayName") + "」" + targetText,
                SpellCastReason(typeName, strategy, routeFocus),
                "根法术事件已结清。",
                before,
                StateSnapshot());
        }
    }

    private static bool ShouldCastPeakCashout(PlayStrategy strategy, System.Random decisionRandom)
    {
        if (strategy == PlayStrategy.Economy || strategy == PlayStrategy.ImmediateScore)
        {
            return true;
        }
        if (strategy == PlayStrategy.RandomBaseline)
        {
            return decisionRandom.NextDouble() < 0.5d;
        }
        return DemoInt("chapterGold") <= 2;
    }

    private static int BestSpellTargetIndex(PlayStrategy strategy, string routeFocus)
    {
        IList dice = DiceList();
        int bestIndex = -1;
        double bestUtility = double.MinValue;
        for (int i = 0; i < dice.Count; i++)
        {
            object die = dice[i];
            if (ObjectBool(die, "Temporary"))
            {
                continue;
            }

            double utility = DieUtility(die, strategy, routeFocus);
            if (utility > bestUtility)
            {
                bestUtility = utility;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    private static object BestRestockFamily(PlayStrategy strategy, string routeFocus, System.Random decisionRandom)
    {
        Type familyType = demoType.GetNestedType("MainFamily", BindingFlags.NonPublic);
        if (familyType == null)
        {
            return null;
        }

        string[] preferred;
        if (strategy == PlayStrategy.LongTermGrowth)
        {
            preferred = new[] { "Pig", "Turtle", "Devil", "Pirate" };
        }
        else if (strategy == PlayStrategy.Economy)
        {
            preferred = new[] { "Pirate", "Turtle", "Devil", "Pig" };
        }
        else if (strategy == PlayStrategy.RouteBuilder || strategy == PlayStrategy.ImmediateScore)
        {
            preferred = new[] { "Turtle", "Pig", "Devil", "Pirate" };
        }
        else
        {
            preferred = new[] { "Pig", "Devil", "Turtle", "Pirate" };
            for (int i = preferred.Length - 1; i > 0; i--)
            {
                int swap = decisionRandom.Next(i + 1);
                string temporary = preferred[i];
                preferred[i] = preferred[swap];
                preferred[swap] = temporary;
            }
        }

        for (int i = 0; i < preferred.Length; i++)
        {
            object family = Enum.Parse(familyType, preferred[i]);
            if (Convert.ToBoolean(InvokeDemo("CanRestockFamily", family), CultureInfo.InvariantCulture))
            {
                return family;
            }
        }
        return null;
    }

    private static int WorstNonEmptyShelfIndex(PlayStrategy strategy, string routeFocus)
    {
        IList offers = MarketOfferList();
        int worstIndex = -1;
        double worstUtility = double.MaxValue;
        for (int i = 0; i < offers.Count; i++)
        {
            object offer = offers[i];
            string kind = EnumText(ObjectField(offer, "Kind"));
            if (kind == "Empty")
            {
                continue;
            }

            double utility = 0d;
            if (kind == "Die")
            {
                utility = DieUtility(ObjectField(offer, "Die"), strategy, routeFocus);
            }
            else if (kind == "Spell")
            {
                object spell = ObjectField(offer, "Spell");
                utility = SpellUtility(EnumText(ObjectField(spell, "Type")), strategy, routeFocus);
            }
            if (utility < worstUtility)
            {
                worstUtility = utility;
                worstIndex = i;
            }
        }
        return worstIndex;
    }

    private static void ReorderDice(
        PlayStrategy strategy,
        string routeFocus,
        System.Random decisionRandom,
        string timing,
        RunSummary summary)
    {
        IList dice = DiceList();
        if (dice.Count <= 1)
        {
            return;
        }

        string beforeOrder = DiceOrderText();
        List<object> reordered = new List<object>();
        for (int i = 0; i < dice.Count; i++)
        {
            reordered.Add(dice[i]);
        }

        if (strategy == PlayStrategy.RandomBaseline)
        {
            for (int i = reordered.Count - 1; i > 0; i--)
            {
                int swap = decisionRandom.Next(i + 1);
                object temporary = reordered[i];
                reordered[i] = reordered[swap];
                reordered[swap] = temporary;
            }
        }
        else
        {
            reordered = BuildTurtleAwareOrder(reordered, strategy, routeFocus);
        }

        bool changed = false;
        for (int i = 0; i < dice.Count; i++)
        {
            if (!ReferenceEquals(dice[i], reordered[i]))
            {
                changed = true;
                break;
            }
        }
        if (!changed)
        {
            return;
        }

        string before = StateSnapshot();
        dice.Clear();
        for (int i = 0; i < reordered.Count; i++)
        {
            dice.Add(reordered[i]);
        }
        string afterOrder = DiceOrderText();
        summary.Decisions++;
        WriteDecision(
            "骰袋排序",
            beforeOrder,
            afterOrder,
            strategy == PlayStrategy.RandomBaseline
                ? timing + "执行随机重排，作为无规划槽位基线。"
                : timing + "调整顺序；优先让可化壳龟紧邻合法宿主，并按策略保留核心骰。",
            "仅改变槽位顺序，不改变实例状态。",
            before,
            StateSnapshot());
    }

    private static List<object> BuildTurtleAwareOrder(List<object> source, PlayStrategy strategy, string routeFocus)
    {
        List<object> absorbable = new List<object>();
        List<object> hosts = new List<object>();
        List<object> residents = new List<object>();
        for (int i = 0; i < source.Count; i++)
        {
            object die = source[i];
            if (IsAbsorbableTurtle(die))
            {
                absorbable.Add(die);
            }
            else if (!IsAnyTurtle(die))
            {
                hosts.Add(die);
            }
            else
            {
                residents.Add(die);
            }
        }

        Comparison<object> comparison = delegate(object left, object right)
        {
            return DieUtility(right, strategy, routeFocus).CompareTo(DieUtility(left, strategy, routeFocus));
        };
        absorbable.Sort(comparison);
        hosts.Sort(comparison);
        residents.Sort(comparison);

        List<object> result = new List<object>();
        int pairs = Mathf.Min(absorbable.Count, hosts.Count);
        for (int i = 0; i < pairs; i++)
        {
            result.Add(absorbable[i]);
            result.Add(hosts[i]);
        }
        for (int i = pairs; i < residents.Count; i++)
        {
            result.Add(residents[i]);
        }
        for (int i = pairs; i < absorbable.Count; i++)
        {
            result.Add(absorbable[i]);
        }
        for (int i = pairs; i < hosts.Count; i++)
        {
            result.Add(hosts[i]);
        }

        // Residents before unmatched turtles are stable scoring bodies and do not block completed pairs.
        if (pairs > 0 && residents.Count > 0)
        {
            int insertionIndex = pairs * 2;
            for (int i = 0; i < residents.Count && !result.Contains(residents[i]); i++)
            {
                result.Insert(insertionIndex++, residents[i]);
            }
        }
        return result;
    }

    private static int WorstDieIndex(PlayStrategy strategy, string routeFocus)
    {
        IList dice = DiceList();
        int nonTurtleHosts = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            if (!IsAnyTurtle(dice[i]))
            {
                nonTurtleHosts++;
            }
        }

        int worstIndex = -1;
        double worstUtility = double.MaxValue;
        for (int i = 0; i < dice.Count; i++)
        {
            object die = dice[i];
            double utility = DieUtility(die, strategy, routeFocus);
            if (strategy != PlayStrategy.RandomBaseline && nonTurtleHosts <= 1 && !IsAnyTurtle(die))
            {
                utility += 100d;
            }
            if (utility < worstUtility)
            {
                worstUtility = utility;
                worstIndex = i;
            }
        }
        return worstIndex;
    }

    private static double DieUtility(object die, PlayStrategy strategy, string routeFocus)
    {
        if (die == null)
        {
            return -100d;
        }

        string type = DieTypeName(die);
        double value = AverageFaces(die);
        value += ObjectInt(die, "FeedValue") * 0.7d;
        IList attachments = ObjectField(die, "TurtleAttachments") as IList;
        if (attachments != null)
        {
            value += attachments.Count * 2.0d;
        }
        if (type == "Basic")
        {
            value -= 2.5d;
        }

        if (strategy == PlayStrategy.ImmediateScore)
        {
            value += TypeBonus(type, ImmediateScoreTypes(), 4.5d);
            value += TypeBonus(type, EconomyTypes(), 0.8d);
        }
        else if (strategy == PlayStrategy.LongTermGrowth)
        {
            value += TypeBonus(type, GrowthTypes(), 6.0d);
            value += TypeBonus(type, SpellTurtleTypes(), 1.5d);
        }
        else if (strategy == PlayStrategy.Economy)
        {
            value += TypeBonus(type, EconomyTypes(), 7.0d);
            value += TypeBonus(type, GrowthTypes(), 1.0d);
        }
        else if (strategy == PlayStrategy.RouteBuilder)
        {
            value += TypeBonus(type, RouteTypes(routeFocus), 8.0d);
            value += TypeBonus(type, GrowthTypes(), 1.0d);
        }
        return value;
    }

    private static double SpellUtility(string type, PlayStrategy strategy, string routeFocus)
    {
        double value;
        switch (type)
        {
            case "PassBoost": value = 7d; break;
            case "ChainDividend": value = 6d; break;
            case "EchoCommand": value = 6d; break;
            case "TargetedRestock": value = 5d; break;
            case "PrecisionCalibration": value = 9d; break;
            case "FeedBag": value = 8d; break;
            case "PeakCashout": value = 5d; break;
            case "OverachieverBounty": value = 5d; break;
            default: value = 0d; break;
        }

        if (strategy == PlayStrategy.LongTermGrowth && (type == "PrecisionCalibration" || type == "FeedBag" || type == "EchoCommand"))
        {
            value += 4d;
        }
        if (strategy == PlayStrategy.Economy && (type == "ChainDividend" || type == "PeakCashout" || type == "OverachieverBounty"))
        {
            value += 4d;
        }
        if (strategy == PlayStrategy.RouteBuilder && routeFocus == "法术壳")
        {
            value += 4d;
        }
        return value;
    }

    private static double GoldCostWeight(PlayStrategy strategy)
    {
        if (strategy == PlayStrategy.Economy)
        {
            return 1.15d;
        }
        if (strategy == PlayStrategy.LongTermGrowth)
        {
            return 0.65d;
        }
        return 0.85d;
    }

    private static string[] ImmediateScoreTypes()
    {
        return new[] { "ThreeLittlePigs", "HeavyArmorTurtle", "RallyTurtle", "ElderTurtle", "Crown", "Airstrike", "Stitch", "PirateKing" };
    }

    private static string[] GrowthTypes()
    {
        return new[] { "PigFarmer", "MeatPig", "SowPig", "GreedyPig", "FeedWholesaler", "TinyTurtle", "DoubleTurtle", "LuckyTurtle", "MagnetTurtle", "CloneTurtle", "RotationTurtle", "Lightfang", "TrainingPirate", "TreasurePirate" };
    }

    private static string[] EconomyTypes()
    {
        return new[] { "TradePig", "Devourer", "MoneyTurtle", "RefreshPirate", "PlunderPirate", "CrewPirate", "PirateCaptain", "TreasurePirate", "RobberyPirate", "BlackMarketImp", "EmptyHoldTurtle" };
    }

    private static string[] SpellTurtleTypes()
    {
        return new[] { "SpellApprenticeTurtle", "ScribeTurtle", "CurseShellTurtle", "GrandSpellTurtle" };
    }

    private static string[] RouteTypes(string routeFocus)
    {
        switch (routeFocus)
        {
            case "深壳":
                return new[] { "GuideTurtle", "DoubleTurtle", "LuckyTurtle", "HeavyArmorTurtle", "GreatTurtle", "MoneyTurtle" };
            case "广壳":
                return new[] { "TinyTurtle", "MagnetTurtle", "RallyTurtle", "RotationTurtle", "MoneyTurtle" };
            case "法术壳":
                return SpellTurtleTypes();
            default:
                return new[] { "RunawayTurtle", "CloneTurtle", "ElderTurtle", "LeaderTurtle", "LegacyDemonTurtle", "EmptyHoldTurtle" };
        }
    }

    private static double TypeBonus(string type, string[] types, double bonus)
    {
        for (int i = 0; i < types.Length; i++)
        {
            if (string.Equals(type, types[i], StringComparison.Ordinal))
            {
                return bonus;
            }
        }
        return 0d;
    }

    private static string StrategyName(PlayStrategy strategy)
    {
        switch (strategy)
        {
            case PlayStrategy.RandomBaseline: return "随机基线";
            case PlayStrategy.ImmediateScore: return "眼前得分";
            case PlayStrategy.LongTermGrowth: return "长期成长";
            case PlayStrategy.Economy: return "经济利息";
            case PlayStrategy.RouteBuilder: return "路线构筑";
            default: return strategy.ToString();
        }
    }

    private static string RouteFocusForSeed(int seed)
    {
        string[] routes = { "深壳", "广壳", "法术壳", "驻场龟群" };
        int index = Math.Abs(seed % routes.Length);
        return routes[index];
    }

    private static string RollReason(PlayStrategy strategy, string routeFocus)
    {
        if (strategy == PlayStrategy.RouteBuilder)
        {
            return "按“" + routeFocus + "”路线安排槽位，之后交给真实随机与结算规则。";
        }
        if (strategy == PlayStrategy.RandomBaseline)
        {
            return "随机基线不追求最优顺序，用于衡量无规划情况下的流程下限。";
        }
        return "当前没有投骰前可继续执行的高价值动作，按策略排序后提交本手。";
    }

    private static string RefreshReason(PlayStrategy strategy, int cost)
    {
        return "当前货架没有达到“" + StrategyName(strategy) + "”策略阈值；刷新费用为 " + cost + " 金，预算允许一次尝试。";
    }

    private static string SpellPurchaseReason(string spellType, PlayStrategy strategy, string routeFocus, int price)
    {
        if (strategy == PlayStrategy.RandomBaseline)
        {
            return "随机基线从当前合法商品中抽取法术；价格 " + price + " 金。";
        }
        return "法术 " + spellType + " 与“" + StrategyName(strategy) + (string.IsNullOrEmpty(routeFocus) ? string.Empty : " / " + routeFocus) + "”目标匹配，价格 " + price + " 金处于可接受范围。";
    }

    private static string DiePurchaseReason(object die, PlayStrategy strategy, string routeFocus, int price, bool replacesDie)
    {
        if (strategy == PlayStrategy.RandomBaseline)
        {
            return "随机基线从当前合法商品中抽取骰子；价格 " + price + " 金" + (replacesDie ? "，随机替换一颗现有骰。" : "。" );
        }
        return "骰子 " + DieTypeName(die) + " 在“" + StrategyName(strategy) + (string.IsNullOrEmpty(routeFocus) ? string.Empty : " / " + routeFocus) + "”评分中高于阈值；价格 " + price + " 金" + (replacesDie ? "，替换当前最低估值骰。" : "。" );
    }

    private static string SpellCastReason(string spellType, PlayStrategy strategy, string routeFocus)
    {
        return "在合法阶段结清 " + spellType + "；目标按“" + StrategyName(strategy) + (string.IsNullOrEmpty(routeFocus) ? string.Empty : " / " + routeFocus) + "”估值选择。";
    }

    private static string CandidateOptionsText(List<MarketCandidate> candidates)
    {
        if (candidates == null || candidates.Count == 0)
        {
            return "无可购买候选";
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < candidates.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(" | ");
            }
            builder.Append(candidates[i].Description);
            builder.Append("，评分 ");
            builder.Append(candidates[i].Utility.ToString("0.00", CultureInfo.InvariantCulture));
        }
        return builder.ToString();
    }

    private static string StateSnapshot()
    {
        object encounter = InvokeDemo("CurrentEncounter");
        return
            "关卡=" + (DemoInt("stageIndex") + 1) +
            ",章节=" + ObjectInt(encounter, "ChapterIndex") +
            ",目标=" + ObjectInt(encounter, "Target") +
            ",累计=" + DemoInt("currentScore") +
            ",金币=" + DemoInt("chapterGold") +
            ",生命=" + DemoInt("remainingLives") +
            ",模式=" + EnumText(DemoField("mode")) +
            ",阶段=" + EnumText(DemoField("rollPhase")) +
            ",骰袋=[" + DiceDetailedText() + "]" +
            ",法术袋=[" + SpellSummary() + "]";
    }

    private static string DiceDetailedText()
    {
        IList dice = DiceList();
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < dice.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(";");
            }
            object die = dice[i];
            builder.Append(i + 1);
            builder.Append(":");
            builder.Append(DieTypeName(die));
            builder.Append("#");
            builder.Append(ObjectInt(die, "Id"));
            builder.Append("{");
            builder.Append(FacesText(die));
            builder.Append("}");
            int feed = ObjectInt(die, "FeedValue");
            if (feed != 0)
            {
                builder.Append("饲+");
                builder.Append(feed);
            }
            IList attachments = ObjectField(die, "TurtleAttachments") as IList;
            if (attachments != null && attachments.Count > 0)
            {
                builder.Append("壳x");
                builder.Append(attachments.Count);
            }
        }
        return builder.ToString();
    }

    private static string DiceOrderText()
    {
        IList dice = DiceList();
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < dice.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(" → ");
            }
            builder.Append(DieTypeName(dice[i]));
            builder.Append("#");
            builder.Append(ObjectInt(dice[i], "Id"));
        }
        return builder.ToString();
    }

    private static string DiceSummary()
    {
        IList dice = DiceList();
        Dictionary<string, int> counts = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < dice.Count; i++)
        {
            string name = DieTypeName(dice[i]);
            int count;
            counts.TryGetValue(name, out count);
            counts[name] = count + 1;
        }

        List<string> parts = new List<string>();
        foreach (KeyValuePair<string, int> pair in counts)
        {
            parts.Add(pair.Key + "x" + pair.Value);
        }
        parts.Sort(StringComparer.Ordinal);
        return string.Join("/", parts.ToArray());
    }

    private static string SpellSummary()
    {
        IList spells = SpellBagList();
        if (spells.Count == 0)
        {
            return "空";
        }
        string[] names = new string[spells.Count];
        for (int i = 0; i < spells.Count; i++)
        {
            object definition = InvokeDemo("SpellDefinitionFor", spells[i]);
            names[i] = definition != null ? ObjectString(definition, "DisplayName") : EnumText(spells[i]);
        }
        return string.Join("/", names);
    }

    private static string FacesText(object die)
    {
        int[] faces = ObjectField(die, "Faces") as int[];
        if (faces == null)
        {
            return string.Empty;
        }
        string[] values = new string[faces.Length];
        for (int i = 0; i < faces.Length; i++)
        {
            values[i] = faces[i].ToString(CultureInfo.InvariantCulture);
        }
        return string.Join(",", values);
    }

    private static double AverageFaces(object die)
    {
        int[] faces = ObjectField(die, "Faces") as int[];
        if (faces == null || faces.Length == 0)
        {
            return 0d;
        }
        long total = 0;
        for (int i = 0; i < faces.Length; i++)
        {
            total += faces[i];
        }
        return total / (double)faces.Length;
    }

    private static bool IsAbsorbableTurtle(object die)
    {
        return Convert.ToBoolean(InvokeDemoStatic("IsF018AbsorbableTurtleType", ObjectField(die, "Type")), CultureInfo.InvariantCulture);
    }

    private static bool IsAnyTurtle(object die)
    {
        return Convert.ToBoolean(InvokeDemoStatic("IsAnyTurtleFamilyType", ObjectField(die, "Type")), CultureInfo.InvariantCulture);
    }

    private static string DieTypeName(object die)
    {
        return die == null ? "无" : EnumText(ObjectField(die, "Type"));
    }

    private static void WriteDecision(
        string action,
        string options,
        string choice,
        string reason,
        string outcome,
        string stateBefore,
        string stateAfter)
    {
        currentDecisionIndex++;
        if (decisionWriter == null)
        {
            return;
        }
        decisionWriter.WriteLine(
            "{" +
            "\"run_id\":\"" + JsonEscape(currentRunId) + "\"," +
            "\"seed\":" + currentSeed.ToString(CultureInfo.InvariantCulture) + "," +
            "\"strategy\":\"" + JsonEscape(currentStrategy) + "\"," +
            "\"sequence\":" + currentDecisionIndex.ToString(CultureInfo.InvariantCulture) + "," +
            "\"action\":\"" + JsonEscape(action) + "\"," +
            "\"options\":\"" + JsonEscape(options) + "\"," +
            "\"choice\":\"" + JsonEscape(choice) + "\"," +
            "\"reason\":\"" + JsonEscape(reason) + "\"," +
            "\"outcome\":\"" + JsonEscape(outcome) + "\"," +
            "\"state_before\":\"" + JsonEscape(stateBefore) + "\"," +
            "\"state_after\":\"" + JsonEscape(stateAfter) + "\"" +
            "}");
    }

    private static void CaptureRuntimeLog(string condition, string stackTrace, LogType type)
    {
        if (chainWriter == null || string.IsNullOrEmpty(currentRunId))
        {
            return;
        }

        bool chain = condition.IndexOf("[CHAIN]", StringComparison.Ordinal) >= 0;
        bool problem = type == LogType.Error || type == LogType.Exception || type == LogType.Assert;
        if (!chain && !problem)
        {
            return;
        }

        chainWriter.WriteLine("[" + currentRunId + "][" + type + "] " + condition);
        if (problem || condition.IndexOf("上限", StringComparison.Ordinal) >= 0 || condition.IndexOf("aborted", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            currentChainWarnings++;
        }
    }

    private static void WriteRunCsv()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("run_id,strategy,route_focus,seed,result,stage_reached,stages_cleared,rolls,markets,decisions,dice_bought,dice_sold,spells_bought,spells_cast,refreshes,lives_remaining,gold,final_score,chain_warnings,action_limit,dice_summary,spell_summary,error");
        for (int i = 0; i < summaries.Count; i++)
        {
            RunSummary summary = summaries[i];
            string[] fields =
            {
                summary.RunId,
                summary.Strategy,
                summary.RouteFocus,
                summary.Seed.ToString(CultureInfo.InvariantCulture),
                summary.Result,
                summary.StageReached.ToString(CultureInfo.InvariantCulture),
                summary.StagesCleared.ToString(CultureInfo.InvariantCulture),
                summary.Rolls.ToString(CultureInfo.InvariantCulture),
                summary.Markets.ToString(CultureInfo.InvariantCulture),
                summary.Decisions.ToString(CultureInfo.InvariantCulture),
                summary.DiceBought.ToString(CultureInfo.InvariantCulture),
                summary.DiceSold.ToString(CultureInfo.InvariantCulture),
                summary.SpellsBought.ToString(CultureInfo.InvariantCulture),
                summary.SpellsCast.ToString(CultureInfo.InvariantCulture),
                summary.Refreshes.ToString(CultureInfo.InvariantCulture),
                summary.LivesRemaining.ToString(CultureInfo.InvariantCulture),
                summary.Gold.ToString(CultureInfo.InvariantCulture),
                summary.FinalScore.ToString(CultureInfo.InvariantCulture),
                summary.ChainWarnings.ToString(CultureInfo.InvariantCulture),
                summary.ActionLimitReached ? "1" : "0",
                summary.DiceSummary,
                summary.SpellSummary,
                summary.Error
            };
            for (int fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
            {
                if (fieldIndex > 0)
                {
                    builder.Append(',');
                }
                builder.Append(CsvEscape(fields[fieldIndex]));
            }
            builder.AppendLine();
        }
        File.WriteAllText(outputPrefix + "_playtest_runs.csv", builder.ToString(), Utf8NoBom);
    }

    private static void WriteSummaryMarkdown(int seedCount, int baseSeed)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("# 骰子王后台试玩分析");
        builder.AppendLine();
        builder.AppendLine("- 生成时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture));
        builder.AppendLine("- 固定种子：`" + baseSeed + "` 起，共 `" + seedCount + "` 个。");
        builder.AppendLine("- 策略：随机基线、眼前得分、长期成长、经济利息、路线构筑。");
        builder.AppendLine("- 总样本：`" + summaries.Count + "` 局。");
        builder.AppendLine("- 执行口径：调用真实投点、`ScoreDice`、市场、法术、离场和生命续投逻辑；跳过纯表现等待。" );
        builder.AppendLine();
        builder.AppendLine("## 策略对照");
        builder.AppendLine();
        builder.AppendLine("| 策略 | 样本 | 通关率 | 平均到达关卡 | 平均投掷 | 平均购骰 | 平均施法 | 平均刷新 | 平均剩余生命 | 连锁警告 |");
        builder.AppendLine("|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|");

        string[] strategyNames = { "随机基线", "眼前得分", "长期成长", "经济利息", "路线构筑" };
        for (int strategyIndex = 0; strategyIndex < strategyNames.Length; strategyIndex++)
        {
            List<RunSummary> group = SummariesForStrategy(strategyNames[strategyIndex]);
            int wins = CountResult(group, "通关");
            builder.Append("| ");
            builder.Append(strategyNames[strategyIndex]);
            builder.Append(" | ");
            builder.Append(group.Count);
            builder.Append(" | ");
            builder.Append(Percent(wins, group.Count));
            builder.Append(" | ");
            builder.Append(Average(group, "stage").ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" | ");
            builder.Append(Average(group, "rolls").ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" | ");
            builder.Append(Average(group, "dice_bought").ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" | ");
            builder.Append(Average(group, "spells_cast").ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" | ");
            builder.Append(Average(group, "refreshes").ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" | ");
            builder.Append(Average(group, "lives").ToString("0.00", CultureInfo.InvariantCulture));
            builder.Append(" | ");
            builder.Append(Sum(group, "warnings"));
            builder.AppendLine(" |");
        }

        builder.AppendLine();
        builder.AppendLine("## 自动诊断");
        builder.AppendLine();
        List<string> diagnostics = BuildDiagnostics();
        for (int i = 0; i < diagnostics.Count; i++)
        {
            builder.AppendLine("- " + diagnostics[i]);
        }

        builder.AppendLine();
        builder.AppendLine("## 详细证据");
        builder.AppendLine();
        builder.AppendLine("- 逐步决策：`" + Path.GetFileName(outputPrefix) + "_playtest_decisions.jsonl`");
        builder.AppendLine("- 单局汇总：`" + Path.GetFileName(outputPrefix) + "_playtest_runs.csv`");
        builder.AppendLine("- 真实连锁日志：`" + Path.GetFileName(outputPrefix) + "_playtest_chain.log`");
        builder.AppendLine("- 后台 Unity 日志：`" + Path.GetFileName(outputPrefix) + "_unity.log`");
        builder.AppendLine();
        builder.AppendLine("## 结论边界");
        builder.AppendLine();
        builder.AppendLine("本报告适合发现规则错误、死循环、经济断层、路线强弱、无效选择与数值异常。无窗口运行不验证动画、声音、鼠标手感、信息可读性或主观爽感；这些项目仍需离屏画面检查或真人 Play Mode 体验。" );
        File.WriteAllText(outputPrefix + "_playtest_summary.md", builder.ToString(), Utf8NoBom);
    }

    private static List<string> BuildDiagnostics()
    {
        List<string> result = new List<string>();
        int exceptions = 0;
        int actionLimits = 0;
        int totalWarnings = 0;
        for (int i = 0; i < summaries.Count; i++)
        {
            if (!string.IsNullOrEmpty(summaries[i].Error))
            {
                exceptions++;
            }
            if (summaries[i].ActionLimitReached)
            {
                actionLimits++;
            }
            totalWarnings += summaries[i].ChainWarnings;
        }

        result.Add(exceptions == 0 ? "全部样本均完成策略循环，没有运行异常。" : "有 `" + exceptions + "` 局发生运行异常，应先查看单局 CSV 与后台 Unity 日志。" );
        result.Add(actionLimits == 0 ? "没有样本触发动作上限，未发现自动流程死循环。" : "有 `" + actionLimits + "` 局触发动作上限，可能存在状态停滞或重复决策。" );
        result.Add(totalWarnings == 0 ? "真实 `[CHAIN]` 日志未记录错误级警告或响应上限。" : "真实 `[CHAIN]` 路径累计 `" + totalWarnings + "` 条警告，需要检查连锁日志。" );

        List<RunSummary> random = SummariesForStrategy("随机基线");
        List<RunSummary> route = SummariesForStrategy("路线构筑");
        double randomStage = Average(random, "stage");
        double routeStage = Average(route, "stage");
        if (routeStage <= randomStage + 0.5d)
        {
            result.Add("路线构筑平均到达关卡没有明显领先随机基线；可能是构筑回报不足，也可能需要加强策略评分。" );
        }
        else
        {
            result.Add("路线构筑平均到达关卡比随机基线高 `" + (routeStage - randomStage).ToString("0.00", CultureInfo.InvariantCulture) + "` 关，规划目前能产生可测收益。" );
        }

        for (int i = 0; i < 5; i++)
        {
            string strategy = new[] { "随机基线", "眼前得分", "长期成长", "经济利息", "路线构筑" }[i];
            List<RunSummary> group = SummariesForStrategy(strategy);
            int wins = CountResult(group, "通关");
            if (group.Count >= 10 && (wins == 0 || wins == group.Count))
            {
                result.Add("“" + strategy + "”在全部样本中" + (wins == 0 ? "没有通关" : "全部通关") + "，需要重点复核目标曲线与策略行为。" );
            }
        }
        return result;
    }

    private static List<RunSummary> SummariesForStrategy(string strategy)
    {
        List<RunSummary> result = new List<RunSummary>();
        for (int i = 0; i < summaries.Count; i++)
        {
            if (string.Equals(summaries[i].Strategy, strategy, StringComparison.Ordinal))
            {
                result.Add(summaries[i]);
            }
        }
        return result;
    }

    private static int CountResult(List<RunSummary> group, string result)
    {
        int count = 0;
        for (int i = 0; i < group.Count; i++)
        {
            if (string.Equals(group[i].Result, result, StringComparison.Ordinal))
            {
                count++;
            }
        }
        return count;
    }

    private static double Average(List<RunSummary> group, string field)
    {
        return group.Count == 0 ? 0d : Sum(group, field) / (double)group.Count;
    }

    private static int Sum(List<RunSummary> group, string field)
    {
        int total = 0;
        for (int i = 0; i < group.Count; i++)
        {
            RunSummary summary = group[i];
            switch (field)
            {
                case "stage": total += summary.StageReached; break;
                case "rolls": total += summary.Rolls; break;
                case "dice_bought": total += summary.DiceBought; break;
                case "spells_cast": total += summary.SpellsCast; break;
                case "refreshes": total += summary.Refreshes; break;
                case "lives": total += summary.LivesRemaining; break;
                case "warnings": total += summary.ChainWarnings; break;
            }
        }
        return total;
    }

    private static string Percent(int numerator, int denominator)
    {
        return denominator <= 0
            ? "0.0%"
            : (numerator * 100d / denominator).ToString("0.0", CultureInfo.InvariantCulture) + "%";
    }

    private static RunSummary BuildExceptionSummary(PlayStrategy strategy, int seed, int ordinal, Exception exception)
    {
        string strategyName = StrategyName(strategy);
        string runId = "S" + ordinal.ToString("D4", CultureInfo.InvariantCulture) + "-" + strategyName;
        currentRunId = runId;
        currentStrategy = strategyName;
        currentSeed = seed;
        currentChainWarnings++;
        if (decisionWriter != null)
        {
            WriteDecision("运行异常", "", "停止本局", "捕获异常，避免影响后续固定种子。", exception.Message, StateSnapshotSafe(), StateSnapshotSafe());
        }
        return new RunSummary
        {
            RunId = runId,
            Strategy = strategyName,
            RouteFocus = strategy == PlayStrategy.RouteBuilder ? RouteFocusForSeed(seed) : string.Empty,
            Seed = seed,
            Result = "异常",
            StageReached = DemoIntSafe("stageIndex") + 1,
            LivesRemaining = DemoIntSafe("remainingLives"),
            Gold = DemoIntSafe("chapterGold"),
            FinalScore = DemoIntSafe("currentScore"),
            ChainWarnings = currentChainWarnings,
            Error = exception.GetType().Name + ": " + exception.Message,
            DiceSummary = DiceSummarySafe(),
            SpellSummary = SpellSummarySafe()
        };
    }

    private static void ValidateReflectionContract()
    {
        string[] fields =
        {
            "mode", "rollPhase", "dice", "marketOffers", "spellBag", "stageIndex", "chapterGold",
            "remainingLives", "currentScore", "resolvedScore", "suppressSave", "stageFailureRetryAvailable",
            "marketLeaveEffectSequenceActive", "selectedMarketDieIndex"
        };
        for (int i = 0; i < fields.Length; i++)
        {
            RequireField(demoType, fields[i]);
        }

        string[] zeroArgumentMethods =
        {
            "StartDefaultBuild", "BeginShakeRoll", "BeginStopRoll", "FinishShakeRoll", "BeginSettle",
            "CompleteScoreReveal", "CurrentEncounter", "CompleteStageFailureRetry", "BeginMarketLeaveEffects",
            "ProcessNextMarketLeaveEffect", "CompleteMarketLeave", "CancelSpellCastSelection"
        };
        for (int i = 0; i < zeroArgumentMethods.Length; i++)
        {
            RequireMethod(demoType, zeroArgumentMethods[i], 0);
        }

        RequireMethod(demoType, "ContinueAfterStageClear", 1);
        RequireMethod(demoType, "CanBuyMarketOffer", 1);
        RequireMethod(demoType, "CurrentOfferPrice", 1);
        RequireMethod(demoType, "SellPrice", 1, "Die");
        RequireMethod(demoType, "BuyOffer", 1);
        RequireMethod(demoType, "RefreshMarketOffers", 1);
        RequireMethod(demoType, "CurrentMarketRefreshNetCost", 1);
        RequireMethod(demoType, "SpellDefinitionFor", 1);
        RequireMethod(demoType, "CanCastSpellNow", 1);
        RequireMethod(demoType, "BeginSpellCast", 1);
        RequireMethod(demoType, "CommitSpellDieTarget", 1);
        RequireMethod(demoType, "CommitSpellFamily", 1);
        RequireMethod(demoType, "CommitSpellShelf", 1);
        RequireMethod(demoType, "CanRestockFamily", 1);
        RequireStaticMethod(demoType, "IsF018AbsorbableTurtleType", 1);
        RequireStaticMethod(demoType, "IsAnyTurtleFamilyType", 1);
    }

    private static object InvokeDemo(string methodName, params object[] arguments)
    {
        MethodInfo method = FindCompatibleMethod(demoType, methodName, arguments, false);
        if (method == null)
        {
            throw new MissingMethodException(demoType.FullName, methodName);
        }
        try
        {
            return method.Invoke(demo, arguments);
        }
        catch (TargetInvocationException exception)
        {
            throw exception.InnerException ?? exception;
        }
    }

    private static object InvokeDemoStatic(string methodName, params object[] arguments)
    {
        MethodInfo method = FindCompatibleMethod(demoType, methodName, arguments, true);
        if (method == null)
        {
            throw new MissingMethodException(demoType.FullName, methodName);
        }
        try
        {
            return method.Invoke(null, arguments);
        }
        catch (TargetInvocationException exception)
        {
            throw exception.InnerException ?? exception;
        }
    }

    private static MethodInfo FindMethod(Type type, string name, int argumentCount, string firstParameterTypeName, bool staticOnly)
    {
        MethodInfo[] methods = type.GetMethods(staticOnly ? StaticFlags : InstanceFlags);
        for (int i = 0; i < methods.Length; i++)
        {
            if (!string.Equals(methods[i].Name, name, StringComparison.Ordinal))
            {
                continue;
            }
            ParameterInfo[] parameters = methods[i].GetParameters();
            if (parameters.Length != argumentCount)
            {
                continue;
            }
            if (!string.IsNullOrEmpty(firstParameterTypeName)
                && (parameters.Length == 0 || !string.Equals(parameters[0].ParameterType.Name, firstParameterTypeName, StringComparison.Ordinal)))
            {
                continue;
            }
            return methods[i];
        }
        return null;
    }

    private static MethodInfo FindCompatibleMethod(Type type, string name, object[] arguments, bool staticOnly)
    {
        int argumentCount = arguments == null ? 0 : arguments.Length;
        MethodInfo[] methods = type.GetMethods(staticOnly ? StaticFlags : InstanceFlags);
        for (int methodIndex = 0; methodIndex < methods.Length; methodIndex++)
        {
            MethodInfo method = methods[methodIndex];
            if (!string.Equals(method.Name, name, StringComparison.Ordinal))
            {
                continue;
            }

            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != argumentCount)
            {
                continue;
            }

            bool compatible = true;
            for (int argumentIndex = 0; argumentIndex < argumentCount; argumentIndex++)
            {
                object argument = arguments[argumentIndex];
                if (argument == null)
                {
                    if (parameters[argumentIndex].ParameterType.IsValueType)
                    {
                        compatible = false;
                        break;
                    }
                    continue;
                }

                if (!parameters[argumentIndex].ParameterType.IsInstanceOfType(argument))
                {
                    compatible = false;
                    break;
                }
            }

            if (compatible)
            {
                return method;
            }
        }
        return null;
    }

    private static void RequireMethod(Type type, string name, int argumentCount, string firstParameterTypeName = null)
    {
        if (FindMethod(type, name, argumentCount, firstParameterTypeName, false) == null)
        {
            throw new MissingMethodException(type.FullName, name);
        }
    }

    private static void RequireStaticMethod(Type type, string name, int argumentCount)
    {
        if (FindMethod(type, name, argumentCount, null, true) == null)
        {
            throw new MissingMethodException(type.FullName, name);
        }
    }

    private static FieldInfo RequireField(Type type, string name)
    {
        FieldInfo field = type.GetField(name, InstanceFlags);
        if (field == null)
        {
            throw new MissingFieldException(type.FullName, name);
        }
        return field;
    }

    private static object DemoField(string name)
    {
        return RequireField(demoType, name).GetValue(demo);
    }

    private static void SetDemoField(string name, object value)
    {
        RequireField(demoType, name).SetValue(demo, value);
    }

    private static int DemoInt(string name)
    {
        return Convert.ToInt32(DemoField(name), CultureInfo.InvariantCulture);
    }

    private static bool DemoBool(string name)
    {
        return Convert.ToBoolean(DemoField(name), CultureInfo.InvariantCulture);
    }

    private static IList DiceList()
    {
        return (IList)DemoField("dice");
    }

    private static IList MarketOfferList()
    {
        return (IList)DemoField("marketOffers");
    }

    private static IList SpellBagList()
    {
        return (IList)DemoField("spellBag");
    }

    private static object ObjectField(object target, string name)
    {
        if (target == null)
        {
            return null;
        }
        FieldInfo field = target.GetType().GetField(name, InstanceFlags);
        if (field == null)
        {
            throw new MissingFieldException(target.GetType().FullName, name);
        }
        return field.GetValue(target);
    }

    private static string ObjectString(object target, string name)
    {
        object value = ObjectField(target, name);
        return value == null ? string.Empty : Convert.ToString(value, CultureInfo.InvariantCulture);
    }

    private static int ObjectInt(object target, string name)
    {
        object value = ObjectField(target, name);
        return value == null ? 0 : Convert.ToInt32(value, CultureInfo.InvariantCulture);
    }

    private static bool ObjectBool(object target, string name)
    {
        object value = ObjectField(target, name);
        return value != null && Convert.ToBoolean(value, CultureInfo.InvariantCulture);
    }

    private static string EnumText(object value)
    {
        return value == null ? string.Empty : value.ToString();
    }

    private static string JsonEscape(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }
        return value
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\r", "\\r")
            .Replace("\n", "\\n")
            .Replace("\t", "\\t");
    }

    private static string CsvEscape(string value)
    {
        value = value ?? string.Empty;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private static string CommandLineValue(string[] args, string key)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], key, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }
        return string.Empty;
    }

    private static int CommandLineInt(string[] args, string key, int fallback, int minimum, int maximum)
    {
        int value;
        return int.TryParse(CommandLineValue(args, key), NumberStyles.Integer, CultureInfo.InvariantCulture, out value)
            ? Math.Max(minimum, Math.Min(maximum, value))
            : fallback;
    }

    private static string StateSnapshotSafe()
    {
        try { return StateSnapshot(); } catch { return string.Empty; }
    }

    private static int DemoIntSafe(string name)
    {
        try { return DemoInt(name); } catch { return 0; }
    }

    private static string DiceSummarySafe()
    {
        try { return DiceSummary(); } catch { return string.Empty; }
    }

    private static string SpellSummarySafe()
    {
        try { return SpellSummary(); } catch { return string.Empty; }
    }

    private static void WriteStatus(string text)
    {
        try
        {
            if (string.IsNullOrEmpty(outputPrefix))
            {
                outputPrefix = SessionState.GetString(OutputPrefixKey, string.Empty);
            }
            if (!string.IsNullOrEmpty(outputPrefix))
            {
                File.WriteAllText(outputPrefix + "_status.txt", text + Environment.NewLine, Utf8NoBom);
            }
        }
        catch
        {
            // Status is advisory; report generation remains authoritative.
        }
    }

    private static void WriteFailureReport(Exception exception)
    {
        try
        {
            if (string.IsNullOrEmpty(outputPrefix))
            {
                outputPrefix = SessionState.GetString(OutputPrefixKey, string.Empty);
            }
            if (string.IsNullOrEmpty(outputPrefix))
            {
                return;
            }
            File.WriteAllText(
                outputPrefix + "_playtest_failure.txt",
                "后台试玩失败。\n" + exception + Environment.NewLine,
                Utf8NoBom);
            WriteStatus("状态：后台试玩失败。\n原因：" + exception.Message);
        }
        catch
        {
            // Unity's own batch log remains the final fallback.
        }
    }

    private static void FailBeforePlayMode(Exception exception)
    {
        outputPrefix = string.IsNullOrEmpty(outputPrefix)
            ? CommandLineValue(Environment.GetCommandLineArgs(), "-wabishPlaytestOutputPrefix")
            : outputPrefix;
        WriteFailureReport(exception);
        SessionState.SetBool(ActiveKey, false);
        EditorApplication.Exit(1);
    }

    private static void FailInPlayMode(Exception exception)
    {
        WriteFailureReport(exception);
        SessionState.SetBool(ActiveKey, false);
        EditorApplication.Exit(1);
    }

    private static void CloseWriter(StreamWriter writer)
    {
        if (writer == null)
        {
            return;
        }
        try
        {
            writer.Flush();
            writer.Dispose();
        }
        catch
        {
            // Best effort during batch shutdown.
        }
    }
}
