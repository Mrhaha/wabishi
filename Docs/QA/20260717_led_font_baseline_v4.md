# F021 主游戏 LED 字体基线校准 V4

状态：静态与自动检查通过；等待用户确认混合排版是否处于同一水平线；正式字库与 Unity 接入继续暂停
日期：2026-07-17
范围：主游戏动态 LED 字体的纵向基线，不包含字形重画、辉光、完整界面文字框回排、骰面实体数字、关间市场、玩法、数据或存档

## 反馈来源

用户对 V3 的结论为：“字形通过，但是排榜有问题，不处于同一水平线”。因此 V3 的清晰度和字形身份保留为通过；V4 只修复中文、英文大写、数字和标点在同一行内的纵向锚点。

## 确定根因

- V3 先把每个字符裁切到自己的活动像素边界，再居中存入 `16×16` mask。
- 实际绘制时，`draw_glyph()` 又减去每字的 `min_y`，使所有字符从各自的第一行亮灯开始绘制。
- 中文活动高度通常为 `14–15` 格，英文大写和数字通常为 `12` 格。逐字顶对齐会让英文和数字底部比中文高约 `2–3` 个逻辑点距，在 720p 下形成约 `6–9 px` 的可见上浮。
- V3 的机器检查只证明复杂字 mask 不碰撞，没有记录绘制后的底行位置，所以未能提前拦截该问题。

## V4 单一修正

- 直接复用 V3 每个字符裁切后的二值活动位图；不增删灯珠，不改阈值、字重、字宽或字符步进。
- 中文、英文大写和数字统一把最后一行亮灯放到 `16×16` 字格的逻辑第 `14` 行；第 `15` 行保留为空白 / 后续扩展余量。
- `- / ·` 等悬浮标点使用固定逻辑中线，不参与底线误差统计；`， / 。` 使用统一下沿。
- 所有修正都是完整活动位图的整数逻辑行位移。720p 仍使用 `2×2 px` 灯珠和 `3 px` 点距；无缩放、无灰度抗锯齿、无辉光。

## 产物

- 主校准板：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontBaselineV4/led_font_v4_baseline_calibration_board_1280x720.png`
- V4 单候选 1:1 验收条：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontBaselineV4/led_font_v4_unified_baseline_1to1.png`
- V4 整数 `2× NEAREST` 检查图：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontBaselineV4/led_font_v4_baseline_zoom2x_nearest.png`
- 自动报告：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontBaselineV4/led_font_baseline_metrics_v4_20260717.json`
- 可复现生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_led_font_baseline_v4.py`

## 自动检查结果

- `PASS`：主校准板为原生 `1280×720`，没有发布用二次缩放。
- `PASS`：测试字符的活动位图哈希与 V3 完全一致；亮灯数完全一致。
- `PASS`：36 个中文、英文大写和数字样本的底行全部为逻辑第 `14` 行；最大偏差为 `0` 个逻辑行、720p `0 px`。
- `PASS`：所有位移均为整数逻辑行，且全部装入 `16×16` 字格。
- `PASS`：输出仍为二值硬核心，无抗锯齿、无辉光；放大图只使用整数 `2× NEAREST`。
- `PASS`：没有修改正式运行字库、`MainGameLedFont.cs`、`DiceKingDemo.cs` 或运行资源。
- `PENDING`：用户确认 V4 的中英数混排是否视觉上处于同一水平线。机器的底行一致不能替代最终视觉结论。

## 验收方式

1. 先看主校准板上方同文案对照：左侧 V3 是历史失败证据，右侧 V4 是唯一候选，不是 A / B 选型。
2. 再看 `led_font_v4_unified_baseline_1to1.png`，重点检查章节数字、金币数、目标 / 当前分、`SPACE` 和 `ABC 0123456789` 是否仍有上下跳动。
3. 绿线只用于暴露底线，不属于正式 UI。短横和间隔点应位于中线；不能要求它们落到底线。
4. 若 V4 通过，下一轮才做积分塔、终端标题 / 正文和排序微提示的一次文字框回排；若未通过，继续只改纵向锚点。

## 本轮边界

- V4 只位于 `Assets/ArtSource`，没有运行时加载 key。
- 没有生成正式 atlas / map，没有修改命名样式或 Unity 运行坐标。
- 没有启动 Unity，也没有使用 Computer Use。
- 关间市场继续排除。
