using UnityEngine;

[CreateAssetMenu(fileName = "main_menu_rain_profile", menuName = "骰子王/开始界面雨水参数")]
public sealed class MainMenuRainProfile : ScriptableObject
{
    public const string ResourcePath = "Config/main_menu_rain_profile";

    [Header("总开关与验收随机")]
    [Tooltip("是否绘制窗外持续下降的远近两层雨幕。")]
    public bool OutsideRainEnabled = true;

    [Tooltip("是否绘制玻璃撞击水滴、聚合和汇流。")]
    public bool GlassWaterEnabled = true;

    [Tooltip("开启后，每次进入开始界面都从同一随机序列开始，便于录屏和验收；关闭后每次进入都会变化。")]
    public bool LockPreviewSeed = true;

    [Tooltip("锁定验收随机时使用的种子。修改后可在 Play Mode 按 F8 重新开始预览。")]
    public int RandomSeed = 777;

    [Header("窗外雨幕：整体")]
    [Range(0f, 2f)]
    [Tooltip("远、近两层雨线数量的统一倍率。0 为关闭，1 为当前基准。")]
    public float OutsideRainDensity = 1f;

    [Range(0.25f, 3f)]
    [Tooltip("远、近两层下落速度的统一倍率。")]
    public float OutsideRainSpeed = 1f;

    [Range(-0.3f, 0.3f)]
    [Tooltip("雨线每向下移动 1 像素时的横向偏移比例；负值向左倾。")]
    public float WindDrift = -0.08f;

    [Header("窗外雨幕：远层")]
    [Range(0, 96)]
    public int FarLayerCount = 34;

    [Tooltip("远层下落速度范围，单位为 1920x1080 设计像素/秒。")]
    public Vector2 FarSpeedRange = new Vector2(170f, 240f);

    [Tooltip("远层雨线长度范围，单位为 1920x1080 设计像素。")]
    public Vector2 FarLengthRange = new Vector2(18f, 40f);

    [Tooltip("远层雨线宽度范围，单位为 1920x1080 设计像素。")]
    public Vector2 FarWidthRange = new Vector2(1f, 2f);

    [Range(0f, 1f)]
    public float FarOpacity = 0.18f;

    [Header("窗外雨幕：近层")]
    [Range(0, 96)]
    public int NearLayerCount = 18;

    [Tooltip("近层下落速度范围，单位为 1920x1080 设计像素/秒。")]
    public Vector2 NearSpeedRange = new Vector2(480f, 650f);

    [Tooltip("近层雨线长度范围，单位为 1920x1080 设计像素。")]
    public Vector2 NearLengthRange = new Vector2(50f, 105f);

    [Tooltip("近层雨线宽度范围，单位为 1920x1080 设计像素。")]
    public Vector2 NearWidthRange = new Vector2(1.5f, 3f);

    [Range(0f, 1f)]
    public float NearOpacity = 0.30f;

    [Header("玻璃水滴：撞击与聚合")]
    [Range(0.05f, 4f)]
    [Tooltip("平均每秒新增的玻璃撞击水滴数量。低于最少可见数时会临时加快补充。")]
    public float ImpactRate = 1f;

    [Tooltip("玻璃上同时可见的附着水滴数量范围。")]
    public Vector2Int ActiveDropletRange = new Vector2Int(8, 14);

    [Tooltip("同一时段分布水滴的局部区域数量范围。")]
    public Vector2Int ClusterCountRange = new Vector2Int(2, 3);

    [Tooltip("玻璃安全区内的归一化生成范围：X=左，Y=上，Z=右，W=下。")]
    public Vector4 SpawnRegionNormalized = new Vector4(0.06f, 0.06f, 0.94f, 0.72f);

    [Range(4f, 160f)]
    [Tooltip("每个局部区域内水滴散布半径，单位为 1920x1080 设计像素。")]
    public float ClusterSpread = 64f;

    [Range(0f, 1f)]
    [Tooltip("新雨滴直接撞到同一区域既有水珠附近的概率；这是接触合并的来源，不会让远处水滴互相磁吸。")]
    public float ContactImpactChance = 0.4f;

    [Tooltip("单颗水滴直径范围，单位为 1920x1080 设计像素。")]
    public Vector2 DropletSizeRange = new Vector2(7f, 14f);

    [Tooltip("未汇流水滴的保留时间范围，单位为秒。")]
    public Vector2 DropletLifetimeRange = new Vector2(8f, 18f);

    [Tooltip("水滴撞击后停留在玻璃表面的时间范围，单位为秒。")]
    public Vector2 SurfaceHoldRange = new Vector2(0.55f, 1.8f);

    [Tooltip("普通小水滴受重力缓慢下滑的速度范围，单位为 1920x1080 设计像素/秒。")]
    public Vector2 SurfaceCreepSpeedRange = new Vector2(0.35f, 1.4f);

    [Range(0f, 12f)]
    [Tooltip("两颗水滴可见边缘的额外接触容差，单位为 1920x1080 设计像素；不会作为远距离吸附半径。")]
    public float MergeRadius = 2f;

    [Range(4f, 180f)]
    [Tooltip("水滴合并变重后受重力向下滑动的速度上限，单位为 1920x1080 设计像素/秒。")]
    public float MergeSpeed = 52f;

    [Range(2, 8)]
    [Tooltip("聚合质量达到多少颗普通水滴后转为主汇流。")]
    public int FlowThreshold = 4;

    [Range(0f, 1f)]
    public float GlassOpacity = 0.72f;

    [Header("玻璃水流：长度、速度与分支")]
    [Tooltip("主汇流向下延伸速度范围，单位为 1920x1080 设计像素/秒。")]
    public Vector2 FlowSpeedRange = new Vector2(72f, 120f);

    [Tooltip("主汇流目标长度占当前位置可用玻璃高度的比例。")]
    public Vector2 FlowLengthRange = new Vector2(0.25f, 0.55f);

    [Tooltip("主汇流宽度范围，单位为 1920x1080 设计像素。")]
    public Vector2 FlowWidthRange = new Vector2(5f, 10f);

    [Range(1, 4)]
    public int MaxConcurrentFlows = 2;

    [Range(0f, 1f)]
    public float BranchChance = 0.15f;

    [Range(0f, 1f)]
    public float PauseChance = 0.30f;

    [Tooltip("汇流中途短暂停滞的时长范围，单位为秒。")]
    public Vector2 FlowPauseRange = new Vector2(0.15f, 0.48f);

    [Range(0.2f, 5f)]
    [Tooltip("主流达到目标长度后继续保留的时间。")]
    public float FlowHoldDuration = 1.6f;

    public static MainMenuRainProfile CreateRuntimeDefault()
    {
        MainMenuRainProfile profile = CreateInstance<MainMenuRainProfile>();
        profile.hideFlags = HideFlags.HideAndDontSave;
        profile.Normalize();
        return profile;
    }

    public void Normalize()
    {
        OutsideRainDensity = Mathf.Clamp(OutsideRainDensity, 0f, 2f);
        OutsideRainSpeed = Mathf.Clamp(OutsideRainSpeed, 0.25f, 3f);
        WindDrift = Mathf.Clamp(WindDrift, -0.3f, 0.3f);
        FarLayerCount = Mathf.Clamp(FarLayerCount, 0, 96);
        NearLayerCount = Mathf.Clamp(NearLayerCount, 0, 96);
        FarSpeedRange = SortRange(FarSpeedRange, 10f, 800f);
        NearSpeedRange = SortRange(NearSpeedRange, 10f, 1200f);
        FarLengthRange = SortRange(FarLengthRange, 2f, 240f);
        NearLengthRange = SortRange(NearLengthRange, 2f, 320f);
        FarWidthRange = SortRange(FarWidthRange, 0.5f, 12f);
        NearWidthRange = SortRange(NearWidthRange, 0.5f, 16f);
        FarOpacity = Mathf.Clamp01(FarOpacity);
        NearOpacity = Mathf.Clamp01(NearOpacity);
        ImpactRate = Mathf.Clamp(ImpactRate, 0.05f, 4f);
        ActiveDropletRange = SortRange(ActiveDropletRange, 1, 32);
        ClusterCountRange = SortRange(ClusterCountRange, 1, 4);

        float left = Mathf.Clamp01(Mathf.Min(SpawnRegionNormalized.x, SpawnRegionNormalized.z));
        float right = Mathf.Clamp01(Mathf.Max(SpawnRegionNormalized.x, SpawnRegionNormalized.z));
        float top = Mathf.Clamp01(Mathf.Min(SpawnRegionNormalized.y, SpawnRegionNormalized.w));
        float bottom = Mathf.Clamp01(Mathf.Max(SpawnRegionNormalized.y, SpawnRegionNormalized.w));
        SpawnRegionNormalized = new Vector4(left, top, Mathf.Max(left + 0.02f, right), Mathf.Max(top + 0.02f, bottom));
        SpawnRegionNormalized.z = Mathf.Clamp01(SpawnRegionNormalized.z);
        SpawnRegionNormalized.w = Mathf.Clamp01(SpawnRegionNormalized.w);

        ClusterSpread = Mathf.Clamp(ClusterSpread, 4f, 160f);
        ContactImpactChance = Mathf.Clamp01(ContactImpactChance);
        DropletSizeRange = SortRange(DropletSizeRange, 2f, 48f);
        DropletLifetimeRange = SortRange(DropletLifetimeRange, 1f, 60f);
        SurfaceHoldRange = SortRange(SurfaceHoldRange, 0.28f, 12f);
        SurfaceCreepSpeedRange = SortRange(SurfaceCreepSpeedRange, 0f, 60f);
        MergeRadius = Mathf.Clamp(MergeRadius, 0f, 12f);
        MergeSpeed = Mathf.Clamp(MergeSpeed, 4f, 180f);
        FlowThreshold = Mathf.Clamp(FlowThreshold, 2, 8);
        GlassOpacity = Mathf.Clamp01(GlassOpacity);
        FlowSpeedRange = SortRange(FlowSpeedRange, 10f, 500f);
        FlowLengthRange = SortRange(FlowLengthRange, 0.05f, 0.95f);
        FlowWidthRange = SortRange(FlowWidthRange, 1f, 32f);
        MaxConcurrentFlows = Mathf.Clamp(MaxConcurrentFlows, 1, 4);
        BranchChance = Mathf.Clamp01(BranchChance);
        PauseChance = Mathf.Clamp01(PauseChance);
        FlowPauseRange = SortRange(FlowPauseRange, 0f, 3f);
        FlowHoldDuration = Mathf.Clamp(FlowHoldDuration, 0.2f, 5f);
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

    private static Vector2Int SortRange(Vector2Int value, int minimum, int maximum)
    {
        int min = Mathf.Clamp(Mathf.Min(value.x, value.y), minimum, maximum);
        int max = Mathf.Clamp(Mathf.Max(value.x, value.y), min, maximum);
        return new Vector2Int(min, max);
    }
}
