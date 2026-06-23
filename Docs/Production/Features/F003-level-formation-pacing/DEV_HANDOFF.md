# F003 程序交接

状态：部分实现，待运行验证
功能：F003 关卡与成型节奏优化
关联主文档：PROJECT_CONTEXT.md、GAME_FLOW.md、DICE_ARCHETYPES.md
实现事实来源：Assets/Scripts/DiceKingDemo.cs

## 输入决策

- 首版执行切片为：数据调参、市场生成规则、经济配置、验证报告。
- 市场生成保持 3 个货架，但每次市场有 1 个隐藏轻度倾向槽位。
- 内容需要按章节渐进开放，影响骰子类型、材质和面组模板。
- 经济首版优先使用小额固定通关收入和早期低刷新成本。
- 不新增骰子、不改变结算公式、不改变存档格式。
- 用户已确认默认补丁组：首版用代码常量 / helper 表达渐进开放，不新增解锁 CSV。

## 实现目标

1. 重算或调整 `chapter_score_table.csv` 的全 40 关目标分，让章节开局有喘息，章内逐步加压。
2. 在市场生成中加入隐藏倾向槽位和章节解锁过滤。
3. 调整 `global.csv` 与 `market_rule_config.csv`，给前期经济留出稳定购买和刷新空间。
4. 输出验证报告，覆盖目标曲线、市场刷新样本、经济流水和 UI 信息量。

## 已确认行为

- 隐藏倾向槽位不显示给玩家。
- 第 1 章不能满池出现所有骰子、材质和强面组。
- 全章节都要检查目标分难度。
- 当前代码实际经济入口是 `global.csv` 的 `stage_clear_base_gold / roll_left_gold_bonus / cheat_left_gold_bonus`。
- `stage_clear_base_gold = 2`、`roll_left_gold_bonus = 0`、`cheat_left_gold_bonus = 0`。
- 第 1-2 章普通市场 `normal_refresh_cost = 1`，第 1-2 章 Boss 后章节市场 `boss_refresh_cost = 2`。
- 不做“每个市场第一次刷新免费”。
- 隐藏倾向不新增存档字段，继续游戏时按当前骰袋重新推断路线。

## 实现任务

| 任务 | 文件 / 数据 | 依赖 | 验收 | 状态 |
|---|---|---|---|---|
| 重算目标曲线 | `Assets/Resources/Data/chapter_score_table.csv` | F003-01 | 全 40 关目标有报告解释，章节开局不再简单等于上一章 Boss | 已完成，待运行验证 |
| 添加章节开放过滤 | `Assets/Scripts/DiceKingDemo.cs` | F003-01 | 第 1 章只出现开放池内容，第 2-3 章逐步开放，第 4 章后进入全池 | 已完成，待运行验证 |
| 添加隐藏倾向槽位 | `Assets/Scripts/DiceKingDemo.cs` | F003-02 | 每次市场后台 1 槽轻度倾向，界面不明示，`70% / 30%` 规则生效 | 已完成，待运行验证 |
| 添加路线识别 | `Assets/Scripts/DiceKingDemo.cs` | F003-02 | 本市场刚购买路线优先，其次当前骰袋主路线，最后通用池 | 已完成，待运行验证 |
| 调整经济配置 | `Assets/Resources/Data/global.csv`、`Assets/Resources/Data/market_rule_config.csv` | F003-03 | 通关固定收入、早期普通市场刷新和 Boss 市场刷新符合已确认默认值 | 已完成，待运行验证 |
| 输出验证报告 | `Docs/Production/Features/F003-level-formation-pacing/VALIDATION_REPORT.md` | F003-04 | 报告覆盖曲线、市场样本、经济样本、信息量检查 | 已完成静态记录，完整验证待执行 |
| 同步主文档 | `PROJECT_CONTEXT.md`、`GAME_FLOW.md`、`DICE_ARCHETYPES.md` | 实现后 | 文档反映已实现行为，不提前写成当前事实 | 已完成当前切片同步 |

## 代码影响

主要影响 `Assets/Scripts/DiceKingDemo.cs` 的市场生成区域：

- 当前市场商品生成先按 `dice_market_config.csv` 抽类型，再生成面组，再抽材质。
- F003 需要在抽类型、抽材质、抽面组前加入章节开放过滤。
- F003 已在 3 个货架中随机指定一个隐藏倾向槽位。
- 倾向槽位会读取本市场刚购买路线或当前骰袋唯一主路线。
- 购买非基础路线骰后，替换货架会优先按刚购买路线应用隐藏倾向规则。
- 倾向槽位候选池为空、候选未解锁或与同屏货架冲突时，已回退到通用已解锁池。

建议尽量小改：

1. 新增路线家族映射函数。
2. 新增章节开放判断函数。
3. 新增市场槽位生成上下文，包含是否倾向槽位。
4. 在现有生成流程内使用过滤和权重，不重写整套市场 UI。

## 数据影响

### 目标分

更新 `chapter_score_table.csv` 的 `target_score` 和 `balance_note`。首版应保留 10 章、每章 3 普通关加 1 Boss 的结构。

### 已确认开放表

| 章节 | 类型池 | 材质池 | 面组模板池 |
|---|---|---|---|
| 第 1 章 | `Basic / Odd / Even / Piggy / Turtle / Double` | `OfficialIron / GiltSeal` | `balanced / low_dense / high_bias` |
| 第 2 章新增 | `LoneWitness / Stamp / HalfStep / Track / Shellsmith / SlowTurtle / Treasury / Bribe / Tree` | `ClearGlaze / CopperBone` | `same_focus / straight_patch` |
| 第 3 章新增 | `Nest / Gardener / Irrigation / Gambler / Investment` | `LeadSeal` | `same_extreme`，低权重 |
| 第 4 章后 | 全部已实现类型 | 全部材质 | 全部模板 |

### 经济

优先更新：

- `global.csv` 的 `stage_clear_base_gold`
- `market_rule_config.csv` 的第 1-2 章 `normal_refresh_cost` 和 `boss_refresh_cost`

如需更细节的章节奖励，另行决定是否接回 `chapter_score_table.csv` 的奖励字段。

### 市场开放

首版实现路径：

- 在代码常量 / helper 中定义类型、材质、面组模板的开放章节。
- 不新增 `market_unlock_config.csv`，也不扩展现有 CSV 字段。
- 后续如果调参频繁，再把解锁表迁移到数据表。

### 路线家族

| 路线 | 类型 |
|---|---|
| 奇数 | `Odd / LoneWitness / Stamp` |
| 偶数 | `Even / HalfStep / Track` |
| 金币 | `Piggy / Treasury / Bribe / Investment` |
| 乌龟 | `Turtle / Shellsmith / Nest / SlowTurtle` |
| 大树 | `Tree / Gardener / Irrigation` |
| 爆发 | `Double / Gambler` |

`Basic` 不参与倾向路线统计，只进入通用池。

## 存档影响

首版不新增长期存档字段。隐藏倾向可以由本市场刚购买行为和当前骰袋在运行时计算。继续游戏时不恢复最近购买路线，直接按当前骰袋路线计算。

## 界面影响

- 不新增市场面板。
- 不显示倾向槽位。
- 不显示未解锁池。
- 第 1 章商品卡应因数据过滤自然减少复杂机制。

## 测试 / 验证计划

- 生成全 40 关目标曲线表，检查章节首关、普通关、Boss 的增长关系。
- 采样第 1、2、3、4、7、10 章各至少 100 次市场刷新，统计类型、材质、面组是否符合开放规则；未解锁内容出现次数必须为 0。
- 构造奇数、偶数、金币、乌龟、大树、爆发路线样本，各至少 100 次，检查隐藏倾向槽位相关补强出现率高于通用池。
- 检查同屏 3 个货架重复率是否下降或可接受。
- 检查第 1 章市场不会出现第 3 章才开放的类型、材质或强面组。
- 检查通关收入预览和实际收入使用 `global.csv` 数值。
- 检查第 1-2 章采样市场中入口路线骰、补强骰和 `Double` 的实际成交价区间。
- 执行 `git diff --check`。
- 有 Unity 环境时运行 Play Mode 验证市场和通关收入。

## 文档同步

| 文档 | 是否需要更新 | 已确认来源 |
|---|---|---|
| PROJECT_CONTEXT.md | 实现后需要 | F003 已确认范围 |
| GAME_FLOW.md | 实现后需要 | 市场、经济、目标曲线变化 |
| DICE_ARCHETYPES.md | 实现后需要 | 市场开放节奏、材质开放节奏、面组模板开放节奏 |
| ART_ASSETS.md | 不需要 | 无新增资源 |
| ACCEPTANCE.md | 实现后需要 | 验证结果 |

## 阻塞项

无硬阻塞。F003-01 / F003-02 / F003-03 已完成静态实现，完整 Unity Play Mode、市集采样和 UI 截图验证仍待执行。
