# F021 主游戏 LED 字体字形识别压力板 V3

状态：字形识别已获用户通过；混合排版基线失败并转入 V4；正式字库与 Unity 接入继续暂停
日期：2026-07-17
范围：主游戏动态 LED 中文字形识别，不包含辉光偏好、完整界面回排、骰面实体数字、关间市场、玩法、数据或存档

## 反馈来源

用户对 V2 的结论为：“清晰度没问题，但是字形无法识别内容了”。因此 V2 的原生像素、整数灯珠、无二次缩放和核心 / 辉光分层门禁保留为通过；`7×7–10×10` 中文逻辑网格作为字形方案退回。正式接入不得把“清晰”误写成“字体通过”。

## 根因

- V2 为了让所有文案继续装入旧文字框，把微软雅黑粗体的复杂中文轮廓自动压进 `7×7–10×10` 逻辑网格。
- 该方法虽然能生成边界锐利、大小一致的方形灯珠，却会合并或丢失偏旁、内腔和交叉笔画，形成“像素很清楚，但不知道写了什么”。
- V2 的自动报告只验证分辨率、采样、整数灯珠、区域装入和缺字；“字符存在于字表”不等于“字符形状可识别”。

## V3 修正范围

- 不再先做完整 Ready 界面，只做真实文案和复杂字压力板，避免把字形、布局和辉光重新混在一轮反馈里。
- 字形源改为 `Noto Sans SC` 中等字重在目标尺寸的 hinted mask；先在 `16×16` 逻辑网格直接栅格化，再二值化为 LED 开 / 关状态。输出阶段没有灰度抗锯齿。
- `1280×720` 统一使用 `2×2 px` 硬核心和 `3 px` 点距；不允许为继续装入旧框退回 `1 px` 细灯珠。
- 压力字符固定覆盖 `章 / 额 / 规 / 检 / 骰 / 袋 / 调 / 整 / 结 / 算 / 顺 / 序 / 掷`，并覆盖七组当前真实游戏文案。
- 验收板为原生 `1280×720`，无辉光、无缩放；复杂字检查图只用整数 `2× NEAREST` 放大。

## 产物

- 主验收板：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRecognitionV3/led_font_v3_recognition_pressure_board_1280x720.png`
- 1:1 盲读板：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRecognitionV3/led_font_v3_blind_reading_sheet_1to1.png`
- 复杂字 `2× NEAREST` 检查图：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRecognitionV3/led_font_v3_complex_glyphs_zoom2x_nearest.png`
- 内部字体源筛选：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRecognitionV3/_internal_font_source_probe_v3.png`，只用于制作诊断，不承担用户放行。
- 自动报告：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRecognitionV3/led_font_recognition_metrics_v3_20260717.json`
- 可复现生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_led_font_recognition_v3.py`

## 自动检查结果

- `PASS`：主验收板为原生 `1280×720`。
- `PASS`：输出灯珠为二值硬核心，没有灰度边缘、辉光或模糊滤镜。
- `PASS`：候选灯珠像素只出现两组明确 RGB 核心色；压力集 13 个复杂字拥有 13 个不同 mask，最小两两汉明距离为 `66`，不存在字形碰撞。
- `PASS`：验收板不做任何重采样；复杂字放大只使用整数 `2× NEAREST`。
- `PASS`：逻辑网格为 `16×16`，压力字符全部进入测试集。
- `PASS`：生成器可编译并可重复生成同尺寸、同哈希产物。
- `PASS`：用户确认 V3 字形通过，T1–T7 与复杂字压力集的字形身份门关闭。
- `FAIL`：用户同时指出字符排版不处于同一水平线。V3 的逐字活动边界顶对齐丢失了共享基线；该问题转入 `Docs/QA/20260717_led_font_baseline_v4.md`，不能把“字形通过”写成“字体通过”。

## 当前文字框影响

以 `1280×720`、`16×16`、`2 px / 3 px` 和紧凑字符间距回算当前区域：

| 区域 | 当前框 | 字形需求 | 结论 |
|---|---:|---:|---|
| 顶部章节 | `760×50` | `275×47` | 可保留 |
| 顶部生命 | `158×50` | `137×47` | 可保留 |
| 顶部金币 | `174×50` | `112×47` | 可保留 |
| 积分塔目标 | `88×28` | `140×47` | 必须改为标签 / 数值分层或扩区 |
| 积分塔当前 | `88×28` | `115×47` | 必须改为标签 / 数值分层或扩区 |
| 终端标题 | `162×44` | `176×47` | 扩大内宽和高度 |
| 终端正文 | `142×174` | `133×251`、5 行 | 向下使用终端空白或分页，不再缩字 |
| 排序微提示 | `570×28` | `458×47` | 增高提示带 |
| 主操作键 | `356×72` | `215×47` | 可保留 |

这不是 V3 的字形失败项，而是后续需要承担的真实布局成本。V4 基线通过后再制作一次“现有界面文字框回排图”；在此之前不生成完整 Ready 候选，不修改运行时代码。

## 本轮边界

- V3 只位于 `Assets/ArtSource`，没有运行时加载 key。
- 没有替换正式 atlas / map。
- 没有修改 `MainGameLedFont.cs`、`DiceKingDemo.cs`、玩法、数据或存档。
- 没有启动 Unity，也没有使用 Computer Use。
