# F001-A 美术需求

状态：美术已完成
功能：F001-A 金币骰规则细化
关联美术文档：PROJECT_CONTEXT.md、ART_ASSETS.md
最后确认：2026-06-04

## 输入决策

| 决策 | 状态 | 来源 | 美术影响 |
|---|---|---|---|
| 国库骰、贿赂骰、投资骰为金币流第一批候选。 | 已确认 | D1 设计记录、F001 | 可进入图标生产准备。 |
| 三颗骰规则已确认。 | 已确认 | RULE_CARDS.md、用户 2026-06-04 决策 | 已生成最终运行资源。 |
| 图标主体方向采用骰子本体原型。 | 已确认 | 用户 2026-06-04 反馈 | 金库、钱袋、印章、增长等元素只作为骰面、边缘或小角标，不让骰子变成配饰。 |
| 图标风格沿用明亮账本桌游风。 | 已确认 | PROJECT_CONTEXT.md | 视觉关键词可执行。 |

## 视觉意图

三颗金币骰图标应看起来属于同一条“王室账务”路线，但分别表达不同经济行为：

- 国库骰：储备、金库、本金、安全感。
- 贿赂骰：暗中递交、封口费、临门一脚，不要赌场化。
- 投资骰：预算、契约、长期增长、提前投入。

## 资源清单

最终运行资源已生成，以下为当前生产结果。

| 资源 | 用途 | 尺寸 / 格式 | 目标路径 | 来源决策 | 状态 |
|---|---|---|---|---|---|
| 国库骰图标 | 市场、类型卡、结果角标 | PNG，透明背景 | Assets/Resources/Art/DiceTypes/treasury_die_icon.png | 国库骰已确认规则 | 已生产 |
| 贿赂骰图标 | 市场、类型卡、结果角标 | PNG，透明背景 | Assets/Resources/Art/DiceTypes/bribe_die_icon.png | 贿赂骰已确认规则 | 已生产 |
| 投资骰图标 | 市场、类型卡、结果角标 | PNG，透明背景 | Assets/Resources/Art/DiceTypes/investment_die_icon.png | 投资骰已确认规则 | 已生产 |

## 风格关键词

- 明亮账本桌游风
- 扁平 2D 图标
- 王室账簿、金币袋、蜡封、印章、预算契约
- 清晰厚实轮廓
- 无生成文字
- 骰子本体占主体，经济元素嵌入骰面、边缘或小角标

## 必须保留

- 小尺寸角标中能区分三颗骰。
- 金币流视觉统一，但国库、贿赂、投资的行为差异明显。
- 骰子必须是图标主体，不得被金库、账单、契约等道具压成配饰。
- 国库骰偏稳定和储备，不要像直接得分爆发。
- 贿赂骰偏荒诞账务和临门补账，不要犯罪化或赌场化。
- 投资骰偏预算契约和增长，不要像一次性消费。

## 避免

- 赌场筹码、老虎机、霓虹赌桌。
- 写实钱币堆、暗黑犯罪感。
- 图标内可读文字。
- 过密细节导致市场货架不可读。
- 大型金库、账单、契约或钱堆抢走骰子主体。

## 生成 / 来源备注

已使用 `$wabish-art-production` 协调，并由 `$wabish-art-assets` 生成对应最终图标。生成方向为以骰子本体为主，国库、贿赂、投资元素分别集成到骰面、边缘配件或小角标中。源图和接触图放入 `Assets/ArtSource/`，最终运行图放入 `Assets/Resources/Art/DiceTypes/`。

源图和接触图：

```text
Assets/ArtSource/DiceTypes/_source_chromakey/treasury_die_icon_source.png
Assets/ArtSource/DiceTypes/_source_chromakey/bribe_die_icon_source.png
Assets/ArtSource/DiceTypes/_source_chromakey/investment_die_icon_source.png
Assets/ArtSource/DiceTypes/_gold_dice_contact_sheet_20260604.png
```

## 放置 / 运行时用途

最终图标需要支持：

- 市场货架主图。
- 未投掷类型卡。
- 结果骰角标。
- 骰袋路线摘要中的小图标候选。

## 实现备注

程序接入时需要更新 `DieTypeIconFileName` 和图标加载映射。本次不修改代码。

## 阻塞项

无美术规则阻塞。

## 验收

- [x] 对应图标在市场和结果角标小尺寸下可读。
- [x] 图标符合明亮账本桌游风。
- [x] 三颗图标同属金币流，但行为差异清楚。
- [x] 最终资源登记到 `ART_ASSETS.md`。
