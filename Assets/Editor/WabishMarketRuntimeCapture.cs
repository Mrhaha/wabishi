using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WabishMarketRuntimeCapture
{
    private const string ActiveKey = "Wabish.MarketRuntimeCapture.Active";
    private const string StepKey = "Wabish.MarketRuntimeCapture.Step";
    private const string LaunchPendingKey = "Wabish.MarketRuntimeCapture.LaunchPending";
    private const string TriggerFileName = "WabishMarketRuntimeCapture.request";
    private const string StatusFileName = "20260717_market_runtime_capture_status.txt";
    private const double ResolutionRetryIntervalSeconds = 0.5d;
    private const double ResolutionCaptureTimeoutSeconds = 10d;
    private const int RequiredStableResolutionFrames = 2;

    private static readonly string[] ExpectedFiles =
    {
        "20260717_market_runtime_normal_1920x1080.png",
        "20260717_market_runtime_purchase_1920x1080.png",
        "20260717_market_runtime_leave_1920x1080.png",
        "20260717_market_runtime_normal_1280x720.png",
        "20260717_market_runtime_purchase_1280x720.png",
        "20260717_market_runtime_leave_1280x720.png"
    };

    private static DiceKingDemo demo;
    private static Type demoType;
    private static MethodInfo startDefaultBuildMethod;
    private static MethodInfo enterMarketMethod;
    private static MethodInfo buyOfferMethod;
    private static MethodInfo beginMarketLeaveEffectsMethod;
    private static FieldInfo modeField;
    private static FieldInfo diceField;
    private static FieldInfo marketOffersField;
    private static FieldInfo chapterGoldField;
    private static FieldInfo selectedMarketDieIndexField;
    private static FieldInfo marketLeaveEffectSequenceActiveField;
    private static FieldInfo marketCommonBaseTextureField;
    private static FieldInfo physicalKeyLabelFontField;
    private static FieldInfo affixFeatureEnabledField;
    private static FieldInfo suppressSaveField;
    private static float stepStartedAt;
    private static bool registered;
    private static string outputDirectory;
    private static int expectedWidth;
    private static int expectedHeight;
    private static double resolutionWaitStartedAt = -1d;
    private static double lastResolutionRequestAt = -1d;
    private static int stableResolutionFrameCount;

    [InitializeOnLoadMethod]
    private static void ResumeAfterDomainReload()
    {
        EditorApplication.update -= MonitorTriggerFile;
        EditorApplication.update += MonitorTriggerFile;

        string triggerPath = TriggerPath();
        if (File.Exists(triggerPath))
        {
            File.Delete(triggerPath);
            SessionState.SetBool(LaunchPendingKey, true);
        }

        if (SessionState.GetBool(LaunchPendingKey, false))
        {
            EditorApplication.update -= LaunchWhenReady;
            EditorApplication.update += LaunchWhenReady;
        }

        if (SessionState.GetBool(ActiveKey, false))
        {
            EditorApplication.delayCall += RegisterUpdate;
        }
    }

    private static void MonitorTriggerFile()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating ||
            SessionState.GetBool(ActiveKey, false) || SessionState.GetBool(LaunchPendingKey, false))
        {
            return;
        }

        string triggerPath = TriggerPath();
        if (!File.Exists(triggerPath))
        {
            return;
        }

        File.Delete(triggerPath);
        SessionState.SetBool(LaunchPendingKey, true);
        EditorApplication.update -= LaunchWhenReady;
        EditorApplication.update += LaunchWhenReady;
    }

    private static string TriggerPath()
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp", TriggerFileName));
    }

    private static void LaunchWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        EditorApplication.update -= LaunchWhenReady;
        SessionState.SetBool(LaunchPendingKey, false);
        Capture();
    }

    public static void Capture()
    {
        SessionState.SetBool(LaunchPendingKey, false);
        outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/QA"));
        Directory.CreateDirectory(outputDirectory);
        DeletePreviousOutputs();
        demo = null;
        demoType = null;
        registered = false;
        Time.timeScale = 1f;
        SessionState.SetBool(ActiveKey, true);
        SessionState.SetInt(StepKey, 0);

        if (!EditorApplication.isPlayingOrWillChangePlaymode)
        {
            EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
            return;
        }

        RegisterUpdate();
    }

    private static void DeletePreviousOutputs()
    {
        string statusPath = Path.Combine(outputDirectory, StatusFileName);
        if (File.Exists(statusPath))
        {
            File.Delete(statusPath);
        }

        for (int i = 0; i < ExpectedFiles.Length; i++)
        {
            string path = Path.Combine(outputDirectory, ExpectedFiles[i]);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static void RegisterUpdate()
    {
        if (registered || !SessionState.GetBool(ActiveKey, false))
        {
            return;
        }

        registered = true;
        outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/QA"));
        stepStartedAt = Time.realtimeSinceStartup;
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        try
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            if (demo == null)
            {
                demo = UnityEngine.Object.FindObjectOfType<DiceKingDemo>();
                if (demo == null)
                {
                    if (StepElapsed() > 12f)
                    {
                        Fail("DiceKingDemo did not bootstrap in Play Mode.");
                    }
                    return;
                }

                CacheReflectionHooks();
            }

            int step = SessionState.GetInt(StepKey, 0);
            switch (step)
            {
                case 0:
                    if (StepElapsed() < 1f) return;
                    ConfigureFreshMarket();
                    SetResolution(1920, 1080);
                    Advance(1);
                    break;
                case 1:
                    if (StepElapsed() < 0.8f) return;
                    if (!TryCaptureFrame(ExpectedFiles[0])) return;
                    Advance(2);
                    break;
                case 2:
                    if (StepElapsed() < 0.45f) return;
                    buyOfferMethod.Invoke(demo, new object[] { 1 });
                    Advance(3);
                    break;
                case 3:
                    if (StepElapsed() < 0.65f) return;
                    if (!TryCaptureFrame(ExpectedFiles[1])) return;
                    Advance(4);
                    break;
                case 4:
                    if (StepElapsed() < 0.45f) return;
                    BeginPausedLeaveState();
                    Advance(5);
                    break;
                case 5:
                    if (StepElapsed() < 0.65f) return;
                    if (!TryCaptureFrame(ExpectedFiles[2])) return;
                    Advance(6);
                    break;
                case 6:
                    Time.timeScale = 1f;
                    ConfigureFreshMarket();
                    SetResolution(1280, 720);
                    Advance(7);
                    break;
                case 7:
                    if (StepElapsed() < 0.8f) return;
                    if (!TryCaptureFrame(ExpectedFiles[3])) return;
                    Advance(8);
                    break;
                case 8:
                    if (StepElapsed() < 0.45f) return;
                    buyOfferMethod.Invoke(demo, new object[] { 1 });
                    Advance(9);
                    break;
                case 9:
                    if (StepElapsed() < 0.65f) return;
                    if (!TryCaptureFrame(ExpectedFiles[4])) return;
                    Advance(10);
                    break;
                case 10:
                    if (StepElapsed() < 0.45f) return;
                    BeginPausedLeaveState();
                    Advance(11);
                    break;
                case 11:
                    if (StepElapsed() < 0.65f) return;
                    if (!TryCaptureFrame(ExpectedFiles[5])) return;
                    Advance(12);
                    break;
                case 12:
                    if (StepElapsed() < 0.8f) return;
                    Time.timeScale = 1f;
                    ValidateAndFinish();
                    break;
            }
        }
        catch (Exception exception)
        {
            Fail(exception.ToString());
        }
    }

    private static void CacheReflectionHooks()
    {
        demoType = typeof(DiceKingDemo);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        startDefaultBuildMethod = demoType.GetMethod("StartDefaultBuild", flags);
        enterMarketMethod = demoType.GetMethod("EnterMarket", flags);
        buyOfferMethod = demoType.GetMethod("BuyOffer", flags);
        beginMarketLeaveEffectsMethod = demoType.GetMethod("BeginMarketLeaveEffects", flags);
        modeField = demoType.GetField("mode", flags);
        diceField = demoType.GetField("dice", flags);
        marketOffersField = demoType.GetField("marketOffers", flags);
        chapterGoldField = demoType.GetField("chapterGold", flags);
        selectedMarketDieIndexField = demoType.GetField("selectedMarketDieIndex", flags);
        marketLeaveEffectSequenceActiveField = demoType.GetField("marketLeaveEffectSequenceActive", flags);
        marketCommonBaseTextureField = demoType.GetField("marketCommonBaseTexture", flags);
        physicalKeyLabelFontField = demoType.GetField("physicalKeyLabelFont", flags);
        affixFeatureEnabledField = demoType.GetField("affixFeatureEnabled", flags);
        suppressSaveField = demoType.GetField("suppressSave", flags);

        if (startDefaultBuildMethod == null || enterMarketMethod == null || buyOfferMethod == null || beginMarketLeaveEffectsMethod == null ||
            modeField == null || diceField == null || marketOffersField == null || chapterGoldField == null ||
            selectedMarketDieIndexField == null || marketLeaveEffectSequenceActiveField == null || marketCommonBaseTextureField == null || physicalKeyLabelFontField == null ||
            affixFeatureEnabledField == null || suppressSaveField == null)
        {
            throw new InvalidOperationException("Market capture reflection hooks are missing from DiceKingDemo.");
        }
    }

    private static void ConfigureFreshMarket()
    {
        suppressSaveField.SetValue(demo, true);
        affixFeatureEnabledField.SetValue(demo, false);
        startDefaultBuildMethod.Invoke(demo, null);
        chapterGoldField.SetValue(demo, 50);

        IList dice = diceField.GetValue(demo) as IList;
        if (dice == null || dice.Count < 5)
        {
            throw new InvalidOperationException("Default build does not contain five dice for the market showcase.");
        }
        while (dice.Count > 5)
        {
            dice.RemoveAt(dice.Count - 1);
        }

        string[] bagTypes = { "Basic", "Piggy", "Turtle", "DemonBat", "RobberyPirate" };
        string[] bagNames = { "基础骰子", "猪猪骰子", "龟龟骰子", "黑帆蝙蝠", "劫掠海盗" };
        ConfigureDiceList(dice, bagTypes, bagNames);

        enterMarketMethod.Invoke(demo, new object[] { false });
        modeField.SetValue(demo, Enum.Parse(modeField.FieldType, "InterStageMarket"));
        selectedMarketDieIndexField.SetValue(demo, 0);
        ConfigureOfferShowcase();

        if (marketCommonBaseTextureField.GetValue(demo) == null)
        {
            throw new InvalidOperationException("Resources/Art/Market/arcade_market_common_base did not load.");
        }
        if (physicalKeyLabelFontField.GetValue(demo) == null)
        {
            throw new InvalidOperationException("Resources/Art/MainGame/Flow/wabish_physical_key_sans_sc_semibold did not load.");
        }
    }

    private static void ConfigureOfferShowcase()
    {
        IList offers = marketOffersField.GetValue(demo) as IList;
        if (offers == null || offers.Count < 3)
        {
            throw new InvalidOperationException("Market did not build three offers.");
        }

        string[] offerTypes = { "PigFarmer", "MagnetTurtle", "BlackSailBat" };
        string[] offerNames = { "猪猪农场", "磁力龟", "黑帆魔蝠" };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        for (int i = 0; i < 3; i++)
        {
            object offer = offers[i];
            FieldInfo dieField = offer.GetType().GetField("Die", flags);
            object die = dieField != null ? dieField.GetValue(offer) : null;
            if (die == null)
            {
                throw new InvalidOperationException("Market offer " + i + " has no die.");
            }
            ConfigureDie(die, offerTypes[i], offerNames[i]);
        }
    }

    private static void ConfigureDiceList(IList dice, string[] types, string[] names)
    {
        for (int i = 0; i < dice.Count && i < types.Length; i++)
        {
            ConfigureDie(dice[i], types[i], names[i]);
        }
    }

    private static void ConfigureDie(object die, string typeName, string displayName)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo typeField = die.GetType().GetField("Type", flags);
        FieldInfo nameField = die.GetType().GetField("Name", flags);
        if (typeField == null || nameField == null)
        {
            throw new InvalidOperationException("Die type/name reflection hooks are unavailable.");
        }

        typeField.SetValue(die, Enum.Parse(typeField.FieldType, typeName));
        nameField.SetValue(die, displayName);
    }

    private static void BeginPausedLeaveState()
    {
        Time.timeScale = 0f;
        beginMarketLeaveEffectsMethod.Invoke(demo, null);
        bool active = (bool)marketLeaveEffectSequenceActiveField.GetValue(demo);
        if (!active)
        {
            throw new InvalidOperationException("Market leave sequence did not enter its locked state.");
        }
    }

    private static void SetResolution(int width, int height)
    {
        expectedWidth = width;
        expectedHeight = height;
        resolutionWaitStartedAt = -1d;
        stableResolutionFrameCount = 0;
        ApplyResolution(width, height);
    }

    private static void ApplyResolution(int width, int height)
    {
        Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
        int index = EnsureCustomGameViewSize(width, height, "Wabish Market " + width + "x" + height);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        PropertyInfo selectedSizeIndex = gameViewType.GetProperty("selectedSizeIndex", flags);
        if (selectedSizeIndex == null || index < 0)
        {
            throw new InvalidOperationException("Unity GameView resolution selector is unavailable.");
        }

        selectedSizeIndex.SetValue(gameView, index, null);
        gameView.Show();
        gameView.Focus();
        gameView.Repaint();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        lastResolutionRequestAt = EditorApplication.timeSinceStartup;
    }

    private static int EnsureCustomGameViewSize(int width, int height, string label)
    {
        Assembly editorAssembly = typeof(Editor).Assembly;
        Type sizesType = editorAssembly.GetType("UnityEditor.GameViewSizes");
        Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        object sizes = singletonType.GetProperty("instance").GetValue(null, null);
        object group = sizesType.GetMethod("GetGroup").Invoke(sizes, new object[] { (int)GameViewSizeGroupType.Standalone });
        Type groupType = group.GetType();
        int total = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null);
        MethodInfo getGameViewSize = groupType.GetMethod("GetGameViewSize");
        for (int i = 0; i < total; i++)
        {
            object size = getGameViewSize.Invoke(group, new object[] { i });
            Type sizeType = size.GetType();
            int sizeWidth = (int)sizeType.GetProperty("width").GetValue(size, null);
            int sizeHeight = (int)sizeType.GetProperty("height").GetValue(size, null);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo sizeKindProperty = sizeType.GetProperty("sizeType", flags);
            FieldInfo sizeKindField = sizeType.GetField("m_SizeType", flags);
            object sizeKind = sizeKindProperty != null
                ? sizeKindProperty.GetValue(size, null)
                : sizeKindField != null ? sizeKindField.GetValue(size) : null;
            bool fixedResolution = sizeKind != null && string.Equals(sizeKind.ToString(), "FixedResolution", StringComparison.Ordinal);
            if (sizeWidth == width && sizeHeight == height && fixedResolution)
            {
                return i;
            }
        }

        Type gameViewSizeType = editorAssembly.GetType("UnityEditor.GameViewSize");
        Type gameViewSizeKindType = editorAssembly.GetType("UnityEditor.GameViewSizeType");
        ConstructorInfo constructor = gameViewSizeType.GetConstructor(new[] { gameViewSizeKindType, typeof(int), typeof(int), typeof(string) });
        object fixedResolutionKind = Enum.Parse(gameViewSizeKindType, "FixedResolution");
        object newSize = constructor.Invoke(new object[] { fixedResolutionKind, width, height, label });
        groupType.GetMethod("AddCustomSize").Invoke(group, new[] { newSize });
        return total;
    }

    private static bool TryCaptureFrame(string fileName)
    {
        double now = EditorApplication.timeSinceStartup;
        if (Screen.width != expectedWidth || Screen.height != expectedHeight)
        {
            stableResolutionFrameCount = 0;
            if (resolutionWaitStartedAt < 0d)
            {
                resolutionWaitStartedAt = now;
            }
            if (now - lastResolutionRequestAt >= ResolutionRetryIntervalSeconds)
            {
                ApplyResolution(expectedWidth, expectedHeight);
            }
            if (now - resolutionWaitStartedAt >= ResolutionCaptureTimeoutSeconds)
            {
                throw new InvalidOperationException(
                    "GameView did not stabilize. Expected " + expectedWidth + "x" + expectedHeight +
                    ", current " + Screen.width + "x" + Screen.height + ".");
            }
            return false;
        }

        stableResolutionFrameCount++;
        if (stableResolutionFrameCount < RequiredStableResolutionFrames)
        {
            return false;
        }

        resolutionWaitStartedAt = -1d;
        stableResolutionFrameCount = 0;
        ScreenCapture.CaptureScreenshot(Path.Combine(outputDirectory, fileName), 1);
        Debug.Log("WabishMarketRuntimeCapture: captured " + fileName + " at " + Screen.width + "x" + Screen.height + ".");
        return true;
    }

    private static void ValidateAndFinish()
    {
        List<string> invalid = new List<string>();
        for (int i = 0; i < ExpectedFiles.Length; i++)
        {
            string path = Path.Combine(outputDirectory, ExpectedFiles[i]);
            if (!File.Exists(path) || new FileInfo(path).Length <= 0)
            {
                invalid.Add(ExpectedFiles[i] + " (missing)");
                continue;
            }

            int width = ExpectedFiles[i].Contains("1920x1080") ? 1920 : 1280;
            int height = ExpectedFiles[i].Contains("1920x1080") ? 1080 : 720;
            Texture2D screenshot = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            bool loaded = ImageConversion.LoadImage(screenshot, File.ReadAllBytes(path), false);
            if (!loaded || screenshot.width != width || screenshot.height != height)
            {
                invalid.Add(ExpectedFiles[i] + " (unexpected size " + screenshot.width + "x" + screenshot.height + ")");
            }
            UnityEngine.Object.DestroyImmediate(screenshot);
        }

        if (invalid.Count > 0)
        {
            Fail("Invalid screenshots: " + string.Join(", ", invalid.ToArray()));
            return;
        }

        Finish(0, "PASS: six market screenshots captured at 1920x1080 and 1280x720 for normal, purchase-empty and leave-locked states.");
    }

    private static float StepElapsed()
    {
        return Time.realtimeSinceStartup - stepStartedAt;
    }

    private static void Advance(int step)
    {
        SessionState.SetInt(StepKey, step);
        stepStartedAt = Time.realtimeSinceStartup;
    }

    private static void Fail(string message)
    {
        Debug.LogError("WabishMarketRuntimeCapture: " + message);
        Finish(1, "FAIL: " + message);
    }

    private static void Finish(int exitCode, string status)
    {
        Time.timeScale = 1f;
        SessionState.SetBool(ActiveKey, false);
        EditorApplication.update -= Update;
        registered = false;
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllText(
            Path.Combine(outputDirectory, StatusFileName),
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + status + Environment.NewLine);
        AssetDatabase.Refresh();

        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
        if (Application.isBatchMode)
        {
            EditorApplication.Exit(exitCode);
        }
    }
}
