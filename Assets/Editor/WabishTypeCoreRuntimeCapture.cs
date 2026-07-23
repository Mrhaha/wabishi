using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WabishTypeCoreRuntimeCapture
{
    private const string ActiveKey = "Wabish.TypeCoreRuntimeCapture.Active";
    private const string StepKey = "Wabish.TypeCoreRuntimeCapture.Step";
    private const string PendingKey = "Wabish.TypeCoreRuntimeCapture.Pending";
    private const string TriggerFileName = "WabishTypeCoreRuntimeCapture.request";
    private const string StatusFileName = "20260717_type_core_runtime_capture_status.txt";
    private const double ResolutionRetryInterval = 0.5d;
    private const double ResolutionTimeout = 10d;
    private const double CaptureWriteHold = 0.75d;
    private const int StableFramesRequired = 2;

    private static readonly string[] ExpectedFiles =
    {
        "20260717_type_core_ready_1920x1080.png",
        "20260717_type_core_ready_1280x720.png",
        "20260717_type_core_result_1920x1080.png",
        "20260717_type_core_result_1280x720.png",
        "20260717_type_core_market_1920x1080.png",
        "20260717_type_core_market_1280x720.png"
    };

    private static readonly string[] ResourceSlugs =
    {
        "basic", "pig_farmer", "meat_pig", "trade_pig", "sow_pig", "three_little_pigs", "greedy_pig", "feed_wholesaler",
        "imp", "devourer", "demon", "demon_bat", "abyss_summon", "tribute",
        "money_turtle", "tiny_turtle", "double_turtle", "lucky_turtle", "magnet_turtle", "rally_turtle", "leader_turtle",
        "refresh_pirate", "plunder_pirate", "crew_pirate", "pirate_captain", "training_pirate", "treasure_pirate",
        "robbery_pirate", "pirate_king", "lightfang", "duet", "trigger", "crown", "relief", "airstrike", "pact", "stitch"
    };

    private static DiceKingDemo demo;
    private static Type demoType;
    private static MethodInfo startDefaultBuildMethod;
    private static MethodInfo beginShakeRollMethod;
    private static MethodInfo enterMarketMethod;
    private static FieldInfo rollPhaseField;
    private static FieldInfo modeField;
    private static FieldInfo diceField;
    private static FieldInfo marketOffersField;
    private static FieldInfo chapterGoldField;
    private static FieldInfo selectedMarketDieIndexField;
    private static FieldInfo affixFeatureEnabledField;
    private static FieldInfo suppressSaveField;
    private static FieldInfo menuLampFlickerUserEnabledField;
    private static FieldInfo typeCoreIdlePulsesField;
    private static bool registered;
    private static float stepStartedAt;
    private static string outputDirectory;
    private static int expectedWidth;
    private static int expectedHeight;
    private static double resolutionWaitStartedAt = -1d;
    private static double lastResolutionRequestAt = -1d;
    private static int stableResolutionFrames;
    private static string pendingCapturePath;
    private static double pendingCaptureStartedAt = -1d;
    private static int pendingCaptureWidth;
    private static int pendingCaptureHeight;

    [InitializeOnLoadMethod]
    private static void ResumeAfterDomainReload()
    {
        EditorApplication.update -= MonitorTrigger;
        EditorApplication.update += MonitorTrigger;
        string trigger = TriggerPath();
        if (File.Exists(trigger))
        {
            File.Delete(trigger);
            SessionState.SetBool(PendingKey, true);
        }

        if (SessionState.GetBool(PendingKey, false))
        {
            EditorApplication.update -= LaunchWhenReady;
            EditorApplication.update += LaunchWhenReady;
        }

        if (SessionState.GetBool(ActiveKey, false))
        {
            EditorApplication.delayCall += RegisterUpdate;
        }
    }

    private static string TriggerPath()
    {
        return Path.GetFullPath(Path.Combine(Path.Combine(Application.dataPath, "../Temp"), TriggerFileName));
    }

    private static void MonitorTrigger()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || SessionState.GetBool(ActiveKey, false) || SessionState.GetBool(PendingKey, false))
        {
            return;
        }

        string trigger = TriggerPath();
        if (!File.Exists(trigger))
        {
            return;
        }

        File.Delete(trigger);
        SessionState.SetBool(PendingKey, true);
        EditorApplication.update -= LaunchWhenReady;
        EditorApplication.update += LaunchWhenReady;
    }

    private static void LaunchWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        EditorApplication.update -= LaunchWhenReady;
        SessionState.SetBool(PendingKey, false);
        Capture();
    }

    [MenuItem("Wabish/QA/Capture Dice Type Cores V1")]
    public static void Capture()
    {
        outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/QA"));
        Directory.CreateDirectory(outputDirectory);
        DeletePreviousOutputs();
        demo = null;
        demoType = null;
        registered = false;
        ClearPendingCapture();
        Time.timeScale = 1f;
        SessionState.SetBool(PendingKey, false);
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
        string status = Path.Combine(outputDirectory, StatusFileName);
        if (File.Exists(status)) File.Delete(status);
        for (int i = 0; i < ExpectedFiles.Length; i++)
        {
            string file = Path.Combine(outputDirectory, ExpectedFiles[i]);
            if (File.Exists(file)) File.Delete(file);
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
            if (!EditorApplication.isPlaying) return;
            if (demo == null)
            {
                demo = UnityEngine.Object.FindObjectOfType<DiceKingDemo>();
                if (demo == null)
                {
                    if (StepElapsed() > 12f) Fail("DiceKingDemo did not bootstrap in Play Mode.");
                    return;
                }
                CacheReflectionHooks();
            }

            switch (SessionState.GetInt(StepKey, 0))
            {
                case 0:
                    if (StepElapsed() < 1f) return;
                    ValidateResources();
                    suppressSaveField.SetValue(demo, true);
                    menuLampFlickerUserEnabledField.SetValue(demo, true);
                    startDefaultBuildMethod.Invoke(demo, null);
                    ConfigureDice(new[] { "Lightfang", "Crown", "PigFarmer", "MeatPig", "TinyTurtle", "DoubleTurtle" });
                    SetResolution(1920, 1080);
                    Advance(1);
                    break;
                case 1:
                    if (StepElapsed() < 0.8f || !TryCaptureFrame(ExpectedFiles[0])) return;
                    SetResolution(1280, 720);
                    Advance(2);
                    break;
                case 2:
                    if (StepElapsed() < 0.8f || !TryCaptureFrame(ExpectedFiles[1])) return;
                    Advance(20);
                    break;
                case 20:
                    IList idlePulses = typeCoreIdlePulsesField.GetValue(demo) as IList;
                    if (idlePulses != null && idlePulses.Count > 0)
                    {
                        if (idlePulses.Count > 2) Fail("Ready idle type-core concurrency exceeded two: " + idlePulses.Count);
                        beginShakeRollMethod.Invoke(demo, null);
                        Advance(3);
                    }
                    else if (StepElapsed() > 6.5f)
                    {
                        Fail("Ready idle type-core pulse did not start within 6.5 seconds.");
                    }
                    break;
                case 3:
                    if (CurrentRollPhase() == "ResultDecision")
                    {
                        ConfigureResultValues(new[] { 8, 88, 888, 8888, 123, 1234 });
                        SetResolution(1920, 1080);
                        Advance(4);
                    }
                    else if (StepElapsed() > 12f)
                    {
                        Fail("Roll did not reach ResultDecision. Current phase: " + CurrentRollPhase());
                    }
                    break;
                case 4:
                    if (StepElapsed() < 0.8f || !TryCaptureFrame(ExpectedFiles[2])) return;
                    SetResolution(1280, 720);
                    Advance(5);
                    break;
                case 5:
                    if (StepElapsed() < 0.8f || !TryCaptureFrame(ExpectedFiles[3])) return;
                    ConfigureMarket();
                    SetResolution(1920, 1080);
                    Advance(6);
                    break;
                case 6:
                    if (StepElapsed() < 0.8f || !TryCaptureFrame(ExpectedFiles[4])) return;
                    SetResolution(1280, 720);
                    Advance(7);
                    break;
                case 7:
                    if (StepElapsed() < 0.8f || !TryCaptureFrame(ExpectedFiles[5])) return;
                    Advance(8);
                    break;
                case 8:
                    if (StepElapsed() < 1.4f) return;
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
        beginShakeRollMethod = demoType.GetMethod("BeginShakeRoll", flags);
        enterMarketMethod = demoType.GetMethod("EnterMarket", flags);
        rollPhaseField = demoType.GetField("rollPhase", flags);
        modeField = demoType.GetField("mode", flags);
        diceField = demoType.GetField("dice", flags);
        marketOffersField = demoType.GetField("marketOffers", flags);
        chapterGoldField = demoType.GetField("chapterGold", flags);
        selectedMarketDieIndexField = demoType.GetField("selectedMarketDieIndex", flags);
        affixFeatureEnabledField = demoType.GetField("affixFeatureEnabled", flags);
        suppressSaveField = demoType.GetField("suppressSave", flags);
        menuLampFlickerUserEnabledField = demoType.GetField("menuLampFlickerUserEnabled", flags);
        typeCoreIdlePulsesField = demoType.GetField("typeCoreIdlePulses", flags);
        if (startDefaultBuildMethod == null || beginShakeRollMethod == null || enterMarketMethod == null || rollPhaseField == null ||
            modeField == null || diceField == null || marketOffersField == null || chapterGoldField == null ||
            selectedMarketDieIndexField == null || affixFeatureEnabledField == null || suppressSaveField == null ||
            menuLampFlickerUserEnabledField == null || typeCoreIdlePulsesField == null)
        {
            throw new InvalidOperationException("Type-core capture reflection hooks are missing.");
        }
    }

    private static void ValidateResources()
    {
        for (int i = 0; i < ResourceSlugs.Length; i++)
        {
            string basePath = "Art/DiceTypes/arcade_type_core_" + ResourceSlugs[i];
            Texture2D icon = Resources.Load<Texture2D>(basePath);
            Texture2D activity = Resources.Load<Texture2D>(basePath + "_activity");
            if (icon == null || activity == null)
            {
                throw new InvalidOperationException("Missing type-core Resources pair: " + basePath);
            }
            if (icon.width != 512 || icon.height != 512 || activity.width != 512 || activity.height != 512)
            {
                throw new InvalidOperationException("Unexpected type-core dimensions: " + basePath);
            }
        }
    }

    private static void ConfigureDice(string[] typeNames)
    {
        IList dice = diceField.GetValue(demo) as IList;
        if (dice == null || dice.Count < typeNames.Length) throw new InvalidOperationException("Default build does not contain six dice.");
        for (int i = 0; i < typeNames.Length; i++) ConfigureDie(dice[i], typeNames[i]);
    }

    private static void ConfigureResultValues(int[] values)
    {
        IList dice = diceField.GetValue(demo) as IList;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        for (int i = 0; dice != null && i < values.Length && i < dice.Count; i++)
        {
            FieldInfo valueField = dice[i].GetType().GetField("EffectiveValue", flags);
            if (valueField == null) throw new InvalidOperationException("Die.EffectiveValue reflection hook is unavailable.");
            valueField.SetValue(dice[i], values[i]);
        }
    }

    private static void ConfigureMarket()
    {
        Time.timeScale = 1f;
        affixFeatureEnabledField.SetValue(demo, false);
        chapterGoldField.SetValue(demo, 50);
        enterMarketMethod.Invoke(demo, new object[] { false });
        modeField.SetValue(demo, Enum.Parse(modeField.FieldType, "InterStageMarket"));
        selectedMarketDieIndexField.SetValue(demo, 0);

        IList dice = diceField.GetValue(demo) as IList;
        string[] bagTypes = { "Imp", "Demon", "RefreshPirate", "PlunderPirate", "RobberyPirate", "PirateKing" };
        for (int i = 0; dice != null && i < bagTypes.Length && i < dice.Count; i++) ConfigureDie(dice[i], bagTypes[i]);

        IList offers = marketOffersField.GetValue(demo) as IList;
        string[] offerTypes = { "DemonBat", "CrewPirate", "PirateCaptain" };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        for (int i = 0; offers != null && i < offerTypes.Length && i < offers.Count; i++)
        {
            FieldInfo dieField = offers[i].GetType().GetField("Die", flags);
            object offerDie = dieField != null ? dieField.GetValue(offers[i]) : null;
            if (offerDie == null) throw new InvalidOperationException("Market offer has no die for type-core capture.");
            ConfigureDie(offerDie, offerTypes[i]);
        }
    }

    private static void ConfigureDie(object die, string typeName)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo typeField = die.GetType().GetField("Type", flags);
        FieldInfo nameField = die.GetType().GetField("Name", flags);
        if (typeField == null || nameField == null) throw new InvalidOperationException("Die type/name reflection hooks are unavailable.");
        typeField.SetValue(die, Enum.Parse(typeField.FieldType, typeName));
        nameField.SetValue(die, typeName);
    }

    private static string CurrentRollPhase()
    {
        object value = rollPhaseField.GetValue(demo);
        return value != null ? value.ToString() : string.Empty;
    }

    private static void SetResolution(int width, int height)
    {
        expectedWidth = width;
        expectedHeight = height;
        resolutionWaitStartedAt = -1d;
        stableResolutionFrames = 0;
        ApplyResolution(width, height);
    }

    private static void ApplyResolution(int width, int height)
    {
        Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
        int index = EnsureGameViewSize(width, height, "Wabish Type Core " + width + "x" + height);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        PropertyInfo selectedSizeIndex = gameViewType.GetProperty("selectedSizeIndex", flags);
        if (selectedSizeIndex == null || index < 0) throw new InvalidOperationException("GameView resolution selector is unavailable.");
        selectedSizeIndex.SetValue(gameView, index, null);
        gameView.Show();
        gameView.Focus();
        gameView.Repaint();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        lastResolutionRequestAt = EditorApplication.timeSinceStartup;
    }

    private static int EnsureGameViewSize(int width, int height, string label)
    {
        Assembly editorAssembly = typeof(Editor).Assembly;
        Type sizesType = editorAssembly.GetType("UnityEditor.GameViewSizes");
        Type singletonType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        object sizes = singletonType.GetProperty("instance").GetValue(null, null);
        object group = sizesType.GetMethod("GetGroup").Invoke(sizes, new object[] { (int)GameViewSizeGroupType.Standalone });
        Type groupType = group.GetType();
        int total = (int)groupType.GetMethod("GetTotalCount").Invoke(group, null);
        MethodInfo getSize = groupType.GetMethod("GetGameViewSize");
        for (int i = 0; i < total; i++)
        {
            object size = getSize.Invoke(group, new object[] { i });
            Type sizeType = size.GetType();
            int sizeWidth = (int)sizeType.GetProperty("width").GetValue(size, null);
            int sizeHeight = (int)sizeType.GetProperty("height").GetValue(size, null);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            PropertyInfo kindProperty = sizeType.GetProperty("sizeType", flags);
            FieldInfo kindField = sizeType.GetField("m_SizeType", flags);
            object kind = kindProperty != null ? kindProperty.GetValue(size, null) : kindField != null ? kindField.GetValue(size) : null;
            if (sizeWidth == width && sizeHeight == height && kind != null && kind.ToString() == "FixedResolution") return i;
        }

        Type gameViewSizeType = editorAssembly.GetType("UnityEditor.GameViewSize");
        Type kindType = editorAssembly.GetType("UnityEditor.GameViewSizeType");
        ConstructorInfo constructor = gameViewSizeType.GetConstructor(new[] { kindType, typeof(int), typeof(int), typeof(string) });
        object fixedKind = Enum.Parse(kindType, "FixedResolution");
        object newSize = constructor.Invoke(new object[] { fixedKind, width, height, label });
        groupType.GetMethod("AddCustomSize").Invoke(group, new[] { newSize });
        return total;
    }

    private static bool TryCaptureFrame(string fileName)
    {
        double now = EditorApplication.timeSinceStartup;
        string capturePath = Path.Combine(outputDirectory, fileName);
        if (!string.IsNullOrEmpty(pendingCapturePath))
        {
            if (!string.Equals(pendingCapturePath, capturePath, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Capture state changed before the previous PNG completed: " + pendingCapturePath);
            }
            if (now - pendingCaptureStartedAt < CaptureWriteHold) return false;
            if (TryReadCompletePngSize(pendingCapturePath, out int writtenWidth, out int writtenHeight))
            {
                if (writtenWidth != pendingCaptureWidth || writtenHeight != pendingCaptureHeight)
                {
                    throw new InvalidOperationException("Capture resolution mismatch for " + fileName + ". Expected " +
                        pendingCaptureWidth + "x" + pendingCaptureHeight + ", written " + writtenWidth + "x" + writtenHeight + ".");
                }
                ClearPendingCapture();
                resolutionWaitStartedAt = -1d;
                return true;
            }
            if (now - pendingCaptureStartedAt >= ResolutionTimeout)
            {
                throw new InvalidOperationException("Capture did not finish writing: " + pendingCapturePath);
            }
            return false;
        }

        if (Screen.width != expectedWidth || Screen.height != expectedHeight)
        {
            stableResolutionFrames = 0;
            if (resolutionWaitStartedAt < 0d) resolutionWaitStartedAt = now;
            if (now - lastResolutionRequestAt >= ResolutionRetryInterval) ApplyResolution(expectedWidth, expectedHeight);
            if (now - resolutionWaitStartedAt >= ResolutionTimeout)
            {
                throw new InvalidOperationException("GameView did not stabilize. Expected " + expectedWidth + "x" + expectedHeight +
                    ", current " + Screen.width + "x" + Screen.height + ".");
            }
            return false;
        }

        stableResolutionFrames++;
        if (stableResolutionFrames < StableFramesRequired) return false;
        stableResolutionFrames = 0;
        if (File.Exists(capturePath)) File.Delete(capturePath);
        ScreenCapture.CaptureScreenshot(capturePath, 1);
        pendingCapturePath = capturePath;
        pendingCaptureStartedAt = now;
        pendingCaptureWidth = expectedWidth;
        pendingCaptureHeight = expectedHeight;
        return false;
    }

    private static bool TryReadCompletePngSize(string path, out int width, out int height)
    {
        width = 0;
        height = 0;
        if (!File.Exists(path)) return false;
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            if (bytes.Length < 36 || bytes[0] != 0x89 || bytes[1] != 0x50 || bytes[2] != 0x4e || bytes[3] != 0x47)
            {
                return false;
            }
            int end = bytes.Length - 12;
            if (bytes[end] != 0 || bytes[end + 1] != 0 || bytes[end + 2] != 0 || bytes[end + 3] != 0 ||
                bytes[end + 4] != 0x49 || bytes[end + 5] != 0x45 || bytes[end + 6] != 0x4e || bytes[end + 7] != 0x44)
            {
                return false;
            }
            width = ReadBigEndianInt32(bytes, 16);
            height = ReadBigEndianInt32(bytes, 20);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static int ReadBigEndianInt32(byte[] bytes, int offset)
    {
        return (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];
    }

    private static void ClearPendingCapture()
    {
        pendingCapturePath = null;
        pendingCaptureStartedAt = -1d;
        pendingCaptureWidth = 0;
        pendingCaptureHeight = 0;
    }

    private static void ValidateAndFinish()
    {
        for (int i = 0; i < ExpectedFiles.Length; i++)
        {
            string path = Path.Combine(outputDirectory, ExpectedFiles[i]);
            if (!File.Exists(path) || new FileInfo(path).Length <= 0) throw new InvalidOperationException("Capture missing: " + path);
        }
        File.WriteAllText(Path.Combine(outputDirectory, StatusFileName),
            "PASS\nresources=37 static + 37 activity\nready=1920x1080,1280x720\nresult=1920x1080,1280x720\n" +
            "market=1920x1080,1280x720\nready_idle=started within 6.5s, concurrency <= 2\n");
        Cleanup();
    }

    private static void Advance(int step)
    {
        SessionState.SetInt(StepKey, step);
        stepStartedAt = Time.realtimeSinceStartup;
    }

    private static float StepElapsed()
    {
        return Time.realtimeSinceStartup - stepStartedAt;
    }

    private static void Fail(string message)
    {
        outputDirectory = string.IsNullOrEmpty(outputDirectory)
            ? Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/QA"))
            : outputDirectory;
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllText(Path.Combine(outputDirectory, StatusFileName), "FAIL\n" + message);
        Debug.LogError("WabishTypeCoreRuntimeCapture: " + message);
        Cleanup();
    }

    private static void Cleanup()
    {
        EditorApplication.update -= Update;
        registered = false;
        SessionState.SetBool(ActiveKey, false);
        SessionState.SetBool(PendingKey, false);
        SessionState.SetInt(StepKey, 0);
        ClearPendingCapture();
        Time.timeScale = 1f;
        if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
    }
}
