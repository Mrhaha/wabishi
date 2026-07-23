# 关间市场浅色实体键帽 V2 Unity 运行验证

日期：2026-07-17
结论：PASS

## 范围

- 用户已通过 `MarketPhysicalKeyLabelsV2`，本轮只接入卖出、三枚购买和离开市场五处浅色实体键帽。
- 暗色刷新键、商品名、六面、中央反馈、顶部资源窗和 Tooltip 继续使用 V4 LED。
- 不改变市场布局、按钮热区、价格、购买、卖出、刷新、离场、经济、随机、数据或存档。

## 运行资源

- 字体：`Assets/Resources/Art/MainGame/Flow/wabish_physical_key_sans_sc_semibold.ttf`
- Resources key：`Art/MainGame/Flow/wabish_physical_key_sans_sc_semibold`
- 家族 / 字重：`Wabish Physical Key Sans SC / SemiBold 600`
- 大小：`19,788` bytes
- SHA-256：`72e0a6b2ef7ba7adf9ee05d6c28269fa9e9e637a46ebb1c5e9525f605ac5c7c5`
- 字符：当前 51 个可达键帽字符全部包含；字体中无 `fvar`，是固定静态字重。
- 许可证：`Assets/Resources/Licenses/WabishPhysicalKeySansSC_OFL.txt`

## 实现检查

- `DrawArcadeMarketButton(..., lightKey: true)` 只路由到 `DrawArcadePhysicalKeyLabel`；暗色刷新键仍路由到 `DrawRunText` / V4 LED。
- 正常态没有绘制新的承载底板。按下与禁用只有状态性低 Alpha 材质处理，原纹理继续可见。
- 虚拟安全框先通过当前 GUI 矩阵转成屏幕物理矩形，再以单位矩阵直接栅格化，避免 720p 字图被 1.5 倍缩放到 1080p。
- 720p 购买 / 卖出 `22 px`、离开 `24 px`；1080p 对应 `33 / 36 px`。长文案只按整数物理字号向下装入。
- 资源缺失只允许安全回退并报警；`WabishMarketRuntimeCapture` 会把缺失字体判为失败。

## 自动 Play Mode 截图

- `20260717_market_runtime_normal_1280x720.png`
- `20260717_market_runtime_purchase_1280x720.png`
- `20260717_market_runtime_leave_1280x720.png`
- `20260717_market_runtime_normal_1920x1080.png`
- `20260717_market_runtime_purchase_1920x1080.png`
- `20260717_market_runtime_leave_1920x1080.png`
- 状态文件：`20260717_market_runtime_capture_status.txt`

Unity 2019.4.33f1 状态为：`PASS: six market screenshots captured at 1920x1080 and 1280x720 for normal, purchase-empty and leave-locked states.`

## 像素验证

验证脚本：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketPhysicalKeyLabelsV2/validate_market_physical_key_runtime_v1.py`
指标：`Docs/QA/20260717_market_physical_key_runtime_metrics_v1.json`

- 启用态五处均存在精确 `#180E08` 实心核心；禁用态五处均存在精确 `#30261D` 实心核心。
- 720p 核心字高：购买 / 卖出 `19 px`，离开 `22 px`；1080p：`29 / 33 px`。
- 三枚购买键文字中心线跨度：720p `0 px`，1080p `0 px`。
- 全部文字垂直居中偏差在合同阈值内，没有换行或安全框外裁切。
- 五处常态与禁用键帽的非文字亮度标准差均高于 `10`；原材质没有被纯色矩形覆盖。
- 运行字体哈希与冻结清单完全一致；重复生成哈希一致。

## 编译与工具边界

- Unity 已完成 `Assembly-CSharp.dll` 与 `Assembly-CSharp-Editor.dll` 编译，无新增错误；工程既有不可达代码警告保持不变。
- 截图由编辑器脚本和一次性请求自动完成，没有使用 Computer Use，也没有依赖人工进入 Unity 截图。
