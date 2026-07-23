# F014 美术需求

状态：美术就绪；视觉母版与正式无字运行时素材已完成
功能：骰子悬浮窗 UI
关联美术文档：PROJECT_CONTEXT.md、ART_ASSETS.md
最后更新：2026-07-22

## 已通过视觉母版

- 源图：`Assets/ArtSource/Production/ArcadeDiceKing/DiceTooltipV1/arcade_dice_king_tooltip_three_state_contact_v1_20260717.png`。
- 验收结论：用户于 2026-07-17 回复“接受”，样张成为 F014 新方向的视觉与容量母版。
- 样张覆盖基础简版、复杂动态状态版和市场价格版，并验证双家族、负数、三位数、四位数、饲料、吞吃和吸附信息。
- 该图仍只位于 `Assets/ArtSource/`，没有 Unity 加载 key；不得直接裁切带字区域进入 `Assets/Resources/Art/`。

## 视觉意图

悬浮窗是一块安装在午夜旧机厅竞技台上的轻量检修终端，不是纸卡、账本、军工设备或现代霓虹 HUD。玩家第一眼先识别骰子类型和六面分布，第二眼读取完整规则，最后只在需要时读取个体状态和价格。

## 风格关键词

- 深海军蓝老化塑料与暗色喷漆金属。
- 烟熏玻璃、轻微旧式贴花和克制磨损。
- 琥珀点阵为主信息色，青白磷光只用于状态和选中提示。
- 锈红只用于危险或负向提示，不做整块红色警报。
- 轮廓硬朗但不过度铆钉化，保持旧娱乐街机而非工业控制台。
- 可爱但不幼态，信息密度高但仍像游戏界面。

## 布局母版

| 档位 | 1280 虚拟尺寸 | 用途 | 内容 |
|---|---:|---|---|
| 简版 | 约 `448×196` | 基础骰、无个体状态、主流程 | 头部、六面、静态规则 |
| 中版 | 约 `448×244` | 有一至两条状态或市场经济栏 | 简版内容加状态或价格 |
| 复杂版 | 约 `448×288` | 双家族、三条以上状态或复杂市场商品 | 完整状态与经济栏 |

三档宽度固定、高度分档，避免把装饰面板任意纵向拉伸。主画面与市场共用同一套框架，只改变显示模块和锚点。

## 正式运行时素材计划

以下资源已按通过的视觉母版生成并登记，程序已经静态接入；Unity 运行验收仍待执行。程序内容高度超过 `288` 时复用复杂版底件扩展到 `352 / 416 / 480`，需在双分辨率验收中特别检查纵向拉伸观感。

| 资源 | 目标文件 | Unity 加载 key | 规格 | 状态 |
|---|---|---|---|---|
| 简版无字面板 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_arcade_panel_short.png` | `Art/UI/Tooltip/ui_tooltip_arcade_panel_short` | `896×392` RGBA，按 `2×` 制作 | 已生成 |
| 中版无字面板 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_arcade_panel_medium.png` | `Art/UI/Tooltip/ui_tooltip_arcade_panel_medium` | `896×488` RGBA，按 `2×` 制作 | 已生成 |
| 复杂版无字面板 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_arcade_panel_tall.png` | `Art/UI/Tooltip/ui_tooltip_arcade_panel_tall` | `896×576` RGBA，按 `2×` 制作 | 已生成 |
| 数字面格 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_arcade_face_cell.png` | `Art/UI/Tooltip/ui_tooltip_arcade_face_cell` | `120×88` RGBA，中心深色，能承载 `-12` 与 `8888` | 已生成 |
| 类型芯框 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_arcade_type_core_frame.png` | `Art/UI/Tooltip/ui_tooltip_arcade_type_core_frame` | `128×128` RGBA，复用现有骰子类型图标 | 已生成 |

家族、触发、状态和价格胶囊优先由程序绘制可变宽底色与边框，不为每个文案生成固定贴图。分隔线和小螺钉也优先程序绘制，减少拉伸伪影和资源数量。

生产源、构建脚本、透明预览和机器指标位于 `Assets/ArtSource/Production/ArcadeDiceKing/DiceTooltipV1/RuntimeSourcesV1/`。五张运行图的尺寸均与表格一致，绿色残边自动检查为零至两个边缘像素，人工透明棋盘预览未发现可见色键残留。

## 生产边界

- 所有运行时素材必须是无字、无数字、无价格的干净底件。
- 正式素材需根据已通过样张重绘，不能从生成图硬裁。
- 三档面板四角透明，边缘无白边或色键残留。
- 数字格必须同时检查 `-12 / 888 / 8888`，不能只用 `1–6` 验证。
- 青色外轮廓保持克制，若实机抢过琥珀主信息，可降低约四分之一亮度。
- 运行时字体必须先通过无辉光硬核心清晰度，再单独添加受限辉光。

## 避免

- 羊皮纸、账本装订、书脊、卷边、印章和王冠。
- 蒸汽管线、密集铆钉、军工箱体、压力表和机械仪表盘。
- 现代玻璃拟态、赛博霓虹、全边发光和蓝紫渐变。
- 把中文规则、数字、家族名或价格烘焙进图片。
- 页签、滚动条、空材质区和无功能装饰按钮。

## 旧资源处理

`Assets/Resources/Art/UI/Tooltip/` 中现有浅色竖版资源和 `336×400` 程序布局继续作为旧版回退。新方向完成 `1280x720` 与 `1920x1080` 运行验收前不得删除、改名或覆盖旧文件；新资源使用 `ui_tooltip_arcade_*` 独立命名。

## 美术验收

- 与当前午夜旧机厅主游戏截图放在一起时材质、色温和年代感一致。
- 基础、中等、复杂三档在实际尺寸下都能一眼分清标题、六面、规则和状态。
- 面板不抢过骰子本体、目标积分和底部主操作。
- 透明边缘干净，三档固定比例不依赖危险拉伸。
- 文件名、尺寸、加载 key 和 `ART_ASSETS.md` 登记一致。
