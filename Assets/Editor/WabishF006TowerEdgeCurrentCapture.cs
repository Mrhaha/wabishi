using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WabishF006TowerEdgeCurrentCapture
{
    private const string ActiveKey = "Wabish.F006TowerEdgeCurrentCapture.Active";
    private const string StepKey = "Wabish.F006TowerEdgeCurrentCapture.Step";
    private const string CaptureIndexKey = "Wabish.F006TowerEdgeCurrentCapture.CaptureIndex";
    private const string StatusFileName = "20260723_f006_tower_edge_current_status.txt";

    private static readonly string[] ExpectedFiles =
    {
        "20260723_f006_tower_edge_current_early_1920x1080.png",
        "20260723_f006_tower_edge_current_mid_1920x1080.png",
        "20260723_f006_tower_edge_current_late_1920x1080.png",
        "20260723_f006_tower_edge_current_early_1280x720.png",
        "20260723_f006_tower_edge_current_mid_1280x720.png",
        "20260723_f006_tower_edge_current_late_1280x720.png"
    };

    private static readonly float[] NormalizedAfterglowTimes =
    {
        0.16f,
        0.46f,
        0.76f,
        0.16f,
        0.46f,
        0.76f
    };

    private static DiceKingDemo demo;
    private static FieldInfo rollPhaseField;
    private static FieldInfo rollResultsLockedField;
    private static FieldInfo diceField;
    private static FieldInfo scoreStepTimerField;
    private static FieldInfo activeSettlementEventField;
    private static FieldInfo diceProcessVisualsEnabledField;
    private static FieldInfo settlementPlaybackSpeedField;
    private static FieldInfo settlementImpactPresentationActiveField;
    private static FieldInfo settlementTowerAfterglowStartedAtField;
    private static FieldInfo settlementTowerAfterglowWeightField;
    private static FieldInfo settlementTowerAfterglowSignField;
    private static FieldInfo mainGameFlowPresentationProfileField;
    private static MethodInfo startDefaultBuildMethod;
    private static MethodInfo beginSettleMethod;
    private static MethodInfo currentEncounterMethod;
    private static MethodInfo settlementDisplayDurationMethod;
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

    [MenuItem("Tools/Wabish/Capture F006 Tower Edge Current")]
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
        SessionState.SetInt(CaptureIndexKey, 0);
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
                    Fail("DiceKingDemo or tower-edge reflection hooks are unavailable.");
                }
                return;
            }

            int captureIndex = SessionState.GetInt(CaptureIndexKey, 0);
            switch (SessionState.GetInt(StepKey, 0))
            {
                case 0:
                    if (StepElapsed() < 0.8f) return;
                    PrepareCapture(captureIndex);
                    Advance(1);
                    break;
                case 1:
                    if (!ResolutionStable()) return;
                    PrepareNormalSettlementFrame(captureIndex);
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    Advance(2);
                    break;
                case 2:
                    if (StepElapsed() < 0.18f) return;
                    CaptureFrame(ExpectedFiles[captureIndex]);
                    Advance(3);
                    break;
                case 3:
                    if (StepElapsed() < 0.70f) return;
                    if (!File.Exists(Path.Combine(outputDirectory, ExpectedFiles[captureIndex])))
                    {
                        if (StepElapsed() > 4f)
                        {
                            throw new InvalidOperationException("Screenshot was not written: " + ExpectedFiles[captureIndex]);
                        }
                        return;
                    }

                    Time.timeScale = 1f;
                    captureIndex++;
                    SessionState.SetInt(CaptureIndexKey, captureIndex);
                    if (captureIndex >= ExpectedFiles.Length)
                    {
                        ValidateAndExit();
                        return;
                    }

                    PrepareCapture(captureIndex);
                    Advance(1);
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
        rollResultsLockedField = type.GetField("rollResultsLocked", flags);
        diceField = type.GetField("dice", flags);
        scoreStepTimerField = type.GetField("scoreStepTimer", flags);
        activeSettlementEventField = type.GetField("activeSettlementEvent", flags);
        diceProcessVisualsEnabledField = type.GetField("diceProcessVisualsEnabled", flags);
        settlementPlaybackSpeedField = type.GetField("settlementPlaybackSpeed", flags);
        settlementImpactPresentationActiveField = type.GetField("settlementImpactPresentationActive", flags);
        settlementTowerAfterglowStartedAtField = type.GetField("settlementTowerAfterglowStartedAt", flags);
        settlementTowerAfterglowWeightField = type.GetField("settlementTowerAfterglowWeight", flags);
        settlementTowerAfterglowSignField = type.GetField("settlementTowerAfterglowSign", flags);
        mainGameFlowPresentationProfileField = type.GetField("mainGameFlowPresentationProfile", flags);
        startDefaultBuildMethod = type.GetMethod("StartDefaultBuild", flags);
        beginSettleMethod = type.GetMethod("BeginSettle", flags);
        currentEncounterMethod = type.GetMethod("CurrentEncounter", flags);
        settlementDisplayDurationMethod = type.GetMethod("SettlementDisplayDuration", flags);
        return rollPhaseField != null
            && rollResultsLockedField != null
            && diceField != null
            && scoreStepTimerField != null
            && activeSettlementEventField != null
            && diceProcessVisualsEnabledField != null
            && settlementPlaybackSpeedField != null
            && settlementImpactPresentationActiveField != null
            && settlementTowerAfterglowStartedAtField != null
            && settlementTowerAfterglowWeightField != null
            && settlementTowerAfterglowSignField != null
            && mainGameFlowPresentationProfileField != null
            && startDefaultBuildMethod != null
            && beginSettleMethod != null
            && currentEncounterMethod != null
            && settlementDisplayDurationMethod != null;
    }

    private static void PrepareCapture(int captureIndex)
    {
        Time.timeScale = 1f;
        startDefaultBuildMethod.Invoke(demo, null);
        ConfigureNormalSettlement();
        int width = captureIndex < 3 ? 1920 : 1280;
        int height = captureIndex < 3 ? 1080 : 720;
        SetResolution(width, height);
    }

    private static void ConfigureNormalSettlement()
    {
        IList dice = diceField.GetValue(demo) as IList;
        if (dice == null || dice.Count < 6)
        {
            throw new InvalidOperationException("Default build must expose six physical dice.");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        int[] values = { 3, 4, 2, 5, 3, 4 };
        for (int i = 0; i < 6; i++)
        {
            object die = dice[i];
            Type dieType = die.GetType();
            FieldInfo typeField = dieType.GetField("Type", flags);
            FieldInfo nameField = dieType.GetField("Name", flags);
            FieldInfo lastValueField = dieType.GetField("LastValue", flags);
            FieldInfo effectiveValueField = dieType.GetField("EffectiveValue", flags);
            FieldInfo scoreField = dieType.GetField("Score", flags);
            typeField.SetValue(die, Enum.Parse(typeField.FieldType, "Basic"));
            nameField.SetValue(die, "基础骰");
            lastValueField.SetValue(die, values[i]);
            effectiveValueField.SetValue(die, values[i]);
            scoreField.SetValue(die, 0);
        }

        object encounter = currentEncounterMethod.Invoke(demo, null);
        if (encounter != null)
        {
            FieldInfo targetField = encounter.GetType().GetField("Target", flags);
            if (targetField != null)
            {
                targetField.SetValue(encounter, 999);
            }
        }

        diceProcessVisualsEnabledField.SetValue(demo, true);
        settlementPlaybackSpeedField.SetValue(demo, 1f);
        rollResultsLockedField.SetValue(demo, true);
    }

    private static void PrepareNormalSettlementFrame(int captureIndex)
    {
        object resultDecision = Enum.Parse(rollPhaseField.FieldType, "ResultDecision");
        rollPhaseField.SetValue(demo, resultDecision);
        rollResultsLockedField.SetValue(demo, true);
        beginSettleMethod.Invoke(demo, null);

        object settlementEvent = activeSettlementEventField.GetValue(demo);
        if (settlementEvent == null)
        {
            throw new InvalidOperationException("Normal settlement did not activate a display event.");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type eventType = settlementEvent.GetType();
        FieldInfo kindField = eventType.GetField("Kind", flags);
        FieldInfo durationField = eventType.GetField("Duration", flags);
        FieldInfo counterAppliedField = eventType.GetField("CounterApplied", flags);
        string kind = kindField != null ? Convert.ToString(kindField.GetValue(settlementEvent)) : string.Empty;
        if (!string.Equals(kind, "SlotScore", StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Expected SlotScore, got " + kind + ".");
        }

        float baseDuration = durationField != null ? (float)durationField.GetValue(settlementEvent) : 0.55f;
        float displayDuration = (float)settlementDisplayDurationMethod.Invoke(demo, new object[] { baseDuration });
        scoreStepTimerField.SetValue(demo, displayDuration * 0.82f);
        if (counterAppliedField != null)
        {
            counterAppliedField.SetValue(settlementEvent, true);
        }
        settlementImpactPresentationActiveField.SetValue(demo, true);
        settlementTowerAfterglowWeightField.SetValue(demo, 0.34f);
        settlementTowerAfterglowSignField.SetValue(demo, 1);

        object profile = mainGameFlowPresentationProfileField.GetValue(demo);
        if (profile == null)
        {
            throw new InvalidOperationException("Main game flow presentation profile was not loaded.");
        }

        FieldInfo afterglowDurationField = profile.GetType().GetField(
            "SettlementTowerAfterglowDuration",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        float afterglowDuration = afterglowDurationField != null
            ? (float)afterglowDurationField.GetValue(profile)
            : 0.46f;
        float elapsed = afterglowDuration * NormalizedAfterglowTimes[captureIndex];
        settlementTowerAfterglowStartedAtField.SetValue(demo, Time.time - elapsed);
        Time.timeScale = 0f;
    }

    private static void SetResolution(int width, int height)
    {
        expectedWidth = width;
        expectedHeight = height;
        stableResolutionFrames = 0;
        Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
        int index = EnsureCustomGameViewSize(width, height, "F006 Tower Edge " + width + "x" + height);
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
        ConstructorInfo constructor = gameViewSizeType.GetConstructor(
            new[] { gameViewSizeKindType, typeof(int), typeof(int), typeof(string) });
        object fixedResolutionKind = Enum.Parse(gameViewSizeKindType, "FixedResolution");
        object newSize = constructor.Invoke(new object[] { fixedResolutionKind, width, height, label });
        groupType.GetMethod("AddCustomSize").Invoke(group, new[] { newSize });
        return total;
    }

    private static void CaptureFrame(string fileName)
    {
        if (Screen.width != expectedWidth || Screen.height != expectedHeight)
        {
            throw new InvalidOperationException("Capture resolution changed before " + fileName + ".");
        }

        ScreenCapture.CaptureScreenshot(Path.Combine(outputDirectory, fileName), 1);
        Debug.Log("WabishF006TowerEdgeCurrentCapture: captured " + fileName + ".");
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

        Finish(0, "PASS: ordinary SlotScore tower-edge current captured at early, mid and late phases in both target resolutions.");
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
        Debug.LogError("WabishF006TowerEdgeCurrentCapture: " + message);
        Finish(1, "FAIL: " + message);
    }

    private static void Finish(int exitCode, string status)
    {
        Time.timeScale = 1f;
        SessionState.SetBool(ActiveKey, false);
        EditorApplication.update -= Update;
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
