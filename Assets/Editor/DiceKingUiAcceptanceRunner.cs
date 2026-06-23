using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class DiceKingUiAcceptanceRunner
{
    private const string ActiveKey = "DiceKing.UiAcceptance.Active";
    private const string StepKey = "DiceKing.UiAcceptance.Step";
    private const string FrameKey = "DiceKing.UiAcceptance.Frame";
    private const string Root = "Docs/QA";
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    private static readonly List<string> report = new List<string>();
    private static DiceKingDemo demo;
    private static Type demoType;
    private static int framesToWait;
    private static string currentCapture;

    [InitializeOnLoadMethod]
    private static void ResumeIfActive()
    {
        if (SessionState.GetBool(ActiveKey, false))
        {
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }
    }

    public static void Run()
    {
        Directory.CreateDirectory(Root);
        SessionState.SetBool(ActiveKey, true);
        SessionState.SetInt(StepKey, 0);
        SessionState.SetInt(FrameKey, 0);
        report.Clear();
        report.Add("骰子王 UI 验收脚本日志");
        report.Add("时间: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        report.Add("说明: StageClear 截图通过脚本强制达标状态生成，用于检查 UI 状态，不代表自然通关样本。");

        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        SetGameViewSize(1920, 1080, "QA 1920x1080");
        EditorApplication.isPlaying = true;
    }

    private static void Tick()
    {
        if (!SessionState.GetBool(ActiveKey, false))
        {
            EditorApplication.update -= Tick;
            return;
        }

        if (framesToWait > 0)
        {
            framesToWait--;
            if (framesToWait == 0 && !string.IsNullOrEmpty(currentCapture))
            {
                CaptureNow(currentCapture);
                currentCapture = null;
            }

            return;
        }

        int step = SessionState.GetInt(StepKey, 0);
        try
        {
            RunStep(step);
            SessionState.SetInt(StepKey, step + 1);
        }
        catch (Exception ex)
        {
            report.Add("ERROR step " + step + ": " + ex);
            Finish();
        }
    }

    private static void RunStep(int step)
    {
        if (step == 0)
        {
            if (!EditorApplication.isPlaying)
            {
                WaitFrames(5);
                return;
            }

            demo = UnityEngine.Object.FindObjectOfType<DiceKingDemo>();
            if (demo == null)
            {
                WaitFrames(10);
                SessionState.SetInt(StepKey, 0);
                return;
            }

            demoType = typeof(DiceKingDemo);
            report.Add("进入 Play Mode，找到 DiceKingDemo。");
            CaptureAfterFrames("20260603_ui_main_menu_1920x1080.png", 4);
            return;
        }

        if (step == 1)
        {
            Invoke("StartDefaultBuild");
            CaptureAfterFrames("20260603_ui_ready_1920x1080.png", 4);
            return;
        }

        if (step == 2)
        {
            Invoke("BeginShakeRoll");
            CaptureAfterFrames("20260603_ui_shaking_1920x1080.png", 8);
            return;
        }

        if (step == 3)
        {
            Invoke("BeginStopRoll");
            CaptureAfterFrames("20260603_ui_stopping_1920x1080.png", 6);
            return;
        }

        if (step == 4)
        {
            Invoke("FinishShakeRoll");
            CaptureAfterFrames("20260603_ui_result_1920x1080.png", 5);
            return;
        }

        if (step == 5)
        {
            SetField("showHandReference", true);
            CaptureAfterFrames("20260603_ui_hand_reference_1920x1080.png", 4);
            return;
        }

        if (step == 6)
        {
            SetField("showHandReference", false);
            Invoke("BeginCheatEdit");
            CaptureAfterFrames("20260603_ui_cheat_empty_1920x1080.png", 4);
            return;
        }

        if (step == 7)
        {
            SelectCheatDice(3);
            CaptureAfterFrames("20260603_ui_cheat_three_selected_1920x1080.png", 4);
            return;
        }

        if (step == 8)
        {
            int before = GetCheatSelectionCount();
            TrySelectCheatDieAt(3);
            int after = GetCheatSelectionCount();
            report.Add("出千第 4 颗选择限制: before=" + before + ", after=" + after + "。");
            Invoke("CancelCheatEdit");
            CaptureAfterFrames("20260603_ui_result_after_cheat_cancel_1920x1080.png", 4);
            return;
        }

        if (step == 9)
        {
            Invoke("BeginCheatEdit");
            SelectCheatDice(1);
            Invoke("ConfirmCheatAndSettle");
            CaptureAfterFrames("20260603_ui_scoring_1920x1080.png", 5);
            return;
        }

        if (step == 10)
        {
            ForceStageClear();
            CaptureAfterFrames("20260603_ui_stage_clear_1920x1080.png", 4);
            return;
        }

        if (step == 11)
        {
            Invoke("EnterMarket", false);
            SetEnumField("mode", "InterStageMarket");
            CaptureAfterFrames("20260603_ui_market_1920x1080.png", 4);
            return;
        }

        if (step == 12)
        {
            SetGameViewSize(1280, 720, "QA 1280x720");
            Invoke("StartDefaultBuild");
            CaptureAfterFrames("20260603_ui_ready_1280x720.png", 8);
            return;
        }

        if (step == 13)
        {
            Invoke("BeginShakeRoll");
            Invoke("FinishShakeRoll");
            CaptureAfterFrames("20260603_ui_result_1280x720.png", 6);
            return;
        }

        if (step == 14)
        {
            Invoke("BeginCheatEdit");
            SelectCheatDice(3);
            CaptureAfterFrames("20260603_ui_cheat_1280x720.png", 5);
            return;
        }

        if (step == 15)
        {
            Invoke("CancelCheatEdit");
            ForceStageClear();
            CaptureAfterFrames("20260603_ui_stage_clear_1280x720.png", 5);
            return;
        }

        if (step == 16)
        {
            Invoke("EnterMarket", false);
            SetEnumField("mode", "InterStageMarket");
            CaptureAfterFrames("20260603_ui_market_1280x720.png", 5);
            return;
        }

        Finish();
    }

    private static void CaptureAfterFrames(string fileName, int frames)
    {
        currentCapture = fileName;
        WaitFrames(frames);
    }

    private static void CaptureNow(string fileName)
    {
        string path = Path.Combine(Root, fileName).Replace("\\", "/");
        ScreenCapture.CaptureScreenshot(path);
        report.Add("截图: " + path + " (" + Screen.width + "x" + Screen.height + ")");
        AssetDatabase.Refresh();
    }

    private static void WaitFrames(int frames)
    {
        framesToWait = Mathf.Max(1, frames);
    }

    private static object Invoke(string methodName, params object[] args)
    {
        MethodInfo method = demoType.GetMethod(methodName, InstanceFlags);
        if (method == null)
        {
            throw new MissingMethodException(demoType.FullName, methodName);
        }

        return method.Invoke(demo, args);
    }

    private static object GetField(string fieldName)
    {
        FieldInfo field = demoType.GetField(fieldName, InstanceFlags);
        if (field == null)
        {
            throw new MissingFieldException(demoType.FullName, fieldName);
        }

        return field.GetValue(demo);
    }

    private static void SetField(string fieldName, object value)
    {
        FieldInfo field = demoType.GetField(fieldName, InstanceFlags);
        if (field == null)
        {
            throw new MissingFieldException(demoType.FullName, fieldName);
        }

        field.SetValue(demo, value);
    }

    private static void SetEnumField(string fieldName, string value)
    {
        FieldInfo field = demoType.GetField(fieldName, InstanceFlags);
        if (field == null)
        {
            throw new MissingFieldException(demoType.FullName, fieldName);
        }

        field.SetValue(demo, Enum.Parse(field.FieldType, value));
    }

    private static void SelectCheatDice(int desiredCount)
    {
        IList dice = (IList)GetField("dice");
        MethodInfo toggle = demoType.GetMethod("ToggleCheatReroll", InstanceFlags);
        for (int i = 0; i < dice.Count && i < desiredCount; i++)
        {
            toggle.Invoke(demo, new object[] { dice[i] });
        }
    }

    private static void TrySelectCheatDieAt(int index)
    {
        IList dice = (IList)GetField("dice");
        if (index < 0 || index >= dice.Count)
        {
            return;
        }

        MethodInfo toggle = demoType.GetMethod("ToggleCheatReroll", InstanceFlags);
        toggle.Invoke(demo, new object[] { dice[index] });
    }

    private static int GetCheatSelectionCount()
    {
        IList selection = (IList)GetField("cheatRerollIds");
        return selection.Count;
    }

    private static void ForceStageClear()
    {
        object encounter = Invoke("CurrentEncounter");
        int target = 100;
        if (encounter != null)
        {
            FieldInfo targetField = encounter.GetType().GetField("Target", InstanceFlags);
            if (targetField != null)
            {
                target = (int)targetField.GetValue(encounter);
            }
        }

        SetField("currentScore", target);
        SetField("resolvedScore", target);
        SetField("passed", true);
        SetField("lastStageLedgerIncome", 1);
        SetField("lastStageFlatIncome", 0);
        SetField("lastStageInterestIncome", 3);
        SetField("lastStageIncome", 4);
        SetField("rewardBanner", "验收脚本强制达标，用于检查通关 UI。");
        SetEnumField("rollPhase", "StageClear");
    }

    private static void SetGameViewSize(int width, int height, string label)
    {
        Type gameViewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
        int index = EnsureCustomGameViewSize(width, height, label);
        PropertyInfo selectedSizeIndex = gameViewType.GetProperty("selectedSizeIndex", InstanceFlags);
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
            int w = (int)sizeType.GetProperty("width").GetValue(size, null);
            int h = (int)sizeType.GetProperty("height").GetValue(size, null);
            if (w == width && h == height)
            {
                return i;
            }
        }

        Type gameViewSizeType = editorAssembly.GetType("UnityEditor.GameViewSize");
        Type gameViewSizeKindType = editorAssembly.GetType("UnityEditor.GameViewSizeType");
        ConstructorInfo ctor = gameViewSizeType.GetConstructor(new[] { gameViewSizeKindType, typeof(int), typeof(int), typeof(string) });
        object fixedResolution = Enum.Parse(gameViewSizeKindType, "FixedResolution");
        object newSize = ctor.Invoke(new object[] { fixedResolution, width, height, label });
        groupType.GetMethod("AddCustomSize").Invoke(group, new[] { newSize });
        return total;
    }

    private static void Finish()
    {
        string reportPath = Path.Combine(Root, "20260603_ui_acceptance_script_log.txt");
        File.WriteAllLines(reportPath, report.ToArray());
        AssetDatabase.Refresh();
        SessionState.SetBool(ActiveKey, false);
        EditorApplication.update -= Tick;
        EditorApplication.isPlaying = false;
        EditorApplication.Exit(0);
    }
}
