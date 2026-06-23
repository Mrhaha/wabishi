using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DiceKingF010ValidationRunner
{
    // Editor-only runner used by the F010 production packet validation step.
    private const string ActiveKey = "DiceKing.F010Validation.Active";
    private const string ChecksStartedKey = "DiceKing.F010Validation.ChecksStarted";
    private const string ScreenshotStepKey = "DiceKing.F010Validation.ScreenshotStep";
    private const string ScenePath = "Assets/Scenes/SampleScene.unity";
    private const string RequestPath = "Temp/F010OddEvenValidationRequest.txt";
    private const string ReportPath = "Docs/QA/F010/20260616_f010_validation_report.txt";

    private static readonly BindingFlags AnyInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly List<string> Lines = new List<string>();
    private static readonly List<string> Failures = new List<string>();

    private static DiceKingDemo demo;
    private static Type dieTypeEnum;
    private static Type rollPhaseEnum;
    private static Type gameModeEnum;
    private static bool running;
    private static int screenshotFramesToWait;
    private static string pendingScreenshotPath;
    private static string pendingScreenshotLabel;

    [InitializeOnLoadMethod]
    private static void AutoRunFromMarker()
    {
        if (File.Exists(AbsolutePath(RequestPath)))
        {
            File.Delete(AbsolutePath(RequestPath));
            BeginRun();
            return;
        }

        if (SessionState.GetBool(ActiveKey, false))
        {
            HookPlayModeEvents();
            if (EditorApplication.isPlaying)
            {
                SchedulePlayModeChecks();
            }
            else if (!SessionState.GetBool(ChecksStartedKey, false))
            {
                QueueStartWhenReady();
            }
        }
    }

    [MenuItem("Tools/Dice King/Validate F010 Odd Even Dice")]
    public static void Run()
    {
        BeginRun();
    }

    private static void BeginRun()
    {
        if (running || SessionState.GetBool(ActiveKey, false))
        {
            return;
        }

        running = true;
        SessionState.SetBool(ActiveKey, true);
        SessionState.SetBool(ChecksStartedKey, false);
        HookPlayModeEvents();
        QueueStartWhenReady();
    }

    private static void HookPlayModeEvents()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void QueueStartWhenReady()
    {
        EditorApplication.update -= StartWhenReady;
        EditorApplication.update += StartWhenReady;
    }

    private static void StartWhenReady()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating)
        {
            return;
        }

        EditorApplication.update -= StartWhenReady;
        if (EditorApplication.isPlaying)
        {
            SchedulePlayModeChecks();
            return;
        }

        Lines.Clear();
        Failures.Clear();

        EnsureSceneAndEnterPlayMode();
    }

    private static void EnsureSceneAndEnterPlayMode()
    {
        if (EditorSceneManager.GetActiveScene().path != ScenePath)
        {
            EditorSceneManager.OpenScene(ScenePath);
        }

        if (!EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = true;
            return;
        }

        EditorApplication.delayCall += RunPlayModeChecks;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (!running || state != PlayModeStateChange.EnteredPlayMode)
        {
            if (!SessionState.GetBool(ActiveKey, false) || state != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }
        }

        SchedulePlayModeChecks();
    }

    private static void SchedulePlayModeChecks()
    {
        if (SessionState.GetBool(ChecksStartedKey, false))
        {
            return;
        }

        SessionState.SetBool(ChecksStartedKey, true);
        EditorApplication.update -= RunChecksWhenPlaying;
        EditorApplication.update += RunChecksWhenPlaying;
    }

    private static void RunChecksWhenPlaying()
    {
        if (!EditorApplication.isPlaying)
        {
            return;
        }

        EditorApplication.update -= RunChecksWhenPlaying;
        RunPlayModeChecks();
    }

    private static void RunPlayModeChecks()
    {
        running = true;
        Lines.Clear();
        Failures.Clear();
        Lines.Add("# F010 奇偶短规则骰 Unity 验证报告");
        Lines.Add("");
        Lines.Add("时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        Lines.Add("范围: Play Mode、七颗骰触发样例、出千路径检查；不新增规则、不生成最终资源。");
        Lines.Add("");

        bool deferFinish = false;
        try
        {
            demo = UnityEngine.Object.FindObjectOfType<DiceKingDemo>();
            if (demo == null)
            {
                Fail("Play Mode", "未找到 DiceKingDemo。");
                Finish();
                return;
            }

            dieTypeEnum = typeof(DiceKingDemo).GetNestedType("DieType", BindingFlags.NonPublic);
            rollPhaseEnum = typeof(DiceKingDemo).GetNestedType("RollPhase", BindingFlags.NonPublic);
            gameModeEnum = typeof(DiceKingDemo).GetNestedType("GameMode", BindingFlags.NonPublic);

            InvokeDemo("StartDefaultBuild");
            RunRuleSamples();
            RunCheatPathSamples();
            BeginScreenshotSequence();
            deferFinish = true;
        }
        catch (Exception ex)
        {
            Fail("Runner", ex.ToString());
        }
        finally
        {
            if (!deferFinish)
            {
                Finish();
            }
        }
    }

    private static void RunRuleSamples()
    {
        Section("七颗骰触发样例");

        Case("异邻骰: 左邻奇偶不同，本骰 6 分", delegate
        {
            IList dice = PrepareDice(
                Spec("basic", "Basic", 2),
                Spec("diff", "ParityNeighborDiff", 3),
                Spec("b1", "Basic", 2),
                Spec("b2", "Basic", 4),
                Spec("b3", "Basic", 6),
                Spec("b4", "Basic", 2));
            ScoreAndPreview();
            ExpectInt(6, GetDieScore(dice[1]), "异邻骰触发后单骰分值");
            ExpectContains(GetDieNote(dice[1]), "异邻", "异邻骰 RoundNote");
        });

        Case("异邻骰: 最左位无左邻，不触发", delegate
        {
            IList dice = PrepareDice(
                Spec("diff", "ParityNeighborDiff", 3),
                Spec("b0", "Basic", 2),
                Spec("b1", "Basic", 2),
                Spec("b2", "Basic", 4),
                Spec("b3", "Basic", 6),
                Spec("b4", "Basic", 2));
            ScoreAndPreview();
            ExpectInt(3, GetDieScore(dice[0]), "最左异邻骰保持基础分");
        });

        Case("同邻骰: 左邻奇偶相同，本骰 6 分", delegate
        {
            IList dice = PrepareDice(
                Spec("basic", "Basic", 3),
                Spec("same", "ParityNeighborSame", 5),
                Spec("b1", "Basic", 2),
                Spec("b2", "Basic", 4),
                Spec("b3", "Basic", 6),
                Spec("b4", "Basic", 2));
            ScoreAndPreview();
            ExpectInt(6, GetDieScore(dice[1]), "同邻骰触发后单骰分值");
            ExpectContains(GetDieNote(dice[1]), "同邻", "同邻骰 RoundNote");
        });

        Case("同邻骰: 左邻奇偶不同，不触发", delegate
        {
            IList dice = PrepareDice(
                Spec("basic", "Basic", 2),
                Spec("same", "ParityNeighborSame", 5),
                Spec("b1", "Basic", 2),
                Spec("b2", "Basic", 4),
                Spec("b3", "Basic", 6),
                Spec("b4", "Basic", 2));
            ScoreAndPreview();
            ExpectInt(5, GetDieScore(dice[1]), "未触发同邻骰保持基础分");
        });

        Case("补全骰: 只差本骰时，补全全奇", delegate
        {
            IList dice = PrepareDice(
                Spec("b0", "Basic", 1),
                Spec("b1", "Basic", 3),
                Spec("b2", "Basic", 5),
                Spec("b3", "Basic", 1),
                Spec("b4", "Basic", 3),
                Spec("complete", "ParityComplete", 2));
            ScoreAndPreview();
            ExpectBool(true, GetDieBool(dice[5], "ParityCompleteUsed"), "补全标记");
            ExpectContains((string)GetDemoField("lastHandName"), "全奇", "补全后牌型名");
        });

        Case("补全骰: 多个缺口时，不补全", delegate
        {
            IList dice = PrepareDice(
                Spec("b0", "Basic", 1),
                Spec("b1", "Basic", 3),
                Spec("b2", "Basic", 5),
                Spec("b3", "Basic", 2),
                Spec("b4", "Basic", 3),
                Spec("complete", "ParityComplete", 2));
            ScoreAndPreview();
            ExpectBool(false, GetDieBool(dice[5], "ParityCompleteUsed"), "多缺口补全标记");
        });

        Case("复核骰: 本骰破坏全奇时，重摇一次", delegate
        {
            IList dice = PrepareDice(
                Spec("b0", "Basic", 1),
                Spec("b1", "Basic", 3),
                Spec("b2", "Basic", 5),
                Spec("b3", "Basic", 1),
                Spec("b4", "Basic", 3),
                Spec("review", "ParityReview", 2, 1, 1, 1, 1, 1, 1));
            InvokeDemo("ApplyParityReviewRerolls");
            ScoreAndPreview();
            ExpectBool(true, GetDieBool(dice[5], "ParityReviewRerolled"), "复核重摇标记");
            ExpectInt(2, GetDieInt(dice[5], "ParityReviewPreviousValue"), "复核记录旧点数");
            ExpectInt(1, GetDieInt(dice[5], "EffectiveValue"), "复核后点数");
            ExpectContains(GetDieNote(dice[5]), "复核", "复核骰 RoundNote");
        });

        Case("复核骰: 不是唯一破坏者时，不重摇", delegate
        {
            IList dice = PrepareDice(
                Spec("b0", "Basic", 1),
                Spec("b1", "Basic", 3),
                Spec("b2", "Basic", 5),
                Spec("b3", "Basic", 2),
                Spec("b4", "Basic", 3),
                Spec("review", "ParityReview", 2, 1, 1, 1, 1, 1, 1));
            InvokeDemo("ApplyParityReviewRerolls");
            ScoreAndPreview();
            ExpectBool(false, GetDieBool(dice[5], "ParityReviewRerolled"), "非唯一破坏者复核标记");
        });
    }

    private static void RunCheatPathSamples()
    {
        Section("出千路径检查");

        Case("翻号骰: 出千后奇偶变了，8 分", delegate
        {
            IList dice = PrepareDice(
                Spec("flip", "ParityFlipScore", 2, 1, 3, 5, 1, 3, 5),
                Spec("b0", "Basic", 2),
                Spec("b1", "Basic", 2),
                Spec("b2", "Basic", 4),
                Spec("b3", "Basic", 6),
                Spec("b4", "Basic", 2));
            SelectAndConfirmCheat(dice[0]);
            ExpectBool(true, GetDieBool(dice[0], "CheatRerolledThisSettle"), "翻号出千标记");
            ExpectInt(2, GetDieInt(dice[0], "CheatPreviousValue"), "翻号旧点数");
            ExpectBool(true, GetDieInt(dice[0], "EffectiveValue") % 2 == 1, "翻号后为奇数");
            ExpectInt(8, GetDieScore(dice[0]), "翻号骰单骰分值");
            ExpectContains(GetDieNote(dice[0]), "翻号", "翻号骰 RoundNote");
        });

        Case("守号骰: 出千后奇偶没变，6 分", delegate
        {
            IList dice = PrepareDice(
                Spec("hold", "ParityHoldScore", 2, 2, 4, 6, 2, 4, 6),
                Spec("b0", "Basic", 1),
                Spec("b1", "Basic", 3),
                Spec("b2", "Basic", 5),
                Spec("b3", "Basic", 1),
                Spec("b4", "Basic", 3));
            SelectAndConfirmCheat(dice[0]);
            ExpectBool(true, GetDieBool(dice[0], "CheatRerolledThisSettle"), "守号出千标记");
            ExpectInt(2, GetDieInt(dice[0], "CheatPreviousValue"), "守号旧点数");
            ExpectBool(true, GetDieInt(dice[0], "EffectiveValue") % 2 == 0, "守号后仍为偶数");
            ExpectInt(6, GetDieScore(dice[0]), "守号骰单骰分值");
            ExpectContains(GetDieNote(dice[0]), "守号", "守号骰 RoundNote");
        });

        Case("转号骰: 出千后奇偶必变", delegate
        {
            IList dice = PrepareDice(
                Spec("turner", "ParityTurner", 2, 1, 3, 5, 1, 3, 5),
                Spec("b0", "Basic", 2),
                Spec("b1", "Basic", 2),
                Spec("b2", "Basic", 4),
                Spec("b3", "Basic", 6),
                Spec("b4", "Basic", 2));
            SelectAndConfirmCheat(dice[0]);
            ExpectBool(true, GetDieBool(dice[0], "CheatRerolledThisSettle"), "转号出千标记");
            ExpectInt(2, GetDieInt(dice[0], "CheatPreviousValue"), "转号旧点数");
            ExpectBool(true, GetDieInt(dice[0], "EffectiveValue") % 2 == 1, "转号后奇偶改变");
            ExpectContains(GetDieNote(dice[0]), "转号", "转号骰 RoundNote");
        });

        Case("转号骰: 无相反奇偶候选时，不能被选中", delegate
        {
            IList dice = PrepareDice(
                Spec("turner", "ParityTurner", 2, 2, 4, 6, 2, 4, 6),
                Spec("b0", "Basic", 2),
                Spec("b1", "Basic", 2),
                Spec("b2", "Basic", 4),
                Spec("b3", "Basic", 6),
                Spec("b4", "Basic", 2));
            BeginCheatSelection();
            InvokeDemo("ToggleCheatReroll", dice[0]);
            IList selected = (IList)GetDemoField("cheatRerollIds");
            ExpectInt(0, selected.Count, "转号无候选时选择数");
            InvokeDemo("CancelCheatEdit");
        });

        Case("出千取消: 不消耗出千次数，不改点数", delegate
        {
            IList dice = PrepareDice(
                Spec("flip", "ParityFlipScore", 2, 1, 3, 5, 1, 3, 5),
                Spec("b0", "Basic", 2),
                Spec("b1", "Basic", 2),
                Spec("b2", "Basic", 4),
                Spec("b3", "Basic", 6),
                Spec("b4", "Basic", 2));
            BeginCheatSelection();
            InvokeDemo("ToggleCheatReroll", dice[0]);
            InvokeDemo("CancelCheatEdit");
            ExpectInt(1, (int)GetDemoField("cheatsLeft"), "取消后出千次数");
            ExpectInt(2, GetDieInt(dice[0], "EffectiveValue"), "取消后点数");
            ExpectBool(false, GetDieBool(dice[0], "CheatRerolledThisSettle"), "取消后出千标记");
        });

        Case("未选中骰: 结算不获得翻号加成", delegate
        {
            IList dice = PrepareDice(
                Spec("flip", "ParityFlipScore", 2, 1, 3, 5, 1, 3, 5),
                Spec("b0", "Basic", 2),
                Spec("b1", "Basic", 2),
                Spec("b2", "Basic", 4),
                Spec("b3", "Basic", 6),
                Spec("b4", "Basic", 2));
            SelectAndConfirmCheat(dice[1]);
            ExpectBool(false, GetDieBool(dice[0], "CheatRerolledThisSettle"), "未选中翻号出千标记");
            ExpectInt(2, GetDieScore(dice[0]), "未选中翻号基础分");
        });
    }

    private static void CaptureScreenshots()
    {
        Section("截图");

        Case("结果界面 1920x1080 截图状态准备", delegate
        {
            PrepareDice(
                Spec("basic", "Basic", 2),
                Spec("diff", "ParityNeighborDiff", 3),
                Spec("same", "ParityNeighborSame", 5),
                Spec("complete", "ParityComplete", 2),
                Spec("review", "ParityReview", 2, 1, 1, 1, 1, 1, 1),
                Spec("turner", "ParityTurner", 4, 1, 3, 5, 1, 3, 5));
            ScoreAndPreview();
            SetDemoField("rollPhase", ParseRollPhase("ResultDecision"));
        });

        Case("出千界面 1920x1080 截图状态准备", delegate
        {
            IList dice = PrepareDice(
                Spec("flip", "ParityFlipScore", 2, 1, 3, 5, 1, 3, 5),
                Spec("hold", "ParityHoldScore", 2, 2, 4, 6, 2, 4, 6),
                Spec("turner", "ParityTurner", 4, 1, 3, 5, 1, 3, 5),
                Spec("b1", "Basic", 1),
                Spec("b2", "Basic", 3),
                Spec("b3", "Basic", 5));
            BeginCheatSelection();
            InvokeDemo("ToggleCheatReroll", dice[0]);
            InvokeDemo("ToggleCheatReroll", dice[1]);
            InvokeDemo("ToggleCheatReroll", dice[2]);
            InvokeDemo("CancelCheatEdit");
        });

        Case("结果界面 1280x720 截图状态准备", delegate
        {
            PrepareDice(
                Spec("basic", "Basic", 2),
                Spec("diff", "ParityNeighborDiff", 3),
                Spec("same", "ParityNeighborSame", 5),
                Spec("complete", "ParityComplete", 2),
                Spec("review", "ParityReview", 2, 1, 1, 1, 1, 1, 1),
                Spec("turner", "ParityTurner", 4, 1, 3, 5, 1, 3, 5));
            ScoreAndPreview();
            SetDemoField("rollPhase", ParseRollPhase("ResultDecision"));
        });
    }

    private static void BeginScreenshotSequence()
    {
        Section("截图");
        SessionState.SetInt(ScreenshotStepKey, 0);
        screenshotFramesToWait = 0;
        pendingScreenshotPath = null;
        pendingScreenshotLabel = null;
        EditorApplication.update -= ScreenshotTick;
        EditorApplication.update += ScreenshotTick;
    }

    private static void ScreenshotTick()
    {
        if (!EditorApplication.isPlaying)
        {
            return;
        }

        try
        {
            if (screenshotFramesToWait > 0)
            {
                screenshotFramesToWait--;
                if (screenshotFramesToWait == 0 && !string.IsNullOrEmpty(pendingScreenshotPath))
                {
                    string path = pendingScreenshotPath;
                    string label = pendingScreenshotLabel;
                    pendingScreenshotPath = null;
                    pendingScreenshotLabel = null;
                    Directory.CreateDirectory(Path.GetDirectoryName(AbsolutePath(path)));
                    ScreenCapture.CaptureScreenshot(path);
                    Pass(label);
                    SessionState.SetInt(ScreenshotStepKey, SessionState.GetInt(ScreenshotStepKey, 0) + 1);
                }

                return;
            }

            int step = SessionState.GetInt(ScreenshotStepKey, 0);
            if (step == 0)
            {
                PrepareResultScreenshotState();
                RequestScreenshot("结果界面 1920x1080 截图", "Docs/QA/F010/20260616_f010_result_1920x1080.png", 1920, 1080);
                return;
            }

            if (step == 1)
            {
                PrepareCheatScreenshotState();
                RequestScreenshot("出千界面 1920x1080 截图", "Docs/QA/F010/20260616_f010_cheat_1920x1080.png", 1920, 1080);
                return;
            }

            if (step == 2)
            {
                PrepareResultScreenshotState();
                RequestScreenshot("结果界面 1280x720 截图", "Docs/QA/F010/20260616_f010_result_1280x720.png", 1280, 720);
                return;
            }

            Finish();
        }
        catch (Exception ex)
        {
            Fail("截图", RootMessage(ex));
            Finish();
        }
    }

    private static void PrepareResultScreenshotState()
    {
        PrepareDice(
            Spec("basic", "Basic", 2),
            Spec("diff", "ParityNeighborDiff", 3),
            Spec("same", "ParityNeighborSame", 5),
            Spec("complete", "ParityComplete", 2),
            Spec("review", "ParityReview", 2, 1, 1, 1, 1, 1, 1),
            Spec("turner", "ParityTurner", 4, 1, 3, 5, 1, 3, 5));
        ScoreAndPreview();
        SetDemoField("rollPhase", ParseRollPhase("ResultDecision"));
    }

    private static void PrepareCheatScreenshotState()
    {
        IList dice = PrepareDice(
            Spec("flip", "ParityFlipScore", 2, 1, 3, 5, 1, 3, 5),
            Spec("hold", "ParityHoldScore", 2, 2, 4, 6, 2, 4, 6),
            Spec("turner", "ParityTurner", 4, 1, 3, 5, 1, 3, 5),
            Spec("b1", "Basic", 1),
            Spec("b2", "Basic", 3),
            Spec("b3", "Basic", 5));
        BeginCheatSelection();
        InvokeDemo("ToggleCheatReroll", dice[0]);
        InvokeDemo("ToggleCheatReroll", dice[1]);
        InvokeDemo("ToggleCheatReroll", dice[2]);
    }

    private static void RequestScreenshot(string label, string path, int width, int height)
    {
        SetGameViewSize(width, height);
        pendingScreenshotLabel = label;
        pendingScreenshotPath = path;
        screenshotFramesToWait = 4;
        Lines.Add("  - 截图请求: " + path + " (" + width + "x" + height + ")");
    }

    private static IList PrepareDice(params DieSpec[] specs)
    {
        ResetRunState();
        IList dice = (IList)GetDemoField("dice");
        dice.Clear();
        foreach (DieSpec spec in specs)
        {
            object die = InvokeDemo("NewDie", spec.Id, ParseDieType(spec.Type), spec.Faces);
            SetDieInt(die, "LastValue", spec.Value);
            SetDieInt(die, "EffectiveValue", spec.Value);
            ClearTransientDieState(die);
            dice.Add(die);
        }

        InvokeDemo("RefreshHandPreview");
        return dice;
    }

    private static void ResetRunState()
    {
        SetDemoField("mode", ParseGameMode("Run"));
        SetDemoField("rollPhase", ParseRollPhase("ResultDecision"));
        SetDemoField("currentScore", 0);
        SetDemoField("chapterGold", 999);
        SetDemoField("rollsLeft", 2);
        SetDemoField("cheatsLeft", 1);
        SetDemoField("passed", false);
        SetDemoField("stageIndex", 0);
        SetDemoField("previewRollScore", 0);
        SetDemoField("lastHandName", "");
        SetDemoField("lastMultiplier", 1);

        IList selected = (IList)GetDemoField("cheatRerollIds");
        if (selected != null)
        {
            selected.Clear();
        }

        IList scoring = (IList)GetDemoField("scoringDice");
        if (scoring != null)
        {
            scoring.Clear();
        }
    }

    private static void ClearTransientDieState(object die)
    {
        SetDieInt(die, "Score", 0);
        SetDieString(die, "RoundNote", "");
        SetDieBool(die, "CheatRerolledThisSettle", false);
        SetDieInt(die, "CheatPreviousValue", 0);
        SetDieBool(die, "ParityCompleteUsed", false);
        SetDieBool(die, "ParityReviewRerolled", false);
        SetDieInt(die, "ParityReviewPreviousValue", 0);
        SetDieBool(die, "Temporary", false);
    }

    private static void ScoreAndPreview()
    {
        InvokeDemo("RefreshHandPreview");
        int preview = (int)GetDemoField("previewRollScore");
        int score = (int)InvokeDemo("ScoreDice");
        ExpectInt(preview, score, "预览分与实际结算分一致");
    }

    private static void BeginCheatSelection()
    {
        SetDemoField("rollPhase", ParseRollPhase("ResultDecision"));
        SetDemoField("cheatsLeft", 1);
        InvokeDemo("BeginCheatEdit");
        ExpectEnumField("rollPhase", "CheatEdit", "进入出千选择状态");
    }

    private static void SelectAndConfirmCheat(object die)
    {
        BeginCheatSelection();
        InvokeDemo("ToggleCheatReroll", die);
        IList selected = (IList)GetDemoField("cheatRerollIds");
        ExpectInt(1, selected.Count, "出千选择数");
        InvokeDemo("ConfirmCheatAndSettle");
    }

    private static DieSpec Spec(string id, string type, int value, params int[] faces)
    {
        if (faces == null || faces.Length == 0)
        {
            faces = new[] { value, value, value, value, value, value };
        }

        return new DieSpec
        {
            Id = id,
            Type = type,
            Value = value,
            Faces = faces
        };
    }

    private static object InvokeDemo(string name, params object[] args)
    {
        MethodInfo method = typeof(DiceKingDemo).GetMethod(name, AnyInstance);
        if (method == null)
        {
            throw new MissingMethodException("DiceKingDemo", name);
        }

        return method.Invoke(demo, args);
    }

    private static object GetDemoField(string name)
    {
        FieldInfo field = typeof(DiceKingDemo).GetField(name, AnyInstance);
        if (field == null)
        {
            throw new MissingFieldException("DiceKingDemo", name);
        }

        return field.GetValue(demo);
    }

    private static void SetDemoField(string name, object value)
    {
        FieldInfo field = typeof(DiceKingDemo).GetField(name, AnyInstance);
        if (field == null)
        {
            throw new MissingFieldException("DiceKingDemo", name);
        }

        field.SetValue(demo, value);
    }

    private static object ParseDieType(string name)
    {
        return Enum.Parse(dieTypeEnum, name);
    }

    private static object ParseRollPhase(string name)
    {
        return Enum.Parse(rollPhaseEnum, name);
    }

    private static object ParseGameMode(string name)
    {
        return Enum.Parse(gameModeEnum, name);
    }

    private static void ExpectEnumField(string fieldName, string expectedName, string label)
    {
        string actual = GetDemoField(fieldName).ToString();
        ExpectString(expectedName, actual, label);
    }

    private static int GetDieScore(object die)
    {
        return GetDieInt(die, "Score");
    }

    private static string GetDieNote(object die)
    {
        return (string)GetDieField(die, "RoundNote");
    }

    private static int GetDieInt(object die, string name)
    {
        return (int)GetDieField(die, name);
    }

    private static bool GetDieBool(object die, string name)
    {
        return (bool)GetDieField(die, name);
    }

    private static object GetDieField(object die, string name)
    {
        FieldInfo field = die.GetType().GetField(name, AnyInstance);
        if (field == null)
        {
            throw new MissingFieldException(die.GetType().Name, name);
        }

        return field.GetValue(die);
    }

    private static void SetDieInt(object die, string name, int value)
    {
        SetDieField(die, name, value);
    }

    private static void SetDieBool(object die, string name, bool value)
    {
        SetDieField(die, name, value);
    }

    private static void SetDieString(object die, string name, string value)
    {
        SetDieField(die, name, value);
    }

    private static void SetDieField(object die, string name, object value)
    {
        FieldInfo field = die.GetType().GetField(name, AnyInstance);
        if (field == null)
        {
            throw new MissingFieldException(die.GetType().Name, name);
        }

        field.SetValue(die, value);
    }

    private static void ExpectInt(int expected, int actual, string label)
    {
        if (expected != actual)
        {
            throw new Exception(label + " 期望 " + expected + "，实际 " + actual);
        }
    }

    private static void ExpectString(string expected, string actual, string label)
    {
        if (expected != actual)
        {
            throw new Exception(label + " 期望 " + expected + "，实际 " + actual);
        }
    }

    private static void ExpectBool(bool expected, bool actual, string label)
    {
        if (expected != actual)
        {
            throw new Exception(label + " 期望 " + expected + "，实际 " + actual);
        }
    }

    private static void ExpectContains(string actual, string needle, string label)
    {
        if (actual == null || !actual.Contains(needle))
        {
            throw new Exception(label + " 期望包含 " + needle + "，实际 " + actual);
        }
    }

    private static void Case(string name, Action action)
    {
        try
        {
            action();
            Pass(name);
        }
        catch (Exception ex)
        {
            Fail(name, RootMessage(ex));
        }
    }

    private static string RootMessage(Exception ex)
    {
        TargetInvocationException invocation = ex as TargetInvocationException;
        if (invocation != null && invocation.InnerException != null)
        {
            return invocation.InnerException.Message;
        }

        return ex.Message;
    }

    private static void Section(string title)
    {
        Lines.Add("");
        Lines.Add("## " + title);
    }

    private static void Pass(string name)
    {
        Lines.Add("- PASS: " + name);
    }

    private static void Fail(string name, string message)
    {
        string line = "- FAIL: " + name + " - " + message;
        Lines.Add(line);
        Failures.Add(line);
    }

    private static void Capture(string relativePath, int width, int height)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(AbsolutePath(relativePath)));
        SetGameViewSize(width, height);
        ScreenCapture.CaptureScreenshot(relativePath);
        Lines.Add("  - 截图请求: " + relativePath + " (" + width + "x" + height + ")");
    }

    private static void SetGameViewSize(int width, int height)
    {
        Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
        int index = EnsureCustomGameViewSize(width, height, "F010 " + width + "x" + height);
        PropertyInfo selectedSizeIndex = gameViewType.GetProperty("selectedSizeIndex", AnyInstance);
        if (selectedSizeIndex != null && index >= 0)
        {
            selectedSizeIndex.SetValue(gameView, index, null);
        }

        gameView.Repaint();
    }

    private static int EnsureCustomGameViewSize(int width, int height, string label)
    {
        Assembly editorAssembly = typeof(Editor).Assembly;
        Type sizesType = editorAssembly.GetType("UnityEditor.GameViewSizes");
        Type singleType = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
        object sizes = singleType.GetProperty("instance").GetValue(null, null);
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
        object fixedResolution = Enum.Parse(gameViewSizeKindType, "FixedResolution");
        object newSize = constructor.Invoke(new object[] { fixedResolution, width, height, label });
        groupType.GetMethod("AddCustomSize").Invoke(group, new[] { newSize });
        return total;
    }

    private static void Finish()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(AbsolutePath(ReportPath)));
        Lines.Add("");
        Lines.Add("## 结论");
        if (Failures.Count == 0)
        {
            Lines.Add("PASS: F010 七颗奇偶短规则骰样例和出千路径通过 Unity Play Mode 验证。");
        }
        else
        {
            Lines.Add("FAIL: 发现 " + Failures.Count + " 个问题，需修复后复验。");
        }

        File.WriteAllLines(AbsolutePath(ReportPath), Lines.ToArray());

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.update -= StartWhenReady;
        EditorApplication.update -= RunChecksWhenPlaying;
        EditorApplication.update -= ScreenshotTick;
        SessionState.SetBool(ActiveKey, false);
        SessionState.SetBool(ChecksStartedKey, false);
        SessionState.SetInt(ScreenshotStepKey, 0);
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }

        running = false;
        Debug.Log("[F010 Validation] Report written: " + ReportPath + " failures=" + Failures.Count);
    }

    private static string AbsolutePath(string relativePath)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(projectRoot, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }

    private struct DieSpec
    {
        public string Id;
        public string Type;
        public int Value;
        public int[] Faces;
    }
}
