# F003 执行计划

状态：部分实现，待运行验证
功能：F003 关卡与成型节奏优化
来源：Docs/DesignDialogues/F003-level-formation-pacing.md
编译来源：wabish-production-pipeline

## 就绪度摘要

| 角色 | 就绪度 | 下一步 skill | 阻塞问题 |
|---|---|---|---|
| 策划 | 就绪 | wabish-dev-implementation | 无 |
| 美术 | 就绪 | wabish-art-production | 无；首版无新增资源 |
| 界面体验 | 就绪 | wabish-dev-implementation | 无；首版不明示倾向槽位 |
| 程序 | 就绪 | wabish-dev-implementation | 无；默认补丁组已确认 |

## 执行顺序

1. 执行 F003-01 数据调参，形成目标曲线、开放池、市场价格和权重建议。
2. 执行 F003-03 经济配置，可与 F003-01 并行，先落地稳定通关收入和早期刷新费。
3. 执行 F003-02 市场生成规则，接入隐藏倾向槽位、章节解锁过滤和同屏去重。
4. 执行 F003-04 验证报告，记录曲线、市场样本、经济样本和 UI 信息量。
5. 实现完成后同步 `PROJECT_CONTEXT.md`、`GAME_FLOW.md`、`DICE_ARCHETYPES.md` 和 `ACCEPTANCE.md`。

## 职能任务

| 步骤 | 角色 | 任务 | 输入 | 输出 | 验收 | 状态 |
|---|---|---|---|---|---|---|
| 1 | 策划 | 审查默认值和开放节奏 | FEATURE_BRIEF.md | EXECUTION_REVIEW.md、CONFIRMATION.md 和生产包补丁 | 无阻塞问题，默认值可执行 | 已完成 |
| 2 | 策划 | 重算目标曲线 | FEATURE_BRIEF.md、chapter_score_table.csv | 新目标分建议 | 全 40 关有曲线说明 | 已完成 |
| 3 | 程序 | 更新数据调参 | chapter_score_table.csv、dice_market_config.csv、dice_material_config.csv | 数据改动 | 数据能被当前加载逻辑读取或明确新增加载逻辑 | 已完成，待运行验证 |
| 4 | 程序 | 实现隐藏倾向槽位 | DEV_HANDOFF.md | 市场生成逻辑 | 采样可见相关补强出现率提升，UI 不明示 | 已完成，待运行验证 |
| 5 | 程序 | 实现渐进开放过滤 | DEV_HANDOFF.md | 类型、材质、面组过滤 | 各章节池符合开放表 | 已完成，待运行验证 |
| 6 | 程序 | 调整经济配置 | global.csv、market_rule_config.csv | 经济数据改动 | 通关收入和刷新费用符合设定 | 已完成，待运行验证 |
| 7 | 跨职能 | 输出验证报告 | 实现结果 | VALIDATION_REPORT.md | 报告覆盖四类验证 | 部分完成 |
| 8 | 跨职能 | 文档同步和验收 | 主文档、ACCEPTANCE.md | 文档更新 | 当前实现和文档一致 | 已完成当前切片同步 |

## 执行切片

| 切片 | 范围 | 输出 | 当前状态 | 推荐动作 |
|---|---|---|---|---|
| F003-01 | 数据调参 | 全 40 关目标分曲线、章节开放池、市场价格和权重建议 | 已实现待运行验证 | 已完成静态数据验证 |
| F003-02 | 市场生成规则 | 隐藏倾向槽位、路线识别、章节解锁过滤、同屏去重 | 已实现待运行验证 | 已完成静态代码路径验证 |
| F003-03 | 经济配置 | 小额固定通关收入、早期刷新成本、奖励入口核对 | 已实现待运行验证 | 已完成静态数据验证 |
| F003-04 | 验证报告 | 目标曲线报告、市场刷新样本、经济流水样本、UI 信息量检查 | 部分执行 | 待 Play Mode、市集采样和 UI 截图后补全 |

## 依赖关系

- F003-02 依赖 F003-01 的开放池和目标路线家族。
- F003-03 可与 F003-01 并行，但最终验证必须和目标分一起看。
- F003-04 依赖 F003-01、F003-02、F003-03 完成。
- 美术无新增资源依赖。

## 已确认默认值

- 第 1 章类型池：`Basic / Odd / Even / Piggy / Turtle / Double`。
- 第 2 章新增：`LoneWitness / Stamp / HalfStep / Track / Shellsmith / SlowTurtle / Treasury / Bribe / Tree`。
- 第 3 章新增：`Nest / Gardener / Irrigation / Gambler / Investment`。
- 第 1 章材质池：`OfficialIron / GiltSeal`。
- 第 2 章新增材质：`ClearGlaze / CopperBone`。
- 第 3 章新增材质：`LeadSeal`。
- 第 1 章面组模板：`balanced / low_dense / high_bias`。
- 第 2 章新增模板：`same_focus / straight_patch`。
- 第 3 章新增模板：`same_extreme`，低权重。
- 倾向槽位：`70%` 当前路线池、`30%` 通用池。
- `stage_clear_base_gold = 2`。
- `roll_left_gold_bonus = 0`、`cheat_left_gold_bonus = 0`。
- 第 1-2 章普通刷新费 `normal_refresh_cost = 1`。
- 第 1-2 章 Boss 后章节市场刷新费 `boss_refresh_cost = 2`。
- 渐进开放首版使用 `DiceKingDemo.cs` 代码常量 / helper，不新增解锁 CSV。

## 阻塞问题

无硬阻塞。F003-01 数据调参、F003-02 市场生成规则和 F003-03 经济配置已完成静态实现；完整运行验证仍待执行。

## 验证计划

- 静态检查：`git diff --check`。
- 数据检查：解析目标分、市场、材质和经济 CSV，确认字段合法。
- 曲线检查：输出全 40 关目标、章内增长率、Boss/首关比、下一章首关/上一章 Boss 比，并解释异常点。
- 市场检查：第 1、2、3、4、7、10 章各采样至少 100 次刷新，未解锁类型、材质、面组出现次数必须为 0。
- 倾向检查：奇数、偶数、金币、乌龟、大树、爆发各采样至少 100 次，相关候选出现率应明显高于通用池，但不能让 3 个货架都变成同一路线。
- 经济检查：通关收入预览、实际收入、刷新扣费、第 1-2 章购买可负担性和 `Double` 成交价符合配置。
- UI 检查：第 1 章市场截图不出现明示倾向槽位，商品说明不明显溢出。
- Unity 验证：有 Unity 环境时运行 Play Mode 检查市场刷新、购买、卖出、通关收入和继续游戏。

## 交接备注

本包已完成 `$wabish-packet-review` 审查、用户确认、F003-01 数据调参、F003-02 市场生成规则和 F003-03 经济配置。下一步建议补齐 F003-04 的 Unity Play Mode、市场采样和 UI 截图验证。
