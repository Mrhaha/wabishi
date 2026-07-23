# F014 程序交接

状态：新方向与术语解释已静态实现，待 Unity 运行验收
功能：骰子悬浮窗 UI
实现事实来源：Assets/Scripts/DiceKingDemo.cs
最后更新：2026-07-23

## 当前代码事实

`DiceKingDemo.cs` 已接入统一悬浮窗状态机、市场 / 主流程上下文和数据驱动术语解释：

- `TrySetHoveredTooltip(...)`
- `UpdateDiceHoverTooltipState()`
- `DrawDiceHoverTooltip()`
- `DiceHoverTooltipRect(...)`
- `DrawDiceHoverTooltipContent(...)`

既有 `0.12s` 延迟、`0.10s` 淡入、`0.08s` 淡出、`0.06s` 内容切换和阶段抑制逻辑可保留。

当前静态实现事实：

- `DiceTooltipContext` 区分市场商品、市场骰袋和主玩法，并携带实际买价。
- 面板宽度为 `448`，按内容选择 `196 / 244 / 288 / 352 / 416 / 480 / 568` 高度档。
- 六面显示逐面确定的实际生效点数：长期面组读取 `die.Faces`，再折算喂养最低点和幸运龟最大面；条件计分、邻位、钱包、遭遇和本次随机结果不进入该数组。
- 六面最大绝对值 `<10,000 / 10,000–99,999 / >=100,000` 时分别使用 `1×6` 完整整数、`3×2` 千分位整数、`3×2` 三位有效数字科学计数与中文快速读取；完整整数始终用于比较和结算。
- `dice_type_config.csv` 用 `{{kw:key}}` 引用稳定词键；定义、解释、家族色归属、排序和解释显示开关来自 `keyword_glossary_config.csv`，类型 / 状态关系来自 `keyword_binding_config.csv`。
- `market_floor / family / dual_family` 配置为 `show_explanation=0`，只参与正文替换或身份表达；底部只保留 `feed / devour / shell` 三类机制说明。
- 可见关键词在规则正文中使用所属家族色并加下划线；底部无通用说明标题，每个关键词只绘制一个家族色标签，解释另起一行。
- 龟壳类的头部 `化壳时` 标签已移除；其它触发标签继续按现有映射显示。
- 旧浅色资源与程序化路径仍保留为资源缺失回退；尚缺 Unity Play Mode 与双分辨率截图验收。

## 输入资源

新资源已生成；继续保留旧加载 key 作为回退：

| 用途 | Unity 加载 key | 状态 |
|---|---|---|
| 简版面板 | `Art/UI/Tooltip/ui_tooltip_arcade_panel_short` | 已生成 |
| 中版面板 | `Art/UI/Tooltip/ui_tooltip_arcade_panel_medium` | 已生成 |
| 复杂版面板 | `Art/UI/Tooltip/ui_tooltip_arcade_panel_tall` | 已生成 |
| 数字面格 | `Art/UI/Tooltip/ui_tooltip_arcade_face_cell` | 已生成 |
| 类型芯框 | `Art/UI/Tooltip/ui_tooltip_arcade_type_core_frame` | 已生成 |
| 骰子类型说明 | `Data/dice_type_config` | 已存在 |
| 关键词定义与解释可见性 | `Data/keyword_glossary_config` | 已接入 |
| 骰子 / 状态关键词关系 | `Data/keyword_binding_config` | 已接入 |

如果新资源缺失，仍可用程序化深色面板完成结构验收；不得回退到“把接触图整张加载”为临时方案。

## 建议视图模型

在现有单文件内部增加轻量只读展示模型，不改 `Die` 存档结构：

```text
DiceTooltipContext
- MarketOffer
- MarketBag
- Run

DiceTooltipViewModel
- Die
- Context
- BuyPrice
- DisplayName
- FamilyLabels
- TriggerLabel
- EffectiveFaces[6]
- FaceDisplayMode
- UniqueHighestFaceIndex
- RuleText
- RuleHighlights
- StateRows
- ShowBuyPrice
- ShowSellPrice
- SellPrice
- Keywords
- PanelSize
```

展示模型只能读取现有状态，不得写回骰面、成长、饲料、吞吃、吸附、价格或结算结果。

## 数据口径

| 字段 | 来源 | 规则 |
|---|---|---|
| 名称 | `DieDisplayName(die)` | 不足时回退 `TypeName(die.Type)` |
| 类型芯 | `DieTypeIcon(die.Type)` | 缺失时使用短类型名 |
| 家族标签 | 现有主体家族判定 | 双家族显示两个；吸附壳不追加宿主家族 |
| 触发标签 | 按 `DieType` 的只读展示映射 | 龟壳类返回空；不从 `tooltip_effect` 文本解析，不创建玩法触发 |
| 实际六面 | `die.Faces` + `FeedValue` + 幸运龟壳状态 | 逐面应用 `max(原面, 最低面 + FeedValue)`，有幸运龟时再与当前最高面取大 |
| 六面格式 | 六面最大绝对值 | `<10,000` 单行整数；`10,000–99,999` 两行千分位整数；`>=100,000` 两行科学计数 + 快速读取 |
| 静态规则 | `DiceTypeTooltipEffect(die)` | 不再拼接个体状态 |
| 规则关键词显示名 / 强调 | `keyword_glossary_config.csv` | 解析稳定 `{{kw:key}}`；仅可见词条按 `accent_family` 着色并加下划线 |
| 底部关键词解释 | 词典 `show_explanation / accent_family` + 绑定表 | 只显示开启项；同键去重并稳定排序；标签与解释分行 |
| 成长 | `die.Growth` | 仅大于零时显示 |
| 饲料 | `FeedStateText(die)` 或等价结构数据 | 仅有数据时显示 |
| 最近吞吃 | `die.LastDevourGain / LastDevourTrigger / LastDevourSource` | 仅加值大于零时显示 |
| 龟龟吸附 | `die.TurtleAttachments` | 合并摘要，不逐项铺满 |
| 市场买入价 | `MarketOffer.Price` | 仅 `MarketOffer` 上下文显示 |
| 当前 / 预计卖出价 | `SellPrice(die)` | 市场商品和市场骰袋显示，主玩法隐藏 |

不得读取 `die.Score` 作为悬浮窗内容；不得用 `LastValue` 或当前 `EffectiveValue` 替代逐面模拟。科学计数、千分位和快速读取只在展示层生成，不得回写 `Faces`、存档或结算字段。

## 已实现任务

1. [x] 扩展 hover 候选与活动状态，保存 `DiceTooltipContext` 和可选 `buyPrice`。
2. [x] 市场货架、市场骰袋和主投掷区分别注册正确上下文。
3. [x] 用 `BuildDiceTooltipViewModel(...)` 集中生成标签、状态行、价格、关键词和尺寸档位。
4. [x] 把静态规则与 `Growth / Feed / Devour / TurtleAttachments` 分开绘制。
5. [x] 六面改为实际生效点数数字格，并接入三档整组格式、唯一最高提示与亿级快速读取。
6. [x] 面板改为固定宽 `448` 和七档内容高度。
7. [x] 按上下文接入锚点优先级、避边、新美术资源与程序化回退。
8. [x] 接入关键词定义 / 绑定配置、占位符替换、去重排序和覆盖校验。
9. [x] 用配置字段 `show_explanation` 分离正文替换与解释可见性；隐藏 `market_floor / family / dual_family` 时没有写 C# 特例。
10. [x] 用配置字段 `accent_family` 生成关键词标签色和正文同色下划线，移除通用说明标题。
11. [x] 使用 Unity 2019.4 Roslyn 响应参数独立编译，并反射验证喂养、幸运龟和四个数字阈值。
12. [ ] Unity Play Mode、双分辨率截图及实际交互验收。

## 触发标签映射边界

首版触发标签只用于快速阅读，推荐集合为：

```text
投掷时 / 结算时 / 购买时 / 卖出时 / 刷新时 / 离场时 / 吸附时 / 常驻
```

一个骰子只显示最主要入口；多入口规则使用“多阶段”，并在静态规则中写清具体条件。所有当前可进入市场池的 `DieType` 都必须有映射或明确回退“常驻”，不能依赖类型名猜测。

## 绘制规则

- 悬浮窗继续在普通 UI 之后绘制，不接管鼠标。
- 面板、文字、数字、标签和价格共用同一个 `tooltipAlpha`。
- 标题和实际六面优先于规则，规则优先于状态，状态与价格优先于最底部关键词解释。
- 规则不得使用 `TooltipTrim(..., 112)` 之类固定字符裁切；应按实际 GUIStyle 高度测量并选择尺寸档。
- 同类状态先合并，再选择中版或复杂版；不增加滚动。
- 新点阵正文必须使用整数像素或已验证的清晰字体路径，辉光与硬核心分开。

## 保留的交互状态机

- 候选保持 `0.12s` 后进入淡入。
- 淡入 `0.10s`，离开淡出 `0.08s`。
- 已显示时切换相邻骰子使用 `0.06s` 内容替换。
- 排序拖拽、`Shaking`、`Stopping`、`Scoring`、`StageFailed` 立即清空。
- `Ready`、`ResultDecision`、兼容 `CheatEdit`、`StageClear` 可显示。

## 不改动

- 不改 `ScoreDice()`、左到右结算队列或单骰贡献反馈。
- 不改骰子随机、面组、家族效果、饲料、吞吃、吸附或市场价格公式。
- 不改 `SaveVersion`，不增加存档字段。
- 不恢复材质或词缀显示。
- 不删除旧资源回退。

## 验证

### 静态

- `git diff --check`。
- 当前可用 `DieType` 的触发标签映射无缺失。
- 新资源 key 可加载，缺失时程序化回退可用。
- 新代码不写回 `Die`、`MarketOffer` 或存档。
- 配表可见词条必须有合法 `accent_family`；隐藏词条允许留空。
- 覆盖 `9,999 / 10,000 / 99,999 / 100,000` 四个格式边界，最高判断使用完整整数。

### 运行

- 市场三个货架：买价与预计卖价正确，右侧货架向左避边。
- 市场骰袋：只显示当前卖出价。
- 主流程：不显示价格，不显示伪结果或贡献分。
- 基础、双家族、负数、`9,999 / 10,000 / 99,999 / 100,000 / 128,000,000`、喂养、吞噬、化壳和幸运龟均有覆盖。
- 科学计数主值、`万 / 亿` 快速读取、唯一最高提示、正文同色下划线和底部关键词标签均清晰；点数区不出现来源标签。
- `1280x720` 与 `1920x1080` 原始截图中文字锐利、内容不溢出。
- 延迟、淡入淡出、目标切换、拖拽隐藏和阶段抑制与旧实现一致。

## 完成后同步

- 更新 `EXECUTION_PLAN.md` 与 `ACCEPTANCE.md`。
- 如运行行为与当前主文档不同，同步 `PROJECT_CONTEXT.md` 和 `GAME_FLOW.md`。
- 新资源生成后登记 `ART_ASSETS.md`。
- 按 `Docs/UI_ACCEPTANCE_FEEDBACK_WORKFLOW.md` 保存截图与人工反馈。
