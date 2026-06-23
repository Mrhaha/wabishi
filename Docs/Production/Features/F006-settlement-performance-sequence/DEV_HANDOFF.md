# F006 程序交接

状态：F006-02 / F006-03 已执行，待运行验收
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
- F006-04 的 `Space` 加速未实现，音效和震动未实现，未新增资源、数据表或存档字段。

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
| 验证与文档更新 | ACCEPTANCE.md、PROJECT_CONTEXT.md、GAME_FLOW.md | Play Mode 结果 | 验收样例记录完整 | 文档已更新，运行验收待执行 |

## 代码影响

主要影响 `Assets/Scripts/DiceKingDemo.cs` 的结算展示层。建议优先扩展这些局部：

- 结算相关状态字段：增加演出子阶段、事件列表、当前事件索引和事件计时器。
- `BeginSettle()`：真实 `ScoreDice()` 后生成展示事件。
- `UpdateScoreReveal()`：从固定步进扩展为事件驱动或在现有步进中插入五拍状态。
- `DrawTableDice()`：根据当前展示事件绘制槽位高亮、短浮字和路线反馈。
- 左端积分塔绘制：沿用 `RunScoreCounterState`，补充更强倍率脉冲或盖章效果。
- `CompleteScoreReveal()`：保持最终落点和状态转换不变。

## 数据影响

首版无数据文件变更。若后续需要配置化时长，可考虑新增表现配置，但不属于本包必做。

## 存档影响

无存档格式变更。不更新 `SaveVersion`。动画状态不进入 `PlayerPrefs`。

## 界面影响

- 结算开始有定格。
- 当前结算槽位更明确。
- 路线触发从纯日志变为投掷区短反馈。
- 倍率变化比普通基础分跳涨更重。
- 通关和未达标收束不同。

## 测试 / 验证计划

- 已完成静态检查：`git diff --check -- Assets/Scripts/DiceKingDemo.cs` 通过；花括号计数匹配。
- 本机未发现 Unity / `dotnet` / `csc` 命令，以下运行验收仍需在 Unity Play Mode 中执行。
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

无硬阻塞。`Space` 加速、音效和震动是非阻塞后置项，不进入首轮 F006-02 / F006-03。
