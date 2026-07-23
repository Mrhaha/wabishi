# LED 语义亮度 V2 同级等亮样张

状态：运行时合同源；当前截图反馈进入修复回归

V2 保留用户已经认可的 `L3 / L2 / L1 / L0 / Disabled / InkOnLight` 信息分级，只修正 V1 同一分级内不同色相亮度不一致的问题。V1 完整保留在相邻目录作为问题对照，不覆盖、不删除。

## 同级等亮合同

- 核心亮度使用 `OKLab L` 作为感知亮度指标；同一 `brightness_grade` 共用同一个目标值。
- `L3` 以已认可的琥珀 `#FFD36A` 为锚点；青色焦点、失败色和成功色只改变色相，不再形成第二套亮度层级。
- `L1` 以暖琥珀 `#D99A4A` 为锚点；暖白说明色降低到同一感知亮度。
- 单角色等级仍保留独立目标。所有核心 Alpha 固定为 `1.0`。
- 焦点辉光是独立状态层：只允许一圈 `1` 物理像素、Alpha `0.18`，且同属焦点的琥珀 / 青色参数相同；无辉光核心必须先独立成立。
- RGB 量化后，每个角色相对目标 `OKLab L` 的允许误差不超过 `0.0002`。

## 评审顺序

1. 先看 `same_grade_core`：所有样本位于同一暗底，直接比较 L3 四种色相和 L1 两种色相的核心亮度。
2. 再看主菜单、市场常态、市场 Tooltip 三张 `core` 实景图，确认同级一致后页面层级仍成立。
3. 最后看 `focus`，只判断受限焦点外沿是否保持等量；辉光不承担可读性。
4. `90pct` 仍只检查层级，不用于锐度判断。

## 文件

- `Assets/Resources/Art/MainGame/Flow/main_game_led_text_roles.csv`：离线与 Unity 共用的正式语义角色合同；本目录同名 V2 文件只保留为历史候选快照。
- `Assets/Resources/Art/MainGame/Flow/main_game_led_text_brightness_groups.csv`：正式同级等亮目标、锚点与容差；本目录 V2 文件只保留为历史快照。
- `led_text_brightness_v2_same_grade_*`：统一底色的同级比较条。
- `led_text_brightness_v2_{main_menu,market,market_tooltip}_*`：三种真实上下文的双分辨率核心 / 焦点样张。
- `led_text_brightness_metrics_v2_20260717.json`：感知亮度、对比度、几何、裁切与输出哈希报告。

样张继续留在本目录；生成器改为直接读取 `Assets/Resources/Art/MainGame/Flow/` 的正式 V4 atlas / map / geometry / 语义亮度合同，避免 Python 与 C# 各自复制色值。未调用图像生成模型或 Computer Use，也不改变玩法、数据或存档。

## 复现

在仓库根目录运行：

```powershell
python Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV2/build_led_text_brightness_v2.py
```

运行时接入后，使用真实 Unity 1080p 截图复现反馈现场的 `0.606×` 非整数显示压力：

```powershell
python Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV2/build_led_text_runtime_stress_v1.py
```

该脚本输出三张 `1164×654` 压力图、前后对照和 JSON 指标到 `Docs/QA/`。原生 720p / 1080p 仍是锐度真值；压力图只判断层级和识别生存。
