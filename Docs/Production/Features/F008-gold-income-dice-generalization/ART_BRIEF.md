# F008 美术需求

状态：已生成最终资源
功能：F008 金币收益族泛用化
关联美术文档：PROJECT_CONTEXT.md、ART_ASSETS.md
最后更新：2026-06-15

## 输入决策

- 九颗金币收益族骰子都进入后续生产范围。
- F008-01 已确认九颗骰的触发来源：指定点、最高点、牌型、每小关首次结算、通关利息、出千重摇、临时小骰数量、左邻金币、左邻削分。
- 美术资源只表达骰子类型身份，不承载规则长文。
- 骰子类型图标必须把机制符号融入骰子本体，避免大型外部道具说明图。
- 当前稳定风格为明亮账本桌游风。

## 视觉意图

九颗骰都属于金币收益族，应共享金币、账本、封签、票据、柜台、税章等视觉语言；同时每颗必须通过骰面标记、骰边切角、封章、刻槽或镶嵌图案区分触发来源。

## 资源清单

| 资源 | 用途 | 尺寸 / 格式 | 目标路径 | 来源决策 | 状态 |
|---|---|---|---|---|---|
| 悬赏骰图标 | 市场卡和未投掷类型卡 | 256x256 PNG 透明背景 | Assets/Resources/Art/DiceTypes/bounty_gold_die_icon.png | F008-02 | 已生成 |
| 顶金骰图标 | 市场卡和未投掷类型卡 | 256x256 PNG 透明背景 | Assets/Resources/Art/DiceTypes/top_gold_die_icon.png | F008-02 | 已生成 |
| 牌税骰图标 | 市场卡和未投掷类型卡 | 256x256 PNG 透明背景 | Assets/Resources/Art/DiceTypes/hand_tax_die_icon.png | F008-03 | 已生成 |
| 收账骰图标 | 市场卡和未投掷类型卡 | 256x256 PNG 透明背景 | Assets/Resources/Art/DiceTypes/collection_die_icon.png | F008-04 | 已生成 |
| 复利骰图标 | 市场卡和未投掷类型卡 | 256x256 PNG 透明背景 | Assets/Resources/Art/DiceTypes/compound_interest_die_icon.png | F008-04 | 已生成 |
| 铅票骰图标 | 市场卡和未投掷类型卡 | 256x256 PNG 透明背景 | Assets/Resources/Art/DiceTypes/lead_ticket_die_icon.png | F008-03 | 已生成 |
| 壳税骰图标 | 市场卡和未投掷类型卡 | 256x256 PNG 透明背景 | Assets/Resources/Art/DiceTypes/shell_tax_die_icon.png | F008-03 | 已生成 |
| 柜台骰图标 | 市场卡和未投掷类型卡 | 256x256 PNG 透明背景 | Assets/Resources/Art/DiceTypes/counter_gold_die_icon.png | F008-04 | 已生成 |
| 伐木骰图标 | 市场卡和未投掷类型卡 | 256x256 PNG 透明背景 | Assets/Resources/Art/DiceTypes/lumber_gold_die_icon.png | F008-04 | 已生成 |
| 源文件、接触图和最终预览 | 方向审查、后续返工 | PNG | Assets/ArtSource/Production/F008/ | 美术流程 | 已生成 |

## 风格关键词

- 明亮账本桌游风。
- 2D 扁平、清晰轮廓、浅纸质感、软阴影。
- 皇家官署、账本、封章、金币、票据、柜台。
- 轻松、机智、不幼态。

## 必须保留

- 骰子本体是第一视觉主体。
- 点数可读，不能被金币图案盖住。
- 类型差异来自骰面符号或骰体结构，不依赖生成文字。
- 结果态仍要能叠加代码绘制点数和短标签。

## 避免

- 避免赌场筹码、霓虹、写实 3D、金色豪华赌场风。
- 避免幼态吉祥物和表情脸。
- 避免大斧头、大树、柜台场景等外部物件喧宾夺主。
- 避免生成中文或英文文字。

## 生成 / 来源备注

进入美术生产时使用 `$wabish-art-production` 协调，必要时再路由到 `$wabish-art-assets`。本轮已基于九宫格接触图提取九颗最终运行时透明 PNG。

当前接触图：

```text
Assets/ArtSource/Production/F008/f008_gold_income_dice_contact_sheet_20260615.png
```

接触图状态：已完成，并已用于提取最终运行时资源。该图仍作为方向稿和返工来源，不是最终运行时资源。

最终图标预览：

```text
Assets/ArtSource/Production/F008/f008_gold_income_dice_final_icons_preview_20260615.png
```

最终资源状态：九颗 256x256 透明 PNG 已写入 `Assets/Resources/Art/DiceTypes/`，并已生成对应 `.meta` 文件。

## 放置 / 运行时用途

- 市场商品卡使用类型图标表现身份。
- 投骰前类型卡显示图标和短规则。
- 投骰结果显示运行时骰面点数，叠加小型类型色条或角标。
- 若资源缺失，程序可使用程序化占位，但验收不能视为美术完成。

## 实现备注

新增图标后必须更新 `ART_ASSETS.md`。资源命名使用 ASCII，避免中文文件名。美术不修改玩法规则、数值、市场权重或结算边界。

## 阻塞项

无美术硬阻塞。F008-01 已完成，九宫格接触图和九颗最终透明 PNG 已生成；仍需 Unity 市场卡和主流程截图确认小尺寸实际观感。

## 样张初审

- 风格适配：通过，整体符合明亮账本桌游风。
- 主体识别：通过，九颗都以骰子本体为主。
- 机制识别：通过，指定点、最高点、牌型、利息、出千、临时小骰、柜台和削分方向可读。
- 需修订点：收账骰票据面有类文字纹理，最终图标应改为纯线条、封章或账格，避免生成文字感。

## 最终资源静态检查

- 九颗最终 PNG 均为 256x256、RGBA、透明背景，并带有 Unity `.meta`。
- 最终预览图已写入 `Assets/ArtSource/Production/F008/f008_gold_income_dice_final_icons_preview_20260615.png`。
- 收账骰仍保留账页线条感，但未包含可读文字；后续如做精修，可优先替换为更抽象的账格和封章。

## 验收

- 九颗图标在 128x128 和市场卡小尺寸下仍能区分。
- 图标不含生成文字。
- 图标风格与现有 `Assets/Resources/Art/DiceTypes/` 一致。
- 每颗图标能表达触发来源：指定点、高点、牌型、收账、利息、出千、临时小骰、槽位、削点。
