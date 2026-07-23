# 关间市场浅色实体键帽文字 V2

状态：V1 因新增纯色承载层显得突兀而退回；V2 样张已于 2026-07-17 通过，允许进入 Unity。

V2 不再绘制任何纯色矩形或新铭牌。生成器从 `Assets/Resources/Art/Market/arcade_market_common_base.png` 恢复原始无字键帽，完整保留原有磨损、颗粒、凹凸和明暗，再把与 V1 相同的实心深墨字直接印在材质表面。

## 保留项

- 字形：`Noto Sans SC 600`。
- 启用字色：`#180E08`，五处一致。
- `1280×720`：购买 / 卖出 `22 px`，离开市场 `24 px`。
- `1920×1080`：购买 / 卖出 `33 px`，离开市场 `36 px`。
- 暗色刷新键、商品名、六面、反馈、资源窗和 Tooltip 继续使用 V4 LED。

## Unity 运行资源

- 使用 `build_physical_key_runtime_font_v1.py` 将批准样张所用的 600 字重固定为静态中文子集。
- 运行资源：`Assets/Resources/Art/MainGame/Flow/wabish_physical_key_sans_sc_semibold.ttf`。
- Resources key：`Art/MainGame/Flow/wabish_physical_key_sans_sc_semibold`。
- 字体族已改名为 `Wabish Physical Key Sans SC`，避免修改后的子集继续使用上游保留名称。
- 许可证：`Assets/Resources/Licenses/WabishPhysicalKeySansSC_OFL.txt`。

## V2 修订项

- 删除 V1 的 `#D6B88C` 纯色键帽内衬。
- 恢复每个键帽原始纹理和局部光影。
- 悬停只在原材质边缘加光；按下只整体压暗并位移文字；禁用仍保留纹理。
- 纹理底上的最暗 5% 区域按大号粗体门槛检查不低于 `3:1`，中位背景不低于 `4.5:1`。

## 评审文件

- `market_physical_key_labels_v2_full_1280x720.png`
- `market_physical_key_labels_v2_key_strip_1280x260.png`
- `market_physical_key_labels_v2_state_stress_1280x500.png`
- 同名 `1920×1080 / 1920×390 / 1920×750` 原生输出
- `market_physical_key_labels_metrics_v2_20260717.json`
