# F011 程序交接

状态：程序已静态接入，待 Unity 运行验证
功能：F011 大树长期成长接口骰
关联主文档：PROJECT_CONTEXT.md、GAME_FLOW.md、DICE_ARCHETYPES.md
实现事实来源：Assets/Scripts/DiceKingDemo.cs

## 输入决策

- 本包整理七颗大树重设骰；程序实现已开始，七颗已一次性静态接入。
- F011 不新增“发芽”“芽点”“成长进度”或 `GrowthProgress` 字段。
- 触发效果后直接真实成长：触发骰自身所有面整体 `+1`，现有 `Growth +1`。
- 当前实现中已有 `Tree / Gardener / Irrigation`，并使用 `Growth`、`pendingTreeGrowth`、`QueueTreeGrowths()`、`ApplyPendingTreeGrowth()`。
- 旧 `Tree / Gardener / Irrigation` 在 F011 实现后从市场下架；代码兼容可暂时保留。
- 当前结算短反馈大量写入 `RoundNote`，但根系树不应通过解析 `RoundNote` 文本判定触发。
- 当前市场数据在 `Assets/Resources/Data/dice_market_config.csv`，树路线已有 `Tree / Gardener / Irrigation` 配置。
- 当前 `starting_gold = 1000` 是测试期配置，不作为肥料树强度调参依据。
- 七颗最终图标已生成并登记到 `ART_ASSETS.md`，程序接入时直接使用 `Art/DiceTypes/point_seed_tree_die_icon`、`Art/DiceTypes/pattern_tree_die_icon`、`Art/DiceTypes/canopy_tree_die_icon`、`Art/DiceTypes/ring_tree_die_icon`、`Art/DiceTypes/fertilizer_tree_die_icon`、`Art/DiceTypes/pruning_tree_die_icon`、`Art/DiceTypes/root_tree_die_icon`。

## 实现目标

审查通过后，实现大树长期成长接口骰：

- 新增七颗大树类型。
- 旧 `Tree / Gardener / Irrigation` 市场下架，但保留代码兼容。
- 支持触发后直接真实成长。
- 预览与真实结算一致，预览不提前写入面组。
- 出千不刷新既定目标。
- 根系读取结构化类型触发标签。
- 市场数据、图标加载和文档同步完整。

## 已确认行为

- 真实成长只在二次确认后的真实结算中入档。
- 大树成长不影响本次已锁定点数、牌型或分数。
- 肥料树读取当前金币存量，不消费金币、不创造金币、不读取金币变化次数、不设单次成长次数上限。
- 肥料树默认使用 `interest_gold_step` 作为标准金币步长。
- 年轮树每次出手最多触发一次。
- 根系树每次出手最多触发一次，左右都触发也只触发一次。
- 根系树不读取材质触发、不读取无条件常驻被动、不读取基础计分或主牌型倍率。

## 实现任务

| 任务 | 文件 / 数据 | 依赖 | 验收 | 状态 |
|---|---|---|---|---|
| 新增类型 key | `Assets/Scripts/DiceKingDemo.cs` | 命名确认 | 类型可显示、可生成、可保存 | 已静态接入 |
| 旧树市场下架 | `dice_market_config.csv` / 市场生成过滤 | 下架口径 | 旧三件套不再从正式市场和测试全池刷出 | 已静态接入 |
| 类型触发标签结构 | `ScoreDice()` 附近结算流程 | 根系判定规则 | 根系不解析 `RoundNote` 文本 | 已静态接入 |
| 直接成长队列 | `QueueTreeGrowths()` / `ApplyPendingTreeGrowth()` 或新队列 | 真实成长模型 | 触发后本骰所有面 +1，`Growth +1` | 已静态接入 |
| 点籽树 | 投骰目标生成、预览、结算 | 类型 key | 命中指定点直接成长，出千不刷新目标 | 已静态接入，待运行验证 |
| 牌谱树 | 牌型目标、`EvaluateHand()` 结果读取 | 牌型目标池 | 命中目标牌型直接成长 | 已静态接入，待运行验证 |
| 冠层树 | 最高面判定 | 最高面读取 | 掷出自身最高面直接成长 | 已静态接入，待运行验证 |
| 修枝树 | 出千前后比较 | 出千状态记录 | 结果改善 / 成牌 / 通关直接成长 | 已静态接入，待运行验证 |
| 年轮树 | 真实结算结束 | 成长队列 | 每次出手一次直接成长 | 已静态接入，待运行验证 |
| 肥料树 | 真实结算结束钱包读取 | `interest_gold_step` | 按当前钱包计算成长次数，无上限 | 已静态接入，待正式经济验证 |
| 根系树 | 相邻触发标签读取 | 类型触发标签结构 | 左右相邻触发后只成长一次 | 已静态接入，待样例验证 |
| 市场配置 | `Assets/Resources/Data/dice_market_config.csv` | 价格和章节 | 新树商品可生成，旧树不再生成 | 已静态接入 |
| 图标加载 | `DieTypeIconFileName()` | 最终资源已生成 | 七颗图标可加载，缺失可回退 | 已静态接入 |
| 验证 | 手动样例 / Play Mode | 实现完成 | 预览和结算一致，保存恢复正确 | 待执行 |

## 代码影响

可能涉及：

- `DieType` 枚举。
- 目标生成和出手初始化。
- `ScoreDice()`。
- `QueueTreeGrowths()` / `ApplyPendingTreeGrowth()` 或新的直接成长队列。
- 结构化类型触发标签。
- `RoundTag()`、市场卡文案、骰袋摘要、详情文案。
- `DieTypeIconFileName()`。
- 市场路线推断 `MarketRoute.Tree`。
- 市场正式池和测试全池过滤。

不应涉及：

- 不新增 `GrowthProgress` 字段。
- 不为 F011 单独新增保存字段。
- 不因“成长进度”更新 `SaveVersion`。

## 数据影响

预计新增 `dice_market_config.csv` 行，建议默认：

| key | 买价建议 | 卖价建议 | 阶级建议 | 1-2章 | 3-5章 | 6-10章 |
|---|---:|---:|---|---:|---:|---:|
| PointSeedTree | 10 | 5 | T3 | 3 | 5 | 5 |
| PatternTree | 11 | 5 | T3 | 0 | 4 | 5 |
| CanopyTree | 10 | 5 | T3 | 0 | 5 | 6 |
| PruningTree | 10 | 5 | T3 | 0 | 4 | 6 |
| RingTree | 9 | 4 | T2 | 0 | 4 | 6 |
| FertilizerTree | 12 | 6 | T3 | 0 | 3 | 5 |
| RootTree | 11 | 5 | T3 | 0 | 4 | 6 |

旧 `Tree / Gardener / Irrigation` 在 F011 实现时应从市场生成中下架。建议方式：

- 正式市场权重置为 `0 / 0 / 0`，或从正式候选池过滤。
- 测试全池随机也必须过滤旧三件套，否则会继续刷出旧树。
- 旧类型代码和旧资源先保留，不直接删除。

肥料树不新增 `fertilizer_growth_gold_step`。默认读取 `global.csv` 的 `interest_gold_step` 作为标准金币步长。当前 `starting_gold = 1000` 是测试期配置，不能用来倒推肥料步长。

## 存档影响

- F011 不新增持久成长进度字段。
- 真实成长继续写入现有 `Faces` 和 `Growth`。
- 如果仅新增类型 key 且序列化字段不变，可以不因 F011 的成长模型单独更新 `SaveVersion`。
- 若后续决定清理旧 `Tree / Gardener / Irrigation` 存档，需另开迁移或清档策略。

## 界面影响

- 新增目标 / 状态标签。
- 需要结算短反馈和成长入档反馈。
- 肥料树需要显示多次成长，例如 `肥料成长 xN`。
- 根系需要显示相邻触发来源。
- 不显示 `芽 X/3` 或成长进度条。

## 测试 / 验证计划

静态检查：

- `git diff --check`。
- 枚举、CSV key、图标 key 一致。
- 确认没有新增 `GrowthProgress` 或发芽进度保存字段。
- 确认旧三件套不再从正式市场和测试全池刷出。

手动样例：

- 点籽命中、未命中、出千后命中。
- 牌谱目标命中三同、顺子、全奇、全偶。
- 冠层掷出自身最高面。
- 修枝出千点数变好、牌型变好、帮助通关、取消出千不触发。
- 年轮每次出手只触发一次。
- 肥料不同金币存量下直接成长对应次数，且不加金币不扣金币。
- 肥料在正式经济配置下验证，不能使用 1000 金测试开局作为强度结论。
- 根系左邻触发、右邻触发、左右都触发、相邻基础骰不触发、相邻双倍不触发、相邻材质触发不触发。
- 成长入档后保存，再继续游戏恢复。

## 文档同步

| 文档 | 是否需要更新 | 已确认来源 |
|---|---|---|
| PROJECT_CONTEXT.md | 实现后需要 | F011 进入原型事实 |
| GAME_FLOW.md | 若市场下架或结算边界变化则需要 | 直接成长和根系标签 |
| DICE_ARCHETYPES.md | 实现后需要 | 七颗正式规则 |
| ART_ASSETS.md | 已更新 | F011 图标 |
| Docs/DesignDialogues/D1-future-dice-types.md | 已需要 | F011 包路径和状态 |

## 剩余验证项

- 根系触发标签结构已落为 `TypeTriggeredThisSettle`，仍需样例验证覆盖面。
- 旧三件套市场下架已同步正式市场和测试全池，仍需实际刷新验证。
- 肥料树强度需要在正式经济配置下验证，当前 1000 金测试起手不能用于平衡结论。
- 程序实现已一次性静态接入七颗，仍需 Unity Play Mode、截图和保存恢复验证。
