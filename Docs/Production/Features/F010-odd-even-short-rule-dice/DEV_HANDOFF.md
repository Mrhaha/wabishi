# F010 程序交接

状态：已实现并通过 Unity Play Mode 验证
功能：F010 奇偶短规则骰
关联主文档：PROJECT_CONTEXT.md、GAME_FLOW.md、DICE_ARCHETYPES.md
实现事实来源：Assets/Scripts/DiceKingDemo.cs
最后更新：2026-06-16

## 输入决策

- 第一阶段不改代码、不写正式市场数据、不生成最终运行时资源；该门禁已完成。
- 七颗候选为 `异邻骰 / 同邻骰 / 补全骰 / 复核骰 / 翻号骰 / 守号骰 / 转号骰`。
- 接触图已通过，最终图标已入库，程序实现已接入当前原型。

## 实现目标

本文件记录程序实现边界和验证状态。当前已新增七种奇偶短规则骰的类型、市场配置、预览和真实结算逻辑，并保持出千、全奇 / 全偶副牌型和现有 F001-C 奇偶补强骰的边界不变。

## 已确认行为

- 不新增全局牌型。
- 不新增长期成长字段。
- 不改变出千次数、出千选择上限和出千确认后必须接受新结果的规则。
- 接触图通过前不动代码。

## 实现任务

| 任务 | 文件 / 数据 | 依赖 | 验收 | 状态 |
|---|---|---|---|---|
| 增加七种类型枚举和显示名 | Assets/Scripts/DiceKingDemo.cs | F010-03 接触图接受、规则卡审查 | 类型能在市场和骰袋显示 | 已完成并通过 Play Mode 验证 |
| 增加市场配置行 | Assets/Resources/Data/dice_market_config.csv | 市场章节和数值确认 | CSV 可解析，市场能刷出 | 已完成静态校验 |
| 接入最终图标 key | Assets/Scripts/DiceKingDemo.cs、ART_ASSETS.md | F010-04 最终图标已生成 | 资源缺失时有占位，资源存在时加载正确 | 已完成静态校验 |
| 实现邻位固定分 | Assets/Scripts/DiceKingDemo.cs | 固定分口径确认 | 左邻同 / 异触发和预览一致 | 已完成并通过样例验证 |
| 实现补全副牌型 | Assets/Scripts/DiceKingDemo.cs | `补全骰` 牌型边界确认 | 只影响全奇 / 全偶，不改有效点数 | 已完成并通过样例验证 |
| 实现复核重摇 | Assets/Scripts/DiceKingDemo.cs | 复核触发时机确认 | 自然开盅触发一次，出千后不触发 | 已完成并通过样例验证 |
| 实现出千收益 | Assets/Scripts/DiceKingDemo.cs | 出千标记和固定分口径确认 | 翻号 / 守号触发互斥 | 已完成并通过样例验证 |
| 实现转号控制 | Assets/Scripts/DiceKingDemo.cs | 面组限制确认 | 出千重摇只抽相反奇偶面 | 已完成并通过样例验证 |
| 文档同步 | PROJECT_CONTEXT.md、GAME_FLOW.md、DICE_ARCHETYPES.md | 程序验证完成 | 当前规则与实现一致，并记录运行验证结论 | 已完成 |

## 代码影响

已影响：

- `DieType` 或等价类型枚举。
- 市场生成、显示名、短规则和图标 key。
- 结果锁定后的自动重摇流程。
- 牌型识别中的全奇 / 全偶副牌型判断。
- 出千确认前后的旧值 / 新值记录。
- `ScoreDice()` 的预览和真实结算一致性。

## 数据影响

已影响：

- `Assets/Resources/Data/dice_market_config.csv`
- 未新增 `global.csv` 数值键；首版固定分和市场初值写入代码默认值与 `dice_market_config.csv`。

建议代码键：

| 骰子 | 建议代码键 |
|---|---|
| 异邻骰 | `ParityNeighborDiff` |
| 同邻骰 | `ParityNeighborSame` |
| 补全骰 | `ParityComplete` |
| 复核骰 | `ParityReview` |
| 翻号骰 | `ParityFlipScore` |
| 守号骰 | `ParityHoldScore` |
| 转号骰 | `ParityTurner` |

## 存档影响

首版建议不新增长期存档字段。可能需要本小关或本次出手临时标记：

- 本骰是否已经自动复核。
- 本骰出千前奇偶。
- 本骰是否被确认出千并实际重摇。

这些状态不应写入长期 `DiceData`。

## 界面影响

当前实现已接入：

- 市场卡短规则。
- 骰袋类型卡图标。
- 结果预览短标签。
- 出千选择提示。
- 结算演出短标签。

## 测试 / 验证计划

程序验收至少覆盖：

- 每颗骰一个触发样例和一个未触发样例。
- `异邻骰 / 同邻骰` 的最左槽不触发样例。
- `补全骰` 成立全奇 / 全偶但不改变有效点数的样例。
- `复核骰` 自然触发一次和出千后不触发样例。
- `翻号骰 / 守号骰` 出千确认、取消、未选择三类路径。
- `转号骰` 只从相反奇偶面抽取，且无相反奇偶面时不会生成或不会启用。
- 预览和真实结算一致。

当前验证记录：

- 七颗 F010 类型均存在于 `DiceKingDemo.cs`、`dice_market_config.csv` 和 `Assets/Resources/Art/DiceTypes/`。
- 七颗运行时图标为 256x256 RGBA PNG，四角透明，已生成 `.meta`。
- 2026-06-16 使用当前打开的 Unity 2019.4.33f1 编辑器完成 Play Mode 验证；报告为 `Docs/QA/F010/20260616_f010_validation_report.txt`。
- 已覆盖异邻、同邻、补全、复核、翻号、守号、转号样例，确认预览分与真实结算分一致。
- 已覆盖出千确认、取消、未选择和 `ParityTurner` 无相反奇偶候选时不可选路径。
- QA 截图已生成：`Docs/QA/F010/20260616_f010_result_1920x1080.png`、`Docs/QA/F010/20260616_f010_cheat_1920x1080.png`、`Docs/QA/F010/20260616_f010_result_1280x720.png`。

## 文档同步

| 文档 | 是否需要更新 | 已确认来源 |
|---|---|---|
| PROJECT_CONTEXT.md | 已更新 | F010 规则卡、实现结果和运行验证结论 |
| GAME_FLOW.md | 已更新 | 出千、复核、全奇 / 全偶边界和运行验证结论 |
| DICE_ARCHETYPES.md | 已更新 | 七颗骰正式规则和运行验证结论 |
| ART_ASSETS.md | 已更新 | F010-04 |
| Docs/DesignDialogues/D1-future-dice-types.md | 已更新 | F010 路径和状态 |

## 阻塞项

无程序实现阻塞。当前缺口仅是用户实玩验收和后续数值 / UI 反馈。
