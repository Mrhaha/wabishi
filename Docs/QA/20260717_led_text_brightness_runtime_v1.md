# F021 LED 语义亮度运行回归 V1

日期：2026-07-17  
版本：`F021-LEDFONT-V4-BRIGHTNESS-RUNTIME-R1`（当前未提交工作区）  
结论：自动回归通过；用户视觉确认待完成。

## 反馈与根因

用户提供的主菜单、局内 Ready、关间市场截图均来自 `1920×1080` Game View、编辑器显示比例 `0.606×`。问题由两层叠加：

1. V2 语义亮度只存在于离线样张，运行时仍有任意旧颜色、旧 Alpha 和偏小几何。
2. `0.606×` 非整数显示缩放会对点阵做二次采样，使较小、较暗的文字进一步失去灯珠面积。

本轮没有把编辑器缩放当作原生锐度基准。`1280×720 / 1920×1080` 原始 PNG 负责清晰度与装入验收；`0.606×` 派生图只负责检查最不利缩放下的信息层级与识别存活。

## 实现范围

- `MainGameLedFont` 从 `Resources/Art/MainGame/Flow/main_game_led_text_roles.csv` 读取 `10` 个语义角色；离线生成器与 C# 不再复制色值。
- 同一 `brightness_grade` 共用 `main_game_led_text_brightness_groups.csv` 的 `OKLab L` 目标；Unity 加载时会逐角色复算并校验容差，失配则字库合同无效；所有核心 Alpha 为 `1.0`。
- `L3 / L2` 只允许 `Display -> Compact`，装不下时不再静默退化为 `Micro`。
- 主菜单选项、继续游戏摘要、确认提示和实体 `SPACE`；局内顶部 HUD、目标 / 当前、终端、提示和主操作键；市场顶部 HUD、商品名、六面、刷新与反馈均改为语义角色入口。
- 市场卖出、购买和离开五处浅色实体键帽继续使用已批准的实心字与原始纹理，不改回点阵，也不增加纯色底板。

## 自动验证

- Unity Play Mode 自动截图：主游戏五状态与主菜单共 `10` 张，`1280×720 / 1920×1080` 全部通过；状态文件为 `20260716_main_game_runtime_capture_status.txt`。
- 市场 normal / purchase / leave 共 `6` 张双分辨率截图通过；状态文件为 `20260717_market_runtime_capture_status.txt`。
- `0.606×` 压力：从三张 `1920×1080` 原图以双线性方式生成 `1164×654`，覆盖 `3` 个界面、`10` 个关键模块；所有模块同时检测到正式角色核心色与缩放后的高亮色素，报告状态 `PASS`。
- 同级亮度离线合同：`320` 次绘制检查通过；同级最大目标差 `0.000045263`、最大跨度 `0.000067127`，均低于 `0.0002`。
- 浅色实体键帽回归：启用 / 禁用实心字、字高、居中、三枚购买键同线、字体哈希和原纹理方差全部通过，失败 `0`。
- Unity 2019 Roslyn 静态编译错误 `0`。本轮未使用 Computer Use。

## 证据

- 用户反馈原图：`20260717_led_brightness_feedback_{main_menu,run_ready,market}_editor_0606.png`
- 原生主菜单：`20260717_led_brightness_runtime_main_menu_{1280x720,1920x1080}.png`
- 原生局内：`20260716_main_game_runtime_{ready,result,scoring,digits}_{1280x720,1920x1080}.png`
- 原生市场：`20260717_market_runtime_{normal,purchase,leave}_{1280x720,1920x1080}.png`
- `0.606×`：`20260717_led_brightness_runtime_{main_menu,run_ready,market}_0606.png`
- 前后对照：`20260717_led_brightness_runtime_0606_before_after.png`
- 指标：`20260717_led_brightness_runtime_0606_metrics.json`

## 尚未放行

自动任务的 `PASS` 只证明资源、角色、尺寸、装入和压力检查成立，不代替用户视觉结论。待用户确认本轮三张运行结果后，才能把本项记为最终视觉通过。
