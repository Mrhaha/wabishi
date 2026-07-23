using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WabishF009FaceReelCapture
{
    private const string ActiveKey = "Wabish.F009FaceReelCapture.Active";
    private const string StepKey = "Wabish.F009FaceReelCapture.Step";
    private const string StatusFileName = "20260721_f009_face_reel_runtime_status.txt";
    private static readonly string[] ExpectedFiles =
    {
        "20260721_f009_face_reel_fast_1920x1080.png",
        "20260721_f009_face_reel_stop_wave_1920x1080.png",
        "20260721_f009_face_reel_auto_scoring_1920x1080.png",
        "20260721_f009_face_reel_fast_1280x720.png",
        "20260721_f009_face_reel_stop_wave_1280x720.png",
        "20260721_f009_face_reel_auto_scoring_1280x720.png"
    };

    private static DiceKingDemo demo;
    private static FieldInfo rollPhaseField;
    private static FieldInfo diceField;
    private static FieldInfo stopTimerField;
    private static MethodInfo startDefaultBuildMethod;
    private static MethodInfo beginShakeRollMethod;
    private static float stepStartedAt;
    private static string outputDirectory;
    private static int expectedWidth;
    private static int expectedHeight;
    private static int stableResolutionFrames;

    [InitializeOnLoadMethod]
    private static void ResumeAfterDomainReload()
    {
        if (SessionState.GetBool(ActiveKey, false))
        {
            EditorApplication.delayCall += RegisterUpdate;
        }
    }

    [MenuItem("Tools/Wabish/Capture F009 Face Reel")]
    public static void Capture()
    {
        outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/QA"));
        Directory.CreateDirectory(outputDirectory);
        for (int i = 0; i < ExpectedFiles.Length; i++)
        {
            string path = Path.Combine(outputDirectory, ExpectedFiles[i]);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        string statusPath = Path.Combine(outputDirectory, StatusFileName);
        if (File.Exists(statusPath))
        {
            File.Delete(statusPath);
        }

        SessionState.SetBool(ActiveKey, true);
        SessionState.SetInt(StepKey, 0);
        stepStartedAt = Time.realtimeSinceStartup;
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
        if (!SessionState.GetBool(ActiveKey, false))
        {
            return;
        }

        outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/QA"));
        stepStartedAt = Time.realtimeSinceStartup;
        EditorApplication.update -= Update;
        EditorApplication.update += Update;
    }

    private static void Update()
    {
        try
        {
            if (!EditorApplication.isPlaying)
            {
                if (StepElapsed() > 20f)
                {
                    Fail("Play Mode did not start.");
                }
                return;
            }

            if (!ResolveHooks())
            {
                if (StepElapsed() > 12f)
                {
                    Fail("DiceKingDemo or F009 reflection hooks are unavailable.");
                }
                return;
            }

            switch (SessionState.GetInt(StepKey, 0))
            {
                case 0:
                    if (StepElapsed() < 0.8f) return;
                    PrepareShowcase(1920, 1080);
                    Advance(1);
                    break;
                case 1:
                    if (!ResolutionStable()) return;
                    beginShakeRollMethod.Invoke(demo, null);
                    Advance(2);
                    break;
                case 2:
                    if (!WaitForPhase("Shaking", 0.28f, 3f)) return;
                    CaptureFrame(ExpectedFiles[0]);
                    Advance(3);
                    break;
                case 3:
                    if (!WaitForStopTimer(0.31f, 4f)) return;
                    CaptureFrame(ExpectedFiles[1]);
                    Advance(4);
                    break;
                case 4:
                    if (!WaitForPhase("Scoring", 0.08f, 4f)) return;
                    CaptureFrame(ExpectedFiles[2]);
                    Advance(5);
                    break;
                case 5:
                    if (StepElapsed() < 0.5f) return;
                    PrepareShowcase(1280, 720);
                    Advance(6);
                    break;
                case 6:
                    if (!ResolutionStable()) return;
                    beginShakeRollMethod.Invoke(demo, null);
                    Advance(7);
                    break;
                case 7:
                    if (!WaitForPhase("Shaking", 0.28f, 3f)) return;
                    CaptureFrame(ExpectedFiles[3]);
                    Advance(8);
                    break;
                case 8:
                    if (!WaitForStopTimer(0.31f, 4f)) return;
                    CaptureFrame(ExpectedFiles[4]);
                    Advance(9);
                    break;
                case 9:
                    if (!WaitForPhase("Scoring", 0.08f, 4f)) return;
                    CaptureFrame(ExpectedFiles[5]);
                    Advance(10);
                    break;
                case 10:
                    if (StepElapsed() < 0.8f) return;
                    ValidateAndExit();
                    break;
            }
        }
        catch (Exception exception)
        {
            Fail(exception.ToString());
        }
    }

    private static bool ResolveHooks()
    {
        if (demo != null)
        {
            return true;
        }

        demo = UnityEngine.Object.FindObjectOfType<DiceKingDemo>();
        if (demo == null)
        {
            return false;
        }

        Type type = typeof(DiceKingDemo);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        rollPhaseField = type.GetField("rollPhase", flags);
        diceField = type.GetField("dice", flags);
        stopTimerField = type.GetField("stopTimer", flags);
        startDefaultBuildMethod = type.GetMethod("StartDefaultBuild", flags);
        beginShakeRollMethod = type.GetMethod("BeginShakeRoll", flags);
        return rollPhaseField != null && diceField != null && stopTimerField != null
            && startDefaultBuildMethod != null && beginShakeRollMethod != null;
    }

    private static void PrepareShowcase(int width, int height)
    {
        startDefaultBuildMethod.Invoke(demo, null);
        ConfigureFamilyShowcase();
        SetResolution(width, height);
    }

    private static void ConfigureFamilyShowcase()
    {
        IList dice = diceField.GetValue(demo) as IList;
        if (dice == null || dice.Count < 6)
        {
            throw new InvalidOperationException("Default build must expose six physical dice for F009 capture.");
        }

        string[] showcaseTypes = { "Basic", "PigFarmer", "Imp", "Turtle", "RefreshPirate", "BlackSailBat" };
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        for (int i = 0; i < showcaseTypes.Length; i++)
        {
            object die = dice[i];
            FieldInfo typeField = die.GetType().GetField("Type", flags);
            typeField.SetValue(die, Enum.Parse(typeField.FieldType, showcaseTypes[i]));
        }
    }

    private static bool WaitForPhase(string phase, float minimumElapsed, float timeout)
    {
        if (StepElapsed() > timeout)
        {
            throw new InvalidOperationException("Timed out waiting for " + phase + "; current phase is " + CurrentPhase() + ".");
        }

        return StepElapsed() >= minimumElapsed && string.Equals(CurrentPhase(), phase, StringComparison.Ordinal);
    }

    private static bool WaitForStopTimer(float minimumTimer, float timeout)
    {
        if (StepElapsed() > timeout)
        {
            throw new InvalidOperationException("Timed out waiting for stop wave; current phase is " + CurrentPhase() + ".");
        }

        return string.Equals(CurrentPhase(), "Stopping", StringComparison.Ordinal)
            && (float)stopTimerField.GetValue(demo) >= minimumTimer;
    }

    private static string CurrentPhase()
    {
        object value = rollPhaseField.GetValue(demo);
        return value != null ? value.ToString() : string.Empty;
    }

    private static void SetResolution(int width, int height)
    {
        expectedWidth = width;
        expectedHeight = height;
        stableResolutionFrames = 0;
        Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
        int index = EnsureCustomGameViewSize(width, height, "F009 " + width + "x" + height);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        PropertyInfo selectedSizeIndex = gameViewType.GetProperty("selectedSizeIndex", flags);
        selectedSizeIndex.SetValue(gameView, index, null);
        gameView.Show();
        gameView.Repaint();
        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
    }

    private static bool ResolutionStable()
    {
        if (Screen.width != expectedWidth || Screen.height != expectedHeight)
        {
            stableResolutionFrames = 0;
            if (StepElapsed() > 10f)
            {
                throw new InvalidOperationException(
                    "GameView resolution did not stabilize. Expected " + expectedWidth + "x" + expectedHeight
                    + ", actual " + Screen.width + "x" + Screen.height + ".");
            }
            return false;
        }

        stableResolutionFrames++;
        return stableResolutionFrames >= 2;
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
            if (sizeWidth == width && sizeHeight == height)
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

    private static void CaptureFrame(string fileName)
    {
        if (Screen.width != expectedWidth || Screen.height != expectedHeight)
        {
            throw new InvalidOperationException("Capture resolution changed unexpectedly before " + fileName + ".");
        }

        ScreenCapture.CaptureScreenshot(Path.Combine(outputDirectory, fileName), 1);
        Debug.Log("WabishF009FaceReelCapture: captured " + fileName + " during " + CurrentPhase() + ".");
    }

    private static void ValidateAndExit()
    {
        List<string> invalid = new List<string>();
        for (int i = 0; i < ExpectedFiles.Length; i++)
        {
            string path = Path.Combine(outputDirectory, ExpectedFiles[i]);
            if (!File.Exists(path) || new FileInfo(path).Length <= 0)
            {
                invalid.Add(ExpectedFiles[i]);
            }
        }

        if (invalid.Count > 0)
        {
            Fail("Missing captures: " + string.Join(", ", invalid.ToArray()));
            return;
        }

        Finish(0, "PASS: fixed square shells, face-only reel motion, left-to-right stop wave and automatic scoring captured at both target resolutions.");
    }

    private static float StepElapsed()
    {
        return Time.realtimeSinceStartup - stepStartedAt;
    }

    private static void Advance(int nextStep)
    {
        SessionState.SetInt(StepKey, nextStep);
        stepStartedAt = Time.realtimeSinceStartup;
    }

    private static void Fail(string message)
    {
        Debug.LogError("WabishF009FaceReelCapture: " + message);
        Finish(1, "FAIL: " + message);
    }

    private static void Finish(int exitCode, string status)
    {
        SessionState.SetBool(ActiveKey, false);
        EditorApplication.update -= Update;
        Directory.CreateDirectory(outputDirectory);
        File.WriteAllText(Path.Combine(outputDirectory, StatusFileName), DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + Environment.NewLine + status + Environment.NewLine);
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
