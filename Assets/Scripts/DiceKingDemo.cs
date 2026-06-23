using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEngine;

public sealed class DiceKingDemo : MonoBehaviour
{
    private enum GameMode
    {
        MainMenu,
        Opening,
        Run,
        InterStageMarket,
        ChapterShop,
        Settings,
        Win,
        GameOver
    }

    private enum DieType
    {
        Basic,
        Piggy,
        Turtle,
        Double,
        Odd,
        Even,
        Tree,
        Gambler,
        Treasury,
        Bribe,
        Investment,
        BountyGold,
        TopGold,
        HandTax,
        Collection,
        CompoundInterest,
        LeadTicket,
        ShellTax,
        CounterGold,
        LumberGold,
        Shellsmith,
        Nest,
        SlowTurtle,
        LoneWitness,
        Stamp,
        HalfStep,
        Track,
        ParityNeighborDiff,
        ParityNeighborSame,
        ParityComplete,
        ParityReview,
        ParityFlipScore,
        ParityHoldScore,
        ParityTurner,
        Gardener,
        Irrigation,
        PointSeedTree,
        PatternTree,
        CanopyTree,
        RingTree,
        FertilizerTree,
        PruningTree,
        RootTree
    }

    private enum TreePatternTarget
    {
        None,
        ThreeKind,
        Straight,
        AllOdd,
        AllEven
    }

    private enum DiceMaterial
    {
        None,
        OfficialIron,
        GiltSeal,
        ClearGlaze,
        LeadSeal,
        CopperBone
    }

    private enum AffixSlot
    {
        Prefix,
        Suffix
    }

    private enum RuleKind
    {
        None,
        OddLedger,
        HandAudit,
        LowFog,
        DoubleJudge
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

    private enum UiButtonKind
    {
        Primary,
        Secondary,
        Danger
    }

    private enum RunUiIcon
    {
        Coin,
        Roll,
        Cheat,
        CurrentScore,
        TargetScore
    }

    private enum DiceTooltipState
    {
        Hidden,
        Waiting,
        FadingIn,
        Visible,
        Swapping,
        FadingOut
    }

    private enum RunScoreCounterPhase
    {
        Idle,
        Preview,
        Settling,
        Settled
    }

    private enum DiceVisualMotion
    {
        Entry,
        Rolling,
        Reveal,
        Settled
    }

    private enum DiceVisualProfile
    {
        Balanced,
        Parity,
        Turtle,
        Tree,
        Gold,
        Burst
    }

    private enum SettlementEventKind
    {
        SlotScore,
        RouteHighlight,
        MultiplierStamp,
        BribeFinal,
        TargetSettle
    }

    private enum SettlementHighlightLevel
    {
        Normal,
        Route,
        Multiplier,
        Target
    }

    private enum SettlementTargetArea
    {
        Dice,
        Coin,
        Multiplier,
        Target
    }

    private enum MarketRoute
    {
        None,
        Parity,
        Odd,
        Even,
        Gold,
        Turtle,
        Tree,
        Burst
    }

    private enum MarketOfferKind
    {
        Die,
        CraftingItem
    }

    private sealed class AffixInstance
    {
        public string Key;
        public int Tier;

        public AffixInstance Clone()
        {
            return new AffixInstance
            {
                Key = Key,
                Tier = Tier
            };
        }
    }

    private sealed class Die
    {
        public int Id;
        public string Name;
        public DieType Type;
        public DiceMaterial Material;
        public int[] Faces;
        public int LastValue;
        public int EffectiveValue;
        public int Score;
        public int Growth;
        public int TargetFace;
        public TreePatternTarget PatternTarget;
        public int GamblerThreshold;
        public int InvestmentGold;
        public bool CollectionTriggeredThisStage;
        public List<AffixInstance> PrefixAffixes = new List<AffixInstance>();
        public List<AffixInstance> SuffixAffixes = new List<AffixInstance>();
        public bool LoneWitnessRerolled;
        public int LoneWitnessPreviousValue;
        public bool HalfStepBorrowed;
        public bool CheatRerolledThisSettle;
        public int CheatPreviousValue;
        public bool ParityCompleteUsed;
        public bool ParityReviewRerolled;
        public int ParityReviewPreviousValue;
        public bool TypeTriggeredThisSettle;
        public bool Temporary;
        public string RoundNote;

        public Die Clone(int id)
        {
            return new Die
            {
                Id = id,
                Name = Name,
                Type = Type,
                Material = Material,
                Faces = (int[])Faces.Clone(),
                LastValue = 0,
                EffectiveValue = 0,
                Score = 0,
                Growth = Growth,
                TargetFace = 0,
                PatternTarget = TreePatternTarget.None,
                GamblerThreshold = 0,
                InvestmentGold = 0,
                CollectionTriggeredThisStage = false,
                PrefixAffixes = CloneAffixes(PrefixAffixes),
                SuffixAffixes = CloneAffixes(SuffixAffixes),
                LoneWitnessRerolled = false,
                LoneWitnessPreviousValue = 0,
                HalfStepBorrowed = false,
                CheatRerolledThisSettle = false,
                CheatPreviousValue = 0,
                ParityCompleteUsed = false,
                ParityReviewRerolled = false,
                ParityReviewPreviousValue = 0,
                TypeTriggeredThisSettle = false,
                Temporary = Temporary,
                RoundNote = string.Empty
            };
        }

        private static List<AffixInstance> CloneAffixes(List<AffixInstance> affixes)
        {
            List<AffixInstance> result = new List<AffixInstance>();
            if (affixes == null)
            {
                return result;
            }

            for (int i = 0; i < affixes.Count; i++)
            {
                if (affixes[i] != null)
                {
                    result.Add(affixes[i].Clone());
                }
            }

            return result;
        }
    }

    private sealed class Encounter
    {
        public string Id;
        public string Name;
        public int Target;
        public RuleKind Rule;
        public string RuleText;
        public bool Boss;
        public int ChapterIndex;
        public int StageIndexInChapter;
        public int NormalStageCount;
        public int BossCount;
        public int TotalEncounterCount;

        public Encounter(
            string id,
            string name,
            int target,
            RuleKind rule,
            string ruleText,
            bool boss,
            int chapterIndex,
            int stageIndexInChapter,
            int normalStageCount,
            int bossCount,
            int totalEncounterCount)
        {
            Id = id;
            Name = name;
            Target = target;
            Rule = rule;
            RuleText = ruleText;
            Boss = boss;
            ChapterIndex = chapterIndex;
            StageIndexInChapter = stageIndexInChapter;
            NormalStageCount = normalStageCount;
            BossCount = bossCount;
            TotalEncounterCount = totalEncounterCount;
        }
    }

    private sealed class MarketOffer
    {
        public MarketOfferKind Kind;
        public Die Die;
        public CraftingItemDefinition CraftingItem;
        public int Price;
    }

    private sealed class MarketDieConfig
    {
        public DieType Type;
        public int BuyPrice;
        public int SellPrice;
        public string Tier;
        public int WeightChapter1To2;
        public int WeightChapter3To5;
        public int WeightChapter6To10;
    }

    private sealed class DiceMaterialConfig
    {
        public DiceMaterial Material;
        public string DisplayName;
        public int PriceModifier;
        public int SellModifier;
        public int WeightChapter1To2;
        public int WeightChapter3To5;
        public int WeightChapter6To10;
        public string ShortRule;
    }

    private sealed class DiceTypeTooltipConfig
    {
        public DieType Type;
        public string DisplayName;
        public string TooltipEffect;
    }

    private sealed class AffixTierConfig
    {
        public string CurveKey;
        public int[] Values = new int[6];
    }

    private sealed class AffixDefinition
    {
        public string Key;
        public string DisplayName;
        public AffixSlot Slot;
        public string TriggerKey;
        public string CurveKey;
        public int WeightChapter1To2;
        public int WeightChapter3To5;
        public int WeightChapter6To10;
        public string ShortRule;
        public string MutexGroup;
        public bool Enabled;
    }

    private sealed class CraftingItemDefinition
    {
        public string Key;
        public string DisplayName;
        public string CraftType;
        public int BuyPrice;
        public int UnlockChapter;
        public int WeightChapter1To2;
        public int WeightChapter3To5;
        public int WeightChapter6To10;
        public string ShortRule;
    }

    private sealed class AffixLocation
    {
        public AffixSlot Slot;
        public int Index;
        public AffixInstance Affix;
    }

    private sealed class FaceTemplateConfig
    {
        public string Key;
        public int PriceModifier;
        public int WeightChapter1To2;
        public int WeightChapter3To5;
        public int WeightChapter6To10;
        public int[] GenericFaces;
        public int[] OddFaces;
        public int[] EvenFaces;
    }

    private sealed class MarketRuleConfig
    {
        public int ChapterFrom;
        public int ChapterTo;
        public int NormalRefreshCost;
        public int BossRefreshCost;
        public int HighTierPityRefreshes;
    }

    private sealed class RollFeedbackConfig
    {
        public float StartResponseTime;
        public float InputWindowDuration;
        public float InputDebounce;
        public int MaxImpulseCount;
        public float BasePower;
        public float ImpulsePower;
        public float MaxPower;
        public float PowerDecayPerSecond;
        public float StopDuration;
        public float StopMinPower;
        public float ExpiredPromptDuration;
        public float CupXAmplitude;
        public float CupYAmplitude;
        public float CupRotationAmplitude;
        public float CupFrequency;
        public float TableShakeAmplitude;
        public float TableShakeDuration;
        public float PromptPulseScale;
        public float PromptPulseDuration;
        public float TapSoundVolume;
        public float ExpiredTapVolume;

        public static RollFeedbackConfig CreateDefault()
        {
            return new RollFeedbackConfig
            {
                StartResponseTime = 0.15f,
                InputWindowDuration = 1.35f,
                InputDebounce = 0.06f,
                MaxImpulseCount = 8,
                BasePower = 0.45f,
                ImpulsePower = 0.18f,
                MaxPower = 1.75f,
                PowerDecayPerSecond = 0.22f,
                StopDuration = 0.9f,
                StopMinPower = 0.02f,
                ExpiredPromptDuration = 0.12f,
                CupXAmplitude = 32f,
                CupYAmplitude = 7f,
                CupRotationAmplitude = 6f,
                CupFrequency = 20f,
                TableShakeAmplitude = 2f,
                TableShakeDuration = 0.08f,
                PromptPulseScale = 1.04f,
                PromptPulseDuration = 0.1f,
                TapSoundVolume = 0.65f,
                ExpiredTapVolume = 0.18f
            };
        }

        public RollFeedbackConfig Clone()
        {
            return new RollFeedbackConfig
            {
                StartResponseTime = StartResponseTime,
                InputWindowDuration = InputWindowDuration,
                InputDebounce = InputDebounce,
                MaxImpulseCount = MaxImpulseCount,
                BasePower = BasePower,
                ImpulsePower = ImpulsePower,
                MaxPower = MaxPower,
                PowerDecayPerSecond = PowerDecayPerSecond,
                StopDuration = StopDuration,
                StopMinPower = StopMinPower,
                ExpiredPromptDuration = ExpiredPromptDuration,
                CupXAmplitude = CupXAmplitude,
                CupYAmplitude = CupYAmplitude,
                CupRotationAmplitude = CupRotationAmplitude,
                CupFrequency = CupFrequency,
                TableShakeAmplitude = TableShakeAmplitude,
                TableShakeDuration = TableShakeDuration,
                PromptPulseScale = PromptPulseScale,
                PromptPulseDuration = PromptPulseDuration,
                TapSoundVolume = TapSoundVolume,
                ExpiredTapVolume = ExpiredTapVolume
            };
        }

        public bool ApplyValue(string key, float value)
        {
            switch (key.Trim().ToLowerInvariant())
            {
                case "start_response_time":
                    StartResponseTime = ClampConfigFloat(value, 0f, 1f, StartResponseTime);
                    return true;
                case "input_window_duration":
                    InputWindowDuration = ClampConfigFloat(value, 0.1f, 5f, InputWindowDuration);
                    return true;
                case "input_debounce":
                    InputDebounce = ClampConfigFloat(value, 0f, 0.3f, InputDebounce);
                    return true;
                case "max_impulse_count":
                    MaxImpulseCount = Mathf.Clamp(Mathf.RoundToInt(value), 0, 64);
                    return true;
                case "base_power":
                    BasePower = ClampConfigFloat(value, 0f, 3f, BasePower);
                    return true;
                case "impulse_power":
                    ImpulsePower = ClampConfigFloat(value, 0f, 3f, ImpulsePower);
                    return true;
                case "max_power":
                    MaxPower = ClampConfigFloat(value, 0f, 5f, MaxPower);
                    return true;
                case "power_decay_per_second":
                    PowerDecayPerSecond = ClampConfigFloat(value, 0f, 5f, PowerDecayPerSecond);
                    return true;
                case "stop_duration":
                    StopDuration = ClampConfigFloat(value, 0.05f, 3f, StopDuration);
                    return true;
                case "stop_min_power":
                    StopMinPower = ClampConfigFloat(value, 0f, 1f, StopMinPower);
                    return true;
                case "expired_prompt_duration":
                    ExpiredPromptDuration = ClampConfigFloat(value, 0f, 1f, ExpiredPromptDuration);
                    return true;
                case "cup_x_amplitude":
                    CupXAmplitude = ClampConfigFloat(value, 0f, 120f, CupXAmplitude);
                    return true;
                case "cup_y_amplitude":
                    CupYAmplitude = ClampConfigFloat(value, 0f, 60f, CupYAmplitude);
                    return true;
                case "cup_rotation_amplitude":
                    CupRotationAmplitude = ClampConfigFloat(value, 0f, 30f, CupRotationAmplitude);
                    return true;
                case "cup_frequency":
                    CupFrequency = ClampConfigFloat(value, 1f, 80f, CupFrequency);
                    return true;
                case "table_shake_amplitude":
                    TableShakeAmplitude = ClampConfigFloat(value, 0f, 20f, TableShakeAmplitude);
                    return true;
                case "table_shake_duration":
                    TableShakeDuration = ClampConfigFloat(value, 0f, 1f, TableShakeDuration);
                    return true;
                case "prompt_pulse_scale":
                    PromptPulseScale = ClampConfigFloat(value, 1f, 1.5f, PromptPulseScale);
                    return true;
                case "prompt_pulse_duration":
                    PromptPulseDuration = ClampConfigFloat(value, 0f, 1f, PromptPulseDuration);
                    return true;
                case "tap_sound_volume":
                    TapSoundVolume = ClampConfigFloat(value, 0f, 1f, TapSoundVolume);
                    return true;
                case "expired_tap_volume":
                    ExpiredTapVolume = ClampConfigFloat(value, 0f, 1f, ExpiredTapVolume);
                    return true;
            }

            return false;
        }
    }

    private sealed class TableDieView
    {
        public Die Die;
        public int ScoreIndexStart;
        public int ScoreIndexEnd;
        public int Value;
        public int ScoreFloatValue;
        public string PrimaryText;
        public string SecondaryText;
        public bool Summary;
    }

    private sealed class DiceVisualState
    {
        public int DieId;
        public float Seed;
    }

    private sealed class RunScoreCounterState
    {
        public RunScoreCounterPhase Phase;
        public int ScoreBeforeRoll;
        public int ProgressScore;
        public int TargetScore;
        public int BaseScore;
        public float Multiplier;
        public int MultipliedScore;
        public int BribeScore;
        public int RollScore;
        public int ResolvedScore;
    }

    private sealed class RunScoreCounterStep
    {
        public int BaseScore;
        public float Multiplier;
        public int MultipliedScore;
        public int BribeScore;
        public int RollScore;
        public int ProgressScore;
    }

    private sealed class SettlementDisplayEvent
    {
        public SettlementEventKind Kind;
        public int SlotIndex = -1;
        public int ScoreIndex = -1;
        public int ScoreIndexEnd = -1;
        public int DieId;
        public string Label;
        public int ValueDelta;
        public int GoldDelta;
        public int BaseScore;
        public float Multiplier = 1f;
        public int ProgressScore;
        public float Duration;
        public SettlementHighlightLevel HighlightLevel;
        public SettlementTargetArea TargetArea;
        public int CounterStepIndex = -1;
        public bool ApplyCounterStep;
        public bool ApplyFinal;
        public bool Passed;
    }

    private struct TurtlePreviewStats
    {
        public float Score;
        public float Count;
    }

    private sealed class HandResult
    {
        public string Name;
        public float Multiplier;
        public int LongestRun;
        public bool AllOdd;
        public bool AllEven;
        public bool UsedHalfStep;
        public bool UsedParityComplete;
    }

    private const float DesignWidth = 1920f;
    private const float DesignHeight = 1080f;
    private const float VirtualWidth = 1280f;
    private const float VirtualHeight = 720f;
    private const float PrototypeToDesignScale = 1.5f;
    private const int DiceCapacity = 6;
    private const int RollsPerStage = 3;
    private const int CheatsPerStage = 1;
    private const int MaxCheatRerollDice = 3;
    private const int TemporaryDiceDisplayLimit = 8;
    private const int MaxPrefixAffixes = 2;
    private const int MaxSuffixAffixes = 2;
    private const int BaseScorePerPip = 1;
    private const float ScoreStepDuration = 0.25f;
    private const float FinalScoreDuration = 0.34f;
    private const float ScoreCounterPulseDuration = 0.24f;
    private const float SettlementSlotDuration = 0.2f;
    private const float SettlementRouteDuration = 0.22f;
    private const float SettlementMultiplierDuration = 0.24f;
    private const float SettlementFinalDuration = 0.34f;
    private const float SettlementTargetDuration = 0.3f;
    private const float DiceVisualEnterDuration = 0.28f;
    private const float DiceVisualEnterStagger = 0.025f;
    private const float DiceVisualRevealDuration = 0.72f;
    private const float DiceVisualImpulseDuration = 0.18f;
    private const float DiceVisualRollSpeed = 8.6f;
    private const int DiceVisualSequenceFrameCount = 24;
    private const int DiceVisualGeneratedSequenceFrameCount = 12;
    private const int DiceRollLoopFrameCount = 24;
    private const int DiceRollStopFrameCount = 8;
    private const int DiceRollResultFaceCount = 6;
    private const string SavePrefix = "DiceKingDemo.";
    private const int CurrentSaveVersion = 4;
    private const string EncounterTableResourcePath = "Data/chapter_score_table";
    private const string MarketDieConfigResourcePath = "Data/dice_market_config";
    private const string DiceTypeConfigResourcePath = "Data/dice_type_config";
    private const string MarketRuleConfigResourcePath = "Data/market_rule_config";
    private const string DiceMaterialConfigResourcePath = "Data/dice_material_config";
    private const string DiceAffixTierConfigResourcePath = "Data/dice_affix_tier_config";
    private const string DiceAffixConfigResourcePath = "Data/dice_affix_config";
    private const string DiceCraftingItemConfigResourcePath = "Data/dice_crafting_item_config";
    private const string GlobalConfigResourcePath = "Data/global";
    private const string RollFeedbackConfigResourcePath = "Data/roll_feedback_config";
    private const string RollFeedbackConfigOverrideFileName = "roll_feedback_config.csv";
    private const string DiceTypeIconResourcePrefix = "Art/DiceTypes/";
    private const string RuntimeDieFaceBaseResourcePath = "Art/DiceFaces/runtime_die_face_base";
    private const string DiceRollReadySpriteResourcePath = "Art/DiceRoll/f009_unified_ready_die_256";
    private const string DiceRollLoopStripResourcePath = "Art/DiceRoll/f009_unified_spin_loop_strip_24f_256";
    private const string DiceRollStopStripResourcePath = "Art/DiceRoll/f009_unified_spin_stop_strip_8f_256";
    private const string DiceRollResultFacesResourcePath = "Art/DiceFaces/f009_unified_result_die_faces_6x256";
    private const string DiceRollLoopStripFallbackResourcePath = "Art/DiceRoll/f009_table_friction_spin_loop_strip_24f_256";
    private const string DiceRollStopStripFallbackResourcePath = "Art/DiceRoll/f009_table_friction_spin_stop_strip_8f_256";
    private const string DiceMaterialShaderResourcePath = "Shaders/DiceMaterialOverlay";
    private const string UiResourcePrefix = "Art/UI/";
    private const string TooltipUiResourcePrefix = "Art/UI/Tooltip/";
    private const string ItemResourcePrefix = "Art/Items/";
    private const float TooltipHoverDelay = 0.12f;
    private const float TooltipFadeInDuration = 0.10f;
    private const float TooltipFadeOutDuration = 0.08f;
    private const float TooltipContentSwapDuration = 0.06f;
    private const float TooltipEnterOffsetY = 6f;
    private const float TooltipEnterScaleFrom = 0.98f;
    private const float TooltipPanelWidth = 336f;
    private const float TooltipPanelHeight = 400f;

    private static int nextDieId = 1;

    private readonly List<Die> dice = new List<Die>();
    private readonly List<Die> scoringDice = new List<Die>();
    private readonly List<RunScoreCounterStep> runScoreCounterSteps = new List<RunScoreCounterStep>();
    private readonly List<SettlementDisplayEvent> settlementDisplayEvents = new List<SettlementDisplayEvent>();
    private readonly List<DiceVisualState> diceVisualStates = new List<DiceVisualState>();
    private readonly List<Die> pendingTreeGrowth = new List<Die>();
    private readonly List<int> cheatRerollIds = new List<int>();
    private readonly List<Encounter> encounters = new List<Encounter>();
    private readonly List<MarketOffer> marketOffers = new List<MarketOffer>();
    private readonly List<string> logLines = new List<string>();
    private readonly Dictionary<DieType, MarketDieConfig> marketDieConfigs = new Dictionary<DieType, MarketDieConfig>();
    private readonly Dictionary<DiceMaterial, DiceMaterialConfig> diceMaterialConfigs = new Dictionary<DiceMaterial, DiceMaterialConfig>();
    private readonly Dictionary<DieType, DiceTypeTooltipConfig> diceTypeTooltipConfigs = new Dictionary<DieType, DiceTypeTooltipConfig>();
    private readonly Dictionary<string, AffixTierConfig> affixTierConfigs = new Dictionary<string, AffixTierConfig>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, AffixDefinition> affixDefinitions = new Dictionary<string, AffixDefinition>(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, CraftingItemDefinition> craftingItemDefinitions = new Dictionary<string, CraftingItemDefinition>(StringComparer.OrdinalIgnoreCase);
    private readonly List<FaceTemplateConfig> faceTemplateConfigs = new List<FaceTemplateConfig>();
    private readonly List<MarketRuleConfig> marketRuleConfigs = new List<MarketRuleConfig>();
    private readonly Dictionary<DieType, Texture2D> dieTypeIconTextures = new Dictionary<DieType, Texture2D>();
    private readonly Dictionary<DiceMaterial, UnityEngine.Material> diceMaterialRenderMaterials = new Dictionary<DiceMaterial, UnityEngine.Material>();

    private GameMode mode = GameMode.MainMenu;
    private GameMode settingsReturnMode = GameMode.MainMenu;
    private RollPhase rollPhase = RollPhase.Ready;
    private int selectedReadySlotIndex = -1;
    private bool diceProcessVisualsEnabled = true;
    private float diceVisualEnterStartTime = -999f;
    private float diceVisualRollStartTime = -999f;
    private float diceVisualRevealStartTime = -999f;
    private float diceVisualImpulseTimer;

    private Texture2D whiteTexture;
    private Texture2D pipTexture;
    private Texture2D tableBackgroundTexture;
    private Texture2D diceCupTexture;
    private Texture2D runtimeDieFaceBaseTexture;
    private Texture2D diceRollReadyTexture;
    private Texture2D diceRollLoopStripTexture;
    private Texture2D diceRollStopStripTexture;
    private Texture2D diceRollResultFacesTexture;
    private Texture2D uiIconCoinTexture;
    private Texture2D uiIconRefreshTexture;
    private Texture2D uiIconSettingsTexture;
    private Texture2D uiIconCloseTexture;
    private Texture2D uiIconTargetTexture;
    private Texture2D uiIconSellTexture;
    private Texture2D uiIconBagTexture;
    private Texture2D tooltipPanelTexture;
    private Texture2D tooltipPriceChipTexture;
    private Texture2D tooltipLabelChipBlueTexture;
    private Texture2D tooltipLabelChipGreenTexture;
    private Texture2D tooltipFaceCellTexture;
    private Texture2D affixAddStoneTexture;
    private Texture2D affixRemoveStoneTexture;
    private Texture2D affixReplaceStoneTexture;
    private Shader diceMaterialOverlayShader;
    private GUIStyle titleStyle;
    private GUIStyle hudTitleStyle;
    private GUIStyle hudHeaderStyle;
    private GUIStyle hudBodyStyle;
    private GUIStyle headerStyle;
    private GUIStyle bodyStyle;
    private GUIStyle smallStyle;
    private GUIStyle tinyStyle;
    private GUIStyle buttonStyle;
    private GUIStyle selectedButtonStyle;
    private GUIStyle artButtonStyle;
    private GUIStyle disabledButtonLabelStyle;
    private GUIStyle cardTitleStyle;
    private GUIStyle centerStyle;
    private GUIStyle phaseRibbonStyle;
    private GUIStyle tooltipTitleStyle;
    private GUIStyle tooltipLabelStyle;
    private GUIStyle tooltipBodyStyle;
    private GUIStyle tooltipTinyStyle;
    private Font uiFont;

    private int stageIndex;
    private int chapterGold;
    private int rollsLeft;
    private int cheatsLeft;
    private int currentScore;
    private int resolvedScore;
    private int scoreRevealIndex;
    private int settlementEventIndex;
    private int lastStageFlatIncome;
    private int lastStageInterestIncome;
    private int lastStageCompoundInterestIncome;
    private int lastStageIncome;
    private int lastComboBonus;
    private int lastTemporaryScore;
    private int previewRollScore;
    private int previewIndividualScore;
    private int previewTemporaryScore;
    private int previewTurtleTemporaryDieCount;
    private int previewNestBonusDieCount;
    private int previewShellsmithScoreBonus;
    private int previewRuleBonus;
    private int previewBribeGoldCost;
    private int previewBribeScoreBonus;
    private int previewAffixScoreBonus;
    private int previewWalletIncome;
    private int stageStartGold;
    private int stageInvestmentGold;
    private int lastBribeGoldSpent;
    private int lastBribeScoreBonus;
    private int lastAffixScoreBonus;
    private int lastWalletIncome;
    private int committedCounterScoreBeforeRoll;
    private int committedCounterBaseScore;
    private int committedCounterMultipliedScore;
    private int committedCounterBribeScore;
    private int committedCounterRollScore;
    private int committedCounterResolvedScore;
    private int animatedCounterBaseScore;
    private int animatedCounterMultipliedScore;
    private int animatedCounterBribeScore;
    private int animatedCounterRollScore;
    private int animatedCounterProgressScore;
    private int marketRefreshesWithoutHighTier;
    private int marketTendencySlotIndex = -1;
    private MarketRoute marketRecentPurchaseRoute = MarketRoute.None;
    private int lastTurtleTemporaryDieCount;
    private int lastNestBonusDieCount;
    private int lastShellsmithScoreBonus;
    private int selectedMarketDieIndex = -1;
    private bool currentMarketIsChapter;
    private bool marketTestRandomRefresh;
    private bool affixFeatureEnabled;
    private string activeCraftingItemKey = string.Empty;
    private int affixAddStoneCount;
    private int affixRemoveStoneCount;
    private int affixReplaceStoneCount;
    private int startingGold = 18;
    private int stageClearBaseGold;
    private int rollLeftGoldBonus;
    private int cheatLeftGoldBonus;
    private int interestGoldStep = 5;
    private int interestGoldPerStep = 1;
    private int interestGoldCap = 5;
    private int piggyGoldPerHit = 1;
    private int bountyGoldPerHit = 3;
    private int topGoldPerHit = 1;
    private int handTaxLowGold = 1;
    private int handTaxHighGold = 2;
    private int collectionGoldPerStage = 1;
    private int compoundInterestPerDieCap = 2;
    private int compoundInterestTotalCap = 4;
    private int leadTicketGold = 2;
    private int shellTaxThreshold = 3;
    private int shellTaxGold = 2;
    private int counterGold = 1;
    private int lumberGold = 2;
    private int lumberScorePenalty = 1;
    private int treasuryGoldStep = 10;
    private int treasuryScoreCap;
    private int bribeScorePerGold = 4;
    private int bribeGoldCapPerDie = 2;
    private int investmentGoldCapPerDie = 2;
    private int investmentWalletDivisor = 3;
    private int investmentScorePerGold = 2;
    private float lastMultiplier = 1f;
    private float committedCounterMultiplier = 1f;
    private float animatedCounterMultiplier = 1f;
    private float shakeTimer;
    private float shakePower;
    private float stopTimer;
    private float stopStartPower;
    private float lastShakeTapTime;
    private float shakeExpiredPromptTimer;
    private float promptPulseTimer;
    private float scoreStepTimer;
    private float counterBasePulseTimer;
    private float counterMultiplierPulseTimer;
    private float counterProgressPulseTimer;
    private float openingTimer;
    private float settingsVolume = 1f;
    private int shakeImpulseCount;
    private RollFeedbackConfig rollFeedbackConfig = RollFeedbackConfig.CreateDefault();
    private RollFeedbackConfig activeRollFeedbackConfig = RollFeedbackConfig.CreateDefault();
    private string rollFeedbackConfigSource = "代码安全默认值";
    private bool passed;
    private bool dieTypeIconsLoaded;
    private bool rolledThisEncounter;
    private bool finalScoreApplied;
    private bool confirmNewGame;
    private bool hasSave;
    private bool seenOpening;
    private bool windowed = true;
    private bool suppressSave;
    private bool previewHasTurtleRandomness;
    private bool committedCounterValid;
    private bool runScoreCounterAnimationActive;
    private bool showHandReference;
    private string rewardBanner = string.Empty;
    private string buildName = string.Empty;
    private string buildSummary = string.Empty;
    private string lastHandName = "无牌型";
    private SettlementDisplayEvent activeSettlementEvent;
    private DiceTooltipState tooltipState = DiceTooltipState.Hidden;
    private Vector2 uiMousePosition;
    private bool uiMousePositionValid;
    private string hoverCandidateKey = string.Empty;
    private Die hoverCandidateDie;
    private Rect hoverCandidateRect;
    private bool hoverCandidateAllowCurrentStateText;
    private string activeTooltipKey = string.Empty;
    private Die activeTooltipDie;
    private Rect activeTooltipRect;
    private bool activeTooltipAllowCurrentStateText;
    private float hoverCandidateStartedAt = -999f;
    private float tooltipVisibleStartedAt = -999f;
    private float tooltipHideStartedAt = -999f;
    private float tooltipContentSwapStartedAt = -999f;
    private float tooltipAlpha;
    private float tooltipFadeOutStartAlpha;

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

        LoadGlobalConfigs();
        LoadRollFeedbackConfig(false);
        LoadMarketConfigs();
        LoadDiceTypeTooltipConfigs();
        LoadAffixConfigs();
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

        if (Input.GetKeyDown(KeyCode.F5))
        {
            LoadRollFeedbackConfig(true);
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            diceProcessVisualsEnabled = !diceProcessVisualsEnabled;
            Debug.Log("DiceKingDemo: F009 dice process visuals " + (diceProcessVisualsEnabled ? "enabled" : "disabled") + ".");
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

        float scale = Mathf.Min(Screen.width / DesignWidth, Screen.height / DesignHeight);
        if (scale <= 0f)
        {
            scale = 1f;
        }

        float offsetX = (Screen.width - DesignWidth * scale) * 0.5f;
        float offsetY = (Screen.height - DesignHeight * scale) * 0.5f;
        UpdateUiMousePosition(scale, offsetX, offsetY);
        BeginDiceHoverTooltipFrame();
        Matrix4x4 oldMatrix = GUI.matrix;
        Matrix4x4 fitMatrix = Matrix4x4.TRS(new Vector3(offsetX, offsetY, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
        Matrix4x4 prototypeMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(PrototypeToDesignScale, PrototypeToDesignScale, 1f));
        GUI.matrix = fitMatrix * prototypeMatrix;

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
            case GameMode.InterStageMarket:
                DrawMarket(false);
                break;
            case GameMode.ChapterShop:
                DrawMarket(true);
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

        UpdateDiceHoverTooltipState();
        DrawDiceHoverTooltip();

        GUI.matrix = oldMatrix;
    }

    private void DrawMainMenu()
    {
        DrawHudBar("骰子王", string.Empty, string.Empty);

        Rect menu = new Rect(382f, 148f, 516f, 430f);
        DrawUiPanel(menu);
        GUI.Label(new Rect(menu.x + 58f, menu.y + 62f, 330f, 34f), "主菜单", headerStyle);
        GUI.Label(new Rect(menu.x + 58f, menu.y + 98f, 390f, 44f), "六颗骰子，一份会越写越离谱的王室账本。", smallStyle);

        if (DrawUiButton(new Rect(menu.x + 58f, menu.y + 158f, 388f, 56f), confirmNewGame && hasSave ? "确认覆盖并开始" : "开始新游戏", UiButtonKind.Primary))
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
        if (DrawUiButton(new Rect(menu.x + 58f, menu.y + 224f, 388f, 52f), hasSave ? "继续游戏" : "继续游戏（无存档）", UiButtonKind.Secondary))
        {
            ContinueGameFlow();
        }
        GUI.enabled = true;

        if (DrawUiButton(new Rect(menu.x + 58f, menu.y + 286f, 388f, 52f), "设置", UiButtonKind.Secondary))
        {
            settingsReturnMode = GameMode.MainMenu;
            mode = GameMode.Settings;
        }

        if (DrawUiButton(new Rect(menu.x + 58f, menu.y + 348f, 388f, 52f), "退出游戏", UiButtonKind.Danger))
        {
            Application.Quit();
        }

        if (confirmNewGame && hasSave)
        {
            GUI.Label(new Rect(menu.x + 58f, menu.y + 402f, 388f, 24f), "再次点击开始会覆盖当前存档。", tinyStyle);
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
        GUI.Label(new Rect(0f, 320f, VirtualWidth, 46f), "有人把六颗骰子推到了你面前。", centerStyle);
        GUI.color = Color.white;
    }

    private void DrawSettings()
    {
        DrawHudBar("设置", string.Empty, string.Empty);
        DrawUiPanel(new Rect(320f, 150f, 640f, 420f));

        GUI.Label(new Rect(370f, 205f, 160f, 28f), "音量", headerStyle);
        settingsVolume = GUI.HorizontalSlider(new Rect(530f, 214f, 300f, 24f), settingsVolume, 0f, 1f);
        GUI.Label(new Rect(850f, 204f, 70f, 28f), Mathf.RoundToInt(settingsVolume * 100f) + "%", smallStyle);

        windowed = GUI.Toggle(new Rect(370f, 270f, 260f, 30f), windowed, "窗口模式");
        GUI.Label(new Rect(370f, 306f, 520f, 24f), "画面：原型默认布局", smallStyle);
        GUI.Label(new Rect(370f, 346f, 520f, 76f), "操作说明：按 Space 开始旋骰，加力窗口内连续敲 Space 只增强表现；窗口结束后按 Space 只提示停转显点。结果锁定后按 Space 结算，或出千选择最多 3 颗实体骰点数 +1，最高不超过骰面上限；出千确认前可取消。", smallStyle);

        if (DrawUiButton(new Rect(370f, 448f, 206f, 52f), "保存并返回", UiButtonKind.Primary))
        {
            SaveMenuState();
            Screen.fullScreen = !windowed;
            mode = settingsReturnMode;
        }
    }

    private void DrawRun()
    {
        Encounter encounter = CurrentEncounter();
        if (encounter == null)
        {
            mode = GameMode.Win;
            return;
        }

        DrawHudBar("骰子王", EncounterTitle(encounter), string.Empty);

        DrawRunLeftRail(new Rect(32f, 116f, 196f, 488f), encounter);
        DrawRunPlayPanel(new Rect(260f, 112f, 708f, 500f), encounter);
        DrawRunSupportPanel(new Rect(1000f, 112f, 212f, 210f), encounter);
        DrawRunActionBar(new Rect(470f, 650f, 340f, 46f), encounter);

        GUI.enabled = true;

        if (showHandReference)
        {
            DrawHandReferenceOverlay();
        }
    }

    private void DrawRunLeftRail(Rect rect, Encounter encounter)
    {
        DrawRunScoreTower(new Rect(rect.x, rect.y, rect.width, 246f), encounter);

        float resourceY = rect.y + 284f;
        DrawRunResourceCard(new Rect(rect.x, resourceY, 152f, 52f), RunUiIcon.Coin, chapterGold.ToString(CultureInfo.InvariantCulture));
        DrawRunResourceCard(new Rect(rect.x, resourceY + 64f, 152f, 52f), RunUiIcon.Roll, rollsLeft + " / " + RollsPerStage);
        DrawRunResourceCard(new Rect(rect.x, resourceY + 128f, 152f, 52f), RunUiIcon.Cheat, cheatsLeft + " / " + CheatsPerStage);
    }

    private void DrawRunResourceCard(Rect rect, RunUiIcon icon, string value)
    {
        DrawUiSmallPanel(rect);
        DrawRunUiIcon(new Rect(rect.x + 13f, rect.y + 12f, 28f, 28f), icon);
        DrawRunText(new Rect(rect.x + 54f, rect.y + 4f, rect.width - 64f, rect.height - 8f), value, 23, FontStyle.Bold, new Color(0.2f, 0.14f, 0.09f), TextAnchor.MiddleLeft);
    }

    private void DrawRunScoreTower(Rect rect, Encounter encounter)
    {
        RunScoreCounterState counterState = BuildRunScoreCounterState(encounter);

        DrawUiSidePanel(rect);

        DrawScoreLedgerRow(new Rect(rect.x + 14f, rect.y + 16f, rect.width - 28f, 54f), RunUiIcon.TargetScore, counterState.TargetScore.ToString(CultureInfo.InvariantCulture), new Color(0.66f, 0.25f, 0.18f));
        DrawScoreLedgerRow(new Rect(rect.x + 14f, rect.y + 82f, rect.width - 28f, 54f), RunUiIcon.CurrentScore, counterState.ProgressScore.ToString(CultureInfo.InvariantCulture), new Color(0.23f, 0.17f, 0.11f), CounterPulseAmount(counterProgressPulseTimer));

        Rect counter = new Rect(rect.x + 14f, rect.y + 154f, rect.width - 28f, 70f);
        DrawRect(new Rect(counter.x + 3f, counter.y + 4f, counter.width, counter.height), new Color(0.1f, 0.06f, 0.03f, 0.18f));
        DrawRect(counter, new Color(1f, 0.88f, 0.55f, 0.86f));
        DrawBorder(counter, new Color(0.62f, 0.38f, 0.13f, 0.88f), 2f);
        DrawBorder(new Rect(counter.x + 6f, counter.y + 6f, counter.width - 12f, counter.height - 12f), new Color(1f, 0.96f, 0.75f, 0.42f), 1f);

        Rect baseRect = new Rect(counter.x + 10f, counter.y + 4f, 82f, counter.height - 8f);
        Rect split = new Rect(counter.x + 98f, counter.y + 13f, 2f, counter.height - 26f);
        Rect multiplierRect = new Rect(counter.x + 106f, counter.y + 7f, counter.width - 116f, counter.height - 14f);
        DrawRect(split, new Color(0.38f, 0.24f, 0.13f, 0.72f));
        DrawRunText(baseRect, counterState.BaseScore.ToString(CultureInfo.InvariantCulture), 42 + Mathf.RoundToInt(5f * CounterPulseAmount(counterBasePulseTimer)), FontStyle.Bold, new Color(0.13f, 0.09f, 0.07f), TextAnchor.MiddleCenter);
        DrawMultiplierBadge(multiplierRect, "×" + MultiplierText(counterState.Multiplier), CounterPulseAmount(counterMultiplierPulseTimer));
    }

    private void DrawScoreLedgerRow(Rect rect, RunUiIcon icon, string value, Color valueColor, float pulse = 0f)
    {
        DrawUiSmallPanel(rect);
        DrawRunUiIcon(new Rect(rect.x + 10f, rect.y + 11f, 30f, 30f), icon);
        DrawRunText(new Rect(rect.x + 52f, rect.y + 4f, rect.width - 62f, rect.height - 8f), value, 30 + Mathf.RoundToInt(4f * Mathf.Clamp01(pulse)), FontStyle.Bold, valueColor, TextAnchor.MiddleLeft);
    }

    private RunScoreCounterState BuildRunScoreCounterState(Encounter encounter)
    {
        RunScoreCounterState state = new RunScoreCounterState
        {
            Phase = RunScoreCounterPhase.Idle,
            ScoreBeforeRoll = currentScore,
            ProgressScore = currentScore,
            TargetScore = encounter != null ? encounter.Target : 0,
            BaseScore = 0,
            Multiplier = 1f,
            MultipliedScore = 0,
            BribeScore = 0,
            RollScore = 0,
            ResolvedScore = currentScore
        };

        if (rollPhase == RollPhase.ResultDecision || rollPhase == RollPhase.CheatEdit)
        {
            int bribeScore = Mathf.Max(0, previewBribeScoreBonus);
            int rollScore = Mathf.Max(0, previewRollScore);

            state.Phase = RunScoreCounterPhase.Preview;
            state.ScoreBeforeRoll = currentScore;
            state.BaseScore = 0;
            state.Multiplier = 1f;
            state.MultipliedScore = 0;
            state.BribeScore = bribeScore;
            state.RollScore = rollScore;
            state.ResolvedScore = currentScore + rollScore;
            state.ProgressScore = currentScore;
            return state;
        }

        if (rollPhase == RollPhase.Scoring)
        {
            state.Phase = RunScoreCounterPhase.Settling;
            PopulateCommittedRunScoreCounterState(state);
            if (runScoreCounterAnimationActive)
            {
                state.BaseScore = animatedCounterBaseScore;
                state.Multiplier = animatedCounterMultiplier;
                state.MultipliedScore = animatedCounterMultipliedScore;
                state.BribeScore = animatedCounterBribeScore;
                state.RollScore = animatedCounterRollScore;
                state.ProgressScore = animatedCounterProgressScore;
            }
            else
            {
                state.ProgressScore = currentScore;
            }

            return state;
        }

        if (rollPhase == RollPhase.StageClear || rollPhase == RollPhase.StageFailed)
        {
            state.Phase = RunScoreCounterPhase.Settled;
            PopulateCommittedRunScoreCounterState(state);
            state.ProgressScore = currentScore;
            return state;
        }

        if (rollPhase == RollPhase.Ready && committedCounterValid)
        {
            state.Phase = RunScoreCounterPhase.Settled;
            PopulateCommittedRunScoreCounterState(state);
            state.ProgressScore = currentScore;
            return state;
        }

        return state;
    }

    private void PopulateCommittedRunScoreCounterState(RunScoreCounterState state)
    {
        if (committedCounterValid)
        {
            state.ScoreBeforeRoll = committedCounterScoreBeforeRoll;
            state.BaseScore = committedCounterBaseScore;
            state.Multiplier = committedCounterMultiplier;
            state.MultipliedScore = committedCounterMultipliedScore;
            state.BribeScore = committedCounterBribeScore;
            state.RollScore = committedCounterRollScore;
            state.ResolvedScore = committedCounterResolvedScore;
            return;
        }

        int baseScore = CurrentCounterBaseScore();
        float multiplier = Mathf.Max(1f, lastMultiplier);
        int rollScore = Mathf.Max(0, previewRollScore);
        state.ScoreBeforeRoll = Mathf.Max(0, resolvedScore - rollScore);
        state.BaseScore = baseScore;
        state.Multiplier = multiplier;
        state.MultipliedScore = Mathf.RoundToInt(baseScore * multiplier);
        state.BribeScore = Mathf.Max(0, lastBribeScoreBonus);
        state.RollScore = rollScore;
        state.ResolvedScore = resolvedScore > 0 ? resolvedScore : currentScore;
    }

    private int CurrentCounterBaseScore()
    {
        return Mathf.Max(0, previewIndividualScore + previewTemporaryScore + previewRuleBonus);
    }

    private void ClearCommittedRunScoreCounter()
    {
        committedCounterValid = false;
        committedCounterScoreBeforeRoll = 0;
        committedCounterBaseScore = 0;
        committedCounterMultiplier = 1f;
        committedCounterMultipliedScore = 0;
        committedCounterBribeScore = 0;
        committedCounterRollScore = 0;
        committedCounterResolvedScore = 0;
        ClearRunScoreCounterAnimation();
    }

    private void CaptureCommittedRunScoreCounter(int scoreBeforeRoll, int rollScore, int resolved)
    {
        int baseScore = CurrentCounterBaseScore();
        float multiplier = Mathf.Max(1f, lastMultiplier);

        committedCounterValid = true;
        committedCounterScoreBeforeRoll = Mathf.Max(0, scoreBeforeRoll);
        committedCounterBaseScore = baseScore;
        committedCounterMultiplier = multiplier;
        committedCounterMultipliedScore = Mathf.RoundToInt(baseScore * multiplier);
        committedCounterBribeScore = Mathf.Max(0, lastBribeScoreBonus);
        committedCounterRollScore = Mathf.Max(0, rollScore);
        committedCounterResolvedScore = Mathf.Max(0, resolved);
    }

    private void ClearRunScoreCounterAnimation()
    {
        runScoreCounterSteps.Clear();
        ClearSettlementDisplayEvents();
        runScoreCounterAnimationActive = false;
        animatedCounterBaseScore = 0;
        animatedCounterMultiplier = 1f;
        animatedCounterMultipliedScore = 0;
        animatedCounterBribeScore = 0;
        animatedCounterRollScore = 0;
        animatedCounterProgressScore = currentScore;
        counterBasePulseTimer = 0f;
        counterMultiplierPulseTimer = 0f;
        counterProgressPulseTimer = 0f;
    }

    private void PrepareRunScoreCounterAnimation(int scoreBeforeRoll)
    {
        runScoreCounterSteps.Clear();
        runScoreCounterAnimationActive = true;
        animatedCounterBaseScore = 0;
        animatedCounterMultiplier = 1f;
        animatedCounterMultipliedScore = 0;
        animatedCounterBribeScore = 0;
        animatedCounterRollScore = 0;
        animatedCounterProgressScore = Mathf.Max(0, scoreBeforeRoll);
        counterBasePulseTimer = 0f;
        counterMultiplierPulseTimer = 0f;
        counterProgressPulseTimer = 0f;

        int runningBaseScore = 0;
        for (int i = 0; i < scoringDice.Count; i++)
        {
            Die die = scoringDice[i];
            if (die != null)
            {
                runningBaseScore += Mathf.Max(0, die.Score);
            }

            if (i == scoringDice.Count - 1)
            {
                runningBaseScore = committedCounterValid ? committedCounterBaseScore : runningBaseScore;
            }

            float multiplier = DisplayMultiplierForScorePrefix(i);
            int multipliedScore = Mathf.RoundToInt(runningBaseScore * multiplier);
            runScoreCounterSteps.Add(new RunScoreCounterStep
            {
                BaseScore = runningBaseScore,
                Multiplier = multiplier,
                MultipliedScore = multipliedScore,
                BribeScore = 0,
                RollScore = multipliedScore,
                ProgressScore = Mathf.Max(0, scoreBeforeRoll + multipliedScore)
            });
        }
    }

    private void ClearSettlementDisplayEvents()
    {
        settlementDisplayEvents.Clear();
        activeSettlementEvent = null;
        settlementEventIndex = 0;
    }

    private void PrepareSettlementDisplayEvents(int scoreBeforeRoll)
    {
        ClearSettlementDisplayEvents();

        int shownTemporaryDice = 0;
        int hiddenTemporaryStart = -1;
        int hiddenTemporaryEnd = -1;
        int hiddenTemporaryCount = 0;
        int hiddenTemporaryScore = 0;
        float previousMultiplier = 1f;

        for (int i = 0; i < scoringDice.Count; i++)
        {
            Die die = scoringDice[i];
            if (die == null)
            {
                continue;
            }

            if (die.Temporary && shownTemporaryDice >= TemporaryDiceDisplayLimit)
            {
                if (hiddenTemporaryStart < 0)
                {
                    hiddenTemporaryStart = i;
                }

                hiddenTemporaryEnd = i;
                hiddenTemporaryCount++;
                hiddenTemporaryScore += Mathf.Max(0, die.Score);
                continue;
            }

            AddSettlementSlotEvent(i, i, die, false, Mathf.Max(0, die.Score), SettlementSlotLabelForDie(die));
            AddSettlementRouteEventIfNeeded(die, i);
            AddSettlementMultiplierEventIfNeeded(i, die, ref previousMultiplier);

            if (die.Temporary)
            {
                shownTemporaryDice++;
            }
        }

        if (hiddenTemporaryCount > 0)
        {
            AddSettlementSlotEvent(
                hiddenTemporaryStart,
                hiddenTemporaryEnd,
                null,
                true,
                hiddenTemporaryScore,
                "余骰 x" + hiddenTemporaryCount + " +" + hiddenTemporaryScore);
            AddSettlementMultiplierEventIfNeeded(hiddenTemporaryEnd, null, ref previousMultiplier);
        }

        AddSettlementFinalEvents();
    }

    private void AddSettlementSlotEvent(int scoreIndexStart, int scoreIndexEnd, Die die, bool summary, int valueDelta, string label)
    {
        RunScoreCounterStep step = SettlementCounterStepAt(scoreIndexEnd);
        SettlementDisplayEvent settlementEvent = new SettlementDisplayEvent
        {
            Kind = SettlementEventKind.SlotScore,
            SlotIndex = die != null && !die.Temporary ? dice.IndexOf(die) : -1,
            ScoreIndex = scoreIndexStart,
            ScoreIndexEnd = scoreIndexEnd,
            DieId = die != null ? die.Id : 0,
            Label = label,
            ValueDelta = Mathf.Max(0, valueDelta),
            Duration = summary ? SettlementRouteDuration : SettlementSlotDuration,
            HighlightLevel = SettlementHighlightLevel.Normal,
            TargetArea = SettlementTargetArea.Dice,
            CounterStepIndex = scoreIndexEnd,
            ApplyCounterStep = true
        };

        if (step != null)
        {
            settlementEvent.BaseScore = step.BaseScore;
            settlementEvent.Multiplier = step.Multiplier;
            settlementEvent.ProgressScore = step.ProgressScore;
        }

        settlementDisplayEvents.Add(settlementEvent);
    }

    private void AddSettlementRouteEventIfNeeded(Die die, int scoreIndex)
    {
        string label = SettlementRouteLabelForDie(die);
        if (string.IsNullOrEmpty(label))
        {
            return;
        }

        RunScoreCounterStep step = SettlementCounterStepAt(scoreIndex);
        int goldDelta = SettlementGoldDeltaFromLabel(label);
        SettlementDisplayEvent settlementEvent = new SettlementDisplayEvent
        {
            Kind = SettlementEventKind.RouteHighlight,
            SlotIndex = die != null && !die.Temporary ? dice.IndexOf(die) : -1,
            ScoreIndex = scoreIndex,
            ScoreIndexEnd = scoreIndex,
            DieId = die != null ? die.Id : 0,
            Label = label,
            GoldDelta = goldDelta,
            Duration = SettlementRouteDuration,
            HighlightLevel = SettlementHighlightLevel.Route,
            TargetArea = goldDelta > 0 ? SettlementTargetArea.Coin : SettlementTargetArea.Dice
        };

        if (step != null)
        {
            settlementEvent.BaseScore = step.BaseScore;
            settlementEvent.Multiplier = step.Multiplier;
            settlementEvent.ProgressScore = step.ProgressScore;
        }

        settlementDisplayEvents.Add(settlementEvent);
    }

    private void AddSettlementMultiplierEventIfNeeded(int scoreIndex, Die die, ref float previousMultiplier)
    {
        RunScoreCounterStep step = SettlementCounterStepAt(scoreIndex);
        if (step == null)
        {
            return;
        }

        if (step.Multiplier <= previousMultiplier || Mathf.Approximately(step.Multiplier, previousMultiplier))
        {
            previousMultiplier = step.Multiplier;
            return;
        }

        SettlementDisplayEvent settlementEvent = new SettlementDisplayEvent
        {
            Kind = SettlementEventKind.MultiplierStamp,
            SlotIndex = die != null && !die.Temporary ? dice.IndexOf(die) : -1,
            ScoreIndex = scoreIndex,
            ScoreIndexEnd = scoreIndex,
            DieId = die != null ? die.Id : 0,
            Label = "倍率盖章 ×" + MultiplierText(step.Multiplier),
            BaseScore = step.BaseScore,
            Multiplier = step.Multiplier,
            ProgressScore = step.ProgressScore,
            Duration = SettlementMultiplierDuration,
            HighlightLevel = SettlementHighlightLevel.Multiplier,
            TargetArea = SettlementTargetArea.Multiplier
        };

        settlementDisplayEvents.Add(settlementEvent);
        previousMultiplier = step.Multiplier;
    }

    private void AddSettlementFinalEvents()
    {
        Encounter encounter = CurrentEncounter();
        bool willPass = encounter != null && resolvedScore >= encounter.Target;
        bool bribeEventAppliesFinal = lastBribeScoreBonus > 0;

        if (bribeEventAppliesFinal)
        {
            string bribeLabel = "贿赂 +" + lastBribeScoreBonus;
            if (lastBribeGoldSpent > 0)
            {
                bribeLabel += " / -" + lastBribeGoldSpent + " 金";
            }

            settlementDisplayEvents.Add(new SettlementDisplayEvent
            {
                Kind = SettlementEventKind.BribeFinal,
                ScoreIndex = scoringDice.Count,
                ScoreIndexEnd = scoringDice.Count,
                Label = bribeLabel,
                ValueDelta = Mathf.Max(0, lastBribeScoreBonus),
                GoldDelta = -Mathf.Max(0, lastBribeGoldSpent),
                BaseScore = committedCounterBaseScore,
                Multiplier = committedCounterMultiplier,
                ProgressScore = committedCounterResolvedScore,
                Duration = SettlementFinalDuration,
                HighlightLevel = SettlementHighlightLevel.Route,
                TargetArea = SettlementTargetArea.Target,
                ApplyFinal = true
            });
        }

        settlementDisplayEvents.Add(new SettlementDisplayEvent
        {
            Kind = SettlementEventKind.TargetSettle,
            ScoreIndex = scoringDice.Count,
            ScoreIndexEnd = scoringDice.Count,
            Label = SettlementTargetLabel(encounter, willPass),
            BaseScore = committedCounterBaseScore,
            Multiplier = committedCounterMultiplier,
            ProgressScore = committedCounterResolvedScore,
            Duration = SettlementTargetDuration,
            HighlightLevel = SettlementHighlightLevel.Target,
            TargetArea = SettlementTargetArea.Target,
            ApplyFinal = !bribeEventAppliesFinal,
            Passed = willPass
        });
    }

    private RunScoreCounterStep SettlementCounterStepAt(int scoreIndex)
    {
        if (scoreIndex < 0 || scoreIndex >= runScoreCounterSteps.Count)
        {
            return null;
        }

        return runScoreCounterSteps[scoreIndex];
    }

    private string SettlementSlotLabelForDie(Die die)
    {
        if (die == null)
        {
            return string.Empty;
        }

        if (die.Temporary)
        {
            return "小骰 +" + Mathf.Max(0, die.Score);
        }

        int slotIndex = dice.IndexOf(die);
        return "槽 " + (slotIndex + 1) + " +" + Mathf.Max(0, die.Score);
    }

    private string SettlementRouteLabelForDie(Die die)
    {
        if (die == null || die.Temporary || string.IsNullOrEmpty(die.RoundNote))
        {
            return string.Empty;
        }

        string note = die.RoundNote;
        string label = ExtractSettlementNote(note, "猪猪");
        if (IsActionableSettlementRouteLabel(label))
        {
            return label;
        }

        label = ExtractSettlementNote(note, "鎏印");
        if (IsActionableSettlementRouteLabel(label))
        {
            return label;
        }

        label = ExtractSettlementNote(note, "金");
        if (IsActionableSettlementRouteLabel(label))
        {
            return label;
        }

        string[] priorityMarkers =
        {
            "盖章",
            "轨道",
            "半步",
            "孤证",
            "双倍",
            "赌徒爆发",
            "国库",
            "投资",
            "壳匠",
            "大树命中",
            "灌溉",
            "园丁"
        };

        for (int i = 0; i < priorityMarkers.Length; i++)
        {
            label = ExtractSettlementNote(note, priorityMarkers[i]);
            if (IsActionableSettlementRouteLabel(label))
            {
                return label;
            }
        }

        return string.Empty;
    }

    private string ExtractSettlementNote(string note, string marker)
    {
        if (string.IsNullOrEmpty(note) || string.IsNullOrEmpty(marker))
        {
            return string.Empty;
        }

        string fallback = string.Empty;
        string[] parts = note.Split(new char[] { '，', '；', ';', '/', '、' }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < parts.Length; i++)
        {
            string part = parts[i].Trim();
            if (part.IndexOf(marker, StringComparison.Ordinal) >= 0)
            {
                if (SettlementNoteLooksActionable(part))
                {
                    return part;
                }

                if (string.IsNullOrEmpty(fallback))
                {
                    fallback = part;
                }
            }
        }

        if (!string.IsNullOrEmpty(fallback))
        {
            return fallback;
        }

        if (note.IndexOf(marker, StringComparison.Ordinal) >= 0 && SettlementNoteLooksActionable(note))
        {
            return note;
        }

        return string.Empty;
    }

    private bool SettlementNoteLooksActionable(string note)
    {
        if (string.IsNullOrEmpty(note))
        {
            return false;
        }

        return note.IndexOf("+", StringComparison.Ordinal) >= 0
            || note.IndexOf("金", StringComparison.Ordinal) >= 0
            || note.IndexOf("分", StringComparison.Ordinal) >= 0
            || note.IndexOf("命中", StringComparison.Ordinal) >= 0
            || note.IndexOf("借位", StringComparison.Ordinal) >= 0
            || note.IndexOf("爆发", StringComparison.Ordinal) >= 0
            || note.IndexOf("成长", StringComparison.Ordinal) >= 0
            || note.IndexOf("双倍", StringComparison.Ordinal) >= 0
            || note.IndexOf("→", StringComparison.Ordinal) >= 0
            || note.StartsWith("灌溉 ", StringComparison.Ordinal);
    }

    private bool IsActionableSettlementRouteLabel(string label)
    {
        if (string.IsNullOrEmpty(label))
        {
            return false;
        }

        return !string.Equals(label, "投资", StringComparison.Ordinal)
            && !string.Equals(label, "壳匠", StringComparison.Ordinal)
            && !string.Equals(label, "园丁", StringComparison.Ordinal)
            && !string.Equals(label, "灌溉", StringComparison.Ordinal);
    }

    private int SettlementGoldDeltaFromLabel(string label)
    {
        if (string.IsNullOrEmpty(label))
        {
            return 0;
        }

        int plusIndex = label.IndexOf("+", StringComparison.Ordinal);
        int goldIndex = label.IndexOf("金", StringComparison.Ordinal);
        if (plusIndex < 0 || goldIndex <= plusIndex)
        {
            return 0;
        }

        StringBuilder digits = new StringBuilder();
        for (int i = plusIndex + 1; i < goldIndex; i++)
        {
            if (char.IsDigit(label[i]))
            {
                digits.Append(label[i]);
            }
        }

        int value;
        return int.TryParse(digits.ToString(), out value) ? value : 0;
    }

    private string SettlementTargetLabel(Encounter encounter, bool willPass)
    {
        if (encounter == null)
        {
            return "结算完成";
        }

        if (willPass)
        {
            return "达标盖章 " + resolvedScore + "/" + encounter.Target;
        }

        int gap = Mathf.Max(0, encounter.Target - resolvedScore);
        return "差 " + gap + " 未达标";
    }

    private float DisplayMultiplierForScorePrefix(int scoreIndex)
    {
        List<int> revealedValues = new List<int>();
        for (int i = 0; i <= scoreIndex && i < scoringDice.Count; i++)
        {
            Die die = scoringDice[i];
            if (die == null || die.Temporary)
            {
                continue;
            }

            revealedValues.Add(die.EffectiveValue);
        }

        if (revealedValues.Count <= 0)
        {
            return 1f;
        }

        if (committedCounterValid && revealedValues.Count >= dice.Count)
        {
            return committedCounterMultiplier;
        }

        float multiplier = 1f;
        int maxCount = MaxDuplicateCount(revealedValues);
        if (maxCount >= 6)
        {
            multiplier = 6f;
        }
        else if (maxCount >= 5)
        {
            multiplier = 4f;
        }
        else if (maxCount >= 4)
        {
            multiplier = 3f;
        }
        else if (revealedValues.Count >= 5 && LongestRun(revealedValues) >= 5)
        {
            multiplier = 2f;
        }
        else if (maxCount >= 3)
        {
            multiplier = 2f;
        }

        if (revealedValues.Count >= dice.Count && (AllValuesOdd(revealedValues) || AllValuesEven(revealedValues)))
        {
            multiplier = Mathf.Max(multiplier, 2f);
        }

        return multiplier;
    }

    private int MaxDuplicateCount(List<int> values)
    {
        Dictionary<int, int> counts = new Dictionary<int, int>();
        int maxCount = 0;
        for (int i = 0; i < values.Count; i++)
        {
            int value = values[i];
            int count;
            counts.TryGetValue(value, out count);
            count++;
            counts[value] = count;
            if (count > maxCount)
            {
                maxCount = count;
            }
        }

        return maxCount;
    }

    private bool AllValuesOdd(List<int> values)
    {
        if (values.Count <= 0)
        {
            return false;
        }

        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] % 2 == 0)
            {
                return false;
            }
        }

        return true;
    }

    private bool AllValuesEven(List<int> values)
    {
        if (values.Count <= 0)
        {
            return false;
        }

        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] % 2 != 0)
            {
                return false;
            }
        }

        return true;
    }

    private void ApplyRunScoreCounterStep(int stepIndex)
    {
        if (stepIndex < 0 || stepIndex >= runScoreCounterSteps.Count)
        {
            return;
        }

        RunScoreCounterStep step = runScoreCounterSteps[stepIndex];
        int previousBaseScore = animatedCounterBaseScore;
        float previousMultiplier = animatedCounterMultiplier;
        int previousProgressScore = animatedCounterProgressScore;

        animatedCounterBaseScore = step.BaseScore;
        animatedCounterMultiplier = step.Multiplier;
        animatedCounterMultipliedScore = step.MultipliedScore;
        animatedCounterBribeScore = step.BribeScore;
        animatedCounterRollScore = step.RollScore;
        animatedCounterProgressScore = step.ProgressScore;
        currentScore = animatedCounterProgressScore;

        TriggerRunScoreCounterPulses(previousBaseScore, previousMultiplier, previousProgressScore);
    }

    private void ApplyRunScoreCounterFinal()
    {
        int previousBaseScore = animatedCounterBaseScore;
        float previousMultiplier = animatedCounterMultiplier;
        int previousProgressScore = animatedCounterProgressScore;

        if (committedCounterValid)
        {
            animatedCounterBaseScore = committedCounterBaseScore;
            animatedCounterMultiplier = committedCounterMultiplier;
            animatedCounterMultipliedScore = committedCounterMultipliedScore;
            animatedCounterBribeScore = committedCounterBribeScore;
            animatedCounterRollScore = committedCounterRollScore;
            animatedCounterProgressScore = committedCounterResolvedScore;
        }
        else
        {
            animatedCounterProgressScore = resolvedScore;
        }

        currentScore = resolvedScore;
        TriggerRunScoreCounterPulses(previousBaseScore, previousMultiplier, previousProgressScore);
    }

    private void TriggerRunScoreCounterPulses(int previousBaseScore, float previousMultiplier, int previousProgressScore)
    {
        if (animatedCounterBaseScore != previousBaseScore)
        {
            counterBasePulseTimer = ScoreCounterPulseDuration;
        }

        if (!Mathf.Approximately(animatedCounterMultiplier, previousMultiplier))
        {
            counterMultiplierPulseTimer = ScoreCounterPulseDuration;
        }

        if (animatedCounterProgressScore != previousProgressScore)
        {
            counterProgressPulseTimer = ScoreCounterPulseDuration;
        }
    }

    private void UpdateRunScoreCounterPulses(float deltaTime)
    {
        counterBasePulseTimer = Mathf.Max(0f, counterBasePulseTimer - deltaTime);
        counterMultiplierPulseTimer = Mathf.Max(0f, counterMultiplierPulseTimer - deltaTime);
        counterProgressPulseTimer = Mathf.Max(0f, counterProgressPulseTimer - deltaTime);
    }

    private float CounterPulseAmount(float timer)
    {
        return Mathf.Clamp01(timer / ScoreCounterPulseDuration);
    }

    private void DrawMultiplierBadge(Rect rect, string text, float pulse = 0f)
    {
        DrawRect(new Rect(rect.x + 2f, rect.y + 3f, rect.width, rect.height), new Color(0.12f, 0.06f, 0.04f, 0.16f));
        Color badgeColor = Color.Lerp(new Color(0.84f, 0.22f, 0.2f, 0.9f), new Color(1f, 0.48f, 0.18f, 0.96f), Mathf.Clamp01(pulse));
        DrawRect(rect, badgeColor);
        DrawBorder(rect, new Color(0.44f, 0.12f, 0.1f, 0.9f), 2f);
        DrawBorder(new Rect(rect.x + 5f, rect.y + 5f, rect.width - 10f, rect.height - 10f), new Color(1f, 0.76f, 0.48f, 0.44f), 1f);
        DrawRunText(rect, text, 31 + Mathf.RoundToInt(4f * Mathf.Clamp01(pulse)), FontStyle.Bold, new Color(1f, 0.88f, 0.64f), TextAnchor.MiddleCenter);
    }

    private void DrawRunPlayPanel(Rect rect, Encounter encounter)
    {
        DrawUiPanel(rect);
        GUI.Label(new Rect(rect.x + 30f, rect.y + 24f, 260f, 30f), "投掷区", headerStyle);

        Rect tableArea = new Rect(rect.x + 34f, rect.y + 78f, rect.width - 68f, rect.height - 112f);
        DrawTableDice(tableArea);
    }

    private void DrawRunSupportPanel(Rect rect, Encounter encounter)
    {
        DrawUiSidePanel(rect);
        GUI.Label(new Rect(rect.x + 24f, rect.y + 24f, rect.width - 48f, 30f), "本关规则", headerStyle);

        Rect ruleRect = new Rect(rect.x + 24f, rect.y + 70f, rect.width - 48f, 64f);
        DrawUiSmallPanel(ruleRect);
        DrawRunText(new Rect(ruleRect.x + 12f, ruleRect.y + 8f, ruleRect.width - 24f, ruleRect.height - 16f), encounter.RuleText, 13, FontStyle.Normal, new Color(0.42f, 0.32f, 0.24f), TextAnchor.MiddleCenter);

        if (DrawUiButton(new Rect(rect.x + 24f, rect.y + 154f, rect.width - 48f, 42f), showHandReference ? "关闭牌型表" : "牌型表", UiButtonKind.Secondary))
        {
            showHandReference = !showHandReference;
        }
    }

    private void DrawRunActionBar(Rect rect, Encounter encounter)
    {
        Rect fullPrimary = new Rect(rect.x, rect.y, rect.width, rect.height);
        Rect primary = new Rect(rect.x, rect.y, 222f, rect.height);
        Rect secondary = new Rect(rect.x + 238f, rect.y, 102f, rect.height);

        if (rollPhase == RollPhase.ResultDecision)
        {
            if (DrawUiButton(primary, SettleButtonText(), UiButtonKind.Primary))
            {
                BeginSettle();
            }

            GUI.enabled = cheatsLeft > 0;
            if (DrawUiButton(secondary, cheatsLeft > 0 ? "出千" : "已用", UiButtonKind.Secondary))
            {
                BeginCheatEdit();
            }
            GUI.enabled = true;
            return;
        }

        if (rollPhase == RollPhase.CheatEdit)
        {
            GUI.enabled = cheatRerollIds.Count > 0;
            if (DrawUiButton(primary, "确认 " + cheatRerollIds.Count + "/" + MaxCheatRerollDice, UiButtonKind.Primary))
            {
                ConfirmCheatAndSettle();
            }
            GUI.enabled = true;

            if (DrawUiButton(secondary, "取消", UiButtonKind.Secondary))
            {
                CancelCheatEdit();
            }
            return;
        }

        if (rollPhase == RollPhase.StageClear)
        {
            if (DrawUiButton(fullPrimary, HasNextEncounter() ? "进入市场" : "完成本轮", UiButtonKind.Primary))
            {
                if (HasNextEncounter())
                {
                    EnterMarket(encounter.Boss);
                    mode = encounter.Boss ? GameMode.ChapterShop : GameMode.InterStageMarket;
                }
                else
                {
                    ClearSave();
                    mode = GameMode.Win;
                }
            }
            return;
        }

        DrawActionPill(fullPrimary, CompactActionBarText());
    }

    private string SettleButtonText()
    {
        return "Space";
    }

    private string CompactActionBarText()
    {
        if (rollPhase == RollPhase.Scoring)
        {
            return "结算中";
        }

        if (rollPhase == RollPhase.StageFailed)
        {
            return "失败";
        }

        if (dice.Count == 0)
        {
            return "无骰";
        }

        return "Space";
    }

    private void DrawInfoTag(Rect rect, string text, bool highlighted)
    {
        DrawUiSmallPanel(rect);
        if (highlighted)
        {
            DrawRect(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, 3f), new Color(0.2f, 0.58f, 0.53f, 0.85f));
        }

        GUI.Label(rect, text, centerStyle);
    }

    private void DrawRecentLog(Rect rect, int maxLines)
    {
        float y = rect.y;
        int drawn = 0;
        if (!string.IsNullOrEmpty(rewardBanner))
        {
            GUI.Label(new Rect(rect.x, y, rect.width, 20f), rewardBanner, tinyStyle);
            y += 22f;
            drawn++;
        }

        for (int i = logLines.Count - 1; i >= 0 && drawn < maxLines; i--)
        {
            string line = CompactLogLine(logLines[i]);
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            GUI.Label(new Rect(rect.x, y, rect.width, 20f), line, tinyStyle);
            y += 22f;
            drawn++;
        }

        if (drawn == 0)
        {
            GUI.Label(rect, "暂无反馈", tinyStyle);
        }
    }

    private string CompactLogLine(string line)
    {
        if (line.StartsWith("进入 ", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        if (line == "摇骰盅开始，敲空格加力。" || line == "骰子开始旋转，敲空格加速。")
        {
            return string.Empty;
        }

        if (line == "加力窗口结束，骰盅回落。" || line == "加力窗口结束，骰子即将停转。")
        {
            return string.Empty;
        }

        if (line == "开盅，结果锁定。可直接结算或出千。" || line == "停转显点，结果锁定。可直接结算或出千。")
        {
            return string.Empty;
        }

        if (line == "二次确认，开始从左到右结算。")
        {
            return string.Empty;
        }

        if (line.StartsWith("出千：", StringComparison.Ordinal))
        {
            return string.Empty;
        }

        if (line == "取消出千选择，返回结算。")
        {
            return "已取消出千";
        }

        if (line == "出千需要至少选择一颗实体骰。")
        {
            return "需选择至少 1 颗";
        }

        if (line.StartsWith("出千最多选择 ", StringComparison.Ordinal))
        {
            return "最多选择 " + MaxCheatRerollDice + " 颗";
        }

        return line;
    }

    private void DrawPhaseRibbon(Rect rect, Encounter encounter)
    {
        Color accent = PhaseAccentColor();
        DrawRect(new Rect(rect.x + 2f, rect.y + 3f, rect.width, rect.height), new Color(0.1f, 0.06f, 0.03f, 0.18f));
        DrawRect(rect, new Color(1f, 0.91f, 0.64f, 0.76f));
        DrawPromptPulse(rect);
        DrawBorder(rect, new Color(0.28f, 0.19f, 0.11f, 0.56f), 2f);
        DrawRect(new Rect(rect.x + 5f, rect.y + 5f, 7f, rect.height - 10f), accent);
        GUI.Label(new Rect(rect.x + 18f, rect.y + 4f, rect.width - 24f, rect.height - 8f), PhaseTitleText() + " | " + PhaseDetailText(encounter), phaseRibbonStyle);
    }

    private void DrawPromptPulse(Rect rect)
    {
        RollFeedbackConfig config = CurrentRollFeedbackConfig();
        float pulse = PromptPulseAmount(config);
        if (pulse <= 0f)
        {
            return;
        }

        float scale = Mathf.Max(1f, config.PromptPulseScale);
        float growX = rect.width * (scale - 1f) * pulse;
        float growY = rect.height * (scale - 1f) * pulse;
        Rect pulseRect = new Rect(rect.x + 14f - growX * 0.5f, rect.y + 4f - growY * 0.5f, rect.width - 20f + growX, rect.height - 8f + growY);
        float alpha = Mathf.Lerp(0.04f, 0.18f, pulse);
        DrawRect(pulseRect, new Color(1f, 0.77f, 0.24f, alpha));
    }

    private float PromptPulseAmount(RollFeedbackConfig config)
    {
        if (config == null)
        {
            return 0f;
        }

        float pulse = 0f;
        if (promptPulseTimer > 0f)
        {
            pulse = Mathf.Max(pulse, promptPulseTimer / Mathf.Max(0.01f, config.PromptPulseDuration));
        }

        if (shakeExpiredPromptTimer > 0f)
        {
            pulse = Mathf.Max(pulse, shakeExpiredPromptTimer / Mathf.Max(0.01f, config.ExpiredPromptDuration));
        }

        return Mathf.Clamp01(pulse);
    }

    private Color PhaseAccentColor()
    {
        switch (rollPhase)
        {
            case RollPhase.ResultDecision:
            case RollPhase.StageClear:
                return new Color(0.2f, 0.62f, 0.56f, 0.92f);
            case RollPhase.CheatEdit:
                return new Color(0.95f, 0.62f, 0.18f, 0.94f);
            case RollPhase.Scoring:
                return new Color(0.88f, 0.58f, 0.16f, 0.94f);
            case RollPhase.StageFailed:
                return new Color(0.78f, 0.25f, 0.2f, 0.94f);
        }

        return new Color(0.44f, 0.64f, 0.58f, 0.9f);
    }

    private string PhaseTitleText()
    {
        switch (rollPhase)
        {
            case RollPhase.Ready:
                return "待投";
            case RollPhase.Shaking:
                return "旋骰";
            case RollPhase.Stopping:
                return "停转";
            case RollPhase.ResultDecision:
                return "决策";
            case RollPhase.CheatEdit:
                return "出千";
            case RollPhase.Scoring:
                return "结算";
            case RollPhase.StageClear:
                return "达标";
            case RollPhase.StageFailed:
                return "失败";
        }

        return "阶段";
    }

    private string PhaseDetailText(Encounter encounter)
    {
        if (rollPhase == RollPhase.Ready)
        {
            if (!rolledThisEncounter && stageInvestmentGold > 0)
            {
                return "投资已锁 " + stageInvestmentGold + " 金";
            }

            if (!rolledThisEncounter && CountDiceOfType(DieType.Treasury) > 0)
            {
                return "国库本金 +" + TreasuryScoreBonus();
            }

            return rolledThisEncounter ? "Space 再旋一次" : "Space 开始旋骰";
        }

        if (rollPhase == RollPhase.Shaking)
        {
            return "敲 Space 加速";
        }

        if (rollPhase == RollPhase.Stopping)
        {
            return "停转显点";
        }

        if (rollPhase == RollPhase.ResultDecision)
        {
            int afterScore = currentScore + previewRollScore;
            if (encounter != null && afterScore >= encounter.Target)
            {
                if (previewBribeGoldCost > 0)
                {
                    return "Space 结算，贿赂 -" + previewBribeGoldCost + " 金";
                }

                return "Space 结算达标";
            }

            return cheatsLeft > 0 ? "Space 结算，或出千改点" : "Space 结算";
        }

        if (rollPhase == RollPhase.CheatEdit)
        {
            return "已选 " + cheatRerollIds.Count + "/" + MaxCheatRerollDice + "，确认后直接结算";
        }

        if (rollPhase == RollPhase.Scoring)
        {
            if (activeSettlementEvent != null && !string.IsNullOrEmpty(activeSettlementEvent.Label))
            {
                return activeSettlementEvent.Label;
            }

            return "从左到右计入得分";
        }

        if (rollPhase == RollPhase.StageClear)
        {
            return HasNextEncounter() ? "收入入账，进入市场" : "收入入账，完成本轮";
        }

        if (rollPhase == RollPhase.StageFailed)
        {
            return "目标未达成，本轮结束";
        }

        return RollPromptText();
    }

    private void DrawActionPill(Rect rect, string text)
    {
        DrawScalableButtonFrame(rect, UiButtonKind.Primary, true);
        GUI.Label(rect, text, artButtonStyle);
    }

    private int CountDiceOfType(DieType type)
    {
        int count = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i].Type == type)
            {
                count++;
            }
        }

        return count;
    }

    private int TurtleRouteDiceCount()
    {
        return CountDiceOfType(DieType.Turtle)
            + CountDiceOfType(DieType.Shellsmith)
            + CountDiceOfType(DieType.Nest)
            + CountDiceOfType(DieType.SlowTurtle);
    }

    private int GoldRouteDiceCount()
    {
        return CountDiceOfType(DieType.Piggy)
            + CountDiceOfType(DieType.Treasury)
            + CountDiceOfType(DieType.Bribe)
            + CountDiceOfType(DieType.Investment)
            + CountDiceOfType(DieType.BountyGold)
            + CountDiceOfType(DieType.TopGold)
            + CountDiceOfType(DieType.HandTax)
            + CountDiceOfType(DieType.Collection)
            + CountDiceOfType(DieType.CompoundInterest)
            + CountDiceOfType(DieType.LeadTicket)
            + CountDiceOfType(DieType.ShellTax)
            + CountDiceOfType(DieType.CounterGold)
            + CountDiceOfType(DieType.LumberGold);
    }

    private int TreeInterfaceDiceCount()
    {
        return CountDiceOfType(DieType.PointSeedTree)
            + CountDiceOfType(DieType.PatternTree)
            + CountDiceOfType(DieType.CanopyTree)
            + CountDiceOfType(DieType.RingTree)
            + CountDiceOfType(DieType.FertilizerTree)
            + CountDiceOfType(DieType.PruningTree)
            + CountDiceOfType(DieType.RootTree);
    }

    private int TreeRouteDiceCount()
    {
        return CountDiceOfType(DieType.Tree)
            + CountDiceOfType(DieType.Gardener)
            + CountDiceOfType(DieType.Irrigation)
            + TreeInterfaceDiceCount();
    }

    private bool IsTreeGrowthType(DieType type)
    {
        switch (type)
        {
            case DieType.Tree:
            case DieType.PointSeedTree:
            case DieType.PatternTree:
            case DieType.CanopyTree:
            case DieType.RingTree:
            case DieType.FertilizerTree:
            case DieType.PruningTree:
            case DieType.RootTree:
                return true;
        }

        return false;
    }

    private string RunHudResourceText()
    {
        string investment = stageInvestmentGold > 0 ? "   投 " + stageInvestmentGold : string.Empty;
        return "金 " + chapterGold + investment + "   出手 " + rollsLeft + "/" + RollsPerStage + "   出千 " + cheatsLeft + "/" + CheatsPerStage;
    }

    private string[] DiceBagTypeLines()
    {
        List<string> lines = new List<string>();
        AddDiceBagTypeLine(lines, DieType.Basic, "基础");
        AddDiceBagTypeLine(lines, DieType.Odd, "奇数");
        AddDiceBagTypeLine(lines, DieType.Even, "偶数");
        AddDiceBagTypeLine(lines, DieType.LoneWitness, "孤证");
        AddDiceBagTypeLine(lines, DieType.Stamp, "盖章");
        AddDiceBagTypeLine(lines, DieType.HalfStep, "半步");
        AddDiceBagTypeLine(lines, DieType.Track, "轨道");
        AddDiceBagTypeLine(lines, DieType.ParityNeighborDiff, "异邻");
        AddDiceBagTypeLine(lines, DieType.ParityNeighborSame, "同邻");
        AddDiceBagTypeLine(lines, DieType.ParityComplete, "补全");
        AddDiceBagTypeLine(lines, DieType.ParityReview, "复核");
        AddDiceBagTypeLine(lines, DieType.ParityFlipScore, "翻号");
        AddDiceBagTypeLine(lines, DieType.ParityHoldScore, "守号");
        AddDiceBagTypeLine(lines, DieType.ParityTurner, "转号");
        AddDiceBagTypeLine(lines, DieType.Piggy, "猪猪");
        AddDiceBagTypeLine(lines, DieType.Treasury, "国库");
        AddDiceBagTypeLine(lines, DieType.Bribe, "贿赂");
        AddDiceBagTypeLine(lines, DieType.Investment, "投资");
        AddDiceBagTypeLine(lines, DieType.BountyGold, "悬赏");
        AddDiceBagTypeLine(lines, DieType.TopGold, "顶金");
        AddDiceBagTypeLine(lines, DieType.HandTax, "牌税");
        AddDiceBagTypeLine(lines, DieType.Collection, "收账");
        AddDiceBagTypeLine(lines, DieType.CompoundInterest, "复利");
        AddDiceBagTypeLine(lines, DieType.LeadTicket, "铅票");
        AddDiceBagTypeLine(lines, DieType.ShellTax, "壳税");
        AddDiceBagTypeLine(lines, DieType.CounterGold, "柜台");
        AddDiceBagTypeLine(lines, DieType.LumberGold, "伐木");
        AddDiceBagTypeLine(lines, DieType.Turtle, "龟龟");
        AddDiceBagTypeLine(lines, DieType.Shellsmith, "壳匠");
        AddDiceBagTypeLine(lines, DieType.Nest, "巢穴");
        AddDiceBagTypeLine(lines, DieType.SlowTurtle, "慢龟");
        AddDiceBagTypeLine(lines, DieType.Double, "双倍");
        AddDiceBagTypeLine(lines, DieType.Tree, "大树");
        AddDiceBagTypeLine(lines, DieType.Gardener, "园丁");
        AddDiceBagTypeLine(lines, DieType.Irrigation, "灌溉");
        AddDiceBagTypeLine(lines, DieType.PointSeedTree, "点籽");
        AddDiceBagTypeLine(lines, DieType.PatternTree, "牌谱");
        AddDiceBagTypeLine(lines, DieType.CanopyTree, "冠层");
        AddDiceBagTypeLine(lines, DieType.RingTree, "年轮");
        AddDiceBagTypeLine(lines, DieType.FertilizerTree, "肥料");
        AddDiceBagTypeLine(lines, DieType.PruningTree, "修枝");
        AddDiceBagTypeLine(lines, DieType.RootTree, "根系");
        AddDiceBagTypeLine(lines, DieType.Gambler, "赌徒");

        if (lines.Count == 0)
        {
            lines.Add("暂无骰子");
        }

        return lines.ToArray();
    }

    private void AddDiceBagTypeLine(List<string> lines, DieType type, string label)
    {
        int count = CountDiceOfType(type);
        if (count > 0)
        {
            lines.Add(label + " x" + count);
        }
    }

    private string DiceBagFocusText()
    {
        int bestTreeGrowth = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            if (IsTreeGrowthType(dice[i].Type) && dice[i].Growth > bestTreeGrowth)
            {
                bestTreeGrowth = dice[i].Growth;
            }
        }

        if (bestTreeGrowth > 0)
        {
            return "大树最高 +" + bestTreeGrowth;
        }

        int interfaceTree = TreeInterfaceDiceCount();
        if (interfaceTree > 0)
        {
            return "接触触发大树成长";
        }

        int tree = CountDiceOfType(DieType.Tree);
        int gardener = CountDiceOfType(DieType.Gardener);
        int irrigation = CountDiceOfType(DieType.Irrigation);
        if (tree > 0 && gardener > 0)
        {
            return "园丁叠大树成长";
        }

        if (tree > 0 && irrigation > 0)
        {
            return "灌溉追大树目标";
        }

        if (gardener > 0)
        {
            return "园丁等大树";
        }

        if (irrigation > 0)
        {
            return "灌溉等大树";
        }

        if (stageInvestmentGold > 0)
        {
            return "投资锁 " + stageInvestmentGold + " 金";
        }

        int treasury = CountDiceOfType(DieType.Treasury);
        if (treasury > 0)
        {
            return "国库本金 +" + TreasuryScoreBonus();
        }

        int bribe = CountDiceOfType(DieType.Bribe);
        if (bribe > 0)
        {
            int reserve = Mathf.Min(Mathf.Max(0, chapterGold), bribe * Mathf.Max(0, bribeGoldCapPerDie));
            return reserve > 0 ? "贿赂可用 " + reserve + " 金" : "贿赂暂无现金";
        }

        int piggy = CountDiceOfType(DieType.Piggy);
        if (CountDiceOfType(DieType.LoneWitness) > 0)
        {
            return "孤证会重找同点";
        }

        if (CountDiceOfType(DieType.Stamp) > 0)
        {
            return "盖章吃同点伙伴";
        }

        if (CountDiceOfType(DieType.HalfStep) > 0)
        {
            return "半步可补顺子";
        }

        if (CountDiceOfType(DieType.Track) > 0)
        {
            return "轨道看 2/4/6";
        }

        if (CountDiceOfType(DieType.ParityNeighborDiff) > 0 || CountDiceOfType(DieType.ParityNeighborSame) > 0)
        {
            return "邻位看奇偶";
        }

        if (CountDiceOfType(DieType.ParityComplete) > 0)
        {
            return "补全全奇全偶";
        }

        if (CountDiceOfType(DieType.ParityReview) > 0)
        {
            return "复核会重摇";
        }

        if (CountDiceOfType(DieType.ParityFlipScore) > 0 || CountDiceOfType(DieType.ParityHoldScore) > 0 || CountDiceOfType(DieType.ParityTurner) > 0)
        {
            return "出千看奇偶";
        }

        if (piggy > 0)
        {
            return "猪猪看目标";
        }

        int turtle = TurtleRouteDiceCount();
        if (turtle > 0)
        {
            if (CountDiceOfType(DieType.Shellsmith) > 0)
            {
                return "壳匠吃小骰数量";
            }

            if (CountDiceOfType(DieType.Nest) > 0)
            {
                return "巢穴等龟链";
            }

            return "龟龟会追加小骰";
        }

        int gambler = CountDiceOfType(DieType.Gambler);
        if (gambler > 0)
        {
            return "赌徒看阈值";
        }

        return "看点数和牌型";
    }

    private string DiceBagRouteText()
    {
        int odd = CountDiceOfType(DieType.Odd) + CountDiceOfType(DieType.LoneWitness) + CountDiceOfType(DieType.Stamp);
        int even = CountDiceOfType(DieType.Even) + CountDiceOfType(DieType.HalfStep) + CountDiceOfType(DieType.Track);
        int parityShort = CountDiceOfType(DieType.ParityNeighborDiff)
            + CountDiceOfType(DieType.ParityNeighborSame)
            + CountDiceOfType(DieType.ParityComplete)
            + CountDiceOfType(DieType.ParityReview)
            + CountDiceOfType(DieType.ParityFlipScore)
            + CountDiceOfType(DieType.ParityHoldScore)
            + CountDiceOfType(DieType.ParityTurner);
        int piggy = CountDiceOfType(DieType.Piggy);
        int goldDice = GoldRouteDiceCount();
        int turtle = TurtleRouteDiceCount();
        int tree = TreeRouteDiceCount();
        int burst = CountDiceOfType(DieType.Double) + CountDiceOfType(DieType.Gambler);

        if (goldDice > piggy)
        {
            return "金币路线";
        }

        if (parityShort >= 2)
        {
            return "奇偶短规则";
        }

        if (even >= 3)
        {
            return "偶数路线";
        }

        if (odd >= 3)
        {
            return "奇数路线";
        }

        if (piggy >= 2)
        {
            return "经济路线";
        }

        if (tree > 0)
        {
            return "成长路线";
        }

        if (turtle > 0)
        {
            return "迭代路线";
        }

        if (burst >= 2)
        {
            return "爆发路线";
        }

        return "基础过渡";
    }

    private string StagePrepFocusText(Encounter encounter)
    {
        if (rollPhase == RollPhase.Shaking)
        {
            return "敲 Space 加速";
        }

        if (rollPhase == RollPhase.Stopping)
        {
            return "停转显点";
        }

        if (rollPhase == RollPhase.Scoring)
        {
            return "正在计入本次得分";
        }

        if (rollPhase == RollPhase.StageFailed)
        {
            return "本轮失败";
        }

        if (encounter != null && encounter.Rule != RuleKind.None)
        {
            return RuleFocusText(encounter);
        }

        return rolledThisEncounter ? "继续追分" : "看点数和牌型";
    }

    private string RuleFocusText(Encounter encounter)
    {
        switch (encounter.Rule)
        {
            case RuleKind.OddLedger:
                return "奇数骰本关更值钱";
            case RuleKind.HandAudit:
                return "有牌型会额外加分";
            case RuleKind.LowFog:
                return "低点数本关会吃亏";
            case RuleKind.DoubleJudge:
                return "偶数优先，顺子加分";
        }

        return "无额外规则";
    }

    private string ResultBreakdownText()
    {
        List<string> parts = new List<string>();
        parts.Add("单骰 " + previewIndividualScore);
        if (previewTemporaryScore > 0 || previewHasTurtleRandomness)
        {
            parts.Add("小骰 " + previewTemporaryScore);
        }

        if (previewShellsmithScoreBonus > 0)
        {
            parts.Add("壳匠 +" + previewShellsmithScoreBonus);
        }

        if (previewAffixScoreBonus > 0)
        {
            parts.Add("词缀 +" + previewAffixScoreBonus);
        }

        if (previewRuleBonus > 0)
        {
            parts.Add("规则 " + previewRuleBonus);
        }

        if (previewBribeScoreBonus > 0)
        {
            parts.Add("贿赂 +" + previewBribeScoreBonus);
        }

        return string.Join(" / ", parts.ToArray());
    }

    private string ResultScoreNote()
    {
        if (previewBribeGoldCost > 0)
        {
            return "贿赂 -" + previewBribeGoldCost + "金 +" + previewBribeScoreBonus;
        }

        if (previewWalletIncome > 0)
        {
            return "钱包 +" + previewWalletIncome;
        }

        if (stageInvestmentGold > 0)
        {
            return "投资已锁 " + stageInvestmentGold + "金";
        }

        int treasuryBonus = TotalTreasuryScoreBonus();
        if (treasuryBonus > 0)
        {
            return "国库 +" + treasuryBonus;
        }

        if (lastHandName == "顺子（半步）")
        {
            return "半步借位成顺";
        }

        int stampCount = TriggeredStampDiceCount();
        if (stampCount > 0)
        {
            return "盖章 6分 x" + stampCount;
        }

        int trackCount = TriggeredTrackDiceCount();
        if (trackCount > 0)
        {
            return "轨道 8分 x" + trackCount;
        }

        if (previewHasTurtleRandomness)
        {
            string nestText = previewNestBonusDieCount > 0 ? " / 巢 +" + previewNestBonusDieCount : string.Empty;
            return "小龟预估" + nestText;
        }

        return string.Empty;
    }

    private string ResultProgressText(Encounter encounter)
    {
        int afterScore = currentScore;
        if (rollPhase == RollPhase.StageClear)
        {
            afterScore = resolvedScore;
        }

        int remaining = Mathf.Max(0, encounter.Target - afterScore);
        return afterScore + "/" + encounter.Target + (remaining > 0 ? " 差" + remaining : " 达标");
    }

    private string CheatAdviceText(Encounter encounter)
    {
        if (rollPhase == RollPhase.StageClear)
        {
            return "进入市场";
        }

        int afterScore = currentScore + previewRollScore;
        if (afterScore >= encounter.Target)
        {
            if (previewBribeGoldCost > 0)
            {
                return "贿赂 -" + previewBribeGoldCost + " 金";
            }

            return "建议结算";
        }

        if (cheatsLeft <= 0)
        {
            return "无出千";
        }

        if (rollPhase == RollPhase.CheatEdit)
        {
            return "选低点 +1";
        }

        return rollsLeft <= 0 ? "建议出千" : "可给低点 +1";
    }

    private string ActionBarPrimaryText()
    {
        if (rollPhase == RollPhase.Ready)
        {
            if (!rolledThisEncounter && stageInvestmentGold > 0)
            {
                return "投资已锁 " + stageInvestmentGold + " 金，本关 +" + TotalInvestmentScoreBonus();
            }

            if (!rolledThisEncounter && CountDiceOfType(DieType.Treasury) > 0)
            {
                return "国库按本金 +" + TreasuryScoreBonus();
            }

            return rolledThisEncounter ? "Space 再旋一次" : "Space 开始旋骰";
        }

        if (rollPhase == RollPhase.Shaking)
        {
            return "敲 Space 加速";
        }

        if (rollPhase == RollPhase.Stopping)
        {
            return "停转显点";
        }

        if (rollPhase == RollPhase.Scoring)
        {
            return "结算中";
        }

        if (rollPhase == RollPhase.StageFailed)
        {
            return "本轮失败";
        }

        return RollPromptText();
    }

    private string EncounterTitle(Encounter encounter)
    {
        if (encounter == null)
        {
            return "骰子王";
        }

        if (encounter.ChapterIndex > 0 && encounter.StageIndexInChapter > 0)
        {
            return "第 " + encounter.ChapterIndex + " 章-" + encounter.StageIndexInChapter + " | " + encounter.Name;
        }

        return encounter.Name;
    }

    private bool ShouldShowResultSummary()
    {
        return rollPhase == RollPhase.ResultDecision
            || rollPhase == RollPhase.CheatEdit
            || rollPhase == RollPhase.Scoring
            || rollPhase == RollPhase.StageClear;
    }

    private bool ShouldShowRecentFeedback()
    {
        return (ShouldShowResultSummary() || rollPhase == RollPhase.StageFailed) && HasRecentFeedback();
    }

    private bool HasRecentFeedback()
    {
        if (!string.IsNullOrEmpty(rewardBanner))
        {
            return true;
        }

        for (int i = logLines.Count - 1; i >= 0; i--)
        {
            if (!string.IsNullOrEmpty(CompactLogLine(logLines[i])))
            {
                return true;
            }
        }

        return false;
    }

    private void DrawMarket(bool chapterMarket)
    {
        if (!affixFeatureEnabled && !string.IsNullOrEmpty(activeCraftingItemKey))
        {
            activeCraftingItemKey = string.Empty;
        }

        Encounter encounter = CurrentEncounter();
        string title = chapterMarket && encounter != null ? "章节市场" : "关间市场";
        DrawHudBar(title, "金币 " + chapterGold, "骰袋 " + dice.Count + " / " + DiceCapacity + "   刷新 " + RefreshCost(chapterMarket) + " 金");

        if (selectedMarketDieIndex >= dice.Count)
        {
            selectedMarketDieIndex = dice.Count - 1;
        }
        if (selectedMarketDieIndex < 0 && dice.Count > 0)
        {
            selectedMarketDieIndex = 0;
        }

        DrawUiPanel(new Rect(36f, 128f, 430f, 556f));
        DrawIconLabel(new Rect(64f, 158f, 260f, 30f), uiIconBagTexture, string.IsNullOrEmpty(activeCraftingItemKey) ? "当前骰袋" : "选择改造目标", headerStyle);
        GUI.Label(new Rect(320f, 162f, 110f, 24f), dice.Count + " / " + DiceCapacity, smallStyle);
        for (int i = 0; i < dice.Count; i++)
        {
            Rect rect = new Rect(64f, 204f + i * 58f, 350f, 52f);
            if (DrawCompactDie(rect, dice[i], i == selectedMarketDieIndex, "market-bag-" + dice[i].Id))
            {
                selectedMarketDieIndex = i;
            }
        }

        Rect sellPanel = new Rect(64f, 562f, 350f, 86f);
        DrawUiSmallPanel(sellPanel);
        if (affixFeatureEnabled && !string.IsNullOrEmpty(activeCraftingItemKey))
        {
            DrawCraftingTargetPanel(sellPanel);
        }
        else if (selectedMarketDieIndex >= 0 && selectedMarketDieIndex < dice.Count)
        {
            Die selectedDie = dice[selectedMarketDieIndex];
            int sell = SellPrice(selectedDie.Type);
            GUI.Label(new Rect(sellPanel.x + 20f, sellPanel.y + 10f, 168f, 22f), DieDisplayName(selectedDie), smallStyle);
            GUI.Label(new Rect(sellPanel.x + 20f, sellPanel.y + 34f, 168f, 20f), TypeName(selectedDie.Type) + " | " + FaceText(selectedDie.Faces), tinyStyle);
            GUI.Label(new Rect(sellPanel.x + 20f, sellPanel.y + 56f, 168f, 18f), "按类型回收", tinyStyle);
            if (DrawUiButton(new Rect(sellPanel.x + 204f, sellPanel.y + 23f, 122f, 42f), "卖 " + sell + " 金", UiButtonKind.Danger))
            {
                chapterGold += sell;
                AddLog("卖出 " + DieDisplayName(selectedDie) + "，+" + sell + " 金币。");
                dice.RemoveAt(selectedMarketDieIndex);
                selectedMarketDieIndex = dice.Count > 0 ? Mathf.Clamp(selectedMarketDieIndex, 0, dice.Count - 1) : -1;
                rewardBanner = "卖出骰子，骰袋 " + dice.Count + " / " + DiceCapacity + "。";
                SaveRun();
            }
        }
        else
        {
            string sellHint = dice.Count >= DiceCapacity ? "袋满：卖一颗再买。" : "选择骰子查看卖价。";
            GUI.Label(new Rect(sellPanel.x + 20f, sellPanel.y + 26f, sellPanel.width - 40f, 34f), sellHint, smallStyle);
        }

        DrawUiPanel(new Rect(496f, 128f, 748f, 556f));
        GUI.Label(new Rect(528f, 158f, 300f, 30f), "本次货架", headerStyle);
        if (dice.Count >= DiceCapacity)
        {
            GUI.Label(new Rect(832f, 162f, 360f, 24f), affixFeatureEnabled ? "骰袋已满：仍可买改造道具" : "骰袋已满：卖一颗再买", smallStyle);
        }

        for (int i = 0; i < marketOffers.Count; i++)
        {
            Rect rect = new Rect(528f + i * 226f, 214f, 204f, 300f);
            DrawMarketOffer(rect, marketOffers[i], "market-offer-" + i);
            bool canBuy = CanBuyMarketOffer(marketOffers[i]);
            GUI.enabled = canBuy;
            if (DrawUiButton(new Rect(rect.x + 26f, rect.y + 242f, 152f, 44f), MarketBuyButtonText(marketOffers[i]), UiButtonKind.Primary))
            {
                BuyOffer(i);
                break;
            }
            GUI.enabled = true;
        }

        if (affixFeatureEnabled)
        {
            DrawCraftingInventoryPanel(new Rect(528f, 526f, 666f, 52f));
        }

        MarketRuleConfig marketRule = CurrentMarketRule();
        int refreshCost = RefreshCost(chapterMarket);
        GUI.enabled = chapterGold >= refreshCost;
        if (DrawUiButton(new Rect(528f, 594f, 172f, 46f), refreshCost > 0 ? "刷新 " + refreshCost + " 金" : "刷新货架", UiButtonKind.Secondary))
        {
            chapterGold -= refreshCost;
            BuildMarketOffers(true);
            rewardBanner = "刷新货架，-" + refreshCost + " 金币。";
            SaveRun();
        }
        GUI.enabled = true;
        string refreshHint = marketTestRandomRefresh
            ? "测试随机：全池骰子"
            : (marketRule.HighTierPityRefreshes > 0 ? "高阶保底 " + marketRefreshesWithoutHighTier + " / " + marketRule.HighTierPityRefreshes : "刷新后替换全部货架");
        DrawIconLabel(new Rect(714f, 600f, 240f, 34f), uiIconRefreshTexture, refreshHint, smallStyle);

        GUI.enabled = dice.Count > 0;
        if (DrawUiButton(new Rect(1010f, 594f, 184f, 50f), HasNextEncounter() ? "离开市场" : "完成本轮", UiButtonKind.Primary))
        {
            if (HasNextEncounter())
            {
                stageIndex++;
                rewardBanner = string.Empty;
                activeCraftingItemKey = string.Empty;
                selectedMarketDieIndex = -1;
                SaveRun();
                StartEncounter();
                mode = GameMode.Run;
            }
            else
            {
                activeCraftingItemKey = string.Empty;
                ClearSave();
                mode = GameMode.Win;
            }
        }
        GUI.enabled = true;

        if (dice.Count <= 0)
        {
            GUI.Label(new Rect(966f, 648f, 260f, 26f), "骰袋为空，不能离开市场。", smallStyle);
        }

        if (!string.IsNullOrEmpty(rewardBanner))
        {
            GUI.Label(new Rect(528f, 654f, 420f, 24f), rewardBanner, smallStyle);
        }
    }

    private void DrawEnd(bool won)
    {
        DrawHudBar("骰子王", won ? "本轮通关" : "本轮失败", string.Empty);
        DrawUiPanel(new Rect(216f, 136f, 848f, 430f));
        GUI.Label(new Rect(264f, 184f, 680f, 52f), won ? "本轮通关" : "本轮失败", headerStyle);
        string result = won
            ? "本轮已经完成。目标分来自 Resources/Data/chapter_score_table.csv，市场和骰子类型已按当前设计运行。"
            : "任意小关失败会直接结束本轮，并清除本轮存档；需要从主菜单重新开始。";
        GUI.Label(new Rect(266f, 252f, 720f, 90f), result, bodyStyle);
        GUI.Label(new Rect(266f, 360f, 720f, 36f), "最终金币 " + chapterGold + " | 骰子数量 " + dice.Count, headerStyle);

        if (DrawUiButton(new Rect(266f, 438f, 206f, 52f), "回主菜单", UiButtonKind.Primary))
        {
            mode = GameMode.MainMenu;
            rewardBanner = string.Empty;
        }
    }

    private void DrawHandReferenceOverlay()
    {
        DrawRect(new Rect(0f, 0f, VirtualWidth, VirtualHeight), new Color(0f, 0f, 0f, 0.42f));
        Rect panel = new Rect(190f, 78f, 900f, 572f);
        DrawUiPanel(panel);
        GUI.Label(new Rect(panel.x + 36f, panel.y + 26f, 360f, 46f), "牌型表", headerStyle);
        if (DrawUiButton(new Rect(panel.x + panel.width - 150f, panel.y + 30f, 112f, 42f), "关闭", UiButtonKind.Secondary))
        {
            showHandReference = false;
        }

        string[] names = new string[]
        {
            "六同", "五同", "四同", "顺子", "三同", "全奇", "全偶", "无牌型"
        };
        string[] examples = new string[]
        {
            "1 1 1 1 1 1", "1 1 1 1 1 2", "3 3 3 3 1 2", "1 2 3 4 5 ?",
            "2 2 2 1 4 6", "1 1 3 3 5 5", "2 2 4 4 6 6", "没有以上牌型"
        };
        string[] multipliers = new string[]
        {
            "x6", "x4", "x3", "x2", "x2", "x2", "x2", "x1"
        };

        for (int i = 0; i < names.Length; i++)
        {
            int column = i / 4;
            int row = i % 4;
            float x = panel.x + 40f + column * 428f;
            float y = panel.y + 100f + row * 88f;
            DrawHandReferenceRow(new Rect(x, y, 374f, 70f), names[i], multipliers[i], examples[i]);
        }

        GUI.Label(new Rect(panel.x + 40f, panel.y + panel.height - 76f, panel.width - 80f, 54f), "只认上表牌型。主牌型按优先级只取一个；全奇和全偶是副牌型，最终倍率取更高的那个。", smallStyle);
    }

    private void DrawHandReferenceRow(Rect rect, string handName, string multiplier, string example)
    {
        DrawUiSmallPanel(rect);
        DrawRect(new Rect(rect.x + 6f, rect.y + 8f, 4f, rect.height - 16f), new Color(0.78f, 0.58f, 0.28f, 0.72f));
        GUI.Label(new Rect(rect.x + 16f, rect.y + 8f, 126f, 34f), handName, cardTitleStyle);
        GUI.Label(new Rect(rect.x + 150f, rect.y + 8f, 70f, 34f), multiplier, headerStyle);
        GUI.Label(new Rect(rect.x + 16f, rect.y + 42f, rect.width - 32f, 24f), "例：" + example, smallStyle);
    }

    private void DrawDiceBag()
    {
        float y = 172f;
        for (int i = 0; i < dice.Count; i++)
        {
            Rect rect = new Rect(56f, y, 238f, 76f);
            DrawDieCard(rect, dice[i]);
            y += 84f;
        }
    }

    private void DrawTableDice(Rect area)
    {
        if (dice.Count == 0)
        {
            GUI.Label(new Rect(area.x + 20f, area.y + 92f, area.width - 40f, 36f), "骰袋为空。", centerStyle);
            return;
        }

        bool useProcessVisuals = UseDiceProcessVisuals();
        if (!useProcessVisuals && (rollPhase == RollPhase.Shaking || rollPhase == RollPhase.Stopping))
        {
            DrawDiceCup(area);
            return;
        }

        List<TableDieView> displayDice = BuildTableDieViews();
        int displayCount = displayDice.Count;
        int rows = displayCount > 8 ? 2 : 1;
        int columns = rows == 1 ? displayCount : Mathf.CeilToInt(displayCount / 2f);
        float gap = rows == 1 ? 14f : 8f;
        float maxCardWidth = rows == 1 ? 86f : 54f;
        float cardWidth = Mathf.Min(maxCardWidth, (area.width - gap * Mathf.Max(0, columns - 1)) / Mathf.Max(1, columns));
        cardWidth = Mathf.Max(30f, cardWidth);
        float rowGap = rows == 1 ? 0f : 48f;
        float rowHeight = cardWidth + rowGap;
        float firstY = rows == 1 ? area.y + 18f : area.y + 6f;

        for (int i = 0; i < displayCount; i++)
        {
            TableDieView view = displayDice[i];
            Die die = view.Die;
            int row = rows == 1 ? 0 : i / columns;
            int column = rows == 1 ? i : i % columns;
            int rowCount = rows == 1 || row == 0 ? Mathf.Min(columns, displayCount) : displayCount - columns;
            float rowStartX = area.x + (area.width - rowCount * cardWidth - Mathf.Max(0, rowCount - 1) * gap) * 0.5f;
            float scale = ScoreScaleForView(view);
            float scaledWidth = cardWidth * scale;
            Rect dieRect = new Rect(rowStartX + column * (cardWidth + gap) - (scaledWidth - cardWidth) * 0.5f, firstY + row * rowHeight - (scaledWidth - cardWidth) * 0.5f, scaledWidth, scaledWidth);
            int value = view.Value;
            if (useProcessVisuals)
            {
                DrawDiceProcessToken(area, dieRect, view, i, displayCount);
            }
            else
            {
                DrawDieToken(dieRect, die, value);
            }

            if (IsScoreViewActive(view))
            {
                Rect highlight = new Rect(dieRect.x - 8f, dieRect.y - 8f, dieRect.width + 16f, dieRect.height + 16f);
                DrawBorder(highlight, new Color(1f, 0.79f, 0.22f, 0.98f), 4f);
                DrawBorder(new Rect(highlight.x - 4f, highlight.y - 4f, highlight.width + 8f, highlight.height + 8f), new Color(0.45f, 0.28f, 0.08f, 0.65f), 2f);
            }

            if (!die.Temporary && rollPhase == RollPhase.CheatEdit && IsCheatRerollSelected(die.Id))
            {
                DrawRect(new Rect(dieRect.x - 5f, dieRect.y - 5f, dieRect.width + 10f, 4f), new Color(1f, 0.82f, 0.28f));
                DrawRect(new Rect(dieRect.x - 5f, dieRect.y + dieRect.height + 1f, dieRect.width + 10f, 4f), new Color(1f, 0.82f, 0.28f));
            }

            if (!die.Temporary && rollPhase == RollPhase.CheatEdit && GUI.Button(dieRect, GUIContent.none, GUIStyle.none))
            {
                ToggleCheatReroll(die);
            }

            if (IsScoreViewActive(view))
            {
                SettlementDisplayEvent viewEvent = ActiveSettlementEventForView(view);
                float progress = SettlementEventProgress();
                if (viewEvent == null || viewEvent.Kind == SettlementEventKind.SlotScore)
                {
                    DrawScoreFloat(dieRect, ScoreFloatValueForView(view), progress);
                }

                DrawSettlementDieEventTag(area, dieRect, viewEvent, progress);
                DrawSettlementCoinFlight(dieRect, viewEvent, progress);
            }

            int dieIndex = die != null && !die.Temporary ? dice.IndexOf(die) : -1;
            if (dieIndex >= 0)
            {
                TrySetHoveredTooltip(dieRect, die, true, "table-die-" + die.Id);

                if (rollPhase == RollPhase.Ready && selectedReadySlotIndex == dieIndex)
                {
                    DrawBorder(new Rect(dieRect.x - 7f, dieRect.y - 7f, dieRect.width + 14f, dieRect.height + 14f), new Color(0.2f, 0.62f, 0.56f, 0.96f), 3f);
                }
            }

            if (rollPhase == RollPhase.Ready && dieIndex >= 0 && GUI.Button(dieRect, GUIContent.none, GUIStyle.none))
            {
                ToggleReadySlotSelection(dieIndex);
            }
        }

        if (rollPhase == RollPhase.Scoring && scoreRevealIndex >= scoringDice.Count)
        {
            int bonus = Mathf.Max(0, resolvedScore - currentScore);
            if (bonus > 0)
            {
                DrawScoreFloat(new Rect(area.x + area.width * 0.5f - 54f, area.y + 126f, 108f, 48f), bonus, SettlementEventProgress());
            }
        }

        DrawSettlementStageFeedback(area);
    }

    private void DrawCheatRerollControls(Rect area)
    {
        if (dice.Count == 0)
        {
            return;
        }

        GUI.Label(new Rect(area.x + 50f, area.y + 190f, area.width - 100f, 24f), "出千：点击选择 1-3 颗实体骰点数 +1。", centerStyle);
        GUI.Label(new Rect(area.x + 80f, area.y + 224f, area.width - 160f, 24f), "已选 " + cheatRerollIds.Count + " / " + MaxCheatRerollDice + "，确认后按骰面上限封顶并直接结算。", centerStyle);
    }

    private void DrawDiceCup(Rect area)
    {
        RollFeedbackConfig config = CurrentRollFeedbackConfig();
        float time = Time.time;
        float frequency = Mathf.Max(1f, config.CupFrequency);
        float reveal = DiceStopRevealProgress();
        float cupPower = Mathf.Lerp(shakePower, Mathf.Min(shakePower, config.StopMinPower), reveal);
        float xOffset = Mathf.Sin(time * frequency) * config.CupXAmplitude * cupPower;
        float yOffset = Mathf.Sin(time * frequency * 1.25f) * config.CupYAmplitude * cupPower;
        float rotation = Mathf.Sin(time * frequency * 0.75f) * config.CupRotationAmplitude * cupPower;
        Rect cup = new Rect(area.x + area.width * 0.5f - 105f + xOffset, area.y + 18f + yOffset, 210f, 210f);
        if (reveal > 0f)
        {
            cup.x += reveal * 42f;
            cup.y -= reveal * 76f;
        }

        DrawRect(new Rect(cup.x + 48f, cup.y + 178f, cup.width - 96f, 20f), new Color(0f, 0f, 0f, Mathf.Lerp(0.34f, 0.12f, reveal)));
        Matrix4x4 oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(rotation, new Vector2(cup.x + cup.width * 0.5f, cup.y + cup.height * 0.5f));
        Color oldColor = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0.46f, reveal));
        if (diceCupTexture != null)
        {
            GUI.DrawTexture(cup, diceCupTexture, ScaleMode.ScaleToFit, true);
        }
        else
        {
            DrawPanel(cup, new Color(0.42f, 0.2f, 0.12f));
            GUI.Label(new Rect(cup.x, cup.y + 82f, cup.width, 34f), "骰盅", centerStyle);
        }
        GUI.color = oldColor;
        GUI.matrix = oldMatrix;

        if (rollPhase == RollPhase.Stopping)
        {
            float stopDuration = Mathf.Max(0.05f, config.StopDuration);
            float t = Mathf.Clamp01(stopTimer / stopDuration);
            GUI.Label(new Rect(area.x + 120f, area.y + 228f, area.width - 240f, 24f), "回落 " + Mathf.RoundToInt(t * 100f) + "%", centerStyle);
        }
    }

    private float DiceStopRevealProgress()
    {
        if (rollPhase != RollPhase.Stopping)
        {
            return 0f;
        }

        RollFeedbackConfig config = CurrentRollFeedbackConfig();
        float stopDuration = config != null ? Mathf.Max(0.05f, config.StopDuration) : 0.6f;
        float t = Mathf.Clamp01(stopTimer / stopDuration);
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.28f, 1f, t));
    }

    private bool UseDiceProcessVisuals()
    {
        return diceProcessVisualsEnabled;
    }

    private void ClearDiceVisualStates()
    {
        diceVisualStates.Clear();
        diceVisualEnterStartTime = -999f;
        diceVisualRollStartTime = -999f;
        diceVisualRevealStartTime = -999f;
        diceVisualImpulseTimer = 0f;
    }

    private void BeginDiceVisualEnter()
    {
        SyncDiceVisualStates();
        diceVisualEnterStartTime = Time.time;
        diceVisualRollStartTime = -999f;
        diceVisualRevealStartTime = -999f;
        diceVisualImpulseTimer = 0f;
    }

    private void BeginDiceVisualRoll()
    {
        SyncDiceVisualStates();
        diceVisualRollStartTime = Time.time;
        diceVisualRevealStartTime = -999f;
        diceVisualImpulseTimer = 0f;
    }

    private void BeginDiceVisualReveal()
    {
        SyncDiceVisualStates();
        diceVisualRevealStartTime = Time.time;
    }

    private void SyncDiceVisualStates()
    {
        for (int i = diceVisualStates.Count - 1; i >= 0; i--)
        {
            if (FindDieById(diceVisualStates[i].DieId) == null)
            {
                diceVisualStates.RemoveAt(i);
            }
        }

        for (int i = 0; i < dice.Count; i++)
        {
            DiceVisualSeed(dice[i], i);
        }
    }

    private float DiceVisualSeed(Die die, int viewIndex)
    {
        if (die == null || die.Id <= 0)
        {
            return NewDiceVisualSeed(-1000 - viewIndex, viewIndex);
        }

        for (int i = 0; i < diceVisualStates.Count; i++)
        {
            if (diceVisualStates[i].DieId == die.Id)
            {
                return diceVisualStates[i].Seed;
            }
        }

        DiceVisualState state = new DiceVisualState
        {
            DieId = die.Id,
            Seed = NewDiceVisualSeed(die.Id, viewIndex)
        };
        diceVisualStates.Add(state);
        return state.Seed;
    }

    private float NewDiceVisualSeed(int dieId, int viewIndex)
    {
        float raw = Mathf.Sin(dieId * 12.9898f + viewIndex * 78.233f) * 437.5453f;
        return Mathf.Abs(raw - Mathf.Floor(raw));
    }

    private void DrawDiceUnderCupHint(Rect area, List<TableDieView> displayDice)
    {
        if (displayDice == null || displayDice.Count == 0)
        {
            return;
        }

        RollFeedbackConfig config = CurrentRollFeedbackConfig();
        float maxPower = config != null ? Mathf.Max(config.MaxPower, config.BasePower, 0.01f) : 1f;
        float power = Mathf.Clamp01(shakePower / maxPower);
        float impulse = Mathf.Clamp01(diceVisualImpulseTimer / DiceVisualImpulseDuration);
        Vector2 center = new Vector2(area.x + area.width * 0.5f, area.y + 156f);
        int count = Mathf.Min(displayDice.Count, DiceCapacity);
        for (int i = 0; i < count; i++)
        {
            TableDieView view = displayDice[i];
            Die die = view != null ? view.Die : null;
            DieType visualType = die != null && !die.Temporary ? die.Type : DieType.Turtle;
            float seed = DiceVisualSeed(die, i);
            float time = Time.time * (8.5f + power * 5.5f) + seed * 6.283f;
            float angle = (i / Mathf.Max(1f, count)) * Mathf.PI * 2f + Mathf.Sin(time * 0.6f) * 0.18f;
            float radiusX = 40f + seed * 42f;
            float radiusY = 14f + seed * 18f;
            float size = 14f + power * 10f + impulse * 5f;
            float x = center.x + Mathf.Cos(angle + time * 0.18f) * radiusX - size * 0.5f;
            float y = center.y + Mathf.Sin(angle + time * 0.25f) * radiusY - size * 0.25f;
            Color typeColor = TypeColor(visualType);
            DrawTintedCircle(new Rect(x, y, size, size * 0.52f), new Color(typeColor.r, typeColor.g, typeColor.b, 0.06f + power * 0.09f + impulse * 0.04f));
            DrawTintedCircle(new Rect(x + size * 0.22f, y + size * 0.08f, size * 0.56f, size * 0.28f), new Color(1f, 0.9f, 0.62f, 0.03f + power * 0.05f));
        }
    }

    private void DrawDiceProcessToken(Rect area, Rect baseRect, TableDieView view, int viewIndex, int displayCount)
    {
        if (view == null || view.Die == null)
        {
            DrawPanel(baseRect, new Color(0.9f, 0.82f, 0.62f, 0.82f));
            return;
        }

        if (view.Summary)
        {
            DrawDieToken(baseRect, view.Die, view.Value);
            return;
        }

        Die die = view.Die;
        float seed = DiceVisualSeed(die, viewIndex);
        DiceVisualMotion motion = CurrentDiceVisualMotion(viewIndex, displayCount);
        DiceVisualProfile profile = DiceVisualProfileForDie(die);
        int visualValue = DiceVisualDisplayValue(view.Value, seed, viewIndex, displayCount, motion);
        Rect visualRect = DiceVisualAnimatedRect(area, baseRect, seed, viewIndex, displayCount, motion, view, profile);
        if (rollPhase == RollPhase.Ready)
        {
            DrawReadyDieToken(visualRect, die, die.Temporary ? DieType.Turtle : die.Type);
            return;
        }

        int sequenceFrame = DiceVisualSequenceFrameIndex(seed, viewIndex, displayCount, motion, profile);
        float stopProgress = DiceVisualSlotStopProgress(viewIndex, displayCount);
        float revealProgress = DiceVisualSlotRevealProgress(viewIndex, displayCount, motion);
        DrawDiceSequenceFrame(visualRect, die, visualValue, sequenceFrame, stopProgress, revealProgress, seed, profile);
    }

    private DiceVisualMotion CurrentDiceVisualMotion(int viewIndex, int displayCount)
    {
        if (rollPhase == RollPhase.Shaking || rollPhase == RollPhase.Stopping)
        {
            return DiceVisualMotion.Rolling;
        }

        if (rollPhase == RollPhase.ResultDecision && Time.time - diceVisualRevealStartTime < DiceVisualRevealDuration)
        {
            return DiceVisualMotion.Reveal;
        }

        float entrySpan = DiceVisualEnterDuration + Mathf.Max(0, displayCount - 1) * DiceVisualEnterStagger;
        if (rollPhase == RollPhase.Ready && Time.time - diceVisualEnterStartTime < entrySpan + 0.12f)
        {
            return DiceVisualMotion.Entry;
        }

        return DiceVisualMotion.Settled;
    }

    private int DiceVisualDisplayValue(int targetValue, float seed, int viewIndex, int displayCount, DiceVisualMotion motion)
    {
        if (motion == DiceVisualMotion.Rolling)
        {
            if (rollPhase == RollPhase.Stopping && targetValue > 0)
            {
                return DiceVisualSlotStopProgress(viewIndex, displayCount) >= 0.82f ? targetValue : 0;
            }

            return 0;
        }

        if (motion == DiceVisualMotion.Reveal && targetValue > 0)
        {
            if (DiceVisualSlotRevealProgress(viewIndex, displayCount, motion) < 0.72f)
            {
                return 0;
            }
        }

        return Mathf.Max(0, targetValue);
    }

    private int DiceVisualSequenceFrameIndex(float seed, int viewIndex, int displayCount, DiceVisualMotion motion, DiceVisualProfile profile)
    {
        if (motion == DiceVisualMotion.Entry || motion == DiceVisualMotion.Settled)
        {
            return 0;
        }

        float stopProgress = DiceVisualSlotStopProgress(viewIndex, displayCount);
        if (rollPhase == RollPhase.Stopping && stopProgress >= 0.82f)
        {
            return 0;
        }

        float elapsed = diceVisualRollStartTime > -900f ? Mathf.Max(0f, Time.time - diceVisualRollStartTime) : Time.time;
        RollFeedbackConfig config = CurrentRollFeedbackConfig();
        float maxPower = config != null ? Mathf.Max(config.MaxPower, config.BasePower, 0.01f) : 1f;
        float power = Mathf.Clamp01(shakePower / maxPower);
        float impulse = Mathf.Clamp01(diceVisualImpulseTimer / DiceVisualImpulseDuration);
        float profileSpeed = DiceVisualProfileFrameSpeed(profile);
        float fps = Mathf.Lerp(9.5f, 15.5f, power) * profileSpeed + impulse * 3.5f;

        if (rollPhase == RollPhase.Stopping)
        {
            fps = Mathf.Lerp(fps, 1.2f, stopProgress);
        }

        int frame = Mathf.FloorToInt(elapsed * fps + seed * DiceVisualSequenceFrameCount + viewIndex * 2.13f);
        return Mathf.Abs(frame) % DiceVisualSequenceFrameCount;
    }

    private float DiceVisualSlotStopProgress(int viewIndex, int displayCount)
    {
        if (rollPhase != RollPhase.Stopping)
        {
            return rollPhase == RollPhase.ResultDecision || rollPhase == RollPhase.CheatEdit || rollPhase == RollPhase.Scoring || rollPhase == RollPhase.StageClear ? 1f : 0f;
        }

        RollFeedbackConfig config = CurrentRollFeedbackConfig();
        float stopDuration = config != null ? Mathf.Max(0.05f, config.StopDuration) : 0.9f;
        float t = Mathf.Clamp01(stopTimer / stopDuration);
        float slot = displayCount <= 1 ? 0f : Mathf.Clamp01(viewIndex / Mathf.Max(1f, displayCount - 1f));
        float start = Mathf.Lerp(0f, 0.46f, slot);
        float end = Mathf.Min(0.96f, start + 0.38f);
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(start, end, t));
    }

    private float DiceVisualSlotRevealProgress(int viewIndex, int displayCount, DiceVisualMotion motion)
    {
        if (motion == DiceVisualMotion.Settled)
        {
            return 1f;
        }

        if (motion != DiceVisualMotion.Reveal)
        {
            return 0f;
        }

        float t = Mathf.Clamp01((Time.time - diceVisualRevealStartTime) / DiceVisualRevealDuration);
        float slot = displayCount <= 1 ? 0f : Mathf.Clamp01(viewIndex / Mathf.Max(1f, displayCount - 1f));
        float start = slot * 0.34f;
        float end = Mathf.Min(1f, start + 0.34f);
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(start, end, t));
    }

    private int RollingVisualValue(float seed, int viewIndex, float speedScale)
    {
        float speed = DiceVisualRollSpeed * Mathf.Max(0.35f, speedScale);
        int step = Mathf.FloorToInt((Time.time * speed + seed * 19.7f + viewIndex * 1.31f) * 1.37f);
        return 1 + Mathf.Abs(step) % 6;
    }

    private Rect DiceVisualAnimatedRect(Rect area, Rect baseRect, float seed, int viewIndex, int displayCount, DiceVisualMotion motion, TableDieView view, DiceVisualProfile profile)
    {
        Rect rect = baseRect;
        float scale = 1f;

        if (motion == DiceVisualMotion.Entry)
        {
            float delay = viewIndex * DiceVisualEnterStagger;
            float t = Mathf.Clamp01((Time.time - diceVisualEnterStartTime - delay) / DiceVisualEnterDuration);
            float ease = EaseOutBack(t);
            float startX = (seed - 0.5f) * 16f;
            float startY = -20f - viewIndex * 1.4f;
            rect.x += Mathf.Lerp(startX, 0f, ease);
            rect.y += Mathf.Lerp(startY, 0f, ease);
            rect.y -= Mathf.Sin(Mathf.Clamp01(t) * Mathf.PI) * 4f;
            scale = Mathf.Lerp(0.94f, 1f, Mathf.Clamp01(t));
        }
        else if (motion == DiceVisualMotion.Rolling)
        {
            RollFeedbackConfig config = CurrentRollFeedbackConfig();
            float maxPower = config != null ? Mathf.Max(config.MaxPower, config.BasePower, 0.01f) : 1f;
            float power = Mathf.Clamp01(shakePower / maxPower);
            float impulse = Mathf.Clamp01(diceVisualImpulseTimer / DiceVisualImpulseDuration);
            float stopProgress = DiceVisualSlotStopProgress(viewIndex, displayCount);
            scale = 1f + (0.012f + impulse * 0.018f + power * 0.006f) * (1f - stopProgress);
        }
        else if (motion == DiceVisualMotion.Reveal)
        {
            float t = Mathf.Clamp01((Time.time - diceVisualRevealStartTime) / DiceVisualRevealDuration);
            rect.y -= Mathf.Sin(t * Mathf.PI) * 5f;
            scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.04f;
        }

        if (IsScoreViewActive(view))
        {
            float t = SettlementEventProgress();
            rect.y -= Mathf.Sin(t * Mathf.PI) * 10f;
        }

        return ScaleRect(rect, Mathf.Clamp(scale, 0.72f, 1.18f));
    }

    private float DiceVisualRotation(float seed, int viewIndex, DiceVisualMotion motion, DiceVisualProfile profile)
    {
        if (motion == DiceVisualMotion.Entry)
        {
            float delay = viewIndex * DiceVisualEnterStagger;
            float t = Mathf.Clamp01((Time.time - diceVisualEnterStartTime - delay) / DiceVisualEnterDuration);
            return Mathf.Lerp(-18f + seed * 36f, 0f, Mathf.SmoothStep(0f, 1f, t));
        }

        if (motion == DiceVisualMotion.Rolling)
        {
            RollFeedbackConfig config = CurrentRollFeedbackConfig();
            float maxPower = config != null ? Mathf.Max(config.MaxPower, config.BasePower, 0.01f) : 1f;
            float power = Mathf.Clamp01(shakePower / maxPower);
            float speed = DiceVisualProfileSpeed(profile);
            float amplitude = DiceVisualProfileRotationAmplitude(profile);
            float reveal = DiceStopRevealProgress();
            float time = Time.time * ((64f + power * 36f) * speed) + seed * 360f + viewIndex * 19f;
            float wave = Mathf.Sin(time * Mathf.Deg2Rad);
            if (profile == DiceVisualProfile.Parity)
            {
                wave = Mathf.Sign(wave) * Mathf.Pow(Mathf.Abs(wave), 0.62f);
            }
            else if (profile == DiceVisualProfile.Gold)
            {
                wave = Mathf.Sin(time * Mathf.Deg2Rad) * 0.7f + Mathf.Sin(time * 2.4f * Mathf.Deg2Rad) * 0.3f;
            }

            return wave * amplitude * Mathf.Lerp(1f, 0.12f, reveal);
        }

        if (motion == DiceVisualMotion.Reveal)
        {
            float t = Mathf.Clamp01((Time.time - diceVisualRevealStartTime) / DiceVisualRevealDuration);
            float amplitude = DiceVisualProfileRotationAmplitude(profile);
            return Mathf.Sin(t * Mathf.PI * DiceVisualProfileRevealTurns(profile) + seed * 3f) * (1f - t) * amplitude * 0.7f;
        }

        return 0f;
    }

    private float DiceVisualRollAmount(float seed, int viewIndex, DiceVisualMotion motion, DiceVisualProfile profile)
    {
        if (motion == DiceVisualMotion.Rolling)
        {
            float reveal = DiceStopRevealProgress();
            RollFeedbackConfig config = CurrentRollFeedbackConfig();
            float maxPower = config != null ? Mathf.Max(config.MaxPower, config.BasePower, 0.01f) : 1f;
            float power = Mathf.Clamp01(shakePower / maxPower);
            float impulse = Mathf.Clamp01(diceVisualImpulseTimer / DiceVisualImpulseDuration);
            float speed = DiceVisualProfileSpeed(profile);
            float amount = 0.5f + 0.5f * Mathf.Sin(Time.time * ((DiceVisualRollSpeed + power * 4.4f) * speed) + seed * 6.283f + viewIndex * 0.72f);
            amount = Mathf.Clamp01(amount * DiceVisualProfileRollWeight(profile) + impulse * 0.2f);
            return amount * Mathf.Lerp(1f, 0.04f, reveal);
        }

        if (motion == DiceVisualMotion.Reveal)
        {
            float t = Mathf.Clamp01((Time.time - diceVisualRevealStartTime) / DiceVisualRevealDuration);
            return Mathf.Abs(Mathf.Sin(t * Mathf.PI * DiceVisualProfileRevealTurns(profile))) * (1f - t) * 0.78f * DiceVisualProfileRollWeight(profile);
        }

        if (motion == DiceVisualMotion.Entry)
        {
            float delay = viewIndex * DiceVisualEnterStagger;
            float t = Mathf.Clamp01((Time.time - diceVisualEnterStartTime - delay) / DiceVisualEnterDuration);
            return (1f - t) * 0.18f;
        }

        return 0f;
    }

    private void DrawDiceSequenceFrame(Rect rect, Die die, int value, int frameIndex, float stopProgress, float revealProgress, float seed, DiceVisualProfile profile)
    {
        DieType visualType = die.Temporary ? DieType.Turtle : die.Type;
        DiceMaterial material = die.Temporary ? DiceMaterial.None : die.Material;
        Color typeColor = TypeColor(visualType);
        bool resultVisible = value > 0;
        if (resultVisible && DrawUnifiedResultDieFace(rect, value, material))
        {
            DrawProcessDieTypeMarker(rect, visualType);
            return;
        }

        if (!resultVisible && DrawDiceRollSpriteSequenceFrame(rect, visualType, frameIndex, stopProgress, seed, profile))
        {
            return;
        }

        int frame = Mathf.Abs(frameIndex) % DiceVisualGeneratedSequenceFrameCount;
        float turn = resultVisible ? 0f : DiceSequenceFrameTurn(frame);
        if (!resultVisible && stopProgress >= 0.98f)
        {
            turn = 0f;
            frame = 0;
        }

        bool verticalAxis = DiceSequenceUsesVerticalAxis(profile, frame, seed);
        bool backFrame = DiceSequenceShowsBack(frame);
        float sideSign = DiceSequenceSideSign(frame, seed);
        Color frontColor = resultVisible
            ? new Color(0.98f, 0.9f, 0.68f, 0.98f)
            : Color.Lerp(new Color(0.98f, 0.9f, 0.68f, 0.98f), typeColor, backFrame ? 0.32f : 0.16f);
        Color sideColor = Color.Lerp(frontColor, new Color(0.46f, 0.32f, 0.18f, 0.96f), 0.28f + turn * 0.18f);
        Color topColor = Color.Lerp(frontColor, Color.white, 0.22f + (1f - turn) * 0.08f);
        Color edgeColor = new Color(0.22f, 0.14f, 0.08f, 0.9f);
        float depth = Mathf.Clamp(rect.width * (0.1f + turn * 0.16f), 5f, 17f);

        Rect shadow = new Rect(rect.x + rect.width * 0.12f, rect.y + rect.height * (0.86f + turn * 0.02f), rect.width * (0.86f - turn * 0.12f), rect.height * (0.19f - turn * 0.035f));
        DrawTintedCircle(shadow, new Color(0f, 0f, 0f, 0.13f + turn * 0.08f));

        float faceWidth = verticalAxis ? rect.width * Mathf.Lerp(1f, 0.28f, turn) : rect.width * Mathf.Lerp(0.96f, 0.9f, turn);
        float faceHeight = verticalAxis ? rect.height * Mathf.Lerp(0.96f, 0.9f, turn) : rect.height * Mathf.Lerp(1f, 0.3f, turn);
        faceWidth = Mathf.Max(rect.width * 0.28f, faceWidth);
        faceHeight = Mathf.Max(rect.height * 0.3f, faceHeight);
        Rect face = new Rect(rect.x + (rect.width - faceWidth) * 0.5f, rect.y + (rect.height - faceHeight) * 0.5f, faceWidth, faceHeight);

        Rect back = new Rect(face.x + depth * 0.45f * sideSign, face.y - depth * 0.45f, face.width, face.height);
        if (turn > 0.08f)
        {
            DrawRect(back, Color.Lerp(sideColor, typeColor, backFrame ? 0.18f : 0.06f));
        }

        if (verticalAxis)
        {
            Rect side = sideSign > 0f
                ? new Rect(face.x + face.width, face.y - depth * 0.45f, depth, face.height + depth * 0.45f)
                : new Rect(face.x - depth, face.y - depth * 0.45f, depth, face.height + depth * 0.45f);
            DrawRect(side, sideColor);
            DrawRect(new Rect(face.x + depth * 0.12f * sideSign, face.y - depth * 0.45f, face.width, Mathf.Max(3f, depth * 0.62f)), topColor);
        }
        else
        {
            Rect top = new Rect(face.x + depth * 0.18f * sideSign, face.y - depth, face.width, Mathf.Max(3f, depth));
            Rect bottom = new Rect(face.x - depth * 0.12f * sideSign, face.y + face.height, face.width, Mathf.Max(3f, depth * 0.62f));
            DrawRect(top, topColor);
            DrawRect(bottom, sideColor);
        }

        DrawPanel(face, frontColor);
        DrawDiceMaterialOverlay(InsetRect(face, face.width * 0.08f, face.height * 0.08f), material, resultVisible ? 0.12f : 0.18f);
        DrawRect(new Rect(face.x + face.width * 0.08f, face.y + face.height * 0.08f, face.width * 0.84f, Mathf.Clamp(face.height * 0.07f, 3f, 6f)), new Color(1f, 1f, 1f, 0.22f));
        DrawBorder(face, edgeColor, resultVisible ? 2f : Mathf.Lerp(2f, 3f, turn));

        if (resultVisible)
        {
            Rect pipArea = InsetRect(face, face.width * 0.24f, face.height * 0.22f);
            DrawDiePips(pipArea, value, new Color(0.08f, 0.08f, 0.07f));
            DrawProcessDieTypeMarker(face, visualType);
            return;
        }

        DrawSequenceFrameIdentity(face, visualType, typeColor, frame, turn, revealProgress);
    }

    private bool DrawDiceRollSpriteSequenceFrame(Rect rect, DieType visualType, int frameIndex, float stopProgress, float seed, DiceVisualProfile profile)
    {
        if (rollPhase != RollPhase.Shaking && rollPhase != RollPhase.Stopping)
        {
            return false;
        }

        Texture2D strip = diceRollLoopStripTexture;
        int frameCount = DiceRollLoopFrameCount;
        int sourceFrame = DiceRollLoopFrameForProfile(frameIndex, visualType, profile, seed);
        float slotStop = Mathf.Clamp01(stopProgress);
        bool useStopPreview = rollPhase == RollPhase.Stopping && slotStop > 0.02f && diceRollStopStripTexture != null;
        if (useStopPreview)
        {
            strip = diceRollStopStripTexture;
            frameCount = DiceRollStopFrameCount;
            sourceFrame = DiceRollStopFrameIndex(slotStop, visualType, profile, seed);
        }

        if (strip == null)
        {
            return false;
        }

        sourceFrame = Mathf.Clamp(sourceFrame, 0, frameCount - 1);
        float u = sourceFrame / (float)frameCount;
        Rect source = new Rect(u, 0f, 1f / frameCount, 1f);
        Color oldColor = GUI.color;
        float settleAlpha = Mathf.Lerp(1f, 0.92f, slotStop);
        GUI.color = new Color(1f, 1f, 1f, settleAlpha);
        GUI.DrawTextureWithTexCoords(rect, strip, source, true);
        GUI.color = oldColor;

        DrawDiceRollSpriteMarker(rect, visualType, profile, slotStop, useStopPreview);
        return true;
    }

    private bool DrawUnifiedResultDieFace(Rect rect, int value, DiceMaterial material)
    {
        if (diceRollResultFacesTexture == null || value <= 0 || value > DiceRollResultFaceCount)
        {
            return false;
        }

        float u = (value - 1) / (float)DiceRollResultFaceCount;
        Rect source = new Rect(u, 0f, 1f / DiceRollResultFaceCount, 1f);
        Color oldColor = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTextureWithTexCoords(rect, diceRollResultFacesTexture, source, true);
        GUI.color = oldColor;

        if (material != DiceMaterial.None)
        {
            DrawDiceMaterialOverlay(InsetRect(rect, rect.width * 0.1f, rect.height * 0.12f), material, 0.08f);
        }

        return true;
    }

    private int DiceRollLoopFrameForProfile(int frameIndex, DieType visualType, DiceVisualProfile profile, float seed)
    {
        int frame = Mathf.Abs(frameIndex) % DiceRollLoopFrameCount;
        int offset = Mathf.FloorToInt(seed * DiceRollLoopFrameCount);
        switch (profile)
        {
            case DiceVisualProfile.Turtle:
                frame = (frame + offset / 2) % DiceRollLoopFrameCount;
                break;
            case DiceVisualProfile.Tree:
                frame = (frame + offset / 3 + 2) % DiceRollLoopFrameCount;
                break;
            case DiceVisualProfile.Gold:
                frame = (frame + offset + 4) % DiceRollLoopFrameCount;
                break;
            case DiceVisualProfile.Burst:
                frame = (frame * 2 + offset + 5) % DiceRollLoopFrameCount;
                break;
            case DiceVisualProfile.Parity:
                frame = (DiceRollLoopFrameCount - 1 - frame + offset) % DiceRollLoopFrameCount;
                break;
        }

        if (visualType == DieType.Basic)
        {
            return frame;
        }

        return (frame + Mathf.Abs((int)visualType) % 5) % DiceRollLoopFrameCount;
    }

    private int DiceRollStopFrameIndex(float stopProgress, DieType visualType, DiceVisualProfile profile, float seed)
    {
        float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(stopProgress));
        int frame = Mathf.FloorToInt(eased * (DiceRollStopFrameCount - 1));
        frame = Mathf.Clamp(frame, 0, DiceRollStopFrameCount - 1);
        if (profile == DiceVisualProfile.Burst && stopProgress < 0.72f)
        {
            frame = Mathf.Min(DiceRollStopFrameCount - 2, frame + 1);
        }
        else if ((profile == DiceVisualProfile.Turtle || profile == DiceVisualProfile.Tree) && stopProgress < 0.82f)
        {
            frame = Mathf.Max(0, frame - 1);
        }

        if (visualType != DieType.Basic && seed > 0.62f && stopProgress < 0.64f)
        {
            frame = Mathf.Min(DiceRollStopFrameCount - 2, frame + 1);
        }

        return frame;
    }

    private void DrawDiceRollSpriteMarker(Rect rect, DieType visualType, DiceVisualProfile profile, float stopProgress, bool useStopPreview)
    {
        if (visualType == DieType.Basic)
        {
            return;
        }

        Color typeColor = TypeColor(visualType);
        float alpha = 0.58f;
        alpha *= useStopPreview ? Mathf.Lerp(0.86f, 0.52f, stopProgress) : 1f;
        float stripHeight = Mathf.Clamp(rect.height * 0.055f, 3f, 5f);
        Rect strip = new Rect(rect.x + rect.width * 0.21f, rect.y + rect.height * 0.83f, rect.width * 0.58f, stripHeight);
        DrawRect(strip, new Color(typeColor.r, typeColor.g, typeColor.b, alpha));

        float badgeSize = Mathf.Clamp(rect.width * 0.24f, 14f, 21f);
        Rect badgeRect = new Rect(rect.x + rect.width * 0.68f, rect.y + rect.height * 0.04f, badgeSize, badgeSize);
        DrawTintedCircle(badgeRect, new Color(1f, 0.91f, 0.66f, 0.72f));
        DrawBorder(badgeRect, new Color(typeColor.r, typeColor.g, typeColor.b, 0.72f), 1.5f);
        Texture2D icon = DieTypeIcon(visualType);
        Rect iconRect = InsetRect(badgeRect, badgeSize * 0.2f, badgeSize * 0.2f);
        if (icon != null)
        {
            Color old = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, 0.9f);
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            GUI.color = old;
        }

        if (profile == DiceVisualProfile.Gold)
        {
            DrawTintedCircle(new Rect(rect.x + rect.width * 0.18f, rect.y + rect.height * 0.72f, rect.width * 0.1f, rect.height * 0.05f), new Color(1f, 0.76f, 0.22f, 0.22f));
        }
        else if (profile == DiceVisualProfile.Burst)
        {
            DrawRect(new Rect(rect.x + rect.width * 0.25f, rect.y + rect.height * 0.18f, rect.width * 0.5f, Mathf.Clamp(rect.height * 0.035f, 2f, 4f)), new Color(typeColor.r, typeColor.g, typeColor.b, 0.18f));
        }
    }

    private float DiceSequenceFrameTurn(int frame)
    {
        switch (frame % DiceVisualGeneratedSequenceFrameCount)
        {
            case 0:
                return 0f;
            case 1:
                return 0.22f;
            case 2:
                return 0.52f;
            case 3:
                return 0.82f;
            case 4:
                return 0.96f;
            case 5:
                return 0.82f;
            case 6:
                return 0.52f;
            case 7:
                return 0.22f;
            case 8:
                return 0.08f;
            case 9:
                return 0.38f;
            case 10:
                return 0.68f;
        }

        return 0.32f;
    }

    private bool DiceSequenceShowsBack(int frame)
    {
        int phase = frame % DiceVisualGeneratedSequenceFrameCount;
        return (phase >= 3 && phase <= 5) || phase == 10;
    }

    private bool DiceSequenceUsesVerticalAxis(DiceVisualProfile profile, int frame, float seed)
    {
        if (profile == DiceVisualProfile.Turtle || profile == DiceVisualProfile.Tree)
        {
            return false;
        }

        if (profile == DiceVisualProfile.Parity)
        {
            return true;
        }

        if (profile == DiceVisualProfile.Burst)
        {
            return frame % 3 != 1;
        }

        if (profile == DiceVisualProfile.Gold)
        {
            return frame % 2 == 0;
        }

        return seed > 0.46f;
    }

    private float DiceSequenceSideSign(int frame, float seed)
    {
        int phase = frame % DiceVisualGeneratedSequenceFrameCount;
        if (phase >= 1 && phase <= 5)
        {
            return seed > 0.5f ? 1f : -1f;
        }

        return seed > 0.5f ? -1f : 1f;
    }

    private float DiceVisualProfileFrameSpeed(DiceVisualProfile profile)
    {
        switch (profile)
        {
            case DiceVisualProfile.Turtle:
                return 0.78f;
            case DiceVisualProfile.Tree:
                return 0.86f;
            case DiceVisualProfile.Gold:
                return 1.08f;
            case DiceVisualProfile.Burst:
                return 1.22f;
            case DiceVisualProfile.Parity:
                return 1.04f;
        }

        return 1f;
    }

    private void DrawSequenceFrameIdentity(Rect face, DieType visualType, Color typeColor, int frame, float turn, float revealProgress)
    {
        float stripHeight = Mathf.Clamp(face.height * 0.08f, 4f, 7f);
        Color stripColor = new Color(typeColor.r, typeColor.g, typeColor.b, Mathf.Lerp(0.8f, 0.52f, turn));
        DrawRect(new Rect(face.x + face.width * 0.14f, face.y + face.height * 0.13f, face.width * 0.72f, stripHeight), stripColor);
        DrawRect(new Rect(face.x + face.width * 0.14f, face.y + face.height * 0.82f - stripHeight, face.width * 0.72f, stripHeight), new Color(typeColor.r, typeColor.g, typeColor.b, 0.34f));

        float iconScale = Mathf.Lerp(0.54f, 0.32f, turn);
        float iconSize = Mathf.Min(face.width, face.height) * iconScale;
        Rect iconRect = new Rect(face.x + (face.width - iconSize) * 0.5f, face.y + (face.height - iconSize) * 0.5f, iconSize, iconSize);
        if (iconSize >= 12f)
        {
            DrawDieTypeIcon(iconRect, visualType);
        }

        if (turn > 0.55f)
        {
            Rect glint = new Rect(face.x + face.width * 0.3f, face.y + face.height * 0.28f, face.width * 0.4f, Mathf.Clamp(face.height * 0.06f, 2f, 5f));
            DrawRect(glint, new Color(1f, 0.92f, 0.58f, 0.12f + revealProgress * 0.08f));
        }
    }

    private void DrawDiceProcessCube(Rect rect, Die die, int value, float rotation, float rollAmount, float seed, DiceVisualProfile profile)
    {
        DieType visualType = die.Temporary ? DieType.Turtle : die.Type;
        DiceMaterial material = die.Temporary ? DiceMaterial.None : die.Material;
        Color typeColor = TypeColor(visualType);
        float turn = Mathf.Clamp01(rollAmount);
        bool verticalAxis = profile == DiceVisualProfile.Parity || profile == DiceVisualProfile.Gold || (seed > 0.68f && profile != DiceVisualProfile.Turtle);
        if (profile == DiceVisualProfile.Tree || profile == DiceVisualProfile.Turtle)
        {
            verticalAxis = false;
        }

        Color faceColor = value > 0
            ? new Color(0.98f, 0.9f, 0.68f, 0.98f)
            : Color.Lerp(new Color(0.98f, 0.9f, 0.68f, 0.98f), typeColor, 0.18f);
        Color sideColor = Color.Lerp(faceColor, new Color(0.46f, 0.32f, 0.18f, 0.96f), 0.32f + turn * 0.16f);
        Color topColor = Color.Lerp(faceColor, Color.white, 0.2f + (1f - turn) * 0.08f);
        Color edgeColor = new Color(0.22f, 0.14f, 0.08f, 0.9f);
        float depth = Mathf.Clamp(rect.width * (0.12f + turn * 0.18f), 5f, 19f);

        Rect shadow = new Rect(rect.x + rect.width * (0.12f + turn * 0.04f), rect.y + rect.height * (0.86f + turn * 0.02f), rect.width * (0.86f - turn * 0.12f), rect.height * (0.2f - turn * 0.04f));
        DrawTintedCircle(shadow, new Color(0f, 0f, 0f, 0.14f + turn * 0.1f));

        Matrix4x4 oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(rotation, new Vector2(rect.x + rect.width * 0.5f, rect.y + rect.height * 0.5f));

        float insetX = verticalAxis ? rect.width * Mathf.Lerp(0.02f, 0.22f, turn) : rect.width * Mathf.Lerp(0.02f, 0.06f, turn);
        float insetY = verticalAxis ? rect.height * Mathf.Lerp(0.02f, 0.06f, turn) : rect.height * Mathf.Lerp(0.02f, 0.23f, turn);
        Rect face = new Rect(rect.x + insetX, rect.y + insetY, rect.width - insetX * 2f, rect.height - insetY * 2f);
        face.width = Mathf.Max(rect.width * 0.52f, face.width);
        face.height = Mathf.Max(rect.height * 0.5f, face.height);
        face.x = rect.x + (rect.width - face.width) * 0.5f;
        face.y = rect.y + (rect.height - face.height) * 0.5f;

        float sideSign = seed > 0.5f ? 1f : -1f;
        Rect back = new Rect(face.x + depth * 0.55f * sideSign, face.y - depth * 0.5f, face.width, face.height);
        DrawRect(back, Color.Lerp(sideColor, typeColor, value > 0 ? 0.04f : 0.14f));
        if (verticalAxis)
        {
            Rect side = sideSign > 0f
                ? new Rect(face.x + face.width, face.y - depth * 0.5f, depth, face.height + depth * 0.5f)
                : new Rect(face.x - depth, face.y - depth * 0.5f, depth, face.height + depth * 0.5f);
            DrawRect(side, sideColor);
            DrawRect(new Rect(face.x + depth * 0.16f * sideSign, face.y - depth * 0.5f, face.width, Mathf.Max(3f, depth * 0.72f)), topColor);
        }
        else
        {
            Rect top = new Rect(face.x + depth * 0.22f * sideSign, face.y - depth, face.width, depth);
            Rect bottom = new Rect(face.x - depth * 0.18f * sideSign, face.y + face.height, face.width, Mathf.Max(3f, depth * 0.72f));
            DrawRect(top, topColor);
            DrawRect(bottom, sideColor);
        }
        DrawBorder(back, new Color(edgeColor.r, edgeColor.g, edgeColor.b, 0.36f), 1f);

        DrawPanel(face, faceColor);
        DrawDiceMaterialOverlay(InsetRect(face, face.width * 0.08f, face.height * 0.08f), material, value > 0 ? 0.14f : 0.22f);
        DrawRect(new Rect(face.x + face.width * 0.08f, face.y + face.height * 0.08f, face.width * 0.84f, Mathf.Clamp(face.height * 0.07f, 3f, 6f)), new Color(1f, 1f, 1f, 0.24f));
        DrawBorder(face, edgeColor, turn > 0.1f ? 3f : 2f);

        if (value > 0)
        {
            Rect pipArea = InsetRect(face, face.width * 0.24f, face.height * 0.22f);
            DrawDiePips(pipArea, value, new Color(0.08f, 0.08f, 0.07f));
            DrawProcessDieTypeMarker(face, visualType);
        }
        else
        {
            DrawRect(new Rect(face.x + face.width * 0.12f, face.y + face.height * 0.12f, face.width * 0.76f, Mathf.Clamp(face.height * 0.08f, 4f, 7f)), new Color(typeColor.r, typeColor.g, typeColor.b, 0.82f));
            Rect iconRect = new Rect(face.x + face.width * 0.24f, face.y + face.height * 0.26f, face.width * 0.52f, face.height * 0.52f);
            DrawDieTypeIcon(iconRect, visualType);
        }

        GUI.matrix = oldMatrix;
    }

    private DiceVisualProfile DiceVisualProfileForDie(Die die)
    {
        if (die == null)
        {
            return DiceVisualProfile.Balanced;
        }

        DieType type = die.Temporary ? DieType.Turtle : die.Type;
        switch (type)
        {
            case DieType.Turtle:
            case DieType.Shellsmith:
            case DieType.Nest:
            case DieType.SlowTurtle:
            case DieType.ShellTax:
                return DiceVisualProfile.Turtle;
            case DieType.Odd:
            case DieType.Even:
            case DieType.LoneWitness:
            case DieType.Stamp:
            case DieType.HalfStep:
            case DieType.Track:
            case DieType.ParityNeighborDiff:
            case DieType.ParityNeighborSame:
            case DieType.ParityComplete:
            case DieType.ParityReview:
            case DieType.ParityFlipScore:
            case DieType.ParityHoldScore:
            case DieType.ParityTurner:
                return DiceVisualProfile.Parity;
            case DieType.Tree:
            case DieType.Gardener:
            case DieType.Irrigation:
            case DieType.PointSeedTree:
            case DieType.PatternTree:
            case DieType.CanopyTree:
            case DieType.RingTree:
            case DieType.FertilizerTree:
            case DieType.PruningTree:
            case DieType.RootTree:
                return DiceVisualProfile.Tree;
            case DieType.Piggy:
            case DieType.Treasury:
            case DieType.Bribe:
            case DieType.Investment:
            case DieType.BountyGold:
            case DieType.TopGold:
            case DieType.HandTax:
            case DieType.Collection:
            case DieType.CompoundInterest:
            case DieType.LeadTicket:
            case DieType.CounterGold:
            case DieType.LumberGold:
                return DiceVisualProfile.Gold;
            case DieType.Double:
            case DieType.Gambler:
                return DiceVisualProfile.Burst;
        }

        return DiceVisualProfile.Balanced;
    }

    private float DiceVisualProfileSpeed(DiceVisualProfile profile)
    {
        switch (profile)
        {
            case DiceVisualProfile.Turtle:
                return 0.66f;
            case DiceVisualProfile.Tree:
                return 0.78f;
            case DiceVisualProfile.Gold:
                return 1.08f;
            case DiceVisualProfile.Burst:
                return 1.34f;
            case DiceVisualProfile.Parity:
                return 1.02f;
        }

        return 0.92f;
    }

    private float DiceVisualProfileXAmplitude(DiceVisualProfile profile)
    {
        switch (profile)
        {
            case DiceVisualProfile.Turtle:
                return 1.6f;
            case DiceVisualProfile.Tree:
                return 1.3f;
            case DiceVisualProfile.Gold:
                return 2.1f;
            case DiceVisualProfile.Burst:
                return 2.6f;
            case DiceVisualProfile.Parity:
                return 1.9f;
        }

        return 1.7f;
    }

    private float DiceVisualProfileYAmplitude(DiceVisualProfile profile)
    {
        switch (profile)
        {
            case DiceVisualProfile.Turtle:
                return 1.2f;
            case DiceVisualProfile.Tree:
                return 1f;
            case DiceVisualProfile.Gold:
                return 1.6f;
            case DiceVisualProfile.Burst:
                return 2.2f;
            case DiceVisualProfile.Parity:
                return 1.3f;
        }

        return 1.4f;
    }

    private float DiceVisualProfileRotationAmplitude(DiceVisualProfile profile)
    {
        switch (profile)
        {
            case DiceVisualProfile.Turtle:
                return 5f;
            case DiceVisualProfile.Tree:
                return 4f;
            case DiceVisualProfile.Gold:
                return 9f;
            case DiceVisualProfile.Burst:
                return 13f;
            case DiceVisualProfile.Parity:
                return 6f;
        }

        return 7f;
    }

    private float DiceVisualProfileRollWeight(DiceVisualProfile profile)
    {
        switch (profile)
        {
            case DiceVisualProfile.Turtle:
                return 0.78f;
            case DiceVisualProfile.Tree:
                return 0.7f;
            case DiceVisualProfile.Gold:
                return 0.9f;
            case DiceVisualProfile.Burst:
                return 1.08f;
            case DiceVisualProfile.Parity:
                return 0.86f;
        }

        return 0.84f;
    }

    private float DiceVisualProfileRevealTurns(DiceVisualProfile profile)
    {
        switch (profile)
        {
            case DiceVisualProfile.Turtle:
                return 1.25f;
            case DiceVisualProfile.Tree:
                return 1.15f;
            case DiceVisualProfile.Gold:
                return 1.8f;
            case DiceVisualProfile.Burst:
                return 2.25f;
            case DiceVisualProfile.Parity:
                return 1.55f;
        }

        return 1.65f;
    }

    private void DrawProcessDieTypeMarker(Rect rect, DieType type)
    {
        if (type == DieType.Basic)
        {
            return;
        }

        Color typeColor = TypeColor(type);
        Color markerColor = new Color(typeColor.r, typeColor.g, typeColor.b, 0.86f);
        float stripHeight = Mathf.Clamp(rect.height * 0.055f, 3f, 5f);
        DrawRect(new Rect(rect.x + rect.width * 0.16f, rect.y + rect.height * 0.88f, rect.width * 0.68f, stripHeight), markerColor);

        float sideWidth = Mathf.Clamp(rect.width * 0.055f, 3f, 5f);
        DrawRect(new Rect(rect.x + rect.width * 0.08f, rect.y + rect.height * 0.23f, sideWidth, rect.height * 0.54f), markerColor);

        float badgeSize = Mathf.Clamp(rect.width * 0.28f, 16f, 24f);
        Rect badgeRect = new Rect(rect.x + rect.width - badgeSize * 0.68f, rect.y - badgeSize * 0.08f, badgeSize, badgeSize);
        DrawPanel(badgeRect, new Color(1f, 0.91f, 0.66f, 0.95f));
        DrawBorder(badgeRect, markerColor, 2f);

        Texture2D icon = DieTypeIcon(type);
        Rect iconRect = InsetRect(badgeRect, badgeSize * 0.16f, badgeSize * 0.16f);
        if (icon == null)
        {
            DrawRect(iconRect, markerColor);
            return;
        }

        Color old = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
        GUI.color = old;
    }

    private Rect ScaleRect(Rect rect, float scale)
    {
        float width = rect.width * scale;
        float height = rect.height * scale;
        return new Rect(rect.x + (rect.width - width) * 0.5f, rect.y + (rect.height - height) * 0.5f, width, height);
    }

    private float EaseOutBack(float t)
    {
        t = Mathf.Clamp01(t);
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        float p = t - 1f;
        return 1f + c3 * p * p * p + c1 * p * p;
    }

    private void UpdateUiMousePosition(float scale, float offsetX, float offsetY)
    {
        Event currentEvent = Event.current;
        if (currentEvent == null || scale <= 0f)
        {
            uiMousePosition = new Vector2(-9999f, -9999f);
            uiMousePositionValid = false;
            return;
        }

        float totalScale = scale * PrototypeToDesignScale;
        if (totalScale <= 0f)
        {
            uiMousePosition = new Vector2(-9999f, -9999f);
            uiMousePositionValid = false;
            return;
        }

        Vector2 raw = currentEvent.mousePosition;
        uiMousePosition = new Vector2((raw.x - offsetX) / totalScale, (raw.y - offsetY) / totalScale);
        uiMousePositionValid = uiMousePosition.x >= 0f
            && uiMousePosition.x <= VirtualWidth
            && uiMousePosition.y >= 0f
            && uiMousePosition.y <= VirtualHeight;
    }

    private void BeginDiceHoverTooltipFrame()
    {
        hoverCandidateKey = string.Empty;
        hoverCandidateDie = null;
        hoverCandidateRect = default(Rect);
        hoverCandidateAllowCurrentStateText = false;
    }

    private void TrySetHoveredTooltip(Rect rect, Die die, bool allowCurrentStateText, string sourceKey)
    {
        if (die == null || die.Temporary || string.IsNullOrEmpty(sourceKey))
        {
            return;
        }

        if (!CanAcceptDiceHoverTooltipCandidate())
        {
            return;
        }

        if (!uiMousePositionValid || !rect.Contains(uiMousePosition))
        {
            return;
        }

        hoverCandidateKey = sourceKey;
        hoverCandidateDie = die;
        hoverCandidateRect = rect;
        hoverCandidateAllowCurrentStateText = allowCurrentStateText;
    }

    private bool CanAcceptDiceHoverTooltipCandidate()
    {
        if (mode == GameMode.InterStageMarket || mode == GameMode.ChapterShop)
        {
            return true;
        }

        if (mode != GameMode.Run)
        {
            return false;
        }

        return rollPhase == RollPhase.Ready
            || rollPhase == RollPhase.ResultDecision
            || rollPhase == RollPhase.CheatEdit
            || rollPhase == RollPhase.StageClear;
    }

    private bool ShouldClearDiceHoverTooltipImmediately()
    {
        if (mode != GameMode.Run && mode != GameMode.InterStageMarket && mode != GameMode.ChapterShop)
        {
            return true;
        }

        if (mode != GameMode.Run)
        {
            return false;
        }

        return rollPhase == RollPhase.Shaking
            || rollPhase == RollPhase.Stopping
            || rollPhase == RollPhase.Scoring
            || rollPhase == RollPhase.StageFailed;
    }

    private void UpdateDiceHoverTooltipState()
    {
        float now = Time.unscaledTime;
        if (ShouldClearDiceHoverTooltipImmediately())
        {
            ClearDiceHoverTooltip();
            return;
        }

        bool hasCandidate = !string.IsNullOrEmpty(hoverCandidateKey) && hoverCandidateDie != null;
        if (!hasCandidate)
        {
            UpdateDiceHoverTooltipWithoutCandidate(now);
            return;
        }

        if (tooltipState == DiceTooltipState.Hidden)
        {
            SetActiveTooltipFromCandidate();
            hoverCandidateStartedAt = now;
            tooltipState = DiceTooltipState.Waiting;
            tooltipAlpha = 0f;
            return;
        }

        if (tooltipState == DiceTooltipState.Waiting)
        {
            if (!string.Equals(activeTooltipKey, hoverCandidateKey, StringComparison.Ordinal))
            {
                SetActiveTooltipFromCandidate();
                hoverCandidateStartedAt = now;
                return;
            }

            SetActiveTooltipFromCandidate();
            if (now - hoverCandidateStartedAt >= TooltipHoverDelay)
            {
                tooltipVisibleStartedAt = now;
                tooltipState = DiceTooltipState.FadingIn;
                tooltipAlpha = 0f;
            }

            return;
        }

        if (tooltipState == DiceTooltipState.FadingOut)
        {
            if (string.Equals(activeTooltipKey, hoverCandidateKey, StringComparison.Ordinal))
            {
                SetActiveTooltipFromCandidate();
                tooltipVisibleStartedAt = now - TooltipFadeInDuration * Mathf.Clamp01(tooltipAlpha);
                tooltipState = DiceTooltipState.FadingIn;
            }
            else
            {
                SwitchActiveTooltip(now);
            }
        }
        else if (!string.Equals(activeTooltipKey, hoverCandidateKey, StringComparison.Ordinal))
        {
            if (tooltipAlpha > 0.01f)
            {
                SwitchActiveTooltip(now);
            }
            else
            {
                SetActiveTooltipFromCandidate();
                hoverCandidateStartedAt = now;
                tooltipState = DiceTooltipState.Waiting;
            }
        }
        else
        {
            SetActiveTooltipFromCandidate();
        }

        UpdateActiveDiceHoverTooltipAlpha(now);
    }

    private void UpdateDiceHoverTooltipWithoutCandidate(float now)
    {
        if (tooltipState == DiceTooltipState.Hidden)
        {
            return;
        }

        if (tooltipState == DiceTooltipState.Waiting)
        {
            ClearDiceHoverTooltip();
            return;
        }

        if (tooltipState != DiceTooltipState.FadingOut)
        {
            StartDiceHoverTooltipFadeOut(now);
        }

        UpdateActiveDiceHoverTooltipAlpha(now);
    }

    private void UpdateActiveDiceHoverTooltipAlpha(float now)
    {
        if (tooltipState == DiceTooltipState.FadingIn)
        {
            tooltipAlpha = Mathf.Clamp01((now - tooltipVisibleStartedAt) / TooltipFadeInDuration);
            if (tooltipAlpha >= 1f)
            {
                tooltipAlpha = 1f;
                tooltipState = DiceTooltipState.Visible;
            }
            return;
        }

        if (tooltipState == DiceTooltipState.FadingOut)
        {
            float t = Mathf.Clamp01((now - tooltipHideStartedAt) / TooltipFadeOutDuration);
            tooltipAlpha = Mathf.Clamp01(tooltipFadeOutStartAlpha * (1f - t));
            if (t >= 1f)
            {
                ClearDiceHoverTooltip();
            }
            return;
        }

        if (tooltipState == DiceTooltipState.Swapping)
        {
            tooltipAlpha = 1f;
            if (now - tooltipContentSwapStartedAt >= TooltipContentSwapDuration)
            {
                tooltipState = DiceTooltipState.Visible;
            }
            return;
        }

        if (tooltipState == DiceTooltipState.Visible)
        {
            tooltipAlpha = 1f;
        }
    }

    private void SetActiveTooltipFromCandidate()
    {
        activeTooltipKey = hoverCandidateKey;
        activeTooltipDie = hoverCandidateDie;
        activeTooltipRect = hoverCandidateRect;
        activeTooltipAllowCurrentStateText = hoverCandidateAllowCurrentStateText;
    }

    private void SwitchActiveTooltip(float now)
    {
        SetActiveTooltipFromCandidate();
        tooltipContentSwapStartedAt = now;
        tooltipState = DiceTooltipState.Swapping;
        tooltipAlpha = Mathf.Max(tooltipAlpha, 0.82f);
    }

    private void StartDiceHoverTooltipFadeOut(float now)
    {
        tooltipHideStartedAt = now;
        tooltipFadeOutStartAlpha = Mathf.Clamp01(tooltipAlpha);
        if (tooltipFadeOutStartAlpha <= 0.01f)
        {
            ClearDiceHoverTooltip();
            return;
        }

        tooltipState = DiceTooltipState.FadingOut;
    }

    private void ClearDiceHoverTooltip()
    {
        tooltipState = DiceTooltipState.Hidden;
        activeTooltipKey = string.Empty;
        activeTooltipDie = null;
        activeTooltipRect = default(Rect);
        activeTooltipAllowCurrentStateText = false;
        hoverCandidateStartedAt = -999f;
        tooltipVisibleStartedAt = -999f;
        tooltipHideStartedAt = -999f;
        tooltipContentSwapStartedAt = -999f;
        tooltipAlpha = 0f;
        tooltipFadeOutStartAlpha = 0f;
    }

    private void DrawDiceHoverTooltip()
    {
        if (activeTooltipDie == null || tooltipState == DiceTooltipState.Hidden || tooltipState == DiceTooltipState.Waiting || tooltipAlpha <= 0f)
        {
            return;
        }

        Rect totalRect = DiceHoverTooltipRect(activeTooltipRect);
        float enterProgress = tooltipState == DiceTooltipState.FadingIn ? Mathf.Clamp01(tooltipAlpha) : 1f;
        totalRect.y += (1f - enterProgress) * TooltipEnterOffsetY;

        float totalAlpha = Mathf.Clamp01(tooltipAlpha);
        float contentAlpha = totalAlpha;
        if (tooltipState == DiceTooltipState.Swapping)
        {
            float swap = Mathf.Clamp01((Time.unscaledTime - tooltipContentSwapStartedAt) / TooltipContentSwapDuration);
            contentAlpha *= Mathf.Lerp(0.62f, 1f, swap);
        }

        Rect panelRect = new Rect(totalRect.x, totalRect.y, TooltipPanelWidth, TooltipPanelHeight);
        DrawDiceHoverTooltipFrame(panelRect, totalAlpha);
        DrawDiceHoverTooltipContent(panelRect, activeTooltipDie, activeTooltipAllowCurrentStateText, contentAlpha);
    }

    private Rect DiceHoverTooltipRect(Rect target)
    {
        float safe = 16f;
        float totalWidth = TooltipPanelWidth;
        float x = target.x + target.width + 12f;
        if (x + totalWidth > VirtualWidth - safe)
        {
            x = target.x - totalWidth - 12f;
        }

        float bottomLimit = VirtualHeight - safe;
        if (mode == GameMode.Run)
        {
            bottomLimit = 628f;
        }
        else if (mode == GameMode.InterStageMarket || mode == GameMode.ChapterShop)
        {
            bottomLimit = 584f;
        }

        float y = target.y - 24f;
        if (y + TooltipPanelHeight > bottomLimit)
        {
            float above = target.y - TooltipPanelHeight - 10f;
            y = above >= safe ? above : bottomLimit - TooltipPanelHeight;
        }
        if (y < safe)
        {
            y = Mathf.Min(bottomLimit - TooltipPanelHeight, target.y + target.height + 10f);
        }

        x = Mathf.Clamp(x, safe, VirtualWidth - totalWidth - safe);
        y = Mathf.Clamp(y, safe, bottomLimit - TooltipPanelHeight);
        return new Rect(x, y, totalWidth, TooltipPanelHeight);
    }

    private void DrawDiceHoverTooltipFrame(Rect panelRect, float alpha)
    {
        DrawTooltipTexture(panelRect, tooltipPanelTexture, alpha, new Color(1f, 0.94f, 0.78f, 0.96f));
        DrawBorder(new Rect(panelRect.x + 10f, panelRect.y + 10f, panelRect.width - 20f, panelRect.height - 20f), new Color(0.62f, 0.38f, 0.16f, alpha * 0.48f), 1f);
    }

    private void DrawDiceHoverTooltipContent(Rect panelRect, Die die, bool allowCurrentStateText, float alpha)
    {
        float left = panelRect.x + 24f;
        float right = panelRect.x + panelRect.width - 24f;
        float contentWidth = right - left;
        Rect iconRect = new Rect(left, panelRect.y + 24f, 68f, 68f);
        DrawTooltipDieIcon(iconRect, die, alpha);

        Rect priceRect = new Rect(right - 76f, panelRect.y + 40f, 76f, 30f);
        float titleX = left + 82f;
        float titleWidth = Mathf.Max(104f, priceRect.x - titleX - 8f);
        string displayName = TooltipDisplayName(die);
        DrawTooltipLabel(new Rect(titleX, panelRect.y + 28f, titleWidth, 26f), displayName, tooltipTitleStyle, alpha);
        DrawTooltipLabel(new Rect(titleX, panelRect.y + 58f, titleWidth, 22f), die != null ? TooltipTypeName(die.Type) : string.Empty, tooltipTinyStyle, alpha);

        DrawTooltipTexture(priceRect, tooltipPriceChipTexture, alpha, new Color(0.96f, 0.72f, 0.28f, 0.94f));
        DrawTooltipLabel(priceRect, die != null ? "卖 " + SellPrice(die.Type) : string.Empty, tooltipLabelStyle, alpha);

        DrawTooltipDivider(new Rect(left, panelRect.y + 102f, contentWidth, 1f), alpha);
        DrawTooltipLabel(new Rect(left, panelRect.y + 124f, 62f, 20f), "点面", tooltipTinyStyle, alpha);
        DrawTooltipFaces(new Rect(left + 2f, panelRect.y + 154f, contentWidth - 4f, 40f), die, alpha);
        DrawTooltipDivider(new Rect(left, panelRect.y + 212f, contentWidth, 1f), alpha);

        DrawTooltipSection(
            new Rect(left, panelRect.y + 232f, contentWidth, 70f),
            tooltipLabelChipBlueTexture,
            "骰效",
            TooltipEffectText(die, allowCurrentStateText),
            alpha,
            76);

        DrawTooltipDivider(new Rect(left + 20f, panelRect.y + 318f, contentWidth - 40f, 1f), alpha * 0.72f);

        DrawTooltipSection(
            new Rect(left, panelRect.y + 334f, contentWidth, 42f),
            tooltipLabelChipGreenTexture,
            "质效",
            TooltipMaterialText(die),
            alpha,
            42);
    }

    private void DrawTooltipSection(Rect rect, Texture2D labelTexture, string label, string text, float alpha, int maxCharacters)
    {
        Rect labelRect = new Rect(rect.x, rect.y + 2f, 58f, 28f);
        DrawTooltipTexture(labelRect, labelTexture, alpha, new Color(0.64f, 0.7f, 0.62f, 0.9f));
        DrawTooltipLabel(labelRect, label, tooltipLabelStyle, alpha);
        DrawTooltipLabel(new Rect(rect.x + 74f, rect.y - 1f, rect.width - 74f, rect.height + 2f), TooltipTrim(text, maxCharacters), tooltipBodyStyle, alpha);
    }

    private void DrawTooltipFaces(Rect rect, Die die, float alpha)
    {
        int[] faces = die != null && die.Faces != null ? die.Faces : new int[0];
        float cellSize = 38f;
        float gap = Mathf.Max(6f, (rect.width - cellSize * 6f) / 5f);
        for (int i = 0; i < 6; i++)
        {
            Rect cell = new Rect(rect.x + i * (cellSize + gap), rect.y, cellSize, cellSize);
            DrawTooltipTexture(cell, tooltipFaceCellTexture, alpha, new Color(1f, 0.94f, 0.76f, 0.9f));
            int value = i < faces.Length ? faces[i] : 0;
            if (value > 0 && value <= 12)
            {
                DrawDiePips(InsetRect(cell, 8f, 8f), value, new Color(0.08f, 0.08f, 0.07f, alpha * 0.95f));
            }
            else if (value > 0)
            {
                DrawTooltipLabel(cell, value.ToString(CultureInfo.InvariantCulture), tooltipLabelStyle, alpha);
            }
        }
    }

    private void DrawTooltipDieIcon(Rect rect, Die die, float alpha)
    {
        Color accent = TypeColor(die.Type);
        DrawTooltipTexture(rect, null, alpha, new Color(accent.r, accent.g, accent.b, 0.22f));
        DrawBorder(rect, new Color(0.32f, 0.22f, 0.13f, alpha * 0.42f), 1f);

        Texture2D icon = DieTypeIcon(die.Type);
        if (icon != null)
        {
            DrawTooltipTexture(InsetRect(rect, 7f, 7f), icon, alpha, Color.white);
            return;
        }

        DrawTooltipTexture(InsetRect(rect, 10f, 10f), runtimeDieFaceBaseTexture, alpha, new Color(1f, 0.94f, 0.76f, 0.94f));
        int value = die.Faces != null && die.Faces.Length > 0 ? die.Faces[0] : 1;
        DrawDiePips(InsetRect(rect, 22f, 22f), value, new Color(0.08f, 0.08f, 0.07f, alpha * 0.95f));
    }

    private void DrawTooltipDivider(Rect rect, float alpha)
    {
        DrawRect(rect, new Color(0.71f, 0.47f, 0.2f, Mathf.Clamp01(alpha) * 0.34f));
    }

    private void DrawTooltipTexture(Rect rect, Texture2D texture, float alpha, Color fallback)
    {
        Color old = GUI.color;
        if (texture != null)
        {
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, true);
        }
        else
        {
            fallback.a *= alpha;
            DrawRect(rect, fallback);
        }

        GUI.color = old;
    }

    private void DrawTooltipLabel(Rect rect, string text, GUIStyle style, float alpha)
    {
        if (string.IsNullOrEmpty(text) || style == null)
        {
            return;
        }

        Color old = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.Label(rect, text, style);
        GUI.color = old;
    }

    private string TooltipDisplayName(Die die)
    {
        string name = DieDisplayName(die);
        if (string.IsNullOrEmpty(name) && die != null)
        {
            name = TypeName(die.Type);
        }

        return TooltipTrim(name, 16);
    }

    private string TooltipEffectText(Die die, bool allowCurrentStateText)
    {
        if (die == null)
        {
            return string.Empty;
        }

        string text = DiceTypeTooltipEffect(die);
        if (string.IsNullOrEmpty(text))
        {
            text = allowCurrentStateText ? RoundTag(die) : MarketOfferHint(die);
        }

        if (string.IsNullOrEmpty(text))
        {
            text = TypeName(die.Type);
        }

        return text;
    }

    private string TooltipTypeName(DieType type)
    {
        DiceTypeTooltipConfig config;
        if (diceTypeTooltipConfigs.TryGetValue(type, out config) && !string.IsNullOrEmpty(config.DisplayName))
        {
            return config.DisplayName;
        }

        return TypeName(type);
    }

    private string DiceTypeTooltipEffect(Die die)
    {
        if (die == null)
        {
            return string.Empty;
        }

        DiceTypeTooltipConfig config;
        if (diceTypeTooltipConfigs.TryGetValue(die.Type, out config) && !string.IsNullOrEmpty(config.TooltipEffect))
        {
            return config.TooltipEffect;
        }

        string fallback = MarketOfferHint(die);
        if (string.IsNullOrEmpty(fallback))
        {
            fallback = RoundTag(die);
        }

        return string.IsNullOrEmpty(fallback) ? TypeName(die.Type) : fallback;
    }

    private string TooltipMaterialText(Die die)
    {
        if (die == null)
        {
            return string.Empty;
        }

        string text = MaterialShortRule(die.Material);
        return string.IsNullOrEmpty(text) ? "无品质效果" : text;
    }

    private string TooltipTrim(string text, int maxCharacters)
    {
        if (string.IsNullOrEmpty(text) || maxCharacters <= 0 || text.Length <= maxCharacters)
        {
            return text;
        }

        return text.Substring(0, Mathf.Max(1, maxCharacters - 2)) + "..";
    }

    private bool DrawCompactDie(Rect rect, Die die, bool selected, string hoverKey = null)
    {
        DrawUiSmallPanel(rect);
        if (selected)
        {
            DrawRect(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, 4f), new Color(0.87f, 0.62f, 0.18f, 0.95f));
            DrawRect(new Rect(rect.x + 4f, rect.y + rect.height - 8f, rect.width - 8f, 4f), new Color(0.87f, 0.62f, 0.18f, 0.95f));
        }

        DrawDieToken(new Rect(rect.x + 8f, rect.y + 8f, 38f, 38f), die, die.EffectiveValue > 0 ? die.EffectiveValue : die.Faces[0]);
        GUI.Label(new Rect(rect.x + 56f, rect.y + 7f, rect.width - 62f, 22f), DieDisplayName(die), smallStyle);
        string detail = affixFeatureEnabled ? AffixSlotSummary(die) : MaterialDisplayName(die.Material);
        GUI.Label(new Rect(rect.x + 56f, rect.y + 28f, rect.width - 62f, 20f), FaceText(die.Faces) + " | " + detail, tinyStyle);
        TrySetHoveredTooltip(rect, die, false, hoverKey);
        return GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }

    private void DrawCraftingTargetPanel(Rect rect)
    {
        CraftingItemDefinition item = CraftingItemDefinitionForKey(activeCraftingItemKey);
        string itemName = item != null ? item.DisplayName : "改造道具";
        GUI.Label(new Rect(rect.x + 18f, rect.y + 8f, 190f, 22f), "使用 " + itemName, smallStyle);

        Die selectedDie = selectedMarketDieIndex >= 0 && selectedMarketDieIndex < dice.Count ? dice[selectedMarketDieIndex] : null;
        string reason;
        bool canUse = CanUseCraftingItemOnDie(activeCraftingItemKey, selectedDie, out reason);
        GUI.Label(new Rect(rect.x + 18f, rect.y + 34f, 190f, 20f), selectedDie != null ? DieDisplayName(selectedDie) : "请选择目标骰", tinyStyle);
        GUI.Label(new Rect(rect.x + 18f, rect.y + 56f, 190f, 18f), reason, tinyStyle);

        GUI.enabled = canUse;
        if (DrawUiButton(new Rect(rect.x + 208f, rect.y + 10f, 118f, 34f), "执行", UiButtonKind.Primary))
        {
            ApplyCraftingItemToDie(activeCraftingItemKey, selectedDie);
        }
        GUI.enabled = true;

        if (DrawUiButton(new Rect(rect.x + 208f, rect.y + 48f, 118f, 28f), "取消", UiButtonKind.Secondary))
        {
            activeCraftingItemKey = string.Empty;
            rewardBanner = "已取消改造，道具未消耗。";
        }
    }

    private void DrawCraftingInventoryPanel(Rect rect)
    {
        DrawUiSmallPanel(rect);
        GUI.Label(new Rect(rect.x + 16f, rect.y + 15f, 86f, 22f), "改造道具", smallStyle);
        DrawCraftingInventoryButton(new Rect(rect.x + 108f, rect.y + 9f, 168f, 34f), "affix_add_stone");
        DrawCraftingInventoryButton(new Rect(rect.x + 286f, rect.y + 9f, 168f, 34f), "affix_remove_stone");
        DrawCraftingInventoryButton(new Rect(rect.x + 464f, rect.y + 9f, 168f, 34f), "affix_replace_stone");
    }

    private void DrawCraftingInventoryButton(Rect rect, string itemKey)
    {
        CraftingItemDefinition item = CraftingItemDefinitionForKey(itemKey);
        string name = item != null ? item.DisplayName : itemKey;
        int count = CraftingItemCount(itemKey);
        bool selected = string.Equals(activeCraftingItemKey, itemKey, StringComparison.OrdinalIgnoreCase);
        DrawScalableButtonFrame(rect, selected ? UiButtonKind.Primary : UiButtonKind.Secondary, count > 0);
        if (GUI.Button(rect, GUIContent.none, GUIStyle.none) && count > 0)
        {
            activeCraftingItemKey = selected ? string.Empty : itemKey;
            rewardBanner = selected ? "已取消改造，道具未消耗。" : "选择一颗骰子使用 " + name + "。";
        }

        Texture2D icon = CraftingItemIcon(itemKey);
        if (icon != null)
        {
            GUI.DrawTexture(new Rect(rect.x + 8f, rect.y + 5f, 24f, 24f), icon, ScaleMode.ScaleToFit, true);
            GUI.Label(new Rect(rect.x + 38f, rect.y, rect.width - 42f, rect.height), name + " x" + count, count > 0 ? artButtonStyle : disabledButtonLabelStyle);
        }
        else
        {
            GUI.Label(rect, name + " x" + count, count > 0 ? artButtonStyle : disabledButtonLabelStyle);
        }
    }

    private void DrawMarketOffer(Rect rect, MarketOffer offer, string hoverKey = null)
    {
        DrawUiCard(rect);
        if (offer == null)
        {
            return;
        }

        if (offer.Kind == MarketOfferKind.CraftingItem)
        {
            if (!affixFeatureEnabled)
            {
                GUI.Label(new Rect(rect.x + 22f, rect.y + 26f, rect.width - 44f, 28f), "货架刷新中", cardTitleStyle);
                GUI.Label(new Rect(rect.x + 22f, rect.y + 160f, rect.width - 44f, 22f), "刷新后替换为骰子商品", smallStyle);
                return;
            }

            DrawCraftingMarketOffer(rect, offer);
            return;
        }

        GUI.Label(new Rect(rect.x + 22f, rect.y + 26f, rect.width - 44f, 28f), offer.Die.Name, cardTitleStyle);
        Rect iconBack = new Rect(rect.x + 52f, rect.y + 58f, 100f, 96f);
        DrawDiceMaterialOverlay(iconBack, offer.Die.Material, 0.42f);
        DrawDieTypeIcon(new Rect(rect.x + 58f, rect.y + 62f, 88f, 88f), offer.Die.Type);
        Rect materialStrip = new Rect(rect.x + 22f, rect.y + 152f, rect.width - 44f, 24f);
        DrawDiceMaterialOverlay(materialStrip, offer.Die.Material, 0.45f);
        GUI.Label(new Rect(rect.x + 22f, rect.y + 154f, rect.width - 44f, 22f), MaterialDisplayName(offer.Die.Material), smallStyle);
        GUI.Label(new Rect(rect.x + 22f, rect.y + 176f, rect.width - 44f, 22f), TypeName(offer.Die.Type) + " | " + FaceText(offer.Die.Faces), smallStyle);
        GUI.Label(new Rect(rect.x + 22f, rect.y + 200f, rect.width - 44f, 18f), MarketOfferHint(offer.Die), tinyStyle);
        GUI.Label(new Rect(rect.x + 22f, rect.y + 220f, rect.width - 44f, 18f), MaterialShortRule(offer.Die.Material), tinyStyle);
        TrySetHoveredTooltip(rect, offer.Die, false, hoverKey + "-" + offer.Die.Id);
    }

    private void DrawCraftingMarketOffer(Rect rect, MarketOffer offer)
    {
        CraftingItemDefinition item = offer.CraftingItem;
        string name = item != null ? item.DisplayName : "改造道具";
        GUI.Label(new Rect(rect.x + 22f, rect.y + 26f, rect.width - 44f, 28f), name, cardTitleStyle);

        Rect iconRect = new Rect(rect.x + 52f, rect.y + 62f, 100f, 88f);
        Texture2D itemIcon = CraftingItemIcon(item != null ? item.Key : string.Empty);
        if (itemIcon != null)
        {
            GUI.DrawTexture(iconRect, itemIcon, ScaleMode.ScaleToFit, true);
        }
        else
        {
            Color accent = CraftingItemColor(item != null ? item.Key : string.Empty);
            DrawPanel(iconRect, new Color(1f, 0.88f, 0.58f, 0.96f));
            DrawBorder(iconRect, accent, 4f);
            DrawRect(new Rect(iconRect.x + 28f, iconRect.y + 14f, 44f, 46f), new Color(accent.r, accent.g, accent.b, 0.86f));
            DrawRect(new Rect(iconRect.x + 20f, iconRect.y + 58f, 60f, 14f), new Color(0.42f, 0.28f, 0.18f, 0.9f));
            DrawRect(new Rect(iconRect.x + 34f, iconRect.y + 22f, 32f, 8f), new Color(1f, 0.96f, 0.78f, 0.7f));
        }

        GUI.Label(new Rect(rect.x + 22f, rect.y + 160f, rect.width - 44f, 22f), "市场改造道具", smallStyle);
        GUI.Label(new Rect(rect.x + 22f, rect.y + 184f, rect.width - 44f, 20f), item != null ? item.ShortRule : "随机改造词缀", tinyStyle);
        GUI.Label(new Rect(rect.x + 22f, rect.y + 206f, rect.width - 44f, 18f), "持有 " + CraftingItemCount(item != null ? item.Key : string.Empty), tinyStyle);
        GUI.Label(new Rect(rect.x + 22f, rect.y + 226f, rect.width - 44f, 18f), "购买后市场阶段使用", tinyStyle);
    }

    private string MarketBuyButtonText(MarketOffer offer)
    {
        if (offer == null)
        {
            return "不可购买";
        }

        if (offer.Kind == MarketOfferKind.CraftingItem && !affixFeatureEnabled)
        {
            return "不可购买";
        }

        if (offer.Kind == MarketOfferKind.Die && dice.Count >= DiceCapacity)
        {
            return "骰袋已满";
        }

        int missingGold = offer.Price - chapterGold;
        if (missingGold > 0)
        {
            return "差 " + missingGold + " 金";
        }

        return "购买 " + offer.Price + " 金";
    }

    private string MarketOfferHint(Die die)
    {
        switch (die.Type)
        {
            case DieType.Piggy:
                return CountDiceOfType(DieType.Piggy) > 0 ? "强化经济循环" : "早买更会赚钱";
            case DieType.Treasury:
                return "本金越厚越稳";
            case DieType.Bribe:
                return "差点过关可补分";
            case DieType.Investment:
                return "每关自动投预算";
            case DieType.BountyGold:
                return "高风险目标给大钱";
            case DieType.TopGold:
                return "最大面稳定回金";
            case DieType.HandTax:
                return "牌型越好越会收";
            case DieType.Collection:
                return "每关先收一笔";
            case DieType.CompoundInterest:
                return "过关利息再放大";
            case DieType.LeadTicket:
                return "接出千确认收益";
            case DieType.ShellTax:
                return "小骰数量达标收税";
            case DieType.CounterGold:
                return "贴左侧收益位";
            case DieType.LumberGold:
                return "压左侧分换金币";
            case DieType.Turtle:
                return "高点启动壳链";
            case DieType.Shellsmith:
                return "小骰越多越加分";
            case DieType.Nest:
                return "有龟链就补 d1";
            case DieType.SlowTurtle:
                return "慢壳链更长";
            case DieType.Double:
                return "直接提高单骰分";
            case DieType.Odd:
                return CountDiceOfType(DieType.Odd) >= 2 ? "补强全奇路线" : "开启奇数组合";
            case DieType.Even:
                return CountDiceOfType(DieType.Even) >= 2 ? "补强全偶路线" : "开启偶数组合";
            case DieType.LoneWitness:
                return "落单时自己重摇";
            case DieType.Stamp:
                return "同点伙伴给 6 分";
            case DieType.HalfStep:
                return "低一格补顺子";
            case DieType.Track:
                return "2/4/6 齐备给 8 分";
            case DieType.ParityNeighborDiff:
                return "左邻不同给 6 分";
            case DieType.ParityNeighborSame:
                return "左邻相同给 6 分";
            case DieType.ParityComplete:
                return "补全全奇全偶";
            case DieType.ParityReview:
                return "破坏全奇全偶会重摇";
            case DieType.ParityFlipScore:
                return "出千变号给 8 分";
            case DieType.ParityHoldScore:
                return "出千守号给 6 分";
            case DieType.ParityTurner:
                return "出千改点变号";
            case DieType.Tree:
                return "长期成长，早买更赚";
            case DieType.Gardener:
                return "大树命中额外成长";
            case DieType.Irrigation:
                return "掷中目标也能浇树";
            case DieType.PointSeedTree:
                return "命中指定点成长";
            case DieType.PatternTree:
                return "成指定牌型成长";
            case DieType.CanopyTree:
                return "掷最高面成长";
            case DieType.RingTree:
                return "每次出手慢养";
            case DieType.FertilizerTree:
                return "金币越多长越快";
            case DieType.PruningTree:
                return "出千变好成长";
            case DieType.RootTree:
                return "贴触发骰旁成长";
            case DieType.Gambler:
                return "高波动爆发骰";
        }

        return dice.Count < DiceCapacity ? "便宜补位" : "低价替换材料";
    }

    private bool DrawDieCard(Rect rect, Die die)
    {
        DrawUiSmallPanel(rect);
        DrawDieToken(new Rect(rect.x + 8f, rect.y + 10f, 56f, 56f), die, die.EffectiveValue > 0 ? die.EffectiveValue : 0);
        GUI.Label(new Rect(rect.x + 74f, rect.y + 8f, rect.width - 82f, 24f), DieDisplayName(die), smallStyle);
        GUI.Label(new Rect(rect.x + 74f, rect.y + 32f, rect.width - 82f, 20f), TypeName(die.Type) + " | " + FaceText(die.Faces), tinyStyle);
        GUI.Label(new Rect(rect.x + 74f, rect.y + 52f, rect.width - 82f, 20f), AffixOrRoundTag(die), tinyStyle);
        return GUI.Button(rect, GUIContent.none, GUIStyle.none);
    }

    private void DrawLog(Rect area)
    {
        int start = Mathf.Max(0, logLines.Count - 7);
        float y = area.y;
        for (int i = start; i < logLines.Count; i++)
        {
            GUI.Label(new Rect(area.x, y, area.width, 24f), logLines[i], tinyStyle);
            y += 24f;
        }
    }

    private List<TableDieView> BuildTableDieViews()
    {
        List<TableDieView> views = new List<TableDieView>();
        if (rollPhase != RollPhase.Scoring || scoringDice.Count == 0)
        {
            for (int i = 0; i < dice.Count; i++)
            {
                views.Add(NewTableDieView(dice[i], -1, -1));
            }

            return views;
        }

        int shownTemporaryDice = 0;
        int hiddenTemporaryDice = 0;
        int hiddenTemporaryScore = 0;
        int hiddenStartIndex = -1;
        int hiddenEndIndex = -1;

        for (int i = 0; i < scoringDice.Count; i++)
        {
            Die die = scoringDice[i];
            if (!die.Temporary || shownTemporaryDice < TemporaryDiceDisplayLimit)
            {
                views.Add(NewTableDieView(die, i, i));
                if (die.Temporary)
                {
                    shownTemporaryDice++;
                }

                continue;
            }

            if (hiddenStartIndex < 0)
            {
                hiddenStartIndex = i;
            }

            hiddenEndIndex = i;
            hiddenTemporaryDice++;
            hiddenTemporaryScore += Mathf.Max(0, die.Score);
        }

        if (hiddenTemporaryDice > 0)
        {
            Die summary = NewTemporaryDie("余骰 x" + hiddenTemporaryDice, 0);
            summary.Score = hiddenTemporaryScore;
            summary.RoundNote = "折叠显示";
            TableDieView view = NewTableDieView(summary, hiddenStartIndex, hiddenEndIndex);
            view.Value = 0;
            view.ScoreFloatValue = hiddenTemporaryScore;
            view.PrimaryText = "余骰 x" + hiddenTemporaryDice;
            view.SecondaryText = "+" + hiddenTemporaryScore;
            view.Summary = true;
            views.Add(view);
        }

        return views;
    }

    private TableDieView NewTableDieView(Die die, int scoreIndexStart, int scoreIndexEnd)
    {
        return new TableDieView
        {
            Die = die,
            ScoreIndexStart = scoreIndexStart,
            ScoreIndexEnd = scoreIndexEnd,
            Value = die.EffectiveValue > 0 ? die.EffectiveValue : 0,
            ScoreFloatValue = die.Score,
            PrimaryText = string.Empty,
            SecondaryText = string.Empty,
            Summary = false
        };
    }

    private bool IsScoreViewActive(TableDieView view)
    {
        if (rollPhase != RollPhase.Scoring || view.ScoreIndexStart < 0)
        {
            return false;
        }

        if (settlementDisplayEvents.Count > 0)
        {
            return ActiveSettlementEventForView(view) != null;
        }

        return scoreRevealIndex >= view.ScoreIndexStart && scoreRevealIndex <= view.ScoreIndexEnd;
    }

    private float ScoreScaleForView(TableDieView view)
    {
        if (!IsScoreViewActive(view))
        {
            return 1f;
        }

        SettlementDisplayEvent settlementEvent = ActiveSettlementEventForView(view);
        float t = settlementEvent != null ? SettlementEventProgress() : Mathf.Clamp01(scoreStepTimer / ScoreStepDuration);
        float amplitude = settlementEvent != null && settlementEvent.Kind != SettlementEventKind.SlotScore ? 0.11f : 0.16f;
        return 1f + Mathf.Sin(t * Mathf.PI) * amplitude;
    }

    private int ScoreFloatValueForView(TableDieView view)
    {
        SettlementDisplayEvent settlementEvent = ActiveSettlementEventForView(view);
        if (settlementEvent != null && settlementEvent.Kind == SettlementEventKind.SlotScore && settlementEvent.ValueDelta > 0)
        {
            return settlementEvent.ValueDelta;
        }

        if (view.Summary && scoreRevealIndex >= 0 && scoreRevealIndex < scoringDice.Count)
        {
            return scoringDice[scoreRevealIndex].Score;
        }

        return view.ScoreFloatValue;
    }

    private SettlementDisplayEvent ActiveSettlementEventForView(TableDieView view)
    {
        if (rollPhase != RollPhase.Scoring || activeSettlementEvent == null || view == null)
        {
            return null;
        }

        if (activeSettlementEvent.Kind == SettlementEventKind.BribeFinal
            || activeSettlementEvent.Kind == SettlementEventKind.TargetSettle)
        {
            return null;
        }

        if (activeSettlementEvent.ScoreIndex < 0 || view.ScoreIndexStart < 0)
        {
            return null;
        }

        int eventEnd = activeSettlementEvent.ScoreIndexEnd >= activeSettlementEvent.ScoreIndex
            ? activeSettlementEvent.ScoreIndexEnd
            : activeSettlementEvent.ScoreIndex;

        if (eventEnd < view.ScoreIndexStart || activeSettlementEvent.ScoreIndex > view.ScoreIndexEnd)
        {
            return null;
        }

        return activeSettlementEvent;
    }

    private float SettlementEventProgress()
    {
        if (rollPhase == RollPhase.Scoring && activeSettlementEvent != null)
        {
            return Mathf.Clamp01(scoreStepTimer / Mathf.Max(0.01f, activeSettlementEvent.Duration));
        }

        float duration = scoreRevealIndex >= scoringDice.Count ? FinalScoreDuration : ScoreStepDuration;
        return Mathf.Clamp01(scoreStepTimer / Mathf.Max(0.01f, duration));
    }

    private void DrawScoreFloat(Rect dieRect, int score, float normalizedTime)
    {
        if (score <= 0)
        {
            return;
        }

        float t = Mathf.Clamp01(normalizedTime);
        Color old = GUI.color;
        GUI.color = new Color(1f, 0.86f, 0.36f, 1f - Mathf.Max(0f, t - 0.65f) * 2.2f);
        GUI.Label(new Rect(dieRect.x - 20f, dieRect.y - 28f - t * 32f, dieRect.width + 40f, 32f), "+" + score, centerStyle);
        GUI.color = old;
    }

    private void DrawSettlementDieEventTag(Rect area, Rect dieRect, SettlementDisplayEvent settlementEvent, float progress)
    {
        if (settlementEvent == null || string.IsNullOrEmpty(settlementEvent.Label))
        {
            return;
        }

        if (settlementEvent.Kind == SettlementEventKind.SlotScore && settlementEvent.ScoreIndexEnd <= settlementEvent.ScoreIndex)
        {
            return;
        }

        float width = settlementEvent.Kind == SettlementEventKind.MultiplierStamp ? 118f : 132f;
        float tagX = dieRect.x + dieRect.width * 0.5f - width * 0.5f;
        tagX = Mathf.Clamp(tagX, area.x + 4f, area.x + area.width - width - 4f);
        float tagY = dieRect.y - 25f;
        if (tagY < area.y + 2f)
        {
            tagY = dieRect.y + dieRect.height + 6f;
        }

        if (tagY + 22f > area.y + area.height - 44f)
        {
            tagY = dieRect.y - 25f;
        }

        Rect tag = new Rect(tagX, tagY, width, 22f);
        Color fill = SettlementEventFillColor(settlementEvent);
        fill.a *= 0.86f + Mathf.Sin(Mathf.Clamp01(progress) * Mathf.PI) * 0.1f;
        DrawRect(new Rect(tag.x + 2f, tag.y + 2f, tag.width, tag.height), new Color(0.08f, 0.04f, 0.02f, 0.2f));
        DrawRect(tag, fill);
        DrawBorder(tag, new Color(1f, 0.9f, 0.55f, 0.76f), 1f);
        DrawRunText(new Rect(tag.x + 5f, tag.y + 1f, tag.width - 10f, tag.height - 2f), settlementEvent.Label, 12, FontStyle.Bold, new Color(0.18f, 0.11f, 0.06f), TextAnchor.MiddleCenter);
    }

    private void DrawSettlementCoinFlight(Rect dieRect, SettlementDisplayEvent settlementEvent, float progress)
    {
        if (settlementEvent == null || settlementEvent.GoldDelta <= 0)
        {
            return;
        }

        float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(progress));
        Vector2 start = new Vector2(dieRect.x + dieRect.width * 0.5f, dieRect.y + dieRect.height * 0.5f);
        Vector2 end = new Vector2(59f, 405f);
        Vector2 position = Vector2.Lerp(start, end, t);
        float size = 20f + Mathf.Sin(t * Mathf.PI) * 7f;
        Rect coin = new Rect(position.x - size * 0.5f, position.y - size * 0.5f, size, size);
        DrawRunUiIcon(coin, RunUiIcon.Coin);

        if (settlementEvent.GoldDelta > 1)
        {
            DrawRunText(new Rect(coin.x + 14f, coin.y - 4f, 34f, 18f), "+" + settlementEvent.GoldDelta, 11, FontStyle.Bold, new Color(0.35f, 0.2f, 0.05f), TextAnchor.MiddleLeft);
        }
    }

    private void DrawSettlementStageFeedback(Rect area)
    {
        if (rollPhase != RollPhase.Scoring || activeSettlementEvent == null || string.IsNullOrEmpty(activeSettlementEvent.Label))
        {
            return;
        }

        Rect banner = new Rect(area.x + area.width * 0.5f - 154f, area.y + area.height - 39f, 308f, 31f);
        Color fill = SettlementEventFillColor(activeSettlementEvent);
        fill.a = 0.92f;
        DrawRect(new Rect(banner.x + 3f, banner.y + 3f, banner.width, banner.height), new Color(0.08f, 0.04f, 0.02f, 0.22f));
        DrawRect(banner, fill);
        DrawBorder(banner, activeSettlementEvent.Passed ? new Color(0.42f, 0.78f, 0.48f, 0.82f) : new Color(1f, 0.86f, 0.48f, 0.78f), 1f);

        Rect textRect = new Rect(banner.x + 14f, banner.y + 2f, banner.width - 28f, banner.height - 4f);
        if (activeSettlementEvent.TargetArea == SettlementTargetArea.Coin)
        {
            DrawRunUiIcon(new Rect(banner.x + 10f, banner.y + 6f, 19f, 19f), RunUiIcon.Coin);
            textRect.x += 22f;
            textRect.width -= 22f;
        }
        else if (activeSettlementEvent.TargetArea == SettlementTargetArea.Target)
        {
            DrawRunUiIcon(new Rect(banner.x + 10f, banner.y + 6f, 19f, 19f), RunUiIcon.TargetScore);
            textRect.x += 22f;
            textRect.width -= 22f;
        }

        DrawRunText(textRect, activeSettlementEvent.Label, 14, FontStyle.Bold, new Color(0.18f, 0.11f, 0.06f), TextAnchor.MiddleCenter);
    }

    private Color SettlementEventFillColor(SettlementDisplayEvent settlementEvent)
    {
        if (settlementEvent == null)
        {
            return new Color(0.92f, 0.78f, 0.48f, 0.9f);
        }

        switch (settlementEvent.HighlightLevel)
        {
            case SettlementHighlightLevel.Route:
                return settlementEvent.GoldDelta > 0
                    ? new Color(0.95f, 0.78f, 0.32f, 0.9f)
                    : new Color(0.6f, 0.78f, 0.62f, 0.9f);
            case SettlementHighlightLevel.Multiplier:
                return new Color(0.9f, 0.55f, 0.34f, 0.92f);
            case SettlementHighlightLevel.Target:
                return settlementEvent.Passed
                    ? new Color(0.58f, 0.78f, 0.56f, 0.92f)
                    : new Color(0.78f, 0.48f, 0.42f, 0.92f);
        }

        return new Color(0.93f, 0.76f, 0.46f, 0.9f);
    }

    private void DrawDieToken(Rect rect, Die die, int value)
    {
        DieType visualType = die.Temporary ? DieType.Turtle : die.Type;
        if (value <= 0)
        {
            DrawReadyDieToken(rect, die, visualType);
            return;
        }

        DrawRuntimeDieFace(rect, value, die.Temporary ? DiceMaterial.None : die.Material);
        DrawResultDieTypeMarker(rect, visualType);
    }

    private string DiceTableSecondaryText(Die die, int value)
    {
        if (value <= 0)
        {
            if (!die.Temporary && die.Type == DieType.Treasury)
            {
                return "本金 +" + TreasuryScoreBonus();
            }

            if (!die.Temporary && die.Type == DieType.Bribe)
            {
                return "救急补分";
            }

            if (!die.Temporary && die.Type == DieType.Investment)
            {
                return die.InvestmentGold > 0 ? "已锁 " + die.InvestmentGold + "金" : "未锁金";
            }

            if (!die.Temporary && die.Type == DieType.BountyGold)
            {
                return die.TargetFace > 0 ? "悬赏 " + die.TargetFace : "抽目标";
            }

            if (!die.Temporary && die.Type == DieType.TopGold)
            {
                return "最大面给金";
            }

            if (!die.Temporary && die.Type == DieType.HandTax)
            {
                return "牌型给金";
            }

            if (!die.Temporary && die.Type == DieType.Collection)
            {
                return die.CollectionTriggeredThisStage ? "已收账" : "首结给金";
            }

            if (!die.Temporary && die.Type == DieType.CompoundInterest)
            {
                return "过关复利";
            }

            if (!die.Temporary && die.Type == DieType.LeadTicket)
            {
                return "出千给金";
            }

            if (!die.Temporary && die.Type == DieType.ShellTax)
            {
                return "小骰税";
            }

            if (!die.Temporary && die.Type == DieType.CounterGold)
            {
                return "左侧收款";
            }

            if (!die.Temporary && die.Type == DieType.LumberGold)
            {
                return "砍左给金";
            }

            if (!die.Temporary && die.Type == DieType.LoneWitness)
            {
                return "落单重摇";
            }

            if (!die.Temporary && die.Type == DieType.Stamp)
            {
                return "同点盖章";
            }

            if (!die.Temporary && die.Type == DieType.HalfStep)
            {
                return "顺子借位";
            }

            if (!die.Temporary && die.Type == DieType.Track)
            {
                return "2/4/6 轨道";
            }

            if (!die.Temporary && die.Type == DieType.ParityNeighborDiff)
            {
                return "左异 6分";
            }

            if (!die.Temporary && die.Type == DieType.ParityNeighborSame)
            {
                return "左同 6分";
            }

            if (!die.Temporary && die.Type == DieType.ParityComplete)
            {
                return "补全奇偶";
            }

            if (!die.Temporary && die.Type == DieType.ParityReview)
            {
                return "破坏复核";
            }

            if (!die.Temporary && die.Type == DieType.ParityFlipScore)
            {
                return "变号 8分";
            }

            if (!die.Temporary && die.Type == DieType.ParityHoldScore)
            {
                return "守号 6分";
            }

            if (!die.Temporary && die.Type == DieType.ParityTurner)
            {
                return "出千变号";
            }

            if (!die.Temporary && die.Type == DieType.Gardener)
            {
                return "大树加速";
            }

            if (!die.Temporary && die.Type == DieType.Irrigation)
            {
                return "浇中成长";
            }

            if (!die.Temporary && die.Type == DieType.PointSeedTree)
            {
                return die.TargetFace > 0 ? "点 " + die.TargetFace + " 成长" : "抽点成长";
            }

            if (!die.Temporary && die.Type == DieType.PatternTree)
            {
                return die.PatternTarget != TreePatternTarget.None ? "谱 " + TreePatternTargetShortText(die.PatternTarget) : "抽牌型";
            }

            if (!die.Temporary && die.Type == DieType.CanopyTree)
            {
                return "最高面成长";
            }

            if (!die.Temporary && die.Type == DieType.RingTree)
            {
                return "出手后成长";
            }

            if (!die.Temporary && die.Type == DieType.FertilizerTree)
            {
                return "金币肥料";
            }

            if (!die.Temporary && die.Type == DieType.PruningTree)
            {
                return "出千成长";
            }

            if (!die.Temporary && die.Type == DieType.RootTree)
            {
                return "看左右";
            }

            return die.Temporary ? "临时" : FaceText(die.Faces);
        }

        if (die.Score > 0 && (rollPhase == RollPhase.Scoring || rollPhase == RollPhase.StageClear))
        {
            return "+" + die.Score;
        }

        if (die.Temporary)
        {
            return "临时";
        }

        if (die.Type == DieType.Piggy && die.TargetFace > 0)
        {
            return die.EffectiveValue == die.TargetFace ? "命中 +" + piggyGoldPerHit + "金" : "目标 " + die.TargetFace;
        }

        if (die.Type == DieType.BountyGold && die.TargetFace > 0)
        {
            return die.EffectiveValue == die.TargetFace ? "悬赏 +" + bountyGoldPerHit + "金" : "悬赏 " + die.TargetFace;
        }

        if (die.Type == DieType.TopGold)
        {
            return RolledHighestFace(die) ? "顶金 +" + topGoldPerHit + "金" : "等最大面";
        }

        if (die.Type == DieType.HandTax)
        {
            int gold = CurrentHandTaxGold();
            return gold > 0 ? "牌税 +" + gold + "金" : "等牌型";
        }

        if (die.Type == DieType.Collection)
        {
            return die.CollectionTriggeredThisStage ? "已收账" : "收账 +" + collectionGoldPerStage + "金";
        }

        if (die.Type == DieType.CompoundInterest)
        {
            int compound = CompoundInterestPreview(StageInterestPreview());
            return compound > 0 ? "复利 +" + compound : "等利息";
        }

        if (die.Type == DieType.LeadTicket)
        {
            return die.CheatRerolledThisSettle ? "铅票 +" + leadTicketGold + "金" : "等出千";
        }

        if (die.Type == DieType.ShellTax)
        {
            return shellTaxThreshold > 0 && previewTurtleTemporaryDieCount >= shellTaxThreshold ? "壳税 +" + shellTaxGold + "金" : "等小骰";
        }

        if (die.Type == DieType.CounterGold)
        {
            return "看左槽";
        }

        if (die.Type == DieType.LumberGold)
        {
            return "左骰 -1";
        }

        if (die.Type == DieType.Tree && die.TargetFace > 0)
        {
            return die.EffectiveValue == die.TargetFace ? "命中成长" : "命中 " + die.TargetFace;
        }

        if (die.Type == DieType.PointSeedTree && die.TargetFace > 0)
        {
            return die.EffectiveValue == die.TargetFace ? "籽中成长" : "籽 " + die.TargetFace;
        }

        if (die.Type == DieType.PatternTree)
        {
            string target = TreePatternTargetShortText(die.PatternTarget);
            return CurrentRollHasLockedResults() && PatternTreeTargetHit(die.PatternTarget, CurrentDiceHandResult()) ? "谱成成长" : "谱 " + target;
        }

        if (die.Type == DieType.CanopyTree)
        {
            return RolledHighestFace(die) ? "冠顶成长" : "等最高面";
        }

        if (die.Type == DieType.RingTree)
        {
            return "年轮成长";
        }

        if (die.Type == DieType.FertilizerTree)
        {
            int fertilizerGrowth = FertilizerGrowthCount();
            return fertilizerGrowth > 0 ? "肥料 x" + fertilizerGrowth : "等金币";
        }

        if (die.Type == DieType.PruningTree)
        {
            if (die.TypeTriggeredThisSettle)
            {
                return "修枝成长";
            }

            return die.CheatRerolledThisSettle ? "已修枝" : "等出千";
        }

        if (die.Type == DieType.RootTree)
        {
            string triggerLabel;
            return RootTreeTriggerLabel(die, out triggerLabel) ? triggerLabel : "看左右";
        }

        if (die.Type == DieType.Gambler && die.GamblerThreshold > 0)
        {
            if (die.EffectiveValue < die.GamblerThreshold)
            {
                return "低于阈值";
            }

            return die.EffectiveValue > die.GamblerThreshold ? "高于阈值" : "阈值 " + die.GamblerThreshold;
        }

        if (IsTreeGrowthType(die.Type) && die.Growth > 0)
        {
            return "成长 " + die.Growth;
        }

        if (die.Type == DieType.Turtle)
        {
            return die.EffectiveValue > 1 ? "启动壳链" : "无壳链";
        }

        if (die.Type == DieType.Shellsmith)
        {
            return previewTurtleTemporaryDieCount > 0 ? "壳 +" + previewTurtleTemporaryDieCount : "等小骰";
        }

        if (die.Type == DieType.Nest)
        {
            return previewNestBonusDieCount > 0 ? "巢 +" + previewNestBonusDieCount : "待龟链";
        }

        if (die.Type == DieType.SlowTurtle)
        {
            return die.EffectiveValue > 1 ? "慢壳链" : "无壳链";
        }

        if (die.Type == DieType.Treasury)
        {
            return "本金 +" + TreasuryScoreBonus();
        }

        if (die.Type == DieType.Bribe)
        {
            return previewBribeGoldCost > 0 ? "可补" : "救急";
        }

        if (die.Type == DieType.Investment)
        {
            int bonus = InvestmentScoreBonus(die);
            return bonus > 0 ? "投资 +" + bonus : "未锁金";
        }

        if (die.Type == DieType.LoneWitness)
        {
            return die.LoneWitnessRerolled ? "重摇为 " + die.EffectiveValue : "追同点";
        }

        if (die.Type == DieType.Stamp)
        {
            return HasSameEffectiveValuePartner(die) ? "盖章 6分" : "待同点";
        }

        if (die.Type == DieType.HalfStep)
        {
            return die.HalfStepBorrowed ? "借 " + die.EffectiveValue + "→" + (die.EffectiveValue - 1) : "可半步";
        }

        if (die.Type == DieType.Track)
        {
            return HasTrackValuesComplete() ? "轨道 8分" : "等 2/4/6";
        }

        if (die.Type == DieType.ParityNeighborDiff)
        {
            return HasLeftParityRelation(die, false) ? "异邻 6" : "等左异";
        }

        if (die.Type == DieType.ParityNeighborSame)
        {
            return HasLeftParityRelation(die, true) ? "同邻 6" : "等左同";
        }

        if (die.Type == DieType.ParityComplete)
        {
            return die.ParityCompleteUsed ? "补全" : "等缺口";
        }

        if (die.Type == DieType.ParityReview)
        {
            return die.ParityReviewRerolled ? "复核为 " + die.EffectiveValue : "复核";
        }

        if (die.Type == DieType.ParityFlipScore)
        {
            return CheatParityChanged(die) ? "翻号 8" : "等变号";
        }

        if (die.Type == DieType.ParityHoldScore)
        {
            return CheatParityHeld(die) ? "守号 6" : "等守号";
        }

        if (die.Type == DieType.ParityTurner)
        {
            return die.CheatRerolledThisSettle ? "已转号" : "等出千 +1";
        }

        if (die.Type == DieType.Gardener)
        {
            int naturalHits = NaturalTreeHitCount();
            return naturalHits > 0 ? "园丁 +" + naturalHits : "等树命中";
        }

        if (die.Type == DieType.Irrigation)
        {
            Die target = FindIrrigationPreviewTarget(die);
            return target != null ? "灌溉 " + target.TargetFace : "等命中点";
        }

        if (die.Type == DieType.Double)
        {
            return "自身 x2";
        }

        if (die.Type == DieType.Odd)
        {
            return "奇面";
        }

        if (die.Type == DieType.Even)
        {
            return "偶面";
        }

        return string.Empty;
    }

    private string DiceTablePrimaryText(Die die, int value)
    {
        if (die.Temporary)
        {
            return die.Name;
        }

        if (die.Type == DieType.Basic && value > 0)
        {
            return string.Empty;
        }

        return ShortTypeName(die.Type);
    }

    private void DrawReadyDieToken(Rect rect, Die die, DieType type)
    {
        if (diceRollReadyTexture != null)
        {
            Color old = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(rect, diceRollReadyTexture, ScaleMode.ScaleToFit, true);
            GUI.color = old;
            DrawReadyDieTypeMarker(rect, type);
            return;
        }

        Texture2D icon = DieTypeIcon(type);
        DrawTintedCircle(new Rect(rect.x + rect.width * 0.12f, rect.y + rect.height * 0.84f, rect.width * 0.76f, rect.height * 0.16f), new Color(0f, 0f, 0f, 0.16f));
        if (icon != null)
        {
            Color old = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
            GUI.color = old;
            return;
        }

        Color typeColor = TypeColor(type);
        Color fallback = Color.Lerp(new Color(0.98f, 0.9f, 0.68f, 0.96f), typeColor, 0.22f);
        DrawDieFace(InsetRect(rect, rect.width * 0.08f, rect.height * 0.08f), 0, fallback);
    }

    private void DrawReadyDieTypeMarker(Rect rect, DieType type)
    {
        if (type == DieType.Basic)
        {
            return;
        }

        Color typeColor = TypeColor(type);
        Color markerColor = new Color(typeColor.r, typeColor.g, typeColor.b, 0.82f);
        float stripHeight = Mathf.Clamp(rect.height * 0.05f, 3f, 5f);
        DrawRect(new Rect(rect.x + rect.width * 0.18f, rect.y + rect.height * 0.86f, rect.width * 0.64f, stripHeight), markerColor);

        float badgeSize = Mathf.Clamp(rect.width * 0.26f, 16f, 24f);
        Rect badgeRect = new Rect(rect.x + rect.width - badgeSize * 0.72f, rect.y + rect.height * 0.02f, badgeSize, badgeSize);
        DrawPanel(badgeRect, new Color(1f, 0.91f, 0.66f, 0.94f));
        DrawBorder(badgeRect, markerColor, 2f);

        Texture2D icon = DieTypeIcon(type);
        Rect iconRect = InsetRect(badgeRect, badgeSize * 0.16f, badgeSize * 0.16f);
        if (icon == null)
        {
            DrawRect(iconRect, markerColor);
            return;
        }

        Color old = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
        GUI.color = old;
    }

    private void DrawResultDieTypeMarker(Rect rect, DieType type)
    {
        Color typeColor = TypeColor(type);
        Color markerColor = new Color(typeColor.r, typeColor.g, typeColor.b, 0.92f);
        if (type == DieType.Basic)
        {
            DrawRect(new Rect(rect.x + rect.width * 0.16f, rect.y + rect.height * 0.88f, rect.width * 0.68f, Mathf.Clamp(rect.height * 0.06f, 3f, 5f)), new Color(0.48f, 0.42f, 0.32f, 0.42f));
            return;
        }

        float stripHeight = Mathf.Clamp(rect.height * 0.08f, 4f, 7f);
        DrawRect(new Rect(rect.x - 2f, rect.y + rect.height * 0.18f, Mathf.Clamp(rect.width * 0.08f, 5f, 8f), rect.height * 0.64f), markerColor);
        DrawRect(new Rect(rect.x + rect.width * 0.16f, rect.y + rect.height * 0.08f, rect.width * 0.68f, stripHeight), markerColor);

        float badgeSize = Mathf.Clamp(rect.width * 0.46f, 22f, 34f);
        Rect badgeRect = new Rect(rect.x + rect.width - badgeSize * 0.78f, rect.y - badgeSize * 0.18f, badgeSize, badgeSize);
        DrawPanel(badgeRect, new Color(1f, 0.91f, 0.66f, 0.97f));
        DrawBorder(badgeRect, markerColor, 3f);

        Rect iconRect = InsetRect(badgeRect, badgeSize * 0.14f, badgeSize * 0.14f);
        Texture2D icon = DieTypeIcon(type);
        if (icon == null)
        {
            DrawRect(iconRect, markerColor);
            return;
        }

        Color old = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
        GUI.color = old;
    }

    private void DrawDieTypeIcon(Rect rect, DieType type)
    {
        Texture2D icon = DieTypeIcon(type);
        if (icon == null)
        {
            DrawDieFace(rect, 0, TypeColor(type));
            return;
        }

        DrawRect(new Rect(rect.x + rect.width * 0.1f, rect.y + rect.height * 0.84f, rect.width * 0.8f, rect.height * 0.12f), new Color(0f, 0f, 0f, 0.16f));
        Color old = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(rect, icon, ScaleMode.ScaleToFit, true);
        GUI.color = old;
    }

    private void DrawRuntimeDieFace(Rect rect, int value, DiceMaterial material)
    {
        DrawRect(new Rect(rect.x + rect.width * 0.12f, rect.y + rect.height * 0.88f, rect.width * 0.76f, rect.height * 0.11f), new Color(0f, 0f, 0f, 0.18f));
        if (runtimeDieFaceBaseTexture != null)
        {
            if (!DrawTextureWithDiceMaterial(rect, runtimeDieFaceBaseTexture, material, 1f))
            {
                Color old = GUI.color;
                GUI.color = Color.white;
                GUI.DrawTexture(rect, runtimeDieFaceBaseTexture, ScaleMode.ScaleToFit, true);
                GUI.color = old;
                DrawDiceMaterialOverlay(rect, material, 0.28f);
            }
        }
        else
        {
            DrawPanel(rect, new Color(0.9f, 0.82f, 0.62f));
            DrawDiceMaterialOverlay(rect, material, 0.34f);
            DrawRect(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, 4f), new Color(1f, 1f, 1f, 0.22f));
        }

        Rect pipArea = InsetRect(rect, rect.width * 0.18f, rect.height * 0.18f);
        DrawDiePips(pipArea, value, new Color(0.08f, 0.08f, 0.07f));
    }

    private void DrawDiceMaterialOverlay(Rect rect, DiceMaterial material, float alpha)
    {
        if (material == DiceMaterial.None)
        {
            return;
        }

        if (whiteTexture != null && DrawTextureWithDiceMaterial(rect, whiteTexture, material, alpha))
        {
            return;
        }

        DrawRect(rect, DiceMaterialFallbackColor(material, alpha));
    }

    private bool DrawTextureWithDiceMaterial(Rect rect, Texture texture, DiceMaterial material, float alpha)
    {
        if (material == DiceMaterial.None || texture == null)
        {
            return false;
        }

        UnityEngine.Material renderMaterial = DiceMaterialRenderMaterial(material);
        if (renderMaterial == null)
        {
            return false;
        }

        renderMaterial.SetFloat("_Alpha", Mathf.Clamp01(alpha));
        Graphics.DrawTexture(rect, texture, renderMaterial);
        return true;
    }

    private UnityEngine.Material DiceMaterialRenderMaterial(DiceMaterial material)
    {
        LoadDiceMaterialRenderMaterials();
        UnityEngine.Material renderMaterial;
        diceMaterialRenderMaterials.TryGetValue(material, out renderMaterial);
        return renderMaterial;
    }

    private Color DiceMaterialFallbackColor(DiceMaterial material, float alpha)
    {
        Color color;
        switch (material)
        {
            case DiceMaterial.OfficialIron:
                color = new Color(0.52f, 0.57f, 0.58f);
                break;
            case DiceMaterial.GiltSeal:
                color = new Color(0.95f, 0.68f, 0.22f);
                break;
            case DiceMaterial.ClearGlaze:
                color = new Color(0.72f, 0.9f, 0.96f);
                break;
            case DiceMaterial.LeadSeal:
                color = new Color(0.38f, 0.36f, 0.34f);
                break;
            case DiceMaterial.CopperBone:
                color = new Color(0.78f, 0.46f, 0.25f);
                break;
            default:
                color = Color.clear;
                break;
        }

        color.a = Mathf.Clamp01(alpha);
        return color;
    }

    private void DrawDieTypeCornerBadge(Rect rect, DieType type)
    {
        float badgeSize = Mathf.Clamp(rect.width * 0.3f, 13f, 24f);
        Rect badgeRect = new Rect(rect.x + rect.width - badgeSize * 0.72f, rect.y - badgeSize * 0.24f, badgeSize, badgeSize);
        DrawPanel(badgeRect, new Color(1f, 0.9f, 0.62f, 0.96f));

        Texture2D icon = DieTypeIcon(type);
        Rect iconRect = InsetRect(badgeRect, badgeSize * 0.12f, badgeSize * 0.12f);
        if (icon == null)
        {
            DrawRect(iconRect, TypeColor(type));
            return;
        }

        Color old = GUI.color;
        GUI.color = Color.white;
        GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
        GUI.color = old;
    }

    private void DrawDieFace(Rect rect, int value, Color color)
    {
        DrawPanel(rect, color);
        DrawRect(new Rect(rect.x + 4f, rect.y + 4f, rect.width - 8f, 4f), new Color(1f, 1f, 1f, 0.2f));

        if (value <= 0)
        {
            return;
        }

        DrawDiePips(InsetRect(rect, rect.width * 0.18f, rect.height * 0.18f), value, new Color(0.08f, 0.08f, 0.07f));
    }

    private Rect InsetRect(Rect rect, float horizontal, float vertical)
    {
        return new Rect(rect.x + horizontal, rect.y + vertical, Mathf.Max(1f, rect.width - horizontal * 2f), Mathf.Max(1f, rect.height - vertical * 2f));
    }

    private void DrawDiePips(Rect area, int value, Color pipColor)
    {
        if (value <= 0)
        {
            return;
        }

        if (value <= 6)
        {
            DrawStandardDiePips(area, value, pipColor);
            return;
        }

        DrawGridDiePips(area, value, pipColor);
    }

    private void DrawStandardDiePips(Rect area, int value, Color pipColor)
    {
        float pip = Mathf.Clamp(Mathf.Min(area.width, area.height) * 0.22f, 4f, 12f);
        float left = area.x + area.width * 0.25f;
        float mid = area.x + area.width * 0.5f;
        float right = area.x + area.width * 0.75f;
        float top = area.y + area.height * 0.25f;
        float center = area.y + area.height * 0.5f;
        float bottom = area.y + area.height * 0.75f;

        if (value == 1 || value == 3 || value == 5)
        {
            DrawPip(mid, center, pip, pipColor);
        }

        if (value >= 2)
        {
            DrawPip(left, top, pip, pipColor);
            DrawPip(right, bottom, pip, pipColor);
        }

        if (value >= 4)
        {
            DrawPip(right, top, pip, pipColor);
            DrawPip(left, bottom, pip, pipColor);
        }

        if (value == 6)
        {
            DrawPip(left, center, pip, pipColor);
            DrawPip(right, center, pip, pipColor);
        }
    }

    private void DrawGridDiePips(Rect area, int value, Color pipColor)
    {
        int columns = Mathf.Clamp(Mathf.CeilToInt(Mathf.Sqrt(value)), 3, 6);
        int rows = Mathf.CeilToInt(value / (float)columns);
        float cellWidth = area.width / columns;
        float cellHeight = area.height / rows;
        float pip = Mathf.Clamp(Mathf.Min(cellWidth, cellHeight) * 0.42f, 3f, Mathf.Min(area.width, area.height) * 0.12f);

        for (int row = 0; row < rows; row++)
        {
            int rowStart = row * columns;
            int rowCount = Mathf.Min(columns, value - rowStart);
            float rowWidth = rowCount * cellWidth;
            float startX = area.x + (area.width - rowWidth) * 0.5f + cellWidth * 0.5f;
            float y = area.y + (row + 0.5f) * cellHeight;
            for (int column = 0; column < rowCount; column++)
            {
                DrawPip(startX + column * cellWidth, y, pip, pipColor);
            }
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
        DrawRect(rect, new Color(0.21f, 0.15f, 0.09f, 0.42f));
        Rect inner = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
        DrawRect(inner, new Color(0.04f, 0.05f, 0.05f));
        float amount = Mathf.Clamp01((float)value / Mathf.Max(1, target));
        if (amount > 0f)
        {
            DrawRect(new Rect(inner.x, inner.y, inner.width * amount, inner.height), new Color(0.88f, 0.63f, 0.26f));
        }
    }

    private void DrawHudBar(string left, string center, string right)
    {
        DrawRect(new Rect(0f, 0f, VirtualWidth, 90f), new Color(0.16f, 0.11f, 0.07f, 0.58f));
        DrawRect(new Rect(0f, 88f, VirtualWidth, 4f), new Color(0.83f, 0.62f, 0.31f, 0.78f));
        GUI.Label(new Rect(48f, 18f, 260f, 46f), left, hudTitleStyle);
        GUI.Label(new Rect(316f, 26f, 440f, 32f), center, hudHeaderStyle);
        GUI.Label(new Rect(770f, 28f, 456f, 30f), right, hudBodyStyle);
    }

    private void DrawUiPanel(Rect rect)
    {
        DrawParchmentFrame(rect, true, true);
    }

    private void DrawUiSidePanel(Rect rect)
    {
        DrawParchmentFrame(rect, false, false);
    }

    private void DrawUiCard(Rect rect)
    {
        DrawParchmentFrame(rect, true, false);
    }

    private void DrawUiSmallPanel(Rect rect)
    {
        DrawParchmentFrame(rect, false, false);
    }

    private bool DrawUiButton(Rect rect, string label, UiButtonKind kind)
    {
        bool enabled = GUI.enabled;
        DrawScalableButtonFrame(rect, kind, enabled);

        bool clicked = enabled && GUI.Button(rect, GUIContent.none, GUIStyle.none);
        bool oldEnabled = GUI.enabled;
        GUI.enabled = true;
        GUI.Label(rect, label, enabled ? artButtonStyle : disabledButtonLabelStyle);
        GUI.enabled = oldEnabled;
        return clicked;
    }

    private void DrawParchmentFrame(Rect rect, bool strong, bool allowSeal)
    {
        Color shadow = new Color(0.12f, 0.08f, 0.05f, strong ? 0.32f : 0.12f);
        Color fill = strong ? new Color(1f, 0.94f, 0.76f, 0.94f) : new Color(1f, 0.93f, 0.75f, 0.76f);
        Color outline = new Color(0.23f, 0.16f, 0.11f, strong ? 0.92f : 0.52f);
        Color brass = new Color(0.9f, 0.64f, 0.22f, strong ? 0.72f : 0.24f);

        DrawRect(new Rect(rect.x + 4f, rect.y + 5f, rect.width, rect.height), shadow);
        DrawRect(rect, fill);
        DrawBorder(rect, outline, strong ? 4f : 2f);
        DrawBorder(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f), new Color(0.84f, 0.56f, 0.18f, strong ? 0.5f : 0.16f), strong ? 2f : 1f);

        float corner = Mathf.Clamp(Mathf.Min(rect.width, rect.height) * 0.08f, 12f, 30f);
        DrawCornerBlock(new Rect(rect.x + 12f, rect.y + 12f, corner, corner), brass);
        DrawCornerBlock(new Rect(rect.x + rect.width - corner - 12f, rect.y + 12f, corner, corner), brass);
        DrawCornerBlock(new Rect(rect.x + 12f, rect.y + rect.height - corner - 12f, corner, corner), brass);
        DrawCornerBlock(new Rect(rect.x + rect.width - corner - 12f, rect.y + rect.height - corner - 12f, corner, corner), brass);

        if (allowSeal && rect.width >= 420f && rect.height >= 180f)
        {
            DrawFixedCrownSeal(new Rect(rect.x + rect.width * 0.5f - 30f, rect.y - 8f, 60f, 44f));
        }
    }

    private void DrawScalableButtonFrame(Rect rect, UiButtonKind kind, bool enabled)
    {
        Color fill;
        Color inner;
        Color edge = new Color(0.22f, 0.15f, 0.1f, 0.92f);
        Color accent;
        if (!enabled)
        {
            fill = new Color(0.67f, 0.63f, 0.54f, 0.92f);
            inner = new Color(0.82f, 0.77f, 0.66f, 0.7f);
            accent = new Color(0.5f, 0.47f, 0.42f, 0.72f);
        }
        else if (kind == UiButtonKind.Primary)
        {
            fill = new Color(0.95f, 0.72f, 0.25f, 0.95f);
            inner = new Color(1f, 0.91f, 0.62f, 0.9f);
            accent = new Color(0.22f, 0.63f, 0.58f, 0.85f);
        }
        else if (kind == UiButtonKind.Danger)
        {
            fill = new Color(0.87f, 0.35f, 0.3f, 0.95f);
            inner = new Color(1f, 0.65f, 0.5f, 0.72f);
            accent = new Color(0.72f, 0.2f, 0.18f, 0.85f);
        }
        else
        {
            fill = new Color(1f, 0.93f, 0.72f, 0.92f);
            inner = new Color(1f, 0.98f, 0.84f, 0.82f);
            accent = new Color(0.84f, 0.57f, 0.18f, 0.72f);
        }

        DrawRect(new Rect(rect.x + 3f, rect.y + 4f, rect.width, rect.height), new Color(0.1f, 0.06f, 0.03f, 0.28f));
        DrawRect(rect, fill);
        DrawBorder(rect, edge, 3f);
        DrawBorder(new Rect(rect.x + 6f, rect.y + 6f, rect.width - 12f, rect.height - 12f), inner, 2f);

        float leftWidth = Mathf.Clamp(rect.height * 0.72f, 28f, 42f);
        Rect accentRect = new Rect(rect.x + 8f, rect.y + 8f, leftWidth, rect.height - 16f);
        DrawRect(accentRect, accent);
        DrawMiniCrown(new Rect(accentRect.x + accentRect.width * 0.5f - 12f, accentRect.y + accentRect.height * 0.5f - 10f, 24f, 20f), enabled ? new Color(1f, 0.92f, 0.66f, 0.92f) : new Color(0.78f, 0.72f, 0.6f, 0.78f), edge);
    }

    private void DrawBorder(Rect rect, Color color, float thickness)
    {
        DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
        DrawRect(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color);
        DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
        DrawRect(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color);
    }

    private void DrawCornerBlock(Rect rect, Color color)
    {
        DrawRect(rect, color);
        DrawRect(new Rect(rect.x + rect.width * 0.22f, rect.y, rect.width * 0.56f, rect.height), new Color(1f, 0.92f, 0.58f, color.a * 0.42f));
    }

    private void DrawFixedCrownSeal(Rect rect)
    {
        DrawRect(new Rect(rect.x + 3f, rect.y + 4f, rect.width, rect.height), new Color(0.12f, 0.08f, 0.05f, 0.28f));
        DrawRect(rect, new Color(0.78f, 0.54f, 0.2f, 0.95f));
        DrawBorder(rect, new Color(0.22f, 0.15f, 0.1f, 0.92f), 3f);
        DrawMiniCrown(new Rect(rect.x + rect.width * 0.5f - 15f, rect.y + rect.height * 0.5f - 12f, 30f, 24f), new Color(1f, 0.85f, 0.38f, 0.95f), new Color(0.34f, 0.24f, 0.14f, 0.95f));
    }

    private void DrawMiniCrown(Rect rect, Color fill, Color outline)
    {
        DrawRect(new Rect(rect.x, rect.y + rect.height * 0.68f, rect.width, rect.height * 0.22f), outline);
        DrawRect(new Rect(rect.x + rect.width * 0.08f, rect.y + rect.height * 0.62f, rect.width * 0.84f, rect.height * 0.18f), fill);
        DrawRect(new Rect(rect.x + rect.width * 0.08f, rect.y + rect.height * 0.32f, rect.width * 0.16f, rect.height * 0.36f), fill);
        DrawRect(new Rect(rect.x + rect.width * 0.42f, rect.y + rect.height * 0.08f, rect.width * 0.16f, rect.height * 0.6f), fill);
        DrawRect(new Rect(rect.x + rect.width * 0.76f, rect.y + rect.height * 0.32f, rect.width * 0.16f, rect.height * 0.36f), fill);
    }

    private void DrawRunText(Rect rect, string text, int size, FontStyle fontStyle, Color color, TextAnchor alignment)
    {
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.font = uiFont;
        style.fontSize = size;
        style.fontStyle = fontStyle;
        style.normal.textColor = color;
        style.alignment = alignment;
        style.wordWrap = false;
        style.clipping = TextClipping.Clip;
        GUI.Label(rect, text, style);
    }

    private void DrawRunUiIcon(Rect rect, RunUiIcon icon)
    {
        Texture2D texture = null;
        if (icon == RunUiIcon.Coin)
        {
            texture = uiIconCoinTexture;
        }
        else if (icon == RunUiIcon.TargetScore)
        {
            texture = uiIconTargetTexture;
        }

        if (texture != null)
        {
            Color old = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
            GUI.color = old;
            return;
        }

        if (icon == RunUiIcon.Roll)
        {
            DrawDieFace(rect, 3, new Color(0.96f, 0.86f, 0.64f, 0.95f));
            return;
        }

        if (icon == RunUiIcon.Cheat)
        {
            DrawTintedCircle(rect, new Color(0.76f, 0.18f, 0.16f, 0.94f));
            DrawMiniCrown(InsetRect(rect, rect.width * 0.22f, rect.height * 0.25f), new Color(1f, 0.84f, 0.48f, 0.96f), new Color(0.4f, 0.08f, 0.06f, 0.92f));
            return;
        }

        if (icon == RunUiIcon.TargetScore)
        {
            DrawTintedCircle(rect, new Color(0.88f, 0.22f, 0.18f, 0.88f));
            DrawTintedCircle(InsetRect(rect, rect.width * 0.24f, rect.height * 0.24f), new Color(1f, 0.88f, 0.64f, 0.95f));
            DrawTintedCircle(InsetRect(rect, rect.width * 0.39f, rect.height * 0.39f), new Color(0.72f, 0.14f, 0.12f, 0.9f));
            return;
        }

        DrawTintedCircle(rect, new Color(0.88f, 0.62f, 0.22f, 0.9f));
        DrawMiniCrown(InsetRect(rect, rect.width * 0.16f, rect.height * 0.22f), new Color(1f, 0.9f, 0.58f, 0.96f), new Color(0.45f, 0.28f, 0.1f, 0.92f));
    }

    private void DrawTintedCircle(Rect rect, Color color)
    {
        Color old = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, pipTexture);
        GUI.color = old;
    }

    private void DrawIconLabel(Rect rect, Texture2D icon, string label, GUIStyle style)
    {
        if (icon != null)
        {
            Rect iconRect = new Rect(rect.x, rect.y + Mathf.Max(0f, (rect.height - 24f) * 0.5f), 24f, 24f);
            Color old = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleToFit, true);
            GUI.color = old;
        }

        GUI.Label(new Rect(rect.x + 30f, rect.y, rect.width - 30f, rect.height), label, style);
    }

    private void DrawPanel(Rect rect, Color color)
    {
        DrawRect(rect, new Color(0f, 0f, 0f, 0.28f));
        Rect inner = new Rect(rect.x + 2f, rect.y + 2f, rect.width - 4f, rect.height - 4f);
        DrawRect(inner, color);
        DrawRect(new Rect(inner.x, inner.y, inner.width, 2f), new Color(1f, 1f, 1f, 0.08f));
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
            Color old = GUI.color;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(0f, 0f, VirtualWidth, VirtualHeight), tableBackgroundTexture, ScaleMode.ScaleAndCrop);
            GUI.color = old;
        }
        else
        {
            DrawRect(new Rect(0f, 0f, VirtualWidth, VirtualHeight), new Color(0.12f, 0.15f, 0.14f));
            DrawRect(new Rect(0f, 0f, VirtualWidth, VirtualHeight), new Color(0.02f, 0.02f, 0.02f, 0.24f));
        }

        DrawRect(new Rect(0f, 0f, VirtualWidth, 90f), new Color(0.18f, 0.12f, 0.07f, 0.34f));
        DrawRect(new Rect(0f, 88f, VirtualWidth, 4f), new Color(0.83f, 0.62f, 0.31f, 0.62f));
        DrawRect(new Rect(0f, 650f, VirtualWidth, 70f), new Color(0.36f, 0.23f, 0.12f, 0.16f));
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
        if (!HasCurrentRunSaveVersion())
        {
            ClearSave();
            StartDefaultBuild();
            mode = GameMode.Run;
            return;
        }

        int savedStage = Mathf.Clamp(PlayerPrefs.GetInt(SavePrefix + "StageIndex", 0), 0, Mathf.Max(0, encounters.Count - 1));
        int savedGold = PlayerPrefs.GetInt(SavePrefix + "Gold", startingGold);

        suppressSave = true;
        ResetRun("王室骰袋", "当前骰袋容量上限 6，关间市场负责买卖调整。");
        LoadRunCollection();
        LoadCraftingItemInventory();
        if (dice.Count == 0)
        {
            AddStarterDiceBag();
        }
        stageIndex = savedStage;
        chapterGold = savedGold;
        suppressSave = false;
        StartEncounter();
        mode = GameMode.Run;
    }

    private void LoadMenuState()
    {
        hasSave = PlayerPrefs.GetInt(SavePrefix + "HasSave", 0) == 1;
        if (hasSave && !HasCurrentRunSaveVersion())
        {
            ClearSave();
            hasSave = false;
        }

        seenOpening = PlayerPrefs.GetInt(SavePrefix + "SeenOpening", 0) == 1;
        settingsVolume = PlayerPrefs.GetFloat(SavePrefix + "Volume", 1f);
        windowed = PlayerPrefs.GetInt(SavePrefix + "Windowed", 1) == 1;
        AudioListener.volume = settingsVolume;
        Screen.fullScreen = !windowed;
    }

    private bool HasCurrentRunSaveVersion()
    {
        return PlayerPrefs.GetInt(SavePrefix + "HasSave", 0) == 1
            && PlayerPrefs.GetInt(SavePrefix + "SaveVersion", 0) >= CurrentSaveVersion;
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
        if (suppressSave)
        {
            return;
        }

        hasSave = true;
        PlayerPrefs.SetInt(SavePrefix + "HasSave", 1);
        PlayerPrefs.SetInt(SavePrefix + "SaveVersion", CurrentSaveVersion);
        PlayerPrefs.SetInt(SavePrefix + "StageIndex", stageIndex);
        PlayerPrefs.SetInt(SavePrefix + "Gold", chapterGold);
        PlayerPrefs.SetInt(SavePrefix + "SeenOpening", seenOpening ? 1 : 0);
        PlayerPrefs.SetString(SavePrefix + "DiceData", SerializeDice());
        PlayerPrefs.SetInt(SavePrefix + "AffixAddStone", affixAddStoneCount);
        PlayerPrefs.SetInt(SavePrefix + "AffixRemoveStone", affixRemoveStoneCount);
        PlayerPrefs.SetInt(SavePrefix + "AffixReplaceStone", affixReplaceStoneCount);
        PlayerPrefs.Save();
    }

    private void ClearSave()
    {
        hasSave = false;
        PlayerPrefs.SetInt(SavePrefix + "HasSave", 0);
        PlayerPrefs.DeleteKey(SavePrefix + "SaveVersion");
        PlayerPrefs.DeleteKey(SavePrefix + "DiceData");
        PlayerPrefs.DeleteKey(SavePrefix + "StageIndex");
        PlayerPrefs.DeleteKey(SavePrefix + "Gold");
        PlayerPrefs.DeleteKey(SavePrefix + "AffixAddStone");
        PlayerPrefs.DeleteKey(SavePrefix + "AffixRemoveStone");
        PlayerPrefs.DeleteKey(SavePrefix + "AffixReplaceStone");
        PlayerPrefs.Save();
    }

    private void LoadCraftingItemInventory()
    {
        affixAddStoneCount = Mathf.Max(0, PlayerPrefs.GetInt(SavePrefix + "AffixAddStone", 0));
        affixRemoveStoneCount = Mathf.Max(0, PlayerPrefs.GetInt(SavePrefix + "AffixRemoveStone", 0));
        affixReplaceStoneCount = Mathf.Max(0, PlayerPrefs.GetInt(SavePrefix + "AffixReplaceStone", 0));
    }

    private string SerializeDice()
    {
        StringBuilder data = new StringBuilder();
        for (int i = 0; i < dice.Count; i++)
        {
            Die die = dice[i];
            if (i > 0)
            {
                data.Append("|");
            }

            data.Append(die.Name.Replace("~", string.Empty).Replace("|", string.Empty));
            data.Append("~");
            data.Append(die.Type.ToString());
            data.Append("~");
            data.Append(FaceText(die.Faces).Replace("/", ","));
            data.Append("~");
            data.Append(die.Growth);
            data.Append("~");
            data.Append(MaterialKey(die.Material));
            data.Append("~");
            data.Append(SerializeAffixes(die.PrefixAffixes));
            data.Append("~");
            data.Append(SerializeAffixes(die.SuffixAffixes));
        }

        return data.ToString();
    }

    private void LoadRunCollection()
    {
        string diceData = PlayerPrefs.GetString(SavePrefix + "DiceData", string.Empty);
        if (string.IsNullOrEmpty(diceData))
        {
            return;
        }

        dice.Clear();
        nextDieId = 1;
        string[] entries = diceData.Split('|');
        for (int i = 0; i < entries.Length; i++)
        {
            string[] fields = entries[i].Split('~');
            if (fields.Length < 5)
            {
                continue;
            }

            int growth;
            DieType type;
            if (!TryParseDieType(fields[1], out type))
            {
                continue;
            }

            DiceMaterial material;
            if (!TryParseDiceMaterial(fields[4], out material))
            {
                continue;
            }

            int.TryParse(fields[3], out growth);
            Die die = NewDie(fields[0], type, ParseFaces(fields[2]));
            die.Material = material;
            die.Growth = Mathf.Max(0, growth);
            die.PrefixAffixes = fields.Length > 5 ? ParseAffixes(fields[5], AffixSlot.Prefix) : new List<AffixInstance>();
            die.SuffixAffixes = fields.Length > 6 ? ParseAffixes(fields[6], AffixSlot.Suffix) : new List<AffixInstance>();
            dice.Add(die);
        }

        EnsureDiceLimit();
    }

    private string SerializeAffixes(List<AffixInstance> affixes)
    {
        if (affixes == null || affixes.Count == 0)
        {
            return string.Empty;
        }

        StringBuilder data = new StringBuilder();
        for (int i = 0; i < affixes.Count; i++)
        {
            AffixInstance affix = affixes[i];
            if (affix == null || string.IsNullOrEmpty(affix.Key))
            {
                continue;
            }

            if (data.Length > 0)
            {
                data.Append(";");
            }

            data.Append(SanitizeSaveToken(affix.Key));
            data.Append(":");
            data.Append(Mathf.Clamp(affix.Tier, 1, 6));
        }

        return data.ToString();
    }

    private List<AffixInstance> ParseAffixes(string data, AffixSlot slot)
    {
        List<AffixInstance> result = new List<AffixInstance>();
        if (string.IsNullOrEmpty(data))
        {
            return result;
        }

        int limit = slot == AffixSlot.Prefix ? MaxPrefixAffixes : MaxSuffixAffixes;
        HashSet<string> mutexGroups = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        string[] parts = data.Split(';');
        for (int i = 0; i < parts.Length && result.Count < limit; i++)
        {
            string part = parts[i].Trim();
            if (part.Length == 0)
            {
                continue;
            }

            string[] fields = part.Split(':');
            string key = fields[0].Trim();
            int tier = 6;
            if (fields.Length > 1)
            {
                int parsedTier;
                if (int.TryParse(fields[1], out parsedTier))
                {
                    tier = parsedTier;
                }
            }

            AffixDefinition definition;
            if (!affixDefinitions.TryGetValue(key, out definition) || definition.Slot != slot)
            {
                continue;
            }

            string mutex = string.IsNullOrEmpty(definition.MutexGroup) ? definition.Key : definition.MutexGroup;
            if (mutexGroups.Contains(mutex))
            {
                continue;
            }

            mutexGroups.Add(mutex);
            result.Add(new AffixInstance
            {
                Key = definition.Key,
                Tier = Mathf.Clamp(tier, 1, 6)
            });
        }

        return result;
    }

    private static string SanitizeSaveToken(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Replace("~", string.Empty).Replace("|", string.Empty).Replace(";", string.Empty).Replace(":", string.Empty);
    }

    private static bool TryParseDieType(string value, out DieType type)
    {
        type = DieType.Basic;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        int typeIndex;
        if (int.TryParse(value, out typeIndex))
        {
            if (Enum.IsDefined(typeof(DieType), typeIndex))
            {
                type = (DieType)typeIndex;
                return true;
            }

            return false;
        }

        try
        {
            type = (DieType)Enum.Parse(typeof(DieType), value, true);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static bool TryParseDiceMaterial(string value, out DiceMaterial material)
    {
        material = DiceMaterial.None;
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        string key = value.Trim().ToLowerInvariant();
        switch (key)
        {
            case "none":
                material = DiceMaterial.None;
                return true;
            case "official_iron":
            case "officialiron":
                material = DiceMaterial.OfficialIron;
                return true;
            case "gilt_seal":
            case "giltseal":
                material = DiceMaterial.GiltSeal;
                return true;
            case "clear_glaze":
            case "clearglaze":
                material = DiceMaterial.ClearGlaze;
                return true;
            case "lead_seal":
            case "leadseal":
                material = DiceMaterial.LeadSeal;
                return true;
            case "copper_bone":
            case "copperbone":
                material = DiceMaterial.CopperBone;
                return true;
        }

        int materialIndex;
        if (int.TryParse(value, out materialIndex))
        {
            if (Enum.IsDefined(typeof(DiceMaterial), materialIndex))
            {
                material = (DiceMaterial)materialIndex;
                return true;
            }

            return false;
        }

        try
        {
            material = (DiceMaterial)Enum.Parse(typeof(DiceMaterial), value, true);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }

    private static string MaterialKey(DiceMaterial material)
    {
        switch (material)
        {
            case DiceMaterial.OfficialIron:
                return "official_iron";
            case DiceMaterial.GiltSeal:
                return "gilt_seal";
            case DiceMaterial.ClearGlaze:
                return "clear_glaze";
            case DiceMaterial.LeadSeal:
                return "lead_seal";
            case DiceMaterial.CopperBone:
                return "copper_bone";
        }

        return "none";
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
                faces[i] = Mathf.Max(1, value);
            }
        }

        Array.Sort(faces);
        return faces;
    }

    private static int[] SortedFaces(int[] faces)
    {
        if (faces == null || faces.Length == 0)
        {
            return null;
        }

        int[] copy = (int[])faces.Clone();
        Array.Sort(copy);
        return copy;
    }

    private void StartDefaultBuild()
    {
        ResetRun("王室骰袋", "初始骰袋：6 颗基础骰。其它骰子通过关间市场购买获得。");
        AddStarterDiceBag();
        StartEncounter();
    }

    private void AddStarterDiceBag()
    {
        for (int i = 0; i < DiceCapacity; i++)
        {
            AddDie(NewDie("基础骰", DieType.Basic, new int[] { 1, 2, 3, 4, 5, 6 }));
        }
    }

    private void ResetRun(string newBuildName, string newBuildSummary)
    {
        dice.Clear();
        marketOffers.Clear();
        logLines.Clear();
        scoringDice.Clear();
        ClearDiceVisualStates();
        pendingTreeGrowth.Clear();
        activeCraftingItemKey = string.Empty;
        affixAddStoneCount = 0;
        affixRemoveStoneCount = 0;
        affixReplaceStoneCount = 0;
        nextDieId = 1;
        stageIndex = 0;
        chapterGold = startingGold;
        stageStartGold = chapterGold;
        stageInvestmentGold = 0;
        lastBribeGoldSpent = 0;
        lastBribeScoreBonus = 0;
        lastAffixScoreBonus = 0;
        lastWalletIncome = 0;
        currentScore = 0;
        resolvedScore = 0;
        ClearCommittedRunScoreCounter();
        passed = false;
        rolledThisEncounter = false;
        rewardBanner = string.Empty;
        buildName = newBuildName;
        buildSummary = newBuildSummary;
        lastStageFlatIncome = 0;
        lastStageInterestIncome = 0;
        lastStageCompoundInterestIncome = 0;
        lastStageIncome = 0;
        mode = GameMode.Run;
    }

    private void StartEncounter()
    {
        Encounter encounter = CurrentEncounter();
        currentScore = 0;
        resolvedScore = 0;
        scoreRevealIndex = 0;
        stageStartGold = Mathf.Max(0, chapterGold);
        stageInvestmentGold = 0;
        lastStageFlatIncome = 0;
        lastStageInterestIncome = 0;
        lastStageCompoundInterestIncome = 0;
        lastStageIncome = 0;
        lastBribeGoldSpent = 0;
        lastBribeScoreBonus = 0;
        lastAffixScoreBonus = 0;
        lastWalletIncome = 0;
        lastComboBonus = 0;
        lastTemporaryScore = 0;
        ClearResultPreview();
        ClearCommittedRunScoreCounter();
        shakeTimer = 0f;
        shakePower = 0f;
        stopTimer = 0f;
        stopStartPower = 0f;
        lastShakeTapTime = -999f;
        shakeExpiredPromptTimer = 0f;
        promptPulseTimer = 0f;
        shakeImpulseCount = 0;
        scoreStepTimer = 0f;
        passed = false;
        rolledThisEncounter = false;
        finalScoreApplied = false;
        rollPhase = RollPhase.Ready;
        BeginDiceVisualEnter();
        cheatRerollIds.Clear();
        selectedReadySlotIndex = -1;
        rewardBanner = string.Empty;
        scoringDice.Clear();
        pendingTreeGrowth.Clear();
        rollsLeft = RollsPerStage;
        cheatsLeft = CheatsPerStage;

        for (int i = 0; i < dice.Count; i++)
        {
            ResetRoundState(dice[i]);
            dice[i].CollectionTriggeredThisStage = false;
        }

        EnsureDiceLimit();
        if (encounter != null)
        {
            AddLog("进入 " + encounter.Name + "，目标 " + encounter.Target + "。");
        }

        SaveRun();
        AllocateStageInvestments();
    }

    private void AllocateStageInvestments()
    {
        stageInvestmentGold = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            dice[i].InvestmentGold = 0;
        }

        int investmentCount = CountDiceOfType(DieType.Investment);
        if (investmentCount <= 0 || investmentGoldCapPerDie <= 0 || investmentScorePerGold <= 0)
        {
            return;
        }

        int walletLimit = investmentWalletDivisor > 0 ? stageStartGold / investmentWalletDivisor : stageStartGold;
        int totalBudget = Mathf.Min(investmentCount * investmentGoldCapPerDie, walletLimit, Mathf.Max(0, chapterGold));
        if (totalBudget <= 0)
        {
            AddLog("投资骰没有可锁定预算。");
            return;
        }

        int remaining = totalBudget;
        for (int i = 0; i < dice.Count && remaining > 0; i++)
        {
            Die die = dice[i];
            if (die.Type != DieType.Investment)
            {
                continue;
            }

            int locked = Mathf.Min(investmentGoldCapPerDie, remaining);
            die.InvestmentGold = locked;
            remaining -= locked;
        }

        stageInvestmentGold = totalBudget - remaining;
        chapterGold = Mathf.Max(0, chapterGold - stageInvestmentGold);
        if (stageInvestmentGold > 0)
        {
            int scoreBonus = stageInvestmentGold * Mathf.Max(0, investmentScorePerGold);
            rewardBanner = "投资锁定 " + stageInvestmentGold + " 金，剩余 " + chapterGold + " 金，本关每次 +" + scoreBonus + " 单骰分。";
            AddLog("投资锁定 " + stageInvestmentGold + " 金，本关每次结算加 " + scoreBonus + " 分。");
        }
    }

    private void BeginShakeRoll()
    {
        if (rollsLeft <= 0 || passed || rollPhase != RollPhase.Ready)
        {
            return;
        }

        if (dice.Count == 0)
        {
            AddLog("骰袋为空，不能投掷。");
            return;
        }

        rolledThisEncounter = true;
        resolvedScore = currentScore;
        lastBribeGoldSpent = 0;
        lastBribeScoreBonus = 0;
        lastAffixScoreBonus = 0;
        lastWalletIncome = 0;
        lastComboBonus = 0;
        lastTemporaryScore = 0;
        ClearResultPreview();
        ClearCommittedRunScoreCounter();
        scoreRevealIndex = 0;
        scoreStepTimer = 0f;
        activeRollFeedbackConfig = rollFeedbackConfig.Clone();
        shakeTimer = activeRollFeedbackConfig.InputWindowDuration;
        shakePower = activeRollFeedbackConfig.BasePower;
        stopTimer = 0f;
        stopStartPower = 0f;
        lastShakeTapTime = -999f;
        shakeExpiredPromptTimer = 0f;
        promptPulseTimer = 0f;
        shakeImpulseCount = 0;
        finalScoreApplied = false;
        scoringDice.Clear();
        pendingTreeGrowth.Clear();
        cheatRerollIds.Clear();
        selectedReadySlotIndex = -1;

        for (int i = 0; i < dice.Count; i++)
        {
            ResetRoundState(dice[i]);
            AssignRoundTarget(dice[i]);
        }

        BeginDiceVisualRoll();
        rollPhase = RollPhase.Shaking;
        AddLog("骰子开始旋转，敲空格加速。");
        Debug.Log("DiceKingDemo: roll feedback snapshot captured from " + rollFeedbackConfigSource + ". " + RollFeedbackSummary(activeRollFeedbackConfig));
    }

    private void UpdateShakeRoll()
    {
        RollFeedbackConfig config = CurrentRollFeedbackConfig();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryApplyShakeImpulse(config);
        }

        shakeTimer -= Time.deltaTime;
        UpdateRollPromptTimers();
        shakePower = Mathf.Max(config.BasePower, shakePower - Time.deltaTime * config.PowerDecayPerSecond);
        if (shakeTimer <= 0f)
        {
            BeginStopRoll();
        }
    }

    private void TryApplyShakeImpulse(RollFeedbackConfig config)
    {
        if (config == null || shakeTimer <= 0f)
        {
            return;
        }

        float now = Time.time;
        if (now - lastShakeTapTime < config.InputDebounce)
        {
            return;
        }

        lastShakeTapTime = now;
        if (shakeImpulseCount >= config.MaxImpulseCount)
        {
            return;
        }

        shakeImpulseCount++;
        float maxPower = Mathf.Max(config.BasePower, config.MaxPower);
        shakePower = Mathf.Min(maxPower, shakePower + config.ImpulsePower);
        promptPulseTimer = config.PromptPulseDuration;
        diceVisualImpulseTimer = DiceVisualImpulseDuration;
    }

    private void BeginStopRoll()
    {
        RollFeedbackConfig config = CurrentRollFeedbackConfig();
        shakeTimer = 0f;
        stopTimer = 0f;
        stopStartPower = Mathf.Max(config.StopMinPower, shakePower);
        shakeExpiredPromptTimer = config.ExpiredPromptDuration;
        LockCurrentRollResults();
        rollPhase = RollPhase.Stopping;
        AddLog("加力窗口结束，骰子即将停转。");
    }

    private void UpdateStopRoll()
    {
        RollFeedbackConfig config = CurrentRollFeedbackConfig();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            shakeExpiredPromptTimer = config.ExpiredPromptDuration;
        }

        stopTimer += Time.deltaTime;
        UpdateRollPromptTimers();
        float stopDuration = Mathf.Max(0.05f, config.StopDuration);
        float t = Mathf.Clamp01(stopTimer / stopDuration);
        shakePower = Mathf.Max(config.StopMinPower, stopStartPower * (1f - t));
        if (stopTimer >= stopDuration)
        {
            FinishShakeRoll();
        }
    }

    private void UpdateRollPromptTimers()
    {
        promptPulseTimer = Mathf.Max(0f, promptPulseTimer - Time.deltaTime);
        shakeExpiredPromptTimer = Mathf.Max(0f, shakeExpiredPromptTimer - Time.deltaTime);
        diceVisualImpulseTimer = Mathf.Max(0f, diceVisualImpulseTimer - Time.deltaTime);
    }

    private void FinishShakeRoll()
    {
        if (dice.Count == 0)
        {
            rollPhase = RollPhase.Ready;
            return;
        }

        LockCurrentRollResults();
        rollPhase = RollPhase.ResultDecision;
        AddLog("停转显点，结果锁定。可直接结算或出千。");
    }

    private void LockCurrentRollResults()
    {
        if (dice.Count == 0 || CurrentRollHasLockedResults())
        {
            return;
        }

        rollsLeft--;
        for (int i = 0; i < dice.Count; i++)
        {
            RollOneDie(dice[i]);
        }

        ApplyLoneWitnessRerolls();
        ApplyParityReviewRerolls();
        scoringDice.Clear();
        scoringDice.AddRange(dice);
        RefreshHandPreview();

        resolvedScore = currentScore;
        scoreRevealIndex = 0;
        scoreStepTimer = 0f;
        finalScoreApplied = false;
        cheatRerollIds.Clear();
    }

    private bool CurrentRollHasLockedResults()
    {
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i] != null && dice[i].EffectiveValue > 0)
            {
                return true;
            }
        }

        return false;
    }

    private void BeginCheatEdit()
    {
        if (cheatsLeft <= 0 || rollPhase != RollPhase.ResultDecision || dice.Count == 0)
        {
            return;
        }

        cheatRerollIds.Clear();
        rollPhase = RollPhase.CheatEdit;
        AddLog("出千：选择最多 " + MaxCheatRerollDice + " 颗实体骰点数 +1，最高不超过骰面上限。");
    }

    private void CancelCheatEdit()
    {
        if (rollPhase != RollPhase.CheatEdit)
        {
            return;
        }

        cheatRerollIds.Clear();
        RefreshHandPreview();
        rollPhase = RollPhase.ResultDecision;
        AddLog("取消出千选择，返回结算。");
    }

    private void ConfirmCheatAndSettle()
    {
        if (cheatsLeft <= 0 || rollPhase != RollPhase.CheatEdit)
        {
            return;
        }

        RemoveInvalidCheatSelections();
        if (cheatRerollIds.Count == 0)
        {
            AddLog("出千需要至少选择一颗实体骰。");
            return;
        }

        int changedCount = 0;
        int cappedCount = 0;
        for (int i = 0; i < cheatRerollIds.Count; i++)
        {
            Die die = FindDieById(cheatRerollIds[i]);
            if (die == null || die.Temporary)
            {
                continue;
            }

            bool capped;
            if (ApplyCheatIncrement(die, out capped))
            {
                if (capped)
                {
                    cappedCount++;
                }
                else
                {
                    changedCount++;
                }
            }
        }

        int appliedCount = changedCount + cappedCount;
        if (appliedCount == 0)
        {
            cheatRerollIds.Clear();
            AddLog("没有可改点的实体骰。");
            return;
        }

        cheatsLeft--;
        cheatRerollIds.Clear();
        string capText = cappedCount > 0 ? "，其中 " + cappedCount + " 颗已到上限" : string.Empty;
        AddLog("出千改点 " + appliedCount + " 颗" + capText + "，剩余出千 " + cheatsLeft + "。");
        scoringDice.Clear();
        scoringDice.AddRange(dice);
        RefreshHandPreview();
        BeginSettle();
    }

    private void ToggleCheatReroll(Die die)
    {
        if (die == null || die.Temporary)
        {
            return;
        }

        int index = cheatRerollIds.IndexOf(die.Id);
        if (index >= 0)
        {
            cheatRerollIds.RemoveAt(index);
            return;
        }

        if (cheatRerollIds.Count >= MaxCheatRerollDice)
        {
            AddLog("出千最多选择 " + MaxCheatRerollDice + " 颗骰子。");
            return;
        }

        cheatRerollIds.Add(die.Id);
    }

    private bool IsCheatRerollSelected(int dieId)
    {
        return cheatRerollIds.IndexOf(dieId) >= 0;
    }

    private Die FindDieById(int dieId)
    {
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i].Id == dieId)
            {
                return dice[i];
            }
        }

        return null;
    }

    private void ToggleReadySlotSelection(int index)
    {
        if (rollPhase != RollPhase.Ready || index < 0 || index >= dice.Count)
        {
            return;
        }

        if (selectedReadySlotIndex == index)
        {
            selectedReadySlotIndex = -1;
            return;
        }

        if (selectedReadySlotIndex < 0 || selectedReadySlotIndex >= dice.Count)
        {
            selectedReadySlotIndex = index;
            return;
        }

        int from = selectedReadySlotIndex;
        SwapDiceSlots(from, index);
        selectedReadySlotIndex = -1;
        AddLog("调整槽位：" + (from + 1) + " 与 " + (index + 1) + " 交换。");
        SaveDiceOrderIfStageStart();
    }

    private void SwapDiceSlots(int firstIndex, int secondIndex)
    {
        if (firstIndex == secondIndex
            || firstIndex < 0
            || secondIndex < 0
            || firstIndex >= dice.Count
            || secondIndex >= dice.Count)
        {
            return;
        }

        Die first = dice[firstIndex];
        dice[firstIndex] = dice[secondIndex];
        dice[secondIndex] = first;
    }

    private void SaveDiceOrderIfStageStart()
    {
        if (suppressSave || !hasSave || rolledThisEncounter || currentScore != 0)
        {
            return;
        }

        PlayerPrefs.SetString(SavePrefix + "DiceData", SerializeDice());
        PlayerPrefs.Save();
    }

    private void RemoveInvalidCheatSelections()
    {
        for (int i = cheatRerollIds.Count - 1; i >= 0; i--)
        {
            Die die = FindDieById(cheatRerollIds[i]);
            if (die == null
                || die.Temporary
                || die.EffectiveValue <= 0)
            {
                cheatRerollIds.RemoveAt(i);
            }
        }
    }

    private void BeginSettle()
    {
        if (rollPhase != RollPhase.ResultDecision && rollPhase != RollPhase.CheatEdit)
        {
            return;
        }

        if (dice.Count == 0)
        {
            rollPhase = RollPhase.Ready;
            return;
        }

        scoringDice.Clear();
        scoringDice.AddRange(dice);
        RefreshHandPreview();
        pendingTreeGrowth.Clear();
        int scoreBeforeSettle = currentScore;
        int rollScore = ScoreDice();
        resolvedScore = scoreBeforeSettle + rollScore;
        CaptureCommittedRunScoreCounter(scoreBeforeSettle, rollScore, resolvedScore);
        PrepareRunScoreCounterAnimation(scoreBeforeSettle);
        PrepareSettlementDisplayEvents(scoreBeforeSettle);
        currentScore = scoreBeforeSettle;
        scoreRevealIndex = 0;
        scoreStepTimer = 0f;
        finalScoreApplied = false;
        ActivateSettlementDisplayEvent(0);
        rollPhase = RollPhase.Scoring;
        AddLog("二次确认，开始从左到右结算。");
    }

    private void UpdateScoreReveal()
    {
        scoreStepTimer += Time.deltaTime;
        UpdateRunScoreCounterPulses(Time.deltaTime);

        if (UpdateSettlementDisplayEvents())
        {
            return;
        }

        UpdateLegacyScoreReveal();
    }

    private bool UpdateSettlementDisplayEvents()
    {
        if (settlementDisplayEvents.Count <= 0)
        {
            return false;
        }

        if (activeSettlementEvent == null)
        {
            ActivateSettlementDisplayEvent(settlementEventIndex);
        }

        if (activeSettlementEvent == null)
        {
            CompleteScoreReveal();
            return true;
        }

        float duration = Mathf.Max(0.01f, activeSettlementEvent.Duration);
        if (scoreStepTimer < duration)
        {
            return true;
        }

        ApplySettlementDisplayEvent(activeSettlementEvent);
        settlementEventIndex++;
        scoreStepTimer = 0f;

        if (settlementEventIndex < settlementDisplayEvents.Count)
        {
            ActivateSettlementDisplayEvent(settlementEventIndex);
        }
        else
        {
            CompleteScoreReveal();
        }

        return true;
    }

    private void UpdateLegacyScoreReveal()
    {
        if (scoreRevealIndex < scoringDice.Count)
        {
            if (scoreStepTimer >= ScoreStepDuration)
            {
                ApplyRunScoreCounterStep(scoreRevealIndex);
                scoreRevealIndex++;
                scoreStepTimer = 0f;
            }

            return;
        }

        if (!finalScoreApplied)
        {
            if (scoreStepTimer >= FinalScoreDuration)
            {
                ApplyRunScoreCounterFinal();
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

    private void ActivateSettlementDisplayEvent(int eventIndex)
    {
        if (eventIndex < 0 || eventIndex >= settlementDisplayEvents.Count)
        {
            activeSettlementEvent = null;
            return;
        }

        activeSettlementEvent = settlementDisplayEvents[eventIndex];
        if (activeSettlementEvent.ScoreIndex >= 0 && activeSettlementEvent.ScoreIndex < scoringDice.Count)
        {
            scoreRevealIndex = activeSettlementEvent.ScoreIndex;
        }
        else if (activeSettlementEvent.Kind == SettlementEventKind.BribeFinal || activeSettlementEvent.Kind == SettlementEventKind.TargetSettle)
        {
            scoreRevealIndex = scoringDice.Count;
        }
    }

    private void ApplySettlementDisplayEvent(SettlementDisplayEvent settlementEvent)
    {
        if (settlementEvent == null)
        {
            return;
        }

        if (settlementEvent.ApplyCounterStep)
        {
            ApplyRunScoreCounterStep(settlementEvent.CounterStepIndex);
            if (settlementEvent.ScoreIndexEnd >= 0)
            {
                scoreRevealIndex = Mathf.Min(scoringDice.Count, settlementEvent.ScoreIndexEnd + 1);
            }
        }

        if (settlementEvent.ApplyFinal && !finalScoreApplied)
        {
            ApplyRunScoreCounterFinal();
            finalScoreApplied = true;
            scoreRevealIndex = scoringDice.Count;
        }
    }

    private void CompleteScoreReveal()
    {
        currentScore = resolvedScore;
        runScoreCounterAnimationActive = false;
        ClearSettlementDisplayEvents();
        ApplyPendingTreeGrowth();
        Encounter encounter = CurrentEncounter();
        passed = encounter != null && currentScore >= encounter.Target;

        if (passed)
        {
            ResolvePassIncome();
            rollPhase = RollPhase.StageClear;
            AddLog("达标：" + currentScore + " / " + encounter.Target + "。");
        }
        else if (rollsLeft > 0)
        {
            PrepareNextRollVisualReadyState();
            rollPhase = RollPhase.Ready;
            AddLog("未达标：" + currentScore + " / " + encounter.Target + "，还可继续出手。");
        }
        else
        {
            rollPhase = RollPhase.StageFailed;
            AddLog("未达标：" + currentScore + " / " + encounter.Target + "，本轮结束。");
            ClearSave();
            mode = GameMode.GameOver;
        }
    }

    private void PrepareNextRollVisualReadyState()
    {
        for (int i = 0; i < dice.Count; i++)
        {
            ResetRoundState(dice[i]);
        }

        scoringDice.Clear();
        selectedReadySlotIndex = -1;
        BeginDiceVisualEnter();
    }

    private string RollPromptText()
    {
        Encounter encounter = CurrentEncounter();

        if (dice.Count == 0)
        {
            return "骰袋为空";
        }

        if (encounter != null && rollsLeft <= 0)
        {
            return rollPhase == RollPhase.StageFailed ? "本轮失败" : "出手用尽";
        }

        return string.Empty;
    }

    private void RollOneDie(Die die)
    {
        RollOneDieFromFaces(die, null);
    }

    private bool ApplyCheatIncrement(Die die, out bool capped)
    {
        capped = false;
        if (die == null || die.Temporary || die.EffectiveValue <= 0)
        {
            return false;
        }

        int previous = die.EffectiveValue;
        int faceLimit = HighestFaceValue(die);
        if (faceLimit <= 0)
        {
            return false;
        }

        int next = Mathf.Min(previous + 1, faceLimit);
        capped = next == previous;
        die.LoneWitnessRerolled = false;
        die.LoneWitnessPreviousValue = 0;
        die.HalfStepBorrowed = false;
        die.ParityCompleteUsed = false;
        die.ParityReviewRerolled = false;
        die.ParityReviewPreviousValue = 0;
        die.LastValue = next;
        die.EffectiveValue = next;
        die.RoundNote = RoundTag(die);
        die.CheatRerolledThisSettle = true;
        die.CheatPreviousValue = previous;
        die.RoundNote = AppendNote(die.RoundNote, capped ? "出千封顶" : "出千 " + previous + "→" + next);
        if (die.Type == DieType.ParityTurner)
        {
            MarkTypeTriggered(die);
            die.RoundNote = AppendNote(die.RoundNote, capped ? "转号封顶" : "转号");
        }

        return true;
    }

    private int HighestFaceValue(Die die)
    {
        if (die == null || die.Faces == null || die.Faces.Length == 0)
        {
            return 0;
        }

        int highest = Mathf.Max(1, die.Faces[0]);
        for (int i = 1; i < die.Faces.Length; i++)
        {
            int face = Mathf.Max(1, die.Faces[i]);
            if (face > highest)
            {
                highest = face;
            }
        }

        return highest;
    }

    private void RollOneDieFromFaces(Die die, List<int> candidates)
    {
        if (die == null || die.Faces == null || die.Faces.Length == 0)
        {
            return;
        }

        die.LoneWitnessRerolled = false;
        die.LoneWitnessPreviousValue = 0;
        die.HalfStepBorrowed = false;
        die.CheatRerolledThisSettle = false;
        die.CheatPreviousValue = 0;
        die.ParityCompleteUsed = false;
        die.ParityReviewRerolled = false;
        die.ParityReviewPreviousValue = 0;
        int raw = candidates != null && candidates.Count > 0
            ? candidates[UnityEngine.Random.Range(0, candidates.Count)]
            : die.Faces[UnityEngine.Random.Range(0, die.Faces.Length)];
        die.LastValue = raw;
        die.EffectiveValue = raw;
        die.RoundNote = RoundTag(die);
    }

    private void ApplyLoneWitnessRerolls()
    {
        Dictionary<int, int> initialCounts = new Dictionary<int, int>();
        for (int i = 0; i < dice.Count; i++)
        {
            int value = dice[i].EffectiveValue;
            if (value <= 0)
            {
                continue;
            }

            if (!initialCounts.ContainsKey(value))
            {
                initialCounts[value] = 0;
            }

            initialCounts[value]++;
        }

        int rerollCount = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            Die die = dice[i];
            if (die.Type != DieType.LoneWitness || die.EffectiveValue <= 0)
            {
                continue;
            }

            int count;
            if (!initialCounts.TryGetValue(die.EffectiveValue, out count) || count > 1)
            {
                continue;
            }

            int previous = die.EffectiveValue;
            RollOneDie(die);
            die.LoneWitnessRerolled = true;
            die.LoneWitnessPreviousValue = previous;
            MarkTypeTriggered(die);
            die.RoundNote = "孤证 " + previous + "→" + die.EffectiveValue;
            rerollCount++;
        }

        if (rerollCount > 0)
        {
            AddLog("孤证重摇 " + rerollCount + " 颗，必须接受新结果。");
        }
    }

    private void ApplyParityReviewRerolls()
    {
        int rerollCount = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            Die die = dice[i];
            if (die.Type != DieType.ParityReview || die.EffectiveValue <= 0 || !IsOnlyParityMismatch(die))
            {
                continue;
            }

            int previous = die.EffectiveValue;
            RollOneDie(die);
            die.ParityReviewRerolled = true;
            die.ParityReviewPreviousValue = previous;
            MarkTypeTriggered(die);
            die.RoundNote = "复核 " + previous + "→" + die.EffectiveValue;
            rerollCount++;
        }

        if (rerollCount > 0)
        {
            AddLog("复核重摇 " + rerollCount + " 颗，必须接受新结果。");
        }
    }

    private void ClearResultPreview()
    {
        previewRollScore = 0;
        previewIndividualScore = 0;
        previewTemporaryScore = 0;
        previewTurtleTemporaryDieCount = 0;
        previewNestBonusDieCount = 0;
        previewShellsmithScoreBonus = 0;
        previewRuleBonus = 0;
        previewBribeGoldCost = 0;
        previewBribeScoreBonus = 0;
        previewAffixScoreBonus = 0;
        previewWalletIncome = 0;
        previewHasTurtleRandomness = false;
        lastTurtleTemporaryDieCount = 0;
        lastNestBonusDieCount = 0;
        lastShellsmithScoreBonus = 0;
        lastAffixScoreBonus = 0;
        lastWalletIncome = 0;
        lastMultiplier = 1f;
        lastHandName = "无牌型";
    }

    private void RefreshHandPreview()
    {
        Encounter encounter = CurrentEncounter();
        if (encounter == null || dice.Count == 0)
        {
            ClearResultPreview();
            return;
        }

        List<int> handValues = new List<int>();
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i].EffectiveValue <= 0)
            {
                ClearResultPreview();
                return;
            }

            handValues.Add(dice[i].EffectiveValue);
        }

        HandResult hand = EvaluateHand(handValues, dice);
        bool straightTriggered = hand.LongestRun >= 5;
        float parityMultiplier = ParityMultiplier(hand, encounter);
        lastHandName = FinalHandName(hand, parityMultiplier);
        lastMultiplier = Mathf.Max(hand.Multiplier, parityMultiplier);
        previewTemporaryScore = PreviewTemporaryTurtleDiceScore(out previewHasTurtleRandomness, out previewTurtleTemporaryDieCount, out previewNestBonusDieCount);
        previewShellsmithScoreBonus = CountDiceOfType(DieType.Shellsmith) * previewTurtleTemporaryDieCount;
        previewIndividualScore = 0;
        previewAffixScoreBonus = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            previewIndividualScore += ScoreOneDieValue(dice[i], encounter, false, previewTurtleTemporaryDieCount, straightTriggered);
            previewAffixScoreBonus += AffixScoreBonusForDie(dice[i], straightTriggered, false);
        }

        List<int> previewDieScores = PreviewDieScores(encounter, previewTurtleTemporaryDieCount, straightTriggered);
        List<int> walletScoreSnapshot = new List<int>(previewDieScores);
        previewWalletIncome = WalletIncomeForCurrentRoll(hand, previewTurtleTemporaryDieCount, walletScoreSnapshot, false, ref previewIndividualScore);
        previewRuleBonus = RuleComboBonus(hand, encounter);
        lastComboBonus = previewRuleBonus;
        lastTemporaryScore = previewTemporaryScore;
        int baseRollScore = Mathf.RoundToInt((previewIndividualScore + previewTemporaryScore + previewRuleBonus) * lastMultiplier);
        UpdateBribePreview(baseRollScore, hand, previewTurtleTemporaryDieCount, previewDieScores);
        previewRollScore = baseRollScore + previewBribeScoreBonus;
    }

    private List<int> PreviewDieScores(Encounter encounter, int turtleTemporaryDieCount, bool straightTriggered)
    {
        List<int> result = new List<int>();
        for (int i = 0; i < dice.Count; i++)
        {
            result.Add(ScoreOneDieValue(dice[i], encounter, false, turtleTemporaryDieCount, straightTriggered));
        }

        return result;
    }

    private void UpdateBribePreview(int baseRollScore, HandResult hand, int turtleTemporaryDieCount, List<int> dieScores)
    {
        previewBribeGoldCost = 0;
        previewBribeScoreBonus = 0;

        int goldCost;
        int scoreBonus;
        int maxGold = BribeGoldCapacityInCurrentOrder(hand, chapterGold, turtleTemporaryDieCount, dieScores);
        if (TryCalculateBribe(baseRollScore, maxGold, out goldCost, out scoreBonus))
        {
            previewBribeGoldCost = goldCost;
            previewBribeScoreBonus = scoreBonus;
        }
    }

    private int ApplyBribeIfNeeded(int baseRollScore, int maxGold)
    {
        lastBribeGoldSpent = 0;
        lastBribeScoreBonus = 0;

        int goldCost;
        int scoreBonus;
        if (!TryCalculateBribe(baseRollScore, maxGold, out goldCost, out scoreBonus))
        {
            return 0;
        }

        chapterGold = Mathf.Max(0, chapterGold - goldCost);
        lastBribeGoldSpent = goldCost;
        lastBribeScoreBonus = scoreBonus;
        previewBribeGoldCost = goldCost;
        previewBribeScoreBonus = scoreBonus;
        MarkBribeDiceTriggered();
        AddLog("贿赂花 " + goldCost + " 金，把账补平 +" + scoreBonus + " 分。");
        return scoreBonus;
    }

    private void MarkBribeDiceTriggered()
    {
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i] != null && dice[i].Type == DieType.Bribe)
            {
                MarkTypeTriggered(dice[i]);
            }
        }
    }

    private bool TryCalculateBribe(int baseRollScore, int maxGold, out int goldCost, out int scoreBonus)
    {
        goldCost = 0;
        scoreBonus = 0;

        Encounter encounter = CurrentEncounter();
        if (encounter == null || bribeScorePerGold <= 0 || bribeGoldCapPerDie <= 0)
        {
            return false;
        }

        int bribeDice = CountDiceOfType(DieType.Bribe);
        if (bribeDice <= 0)
        {
            return false;
        }

        int gap = encounter.Target - (currentScore + baseRollScore);
        if (gap <= 0)
        {
            return false;
        }

        int neededGold = Mathf.CeilToInt(gap / (float)bribeScorePerGold);
        maxGold = Mathf.Min(Mathf.Max(0, maxGold), bribeDice * bribeGoldCapPerDie);
        if (neededGold <= 0 || neededGold > maxGold)
        {
            return false;
        }

        goldCost = neededGold;
        scoreBonus = neededGold * bribeScorePerGold;
        return scoreBonus >= gap;
    }

    private int TreasuryScoreBonus()
    {
        if (treasuryGoldStep <= 0)
        {
            return 0;
        }

        int bonus = Mathf.Max(0, stageStartGold) / treasuryGoldStep;
        if (treasuryScoreCap > 0)
        {
            bonus = Mathf.Min(bonus, treasuryScoreCap);
        }

        return Mathf.Max(0, bonus);
    }

    private int InvestmentScoreBonus(Die die)
    {
        if (die == null || die.Type != DieType.Investment)
        {
            return 0;
        }

        return Mathf.Max(0, die.InvestmentGold) * Mathf.Max(0, investmentScorePerGold);
    }

    private int TotalTreasuryScoreBonus()
    {
        return CountDiceOfType(DieType.Treasury) * TreasuryScoreBonus();
    }

    private int TotalInvestmentScoreBonus()
    {
        int total = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            total += InvestmentScoreBonus(dice[i]);
        }

        return total;
    }

    private int TriggeredStampDiceCount()
    {
        int count = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i].Type == DieType.Stamp && HasSameEffectiveValuePartner(dice[i]))
            {
                count++;
            }
        }

        return count;
    }

    private int TriggeredTrackDiceCount()
    {
        if (!HasTrackValuesComplete())
        {
            return 0;
        }

        return CountDiceOfType(DieType.Track);
    }

    private bool HasSameEffectiveValuePartner(Die die)
    {
        if (die == null || die.Temporary || die.EffectiveValue <= 0)
        {
            return false;
        }

        for (int i = 0; i < dice.Count; i++)
        {
            Die other = dice[i];
            if (other == null || other.Temporary || other.Id == die.Id)
            {
                continue;
            }

            if (other.EffectiveValue == die.EffectiveValue)
            {
                return true;
            }
        }

        return false;
    }

    private bool HasTrackValuesComplete()
    {
        bool hasTwo = false;
        bool hasFour = false;
        bool hasSix = false;

        for (int i = 0; i < dice.Count; i++)
        {
            Die die = dice[i];
            if (die == null || die.Temporary)
            {
                continue;
            }

            if (die.EffectiveValue == 2)
            {
                hasTwo = true;
            }
            else if (die.EffectiveValue == 4)
            {
                hasFour = true;
            }
            else if (die.EffectiveValue == 6)
            {
                hasSix = true;
            }
        }

        return hasTwo && hasFour && hasSix;
    }

    private Die LeftNeighborDie(Die die)
    {
        if (die == null || die.Temporary)
        {
            return null;
        }

        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i] != null && dice[i].Id == die.Id)
            {
                return i > 0 ? dice[i - 1] : null;
            }
        }

        return null;
    }

    private bool HasLeftParityRelation(Die die, bool sameParity)
    {
        Die left = LeftNeighborDie(die);
        if (left == null || left.Temporary || die.EffectiveValue <= 0 || left.EffectiveValue <= 0)
        {
            return false;
        }

        bool same = die.EffectiveValue % 2 == left.EffectiveValue % 2;
        return sameParity ? same : !same;
    }

    private bool IsOnlyParityMismatch(Die die)
    {
        if (die == null || die.Temporary || die.EffectiveValue <= 0)
        {
            return false;
        }

        int otherParity = -1;
        for (int i = 0; i < dice.Count; i++)
        {
            Die other = dice[i];
            if (other == null || other.Temporary || other.Id == die.Id || other.EffectiveValue <= 0)
            {
                continue;
            }

            int parity = other.EffectiveValue % 2;
            if (otherParity < 0)
            {
                otherParity = parity;
            }
            else if (otherParity != parity)
            {
                return false;
            }
        }

        return otherParity >= 0 && die.EffectiveValue % 2 != otherParity;
    }

    private bool CheatParityChanged(Die die)
    {
        return die != null
            && die.CheatRerolledThisSettle
            && die.CheatPreviousValue > 0
            && die.EffectiveValue > 0
            && die.CheatPreviousValue % 2 != die.EffectiveValue % 2;
    }

    private bool CheatParityHeld(Die die)
    {
        return die != null
            && die.CheatRerolledThisSettle
            && die.CheatPreviousValue > 0
            && die.EffectiveValue > 0
            && die.CheatPreviousValue % 2 == die.EffectiveValue % 2;
    }

    private bool IsNaturalTreeHit(Die die)
    {
        return die != null
            && !die.Temporary
            && die.Type == DieType.Tree
            && die.TargetFace > 0
            && die.EffectiveValue == die.TargetFace;
    }

    private int NaturalTreeHitCount()
    {
        int count = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            if (IsNaturalTreeHit(dice[i]))
            {
                count++;
            }
        }

        return count;
    }

    private Die FindIrrigationPreviewTarget(Die irrigation)
    {
        if (irrigation == null || irrigation.Temporary || irrigation.Type != DieType.Irrigation || irrigation.EffectiveValue <= 0)
        {
            return null;
        }

        Die best = null;
        for (int i = 0; i < dice.Count; i++)
        {
            Die tree = dice[i];
            if (tree == null || tree.Temporary || tree.Type != DieType.Tree || tree.TargetFace <= 0 || IsNaturalTreeHit(tree))
            {
                continue;
            }

            if (tree.TargetFace != irrigation.EffectiveValue)
            {
                continue;
            }

            if (best == null || tree.Id < best.Id)
            {
                best = tree;
            }
        }

        return best;
    }

    private Die FindIrrigationTarget(Die irrigation, HashSet<int> naturallyHitTreeIds, HashSet<int> wateredTreeIds)
    {
        if (irrigation == null || irrigation.Temporary || irrigation.Type != DieType.Irrigation || irrigation.EffectiveValue <= 0)
        {
            return null;
        }

        Die best = null;
        for (int i = 0; i < dice.Count; i++)
        {
            Die tree = dice[i];
            if (tree == null || tree.Temporary || tree.Type != DieType.Tree || tree.TargetFace <= 0)
            {
                continue;
            }

            if (naturallyHitTreeIds.Contains(tree.Id) || wateredTreeIds.Contains(tree.Id))
            {
                continue;
            }

            if (tree.TargetFace != irrigation.EffectiveValue)
            {
                continue;
            }

            if (best == null || tree.Id < best.Id)
            {
                best = tree;
            }
        }

        return best;
    }

    private void QueueTreeGrowths(HandResult hand, int rollScore)
    {
        int gardenerCount = CountDiceOfType(DieType.Gardener);
        int naturalHitCount = 0;
        HashSet<int> naturallyHitTreeIds = new HashSet<int>();

        for (int i = 0; i < dice.Count; i++)
        {
            Die tree = dice[i];
            if (!IsNaturalTreeHit(tree))
            {
                continue;
            }

            naturalHitCount++;
            naturallyHitTreeIds.Add(tree.Id);
            AddPendingTreeGrowth(tree, 1 + gardenerCount);
            MarkTypeTriggered(tree);
            if (gardenerCount > 0)
            {
                tree.RoundNote = AppendNote(tree.RoundNote, "园丁 +" + gardenerCount);
            }
        }

        if (gardenerCount > 0 && naturalHitCount > 0)
        {
            for (int i = 0; i < dice.Count; i++)
            {
                Die gardener = dice[i];
                if (gardener.Type == DieType.Gardener)
                {
                    MarkTypeTriggered(gardener);
                    gardener.RoundNote = AppendNote(gardener.RoundNote, "园丁 +" + naturalHitCount + " 成长");
                }
            }

            AddLog("园丁让 " + naturalHitCount + " 棵自然命中的大树各额外成长 " + gardenerCount + " 次。");
        }

        HashSet<int> wateredTreeIds = new HashSet<int>();
        int irrigationHitCount = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            Die irrigation = dice[i];
            if (irrigation.Type != DieType.Irrigation)
            {
                continue;
            }

            Die target = FindIrrigationTarget(irrigation, naturallyHitTreeIds, wateredTreeIds);
            if (target == null)
            {
                continue;
            }

            wateredTreeIds.Add(target.Id);
            irrigationHitCount++;
            AddPendingTreeGrowth(target, 1);
            MarkTypeTriggered(target);
            MarkTypeTriggered(irrigation);
            target.RoundNote = AppendNote(target.RoundNote, "灌溉成长");
            irrigation.RoundNote = AppendNote(irrigation.RoundNote, "灌溉 " + target.TargetFace);
        }

        if (irrigationHitCount > 0)
        {
            AddLog("灌溉命中 " + irrigationHitCount + " 棵未自然命中的大树。");
        }

        int interfaceGrowthCount = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            Die die = dice[i];
            if (die == null || die.Temporary)
            {
                continue;
            }

            switch (die.Type)
            {
                case DieType.PointSeedTree:
                    if (die.TargetFace > 0 && die.EffectiveValue == die.TargetFace)
                    {
                        QueueDirectTreeGrowth(die, 1, "籽中成长", true);
                        interfaceGrowthCount++;
                    }
                    break;
                case DieType.PatternTree:
                    if (PatternTreeTargetHit(die.PatternTarget, hand))
                    {
                        QueueDirectTreeGrowth(die, 1, "谱成成长", true);
                        interfaceGrowthCount++;
                    }
                    break;
                case DieType.CanopyTree:
                    if (RolledHighestFace(die))
                    {
                        QueueDirectTreeGrowth(die, 1, "冠顶成长", true);
                        interfaceGrowthCount++;
                    }
                    break;
                case DieType.PruningTree:
                    if (PruningTreeTriggered(die, hand, rollScore))
                    {
                        QueueDirectTreeGrowth(die, 1, "修枝成长", true);
                        interfaceGrowthCount++;
                    }
                    break;
                case DieType.RingTree:
                    QueueDirectTreeGrowth(die, 1, "年轮成长", true);
                    interfaceGrowthCount++;
                    break;
                case DieType.FertilizerTree:
                    int fertilizerGrowth = FertilizerGrowthCount();
                    if (fertilizerGrowth > 0)
                    {
                        QueueDirectTreeGrowth(die, fertilizerGrowth, "肥料成长 x" + fertilizerGrowth, true);
                        interfaceGrowthCount += fertilizerGrowth;
                    }
                    break;
            }
        }

        int rootGrowthCount = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            Die root = dice[i];
            string triggerLabel;
            if (root == null
                || root.Temporary
                || root.Type != DieType.RootTree
                || !RootTreeTriggerLabel(root, out triggerLabel))
            {
                continue;
            }

            QueueDirectTreeGrowth(root, 1, triggerLabel, false);
            rootGrowthCount++;
        }

        if (interfaceGrowthCount > 0)
        {
            AddLog("新大树接口触发成长 " + interfaceGrowthCount + " 次。");
        }

        if (rootGrowthCount > 0)
        {
            AddLog("根系读取相邻触发，成长 " + rootGrowthCount + " 次。");
        }
    }

    private void QueueDirectTreeGrowth(Die die, int count, string note, bool markTypeTrigger)
    {
        if (die == null || count <= 0)
        {
            return;
        }

        AddPendingTreeGrowth(die, count);
        if (!string.IsNullOrEmpty(note))
        {
            die.RoundNote = AppendNote(die.RoundNote, note);
        }

        if (markTypeTrigger)
        {
            MarkTypeTriggered(die);
        }
    }

    private int FertilizerGrowthCount()
    {
        if (interestGoldStep <= 0)
        {
            return 0;
        }

        return Mathf.Max(0, chapterGold) / interestGoldStep;
    }

    private bool PatternTreeTargetHit(TreePatternTarget target, HandResult hand)
    {
        if (hand == null)
        {
            return false;
        }

        switch (target)
        {
            case TreePatternTarget.ThreeKind:
                return string.Equals(hand.Name, "三同", StringComparison.Ordinal)
                    || string.Equals(hand.Name, "四同", StringComparison.Ordinal)
                    || string.Equals(hand.Name, "五同", StringComparison.Ordinal)
                    || string.Equals(hand.Name, "六同", StringComparison.Ordinal);
            case TreePatternTarget.Straight:
                return hand.LongestRun >= 5;
            case TreePatternTarget.AllOdd:
                return hand.AllOdd;
            case TreePatternTarget.AllEven:
                return hand.AllEven;
        }

        return false;
    }

    private bool PruningTreeTriggered(Die die, HandResult currentHand, int rollScore)
    {
        if (die == null || die.Temporary || die.Type != DieType.PruningTree || !die.CheatRerolledThisSettle || die.CheatPreviousValue <= 0)
        {
            return false;
        }

        if (die.EffectiveValue > die.CheatPreviousValue)
        {
            return true;
        }

        List<int> previousValues = CurrentHandValuesWithPreviousValue(die);
        HandResult previousHand = EvaluateHand(previousValues, null);
        float currentMultiplier = FinalMultiplierValue(currentHand);
        float previousMultiplier = FinalMultiplierValue(previousHand);
        if (currentMultiplier > previousMultiplier)
        {
            return true;
        }

        Encounter encounter = CurrentEncounter();
        if (encounter == null)
        {
            return false;
        }

        int previousRollScore = EstimateSimpleRollScore(previousValues, previousHand, encounter);
        return currentScore + rollScore >= encounter.Target && currentScore + previousRollScore < encounter.Target;
    }

    private List<int> CurrentHandValuesWithPreviousValue(Die changedDie)
    {
        List<int> values = new List<int>();
        for (int i = 0; i < dice.Count; i++)
        {
            Die die = dice[i];
            if (die != null && changedDie != null && die.Id == changedDie.Id && changedDie.CheatPreviousValue > 0)
            {
                values.Add(changedDie.CheatPreviousValue);
            }
            else
            {
                values.Add(die != null ? die.EffectiveValue : 0);
            }
        }

        return values;
    }

    private HandResult CurrentDiceHandResult()
    {
        List<int> values = new List<int>();
        for (int i = 0; i < dice.Count; i++)
        {
            values.Add(dice[i] != null ? dice[i].EffectiveValue : 0);
        }

        return EvaluateHand(values, dice);
    }

    private float FinalMultiplierValue(HandResult hand)
    {
        if (hand == null)
        {
            return 1f;
        }

        return Mathf.Max(hand.Multiplier, ParityMultiplier(hand, CurrentEncounter()));
    }

    private int EstimateSimpleRollScore(List<int> values, HandResult hand, Encounter encounter)
    {
        if (values == null || hand == null || encounter == null)
        {
            return 0;
        }

        int individual = 0;
        for (int i = 0; i < values.Count; i++)
        {
            individual += Mathf.Max(1, values[i]) * BaseScorePerPip;
        }

        int ruleBonus = RuleComboBonus(hand, encounter);
        return Mathf.RoundToInt((individual + ruleBonus) * FinalMultiplierValue(hand));
    }

    private bool RootTreeTriggerLabel(Die root, out string label)
    {
        label = string.Empty;
        Die left = LeftNeighborDie(root);
        Die right = RightNeighborDie(root);
        bool leftTriggered = left != null && left.TypeTriggeredThisSettle;
        bool rightTriggered = right != null && right.TypeTriggeredThisSettle;

        if (leftTriggered && rightTriggered)
        {
            label = "根:邻触发";
            return true;
        }

        if (leftTriggered)
        {
            label = "根:左";
            return true;
        }

        if (rightTriggered)
        {
            label = "根:右";
            return true;
        }

        return false;
    }

    private Die RightNeighborDie(Die die)
    {
        if (die == null || die.Temporary)
        {
            return null;
        }

        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i] != null && dice[i].Id == die.Id)
            {
                return i + 1 < dice.Count ? dice[i + 1] : null;
            }
        }

        return null;
    }

    private void AddPendingTreeGrowth(Die die, int count)
    {
        for (int i = 0; i < count; i++)
        {
            pendingTreeGrowth.Add(die);
        }
    }

    private int ScoreDice()
    {
        Encounter encounter = CurrentEncounter();
        if (encounter == null)
        {
            return 0;
        }

        List<int> handValues = new List<int>();
        for (int i = 0; i < dice.Count; i++)
        {
            handValues.Add(dice[i].EffectiveValue);
        }

        HandResult hand = EvaluateHand(handValues, dice);
        bool straightTriggered = hand.LongestRun >= 5;

        int tempCount;
        int nestBonusCount;
        int tempScore = ScoreTemporaryTurtleDice(out tempCount, out nestBonusCount);
        lastTurtleTemporaryDieCount = tempCount;
        lastNestBonusDieCount = nestBonusCount;
        lastShellsmithScoreBonus = CountDiceOfType(DieType.Shellsmith) * tempCount;

        int individual = 0;
        lastAffixScoreBonus = 0;
        lastWalletIncome = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            Die die = dice[i];
            die.Score = ScoreOneDieValue(die, encounter, true, tempCount, straightTriggered);
            individual += die.Score;
            lastAffixScoreBonus += AffixScoreBonusForDie(die, straightTriggered, false);
        }

        int bribeGoldCapacity = BribeGoldCapacityInCurrentOrder(hand, chapterGold, tempCount);
        lastWalletIncome = WalletIncomeForCurrentRoll(hand, tempCount, null, true, ref individual);
        if (lastWalletIncome > 0)
        {
            chapterGold += lastWalletIncome;
        }

        int ruleBonus = RuleComboBonus(hand, encounter);
        float parityMultiplier = ParityMultiplier(hand, encounter);
        float multiplier = Mathf.Max(hand.Multiplier, parityMultiplier);
        int baseRollScore = Mathf.RoundToInt((individual + tempScore + ruleBonus) * multiplier);
        int bribeScore = ApplyBribeIfNeeded(baseRollScore, bribeGoldCapacity);
        int rollScore = baseRollScore + bribeScore;
        QueueTreeGrowths(hand, rollScore);
        previewIndividualScore = individual;
        previewTemporaryScore = tempScore;
        previewTurtleTemporaryDieCount = tempCount;
        previewNestBonusDieCount = nestBonusCount;
        previewShellsmithScoreBonus = lastShellsmithScoreBonus;
        previewAffixScoreBonus = lastAffixScoreBonus;
        previewWalletIncome = lastWalletIncome;
        previewRuleBonus = ruleBonus;
        previewRollScore = rollScore;
        previewHasTurtleRandomness = false;
        lastComboBonus = ruleBonus;
        lastTemporaryScore = tempScore;
        lastMultiplier = multiplier;
        lastHandName = FinalHandName(hand, parityMultiplier);

        string bribeText = bribeScore > 0 ? " + 贿赂 " + bribeScore : string.Empty;
        string shellsmithText = lastShellsmithScoreBonus > 0 ? "，壳匠 +" + lastShellsmithScoreBonus : string.Empty;
        AddLog("本次 " + rollScore + " = (" + individual + " + 小骰 " + tempScore + " + 规则 " + ruleBonus + ") x " + MultiplierText(multiplier) + bribeText + shellsmithText + "。");
        if (lastWalletIncome > 0)
        {
            AddLog("本次金币收入直接入钱包 +" + lastWalletIncome + "。");
        }
        return rollScore;
    }

    private int ScoreOneDieValue(Die die, Encounter encounter, bool updateNote, int turtleTemporaryDieCount = 0, bool straightTriggered = false)
    {
        int value = Mathf.Max(1, die.EffectiveValue);
        int baseScore = value * BaseScorePerPip;
        int score = baseScore;

        if (die.Type == DieType.Double)
        {
            score = baseScore * 2;
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, "双倍");
            }
        }
        else if (die.Type == DieType.Gambler)
        {
            if (value < die.GamblerThreshold)
            {
                score = 0;
                if (updateNote)
                {
                    die.RoundNote = AppendNote(die.RoundNote, "低于阈值");
                    MarkTypeTriggered(die);
                }
            }
            else if (value > die.GamblerThreshold)
            {
                float multiplier = GamblerMultiplier(die.GamblerThreshold);
                score = Mathf.RoundToInt(baseScore * multiplier);
                if (updateNote)
                {
                    die.RoundNote = AppendNote(die.RoundNote, "赌徒爆发");
                    MarkTypeTriggered(die);
                }
            }
            else
            {
                if (updateNote)
                {
                    die.RoundNote = AppendNote(die.RoundNote, "阈值正常");
                }
            }
        }
        else if (die.Type == DieType.Treasury)
        {
            int bonus = TreasuryScoreBonus();
            score += bonus;
            if (updateNote && bonus > 0)
            {
                die.RoundNote = AppendNote(die.RoundNote, "国库 +" + bonus);
                MarkTypeTriggered(die);
            }
        }
        else if (die.Type == DieType.Investment)
        {
            int bonus = InvestmentScoreBonus(die);
            score += bonus;
            if (updateNote && bonus > 0)
            {
                die.RoundNote = AppendNote(die.RoundNote, "投资 +" + bonus);
                MarkTypeTriggered(die);
            }
        }
        else if (die.Type == DieType.Shellsmith)
        {
            int bonus = Mathf.Max(0, turtleTemporaryDieCount);
            score += bonus;
            if (updateNote && bonus > 0)
            {
                die.RoundNote = AppendNote(die.RoundNote, "壳匠 +" + bonus);
                MarkTypeTriggered(die);
            }
        }
        else if (die.Type == DieType.ParityNeighborDiff && HasLeftParityRelation(die, false))
        {
            score = 6;
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, "异邻 6");
                MarkTypeTriggered(die);
            }
        }
        else if (die.Type == DieType.ParityNeighborSame && HasLeftParityRelation(die, true))
        {
            score = 6;
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, "同邻 6");
                MarkTypeTriggered(die);
            }
        }
        else if (die.Type == DieType.ParityFlipScore && CheatParityChanged(die))
        {
            score = 8;
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, "翻号 8");
                MarkTypeTriggered(die);
            }
        }
        else if (die.Type == DieType.ParityHoldScore && CheatParityHeld(die))
        {
            score = 6;
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, "守号 6");
                MarkTypeTriggered(die);
            }
        }
        else if (die.Type == DieType.Stamp && HasSameEffectiveValuePartner(die))
        {
            score = 6;
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, "盖章 6分");
                MarkTypeTriggered(die);
            }
        }
        else if (die.Type == DieType.Track && HasTrackValuesComplete())
        {
            score = 8;
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, "轨道 8分");
                MarkTypeTriggered(die);
            }
        }
        else if (die.Type == DieType.HalfStep && die.HalfStepBorrowed)
        {
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, "半步借位");
                MarkTypeTriggered(die);
            }
        }
        else if (die.Type == DieType.ParityComplete && die.ParityCompleteUsed)
        {
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, "补全");
                MarkTypeTriggered(die);
            }
        }
        else if (die.Type == DieType.ParityReview && die.ParityReviewRerolled)
        {
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, "复核");
                MarkTypeTriggered(die);
            }
        }
        else if (die.Type == DieType.Tree && die.EffectiveValue == die.TargetFace)
        {
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, "大树命中");
                MarkTypeTriggered(die);
            }
        }

        score = ApplyMaterialScore(die, score, updateNote);
        score = ApplyAffixScoreBonuses(die, score, straightTriggered, updateNote);

        if (encounter.Rule == RuleKind.OddLedger && value % 2 == 1)
        {
            score += 1;
        }
        else if (encounter.Rule == RuleKind.LowFog && value <= 2)
        {
            score = Mathf.Max(0, score - 1);
        }
        else if (encounter.Rule == RuleKind.DoubleJudge)
        {
            score = value % 2 == 0 ? score + 1 : Mathf.Max(0, score - 1);
        }

        return score;
    }

    private int ApplyMaterialScore(Die die, int score, bool updateNote)
    {
        if (die == null || die.Temporary)
        {
            return score;
        }

        int bonus = 0;
        string note = string.Empty;
        switch (die.Material)
        {
            case DiceMaterial.OfficialIron:
                bonus = 1;
                note = "官铁 +1";
                break;
            case DiceMaterial.ClearGlaze:
                if (RolledHighestFace(die))
                {
                    bonus = 2;
                    note = "明釉 +2";
                }
                break;
            case DiceMaterial.LeadSeal:
                if (die.CheatRerolledThisSettle)
                {
                    bonus = 2;
                    note = "铅封 +2";
                }
                break;
            case DiceMaterial.CopperBone:
                if (HasSameEffectiveValuePartner(die))
                {
                    bonus = 1;
                    note = "铜骨 +1";
                }
                break;
        }

        if (bonus <= 0)
        {
            return score;
        }

        if (updateNote)
        {
            die.RoundNote = AppendNote(die.RoundNote, note);
        }

        return score + bonus;
    }

    private int ApplyAffixScoreBonuses(Die die, int score, bool straightTriggered, bool updateNote)
    {
        int bonus = AffixScoreBonusForDie(die, straightTriggered, updateNote);
        return score + bonus;
    }

    private int AffixScoreBonusForDie(Die die, bool straightTriggered, bool updateNote)
    {
        if (!affixFeatureEnabled || die == null || die.Temporary || die.PrefixAffixes == null)
        {
            return 0;
        }

        int bonus = 0;
        for (int i = 0; i < die.PrefixAffixes.Count; i++)
        {
            AffixInstance affix = die.PrefixAffixes[i];
            AffixDefinition definition;
            if (affix == null || !affixDefinitions.TryGetValue(affix.Key, out definition) || definition.Slot != AffixSlot.Prefix)
            {
                continue;
            }

            if (!IsAffixTriggerMet(die, definition.TriggerKey, straightTriggered))
            {
                continue;
            }

            int value = AffixValue(affix);
            if (value <= 0)
            {
                continue;
            }

            bonus += value;
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, definition.DisplayName + " +" + value);
            }
        }

        return bonus;
    }

    private int WalletIncomeForCurrentRoll(HandResult hand, int turtleTemporaryDieCount, List<int> dieScores, bool updateNote, ref int individualScore)
    {
        int totalIncome = 0;
        int[] slotIncome = new int[dice.Count];
        HashSet<int> lumberedTargetIds = new HashSet<int>();

        for (int i = 0; i < dice.Count; i++)
        {
            Die die = dice[i];
            int leftIncome = i > 0 ? slotIncome[i - 1] : 0;
            int income = WalletIncomeForDie(die, hand, turtleTemporaryDieCount, leftIncome, updateNote);
            income += LumberGoldIncomeForDie(die, i, dieScores, lumberedTargetIds, updateNote, ref individualScore);
            slotIncome[i] = income;
            totalIncome += income;
        }

        return totalIncome;
    }

    private int WalletIncomeForDie(Die die, HandResult hand, int turtleTemporaryDieCount, int leftDieWalletIncome, bool updateNote)
    {
        if (die == null || die.Temporary)
        {
            return 0;
        }

        bool straightTriggered = hand != null && hand.LongestRun >= 5;
        int income = 0;
        if (die.Type == DieType.Piggy && die.EffectiveValue == die.TargetFace)
        {
            int piggyGold = Mathf.Max(0, piggyGoldPerHit);
            if (piggyGold > 0)
            {
                income += piggyGold;
                if (updateNote)
                {
                    die.RoundNote = AppendNote(die.RoundNote, "猪猪 +" + piggyGold + " 金");
                    MarkTypeTriggered(die);
                }
            }
        }

        if (die.Type == DieType.BountyGold && die.TargetFace > 0 && die.EffectiveValue == die.TargetFace)
        {
            int gold = Mathf.Max(0, bountyGoldPerHit);
            if (gold > 0)
            {
                income += gold;
                if (updateNote)
                {
                    die.RoundNote = AppendNote(die.RoundNote, "悬赏 +" + gold + " 金");
                    MarkTypeTriggered(die);
                }
            }
        }

        if (die.Type == DieType.TopGold && RolledHighestFace(die))
        {
            int gold = Mathf.Max(0, topGoldPerHit);
            if (gold > 0)
            {
                income += gold;
                if (updateNote)
                {
                    die.RoundNote = AppendNote(die.RoundNote, "顶金 +" + gold + " 金");
                    MarkTypeTriggered(die);
                }
            }
        }

        if (die.Type == DieType.HandTax)
        {
            int gold = HandTaxGold(hand);
            if (gold > 0)
            {
                income += gold;
                if (updateNote)
                {
                    die.RoundNote = AppendNote(die.RoundNote, "牌税 +" + gold + " 金");
                    MarkTypeTriggered(die);
                }
            }
        }

        if (die.Type == DieType.Collection && !die.CollectionTriggeredThisStage)
        {
            int gold = Mathf.Max(0, collectionGoldPerStage);
            if (gold > 0)
            {
                income += gold;
                if (updateNote)
                {
                    die.CollectionTriggeredThisStage = true;
                    die.RoundNote = AppendNote(die.RoundNote, "收账 +" + gold + " 金");
                    MarkTypeTriggered(die);
                }
            }
        }

        if (die.Type == DieType.LeadTicket && die.CheatRerolledThisSettle)
        {
            int gold = Mathf.Max(0, leadTicketGold);
            if (gold > 0)
            {
                income += gold;
                if (updateNote)
                {
                    die.RoundNote = AppendNote(die.RoundNote, "铅票 +" + gold + " 金");
                    MarkTypeTriggered(die);
                }
            }
        }

        if (die.Type == DieType.ShellTax && shellTaxThreshold > 0 && turtleTemporaryDieCount >= shellTaxThreshold)
        {
            int gold = Mathf.Max(0, shellTaxGold);
            if (gold > 0)
            {
                income += gold;
                if (updateNote)
                {
                    die.RoundNote = AppendNote(die.RoundNote, "壳税 +" + gold + " 金");
                    MarkTypeTriggered(die);
                }
            }
        }

        if (die.Type == DieType.CounterGold && leftDieWalletIncome > 0)
        {
            int gold = Mathf.Max(0, counterGold);
            if (gold > 0)
            {
                income += gold;
                if (updateNote)
                {
                    die.RoundNote = AppendNote(die.RoundNote, "柜台 +" + gold + " 金");
                    MarkTypeTriggered(die);
                }
            }
        }

        if (die.Material == DiceMaterial.GiltSeal && RolledHighestFace(die))
        {
            income += 1;
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, "鎏印 +1 金");
            }
        }

        if (!affixFeatureEnabled || die.SuffixAffixes == null)
        {
            return income;
        }

        for (int i = 0; i < die.SuffixAffixes.Count; i++)
        {
            AffixInstance affix = die.SuffixAffixes[i];
            AffixDefinition definition;
            if (affix == null || !affixDefinitions.TryGetValue(affix.Key, out definition) || definition.Slot != AffixSlot.Suffix)
            {
                continue;
            }

            if (!IsAffixTriggerMet(die, definition.TriggerKey, straightTriggered))
            {
                continue;
            }

            int value = AffixValue(affix);
            if (value <= 0)
            {
                continue;
            }

            income += value;
            if (updateNote)
            {
                die.RoundNote = AppendNote(die.RoundNote, definition.DisplayName + " +" + value + " 金");
            }
        }

        return income;
    }

    private int HandTaxGold(HandResult hand)
    {
        if (hand == null)
        {
            return 0;
        }

        if (string.Equals(hand.Name, "四同", StringComparison.Ordinal)
            || string.Equals(hand.Name, "五同", StringComparison.Ordinal)
            || string.Equals(hand.Name, "六同", StringComparison.Ordinal))
        {
            return Mathf.Max(0, handTaxHighGold);
        }

        if (hand.Multiplier > 1f || hand.AllOdd || hand.AllEven)
        {
            return Mathf.Max(0, handTaxLowGold);
        }

        return 0;
    }

    private int CurrentHandTaxGold()
    {
        List<int> values = new List<int>();
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i] == null || dice[i].EffectiveValue <= 0)
            {
                return 0;
            }

            values.Add(dice[i].EffectiveValue);
        }

        return HandTaxGold(EvaluateHand(values, dice));
    }

    private int LumberGoldIncomeForDie(Die die, int dieIndex, List<int> dieScores, HashSet<int> lumberedTargetIds, bool updateNote, ref int individualScore)
    {
        if (die == null || die.Temporary || die.Type != DieType.LumberGold || dieIndex <= 0)
        {
            return 0;
        }

        int gold = Mathf.Max(0, lumberGold);
        int penalty = Mathf.Max(0, lumberScorePenalty);
        if (gold <= 0 || penalty <= 0)
        {
            return 0;
        }

        Die target = dice[dieIndex - 1];
        if (target == null || target.Temporary || target.Id == die.Id || lumberedTargetIds.Contains(target.Id))
        {
            return 0;
        }

        int currentScore = dieScores != null && dieIndex - 1 < dieScores.Count ? dieScores[dieIndex - 1] : target.Score;
        if (currentScore <= 0)
        {
            return 0;
        }

        int actualPenalty = Mathf.Min(penalty, currentScore);
        if (dieScores != null && dieIndex - 1 < dieScores.Count)
        {
            dieScores[dieIndex - 1] = Mathf.Max(0, currentScore - actualPenalty);
        }
        else if (updateNote)
        {
            target.Score = Mathf.Max(0, target.Score - actualPenalty);
        }

        lumberedTargetIds.Add(target.Id);
        individualScore = Mathf.Max(0, individualScore - actualPenalty);

        if (updateNote)
        {
            target.RoundNote = AppendNote(target.RoundNote, "伐木 -" + actualPenalty);
            die.RoundNote = AppendNote(die.RoundNote, "伐木 +" + gold + " 金");
            MarkTypeTriggered(die);
        }

        return gold;
    }

    private int BribeGoldCapacityInCurrentOrder(HandResult hand, int startingWallet, int turtleTemporaryDieCount, List<int> dieScores = null)
    {
        if (bribeGoldCapPerDie <= 0)
        {
            return 0;
        }

        List<int> scoreSnapshot = dieScores != null ? new List<int>(dieScores) : CurrentDieScores();
        int ignoredIndividualScore = 0;
        HashSet<int> lumberedTargetIds = new HashSet<int>();
        int[] slotIncome = new int[dice.Count];
        int simulatedWallet = Mathf.Max(0, startingWallet);
        int capacity = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            Die die = dice[i];
            int leftIncome = i > 0 ? slotIncome[i - 1] : 0;
            int income = WalletIncomeForDie(die, hand, turtleTemporaryDieCount, leftIncome, false);
            income += LumberGoldIncomeForDie(die, i, scoreSnapshot, lumberedTargetIds, false, ref ignoredIndividualScore);
            slotIncome[i] = income;
            simulatedWallet += income;
            if (die == null || die.Temporary || die.Type != DieType.Bribe)
            {
                continue;
            }

            int spend = Mathf.Min(simulatedWallet, Mathf.Max(0, bribeGoldCapPerDie));
            if (spend <= 0)
            {
                continue;
            }

            simulatedWallet -= spend;
            capacity += spend;
        }

        return capacity;
    }

    private List<int> CurrentDieScores()
    {
        List<int> result = new List<int>();
        for (int i = 0; i < dice.Count; i++)
        {
            result.Add(dice[i] != null ? dice[i].Score : 0);
        }

        return result;
    }

    private bool IsAffixTriggerMet(Die die, string triggerKey, bool straightTriggered)
    {
        if (die == null || die.Temporary)
        {
            return false;
        }

        if (string.IsNullOrEmpty(triggerKey) || string.Equals(triggerKey, "always", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(triggerKey, "top_face", StringComparison.OrdinalIgnoreCase))
        {
            return RolledHighestFace(die);
        }

        if (string.Equals(triggerKey, "low_face", StringComparison.OrdinalIgnoreCase))
        {
            return die.EffectiveValue > 0 && die.EffectiveValue <= 2;
        }

        if (string.Equals(triggerKey, "same_value", StringComparison.OrdinalIgnoreCase))
        {
            return HasSameEffectiveValuePartner(die);
        }

        if (string.Equals(triggerKey, "straight", StringComparison.OrdinalIgnoreCase))
        {
            return straightTriggered;
        }

        if (string.Equals(triggerKey, "cheat_rerolled", StringComparison.OrdinalIgnoreCase))
        {
            return die.CheatRerolledThisSettle;
        }

        if (string.Equals(triggerKey, "first_roll", StringComparison.OrdinalIgnoreCase))
        {
            return RollsPerStage - rollsLeft == 1;
        }

        return false;
    }

    private int AffixValue(AffixInstance affix)
    {
        if (affix == null || string.IsNullOrEmpty(affix.Key))
        {
            return 0;
        }

        AffixDefinition definition;
        if (!affixDefinitions.TryGetValue(affix.Key, out definition))
        {
            return 0;
        }

        AffixTierConfig curve;
        if (!affixTierConfigs.TryGetValue(definition.CurveKey, out curve) || curve.Values == null || curve.Values.Length < 6)
        {
            return 0;
        }

        int tier = Mathf.Clamp(affix.Tier, 1, 6);
        return Mathf.Max(0, curve.Values[tier - 1]);
    }

    private int ScoreTemporaryTurtleDice(out int temporaryDieCount, out int nestBonusDieCount)
    {
        int total = 0;
        int producedChainCount = 0;
        temporaryDieCount = 0;
        nestBonusDieCount = 0;

        for (int i = 0; i < dice.Count; i++)
        {
            Die source = dice[i];
            if (!IsTurtleChainSource(source.Type))
            {
                continue;
            }

            int chainCount;
            int chainScore = ScoreTemporaryTurtleChain(source, out chainCount);
            total += chainScore;
            temporaryDieCount += chainCount;
            if (chainCount > 0)
            {
                producedChainCount++;
                MarkTypeTriggered(source);
            }
        }

        int nestCount = CountDiceOfType(DieType.Nest);
        nestBonusDieCount = Mathf.Min(producedChainCount, nestCount);
        int markedNestCount = 0;
        if (nestBonusDieCount > 0)
        {
            for (int i = 0; i < dice.Count && markedNestCount < nestBonusDieCount; i++)
            {
                if (dice[i] != null && dice[i].Type == DieType.Nest)
                {
                    MarkTypeTriggered(dice[i]);
                    markedNestCount++;
                }
            }
        }

        for (int i = 0; i < nestBonusDieCount; i++)
        {
            Die temp = NewTemporaryDie("巢穴 d1", 1);
            temp.Score = BaseScorePerPip;
            temp.RoundNote = "巢穴补骰";
            scoringDice.Add(temp);
            total += temp.Score;
            temporaryDieCount++;
        }

        return total;
    }

    private int ScoreTemporaryTurtleChain(Die source, out int chainCount)
    {
        int value = Mathf.Max(1, source.EffectiveValue);
        bool slow = source.Type == DieType.SlowTurtle;
        int seedMax = TurtleChainSeedMax(value, slow);
        return ScoreTemporaryTurtleChainFromSeed(seedMax, slow, out chainCount);
    }

    private int ScoreTemporaryTurtleChainFromSeed(int seedMax, bool slow, out int chainCount)
    {
        int total = 0;
        int currentMax = Mathf.Max(0, seedMax);
        chainCount = 0;

        while (currentMax >= 1)
        {
            int rolled = UnityEngine.Random.Range(1, currentMax + 1);
            Die temp = NewTemporaryDie(slow ? "慢龟 d" + currentMax : "小龟 d" + currentMax, rolled);
            temp.Score = rolled * BaseScorePerPip;
            temp.RoundNote = slow ? "慢龟壳链" : "龟龟壳链";
            scoringDice.Add(temp);
            total += temp.Score;
            chainCount++;
            currentMax--;
        }

        return total;
    }

    private int PreviewTemporaryTurtleDiceScore(out bool hasRandomness, out int temporaryDieCount, out int nestBonusDieCount)
    {
        hasRandomness = false;
        float total = 0f;
        float totalCount = 0f;
        int producedChainCount = 0;

        for (int i = 0; i < dice.Count; i++)
        {
            Die source = dice[i];
            if (!IsTurtleChainSource(source.Type))
            {
                continue;
            }

            bool slow = source.Type == DieType.SlowTurtle;
            int seedMax = TurtleChainSeedMax(Mathf.Max(1, source.EffectiveValue), slow);
            if (seedMax >= 1)
            {
                hasRandomness = true;
                producedChainCount++;
                TurtlePreviewStats stats = ExpectedTemporaryTurtleStatsFromSeed(seedMax);
                total += stats.Score;
                totalCount += stats.Count;
            }
        }

        nestBonusDieCount = Mathf.Min(producedChainCount, CountDiceOfType(DieType.Nest));
        total += nestBonusDieCount * BaseScorePerPip;
        totalCount += nestBonusDieCount;
        temporaryDieCount = Mathf.RoundToInt(totalCount);
        return Mathf.RoundToInt(total);
    }

    private TurtlePreviewStats ExpectedTemporaryTurtleStatsFromSeed(int seedMax)
    {
        TurtlePreviewStats result = new TurtlePreviewStats();
        for (int currentMax = Mathf.Max(0, seedMax); currentMax >= 1; currentMax--)
        {
            result.Score += ((currentMax + 1) * 0.5f) * BaseScorePerPip;
            result.Count += 1f;
        }

        return result;
    }

    private int TurtleChainSeedMax(int value, bool slow)
    {
        if (slow)
        {
            return value <= 1 ? 0 : Mathf.CeilToInt(value / 2f);
        }

        return Mathf.FloorToInt(value / 2f);
    }

    private bool IsTurtleChainSource(DieType type)
    {
        return type == DieType.Turtle || type == DieType.SlowTurtle;
    }

    private bool RolledHighestFace(Die die)
    {
        if (die == null || die.Faces == null || die.Faces.Length == 0 || die.EffectiveValue <= 0)
        {
            return false;
        }

        return die.EffectiveValue == HighestFaceValue(die);
    }

    private void ApplyPendingTreeGrowth()
    {
        if (pendingTreeGrowth.Count == 0)
        {
            return;
        }

        Dictionary<int, int> growthCounts = new Dictionary<int, int>();
        List<Die> growthDice = new List<Die>();
        for (int i = 0; i < pendingTreeGrowth.Count; i++)
        {
            Die die = pendingTreeGrowth[i];
            if (die == null)
            {
                continue;
            }

            if (!growthCounts.ContainsKey(die.Id))
            {
                growthCounts[die.Id] = 0;
                growthDice.Add(die);
            }

            growthCounts[die.Id]++;
        }

        for (int i = 0; i < growthDice.Count; i++)
        {
            Die die = growthDice[i];
            int growthCount = growthCounts[die.Id];
            for (int step = 0; step < growthCount; step++)
            {
                for (int f = 0; f < die.Faces.Length; f++)
                {
                    die.Faces[f] = Mathf.Max(1, die.Faces[f] + 1);
                }

                die.Growth++;
            }

            Array.Sort(die.Faces);
            string growthText = growthCount > 1 ? "成长 " + growthCount + " 次" : "成长";
            AddLog(die.Name + " " + growthText + "，骰面变为 " + FaceText(die.Faces) + "。");
        }

        pendingTreeGrowth.Clear();
    }

    private void ResolvePassIncome()
    {
        int flatGold = StageFlatGoldPreview();
        int interestGold = StageInterestPreview();
        int compoundGold = CompoundInterestPreview(interestGold);
        int income = flatGold + interestGold + compoundGold;
        lastStageFlatIncome = flatGold;
        lastStageInterestIncome = interestGold;
        lastStageCompoundInterestIncome = compoundGold;
        lastStageIncome = income;
        chapterGold += income;
        string flatText = flatGold > 0 ? "固定 +" + flatGold + "，" : string.Empty;
        string compoundText = compoundGold > 0 ? "，复利 +" + compoundGold : string.Empty;
        rewardBanner = flatText + "利息 +" + interestGold + compoundText + "，金币 +" + income + "。";
        AddLog("过关收入 +" + income + " 金币（固定 " + flatGold + "，利息 " + interestGold + "，复利 " + compoundGold + "）。");
        SaveRun();
    }

    private string StageIncomeDetailText()
    {
        string compoundText = lastStageCompoundInterestIncome > 0 ? " 复利 +" + lastStageCompoundInterestIncome : string.Empty;
        if (lastStageFlatIncome > 0)
        {
            return "固定 +" + lastStageFlatIncome + " 利息 +" + lastStageInterestIncome + compoundText;
        }

        return "利息 +" + lastStageInterestIncome + compoundText;
    }

    private int StageRewardPreview()
    {
        int interestGold = StageInterestPreview();
        return StageFlatGoldPreview() + interestGold + CompoundInterestPreview(interestGold);
    }

    private int StageFlatGoldPreview()
    {
        return Mathf.Max(0, stageClearBaseGold)
            + Mathf.Max(0, rollsLeft) * Mathf.Max(0, rollLeftGoldBonus)
            + Mathf.Max(0, cheatsLeft) * Mathf.Max(0, cheatLeftGoldBonus);
    }

    private int StageInterestPreview()
    {
        int goldBeforeInterest = Mathf.Max(0, chapterGold) + StageFlatGoldPreview();
        return CalculateInterestGold(goldBeforeInterest);
    }

    private int CalculateInterestGold(int goldBeforeInterest)
    {
        if (interestGoldStep <= 0 || interestGoldPerStep <= 0)
        {
            return 0;
        }

        int interest = Mathf.Max(0, goldBeforeInterest) / interestGoldStep * interestGoldPerStep;
        if (interestGoldCap > 0)
        {
            interest = Mathf.Min(interest, interestGoldCap);
        }

        return Mathf.Max(0, interest);
    }

    private int CompoundInterestPreview(int baseInterestGold)
    {
        int count = CountDiceOfType(DieType.CompoundInterest);
        if (count <= 0 || baseInterestGold <= 0 || compoundInterestPerDieCap <= 0)
        {
            return 0;
        }

        int perDie = Mathf.Min(compoundInterestPerDieCap, Mathf.Max(0, baseInterestGold) / 2);
        int total = perDie * count;
        if (compoundInterestTotalCap > 0)
        {
            total = Mathf.Min(total, compoundInterestTotalCap);
        }

        return Mathf.Max(0, total);
    }

    private void EnterMarket(bool chapterMarket)
    {
        currentMarketIsChapter = chapterMarket;
        marketRefreshesWithoutHighTier = 0;
        marketRecentPurchaseRoute = MarketRoute.None;
        activeCraftingItemKey = string.Empty;
        rewardBanner = string.Empty;
        BuildMarketOffers(false);
    }

    private void BuildMarketOffers(bool paidRefresh)
    {
        HashSet<DieType> previousRefreshTypes = paidRefresh ? CurrentOfferDieTypes(-1) : null;
        marketOffers.Clear();
        marketTendencySlotIndex = marketTestRandomRefresh ? -1 : UnityEngine.Random.Range(0, 3);
        MarketRuleConfig rule = CurrentMarketRule();
        bool forceHighTier = !marketTestRandomRefresh && paidRefresh && rule.HighTierPityRefreshes > 0 && marketRefreshesWithoutHighTier >= rule.HighTierPityRefreshes;
        bool hasHighTier = false;
        HashSet<DieType> usedTypes = new HashSet<DieType>();
        HashSet<string> usedCraftingItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < 3; i++)
        {
            MarketOffer offer = MakeRandomMarketOffer(usedTypes, usedCraftingItems, forceHighTier && i == 2, !marketTestRandomRefresh && i == marketTendencySlotIndex, previousRefreshTypes);
            if (offer.Kind == MarketOfferKind.Die && offer.Die != null)
            {
                usedTypes.Add(offer.Die.Type);
                if (IsHighTier(offer.Die.Type))
                {
                    hasHighTier = true;
                }
            }
            else if (offer.Kind == MarketOfferKind.CraftingItem && offer.CraftingItem != null)
            {
                usedCraftingItems.Add(offer.CraftingItem.Key);
            }

            marketOffers.Add(offer);
        }

        if (paidRefresh)
        {
            marketRefreshesWithoutHighTier = marketTestRandomRefresh ? 0 : (hasHighTier ? 0 : marketRefreshesWithoutHighTier + 1);
        }
    }

    private MarketOffer MakeRandomMarketOffer(HashSet<DieType> excludedTypes, HashSet<string> excludedCraftingItems, bool forceHighTier, bool tendencySlot, HashSet<DieType> previousRefreshTypes = null)
    {
        if (ShouldMakeCraftingItemOffer(excludedTypes, excludedCraftingItems, forceHighTier))
        {
            CraftingItemDefinition item = PickCraftingItemDefinitionForOffer(excludedCraftingItems);
            if (item != null)
            {
                return new MarketOffer
                {
                    Kind = MarketOfferKind.CraftingItem,
                    CraftingItem = item,
                    Price = Mathf.Max(1, item.BuyPrice)
                };
            }
        }

        MarketDieConfig config = PickMarketDieConfigForOffer(excludedTypes, forceHighTier, tendencySlot, previousRefreshTypes);
        DieType type = config != null ? config.Type : FallbackMarketOfferType();
        FaceTemplateConfig faceTemplate = PickFaceTemplate(type);
        DiceMaterial material = PickDiceMaterial();
        Die die = NewDie(DefaultName(type), type, FacesForTemplate(faceTemplate, type));
        die.Material = material;

        return new MarketOffer
        {
            Kind = MarketOfferKind.Die,
            Die = die,
            Price = MarketOfferPrice(type, faceTemplate, material)
        };
    }

    private void BuyOffer(int index)
    {
        if (index < 0 || index >= marketOffers.Count)
        {
            return;
        }

        MarketOffer offer = marketOffers[index];
        if (!CanBuyMarketOffer(offer))
        {
            return;
        }

        if (offer.Kind == MarketOfferKind.CraftingItem)
        {
            CraftingItemDefinition item = offer.CraftingItem;
            if (item == null)
            {
                return;
            }

            chapterGold -= offer.Price;
            AddCraftingItem(item.Key, 1);
            activeCraftingItemKey = string.Empty;
            rewardBanner = "购买 " + item.DisplayName + "，持有 " + CraftingItemCount(item.Key) + "。";
            AddLog("买入 " + item.DisplayName + "，-" + offer.Price + " 金币。");
            HashSet<DieType> usedTypesForItem = CurrentOfferDieTypes(index);
            HashSet<string> usedItemsForItem = CurrentOfferCraftingItems(index);
            marketOffers[index] = MakeRandomMarketOffer(usedTypesForItem, usedItemsForItem, false, index == marketTendencySlotIndex);
            SaveRun();
            return;
        }

        chapterGold -= offer.Price;
        Die bought = offer.Die.Clone(nextDieId++);
        ResetRoundState(bought);
        dice.Add(bought);
        MarketRoute boughtRoute = MarketRouteForType(bought.Type);
        if (boughtRoute != MarketRoute.None)
        {
            marketRecentPurchaseRoute = boughtRoute;
            marketTendencySlotIndex = index;
        }

        rewardBanner = "购买 " + DieDisplayName(bought) + "，骰袋 " + dice.Count + " / " + DiceCapacity + "。";
        AddLog("买入 " + DieDisplayName(bought) + "，-" + offer.Price + " 金币。");
        HashSet<DieType> excludedTypes = new HashSet<DieType>();
        for (int i = 0; i < marketOffers.Count; i++)
        {
            if (i != index && marketOffers[i].Kind == MarketOfferKind.Die && marketOffers[i].Die != null)
            {
                excludedTypes.Add(marketOffers[i].Die.Type);
            }
        }

        marketOffers[index] = MakeRandomMarketOffer(excludedTypes, CurrentOfferCraftingItems(index), false, index == marketTendencySlotIndex);
        SaveRun();
    }

    private bool CanBuyMarketOffer(MarketOffer offer)
    {
        if (offer == null || chapterGold < offer.Price)
        {
            return false;
        }

        if (offer.Kind == MarketOfferKind.Die)
        {
            return offer.Die != null && dice.Count < DiceCapacity;
        }

        return affixFeatureEnabled && offer.Kind == MarketOfferKind.CraftingItem && offer.CraftingItem != null;
    }

    private HashSet<DieType> CurrentOfferDieTypes(int skipIndex)
    {
        HashSet<DieType> excludedTypes = new HashSet<DieType>();
        for (int i = 0; i < marketOffers.Count; i++)
        {
            if (i == skipIndex || marketOffers[i].Kind != MarketOfferKind.Die || marketOffers[i].Die == null)
            {
                continue;
            }

            excludedTypes.Add(marketOffers[i].Die.Type);
        }

        return excludedTypes;
    }

    private HashSet<string> CurrentOfferCraftingItems(int skipIndex)
    {
        HashSet<string> excludedItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < marketOffers.Count; i++)
        {
            if (i == skipIndex || marketOffers[i].Kind != MarketOfferKind.CraftingItem || marketOffers[i].CraftingItem == null)
            {
                continue;
            }

            excludedItems.Add(marketOffers[i].CraftingItem.Key);
        }

        return excludedItems;
    }

    private bool ShouldMakeCraftingItemOffer(HashSet<DieType> excludedTypes, HashSet<string> excludedCraftingItems, bool forceHighTier)
    {
        if (!affixFeatureEnabled || marketTestRandomRefresh)
        {
            return false;
        }

        if (forceHighTier)
        {
            return false;
        }

        int chapter = CurrentChapterIndex();
        int itemWeight = TotalCraftingItemOfferWeight(excludedCraftingItems, chapter);
        if (itemWeight <= 0)
        {
            return false;
        }

        int dieWeight = TotalMarketDieOfferWeight(excludedTypes, chapter);
        int adjustedItemWeight = itemWeight * (currentMarketIsChapter ? 2 : 1);
        int totalWeight = dieWeight + adjustedItemWeight;
        return totalWeight > 0 && UnityEngine.Random.Range(0, totalWeight) < adjustedItemWeight;
    }

    private int TotalMarketDieOfferWeight(HashSet<DieType> excludedTypes, int chapter)
    {
        int totalWeight = 0;
        foreach (MarketDieConfig config in marketDieConfigs.Values)
        {
            if (CanPickMarketDie(config, excludedTypes, false, chapter, MarketRoute.None))
            {
                totalWeight += MarketWeight(config, chapter);
            }
        }

        return totalWeight;
    }

    private int TotalCraftingItemOfferWeight(HashSet<string> excludedCraftingItems, int chapter)
    {
        int totalWeight = 0;
        foreach (CraftingItemDefinition item in craftingItemDefinitions.Values)
        {
            if (CanPickCraftingItem(item, excludedCraftingItems, chapter))
            {
                totalWeight += CraftingItemWeight(item, chapter);
            }
        }

        return totalWeight;
    }

    private CraftingItemDefinition PickCraftingItemDefinitionForOffer(HashSet<string> excludedCraftingItems)
    {
        int chapter = CurrentChapterIndex();
        int totalWeight = TotalCraftingItemOfferWeight(excludedCraftingItems, chapter);
        if (totalWeight <= 0 && excludedCraftingItems != null && excludedCraftingItems.Count > 0)
        {
            excludedCraftingItems = null;
            totalWeight = TotalCraftingItemOfferWeight(excludedCraftingItems, chapter);
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);
        foreach (CraftingItemDefinition item in craftingItemDefinitions.Values)
        {
            if (!CanPickCraftingItem(item, excludedCraftingItems, chapter))
            {
                continue;
            }

            roll -= CraftingItemWeight(item, chapter);
            if (roll < 0)
            {
                return item;
            }
        }

        return null;
    }

    private bool CanPickCraftingItem(CraftingItemDefinition item, HashSet<string> excludedCraftingItems, int chapter)
    {
        if (!affixFeatureEnabled)
        {
            return false;
        }

        if (item == null || string.IsNullOrEmpty(item.Key) || CraftingItemWeight(item, chapter) <= 0)
        {
            return false;
        }

        if (chapter < Mathf.Max(1, item.UnlockChapter))
        {
            return false;
        }

        return excludedCraftingItems == null || !excludedCraftingItems.Contains(item.Key);
    }

    private int CraftingItemWeight(CraftingItemDefinition item, int chapter)
    {
        if (item == null)
        {
            return 0;
        }

        if (chapter <= 2)
        {
            return Mathf.Max(0, item.WeightChapter1To2);
        }

        if (chapter <= 5)
        {
            return Mathf.Max(0, item.WeightChapter3To5);
        }

        return Mathf.Max(0, item.WeightChapter6To10);
    }

    private CraftingItemDefinition CraftingItemDefinitionForKey(string itemKey)
    {
        if (string.IsNullOrEmpty(itemKey))
        {
            return null;
        }

        CraftingItemDefinition item;
        craftingItemDefinitions.TryGetValue(itemKey, out item);
        return item;
    }

    private int CraftingItemCount(string itemKey)
    {
        if (string.Equals(itemKey, "affix_add_stone", StringComparison.OrdinalIgnoreCase))
        {
            return affixAddStoneCount;
        }

        if (string.Equals(itemKey, "affix_remove_stone", StringComparison.OrdinalIgnoreCase))
        {
            return affixRemoveStoneCount;
        }

        if (string.Equals(itemKey, "affix_replace_stone", StringComparison.OrdinalIgnoreCase))
        {
            return affixReplaceStoneCount;
        }

        return 0;
    }

    private void AddCraftingItem(string itemKey, int delta)
    {
        SetCraftingItemCount(itemKey, CraftingItemCount(itemKey) + delta);
    }

    private void SetCraftingItemCount(string itemKey, int count)
    {
        count = Mathf.Max(0, count);
        if (string.Equals(itemKey, "affix_add_stone", StringComparison.OrdinalIgnoreCase))
        {
            affixAddStoneCount = count;
        }
        else if (string.Equals(itemKey, "affix_remove_stone", StringComparison.OrdinalIgnoreCase))
        {
            affixRemoveStoneCount = count;
        }
        else if (string.Equals(itemKey, "affix_replace_stone", StringComparison.OrdinalIgnoreCase))
        {
            affixReplaceStoneCount = count;
        }
    }

    private Color CraftingItemColor(string itemKey)
    {
        if (string.Equals(itemKey, "affix_add_stone", StringComparison.OrdinalIgnoreCase))
        {
            return new Color(0.24f, 0.62f, 0.48f);
        }

        if (string.Equals(itemKey, "affix_remove_stone", StringComparison.OrdinalIgnoreCase))
        {
            return new Color(0.62f, 0.45f, 0.42f);
        }

        if (string.Equals(itemKey, "affix_replace_stone", StringComparison.OrdinalIgnoreCase))
        {
            return new Color(0.42f, 0.42f, 0.72f);
        }

        return new Color(0.68f, 0.54f, 0.28f);
    }

    private bool CanUseCraftingItemOnDie(string itemKey, Die die, out string reason)
    {
        if (!affixFeatureEnabled)
        {
            reason = "改造入口已隐藏";
            return false;
        }

        CraftingItemDefinition item = CraftingItemDefinitionForKey(itemKey);
        string craftType = item != null ? item.CraftType : string.Empty;
        if (CraftingItemCount(itemKey) <= 0)
        {
            reason = "没有持有道具";
            return false;
        }

        if (die == null)
        {
            reason = "请选择目标骰";
            return false;
        }

        if (die.Temporary)
        {
            reason = "临时小骰不可改造";
            return false;
        }

        if (string.Equals(craftType, "add_random_affix", StringComparison.OrdinalIgnoreCase))
        {
            bool canAdd = HasLegalAddAffix(die, null);
            reason = canAdd ? "可随机加印" : "无合法空槽";
            return canAdd;
        }

        if (string.Equals(craftType, "remove_random_affix", StringComparison.OrdinalIgnoreCase))
        {
            bool canRemove = AffixCount(die) > 0;
            reason = canRemove ? "可随机剥印" : "没有可剥词缀";
            return canRemove;
        }

        if (string.Equals(craftType, "replace_random_affix", StringComparison.OrdinalIgnoreCase))
        {
            bool canReplace = GetReplaceableAffixLocations(die).Count > 0;
            reason = canReplace ? "可随机换印" : "没有可替换词缀";
            return canReplace;
        }

        reason = "未知道具";
        return false;
    }

    private void ApplyCraftingItemToDie(string itemKey, Die die)
    {
        string reason;
        if (!CanUseCraftingItemOnDie(itemKey, die, out reason))
        {
            rewardBanner = reason;
            return;
        }

        CraftingItemDefinition item = CraftingItemDefinitionForKey(itemKey);
        if (item == null)
        {
            return;
        }

        AffixInstance added;
        AffixInstance removed;
        bool success = false;
        string resultText = string.Empty;
        if (string.Equals(item.CraftType, "add_random_affix", StringComparison.OrdinalIgnoreCase))
        {
            success = AddRandomAffix(die, null, out added);
            resultText = success ? "新增 " + AffixLabel(added) : string.Empty;
        }
        else if (string.Equals(item.CraftType, "remove_random_affix", StringComparison.OrdinalIgnoreCase))
        {
            success = RemoveRandomAffix(die, out removed);
            resultText = success ? "移除 " + AffixLabel(removed) : string.Empty;
        }
        else if (string.Equals(item.CraftType, "replace_random_affix", StringComparison.OrdinalIgnoreCase))
        {
            success = ReplaceRandomAffix(die, out removed, out added);
            resultText = success ? "移除 " + AffixLabel(removed) + "，新增 " + AffixLabel(added) : string.Empty;
        }

        if (!success)
        {
            rewardBanner = "没有合法改造结果，道具未消耗。";
            return;
        }

        AddCraftingItem(itemKey, -1);
        activeCraftingItemKey = string.Empty;
        rewardBanner = item.DisplayName + "改造完成：" + resultText + "。";
        AddLog(item.DisplayName + "改造 " + DieDisplayName(die) + "：" + resultText + "。");
        SaveRun();
    }

    private bool AddRandomAffix(Die die, string forbiddenMutex, out AffixInstance added)
    {
        added = null;
        List<AffixDefinition> candidates = LegalAffixDefinitions(die, forbiddenMutex);
        if (candidates.Count == 0)
        {
            return false;
        }

        AffixDefinition definition = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        added = new AffixInstance
        {
            Key = definition.Key,
            Tier = UnityEngine.Random.Range(1, 7)
        };

        EnsureAffixLists(die);
        if (definition.Slot == AffixSlot.Prefix)
        {
            die.PrefixAffixes.Add(added);
        }
        else
        {
            die.SuffixAffixes.Add(added);
        }

        return true;
    }

    private bool RemoveRandomAffix(Die die, out AffixInstance removed)
    {
        removed = null;
        List<AffixLocation> locations = GetAffixLocations(die);
        if (locations.Count == 0)
        {
            return false;
        }

        AffixLocation location = locations[UnityEngine.Random.Range(0, locations.Count)];
        removed = location.Affix;
        RemoveAffixAt(die, location);
        return true;
    }

    private bool ReplaceRandomAffix(Die die, out AffixInstance removed, out AffixInstance added)
    {
        removed = null;
        added = null;
        List<AffixLocation> locations = GetReplaceableAffixLocations(die);
        if (locations.Count == 0)
        {
            return false;
        }

        AffixLocation location = locations[UnityEngine.Random.Range(0, locations.Count)];
        removed = location.Affix;
        string forbiddenMutex = AffixMutex(removed);
        RemoveAffixAt(die, location);
        if (AddRandomAffix(die, forbiddenMutex, out added))
        {
            return true;
        }

        InsertAffixAt(die, location);
        removed = null;
        added = null;
        return false;
    }

    private List<AffixDefinition> LegalAffixDefinitions(Die die, string forbiddenMutex)
    {
        List<AffixDefinition> candidates = new List<AffixDefinition>();
        if (die == null || die.Temporary)
        {
            return candidates;
        }

        EnsureAffixLists(die);
        int prefixCount = die.PrefixAffixes.Count;
        int suffixCount = die.SuffixAffixes.Count;
        HashSet<string> existing = ExistingAffixMutexes(die, null);
        foreach (AffixDefinition definition in affixDefinitions.Values)
        {
            if (definition == null || !definition.Enabled)
            {
                continue;
            }

            if (definition.Slot == AffixSlot.Prefix && prefixCount >= MaxPrefixAffixes)
            {
                continue;
            }

            if (definition.Slot == AffixSlot.Suffix && suffixCount >= MaxSuffixAffixes)
            {
                continue;
            }

            string mutex = AffixMutex(definition);
            if (!string.IsNullOrEmpty(forbiddenMutex) && string.Equals(mutex, forbiddenMutex, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (existing.Contains(mutex))
            {
                continue;
            }

            candidates.Add(definition);
        }

        return candidates;
    }

    private bool HasLegalAddAffix(Die die, string forbiddenMutex)
    {
        return LegalAffixDefinitions(die, forbiddenMutex).Count > 0;
    }

    private List<AffixLocation> GetReplaceableAffixLocations(Die die)
    {
        List<AffixLocation> result = new List<AffixLocation>();
        List<AffixLocation> locations = GetAffixLocations(die);
        for (int i = 0; i < locations.Count; i++)
        {
            if (HasLegalAddAfterRemoving(die, locations[i]))
            {
                result.Add(locations[i]);
            }
        }

        return result;
    }

    private bool HasLegalAddAfterRemoving(Die die, AffixLocation removed)
    {
        if (die == null || removed == null || removed.Affix == null)
        {
            return false;
        }

        EnsureAffixLists(die);
        int prefixCount = die.PrefixAffixes.Count - (removed.Slot == AffixSlot.Prefix ? 1 : 0);
        int suffixCount = die.SuffixAffixes.Count - (removed.Slot == AffixSlot.Suffix ? 1 : 0);
        string removedMutex = AffixMutex(removed.Affix);
        HashSet<string> existing = ExistingAffixMutexes(die, removed);

        foreach (AffixDefinition definition in affixDefinitions.Values)
        {
            if (definition == null || !definition.Enabled)
            {
                continue;
            }

            if (definition.Slot == AffixSlot.Prefix && prefixCount >= MaxPrefixAffixes)
            {
                continue;
            }

            if (definition.Slot == AffixSlot.Suffix && suffixCount >= MaxSuffixAffixes)
            {
                continue;
            }

            string mutex = AffixMutex(definition);
            if (string.Equals(mutex, removedMutex, StringComparison.OrdinalIgnoreCase) || existing.Contains(mutex))
            {
                continue;
            }

            return true;
        }

        return false;
    }

    private List<AffixLocation> GetAffixLocations(Die die)
    {
        List<AffixLocation> result = new List<AffixLocation>();
        if (die == null)
        {
            return result;
        }

        EnsureAffixLists(die);
        for (int i = 0; i < die.PrefixAffixes.Count; i++)
        {
            if (die.PrefixAffixes[i] != null)
            {
                result.Add(new AffixLocation { Slot = AffixSlot.Prefix, Index = i, Affix = die.PrefixAffixes[i] });
            }
        }

        for (int i = 0; i < die.SuffixAffixes.Count; i++)
        {
            if (die.SuffixAffixes[i] != null)
            {
                result.Add(new AffixLocation { Slot = AffixSlot.Suffix, Index = i, Affix = die.SuffixAffixes[i] });
            }
        }

        return result;
    }

    private void RemoveAffixAt(Die die, AffixLocation location)
    {
        if (die == null || location == null)
        {
            return;
        }

        EnsureAffixLists(die);
        if (location.Slot == AffixSlot.Prefix && location.Index >= 0 && location.Index < die.PrefixAffixes.Count)
        {
            die.PrefixAffixes.RemoveAt(location.Index);
        }
        else if (location.Slot == AffixSlot.Suffix && location.Index >= 0 && location.Index < die.SuffixAffixes.Count)
        {
            die.SuffixAffixes.RemoveAt(location.Index);
        }
    }

    private void InsertAffixAt(Die die, AffixLocation location)
    {
        if (die == null || location == null || location.Affix == null)
        {
            return;
        }

        EnsureAffixLists(die);
        if (location.Slot == AffixSlot.Prefix)
        {
            int index = Mathf.Clamp(location.Index, 0, die.PrefixAffixes.Count);
            die.PrefixAffixes.Insert(index, location.Affix);
        }
        else
        {
            int index = Mathf.Clamp(location.Index, 0, die.SuffixAffixes.Count);
            die.SuffixAffixes.Insert(index, location.Affix);
        }
    }

    private HashSet<string> ExistingAffixMutexes(Die die, AffixLocation excludedLocation)
    {
        HashSet<string> result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        List<AffixLocation> locations = GetAffixLocations(die);
        for (int i = 0; i < locations.Count; i++)
        {
            AffixLocation location = locations[i];
            if (excludedLocation != null && location.Slot == excludedLocation.Slot && location.Index == excludedLocation.Index)
            {
                continue;
            }

            string mutex = AffixMutex(location.Affix);
            if (!string.IsNullOrEmpty(mutex))
            {
                result.Add(mutex);
            }
        }

        return result;
    }

    private int AffixCount(Die die)
    {
        if (die == null)
        {
            return 0;
        }

        EnsureAffixLists(die);
        return die.PrefixAffixes.Count + die.SuffixAffixes.Count;
    }

    private void EnsureAffixLists(Die die)
    {
        if (die == null)
        {
            return;
        }

        if (die.PrefixAffixes == null)
        {
            die.PrefixAffixes = new List<AffixInstance>();
        }

        if (die.SuffixAffixes == null)
        {
            die.SuffixAffixes = new List<AffixInstance>();
        }
    }

    private string AffixMutex(AffixInstance affix)
    {
        if (affix == null || string.IsNullOrEmpty(affix.Key))
        {
            return string.Empty;
        }

        AffixDefinition definition;
        return affixDefinitions.TryGetValue(affix.Key, out definition) ? AffixMutex(definition) : affix.Key;
    }

    private static string AffixMutex(AffixDefinition definition)
    {
        if (definition == null)
        {
            return string.Empty;
        }

        return string.IsNullOrEmpty(definition.MutexGroup) ? definition.Key : definition.MutexGroup;
    }

    private MarketDieConfig PickMarketDieConfigForOffer(HashSet<DieType> excludedTypes, bool forceHighTier, bool tendencySlot, HashSet<DieType> previousRefreshTypes)
    {
        if (marketTestRandomRefresh)
        {
            MarketDieConfig random = PickTestRandomMarketDieConfig(excludedTypes, previousRefreshTypes);
            if (random != null)
            {
                return random;
            }
        }

        if (tendencySlot)
        {
            MarketRoute route = CurrentMarketTendencyRoute();
            if (route != MarketRoute.None && UnityEngine.Random.Range(0, 100) < 70)
            {
                MarketDieConfig routed = PickMarketDieConfig(excludedTypes, forceHighTier, route, previousRefreshTypes);
                if (routed != null)
                {
                    return routed;
                }
            }
        }

        return PickMarketDieConfig(excludedTypes, forceHighTier, previousRefreshTypes);
    }

    private MarketDieConfig PickTestRandomMarketDieConfig(HashSet<DieType> excludedTypes, HashSet<DieType> previousRefreshTypes)
    {
        if (previousRefreshTypes != null && previousRefreshTypes.Count > 0)
        {
            List<MarketDieConfig> preferredCandidates = TestRandomMarketDieCandidates(CombinedExcludedTypes(excludedTypes, previousRefreshTypes));
            if (preferredCandidates.Count > 0)
            {
                return preferredCandidates[UnityEngine.Random.Range(0, preferredCandidates.Count)];
            }
        }

        return PickTestRandomMarketDieConfig(excludedTypes);
    }

    private MarketDieConfig PickTestRandomMarketDieConfig(HashSet<DieType> excludedTypes)
    {
        List<MarketDieConfig> candidates = TestRandomMarketDieCandidates(excludedTypes);
        if (candidates.Count <= 0 && excludedTypes != null && excludedTypes.Count > 0)
        {
            candidates = TestRandomMarketDieCandidates(null);
        }

        if (candidates.Count <= 0)
        {
            return null;
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)];
    }

    private List<MarketDieConfig> TestRandomMarketDieCandidates(HashSet<DieType> excludedTypes)
    {
        List<MarketDieConfig> candidates = new List<MarketDieConfig>();
        foreach (MarketDieConfig config in marketDieConfigs.Values)
        {
            if (config == null || config.Type == DieType.Basic || IsRetiredMarketDieType(config.Type))
            {
                continue;
            }

            if (excludedTypes != null && excludedTypes.Contains(config.Type))
            {
                continue;
            }

            candidates.Add(config);
        }

        return candidates;
    }

    private bool IsRetiredMarketDieType(DieType type)
    {
        return type == DieType.Tree || type == DieType.Gardener || type == DieType.Irrigation;
    }

    private DieType FallbackMarketOfferType()
    {
        return IsMarketDieUnlocked(DieType.Odd, CurrentChapterIndex()) ? DieType.Odd : DieType.Basic;
    }

    private int MarketOfferPrice(DieType type, FaceTemplateConfig faceTemplate, DiceMaterial material)
    {
        int price = BuyPrice(type);
        if (faceTemplate != null)
        {
            price += faceTemplate.PriceModifier;
        }

        price += MaterialPriceModifier(material);
        return Mathf.Max(1, price);
    }

    private int BuyPrice(DieType type)
    {
        MarketDieConfig config;
        if (marketDieConfigs.TryGetValue(type, out config))
        {
            return Mathf.Max(0, config.BuyPrice);
        }

        return DefaultMarketDieConfig(type).BuyPrice;
    }

    private int SellPrice(DieType type)
    {
        MarketDieConfig config;
        if (marketDieConfigs.TryGetValue(type, out config))
        {
            return Mathf.Max(0, config.SellPrice);
        }

        return DefaultMarketDieConfig(type).SellPrice;
    }

    private FaceTemplateConfig PickFaceTemplate(DieType type)
    {
        int chapter = CurrentChapterIndex();
        int totalWeight = 0;
        for (int i = 0; i < faceTemplateConfigs.Count; i++)
        {
            FaceTemplateConfig config = faceTemplateConfigs[i];
            if (!CanUseFaceTemplate(config, type) || !IsFaceTemplateUnlocked(config, chapter))
            {
                continue;
            }

            totalWeight += FaceTemplateWeight(config, chapter);
        }

        if (totalWeight <= 0)
        {
            return FaceTemplate("fallback", 0, 1, 1, 1, DefaultFaces(type), DefaultFaces(type), DefaultFaces(type));
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);
        for (int i = 0; i < faceTemplateConfigs.Count; i++)
        {
            FaceTemplateConfig config = faceTemplateConfigs[i];
            if (!CanUseFaceTemplate(config, type) || !IsFaceTemplateUnlocked(config, chapter))
            {
                continue;
            }

            roll -= FaceTemplateWeight(config, chapter);
            if (roll < 0)
            {
                return config;
            }
        }

        return faceTemplateConfigs[0];
    }

    private bool CanUseFaceTemplate(FaceTemplateConfig config, DieType type)
    {
        if (config == null)
        {
            return false;
        }

        if (IsOddFamily(type))
        {
            return config.OddFaces != null && config.OddFaces.Length > 0;
        }

        if (IsEvenFamily(type))
        {
            return config.EvenFaces != null && config.EvenFaces.Length > 0;
        }

        return config.GenericFaces != null && config.GenericFaces.Length > 0;
    }

    private int FaceTemplateWeight(FaceTemplateConfig config, int chapter)
    {
        if (config == null)
        {
            return 0;
        }

        if (chapter <= 2)
        {
            return Mathf.Max(0, config.WeightChapter1To2);
        }

        if (chapter <= 5)
        {
            return Mathf.Max(0, config.WeightChapter3To5);
        }

        return Mathf.Max(0, config.WeightChapter6To10);
    }

    private int[] FacesForTemplate(FaceTemplateConfig config, DieType type)
    {
        if (config == null)
        {
            return DefaultFaces(type);
        }

        if (IsOddFamily(type) && config.OddFaces != null)
        {
            return (int[])config.OddFaces.Clone();
        }

        if (IsEvenFamily(type) && config.EvenFaces != null)
        {
            return (int[])config.EvenFaces.Clone();
        }

        return config.GenericFaces != null ? (int[])config.GenericFaces.Clone() : DefaultFaces(type);
    }

    private DiceMaterial PickDiceMaterial()
    {
        int chapter = CurrentChapterIndex();
        int totalWeight = 0;
        foreach (DiceMaterialConfig config in diceMaterialConfigs.Values)
        {
            if (!IsDiceMaterialUnlocked(config.Material, chapter))
            {
                continue;
            }

            totalWeight += DiceMaterialWeight(config, chapter);
        }

        if (totalWeight <= 0)
        {
            return DiceMaterial.OfficialIron;
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);
        foreach (DiceMaterialConfig config in diceMaterialConfigs.Values)
        {
            if (!IsDiceMaterialUnlocked(config.Material, chapter))
            {
                continue;
            }

            roll -= DiceMaterialWeight(config, chapter);
            if (roll < 0)
            {
                return config.Material;
            }
        }

        return DiceMaterial.OfficialIron;
    }

    private int DiceMaterialWeight(DiceMaterialConfig config, int chapter)
    {
        if (config == null)
        {
            return 0;
        }

        if (chapter <= 2)
        {
            return Mathf.Max(0, config.WeightChapter1To2);
        }

        if (chapter <= 5)
        {
            return Mathf.Max(0, config.WeightChapter3To5);
        }

        return Mathf.Max(0, config.WeightChapter6To10);
    }

    private bool IsOddFamily(DieType type)
    {
        return type == DieType.Odd || type == DieType.LoneWitness || type == DieType.Stamp;
    }

    private bool IsEvenFamily(DieType type)
    {
        return type == DieType.Even || type == DieType.HalfStep || type == DieType.Track;
    }

    private MarketRoute CurrentMarketTendencyRoute()
    {
        if (marketRecentPurchaseRoute != MarketRoute.None)
        {
            return marketRecentPurchaseRoute;
        }

        return DiceBagMainMarketRoute();
    }

    private MarketRoute DiceBagMainMarketRoute()
    {
        MarketRoute bestRoute = MarketRoute.None;
        int bestCount = 0;
        bool tied = false;

        ConsiderMarketRoute(MarketRoute.Odd, CountDiceByMarketRoute(MarketRoute.Odd), ref bestRoute, ref bestCount, ref tied);
        ConsiderMarketRoute(MarketRoute.Even, CountDiceByMarketRoute(MarketRoute.Even), ref bestRoute, ref bestCount, ref tied);
        ConsiderMarketRoute(MarketRoute.Parity, CountDiceByMarketRoute(MarketRoute.Parity), ref bestRoute, ref bestCount, ref tied);
        ConsiderMarketRoute(MarketRoute.Gold, CountDiceByMarketRoute(MarketRoute.Gold), ref bestRoute, ref bestCount, ref tied);
        ConsiderMarketRoute(MarketRoute.Turtle, CountDiceByMarketRoute(MarketRoute.Turtle), ref bestRoute, ref bestCount, ref tied);
        ConsiderMarketRoute(MarketRoute.Tree, CountDiceByMarketRoute(MarketRoute.Tree), ref bestRoute, ref bestCount, ref tied);
        ConsiderMarketRoute(MarketRoute.Burst, CountDiceByMarketRoute(MarketRoute.Burst), ref bestRoute, ref bestCount, ref tied);

        return bestCount > 0 && !tied ? bestRoute : MarketRoute.None;
    }

    private void ConsiderMarketRoute(MarketRoute route, int count, ref MarketRoute bestRoute, ref int bestCount, ref bool tied)
    {
        if (count <= 0)
        {
            return;
        }

        if (count > bestCount)
        {
            bestRoute = route;
            bestCount = count;
            tied = false;
        }
        else if (count == bestCount)
        {
            tied = true;
        }
    }

    private int CountDiceByMarketRoute(MarketRoute route)
    {
        int count = 0;
        for (int i = 0; i < dice.Count; i++)
        {
            if (dice[i] != null && !dice[i].Temporary && MarketRouteForType(dice[i].Type) == route)
            {
                count++;
            }
        }

        return count;
    }

    private MarketRoute MarketRouteForType(DieType type)
    {
        switch (type)
        {
            case DieType.Odd:
            case DieType.LoneWitness:
            case DieType.Stamp:
                return MarketRoute.Odd;
            case DieType.Even:
            case DieType.HalfStep:
            case DieType.Track:
                return MarketRoute.Even;
            case DieType.ParityNeighborDiff:
            case DieType.ParityNeighborSame:
            case DieType.ParityComplete:
            case DieType.ParityReview:
            case DieType.ParityFlipScore:
            case DieType.ParityHoldScore:
            case DieType.ParityTurner:
                return MarketRoute.Parity;
            case DieType.Piggy:
            case DieType.Treasury:
            case DieType.Bribe:
            case DieType.Investment:
            case DieType.BountyGold:
            case DieType.TopGold:
            case DieType.HandTax:
            case DieType.Collection:
            case DieType.CompoundInterest:
            case DieType.LeadTicket:
            case DieType.ShellTax:
            case DieType.CounterGold:
            case DieType.LumberGold:
                return MarketRoute.Gold;
            case DieType.Turtle:
            case DieType.Shellsmith:
            case DieType.Nest:
            case DieType.SlowTurtle:
                return MarketRoute.Turtle;
            case DieType.Tree:
            case DieType.Gardener:
            case DieType.Irrigation:
            case DieType.PointSeedTree:
            case DieType.PatternTree:
            case DieType.CanopyTree:
            case DieType.RingTree:
            case DieType.FertilizerTree:
            case DieType.PruningTree:
            case DieType.RootTree:
                return MarketRoute.Tree;
            case DieType.Double:
            case DieType.Gambler:
                return MarketRoute.Burst;
        }

        return MarketRoute.None;
    }

    private MarketDieConfig PickMarketDieConfig(HashSet<DieType> excludedTypes, bool forceHighTier, HashSet<DieType> previousRefreshTypes)
    {
        int chapter = CurrentChapterIndex();
        MarketDieConfig picked = PickMarketDieConfigAvoidingPrevious(excludedTypes, forceHighTier, chapter, MarketRoute.None, previousRefreshTypes);
        if (picked == null && excludedTypes != null && excludedTypes.Count > 0)
        {
            picked = PickMarketDieConfigAvoidingPrevious(null, forceHighTier, chapter, MarketRoute.None, previousRefreshTypes);
        }

        if (picked == null && forceHighTier)
        {
            picked = PickMarketDieConfigAvoidingPrevious(excludedTypes, false, chapter, MarketRoute.None, previousRefreshTypes);
        }

        if (picked == null)
        {
            picked = DefaultMarketDieConfig(FallbackMarketOfferType());
        }

        return picked;
    }

    private MarketDieConfig PickMarketDieConfig(HashSet<DieType> excludedTypes, bool forceHighTier, MarketRoute route, HashSet<DieType> previousRefreshTypes)
    {
        if (route == MarketRoute.None)
        {
            return PickMarketDieConfig(excludedTypes, forceHighTier, previousRefreshTypes);
        }

        int chapter = CurrentChapterIndex();
        return PickMarketDieConfigAvoidingPrevious(excludedTypes, forceHighTier, chapter, route, previousRefreshTypes);
    }

    private MarketDieConfig PickMarketDieConfigAvoidingPrevious(HashSet<DieType> excludedTypes, bool forceHighTier, int chapter, MarketRoute route, HashSet<DieType> previousRefreshTypes)
    {
        if (previousRefreshTypes != null && previousRefreshTypes.Count > 0)
        {
            MarketDieConfig preferred = PickMarketDieConfigFromPool(CombinedExcludedTypes(excludedTypes, previousRefreshTypes), forceHighTier, chapter, route);
            if (preferred != null)
            {
                return preferred;
            }
        }

        return PickMarketDieConfigFromPool(excludedTypes, forceHighTier, chapter, route);
    }

    private HashSet<DieType> CombinedExcludedTypes(HashSet<DieType> excludedTypes, HashSet<DieType> additionalExcludedTypes)
    {
        if (additionalExcludedTypes == null || additionalExcludedTypes.Count <= 0)
        {
            return excludedTypes;
        }

        HashSet<DieType> combined = excludedTypes != null
            ? new HashSet<DieType>(excludedTypes)
            : new HashSet<DieType>();
        foreach (DieType type in additionalExcludedTypes)
        {
            combined.Add(type);
        }

        return combined;
    }

    private MarketDieConfig PickMarketDieConfigFromPool(HashSet<DieType> excludedTypes, bool forceHighTier, int chapter)
    {
        return PickMarketDieConfigFromPool(excludedTypes, forceHighTier, chapter, MarketRoute.None);
    }

    private MarketDieConfig PickMarketDieConfigFromPool(HashSet<DieType> excludedTypes, bool forceHighTier, int chapter, MarketRoute route)
    {
        int totalWeight = 0;
        foreach (MarketDieConfig config in marketDieConfigs.Values)
        {
            if (!CanPickMarketDie(config, excludedTypes, forceHighTier, chapter, route))
            {
                continue;
            }

            totalWeight += MarketWeight(config, chapter);
        }

        if (totalWeight <= 0)
        {
            return null;
        }

        int roll = UnityEngine.Random.Range(0, totalWeight);
        foreach (MarketDieConfig config in marketDieConfigs.Values)
        {
            if (!CanPickMarketDie(config, excludedTypes, forceHighTier, chapter, route))
            {
                continue;
            }

            roll -= MarketWeight(config, chapter);
            if (roll < 0)
            {
                return config;
            }
        }

        return null;
    }

    private bool CanPickMarketDie(MarketDieConfig config, HashSet<DieType> excludedTypes, bool forceHighTier, int chapter, MarketRoute route)
    {
        if (config == null || config.Type == DieType.Basic || IsRetiredMarketDieType(config.Type) || MarketWeight(config, chapter) <= 0 || !IsMarketDieUnlocked(config.Type, chapter))
        {
            return false;
        }

        if (route != MarketRoute.None && MarketRouteForType(config.Type) != route)
        {
            return false;
        }

        if (excludedTypes != null && excludedTypes.Contains(config.Type))
        {
            return false;
        }

        return !forceHighTier || IsHighTier(config);
    }

    private int MarketWeight(MarketDieConfig config, int chapter)
    {
        if (config == null)
        {
            return 0;
        }

        if (chapter <= 2)
        {
            return Mathf.Max(0, config.WeightChapter1To2);
        }

        if (chapter <= 5)
        {
            return Mathf.Max(0, config.WeightChapter3To5);
        }

        return Mathf.Max(0, config.WeightChapter6To10);
    }

    private bool IsMarketDieUnlocked(DieType type, int chapter)
    {
        chapter = Mathf.Max(1, chapter);
        switch (type)
        {
            case DieType.Basic:
            case DieType.Odd:
            case DieType.Even:
            case DieType.Piggy:
            case DieType.Turtle:
            case DieType.Double:
                return true;
            case DieType.LoneWitness:
            case DieType.Stamp:
            case DieType.HalfStep:
            case DieType.Track:
            case DieType.ParityNeighborDiff:
            case DieType.ParityNeighborSame:
            case DieType.ParityComplete:
            case DieType.ParityReview:
            case DieType.Shellsmith:
            case DieType.SlowTurtle:
            case DieType.Treasury:
            case DieType.Bribe:
            case DieType.BountyGold:
            case DieType.TopGold:
            case DieType.HandTax:
            case DieType.Collection:
            case DieType.ShellTax:
            case DieType.CounterGold:
            case DieType.Tree:
            case DieType.PointSeedTree:
                return chapter >= 2;
            case DieType.Nest:
            case DieType.Gardener:
            case DieType.Irrigation:
            case DieType.Gambler:
            case DieType.Investment:
            case DieType.CompoundInterest:
            case DieType.LeadTicket:
            case DieType.LumberGold:
            case DieType.ParityFlipScore:
            case DieType.ParityHoldScore:
            case DieType.ParityTurner:
            case DieType.PatternTree:
            case DieType.CanopyTree:
            case DieType.RingTree:
            case DieType.FertilizerTree:
            case DieType.PruningTree:
            case DieType.RootTree:
                return chapter >= 3;
        }

        return chapter >= 4;
    }

    private bool IsDiceMaterialUnlocked(DiceMaterial material, int chapter)
    {
        chapter = Mathf.Max(1, chapter);
        switch (material)
        {
            case DiceMaterial.OfficialIron:
            case DiceMaterial.GiltSeal:
                return true;
            case DiceMaterial.ClearGlaze:
            case DiceMaterial.CopperBone:
                return chapter >= 2;
            case DiceMaterial.LeadSeal:
                return chapter >= 3;
        }

        return false;
    }

    private bool IsFaceTemplateUnlocked(FaceTemplateConfig config, int chapter)
    {
        if (config == null || string.IsNullOrEmpty(config.Key))
        {
            return false;
        }

        chapter = Mathf.Max(1, chapter);
        if (string.Equals(config.Key, "balanced", StringComparison.OrdinalIgnoreCase)
            || string.Equals(config.Key, "low_dense", StringComparison.OrdinalIgnoreCase)
            || string.Equals(config.Key, "high_bias", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (string.Equals(config.Key, "same_focus", StringComparison.OrdinalIgnoreCase)
            || string.Equals(config.Key, "straight_patch", StringComparison.OrdinalIgnoreCase))
        {
            return chapter >= 2;
        }

        if (string.Equals(config.Key, "same_extreme", StringComparison.OrdinalIgnoreCase))
        {
            return chapter >= 3;
        }

        return chapter >= 4;
    }

    private bool IsHighTier(DieType type)
    {
        MarketDieConfig config;
        if (!marketDieConfigs.TryGetValue(type, out config))
        {
            config = DefaultMarketDieConfig(type);
        }

        return IsHighTier(config);
    }

    private bool IsHighTier(MarketDieConfig config)
    {
        if (config == null || string.IsNullOrEmpty(config.Tier))
        {
            return false;
        }

        return string.Equals(config.Tier, "T2", StringComparison.OrdinalIgnoreCase)
            || string.Equals(config.Tier, "T3", StringComparison.OrdinalIgnoreCase);
    }

    private MarketRuleConfig CurrentMarketRule()
    {
        int chapter = CurrentChapterIndex();
        MarketRuleConfig nearestRule = null;
        for (int i = 0; i < marketRuleConfigs.Count; i++)
        {
            MarketRuleConfig rule = marketRuleConfigs[i];
            if (chapter >= rule.ChapterFrom && chapter <= rule.ChapterTo)
            {
                return rule;
            }

            if (rule.ChapterFrom <= chapter)
            {
                nearestRule = rule;
            }
        }

        if (nearestRule != null)
        {
            return nearestRule;
        }

        if (marketRuleConfigs.Count > 0)
        {
            return marketRuleConfigs[0];
        }

        return DefaultMarketRuleConfig(1, 10, 1, 2, 2);
    }

    private int RefreshCost(bool chapterMarket)
    {
        MarketRuleConfig rule = CurrentMarketRule();
        return Mathf.Max(0, chapterMarket ? rule.BossRefreshCost : rule.NormalRefreshCost);
    }

    private HandResult EvaluateHand(List<int> values, List<Die> handDice = null)
    {
        ClearHalfStepBorrowMarks(handDice);
        ClearParityCompleteMarks(handDice);
        Dictionary<int, int> counts = new Dictionary<int, int>();
        for (int i = 0; i < values.Count; i++)
        {
            int value = values[i];
            if (!counts.ContainsKey(value))
            {
                counts[value] = 0;
            }

            counts[value]++;
        }

        int maxCount = 0;
        foreach (KeyValuePair<int, int> pair in counts)
        {
            if (pair.Value > maxCount)
            {
                maxCount = pair.Value;
            }
        }

        bool usedHalfStep;
        int run = LongestRunWithHalfStep(values, handDice, out usedHalfStep);
        bool allOdd = true;
        bool allEven = true;
        for (int i = 0; i < values.Count; i++)
        {
            if (values[i] % 2 == 0)
            {
                allOdd = false;
            }
            else
            {
                allEven = false;
            }
        }

        bool usedParityComplete = false;
        if (!allOdd && TryApplyParityComplete(values, handDice, 1))
        {
            allOdd = true;
            usedParityComplete = true;
        }
        else if (!allEven && TryApplyParityComplete(values, handDice, 0))
        {
            allEven = true;
            usedParityComplete = true;
        }

        HandResult result = new HandResult();
        result.LongestRun = run;
        result.AllOdd = allOdd;
        result.AllEven = allEven;
        result.UsedHalfStep = usedHalfStep;
        result.UsedParityComplete = usedParityComplete;

        if (maxCount >= 6)
        {
            result.Name = "六同";
            result.Multiplier = 6f;
        }
        else if (maxCount >= 5)
        {
            result.Name = "五同";
            result.Multiplier = 4f;
        }
        else if (maxCount >= 4)
        {
            result.Name = "四同";
            result.Multiplier = 3f;
        }
        else if (run >= 5)
        {
            result.Name = "顺子";
            result.Multiplier = 2f;
        }
        else if (maxCount >= 3)
        {
            result.Name = "三同";
            result.Multiplier = 2f;
        }
        else
        {
            result.Name = "无牌型";
            result.Multiplier = 1f;
        }

        return result;
    }

    private float ParityMultiplier(HandResult hand, Encounter encounter)
    {
        float multiplier = 1f;
        if (hand.AllOdd)
        {
            multiplier = 2f;
        }
        else if (hand.AllEven)
        {
            multiplier = 2f;
        }

        return multiplier;
    }

    private string FinalHandName(HandResult hand, float parityMultiplier)
    {
        if (parityMultiplier > hand.Multiplier)
        {
            return hand.AllOdd ? "全奇" : "全偶";
        }

        if (hand.UsedHalfStep && hand.Name == "顺子")
        {
            return "顺子（半步）";
        }

        return hand.Name;
    }

    private string MultiplierText(float multiplier)
    {
        if (Mathf.Approximately(multiplier, Mathf.Round(multiplier)))
        {
            return Mathf.RoundToInt(multiplier).ToString();
        }

        return multiplier.ToString("0.##");
    }

    private int RuleComboBonus(HandResult hand, Encounter encounter)
    {
        if (encounter.Rule == RuleKind.HandAudit && hand.Multiplier > 1f)
        {
            return 2;
        }

        if (encounter.Rule == RuleKind.DoubleJudge && hand.LongestRun >= 5)
        {
            return 4;
        }

        return 0;
    }

    private void ClearHalfStepBorrowMarks(List<Die> handDice)
    {
        if (handDice == null)
        {
            return;
        }

        for (int i = 0; i < handDice.Count; i++)
        {
            if (handDice[i] != null)
            {
                handDice[i].HalfStepBorrowed = false;
            }
        }
    }

    private void ClearParityCompleteMarks(List<Die> handDice)
    {
        if (handDice == null)
        {
            return;
        }

        for (int i = 0; i < handDice.Count; i++)
        {
            if (handDice[i] != null)
            {
                handDice[i].ParityCompleteUsed = false;
            }
        }
    }

    private bool TryApplyParityComplete(List<int> values, List<Die> handDice, int targetParity)
    {
        if (values == null || handDice == null || values.Count != handDice.Count || values.Count <= 1)
        {
            return false;
        }

        int mismatchIndex = -1;
        for (int i = 0; i < values.Count; i++)
        {
            if (Mathf.Abs(values[i]) % 2 == targetParity)
            {
                continue;
            }

            if (mismatchIndex >= 0)
            {
                return false;
            }

            mismatchIndex = i;
        }

        if (mismatchIndex < 0)
        {
            return false;
        }

        Die completer = handDice[mismatchIndex];
        if (completer == null || completer.Temporary || completer.Type != DieType.ParityComplete)
        {
            return false;
        }

        completer.ParityCompleteUsed = true;
        return true;
    }

    private int LongestRunWithHalfStep(List<int> values, List<Die> handDice, out bool usedHalfStep)
    {
        usedHalfStep = false;
        int plainRun = LongestRun(values);
        if (plainRun >= 5 || handDice == null || handDice.Count != values.Count)
        {
            return plainRun;
        }

        List<int> selectedValues = new List<int>();
        List<Die> borrowedDice = new List<Die>();
        List<Die> bestBorrowedDice = new List<Die>();
        int bestRun = plainRun;
        SearchHalfStepRun(0, values, handDice, selectedValues, borrowedDice, ref bestRun, bestBorrowedDice);

        if (bestRun >= 5 && bestBorrowedDice.Count > 0)
        {
            usedHalfStep = true;
            for (int i = 0; i < bestBorrowedDice.Count; i++)
            {
                bestBorrowedDice[i].HalfStepBorrowed = true;
            }
        }

        return bestRun;
    }

    private void SearchHalfStepRun(
        int index,
        List<int> values,
        List<Die> handDice,
        List<int> selectedValues,
        List<Die> borrowedDice,
        ref int bestRun,
        List<Die> bestBorrowedDice)
    {
        if (index >= values.Count)
        {
            int run = LongestRun(selectedValues);
            if (run > bestRun || (run == bestRun && run >= 5 && bestBorrowedDice.Count == 0 && borrowedDice.Count > 0))
            {
                bestRun = run;
                bestBorrowedDice.Clear();
                bestBorrowedDice.AddRange(borrowedDice);
            }

            return;
        }

        int value = Mathf.Max(1, values[index]);
        selectedValues.Add(value);
        SearchHalfStepRun(index + 1, values, handDice, selectedValues, borrowedDice, ref bestRun, bestBorrowedDice);
        selectedValues.RemoveAt(selectedValues.Count - 1);

        Die die = handDice[index];
        if (die == null || die.Temporary || die.Type != DieType.HalfStep || value <= 1)
        {
            return;
        }

        selectedValues.Add(value - 1);
        borrowedDice.Add(die);
        SearchHalfStepRun(index + 1, values, handDice, selectedValues, borrowedDice, ref bestRun, bestBorrowedDice);
        borrowedDice.RemoveAt(borrowedDice.Count - 1);
        selectedValues.RemoveAt(selectedValues.Count - 1);
    }

    private int LongestRun(List<int> values)
    {
        List<int> unique = new List<int>();
        for (int i = 0; i < values.Count; i++)
        {
            if (!unique.Contains(values[i]))
            {
                unique.Add(values[i]);
            }
        }

        unique.Sort();
        int best = 0;
        int current = 0;
        int previous = int.MinValue;
        for (int i = 0; i < unique.Count; i++)
        {
            if (i == 0 || unique[i] == previous + 1)
            {
                current++;
            }
            else
            {
                current = 1;
            }

            if (current > best)
            {
                best = current;
            }

            previous = unique[i];
        }

        return best;
    }

    private float GamblerMultiplier(int threshold)
    {
        int highSum = 0;
        for (int value = threshold + 1; value <= 6; value++)
        {
            highSum += value;
        }

        if (highSum <= 0)
        {
            return 1f;
        }

        return (21f - threshold) / highSum;
    }

    private void LoadGlobalConfigs()
    {
        ResetGlobalConfigDefaults();
        if (!TryLoadGlobalConfigsFromCsv())
        {
            Debug.LogWarning("DiceKingDemo: global config missing or invalid. Using fallback global config.");
        }
    }

    private void ResetGlobalConfigDefaults()
    {
        startingGold = 18;
        stageClearBaseGold = 2;
        rollLeftGoldBonus = 0;
        cheatLeftGoldBonus = 0;
        interestGoldStep = 5;
        interestGoldPerStep = 1;
        interestGoldCap = 5;
        piggyGoldPerHit = 1;
        bountyGoldPerHit = 3;
        topGoldPerHit = 1;
        handTaxLowGold = 1;
        handTaxHighGold = 2;
        collectionGoldPerStage = 1;
        compoundInterestPerDieCap = 2;
        compoundInterestTotalCap = 4;
        leadTicketGold = 2;
        shellTaxThreshold = 3;
        shellTaxGold = 2;
        counterGold = 1;
        lumberGold = 2;
        lumberScorePenalty = 1;
        affixFeatureEnabled = false;
        treasuryGoldStep = 10;
        treasuryScoreCap = 0;
        bribeScorePerGold = 4;
        bribeGoldCapPerDie = 2;
        investmentGoldCapPerDie = 2;
        investmentWalletDivisor = 3;
        investmentScorePerGold = 2;
    }

    private bool TryLoadGlobalConfigsFromCsv()
    {
        TextAsset table = Resources.Load<TextAsset>(GlobalConfigResourcePath);
        if (table == null || string.IsNullOrEmpty(table.text))
        {
            return false;
        }

        string[] lines = table.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            return false;
        }

        List<string> headers = SplitCsvLine(lines[0]);
        Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Count; i++)
        {
            string header = headers[i].Trim();
            if (!columns.ContainsKey(header))
            {
                columns.Add(header, i);
            }
        }

        int loadedCount = 0;
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            List<string> fields = SplitCsvLine(lines[i]);
            string key = CsvValue(fields, columns, "key", string.Empty);
            if (string.IsNullOrEmpty(key))
            {
                continue;
            }

            int value = CsvInt(fields, columns, "value", 0);
            if (ApplyGlobalConfigValue(key, value))
            {
                loadedCount++;
            }
        }

        return loadedCount > 0;
    }

    private bool ApplyGlobalConfigValue(string key, int value)
    {
        switch (key.Trim().ToLowerInvariant())
        {
            case "starting_gold":
                startingGold = Mathf.Max(0, value);
                return true;
            case "stage_clear_base_gold":
                stageClearBaseGold = Mathf.Max(0, value);
                return true;
            case "roll_left_gold_bonus":
                rollLeftGoldBonus = Mathf.Max(0, value);
                return true;
            case "cheat_left_gold_bonus":
                cheatLeftGoldBonus = Mathf.Max(0, value);
                return true;
            case "interest_gold_step":
                interestGoldStep = Mathf.Max(0, value);
                return true;
            case "interest_gold_per_step":
                interestGoldPerStep = Mathf.Max(0, value);
                return true;
            case "interest_gold_cap":
                interestGoldCap = Mathf.Max(0, value);
                return true;
            case "piggy_gold_per_hit":
                piggyGoldPerHit = Mathf.Max(0, value);
                return true;
            case "bounty_gold_per_hit":
                bountyGoldPerHit = Mathf.Max(0, value);
                return true;
            case "top_gold_per_hit":
                topGoldPerHit = Mathf.Max(0, value);
                return true;
            case "hand_tax_low_gold":
                handTaxLowGold = Mathf.Max(0, value);
                return true;
            case "hand_tax_high_gold":
                handTaxHighGold = Mathf.Max(0, value);
                return true;
            case "collection_gold_per_stage":
                collectionGoldPerStage = Mathf.Max(0, value);
                return true;
            case "compound_interest_per_die_cap":
                compoundInterestPerDieCap = Mathf.Max(0, value);
                return true;
            case "compound_interest_total_cap":
                compoundInterestTotalCap = Mathf.Max(0, value);
                return true;
            case "lead_ticket_gold":
                leadTicketGold = Mathf.Max(0, value);
                return true;
            case "shell_tax_threshold":
                shellTaxThreshold = Mathf.Max(0, value);
                return true;
            case "shell_tax_gold":
                shellTaxGold = Mathf.Max(0, value);
                return true;
            case "counter_gold":
                counterGold = Mathf.Max(0, value);
                return true;
            case "lumber_gold":
                lumberGold = Mathf.Max(0, value);
                return true;
            case "lumber_score_penalty":
                lumberScorePenalty = Mathf.Max(0, value);
                return true;
            case "market_test_random_refresh":
                marketTestRandomRefresh = value > 0;
                return true;
            case "affix_feature_enabled":
                affixFeatureEnabled = value > 0;
                return true;
            case "treasury_gold_step":
                treasuryGoldStep = Mathf.Max(0, value);
                return true;
            case "treasury_score_cap":
                treasuryScoreCap = Mathf.Max(0, value);
                return true;
            case "bribe_score_per_gold":
                bribeScorePerGold = Mathf.Max(0, value);
                return true;
            case "bribe_gold_cap_per_die":
                bribeGoldCapPerDie = Mathf.Max(0, value);
                return true;
            case "investment_gold_cap_per_die":
                investmentGoldCapPerDie = Mathf.Max(0, value);
                return true;
            case "investment_wallet_divisor":
                investmentWalletDivisor = Mathf.Max(0, value);
                return true;
            case "investment_score_per_gold":
                investmentScorePerGold = Mathf.Max(0, value);
                return true;
        }

        return false;
    }

    private void LoadRollFeedbackConfig(bool manualReload)
    {
        RollFeedbackConfig candidate = RollFeedbackConfig.CreateDefault();
        string overridePath = RollFeedbackConfigOverridePath();
        bool loadedResource = false;
        bool loadedOverride = false;
        int resourceRows = 0;
        int resourceSkipped = 0;
        int overrideRows = 0;
        int overrideSkipped = 0;

        TextAsset resourceTable = Resources.Load<TextAsset>(RollFeedbackConfigResourcePath);
        if (resourceTable != null && !string.IsNullOrEmpty(resourceTable.text))
        {
            loadedResource = TryApplyRollFeedbackConfigCsv(resourceTable.text, candidate, out resourceRows, out resourceSkipped);
            if (!loadedResource)
            {
                Debug.LogWarning("DiceKingDemo: roll feedback config at Resources/" + RollFeedbackConfigResourcePath + " has no valid rows.");
            }
        }
        else
        {
            Debug.LogWarning("DiceKingDemo: roll feedback config missing at Resources/" + RollFeedbackConfigResourcePath + ". Safe defaults are available.");
        }

        if (File.Exists(overridePath))
        {
            try
            {
                string overrideText = File.ReadAllText(overridePath, Encoding.UTF8);
                loadedOverride = TryApplyRollFeedbackConfigCsv(overrideText, candidate, out overrideRows, out overrideSkipped);
                if (!loadedOverride)
                {
                    Debug.LogWarning("DiceKingDemo: roll feedback override has no valid rows. Path: " + overridePath);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("DiceKingDemo: failed to read roll feedback override at " + overridePath + ". " + exception.Message);
            }
        }

        if (!loadedResource && !loadedOverride && manualReload && rollFeedbackConfig != null)
        {
            Debug.LogWarning("DiceKingDemo: roll feedback reload failed; keeping previous valid config. Override path: " + overridePath);
            if (mode == GameMode.Run)
            {
                AddLog("投骰配置重载失败，保留上次有效配置。");
            }

            return;
        }

        rollFeedbackConfig = candidate;
        rollFeedbackConfigSource = loadedOverride ? overridePath : loadedResource ? "Resources/" + RollFeedbackConfigResourcePath : "代码安全默认值";

        string message = "DiceKingDemo: roll feedback config loaded from " + rollFeedbackConfigSource
            + ". Override path: " + overridePath
            + ". Resource rows " + resourceRows + ", skipped " + resourceSkipped
            + "; override rows " + overrideRows + ", skipped " + overrideSkipped
            + ". " + RollFeedbackSummary(rollFeedbackConfig);
        Debug.Log(message);

        if (manualReload && mode == GameMode.Run)
        {
            AddLog("投骰配置已重载：" + RollFeedbackSourceLabel(rollFeedbackConfigSource) + "。");
        }
    }

    private static bool TryApplyRollFeedbackConfigCsv(string text, RollFeedbackConfig config, out int loadedCount, out int skippedCount)
    {
        loadedCount = 0;
        skippedCount = 0;
        if (config == null || string.IsNullOrEmpty(text))
        {
            return false;
        }

        string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            return false;
        }

        List<string> headers = SplitCsvLine(lines[0]);
        Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Count; i++)
        {
            string header = headers[i].Trim();
            if (!columns.ContainsKey(header))
            {
                columns.Add(header, i);
            }
        }

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            List<string> fields = SplitCsvLine(lines[i]);
            string key = CsvValue(fields, columns, "key", string.Empty);
            if (string.IsNullOrEmpty(key))
            {
                skippedCount++;
                continue;
            }

            float value;
            if (!CsvFloat(fields, columns, "value", out value) || !config.ApplyValue(key, value))
            {
                skippedCount++;
                continue;
            }

            loadedCount++;
        }

        return loadedCount > 0;
    }

    private static string RollFeedbackConfigOverridePath()
    {
        return Path.Combine(Application.persistentDataPath, RollFeedbackConfigOverrideFileName);
    }

    private static string RollFeedbackSourceLabel(string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return "代码默认";
        }

        if (source.StartsWith("Resources/", StringComparison.Ordinal))
        {
            return "默认配置";
        }

        if (source == "代码安全默认值")
        {
            return source;
        }

        return "外部覆盖";
    }

    private static string RollFeedbackSummary(RollFeedbackConfig config)
    {
        if (config == null)
        {
            return "无配置";
        }

        return "窗口 " + ConfigFloatText(config.InputWindowDuration)
            + " 秒，冲量上限 " + config.MaxImpulseCount
            + "，回落 " + ConfigFloatText(config.StopDuration)
            + " 秒。";
    }

    private RollFeedbackConfig CurrentRollFeedbackConfig()
    {
        if (activeRollFeedbackConfig != null)
        {
            return activeRollFeedbackConfig;
        }

        if (rollFeedbackConfig != null)
        {
            return rollFeedbackConfig;
        }

        return RollFeedbackConfig.CreateDefault();
    }

    private void LoadMarketConfigs()
    {
        marketDieConfigs.Clear();
        diceMaterialConfigs.Clear();
        faceTemplateConfigs.Clear();
        marketRuleConfigs.Clear();

        if (!TryLoadMarketDieConfigsFromCsv())
        {
            BuildFallbackMarketDieConfigs();
        }

        FillMissingMarketDieConfigs();
        BuildFaceTemplateConfigs();

        if (!TryLoadDiceMaterialConfigsFromCsv())
        {
            BuildFallbackDiceMaterialConfigs();
        }

        FillMissingDiceMaterialConfigs();

        if (!TryLoadMarketRuleConfigsFromCsv())
        {
            BuildFallbackMarketRuleConfigs();
        }

        if (marketRuleConfigs.Count == 0)
        {
            BuildFallbackMarketRuleConfigs();
        }

        marketRuleConfigs.Sort(delegate (MarketRuleConfig left, MarketRuleConfig right)
        {
            return left.ChapterFrom.CompareTo(right.ChapterFrom);
        });
    }

    private void LoadDiceTypeTooltipConfigs()
    {
        diceTypeTooltipConfigs.Clear();
        if (!TryLoadDiceTypeTooltipConfigsFromCsv())
        {
            BuildFallbackDiceTypeTooltipConfigs();
        }

        FillMissingDiceTypeTooltipConfigs();
    }

    private bool TryLoadDiceTypeTooltipConfigsFromCsv()
    {
        TextAsset table = Resources.Load<TextAsset>(DiceTypeConfigResourcePath);
        if (table == null || string.IsNullOrEmpty(table.text))
        {
            Debug.LogWarning("DiceKingDemo: dice type tooltip config not found at Resources/" + DiceTypeConfigResourcePath + ". Using fallback tooltip copy.");
            return false;
        }

        string[] lines = table.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("DiceKingDemo: dice type tooltip config is empty. Using fallback tooltip copy.");
            return false;
        }

        Dictionary<string, int> columns = CsvColumns(SplitCsvLine(lines[0]));
        Dictionary<DieType, DiceTypeTooltipConfig> loaded = new Dictionary<DieType, DiceTypeTooltipConfig>();
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            List<string> fields = SplitCsvLine(lines[i]);
            DieType type;
            if (!CsvDieType(fields, columns, "die_type", out type))
            {
                continue;
            }

            string tooltipEffect = CsvValue(fields, columns, "tooltip_effect", string.Empty);
            if (string.IsNullOrEmpty(tooltipEffect))
            {
                continue;
            }

            loaded[type] = new DiceTypeTooltipConfig
            {
                Type = type,
                DisplayName = CsvValue(fields, columns, "display_name", TypeName(type)),
                TooltipEffect = tooltipEffect
            };
        }

        if (loaded.Count == 0)
        {
            Debug.LogWarning("DiceKingDemo: dice type tooltip config had no valid rows. Using fallback tooltip copy.");
            return false;
        }

        diceTypeTooltipConfigs.Clear();
        foreach (KeyValuePair<DieType, DiceTypeTooltipConfig> pair in loaded)
        {
            diceTypeTooltipConfigs[pair.Key] = pair.Value;
        }

        return true;
    }

    private void BuildFallbackDiceTypeTooltipConfigs()
    {
        diceTypeTooltipConfigs.Clear();
        Array values = Enum.GetValues(typeof(DieType));
        for (int i = 0; i < values.Length; i++)
        {
            DieType type = (DieType)values.GetValue(i);
            diceTypeTooltipConfigs[type] = DefaultDiceTypeTooltipConfig(type);
        }
    }

    private void FillMissingDiceTypeTooltipConfigs()
    {
        Array values = Enum.GetValues(typeof(DieType));
        for (int i = 0; i < values.Length; i++)
        {
            DieType type = (DieType)values.GetValue(i);
            if (!diceTypeTooltipConfigs.ContainsKey(type))
            {
                diceTypeTooltipConfigs[type] = DefaultDiceTypeTooltipConfig(type);
            }
        }
    }

    private DiceTypeTooltipConfig DefaultDiceTypeTooltipConfig(DieType type)
    {
        string displayName = TypeName(type);
        return new DiceTypeTooltipConfig
        {
            Type = type,
            DisplayName = displayName,
            TooltipEffect = displayName + "效果说明未配置"
        };
    }

    private void LoadAffixConfigs()
    {
        affixTierConfigs.Clear();
        affixDefinitions.Clear();
        craftingItemDefinitions.Clear();

        if (!TryLoadAffixTierConfigsFromCsv())
        {
            BuildFallbackAffixTierConfigs();
        }

        if (!TryLoadAffixDefinitionsFromCsv())
        {
            BuildFallbackAffixDefinitions();
        }

        if (!TryLoadCraftingItemDefinitionsFromCsv())
        {
            BuildFallbackCraftingItemDefinitions();
        }
    }

    private bool TryLoadAffixTierConfigsFromCsv()
    {
        TextAsset table = Resources.Load<TextAsset>(DiceAffixTierConfigResourcePath);
        if (table == null || string.IsNullOrEmpty(table.text))
        {
            Debug.LogWarning("DiceKingDemo: affix tier config not found at Resources/" + DiceAffixTierConfigResourcePath + ". Using fallback affix tier config.");
            return false;
        }

        string[] lines = table.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("DiceKingDemo: affix tier config is empty. Using fallback affix tier config.");
            return false;
        }

        Dictionary<string, int> columns = CsvColumns(SplitCsvLine(lines[0]));
        Dictionary<string, AffixTierConfig> loaded = new Dictionary<string, AffixTierConfig>(StringComparer.OrdinalIgnoreCase);
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            List<string> fields = SplitCsvLine(lines[i]);
            string curveKey = CsvValue(fields, columns, "curve_key", string.Empty);
            if (string.IsNullOrEmpty(curveKey))
            {
                continue;
            }

            AffixTierConfig config = new AffixTierConfig
            {
                CurveKey = curveKey
            };
            for (int tier = 1; tier <= 6; tier++)
            {
                config.Values[tier - 1] = Mathf.Max(0, CsvInt(fields, columns, "t" + tier, 0));
            }

            loaded[curveKey] = config;
        }

        if (loaded.Count == 0)
        {
            Debug.LogWarning("DiceKingDemo: affix tier config had no valid rows. Using fallback affix tier config.");
            return false;
        }

        affixTierConfigs.Clear();
        foreach (KeyValuePair<string, AffixTierConfig> pair in loaded)
        {
            affixTierConfigs[pair.Key] = pair.Value;
        }

        return true;
    }

    private bool TryLoadAffixDefinitionsFromCsv()
    {
        TextAsset table = Resources.Load<TextAsset>(DiceAffixConfigResourcePath);
        if (table == null || string.IsNullOrEmpty(table.text))
        {
            Debug.LogWarning("DiceKingDemo: affix config not found at Resources/" + DiceAffixConfigResourcePath + ". Using fallback affix config.");
            return false;
        }

        string[] lines = table.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("DiceKingDemo: affix config is empty. Using fallback affix config.");
            return false;
        }

        Dictionary<string, int> columns = CsvColumns(SplitCsvLine(lines[0]));
        Dictionary<string, AffixDefinition> loaded = new Dictionary<string, AffixDefinition>(StringComparer.OrdinalIgnoreCase);
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            List<string> fields = SplitCsvLine(lines[i]);
            string key = CsvValue(fields, columns, "affix_key", string.Empty);
            AffixSlot slot;
            if (string.IsNullOrEmpty(key) || !CsvAffixSlot(fields, columns, "slot", out slot))
            {
                continue;
            }

            string curveKey = CsvValue(fields, columns, "curve_key", string.Empty);
            if (!affixTierConfigs.ContainsKey(curveKey))
            {
                continue;
            }

            string displayName = CsvValue(fields, columns, "display_name", key);
            string mutexGroup = CsvValue(fields, columns, "mutex_group", key);
            loaded[key] = new AffixDefinition
            {
                Key = key,
                DisplayName = displayName,
                Slot = slot,
                TriggerKey = CsvValue(fields, columns, "trigger_key", string.Empty),
                CurveKey = curveKey,
                WeightChapter1To2 = Mathf.Max(0, CsvInt(fields, columns, "weight_ch1_2", 0)),
                WeightChapter3To5 = Mathf.Max(0, CsvInt(fields, columns, "weight_ch3_5", 0)),
                WeightChapter6To10 = Mathf.Max(0, CsvInt(fields, columns, "weight_ch6_10", 0)),
                ShortRule = CsvValue(fields, columns, "short_rule", displayName),
                MutexGroup = mutexGroup,
                Enabled = CsvBool(fields, columns, "enabled", true)
            };
        }

        if (loaded.Count == 0)
        {
            Debug.LogWarning("DiceKingDemo: affix config had no valid rows. Using fallback affix config.");
            return false;
        }

        affixDefinitions.Clear();
        foreach (KeyValuePair<string, AffixDefinition> pair in loaded)
        {
            affixDefinitions[pair.Key] = pair.Value;
        }

        return true;
    }

    private bool TryLoadCraftingItemDefinitionsFromCsv()
    {
        TextAsset table = Resources.Load<TextAsset>(DiceCraftingItemConfigResourcePath);
        if (table == null || string.IsNullOrEmpty(table.text))
        {
            Debug.LogWarning("DiceKingDemo: crafting item config not found at Resources/" + DiceCraftingItemConfigResourcePath + ". Using fallback crafting item config.");
            return false;
        }

        string[] lines = table.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("DiceKingDemo: crafting item config is empty. Using fallback crafting item config.");
            return false;
        }

        Dictionary<string, int> columns = CsvColumns(SplitCsvLine(lines[0]));
        Dictionary<string, CraftingItemDefinition> loaded = new Dictionary<string, CraftingItemDefinition>(StringComparer.OrdinalIgnoreCase);
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            List<string> fields = SplitCsvLine(lines[i]);
            string key = CsvValue(fields, columns, "item_key", string.Empty);
            string craftType = CsvValue(fields, columns, "craft_type", string.Empty);
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(craftType))
            {
                continue;
            }

            loaded[key] = new CraftingItemDefinition
            {
                Key = key,
                DisplayName = CsvValue(fields, columns, "display_name", key),
                CraftType = craftType,
                BuyPrice = Mathf.Max(0, CsvInt(fields, columns, "buy_price", 0)),
                UnlockChapter = Mathf.Max(1, CsvInt(fields, columns, "unlock_chapter", 2)),
                WeightChapter1To2 = Mathf.Max(0, CsvInt(fields, columns, "weight_ch1_2", 0)),
                WeightChapter3To5 = Mathf.Max(0, CsvInt(fields, columns, "weight_ch3_5", 0)),
                WeightChapter6To10 = Mathf.Max(0, CsvInt(fields, columns, "weight_ch6_10", 0)),
                ShortRule = CsvValue(fields, columns, "short_rule", craftType)
            };
        }

        if (loaded.Count == 0)
        {
            Debug.LogWarning("DiceKingDemo: crafting item config had no valid rows. Using fallback crafting item config.");
            return false;
        }

        craftingItemDefinitions.Clear();
        foreach (KeyValuePair<string, CraftingItemDefinition> pair in loaded)
        {
            craftingItemDefinitions[pair.Key] = pair.Value;
        }

        return true;
    }

    private bool TryLoadMarketDieConfigsFromCsv()
    {
        TextAsset table = Resources.Load<TextAsset>(MarketDieConfigResourcePath);
        if (table == null || string.IsNullOrEmpty(table.text))
        {
            Debug.LogWarning("DiceKingDemo: market die config not found at Resources/" + MarketDieConfigResourcePath + ". Using fallback market dice config.");
            return false;
        }

        string[] lines = table.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("DiceKingDemo: market die config is empty. Using fallback market dice config.");
            return false;
        }

        List<string> headers = SplitCsvLine(lines[0]);
        Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Count; i++)
        {
            string header = headers[i].Trim();
            if (!columns.ContainsKey(header))
            {
                columns.Add(header, i);
            }
        }

        Dictionary<DieType, MarketDieConfig> loaded = new Dictionary<DieType, MarketDieConfig>();
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            List<string> fields = SplitCsvLine(lines[i]);
            DieType type;
            if (!CsvDieType(fields, columns, "die_type", out type))
            {
                continue;
            }

            MarketDieConfig fallback = DefaultMarketDieConfig(type);
            MarketDieConfig config = new MarketDieConfig
            {
                Type = type,
                BuyPrice = Mathf.Max(0, CsvInt(fields, columns, "buy_price", fallback.BuyPrice)),
                SellPrice = Mathf.Max(0, CsvInt(fields, columns, "sell_price", fallback.SellPrice)),
                Tier = CsvValue(fields, columns, "tier", fallback.Tier),
                WeightChapter1To2 = Mathf.Max(0, CsvInt(fields, columns, "weight_ch1_2", fallback.WeightChapter1To2)),
                WeightChapter3To5 = Mathf.Max(0, CsvInt(fields, columns, "weight_ch3_5", fallback.WeightChapter3To5)),
                WeightChapter6To10 = Mathf.Max(0, CsvInt(fields, columns, "weight_ch6_10", fallback.WeightChapter6To10))
            };

            loaded[type] = config;
        }

        if (loaded.Count == 0)
        {
            Debug.LogWarning("DiceKingDemo: market die config had no valid rows. Using fallback market dice config.");
            return false;
        }

        marketDieConfigs.Clear();
        foreach (KeyValuePair<DieType, MarketDieConfig> pair in loaded)
        {
            marketDieConfigs[pair.Key] = pair.Value;
        }

        return true;
    }

    private bool TryLoadMarketRuleConfigsFromCsv()
    {
        TextAsset table = Resources.Load<TextAsset>(MarketRuleConfigResourcePath);
        if (table == null || string.IsNullOrEmpty(table.text))
        {
            Debug.LogWarning("DiceKingDemo: market rule config not found at Resources/" + MarketRuleConfigResourcePath + ". Using fallback market rule config.");
            return false;
        }

        string[] lines = table.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("DiceKingDemo: market rule config is empty. Using fallback market rule config.");
            return false;
        }

        List<string> headers = SplitCsvLine(lines[0]);
        Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Count; i++)
        {
            string header = headers[i].Trim();
            if (!columns.ContainsKey(header))
            {
                columns.Add(header, i);
            }
        }

        List<MarketRuleConfig> loaded = new List<MarketRuleConfig>();
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            List<string> fields = SplitCsvLine(lines[i]);
            int chapterFrom = Mathf.Max(1, CsvInt(fields, columns, "chapter_from", 1));
            int chapterTo = Mathf.Max(chapterFrom, CsvInt(fields, columns, "chapter_to", chapterFrom));
            int normalRefreshCost = Mathf.Max(0, CsvInt(fields, columns, "normal_refresh_cost", 2));
            int bossRefreshCost = Mathf.Max(0, CsvInt(fields, columns, "boss_refresh_cost", normalRefreshCost));
            int highTierPityRefreshes = Mathf.Max(0, CsvInt(fields, columns, "high_tier_pity_refreshes", 0));

            loaded.Add(new MarketRuleConfig
            {
                ChapterFrom = chapterFrom,
                ChapterTo = chapterTo,
                NormalRefreshCost = normalRefreshCost,
                BossRefreshCost = bossRefreshCost,
                HighTierPityRefreshes = highTierPityRefreshes
            });
        }

        if (loaded.Count == 0)
        {
            Debug.LogWarning("DiceKingDemo: market rule config had no valid rows. Using fallback market rule config.");
            return false;
        }

        marketRuleConfigs.Clear();
        marketRuleConfigs.AddRange(loaded);
        return true;
    }

    private bool TryLoadDiceMaterialConfigsFromCsv()
    {
        TextAsset table = Resources.Load<TextAsset>(DiceMaterialConfigResourcePath);
        if (table == null || string.IsNullOrEmpty(table.text))
        {
            Debug.LogWarning("DiceKingDemo: dice material config not found at Resources/" + DiceMaterialConfigResourcePath + ". Using fallback material config.");
            return false;
        }

        string[] lines = table.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("DiceKingDemo: dice material config is empty. Using fallback material config.");
            return false;
        }

        List<string> headers = SplitCsvLine(lines[0]);
        Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Count; i++)
        {
            string header = headers[i].Trim();
            if (!columns.ContainsKey(header))
            {
                columns.Add(header, i);
            }
        }

        Dictionary<DiceMaterial, DiceMaterialConfig> loaded = new Dictionary<DiceMaterial, DiceMaterialConfig>();
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            List<string> fields = SplitCsvLine(lines[i]);
            DiceMaterial material;
            if (!CsvDiceMaterial(fields, columns, "material_key", out material) || material == DiceMaterial.None)
            {
                continue;
            }

            DiceMaterialConfig fallback = DefaultDiceMaterialConfig(material);
            loaded[material] = new DiceMaterialConfig
            {
                Material = material,
                DisplayName = CsvValue(fields, columns, "display_name", fallback.DisplayName),
                PriceModifier = CsvInt(fields, columns, "price_modifier", fallback.PriceModifier),
                SellModifier = CsvInt(fields, columns, "sell_modifier", fallback.SellModifier),
                WeightChapter1To2 = Mathf.Max(0, CsvInt(fields, columns, "weight_ch1_2", fallback.WeightChapter1To2)),
                WeightChapter3To5 = Mathf.Max(0, CsvInt(fields, columns, "weight_ch3_5", fallback.WeightChapter3To5)),
                WeightChapter6To10 = Mathf.Max(0, CsvInt(fields, columns, "weight_ch6_10", fallback.WeightChapter6To10)),
                ShortRule = CsvValue(fields, columns, "short_rule", fallback.ShortRule)
            };
        }

        if (loaded.Count == 0)
        {
            Debug.LogWarning("DiceKingDemo: dice material config had no valid rows. Using fallback material config.");
            return false;
        }

        diceMaterialConfigs.Clear();
        foreach (KeyValuePair<DiceMaterial, DiceMaterialConfig> pair in loaded)
        {
            diceMaterialConfigs[pair.Key] = pair.Value;
        }

        return true;
    }

    private void BuildFaceTemplateConfigs()
    {
        faceTemplateConfigs.Clear();
        faceTemplateConfigs.Add(FaceTemplate("balanced", 0, 42, 34, 28,
            new int[] { 1, 2, 3, 4, 5, 6 },
            new int[] { 1, 1, 3, 3, 5, 5 },
            new int[] { 2, 2, 4, 4, 6, 6 }));
        faceTemplateConfigs.Add(FaceTemplate("high_bias", 2, 18, 22, 24,
            new int[] { 3, 4, 4, 5, 5, 6 },
            new int[] { 3, 3, 5, 5, 5, 5 },
            new int[] { 4, 4, 6, 6, 6, 6 }));
        faceTemplateConfigs.Add(FaceTemplate("low_dense", -1, 16, 14, 10,
            new int[] { 1, 1, 2, 2, 3, 4 },
            new int[] { 1, 1, 1, 3, 3, 5 },
            new int[] { 2, 2, 2, 4, 4, 6 }));
        faceTemplateConfigs.Add(FaceTemplate("same_focus", 3, 16, 18, 20,
            new int[] { 4, 4, 4, 4, 2, 6 },
            new int[] { 3, 3, 3, 3, 1, 5 },
            new int[] { 4, 4, 4, 4, 2, 6 }));
        faceTemplateConfigs.Add(FaceTemplate("same_extreme", 7, 0, 4, 6,
            new int[] { 4, 4, 4, 4, 4, 4 },
            new int[] { 3, 3, 3, 3, 3, 3 },
            new int[] { 4, 4, 4, 4, 4, 4 }));
        faceTemplateConfigs.Add(FaceTemplate("straight_patch", 2, 8, 8, 12,
            new int[] { 2, 3, 3, 4, 4, 5 },
            null,
            null));
    }

    private FaceTemplateConfig FaceTemplate(string key, int priceModifier, int weightChapter1To2, int weightChapter3To5, int weightChapter6To10, int[] genericFaces, int[] oddFaces, int[] evenFaces)
    {
        return new FaceTemplateConfig
        {
            Key = key,
            PriceModifier = priceModifier,
            WeightChapter1To2 = weightChapter1To2,
            WeightChapter3To5 = weightChapter3To5,
            WeightChapter6To10 = weightChapter6To10,
            GenericFaces = SortedFaces(genericFaces),
            OddFaces = SortedFaces(oddFaces),
            EvenFaces = SortedFaces(evenFaces)
        };
    }

    private void BuildFallbackMarketDieConfigs()
    {
        marketDieConfigs.Clear();
        Array values = Enum.GetValues(typeof(DieType));
        for (int i = 0; i < values.Length; i++)
        {
            DieType type = (DieType)values.GetValue(i);
            marketDieConfigs[type] = DefaultMarketDieConfig(type);
        }
    }

    private void FillMissingMarketDieConfigs()
    {
        Array values = Enum.GetValues(typeof(DieType));
        for (int i = 0; i < values.Length; i++)
        {
            DieType type = (DieType)values.GetValue(i);
            if (!marketDieConfigs.ContainsKey(type))
            {
                marketDieConfigs[type] = DefaultMarketDieConfig(type);
            }
        }
    }

    private void BuildFallbackDiceMaterialConfigs()
    {
        diceMaterialConfigs.Clear();
        Array values = Enum.GetValues(typeof(DiceMaterial));
        for (int i = 0; i < values.Length; i++)
        {
            DiceMaterial material = (DiceMaterial)values.GetValue(i);
            if (material == DiceMaterial.None)
            {
                continue;
            }

            diceMaterialConfigs[material] = DefaultDiceMaterialConfig(material);
        }
    }

    private void FillMissingDiceMaterialConfigs()
    {
        Array values = Enum.GetValues(typeof(DiceMaterial));
        for (int i = 0; i < values.Length; i++)
        {
            DiceMaterial material = (DiceMaterial)values.GetValue(i);
            if (material == DiceMaterial.None)
            {
                continue;
            }

            if (!diceMaterialConfigs.ContainsKey(material))
            {
                diceMaterialConfigs[material] = DefaultDiceMaterialConfig(material);
            }
        }
    }

    private void BuildFallbackAffixTierConfigs()
    {
        affixTierConfigs.Clear();
        AddFallbackAffixTier("score_low_freq", 6, 5, 4, 3, 2, 1);
        AddFallbackAffixTier("score_high_freq", 4, 3, 2, 2, 1, 1);
        AddFallbackAffixTier("gold_low_freq", 4, 3, 2, 2, 1, 1);
        AddFallbackAffixTier("gold_high_freq", 3, 2, 2, 1, 1, 1);
        AddFallbackAffixTier("market_discount", 4, 3, 2, 2, 1, 1);
    }

    private void AddFallbackAffixTier(string curveKey, int t1, int t2, int t3, int t4, int t5, int t6)
    {
        affixTierConfigs[curveKey] = new AffixTierConfig
        {
            CurveKey = curveKey,
            Values = new int[] { t1, t2, t3, t4, t5, t6 }
        };
    }

    private void BuildFallbackAffixDefinitions()
    {
        affixDefinitions.Clear();
        AddFallbackAffix("flat_score", "平账", AffixSlot.Prefix, "always", "score_high_freq", 8, 8, 8, "本骰单骰分 +V");
        AddFallbackAffix("top_face_score", "顶面", AffixSlot.Prefix, "top_face", "score_low_freq", 5, 7, 8, "最高面时单骰分 +V");
        AddFallbackAffix("low_face_score", "厚底", AffixSlot.Prefix, "low_face", "score_low_freq", 5, 7, 8, "点数不高于 2 时单骰分 +V");
        AddFallbackAffix("same_value_score", "对印", AffixSlot.Prefix, "same_value", "score_high_freq", 5, 7, 8, "有同点伙伴时单骰分 +V");
        AddFallbackAffix("straight_score", "顺批", AffixSlot.Prefix, "straight", "score_low_freq", 3, 5, 6, "形成顺子时单骰分 +V");
        AddFallbackAffix("cheat_score", "回火", AffixSlot.Prefix, "cheat_rerolled", "score_low_freq", 3, 5, 6, "出千确认后单骰分 +V");
        AddFallbackAffix("top_face_gold", "顶账", AffixSlot.Suffix, "top_face", "gold_low_freq", 4, 6, 7, "最高面时钱包 +G");
        AddFallbackAffix("low_face_gold", "低保", AffixSlot.Suffix, "low_face", "gold_low_freq", 4, 6, 7, "点数不高于 2 时钱包 +G");
        AddFallbackAffix("same_value_gold", "分账", AffixSlot.Suffix, "same_value", "gold_high_freq", 4, 6, 7, "有同点伙伴时钱包 +G");
        AddFallbackAffix("straight_gold", "顺账", AffixSlot.Suffix, "straight", "gold_low_freq", 3, 5, 6, "形成顺子时钱包 +G");
        AddFallbackAffix("cheat_gold", "回火账", AffixSlot.Suffix, "cheat_rerolled", "gold_low_freq", 3, 5, 6, "出千确认后钱包 +G");
        AddFallbackAffix("first_roll_gold", "首单", AffixSlot.Suffix, "first_roll", "gold_high_freq", 5, 7, 7, "小关第一次出手钱包 +G");
    }

    private void AddFallbackAffix(string key, string displayName, AffixSlot slot, string triggerKey, string curveKey, int weightChapter1To2, int weightChapter3To5, int weightChapter6To10, string shortRule)
    {
        affixDefinitions[key] = new AffixDefinition
        {
            Key = key,
            DisplayName = displayName,
            Slot = slot,
            TriggerKey = triggerKey,
            CurveKey = curveKey,
            WeightChapter1To2 = weightChapter1To2,
            WeightChapter3To5 = weightChapter3To5,
            WeightChapter6To10 = weightChapter6To10,
            ShortRule = shortRule,
            MutexGroup = key,
            Enabled = true
        };
    }

    private void BuildFallbackCraftingItemDefinitions()
    {
        craftingItemDefinitions.Clear();
        AddFallbackCraftingItem("affix_add_stone", "加印石", "add_random_affix", 7, 2, 4, 7, 8, "随机加 1 条合法词缀");
        AddFallbackCraftingItem("affix_remove_stone", "剥印石", "remove_random_affix", 5, 2, 3, 5, 6, "随机删 1 条已有词缀");
        AddFallbackCraftingItem("affix_replace_stone", "换印石", "replace_random_affix", 8, 2, 2, 4, 6, "随机删 1 条后再随机加 1 条");
    }

    private void AddFallbackCraftingItem(string key, string displayName, string craftType, int buyPrice, int unlockChapter, int weightChapter1To2, int weightChapter3To5, int weightChapter6To10, string shortRule)
    {
        craftingItemDefinitions[key] = new CraftingItemDefinition
        {
            Key = key,
            DisplayName = displayName,
            CraftType = craftType,
            BuyPrice = buyPrice,
            UnlockChapter = unlockChapter,
            WeightChapter1To2 = weightChapter1To2,
            WeightChapter3To5 = weightChapter3To5,
            WeightChapter6To10 = weightChapter6To10,
            ShortRule = shortRule
        };
    }

    private void BuildFallbackMarketRuleConfigs()
    {
        marketRuleConfigs.Clear();
        marketRuleConfigs.Add(DefaultMarketRuleConfig(1, 2, 1, 2, 2));
        marketRuleConfigs.Add(DefaultMarketRuleConfig(3, 5, 3, 4, 2));
        marketRuleConfigs.Add(DefaultMarketRuleConfig(6, 10, 4, 5, 2));
    }

    private MarketDieConfig DefaultMarketDieConfig(DieType type)
    {
        switch (type)
        {
            case DieType.Basic:
                return new MarketDieConfig { Type = type, BuyPrice = 3, SellPrice = 1, Tier = "T0", WeightChapter1To2 = 0, WeightChapter3To5 = 0, WeightChapter6To10 = 0 };
            case DieType.Odd:
                return new MarketDieConfig { Type = type, BuyPrice = 5, SellPrice = 3, Tier = "T1", WeightChapter1To2 = 20, WeightChapter3To5 = 16, WeightChapter6To10 = 12 };
            case DieType.Even:
                return new MarketDieConfig { Type = type, BuyPrice = 5, SellPrice = 3, Tier = "T1", WeightChapter1To2 = 20, WeightChapter3To5 = 16, WeightChapter6To10 = 12 };
            case DieType.LoneWitness:
                return new MarketDieConfig { Type = type, BuyPrice = 8, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 7, WeightChapter3To5 = 8, WeightChapter6To10 = 8 };
            case DieType.Stamp:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 6, WeightChapter3To5 = 7, WeightChapter6To10 = 8 };
            case DieType.HalfStep:
                return new MarketDieConfig { Type = type, BuyPrice = 8, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 7, WeightChapter3To5 = 8, WeightChapter6To10 = 8 };
            case DieType.Track:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 6, WeightChapter3To5 = 7, WeightChapter6To10 = 8 };
            case DieType.ParityNeighborDiff:
                return new MarketDieConfig { Type = type, BuyPrice = 8, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 10, WeightChapter3To5 = 12, WeightChapter6To10 = 10 };
            case DieType.ParityNeighborSame:
                return new MarketDieConfig { Type = type, BuyPrice = 8, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 10, WeightChapter3To5 = 12, WeightChapter6To10 = 10 };
            case DieType.ParityComplete:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 9, WeightChapter3To5 = 12, WeightChapter6To10 = 10 };
            case DieType.ParityReview:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 8, WeightChapter3To5 = 11, WeightChapter6To10 = 10 };
            case DieType.ParityFlipScore:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 0, WeightChapter3To5 = 11, WeightChapter6To10 = 10 };
            case DieType.ParityHoldScore:
                return new MarketDieConfig { Type = type, BuyPrice = 8, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 0, WeightChapter3To5 = 10, WeightChapter6To10 = 10 };
            case DieType.ParityTurner:
                return new MarketDieConfig { Type = type, BuyPrice = 10, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 8, WeightChapter6To10 = 10 };
            case DieType.Piggy:
                return new MarketDieConfig { Type = type, BuyPrice = 6, SellPrice = 3, Tier = "T1", WeightChapter1To2 = 18, WeightChapter3To5 = 13, WeightChapter6To10 = 8 };
            case DieType.Turtle:
                return new MarketDieConfig { Type = type, BuyPrice = 8, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 10, WeightChapter3To5 = 10, WeightChapter6To10 = 10 };
            case DieType.Shellsmith:
                return new MarketDieConfig { Type = type, BuyPrice = 8, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 6, WeightChapter3To5 = 8, WeightChapter6To10 = 8 };
            case DieType.Nest:
                return new MarketDieConfig { Type = type, BuyPrice = 10, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 5, WeightChapter6To10 = 7 };
            case DieType.SlowTurtle:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 5, WeightChapter3To5 = 7, WeightChapter6To10 = 8 };
            case DieType.Double:
                return new MarketDieConfig { Type = type, BuyPrice = 8, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 6, WeightChapter3To5 = 8, WeightChapter6To10 = 9 };
            case DieType.Tree:
                return new MarketDieConfig { Type = type, BuyPrice = 10, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 0, WeightChapter6To10 = 0 };
            case DieType.Gardener:
                return new MarketDieConfig { Type = type, BuyPrice = 10, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 0, WeightChapter6To10 = 0 };
            case DieType.Irrigation:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 0, WeightChapter3To5 = 0, WeightChapter6To10 = 0 };
            case DieType.PointSeedTree:
                return new MarketDieConfig { Type = type, BuyPrice = 10, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 6, WeightChapter3To5 = 9, WeightChapter6To10 = 8 };
            case DieType.PatternTree:
                return new MarketDieConfig { Type = type, BuyPrice = 11, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 8, WeightChapter6To10 = 8 };
            case DieType.CanopyTree:
                return new MarketDieConfig { Type = type, BuyPrice = 10, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 9, WeightChapter6To10 = 9 };
            case DieType.RingTree:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 0, WeightChapter3To5 = 8, WeightChapter6To10 = 9 };
            case DieType.FertilizerTree:
                return new MarketDieConfig { Type = type, BuyPrice = 12, SellPrice = 6, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 7, WeightChapter6To10 = 8 };
            case DieType.PruningTree:
                return new MarketDieConfig { Type = type, BuyPrice = 10, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 8, WeightChapter6To10 = 9 };
            case DieType.RootTree:
                return new MarketDieConfig { Type = type, BuyPrice = 11, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 8, WeightChapter6To10 = 9 };
            case DieType.Gambler:
                return new MarketDieConfig { Type = type, BuyPrice = 10, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 5, WeightChapter6To10 = 8 };
            case DieType.Treasury:
                return new MarketDieConfig { Type = type, BuyPrice = 8, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 5, WeightChapter3To5 = 9, WeightChapter6To10 = 7 };
            case DieType.Bribe:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 5, WeightChapter3To5 = 8, WeightChapter6To10 = 8 };
            case DieType.Investment:
                return new MarketDieConfig { Type = type, BuyPrice = 10, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 5, WeightChapter6To10 = 7 };
            case DieType.BountyGold:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 12, WeightChapter3To5 = 13, WeightChapter6To10 = 10 };
            case DieType.TopGold:
                return new MarketDieConfig { Type = type, BuyPrice = 8, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 13, WeightChapter3To5 = 13, WeightChapter6To10 = 10 };
            case DieType.HandTax:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 12, WeightChapter3To5 = 13, WeightChapter6To10 = 10 };
            case DieType.Collection:
                return new MarketDieConfig { Type = type, BuyPrice = 8, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 12, WeightChapter3To5 = 12, WeightChapter6To10 = 8 };
            case DieType.CompoundInterest:
                return new MarketDieConfig { Type = type, BuyPrice = 10, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 9, WeightChapter6To10 = 8 };
            case DieType.LeadTicket:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 0, WeightChapter3To5 = 10, WeightChapter6To10 = 10 };
            case DieType.ShellTax:
                return new MarketDieConfig { Type = type, BuyPrice = 9, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 11, WeightChapter3To5 = 12, WeightChapter6To10 = 10 };
            case DieType.CounterGold:
                return new MarketDieConfig { Type = type, BuyPrice = 8, SellPrice = 4, Tier = "T2", WeightChapter1To2 = 12, WeightChapter3To5 = 12, WeightChapter6To10 = 10 };
            case DieType.LumberGold:
                return new MarketDieConfig { Type = type, BuyPrice = 10, SellPrice = 5, Tier = "T3", WeightChapter1To2 = 0, WeightChapter3To5 = 9, WeightChapter6To10 = 9 };
        }

        return new MarketDieConfig { Type = DieType.Basic, BuyPrice = 3, SellPrice = 1, Tier = "T0", WeightChapter1To2 = 0, WeightChapter3To5 = 0, WeightChapter6To10 = 0 };
    }

    private DiceMaterialConfig DefaultDiceMaterialConfig(DiceMaterial material)
    {
        switch (material)
        {
            case DiceMaterial.OfficialIron:
                return new DiceMaterialConfig { Material = material, DisplayName = "官铁", PriceModifier = 2, SellModifier = 0, WeightChapter1To2 = 28, WeightChapter3To5 = 24, WeightChapter6To10 = 22, ShortRule = "单骰分 +1" };
            case DiceMaterial.GiltSeal:
                return new DiceMaterialConfig { Material = material, DisplayName = "鎏印", PriceModifier = 1, SellModifier = 0, WeightChapter1To2 = 22, WeightChapter3To5 = 20, WeightChapter6To10 = 18, ShortRule = "最高面钱包 +1 金" };
            case DiceMaterial.ClearGlaze:
                return new DiceMaterialConfig { Material = material, DisplayName = "明釉", PriceModifier = 2, SellModifier = 0, WeightChapter1To2 = 12, WeightChapter3To5 = 18, WeightChapter6To10 = 18, ShortRule = "最高面单骰分 +2" };
            case DiceMaterial.LeadSeal:
                return new DiceMaterialConfig { Material = material, DisplayName = "铅封", PriceModifier = 1, SellModifier = 0, WeightChapter1To2 = 0, WeightChapter3To5 = 14, WeightChapter6To10 = 18, ShortRule = "出千确认后 +2" };
            case DiceMaterial.CopperBone:
                return new DiceMaterialConfig { Material = material, DisplayName = "铜骨", PriceModifier = 2, SellModifier = 0, WeightChapter1To2 = 10, WeightChapter3To5 = 20, WeightChapter6To10 = 24, ShortRule = "同点伙伴时 +1" };
        }

        return new DiceMaterialConfig { Material = DiceMaterial.None, DisplayName = string.Empty, PriceModifier = 0, SellModifier = 0, WeightChapter1To2 = 0, WeightChapter3To5 = 0, WeightChapter6To10 = 0, ShortRule = string.Empty };
    }

    private MarketRuleConfig DefaultMarketRuleConfig(int chapterFrom, int chapterTo, int normalRefreshCost, int bossRefreshCost, int highTierPityRefreshes)
    {
        return new MarketRuleConfig
        {
            ChapterFrom = chapterFrom,
            ChapterTo = chapterTo,
            NormalRefreshCost = normalRefreshCost,
            BossRefreshCost = bossRefreshCost,
            HighTierPityRefreshes = highTierPityRefreshes
        };
    }

    private void BuildEncounters()
    {
        encounters.Clear();
        if (!TryLoadEncountersFromCsv())
        {
            BuildFallbackEncounters();
        }
    }

    private bool TryLoadEncountersFromCsv()
    {
        TextAsset table = Resources.Load<TextAsset>(EncounterTableResourcePath);
        if (table == null || string.IsNullOrEmpty(table.text))
        {
            Debug.LogWarning("DiceKingDemo: encounter table not found at Resources/" + EncounterTableResourcePath + ". Using fallback encounters.");
            return false;
        }

        string[] lines = table.text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1)
        {
            Debug.LogWarning("DiceKingDemo: encounter table is empty. Using fallback encounters.");
            return false;
        }

        List<string> headers = SplitCsvLine(lines[0]);
        Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Count; i++)
        {
            string header = headers[i].Trim();
            if (!columns.ContainsKey(header))
            {
                columns.Add(header, i);
            }
        }

        List<Encounter> loaded = new List<Encounter>();
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith("#", StringComparison.Ordinal))
            {
                continue;
            }

            List<string> fields = SplitCsvLine(lines[i]);
            int target = CsvInt(fields, columns, "target_score", 0);
            if (target <= 0)
            {
                continue;
            }

            string id = CsvValue(fields, columns, "encounter_id", "encounter_" + (loaded.Count + 1));
            string name = CsvValue(fields, columns, "encounter_name", id);
            string encounterType = CsvValue(fields, columns, "encounter_type", "normal");
            bool boss = string.Equals(encounterType, "boss", StringComparison.OrdinalIgnoreCase);
            RuleKind rule = CsvRule(fields, columns, "rule_kind");
            string ruleText = CsvValue(fields, columns, "rule_text", string.Empty);
            int chapterIndex = CsvInt(fields, columns, "chapter_index", 1);
            int stageInChapter = CsvInt(fields, columns, "stage_index_in_chapter", loaded.Count + 1);
            int normalStageCount = CsvInt(fields, columns, "normal_stage_count", 3);
            int bossCount = CsvInt(fields, columns, "boss_count", 1);
            int totalEncounterCount = CsvInt(fields, columns, "total_encounter_count", normalStageCount + bossCount);

            loaded.Add(new Encounter(id, name, target, rule, ruleText, boss, chapterIndex, stageInChapter, normalStageCount, bossCount, totalEncounterCount));
        }

        if (loaded.Count == 0)
        {
            Debug.LogWarning("DiceKingDemo: encounter table had no valid encounters. Using fallback encounters.");
            return false;
        }

        encounters.AddRange(loaded);
        return true;
    }

    private void BuildFallbackEncounters()
    {
        encounters.Clear();
        encounters.Add(new Encounter("1-1", "1-1 柜台试投", 89, RuleKind.None, "无额外规则，检查基础骰袋。", false, 1, 1, 3, 1, 4));
        encounters.Add(new Encounter("1-2", "1-2 账房抽查", 102, RuleKind.OddLedger, "奇数结果更有利，鼓励奇偶控制。", false, 1, 2, 3, 1, 4));
        encounters.Add(new Encounter("1-3", "1-3 印章复核", 115, RuleKind.HandAudit, "三同、顺子等有效牌型额外 +2 分。", false, 1, 3, 3, 1, 4));
        encounters.Add(new Encounter("1-boss", "1-boss 第1章总审", 130, RuleKind.DoubleJudge, "偶数 +1、奇数 -1；顺子额外 +4。", true, 1, 4, 3, 1, 4));
    }

    private Encounter CurrentEncounter()
    {
        if (encounters.Count == 0)
        {
            return null;
        }

        return encounters[Mathf.Clamp(stageIndex, 0, encounters.Count - 1)];
    }

    private bool HasNextEncounter()
    {
        return stageIndex + 1 < encounters.Count;
    }

    private int CurrentChapterIndex()
    {
        Encounter encounter = CurrentEncounter();
        return encounter != null ? Mathf.Max(1, encounter.ChapterIndex) : 1;
    }

    private static string CsvValue(List<string> fields, Dictionary<string, int> columns, string name, string fallback)
    {
        int index;
        if (!columns.TryGetValue(name, out index) || index < 0 || index >= fields.Count)
        {
            return fallback;
        }

        return fields[index].Trim();
    }

    private static Dictionary<string, int> CsvColumns(List<string> headers)
    {
        Dictionary<string, int> columns = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < headers.Count; i++)
        {
            string header = headers[i].Trim();
            if (!columns.ContainsKey(header))
            {
                columns.Add(header, i);
            }
        }

        return columns;
    }

    private static bool CsvDieType(List<string> fields, Dictionary<string, int> columns, string name, out DieType type)
    {
        type = DieType.Basic;
        string value = CsvValue(fields, columns, name, string.Empty);
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return TryParseDieType(value, out type);
    }

    private static bool CsvDiceMaterial(List<string> fields, Dictionary<string, int> columns, string name, out DiceMaterial material)
    {
        material = DiceMaterial.None;
        string value = CsvValue(fields, columns, name, string.Empty);
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        return TryParseDiceMaterial(value, out material);
    }

    private static bool CsvAffixSlot(List<string> fields, Dictionary<string, int> columns, string name, out AffixSlot slot)
    {
        slot = AffixSlot.Prefix;
        string value = CsvValue(fields, columns, name, string.Empty);
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        string key = value.Trim().ToLowerInvariant();
        if (key == "prefix" || key == "前缀")
        {
            slot = AffixSlot.Prefix;
            return true;
        }

        if (key == "suffix" || key == "后缀")
        {
            slot = AffixSlot.Suffix;
            return true;
        }

        return false;
    }

    private static int CsvInt(List<string> fields, Dictionary<string, int> columns, string name, int fallback)
    {
        int parsed;
        if (int.TryParse(CsvValue(fields, columns, name, string.Empty), out parsed))
        {
            return parsed;
        }

        return fallback;
    }

    private static bool CsvBool(List<string> fields, Dictionary<string, int> columns, string name, bool fallback)
    {
        string value = CsvValue(fields, columns, name, fallback ? "true" : "false").Trim().ToLowerInvariant();
        switch (value)
        {
            case "1":
            case "true":
            case "yes":
            case "y":
            case "enabled":
            case "on":
            case "是":
            case "启用":
            case "已启用":
                return true;
            case "0":
            case "false":
            case "no":
            case "n":
            case "disabled":
            case "off":
            case "否":
            case "禁用":
            case "未启用":
                return false;
        }

        return fallback;
    }

    private static bool CsvFloat(List<string> fields, Dictionary<string, int> columns, string name, out float value)
    {
        string raw = CsvValue(fields, columns, name, string.Empty);
        if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        return float.TryParse(raw, out value);
    }

    private static float ClampConfigFloat(float value, float min, float max, float fallback)
    {
        if (float.IsNaN(value) || float.IsInfinity(value))
        {
            return fallback;
        }

        return Mathf.Clamp(value, min, max);
    }

    private static string ConfigFloatText(float value)
    {
        return value.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static RuleKind CsvRule(List<string> fields, Dictionary<string, int> columns, string name)
    {
        string value = CsvValue(fields, columns, name, "None");
        if (string.IsNullOrEmpty(value))
        {
            return RuleKind.None;
        }

        try
        {
            return (RuleKind)Enum.Parse(typeof(RuleKind), value, true);
        }
        catch (ArgumentException)
        {
            return RuleKind.None;
        }
    }

    private static List<string> SplitCsvLine(string line)
    {
        List<string> fields = new List<string>();
        StringBuilder current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (c == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    current.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(current.ToString());
                current.Length = 0;
            }
            else
            {
                current.Append(c);
            }
        }

        fields.Add(current.ToString());
        return fields;
    }

    private Die NewDie(string name, DieType type, int[] faces)
    {
        return new Die
        {
            Id = nextDieId++,
            Name = name,
            Type = type,
            Material = DiceMaterial.None,
            Faces = (int[])faces.Clone(),
            LastValue = 0,
            EffectiveValue = 0,
            Score = 0,
            Growth = 0,
            TargetFace = 0,
            PatternTarget = TreePatternTarget.None,
            GamblerThreshold = 0,
            InvestmentGold = 0,
            LoneWitnessRerolled = false,
            LoneWitnessPreviousValue = 0,
            HalfStepBorrowed = false,
            CheatRerolledThisSettle = false,
            CheatPreviousValue = 0,
            ParityCompleteUsed = false,
            ParityReviewRerolled = false,
            ParityReviewPreviousValue = 0,
            TypeTriggeredThisSettle = false,
            Temporary = false,
            RoundNote = string.Empty
        };
    }

    private Die NewTemporaryDie(string name, int value)
    {
        return new Die
        {
            Id = -1,
            Name = name,
            Type = DieType.Basic,
            Material = DiceMaterial.None,
            Faces = new int[] { value, value, value, value, value, value },
            LastValue = value,
            EffectiveValue = value,
            Score = 0,
            Growth = 0,
            TargetFace = 0,
            PatternTarget = TreePatternTarget.None,
            GamblerThreshold = 0,
            InvestmentGold = 0,
            LoneWitnessRerolled = false,
            LoneWitnessPreviousValue = 0,
            HalfStepBorrowed = false,
            CheatRerolledThisSettle = false,
            CheatPreviousValue = 0,
            ParityCompleteUsed = false,
            ParityReviewRerolled = false,
            ParityReviewPreviousValue = 0,
            TypeTriggeredThisSettle = false,
            Temporary = true,
            RoundNote = string.Empty
        };
    }

    private void AddDie(Die die)
    {
        if (dice.Count < DiceCapacity)
        {
            dice.Add(die);
        }
    }

    private void EnsureDiceLimit()
    {
        while (dice.Count > DiceCapacity)
        {
            dice.RemoveAt(dice.Count - 1);
        }
    }

    private void ResetRoundState(Die die)
    {
        die.LastValue = 0;
        die.EffectiveValue = 0;
        die.Score = 0;
        die.TargetFace = 0;
        die.PatternTarget = TreePatternTarget.None;
        die.GamblerThreshold = 0;
        die.LoneWitnessRerolled = false;
        die.LoneWitnessPreviousValue = 0;
        die.HalfStepBorrowed = false;
        die.CheatRerolledThisSettle = false;
        die.CheatPreviousValue = 0;
        die.ParityCompleteUsed = false;
        die.ParityReviewRerolled = false;
        die.ParityReviewPreviousValue = 0;
        die.TypeTriggeredThisSettle = false;
        die.RoundNote = string.Empty;
    }

    private void AssignRoundTarget(Die die)
    {
        if (die.Type == DieType.Piggy)
        {
            die.TargetFace = UnityEngine.Random.Range(1, 7);
            die.RoundNote = "目标 " + die.TargetFace;
        }
        else if (die.Type == DieType.BountyGold)
        {
            die.TargetFace = RandomFaceFromCurrentFaces(die);
            die.RoundNote = "悬赏 " + die.TargetFace;
        }
        else if (die.Type == DieType.PointSeedTree)
        {
            die.TargetFace = RandomFaceFromCurrentFaces(die);
            die.RoundNote = "籽" + die.TargetFace;
        }
        else if (die.Type == DieType.PatternTree)
        {
            die.PatternTarget = RandomTreePatternTarget();
            die.RoundNote = "谱:" + TreePatternTargetShortText(die.PatternTarget);
        }
        else if (die.Type == DieType.Tree)
        {
            die.TargetFace = UnityEngine.Random.Range(1, 7);
            die.RoundNote = "命中 " + die.TargetFace;
        }
        else if (die.Type == DieType.Gambler)
        {
            die.GamblerThreshold = UnityEngine.Random.Range(1, 6);
            die.RoundNote = "阈值 " + die.GamblerThreshold;
        }
    }

    private int RandomFaceFromCurrentFaces(Die die)
    {
        if (die == null || die.Faces == null || die.Faces.Length == 0)
        {
            return UnityEngine.Random.Range(1, 7);
        }

        List<int> faces = new List<int>();
        for (int i = 0; i < die.Faces.Length; i++)
        {
            int face = Mathf.Max(1, die.Faces[i]);
            if (!faces.Contains(face))
            {
                faces.Add(face);
            }
        }

        if (faces.Count == 0)
        {
            return UnityEngine.Random.Range(1, 7);
        }

        return faces[UnityEngine.Random.Range(0, faces.Count)];
    }

    private TreePatternTarget RandomTreePatternTarget()
    {
        int roll = UnityEngine.Random.Range(0, 4);
        switch (roll)
        {
            case 0:
                return TreePatternTarget.ThreeKind;
            case 1:
                return TreePatternTarget.Straight;
            case 2:
                return TreePatternTarget.AllOdd;
            default:
                return TreePatternTarget.AllEven;
        }
    }

    private string TreePatternTargetShortText(TreePatternTarget target)
    {
        switch (target)
        {
            case TreePatternTarget.ThreeKind:
                return "三";
            case TreePatternTarget.Straight:
                return "顺";
            case TreePatternTarget.AllOdd:
                return "奇";
            case TreePatternTarget.AllEven:
                return "偶";
        }

        return "?";
    }

    private int[] DefaultFaces(DieType type)
    {
        switch (type)
        {
            case DieType.Odd:
            case DieType.LoneWitness:
            case DieType.Stamp:
                return new int[] { 1, 1, 3, 3, 5, 5 };
            case DieType.Even:
            case DieType.HalfStep:
            case DieType.Track:
                return new int[] { 2, 2, 4, 4, 6, 6 };
        }

        return new int[] { 1, 2, 3, 4, 5, 6 };
    }

    private string DefaultName(DieType type)
    {
        switch (type)
        {
            case DieType.Piggy:
                return "猪猪骰";
            case DieType.Turtle:
                return "龟龟骰";
            case DieType.Shellsmith:
                return "壳匠骰";
            case DieType.Nest:
                return "巢穴骰";
            case DieType.SlowTurtle:
                return "慢龟骰";
            case DieType.Double:
                return "双倍骰";
            case DieType.Odd:
                return "奇数骰";
            case DieType.Even:
                return "偶数骰";
            case DieType.LoneWitness:
                return "孤证骰";
            case DieType.Stamp:
                return "盖章骰";
            case DieType.HalfStep:
                return "半步骰";
            case DieType.Track:
                return "轨道骰";
            case DieType.ParityNeighborDiff:
                return "异邻骰";
            case DieType.ParityNeighborSame:
                return "同邻骰";
            case DieType.ParityComplete:
                return "补全骰";
            case DieType.ParityReview:
                return "复核骰";
            case DieType.ParityFlipScore:
                return "翻号骰";
            case DieType.ParityHoldScore:
                return "守号骰";
            case DieType.ParityTurner:
                return "转号骰";
            case DieType.Tree:
                return "大树骰";
            case DieType.Gardener:
                return "园丁骰";
            case DieType.Irrigation:
                return "灌溉骰";
            case DieType.PointSeedTree:
                return "点籽树";
            case DieType.PatternTree:
                return "牌谱树";
            case DieType.CanopyTree:
                return "冠层树";
            case DieType.RingTree:
                return "年轮树";
            case DieType.FertilizerTree:
                return "肥料树";
            case DieType.PruningTree:
                return "修枝树";
            case DieType.RootTree:
                return "根系树";
            case DieType.Gambler:
                return "赌徒骰";
            case DieType.Treasury:
                return "国库骰";
            case DieType.Bribe:
                return "贿赂骰";
            case DieType.Investment:
                return "投资骰";
            case DieType.BountyGold:
                return "悬赏骰";
            case DieType.TopGold:
                return "顶金骰";
            case DieType.HandTax:
                return "牌税骰";
            case DieType.Collection:
                return "收账骰";
            case DieType.CompoundInterest:
                return "复利骰";
            case DieType.LeadTicket:
                return "铅票骰";
            case DieType.ShellTax:
                return "壳税骰";
            case DieType.CounterGold:
                return "柜台骰";
            case DieType.LumberGold:
                return "伐木骰";
        }

        return "基础骰";
    }

    private string DieDisplayName(Die die)
    {
        if (die == null)
        {
            return string.Empty;
        }

        string material = MaterialDisplayName(die.Material);
        if (string.IsNullOrEmpty(material))
        {
            return die.Name;
        }

        return material + " " + die.Name;
    }

    private string MaterialDisplayName(DiceMaterial material)
    {
        if (material == DiceMaterial.None)
        {
            return string.Empty;
        }

        DiceMaterialConfig config;
        if (diceMaterialConfigs.TryGetValue(material, out config))
        {
            return config.DisplayName;
        }

        return DefaultDiceMaterialConfig(material).DisplayName;
    }

    private string MaterialShortRule(DiceMaterial material)
    {
        if (material == DiceMaterial.None)
        {
            return string.Empty;
        }

        DiceMaterialConfig config;
        if (diceMaterialConfigs.TryGetValue(material, out config))
        {
            return config.ShortRule;
        }

        return DefaultDiceMaterialConfig(material).ShortRule;
    }

    private int MaterialPriceModifier(DiceMaterial material)
    {
        if (material == DiceMaterial.None)
        {
            return 0;
        }

        DiceMaterialConfig config;
        if (diceMaterialConfigs.TryGetValue(material, out config))
        {
            return config.PriceModifier;
        }

        return DefaultDiceMaterialConfig(material).PriceModifier;
    }

    private string TypeName(DieType type)
    {
        switch (type)
        {
            case DieType.Piggy:
                return "猪猪";
            case DieType.Turtle:
                return "龟龟";
            case DieType.Shellsmith:
                return "壳匠";
            case DieType.Nest:
                return "巢穴";
            case DieType.SlowTurtle:
                return "慢龟";
            case DieType.Double:
                return "双倍";
            case DieType.Odd:
                return "奇数";
            case DieType.Even:
                return "偶数";
            case DieType.LoneWitness:
                return "孤证";
            case DieType.Stamp:
                return "盖章";
            case DieType.HalfStep:
                return "半步";
            case DieType.Track:
                return "轨道";
            case DieType.ParityNeighborDiff:
                return "异邻";
            case DieType.ParityNeighborSame:
                return "同邻";
            case DieType.ParityComplete:
                return "补全";
            case DieType.ParityReview:
                return "复核";
            case DieType.ParityFlipScore:
                return "翻号";
            case DieType.ParityHoldScore:
                return "守号";
            case DieType.ParityTurner:
                return "转号";
            case DieType.Tree:
                return "大树";
            case DieType.Gardener:
                return "园丁";
            case DieType.Irrigation:
                return "灌溉";
            case DieType.PointSeedTree:
                return "点籽";
            case DieType.PatternTree:
                return "牌谱";
            case DieType.CanopyTree:
                return "冠层";
            case DieType.RingTree:
                return "年轮";
            case DieType.FertilizerTree:
                return "肥料";
            case DieType.PruningTree:
                return "修枝";
            case DieType.RootTree:
                return "根系";
            case DieType.Gambler:
                return "赌徒";
            case DieType.Treasury:
                return "国库";
            case DieType.Bribe:
                return "贿赂";
            case DieType.Investment:
                return "投资";
            case DieType.BountyGold:
                return "悬赏";
            case DieType.TopGold:
                return "顶金";
            case DieType.HandTax:
                return "牌税";
            case DieType.Collection:
                return "收账";
            case DieType.CompoundInterest:
                return "复利";
            case DieType.LeadTicket:
                return "铅票";
            case DieType.ShellTax:
                return "壳税";
            case DieType.CounterGold:
                return "柜台";
            case DieType.LumberGold:
                return "伐木";
        }

        return "基础";
    }

    private string ShortTypeName(DieType type)
    {
        return TypeName(type);
    }

    private string AffixSlotSummary(Die die)
    {
        if (!affixFeatureEnabled)
        {
            return die == null ? string.Empty : MaterialDisplayName(die.Material);
        }

        if (die == null || die.Temporary)
        {
            return "无词缀";
        }

        int prefixCount = die.PrefixAffixes != null ? die.PrefixAffixes.Count : 0;
        int suffixCount = die.SuffixAffixes != null ? die.SuffixAffixes.Count : 0;
        return "前" + prefixCount + "/" + MaxPrefixAffixes + " 后" + suffixCount + "/" + MaxSuffixAffixes;
    }

    private string AffixOrRoundTag(Die die)
    {
        if (!affixFeatureEnabled || die == null || die.Temporary)
        {
            return RoundTag(die);
        }

        string affixes = AffixShortText(die);
        if (!string.IsNullOrEmpty(affixes))
        {
            return affixes;
        }

        return RoundTag(die);
    }

    private string AffixShortText(Die die)
    {
        if (!affixFeatureEnabled || die == null)
        {
            return string.Empty;
        }

        List<string> labels = new List<string>();
        AddAffixLabels(labels, die.PrefixAffixes);
        AddAffixLabels(labels, die.SuffixAffixes);
        if (labels.Count == 0)
        {
            return string.Empty;
        }

        return string.Join(" / ", labels.ToArray());
    }

    private void AddAffixLabels(List<string> labels, List<AffixInstance> affixes)
    {
        if (affixes == null)
        {
            return;
        }

        for (int i = 0; i < affixes.Count; i++)
        {
            if (affixes[i] != null)
            {
                labels.Add(AffixLabel(affixes[i]));
            }
        }
    }

    private string AffixLabel(AffixInstance affix)
    {
        if (affix == null || string.IsNullOrEmpty(affix.Key))
        {
            return string.Empty;
        }

        AffixDefinition definition;
        string displayName = affixDefinitions.TryGetValue(affix.Key, out definition) ? definition.DisplayName : affix.Key;
        return displayName + "T" + Mathf.Clamp(affix.Tier, 1, 6);
    }

    private string RoundTag(Die die)
    {
        if (die == null)
        {
            return string.Empty;
        }

        if (die.Temporary)
        {
            return "临时";
        }

        if (die.Type == DieType.Piggy && die.TargetFace > 0)
        {
            return "目标 " + die.TargetFace;
        }

        if (die.Type == DieType.BountyGold && die.TargetFace > 0)
        {
            return "悬赏 " + die.TargetFace;
        }

        if (die.Type == DieType.TopGold)
        {
            return RolledHighestFace(die) ? "顶金 +" + topGoldPerHit : "顶金";
        }

        if (die.Type == DieType.HandTax)
        {
            int gold = CurrentHandTaxGold();
            return gold > 0 ? "牌税 +" + gold : "牌税";
        }

        if (die.Type == DieType.Collection)
        {
            return die.CollectionTriggeredThisStage ? "已收账" : "收账";
        }

        if (die.Type == DieType.CompoundInterest)
        {
            int compound = CompoundInterestPreview(StageInterestPreview());
            return compound > 0 ? "复利 +" + compound : "复利";
        }

        if (die.Type == DieType.LeadTicket)
        {
            return die.CheatRerolledThisSettle ? "铅票 +" + leadTicketGold : "铅票";
        }

        if (die.Type == DieType.ShellTax)
        {
            int count = rollPhase == RollPhase.Scoring || rollPhase == RollPhase.StageClear ? lastTurtleTemporaryDieCount : previewTurtleTemporaryDieCount;
            return shellTaxThreshold > 0 && count >= shellTaxThreshold ? "壳税 +" + shellTaxGold : "壳税";
        }

        if (die.Type == DieType.CounterGold)
        {
            return "柜台";
        }

        if (die.Type == DieType.LumberGold)
        {
            return "伐木";
        }

        if (die.Type == DieType.Tree && die.TargetFace > 0)
        {
            return "命中 " + die.TargetFace;
        }

        if (die.Type == DieType.PointSeedTree && die.TargetFace > 0)
        {
            return die.EffectiveValue == die.TargetFace ? "籽中成长" : "籽 " + die.TargetFace;
        }

        if (die.Type == DieType.PatternTree)
        {
            string target = TreePatternTargetShortText(die.PatternTarget);
            return CurrentRollHasLockedResults() && PatternTreeTargetHit(die.PatternTarget, CurrentDiceHandResult()) ? "谱成成长" : "谱 " + target;
        }

        if (die.Type == DieType.CanopyTree)
        {
            return RolledHighestFace(die) ? "冠顶成长" : "冠层";
        }

        if (die.Type == DieType.RingTree)
        {
            return "年轮";
        }

        if (die.Type == DieType.FertilizerTree)
        {
            int fertilizerGrowth = FertilizerGrowthCount();
            return fertilizerGrowth > 0 ? "肥料 x" + fertilizerGrowth : "肥料";
        }

        if (die.Type == DieType.PruningTree)
        {
            if (die.TypeTriggeredThisSettle)
            {
                return "修枝成长";
            }

            return die.CheatRerolledThisSettle ? "已修枝" : "修枝";
        }

        if (die.Type == DieType.RootTree)
        {
            string triggerLabel;
            return RootTreeTriggerLabel(die, out triggerLabel) ? triggerLabel : "根系";
        }

        if (die.Type == DieType.Gambler && die.GamblerThreshold > 0)
        {
            return "阈值 " + die.GamblerThreshold;
        }

        if (die.Type == DieType.Treasury)
        {
            return "本金 +" + TreasuryScoreBonus();
        }

        if (die.Type == DieType.Bribe && previewBribeGoldCost > 0)
        {
            return "可补";
        }

        if (die.Type == DieType.Investment)
        {
            int bonus = InvestmentScoreBonus(die);
            return bonus > 0 ? "投资 +" + bonus : "投资";
        }

        if (die.Type == DieType.LoneWitness)
        {
            return die.LoneWitnessRerolled ? "孤证 " + die.LoneWitnessPreviousValue + "→" + die.EffectiveValue : "孤证";
        }

        if (die.Type == DieType.Stamp)
        {
            return HasSameEffectiveValuePartner(die) ? "盖章 6分" : "待同点";
        }

        if (die.Type == DieType.HalfStep)
        {
            return die.HalfStepBorrowed ? "借 " + die.EffectiveValue + "→" + (die.EffectiveValue - 1) : "半步";
        }

        if (die.Type == DieType.Track)
        {
            return HasTrackValuesComplete() ? "轨道 8分" : "等 2/4/6";
        }

        if (die.Type == DieType.ParityNeighborDiff)
        {
            return HasLeftParityRelation(die, false) ? "异邻 6" : "异邻";
        }

        if (die.Type == DieType.ParityNeighborSame)
        {
            return HasLeftParityRelation(die, true) ? "同邻 6" : "同邻";
        }

        if (die.Type == DieType.ParityComplete)
        {
            return die.ParityCompleteUsed ? "补全" : "补全";
        }

        if (die.Type == DieType.ParityReview)
        {
            return die.ParityReviewRerolled ? "复核 " + die.ParityReviewPreviousValue + "→" + die.EffectiveValue : "复核";
        }

        if (die.Type == DieType.ParityFlipScore)
        {
            return CheatParityChanged(die) ? "翻号 8" : "翻号";
        }

        if (die.Type == DieType.ParityHoldScore)
        {
            return CheatParityHeld(die) ? "守号 6" : "守号";
        }

        if (die.Type == DieType.ParityTurner)
        {
            return die.CheatRerolledThisSettle ? "转号" : "转号";
        }

        if (die.Type == DieType.Gardener)
        {
            int naturalHits = NaturalTreeHitCount();
            return naturalHits > 0 ? "园丁 +" + naturalHits : "园丁";
        }

        if (die.Type == DieType.Irrigation)
        {
            return FindIrrigationPreviewTarget(die) != null ? "灌溉命中" : "灌溉";
        }

        if (die.Type == DieType.Shellsmith)
        {
            int bonus = rollPhase == RollPhase.Scoring || rollPhase == RollPhase.StageClear ? lastTurtleTemporaryDieCount : previewTurtleTemporaryDieCount;
            return bonus > 0 ? "壳 +" + bonus : "壳匠";
        }

        if (die.Type == DieType.Nest)
        {
            int bonus = rollPhase == RollPhase.Scoring || rollPhase == RollPhase.StageClear ? lastNestBonusDieCount : previewNestBonusDieCount;
            return bonus > 0 ? "巢 +" + bonus : "巢穴";
        }

        if (die.Type == DieType.SlowTurtle)
        {
            return "慢壳链";
        }

        if (IsTreeGrowthType(die.Type) && die.Growth > 0)
        {
            return "成长 " + die.Growth;
        }

        return TypeName(die.Type);
    }

    private Color TypeColor(DieType type)
    {
        switch (type)
        {
            case DieType.Piggy:
                return new Color(0.9f, 0.58f, 0.48f);
            case DieType.Turtle:
                return new Color(0.38f, 0.62f, 0.46f);
            case DieType.Shellsmith:
                return new Color(0.58f, 0.62f, 0.34f);
            case DieType.Nest:
                return new Color(0.58f, 0.42f, 0.24f);
            case DieType.SlowTurtle:
                return new Color(0.34f, 0.56f, 0.42f);
            case DieType.Double:
                return new Color(0.74f, 0.54f, 0.26f);
            case DieType.Odd:
                return new Color(0.48f, 0.34f, 0.74f);
            case DieType.Even:
                return new Color(0.3f, 0.56f, 0.78f);
            case DieType.LoneWitness:
                return new Color(0.56f, 0.42f, 0.76f);
            case DieType.Stamp:
                return new Color(0.74f, 0.32f, 0.28f);
            case DieType.HalfStep:
                return new Color(0.28f, 0.62f, 0.58f);
            case DieType.Track:
                return new Color(0.24f, 0.48f, 0.72f);
            case DieType.ParityNeighborDiff:
                return new Color(0.42f, 0.38f, 0.78f);
            case DieType.ParityNeighborSame:
                return new Color(0.24f, 0.6f, 0.58f);
            case DieType.ParityComplete:
                return new Color(0.46f, 0.44f, 0.72f);
            case DieType.ParityReview:
                return new Color(0.7f, 0.48f, 0.26f);
            case DieType.ParityFlipScore:
                return new Color(0.46f, 0.34f, 0.78f);
            case DieType.ParityHoldScore:
                return new Color(0.22f, 0.5f, 0.44f);
            case DieType.ParityTurner:
                return new Color(0.34f, 0.58f, 0.72f);
            case DieType.Tree:
                return new Color(0.42f, 0.54f, 0.22f);
            case DieType.Gardener:
                return new Color(0.48f, 0.62f, 0.26f);
            case DieType.Irrigation:
                return new Color(0.24f, 0.58f, 0.68f);
            case DieType.PointSeedTree:
                return new Color(0.34f, 0.62f, 0.38f);
            case DieType.PatternTree:
                return new Color(0.44f, 0.52f, 0.3f);
            case DieType.CanopyTree:
                return new Color(0.28f, 0.58f, 0.3f);
            case DieType.RingTree:
                return new Color(0.58f, 0.42f, 0.22f);
            case DieType.FertilizerTree:
                return new Color(0.54f, 0.46f, 0.22f);
            case DieType.PruningTree:
                return new Color(0.62f, 0.38f, 0.24f);
            case DieType.RootTree:
                return new Color(0.26f, 0.54f, 0.42f);
            case DieType.Gambler:
                return new Color(0.72f, 0.28f, 0.28f);
            case DieType.Treasury:
                return new Color(0.82f, 0.62f, 0.18f);
            case DieType.Bribe:
                return new Color(0.62f, 0.46f, 0.24f);
            case DieType.Investment:
                return new Color(0.28f, 0.62f, 0.45f);
            case DieType.BountyGold:
                return new Color(0.86f, 0.5f, 0.22f);
            case DieType.TopGold:
                return new Color(0.92f, 0.68f, 0.2f);
            case DieType.HandTax:
                return new Color(0.64f, 0.48f, 0.32f);
            case DieType.Collection:
                return new Color(0.78f, 0.58f, 0.3f);
            case DieType.CompoundInterest:
                return new Color(0.36f, 0.66f, 0.42f);
            case DieType.LeadTicket:
                return new Color(0.5f, 0.5f, 0.46f);
            case DieType.ShellTax:
                return new Color(0.42f, 0.66f, 0.52f);
            case DieType.CounterGold:
                return new Color(0.7f, 0.52f, 0.28f);
            case DieType.LumberGold:
                return new Color(0.58f, 0.46f, 0.24f);
        }

        return new Color(0.68f, 0.64f, 0.55f);
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

    private void MarkTypeTriggered(Die die)
    {
        if (die == null || die.Temporary || die.Type == DieType.RootTree)
        {
            return;
        }

        die.TypeTriggeredThisSettle = true;
    }

    private string FaceText(int[] faces)
    {
        StringBuilder result = new StringBuilder();
        for (int i = 0; i < faces.Length; i++)
        {
            if (i > 0)
            {
                result.Append("/");
            }

            result.Append(faces[i]);
        }

        return result.ToString();
    }

    private Texture2D DieTypeIcon(DieType type)
    {
        LoadDieTypeIcons();
        Texture2D icon;
        dieTypeIconTextures.TryGetValue(type, out icon);
        return icon;
    }

    private void LoadDieTypeIcons()
    {
        if (dieTypeIconsLoaded)
        {
            return;
        }

        dieTypeIconsLoaded = true;
        dieTypeIconTextures.Clear();
        Array values = Enum.GetValues(typeof(DieType));
        for (int i = 0; i < values.Length; i++)
        {
            DieType type = (DieType)values.GetValue(i);
            string path = DiceTypeIconResourcePrefix + DieTypeIconFileName(type);
            Texture2D icon = Resources.Load<Texture2D>(path);
            if (icon == null)
            {
                Debug.LogWarning("DiceKingDemo: dice type icon not found at Resources/" + path + ". Falling back to procedural die face.");
                continue;
            }

            dieTypeIconTextures[type] = icon;
        }
    }

    private void LoadDiceMaterialRenderMaterials()
    {
        if (diceMaterialOverlayShader == null)
        {
            diceMaterialOverlayShader = Resources.Load<Shader>(DiceMaterialShaderResourcePath);
            if (diceMaterialOverlayShader == null)
            {
                diceMaterialOverlayShader = Shader.Find("DiceKing/DiceMaterialOverlay");
            }
        }

        if (diceMaterialOverlayShader == null || diceMaterialRenderMaterials.Count > 0)
        {
            return;
        }

        diceMaterialRenderMaterials[DiceMaterial.OfficialIron] = CreateDiceMaterialRenderMaterial(DiceMaterial.OfficialIron, new Color(0.55f, 0.6f, 0.62f, 0.72f), new Color(0.88f, 0.96f, 1f, 1f), 0f, 0.28f, 0.22f, 0f);
        diceMaterialRenderMaterials[DiceMaterial.GiltSeal] = CreateDiceMaterialRenderMaterial(DiceMaterial.GiltSeal, new Color(0.86f, 0.56f, 0.12f, 0.72f), new Color(1f, 0.88f, 0.42f, 1f), 1f, 0.34f, 0.32f, 0.16f);
        diceMaterialRenderMaterials[DiceMaterial.ClearGlaze] = CreateDiceMaterialRenderMaterial(DiceMaterial.ClearGlaze, new Color(0.68f, 0.86f, 0.96f, 0.48f), new Color(1f, 1f, 0.95f, 1f), 2f, 0.38f, 0.28f, 0.08f);
        diceMaterialRenderMaterials[DiceMaterial.LeadSeal] = CreateDiceMaterialRenderMaterial(DiceMaterial.LeadSeal, new Color(0.34f, 0.33f, 0.32f, 0.78f), new Color(0.68f, 0.65f, 0.58f, 1f), 3f, 0.26f, 0.18f, 0f);
        diceMaterialRenderMaterials[DiceMaterial.CopperBone] = CreateDiceMaterialRenderMaterial(DiceMaterial.CopperBone, new Color(0.74f, 0.42f, 0.22f, 0.7f), new Color(1f, 0.7f, 0.36f, 1f), 4f, 0.34f, 0.28f, 0.04f);
    }

    private UnityEngine.Material CreateDiceMaterialRenderMaterial(DiceMaterial material, Color tint, Color highlight, float patternMode, float patternStrength, float rimStrength, float pulseStrength)
    {
        UnityEngine.Material renderMaterial = new UnityEngine.Material(diceMaterialOverlayShader);
        renderMaterial.name = "DiceMaterial_" + material;
        renderMaterial.hideFlags = HideFlags.HideAndDontSave;
        renderMaterial.SetColor("_Tint", tint);
        renderMaterial.SetColor("_Highlight", highlight);
        renderMaterial.SetFloat("_PatternMode", patternMode);
        renderMaterial.SetFloat("_PatternStrength", patternStrength);
        renderMaterial.SetFloat("_RimStrength", rimStrength);
        renderMaterial.SetFloat("_PulseStrength", pulseStrength);
        renderMaterial.SetFloat("_Alpha", 1f);
        return renderMaterial;
    }

    private string DieTypeIconFileName(DieType type)
    {
        switch (type)
        {
            case DieType.Piggy:
                return "piggy_die_icon";
            case DieType.Turtle:
                return "turtle_die_icon";
            case DieType.Shellsmith:
                return "shellsmith_die_icon";
            case DieType.Nest:
                return "nest_die_icon";
            case DieType.SlowTurtle:
                return "slow_turtle_die_icon";
            case DieType.Double:
                return "double_die_icon";
            case DieType.Odd:
                return "odd_die_icon";
            case DieType.Even:
                return "even_die_icon";
            case DieType.LoneWitness:
                return "lone_witness_die_icon";
            case DieType.Stamp:
                return "stamp_die_icon";
            case DieType.HalfStep:
                return "half_step_die_icon";
            case DieType.Track:
                return "track_die_icon";
            case DieType.ParityNeighborDiff:
                return "parity_neighbor_diff_die_icon";
            case DieType.ParityNeighborSame:
                return "parity_neighbor_same_die_icon";
            case DieType.ParityComplete:
                return "parity_complete_die_icon";
            case DieType.ParityReview:
                return "parity_review_die_icon";
            case DieType.ParityFlipScore:
                return "parity_flip_score_die_icon";
            case DieType.ParityHoldScore:
                return "parity_hold_score_die_icon";
            case DieType.ParityTurner:
                return "parity_turner_die_icon";
            case DieType.Tree:
                return "tree_die_icon";
            case DieType.Gardener:
                return "gardener_die_icon";
            case DieType.Irrigation:
                return "irrigation_die_icon";
            case DieType.PointSeedTree:
                return "point_seed_tree_die_icon";
            case DieType.PatternTree:
                return "pattern_tree_die_icon";
            case DieType.CanopyTree:
                return "canopy_tree_die_icon";
            case DieType.RingTree:
                return "ring_tree_die_icon";
            case DieType.FertilizerTree:
                return "fertilizer_tree_die_icon";
            case DieType.PruningTree:
                return "pruning_tree_die_icon";
            case DieType.RootTree:
                return "root_tree_die_icon";
            case DieType.Gambler:
                return "gambler_die_icon";
            case DieType.Treasury:
                return "treasury_die_icon";
            case DieType.Bribe:
                return "bribe_die_icon";
            case DieType.Investment:
                return "investment_die_icon";
            case DieType.BountyGold:
                return "bounty_gold_die_icon";
            case DieType.TopGold:
                return "top_gold_die_icon";
            case DieType.HandTax:
                return "hand_tax_die_icon";
            case DieType.Collection:
                return "collection_die_icon";
            case DieType.CompoundInterest:
                return "compound_interest_die_icon";
            case DieType.LeadTicket:
                return "lead_ticket_die_icon";
            case DieType.ShellTax:
                return "shell_tax_die_icon";
            case DieType.CounterGold:
                return "counter_gold_die_icon";
            case DieType.LumberGold:
                return "lumber_gold_die_icon";
        }

        return "basic_die_icon";
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

        if (runtimeDieFaceBaseTexture == null)
        {
            runtimeDieFaceBaseTexture = Resources.Load<Texture2D>(RuntimeDieFaceBaseResourcePath);
        }

        LoadDiceRollTextures();
        LoadUiTextures();
        LoadTooltipUiTextures();
        LoadCraftingItemTextures();
        LoadDieTypeIcons();
        LoadDiceMaterialRenderMaterials();

        if (titleStyle != null && phaseRibbonStyle != null)
        {
            return;
        }

        uiFont = Font.CreateDynamicFontFromOSFont(new string[] { "Microsoft YaHei", "SimHei", "Arial" }, 18);

        titleStyle = NewStyle(34, FontStyle.Bold, new Color(0.92f, 0.84f, 0.65f));
        hudTitleStyle = NewStyle(34, FontStyle.Bold, new Color(0.96f, 0.88f, 0.68f));
        hudHeaderStyle = NewStyle(21, FontStyle.Bold, new Color(0.96f, 0.88f, 0.68f));
        hudBodyStyle = NewStyle(17, FontStyle.Bold, new Color(0.92f, 0.82f, 0.62f));
        headerStyle = NewStyle(22, FontStyle.Bold, new Color(0.23f, 0.17f, 0.13f));
        bodyStyle = NewStyle(18, FontStyle.Normal, new Color(0.28f, 0.2f, 0.15f));
        smallStyle = NewStyle(15, FontStyle.Normal, new Color(0.36f, 0.29f, 0.22f));
        tinyStyle = NewStyle(12, FontStyle.Normal, new Color(0.46f, 0.36f, 0.28f));
        cardTitleStyle = NewStyle(20, FontStyle.Bold, new Color(0.48f, 0.3f, 0.1f));
        centerStyle = NewStyle(18, FontStyle.Bold, new Color(0.23f, 0.17f, 0.13f));
        centerStyle.alignment = TextAnchor.MiddleCenter;
        phaseRibbonStyle = NewStyle(14, FontStyle.Bold, new Color(0.24f, 0.17f, 0.11f));
        phaseRibbonStyle.alignment = TextAnchor.MiddleCenter;
        tooltipTitleStyle = NewStyle(17, FontStyle.Bold, new Color(0.22f, 0.15f, 0.1f));
        tooltipTitleStyle.clipping = TextClipping.Clip;
        tooltipLabelStyle = NewStyle(13, FontStyle.Bold, new Color(0.22f, 0.15f, 0.1f));
        tooltipLabelStyle.alignment = TextAnchor.MiddleCenter;
        tooltipLabelStyle.wordWrap = true;
        tooltipBodyStyle = NewStyle(13, FontStyle.Normal, new Color(0.34f, 0.25f, 0.18f));
        tooltipBodyStyle.wordWrap = true;
        tooltipBodyStyle.clipping = TextClipping.Clip;
        tooltipTinyStyle = NewStyle(12, FontStyle.Normal, new Color(0.48f, 0.34f, 0.18f));
        tooltipTinyStyle.clipping = TextClipping.Clip;

        buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.font = uiFont;
        buttonStyle.fontSize = 17;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.normal.textColor = new Color(0.24f, 0.16f, 0.1f);
        buttonStyle.hover.textColor = new Color(0.16f, 0.1f, 0.06f);
        buttonStyle.active.textColor = new Color(0.16f, 0.1f, 0.06f);
        buttonStyle.wordWrap = true;

        selectedButtonStyle = new GUIStyle(buttonStyle);
        selectedButtonStyle.normal.textColor = new Color(0.16f, 0.1f, 0.06f);

        artButtonStyle = NewStyle(17, FontStyle.Bold, new Color(0.18f, 0.11f, 0.06f));
        artButtonStyle.alignment = TextAnchor.MiddleCenter;
        disabledButtonLabelStyle = NewStyle(17, FontStyle.Bold, new Color(0.38f, 0.34f, 0.29f));
        disabledButtonLabelStyle.alignment = TextAnchor.MiddleCenter;
    }

    private void LoadDiceRollTextures()
    {
        if (diceRollReadyTexture == null)
        {
            diceRollReadyTexture = Resources.Load<Texture2D>(DiceRollReadySpriteResourcePath);
        }

        if (diceRollLoopStripTexture == null)
        {
            diceRollLoopStripTexture = Resources.Load<Texture2D>(DiceRollLoopStripResourcePath);
            if (diceRollLoopStripTexture == null)
            {
                diceRollLoopStripTexture = Resources.Load<Texture2D>(DiceRollLoopStripFallbackResourcePath);
            }
        }

        if (diceRollStopStripTexture == null)
        {
            diceRollStopStripTexture = Resources.Load<Texture2D>(DiceRollStopStripResourcePath);
            if (diceRollStopStripTexture == null)
            {
                diceRollStopStripTexture = Resources.Load<Texture2D>(DiceRollStopStripFallbackResourcePath);
            }
        }

        if (diceRollResultFacesTexture == null)
        {
            diceRollResultFacesTexture = Resources.Load<Texture2D>(DiceRollResultFacesResourcePath);
        }
    }

    private void LoadUiTextures()
    {
        if (uiIconCoinTexture == null)
        {
            uiIconCoinTexture = Resources.Load<Texture2D>(UiResourcePrefix + "ui_icon_coin");
            uiIconRefreshTexture = Resources.Load<Texture2D>(UiResourcePrefix + "ui_icon_refresh");
            uiIconSettingsTexture = Resources.Load<Texture2D>(UiResourcePrefix + "ui_icon_settings");
            uiIconCloseTexture = Resources.Load<Texture2D>(UiResourcePrefix + "ui_icon_close");
            uiIconTargetTexture = Resources.Load<Texture2D>(UiResourcePrefix + "ui_icon_target");
            uiIconSellTexture = Resources.Load<Texture2D>(UiResourcePrefix + "ui_icon_sell");
            uiIconBagTexture = Resources.Load<Texture2D>(UiResourcePrefix + "ui_icon_bag");
        }
    }

    private void LoadTooltipUiTextures()
    {
        if (tooltipPanelTexture == null)
        {
            tooltipPanelTexture = Resources.Load<Texture2D>(TooltipUiResourcePrefix + "ui_tooltip_panel_clean");
            tooltipPriceChipTexture = Resources.Load<Texture2D>(TooltipUiResourcePrefix + "ui_tooltip_price_chip");
            tooltipLabelChipBlueTexture = Resources.Load<Texture2D>(TooltipUiResourcePrefix + "ui_tooltip_label_chip_blue");
            tooltipLabelChipGreenTexture = Resources.Load<Texture2D>(TooltipUiResourcePrefix + "ui_tooltip_label_chip_green");
            tooltipFaceCellTexture = Resources.Load<Texture2D>(TooltipUiResourcePrefix + "ui_tooltip_face_cell");
        }
    }

    private void LoadCraftingItemTextures()
    {
        if (affixAddStoneTexture == null)
        {
            affixAddStoneTexture = Resources.Load<Texture2D>(ItemResourcePrefix + "affix_add_stone");
            affixRemoveStoneTexture = Resources.Load<Texture2D>(ItemResourcePrefix + "affix_remove_stone");
            affixReplaceStoneTexture = Resources.Load<Texture2D>(ItemResourcePrefix + "affix_replace_stone");
        }
    }

    private Texture2D CraftingItemIcon(string itemKey)
    {
        LoadCraftingItemTextures();
        if (string.Equals(itemKey, "affix_add_stone", StringComparison.OrdinalIgnoreCase))
        {
            return affixAddStoneTexture;
        }

        if (string.Equals(itemKey, "affix_remove_stone", StringComparison.OrdinalIgnoreCase))
        {
            return affixRemoveStoneTexture;
        }

        if (string.Equals(itemKey, "affix_replace_stone", StringComparison.OrdinalIgnoreCase))
        {
            return affixReplaceStoneTexture;
        }

        return null;
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
