using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WabishF009SettlementSourceCapture
{
    private const string ActiveKey = "Wabish.F009SettlementSourceCapture.Active";
    private const string StepKey = "Wabish.F009SettlementSourceCapture.Step";
    private const string CaptureIndexKey = "Wabish.F009SettlementSourceCapture.CaptureIndex";
    private const string StatusFileName = "20260722_f009_settlement_source_runtime_status.txt";

    private static readonly string[] ExpectedFiles =
    {
        "20260722_f009_settlement_source_trigger_1920x1080.png",
        "20260722_f009_settlement_source_retrigger_1920x1080.png",
        "20260722_f009_settlement_source_response_1920x1080.png",
        "20260722_f009_settlement_source_trigger_1280x720.png",
        "20260722_f009_settlement_source_retrigger_1280x720.png",
        "20260722_f009_settlement_source_response_1280x720.png"
    };

    private static readonly string[] ExpectedLabels =
    {
        "养猪·触发器",
        "再触发·三只小猪",
        "响应·盟约"
    };

    private static readonly string[][] ScenarioTypes =
    {
        new[] { "PigFarmer", "Trigger", "Basic", "Basic", "Basic", "Basic" },
        new[] { "ThreeLittlePigs", "Duet", "Basic", "Basic", "Basic", "Basic" },
        new[] { "PigFarmer", "Pact", "Imp", "Basic", "Basic", "Basic" }
    };

    private static DiceKingDemo demo;
    private static FieldInfo rollPhaseField;
    private static FieldInfo rollResultsLockedField;
    private static FieldInfo diceField;
    private static FieldInfo scoreStepTimerField;
    private static FieldInfo activeSettlementEventField;
    private static FieldInfo diceProcessVisualsEnabledField;
    private static FieldInfo settlementPlaybackSpeedField;
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

    [MenuItem("Tools/Wabish/Capture F009 Settlement Source Clarity")]
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
                    Fail("DiceKingDemo or F009 settlement reflection hooks are unavailable.");
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
                    BeginScenario();
                    Advance(2);
                    break;
                case 2:
                    if (!WaitForSemanticEvent(captureIndex, 8f)) return;
                    Time.timeScale = 0f;
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    Advance(3);
                    break;
                case 3:
                    if (StepElapsed() < 0.16f) return;
                    CaptureFrame(ExpectedFiles[captureIndex]);
                    Advance(4);
                    break;
                case 4:
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
            && startDefaultBuildMethod != null
            && beginSettleMethod != null
            && currentEncounterMethod != null
            && settlementDisplayDurationMethod != null;
    }

    private static void PrepareCapture(int captureIndex)
    {
        Time.timeScale = 1f;
        startDefaultBuildMethod.Invoke(demo, null);
        ConfigureScenario(captureIndex % ScenarioTypes.Length);
        int width = captureIndex < ScenarioTypes.Length ? 1920 : 1280;
        int height = captureIndex < ScenarioTypes.Length ? 1080 : 720;
        SetResolution(width, height);
    }

    private static void ConfigureScenario(int scenarioIndex)
    {
        IList dice = diceField.GetValue(demo) as IList;
        if (dice == null || dice.Count < 6)
        {
            throw new InvalidOperationException("Default build must expose six physical dice for settlement capture.");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        string[] types = ScenarioTypes[scenarioIndex];
        int[] values = { 3, 4, 5, 2, 6, 3 };
        for (int i = 0; i < 6; i++)
        {
            object die = dice[i];
            Type dieType = die.GetType();
            FieldInfo typeField = dieType.GetField("Type", flags);
            FieldInfo nameField = dieType.GetField("Name", flags);
            FieldInfo lastValueField = dieType.GetField("LastValue", flags);
            FieldInfo effectiveValueField = dieType.GetField("EffectiveValue", flags);
            FieldInfo scoreField = dieType.GetField("Score", flags);
            FieldInfo feedCountField = dieType.GetField("FeedCount", flags);
            FieldInfo feedValueField = dieType.GetField("FeedValue", flags);
            FieldInfo roundNoteField = dieType.GetField("RoundNote", flags);
            FieldInfo triggeredField = dieType.GetField("TypeTriggeredThisSettle", flags);
            typeField.SetValue(die, Enum.Parse(typeField.FieldType, types[i]));
            nameField.SetValue(die, types[i]);
            lastValueField.SetValue(die, values[i]);
            effectiveValueField.SetValue(die, values[i]);
            scoreField.SetValue(die, 0);
            feedCountField.SetValue(die, 0);
            feedValueField.SetValue(die, 0);
            roundNoteField.SetValue(die, string.Empty);
            triggeredField.SetValue(die, false);
        }

        if (scenarioIndex == 1)
        {
            SetDieIntField(dice[2], "FeedCount", 2);
            SetDieIntField(dice[2], "FeedValue", 4);
            SetDieIntField(dice[3], "FeedCount", 2);
            SetDieIntField(dice[3], "FeedValue", 3);
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

    private static void SetDieIntField(object die, string fieldName, int value)
    {
        if (die == null)
        {
            return;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo field = die.GetType().GetField(fieldName, flags);
        if (field != null)
        {
            field.SetValue(die, value);
        }
    }

    private static void BeginScenario()
    {
        object resultDecision = Enum.Parse(rollPhaseField.FieldType, "ResultDecision");
        rollPhaseField.SetValue(demo, resultDecision);
        rollResultsLockedField.SetValue(demo, true);
        beginSettleMethod.Invoke(demo, null);
    }

    private static bool WaitForSemanticEvent(int captureIndex, float timeout)
    {
        if (StepElapsed() > timeout)
        {
            throw new InvalidOperationException(
                "Timed out waiting for semantic label " + ExpectedLabels[captureIndex % ExpectedLabels.Length]
                + "; current label is " + CurrentSettlementLabel() + ".");
        }

        object settlementEvent = activeSettlementEventField.GetValue(demo);
        if (settlementEvent == null)
        {
            return false;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type eventType = settlementEvent.GetType();
        FieldInfo labelField = eventType.GetField("Label", flags);
        FieldInfo durationField = eventType.GetField("Duration", flags);
        string label = labelField != null ? labelField.GetValue(settlementEvent) as string : string.Empty;
        if (!string.Equals(label, ExpectedLabels[captureIndex % ExpectedLabels.Length], StringComparison.Ordinal))
        {
            return false;
        }

        float baseDuration = durationField != null ? (float)durationField.GetValue(settlementEvent) : 0.55f;
        float duration = (float)settlementDisplayDurationMethod.Invoke(demo, new object[] { baseDuration });
        float timer = (float)scoreStepTimerField.GetValue(demo);
        return timer >= duration * 0.42f;
    }

    private static string CurrentSettlementLabel()
    {
        object settlementEvent = activeSettlementEventField.GetValue(demo);
        if (settlementEvent == null)
        {
            return "<none>";
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        FieldInfo labelField = settlementEvent.GetType().GetField("Label", flags);
        return labelField != null ? Convert.ToString(labelField.GetValue(settlementEvent)) : "<missing field>";
    }

    private static void SetResolution(int width, int height)
    {
        expectedWidth = width;
        expectedHeight = height;
        stableResolutionFrames = 0;
        Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
        int index = EnsureCustomGameViewSize(width, height, "F009 Settlement " + width + "x" + height);
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
        Debug.Log("WabishF009SettlementSourceCapture: captured " + fileName + " with " + CurrentSettlementLabel() + ".");
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

        Finish(
            0,
            "PASS: nonnumeric source labels, trigger attribution, retrigger source/cursor separation and cross-slot response cues captured at both target resolutions.");
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
        Debug.LogError("WabishF009SettlementSourceCapture: " + message);
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
