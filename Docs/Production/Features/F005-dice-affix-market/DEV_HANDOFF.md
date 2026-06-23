# F005 程序交接

状态：已实现待 Unity 验证
功能：F005 骰子词缀与商店改造道具
关联主文档：PROJECT_CONTEXT.md、GAME_FLOW.md、DICE_ARCHETYPES.md
实现事实来源：Assets/Scripts/DiceKingDemo.cs
最后更新：2026-06-11

## 输入决策

- 词缀槽位：每颗实体骰最多 2 前缀 + 2 后缀。
- 词缀 T 阶：`T1-T6`，`T1` 最高，`T6` 最低，同家族只改数值。
- 首批改造道具：`加印石 / 剥印石 / 换印石`。
- 三类道具均为全随机，且必须遵守槽位上限和同家族不重复。
- 三类道具不强制购买后立即使用；购买后可持有，使用入口只在市场阶段开放。
- 首版商店不能刷出自带词缀的成品骰。
- F005 取消本关账本概念，金币来源按左到右单向流式钱包结算。
- 猪猪骰、鎏印材质和所有旧账本金币源必须复查并统一迁移为直接进入钱包。
- 金币后缀每次出手计算一次，同一条后缀在一次出手中最多触发一次。
- 结算顺序绑定玩家看到的锁定结果展示顺序。
- 预览和真实结算必须使用同一顺序和同一规则。

## 实现目标

在当前 Unity 原型中接入一版可玩的骰子词缀系统：

1. 骰子可以保存和展示词缀实例。
2. 商店可以刷出和出售三类改造道具。
3. 玩家可以持有三类改造道具，并且只能在市场阶段选择目标骰随机改造。
4. 首版启用词缀能参与预览和真实结算。
5. 金币来源按左到右顺序直接进入钱包，并可被后续对象读取或消费。
6. 运行存档升级，旧记录按项目策略清空或迁移。

## 已确认行为

- 临时小骰不继承词缀。
- 临时小骰不触发词缀。
- 词缀不改变骰子类型身份。
- 词缀不替代材质，材质仍是 F002 的辅轴。
- 同一骰不能重复拥有同一词缀家族。
- 三类改造道具都不允许玩家指定词缀、指定槽位或指定删除目标。
- 三类改造道具不允许在小关、出手中或结算中使用。
- 使用改造道具时先校验目标合法性，非法目标不消耗道具。
- 较晚产生的金币不能回头改变较早骰子或词缀已经完成的结算。
- 不保留本关账本概念；金币来源在左到右结算流中直接写入钱包。

## 实现任务

| 任务 | 文件 / 数据 | 依赖 | 验收 | 状态 |
|---|---|---|---|---|
| 新增词缀定义结构 | Assets/Scripts/DiceKingDemo.cs、Assets/Resources/Data/dice_affix_config.csv | F005-01 | 能读取词缀 key、槽位、曲线、触发、权重、短文案 | 已实现待 Unity 验证 |
| 新增 T 阶曲线配置 | Assets/Resources/Data/dice_affix_tier_config.csv | F005-01 | `T1` 数值最高，曲线匹配生产包 | 已实现待 Unity 验证 |
| 新增骰子词缀实例 | Assets/Scripts/DiceKingDemo.cs | 词缀定义 | 每颗骰可保存前缀和后缀实例 | 已实现待 Unity 验证 |
| 升级存档 | Assets/Scripts/DiceKingDemo.cs | 词缀实例 | `DiceData` 写入词缀 key 和 T 阶；旧版本运行记录处理明确 | 已实现待 Unity 验证 |
| 新增改造道具配置 | Assets/Resources/Data/dice_crafting_item_config.csv | F005 道具规则 | 能配置价格、章节权重、道具类型 | 已实现待 Unity 验证 |
| 新增改造道具持有状态 | Assets/Scripts/DiceKingDemo.cs | F005-01 | 能保存和读取三类道具数量，且只在市场阶段开放使用 | 已实现待 Unity 验证 |
| 接入商店道具货架 | Assets/Scripts/DiceKingDemo.cs | 改造道具配置 | 市场能出现三类道具并购买 | 已实现待 Unity 验证 |
| 实现市场阶段目标选择 | Assets/Scripts/DiceKingDemo.cs | UI 规格 | 不合法目标不可选且不消耗道具 | 已实现待 Unity 验证 |
| 实现三类随机改造 | Assets/Scripts/DiceKingDemo.cs | 词缀池和道具 | 添加、删除、替换符合规则 | 已实现待 Unity 验证 |
| 接入词缀预览 | Assets/Scripts/DiceKingDemo.cs | 结算路径 | 结果预览显示词缀影响 | 已实现待 Unity 验证 |
| 接入真实结算 | Assets/Scripts/DiceKingDemo.cs | 预览路径 | 真实结算与预览一致 | 已实现待 Unity 验证 |
| 迁移流式钱包 | Assets/Scripts/DiceKingDemo.cs | F005 第 6 轮 | 猪猪、鎏印和金币后缀按左到右进入钱包 | 已实现待 Unity 验证 |
| 文档同步 | PROJECT_CONTEXT.md、GAME_FLOW.md、DICE_ARCHETYPES.md、F005 文档 | 实现完成 | 主文档不再和实现冲突 | 已同步 |

## 代码影响

当前原型仍集中在 `Assets/Scripts/DiceKingDemo.cs`，首版可以在该文件中受控扩展。已接入或待接入的概念：

- `AffixSlot`：前缀、后缀。已接入。
- `AffixTier`：`T1-T6`。
- `AffixDefinition`：key、显示名、槽位、触发类型、曲线、权重、短规则、互斥组。已接入。
- `AffixInstance`：key、tier。已接入，按 T 阶曲线查表值。
- `CraftingItemDefinition`：key、显示名、类型、价格、章节权重、短规则。已接入。
- `CraftingItemInventory` 或等价字段：保存 `affix_add_stone`、`affix_remove_stone`、`affix_replace_stone` 数量。
- `DiceData`：新增前缀列表和后缀列表。已接入。
- 市场商品类型：区分骰子商品和改造道具商品。
- 结算上下文：支持左到右钱包读写和日志记录。

## 数据影响

建议新增数据表：

| 文件 | 作用 | 关键字段 |
|---|---|---|
| Assets/Resources/Data/dice_affix_tier_config.csv | T 阶数值曲线 | curve_key、t1、t2、t3、t4、t5、t6 |
| Assets/Resources/Data/dice_affix_config.csv | 词缀定义和权重 | affix_key、display_name、slot、trigger_key、curve_key、weight_ch1_2、weight_ch3_5、weight_ch6_10、short_rule、mutex_group、enabled |
| Assets/Resources/Data/dice_crafting_item_config.csv | 改造道具价格和权重 | item_key、display_name、craft_type、buy_price、unlock_chapter、weight_ch1_2、weight_ch3_5、weight_ch6_10、short_rule |

首版若继续维持单文件原型，也可以先用代码常量兜底，但正式行为应优先读取 `Resources/Data` 配置，并在缺失时使用兜底值。F005-04 已新增三张配置表，并保留代码兜底值。

## 存档影响

- 当前运行存档版本已升级为 `SaveVersion = 4`。
- `DiceData` 已新增前缀列表和后缀列表，格式为 `affix_key:tier` 的分号列表。
- 当前项目仍是原型，读取旧版本运行记录时清空本轮记录，保留菜单设置和开场观看状态。
- 如果未来选择迁移旧记录，必须给没有词缀字段的骰子补空词缀列表。
- F005 需要保存三类改造道具持有数量；旧存档默认三类数量为 0。
- 继续游戏恢复到小关开始，不保存小关中途的流式钱包结算状态。

## F005-04 实现记录

2026-06-11：

- 已新增 `dice_affix_tier_config.csv`、`dice_affix_config.csv`、`dice_crafting_item_config.csv`。
- 已在 `DiceKingDemo.cs` 接入词缀槽位、词缀实例、词缀定义、T 阶曲线和改造道具定义读取。
- 已将运行存档升级到 `SaveVersion = 4`，`DiceData` 写入前缀和后缀实例，并保存三类改造道具持有数量。
- 已在骰袋和骰子卡片上加入词缀槽位 / 短标签文本回退。
- 已用 `Import-Csv` 校验三张 CSV 可读取；本机未发现 `Unity`、`dotnet` 或 `csc` 命令行入口，尚未完成 Unity 编译验证。

## F005-05 / F005-06 实现记录

2026-06-11：

- 已在市场货架中接入骰子商品和改造道具商品两类 offer；骰袋满时仍允许购买改造道具。
- 已保存和读取 `affix_add_stone`、`affix_remove_stone`、`affix_replace_stone` 三类道具持有数量。
- 已在市场阶段提供道具栏和目标骰选择；取消选择不消耗道具，非法目标不会执行。
- 已实现全随机加印、剥印、换印，遵守 2 前缀 + 2 后缀上限、同家族互斥和临时小骰不可改造。
- 已将前缀接入单骰分，后缀接入钱包金币，并让预览和真实结算共用同一触发口径。
- 已将猪猪、鎏印和金币后缀改为按锁定结果从左到右直接进入钱包；贿赂可用金币上限按同一顺序模拟，避免后置金币回头供给前置贿赂。
- 已接入三类最终运行时图标：`Art/Items/affix_add_stone`、`Art/Items/affix_remove_stone`、`Art/Items/affix_replace_stone`；资源缺失时仍回退到程序化占位图。
- 已同步 `PROJECT_CONTEXT.md`、`GAME_FLOW.md`、`DICE_ARCHETYPES.md` 和 F005 生产包文档。

## 界面影响

- 市场货架需要能显示道具商品。
- 骰袋列表和目标选择需要显示词缀槽位。
- 结果预览和真实结算日志需要显示词缀触发和钱包变化。
- 词缀文案必须短，完整解释放详情。

## 测试 / 验证计划

- 创建带空槽骰，使用 `加印石`，确认只添加合法前缀或后缀。
- 创建满词缀骰，使用 `加印石` 时目标不可选。
- 创建无词缀骰，使用 `剥印石` 和 `换印石` 时目标不可选。
- 创建已有词缀骰，使用 `剥印石` 后随机减少 1 条。
- 创建已有词缀骰，使用 `换印石` 后词缀数量保持不变，但至少发生一次删除和一次新增。
- 购买改造道具后数量增加，离开再进入市场后数量仍正确。
- 在非市场阶段不能使用改造道具。
- 目标不合法时不消耗改造道具。
- 同家族重复时重新抽取或排除。
- 刷新市场至少 30 次，记录道具出现率、价格和章节分布。
- 验证 12 条首版词缀在预览和真实结算中一致。
- 验证左到右钱包：前一个骰产生金币后直接进入钱包，后一个骰可读取或消费。
- 验证后一个骰产生金币不会回头改变前一个骰。
- 验证猪猪骰、鎏印材质和金币后缀都不再写入本关账本。
- 验证金币后缀每次出手计算一次，同一条后缀在一次出手中最多触发一次。
- 验证临时小骰不触发词缀。
- 验证保存、继续游戏、卖出和购买后词缀仍正确。

## 文档同步

| 文档 | 是否需要更新 | 已确认来源 |
|---|---|---|
| PROJECT_CONTEXT.md | 实现完成后需要 | F005 已确认规则 |
| GAME_FLOW.md | 实现完成后需要 | 流式钱包、商店道具、存档版本 |
| DICE_ARCHETYPES.md | 实现完成后需要 | 猪猪、鎏印、金币路线边界迁移 |
| ART_ASSETS.md | 已更新 | 三类道具图标 |
| Docs/DesignDialogues/F005-dice-affix-market.md | 生产包创建和实现后需要 | 生产包路径、状态 |

## 阻塞项

F005-01 已完成用户确认。程序实现已完成待 Unity 编译 / 运行验证；下一步优先做市场刷新样本、三类道具样例、流式钱包样例和 UI 截图验收。
