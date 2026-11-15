using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 仅在临时实例上验证；图形捕获必须使用独立工程与独立 PlayerPrefs 命名空间。
public static class WabishMarketVariableUiCapture
{
    private const string ActiveKey = "Wabish.MarketVariableUiCapture.Active";
    private const string OutputKey = "Wabish.MarketVariableUiCapture.Output";
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly Type DemoType = typeof(DiceKingDemo);
    private static readonly MethodInfo ProductionOnGui = typeof(DiceKingDemo).GetMethod("OnGUI", Flags);
    private static readonly string[] DieTypes = { "MoneyTurtle", "DoubleTurtle", "LuckyTurtle", "TinyTurtle", "MagnetTurtle", "RallyTurtle", "LeaderTurtle" };
    private static readonly string[] SpellTypes = { "PassBoost", "ChainDividend", "EchoCommand", "TargetedRestock", "PrecisionCalibration", "FeedBag", "PeakCashout", "OverachieverBounty" };
    private static readonly int[] Counts = { 0, 1, 2, 3, 4, 5, 6, 8, 32 };
    private static readonly List<GameObject> HiddenOriginals = new List<GameObject>();
    private static GameObject host;
    private static DiceKingDemo demo;
    private static Report report;
    private static string outputDirectory;
    private static EditorWindow gameView;
    private static int oldSizeIndex;
    private static float oldTimeScale;
    private static float oldVolume;
    private static UnityEngine.Random.State oldRandom;
    private static int captureIndex;
    private static int captureStage;
    private static int stableFrames;
    private static double stageStarted;
    private static double runStarted;
    private static double lastResolutionRequestAt;
    private static double lastHoverEventAt = -999d;
    private static bool captureOwnsGameView;
    private static double lastPointerDiagnosticAt = -999d;
    private static Vector2 lastPointerVirtual;
    private static Vector2 lastPointerWindow;
    private static Vector2 lastPointerOffset;
    private static float lastPointerScale;
    private static string lastSentPointerEvent;
    private static bool injectedPointerActive;
    private static Vector2 injectedPointerVirtual;
    private static Vector2 rawPointerBeforeInjection;
    private static Vector2 rawPointerAfterInjection;
    private static string injectedGuiEventType;
    private static int injectedGuiEvents;
    private static readonly Queue<GuiPointerInput> QueuedGuiInputs = new Queue<GuiPointerInput>();
    private static object[] hoverBaselineOffers;
    private static string[] hoverBaselineKinds;
    private static object[] hoverBaselineSpells;
    private static int hoverBaselineGold;
    private static bool finishing;
    private static int pointerTargetIndex = -1;
    private static int pointerTargetSpellCount;
    private static object[] pointerTargetOffers;

    [Serializable]
    private sealed class Report
    {
        public string Date = "2026-11-10";
        public string Scope;
        public bool Passed;
        public bool PlayMode;
        public bool SuppressSave;
        public bool SelectedPlayerPrefsChecked;
        public bool SelectedPlayerPrefsUnchanged;
        public int QuantityCombinations;
        public int AssertionCount;
        public List<string> Checks = new List<string>();
        public List<string> Captures = new List<string>();
        public string Error;
        public string PointerInputMode = "受控宿主在真实GUI上下文排队注入原始MouseDown/MouseUp及指针，再调用生产OnGUI；未验证GameView原生事件路线、OS鼠标或普通Mono自动派发";
        public int InjectedGuiEvents;
        public List<string> HoverModelInvariance = new List<string>();
        public List<string> AdaptiveRows = new List<string>();
        public List<string> PurchaseEligibility = new List<string>();
        public string Limitation = "反射检查与截图生成不替代用户对连续操作、字形清晰度和正式美术的人工验收。";
    }

    private sealed class Shot
    {
        public string Name;
        public int Dice;
        public int Spells;
        public bool Chapter;
        public bool Last;
        public string Special;
        public Shot(string name, int dice, int spells, bool chapter, bool last, string special)
        { Name = name; Dice = dice; Spells = spells; Chapter = chapter; Last = last; Special = special; }
    }

    private sealed class GuiPointerInput
    {
        public EventType Type;
        public Vector2 VirtualPoint;
    }

    [Serializable]
    private sealed class GuiInputTrace
    {
        public string Type;
        public int Frame;
        public float UnscaledTime;
        public Vector2 VirtualPoint;
        public Vector2 RawPoint;
        public int HotControlBefore;
        public int HotControlAfter;
        public int TargetIndex;
        public string TargetKindBefore;
        public string TargetKindAfter;
        public int SpellBagBefore;
        public int SpellBagAfter;
        public string SpellStepAfter;
        public string EventTypeAfter;
        public string RawTypeBefore;
        public string RawTypeAfter;
        public string TypeForControlBefore;
        public string TypeForControlAfter;
        public bool ConsumedAfter;
        public int ScreenWidthBefore;
        public int ScreenHeightBefore;
        public int ScreenWidthAfter;
        public int ScreenHeightAfter;
        public Vector2 UiMouseAfter;
        public bool UiMouseValidAfter;
        public bool GuiEnabledAfter;
        public string GuiMatrixBefore;
        public string GuiMatrixAfter;
    }

    [Serializable]
    private sealed class PointerDiagnostic
    {
        public string Reason;
        public string Shot;
        public double EditorTime;
        public float UnscaledTime;
        public int Frame;
        public int Stage;
        public int ScreenWidth;
        public int ScreenHeight;
        public Vector2 VirtualPoint;
        public Vector2 WindowPoint;
        public Vector2 GameMouseOffset;
        public float GameMouseScale;
        public Rect WindowRect;
        public Rect TargetInView;
        public Rect ViewInWindow;
        public Vector2 UiMousePosition;
        public bool UiMousePositionValid;
        public string CandidateKey;
        public string HoverKey;
        public bool HoverVisible;
        public float HoverStartedAt;
        public float HoverDelay;
        public bool WantsMouseMove;
        public string LastSentEvent;
        public string CurrentEventType;
        public string CurrentRawType;
        public bool HasCurrentEvent;
        public Vector2 CurrentRawMouse;
        public bool GuiInputInjectionActive;
        public string LastInjectedGuiEventType;
        public Vector2 RawMouseBeforeInjection;
        public Vector2 RawMouseAfterInjection;
        public int InjectedGuiEvents;
    }

    private static readonly Shot[] Shots =
    {
        new Shot("1dice_1spell_centered", 1, 1, false, false, ""),
        new Shot("2dice_2spells_distributed", 2, 2, false, false, ""),
        new Shot("3dice_3spells_distributed", 3, 3, false, false, ""),
        new Shot("3dice_no_spells_affordable", 3, 0, false, false, ""),
        new Shot("3dice_mixed_affordability", 3, 0, false, false, "mixed_prices"),
        new Shot("3dice_no_gold", 3, 0, false, false, "no_gold"),
        new Shot("3dice_bag_full", 3, 0, false, false, "bag_full"),
        new Shot("3dice_blood_pact_full", 3, 0, false, false, "blood_pact"),
        new Shot("3dice_purchase_empty", 3, 0, false, false, "dice_purchase"),
        new Shot("5dice_5spells_first", 5, 5, false, false, ""),
        new Shot("5dice_5spells_last", 5, 5, false, true, ""),
        new Shot("normal_market", 2, 1, false, false, ""),
        new Shot("chapter_market", 1, 2, true, false, ""),
        new Shot("4dice_4spells", 4, 4, false, false, ""),
        new Shot("6dice_6spells_last", 6, 6, false, true, ""),
        new Shot("8dice_8spells_first", 8, 8, false, false, ""),
        new Shot("8dice_8spells_last", 8, 8, false, true, ""),
        new Shot("32dice_32spells_last", 32, 32, false, true, ""),
        new Shot("purchase_empty", 8, 8, false, true, "purchase"),
        new Shot("spell_hover_longest", 4, 4, false, false, "hover"),
        new Shot("spell_target_last", 8, 8, false, true, "target"),
        new Shot("market_leave_locked", 4, 4, false, false, "locked"),
        new Shot("owned_spell_cards", 4, 4, false, false, "bag")
    };

    [InitializeOnLoadMethod]
    private static void Resume()
    {
        if (!SessionState.GetBool(ActiveKey, false)) return;
        outputDirectory = SessionState.GetString(OutputKey, DefaultOutputDirectory());
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        runStarted = EditorApplication.timeSinceStartup;
    }

    // 可使用 -batchmode -nographics；不进入 Play Mode，不创建主场景对象。
    public static void Validate()
    {
        outputDirectory = ResolveOutputDirectory();
        Directory.CreateDirectory(outputDirectory);
        report = new Report { Scope = "编辑器反射与逻辑矩形检查；未进入 Play Mode", PlayMode = false };
        Dictionary<string, string> prefsBefore = ReadSelectedPrefs();
        try
        {
            CreateTemporaryDemo(false);
            RunAssertions();
            report.SuppressSave = (bool)Get("suppressSave");
            DestroyTemporaryDemo();
            report.SelectedPlayerPrefsUnchanged = SamePrefs(prefsBefore, ReadSelectedPrefs());
            report.SelectedPlayerPrefsChecked = true;
            Require(report.SelectedPlayerPrefsUnchanged, "所检查的正式存档与设置键未改变");
            report.Passed = true;
            WriteReport("validation.json", "通过：编辑器反射检查完成；没有进入 Play Mode。");
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            report.Error = Unwrap(exception).ToString();
            report.Passed = false;
            DestroyTemporaryDemo();
            WriteReport("validation.json", "失败：" + report.Error);
            Debug.LogError(report.Error);
            if (Application.isBatchMode) EditorApplication.Exit(1);
        }
    }

    // 图形捕获不可加 -nographics；使用 -isolatedMarketUiValidation true 显式确认隔离工程。
    public static void Capture()
    {
        outputDirectory = ResolveOutputDirectory();
        Directory.CreateDirectory(outputDirectory);
        if (!string.Equals(CommandLineValue("-isolatedMarketUiValidation"), "true", StringComparison.OrdinalIgnoreCase)
            && CommandLineValue("-isolatedMarketUiValidation") != "1")
        {
            File.WriteAllText(Path.Combine(outputDirectory, "capture_status.txt"), "拒绝启动：图形捕获只允许独立验证工程。请先将 companyName/productName 改为验证专用名称，再传 -isolatedMarketUiValidation true；不得对用户主工程直接执行。", new UTF8Encoding(false));
            if (Application.isBatchMode) EditorApplication.Exit(2);
            return;
        }
        if (Application.companyName != "CodexWabishMarketUiQA" || Application.productName != "WabishMarketUi20261109")
        {
            File.WriteAllText(Path.Combine(outputDirectory, "capture_status.txt"), "拒绝启动：隔离工程必须使用 companyName=CodexWabishMarketUiQA、productName=WabishMarketUi20261109，防止主存档命名空间受到影响。", new UTF8Encoding(false));
            if (Application.isBatchMode) EditorApplication.Exit(2);
            return;
        }
        finishing = false;
        injectedGuiEvents = 0;
        WabishMarketVariableUiInputDriver.LastError = null;
        ClearInjectedPointer();
        SessionState.SetString(OutputKey, outputDirectory);
        File.WriteAllText(Path.Combine(outputDirectory, "pointer_diagnostics.jsonl"), string.Empty, new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(outputDirectory, "gui_input_events.jsonl"), string.Empty, new UTF8Encoding(false));
        SessionState.SetBool(ActiveKey, true);
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        runStarted = EditorApplication.timeSinceStartup;
        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }
    }

    private static void Tick()
    {
        if (finishing || !SessionState.GetBool(ActiveKey, false)) return;
        try
        {
            if (!string.IsNullOrEmpty(WabishMarketVariableUiInputDriver.LastError))
                throw new InvalidOperationException("测试GUI宿主绘制异常：" + WabishMarketVariableUiInputDriver.LastError);
            if (EditorApplication.timeSinceStartup - runStarted > 240d) throw new TimeoutException("图形捕获超过四分钟未完成。");
            if (!EditorApplication.isPlaying) return;
            if (demo == null)
            {
                outputDirectory = SessionState.GetString(OutputKey, DefaultOutputDirectory());
                report = new Report { Scope = "隔离工程Play Mode；受控测试宿主在真实GUI上下文调用生产OnGUI，原始指针注入；未覆盖OS鼠标或普通Mono自动派发", PlayMode = true };
                CreateTemporaryDemo(true);
                RunAssertions();
                captureIndex = 0;
                captureStage = 0;
                stageStarted = EditorApplication.timeSinceStartup;
            }
            Shot shot = Shots[captureIndex % Shots.Length];
            int width = captureIndex < Shots.Length ? 1280 : 1920;
            int height = width == 1280 ? 720 : 1080;
            if (captureStage == 0)
            {
                ConfigureFixture(shot.Dice, shot.Spells, shot.Chapter);
                if (shot.Last) { Call("SetMarketShelfPage", false, PageCount(false) - 1); Call("SetMarketShelfPage", true, PageCount(true) - 1); }
                ConfigureShotSpecial(shot);
                ApplyResolution(width, height);
                stableFrames = 0;
                Advance(1);
            }
            else if (captureStage == 1)
            {
                if (shot.Special == "hover") WritePointerDiagnostic("next editor tick");
                if (Screen.width != width || Screen.height != height)
                {
                    if (Elapsed() > 12d) throw new InvalidOperationException("GameView 未稳定为 " + width + "×" + height + "，实际 " + Screen.width + "×" + Screen.height);
                    if (EditorApplication.timeSinceStartup - lastResolutionRequestAt > 0.5d) ApplyResolution(width, height);
                    return;
                }
                if (++stableFrames < 3 || Elapsed() < 0.65d) return;
                MainGameLedFont font = Get("mainGameLedFont") as MainGameLedFont;
                Require(font != null && font.IsReady, "正式 LED 字库已就绪，未以系统字回退冒充通过");
                Require(Get("physicalKeyLabelFont") != null, "实体键字体已加载");
                if (shot.Special == "hover" && !(bool)Get("spellHoverVisible"))
                {
                    if (Elapsed() > 6d) throw new InvalidOperationException("已发送卡面 MouseMove，但完整法术悬停详情未出现。");
                    // SendEvent 同步运行 EditorWindow.OnGUI；其非 Repaint 分支可能临时
                    // 改写 Screen backing-size。先核对正常渲染尺寸，再注入，留下一帧恢复。
                    if (lastHoverEventAt < 0d)
                    {
                        SendHover(width, height);
                        lastHoverEventAt = EditorApplication.timeSinceStartup;
                    }
                    return;
                }
                if (shot.Special == "hover" && EditorApplication.timeSinceStartup - lastHoverEventAt < 0.08d) return;
                if (shot.Special == "hover") VerifyHoverModelUnchanged(width, height);
                string shotPath = ShotPath(shot, width, height);
                if (File.Exists(shotPath)) File.Delete(shotPath);
                ScreenCapture.CaptureScreenshot(shotPath, 1);
                Advance(2);
            }
            else if (captureStage == 2)
            {
                string path = ShotPath(shot, width, height);
                if (!File.Exists(path)) { if (Elapsed() > 8d) throw new IOException("截图未落盘：" + path); return; }
                if (Elapsed() < 0.25d) return;
                Texture2D image = new Texture2D(2, 2);
                bool loaded = ImageConversion.LoadImage(image, File.ReadAllBytes(path));
                bool correct = loaded && image.width == width && image.height == height;
                UnityEngine.Object.DestroyImmediate(image);
                Require(correct, "原生截图尺寸正确：" + Path.GetFileName(path));
                report.Captures.Add(Path.GetFileName(path));
                if (shot.Special == "target")
                {
                    IList visible = (IList)Call("VisibleMarketShelfIndices", true);
                    pointerTargetIndex = (int)visible[visible.Count - 1];
                    pointerTargetOffers = Copy((IList)Get("marketOffers"));
                    pointerTargetSpellCount = ((IList)Get("spellBag")).Count;
                    Require(pointerTargetIndex > 3, "测试GUI输入目标位于第四格之后");
                    Advance(3);
                }
                else NextCapture();
            }
            else if (captureStage == 3)
            {
                if (Elapsed() < 0.2d) return;
                SendTargetPointer(width, 0, EventType.MouseMove);
                QueueTargetGuiPointer(EventType.MouseDown);
                Advance(4);
            }
            else if (captureStage == 4)
            {
                if (Elapsed() < 0.15d) return;
                QueueTargetGuiPointer(EventType.MouseUp);
                Advance(5);
            }
            else if (captureStage == 5)
            {
                IList offers = (IList)Get("marketOffers");
                bool changed = !ReferenceEquals(offers[pointerTargetIndex], pointerTargetOffers[pointerTargetIndex]);
                if (!changed || Kind(offers[pointerTargetIndex]) != "Die")
                {
                    if (Elapsed() > 5d) throw new InvalidOperationException("受控宿主排队原始GUI MouseDown/MouseUp未命中末页定向换货选择，可能被底层市场热区拦截。目标原索引=" + pointerTargetIndex);
                    return;
                }
                Require(offers.Count == pointerTargetOffers.Length, "真实点击换货未增加货架数量");
                Require(((IList)Get("spellBag")).Count == pointerTargetSpellCount - 1, "真实点击换货消耗一张法术");
                for (int i = 0; i < offers.Count; i++) if (i != pointerTargetIndex) Require(ReferenceEquals(offers[i], pointerTargetOffers[i]), "真实点击只替换末页选中的原始货位");
                Require(Get("spellCastStep").ToString() == "None", "真实选择完成后退出换货选目标步骤");
                Require((bool)Get("suppressSave"), "真实鼠标操作仍保持禁止存档");
                report.Checks.Add("原生" + width + "×" + height + "：受控宿主排队原始GUI MouseDown/MouseUp，经生产GUI.Button完成末页换货，原索引=" + pointerTargetIndex);
                NextCapture();
            }
            if (gameView != null) gameView.Repaint();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
        catch (Exception exception)
        {
            if (report == null) report = new Report();
            report.Error = Unwrap(exception).ToString();
            FinishCapture(1, "失败：" + report.Error);
        }
    }

    private static void CreateTemporaryDemo(bool playing)
    {
        oldTimeScale = Time.timeScale;
        oldVolume = AudioListener.volume;
        oldRandom = UnityEngine.Random.state;
        if (playing)
        {
            HiddenOriginals.Clear();
            foreach (DiceKingDemo original in UnityEngine.Object.FindObjectsOfType<DiceKingDemo>())
            {
                if (!original.gameObject.activeSelf) continue;
                HiddenOriginals.Add(original.gameObject);
                original.gameObject.SetActive(false);
            }
        }
        host = new GameObject("关间商店可变数量验证（临时，不写存档）");
        host.hideFlags = HideFlags.DontSave;
        host.SetActive(false);
        demo = host.AddComponent<DiceKingDemo>();
        Set("suppressSave", true);
        if (playing) host.AddComponent<WabishMarketVariableUiInputDriver>();
        if (playing)
        {
            host.SetActive(true);
            // 只由测试宿主在真实OnGUI事件中调用一次生产OnGUI，避免另一次自动派发
            // 重置隐藏窗口的OS鼠标位置。Awake已正常执行，生产业务/绘制代码不改动。
            demo.enabled = false;
        }
        else Call("Awake");
        Set("suppressSave", true);
        Call("LoadMainGameTextures");
        Set("mainMenuLaunchTransitionActive", false, false);
        Set("mainGameEntryPresentationActive", false, false);
        AudioListener.volume = 0f;
        if (playing) { Time.timeScale = 0f; EditorApplication.isPaused = false; }
    }

    private static void ConfigureFixture(int diceCount, int spellCount, bool chapter)
    {
        ClearInjectedPointer();
        Set("suppressSave", true);
        Call("StartDefaultBuild");
        Set("suppressSave", true);
        Set("mode", chapter ? "ChapterShop" : "InterStageMarket");
        Set("chapterGold", 1000000);
        Set("spellSystemEnabled", true);
        Set("spellBagCapacity", 3);
        Set("affixFeatureEnabled", false);
        Set("spellPanelOpen", false);
        Set("marketLeaveEffectSequenceActive", false);
        Set("marketLeaveEffectTimer", 0f);
        Set("rewardBanner", string.Empty);
        Set("diceVisualEnterStartTime", -999f);
        Set("actionTipStartedAt", -999f, false);
        Set("selectedMarketDieIndex", 0);
        IList bag = (IList)Get("dice");
        while (bag.Count > 5) bag.RemoveAt(bag.Count - 1);
        ((IList)Get("spellBag")).Clear();
        ReplaceOffers(diceCount, spellCount);
        Call("ResetMarketShelfPresentation");
        Call("EnsureMarketShelfPresentation");
        Call("ClearSpellHover");
    }

    private static void ReplaceOffers(int diceCount, int spellCount)
    {
        IList offers = (IList)Get("marketOffers");
        offers.Clear();
        int diceIndex = 0;
        int spellIndex = 0;
        while (diceIndex < diceCount || spellIndex < spellCount)
        {
            if (diceIndex < diceCount) { offers.Add(NewDieOffer(DieTypes[diceIndex % DieTypes.Length])); diceIndex++; }
            if (spellIndex < spellCount) { offers.Add(NewSpellOffer(SpellTypes[spellIndex % SpellTypes.Length])); spellIndex++; }
        }
    }

    private static object NewDieOffer(string type)
    {
        object dieType = ParseNested("DieType", type);
        string displayName = (string)Call("DefaultName", dieType);
        object die = Call("NewDie", displayName, dieType, new int[] { 1, 2, 3, 4, 5, 6 });
        object offer = Activator.CreateInstance(Nested("MarketOffer"), true);
        Member(offer, "Kind").SetValue(offer, ParseNested("MarketOfferKind", "Die"));
        Member(offer, "Die").SetValue(offer, die);
        Member(offer, "Price").SetValue(offer, 6);
        return offer;
    }

    private static object NewSpellOffer(string type)
    {
        object definition = Call("SpellDefinitionFor", ParseNested("SpellType", type));
        if (definition == null) throw new InvalidOperationException("法术配置缺失：" + type);
        object offer = Activator.CreateInstance(Nested("MarketOffer"), true);
        Member(offer, "Kind").SetValue(offer, ParseNested("MarketOfferKind", "Spell"));
        Member(offer, "Spell").SetValue(offer, definition);
        Member(offer, "Price").SetValue(offer, 2);
        return offer;
    }

    private static void RunAssertions()
    {
        Texture2D spellArt = Resources.Load<Texture2D>("Art/Market/spell_card_art_pilot_strip");
        Require(spellArt != null, "运行时法术插画条带资源可加载");
        Require(spellArt.width == 2172 && spellArt.height == 724, "运行时法术插画导入尺寸保持2172×724");
        MainGameLedFont readyFont = Get("mainGameLedFont") as MainGameLedFont;
        Require(readyFont != null && readyFont.IsReady, "正式字库资源与角色合同完整");
        for (int mode = 0; mode < 2; mode++)
        {
            for (int d = 0; d < Counts.Length; d++)
            {
                for (int s = 0; s < Counts.Length; s++)
                {
                    int diceCount = Counts[d], spellCount = Counts[s];
                    ConfigureFixture(diceCount, spellCount, mode == 1);
                    IList offers = (IList)Get("marketOffers");
                    Require(offers.Count == diceCount + spellCount, "表现层没有填充假空货架");
                    CheckCategory(false, diceCount);
                    CheckCategory(true, spellCount);
                    Require(offers.Count == diceCount + spellCount, "分页没有改变真实商品数量");
                    Require((int)Call("EmptyMarketOfferCount") == 0, "版面留白未进入清仓猪空格计数");
                    report.QuantityCombinations++;
                }
            }
        }
        foreach (int count in new[] { 1, 2, 3, 4, 5, 8 })
        {
            ConfigureFixture(count, count, false);
            CheckPurchase(true);
            ConfigureFixture(count, count, false);
            CheckPurchase(false);
        }
        CheckSameCountRefresh();
        CheckRestockBeyondThree();
        CheckPurchaseEligibility();
        CheckHoverClear();
        Require((bool)Get("suppressSave"), "整个验证实例保持禁止存档");
    }

    private static void CheckCategory(bool spells, int expected)
    {
        IList indices = (IList)Call("MarketShelfIndices", spells);
        Require(indices.Count == expected, "分类列表完整：" + (spells ? "法术" : "骰子") + expected);
        int pages = PageCount(spells);
        Require(pages == Math.Max(1, (expected + 3) / 4), "页数由真实商品数决定");
        HashSet<int> seen = new HashSet<int>();
        IList offers = (IList)Get("marketOffers");
        for (int page = 0; page < pages; page++)
        {
            Call("SetMarketShelfPage", spells, page);
            IList visible = (IList)Call("VisibleMarketShelfIndices", spells);
            Require(visible.Count <= 4, "每页最多四件，未成为总数上限");
            Rect previous = new Rect();
            Rect first = new Rect();
            float spacing = 0f;
            for (int slot = 0; slot < visible.Count; slot++)
            {
                int index = (int)visible[slot];
                Require(index >= 0 && index < offers.Count && seen.Add(index), "每个原始索引只显示一次且不越界");
                Require(Kind(offers[index]) == (spells ? "Spell" : "Die"), "分类未更改原始货架顺序或身份");
                Rect card = (Rect)Call("VariableMarketOfferRect", spells, slot);
                Rect price = (Rect)Call("VariableMarketPriceRect", spells, card);
                Rect buy = (Rect)Call("VariableMarketBuyRect", spells, card);
                Require(Inside(card, new Rect(0f, 0f, 1280f, 720f)) && Inside(price, card) && Inside(buy, card), "卡面价格与购买热区在安全区内");
                Require(price.yMax <= buy.yMin && Mathf.Abs(price.center.x - buy.center.x) < 0.01f, "价格在上，购买键在下并共用中线");
                if (slot == 0) first = card;
                else
                {
                    Require(card.xMin >= previous.xMax, "自适应商品之间没有重叠");
                    if (slot == 1) spacing = card.center.x - previous.center.x;
                    else Require(Mathf.Abs(card.center.x - previous.center.x - spacing) < 0.01f, "同一行相邻商品中心等距");
                }
                previous = card;
            }
            if (visible.Count > 0)
            {
                Require(Mathf.Abs((first.xMin + previous.xMax) * 0.5f - 740f) < 0.01f, "不足四件与末页均以整行中心对称摆放");
                string geometry = (spells ? "法术" : "骰子") + ":总数=" + expected + ",页=" + page + ",可见=" + visible.Count + ",左=" + first.xMin + ",右=" + previous.xMax + ",中心=740";
                if (!report.AdaptiveRows.Contains(geometry)) report.AdaptiveRows.Add(geometry);
            }
        }
        Require(seen.Count == expected, "翻页可到达全部真实商品");
        Call("SetMarketShelfPage", spells, 9999);
        Require((int)Get(spells ? "marketSpellShelfPage" : "marketDieShelfPage") == pages - 1, "分页右边界夹入");
        Call("SetMarketShelfPage", spells, -5);
        Require((int)Get(spells ? "marketSpellShelfPage" : "marketDieShelfPage") == 0, "分页左边界夹入");
    }

    private static void CheckPurchase(bool spell)
    {
        Call("SetMarketShelfPage", spell, PageCount(spell) - 1);
        IList visible = (IList)Call("VisibleMarketShelfIndices", spell);
        int target = (int)visible[visible.Count - 1];
        IList offers = (IList)Get("marketOffers");
        object[] before = Copy(offers);
        int price = (int)Call("CurrentOfferPrice", offers[target]);
        int gold = (int)Get("chapterGold");
        int page = (int)Get(spell ? "marketSpellShelfPage" : "marketDieShelfPage");
        Rect[] beforeRects = new Rect[visible.Count];
        for (int i = 0; i < visible.Count; i++) beforeRects[i] = (Rect)Call("VariableMarketOfferRect", spell, i);
        Call("BuyOffer", target);
        Call("EnsureMarketShelfPresentation");
        Require(offers.Count == before.Length && Kind(offers[target]) == "Empty", "真实购买仅清空目标原货位");
        Require((int)Get("chapterGold") == gold - price, "真实购买按原商品价格扣款");
        for (int i = 0; i < offers.Count; i++) if (i != target) Require(ReferenceEquals(offers[i], before[i]), "未购买商品没有移动或被替换");
        Require((int)Get(spell ? "marketSpellShelfPage" : "marketDieShelfPage") == page, "购买后页码不跳动");
        IList after = (IList)Call("VisibleMarketShelfIndices", spell);
        Require(after.Count == visible.Count && (int)after[after.Count - 1] == target, "购买后原类别空位与可见位置保留");
        for (int i = 0; i < after.Count; i++) Require(beforeRects[i] == (Rect)Call("VariableMarketOfferRect", spell, i), "购买后保留本行布局与空位，剩余商品不突然移动");
        Require((int)Call("EmptyMarketOfferCount") == 1, "只有真实已购物品形成一个玩法空货架");
    }

    private static void CheckSameCountRefresh()
    {
        ConfigureFixture(2, 1, false);
        ReplaceOffers(1, 2);
        Call("EnsureMarketShelfPresentation");
        Require(((IList)Call("MarketShelfIndices", false)).Count == 1 && ((IList)Call("MarketShelfIndices", true)).Count == 2, "相同总数量更换商品对象后重新分类");
        Set("marketDieShelfPage", 9);
        Set("marketSpellShelfPage", 9);
        Call("BuildMarketOffers", false);
        Call("EnsureMarketShelfPresentation");
        Require((int)Get("marketDieShelfPage") == 0 && (int)Get("marketSpellShelfPage") == 0, "真实三件整刷重建分类并复位两类页码");
        IList offers = (IList)Get("marketOffers");
        int actualSpells = 0;
        for (int i = 0; i < offers.Count; i++) if (Kind(offers[i]) == "Spell") actualSpells++;
        Require(((IList)Call("MarketShelfIndices", true)).Count == actualSpells, "整刷类别由新批商品决定，不只比较总数量");
    }

    private static void CheckRestockBeyondThree()
    {
        ConfigureFixture(8, 8, false);
        IList offers = (IList)Get("marketOffers");
        object[] before = Copy(offers);
        IList bag = (IList)Get("spellBag");
        bag.Add(ParseNested("SpellType", "TargetedRestock"));
        Set("spellCastingRackIndex", 0);
        Set("spellCastStep", "ChooseShelf");
        Set("selectedSpellFamily", "Turtle");
        Call("CommitSpellShelf", 15);
        Call("EnsureMarketShelfPresentation");
        Require(offers.Count == 16 && !ReferenceEquals(offers[15], before[15]) && Kind(offers[15]) == "Die", "定向换货能够替换第十六个真实货位");
        for (int i = 0; i < 15; i++) Require(ReferenceEquals(offers[i], before[i]), "换货未重排其余真实货位");
        Require(((IList)Call("MarketShelfIndices", false)).Count == 9 && ((IList)Call("MarketShelfIndices", true)).Count == 7, "法术变骰后类别更新但总货位不变");
    }

    private static void CheckPurchaseEligibility()
    {
        ConfigureFixture(4, 4, false);
        IList offers = (IList)Get("marketOffers");
        IList dice = (IList)Get("dice");
        Require((bool)Call("CanBuyMarketOffer", offers[0]), "有金币且骰袋未满时普通骰可购买");
        Set("chapterGold", 0);
        Require(!(bool)Call("CanBuyMarketOffer", offers[0]) && !(bool)Call("CanBuyMarketOffer", offers[1]), "金币不足时骰子与法术购买均受阻");
        Set("chapterGold", (int)Call("CurrentOfferPrice", offers[0]));
        Require((bool)Call("CanBuyMarketOffer", offers[0]), "金币恰好等于价格时仍可购买");
        Set("chapterGold", 1000000);
        dice.Add(Call("NewDie", "基础骰", ParseNested("DieType", "Basic"), new int[] { 1, 2, 3, 4, 5, 6 }));
        Require(!(bool)Call("CanBuyMarketOffer", offers[0]), "满骰袋阻止普通骰购买");
        Require((bool)Call("CanBuyMarketOffer", offers[1]), "满骰袋不误禁法术购买");
        IList spells = (IList)Get("spellBag");
        for (int i = 0; i < 3; i++) spells.Add(ParseNested("SpellType", SpellTypes[i]));
        Require(!(bool)Call("CanBuyMarketOffer", offers[1]), "满法术袋真实购买资格受阻");
        dice[0] = Call("NewDie", "血契船长", ParseNested("DieType", "BloodPactCaptain"), new int[] { 1, 2, 3, 4, 5, 6 });
        Require((bool)Call("CanBuyMarketOffer", offers[0]), "血契船长满袋购骰例外继续使用真实资格");
    }

    private static void CheckHoverClear()
    {
        ConfigureFixture(4, 4, false);
        for (int i = 0; i < SpellTypes.Length; i++)
        {
            object definition = Call("SpellDefinitionFor", ParseNested("SpellType", SpellTypes[i]));
            string text = (string)Member(definition, "RuleText").GetValue(definition);
            if (SpellTypes[i] == "TargetedRestock") Require(text.Contains("新商品仍需正常购买"), "悬停使用完整真实法术配置正文");
            Set("spellHoverDefinition", definition);
            Set("spellHoverKey", "validation");
            Set("spellHoverVisible", true);
            Set("spellHoverRect", (Rect)Call("VariableMarketOfferRect", true, i % 4));
            float bodyHeight = (float)Call("MeasureSpellHoverRuleHeight", text, 464f);
            Rect panel = (Rect)Call("SpellHoverPanelRect");
            Require(bodyHeight > 0f && panel.height >= 65f + bodyHeight, "八张法术悬停高度承接完整正文");
            Require(Inside(panel, new Rect(16f, 16f, 1248f, 688f)), "悬停详情夹入画布安全区");
            for (int slot = 0; slot < 4; slot++)
            {
                Rect dieCard = (Rect)Call("VariableMarketOfferRect", false, slot);
                Rect spellCard = (Rect)Call("VariableMarketOfferRect", true, slot);
                Rect dieBuy = (Rect)Call("VariableMarketBuyRect", false, dieCard);
                Rect spellBuy = (Rect)Call("VariableMarketBuyRect", true, spellCard);
                Require(!panel.Overlaps(dieBuy) && !panel.Overlaps(spellBuy), "完整法术详情不遮两类商品购买键");
            }
        }
        Call("ClearSpellHover");
        Require(Get("spellHoverDefinition") == null && !(bool)Get("spellHoverVisible"), "旧悬停详情可以完整清理");
    }

    private static void ConfigureShotSpecial(Shot shot)
    {
        if (shot.Special == "hover")
        {
            IList offers = (IList)Get("marketOffers");
            hoverBaselineOffers = Copy(offers);
            hoverBaselineKinds = new string[offers.Count];
            for (int i = 0; i < offers.Count; i++) hoverBaselineKinds[i] = Kind(offers[i]);
            hoverBaselineSpells = Copy((IList)Get("spellBag"));
            hoverBaselineGold = (int)Get("chapterGold");
        }
        else if (shot.Special == "purchase")
        {
            IList visible = (IList)Call("VisibleMarketShelfIndices", true);
            Call("BuyOffer", (int)visible[visible.Count - 1]);
        }
        else if (shot.Special == "target")
        {
            ((IList)Get("spellBag")).Add(ParseNested("SpellType", "TargetedRestock"));
            Set("spellCastingRackIndex", 0);
            Set("spellCastStep", "ChooseShelf");
            Set("selectedSpellFamily", "Turtle");
            Set("spellPanelOpen", true);
        }
        else if (shot.Special == "locked")
        {
            IList bag = (IList)Get("dice");
            bag[0] = Call("NewDie", "魔蝠骰子", ParseNested("DieType", "DemonBat"), new int[] { 1, 2, 3, 4, 5, 6 });
            Call("BeginMarketLeaveEffects");
            Set("marketLeaveEffectTimer", 999f);
            Require((bool)Get("marketLeaveEffectSequenceActive"), "真实离场队列进入交易锁定");
        }
        else if (shot.Special == "bag")
        {
            IList spells = (IList)Get("spellBag");
            spells.Add(ParseNested("SpellType", "PassBoost"));
            spells.Add(ParseNested("SpellType", "TargetedRestock"));
            spells.Add(ParseNested("SpellType", "EchoCommand"));
            Set("spellCastStep", "None");
            Set("spellPanelOpen", true);
        }
        else if (shot.Special == "dice_purchase")
        {
            IList visible = (IList)Call("VisibleMarketShelfIndices", false);
            Call("BuyOffer", (int)visible[0]);
        }
        else if (shot.Special == "no_gold")
        {
            Set("chapterGold", 0);
        }
        else if (shot.Special == "mixed_prices")
        {
            Set("chapterGold", 6);
            IList offers = (IList)Get("marketOffers");
            int[] prices = { 12, 6, 5 };
            for (int i = 0; i < prices.Length; i++) Member(offers[i], "Price").SetValue(offers[i], prices[i]);
            Require(!(bool)Call("CanBuyMarketOffer", offers[0]) && (bool)Call("CanBuyMarketOffer", offers[1]) && (bool)Call("CanBuyMarketOffer", offers[2]), "三商品混合资格为高价受阻、等价可买、低价可买");
        }
        else if (shot.Special == "bag_full" || shot.Special == "blood_pact")
        {
            IList dice = (IList)Get("dice");
            dice.Add(Call("NewDie", "基础骰", ParseNested("DieType", "Basic"), new int[] { 1, 2, 3, 4, 5, 6 }));
            if (shot.Special == "blood_pact") dice[0] = Call("NewDie", "血契船长", ParseNested("DieType", "BloodPactCaptain"), new int[] { 1, 2, 3, 4, 5, 6 });
            Require((bool)Call("CanBuyMarketOffer", ((IList)Get("marketOffers"))[0]) == (shot.Special == "blood_pact"), "满袋截图使用真实普通阻止或血契例外资格");
        }
        IList visibleOffers = (IList)Get("marketOffers");
        StringBuilder eligibility = new StringBuilder(shot.Name + ":");
        for (int i = 0; i < visibleOffers.Count; i++) eligibility.Append(" ").Append(i).Append("=").Append((bool)Call("CanBuyMarketOffer", visibleOffers[i]) ? "可买" : "受阻");
        if (!report.PurchaseEligibility.Contains(eligibility.ToString())) report.PurchaseEligibility.Add(eligibility.ToString());
    }

    private static void SendHover(int width, int height)
    {
        if (gameView == null) return;
        Rect card = (Rect)Call("VariableMarketOfferRect", true, 3);
        Vector2 point = new Vector2(card.x + 28f, card.y + 70f);
        SetInjectedPointer(point);
        lastSentPointerEvent = "MouseMove";
        gameView.SendEvent(new Event { type = EventType.MouseMove, mousePosition = GamePointToEditorPoint(point, width) });
        WritePointerDiagnostic("sent MouseMove");
        gameView.Repaint();
    }

    private static void SendTargetPointer(int width, int button, EventType type)
    {
        IList visible = (IList)Call("VisibleMarketShelfIndices", true);
        int slot = -1;
        for (int i = 0; i < visible.Count; i++) if ((int)visible[i] == pointerTargetIndex) slot = i;
        if (slot < 0) throw new InvalidOperationException("待选择的原货位已不在当前法术页。");
        Rect card = (Rect)Call("VariableMarketOfferRect", true, slot);
        Rect key = (Rect)Call("VariableMarketBuyRect", true, card);
        SetInjectedPointer(key.center);
        lastSentPointerEvent = type.ToString();
        gameView.SendEvent(new Event { type = type, button = button, mousePosition = GamePointToEditorPoint(key.center, width) });
        gameView.Repaint();
    }

    private static void QueueTargetGuiPointer(EventType type)
    {
        IList visible = (IList)Call("VisibleMarketShelfIndices", true);
        int slot = -1;
        for (int i = 0; i < visible.Count; i++) if ((int)visible[i] == pointerTargetIndex) slot = i;
        if (slot < 0) throw new InvalidOperationException("排队输入的目标货架不在当前显示页。");
        Rect card = (Rect)Call("VariableMarketOfferRect", true, slot);
        Rect key = (Rect)Call("VariableMarketBuyRect", true, card);
        SetInjectedPointer(key.center);
        QueuedGuiInputs.Enqueue(new GuiPointerInput { Type = type, VirtualPoint = key.center });
    }

    private static void VerifyHoverModelUnchanged(int width, int height)
    {
        IList offers = (IList)Get("marketOffers");
        IList spells = (IList)Get("spellBag");
        Require(hoverBaselineOffers != null && offers.Count == hoverBaselineOffers.Length, "悬停前后完整货架数量保持不变");
        int diceCount = 0, spellCount = 0, emptyCount = 0;
        for (int i = 0; i < offers.Count; i++)
        {
            Require(ReferenceEquals(offers[i], hoverBaselineOffers[i]) && Kind(offers[i]) == hoverBaselineKinds[i], "悬停前后每个原始商品及Kind保持不变");
            string kind = Kind(offers[i]);
            if (kind == "Die") diceCount++;
            if (kind == "Spell") spellCount++;
            if (kind == "Empty") emptyCount++;
        }
        Require((int)Get("chapterGold") == hoverBaselineGold, "悬停前后金币保持不变");
        Require(spells.Count == hoverBaselineSpells.Length, "悬停前后法术袋数量保持不变");
        for (int i = 0; i < spells.Count; i++) Require(Equals(spells[i], hoverBaselineSpells[i]), "悬停前后法术袋内容与顺序保持不变");
        report.HoverModelInvariance.Add(width + "×" + height + ": offers=" + offers.Count + ", die=" + diceCount + ", spell=" + spellCount + ", empty=" + emptyCount + ", gold=" + hoverBaselineGold + ", spellBag=" + spells.Count + "；前后相同");
    }

    private static Vector2 GamePointToEditorPoint(Vector2 virtualPoint, int renderWidth)
    {
        Type viewType = gameView.GetType();
        PropertyInfo offsetProperty = viewType.GetProperty("gameMouseOffset", Flags);
        PropertyInfo scaleProperty = viewType.GetProperty("gameMouseScale", Flags);
        if (offsetProperty == null || scaleProperty == null)
            throw new MissingMemberException("Unity 2019 GameView 的鼠标缩放映射接口不可用，拒绝使用未校准坐标冒充点击通过。");
        Vector2 offset = (Vector2)offsetProperty.GetValue(gameView, null);
        float mouseScale = (float)scaleProperty.GetValue(gameView, null);
        if (mouseScale <= 0f) throw new InvalidOperationException("GameView 鼠标坐标缩放无效。");
        // Unity 2019 GameView.OnGUI 使用 (editorPoint + gameMouseOffset) *
        // gameMouseScale 送入玩家事件队列；这里取逆，包含工具条、留边及 Zoom。
        lastPointerVirtual = virtualPoint;
        lastPointerOffset = offset;
        lastPointerScale = mouseScale;
        lastPointerWindow = virtualPoint * (renderWidth / 1280f) / mouseScale - offset;
        return lastPointerWindow;
    }

    private static void WritePointerDiagnostic(string reason)
    {
        double now = EditorApplication.timeSinceStartup;
        if (demo == null || gameView == null || now - lastPointerDiagnosticAt < 0.3d) return;
        lastPointerDiagnosticAt = now;
        PointerDiagnostic diagnostic = new PointerDiagnostic
        {
            Reason = reason,
            Shot = Shots[captureIndex % Shots.Length].Name,
            EditorTime = now,
            UnscaledTime = Time.unscaledTime,
            Frame = Time.frameCount,
            Stage = captureStage,
            ScreenWidth = Screen.width,
            ScreenHeight = Screen.height,
            VirtualPoint = lastPointerVirtual,
            WindowPoint = lastPointerWindow,
            GameMouseOffset = lastPointerOffset,
            GameMouseScale = lastPointerScale,
            WindowRect = gameView.position,
            TargetInView = ViewRectProperty("targetInView"),
            ViewInWindow = ViewRectProperty("viewInWindow"),
            UiMousePosition = OptionalValue<Vector2>("uiMousePosition"),
            UiMousePositionValid = OptionalValue<bool>("uiMousePositionValid"),
            CandidateKey = OptionalValue<string>("spellHoverCandidateKey"),
            HoverKey = OptionalValue<string>("spellHoverKey"),
            HoverVisible = OptionalValue<bool>("spellHoverVisible"),
            HoverStartedAt = OptionalValue<float>("spellHoverStartedAt"),
            HoverDelay = OptionalValue<float>("spellHoverDelay"),
            WantsMouseMove = gameView.wantsMouseMove,
            LastSentEvent = lastSentPointerEvent,
            HasCurrentEvent = Event.current != null,
            CurrentEventType = Event.current != null ? Event.current.type.ToString() : "EditorUpdate",
            CurrentRawType = Event.current != null ? Event.current.rawType.ToString() : string.Empty,
            CurrentRawMouse = Event.current != null ? Event.current.mousePosition : Vector2.zero,
            GuiInputInjectionActive = injectedPointerActive,
            LastInjectedGuiEventType = injectedGuiEventType,
            RawMouseBeforeInjection = rawPointerBeforeInjection,
            RawMouseAfterInjection = rawPointerAfterInjection,
            InjectedGuiEvents = injectedGuiEvents
        };
        File.AppendAllText(Path.Combine(outputDirectory, "pointer_diagnostics.jsonl"), JsonUtility.ToJson(diagnostic) + Environment.NewLine, new UTF8Encoding(false));
    }

    private static T OptionalValue<T>(string name)
    {
        FieldInfo field = DemoType.GetField(name, Flags);
        return field == null ? default(T) : (T)field.GetValue(demo);
    }

    private static Rect ViewRectProperty(string name)
    {
        PropertyInfo property = gameView.GetType().GetProperty(name, Flags);
        return property == null ? default(Rect) : (Rect)property.GetValue(gameView, null);
    }

    private static void NextCapture()
    {
        captureIndex++;
        if (captureIndex == Shots.Length * 2) { Require(injectedGuiEvents > 0, "受控测试GUI宿主原始输入驱动实际执行"); FinishCapture(0, "通过：隔离工程检查、" + (Shots.Length * 2) + "张原生双分辨率截图与受控GUI测试宿主的末页换货点击完成；未验证OS鼠标或普通Mono自动派发，视觉仍需人工复核。"); return; }
        Advance(0);
    }

    private static void ApplyResolution(int width, int height)
    {
        Type viewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        if (gameView == null)
        {
            // 隔离批处理不复用默认 100×100 的隐藏或停靠 GameView。
            gameView = ScriptableObject.CreateInstance(viewType) as EditorWindow;
            captureOwnsGameView = true;
            gameView.titleContent = new GUIContent("Wabish Market UI Capture");
            oldSizeIndex = (int)viewType.GetProperty("selectedSizeIndex", Flags).GetValue(gameView, null);
        }
        Vector2 windowSize = new Vector2(width + 32f, height + 80f);
        gameView.maxSize = new Vector2(4096f, 4096f);
        gameView.minSize = windowSize;
        gameView.position = new Rect(32f, 32f, windowSize.x, windowSize.y);
        gameView.wantsMouseMove = true;
        gameView.Show();
        int index = EnsureCustomGameViewSize(width, height);
        viewType.GetProperty("selectedSizeIndex", Flags).SetValue(gameView, index, null);
        MethodInfo refreshZoom = viewType.GetMethod("UpdateZoomAreaAndParent", Flags);
        if (refreshZoom != null) refreshZoom.Invoke(gameView, null);
        MethodInfo resized = viewType.GetMethod("OnResized", Flags);
        if (resized != null) resized.Invoke(gameView, null);
        gameView.Focus();
        gameView.Repaint();
        lastResolutionRequestAt = EditorApplication.timeSinceStartup;
    }

    private static void SetInjectedPointer(Vector2 virtualPoint)
    {
        injectedPointerVirtual = virtualPoint;
        injectedPointerActive = true;
    }

    private static void ClearInjectedPointer()
    {
        injectedPointerActive = false;
        injectedPointerVirtual = new Vector2(-9999f, -9999f);
        QueuedGuiInputs.Clear();
    }

    // 只注入原始GUI输入，不接触生产hover状态、候选、计时或交易提交方法。
    public static void ApplyTestGuiPointer()
    {
        if (!injectedPointerActive || !SessionState.GetBool(ActiveKey, false) || Event.current == null) return;
        float fit = Mathf.Min(Screen.width / 1920f, Screen.height / 1080f);
        if (fit <= 0f) return;
        Vector2 offset = new Vector2((Screen.width - 1920f * fit) * 0.5f, (Screen.height - 1080f * fit) * 0.5f);
        rawPointerBeforeInjection = Event.current.mousePosition;
        rawPointerAfterInjection = offset + injectedPointerVirtual * (fit * 1.5f);
        injectedGuiEventType = Event.current.type.ToString();
        Event.current.mousePosition = rawPointerAfterInjection;
        injectedGuiEvents++;
    }

    public static void DrawProductionGuiForValidation()
    {
        if (!SessionState.GetBool(ActiveKey, false) || demo == null || Event.current == null) return;
        if (ProductionOnGui == null) throw new MissingMethodException("DiceKingDemo.OnGUI");
        Event originalEvent = Event.current;
        Vector2 originalMouse = originalEvent.mousePosition;
        EventType originalType = originalEvent.type;
        int originalButton = originalEvent.button;
        int originalClickCount = originalEvent.clickCount;
        GuiPointerInput queued = originalEvent.type == EventType.Repaint && QueuedGuiInputs.Count > 0 ? QueuedGuiInputs.Dequeue() : null;
        // 宿主是受控输入测试，不让未排队的OS点击干扰夹具或悬停不变性检查。
        if (queued == null && (originalEvent.type == EventType.MouseDown || originalEvent.type == EventType.MouseUp)) return;
        GuiInputTrace trace = null;
        if (queued != null)
        {
            SetInjectedPointer(queued.VirtualPoint);
            // 在原生GUI当前事件对象上改字段，保持同一个native event pointer。
            originalEvent.type = queued.Type;
            originalEvent.button = 0;
            originalEvent.clickCount = 1;
            if (queued.Type == EventType.MouseDown) { GUIUtility.hotControl = 0; GUIUtility.keyboardControl = 0; }
            IList offers = (IList)Get("marketOffers");
            trace = new GuiInputTrace { Type = queued.Type.ToString(), Frame = Time.frameCount, UnscaledTime = Time.unscaledTime, VirtualPoint = queued.VirtualPoint, HotControlBefore = GUIUtility.hotControl, TargetIndex = pointerTargetIndex, TargetKindBefore = Kind(offers[pointerTargetIndex]), SpellBagBefore = ((IList)Get("spellBag")).Count, RawTypeBefore = originalEvent.rawType.ToString(), TypeForControlBefore = originalEvent.GetTypeForControl(GUIUtility.hotControl).ToString(), ScreenWidthBefore = Screen.width, ScreenHeightBefore = Screen.height, GuiMatrixBefore = GUI.matrix.ToString() };
        }
        ApplyTestGuiPointer();
        if (trace != null) trace.RawPoint = Event.current.mousePosition;
        try
        {
            ProductionOnGui.Invoke(demo, null);
            if (trace != null)
            {
                trace.HotControlAfter = GUIUtility.hotControl;
                trace.TargetKindAfter = Kind(((IList)Get("marketOffers"))[pointerTargetIndex]);
                trace.SpellBagAfter = ((IList)Get("spellBag")).Count;
                trace.SpellStepAfter = Get("spellCastStep").ToString();
                trace.EventTypeAfter = Event.current.type.ToString();
                trace.RawTypeAfter = Event.current.rawType.ToString();
                trace.TypeForControlAfter = Event.current.GetTypeForControl(GUIUtility.hotControl).ToString();
                trace.ConsumedAfter = Event.current.type == EventType.Used;
                trace.ScreenWidthAfter = Screen.width;
                trace.ScreenHeightAfter = Screen.height;
                trace.UiMouseAfter = OptionalValue<Vector2>("uiMousePosition");
                trace.UiMouseValidAfter = OptionalValue<bool>("uiMousePositionValid");
                trace.GuiEnabledAfter = GUI.enabled;
                trace.GuiMatrixAfter = GUI.matrix.ToString();
                File.AppendAllText(Path.Combine(outputDirectory, "gui_input_events.jsonl"), JsonUtility.ToJson(trace) + Environment.NewLine, new UTF8Encoding(false));
                if (queued.Type == EventType.MouseDown)
                {
                    Require(trace.ConsumedAfter, "受控原始MouseDown由生产GUI.Button消费，非先验要求hotControl非零");
                }
                else if (queued.Type == EventType.MouseUp)
                {
                    Require(trace.ConsumedAfter, "受控原始MouseUp由生产GUI.Button消费，最终交易再验证原索引与法术消耗");
                }
            }
        }
        finally
        {
            originalEvent.type = originalType;
            originalEvent.button = originalButton;
            originalEvent.clickCount = originalClickCount;
            originalEvent.mousePosition = originalMouse;
        }
    }

    private static int EnsureCustomGameViewSize(int width, int height)
    {
        Assembly editorAssembly = typeof(Editor).Assembly;
        Type sizesType = editorAssembly.GetType("UnityEditor.GameViewSizes");
        if (sizesType == null) throw new InvalidOperationException("Unity GameViewSizes 不可用。");
        Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        object sizes = singletonType.GetProperty("instance").GetValue(null, null);
        object group = sizesType.GetMethod("GetGroup").Invoke(sizes, new object[] { (int)GameViewSizeGroupType.Standalone });
        Type groupType = group.GetType();
        int count = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null);
        MethodInfo getSize = groupType.GetMethod("GetGameViewSize");
        for (int i = 0; i < count; i++)
        {
            object size = getSize.Invoke(group, new object[] { i });
            Type sizeType = size.GetType();
            int sizeWidth = (int)sizeType.GetProperty("width").GetValue(size, null);
            int sizeHeight = (int)sizeType.GetProperty("height").GetValue(size, null);
            PropertyInfo kindProperty = sizeType.GetProperty("sizeType", Flags);
            FieldInfo kindField = sizeType.GetField("m_SizeType", Flags);
            object kind = kindProperty != null ? kindProperty.GetValue(size, null) : kindField != null ? kindField.GetValue(size) : null;
            if (sizeWidth == width && sizeHeight == height && kind != null && kind.ToString() == "FixedResolution") return i;
        }
        Type gameViewSizeType = editorAssembly.GetType("UnityEditor.GameViewSize");
        Type sizeKindType = editorAssembly.GetType("UnityEditor.GameViewSizeType");
        ConstructorInfo constructor = gameViewSizeType.GetConstructor(new[] { sizeKindType, typeof(int), typeof(int), typeof(string) });
        object newSize = constructor.Invoke(new object[] { Enum.Parse(sizeKindType, "FixedResolution"), width, height, "Wabish Variable Market " + width + "x" + height });
        groupType.GetMethod("AddCustomSize").Invoke(group, new[] { newSize });
        return count;
    }

    private static int PageCount(bool spells) { return (int)Call("MarketShelfPageCount", spells); }
    private static bool Inside(Rect inner, Rect outer) { return inner.xMin >= outer.xMin - 0.01f && inner.yMin >= outer.yMin - 0.01f && inner.xMax <= outer.xMax + 0.01f && inner.yMax <= outer.yMax + 0.01f; }
    private static object[] Copy(IList list) { object[] result = new object[list.Count]; for (int i = 0; i < list.Count; i++) result[i] = list[i]; return result; }
    private static string Kind(object offer) { return Member(offer, "Kind").GetValue(offer).ToString(); }
    private static Type Nested(string name) { Type type = DemoType.GetNestedType(name, BindingFlags.NonPublic | BindingFlags.Public); if (type == null) throw new MissingMemberException(name); return type; }
    private static object ParseNested(string name, string value) { return Enum.Parse(Nested(name), value); }
    private static FieldInfo Member(object value, string field) { FieldInfo info = value.GetType().GetField(field, Flags); if (info == null) throw new MissingFieldException(value.GetType().Name, field); return info; }
    private static object Get(string field) { FieldInfo info = DemoType.GetField(field, Flags); if (info == null) throw new MissingFieldException(DemoType.Name, field); return info.GetValue(demo); }
    private static void Set(string field, object value, bool required = true)
    {
        FieldInfo info = DemoType.GetField(field, Flags);
        if (info == null) { if (required) throw new MissingFieldException(DemoType.Name, field); return; }
        if (info.FieldType.IsEnum && value is string) value = Enum.Parse(info.FieldType, (string)value);
        info.SetValue(demo, value);
    }
    private static object Call(string name, params object[] arguments)
    {
        foreach (MethodInfo method in DemoType.GetMethods(Flags))
        {
            if (method.Name != name || method.GetParameters().Length != arguments.Length) continue;
            return method.Invoke(demo, arguments);
        }
        throw new MissingMethodException(DemoType.Name, name);
    }
    private static void Require(bool condition, string message)
    {
        report.AssertionCount++;
        if (!condition) throw new InvalidOperationException(message);
        if (!report.Checks.Contains(message)) report.Checks.Add(message);
    }
    private static Exception Unwrap(Exception exception) { return exception is TargetInvocationException && exception.InnerException != null ? exception.InnerException : exception; }
    private static Dictionary<string, string> ReadSelectedPrefs()
    {
        string[] ints = { "HasSave", "SaveVersion", "StageIndex", "Gold", "RemainingLives", "CurrentScore", "SeenOpening", "DisplayMode", "WindowWidth", "WindowHeight", "Windowed", "WindowMaximized", "ExclusiveFullscreenWidth", "ExclusiveFullscreenHeight", "MenuLampFlickerEnabled", "ReducedFlashing" };
        string[] strings = { "DiceData", "SpellBagData", "PendingSpellEchoDieIds" };
        Dictionary<string, string> result = new Dictionary<string, string>();
        foreach (string key in ints) result[key] = PlayerPrefs.HasKey("DiceKingDemo." + key) ? PlayerPrefs.GetInt("DiceKingDemo." + key).ToString() : "<missing>";
        foreach (string key in strings) result[key] = PlayerPrefs.HasKey("DiceKingDemo." + key) ? PlayerPrefs.GetString("DiceKingDemo." + key) : "<missing>";
        foreach (string key in new[] { "Volume", "SettlementPlaybackSpeed" }) result[key] = PlayerPrefs.HasKey("DiceKingDemo." + key) ? PlayerPrefs.GetFloat("DiceKingDemo." + key).ToString("R", System.Globalization.CultureInfo.InvariantCulture) : "<missing>";
        return result;
    }
    private static bool SamePrefs(Dictionary<string, string> left, Dictionary<string, string> right) { foreach (KeyValuePair<string, string> pair in left) if (!right.ContainsKey(pair.Key) || right[pair.Key] != pair.Value) return false; return true; }
    private static string DefaultOutputDirectory() { return Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/QA/MarketVariableShelvesUnity20261109")); }
    private static string ResolveOutputDirectory() { string custom = CommandLineValue("-marketUiOutputDir"); if (string.IsNullOrEmpty(custom)) custom = CommandLineValue("-outputDir"); return string.IsNullOrEmpty(custom) ? DefaultOutputDirectory() : Path.GetFullPath(custom); }
    private static string CommandLineValue(string name) { string[] args = Environment.GetCommandLineArgs(); for (int i = 0; i + 1 < args.Length; i++) if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase)) return args[i + 1]; return string.Empty; }
    private static string ShotPath(Shot shot, int width, int height) { return Path.Combine(outputDirectory, shot.Name + "_" + width + "x" + height + ".png"); }
    private static void Advance(int stage) { captureStage = stage; stageStarted = EditorApplication.timeSinceStartup; if (stage == 0) { lastHoverEventAt = -999d; lastPointerDiagnosticAt = -999d; } }
    private static double Elapsed() { return EditorApplication.timeSinceStartup - stageStarted; }
    private static void WriteReport(string file, string status)
    {
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllText(Path.Combine(outputDirectory, file), JsonUtility.ToJson(report, true), new UTF8Encoding(false));
        File.WriteAllText(Path.Combine(outputDirectory, file == "validation.json" ? "validation_status.txt" : "capture_status.txt"), status + Environment.NewLine, new UTF8Encoding(false));
    }
    private static void DestroyTemporaryDemo()
    {
        ClearInjectedPointer();
        if (host != null) UnityEngine.Object.DestroyImmediate(host);
        host = null; demo = null;
        Time.timeScale = oldTimeScale;
        AudioListener.volume = oldVolume;
        UnityEngine.Random.state = oldRandom;
    }
    private static void FinishCapture(int code, string status)
    {
        finishing = true;
        SessionState.SetBool(ActiveKey, false);
        EditorApplication.update -= Tick;
        report.Passed = code == 0;
        report.InjectedGuiEvents = injectedGuiEvents;
        report.SuppressSave = demo != null && (bool)Get("suppressSave");
        DestroyTemporaryDemo();
        foreach (GameObject original in HiddenOriginals) if (original != null) UnityEngine.Object.DestroyImmediate(original);
        HiddenOriginals.Clear();
        if (gameView != null)
        {
            if (captureOwnsGameView) gameView.Close();
            else gameView.GetType().GetProperty("selectedSizeIndex", Flags).SetValue(gameView, oldSizeIndex, null);
        }
        gameView = null;
        WriteReport("capture.json", status);
        if (code != 0) Debug.LogError(status); else Debug.Log(status);
        if (Application.isBatchMode) EditorApplication.Exit(code); else EditorApplication.isPlaying = false;
    }
}

// 编辑器验证专用受控GUI宿主：真实OnGUI上下文、原始事件类型与生产绘制/按钮均保留。
[DefaultExecutionOrder(-32000)]
public sealed class WabishMarketVariableUiInputDriver : MonoBehaviour
{
    public static string LastError;

    private void OnGUI()
    {
        if (!string.IsNullOrEmpty(LastError)) return;
        try
        {
            WabishMarketVariableUiCapture.DrawProductionGuiForValidation();
        }
        catch (Exception exception)
        {
            Exception actual = exception is TargetInvocationException && exception.InnerException != null ? exception.InnerException : exception;
            LastError = actual.ToString();
            Debug.LogException(actual);
        }
    }
}
