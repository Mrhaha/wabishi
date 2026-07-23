# F006 程序交接

状态：V7-R3 五档结算终局与 F006-V7R3-FB-001 至 FB-005 五项表现小修均通过自动化运行验证，待用户实机体验验收
功能：F006 结算演出与槽位顺序
关联主文档：PROJECT_CONTEXT.md、GAME_FLOW.md、DICE_ARCHETYPES.md
实现事实来源：Assets/Scripts/DiceKingDemo.cs

## 输入决策

- 只改结算展示和动画节奏，不改真实计分。
- 当前实现已有 `RunScoreCounterState`、`RunScoreCounterStep`、`BeginSettle()`、`PrepareRunScoreCounterAnimation()`、`UpdateScoreReveal()`、`DrawTableDice()` 和 `DrawScoreFloat()` 等 F007 底座。
- 当前结算阶段已经使用 `scoringDice` 和当前骰袋顺序。
- F006 要在该底座上增加五拍表现和展示事件归因。
- 首轮默认不实现 `Space` 加速；若后续执行 F006-04，加速或跳过必须直接落到同一最终提交态。

## 实现目标

在 `Assets/Scripts/DiceKingDemo.cs` 当前单文件原型中，把 F007-05 的左到右跳涨扩展成更完整的结算舞台：

1. 冻结真实提交态后生成展示事件列表。
2. 在结算期播放结果定格、逐槽点名、路线小爆点、倍率盖章、目标收束。
3. 让金币、龟龟、大树、贿赂和倍率等关键事件有可读反馈。
4. 保证最终显示、钱包、成长和状态转换与真实结算一致。

## 已确认行为

- `ScoreDice()` 仍是本次真实结算的唯一结果来源。
- `resolvedScore`、`currentScore`、提交态字段和钱包变化不得因动画播放次数或加速而改变。
- 槽位顺序使用 `dice` / `scoringDice` 当前顺序，不按有效点数排序。
- 临时小骰仍遵守当前展示上限和折叠规则。
- 大树成长仍在结算完成后入档。
- 贿赂补分仍是倍率后最终补分，不进入倍率。
- 展示事件只能从本次真实提交态、`scoringDice`、每颗骰子的 `RoundNote`、提交后的计分器字段和已记录汇总字段派生。
- 事件播放、重绘、加速或跳过期间不得再次调用 `ScoreDice()`，不得重新触发龟龟随机链，不得重复写入钱包收入，不得重复扣除贿赂金币，不得提前或重复执行大树成长入档。

## 当前实现结果

- `Assets/Scripts/DiceKingDemo.cs` 已新增 `SettlementEventKind`、`SettlementHighlightLevel`、`SettlementTargetArea` 和 `SettlementDisplayEvent`。
- `BeginSettle()` 在真实 `ScoreDice()`、`CaptureCommittedRunScoreCounter()` 和 `PrepareRunScoreCounterAnimation()` 之后调用 `PrepareSettlementDisplayEvents()`，展示事件只读已提交状态。
- `UpdateScoreReveal()` 先播放展示事件；槽位入账事件调用既有 `ApplyRunScoreCounterStep()`，贿赂 / 目标收束事件调用既有 `ApplyRunScoreCounterFinal()`，最终仍由 `CompleteScoreReveal()` 处理状态转换和大树成长入档。
- `DrawTableDice()` 已接入事件驱动高亮、短标签、金币飞向左侧资源牌、折叠余骰反馈和目标收束横幅；左侧积分塔继续沿用 `RunScoreCounterState`。
- F006-04 的 `Space` 加速仍未实现；V7-R3 已新增程序生成的蓄压、击穿和余震音色，以及只作用于结算舞台的整机视觉后坐。未新增外部音频 / 贴图、数据表或存档字段。

### 2026-07-23 V7-R3 增量

- `SettlementDisplayEvent.FeedbackTier` 继续作为隐藏五档唯一表现输入；最终 `TargetSettle` 按档位读取 `SettlementMissFinaleDuration / SettlementPassFinaleDuration / SettlementExceedFinaleDuration / SettlementFarExceedFinaleDuration / SettlementCriticalFinaleDuration`。
- 五档运行时终局分别为 `1.70s / 1.55s / 1.65s / 1.85s / 2.20s`，不是把同一特效按亮度缩放：未达到反向泄压、过关单拍闭环、超过双拍继电、远超过可控整机过载、最高暴击真空击穿。
- 最高暴击的归一化相位由配置给出：真空开始 `0.46`、完全近黑 `0.54`、击穿 `0.66`、余震 `0.82`；结果文案在对应释放点后出现。
- `DrawArcadeRun()` 只在最高暴击终局对机柜内容施加位移与纵向形变，固定背景与终局覆盖层不随动；FB-005 将该变换扩展为下沉压缩、向上过冲和反向回落，避免整张图像平移读成窗口抖动。
- `DrawSettlementFinale()` 消费已经冻结的结算事件，只绘制机柜负载、托盘扫光、CRT 切片、击穿射线 / 碎片和残留；`DrawSettlementFinaleLegacy()` 保留为表现配置缺失时回退。
- `SettlementAudioCue` 增加 `Pressure / Breakthrough / Aftershock`，声音按和视觉相同的归一化节点触发；`GameplayPlaybackSpeed` 同步设置 AudioSource pitch。
- 降闪 / 降镜头运动设置通过 `SettlementReducedEffectsScale` 缩减白闪、CRT、射线和后坐，仍保留档位结构、结果信息与声音身份。

### 2026-07-23 F006-V7R3-FB-001 普通通关光晕小修

- `ActivateSettlementDisplayEvent()` 激活 `TargetSettle` 时统一清空 `mainGameTargetCrossStartedAt`、`settlementTowerAfterglowStartedAt` 和余辉权重，使提前达标、最后一枚达标与贿赂补足拥有同一终局起点。
- `DrawSettlementTargetLatch()` 对 Pass 使用独立暖色硬边目标线接触，不再绘制目标图标上的青白漫射圆光；Exceed / FarExceed / Critical 继续走原分支。
- Pass 的积分塔全高亮丝改为目标线附近 `14 px` 短接触闪光。
- `DrawSettlementFinaleResidue()` 对 Pass 只保留最多 `0.18s` 的低强度积分塔边框回声，不再绘制通用 `0.52s` 塔内填充；其它档位残留规则不变。
- 保持 Pass `1.55s`、结果揭示进度 `0.56`、Latch 音效、五档阈值、真实计分、奖励、事件顺序、数据与存档不变。
- Unity 2019.4.33f1 隔离工程已覆盖提前达标、最后一枚刚好达标、贿赂补足、三档演出速度、降效模式和 StageClear 残留；五档双分辨率全量复跑通过。证据见 `Docs/QA/20260723_f006_pass_glow_fix_runtime.md`。

### 2026-07-23 F006-V7R3-FB-002 积分塔长柱色块小修

- 根因是 BribeFinal、Exceed、FarExceed、Critical 与旧回退直接使用全高 `DrawRect`，长柱跨过积分塔 12 格灯体间隙并读成临时色块。
- 新增 `DrawSettlementTowerSegmentedEnergy()`：使用与 `DrawArcadeRunScoreMeter()` 相同的 12 格 / `4 px` 间隙几何，逐格绘制外晕、琥珀主体、温白热芯和移动继电头。
- 新增 `DrawSettlementTowerSegmentedWash()`：非 Pass 终局残留逐灯格衰减，不再用矩形填满整个塔内区域。
- BribeFinal、Exceed、FarExceed、Critical 蓄压 / 击穿和旧回退入口统一切换到分段合同；Pass 继续只使用目标线 `14 px` 短接点。
- 新增 `DrawSettlementBreakthroughShockBand()` 与 `DrawSettlementBreakthroughDischarge()`：最高档释放的水平色带改为多股热丝，垂直直线改为带断口、错位和支路的上下放电。
- 保持五档时长、Critical 真空 / 击穿 / 余震相位、结果揭示点、程序音效、降效语义、真实计分、阈值、奖励、数据和存档不变。
- Unity 2019.4.33f1 隔离工程已覆盖五档双分辨率、`0.606×` 压力图、真实 `BribeFinal` 和普通 Pass 三路径；证据见 `Docs/QA/20260723_f006_tower_energy_segmentation_runtime.md`。

### 2026-07-23 F006-V7R3-FB-003 普通结算边沿电流可读性小修

- 根因是 `DrawSettlementTowerAfterglow()` 只有低透明整框和单条 `3 px` 内部扫描线；低到中等 `LocalImpactWeight01` 时，动作存在但没有稳定可追踪的电流头。
- `DrawSettlementTowerAfterglow()` 继续保留低强度边框路径底色，把内部扫描线替换为左右双侧边沿电流。
- 新增 `DrawSettlementTowerEdgeCurrent()`：每侧绘制局部外晕、主体、温白热核和四段短尾迹；正贡献自下向上，负贡献反向向下。
- 左右边沿错开约 `0.045` 归一化进度，端点只绘制一次短接帽，避免读成静态整框发亮。
- `SettlementTowerAfterglowDuration` 仍为 `0.46s`；`TargetSettle` 激活时仍统一清除普通余辉，不侵入 Pass 单焦点或五档终局。
- 保持事件队列、计分、阈值、奖励、程序音效、数据和存档不变，未新增外部贴图或音频。
- Unity 2019.4.33f1 隔离工程已覆盖真实 `SlotScore` 双分辨率早 / 中 / 晚三相位，并复跑五档、Critical 三相位、提前达标、最后一枚达标、BribeFinal、三档演出速度与降效模式；证据见 `Docs/QA/20260723_f006_tower_edge_current_runtime.md`。

### 2026-07-23 F006-V7R3-FB-004 第 2 / 3 / 4 档区分度重构

- `DrawSettlementFinale()` 不再让 Pass / Exceed / FarExceed 共用同一套整机负载作为主视觉；三档分别改成“局部点 / 双向线 / 整机面”。
- Pass 只调用目标线机械夹合并绘制局部接触带，不再调用 `DrawSettlementCabinetLoad()` 或 `DrawSettlementTraySweeps()`。
- Exceed 新增 `DrawSettlementExceedRelay()` 与 `DrawSettlementRelayRail()`：上轨由积分塔送往右终端，下轨由右终端反向回扣积分塔；两轨均使用带断口的分段路径、移动热头和继电节点。
- FarExceed 新增 `DrawSettlementFarExceedOverload()` 与 `DrawSettlementOverloadBus()`：整机柜持续持压、12 格塔灯热芯保持、上下分段母线、一次局部轮廓回坐与末段卸压。
- Critical 的 `DrawSettlementCriticalFinale()`、真空 / 击穿 / 余震相位与全屏峰值未改；FarExceed 没有新增近黑、近静默、全屏白金覆盖、射线或碎片。
- 回退入口 `DrawSettlementFinaleLegacy()` 复用相同三档语法，避免表现配置缺失时重新串档。
- 保持五档阈值、`1.70s / 1.55s / 1.65s / 1.85s / 2.20s` 时长、结果揭示点、声音 cue、真实计分、奖励、事件、数据和存档不变。
- Unity 2019.4.33f1 隔离工程完成 13 相位、双分辨率共 26 张真实 `TargetSettle` 捕获；常规、`0.606×` 与降低特效接触板均通过视觉核对。证据见 `Docs/QA/20260723_f006_tier_distinction_runtime.md`。

### 2026-07-23 F006-V7R3-FB-005 第 5 档受力反差重构

- 根因一是旧 `SettlementPulse01()` 以击穿点为中心对称，导致主射线、白闪、扩张波与 Critical 继电在配置击穿点之前已经开始；画面文案处于锁死，视觉却提前泄压。
- 根因二是旧后坐只有短促随机横抖和轻微上移，缺少爆点前的反向位移；释放射线覆盖右半屏、上下放电等权，整体更像开闸倾泻而不是火山顶穿。
- 新增 `SettlementCriticalCrouchAmount / SettlementCriticalLaunchAmount / SettlementCriticalReboundAmount / SettlementCriticalStrikeAmount` 四条 Critical 专属单向曲线。机柜内容在真空前向下沉并纵向压缩，保持到爆点前约 `0.005` 归一化窗口；击穿后向上拉伸过冲，再反向回落。
- `SettlementCriticalStrikeAmount()` 改为单向起爆包络，爆点前为零；Critical 继电、残留与扩张波同样不再提前进入释放分支。
- `DrawSettlementCriticalCompression()` 在积分塔两侧绘制上下压力夹与小热核硬锁；`DrawSettlementBreakthroughDischarge()` 改为向上主干和短下行根部；`DrawSettlementCriticalEruption()` 使用分段喉管和上方破盖，主射线与碎片限制在向上扇区，横向冲击缩短为次级脉冲。
- Critical 音频在真空前增加一次低音量机械锁扣；击穿音减少长低频倾泻，增加短瞬态和上扬音高。近静默窗口、`2.20s` 总时长、配置击穿点 `0.66` 与结果揭示点保持不变。
- 第 1–4 档的世界位移 / 形变保持严格为零；五档阈值、其它档位画面与声音、真实计分、奖励、事件、数据和存档均未修改。
- Unity 2019.4.33f1 隔离工程完成 18 相位、双分辨率共 36 张真实 `TargetSettle` 捕获，并数值校验下蹲向下 / 纵向压缩、顶点向上 / 纵向拉伸、回落重新向下，以及降低特效身份保留。证据见 `Docs/QA/20260723_f006_critical_force_curve_runtime.md`。

## 展示事件契约

首轮建议在 `Assets/Scripts/DiceKingDemo.cs` 内新增轻量展示事件结构，字段至少覆盖：

| 字段 | 用途 |
|---|---|
| `EventKind` | 区分结果定格、槽位入账、路线小爆点、倍率盖章、贿赂补分、目标收束等事件 |
| `SlotIndex` | 当前实体骰槽位；临时小骰或汇总事件可使用来源槽位或 `-1` |
| `ScoreIndex` | 对应 `scoringDice` 中的计分顺序 |
| `DieId` | 关联实体骰或临时小骰，便于绘制高亮 |
| `Label` | 主短反馈文案，例如 `+6`、`金币 +1`、`成长入档` |
| `ValueDelta` | 分数变化或本事件展示的分数增量 |
| `GoldDelta` | 金币变化，仅用于显示，不得再次改写钱包 |
| `BaseScore` | 事件后展示用倍率前基础分 |
| `Multiplier` | 事件后展示用倍率 |
| `ProgressScore` | 事件后展示用当前累计分 |
| `Duration` | 本事件展示时长 |
| `HighlightLevel` | 普通、路线高光、倍率重击、目标收束等强度 |
| `TargetArea` | 反馈飞向或强调的区域，例如骰子、倍率章、金币资源牌、目标数字 |

首轮事件类型建议至少包含：

- 结果定格：进入结算后短暂停住当前槽位队列。
- 槽位入账：按 `scoringDice` 左到右推进基础分和当前积分。
- 金币入账：猪猪、鎏印和金币后缀等钱包来源的短反馈，只展示已提交结果。
- 临时小骰反馈：龟龟链按展示上限播放，超出部分使用汇总事件。
- 成长待入档：大树、园丁和灌溉的成长反馈，真正入档仍由结算完成流程执行。
- 倍率盖章：倍率从 `×1` 跳到更高值时强调倍率章。
- 贿赂补分：最终补分单独展示，不进入倍率。
- 目标收束：最终分滚动到 `resolvedScore`，区分通关和未达标。

事件生成必须发生在真实 `ScoreDice()` 完成之后。事件生成可以读取提交后的分数、钱包、临时小骰、成长待入档列表和骰子短注记，但不能调用会改变结果的写入分支。

## 实现任务

| 任务 | 文件 / 数据 | 依赖 | 验收 | 状态 |
|---|---|---|---|---|
| 梳理现有结算显示状态 | Assets/Scripts/DiceKingDemo.cs | F007-05 当前实现 | 明确 `scoreRevealIndex`、`scoreStepTimer`、`runScoreCounterSteps`、`finalScoreApplied` 的职责 | 已完成 |
| 新增展示事件结构 | Assets/Scripts/DiceKingDemo.cs | FEATURE_BRIEF.md D6、展示事件契约 | 事件能描述槽位、计分索引、类型、文本、数值、持续时间、目标区域和高光等级 | 已完成 |
| 生成结算展示事件 | Assets/Scripts/DiceKingDemo.cs | `ScoreDice()` 提交态、`scoringDice` | 事件列表不调用影响结果的随机或写入分支，不改变真实分数、钱包、贿赂和成长 | 已完成 |
| 接入结果定格 | Assets/Scripts/DiceKingDemo.cs | UI_UX_SPEC.md | 结算开始后有短暂停顿，骰位稳定 | 已完成 |
| 接入逐槽点名 | Assets/Scripts/DiceKingDemo.cs | UI_UX_SPEC.md | 当前骰子高亮、轻抬或脉冲，基础分同步跳涨 | 已完成 |
| 接入路线小爆点 | Assets/Scripts/DiceKingDemo.cs | GAME_FLOW.md、DICE_ARCHETYPES.md | 金币、龟龟、大树、贿赂有短反馈，低价值事件不铺满 UI | 已完成 |
| 强化倍率盖章 | Assets/Scripts/DiceKingDemo.cs | F007-05 倍率步骤 | 倍率变化时倍率章有更强反馈 | 已完成 |
| 区分目标收束 | Assets/Scripts/DiceKingDemo.cs | UI_UX_SPEC.md | 通关和未达标收束表现不同 | 已完成 |
| 可选加速 | Assets/Scripts/DiceKingDemo.cs | FEATURE_BRIEF.md U2 | 加速后最终状态正确，不重复扣钱或成长 | 已后置 |
| 验证与文档更新 | ACCEPTANCE.md、PROJECT_CONTEXT.md、GAME_FLOW.md | Play Mode 结果 | 验收样例记录完整 | 自动化运行与截图已完成，用户体验待回归 |

## 代码影响

主要影响 `Assets/Scripts/DiceKingDemo.cs` 的结算展示层。建议优先扩展这些局部：

- 结算相关状态字段：增加演出子阶段、事件列表、当前事件索引和事件计时器。
- `BeginSettle()`：真实 `ScoreDice()` 后生成展示事件。
- `UpdateScoreReveal()`：从固定步进扩展为事件驱动或在现有步进中插入五拍状态。
- `DrawTableDice()`：根据当前展示事件绘制槽位高亮、短浮字和路线反馈。
- 左端积分塔绘制：沿用 `RunScoreCounterState`，补充更强倍率脉冲或盖章效果。
- `CompleteScoreReveal()`：保持最终落点和状态转换不变。

## 数据影响

无玩法数据 CSV 变更。V7-R3 在既有 `Assets/Resources/Config/main_game_flow_presentation_profile.asset` 中新增五档终局时长、最高档相位、全局压暗、后坐像素和降效缩放；这些字段只影响表现。

## 存档影响

无存档格式变更。不更新 `SaveVersion`。动画状态不进入 `PlayerPrefs`。

## 界面影响

- 结算开始有定格。
- 当前结算槽位更明确。
- 路线触发从纯日志变为投掷区短反馈。
- 倍率变化比普通基础分跳涨更重。
- 通关和未达标收束不同。

## 测试 / 验证计划

- 已完成静态检查：`git diff --check` 通过；花括号计数匹配。
- 已使用 Unity 2019.4.33f1 隔离工程完成运行时与 Editor 脚本编译，`0 error`。
- 已使用真实 `TargetSettle` 自动覆盖五档，校验档位和时长，并在 `1920×1080 / 1280×720` 捕获五档签名与最高档三相位；证据见 `Docs/QA/20260723_f009_settlement_finale_runtime.md`。
- 已对 FB-004 追加 Pass 单扣、Exceed 去程 / 回程、FarExceed 持压 / 回坐及四档降低特效捕获；13 相位共 26 张双分辨率截图全部通过，证据见 `Docs/QA/20260723_f006_tier_distinction_runtime.md`。
- 已对 FB-005 追加 Critical 下蹲、真空、硬锁、击穿、上冲顶点、反向回落、余震与降低特效捕获；18 相位共 36 张双分辨率截图全部通过，并验证第 1–4 档不获得 Critical 世界变换，证据见 `Docs/QA/20260723_f006_critical_force_curve_runtime.md`。
- 以下金币、龟龟、成长、贿赂等专项仍需在完整构筑中回归；本轮六颗基础骰自动捕获不替代这些防重复验证。
- 基础无牌型：六颗基础骰，确认五拍流程快速但不拖慢。
- 三同 / 四同：确认倍率章在对应节点重击。
- 顺子：确认五连成立时倍率反馈正确。
- 全奇 / 全偶：确认第六颗确认后倍率反馈正确。
- 猪猪或鎏印：确认金币反馈和钱包结果一致。
- 龟龟：确认临时小骰入账和折叠反馈不改变分数。
- 大树 / 园丁 / 灌溉：确认成长反馈在分数落定后出现，成长入档正确。
- 贿赂：确认补分不进入倍率，钱包扣除和目标收束正确。
- 钱包防重复：确认播放多帧、重绘或后续加速不会重复增加猪猪、鎏印或金币后缀收入。
- 龟龟防重复：确认展示事件使用已生成的 `scoringDice` 临时小骰，不重新随机临时小骰链。
- 贿赂防重复：确认最终补分展示不会重复扣金币。
- 成长防重复：确认大树、园丁和灌溉只在结算完成流程入档一次。
- 未达标继续投骰：确认回到等待投骰后状态不乱。
- 失败：确认失败前不播放通关式收束。
- 加速如果实现：确认不重复应用分数、金币、成长和收入。

## 文档同步

| 文档 | 是否需要更新 | 已确认来源 |
|---|---|---|
| PROJECT_CONTEXT.md | 实现完成后需要 | F006 已确认方向、实现事实 |
| GAME_FLOW.md | 实现完成后需要 | F006 已确认方向、实现事实 |
| DICE_ARCHETYPES.md | 不需要 | 不改变骰子效果边界 |
| ART_ASSETS.md | 仅新增运行时资源后需要 | 首版不强制新增资源 |
| Docs/Production/Features/F006-settlement-performance-sequence/ACCEPTANCE.md | 需要 | 执行后记录验证 |

## 阻塞项

无硬阻塞。`Space` 加速和独立桌面震动保持后置；程序音效已进入 V7-R3。当前剩余的是用户对第 5 档受力反差、整手节奏、实际声压、连续游玩和降闪设置的体验回归，不阻塞代码编译。
