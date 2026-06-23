# F003 执行审查

状态：可执行
生产包：Docs/Production/Features/F003-level-formation-pacing/
来源设计对话编号：F003
最后更新：2026-06-11

## 审查结论

就绪度：生产包可执行，当前仍未实现。

原因：F003 的核心用户决策已经清楚，包括软引导、不明示倾向槽位、全章节难度检查、类型和材质渐进开放。用户已确认默认补丁组，默认数值、解锁表执行方式、隐藏倾向槽位优先级和验证阈值已经写回生产包文档。

## 摘要

本包没有硬阻塞，可以继续推进到 `$wabish-dev-implementation`。本轮已把默认值和规则批量写回 `FEATURE_BRIEF.md`、`DEV_HANDOFF.md`、`EXECUTION_PLAN.md`、`ACCEPTANCE.md` 和 `VALIDATION_REPORT.md`，并新增 `CONFIRMATION.md` 记录用户确认。

当前实现路径与包内方向基本匹配：市场商品生成集中在 `Assets/Scripts/DiceKingDemo.cs` 的 `BuildMarketOffers`、`MakeRandomMarketOffer`、`PickMarketDieConfig`、`PickFaceTemplate`、`PickDiceMaterial` 和 `MarketOfferPrice`；通关固定收入实际读取 `Assets/Resources/Data/global.csv` 的 `stage_clear_base_gold / roll_left_gold_bonus / cheat_left_gold_bonus`。需要特别注意，`chapter_score_table.csv` 中的 `base_reward_gold / roll_bonus_gold / cheat_bonus_gold` 当前不是已生效经济入口。

## 建议改进

### 已应用生产包补丁

1. 默认经济数值已确认：
   - `stage_clear_base_gold = 2`。
   - `roll_left_gold_bonus = 0`、`cheat_left_gold_bonus = 0` 首版保持关闭。
   - `starting_gold = 18` 首版不改，避免用初始本金掩盖关间收入和价格问题。
   - 第 1-2 章普通市场 `normal_refresh_cost = 1`。
   - 第 1-2 章 Boss 后章节市场建议 `boss_refresh_cost = 2`；若要更保守，可保留为后置微调，但不建议继续为 `3` 后直接实现。
   - `high_tier_pity_refreshes = 2` 首版保持。
   - 不做“每个市场第一次刷新免费”，因为它需要额外市场状态、UI 反馈和存档边界，超出 F003 的低改动目标。

2. 购买价格缺口已补成可验证目标：
   - 当前包提到“市场价格和权重建议”，但还没有写出早期购买可负担性的验收口径。
   - 建议 F003-01 增加一条硬验证：第 1-2 章采样市场中，玩家在正常通关收入节奏下，应能稳定买到至少一个入口路线骰或进行一次有意义刷新。
   - 若固定收入和刷新费下调后仍偏紧，优先调低第 1 章开放池内类型的基础买价或增加早期市场折扣；不优先提高初始金币。
   - `Double` 可以保留在第 1 章作为低权重“直观强力件”，但必须在验证报告中单独检查其实际成交价，因为当前它是 T2，叠加面组和材质后容易过贵。

3. 渐进开放首版已确认走代码常量 / helper，不先新增数据表：
   - 当前 `dice_market_config.csv`、`dice_material_config.csv` 和面组模板权重只支持 `1-2 / 3-5 / 6-10` 章节段，不能表达 F003 要求的第 1 章、第 2 章、第 3 章逐步开放。
   - F002 面组模板目前已经是 `DiceKingDemo.cs` 中的代码常量，因此首版用 `UnlockChapter` 类 helper 对类型、材质和模板做过滤，改动最小。
   - 后续如果调参频繁，再把解锁表迁移到 `market_unlock_config.csv`。

4. 渐进开放表已按以下默认值写入首版：
   - 第 1 章类型：`Basic / Odd / Even / Piggy / Turtle / Double`。
   - 第 2 章新增类型：`LoneWitness / Stamp / HalfStep / Track / Shellsmith / SlowTurtle / Treasury / Bribe / Tree`。
   - 第 3 章新增类型：`Nest / Gardener / Irrigation / Gambler / Investment`。
   - 第 4 章后：全部已实现类型。
   - 第 1 章材质：`OfficialIron / GiltSeal`。
   - 第 2 章新增材质：`ClearGlaze / CopperBone`。
   - 第 3 章新增材质：`LeadSeal`。
   - 第 1 章面组模板：`balanced / low_dense / high_bias`。
   - 第 2 章新增模板：`same_focus / straight_patch`。
   - 第 3 章新增模板：`same_extreme`，保持低权重。

5. 隐藏倾向槽位规则已补清优先级：
   - 每次生成 3 个市场货架时，随机选择 1 个槽位作为隐藏倾向槽位，不在 UI 显示。
   - 倾向槽位以 `70%` 尝试当前路线候选池、`30%` 使用通用已解锁池；这是生成倾向，不是保底锁定。
   - 路线来源优先级：本市场刚购买的非基础骰路线 > 当前骰袋中非基础路线数量最多的路线 > 通用池。
   - 候选池为空、候选未解锁或与同屏货架冲突时，回退到通用已解锁池。
   - 买入后替换该货架时，同样应用章节过滤和隐藏倾向规则。
   - 不为隐藏倾向新增存档字段；继续游戏时用当前骰袋重新推断路线即可。

6. 路线家族已写入程序交接：
   - 奇数：`Odd / LoneWitness / Stamp`
   - 偶数：`Even / HalfStep / Track`
   - 金币：`Piggy / Treasury / Bribe / Investment`
   - 乌龟：`Turtle / Shellsmith / Nest / SlowTurtle`
   - 大树：`Tree / Gardener / Irrigation`
   - 爆发：`Double / Gambler`
   - `Basic` 不参与倾向路线统计，只进入通用池。

7. 验证报告已补最低样本和通过条件：
   - 目标曲线：输出全 40 关目标、章内增长率、Boss/首关比、下一章首关/上一章 Boss 比，并解释所有异常点。
   - 市场开放：第 1、2、3、4、7、10 章各采样至少 100 次刷新，未解锁类型、材质、面组出现次数必须为 0。
   - 隐藏倾向：对奇数、偶数、金币、乌龟、大树、爆发各采样至少 100 次，相关候选出现率应明显高于通用池，但不能让 3 个货架都变成同一路线。
   - 经济：记录第 1-2 章通关后金币、可购买商品价格区间、刷新后余额，验证收入预览和实际收入一致。
   - UI：第 1 章市场截图不得出现“推荐路线”“倾向槽位”等显式标识，商品卡说明不能明显溢出。

### 需要用户决策的阻塞问题

无硬阻塞。用户已确认采用默认补丁组。

### 非阻塞后置细节

- 渐进开放是否数据表化可以后置到调参稳定后。
- 早期购买价格如果仍偏紧，可以在 F003-01 数据验证后再决定是调低基础买价、调低材质修正，还是加入第 1-2 章市场折扣。
- 隐藏倾向槽位是否跨市场保留最近购买历史不建议进入首版；当前骰袋推断已经足够支撑软引导。
- 更完整的自动平衡工具后置，F003 首版只要求生成可读、可复核的验证报告。

## 程序实现路径审查

建议按低改动路径执行：

1. F003-01 和 F003-03 先改数据：`chapter_score_table.csv`、`global.csv`、`market_rule_config.csv`，并给 `dice_market_config.csv` / `dice_material_config.csv` 的价格与权重形成首版建议。
2. F003-02 在 `DiceKingDemo.cs` 中新增章节解锁判断、路线家族判断和市场生成上下文；不要重写市场 UI。
3. `BuildMarketOffers` 负责生成 3 个货架并选择隐藏倾向槽位。
4. `MakeRandomMarketOffer` 或其下游选择函数接收生成上下文，统一应用章节过滤、倾向池和同屏去重。
5. `PickFaceTemplate`、`PickDiceMaterial`、`PickMarketDieConfig` 分别加过滤，不直接依赖现有章节段权重表达解锁。
6. `BuyOffer` 的替换货架也复用同一生成规则，并记录本市场刚购买路线用于即时软引导。
7. 经济只接 `global.csv` 和 `market_rule_config.csv` 的已生效字段；不要把 `chapter_score_table.csv` 的奖励列当成已实现奖励。

## 职能就绪度

| 角色 | 状态 | 原因 | 下一步 skill |
|---|---|---|---|
| 策划 | 就绪 | 默认数值、购买可负担性验收和目标曲线报告阈值已写回生产包 | `$wabish-dev-implementation` |
| 美术 | 就绪 | 首版无新增资源，不需要表现隐藏倾向槽位 | `$wabish-art-production` 仅在后续新增资源时使用 |
| 界面体验 | 就绪 | 首版要求是不新增显式状态，现有市场 UI 可承接；只需实现后截图检查信息量 | `$wabish-packet-review` / `$wabish-dev-implementation` |
| 程序 | 就绪 | 代码入口、隐藏倾向优先级、解锁表归属和验证阈值已写清 | `$wabish-dev-implementation` |

## 执行顺序

1. 执行 F003-01 数据调参和 F003-03 经济配置；两者需要一起看，因为目标分、购买价格和通关收入互相影响。
2. 执行 F003-02 市场生成规则，实现隐藏倾向槽位、章节解锁过滤和同屏去重。
3. 执行 F003-04 验证报告，覆盖全章节目标曲线、市场采样、经济样本和 UI 信息量。
4. 实现通过后再同步 `PROJECT_CONTEXT.md`、`GAME_FLOW.md`、`DICE_ARCHETYPES.md` 和 `ACCEPTANCE.md`，不要提前把未实现行为写成当前事实。

## 建议写回生产包的补丁

| 文件 | 章节 | 建议改动 | 原因 | 状态 |
|---|---|---|---|---|
| `FEATURE_BRIEF.md` | 决策矩阵 / 已确认默认值 / 已解决问题 | 将 `stage_clear_base_gold = 2`、第 1-2 章普通刷新 `1`、首版代码常量解锁表设为已确认；补充第 1-2 章 Boss 刷新建议 `2` | 避免程序切片二次决策 | 已应用 |
| `FEATURE_BRIEF.md` | 购买可负担性验收口径 | 增加购买可负担性验收口径，尤其检查 `Double` 和材质叠价 | 当前反馈包含“购买价格偏高”，需要形成验收目标 | 已应用 |
| `DEV_HANDOFF.md` | 代码影响 / 实现任务 | 补路线家族、倾向槽位优先级、fallback、买入后替换规则和不新增存档字段 | 程序实现路径需闭合 | 已应用 |
| `EXECUTION_PLAN.md` | 已确认默认值 / 验证计划 | 补样本数量和通过条件，明确 F003-01 与 F003-03 先并行执行 | 验证报告需要可执行标准 | 已应用 |
| `ACCEPTANCE.md` | 就绪度门禁 / 验收清单 | 将 `$wabish-packet-review` 审查记录为已完成，并记录用户确认默认补丁组 | 保持状态真实 | 已应用 |
| `VALIDATION_REPORT.md` | 全文 | 扩展为待填模板，预留目标曲线、市场开放、隐藏倾向、经济和 UI 截图结论 | F003-04 需要可直接填写 | 已应用 |
| `CONFIRMATION.md` | 全文 | 记录用户确认采用默认补丁组 | 满足生产包确认追踪 | 已应用 |

## 风险

| 风险 | 影响 | 缓解方式 | 负责人 |
|---|---|---|---|
| 只加通关工资但不处理价格 | 玩家仍可能买不起有效件，F003 体感改善不足 | F003-01 加购买可负担性采样，必要时调低早期价格或加早期折扣 | 策划 / 程序 |
| 只用现有 CSV 权重表达解锁 | 无法区分第 1 章和第 2 章，渐进开放失效 | 首版用代码 helper 表达解锁章节 | 程序 |
| 倾向槽位过强 | 玩家感觉被系统指定路线，违背软引导 | 保持 70/30、同屏去重和 fallback，不做 UI 明示 | 策划 / 程序 / UI |
| 倾向槽位过弱 | 成型难度仍偏随机 | 验证报告采样各路线相关候选出现率，必要时微调比例或候选池 | 策划 |
| 目标曲线只按指数重算 | 全章节难度仍不贴合实际开放池和经济 | 曲线报告必须绑定章节开放池、市场价格和经济样本解释 | 策划 |
| 新增持久化最近购买路线 | 增加存档兼容和继续游戏复杂度 | 首版不新增存档字段，用当前骰袋推断 | 程序 |

## 已记录的用户决策

- F002 算验收通过，F003 承接 F002 商店独特骰系统。
- F003 聚焦关卡体验优化：关卡难度、成型难度、购买价格 / 金币产出和前期信息量。
- 成型保护采用软引导，不做显式路线选择。
- 保留 1 个隐藏轻度倾向槽位，不在界面上明说。
- 检查全部章节的分数难度，不只看第 1 章。
- 骰子类型和材质需要随游戏流程渐进开放。
- 本轮要求只审查生产包，不改代码。
- 用户确认采用默认补丁组。

## 最终建议

建议通过本轮审查。F003 生产包当前可进入 F003-01 / F003-03 执行，随后执行 F003-02 和 F003-04。当前状态只代表生产包可执行，不代表功能已实现。
