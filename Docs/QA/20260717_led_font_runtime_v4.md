# F021 全局 LED 文字标准 V4 运行接入验证

日期：2026-07-17

结论：用户已通过 V4 字形与统一基线，并指定其为默认全局文字标准。正式运行 atlas / map、三档命名样式和全局绘制入口已接入；离线双分辨率真值检查与独立 Unity Roslyn 编译通过。未使用 Computer Use，也未把本轮离线检查冒充为 Unity Play Mode 人工交互验收。

## 全局合同

- 默认范围：主菜单、开场、主流程、设置、关间市场、悬浮窗、结果页、按钮、提示与旧回退界面的所有动态文字。
- 字形：沿用用户通过的 V3 `16×16` Noto Sans SC 中等字重二值活动字形。
- 基线：沿用用户通过的 V4 规则；中文、英文大写和数字落到逻辑第 `14` 行，短横与间隔点使用固定中线。
- 输出：整数物理像素、`Point` 采样、无抗锯齿、无运行辉光、无任意比例字图缩放。
- 明确例外：骰面当前点数保留非自发光实体压印数字；顶部 `DICE KING` 保留已单独批准的实体点阵招牌及其供电演出。
- 本轮对关间市场只统一文字渲染，不改变市场布局、流程或交互规则。

## 运行资源

| 资源 | 路径 / 加载 key | 结果 |
|---|---|---|
| 正式二值字形 atlas | `Assets/Resources/Art/MainGame/Flow/main_game_led_font_atlas.png` / `Art/MainGame/Flow/main_game_led_font_atlas` | `576×486`，当前 `850` 字形，Alpha 仅 `0 / 255` |
| 正式字形 map | `Assets/Resources/Art/MainGame/Flow/main_game_led_font_map.csv` / `Art/MainGame/Flow/main_game_led_font_map` | 当前 `850` 行，无重复 codepoint；每字 `16` 行位图；生成器按当前代码 / 数据字符动态重算 |
| 三档共享样式合同 | `Assets/Resources/Art/MainGame/Flow/main_game_led_font_styles.csv` / `Art/MainGame/Flow/main_game_led_font_styles` | `Display / Compact / Micro` 共 `3` 行；离线生成器输出、C# 运行时直接读取 |
| 可复现生成器 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_main_game_led_font_atlas.py` | 直接复用 V3 / V4 字形合同，并扫描当前 `Assets/Scripts` 与 `Assets/Resources/Data` 字符 |
| 自动指标 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRuntimeV4/led_font_runtime_metrics_v4_20260717.json` | 全部检查通过 |

720p 命名样式只改变整数灯珠密度，不改变字形或基线：

| 样式 | 灯珠 | 点距 | 字间 | 字格高度 | 用途 |
|---|---:|---:|---:|---:|---|
| `Display` | `2 px` | `3 px` | `4 px` | `47 px` | 顶部主信息、主操作、大结果 |
| `Compact` | `1 px` | `2 px` | `2 px` | `31 px` | 常规标签、终端标题、按钮 |
| `Micro` | `1 px` | `1 px` | `1 px` | `16 px` | 既有小型辅助行；仍为同一二值字形，不做滤波缩放 |

运行时按语义字号先选择较大命名样式，再按真实 Rect 装入结果降到下一档；不存在任意连续字号或整张字图非整数缩放。

## 代码审计

- `MainGameLedFont.cs` 不再把旧 `32×32` atlas 整块缩放到任意字号。它从 map 读取逐行二值位图，按当前 GUI 矩阵换算真实屏幕像素，逐灯珠整数取整后缓存为 1:1 文本纹理。
- 三档灯珠几何不再在 Python 与 C# 各写一份常量：生成器写出共享样式 CSV，运行时加载同一文件；离线上下文合成也执行和 Unity 相同的物理像素文字框裁切。
- `DiceKingDemo.cs` 的 `87` 个动态标签 / 开关文案入口统一经过 `DrawStandardLabel`；主菜单、设置、主流程、新旧市场、Tooltip、最终结果和旧回退 UI 均覆盖。
- 原始 `GUI.Label` 只剩 `3` 处白名单：骰面实体数字阴影 / 本体两次绘制，以及字库资源导入失败时的安全回退。原始 `GUI.Toggle` 只剩一个无文字控制框，文字由标准入口另绘。
- 原始 `GUI.Button` 只承担 `GUIContent.none` 热区，不直接携带文字；没有 `GUI.Box / Window / SelectionGrid / Toolbar / TextField / TextArea / PasswordField / GUILayout` 文字旁路。
- `Assets/Scenes` 与 `Assets/Resources` 中没有序列化 `Text / TextMeshPro / TMP_Text` 组件旁路；审计覆盖全部 `Assets/Scripts/*.cs`，不只检查单一文件。
- 主菜单源底图烘焙的旧平滑菜单文字已由不透明动态内容层覆盖；`DICE KING` 招牌保留为明确例外，未出现新旧菜单文字叠影。

## 离线真值产物

所有文字都由正式 map 和与 C# 相同的三档整数几何生成；背景缩放不参与文字锐度验收。

- `led_font_runtime_v4_truth_1280x720.png`
- `led_font_runtime_v4_truth_1920x1080.png`
- `led_font_runtime_v4_main_menu_1280x720.png`
- `led_font_runtime_v4_main_menu_1920x1080.png`
- `led_font_runtime_v4_run_ready_1280x720.png`
- `led_font_runtime_v4_run_ready_1920x1080.png`
- `led_font_runtime_v4_market_1280x720.png`
- `led_font_runtime_v4_market_1920x1080.png`

以上文件统一位于 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRuntimeV4/`。

## 自动检查

- `PASS`：V4 已批准样本的活动字形哈希保持不变。
- `PASS`：批准样本底线偏差为 `0`。
- `PASS`：atlas 二值 Alpha、map 行数、codepoint 唯一性和每字 `16` 行位图通过。
- `PASS`：共享样式合同包含且只服务 `Display / Compact / Micro` 三档；运行时与离线生成器不再维护两套样式常量。
- `PASS`：当前代码 / 数据字符全部进入 map；缺字数为 `0`。
- `PASS`：`1280×720 / 1920×1080` 真值图均为原生输出；无辉光、无抗锯齿，只做物理像素取整。
- `PASS`：`git diff --check` 通过。
- `PASS`：Unity 2019.4.33f1 随附 Roslyn 独立编译全部 `Assets/Scripts/*.cs`，`0 error`；保留工程已有 `7` 条 `CS0162` 不可达代码警告。
- `PENDING`：当前已打开的 Unity Editor 尚未重新获得焦点并完成本轮资源导入，因此没有宣称最新 Play Mode 画面、鼠标热区或 Tooltip 交互人工通过。

## 复现命令

```powershell
& 'C:\Users\admin\.cache\codex-runtimes\codex-primary-runtime\dependencies\python\python.exe' `
  'Assets\ArtSource\Production\ArcadeDiceKing\MainGameFlowV1\RuntimeTools\build_main_game_led_font_atlas.py'
```
