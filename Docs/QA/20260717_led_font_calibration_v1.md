# F021 主游戏 LED 字体校准 V1

状态：用户退回；不得用于字体选型或正式接入
日期：2026-07-17
范围：主游戏动态 LED 文字，不包含骰面实体数字、关间市场、玩法、数据或存档

## 目的

解决批准预览中的 LED 点阵字在 Unity 运行实现中变细、变小、变疏的问题，并把后续验收从 Computer Use 手工进入 Unity 截图，改为“离线同源校准 + 自动指标 + Unity 最终冒烟”。

## 2026-07-17 用户退回结论

- 用户确认 A / B 相比批准预览仍有明显差距，至少存在显示模糊，因此不能继续推进正式接入。
- V1 的机器 `PASS` 只覆盖最小灯珠尺寸、有效字高、区域装入和缺字，不能证明视觉锐度，也不能抵消人工验收失败。
- V1 对照板虽然使用 `Point` 放大局部裁切，但发布用 `1920×1080` 预览又把整张 `3840×2160` 板用 `LANCZOS` 缩小一次；标题中的“真实缩放”因此不成立。
- A / B 继续把低分辨率字图缩放到非整数目标尺寸，并叠加一次扩大后的低透明辉光；点阵核心会出现不均匀复制，辉光又削弱边界，不能作为清晰 LED 字的正式候选。
- 结论：A / B 均标记为“需重做”，V1 保留为失败证据。下一版必须提供未缩放的 1:1 原始裁切、仅用整数倍最近邻放大的像素检查图，并把核心锐度、非整数采样和辉光占比纳入自动门禁。

## 根因

当前运行字库使用 `32×32` 字图块和约 `2 px` 灯珠，但正文仍存在 `14 px` 目标字号。在 `1280×720` 下，单颗灯珠核心只有：

```text
2 × 14 ÷ 32 = 0.875 px
```

因此即使纹理使用 `Point` 采样，灯珠也会在不同位置落成零到一个物理像素，产生发细、断点和亮度不稳定。辉光只是低透明扩边，不能代替核心笔画。

## 同源输入

- 批准参考：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MotionPreviewsV1/_sources/01_ready_1920x1080.png`
- 当前运行截图：`Docs/QA/20260716_main_game_runtime_ready_1920x1080.png`、`Docs/QA/20260716_main_game_runtime_ready_1280x720.png`
- 当前运行字库：`Assets/Resources/Art/MainGame/Flow/main_game_led_font_atlas.png`
- 当前字形映射：`Assets/Resources/Art/MainGame/Flow/main_game_led_font_map.csv`
- 当前无文字底板：`Assets/Resources/Art/MainGame/arcade_main_game_common_base.png`
- 生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_led_font_calibration_v1.py`

生成器复用当前运行时字符映射、字符步距、虚拟坐标、换行、对齐、辉光比例和 `Point` 采样方式。候选帧先从实际运行截图中清除原文字，再在相同区域重绘候选，不重排其它主游戏元素。

## 候选合同

| 样式 | 当前字号 | A 推荐字号 | B 备选字号 |
|---|---:|---:|---:|
| 顶部主信息 | `22` | `28` | `26` |
| 顶部次信息 | `18` | `24` | `23` |
| 积分塔读数 | `16` | `22` | `22` |
| 终端标题 | `18` | `24` | `24` |
| 终端正文 | `14` | `22` | `22` |
| 主操作键 | `24` | `30` | `28` |
| 最小辅助字 | `14` | `22` | `22` |

候选 A 使用 `10×10` 粗点阵；最小灯珠核心为 `2.20 px`，优先接近批准预览的厚重亮字。候选 B 使用 `11×11` 紧凑点阵；最小灯珠核心为 `2.00 px`，保留更多中文字形细节，整体略克制。

## 产物

- `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV1/led_font_calibration_board_v1_1920x1080_preview_20260717.png`
- `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV1/led_font_calibration_board_v1_1280x720_preview_20260717.png`
- 两张 `3840×2160` 原始接触板，文件名中的分辨率表示被校准的实际渲染分辨率。
- A / B 两套 `1920×1080` 与 `1280×720` 完整 Ready 候选帧。
- A / B 两套源区候选字图和映射，仅用于评审，不是运行时资源。
- `led_font_calibration_metrics_v1_20260717.json`，记录物理灯珠尺寸、有效字高、区域装入结果、代表文案缺字列表、文件哈希和运行时未变更状态。

## 自动检查结果

| 检查 | 当前 | A 推荐 | B 备选 |
|---|---:|---:|---:|
| 720p 最小灯珠核心 | `0.875 px` | `2.20 px` | `2.00 px` |
| 主信息有效字高至少 `22 px` | 失败 | 通过 | 通过 |
| 正文有效字高至少 `18 px` | 失败 | 通过 | 通过 |
| 当前区域宽高装入 | 通过 | 通过 | 通过 |
| 代表文案缺字 | `0` | `0` | `0` |

机器结果：当前运行方案 `FAIL`；A、B 候选均为 `PASS`。终端压力样句直接使用当前关卡文案“无额外规则，检查基础骰袋。”，没有用候选专属或字库外字符替代实际内容。

## 视觉判断

- A 推荐：粗细和占屏面积最接近批准预览，720p 下优先级最稳；代价是复杂中文更有早期街机像素字味。
- B 备选：中文字形更规整，终端正文更容易长时间阅读；相对批准预览略薄，但明显优于当前运行方案。
- 当前运行方案不再作为可接受基准，不能只提高辉光后继续使用 `14 px` 正文。

## 本轮边界

- 没有替换 `Assets/Resources/Art/MainGame/Flow/` 中的运行字库。
- 没有修改 `MainGameLedFont.cs`、`DiceKingDemo.cs` 或表现参数资产。
- 没有启动 Unity，也没有使用 Computer Use。
- 接触板通过后，才把选定候选转为正式运行字库和命名文字样式；随后使用编辑器脚本自动生成双分辨率 Unity 冒烟截图。

## 待用户确认

请在 A 与 B 中选择一个正式运行方向，或指出只需调整哪一项：粗细 / 字号与间距 / 颜色与辉光。确认前不全局替换字库。
