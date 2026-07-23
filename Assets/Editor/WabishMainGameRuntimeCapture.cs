using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WabishMainGameRuntimeCapture
{
    private const string ActiveKey = "Wabish.MainGameRuntimeCapture.Active";
    private const string StepKey = "Wabish.MainGameRuntimeCapture.Step";
    private const string LaunchPendingKey = "Wabish.MainGameRuntimeCapture.LaunchPending";
    // A domain reload consumes this file and launches the capture after compilation settles.
    private const string TriggerFileName = "WabishMainGameRuntimeCapture.request";
    private const string StatusFileName = "20260716_main_game_runtime_capture_status.txt";
    private static readonly string[] ExpectedFiles =
    {
        "20260716_main_game_runtime_ready_1920x1080.png",
        "20260716_main_game_runtime_ready_1280x720.png",
        "20260716_main_game_runtime_result_1920x1080.png",
        "20260716_main_game_runtime_result_1280x720.png",
        "20260716_main_game_runtime_scoring_1920x1080.png",
        "20260716_main_game_runtime_scoring_1280x720.png",
        "20260716_main_game_runtime_digits_1920x1080.png",
        "20260716_main_game_runtime_digits_1280x720.png",
        "20260717_led_brightness_runtime_main_menu_1920x1080.png",
        "20260717_led_brightness_runtime_main_menu_1280x720.png"
    };

    private static DiceKingDemo demo;
    private static FieldInfo rollPhaseField;
    private static FieldInfo diceField;
    private static FieldInfo modeField;
    private static FieldInfo mainMenuSelectionField;
    private static FieldInfo hasSaveField;
    private static MethodInfo startDefaultBuildMethod;
    private static MethodInfo beginShakeRollMethod;
    private static MethodInfo beginSettleMethod;
    private static float stepStartedAt;
    private static bool registered;
    private static string outputDirectory;
    private static int expectedWidth;
    private static int expectedHeight;
    private static int[] savedEffectiveValues;
    private const double ResolutionRetryIntervalSeconds = 0.5d;
    private const double ResolutionCaptureTimeoutSeconds = 10d;
    private const int RequiredStableResolutionFrames = 2;
    private static double resolutionWaitStartedAt = -1d;
    private static double lastResolutionRequestAt = -1d;
    private static int stableResolutionFrameCount;

    [InitializeOnLoadMethod]
    private static void ResumeAfterDomainReload()
    {
        EditorApplication.update -= MonitorTriggerFile;
        EditorApplication.update += MonitorTriggerFile;

        string triggerPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp", TriggerFileName));
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

        string triggerPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp", TriggerFileName));
        if (!File.Exists(triggerPath))
        {
            return;
        }

        File.Delete(triggerPath);
        SessionState.SetBool(LaunchPendingKey, true);
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
        SessionState.SetBool(LaunchPendingKey, false);
        Capture();
    }

    public static void Capture()
    {
        SessionState.SetBool(LaunchPendingKey, false);
        outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/QA"));
        Directory.CreateDirectory(outputDirectory);
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

                Type type = typeof(DiceKingDemo);
                const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
                rollPhaseField = type.GetField("rollPhase", flags);
                diceField = type.GetField("dice", flags);
                modeField = type.GetField("mode", flags);
                mainMenuSelectionField = type.GetField("mainMenuSelection", flags);
                hasSaveField = type.GetField("hasSave", flags);
                startDefaultBuildMethod = type.GetMethod("StartDefaultBuild", flags);
                beginShakeRollMethod = type.GetMethod("BeginShakeRoll", flags);
                beginSettleMethod = type.GetMethod("BeginSettle", flags);
                if (rollPhaseField == null || diceField == null || modeField == null || mainMenuSelectionField == null || hasSaveField == null ||
                    startDefaultBuildMethod == null || beginShakeRollMethod == null || beginSettleMethod == null)
                {
                    Fail("Capture reflection hooks are missing from DiceKingDemo.");
                    return;
                }
            }

            int step = SessionState.GetInt(StepKey, 0);
            switch (step)
            {
                case 0:
                    if (StepElapsed() < 1f) return;
                    startDefaultBuildMethod.Invoke(demo, null);
                    ConfigureFamilyShowcase();
                    Advance(1);
                    break;
                case 1:
                    if (StepElapsed() < 0.8f) return;
                    SetResolution(1920, 1080);
                    Advance(2);
                    break;
                case 2:
                    if (StepElapsed() < 0.8f) return;
                    if (!TryCaptureFrame(ExpectedFiles[0])) return;
                    Advance(3);
                    break;
                case 3:
                    if (StepElapsed() < 0.6f) return;
                    SetResolution(1280, 720);
                    Advance(4);
                    break;
                case 4:
                    if (StepElapsed() < 0.8f) return;
                    if (!TryCaptureFrame(ExpectedFiles[1])) return;
                    Advance(5);
                    break;
                case 5:
                    if (StepElapsed() < 0.6f) return;
                    beginShakeRollMethod.Invoke(demo, null);
                    Advance(6);
                    break;
                case 6:
                    if (string.Equals(CurrentRollPhase(), "ResultDecision", StringComparison.Ordinal))
                    {
                        ConfigureLargeValueShowcase();
                        SetResolution(1920, 1080);
                        Advance(7);
                    }
                    else if (StepElapsed() > 12f)
                    {
                        Fail("Roll did not reach ResultDecision. Current phase: " + CurrentRollPhase());
                    }
                    break;
                case 7:
                    if (StepElapsed() < 0.8f) return;
                    if (!TryCaptureFrame(ExpectedFiles[6])) return;
                    Advance(8);
                    break;
                case 8:
                    if (StepElapsed() < 0.6f) return;
                    SetResolution(1280, 720);
                    Advance(9);
                    break;
                case 9:
                    if (StepElapsed() < 0.8f) return;
                    if (!TryCaptureFrame(ExpectedFiles[7])) return;
                    Advance(10);
                    break;
                case 10:
                    if (StepElapsed() < 0.6f) return;
                    RestoreLargeValueShowcase();
                    SetResolution(1920, 1080);
                    Advance(11);
                    break;
                case 11:
                    if (StepElapsed() < 0.8f) return;
                    if (!TryCaptureFrame(ExpectedFiles[2])) return;
                    Advance(12);
                    break;
                case 12:
                    if (StepElapsed() < 0.6f) return;
                    SetResolution(1280, 720);
                    Advance(13);
                    break;
                case 13:
                    if (StepElapsed() < 0.8f) return;
                    if (!TryCaptureFrame(ExpectedFiles[3])) return;
                    Advance(14);
                    break;
                case 14:
                    if (StepElapsed() < 0.6f) return;
                    beginSettleMethod.Invoke(demo, null);
                    SetResolution(1920, 1080);
                    Advance(15);
                    break;
                case 15:
                    if (StepElapsed() < 0.35f) return;
                    if (!string.Equals(CurrentRollPhase(), "Scoring", StringComparison.Ordinal))
                    {
                        Fail("Settle did not enter Scoring. Current phase: " + CurrentRollPhase());
                        return;
                    }
                    if (!TryCaptureFrame(ExpectedFiles[4])) return;
                    Advance(16);
                    break;
                case 16:
                    if (StepElapsed() < 0.6f) return;
                    SetResolution(1280, 720);
                    Advance(17);
                    break;
                case 17:
                    if (StepElapsed() < 0.45f) return;
                    if (!TryCaptureFrame(ExpectedFiles[5])) return;
                    Advance(18);
                    break;
                case 18:
                    if (StepElapsed() < 0.6f) return;
                    hasSaveField.SetValue(demo, true);
                    modeField.SetValue(demo, Enum.Parse(modeField.FieldType, "MainMenu"));
                    mainMenuSelectionField.SetValue(demo, Enum.Parse(mainMenuSelectionField.FieldType, "Continue"));
                    SetResolution(1920, 1080);
                    Advance(19);
                    break;
                case 19:
                    if (StepElapsed() < 0.8f) return;
                    if (!TryCaptureFrame(ExpectedFiles[8])) return;
                    Advance(20);
                    break;
                case 20:
                    if (StepElapsed() < 0.6f) return;
                    SetResolution(1280, 720);
                    Advance(21);
                    break;
                case 21:
                    if (StepElapsed() < 0.8f) return;
                    if (!TryCaptureFrame(ExpectedFiles[9])) return;
                    Advance(22);
                    break;
                case 22:
                    if (StepElapsed() < 1.2f) return;
                    ValidateAndExit();
                    break;
            }
        }
        catch (Exception exception)
        {
            Fail(exception.ToString());
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
        int index = EnsureCustomGameViewSize(width, height, "Wabish Runtime " + width + "x" + height);
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
        Debug.Log("WabishMainGameRuntimeCapture: requested GameView " + width + "x" + height + ".");
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
            bool isFixedResolution = sizeKind != null && string.Equals(sizeKind.ToString(), "FixedResolution", StringComparison.Ordinal);
            if (sizeWidth == width && sizeHeight == height && isFixedResolution)
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
        bool resolutionMatches = Screen.width == expectedWidth && Screen.height == expectedHeight;
        if (!resolutionMatches)
        {
            stableResolutionFrameCount = 0;
            if (resolutionWaitStartedAt < 0d)
            {
                resolutionWaitStartedAt = now;
                Debug.LogWarning(
                    "WabishMainGameRuntimeCapture: waiting for GameView " + expectedWidth + "x" + expectedHeight +
                    "; current transient size is " + Screen.width + "x" + Screen.height + ".");
            }

            if (now - lastResolutionRequestAt >= ResolutionRetryIntervalSeconds)
            {
                ApplyResolution(expectedWidth, expectedHeight);
            }

            if (now - resolutionWaitStartedAt >= ResolutionCaptureTimeoutSeconds)
            {
                throw new InvalidOperationException(
                    "GameView did not stabilize before capture timeout. Expected " + expectedWidth + "x" + expectedHeight +
                    ", current " + Screen.width + "x" + Screen.height +
                    ", waited " + ResolutionCaptureTimeoutSeconds + " seconds.");
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
        string path = Path.Combine(outputDirectory, fileName);
        ScreenCapture.CaptureScreenshot(path, 1);
        Debug.Log("WabishMainGameRuntimeCapture: captured " + fileName + " at " + Screen.width + "x" + Screen.height + ".");
        return true;
    }

    private static string CurrentRollPhase()
    {
        object value = rollPhaseField.GetValue(demo);
        return value != null ? value.ToString() : string.Empty;
    }

    private static void ConfigureFamilyShowcase()
    {
        IList dice = diceField.GetValue(demo) as IList;
        if (dice == null || dice.Count < 5)
        {
            throw new InvalidOperationException("Default build does not contain enough dice for the family-shell showcase.");
        }

        string[] showcaseTypes = { "Basic", "PigFarmer", "Imp", "Turtle", "RefreshPirate", "BlackSailBat" };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        for (int i = 0; i < dice.Count && i < showcaseTypes.Length; i++)
        {
            object die = dice[i];
            FieldInfo typeField = die.GetType().GetField("Type", flags);
            if (typeField == null)
            {
                throw new InvalidOperationException("Die.Type reflection hook is unavailable.");
            }

            typeField.SetValue(die, Enum.Parse(typeField.FieldType, showcaseTypes[i]));
        }

        Debug.Log("WabishMainGameRuntimeCapture: configured neutral / pig / devil / turtle / pirate / dual-family shell showcase.");
    }

    private static void ConfigureLargeValueShowcase()
    {
        IList dice = diceField.GetValue(demo) as IList;
        if (dice == null || dice.Count < 2)
        {
            throw new InvalidOperationException("Result does not contain enough dice for the three/four-digit showcase.");
        }

        savedEffectiveValues = new int[2];
        int[] showcaseValues = { 123, 1234 };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        for (int i = 0; i < showcaseValues.Length; i++)
        {
            FieldInfo valueField = dice[i].GetType().GetField("EffectiveValue", flags);
            if (valueField == null)
            {
                throw new InvalidOperationException("Die.EffectiveValue reflection hook is unavailable.");
            }

            savedEffectiveValues[i] = (int)valueField.GetValue(dice[i]);
            valueField.SetValue(dice[i], showcaseValues[i]);
        }

        Debug.Log("WabishMainGameRuntimeCapture: configured 123 / 1234 display-only value showcase.");
    }

    private static void RestoreLargeValueShowcase()
    {
        if (savedEffectiveValues == null)
        {
            return;
        }

        IList dice = diceField.GetValue(demo) as IList;
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        for (int i = 0; dice != null && i < savedEffectiveValues.Length && i < dice.Count; i++)
        {
            FieldInfo valueField = dice[i].GetType().GetField("EffectiveValue", flags);
            if (valueField != null)
            {
                valueField.SetValue(dice[i], savedEffectiveValues[i]);
            }
        }

        savedEffectiveValues = null;
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

    private static void ValidateAndExit()
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

            int expectedFileWidth = ExpectedFiles[i].Contains("1920x1080") ? 1920 : 1280;
            int expectedFileHeight = ExpectedFiles[i].Contains("1920x1080") ? 1080 : 720;
            Texture2D screenshot = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            bool loaded = ImageConversion.LoadImage(screenshot, File.ReadAllBytes(path), false);
            if (!loaded || screenshot.width != expectedFileWidth || screenshot.height != expectedFileHeight)
            {
                invalid.Add(
                    ExpectedFiles[i] + " (expected " + expectedFileWidth + "x" + expectedFileHeight +
                    ", actual " + screenshot.width + "x" + screenshot.height + ")");
            }

            UnityEngine.Object.DestroyImmediate(screenshot);
        }

        if (invalid.Count > 0)
        {
            Fail("Invalid screenshots: " + string.Join(", ", invalid.ToArray()));
            return;
        }

        Debug.Log("WabishMainGameRuntimeCapture: all ten screenshots written to " + outputDirectory + ".");
        Finish(0, "PASS: ten screenshots captured at 1920x1080 and 1280x720, including 123 / 1234 digit capacity and the main menu.");
    }

    private static void Fail(string message)
    {
        Debug.LogError("WabishMainGameRuntimeCapture: " + message);
        Finish(1, "FAIL: " + message);
    }

    private static void Finish(int exitCode, string status)
    {
        SessionState.SetBool(ActiveKey, false);
        EditorApplication.update -= Update;
        registered = false;
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllText(
            Path.Combine(outputDirectory, StatusFileName),
            DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + status + Environment.NewLine);

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
