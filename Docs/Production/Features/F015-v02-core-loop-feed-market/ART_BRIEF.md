# F015 美术需求

状态：无需新资源，待程序复用验证
功能：V0.2 首版核心循环、饲料和市场构筑
关联美术文档：PROJECT_CONTEXT.md、ART_ASSETS.md

## 输入决策

- 用户确认本包不生产新美术资源，首版直接复用现有资源。
- F015 需要表达四个家族：猪猪、恶魔、龟龟、海盗。
- F015 需要表达两个新运行时状态：饲料 / 最低点提升、龟龟吸附。

## 视觉意图

首版以功能验证优先，不追求正式图标辨识度。已有骰子图标、结果骰面、类型角标、短标签和悬浮窗足够支撑程序验证。正式美术在机制跑通后另开生产包。

## 资源清单

| 资源 | 用途 | 尺寸 / 格式 | 目标路径 | 来源决策 | 状态 |
|---|---|---|---|---|---|
| 现有猪猪图标 | 猪猪家族临时图标 | 现有 PNG | `Assets/Resources/Art/DiceTypes/piggy_die_icon.png` | 复用资源 | 可用 |
| 现有龟龟图标 | 龟龟家族临时图标 | 现有 PNG | `Assets/Resources/Art/DiceTypes/turtle_die_icon.png` | 复用资源 | 可用 |
| 现有金币或赌徒类图标 | 海盗 / 恶魔临时图标候选 | 现有 PNG | `Assets/Resources/Art/DiceTypes/` | 复用资源 | 待程序映射 |
| 统一待机骰 | 待机和结算后重置 | 现有 PNG | `Assets/Resources/Art/DiceRoll/f009_unified_ready_die_256.png` | 复用资源 | 可用 |
| 统一旋转和停转 | 投骰过程 | 现有 PNG strip | `Assets/Resources/Art/DiceRoll/` | 复用资源 | 可用 |
| 统一结果骰面 | 1 到 6 点结果 | 现有 PNG strip | `Assets/Resources/Art/DiceFaces/f009_unified_result_die_faces_6x256.png` | 复用资源 | 可用 |
| 高点数字骰面 | `7+` 点和饲料成长后的高点 | 运行时居中数字 | `Assets/Resources/Art/DiceFaces/runtime_die_face_base.png` | 复用资源 | 可用 |
| 现有 UI 图标 | 金币、刷新、卖出等市场按钮 | 现有 PNG | `Assets/Resources/Art/UI/` | 复用资源 | 可用 |

## 风格关键词

明亮账本桌游风、清晰骰子轮廓、短标签、类型角标、账本式反馈、低成本验证。

## 必须保留

- 不生成新图标。
- 不把短标签烘焙到图片里。
- 不新增大型外部道具构图。
- 不替换 F009 投骰资源。
- 不替换 F014 悬浮窗资源。

## 避免

- 避免在本包中重做四大家族视觉方向。
- 避免用临时图标混入 `ART_ASSETS.md` 当成最终资产。
- 避免为了表达吸附而新增复杂 UI 贴图。

## 生成 / 来源备注

本包没有美术生成任务，不调用 `$wabish-art-assets`。如果程序需要临时类型图标，可以用现有类型图标映射或程序化色条：

| 新家族 | 临时资源建议 |
|---|---|
| 猪猪 | `piggy_die_icon.png` |
| 龟龟 | `turtle_die_icon.png` |
| 恶魔 | 优先程序色条 + 短标签；也可临时复用赌徒 / 大树类图标 |
| 海盗 | 优先程序色条 + 短标签；也可临时复用金币收益族图标 |

## 放置 / 运行时用途

- 饲料状态优先用短标签，例如 `饲料+3`、`最低 4`。
- 吸附龟优先用宿主骰右上角小徽标和悬浮窗字段表达，例如 `吸附：金钱龟`。
- 海盗和恶魔首版允许只用文字短标签区分，不因图标复用影响验收。

## 实现备注

程序需要在缺失图标时保证可玩：使用程序化骰面、色条、家族短名和悬浮窗说明回退。资源缺失不应阻塞 F015 机制验证。

## 阻塞项

无。美术不阻塞本包。

## 验收

- 不新增 `Assets/Resources/Art/` 下的新资源文件。
- 新骰子即使没有专属图标，也能通过短标签和悬浮窗区分。
- 饲料和吸附状态在主流程和市场中有可读表达。
- 资源缺失时运行时不出现空白关键骰子。

## 2026-07-17 已批准的市场换肤例外

原“无需新资源 / 不新增 `Assets/Resources/Art/`”约束只描述 F015 最初的机制验证阶段。用户随后单独通过关间市场三状态换肤 V2，因此以下资源属于经批准的市场表现后续包，不代表四大家族专属图标已全面生产：

| 资源 | 路径 | 状态 |
|---|---|---|
| 三状态评审母版 | `Assets/ArtSource/Production/ArcadeDiceKing/InterStageMarketV2/arcade_dice_king_interstage_market_three_state_contact_v1_20260717.png` | 用户已通过 |
| 市场无字底板源图 | `Assets/ArtSource/Production/ArcadeDiceKing/InterStageMarketRuntimeV1/arcade_market_common_clean_plate_source_v1_20260717.png` | 已生产、已登记哈希 |
| 可复现构建脚本 | `Assets/ArtSource/Production/ArcadeDiceKing/InterStageMarketRuntimeV1/build_market_runtime_assets.ps1` | 已运行 |
| 市场运行时底板 | `Assets/Resources/Art/Market/arcade_market_common_base.png` | 已接入、Unity 已加载 |

运行时商品和骰袋继续复用主游戏中立 / 猪 / 恶魔 / 龟 / 海盗家族壳；文字、数字、骰子、价格、Tooltip、空货架、高亮、锁定、扫描和轨迹不烘焙进底板。生成提示和构建说明见同目录 `README.md`，资产登记见 `ART_ASSETS.md`。
