# LED 语义亮度 V1 样张

状态：样张待验收

本目录只用于离线评审，不是 Unity 运行资源。所有样张直接读取正式 `main_game_led_font_map.csv`、`main_game_led_font_styles.csv` 与 atlas 合同，并使用当前主菜单、市场和 Tooltip 的真实底板与运行 Rect。未调用图像生成模型，也没有修改 `Assets/Resources/` 或游戏代码。

## 评审顺序

1. 先查看三个 `core` 版本。它们只有二值硬核心，负责判断字形、尺寸、层级和对比度。
2. 再查看三个 `focus` 版本。只有当前焦点允许增加一圈 1 个物理像素、Alpha `0.18` 的离散外沿；该外沿不参与可读性成立。
3. `90pct` 文件只模拟 Unity Game 视图非整数显示缩放，用于检查主次关系，不用于判断锐度。
4. `state_strip` 同屏展示焦点、主信息、辅助信息、禁用态和浅色实体键。

## 文件命名

- `led_text_brightness_v1_main_menu_*`：开始界面。
- `led_text_brightness_v1_market_*`：市场常态。
- `led_text_brightness_v1_market_tooltip_*`：市场 Tooltip。
- `led_text_brightness_v1_state_strip_*`：语义状态条。
- `led_text_brightness_metrics_v1_20260717.json`：颜色、几何、角色覆盖、对比度和文件哈希报告。

## 复现

在仓库根目录使用工作区 Python 运行：

```powershell
python Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV1/build_led_text_brightness_v1.py
```

市场商品名使用已批准方案中的 `L2 + Display`。由于原运行 Rect 只有 31px 高，样张在原货架信息区内把名称与六面行重排为 47px 名称和 31px 六面行；购买键和货架交互区域不移动。
