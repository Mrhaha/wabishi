using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WabishF009SettlementFinaleCapture
{
    private const string ActiveKey = "Wabish.F009SettlementFinaleCapture.Active";
    private const string StepKey = "Wabish.F009SettlementFinaleCapture.Step";
    private const string CaptureIndexKey = "Wabish.F009SettlementFinaleCapture.CaptureIndex";
    private const string StatusFileName = "20260723_f006_critical_force_curve_runtime_status.txt";

    private sealed class CaptureSpec
    {
        public string Tier;
        public string Phase;
        public int DieValue;
        public float Progress;
        public float ExpectedDuration;
        public bool ReducedEffects;
        public int ExpectedWorldDirection;
        public float MinimumWorldOffset;
        public int ExpectedScaleYDirection;
    }

    private static readonly CaptureSpec[] Specs =
    {
        new CaptureSpec { Tier = "Miss", Phase = "reverse_release", DieValue = 3, Progress = 0.42f, ExpectedDuration = 1.70f },
        new CaptureSpec { Tier = "Pass", Phase = "single_latch", DieValue = 4, Progress = 0.34f, ExpectedDuration = 1.55f },
        new CaptureSpec { Tier = "Exceed", Phase = "relay_outbound", DieValue = 6, Progress = 0.27f, ExpectedDuration = 1.65f },
        new CaptureSpec { Tier = "Exceed", Phase = "double_relay", DieValue = 6, Progress = 0.56f, ExpectedDuration = 1.65f },
        new CaptureSpec { Tier = "FarExceed", Phase = "cabinet_overload", DieValue = 8, Progress = 0.54f, ExpectedDuration = 1.85f },
        new CaptureSpec { Tier = "FarExceed", Phase = "cabinet_reseat", DieValue = 8, Progress = 0.79f, ExpectedDuration = 1.85f },
        new CaptureSpec { Tier = "Critical", Phase = "crouch", DieValue = 12, Progress = 0.50f, ExpectedDuration = 2.20f, ExpectedWorldDirection = 1, MinimumWorldOffset = 3f, ExpectedScaleYDirection = -1 },
        new CaptureSpec { Tier = "Critical", Phase = "vacuum", DieValue = 12, Progress = 0.58f, ExpectedDuration = 2.20f, ExpectedWorldDirection = 1, MinimumWorldOffset = 5f, ExpectedScaleYDirection = -1 },
        new CaptureSpec { Tier = "Critical", Phase = "hard_lock", DieValue = 12, Progress = 0.635f, ExpectedDuration = 2.20f, ExpectedWorldDirection = 1, MinimumWorldOffset = 5f, ExpectedScaleYDirection = -1 },
        new CaptureSpec { Tier = "Critical", Phase = "breakthrough", DieValue = 12, Progress = 0.66f, ExpectedDuration = 2.20f, ExpectedWorldDirection = -1, MinimumWorldOffset = 1.5f },
        new CaptureSpec { Tier = "Critical", Phase = "eruption_apex", DieValue = 12, Progress = 0.70f, ExpectedDuration = 2.20f, ExpectedWorldDirection = -1, MinimumWorldOffset = 8f, ExpectedScaleYDirection = 1 },
        new CaptureSpec { Tier = "Critical", Phase = "rebound", DieValue = 12, Progress = 0.785f, ExpectedDuration = 2.20f, ExpectedWorldDirection = 1, MinimumWorldOffset = 2f, ExpectedScaleYDirection = -1 },
        new CaptureSpec { Tier = "Critical", Phase = "aftershock", DieValue = 12, Progress = 0.82f, ExpectedDuration = 2.20f },
        new CaptureSpec { Tier = "Pass", Phase = "single_latch_reduced", DieValue = 4, Progress = 0.34f, ExpectedDuration = 1.55f, ReducedEffects = true },
        new CaptureSpec { Tier = "Exceed", Phase = "double_relay_reduced", DieValue = 6, Progress = 0.56f, ExpectedDuration = 1.65f, ReducedEffects = true },
        new CaptureSpec { Tier = "FarExceed", Phase = "cabinet_overload_reduced", DieValue = 8, Progress = 0.54f, ExpectedDuration = 1.85f, ReducedEffects = true },
        new CaptureSpec { Tier = "Critical", Phase = "hard_lock_reduced", DieValue = 12, Progress = 0.635f, ExpectedDuration = 2.20f, ReducedEffects = true, ExpectedWorldDirection = 1, MinimumWorldOffset = 1.5f, ExpectedScaleYDirection = -1 },
        new CaptureSpec { Tier = "Critical", Phase = "eruption_apex_reduced", DieValue = 12, Progress = 0.70f, ExpectedDuration = 2.20f, ReducedEffects = true, ExpectedWorldDirection = -1, MinimumWorldOffset = 3f, ExpectedScaleYDirection = 1 }
    };

    private static DiceKingDemo demo;
    private static FieldInfo rollPhaseField;
    private static FieldInfo rollResultsLockedField;
    private static FieldInfo diceField;
    private static FieldInfo scoreStepTimerField;
    private static FieldInfo activeSettlementEventField;
    private static FieldInfo diceProcessVisualsEnabledField;
    private static FieldInfo settlementPlaybackSpeedField;
    private static FieldInfo menuLampFlickerUserEnabledField;
    private static MethodInfo startDefaultBuildMethod;
    private static MethodInfo beginSettleMethod;
    private static MethodInfo currentEncounterMethod;
    private static MethodInfo settlementDisplayDurationMethod;
    private static MethodInfo settlementFinaleWorldOffsetMethod;
    private static MethodInfo settlementFinaleWorldScaleMethod;
    private static float stepStartedAt;
    private static string outputDirectory;
    private static int expectedWidth;
    private static int expectedHeight;
    private static int stableResolutionFrames;
    private static bool finaleClockReset;

    [InitializeOnLoadMethod]
    private static void ResumeAfterDomainReload()
    {
        if (SessionState.GetBool(ActiveKey, false))
        {
            EditorApplication.delayCall += RegisterUpdate;
        }
    }

    [MenuItem("Tools/Wabish/Capture F006/F009 Settlement Finales")]
    public static void Capture()
    {
        outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/QA"));
        Directory.CreateDirectory(outputDirectory);
        for (int captureIndex = 0; captureIndex < Specs.Length * 2; captureIndex++)
        {
            string path = Path.Combine(outputDirectory, FileNameForCapture(captureIndex));
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
                    Fail("DiceKingDemo or settlement finale reflection hooks are unavailable.");
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
                    if (!WaitForFinaleFrame(captureIndex, 12f)) return;
                    Time.timeScale = 0f;
                    UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    Advance(3);
                    break;
                case 3:
                    if (StepElapsed() < 0.16f) return;
                    CaptureFrame(FileNameForCapture(captureIndex));
                    Advance(4);
                    break;
                case 4:
                    if (StepElapsed() < 0.70f) return;
                    string path = Path.Combine(outputDirectory, FileNameForCapture(captureIndex));
                    if (!File.Exists(path))
                    {
                        if (StepElapsed() > 4f)
                        {
                            throw new InvalidOperationException("Screenshot was not written: " + Path.GetFileName(path));
                        }
                        return;
                    }

                    Time.timeScale = 1f;
                    captureIndex++;
                    SessionState.SetInt(CaptureIndexKey, captureIndex);
                    if (captureIndex >= Specs.Length * 2)
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
        menuLampFlickerUserEnabledField = type.GetField("menuLampFlickerUserEnabled", flags);
        startDefaultBuildMethod = type.GetMethod("StartDefaultBuild", flags);
        beginSettleMethod = type.GetMethod("BeginSettle", flags);
        currentEncounterMethod = type.GetMethod("CurrentEncounter", flags);
        settlementDisplayDurationMethod = type.GetMethod("SettlementDisplayDuration", flags);
        settlementFinaleWorldOffsetMethod = type.GetMethod("SettlementFinaleWorldOffset", flags);
        settlementFinaleWorldScaleMethod = type.GetMethod("SettlementFinaleWorldScale", flags);
        return rollPhaseField != null
            && rollResultsLockedField != null
            && diceField != null
            && scoreStepTimerField != null
            && activeSettlementEventField != null
            && diceProcessVisualsEnabledField != null
            && settlementPlaybackSpeedField != null
            && menuLampFlickerUserEnabledField != null
            && startDefaultBuildMethod != null
            && beginSettleMethod != null
            && currentEncounterMethod != null
            && settlementDisplayDurationMethod != null
            && settlementFinaleWorldOffsetMethod != null
            && settlementFinaleWorldScaleMethod != null;
    }

    private static void PrepareCapture(int captureIndex)
    {
        Time.timeScale = 1f;
        finaleClockReset = false;
        startDefaultBuildMethod.Invoke(demo, null);
        ConfigureScenario(Specs[captureIndex % Specs.Length]);
        int width = captureIndex < Specs.Length ? 1920 : 1280;
        int height = captureIndex < Specs.Length ? 1080 : 720;
        SetResolution(width, height);
    }

    private static void ConfigureScenario(CaptureSpec spec)
    {
        IList dice = diceField.GetValue(demo) as IList;
        if (dice == null || dice.Count < 6)
        {
            throw new InvalidOperationException("Default build must expose six physical dice for finale capture.");
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        for (int index = 0; index < 6; index++)
        {
            object die = dice[index];
            Type dieType = die.GetType();
            FieldInfo typeField = dieType.GetField("Type", flags);
            typeField.SetValue(die, Enum.Parse(typeField.FieldType, "Basic"));
            dieType.GetField("Name", flags).SetValue(die, "基础骰");
            dieType.GetField("LastValue", flags).SetValue(die, spec.DieValue);
            dieType.GetField("EffectiveValue", flags).SetValue(die, spec.DieValue);
            dieType.GetField("Score", flags).SetValue(die, 0);
            dieType.GetField("FeedCount", flags).SetValue(die, 0);
            dieType.GetField("FeedValue", flags).SetValue(die, 0);
            dieType.GetField("RoundNote", flags).SetValue(die, string.Empty);
            dieType.GetField("TypeTriggeredThisSettle", flags).SetValue(die, false);
        }

        object encounter = currentEncounterMethod.Invoke(demo, null);
        FieldInfo targetField = encounter != null ? encounter.GetType().GetField("Target", flags) : null;
        if (targetField == null)
        {
            throw new InvalidOperationException("Current encounter target is unavailable.");
        }
        targetField.SetValue(encounter, 20);

        diceProcessVisualsEnabledField.SetValue(demo, true);
        menuLampFlickerUserEnabledField.SetValue(demo, !spec.ReducedEffects);
        settlementPlaybackSpeedField.SetValue(demo, 2f);
        rollResultsLockedField.SetValue(demo, true);
    }

    private static void BeginScenario()
    {
        rollPhaseField.SetValue(demo, Enum.Parse(rollPhaseField.FieldType, "ResultDecision"));
        rollResultsLockedField.SetValue(demo, true);
        beginSettleMethod.Invoke(demo, null);
    }

    private static bool WaitForFinaleFrame(int captureIndex, float timeout)
    {
        CaptureSpec spec = Specs[captureIndex % Specs.Length];
        if (StepElapsed() > timeout)
        {
            throw new InvalidOperationException("Timed out waiting for " + spec.Tier + " / " + spec.Phase + "; current event is " + CurrentEventSummary() + ".");
        }

        object settlementEvent = activeSettlementEventField.GetValue(demo);
        if (settlementEvent == null)
        {
            return false;
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type eventType = settlementEvent.GetType();
        string kind = Convert.ToString(eventType.GetField("Kind", flags).GetValue(settlementEvent));
        if (!string.Equals(kind, "TargetSettle", StringComparison.Ordinal))
        {
            return false;
        }

        string tier = Convert.ToString(eventType.GetField("FeedbackTier", flags).GetValue(settlementEvent));
        if (!string.Equals(tier, spec.Tier, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Expected tier " + spec.Tier + " but runtime classified " + tier + ".");
        }

        float baseDuration = (float)eventType.GetField("Duration", flags).GetValue(settlementEvent);
        if (Mathf.Abs(baseDuration - spec.ExpectedDuration) > 0.011f)
        {
            throw new InvalidOperationException("Expected finale duration " + spec.ExpectedDuration + " but runtime stored " + baseDuration + ".");
        }

        if (!finaleClockReset)
        {
            settlementPlaybackSpeedField.SetValue(demo, 1f);
            scoreStepTimerField.SetValue(demo, 0f);
            finaleClockReset = true;
            return false;
        }

        float duration = (float)settlementDisplayDurationMethod.Invoke(demo, new object[] { baseDuration });
        float timer = (float)scoreStepTimerField.GetValue(demo);
        if (timer < duration * spec.Progress)
        {
            return false;
        }

        Vector2 worldOffset = (Vector2)settlementFinaleWorldOffsetMethod.Invoke(demo, null);
        Vector2 worldScale = (Vector2)settlementFinaleWorldScaleMethod.Invoke(demo, null);
        if (!string.Equals(spec.Tier, "Critical", StringComparison.Ordinal))
        {
            if (worldOffset.sqrMagnitude > 0.0001f || (worldScale - Vector2.one).sqrMagnitude > 0.000001f)
            {
                throw new InvalidOperationException(spec.Tier + " unexpectedly received the Critical world force transform.");
            }
        }

        if (spec.ExpectedWorldDirection != 0)
        {
            float directedOffset = worldOffset.y * spec.ExpectedWorldDirection;
            if (directedOffset < spec.MinimumWorldOffset)
            {
                throw new InvalidOperationException(
                    spec.Tier + " / " + spec.Phase + " expected directed Y offset >= "
                    + spec.MinimumWorldOffset + " but received " + worldOffset.y + ".");
            }
        }

        if (spec.ExpectedScaleYDirection < 0 && worldScale.y >= 0.998f)
        {
            throw new InvalidOperationException(spec.Tier + " / " + spec.Phase + " did not compress vertically; scaleY=" + worldScale.y + ".");
        }
        if (spec.ExpectedScaleYDirection > 0 && worldScale.y <= 1.002f)
        {
            throw new InvalidOperationException(spec.Tier + " / " + spec.Phase + " did not stretch vertically; scaleY=" + worldScale.y + ".");
        }

        return true;
    }

    private static string CurrentEventSummary()
    {
        object settlementEvent = activeSettlementEventField.GetValue(demo);
        if (settlementEvent == null)
        {
            return "<none>";
        }

        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        Type type = settlementEvent.GetType();
        return Convert.ToString(type.GetField("Kind", flags).GetValue(settlementEvent))
            + "/" + Convert.ToString(type.GetField("FeedbackTier", flags).GetValue(settlementEvent));
    }

    private static string FileNameForCapture(int captureIndex)
    {
        CaptureSpec spec = Specs[captureIndex % Specs.Length];
        string resolution = captureIndex < Specs.Length ? "1920x1080" : "1280x720";
        return "20260723_f006_critical_force_curve_" + spec.Tier.ToLowerInvariant() + "_" + spec.Phase + "_" + resolution + ".png";
    }

    private static void SetResolution(int width, int height)
    {
        expectedWidth = width;
        expectedHeight = height;
        stableResolutionFrames = 0;
        Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
        int index = EnsureCustomGameViewSize(width, height, "F009 Finale " + width + "x" + height);
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
                throw new InvalidOperationException("GameView resolution did not stabilize. Expected " + expectedWidth + "x" + expectedHeight + ", actual " + Screen.width + "x" + Screen.height + ".");
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
        for (int index = 0; index < total; index++)
        {
            object size = getGameViewSize.Invoke(group, new object[] { index });
            Type sizeType = size.GetType();
            int sizeWidth = (int)sizeType.GetProperty("width").GetValue(size, null);
            int sizeHeight = (int)sizeType.GetProperty("height").GetValue(size, null);
            if (sizeWidth == width && sizeHeight == height)
            {
                return index;
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
        Debug.Log("WabishF009SettlementFinaleCapture: captured " + fileName + " at " + CurrentEventSummary() + ".");
    }

    private static void ValidateAndExit()
    {
        List<string> invalid = new List<string>();
        for (int captureIndex = 0; captureIndex < Specs.Length * 2; captureIndex++)
        {
            string fileName = FileNameForCapture(captureIndex);
            string path = Path.Combine(outputDirectory, fileName);
            if (!File.Exists(path) || new FileInfo(path).Length <= 0)
            {
                invalid.Add(fileName);
            }
        }

        if (invalid.Count > 0)
        {
            Fail("Missing captures: " + string.Join(", ", invalid.ToArray()));
            return;
        }

        Finish(0, "PASS: stages 1-4 kept zero Critical world transform; stage 5 crouch, hard lock, breakthrough, eruption apex, rebound, aftershock, and reduced-effects force directions were classified, timed, numerically checked, and captured at 1920x1080 and 1280x720.");
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
        Debug.LogError("WabishF009SettlementFinaleCapture: " + message);
        Finish(1, "FAIL: " + message);
    }

    private static void Finish(int exitCode, string status)
    {
        Time.timeScale = 1f;
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
