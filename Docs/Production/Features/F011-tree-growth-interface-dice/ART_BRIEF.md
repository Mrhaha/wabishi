# F011 美术需求

状态：最终资源已生成
功能：F011 大树长期成长接口骰
关联美术文档：PROJECT_CONTEXT.md、ART_ASSETS.md

## 输入决策

- 本包需要七颗大树重设骰的视觉方向：`点籽树 / 牌谱树 / 冠层树 / 年轮树 / 肥料树 / 修枝树 / 根系树`。
- 第一阶段已完成接触图评审；当前按用户接受的第三版接触图生成最终运行时透明图标。
- 风格延续当前明亮、扁平、账本桌游风；大树要有长期资产感，不做幼态树宠。

## 视觉意图

七颗都应一眼属于“大树 / 成长 / 长期资产”家族，同时通过符号区分触发点：

- 点籽：目标点、种子、点数标记。
- 牌谱：书页、牌型格、树皮图谱。
- 冠层：树冠、高点、向上生长。
- 年轮：年轮圈、时间、生长层。
- 肥料：金币堆、土壤、养分。
- 修枝：剪枝、出千重摇、改变枝向。
- 根系：左右根须、相邻连接、触发传导。

## 资源清单

| 资源 | 用途 | 尺寸 / 格式 | 目标路径 | 来源决策 | 状态 |
|---|---|---|---|---|---|
| F011 七颗接触图 | 方向评审 | 单张 PNG | `Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_v3_20260616.png` | 本包七颗范围 | 第三版已由用户接受 |
| 牌谱树替代小样 | 局部方向评审 | 单张 PNG | `Assets/ArtSource/Production/F011/f011_pattern_tree_replacement_contact_sheet_20260616.png` | 用户反馈第二个书本不行 | 中间方案已确认，用于第三版 |
| 点籽树最终图标 | 运行时骰子类型图标 | 256x256 PNG 透明背景 | `Assets/Resources/Art/DiceTypes/point_seed_tree_die_icon.png` | 点籽规则卡 | 已生成 |
| 牌谱树最终图标 | 运行时骰子类型图标 | 256x256 PNG 透明背景 | `Assets/Resources/Art/DiceTypes/pattern_tree_die_icon.png` | 牌谱规则卡 | 已生成 |
| 冠层树最终图标 | 运行时骰子类型图标 | 256x256 PNG 透明背景 | `Assets/Resources/Art/DiceTypes/canopy_tree_die_icon.png` | 冠层规则卡 | 已生成 |
| 年轮树最终图标 | 运行时骰子类型图标 | 256x256 PNG 透明背景 | `Assets/Resources/Art/DiceTypes/ring_tree_die_icon.png` | 年轮规则卡 | 已生成 |
| 肥料树最终图标 | 运行时骰子类型图标 | 256x256 PNG 透明背景 | `Assets/Resources/Art/DiceTypes/fertilizer_tree_die_icon.png` | 肥料规则卡 | 已生成 |
| 修枝树最终图标 | 运行时骰子类型图标 | 256x256 PNG 透明背景 | `Assets/Resources/Art/DiceTypes/pruning_tree_die_icon.png` | 修枝规则卡 | 已生成 |
| 根系树最终图标 | 运行时骰子类型图标 | 256x256 PNG 透明背景 | `Assets/Resources/Art/DiceTypes/root_tree_die_icon.png` | 根系规则卡 | 已生成 |

## 风格关键词

- 明亮扁平 2D。
- 桌游账本感。
- 树木资产、印章、图谱、木纹、金币肥料、根系连接。
- 可爱但不幼态。

## 必须保留

- 骰子本体必须清楚。
- 类型符号不能靠文字说明。
- 小尺寸下仍能区分七颗。
- 大树家族感一致，但每颗触发点有不同符号。

## 避免

- 不做写实阴暗森林。
- 不做幼儿绘本树宠。
- 不做赌场霓虹和扑克牌赌场风。
- 不把规则文字烘焙进图标。
- 不把肥料树画成直接吐金币，避免误读为金币收益骰。

## 生成 / 来源备注

接触图阶段已完成方向和可读性验证。当前已按用户接受的第三版接触图提取七颗 256x256 透明 PNG，并登记 `ART_ASSETS.md`。

当前通过样张：`Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_v3_20260616.png`。该图只作为方向来源，不进入运行时资源目录；第二颗牌谱树已改为用户确认的中间格纹骰面方案。

最终预览：`Assets/ArtSource/Production/F011/f011_tree_growth_interface_final_icons_preview_20260616.png`。七颗最终透明 PNG 已放入 `Assets/Resources/Art/DiceTypes/`，源裁切图保留在 `Assets/ArtSource/Production/F011/final_icon_sources/`。

局部修订样张：`Assets/ArtSource/Production/F011/f011_pattern_tree_replacement_contact_sheet_20260616.png`。该图只用于选择牌谱树替代方向，不进入运行时资源目录；用户已确认中间方案可用。

历史样张：`Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_20260616.png`。用户反馈七颗太类似，已用第二版提高轮廓、主色和机制符号区分度。

历史样张：`Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_v2_20260616.png`。第二版六颗方向可保留，但第二颗书本 / 账本式牌谱树不通过，已由第三版替换。

## 放置 / 运行时用途

- 接触图放在 `Assets/ArtSource/Production/F011/`。
- 最终运行时图标放在 `Assets/Resources/Art/DiceTypes/`。
- 最终图标由程序按类型 key 加载，资源缺失时可临时回退到程序化占位。

## 实现备注

美术不改玩法规则、不改 CSV、不改代码。

## 阻塞项

- 美术侧无阻塞。
- 程序已静态接入七颗最终图标；仍需 Unity 截图验收确认市场卡、骰袋卡和桌面短标签可读。

## 验收

- 七颗接触图能区分触发点。
- 视觉统一属于大树家族。
- 无规则文字烘焙。
- 肥料、根系、修枝不会和金币收益族、龟龟临时小骰或奇偶短规则骰混淆。
- 七颗最终图标为 256x256 PNG，四角透明，`.meta` 已生成。
