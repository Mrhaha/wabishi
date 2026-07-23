using UnityEngine;

public enum MainMenuMarqueeRelightOrder
{
    DiceThenKing,
    KingThenDice,
    Random
}

[CreateAssetMenu(fileName = "main_menu_marquee_profile", menuName = "骰子王/开始界面顶箱灯牌参数")]
public sealed class MainMenuMarqueeProfile : ScriptableObject
{
    public const string ResourcePath = "Config/main_menu_marquee_profile";

    [Header("开关、循环与验收随机")]
    [Tooltip("是否启用 DICE KING 顶箱的供电波动与分区复亮事件。关闭后标题保持完整常亮。")]
    public bool Enabled = true;

    [Tooltip("完成一次事件后，是否按随机待机间隔继续循环。")]
    public bool LoopEnabled = true;

    [Tooltip("每次进入开始界面后，是否在 InitialDelay 后先演示一次。")]
    public bool PlayOnEnter = true;

    [Tooltip("锁定后每次进入开始界面使用相同随机序列，便于录屏验收；Play Mode 可按 F9 重播。")]
    public bool LockPreviewSeed = true;

    public int RandomSeed = 777;

    [Range(0.05f, 30f)]
    [Tooltip("进入开始界面后首次事件的等待时间，单位为秒。")]
    public float InitialDelay = 1.15f;

    [Tooltip("循环事件之间的随机待机范围，单位为秒。")]
    public Vector2 EventIntervalRange = new Vector2(8f, 14f);

    [Header("压暗与近灭停留")]
    [Range(0.08f, 3f)]
    [Tooltip("两段电压下坠的总时长，单位为秒。")]
    public float BrownoutDuration = 0.41f;

    [Range(0.02f, 0.8f)]
    [Tooltip("最低余亮比例；0.20 会保留暗铜灯座和极弱余辉，不形成永久坏点。")]
    public float MinimumBrightness = 0.20f;

    [Range(0f, 2f)]
    [Tooltip("到达最低亮度后的停留时间，单位为秒。")]
    public float NearOffHoldDuration = 0.22f;

    [Header("失败回亮脉冲")]
    [Range(0, 6)]
    public int FailedPulseCount = 2;

    [Tooltip("每次失败回亮的随机持续时间范围，单位为秒。")]
    public Vector2 FailedPulseDurationRange = new Vector2(0.12f, 0.16f);

    [Tooltip("每次失败回亮的随机峰值亮度范围。")]
    public Vector2 FailedPulseBrightnessRange = new Vector2(0.48f, 0.64f);

    [Header("DICE / KING 分区复亮")]
    public MainMenuMarqueeRelightOrder RelightOrder = MainMenuMarqueeRelightOrder.DiceThenKing;

    [Range(0f, 2f)]
    [Tooltip("第二分区开始复亮前相对第一分区的延迟，单位为秒。")]
    public float SectionStagger = 0.18f;

    [Range(0.05f, 2f)]
    [Tooltip("单个分区从最低亮度恢复到完整常亮的时长，单位为秒。")]
    public float RelightDuration = 0.20f;

    [Range(0f, 1f)]
    [Tooltip("复亮峰值的额外辉光强度；不改变稳定态字形或灯珠数量。")]
    public float RelightOvershoot = 0.32f;

    [Header("稳定收束")]
    [Range(0f, 2f)]
    [Tooltip("两个分区复亮后，额外辉光缓慢回落到常态的时长，单位为秒。")]
    public float SettleDuration = 0.30f;

    [Range(0f, 2f)]
    [Tooltip("所有失败脉冲与复亮辉光的统一倍率。")]
    public float GlowIntensity = 0.78f;

    [Header("输入保护、互斥与可访问性")]
    [Tooltip("发生菜单输入、覆盖确认或进入设置时，立即恢复完整常亮并延后下一次事件。")]
    public bool PauseOnInput = true;

    [Range(0f, 5f)]
    public float InputProtectionDuration = 0.50f;

    [Range(0f, 5f)]
    [Tooltip("招牌与吊灯事件之间的最短稳定间隔，单位为秒。")]
    public float MutualExclusionCooldown = 0.50f;

    [Range(0f, 1f)]
    [Tooltip("整体闪烁幅度倍率。0 为始终常亮，1 为 V2 基准。")]
    public float ReducedFlashingScale = 1f;

    public static MainMenuMarqueeProfile CreateRuntimeDefault()
    {
        MainMenuMarqueeProfile profile = CreateInstance<MainMenuMarqueeProfile>();
        profile.hideFlags = HideFlags.HideAndDontSave;
        profile.Normalize();
        return profile;
    }

    public void Normalize()
    {
        InitialDelay = Mathf.Clamp(InitialDelay, 0.05f, 30f);
        EventIntervalRange = SortRange(EventIntervalRange, 0.5f, 120f);
        BrownoutDuration = Mathf.Clamp(BrownoutDuration, 0.08f, 3f);
        MinimumBrightness = Mathf.Clamp(MinimumBrightness, 0.02f, 0.8f);
        NearOffHoldDuration = Mathf.Clamp(NearOffHoldDuration, 0f, 2f);
        FailedPulseCount = Mathf.Clamp(FailedPulseCount, 0, 6);
        FailedPulseDurationRange = SortRange(FailedPulseDurationRange, 0.04f, 1f);
        FailedPulseBrightnessRange = SortRange(FailedPulseBrightnessRange, MinimumBrightness, 1f);
        SectionStagger = Mathf.Clamp(SectionStagger, 0f, 2f);
        RelightDuration = Mathf.Clamp(RelightDuration, 0.05f, 2f);
        RelightOvershoot = Mathf.Clamp01(RelightOvershoot);
        SettleDuration = Mathf.Clamp(SettleDuration, 0f, 2f);
        GlowIntensity = Mathf.Clamp(GlowIntensity, 0f, 2f);
        InputProtectionDuration = Mathf.Clamp(InputProtectionDuration, 0f, 5f);
        MutualExclusionCooldown = Mathf.Clamp(MutualExclusionCooldown, 0f, 5f);
        ReducedFlashingScale = Mathf.Clamp01(ReducedFlashingScale);
    }

    private void OnValidate()
    {
        Normalize();
    }

    private static Vector2 SortRange(Vector2 value, float minimum, float maximum)
    {
        float min = Mathf.Clamp(Mathf.Min(value.x, value.y), minimum, maximum);
        float max = Mathf.Clamp(Mathf.Max(value.x, value.y), min, maximum);
        return new Vector2(min, max);
    }
}
