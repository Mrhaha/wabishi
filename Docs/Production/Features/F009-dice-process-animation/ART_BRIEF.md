# F009 美术需求

状态：已接入统一资源首版，待运行验证
功能：F009 骰子过程动画表现
关联美术文档：PROJECT_CONTEXT.md、ART_ASSETS.md

## 输入决策

- F009 首版目标是固定槽位内的序列帧式骰子旋转、左到右停转显点和结算点名，不是简单 UI 形变。
- 首版优先 OnGUI 程序序列帧播放器，不因最终资源未完成阻塞。
- 美术方向必须贴合明亮账本桌游风，避免赌场、写实厚重或幼态风格。
- 骰子类型身份不能丢失，至少通过类型色条、角标、贴花或结算标签保底。

## 视觉意图

骰子要像摆在明亮桌面上的实体物件：有倒角感、清楚点数、轻微厚度、柔和阴影和落桌弹性。整体应服务“骰子落定可信”和“结算归因清楚”，不追求写实物理或夸张镜头。

## 资源清单

| 资源 | 用途 | 尺寸 / 格式 | 目标路径 | 来源决策 | 状态 |
|---|---|---|---|---|---|
| 程序序列帧骰子 | 资源缺失时的 F009 回退表现 | OnGUI 程序绘制帧 | `Assets/Scripts/DiceKingDemo.cs` 内生成 | 回退路径 | 已接入待运行验证 |
| 统一待机骰 V1 | F009 `Ready` 和结算后重置阶段的默认骰体 | 256x256 PNG | `Assets/Resources/Art/DiceRoll/f009_unified_ready_die_256.png` | 用户接受三阶段统一方向 | 已接入待运行验证 |
| 统一旋转循环 strip V1 | F009 `Shaking` 阶段固定槽位内旋转 | 24 帧横条 PNG，单帧 256x256 | `Assets/Resources/Art/DiceRoll/f009_unified_spin_loop_strip_24f_256.png` | 用户接受三阶段统一方向 | 已接入待运行验证 |
| 统一停转预览 strip V1 | F009 `Stopping` 阶段减速预览，真实点数显点前使用 | 8 帧横条 PNG，单帧 256x256 | `Assets/Resources/Art/DiceRoll/f009_unified_spin_stop_strip_8f_256.png` | 用户接受三阶段统一方向 | 已接入待运行验证 |
| 统一结果骰面 strip V1 | `ResultDecision` / `Scoring` 阶段 1 到 6 点稳定结果 | 6 帧横条 PNG，单帧 256x256 | `Assets/Resources/Art/DiceFaces/f009_unified_result_die_faces_6x256.png` | 用户接受三阶段统一方向 | 已接入待运行验证 |
| 桌面摩擦旋转 V1 loop strip | F009 `Shaking` 阶段旧版回退资源 | 24 帧横条 PNG，单帧 256x256 | `Assets/Resources/Art/DiceRoll/f009_table_friction_spin_loop_strip_24f_256.png` | 用户确认方向 | 保留为回退 |
| 桌面摩擦旋转 V1 stop preview strip | F009 `Stopping` 阶段旧版回退资源 | 8 帧横条 PNG，单帧 256x256 | `Assets/Resources/Art/DiceRoll/f009_table_friction_spin_stop_strip_8f_256.png` | 用户确认方向 | 保留为回退 |
| 统一骰子母版 V1 接触图 | 统一待机、旋转、停转、结果点面和结算点名的视觉语言，不作为运行时资源 | 16:9 PNG 源稿 | `Assets/ArtSource/Production/F009/f009_unified_die_visual_language_contact_sheet_v1_20260616.png` | 截图反馈后生成 | 已通过并拆分 |
| 统一待机阶段接触图 V1 | `Ready` / 入场阶段的槽位内骰子展示方向，不作为运行时资源 | 16:9 PNG 源稿 | `Assets/ArtSource/Production/F009/f009_unified_stage_ready_contact_sheet_v1_20260616.png` | 用户确认统一方向后生成 | 已通过并拆分 |
| 统一旋转停转阶段接触图 V1 | `Shaking` / `Stopping` 阶段固定槽位内旋转和停转方向，不作为运行时资源 | 16:9 PNG 源稿 | `Assets/ArtSource/Production/F009/f009_unified_stage_roll_stop_contact_sheet_v1_20260616.png` | 用户确认统一方向后生成 | 已通过并拆分 |
| 统一结果结算阶段接触图 V1 | `ResultDecision` / `Scoring` 阶段显点、轻抬和高亮方向，不作为运行时资源 | 16:9 PNG 源稿 | `Assets/ArtSource/Production/F009/f009_unified_stage_result_scoring_contact_sheet_v1_20260616.png` | 用户确认统一方向后生成 | 已通过并拆分 |
| 正式骰子序列帧 | 后续替换或细化 V1，表达更强倒角、厚度和类型族差异 | 后续确认，建议按类型族和最终面组织帧图 | 待定 | F009-05 | 已后置 |
| 点数贴花 / 面贴图 | 表现 1 到 6 点和 7+ 密集点阵 | 后续确认 | 待定 | F009-05 | 已后置 |
| 类型身份贴花 | 在序列帧或结果骰上保留类型身份 | 后续确认，可复用 `Assets/Resources/Art/DiceTypes/` | 待定 | F009-05 | 已后置 |
| 材质表现贴花 / shader 参数 | 表达 F002 材质，不盖过点数和类型 | 后续确认 | 待定 | F009-05 | 已后置 |
| 骰盅装饰 / 特殊事件资源 | 不作为 F009 首版默认阶段；仅保留给回退、菜单装饰或后续特殊事件 | 当前已有 `Assets/Resources/Art/dice_cup.png` | Assets/Resources/Art/dice_cup.png | 用户已要求取消默认骰盅阶段 | 已后置 |
| 桌面阴影 / 接触影 | 强化落桌和翻滚过程 | 程序绘制或后续贴图 | 待定 | F009-01 | 待执行 |

## 风格关键词

- 明亮账本桌游风
- 干净倒角骰子
- 柔和桌面阴影
- 清楚点数
- 类型身份可读
- 轻松、机灵、不幼态
- 桌面物件感

## 必须保留

- 点数可读性高于装饰。
- 类型身份至少有色条、角标或图标保底。
- 基础骰表现更安静，非基础骰身份更强。
- 当前 `Assets/Resources/Art/DiceTypes/` 的类型图标可作为身份来源。
- 当前 `Assets/Resources/Art/DiceFaces/runtime_die_face_base.png` 可作为点数美术参考。

## 避免

- 赌场霓虹、筹码、老虎机气质。
- 写实脏旧、暗色厚重、金属过度反光。
- 幼态玩具、表情化骰子、吉祥物化骰子。
- 过多生成文字。
- 大型外部道具盖过骰子本体。
- 材质或贴花遮挡点数。

## 生成 / 来源备注

首版不要求生成最终资源。F009-01 至 F009-03 先用 OnGUI 程序序列帧完成体验验证。若验证通过，再由 `$wabish-art-production` 生成正式美术资源包，并按 `$wabish-art-assets` 的方向门流程先做接触图或代表性样张。

2026-06-16：已生成统一骰子母版 V1 方向接触图，目标是把 `Ready` 类型原图、`Shaking` 序列帧、`Stopping` 停转预览、`ResultDecision` 结果点面和 `Scoring` 点名收敛成同一颗明亮账本桌游骰。该图只放在 `Assets/ArtSource/Production/F009/` 供方向评审，暂不进入 `Assets/Resources/Art/`，也不替换当前运行时 strip。

2026-06-16：已按统一母版继续生成三张阶段接触图，分别覆盖待机 / 入场、旋转 / 停转、结果 / 结算点名。三张均为源稿候选，只用于样张评审；若用户接受，下一步才拆分为运行时 loop strip、stop strip 和结果骰面母版。

2026-06-16：用户确认按三阶段统一方向替换到游戏中。已从三张接触图拆分运行资源：统一待机骰、24 帧统一旋转循环 strip、8 帧统一停转预览 strip、1 到 6 点统一结果骰面 strip，并生成运行预览图 `Assets/ArtSource/Production/F009/f009_unified_runtime_assets_preview_20260616.png` 和透明检查预览图 `Assets/ArtSource/Production/F009/f009_unified_runtime_assets_checker_preview_20260616.png`。旋转帧为保留完整运动轮廓使用软边裁切，避免浅色骰体被误抠成残片；旧桌面摩擦 strip 仍保留为代码回退资源。

2026-06-16：收到运行截图反馈：旋转动画和最后显示点数存在图像不完整问题。已重拆同名运行资源，不再使用会吃掉骰体边缘的软边透明裁切，改为完整小画幅、安全边距、统一底色和边缘清理；旋转 strip 过滤了会带相邻帧碎片的源帧，结果骰面扩大纵向安全区并整体缩小，确保 1 到 6 点稳定结果不被裁切。运行预览图和检查预览图已同步覆盖。

## 放置 / 运行时用途

- 骰子视觉层应放在主投掷区内，不遮挡左侧积分塔、资源牌、右侧规则和底部提示。
- 投骰中，骰子应固定在各自槽位内播放旋转序列帧，运动期不显示可读点数，避免提前暴露最终结果。
- 停转时，骰子应从左到右逐颗停住并显示真实点数；点数和类型身份要与 2D 结果展示一致。
- 结算点名时，当前骰可轻抬、短翻或高亮；不要让所有骰子同时闪。

## 实现备注

- 程序验证层不需要等待最终资源。
- 若后续使用 3D 模型，需确认 OnGUI 与 3D 相机的遮挡和缩放关系。
- 若后续使用正式 2D 帧动画，需提前定义每种点数和最终朝向的帧组织方式，避免资源爆炸。
- 正式资源接入后必须更新 `ART_ASSETS.md`。

## 阻塞项

当前无美术阻塞。最终模型、贴花和材质资源全部后置到程序原型通过后。

## 验收

- 程序序列帧骰子也要能读清最终点数。
- 骰子落桌、旋转、定格和点名都能被识别为实体过程，而不是普通 UI 抖动。
- 类型身份不会在结果锁定和结算点名时丢失。
- 画面风格不偏向赌场、写实暗色或幼态玩具。
- 后续正式资源进入 `Assets/Resources/Art/` 时，必须登记到 `ART_ASSETS.md`。
