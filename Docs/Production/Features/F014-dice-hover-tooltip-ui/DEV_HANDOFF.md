# F014 程序交接

状态：程序已静态接入，待 Unity 运行验收
功能：骰子悬浮窗 UI
实现事实来源：Assets/Scripts/DiceKingDemo.cs

## 输入资源

| 用途 | Unity 加载 key |
|---|---|
| 主面板 | `Art/UI/Tooltip/ui_tooltip_panel_clean` |
| 卖价胶囊 | `Art/UI/Tooltip/ui_tooltip_price_chip` |
| 骰效标签 | `Art/UI/Tooltip/ui_tooltip_label_chip_blue` |
| 质效标签 | `Art/UI/Tooltip/ui_tooltip_label_chip_green` |
| 点面格 | `Art/UI/Tooltip/ui_tooltip_face_cell` |
| 骰子类型说明 | `Data/dice_type_config` |

UI 资源位于 `Assets/Resources/Art/UI/Tooltip/`，均已包含 `.meta`。源图位于 `Assets/ArtSource/Production/Tooltip/runtime_ui_sources/`。侧栏细轨和三种页签素材保留在资源目录中，但当前运行时不加载、不绘制。

## 接入位置建议

当前相关入口：

- 市场整体：`DrawMarket(bool chapterMarket)`。
- 市场货架：`DrawMarketOffer(Rect rect, MarketOffer offer)`。
- 当前骰袋列表：`DrawCompactDie(Rect rect, Die die, bool selected)`。
- 主投掷区：`DrawTableDice(Rect area)` 和 `DrawDiceProcessToken(...)`。
- 数据文案：`DieDisplayName()`、`FaceText()`、`DiceTypeTooltipEffect()`、`MaterialShortRule()`、`SellPrice()`。

建议新增一套局部 UI 辅助函数，不重构主流程：

- `LoadTooltipUiTextures()`。
- `TrySetHoveredTooltip(Rect rect, Die die, bool allowCurrentStateText, string sourceKey)`。
- `UpdateDiceHoverTooltipState()`。
- `DrawDiceHoverTooltip()`。
- `DrawDiceHoverTooltipContent(Rect rect, Die die, bool allowCurrentStateText)`。

## 数据口径

| 字段 | 建议来源 | 说明 |
|---|---|---|
| 名称 | `DieDisplayName(die)` | 包含材质名时直接复用 |
| 点面 | `die.Faces` | 使用长期面组，不使用本次点数替代 |
| 骰子效果 | `Assets/Resources/Data/dice_type_config.csv` 的 `tooltip_effect` | 为空或缺失时回退短提示 |
| 品质效果 | `MaterialShortRule(die.Material)` | 无材质时显示“无品质效果”或留空 |
| 卖出价 | `SellPrice(die.Type)` | 仍按类型固定回收 |

`dice_type_config.csv` 是悬浮窗展示说明表，不参与 `ScoreDice()`、市场权重、价格或存档。已有短规则不足时，后续只补这张 CSV 的 `tooltip_effect`，不改结算规则。

## 触发点

### 市场

- 在 `DrawMarketOffer()` 中对骰子商品卡区域注册 hover。
- 在 `DrawCompactDie()` 中对当前骰袋骰子行注册 hover。
- 改造道具货架不进入 F014 范围；F013 软关闭期间也不会显示。

### 主投掷区

- 在 `DrawTableDice()` 循环内，对非临时实体骰的 `dieRect` 注册 hover。
- `rollPhase` 为 `Shaking` 或 `Stopping` 时不显示悬浮窗，避免运动和揭示期间信息闪烁。
- `Ready / ResultDecision / CheatEdit / StageClear` 可显示。
- `Scoring` 默认不显示；如果后续已有明确的结算飞字完成状态，可在收束后恢复显示。
- `StageFailed` 不显示。

## 时机和渐变状态机

建议用一个轻量状态对象维护悬浮窗，不引入新 UI 框架。

| 字段 | 用途 |
|---|---|
| `hoverCandidateKey` | 当前帧鼠标压中的候选对象标识，例如货架序号、骰袋序号或投掷区槽位 |
| `hoverCandidateStartedAt` | 候选对象第一次被 hover 的时间 |
| `activeTooltipKey` | 当前已经显示或正在淡出的对象标识 |
| `activeTooltipDie` | 当前显示内容对应的骰子快照或引用 |
| `activeTooltipRect` | 当前目标区域，用于计算面板锚点 |
| `tooltipVisibleStartedAt` | 本次显示开始时间 |
| `tooltipHideStartedAt` | 本次隐藏开始时间 |
| `tooltipContentSwapStartedAt` | 已显示状态下切换目标的时间 |
| `tooltipAlpha` | 当前透明度，绘制时统一作用到面板、文字和图标 |
| `tooltipState` | `Hidden / Waiting / FadingIn / Visible / Swapping / FadingOut` |

时机常量建议先写成 `const float` 或局部配置常量，后续如果需要再进 CSV：

| 常量 | 建议值 | 说明 |
|---|---|---|
| `TooltipHoverDelay` | `0.12f` | 鼠标稳定停留后出现 |
| `TooltipFadeInDuration` | `0.10f` | 透明度从 `0` 到 `1` |
| `TooltipFadeOutDuration` | `0.08f` | 鼠标离开后的隐藏时间 |
| `TooltipContentSwapDuration` | `0.06f` | 已显示时切换目标的内容软替换时间 |
| `TooltipEnterOffsetY` | `6f` | 淡入时从目标锚点下方轻微上浮 |
| `TooltipEnterScaleFrom` | `0.98f` | 可选轻微入场缩放，不得更小 |

执行规则：

- 每帧先收集本帧 hover 候选，再在普通 UI 绘制完成后统一调用 `UpdateDiceHoverTooltipState()` 和 `DrawDiceHoverTooltip()`。
- 候选对象保持不变并超过 `TooltipHoverDelay` 后，进入 `FadingIn`。
- 鼠标离开所有候选对象后，进入 `FadingOut`。
- `FadingOut` 期间如果鼠标回到同一个对象，可以直接回到 `Visible` 或 `FadingIn`，不要先清空内容。
- `Visible` 状态下切换到另一个骰子时，不重新等待 `TooltipHoverDelay`，不重新走完整淡入；更新 `activeTooltipKey / activeTooltipDie / activeTooltipRect`，并在 `TooltipContentSwapDuration` 内完成内容替换。
- `Shaking`、`Stopping`、`StageFailed` 触发时立即清空 tooltip 状态，不播放完整淡出。
- `Scoring` 默认视为不可显示；如果程序确认结算飞字完成，可在该时刻后按普通 hover 规则重新开放。
- 时间使用不受游戏暂停或帧率尖峰影响的当前 UI 时间口径；当前原型可使用 `Time.unscaledTime`。

## 绘制规则

- 悬浮窗最后绘制，盖在普通 UI 之上。
- 悬浮窗不接管鼠标点击。
- 优先使用运行时素材；如果某个素材缺失，回退到现有程序化 `DrawUiPanel()` / `DrawUiSmallPanel()` 风格，不阻塞运行。
- 文本全部由程序绘制。
- 点面格使用 `ui_tooltip_face_cell`，点数 pip 仍由程序绘制。
- 当前实现不绘制右侧“面 / 效 / 质”页签，也不做页签点击切换。
- 绘制时把 `tooltipAlpha` 同时作用到背景、文字、点面格和价格胶囊，避免素材先出现、文字后出现。
- 淡入期间面板整体从最终位置下方 `TooltipEnterOffsetY` 上浮到最终位置；不要做大幅横向滑动。
- 若实现入场缩放，只允许 `0.98 -> 1.00` 的轻微缩放，不能做夸张弹性曲线。

## 避边规则

- 基准位置：目标区域右上方。
- 右侧空间不足时翻到左侧。
- 上方空间不足时下移。
- 不得盖住市场购买按钮、底部 `Space` 条和右侧牌型表按钮。
- 面板必须 clamp 到虚拟画布安全区内。

## 不改动

- 不改 `ScoreDice()`。
- 不改骰子随机、出千、钱包、成长或市场生成。
- 不改 `SaveVersion`。
- 不增加存档字段。
- 不新增材质或类型。
- 不重新显示 F005 词缀。

## 静态验证

- `git diff --check`。
- 资源加载 key 能全部命中或有回退。
- 无新增编译错误。
- `Resources.Load<Texture2D>("Art/UI/Tooltip/ui_tooltip_panel_clean")` 可加载。

## 运行验证

- 市场三个买入货架 hover 均显示悬浮窗。
- 当前骰袋列表 hover 显示卖价。
- 主投掷区六颗实体骰 hover 显示悬浮窗。
- 临时小骰不显示悬浮窗。
- 鼠标扫过货架不足 `0.12s` 时不弹出。
- 鼠标稳定停留后约 `0.10s` 淡入完成。
- 鼠标离开后约 `0.08s` 淡出完成。
- 已显示时切换到相邻骰子不重复等待和重淡入。
- `1280x720` 和 `1920x1080` 截图无遮挡、无文本溢出。
- 靠右货架 hover 时面板向左避边。
- `Shaking` / `Stopping` 阶段不出现闪烁 tooltip。
- `Scoring` 默认不显示 tooltip，不遮挡结算飞字。

## 完成后同步

- 已更新 `ACCEPTANCE.md`、`GAME_FLOW.md` 和 `PROJECT_CONTEXT.md` 到静态接入状态。
- 已同步 `DICE_ARCHETYPES.md` 中的悬浮窗展示数据表边界。
- UI 截图仍需按 `Docs/UI_ACCEPTANCE_FEEDBACK_WORKFLOW.md` 记录。

## 实现记录

- `DiceKingDemo.cs` 已增加统一 hover 状态机、素材加载、悬浮窗绘制和避边逻辑。
- 市场待购骰、当前骰袋列表和主投掷区实体骰已注册 hover 候选。
- 已接入 `0.12s` 停留延迟、`0.10s` 淡入、`0.08s` 淡出和 `0.06s` 目标切换软替换。
- 已按 `Ready / ResultDecision / CheatEdit / StageClear` 开放，`Shaking / Stopping / Scoring / StageFailed` 抑制。
- 已新增 `Assets/Resources/Data/dice_type_config.csv`，悬浮窗骰子效果说明改由 CSV 的 `tooltip_effect` 提供。
- 已移除运行时右侧“面 / 效 / 质”页签绘制和页签素材加载，面板宽度收敛为竖向紧凑简介。
- 本次未改变 `ScoreDice()`、骰子随机、出千、钱包、成长、市场生成、存档版本或 F005 词缀开关。
