# F001 美术需求

状态：已提案
功能：F001 后续骰子种类扩展
关联美术文档：PROJECT_CONTEXT.md、ART_ASSETS.md
最后更新：2026-06-10

## 输入决策

| 决策 | 状态 | 来源 | 美术影响 |
|---|---|---|---|
| F001-A 金币骰已经由用户确认实现验收。 | 已验收 | 用户确认；F001-A ACCEPTANCE.md | 金币骰图标已完成，不再阻塞后续。 |
| F001-B 龟龟骰已由用户确认验收。 | 已验收 | 用户确认；F001-B ACCEPTANCE.md | 龟龟补强骰图标已完成，不再阻塞后续。 |
| F001-C 奇偶骰已由用户确认实现验收。 | 已验收 | F001-C ACCEPTANCE.md | 奇偶最终图标已完成，不再阻塞后续。 |
| F001-D 大树骰已启动规则草案。 | 待回答 | F001-D FEATURE_BRIEF.md | 只保留大树补强视觉动机，不生成最终资源。 |

## 视觉意图

F001 的新增骰图标应延续明亮账本桌游风，并通过骰子本体上的图形元素表达路线身份。每个路线内部可以有统一视觉族谱，但单颗骰在小尺寸市场货架和结果角标中必须可区分。

## 资源清单

| 资源 | 用途 | 目标路径 | 来源决策 | 状态 |
|---|---|---|---|---|
| 国库骰图标 | 市场、类型卡、结果角标 | Assets/Resources/Art/DiceTypes/treasury_die_icon.png | F001-A | 已完成 |
| 贿赂骰图标 | 市场、类型卡、结果角标 | Assets/Resources/Art/DiceTypes/bribe_die_icon.png | F001-A | 已完成 |
| 投资骰图标 | 市场、类型卡、结果角标 | Assets/Resources/Art/DiceTypes/investment_die_icon.png | F001-A | 已完成 |
| 壳匠骰图标 | 市场、类型卡、结果角标 | Assets/Resources/Art/DiceTypes/shellsmith_die_icon.png | F001-B | 已完成 |
| 巢穴骰图标 | 市场、类型卡、结果角标 | Assets/Resources/Art/DiceTypes/nest_die_icon.png | F001-B | 已完成 |
| 慢龟骰图标 | 市场、类型卡、结果角标 | Assets/Resources/Art/DiceTypes/slow_turtle_die_icon.png | F001-B | 已完成 |
| 孤证骰图标 | 市场、类型卡、结果角标 | Assets/Resources/Art/DiceTypes/lone_witness_die_icon.png | F001-C | 已完成 |
| 盖章骰图标 | 市场、类型卡、结果角标 | Assets/Resources/Art/DiceTypes/stamp_die_icon.png | F001-C | 已完成 |
| 半步骰图标 | 市场、类型卡、结果角标 | Assets/Resources/Art/DiceTypes/half_step_die_icon.png | F001-C | 已完成 |
| 轨道骰图标 | 市场、类型卡、结果角标 | Assets/Resources/Art/DiceTypes/track_die_icon.png | F001-C | 已完成 |
| F001-D 大树补强图标 | 市场、类型卡、结果角标 | Assets/Resources/Art/DiceTypes/ | F001-D 待规则确认 | 阻塞 |

## 风格关键词

- 明亮账本桌游风
- 扁平 2D 骰子图标
- 清晰厚实轮廓
- 路线符号嵌入骰子本体
- 无生成文字

## 必须保留

- 图标主体必须是骰子，不是独立道具。
- 每个图标在市场小尺寸中有清楚剪影。
- 龟龟路线可以使用龟壳、工匠工具、小窝、慢速轨迹，但不能变成真实动物插画。

## 避免

- 赌场、霓虹、写实渲染。
- 幼儿贴纸风和过度拟人化表情。
- 图标内可读文字。
- 细节过密导致角标不可读。

## 生成 / 来源备注

F001-A、F001-B、F001-C 图标已完成。F001-D 候选规则确认前，不生成最终图标；规则确认后通过 `$wabish-art-production` 协调生成，并同步 `ART_ASSETS.md`。

## 阻塞项

- F001-D 候选规则尚未确认。

## 验收

- [x] F001-A 金币骰图标已完成并登记。
- [x] F001-B 龟龟补强骰图标已完成并登记。
- [x] F001-C 候选规则已确认。
- [x] F001-C 对应图标小尺寸可读并登记到 `ART_ASSETS.md`。
- [ ] F001-D 候选规则已确认。
- [ ] F001-D 对应图标小尺寸可读。
- [ ] F001-D 对应资源登记到 `ART_ASSETS.md`。
