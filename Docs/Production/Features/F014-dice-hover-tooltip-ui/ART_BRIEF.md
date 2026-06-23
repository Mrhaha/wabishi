# F014 美术需求

状态：运行时 UI 素材已生成；当前实现不绘制页签
功能：骰子悬浮窗 UI
关联美术文档：PROJECT_CONTEXT.md、ART_ASSETS.md

## 输入决策

- 当前实现方向：竖版紧凑悬浮窗，不显示右侧“面 / 效 / 质”页签。
- 不要纸页厚度、账册装订、书脊、铆钉、多层纸张或翻页感。
- 商店待购骰和主投掷区骰使用同一套素材。
- 文字、点数和价格不烘焙进图片，由程序按运行时数据绘制。

## 视觉意图

悬浮窗应像轻量桌游 UI 控件，而不是一本小账册。主体是单层温暖浅色竖向面板；材质保持明亮、克制、可读，适合压在当前主场景背景上。

## 资源清单

| 资源 | 用途 | 目标路径 | Unity 加载 key | 状态 |
|---|---|---|---|---|
| 主面板 | 悬浮窗空面板，程序绘制文字和点数 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_panel_clean.png` | `Art/UI/Tooltip/ui_tooltip_panel_clean` | 已生成 |
| 侧栏细轨 | 历史页签轨道素材 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_side_rail.png` | 当前不加载 | 已生成，当前未使用 |
| 当前页签 | 历史页签底素材 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_tab_active.png` | 当前不加载 | 已生成，当前未使用 |
| 骰效页签 | 历史页签底素材 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_tab_idle_blue.png` | 当前不加载 | 已生成，当前未使用 |
| 质效页签 | 历史页签底素材 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_tab_idle_green.png` | 当前不加载 | 已生成，当前未使用 |
| 卖价胶囊 | 卖出价底和金币图形 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_price_chip.png` | `Art/UI/Tooltip/ui_tooltip_price_chip` | 已生成 |
| 骰效标签 | 骰子效果字段标签底 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_label_chip_blue.png` | `Art/UI/Tooltip/ui_tooltip_label_chip_blue` | 已生成 |
| 质效标签 | 品质效果字段标签底 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_label_chip_green.png` | `Art/UI/Tooltip/ui_tooltip_label_chip_green` | 已生成 |
| 点面格 | 单个点面格底，点数由程序绘制 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_face_cell.png` | `Art/UI/Tooltip/ui_tooltip_face_cell` | 已生成 |

## 源图和预览

- 运行时素材源图：`Assets/ArtSource/Production/Tooltip/runtime_ui_sources/`
- 运行时素材预览：`Assets/ArtSource/Production/Tooltip/tooltip_runtime_ui_assets_preview_20260616.png`
- V10 接触图：`Assets/ArtSource/Production/Tooltip/tooltip_vertical_side_tabs_clean_v10_20260616.png`
- V10 说明：`Assets/ArtSource/Production/Tooltip/tooltip_vertical_side_tabs_notes_v10_20260616.md`

## 风格关键词

- 明亮扁平 2D。
- 温暖浅色桌游 UI。
- 单层浮窗。
- 单层竖向浮窗。
- 干净、紧凑、少装饰。

## 必须保留

- 单层面板感。
- 不显示右侧页签。
- 文字和点数由程序绘制。
- 商店和投掷区共用。
- 鼠标悬浮时信息清晰，不抢主界面。

## 避免

- 不做纸页厚度。
- 不做账册装订。
- 不做书脊和铆钉。
- 不做多层纸边。
- 不做翻页动效暗示。
- 不把大段中文说明烘焙进图片。

## 生成 / 来源备注

本轮资源是基于 V10 接触图拆出的最小运行时素材包。面板当前可作为固定比例素材使用；如果程序实现时需要更灵活缩放，应优先保留程序化面板绘制，只使用胶囊和标签素材，不强行拉伸整张面板。侧栏和页签素材保留为历史资源，当前 F014 运行时不加载、不绘制。

## 验收

- 预览图可读，没有回到 V9 账册感。
- 运行时 PNG 四角透明。
- 已生成 `.meta`。
- 素材内没有业务文字和骰子点数。
- 资源已登记到 `ART_ASSETS.md`。
