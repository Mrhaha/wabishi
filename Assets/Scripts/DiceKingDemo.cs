using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class DiceKingDemo : MonoBehaviour
{
    private enum GameMode
    {
        MainMenu,
        Opening,
        Run,
        InterStageEvent,
        ChapterShop,
        Settings,
        Win,
        GameOver
    }

    private enum DiceMaterial
    {
        Wood,
        Iron,
        Glass,
        Rubber,
        Gold,
        Magnet,
        Stone,
        Paper
    }

    private enum DiceTrait
    {
        Normal,
        Piggy,
        Turtle,
        Joker,
        Boss,
        Clean,
        Social,
        Worker,
        Gambler,
        Crown
    }

    private enum RuleKind
    {
        None,
        OddLedger,
        PairAudit,
        LowFog,
        DoubleJudge
    }

    private enum RewardKind
    {
        AddDice,
        AddRelic,
        ChangeMaterial,
        CarveFace
    }

    private enum RollPhase
    {
        Ready,
        Shaking,
        Stopping,
        ResultDecision,
        CheatEdit,
        Scoring,
        StageClear,
        StageFailed
    }

    private sealed class Die
    {
        public int Id;
        public string Name;
        public DiceMaterial Material;
        public DiceTrait Trait;
        public int[] Faces;
        public bool Selected;
        public int LastValue;
        public int EffectiveValue;
        public int Score;
        public int Bank;
        public int Grit;
        public bool Cracked;
        public string RoundNote;

        public Die Clone(int id)
        {
            return new Die
            {
                Id = id,
                Name = Name,
                Material = Material,
                Trait = Trait,
                Faces = (int[])Faces.Clone(),
                Selected = Selected,
                LastValue = 0,
                EffectiveValue = 0,
                Score = 0,
                Bank = Bank,
                Grit = Grit,
                Cracked = false,
                RoundNote = string.Empty
            };
        }
    }

    private sealed class Encounter
    {
        public string Name;
        public int Target;
        public RuleKind Rule;
        public string RuleText;
        public bool Boss;

        public Encounter(string name, int target, RuleKind rule, string ruleText, bool boss)
        {
            Name = name;
            Target = target;
            Rule = rule;
            RuleText = ruleText;
            Boss = boss;
        }
    }

    private sealed class Relic
    {
        public string Id;
        public string Name;
        public string Text;

        public Relic(string id, string name, string text)
        {
            Id = id;
            Name = name;
            Text = text;
        }
    }

    private sealed class RewardOption
    {
        public RewardKind Kind;
        public string Title;
        public string Text;
        public string Tag;
        public Die Dice;
        public string RelicId;
        public int TargetDieId;
        public DiceMaterial NewMaterial;
        public int NewFaceValue;
    }

    private const float VirtualWidth = 1280f;
    private const float VirtualHeight = 720f;
    private const int MaxSelectedDice = 5;
    private const int RollsPerStage = 3;
    private const int CheatsPerStage = 1;
    private const float ShakeStartTime = 1.05f;
    private const float ShakeTapBonus = 0.32f;
    private const float ShakeMaxTime = 2.65f;
    private const float StopDuration = 0.42f;
    private const float ScoreStepDuration = 0.28f;
    private const float FinalScoreDuration = 0.34f;
    private const string SavePrefix = "DiceKingDemo.";

    private static int nextDieId = 1;

    private readonly List<Die> dice = new List<Die>();
    private readonly List<Relic> relics = new List<Relic>();
    private readonly List<Encounter> encounters = new List<Encounter>();
    private readonly List<RewardOption> rewards = new List<RewardOption>();
    private readonly List<int> shopCosts = new List<int>();
    private readonly List<string> logLines = new List<string>();
    private readonly List<Die> scoringDice = new List<Die>();

    private GameMode mode = GameMode.MainMenu;
    private GameMode settingsReturnMode = GameMode.MainMenu;
    private Texture2D whiteTexture;
    private Texture2D pipTexture;
    private Texture2D tableBackgroundTexture;
    private Texture2D diceCupTexture;
    private GUIStyle titleStyle;
    private GUIStyle headerStyle;
    private GUIStyle bodyStyle;
    private GUIStyle smallStyle;
    private GUIStyle tinyStyle;
    private GUIStyle buttonStyle;
    private GUIStyle selectedButtonStyle;
    private GUIStyle cardTitleStyle;
    private GUIStyle centerStyle;
    private Font uiFont;

    private int stageIndex;
    private int chapterGold;
    private int rollsLeft;
    private int cheatsLeft;
    private int currentScore;
    private int previewScore;
    private int goldEarnedThisRoll;
    private int lastComboScore;
    private int resolvedScore;
    private int scoreRevealIndex;
    private int selectedCheatIndex;
    private int buildId;
    private float lastMultiplier;
    private float shakeTimer;
    private float shakePower;
    private float stopTimer;
    private float stopStartPower;
    private float scoreStepTimer;
    private float openingTimer;
    private float settingsVolume = 1f;
    private float cheatSliderValue = 1f;
    private bool passed;
    private bool rolledThisEncounter;
    private bool finalScoreApplied;
    private bool confirmNewGame;
    private bool hasSave;
    private bool seenOpening;
    private bool windowed = true;
    private bool shopPurchased;
    private bool suppressSave;
    private RollPhase rollPhase = RollPhase.Ready;
    private string buildName = string.Empty;
    private string buildSummary = string.Empty;
    private string rewardBanner = string.Empty;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        if (FindObjectOfType<DiceKingDemo>() != null)
        {
            return;
        }

        GameObject runner = new GameObject("Dice King Demo");
        DontDestroyOnLoad(runner);
        runner.AddComponent<DiceKingDemo>();
    }

    private void Awake()
    {
        Camera camera = Camera.main;
        if (camera != null)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.08f, 0.11f, 0.13f);
        }

        BuildEncounters();
        LoadMenuState();
    }

    private void Update()
    {
        if (mode == GameMode.Opening)
        {
            openingTimer += Time.deltaTime;
            if (openingTimer >= 2.6f)
            {
                seenOpening = true;
                SaveMenuState();
                StartDefaultBuild();
            }

            return;
        }

        if (mode != GameMode.Run)
        {
            return;
        }

        if (rollPhase == RollPhase.Ready)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                BeginShakeRoll();
            }

            return;
        }

        if (rollPhase == RollPhase.Shaking)
        {
            UpdateShakeRoll();
            return;
        }

        if (rollPhase == RollPhase.Stopping)
        {
            UpdateStopRoll();
            return;
        }

        if (rollPhase == RollPhase.ResultDecision)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                BeginSettle();
            }

            return;
        }

        if (rollPhase == RollPhase.Scoring)
        {
            UpdateScoreReveal();
        }
    }

    private void OnGUI()
    {
        EnsureGui();

        float scale = Mathf.Min(Screen.width / VirtualWidth, Screen.height / VirtualHeight);
        if (scale <= 0f)
        {
            scale = 1f;
        }

        float offsetX = (Screen.width - VirtualWidth * scale) * 0.5f;
        float offsetY = (Screen.height - VirtualHeight * scale) * 0.5f;
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));

        DrawSceneBackdrop();

        switch (mode)
        {
            case GameMode.MainMenu:
                DrawMainMenu();
                break;
            case GameMode.Opening:
                DrawOpening();
                break;
            case GameMode.Run:
                DrawRun();
                break;
            case GameMode.InterStageEvent:
                DrawInterStageEvent();
                break;
            case GameMode.ChapterShop:
                DrawChapterShop();
                break;
            case GameMode.Settings:
                DrawSettings();
                break;
            case GameMode.Win:
                DrawEnd(true);
                break;
            case GameMode.GameOver:
                DrawEnd(false);
                break;
        }

        GUI.matrix = oldMatrix;
    }

    private void DrawMainMenu()
    {
        GUI.Label(new Rect(48f, 20f, 600f, 52f), "骰子王", titleStyle);
        GUI.Label(new Rect(54f, 92f, 680f, 36f), "摇骰、出千、结算，然后在关间事件里继续把随机变成生意。", bodyStyle);

        DrawPanel(new Rect(430f, 170f, 420f, 390f), new Color(0.11f, 0.1f, 0.08f, 0.86f));
        GUI.Label(new Rect(474f, 204f, 320f, 34f), "主菜单", headerStyle);

        if (GUI.Button(new Rect(474f, 260f, 332f, 48f), confirmNewGame && hasSave ? "确认覆盖并开始" : "开始新游戏", buttonStyle))
        {
            if (hasSave && !confirmNewGame)
            {
                confirmNewGame = true;
            }
            else
            {
                StartNewGameFlow();
            }
        }

        GUI.enabled = hasSave;
        if (GUI.Button(new Rect(474f, 322f, 332f, 48f), hasSave ? "继续游戏" : "继续游戏（无存档）", buttonStyle))
        {
            ContinueGameFlow();
        }
        GUI.enabled = true;

        if (GUI.Button(new Rect(474f, 384f, 332f, 48f), "设置", buttonStyle))
        {
            settingsReturnMode = GameMode.MainMenu;
            mode = GameMode.Settings;
        }

        if (GUI.Button(new Rect(474f, 446f, 332f, 48f), "退出游戏", buttonStyle))
        {
            Application.Quit();
        }

        if (confirmNewGame && hasSave)
        {
            GUI.Label(new Rect(474f, 512f, 332f, 28f), "再次点击开始会覆盖当前存档。", smallStyle);
        }
    }

    private void DrawOpening()
    {
        float t = Mathf.Clamp01(openingTimer / 2.6f);
        DrawRect(new Rect(0f, 0f, VirtualWidth, VirtualHeight), Color.black);

        float eyeOpen = Mathf.Sin(t * Mathf.PI);
        float slitHeight = 90f + eyeOpen * 430f;
        float y = VirtualHeight * 0.5f - slitHeight * 0.5f;
        DrawRect(new Rect(0f, y, VirtualWidth, slitHeight), new Color(0.1f, 0.12f, 0.12f));
        DrawRect(new Rect(0f, y + slitHeight * 0.46f, VirtualWidth, 8f), new Color(0.73f, 0.56f, 0.28f));

        GUI.color = new Color(0.9f, 0.84f, 0.68f, Mathf.Clamp01((t - 0.45f) * 2f));
        GUI.Label(new Rect(0f, 320f, VirtualWidth, 46f), "有人把骰盅推到了你面前。", centerStyle);
        GUI.color = Color.white;
    }

    private void DrawSettings()
    {
        GUI.Label(new Rect(48f, 20f, 600f, 52f), "设置", titleStyle);
        DrawPanel(new Rect(320f, 150f, 640f, 420f), new Color(0.15f, 0.18f, 0.18f));

        GUI.Label(new Rect(370f, 205f, 160f, 28f), "音量", headerStyle);
        settingsVolume = GUI.HorizontalSlider(new Rect(530f, 214f, 300f, 24f), settingsVolume, 0f, 1f);
        GUI.Label(new Rect(850f, 204f, 70f, 28f), Mathf.RoundToInt(settingsVolume * 100f) + "%", smallStyle);

        windowed = GUI.Toggle(new Rect(370f, 270f, 260f, 30f), windowed, "窗口模式");
        GUI.Label(new Rect(370f, 306f, 520f, 24f), "画面：原型默认布局", smallStyle);
        GUI.Label(new Rect(370f, 346f, 520f, 60f), "操作说明：点击骰袋选择上场骰子。按 Space 开始摇骰盅，摇晃阶段不断敲 Space 可以延长时间但有上限；回落停止后锁定结果。结果界面按 Space 直接结算，也可以出千改动单颗骰子。", smallStyle);

        if (GUI.Button(new Rect(370f, 448f, 180f, 48f), "保存并返回", selectedButtonStyle))
        {
            SaveMenuState();
            Screen.fullScreen = !windowed;
            mode = settingsReturnMode;
        }
    }

    private void DrawRun()
    {
        Encounter encounter = encounters[stageIndex];
        GUI.Label(new Rect(48f, 18f, 420f, 46f), "骰子王", titleStyle);
        GUI.Label(new Rect(300f, 26f, 470f, 32f), buildName + " | " + encounter.Name, headerStyle);
        GUI.Label(new Rect(790f, 28f, 430f, 28f), "目标 " + encounter.Target + "   当前 " + currentScore + "   金币 " + chapterGold + "   出手 " + rollsLeft + "   出千 " + cheatsLeft, bodyStyle);

        DrawPanel(new Rect(34f, 112f, 302f, 566f), new Color(0.14f, 0.16f, 0.17f));
        GUI.Label(new Rect(56f, 130f, 200f, 30f), "骰袋", headerStyle);
        GUI.Label(new Rect(56f, 160f, 250f, 24f), "点击切换上场，最多 " + MaxSelectedDice + " 枚", smallStyle);
        DrawDiceBag();

        DrawPanel(new Rect(360f, 112f, 560f, 400f), new Color(0.12f, 0.15f, 0.16f));
        GUI.Label(new Rect(386f, 132f, 360f, 30f), "投掷区", headerStyle);
        DrawTableDice(new Rect(386f, 180f, 500f, 260f));
        DrawProgressBar(new Rect(392f, 462f, 450f, 18f), currentScore, encounter.Target);
        GUI.Label(new Rect(850f, 455f, 42f, 30f), Mathf.RoundToInt(Mathf.Clamp01((float)currentScore / encounter.Target) * 100f) + "%", tinyStyle);

        DrawPanel(new Rect(940f, 112f, 300f, 566f), new Color(0.14f, 0.15f, 0.16f));
        GUI.Label(new Rect(964f, 132f, 250f, 30f), "本关规则", headerStyle);
        GUI.Label(new Rect(964f, 168f, 238f, 74f), encounter.RuleText, smallStyle);
        GUI.Label(new Rect(964f, 252f, 250f, 30f), "遗物", headerStyle);
        DrawRelics(new Rect(964f, 290f, 238f, 120f));
        GUI.Label(new Rect(964f, 420f, 250f, 30f), "结算记录", headerStyle);
        DrawLog(new Rect(964f, 456f, 238f, 188f));

        DrawPanel(new Rect(360f, 530f, 560f, 148f), new Color(0.16f, 0.18f, 0.17f));
        GUI.Label(new Rect(386f, 548f, 510f, 28f), buildSummary, smallStyle);
        GUI.Label(new Rect(386f, 578f, 510f, 26f), "剩余出手 " + rollsLeft + " | 剩余出千 " + cheatsLeft + " | 通关金币预览 +" + StageRewardPreview(), smallStyle);
        GUI.Label(new Rect(386f, 620f, 320f, 42f), RollPromptText(), headerStyle);

        if (rollPhase == RollPhase.ResultDecision)
        {
            if (GUI.Button(new Rect(604f, 620f, 130f, 42f), "直接结算", selectedButtonStyle))
            {
                BeginSettle();
            }

            GUI.enabled = cheatsLeft > 0;
            if (GUI.Button(new Rect(748f, 620f, 130f, 42f), cheatsLeft > 0 ? "出千" : "出千已用", buttonStyle))
            {
                BeginCheatEdit();
            }
            GUI.enabled = true;
        }

        if (rollPhase == RollPhase.CheatEdit)
        {
            GUI.Label(new Rect(604f, 620f, 162f, 42f), "滑动改面后确认", smallStyle);
            if (GUI.Button(new Rect(748f, 620f, 130f, 42f), "确认出千", selectedButtonStyle))
            {
                ConfirmCheatAndSettle();
            }
        }

        if (rollPhase == RollPhase.StageClear && mode == GameMode.Run && GUI.Button(new Rect(718f, 620f, 150f, 42f), encounter.Boss ? "进入商店" : "关间事件", selectedButtonStyle))
        {
            if (encounter.Boss)
            {
                BuildShopOptions();
                mode = GameMode.ChapterShop;
            }
            else
            {
                rewards.Clear();
                BuildInterStageEvents();
                mode = GameMode.InterStageEvent;
            }
        }

        if (rollPhase == RollPhase.StageFailed && mode == GameMode.Run && GUI.Button(new Rect(718f, 620f, 150f, 42f), "回主菜单", buttonStyle))
        {
            mode = GameMode.MainMenu;
        }

        GUI.enabled = true;
        if (!string.IsNullOrEmpty(rewardBanner))
        {
            GUI.Label(new Rect(386f, 604f, 500f, 24f), rewardBanner, smallStyle);
        }
    }

    private void DrawInterStageEvent()
    {
        GUI.Label(new Rect(48f, 20f, 600f, 52f), "关间事件", titleStyle);
        GUI.Label(new Rect(54f, 92f, 820f, 34f), "从随机抽出的 3 个固定事件中选一个。事件处理结束后直接进入下一小关。", bodyStyle);
        DrawPanel(new Rect(174f, 150f, 932f, 430f), new Color(0.16f, 0.18f, 0.18f));

        for (int i = 0; i < rewards.Count; i++)
        {
            Rect rect = new Rect(224f + i * 292f, 220f, 260f, 272f);
            DrawRewardCard(rect, rewards[i]);
            if (GUI.Button(new Rect(rect.x + 28f, rect.y + 210f, 204f, 42f), "拿走", selectedButtonStyle))
            {
                ApplyEventAndAdvance(rewards[i]);
            }
        }
    }

    private void DrawChapterShop()
    {
        GUI.Label(new Rect(48f, 20f, 600f, 52f), "章节商店", titleStyle);
        GUI.Label(new Rect(54f, 92f, 760f, 34f), "金币 " + chapterGold + "。章节结束后的购物点，买完即可完成当前 demo。", bodyStyle);
        DrawPanel(new Rect(174f, 150f, 932f, 430f), new Color(0.16f, 0.18f, 0.18f));

        for (int i = 0; i < rewards.Count; i++)
        {
            Rect rect = new Rect(224f + i * 292f, 220f, 260f, 272f);
            DrawRewardCard(rect, rewards[i]);
            int cost = i < shopCosts.Count ? shopCosts[i] : 12;
            GUI.enabled = chapterGold >= cost;
            if (GUI.Button(new Rect(rect.x + 28f, rect.y + 210f, 204f, 42f), "购买 " + cost + " 金", selectedButtonStyle))
            {
                chapterGold -= cost;
                ApplyOptionEffect(rewards[i]);
                shopPurchased = true;
                rewards.RemoveAt(i);
                shopCosts.RemoveAt(i);
                SaveRun();
                break;
            }
            GUI.enabled = true;
        }

        if (GUI.Button(new Rect(548f, 612f, 184f, 48f), shopPurchased ? "完成章节" : "不买离开", buttonStyle))
        {
            ClearSave();
            mode = GameMode.Win;
        }
    }

    private void DrawEnd(bool won)
    {
        GUI.Label(new Rect(48f, 18f, 460f, 46f), "骰子王", titleStyle);
        DrawPanel(new Rect(216f, 136f, 848f, 430f), new Color(0.15f, 0.18f, 0.18f));
        GUI.Label(new Rect(264f, 184f, 680f, 52f), won ? "第一章通关" : "本章失败", titleStyle);
        string result = won
            ? "你完成了 6 个小关、Boss 和章节商店。这个 demo 已经覆盖主菜单、开场、小关投骰、出千、关间事件与商店。"
            : "当前构筑没有压过本章目标，可以回主菜单重新开始，或者继续调整原型数值。";
        GUI.Label(new Rect(266f, 252f, 720f, 90f), result, bodyStyle);
        GUI.Label(new Rect(266f, 360f, 720f, 36f), "最终金币 " + chapterGold + " | 骰子数量 " + dice.Count + " | 遗物数量 " + relics.Count, headerStyle);

        if (GUI.Button(new Rect(266f, 438f, 190f, 52f), "回主菜单", buttonStyle))
        {
            mode = GameMode.MainMenu;
            rewardBanner = string.Empty;
        }
    }

    private void DrawDiceBag()
    {
        float y = 196f;
        for (int i = 0; i < dice.Count; i++)
        {
            Die die = dice[i];
            Rect rect = new Rect(56f, y, 238f, 76f);
            bool clicked = DrawDieCard(rect, die, true);
            if (clicked && mode == GameMode.Run && rollPhase == RollPhase.Ready && !passed)
            {
                ToggleDie(die);
            }

            y += 84f;
        }
    }

    private void DrawTableDice(Rect area)
    {
        List<Die> selected = GetSelectedDice();
        if (selected.Count == 0)
        {
            GUI.Label(new Rect(area.x + 20f, area.y + 92f, area.width - 40f, 36f), "至少选择一枚骰子。", centerStyle);
            return;
        }

        if (rollPhase == RollPhase.Shaking || rollPhase == RollPhase.Stopping)
        {
            DrawDiceCup(area);
            return;
        }

        float cardWidth = Mathf.Min(92f, (area.width - 18f * (selected.Count - 1)) / selected.Count);
        float startX = area.x + (area.width - selected.Count * cardWidth - (selected.Count - 1) * 18f) * 0.5f;
        for (int i = 0; i < selected.Count; i++)
        {
            Die die = selected[i];
            float scale = ScoreScaleForIndex(i);
            float scaledWidth = cardWidth * scale;
            Rect dieRect = new Rect(startX + i * (cardWidth + 18f) - (scaledWidth - cardWidth) * 0.5f, area.y + 18f - (scaledWidth - cardWidth) * 0.5f, scaledWidth, scaledWidth);
            int value = die.EffectiveValue > 0 ? die.EffectiveValue : 0;
            DrawDieFace(dieRect, value, MaterialColor(die.Material), die.Cracked);
            if (rollPhase == RollPhase.CheatEdit && i == selectedCheatIndex)
            {
                DrawRect(new Rect(dieRect.x - 5f, dieRect.y - 5f, dieRect.width + 10f, 4f), new Color(1f, 0.82f, 0.28f));
                DrawRect(new Rect(dieRect.x - 5f, dieRect.y + dieRect.height + 1f, dieRect.width + 10f, 4f), new Color(1f, 0.82f, 0.28f));
            }

            if (rollPhase == RollPhase.CheatEdit && GUI.Button(dieRect, GUIContent.none, GUIStyle.none))
            {
                selectedCheatIndex = i;
                cheatSliderValue = Mathf.Clamp(die.EffectiveValue, 1, 6);
            }

            if (rollPhase == RollPhase.Scoring && i == scoreRevealIndex)
            {
                DrawScoreFloat(dieRect, die.Score, scoreStepTimer / ScoreStepDuration);
            }

            GUI.Label(new Rect(dieRect.x - 12f, dieRect.y + dieRect.height + 10f, dieRect.width + 24f, 24f), die.Name, tinyStyle);
            GUI.Label(new Rect(dieRect.x - 12f, dieRect.y + dieRect.height + 32f, dieRect.width + 24f, 24f), die.Score > 0 ? "+" + die.Score : MaterialName(die.Material), tinyStyle);
        }

        if (rollPhase == RollPhase.Scoring && scoreRevealIndex >= selected.Count)
        {
            int bonus = Mathf.Max(0, resolvedScore - currentScore);
            if (bonus > 0)
            {
                DrawScoreFloat(new Rect(area.x + area.width * 0.5f - 54f, area.y + 140f, 108f, 48f), bonus, scoreStepTimer / FinalScoreDuration);
                GUI.Label(new Rect(area.x + 100f, area.y + 200f, area.width - 200f, 28f), "牌型与倍率", centerStyle);
            }
        }

        if (passed)
        {
            GUI.Label(new Rect(area.x + 60f, area.y + 210f, area.width - 120f, 34f), "达标，进入下一步。", centerStyle);
        }
        else if (rollPhase == RollPhase.ResultDecision)
        {
            GUI.Label(new Rect(area.x + 60f, area.y + 210f, area.width - 120f, 34f), "结果已锁定：Space 直接结算，或使用出千。", centerStyle);
        }
        else if (rollPhase == RollPhase.CheatEdit)
        {
            DrawCheatSlider(area, selected);
        }
        else if (rollPhase == RollPhase.StageFailed)
        {
            GUI.Label(new Rect(area.x + 60f, area.y + 210f, area.width - 120f, 34f), "出手用尽，本小关失败。", centerStyle);
        }
    }

    private void DrawCheatSlider(Rect area, List<Die> selected)
    {
        if (selected.Count == 0)
        {
            return;
        }

        selectedCheatIndex = Mathf.Clamp(selectedCheatIndex, 0, selected.Count - 1);
        Die die = selected[selectedCheatIndex];
        GUI.Label(new Rect(area.x + 50f, area.y + 190f, area.width - 100f, 24f), "出千目标：" + die.Name + "，当前朝向 " + die.EffectiveValue, centerStyle);
        cheatSliderValue = GUI.HorizontalSlider(new Rect(area.x + 130f, area.y + 226f, area.width - 260f, 24f), cheatSliderValue, 1f, 6f);
        die.EffectiveValue = Mathf.Clamp(Mathf.RoundToInt(cheatSliderValue), 1, 6);
        GUI.Label(new Rect(area.x + 200f, area.y + 242f, area.width - 400f, 24f), "滑动改变单颗骰子的结果朝向", centerStyle);
    }

    private void DrawDiceCup(Rect area)
    {
        float time = Time.time;
        float amplitude = 8f + shakePower * 24f;
        float xOffset = Mathf.Sin(time * (18f + shakePower * 14f)) * amplitude;
        float yOffset = Mathf.Sin(time * (24f + shakePower * 10f)) * amplitude * 0.22f;
        Rect cup = new Rect(area.x + area.width * 0.5f - 105f + xOffset, area.y + 18f + yOffset, 210f, 210f);

        DrawRect(new Rect(cup.x + 48f, cup.y + 178f, cup.width - 96f, 20f), new Color(0f, 0f, 0f, 0.34f));
        if (diceCupTexture != null)
        {
            Color old = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.98f);
            GUI.DrawTexture(cup, diceCupTexture);
            GUI.color = old;
        }
        else
        {
            DrawRect(new Rect(cup.x + 22f, cup.y + 8f, cup.width - 44f, 22f), new Color(0.75f, 0.56f, 0.28f));
            DrawRect(new Rect(cup.x + 10f, cup.y + 30f, cup.width - 20f, 92f), new Color(0.24f, 0.12f, 0.08f));
            DrawRect(new Rect(cup.x + 28f, cup.y + 42f, cup.width - 56f, 66f), new Color(0.34f, 0.17f, 0.1f));
        }

        if (rollPhase == RollPhase.Stopping)
        {
            float stopProgress = Mathf.Clamp01(stopTimer / StopDuration);
            GUI.Label(new Rect(area.x + 30f, area.y + 190f, area.width - 60f, 30f), "骰盅回落，准备开盅", centerStyle);
            DrawProgressBar(new Rect(area.x + 105f, area.y + 226f, area.width - 210f, 12f), Mathf.RoundToInt(stopProgress * 100f), 100);
        }
        else
        {
            float progress = Mathf.Clamp01(shakeTimer / ShakeMaxTime);
            GUI.Label(new Rect(area.x + 30f, area.y + 190f, area.width - 60f, 30f), "摇晃中：不断敲 Space 延迟停止", centerStyle);
            DrawProgressBar(new Rect(area.x + 105f, area.y + 226f, area.width - 210f, 12f), Mathf.RoundToInt(progress * 100f), 100);
        }
    }

    private float ScoreScaleForIndex(int index)
    {
        if (rollPhase != RollPhase.Scoring || index != scoreRevealIndex)
        {
            return 1f;
        }

        float t = Mathf.Clamp01(scoreStepTimer / ScoreStepDuration);
        return 1f + Mathf.Sin(t * Mathf.PI) * 0.18f;
    }

    private void DrawScoreFloat(Rect dieRect, int score, float normalizedTime)
    {
        float t = Mathf.Clamp01(normalizedTime);
        Color old = GUI.color;
        GUI.color = new Color(1f, 0.85f, 0.34f, 1f - t);
        GUI.Label(new Rect(dieRect.x - 20f, dieRect.y - 28f - t * 32f, dieRect.width + 40f, 32f), "+" + score, centerStyle);
        GUI.color = old;
    }

    private void DrawRelics(Rect area)
    {
        if (relics.Count == 0)
        {
            GUI.Label(area, "暂无遗物。", smallStyle);
            return;
        }

        float y = area.y;
        for (int i = 0; i < relics.Count; i++)
        {
            Relic relic = relics[i];
            GUI.Label(new Rect(area.x, y, area.width, 22f), relic.Name, smallStyle);
            GUI.Label(new Rect(area.x, y + 22f, area.width, 36f), relic.Text, tinyStyle);
            y += 62f;
            if (y > area.yMax - 28f)
            {
                break;
            }
        }
    }

    private void DrawLog(Rect area)
    {
        float y = area.y;
        int start = Mathf.Max(0, logLines.Count - 7);
        for (int i = start; i < logLines.Count; i++)
        {
            GUI.Label(new Rect(area.x, y, area.width, 28f), logLines[i], tinyStyle);
            y += 27f;
        }
    }

    private void DrawRewardCard(Rect rect, RewardOption reward)
    {
        Color color = reward.Tag == "金猪" ? new Color(0.25f, 0.2f, 0.12f) : new Color(0.12f, 0.2f, 0.23f);
        if (reward.Tag == "通用")
        {
            color = new Color(0.19f, 0.18f, 0.16f);
        }

        DrawPanel(rect, color);
        GUI.Label(new Rect(rect.x + 20f, rect.y + 20f, rect.width - 40f, 32f), reward.Title, cardTitleStyle);
        GUI.Label(new Rect(rect.x + 20f, rect.y + 58f, rect.width - 40f, 28f), reward.Tag + "路线", tinyStyle);
        GUI.Label(new Rect(rect.x + 20f, rect.y + 100f, rect.width - 40f, 100f), reward.Text, smallStyle);
    }

    private bool DrawDieCard(Rect rect, Die die, bool clickable)
    {
        Color bg = die.Selected ? new Color(0.24f, 0.27f, 0.22f) : new Color(0.11f, 0.13f, 0.14f);
        DrawPanel(rect, bg);
        DrawDieFace(new Rect(rect.x + 10f, rect.y + 10f, 52f, 52f), die.EffectiveValue > 0 ? die.EffectiveValue : 0, MaterialColor(die.Material), die.Cracked);
        GUI.Label(new Rect(rect.x + 72f, rect.y + 9f, rect.width - 84f, 22f), die.Name, smallStyle);
        GUI.Label(new Rect(rect.x + 72f, rect.y + 32f, rect.width - 84f, 20f), MaterialName(die.Material) + " / " + TraitName(die.Trait), tinyStyle);

        string tail = "面 " + FaceText(die.Faces);
        if (die.Trait == DiceTrait.Piggy)
        {
            tail += " | 存 " + die.Bank;
        }
        else if (die.Material == DiceMaterial.Stone)
        {
            tail += " | 磨 " + die.Grit;
        }

        GUI.Label(new Rect(rect.x + 72f, rect.y + 52f, rect.width - 84f, 18f), tail, tinyStyle);

        if (!clickable)
        {
            return false;
        }

        return GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }

    private void DrawMiniDiceRow(Rect rect, int[] values, DiceMaterial[] materials)
    {
        for (int i = 0; i < values.Length; i++)
        {
            DrawDieFace(new Rect(rect.x + i * 74f, rect.y, 56f, 56f), values[i], MaterialColor(materials[i]), false);
        }
    }

    private void DrawDieFace(Rect rect, int value, Color color, bool cracked)
    {
        DrawRect(rect, color);
        DrawRect(new Rect(rect.x + 3f, rect.y + 3f, rect.width - 6f, rect.height - 6f), cracked ? new Color(0.18f, 0.19f, 0.2f) : new Color(0.91f, 0.88f, 0.78f));
        if (value <= 0)
        {
            GUI.Label(rect, "?", centerStyle);
            return;
        }

        Color pipColor = cracked ? new Color(0.5f, 0.55f, 0.6f) : new Color(0.08f, 0.09f, 0.1f);
        float left = rect.x + rect.width * 0.28f;
        float mid = rect.x + rect.width * 0.5f;
        float right = rect.x + rect.width * 0.72f;
        float top = rect.y + rect.height * 0.28f;
        float center = rect.y + rect.height * 0.5f;
        float bottom = rect.y + rect.height * 0.72f;
        float size = Mathf.Max(6f, rect.width * 0.12f);

        if (value == 1 || value == 3 || value == 5)
        {
            DrawPip(mid, center, size, pipColor);
        }

        if (value >= 2)
        {
            DrawPip(left, top, size, pipColor);
            DrawPip(right, bottom, size, pipColor);
        }

        if (value >= 4)
        {
            DrawPip(right, top, size, pipColor);
            DrawPip(left, bottom, size, pipColor);
        }

        if (value == 6)
        {
            DrawPip(left, center, size, pipColor);
            DrawPip(right, center, size, pipColor);
        }
    }

    private void DrawPip(float x, float y, float size, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(new Rect(x - size * 0.5f, y - size * 0.5f, size, size), pipTexture);
        GUI.color = old;
    }

    private void DrawProgressBar(Rect rect, int value, int target)
    {
        DrawRect(rect, new Color(0.07f, 0.08f, 0.08f));
        float amount = Mathf.Clamp01((float)value / Mathf.Max(1, target));
        DrawRect(new Rect(rect.x, rect.y, rect.width * amount, rect.height), passed ? new Color(0.35f, 0.68f, 0.4f) : new Color(0.76f, 0.56f, 0.26f));
    }

    private void DrawPanel(Rect rect, Color color)
    {
        if (color.a > 0.98f)
        {
            color.a = 0.88f;
        }

        DrawRect(new Rect(rect.x + 6f, rect.y + 8f, rect.width, rect.height), new Color(0f, 0f, 0f, 0.28f));
        DrawRect(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f), color);
        DrawRect(new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, 2f), new Color(1f, 0.86f, 0.48f, 0.28f));
        DrawRect(new Rect(rect.x + 2f, rect.y + rect.height - 4f, rect.width - 4f, 2f), new Color(0f, 0f, 0f, 0.28f));
        DrawRect(new Rect(rect.x + 2f, rect.y + 2f, 2f, rect.height - 4f), new Color(1f, 0.86f, 0.48f, 0.16f));
        DrawRect(new Rect(rect.x + rect.width - 4f, rect.y + 2f, 2f, rect.height - 4f), new Color(0f, 0f, 0f, 0.24f));
    }

    private void DrawRect(Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, whiteTexture);
        GUI.color = old;
    }

    private void DrawSceneBackdrop()
    {
        if (tableBackgroundTexture != null)
        {
            GUI.DrawTexture(new Rect(0f, 0f, VirtualWidth, VirtualHeight), tableBackgroundTexture);
        }
        else
        {
            DrawRect(new Rect(0f, 0f, VirtualWidth, VirtualHeight), new Color(0.08f, 0.1f, 0.12f));
        }

        DrawRect(new Rect(0f, 0f, VirtualWidth, VirtualHeight), new Color(0.02f, 0.02f, 0.02f, 0.24f));
        DrawRect(new Rect(0f, 0f, VirtualWidth, 90f), new Color(0.04f, 0.04f, 0.04f, 0.58f));
        DrawRect(new Rect(0f, 88f, VirtualWidth, 4f), new Color(0.83f, 0.62f, 0.31f, 0.72f));
        DrawRect(new Rect(0f, 650f, VirtualWidth, 70f), new Color(0f, 0f, 0f, 0.28f));
    }

    private void StartNewGameFlow()
    {
        confirmNewGame = false;
        ClearSave();
        openingTimer = 0f;
        if (seenOpening)
        {
            StartDefaultBuild();
        }
        else
        {
            mode = GameMode.Opening;
        }
    }

    private void ContinueGameFlow()
    {
        int savedBuild = PlayerPrefs.GetInt(SavePrefix + "BuildId", 1);
        int savedStage = Mathf.Clamp(PlayerPrefs.GetInt(SavePrefix + "StageIndex", 0), 0, encounters.Count - 1);
        int savedGold = PlayerPrefs.GetInt(SavePrefix + "Gold", 12);

        suppressSave = true;
        if (savedBuild == 2)
        {
            StartStraightBuild();
        }
        else if (savedBuild == 0)
        {
            StartDefaultBuild();
        }
        else
        {
            StartPiggyBuild();
        }

        LoadRunCollection();
        stageIndex = savedStage;
        chapterGold = savedGold;
        suppressSave = false;
        StartEncounter();
        mode = GameMode.Run;
    }

    private void LoadMenuState()
    {
        hasSave = PlayerPrefs.GetInt(SavePrefix + "HasSave", 0) == 1;
        seenOpening = PlayerPrefs.GetInt(SavePrefix + "SeenOpening", 0) == 1;
        settingsVolume = PlayerPrefs.GetFloat(SavePrefix + "Volume", 1f);
        windowed = PlayerPrefs.GetInt(SavePrefix + "Windowed", 1) == 1;
        AudioListener.volume = settingsVolume;
        Screen.fullScreen = !windowed;
    }

    private void SaveMenuState()
    {
        PlayerPrefs.SetInt(SavePrefix + "SeenOpening", seenOpening ? 1 : 0);
        PlayerPrefs.SetFloat(SavePrefix + "Volume", settingsVolume);
        PlayerPrefs.SetInt(SavePrefix + "Windowed", windowed ? 1 : 0);
        AudioListener.volume = settingsVolume;
        PlayerPrefs.Save();
    }

    private void SaveRun()
    {
        hasSave = true;
        PlayerPrefs.SetInt(SavePrefix + "HasSave", 1);
        PlayerPrefs.SetInt(SavePrefix + "BuildId", buildId);
        PlayerPrefs.SetInt(SavePrefix + "StageIndex", stageIndex);
        PlayerPrefs.SetInt(SavePrefix + "Gold", chapterGold);
        PlayerPrefs.SetInt(SavePrefix + "SeenOpening", seenOpening ? 1 : 0);
        PlayerPrefs.SetString(SavePrefix + "DiceData", SerializeDice());
        PlayerPrefs.SetString(SavePrefix + "RelicData", SerializeRelics());
        PlayerPrefs.Save();
    }

    private void ClearSave()
    {
        hasSave = false;
        PlayerPrefs.SetInt(SavePrefix + "HasSave", 0);
        PlayerPrefs.SetString(SavePrefix + "DiceData", string.Empty);
        PlayerPrefs.SetString(SavePrefix + "RelicData", string.Empty);
        PlayerPrefs.Save();
    }

    private string SerializeDice()
    {
        string data = string.Empty;
        for (int i = 0; i < dice.Count; i++)
        {
            Die die = dice[i];
            if (i > 0)
            {
                data += "|";
            }

            data += die.Name + "~" + (int)die.Material + "~" + (int)die.Trait + "~" + FaceText(die.Faces).Replace("/", ",") + "~" + (die.Selected ? 1 : 0) + "~" + die.Bank + "~" + die.Grit;
        }

        return data;
    }

    private string SerializeRelics()
    {
        string data = string.Empty;
        for (int i = 0; i < relics.Count; i++)
        {
            if (i > 0)
            {
                data += ",";
            }

            data += relics[i].Id;
        }

        return data;
    }

    private void LoadRunCollection()
    {
        string diceData = PlayerPrefs.GetString(SavePrefix + "DiceData", string.Empty);
        if (!string.IsNullOrEmpty(diceData))
        {
            dice.Clear();
            nextDieId = 1;
            string[] entries = diceData.Split('|');
            for (int i = 0; i < entries.Length; i++)
            {
                string[] fields = entries[i].Split('~');
                if (fields.Length < 7)
                {
                    continue;
                }

                int material;
                int trait;
                int selected;
                int bank;
                int grit;
                if (!int.TryParse(fields[1], out material) || !int.TryParse(fields[2], out trait))
                {
                    continue;
                }

                int[] faces = ParseFaces(fields[3]);
                int.TryParse(fields[4], out selected);
                int.TryParse(fields[5], out bank);
                int.TryParse(fields[6], out grit);

                Die die = NewDie(fields[0], (DiceMaterial)Mathf.Clamp(material, 0, 7), (DiceTrait)Mathf.Clamp(trait, 0, 9), faces);
                die.Selected = selected == 1;
                die.Bank = bank;
                die.Grit = grit;
                dice.Add(die);
            }
        }

        string relicData = PlayerPrefs.GetString(SavePrefix + "RelicData", string.Empty);
        if (!string.IsNullOrEmpty(relicData))
        {
            relics.Clear();
            string[] ids = relicData.Split(',');
            for (int i = 0; i < ids.Length; i++)
            {
                if (!string.IsNullOrEmpty(ids[i]) && !HasRelic(ids[i]))
                {
                    relics.Add(MakeRelic(ids[i]));
                }
            }
        }
    }

    private int[] ParseFaces(string data)
    {
        int[] faces = new int[] { 1, 2, 3, 4, 5, 6 };
        string[] parts = data.Split(',');
        for (int i = 0; i < faces.Length && i < parts.Length; i++)
        {
            int value;
            if (int.TryParse(parts[i], out value))
            {
                faces[i] = Mathf.Clamp(value, 1, 6);
            }
        }

        Array.Sort(faces);
        return faces;
    }

    private void StartPiggyBuild()
    {
        buildId = 1;
        ResetRun("金猪爆发", "路线提示：优先拿猪猪、金骰、复利遗物。6 点会养存钱罐，金币能支撑后续重试。");
        AddDie(NewDie("猪猪骰", DiceMaterial.Gold, DiceTrait.Piggy, new int[] { 1, 2, 3, 4, 5, 6 }));
        AddDie(NewDie("老板骰", DiceMaterial.Iron, DiceTrait.Boss, new int[] { 2, 3, 3, 4, 4, 5 }));
        AddDie(NewDie("打工骰", DiceMaterial.Wood, DiceTrait.Worker, new int[] { 1, 2, 3, 4, 5, 6 }));
        AddDie(NewDie("玻璃赌徒", DiceMaterial.Glass, DiceTrait.Gambler, new int[] { 1, 2, 3, 4, 5, 6 }));
        relics.Add(MakeRelic("piggyBook"));
        StartEncounter();
    }

    private void StartDefaultBuild()
    {
        buildId = 0;
        ResetRun("王室骰袋", "路线提示：默认骰袋同时带有金猪经济和顺子修正的种子，关间事件会逐步让你偏向其中一条路线。");
        AddDie(NewDie("猪猪骰", DiceMaterial.Gold, DiceTrait.Piggy, new int[] { 1, 2, 3, 4, 5, 6 }));
        AddDie(NewDie("磁尺骰", DiceMaterial.Magnet, DiceTrait.Normal, new int[] { 1, 2, 3, 4, 5, 6 }));
        AddDie(NewDie("小丑骰", DiceMaterial.Rubber, DiceTrait.Joker, new int[] { 1, 2, 3, 4, 5, 6 }));
        AddDie(NewDie("打工骰", DiceMaterial.Wood, DiceTrait.Worker, new int[] { 1, 2, 3, 4, 5, 6 }));
        relics.Add(MakeRelic("piggyBook"));
        relics.Add(MakeRelic("straightBook"));
        StartEncounter();
    }

    private void StartStraightBuild()
    {
        buildId = 2;
        ResetRun("顺子修正", "路线提示：优先拿磁骰、小丑、乌龟和顺子遗物。站位会影响复制和拉点数。");
        AddDie(NewDie("磁尺骰", DiceMaterial.Magnet, DiceTrait.Normal, new int[] { 1, 2, 3, 4, 5, 6 }));
        AddDie(NewDie("小丑骰", DiceMaterial.Rubber, DiceTrait.Joker, new int[] { 1, 2, 3, 4, 5, 6 }));
        AddDie(NewDie("乌龟骰", DiceMaterial.Stone, DiceTrait.Turtle, new int[] { 2, 2, 3, 4, 5, 6 }));
        AddDie(NewDie("洁癖骰", DiceMaterial.Wood, DiceTrait.Clean, new int[] { 1, 2, 3, 4, 5, 6 }));
        relics.Add(MakeRelic("straightBook"));
        StartEncounter();
    }

    private void ResetRun(string newBuildName, string newBuildSummary)
    {
        dice.Clear();
        relics.Clear();
        rewards.Clear();
        logLines.Clear();
        nextDieId = 1;
        stageIndex = 0;
        chapterGold = 12;
        currentScore = 0;
        previewScore = 0;
        goldEarnedThisRoll = 0;
        passed = false;
        rolledThisEncounter = false;
        rewardBanner = string.Empty;
        buildName = newBuildName;
        buildSummary = newBuildSummary;
        mode = GameMode.Run;
    }

    private void StartEncounter()
    {
        currentScore = 0;
        previewScore = 0;
        goldEarnedThisRoll = 0;
        lastComboScore = 0;
        resolvedScore = 0;
        scoreRevealIndex = 0;
        lastMultiplier = 1f;
        shakeTimer = 0f;
        shakePower = 0f;
        stopTimer = 0f;
        stopStartPower = 0f;
        scoreStepTimer = 0f;
        passed = false;
        rolledThisEncounter = false;
        finalScoreApplied = false;
        rollPhase = RollPhase.Ready;
        scoringDice.Clear();
        rollsLeft = RollsPerStage;
        cheatsLeft = CheatsPerStage;
        selectedCheatIndex = 0;
        cheatSliderValue = 1f;
        rewardBanner = string.Empty;

        for (int i = 0; i < dice.Count; i++)
        {
            dice[i].LastValue = 0;
            dice[i].EffectiveValue = 0;
            dice[i].Score = 0;
            dice[i].RoundNote = string.Empty;
            dice[i].Cracked = false;
        }

        AddLog("进入 " + encounters[stageIndex].Name + "，目标 " + encounters[stageIndex].Target + "。");
        if (!suppressSave)
        {
            SaveRun();
        }
    }

    private void BeginShakeRoll()
    {
        if (rollsLeft <= 0 || passed || rollPhase != RollPhase.Ready)
        {
            return;
        }

        List<Die> selected = GetSelectedDice();
        if (selected.Count == 0)
        {
            AddLog("没有骰子上场。");
            return;
        }

        rolledThisEncounter = true;
        currentScore = 0;
        resolvedScore = 0;
        goldEarnedThisRoll = 0;
        lastComboScore = 0;
        lastMultiplier = 1f;
        scoreRevealIndex = 0;
        scoreStepTimer = 0f;
        shakeTimer = ShakeStartTime;
        shakePower = 0.72f;
        stopTimer = 0f;
        stopStartPower = 0f;
        finalScoreApplied = false;
        scoringDice.Clear();

        for (int i = 0; i < dice.Count; i++)
        {
            dice[i].Score = 0;
            dice[i].LastValue = 0;
            dice[i].EffectiveValue = 0;
            dice[i].RoundNote = string.Empty;
            dice[i].Cracked = false;
        }

        rollPhase = RollPhase.Shaking;
        AddLog("摇骰盅开始，敲空格可延迟停止。");
    }

    private void UpdateShakeRoll()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            shakeTimer = Mathf.Min(ShakeMaxTime, shakeTimer + ShakeTapBonus);
            shakePower = Mathf.Min(1.7f, shakePower + 0.18f);
        }

        shakeTimer -= Time.deltaTime;
        shakePower = Mathf.Max(0.35f, shakePower - Time.deltaTime * 0.18f);
        if (shakeTimer <= 0f)
        {
            BeginStopRoll();
        }
    }

    private void BeginStopRoll()
    {
        stopTimer = 0f;
        stopStartPower = Mathf.Max(0.65f, shakePower);
        rollPhase = RollPhase.Stopping;
        AddLog("骰盅快速回落。");
    }

    private void UpdateStopRoll()
    {
        stopTimer += Time.deltaTime;
        float t = Mathf.Clamp01(stopTimer / StopDuration);
        shakePower = Mathf.Max(0.02f, stopStartPower * (1f - t));
        if (stopTimer >= StopDuration)
        {
            FinishShakeRoll();
        }
    }

    private void FinishShakeRoll()
    {
        List<Die> selected = GetSelectedDice();
        if (selected.Count == 0)
        {
            rollPhase = RollPhase.Ready;
            return;
        }

        rollsLeft--;
        scoringDice.Clear();
        scoringDice.AddRange(selected);

        for (int i = 0; i < selected.Count; i++)
        {
            RollOneDie(selected[i]);
        }

        ApplyJokerCopies(selected);
        ApplyMagnetPull(selected);
        currentScore = 0;
        resolvedScore = 0;
        scoreRevealIndex = 0;
        scoreStepTimer = 0f;
        finalScoreApplied = false;
        rollPhase = RollPhase.ResultDecision;
        AddLog("开盅，结果锁定。可直接结算或出千。");
    }

    private void BeginCheatEdit()
    {
        if (cheatsLeft <= 0 || rollPhase != RollPhase.ResultDecision)
        {
            return;
        }

        List<Die> selected = GetSelectedDice();
        if (selected.Count == 0)
        {
            return;
        }

        selectedCheatIndex = Mathf.Clamp(selectedCheatIndex, 0, selected.Count - 1);
        cheatSliderValue = Mathf.Clamp(selected[selectedCheatIndex].EffectiveValue, 1, 6);
        rollPhase = RollPhase.CheatEdit;
        AddLog("出千：选择一颗骰子并滑动改面。");
    }

    private void ConfirmCheatAndSettle()
    {
        if (cheatsLeft <= 0 || rollPhase != RollPhase.CheatEdit)
        {
            return;
        }

        cheatsLeft--;
        AddLog("出千完成，剩余出千 " + cheatsLeft + "。");
        BeginSettle();
    }

    private void BeginSettle()
    {
        if (rollPhase != RollPhase.ResultDecision && rollPhase != RollPhase.CheatEdit)
        {
            return;
        }

        List<Die> selected = GetSelectedDice();
        if (selected.Count == 0)
        {
            rollPhase = RollPhase.Ready;
            return;
        }

        scoringDice.Clear();
        scoringDice.AddRange(selected);
        goldEarnedThisRoll = 0;
        ScoreDice(selected);
        resolvedScore = currentScore;
        currentScore = 0;
        scoreRevealIndex = 0;
        scoreStepTimer = 0f;
        finalScoreApplied = false;
        rollPhase = RollPhase.Scoring;
        AddLog("二次确认，开始从左到右结算。");
    }

    private void UpdateScoreReveal()
    {
        scoreStepTimer += Time.deltaTime;

        if (scoreRevealIndex < scoringDice.Count)
        {
            if (scoreStepTimer >= ScoreStepDuration)
            {
                currentScore += scoringDice[scoreRevealIndex].Score;
                scoreRevealIndex++;
                scoreStepTimer = 0f;
            }

            return;
        }

        if (!finalScoreApplied)
        {
            if (scoreStepTimer >= FinalScoreDuration)
            {
                currentScore = resolvedScore;
                finalScoreApplied = true;
                scoreStepTimer = 0f;
            }

            return;
        }

        if (scoreStepTimer >= FinalScoreDuration)
        {
            CompleteScoreReveal();
        }
    }

    private void CompleteScoreReveal()
    {
        currentScore = resolvedScore;
        passed = currentScore >= encounters[stageIndex].Target;

        if (passed)
        {
            ResolvePassIncome();
            rollPhase = RollPhase.StageClear;
            AddLog("达标：" + currentScore + " / " + encounters[stageIndex].Target + "。");
        }
        else if (rollsLeft > 0)
        {
            rollPhase = RollPhase.Ready;
            AddLog("未达标：" + currentScore + " / " + encounters[stageIndex].Target + "，还可继续出手。");
        }
        else
        {
            rollPhase = RollPhase.StageFailed;
            AddLog("未达标：" + currentScore + " / " + encounters[stageIndex].Target + "。");
        }
    }

    private string RollPromptText()
    {
        if (passed)
        {
            return encounters[stageIndex].Boss ? "已达标，进入商店" : "已达标，进入关间事件";
        }

        if (rollPhase == RollPhase.Shaking)
        {
            return "敲 Space 延迟停止";
        }

        if (rollPhase == RollPhase.Stopping)
        {
            return "骰盅回落中";
        }

        if (rollPhase == RollPhase.ResultDecision)
        {
            return "Space 直接结算";
        }

        if (rollPhase == RollPhase.CheatEdit)
        {
            return "出千改面中";
        }

        if (rollPhase == RollPhase.Scoring)
        {
            return "结算中...";
        }

        if (rollsLeft <= 0)
        {
            return rollPhase == RollPhase.StageFailed ? "小关失败" : "出手用尽";
        }

        if (CountSelectedDice() == 0)
        {
            return "先选择骰子";
        }

        return rolledThisEncounter ? "按 Space 再摇一次" : "按 Space 开始摇骰盅";
    }

    private void RollOneDie(Die die)
    {
        int raw = die.Faces[UnityEngine.Random.Range(0, die.Faces.Length)];
        die.LastValue = raw;
        int value = raw;

        if (die.Material == DiceMaterial.Iron && value < 3)
        {
            value = 3;
            die.RoundNote = "铁骰垫底到 3";
        }
        else if (die.Material == DiceMaterial.Rubber)
        {
            int bounce = die.Faces[UnityEngine.Random.Range(0, die.Faces.Length)];
            value = Mathf.Max(value, bounce);
            die.RoundNote = "橡胶弹跳取高";
        }
        else if (die.Material == DiceMaterial.Glass && value == 1)
        {
            die.Cracked = true;
            die.RoundNote = "玻璃裂开";
        }

        die.EffectiveValue = Mathf.Clamp(value, 1, 6);
    }

    private void ApplyJokerCopies(List<Die> selected)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            Die die = selected[i];
            if (die.Trait != DiceTrait.Joker)
            {
                continue;
            }

            if (die.EffectiveValue % 2 == 1 && i > 0)
            {
                die.EffectiveValue = selected[i - 1].EffectiveValue;
                die.RoundNote = "小丑复制左邻";
            }
            else if (die.EffectiveValue % 2 == 0 && i < selected.Count - 1)
            {
                die.EffectiveValue = selected[i + 1].EffectiveValue;
                die.RoundNote = "小丑复制右邻";
            }
        }
    }

    private void ApplyMagnetPull(List<Die> selected)
    {
        for (int i = 0; i < selected.Count; i++)
        {
            Die magnet = selected[i];
            if (magnet.Material != DiceMaterial.Magnet)
            {
                continue;
            }

            if (i > 0)
            {
                PullToward(selected[i - 1], magnet.EffectiveValue);
            }

            if (i < selected.Count - 1)
            {
                PullToward(selected[i + 1], magnet.EffectiveValue);
            }
        }
    }

    private void PullToward(Die die, int target)
    {
        if (die.EffectiveValue < target)
        {
            die.EffectiveValue++;
            die.RoundNote = AppendNote(die.RoundNote, "被磁骰拉高");
        }
        else if (die.EffectiveValue > target)
        {
            die.EffectiveValue--;
            die.RoundNote = AppendNote(die.RoundNote, "被磁骰压低");
        }
    }

    private void ScoreDice(List<Die> selected)
    {
        Encounter encounter = encounters[stageIndex];
        bool allUnique = AreAllValuesUnique(selected);
        int bossValue = 0;
        for (int i = 0; i < selected.Count; i++)
        {
            if (selected[i].Trait == DiceTrait.Boss)
            {
                bossValue = Mathf.Max(bossValue, selected[i].EffectiveValue);
            }
        }

        int subtotalBeforeTurtle = 0;
        for (int i = 0; i < selected.Count; i++)
        {
            Die die = selected[i];
            if (die.Trait == DiceTrait.Turtle)
            {
                continue;
            }

            die.Score = ScoreOneDie(die, selected, allUnique, bossValue, subtotalBeforeTurtle);
            subtotalBeforeTurtle += die.Score;
        }

        for (int i = 0; i < selected.Count; i++)
        {
            Die die = selected[i];
            if (die.Trait != DiceTrait.Turtle)
            {
                continue;
            }

            die.Score = ScoreOneDie(die, selected, allUnique, bossValue, subtotalBeforeTurtle);
            subtotalBeforeTurtle += die.Score;
        }

        if (encounter.Rule == RuleKind.OddLedger)
        {
            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i].EffectiveValue % 2 == 1)
                {
                    selected[i].Score += 14;
                }
            }
        }
        else if (encounter.Rule == RuleKind.LowFog)
        {
            for (int i = 0; i < selected.Count; i++)
            {
                if (selected[i].EffectiveValue <= 2)
                {
                    selected[i].Score = Mathf.RoundToInt(selected[i].Score * 0.55f);
                }
            }
        }
        else if (encounter.Rule == RuleKind.DoubleJudge)
        {
            for (int i = 0; i < selected.Count; i++)
            {
                float modifier = selected[i].EffectiveValue % 2 == 0 ? 1.35f : 0.8f;
                selected[i].Score = Mathf.RoundToInt(selected[i].Score * modifier);
            }
        }

        int individual = 0;
        for (int i = 0; i < selected.Count; i++)
        {
            individual += selected[i].Score;
        }

        int combo = ScoreCombinations(selected, encounter);
        float multiplier = ScoreMultiplier(selected, encounter);
        currentScore = Mathf.RoundToInt((individual + combo) * multiplier);
        previewScore = combo;
        lastComboScore = combo;
        lastMultiplier = multiplier;

        AddLog("单骰 " + individual + "，牌型 +" + combo + "，倍率 x" + multiplier.ToString("0.00") + "。");
    }

    private int ScoreOneDie(Die die, List<Die> selected, bool allUnique, int bossValue, int subtotalBeforeTurtle)
    {
        if (die.Cracked)
        {
            return 0;
        }

        int value = die.EffectiveValue;
        int score = value * 10;

        switch (die.Material)
        {
            case DiceMaterial.Wood:
                if (value <= 3)
                {
                    score += 16;
                }
                break;
            case DiceMaterial.Iron:
                score += 10;
                break;
            case DiceMaterial.Glass:
                score *= 2;
                break;
            case DiceMaterial.Rubber:
                score += 8;
                break;
            case DiceMaterial.Gold:
                score += value * 4;
                if (value == 6)
                {
                    goldEarnedThisRoll += 6;
                }
                if (HasRelic("goldSeal"))
                {
                    score += 18;
                }
                break;
            case DiceMaterial.Magnet:
                score += HasRelic("compass") ? 18 : 6;
                break;
            case DiceMaterial.Stone:
                score += die.Grit * 5;
                break;
            case DiceMaterial.Paper:
                score += 12;
                if (selected.Count >= 2)
                {
                    score += 10;
                }
                break;
        }

        if (bossValue > 0 && value < bossValue && die.Trait != DiceTrait.Boss)
        {
            score += 12;
        }

        switch (die.Trait)
        {
            case DiceTrait.Piggy:
                if (value == 6)
                {
                    die.Bank++;
                    die.RoundNote = AppendNote(die.RoundNote, "存钱罐 +1");
                }

                score += die.Bank * (HasRelic("piggyBook") ? 16 : 10);
                break;
            case DiceTrait.Turtle:
                if (subtotalBeforeTurtle >= encounters[stageIndex].Target * 0.42f)
                {
                    score *= 2;
                    if (HasRelic("turtleTea"))
                    {
                        score += 35;
                    }
                    die.RoundNote = AppendNote(die.RoundNote, "乌龟赶上了");
                }
                break;
            case DiceTrait.Joker:
                score += 8;
                break;
            case DiceTrait.Boss:
                score += 18;
                break;
            case DiceTrait.Clean:
                if (allUnique)
                {
                    score *= 3;
                    die.RoundNote = AppendNote(die.RoundNote, "全不同洁癖");
                }
                break;
            case DiceTrait.Social:
                if (selected.Count <= 3)
                {
                    score *= 4;
                }
                else
                {
                    score += 6;
                }
                break;
            case DiceTrait.Worker:
                score += 8;
                goldEarnedThisRoll += 3;
                break;
            case DiceTrait.Gambler:
                if (value == 1)
                {
                    score = 0;
                }
                else if (value == 6)
                {
                    score *= 3;
                }
                break;
            case DiceTrait.Crown:
                score += 20;
                break;
        }

        if (HasRelic("cleanLedger") && allUnique)
        {
            score += 10;
        }

        return score;
    }

    private int ScoreCombinations(List<Die> selected, Encounter encounter)
    {
        int[] counts = ValueCounts(selected);
        int combo = 0;
        int pairCount = 0;
        bool hasTriple = false;

        for (int value = 1; value <= 6; value++)
        {
            if (counts[value] >= 2)
            {
                pairCount++;
                combo += encounter.Rule == RuleKind.PairAudit ? 58 : 30;
            }

            if (counts[value] >= 3)
            {
                hasTriple = true;
                combo += 72;
            }

            if (counts[value] >= 4)
            {
                combo += 130;
            }
        }

        if (hasTriple && pairCount >= 2)
        {
            combo += 120;
        }

        int run = LongestRun(counts);
        if (run >= 4)
        {
            combo += HasRelic("straightBook") ? 90 : 50;
        }

        if (run >= 5)
        {
            combo += HasRelic("straightBook") ? 150 : 90;
        }

        if (encounter.Rule == RuleKind.DoubleJudge && run >= 4)
        {
            combo += 80;
        }

        int crownBonus = 0;
        for (int i = 0; i < selected.Count; i++)
        {
            if (selected[i].Trait == DiceTrait.Crown)
            {
                crownBonus = Mathf.Max(crownBonus, HighestSingleScore(selected) / 2);
            }
        }

        combo += crownBonus;
        return combo;
    }

    private float ScoreMultiplier(List<Die> selected, Encounter encounter)
    {
        float multiplier = 1f;
        bool allOdd = true;
        bool allEven = true;
        for (int i = 0; i < selected.Count; i++)
        {
            if (selected[i].EffectiveValue % 2 == 0)
            {
                allOdd = false;
            }
            else
            {
                allEven = false;
            }
        }

        if (allOdd)
        {
            multiplier += encounter.Rule == RuleKind.OddLedger ? 0.55f : 0.35f;
        }

        if (allEven)
        {
            multiplier += encounter.Rule == RuleKind.DoubleJudge ? 0.35f : 0.2f;
        }

        if (AreAllValuesUnique(selected) && HasRelic("cleanLedger"))
        {
            multiplier += 0.25f;
        }

        if (LongestRun(ValueCounts(selected)) >= 5)
        {
            multiplier += HasRelic("straightBook") ? 0.7f : 0.45f;
        }

        lastMultiplier = multiplier;
        return multiplier;
    }

    private void ResolvePassIncome()
    {
        int income = StageRewardPreview();

        chapterGold += income;
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i].Material == DiceMaterial.Stone)
            {
                dice[i].Grit++;
            }
        }

        AddLog("过关收入 +" + income + " 金币。");
        SaveRun();
    }

    private int StageRewardPreview()
    {
        return 10 + Mathf.Max(0, rollsLeft) + Mathf.Max(0, cheatsLeft);
    }

    private void BuildInterStageEvents()
    {
        rewards.Clear();

        RewardOption first = MakePiggyReward();
        RewardOption second = MakeStraightReward();
        RewardOption third = UnityEngine.Random.value > 0.5f ? MakeMaterialReward() : MakeCarveReward();

        rewards.Add(first);
        rewards.Add(second);
        rewards.Add(third);
    }

    private void BuildShopOptions()
    {
        rewards.Clear();
        shopCosts.Clear();
        shopPurchased = false;

        rewards.Add(MakePiggyReward());
        shopCosts.Add(10);
        rewards.Add(MakeStraightReward());
        shopCosts.Add(12);
        rewards.Add(UnityEngine.Random.value > 0.5f ? MakeMaterialReward() : MakeCarveReward());
        shopCosts.Add(8);
    }

    private RewardOption MakePiggyReward()
    {
        int roll = UnityEngine.Random.Range(0, 4);
        if (roll == 0)
        {
            return new RewardOption
            {
                Kind = RewardKind.AddDice,
                Title = "储钱罐骰",
                Text = "新增一枚木质猪猪骰。它投出 6 会积累存钱罐层数，越到后期越值钱。",
                Tag = "金猪",
                Dice = NewDie("储钱罐骰", DiceMaterial.Wood, DiceTrait.Piggy, new int[] { 2, 3, 4, 4, 5, 6 })
            };
        }

        if (roll == 1)
        {
            return new RewardOption
            {
                Kind = RewardKind.AddDice,
                Title = "账房金骰",
                Text = "新增一枚金质打工骰。单次得分不夸张，但会稳定给金币。",
                Tag = "金猪",
                Dice = NewDie("账房金骰", DiceMaterial.Gold, DiceTrait.Worker, new int[] { 1, 2, 3, 4, 5, 6 })
            };
        }

        if (roll == 2 && !HasRelic("goldSeal"))
        {
            return new RewardOption
            {
                Kind = RewardKind.AddRelic,
                Title = "金库印章",
                Text = "每枚金骰结算时额外 +18 分。适合把经济骰变成主力。",
                Tag = "金猪",
                RelicId = "goldSeal"
            };
        }

        return new RewardOption
        {
            Kind = RewardKind.ChangeMaterial,
            Title = "镀金",
            Text = "把一枚随机非金骰变成金骰。6 点会额外产金币。",
            Tag = "金猪",
            TargetDieId = RandomDieIdNotMaterial(DiceMaterial.Gold),
            NewMaterial = DiceMaterial.Gold
        };
    }

    private RewardOption MakeStraightReward()
    {
        int roll = UnityEngine.Random.Range(0, 5);
        if (roll == 0)
        {
            return new RewardOption
            {
                Kind = RewardKind.AddDice,
                Title = "磁针骰",
                Text = "新增一枚磁骰。它会把左右邻位的点数各拉近 1 点，方便凑顺子或对子。",
                Tag = "顺子",
                Dice = NewDie("磁针骰", DiceMaterial.Magnet, DiceTrait.Normal, new int[] { 1, 2, 3, 4, 5, 6 })
            };
        }

        if (roll == 1)
        {
            return new RewardOption
            {
                Kind = RewardKind.AddDice,
                Title = "乌龟账本",
                Text = "新增一枚石质乌龟骰。它最后结算，前面分数够高时自身翻倍。",
                Tag = "顺子",
                Dice = NewDie("乌龟账本", DiceMaterial.Stone, DiceTrait.Turtle, new int[] { 2, 3, 3, 4, 5, 6 })
            };
        }

        if (roll == 2)
        {
            return new RewardOption
            {
                Kind = RewardKind.AddDice,
                Title = "小丑骰",
                Text = "新增一枚橡胶小丑骰。奇数复制左邻，偶数复制右邻，站位越讲究越强。",
                Tag = "顺子",
                Dice = NewDie("小丑骰", DiceMaterial.Rubber, DiceTrait.Joker, new int[] { 1, 2, 3, 4, 5, 6 })
            };
        }

        if (roll == 3 && !HasRelic("compass"))
        {
            return new RewardOption
            {
                Kind = RewardKind.AddRelic,
                Title = "黄铜罗盘",
                Text = "磁骰自身额外 +18 分，顺手解决修点数时的低分问题。",
                Tag = "顺子",
                RelicId = "compass"
            };
        }

        return new RewardOption
        {
            Kind = RewardKind.AddRelic,
            Title = HasRelic("turtleTea") ? "整洁账册" : "乌龟茶点",
            Text = HasRelic("turtleTea") ? "全不同点数额外获得分数和倍率，适合顺子修正路线。" : "乌龟触发延后翻倍时额外 +35 分。",
            Tag = "顺子",
            RelicId = HasRelic("turtleTea") ? "cleanLedger" : "turtleTea"
        };
    }

    private RewardOption MakeMaterialReward()
    {
        DiceMaterial material = DiceMaterial.Glass;
        int roll = UnityEngine.Random.Range(0, 4);
        if (roll == 0)
        {
            material = DiceMaterial.Rubber;
        }
        else if (roll == 1)
        {
            material = DiceMaterial.Magnet;
        }
        else if (roll == 2)
        {
            material = DiceMaterial.Iron;
        }

        int targetId = RandomDieIdNotMaterial(material);
        Die target = FindDie(targetId);
        string targetName = target != null ? target.Name : "一枚骰子";
        return new RewardOption
        {
            Kind = RewardKind.ChangeMaterial,
            Title = "改材质：" + MaterialName(material),
            Text = "把 " + targetName + " 改成" + MaterialName(material) + "。材质会改变投掷或结算方式。",
            Tag = "通用",
            TargetDieId = targetId,
            NewMaterial = material
        };
    }

    private RewardOption MakeCarveReward()
    {
        int targetId = RandomDieId();
        Die target = FindDie(targetId);
        string targetName = target != null ? target.Name : "一枚骰子";
        int face = UnityEngine.Random.value > 0.55f ? 6 : 5;
        return new RewardOption
        {
            Kind = RewardKind.CarveFace,
            Title = "刻面：" + face + " 点",
            Text = "把 " + targetName + " 的最低面改成 " + face + "。这是最直接的稳定性提升。",
            Tag = "通用",
            TargetDieId = targetId,
            NewFaceValue = face
        };
    }

    private void ApplyOptionEffect(RewardOption reward)
    {
        if (reward.Kind == RewardKind.AddDice)
        {
            Die die = reward.Dice.Clone(nextDieId++);
            die.Selected = CountSelectedDice() < MaxSelectedDice;
            dice.Add(die);
            rewardBanner = "获得 " + die.Name + "。";
        }
        else if (reward.Kind == RewardKind.AddRelic)
        {
            if (!HasRelic(reward.RelicId))
            {
                relics.Add(MakeRelic(reward.RelicId));
                rewardBanner = "获得遗物：" + MakeRelic(reward.RelicId).Name + "。";
            }
            else
            {
                chapterGold += 12;
                rewardBanner = "已有遗物，折成 +12 金币。";
            }
        }
        else if (reward.Kind == RewardKind.ChangeMaterial)
        {
            Die target = FindDie(reward.TargetDieId);
            if (target != null)
            {
                target.Material = reward.NewMaterial;
                rewardBanner = target.Name + " 改成" + MaterialName(reward.NewMaterial) + "。";
            }
        }
        else if (reward.Kind == RewardKind.CarveFace)
        {
            Die target = FindDie(reward.TargetDieId);
            if (target != null)
            {
                int index = 0;
                for (int i = 1; i < target.Faces.Length; i++)
                {
                    if (target.Faces[i] < target.Faces[index])
                    {
                        index = i;
                    }
                }

                target.Faces[index] = reward.NewFaceValue;
                Array.Sort(target.Faces);
                rewardBanner = target.Name + " 的最低面刻成 " + reward.NewFaceValue + "。";
            }
        }

        SaveRun();
    }

    private void ApplyEventAndAdvance(RewardOption reward)
    {
        ApplyOptionEffect(reward);
        stageIndex++;
        if (stageIndex >= encounters.Count)
        {
            BuildShopOptions();
            mode = GameMode.ChapterShop;
            return;
        }

        StartEncounter();
        mode = GameMode.Run;
    }

    private void ToggleDie(Die die)
    {
        if (die.Selected)
        {
            die.Selected = false;
            return;
        }

        if (CountSelectedDice() >= MaxSelectedDice)
        {
            AddLog("上场骰子已满。");
            return;
        }

        die.Selected = true;
    }

    private void BuildEncounters()
    {
        encounters.Clear();
        encounters.Add(new Encounter("1-1 柜台试投", 100, RuleKind.None, "没有额外规则，先把基本引擎跑起来。", false));
        encounters.Add(new Encounter("1-2 奇数账房", 150, RuleKind.OddLedger, "奇数骰单骰 +14 分，全奇数倍率更高。", false));
        encounters.Add(new Encounter("1-3 对子审计", 215, RuleKind.PairAudit, "对子牌型奖励提高，复制和磁性会更好用。", false));
        encounters.Add(new Encounter("1-4 低点雾区", 295, RuleKind.LowFog, "1、2 点单骰分会被压低，木骰和刻面能缓解。", false));
        encounters.Add(new Encounter("1-5 金库门槛", 390, RuleKind.None, "纯目标关，检查你的构筑是否已经成型。", false));
        encounters.Add(new Encounter("1-6 王冠试投", 500, RuleKind.PairAudit, "对子奖励再次提高，为 Boss 前最后一次爆分。", false));
        encounters.Add(new Encounter("Boss 双面裁判", 640, RuleKind.DoubleJudge, "偶数骰单骰 x1.35，奇数骰 x0.8；顺子奖励额外 +80。", true));
    }

    private Die NewDie(string dieName, DiceMaterial material, DiceTrait trait, int[] faces)
    {
        return new Die
        {
            Id = nextDieId++,
            Name = dieName,
            Material = material,
            Trait = trait,
            Faces = (int[])faces.Clone(),
            Selected = true,
            LastValue = 0,
            EffectiveValue = 0,
            Score = 0,
            Bank = 0,
            Grit = 0,
            Cracked = false,
            RoundNote = string.Empty
        };
    }

    private void AddDie(Die die)
    {
        die.Selected = CountSelectedDice() < MaxSelectedDice;
        dice.Add(die);
    }

    private Relic MakeRelic(string id)
    {
        if (id == "piggyBook")
        {
            return new Relic(id, "王室存折", "猪猪层数加分提高，过关按层数给金币。");
        }

        if (id == "straightBook")
        {
            return new Relic(id, "顺子账本", "四连和五连顺子奖励显著提高。");
        }

        if (id == "goldSeal")
        {
            return new Relic(id, "金库印章", "每枚金骰结算时额外 +18 分。");
        }

        if (id == "compass")
        {
            return new Relic(id, "黄铜罗盘", "磁骰自身额外加分。");
        }

        if (id == "turtleTea")
        {
            return new Relic(id, "乌龟茶点", "乌龟触发延后翻倍时额外 +35。");
        }

        if (id == "cleanLedger")
        {
            return new Relic(id, "整洁账册", "全不同点数额外获得分数和倍率。");
        }

        return new Relic("rerollCup", "黑胶骰杯", "每关额外 +1 次投掷。");
    }

    private int CountSelectedDice()
    {
        int count = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i].Selected)
            {
                count++;
            }
        }

        return count;
    }

    private List<Die> GetSelectedDice()
    {
        List<Die> selected = new List<Die>();
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i].Selected)
            {
                selected.Add(dice[i]);
            }
        }

        return selected;
    }

    private bool HasRelic(string id)
    {
        for (int i = 0; i < relics.Count; i++)
        {
            if (relics[i].Id == id)
            {
                return true;
            }
        }

        return false;
    }

    private Die FindDie(int id)
    {
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i].Id == id)
            {
                return dice[i];
            }
        }

        return dice.Count > 0 ? dice[0] : null;
    }

    private int RandomDieId()
    {
        if (dice.Count == 0)
        {
            return 0;
        }

        return dice[UnityEngine.Random.Range(0, dice.Count)].Id;
    }

    private int RandomDieIdNotMaterial(DiceMaterial material)
    {
        List<Die> candidates = new List<Die>();
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i].Material != material)
            {
                candidates.Add(dice[i]);
            }
        }

        if (candidates.Count == 0)
        {
            return RandomDieId();
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)].Id;
    }

    private bool AreAllValuesUnique(List<Die> selected)
    {
        bool[] seen = new bool[7];
        for (int i = 0; i < selected.Count; i++)
        {
            int value = Mathf.Clamp(selected[i].EffectiveValue, 1, 6);
            if (seen[value])
            {
                return false;
            }

            seen[value] = true;
        }

        return true;
    }

    private int[] ValueCounts(List<Die> selected)
    {
        int[] counts = new int[7];
        for (int i = 0; i < selected.Count; i++)
        {
            int value = Mathf.Clamp(selected[i].EffectiveValue, 1, 6);
            counts[value]++;
        }

        return counts;
    }

    private int LongestRun(int[] counts)
    {
        int best = 0;
        int current = 0;
        for (int value = 1; value <= 6; value++)
        {
            if (counts[value] > 0)
            {
                current++;
                best = Mathf.Max(best, current);
            }
            else
            {
                current = 0;
            }
        }

        return best;
    }

    private int HighestSingleScore(List<Die> selected)
    {
        int best = 0;
        for (int i = 0; i < selected.Count; i++)
        {
            best = Mathf.Max(best, selected[i].Score);
        }

        return best;
    }

    private int TotalPiggyBank()
    {
        int total = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i].Trait == DiceTrait.Piggy)
            {
                total += dice[i].Bank;
            }
        }

        return total;
    }

    private void AddLog(string line)
    {
        logLines.Add(line);
        if (logLines.Count > 40)
        {
            logLines.RemoveAt(0);
        }
    }

    private string AppendNote(string current, string next)
    {
        if (string.IsNullOrEmpty(current))
        {
            return next;
        }

        return current + "，" + next;
    }

    private string FaceText(int[] faces)
    {
        string result = string.Empty;
        for (int i = 0; i < faces.Length; i++)
        {
            if (i > 0)
            {
                result += "/";
            }

            result += faces[i].ToString();
        }

        return result;
    }

    private string MaterialName(DiceMaterial material)
    {
        switch (material)
        {
            case DiceMaterial.Wood:
                return "木质";
            case DiceMaterial.Iron:
                return "铁质";
            case DiceMaterial.Glass:
                return "玻璃";
            case DiceMaterial.Rubber:
                return "橡胶";
            case DiceMaterial.Gold:
                return "金质";
            case DiceMaterial.Magnet:
                return "磁性";
            case DiceMaterial.Stone:
                return "石质";
            case DiceMaterial.Paper:
                return "纸质";
        }

        return "普通";
    }

    private string TraitName(DiceTrait trait)
    {
        switch (trait)
        {
            case DiceTrait.Piggy:
                return "猪猪";
            case DiceTrait.Turtle:
                return "乌龟";
            case DiceTrait.Joker:
                return "小丑";
            case DiceTrait.Boss:
                return "老板";
            case DiceTrait.Clean:
                return "洁癖";
            case DiceTrait.Social:
                return "社恐";
            case DiceTrait.Worker:
                return "打工";
            case DiceTrait.Gambler:
                return "赌徒";
            case DiceTrait.Crown:
                return "王冠";
        }

        return "普通";
    }

    private Color MaterialColor(DiceMaterial material)
    {
        switch (material)
        {
            case DiceMaterial.Wood:
                return new Color(0.45f, 0.31f, 0.19f);
            case DiceMaterial.Iron:
                return new Color(0.45f, 0.49f, 0.5f);
            case DiceMaterial.Glass:
                return new Color(0.44f, 0.74f, 0.88f);
            case DiceMaterial.Rubber:
                return new Color(0.17f, 0.17f, 0.18f);
            case DiceMaterial.Gold:
                return new Color(0.92f, 0.68f, 0.22f);
            case DiceMaterial.Magnet:
                return new Color(0.68f, 0.22f, 0.26f);
            case DiceMaterial.Stone:
                return new Color(0.42f, 0.43f, 0.4f);
            case DiceMaterial.Paper:
                return new Color(0.82f, 0.78f, 0.66f);
        }

        return Color.gray;
    }

    private void EnsureGui()
    {
        if (whiteTexture == null)
        {
            whiteTexture = Texture2D.whiteTexture;
        }

        if (pipTexture == null)
        {
            pipTexture = MakeCircleTexture(32);
        }

        if (tableBackgroundTexture == null)
        {
            tableBackgroundTexture = Resources.Load<Texture2D>("Art/table_background");
        }

        if (diceCupTexture == null)
        {
            diceCupTexture = Resources.Load<Texture2D>("Art/dice_cup");
        }

        if (titleStyle != null)
        {
            return;
        }

        uiFont = Font.CreateDynamicFontFromOSFont(new string[] { "Microsoft YaHei", "SimHei", "Arial" }, 18);

        titleStyle = NewStyle(34, FontStyle.Bold, new Color(0.92f, 0.84f, 0.65f));
        headerStyle = NewStyle(22, FontStyle.Bold, new Color(0.9f, 0.86f, 0.74f));
        bodyStyle = NewStyle(18, FontStyle.Normal, new Color(0.82f, 0.83f, 0.78f));
        smallStyle = NewStyle(15, FontStyle.Normal, new Color(0.76f, 0.78f, 0.74f));
        tinyStyle = NewStyle(12, FontStyle.Normal, new Color(0.7f, 0.73f, 0.7f));
        cardTitleStyle = NewStyle(20, FontStyle.Bold, new Color(0.95f, 0.84f, 0.54f));
        centerStyle = NewStyle(18, FontStyle.Bold, new Color(0.9f, 0.86f, 0.72f));
        centerStyle.alignment = TextAnchor.MiddleCenter;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.font = uiFont;
        buttonStyle.fontSize = 17;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.normal.textColor = new Color(0.92f, 0.88f, 0.76f);
        buttonStyle.hover.textColor = Color.white;
        buttonStyle.active.textColor = Color.white;
        buttonStyle.wordWrap = true;

        selectedButtonStyle = new GUIStyle(buttonStyle);
        selectedButtonStyle.normal.textColor = new Color(1f, 0.88f, 0.46f);
    }

    private GUIStyle NewStyle(int size, FontStyle fontStyle, Color color)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.font = uiFont;
        style.fontSize = size;
        style.fontStyle = fontStyle;
        style.normal.textColor = color;
        style.wordWrap = true;
        return style;
    }

    private Texture2D MakeCircleTexture(int size)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        float radius = size * 0.48f;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = distance <= radius ? 1f : 0f;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return texture;
    }
}
