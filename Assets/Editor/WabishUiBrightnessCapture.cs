using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// 展示验证专用：独立实例、禁止存档、禁止推进玩法，结束后恢复原界面。
public static class WabishUiBrightnessCapture
{
    private const string ActiveKey = "Wabish.UiBrightnessCapture.Active";
    private const string PendingKey = "Wabish.UiBrightnessCapture.Pending";
    private const string WasPlayingKey = "Wabish.UiBrightnessCapture.WasPlaying";
    private const string WasPausedKey = "Wabish.UiBrightnessCapture.WasPaused";
    private const string TriggerName = "WabishUiBrightnessCapture.request";
    private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly string[] States = { "ready", "spell_bag_empty", "spell_bag", "shaking", "clear", "failed", "market" };
    private static readonly List<GameObject> HiddenOriginals = new List<GameObject>();
    private static DiceKingDemo demo;
    private static GameObject host;
    private static int index;
    private static int stage;
    private static int stableFrames;
    private static double stageStarted;
    private static double captureStarted;
    private static int previousSizeIndex;
    private static float previousAudioVolume;
    private static float previousTimeScale;
    private static UnityEngine.Random.State previousRandomState;
    private static EditorWindow gameView;
    private static string outputDirectory;

    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
    }

    [MenuItem("Tools/Wabish/Capture Unified UI Brightness")]
    public static void Capture()
    {
        if (SessionState.GetBool(ActiveKey, false)) return;
        SessionState.SetBool(WasPlayingKey, EditorApplication.isPlaying);
        SessionState.SetBool(WasPausedKey, EditorApplication.isPaused);
        SessionState.SetBool(PendingKey, true);
        if (!EditorApplication.isPlaying) EditorApplication.isPlaying = true;
    }

    private static void Tick()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        try
        {
            string trigger = Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp", TriggerName));
            if (File.Exists(trigger) && !SessionState.GetBool(ActiveKey, false))
            {
                File.Delete(trigger);
                Capture();
            }
            if (SessionState.GetBool(PendingKey, false) && EditorApplication.isPlaying)
            {
                SessionState.SetBool(PendingKey, false);
                Begin();
            }
            if (!SessionState.GetBool(ActiveKey, false)) return;
            if (!EditorApplication.isPlaying) { Finish("失败：验证期间退出了运行模式。"); return; }
            if (EditorApplication.timeSinceStartup - captureStarted > 90d)
                throw new InvalidOperationException("自动截帧超过 90 秒。");

            int width = index < States.Length ? 1280 : 1920;
            int height = index < States.Length ? 720 : 1080;
            if (stage == 0)
            {
                Configure(States[index % States.Length]);
                SetResolution(width, height);
                stableFrames = 0;
                Advance(1);
            }
            else if (stage == 1)
            {
                if (Screen.width != width || Screen.height != height)
                {
                    stableFrames = 0;
                    if (Elapsed() > 8d) throw new InvalidOperationException("GameView 尺寸未稳定。");
                    return;
                }
                if (++stableFrames < 3 || Elapsed() < 0.5d) return;
                MainGameLedFont font = Get("mainGameLedFont") as MainGameLedFont;
                if (font == null || !font.IsReady) throw new InvalidOperationException("正式 LED 字库未就绪，拒绝将系统字体回退截图视为通过。");
                ScreenCapture.CaptureScreenshot(OutputPath(width, height), 1);
                Advance(2);
            }
            else if (stage == 2 && Elapsed() > 0.6d)
            {
                string path = OutputPath(width, height);
                if (!File.Exists(path))
                {
                    if (Elapsed() > 5d) throw new InvalidOperationException("截图未落盘：" + path);
                    return;
                }
                Texture2D check = new Texture2D(2, 2);
                ImageConversion.LoadImage(check, File.ReadAllBytes(path));
                bool correct = check.width == width && check.height == height;
                UnityEngine.Object.DestroyImmediate(check);
                if (!correct) throw new InvalidOperationException("截图尺寸错误：" + path);
                index++;
                if (index >= States.Length * 2) { Finish("通过：七种展示状态的双分辨率截图均已生成，尺寸正确；MainGameLedFont.IsReady 为真；未执行交易、投掷或存档。"); return; }
                Advance(0);
            }
            if (gameView != null) gameView.Repaint();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            Finish("失败：" + exception);
        }
    }

    private static void Begin()
    {
        SessionState.SetBool(ActiveKey, true);
        outputDirectory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/QA"));
        Directory.CreateDirectory(outputDirectory);
        Type viewType = typeof(Editor).Assembly.GetType("UnityEditor.GameView");
        gameView = EditorWindow.GetWindow(viewType);
        previousSizeIndex = (int)viewType.GetProperty("selectedSizeIndex", InstanceFlags).GetValue(gameView, null);
        previousAudioVolume = AudioListener.volume;
        previousTimeScale = Time.timeScale;
        previousRandomState = UnityEngine.Random.state;
        HiddenOriginals.Clear();
        foreach (DiceKingDemo original in UnityEngine.Object.FindObjectsOfType<DiceKingDemo>())
        {
            if (!original.gameObject.activeSelf) continue;
            HiddenOriginals.Add(original.gameObject);
            original.gameObject.SetActive(false);
        }
        host = new GameObject("界面亮度验证（临时，不写存档）");
        host.SetActive(false);
        demo = host.AddComponent<DiceKingDemo>();
        Set("suppressSave", true);
        host.SetActive(true);
        Invoke("StartDefaultBuild");
        Set("diceVisualEnterStartTime", -999f);
        Set("mainMenuLaunchTransitionActive", false, false);
        Set("mainGameEntryPresentationActive", false, false);
        EditorApplication.isPaused = false;
        Time.timeScale = 0f;
        captureStarted = EditorApplication.timeSinceStartup;
        index = 0;
        Advance(0);
    }

    private static void Configure(string state)
    {
        Set("mode", "Run");
        Set("rollPhase", "Ready");
        Set("spellSystemEnabled", true);
        Set("spellPanelOpen", false);
        Set("rollResultsLocked", false);
        Set("settlementImpactPresentationActive", false);
        Set("stageFailureTransitionActive", false);
        Set("mainGameTargetCrossStartedAt", -999f);
        Set("mainGameRollIgnitionStartedAt", -999f);
        Set("stageClearPresentationStartedAt", -999f);
        Set("currentScore", 0);
        Set("remainingLives", 3);
        Set("rewardBanner", string.Empty);
        Set("diceVisualEnterStartTime", -999f);
        IList spells = (IList)Get("spellBag");
        spells.Clear();
        if (state == "spell_bag_empty" || state == "spell_bag") Set("spellPanelOpen", true);
        if (state == "spell_bag")
        {
            Type spellType = typeof(DiceKingDemo).GetNestedType("SpellType", BindingFlags.NonPublic);
            spells.Add(Enum.Parse(spellType, "PassBoost"));
            spells.Add(Enum.Parse(spellType, "TargetedRestock"));
        }
        if (state == "shaking")
        {
            Set("rollPhase", "Shaking");
            Set("diceVisualRollStartTime", Time.time - 0.25f);
            Set("shakeTimer", 0.25f);
        }
        if (state == "clear" || state == "failed")
        {
            Set("rollPhase", state == "clear" ? "StageClear" : "StageFailed");
            Set("rollResultsLocked", true);
            Set("passed", state == "clear");
            Set("stageFailureRetryAvailable", true);
            Set("currentScore", state == "clear" ? 20 : 8);
            Set("remainingLives", state == "clear" ? 3 : 2);
            Set("lastStageFlatIncome", 3);
            IList dice = (IList)Get("dice");
            for (int i = 0; i < dice.Count; i++)
            {
                object die = dice[i];
                die.GetType().GetField("EffectiveValue", InstanceFlags).SetValue(die, i + 1);
                die.GetType().GetField("LastValue", InstanceFlags).SetValue(die, i + 1);
            }
        }
        if (state == "market")
        {
            Set("mode", "InterStageMarket");
            Invoke("EnterMarket", false);
        }
    }

    private static string OutputPath(int width, int height)
    {
        return Path.Combine(outputDirectory, "20261106_ui_brightness_" + States[index % States.Length] + "_" + width + "x" + height + ".png");
    }

    private static void SetResolution(int width, int height)
    {
        MethodInfo method = typeof(WabishMainGameRuntimeCapture).GetMethod("ApplyResolution", BindingFlags.Static | BindingFlags.NonPublic);
        method.Invoke(null, new object[] { width, height });
    }

    private static object Get(string field)
    {
        return typeof(DiceKingDemo).GetField(field, InstanceFlags).GetValue(demo);
    }

    private static void Set(string field, object value, bool required = true)
    {
        FieldInfo info = typeof(DiceKingDemo).GetField(field, InstanceFlags);
        if (info == null) { if (required) throw new MissingFieldException(field); return; }
        if (info.FieldType.IsEnum && value is string) value = Enum.Parse(info.FieldType, (string)value);
        info.SetValue(demo, value);
    }

    private static object Invoke(string method, params object[] arguments)
    {
        return typeof(DiceKingDemo).GetMethod(method, InstanceFlags).Invoke(demo, arguments);
    }

    private static void Advance(int nextStage) { stage = nextStage; stageStarted = EditorApplication.timeSinceStartup; }
    private static double Elapsed() { return EditorApplication.timeSinceStartup - stageStarted; }

    private static void Finish(string message)
    {
        SessionState.SetBool(ActiveKey, false);
        SessionState.SetBool(PendingKey, false);
        if (host != null) UnityEngine.Object.DestroyImmediate(host);
        host = null;
        demo = null;
        AudioListener.volume = previousAudioVolume;
        Time.timeScale = previousTimeScale;
        UnityEngine.Random.state = previousRandomState;
        foreach (GameObject original in HiddenOriginals) if (original != null) original.SetActive(true);
        HiddenOriginals.Clear();
        if (gameView != null)
            gameView.GetType().GetProperty("selectedSizeIndex", InstanceFlags).SetValue(gameView, previousSizeIndex, null);
        EditorApplication.isPaused = SessionState.GetBool(WasPausedKey, false);
        if (!SessionState.GetBool(WasPlayingKey, false)) EditorApplication.isPlaying = false;
        string directory = outputDirectory ?? Path.GetFullPath(Path.Combine(Application.dataPath, "../Docs/QA"));
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "20261106_ui_brightness_capture_status.txt"), message + Environment.NewLine);
    }
}
