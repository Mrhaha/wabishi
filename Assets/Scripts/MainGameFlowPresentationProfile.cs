using UnityEngine;

[CreateAssetMenu(fileName = "main_game_flow_presentation_profile", menuName = "骰子王/主游戏流程表现参数")]
public sealed class MainGameFlowPresentationProfile : ScriptableObject
{
    public const string ResourcePath = "Config/main_game_flow_presentation_profile";

    [Header("总开关与 LED 点阵字")]
    [Tooltip("关闭后保留玩法状态推进，只停用本轮新增的主游戏流程强调动画。")]
    public bool Enabled = true;

    [Range(0f, 2f)]
    [Tooltip("LED 点阵字的外层余辉强度；不会改变骰面实体数字。")]
    public float LedGlowIntensity = 0.72f;

    [Header("投骰、停转与结果锁定")]
    [Range(0.02f, 1f)]
    public float RollIgnitionDuration = 0.18f;

    [Header("固定骰体·骰面纵向滚轴")]
    [Range(0.02f, 0.3f)]
    [Tooltip("骰面滚轴从起步速度加速到高速循环的时长，单位为秒。")]
    public float FaceReelAccelerationDuration = 0.08f;

    [Range(4f, 40f)]
    [Tooltip("高速阶段每秒经过的通用面值格数量。")]
    public float FaceReelFastCellsPerSecond = 20f;

    [Range(1f, 12f)]
    [Tooltip("最终结果格接入前，滚轴减速到的每秒面值格数量。")]
    public float FaceReelSlowCellsPerSecond = 5f;

    [Range(0.45f, 0.8f)]
    [Tooltip("单槽停转进度达到该比例后，真实结果格从骰面上缘接入。")]
    public float FaceReelLandingStart = 0.64f;

    [Range(0f, 16f)]
    [Tooltip("真实结果首次越过中心的设计像素距离，只移动骰面内容。")]
    public float FaceReelOvershootPixels = 7f;

    [Range(0f, 8f)]
    [Tooltip("真实结果越位后向上回勾的设计像素距离，只移动骰面内容。")]
    public float FaceReelReboundPixels = 2.5f;

    [Range(0f, 0.5f)]
    [Tooltip("相邻物理槽开始停转的时间差，单位为秒。")]
    public float StopSlotStagger = 0.08f;

    [Range(0.05f, 1f)]
    [Tooltip("单个物理槽从旋转到结果锁定的时长，单位为秒。")]
    public float StopSlotSettleDuration = 0.26f;

    [Range(0.05f, 1.5f)]
    public float ResultLockPulseDuration = 0.42f;

    [Header("逐骰结算与目标越线")]
    [Range(0.05f, 2f)]
    public float SettlementSlotDuration = 0.55f;

    [Range(0.05f, 2f)]
    public float SettlementRouteDuration = 0.48f;

    [Range(0.05f, 2f)]
    public float SettlementMultiplierDuration = 0.58f;

    [Range(0.05f, 2f)]
    public float SettlementFinalDuration = 0.68f;

    [Range(0.05f, 2f)]
    public float SettlementTargetDuration = 0.72f;

    [Range(0.05f, 2f)]
    [Tooltip("当前分首次越过目标分后的青白脉冲时长；脉冲期间后续结算仍继续。")]
    public float TargetCrossPulseDuration = 0.60f;

    [Header("F009 积分光晕与积分塔")]
    [Tooltip("开启已确认的六骰原槽、积分光晕直入烟熏玻璃和积分塔五档终局表现。关闭时使用旧结算回退。")]
    public bool SettlementImpactEnabled = true;

    [Range(0.05f, 0.35f)]
    [Tooltip("单次贡献事件中，骰面灯丝先向内聚合所占的时间比例。")]
    public float SettlementCondenseFraction = 0.16f;

    [Range(0.55f, 0.9f)]
    [Tooltip("单次贡献事件中，积分光晕完成玻璃吸收的时间点。精确分数在这一拍更新。")]
    public float SettlementAbsorbFraction = 0.74f;

    [Range(0.08f, 0.3f)]
    [Tooltip("飞行末段横向压窄并回正所占的轨迹比例。")]
    public float SettlementGlassCompressionFraction = 0.18f;

    [Range(8f, 28f)]
    public float SettlementHaloMinSize = 14f;

    [Range(18f, 48f)]
    public float SettlementHaloMaxSize = 34f;

    [Range(0.1f, 1.2f)]
    [Tooltip("前一击可与下一颗骰子预聚合重叠的塔内余辉时长。")]
    public float SettlementTowerAfterglowDuration = 0.46f;

    [Range(1f, 8f)]
    [Tooltip("最高档终局仅作用于积分塔局部机框的最大回坐像素。")]
    public float SettlementCriticalKickPixels = 5f;

    [Range(0f, 1f)]
    [Tooltip("程序合成的玻璃吸收、继电器和终局重击临时声音音量。仍受主音量控制。")]
    public float SettlementAudioVolume = 0.42f;

    [Header("V7-R2 五档终局导演时序")]
    [Range(0.4f, 3f)]
    [Tooltip("未达到目标时，反向泄压从启动到稳定结果的时长。")]
    public float SettlementMissFinaleDuration = 1.70f;

    [Range(0.4f, 3f)]
    [Tooltip("刚好过关时，单拍闭环从启动到稳定结果的时长。")]
    public float SettlementPassFinaleDuration = 1.55f;

    [Range(0.4f, 3f)]
    [Tooltip("超过目标时，双拍继电从启动到稳定结果的时长。")]
    public float SettlementExceedFinaleDuration = 1.65f;

    [Range(0.4f, 3f)]
    [Tooltip("远超过目标时，可控整机过载从启动到稳定结果的时长。")]
    public float SettlementFarExceedFinaleDuration = 1.85f;

    [Range(0.8f, 4f)]
    [Tooltip("最高暴击时，锁死、吸光、真空、击穿和余震完整终局的时长。")]
    public float SettlementCriticalFinaleDuration = 2.20f;

    [Range(0.2f, 0.7f)]
    [Tooltip("最高暴击归一化时间轴中，整机开始进入真空欠压的时间点。")]
    public float SettlementCriticalVacuumStart = 0.46f;

    [Range(0.3f, 0.8f)]
    [Tooltip("最高暴击归一化时间轴中，近黑与近静默达到完整强度的时间点。")]
    public float SettlementCriticalVacuumFull = 0.54f;

    [Range(0.45f, 0.9f)]
    [Tooltip("最高暴击归一化时间轴中，单次击穿释放的时间点。")]
    public float SettlementCriticalBreakthrough = 0.66f;

    [Range(0.55f, 0.98f)]
    [Tooltip("最高暴击归一化时间轴中，唯一一次次级余震的时间点。")]
    public float SettlementCriticalAftershock = 0.82f;

    [Range(0f, 0.95f)]
    [Tooltip("完整动态效果下，最高暴击真空段的全局压暗上限。")]
    public float SettlementCriticalGlobalDim = 0.82f;

    [Range(0f, 12f)]
    [Tooltip("最高暴击单次击穿时，整台竞技台相对背景的最大回坐像素。")]
    public float SettlementCriticalWorldKickPixels = 4.2f;

    [Range(0.1f, 1f)]
    [Tooltip("设置为“减少闪烁”时，击穿白闪、CRT 扰动、射线和镜头回坐的保留比例。")]
    public float SettlementReducedEffectsScale = 0.42f;

    [Header("隐藏整手强度阈值（不显示给玩家）")]
    [Min(0.01f)]
    public float SettlementPassRatio = 1f;

    [Min(0.01f)]
    public float SettlementExceedRatio = 1.5f;

    [Min(0.01f)]
    public float SettlementFarExceedRatio = 2f;

    [Min(0.01f)]
    public float SettlementCriticalRatio = 3f;

    [Header("通关三拍收入")]
    [Range(0f, 2f)]
    public float StageClearIntroHold = 0.32f;

    [Range(0.08f, 2f)]
    public float StageClearIncomeBeatDuration = 0.38f;

    [Range(0f, 1f)]
    public float StageClearIncomeBeatGap = 0.10f;

    [Range(0f, 2f)]
    public float StageClearSettleDuration = 0.24f;

    [Header("失败扫描接口")]
    [Range(0.1f, 3f)]
    [Tooltip("未来生命池允许重试时，确认失败后水平扫描复位的时长。当前不负责扣命或恢复快照。")]
    public float RetryScanDuration = 0.65f;

    [Header("本轮结束分层断电")]
    [Range(0.05f, 2f)]
    public float RunOverKeyTerminalDuration = 0.22f;

    [Range(0.05f, 2f)]
    public float RunOverDiceDuration = 0.26f;

    [Range(0.05f, 2f)]
    public float RunOverHudDuration = 0.25f;

    [Range(0.08f, 2f)]
    public float RunOverCrtCollapseDuration = 0.32f;

    [Range(0.05f, 2f)]
    public float RunOverBlackHoldDuration = 0.22f;

    public static MainGameFlowPresentationProfile CreateRuntimeDefault()
    {
        MainGameFlowPresentationProfile profile = CreateInstance<MainGameFlowPresentationProfile>();
        profile.hideFlags = HideFlags.HideAndDontSave;
        profile.Normalize();
        return profile;
    }

    public void Normalize()
    {
        LedGlowIntensity = Mathf.Clamp(LedGlowIntensity, 0f, 2f);
        RollIgnitionDuration = Mathf.Clamp(RollIgnitionDuration, 0.02f, 1f);
        FaceReelAccelerationDuration = Mathf.Clamp(FaceReelAccelerationDuration, 0.02f, 0.3f);
        FaceReelFastCellsPerSecond = Mathf.Clamp(FaceReelFastCellsPerSecond, 4f, 40f);
        FaceReelSlowCellsPerSecond = Mathf.Clamp(FaceReelSlowCellsPerSecond, 1f, Mathf.Min(12f, FaceReelFastCellsPerSecond));
        FaceReelLandingStart = Mathf.Clamp(FaceReelLandingStart, 0.45f, 0.8f);
        FaceReelOvershootPixels = Mathf.Clamp(FaceReelOvershootPixels, 0f, 16f);
        FaceReelReboundPixels = Mathf.Clamp(FaceReelReboundPixels, 0f, 8f);
        StopSlotStagger = Mathf.Clamp(StopSlotStagger, 0f, 0.5f);
        StopSlotSettleDuration = Mathf.Clamp(StopSlotSettleDuration, 0.05f, 1f);
        ResultLockPulseDuration = Mathf.Clamp(ResultLockPulseDuration, 0.05f, 1.5f);
        SettlementSlotDuration = Mathf.Clamp(SettlementSlotDuration, 0.05f, 2f);
        SettlementRouteDuration = Mathf.Clamp(SettlementRouteDuration, 0.05f, 2f);
        SettlementMultiplierDuration = Mathf.Clamp(SettlementMultiplierDuration, 0.05f, 2f);
        SettlementFinalDuration = Mathf.Clamp(SettlementFinalDuration, 0.05f, 2f);
        SettlementTargetDuration = Mathf.Clamp(SettlementTargetDuration, 0.05f, 2f);
        TargetCrossPulseDuration = Mathf.Clamp(TargetCrossPulseDuration, 0.05f, 2f);
        SettlementCondenseFraction = Mathf.Clamp(SettlementCondenseFraction, 0.05f, 0.35f);
        SettlementAbsorbFraction = Mathf.Clamp(SettlementAbsorbFraction, Mathf.Max(0.55f, SettlementCondenseFraction + 0.2f), 0.9f);
        SettlementGlassCompressionFraction = Mathf.Clamp(SettlementGlassCompressionFraction, 0.08f, 0.3f);
        SettlementHaloMinSize = Mathf.Clamp(SettlementHaloMinSize, 8f, 28f);
        SettlementHaloMaxSize = Mathf.Clamp(SettlementHaloMaxSize, Mathf.Max(18f, SettlementHaloMinSize), 48f);
        SettlementTowerAfterglowDuration = Mathf.Clamp(SettlementTowerAfterglowDuration, 0.1f, 1.2f);
        SettlementCriticalKickPixels = Mathf.Clamp(SettlementCriticalKickPixels, 1f, 8f);
        SettlementAudioVolume = Mathf.Clamp01(SettlementAudioVolume);
        SettlementMissFinaleDuration = Mathf.Clamp(SettlementMissFinaleDuration, 0.4f, 3f);
        SettlementPassFinaleDuration = Mathf.Clamp(SettlementPassFinaleDuration, 0.4f, 3f);
        SettlementExceedFinaleDuration = Mathf.Clamp(SettlementExceedFinaleDuration, 0.4f, 3f);
        SettlementFarExceedFinaleDuration = Mathf.Clamp(SettlementFarExceedFinaleDuration, 0.4f, 3f);
        SettlementCriticalFinaleDuration = Mathf.Clamp(SettlementCriticalFinaleDuration, 0.8f, 4f);
        SettlementCriticalVacuumStart = Mathf.Clamp(SettlementCriticalVacuumStart, 0.2f, 0.7f);
        SettlementCriticalVacuumFull = Mathf.Clamp(SettlementCriticalVacuumFull, SettlementCriticalVacuumStart + 0.02f, 0.8f);
        SettlementCriticalBreakthrough = Mathf.Clamp(SettlementCriticalBreakthrough, SettlementCriticalVacuumFull + 0.02f, 0.9f);
        SettlementCriticalAftershock = Mathf.Clamp(SettlementCriticalAftershock, SettlementCriticalBreakthrough + 0.04f, 0.98f);
        SettlementCriticalGlobalDim = Mathf.Clamp(SettlementCriticalGlobalDim, 0f, 0.95f);
        SettlementCriticalWorldKickPixels = Mathf.Clamp(SettlementCriticalWorldKickPixels, 0f, 12f);
        SettlementReducedEffectsScale = Mathf.Clamp(SettlementReducedEffectsScale, 0.1f, 1f);
        SettlementPassRatio = Mathf.Max(0.01f, SettlementPassRatio);
        SettlementExceedRatio = Mathf.Max(SettlementPassRatio, SettlementExceedRatio);
        SettlementFarExceedRatio = Mathf.Max(SettlementExceedRatio, SettlementFarExceedRatio);
        SettlementCriticalRatio = Mathf.Max(SettlementFarExceedRatio, SettlementCriticalRatio);
        StageClearIntroHold = Mathf.Clamp(StageClearIntroHold, 0f, 2f);
        StageClearIncomeBeatDuration = Mathf.Clamp(StageClearIncomeBeatDuration, 0.08f, 2f);
        StageClearIncomeBeatGap = Mathf.Clamp(StageClearIncomeBeatGap, 0f, 1f);
        StageClearSettleDuration = Mathf.Clamp(StageClearSettleDuration, 0f, 2f);
        RetryScanDuration = Mathf.Clamp(RetryScanDuration, 0.1f, 3f);
        RunOverKeyTerminalDuration = Mathf.Clamp(RunOverKeyTerminalDuration, 0.05f, 2f);
        RunOverDiceDuration = Mathf.Clamp(RunOverDiceDuration, 0.05f, 2f);
        RunOverHudDuration = Mathf.Clamp(RunOverHudDuration, 0.05f, 2f);
        RunOverCrtCollapseDuration = Mathf.Clamp(RunOverCrtCollapseDuration, 0.08f, 2f);
        RunOverBlackHoldDuration = Mathf.Clamp(RunOverBlackHoldDuration, 0.05f, 2f);
    }

    private void OnValidate()
    {
        Normalize();
    }
}
