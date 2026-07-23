# 关间市场浅色实体键帽文字 V1 样张检查

日期：2026-07-17  
状态：机器检查通过，用户视觉待验收；Unity 接入暂停

## 范围

- 替换：卖出、三枚购买、离开市场五处浅色实体键帽文字。
- 保留：暗色青色刷新键、商品名、六面、中央反馈、顶部资源窗、Tooltip 的 V4 LED。
- 不改：市场布局、按钮 Rect / 热区、商品、价格、购买、卖出、刷新、离场、经济、随机、数据、存档与 Unity 代码。

## 样张合同

- 字形：`Noto Sans SC`，可变字重 `600`，实心印刷 / 压印语法。
- 启用字色：`#180E08`；浅键底：`#D6B88C`。
- `1280×720`：购买 / 卖出 `22 px`，离开市场 `24 px`。
- `1920×1080`：购买 / 卖出 `33 px`，离开市场 `36 px`。
- 两个分辨率分别原生 FreeType 栅格化；文字绘制后没有缩放、插值或滤波。
- 禁止点阵、辉光、描边、阴影、磨损遮罩和低透明细线。

## 自动结果

| 检查 | 结果 |
|---|---|
| 启用态局部对比 | `10.0385:1`，通过 `7:1` |
| 禁用态局部对比 | `5.0193:1`，通过 `3:1` |
| 五处启用字色 / 键底一致 | 通过 |
| 双分辨率实际文案装入 | 通过；无裁切、无换行 |
| 720p 有效字高 | 购买 / 卖出 `21 px`；离开 `24 px` |
| 1080p 有效字高 | 购买 / 卖出 `33 px`；离开 `35 px` |
| 压力文案 | `购买 999 金 / 卖出 999 金 / 交互已锁定` 全部通过 |
| 输出后重采样 | 无 |

机器报告：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketPhysicalKeyLabelsV1/market_physical_key_labels_metrics_v1_20260717.json`。

## 输出

- 完整市场：`market_physical_key_labels_v1_full_1280x720.png`、`market_physical_key_labels_v1_full_1920x1080.png`
- 1:1 键帽条：`market_physical_key_labels_v1_key_strip_1280x260.png`、`market_physical_key_labels_v1_key_strip_1920x390.png`
- 状态 / 压力板：`market_physical_key_labels_v1_state_stress_1280x500.png`、`market_physical_key_labels_v1_state_stress_1920x750.png`

## 人工门禁

用户需要确认：实心字气质是否与旧街机实体键帽匹配、字重与尺寸是否合适、五处居中是否稳定、悬停 / 按下 / 禁用规则是否可接受。只有明确通过后，才固化仓库内字形并接入 `DrawPhysicalKeyLabel`；本轮没有启动 Unity，也没有使用 Computer Use。
