# 美术资源登记表

本文件记录已生成美术资源、项目路径、预期 Unity `Resources.Load` 加载 key 和替换备注。

## 当前风格

- 技能：`$wabish-art-assets`
- 风格指南：`C:\Users\admin\.codex\skills\wabish-art-assets\references\art-style-guide.md`
- 当前屏幕级风格名：`Pixel Ledger Scoring Machine` / `像素账本计分机风`
- 继承基底：`Bright Ledger Boardgame` / `明亮账本桌游风`
- 确认时间：2026-07-06
- 适用范围：后续主流程、结算、市场、主菜单 / 主画面等屏幕级 UI 与表现层，默认以 `1920x1080`、`16:9` 横版构图生产。
- 风格状态：方向已确认，运行时资源待拆分；现有 `Bright Ledger Boardgame` 骰子图标、羊皮纸 UI、小图标和旧运行时资源继续作为兼容基底。

## 已确认屏幕风格参考

以下图片是接触图 / 构图参考，位于 `Assets/ArtSource/`，不得直接作为运行时 `Resources.Load` 资源使用。后续若拆成背景、UI 框、灯泡、指针、金币口、市场货架等运行时素材，需要另存到 `Assets/Resources/Art/` 并在本文件登记加载 key。

| 流程 | 参考图 | 状态 | 备注 |
|---|---|---|---|
| 流程总览 | `Assets/ArtSource/Production/FlowContactSheets/flow_overview_contact_sheet_20260706_1602_1920x1080.png` | 风格参考 | 汇总主菜单、主流程、结果决策、结算爆分、关间市场的统一方向。 |
| 主菜单 / 主画面 | `Assets/ArtSource/Production/FlowContactSheets/flow_01_main_menu_20260706_1546_1920x1080.png` | 风格参考 | 皇家账本计分机的首屏气质参考。 |
| Ready 主流程 | `Assets/ArtSource/Production/FlowContactSheets/flow_02_ready_main_run_20260706_1548_1920x1080.png` | 风格参考 | 六槽骰台、左侧积分塔、右侧规则牌和底部短 `Space` 条的布局参考。 |
| ResultDecision | `Assets/ArtSource/Production/FlowContactSheets/flow_03_result_decision_20260706_1551_1920x1080.png` | 风格参考 | 结果锁定但未真实入账的可读状态参考。 |
| Scoring / 爆分 | `Assets/ArtSource/Production/FlowContactSheets/flow_04_scoring_burst_20260706_1555_1920x1080.png` | 风格参考 | 左到右入账、达标指针摇晃、灯泡爆亮和目标收束的演出参考。 |
| InterStageMarket | `Assets/ArtSource/Production/FlowContactSheets/flow_05_interstage_market_20260706_1557_1920x1080.png` | 风格参考 | 三格货架、骰袋和市场反馈的独立市场屏参考。 |

### 待确认的街机主题方向探索

以下图片是 2026-07-13 针对“骰子王 = 六骰街机巡回赛冠军称号”的新主题接触图。它们只用于用户选型，尚未替换上方已确认屏幕风格，也不得作为运行时资源使用。

| 方向 | 参考图 | 状态 | 备注 |
|---|---|---|---|
| A 明亮城市巡回 | `Assets/ArtSource/Production/ArcadeDiceKing/arcade_dice_king_direction_a_city_circuit_contact_sheet_v1_20260713.png` | 待选型 | 并排展示简化开始界面与 Ready；明快、玩法辨识稳定，需补清目标 / 当前双数字并弱化赛车联想。 |
| B 午夜旧机厅 | `Assets/ArtSource/Production/ArcadeDiceKing/arcade_dice_king_direction_b_last_arcade_contact_sheet_v1_20260713.png` | 已选为深化母体 | 用户选择继续深化该方向；原图中的 `骰子 x3`、赔付表式图标和皇冠不得继承到精修版。 |
| C 霓虹大秀 | `Assets/ArtSource/Production/ArcadeDiceKing/arcade_dice_king_direction_c_neon_revue_contact_sheet_v1_20260713.png` | 待选型 | 最贴电子百老汇感；需补清目标 / 当前与一次投掷，并控制节奏游戏联想。 |

三版共同警告：皇冠目前仅意指排行榜冠军标志，但仍可能重新触发王室联想；选定方向后的精修版应限制其使用次数，或改为高分名牌 / 六点星 / 冠军灯牌。

#### 午夜旧机厅开始界面细化样张

以下三张图片只比较单台机承载开始、继续、设置、退出的方式，均为 `1672x941` 不透明源图，不是运行时素材。

| 方向 | 参考图 | 状态 | 备注 |
|---|---|---|---|
| A CRT 存档槽 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuBRefinement/arcade_dice_king_b_main_menu_refine_a_crt_save_slot_v1_20260713.png` | 结构方向已选、待运行时试制 | 用户选择作为 UI 切图和主菜单完善母版；正式资源必须重做 clean plate，不直接硬裁该样张。 |
| B 机框系统键 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuBRefinement/arcade_dice_king_b_main_menu_refine_b_bezel_system_controls_v1_20260713.png` | 未选用 | 物理街机可信度强，但焦点跨越屏幕与实体机框；继续保留为外壳气味参考。 |
| C 机械记忆终端 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuBRefinement/arcade_dice_king_b_main_menu_refine_c_mechanical_memory_terminal_v1_20260713.png` | 不建议继续结构 | 黑玻璃与机械分段材质可保留，但双槽像工业终端或硬盘仓，并暗示不存在的插卡操作。 |

共同生产备注：生成图中的空白线、背景招牌和输入符号仅为构图占位，最终中文菜单、存档摘要、覆盖确认和输入提示必须由 Unity 绘制；A 已被用户选作结构母版，但正式拆件仍需等待细化稿选型。

#### A 结构三版细化接触图（未切图）

以下三张图片都沿用 A「CRT 存档槽」的同一机台与菜单结构，只比较 CRT 表现、信息密度和有 / 无存档状态。均为 `1672x941`、`Format24bppRgb` 不透明源图，不是运行时素材。

| 方向 | 参考图 | 状态 | 备注 |
|---|---|---|---|
| A 暖琥珀存档卡 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuARefinementV2/arcade_dice_king_main_menu_a_refine_v2_amber_save_card_20260713.png` | 未选用、保留参考 | 有存档；暖琥珀双线焦点最明确，摘要含“从本关开始”，继续游戏、两行摘要与次入口的阅读顺序最直接。 |
| B 冷青档案终端 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuARefinementV2/arcade_dice_king_main_menu_b_refine_v2_teal_archive_20260713.png` | 未选用、保留参考 | 有存档；冷青磷光承载信息、琥珀承载焦点；已删除容易像进度或节拍轨的顶部刻度。 |
| C 首次启动 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuARefinementV2/arcade_dice_king_main_menu_c_refine_v2_first_run_20260713.png` | 已选为氛围细化母版 | 无存档；用户于 2026-07-14 确认继续细化。开始新游戏升为主卡，继续游戏在相同位置禁用；文案明确六骰挑战，主图标为新记录符号。 |

共同生产备注：本轮只生成完整接触图，没有 clean plate、透明切片、状态图或 Unity 加载 key；C 虽已选定，但仍不得直接从含文字、扫描线、反光和焦点光的样张硬裁运行时资源。下一门禁为 C 氛围定稿关键帧和六格动效状态图，评审通过后再重绘 clean plate 与动态层。

#### C 常态雨夜氛围预览

| 资源 | 参考图 | 状态 | 备注 |
|---|---|---|---|
| C 常态雨夜氛围关键帧 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_atmosphere_preview_v1_20260714.png` | 用户已接受，可进入动态预览 | `1672x941`、`Format24bppRgb` 不透明 PNG；强化右侧窗雨、玻璃水珠 / 流痕、湿地冷暖反射与稳定 CRT 常态，不含动态时序，不是运行时资源。 |
| C 动态氛围预览 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_motion_preview_v1_20260714.mp4` | 用户已拒绝、保留历史记录 | `1920x1080`、30 fps、30 秒、H.264 High、无声；用户认为多种效果同片且吊灯只做局部压暗，无法形成可信断电感，不作为后续资源拆分依据。 |
| C 动态预览海报 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_motion_preview_v1_poster_20260714.png` | 随附评审图 | 从正式视频常态段抽取的 `1920x1080` PNG，用于文件预览，不是额外视觉方向。 |
| C 动态合成规则 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/motion_preview_filtergraph_v1.ffgraph` | 生产源文件 | 记录预览中的雨层、流滴、灯光掉亮、CRT 底噪和异常时序；只用于复现导演样片，不是 Unity 运行配置。 |
| 吊灯灯灭源状态 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_lamp_off_state_v1_20260714.png` | 用户已接受、已派生运行时层 | 通过内置图像编辑生成；运行时没有直接加载该完整编辑图，只使用其左侧受光区域派生灯灭底图，避免中央 UI 和文字变化进入游戏。 |
| 吊灯断电专用预览 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_lamp_power_cut_preview_v1_20260714.mp4` | 用户已接受、已进入 Unity 实现 | `1920x1080`、30 fps、5 秒、H.264 High、无声；用户评价“还可以”并批准按该节奏接入 Unity。 |
| 吊灯断电评审图 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_lamp_power_cut_review_sheet_v1_20260714.png` | 随附评审图 | 六帧依次展示常亮、熄灭中、停黑、灯芯复亮、环境恢复和稳定常亮。 |
| 吊灯专用合成规则 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/lamp_power_cut_preview_filtergraph_v1.ffgraph` | 生产源文件 | 记录左侧软遮罩、5 秒单效果时序和灯芯 / 近场 / 环境的差速恢复，只用于导演样片。 |
| 右窗局部干净基底 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_window_clean_base_v1_20260714.png` | V1 需修订、保留对照 | `1920x1080`；只清理了玻璃内部，没有移除窗框内沿固定白点，不作为后续生产依据。 |
| 右窗透明干净补片 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_window_clean_patch_v1_20260714.png` | V1 需修订、保留对照 | `405x900` RGBA 局部补片；存在同样的边沿白点问题。 |
| 右窗雨幕与汇流专用预览 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_rain_window_preview_v1_20260714.mp4` | 用户要求修订、保留对照 | `1920x1080`、30 fps、8 秒、H.264 High、无声；用户认为窗外降雨和玻璃汇流没有形成两个清楚效果。 |
| 右窗六帧全景评审图 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_rain_window_review_sheet_v1_20260714.png` | 用户要求修订、保留对照 | 记录 V1 全景节点与问题，不再作为通过候选。 |
| 右窗六帧局部评审图 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_rain_window_detail_review_sheet_v1_20260714.png` | 用户要求修订、保留对照 | 清楚暴露窗框内沿固定白点与玻璃流痕主导的问题。 |
| 右窗清理与动态合成规则 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/window_clean_patch_filtergraph_v1.ffgraph`、`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/rain_window_preview_filtergraph_v1.ffgraph` | 历史生产源文件 | 只用于追溯 V1，不是 Unity 运行配置。 |
| 右窗边沿去点干净基底 V2 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_window_clean_base_v2_20260715.png` | 用户已接受、保留生产源 | `1920x1080` RGBA；在玻璃内部清理基础上进一步去除顶部、底部和左侧内沿连续白点，整屏其它区域保持母版状态。 |
| 右窗边沿去点补片 V2 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_window_clean_patch_v2_20260715.png` | 用户已接受、已派生运行时补片 | `405x900` RGBA；窗外雨幕 V3 已获接受，因此作为正式干净窗体补片的生产源保留。 |
| 窗外持续降雨预览 V2 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_outside_rain_preview_v2_20260715.mp4` | 用户要求修订、保留对照 | `1920x1080`、30 fps、6 秒、H.264 High、无声；纵向滚动方向写反，实际表现为向上运动，不再作为通过候选。 |
| 窗外持续降雨评审图 V2 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_outside_rain_review_sheet_v2_20260715.png` | 历史评审图 | 六个全景节点，用于追溯 V2 雨幕权重与反向运动问题。 |
| 窗外持续降雨预览 V3 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_outside_rain_preview_v3_20260715.mp4` | 用户已接受、背景基准冻结 | `1920x1080`、30 fps、6 秒、H.264 High、无声、180 帧；用户确认窗外雨没有问题，向下方向、密度、远近速度差、长度和透明度可作为玻璃层评审背景。 |
| 窗外持续降雨全景评审图 V3 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_outside_rain_review_sheet_v3_20260715.png` | 已接受随附评审图 | `1472x564` 六帧全景图，用于检查菜单、标题、吊灯和机台在雨幕运动中保持稳定。 |
| 窗外持续降雨局部评审图 V3 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_outside_rain_detail_review_sheet_v3_20260715.png` | 已接受随附评审图 | `1247x1544` 六帧右窗局部图，用于检查远 / 近雨层次、窗框内缩边界与白点问题。 |
| 窗外持续降雨合成规则 V3 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/outside_rain_preview_filtergraph_v3.ffgraph` | 生产源文件 | 只把 V2 两层雨幕的纵向滚动从正值改为负值；不是 Unity 运行配置。 |
| 玻璃撞击与汇流预览 V2 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_glass_flow_preview_v2_20260715.mp4` | 已被 V3 替代、保留对照 | `1920x1080`、30 fps、10 秒、H.264 High、无声；玻璃事件可追溯，但背景窗外雨仍为向上运动，不再作为验收候选。 |
| 玻璃撞击与汇流评审图 V2 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_glass_flow_review_sheet_v2_20260715.png` | 历史评审图 | 六个全景节点，用于追溯 V2 玻璃事件。 |
| 玻璃撞击与汇流预览 V3 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_glass_flow_preview_v3_20260715.mp4` | 条件视觉基准、固定四滴不作为终版 | `1920x1080`、30 fps、10 秒、H.264 High、无声、300 帧；水珠材质、撞击、停滞和汇流语言继续使用，固定四滴与固定路径已由 Unity 随机多点系统替代。 |
| 玻璃撞击与汇流全景评审图 V3 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_glass_flow_review_sheet_v3_20260715.png` | 条件视觉基准随附图 | `2880x1080` 六节点全景图，依次覆盖背景、撞击、附着、初流、停滞和长流；只评材质与节奏，不定义运行时分布。 |
| 玻璃撞击与汇流局部评审图 V3 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_glass_flow_detail_review_sheet_v3_20260715.png` | 条件视觉基准随附图 | `1215x1520` 六节点右窗局部图；固定数量与位置已废止，保留撞击辨识和水流质感参考。 |
| 玻璃撞击与汇流合成规则 V3 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/glass_flow_preview_filtergraph_v3.ffgraph` | 生产源文件 | 相对 V2 只把窗外远 / 近雨改为向下；玻璃层参数保持一致，不是 Unity 运行配置。 |
| 右窗双效果局部评审图 V2 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_rain_two_effect_detail_review_sheet_v2_20260715.png` | 随附评审图、待验收 | 上排为窗外降雨三个节点，下排为撞击、附着和汇流三个节点，统一采用已去白点的窗框。 |
| 右窗 V2 清理与双效果合成规则 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/window_clean_patch_filtergraph_v2.ffgraph`、`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/outside_rain_preview_filtergraph_v2.ffgraph`、`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/glass_flow_preview_filtergraph_v2.ffgraph` | 生产源文件 | 分别记录边沿去点、窗外降雨和玻璃撞击汇流的可复现合成规则；不是 Unity 运行配置。 |
| DICE KING A 点阵补亮动态预览 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_preview_v1_20260715.mp4` | 需修订、保留对照 | `1920x1080`、30 fps、5 秒、H.264 High、无声；用户认为完整事件过快，无法表现停灭和电流不稳定，不再作为通过候选。 |
| DICE KING A 点阵补亮局部循环 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_preview_v1_20260715.gif` | 需修订、保留对照 | `1020x180`、20 fps、5 秒循环；只用于对照 V1 过快问题。 |
| DICE KING A 点阵补亮评审图 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_review_sheet_v1_20260715.png`、`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_detail_review_sheet_v1_20260715.png` | 需修订、保留对照 | 六节点全景与顶箱局部对照；用于追溯 V1 过快和单点压暗问题，不作为运行时贴图。 |
| DICE KING A 点阵补亮合成规则 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/dice_king_marquee_a_dot_relight_preview_filtergraph_v1.ffgraph` | 生产源文件 | 记录顶箱裁区、灯珠色域、四点短暗、`DICE → KING` 峰值与恢复时序；不是 Unity 运行配置。 |
| DICE KING A 点阵补亮动态预览 V2 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_preview_v2_20260715.mp4` | 用户已接受、运行时节奏基准 | `1920x1080`、30 fps、6 秒、H.264 High、无声；约 `1.63s` 供电波动，最低亮度约 `20%`，包含近灭停留、两次失败回亮、`DICE → KING` 分区补亮和缓慢收束。 |
| DICE KING A 点阵补亮局部循环 V2 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_preview_v2_20260715.gif` | 用户已接受、局部节奏基准 | `1020x180`、20 fps、6 秒循环；放大顶箱以检查停灭、电流波动和分区复亮，不含随机单颗坏点。 |
| DICE KING A 点阵补亮评审图 V2 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_review_sheet_v2_20260715.png`、`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_detail_review_sheet_v2_20260715.png` | 用户已接受随附图 | 八节点全景与顶箱局部对照，依次检查常亮、两段压暗、近灭、失败回亮、分区补亮和完整恢复；不作为运行时贴图。 |
| DICE KING A 点阵补亮合成规则 V2 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/dice_king_marquee_a_dot_relight_preview_filtergraph_v2.ffgraph` | 生产源文件 | 记录 `DICE / KING` 分区亮度曲线和 V2 参数基准；不是 Unity 运行配置。 |
| DICE KING 运行时分层合成规则 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/dice_king_marquee_runtime_layers_filtergraph_v1.ffgraph` | 已批准、生产源文件 | 从已接入常亮底图的固定顶箱裁区派生暗铜覆盖层和透明琥珀辉光层；只用于复现运行时贴图，不控制时序。 |

生产备注：正常待机状态的构图与信息层级继续保留，30 秒组合动态预览只保留历史记录。吊灯断电—复亮专用样片已获用户接受；用户同时明确要求完整新主界面直接替换旧首屏，因此本次按原型运行时切片处理。右窗雨水拆为两个独立门禁：效果一 V3 窗外持续降雨已获用户接受并冻结为背景基准；效果二 V3 的撞击、附着、停滞和汇流语言可继续，但用户明确否定固定四滴，正式实现改为 Unity 随机多点撞击、局部聚合和最多两条主流。雨效参数通过 Unity Inspector 中的 `ScriptableObject` 直接查看和持久保存，不扩展 CSV。源图与样片继续保留在 `Assets/ArtSource/`；正式运行时新增下表干净窗体补片，雨滴与水流本身由程序绘制。

#### C 开始界面运行时分层

| 资源 | 项目文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 街机主界面常亮底图 | `Assets/Resources/Art/MainMenu/arcade_main_menu_lamp_on.png` | `Art/MainMenu/arcade_main_menu_lamp_on` | 已批准入库、已接入 | `1920x1080`；作为新 `MainMenu` 常态全屏底图。完整图保留首次启动基准文案，运行时另叠加有存档状态、焦点与覆盖确认。 |
| 街机主界面灯灭底图 | `Assets/Resources/Art/MainMenu/arcade_main_menu_lamp_off.png` | `Art/MainMenu/arcade_main_menu_lamp_off` | 已批准入库、已接入 | 只替换左上吊灯与左侧暖光贡献，中央机台、顶箱和右侧雨窗沿用常亮图；以透明度控制熄灭和环境回暖。 |
| 吊灯近场暖光层 | `Assets/Resources/Art/MainMenu/arcade_main_menu_lamp_glow.png` | `Art/MainMenu/arcade_main_menu_lamp_glow` | 已批准入库、已接入 | 带透明径向遮罩；用于灯芯之后约 `0.14s` 的近场光晕恢复。 |
| 吊灯灯芯层 | `Assets/Resources/Art/MainMenu/arcade_main_menu_lamp_core.png` | `Art/MainMenu/arcade_main_menu_lamp_core` | 已批准入库、已接入 | 带透明小范围遮罩；用于约 `0.06s` 的灯芯优先点亮。 |
| 右窗去白点干净补片 | `Assets/Resources/Art/MainMenu/arcade_main_menu_window_clean_patch.png` | `Art/MainMenu/arcade_main_menu_window_clean_patch` | 已批准入库、已接入 | `405x900` 不透明 PNG；源自用户接受的 V2 清理补片，覆盖旧四层底图中的静态雨痕和窗框白点，作为程序化窗外雨幕与玻璃汇流的唯一干净承载面。SHA-256 `F3366F2EF0337C55FD3FC319794327E9474CBFEF68DD215896DF91111EB8CCD9`。 |
| DICE KING 暗铜覆盖层 | `Assets/Resources/Art/MainMenu/arcade_main_menu_marquee_off_patch.png` | `Art/MainMenu/arcade_main_menu_marquee_off_patch` | 已批准入库、待代码接入 | `680x120` RGBA；运行时按 `DICE / KING` 两段独立取样，以透明度把烘焙常亮灯珠压到暗铜余亮，不关闭随机单颗灯珠。SHA-256 `BD0F525A4D3E0AD1B3C3C1235AF2699DA5595C759541F4BA9FF5EA55C9BCE527`。 |
| DICE KING 琥珀辉光层 | `Assets/Resources/Art/MainMenu/arcade_main_menu_marquee_glow_patch.png` | `Art/MainMenu/arcade_main_menu_marquee_glow_patch` | 已批准入库、待代码接入 | `680x120` RGBA；只在失败回亮、分区补亮峰值与稳定收束阶段叠加，常态透明。SHA-256 `98431225BDF345A36C398A10124ADB4B9BFA789681FB9B93CE34DEBED59DBFC4`。 |

运行时配置：吊灯继续使用 `Assets/Resources/Data/main_menu_visual_config.csv`，加载 key 为 `Data/main_menu_visual_config`；右窗雨水独立使用 `Assets/Resources/Config/main_menu_rain_profile.asset`，加载 key 为 `Config/main_menu_rain_profile`；DICE KING 顶箱独立使用 `Assets/Resources/Config/main_menu_marquee_profile.asset`，加载 key 为 `Config/main_menu_marquee_profile`。雨水与顶箱参数都在 Unity Inspector 直接编辑，不扩展 CSV。当前完整机台层属于用户批准的开始界面原型例外；若后续全面改文案、本地化或制作更多状态，仍需补无文字 clean plate，不应继续在完整含字图上叠加。

## 运行时美术实现约定

当前主流程美术应优先使用项目内生成资源作为可见美术来源：

1. 代码使用的背景和道具必须放在 `Assets/Resources/Art/` 下，并通过 Unity `Resources.Load` 加载。
2. 源图、抠图中间件、接触图和旧探索图必须放在 `Assets/ArtSource/` 下，避免作为运行时 `Resources` 打包。
3. 骰子运行时表现使用三层结构：
   - 基础骰面：来自 `Assets/Resources/Art/DiceFaces/` 的生成空白骰面。
   - 动态结果：点数由代码直接绘制到骰面上，包括 `7+` 成长点数的自适应点阵。
   - 类型身份：来自 `Assets/Resources/Art/DiceTypes/` 的生成骰子类型图标，用作小角标。
   - 状态标签：由界面文字显示目标点、阈值、成长或临时标记。
4. 主投掷结果不使用数值徽章；可见结果必须是实际点数骰面。
5. 新增骰子类型图标必须把机制符号融入骰子本体。优先使用骰面标记、骰边切角、骰角封章、绑带、嵌入标记、刻槽、点阵变形或表面镶嵌；避免“骰子主体 + 外部场景道具”的构图。
6. 程序化骰面只作为生成运行时骰面资源缺失时的兜底，不应成为主流程默认骰子身份。
7. 新增主流程美术资源必须通过 `$wabish-art-assets` 生成，或明确匹配同一风格指南，并在代码引用前登记到本文件。
8. 主流程界面控件使用不拉伸的 `OnGUI` 绘制路径：
   - 可缩放羊皮纸面板和按钮由代码程序化绘制，避免装饰和纸纹被强行拉伸。
   - `Assets/Resources/Art/UI/` 下的生成纹理保留为风格参考、替换候选和小图标资源。
   - 小型生成图标以固定尺寸用于顶部信息和市场操作提示。
9. 新增屏幕级 UI、背景、结算演出和市场演出资产默认匹配 `Pixel Ledger Scoring Machine` / `像素账本计分机风`；旧 `Bright Ledger Boardgame` 平面资源只作为兼容、占位或小资产基底。
10. 屏幕级动效应围绕流程状态改变设计：`Ready` 平静可读，`ResultDecision` 锁定结果但不提前入账，`Scoring` 按左到右槽位把分数送入积分塔，达标时触发指针摇晃、灯泡爆亮和爆分反馈，`StageClear` / 通关收益阶段才吐金币，`InterStageMarket` 与结算屏分离。
11. 生成图中的可读文字、数字和长说明不作为最终 UI 文案来源；运行时文字、分数、价格、按钮文案和规则说明仍由 Unity 绘制和数据表控制。

## 已生成骰子类型图标

最终运行目录：

```text
Assets/Resources/Art/DiceTypes/
```

Unity 加载 key 规则：

```text
Art/DiceTypes/<file-name-without-extension>
```

| 运行时用途 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 基础骰 / 中立与无专属图标回退 | `Assets/Resources/Art/DiceTypes/basic_die_icon.png` | `Art/DiceTypes/basic_die_icon` | 保留 | 当前基础骰图标；部分尚无专属类型芯的当前骰使用程序化或基础回退。 |
| 猪猪家族通用类型芯 | `Assets/Resources/Art/DiceTypes/piggy_die_icon.png` | `Art/DiceTypes/piggy_die_icon` | 保留 | 当前猪猪核心与含猪双阵营骰的通用回退。 |
| 龟龟家族通用类型芯 | `Assets/Resources/Art/DiceTypes/turtle_die_icon.png` | `Art/DiceTypes/turtle_die_icon` | 保留 | 当前龟龟核心与含龟双阵营骰的通用回退。 |

当前正式骰子名单为四家族核心 27、中立 8、双阵营 9；专属类型芯后续按这 44 枚名单生产。历史骰子运行时图标及 `.meta` 已从本目录删除，不再登记加载 key。尚未有专属图标的当前骰使用家族壳、短类型名或程序化回退，不得恢复历史图标占位。
源图副本目录：

```text
Assets/ArtSource/DiceTypes/_source_chromakey/
```

接触图：

```text
Assets/ArtSource/DiceTypes/_dice_type_contact_sheet.png
Assets/ArtSource/DiceTypes/_gold_dice_contact_sheet_20260604.png
Assets/ArtSource/DiceTypes/_f001b_turtle_function_symbol_contact_sheet_20260605.png
Assets/ArtSource/DiceTypes/_f001c_odd_even_embedded_symbol_contact_sheet_20260610.png
Assets/ArtSource/DiceTypes/_f001c_odd_even_final_icons_preview_20260610.png
Assets/ArtSource/DiceTypes/_f001d_tree_gardener_irrigation_direction_a_contact_sheet_20260610.png
Assets/ArtSource/DiceTypes/_f001d_tree_final_icons_preview_20260610.png
Assets/ArtSource/Production/F008/f008_gold_income_dice_contact_sheet_20260615.png
Assets/ArtSource/Production/F008/f008_gold_income_dice_final_icons_preview_20260615.png
Assets/ArtSource/Production/F010/f010_odd_even_short_rule_contact_sheet_20260615.png
Assets/ArtSource/Production/F010/f010_odd_even_short_rule_contact_sheet_v2_20260615.png
Assets/ArtSource/Production/F010/f010_odd_even_short_rule_final_icons_preview_20260615.png
Assets/ArtSource/Production/F010/final_icon_sources/
Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_20260616.png
Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_v2_20260616.png
Assets/ArtSource/Production/F011/f011_pattern_tree_replacement_contact_sheet_20260616.png
Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_v3_20260616.png
Assets/ArtSource/Production/F011/f011_tree_growth_interface_final_icons_preview_20260616.png
Assets/ArtSource/Production/F011/final_icon_sources/
```

## 已生成运行时骰面资源

| 资源 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 空白运行时骰面 | `Assets/Resources/Art/DiceFaces/runtime_die_face_base.png` | `Art/DiceFaces/runtime_die_face_base` | 已生成 | 中性生成骰面底图，用于所有运行时投掷结果的代码绘制点数。 |
| F009 统一待机骰 | `Assets/Resources/Art/DiceRoll/f009_unified_ready_die_256.png` | `Art/DiceRoll/f009_unified_ready_die_256` | 已接入待运行验证 | `Ready` 和结算后重置阶段的默认骰体；已按截图反馈重拆为完整裁切版，非基础骰由代码叠加小类型标记。 |
| F009 统一旋转循环 strip | `Assets/Resources/Art/DiceRoll/f009_unified_spin_loop_strip_24f_256.png` | `Art/DiceRoll/f009_unified_spin_loop_strip_24f_256` | 已接入待运行验证 | 24 帧 256 像素横条，用于 `Shaking` 阶段固定槽位内旋转；已按截图反馈过滤边缘碎片源帧。 |
| F009 统一停转预览 strip | `Assets/Resources/Art/DiceRoll/f009_unified_spin_stop_strip_8f_256.png` | `Art/DiceRoll/f009_unified_spin_stop_strip_8f_256` | 已接入待运行验证 | 8 帧 256 像素横条，用于 `Stopping` 阶段减速预览；已按截图反馈改为完整源帧重组，停住后切到真实结果骰面。 |
| F009 统一结果骰面 strip | `Assets/Resources/Art/DiceFaces/f009_unified_result_die_faces_6x256.png` | `Art/DiceFaces/f009_unified_result_die_faces_6x256` | 已接入待运行验证 | 1 到 6 点结果骰面横条，用于 `ResultDecision` 和 `Scoring` 阶段；已按截图反馈扩大安全边距，`7+` 仍回退到程序点阵。 |
| F009 桌面摩擦旋转循环 strip 旧版 | `Assets/Resources/Art/DiceRoll/f009_table_friction_spin_loop_strip_24f_256.png` | `Art/DiceRoll/f009_table_friction_spin_loop_strip_24f_256` | 保留为回退 | 旧 24 帧桌面摩擦旋转 strip；统一旋转资源缺失时由代码回退读取。 |
| F009 桌面摩擦停转预览 strip 旧版 | `Assets/Resources/Art/DiceRoll/f009_table_friction_spin_stop_strip_8f_256.png` | `Art/DiceRoll/f009_table_friction_spin_stop_strip_8f_256` | 保留为回退 | 旧 8 帧停转预览 strip；统一停转资源缺失时由代码回退读取。 |

源图副本：

```text
Assets/ArtSource/DiceFaces/_source_chromakey/runtime_die_face_base_source.png
Assets/ArtSource/Production/F009/f009_table_friction_spin_loop_strip_24f_256_20260616.png
Assets/ArtSource/Production/F009/f009_table_friction_spin_stop_strip_8f_256_20260616.png
Assets/ArtSource/Production/F009/f009_table_friction_spin_combined_preview_32f_20260616.gif
Assets/ArtSource/Production/F009/f009_table_friction_spin_loop_contact_24f_20260616.png
Assets/ArtSource/Production/F009/f009_table_friction_spin_stop_contact_8f_20260616.png
Assets/ArtSource/Production/F009/f009_unified_die_visual_language_contact_sheet_v1_20260616.png
Assets/ArtSource/Production/F009/f009_unified_stage_ready_contact_sheet_v1_20260616.png
Assets/ArtSource/Production/F009/f009_unified_stage_roll_stop_contact_sheet_v1_20260616.png
Assets/ArtSource/Production/F009/f009_unified_stage_result_scoring_contact_sheet_v1_20260616.png
Assets/ArtSource/Production/F009/f009_unified_runtime_assets_preview_20260616.png
Assets/ArtSource/Production/F009/f009_unified_runtime_assets_checker_preview_20260616.png
```

## 已生成界面资源

运行目录：

```text
Assets/Resources/Art/UI/
```

Unity 加载 key 规则：

```text
Art/UI/<file-name-without-extension>
```

| 资源 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 账本面板 | `Assets/Resources/Art/UI/ui_panel_ledger.png` | `Art/UI/ui_panel_ledger` | 已生成 | 主羊皮纸区块面板。 |
| 夹纸卡片 | `Assets/Resources/Art/UI/ui_card_clip.png` | `Art/UI/ui_card_clip` | 已生成 | 市场货架卡和大物件卡底。 |
| 小面板 | `Assets/Resources/Art/UI/ui_small_panel.png` | `Art/UI/ui_small_panel` | 已生成 | 骰子行、命令条和小详情块。 |
| 主按钮 | `Assets/Resources/Art/UI/ui_button_primary.png` | `Art/UI/ui_button_primary` | 已生成 | 风格参考和替换候选；当前 `OnGUI` 按钮边框由代码程序化绘制以避免拉伸。 |
| 次按钮 | `Assets/Resources/Art/UI/ui_button_secondary.png` | `Art/UI/ui_button_secondary` | 已生成 | 风格参考和替换候选；当前 `OnGUI` 按钮边框由代码程序化绘制以避免拉伸。 |
| 危险按钮 | `Assets/Resources/Art/UI/ui_button_danger.png` | `Art/UI/ui_button_danger` | 已生成 | 风格参考和替换候选；当前 `OnGUI` 按钮边框由代码程序化绘制以避免拉伸。 |
| 禁用按钮 | `Assets/Resources/Art/UI/ui_button_disabled.png` | `Art/UI/ui_button_disabled` | 已生成 | 风格参考和替换候选；当前 `OnGUI` 按钮边框由代码程序化绘制以避免拉伸。 |
| 金币图标 | `Assets/Resources/Art/UI/ui_icon_coin.png` | `Art/UI/ui_icon_coin` | 已生成 | 金币和经济提示图标。 |
| 刷新图标 | `Assets/Resources/Art/UI/ui_icon_refresh.png` | `Art/UI/ui_icon_refresh` | 已生成 | 市场刷新提示图标。 |
| 设置图标 | `Assets/Resources/Art/UI/ui_icon_settings.png` | `Art/UI/ui_icon_settings` | 已生成 | 设置入口图标。 |
| 关闭图标 | `Assets/Resources/Art/UI/ui_icon_close.png` | `Art/UI/ui_icon_close` | 已生成 | 关闭和返回提示图标。 |
| 目标图标 | `Assets/Resources/Art/UI/ui_icon_target.png` | `Art/UI/ui_icon_target` | 已生成 | 目标分和进度提示图标。 |
| 出售图标 | `Assets/Resources/Art/UI/ui_icon_sell.png` | `Art/UI/ui_icon_sell` | 已生成 | 出售操作提示图标。 |
| 骰袋图标 | `Assets/Resources/Art/UI/ui_icon_bag.png` | `Art/UI/ui_icon_bag` | 已生成 | 骰袋容量提示图标。 |

### 骰子悬浮窗 UI 素材

2026-07-17 用户已接受横向“旧街机检修终端”三状态样张，并已据此重绘无字运行时底件。新资源尚待程序接入和双分辨率验收；下方旧版资源继续作为回退。

旧版运行目录：

```text
Assets/Resources/Art/UI/Tooltip/
```

Unity 加载 key 规则：

```text
Art/UI/Tooltip/<file-name-without-extension>
```

| 资源 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 悬浮窗主面板 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_panel_clean.png` | `Art/UI/Tooltip/ui_tooltip_panel_clean` | 旧版已接入、待替换 | F014 旧 `336×400` 竖版单层浮窗空面板；新方向验收完成前保留回退。 |
| 悬浮窗侧栏细轨 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_side_rail.png` | `Art/UI/Tooltip/ui_tooltip_side_rail` | 已生成 | 侧边页签的细轨道，不表现书脊或纸页。 |
| 悬浮窗当前页签 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_tab_active.png` | `Art/UI/Tooltip/ui_tooltip_tab_active` | 已生成 | 当前页签底图，文字由程序绘制。 |
| 悬浮窗骰效页签 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_tab_idle_blue.png` | `Art/UI/Tooltip/ui_tooltip_tab_idle_blue` | 已生成 | 骰效页签底图，文字由程序绘制。 |
| 悬浮窗质效页签 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_tab_idle_green.png` | `Art/UI/Tooltip/ui_tooltip_tab_idle_green` | 已生成 | 品质效果页签底图，文字由程序绘制。 |
| 悬浮窗卖价胶囊 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_price_chip.png` | `Art/UI/Tooltip/ui_tooltip_price_chip` | 已生成 | 卖价字段底图和金币符号，具体价格由程序绘制。 |
| 悬浮窗骰效标签 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_label_chip_blue.png` | `Art/UI/Tooltip/ui_tooltip_label_chip_blue` | 已生成 | 骰子效果字段标签底图，文字由程序绘制。 |
| 悬浮窗质效标签 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_label_chip_green.png` | `Art/UI/Tooltip/ui_tooltip_label_chip_green` | 已生成 | 品质效果字段标签底图，文字由程序绘制。 |
| 悬浮窗点面格 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_face_cell.png` | `Art/UI/Tooltip/ui_tooltip_face_cell` | 已生成 | 单个点面格底图，点数 pip 由程序绘制。 |

源图和接触图：

```text
Assets/ArtSource/Production/Tooltip/tooltip_vertical_side_tabs_clean_v10_20260616.png
Assets/ArtSource/Production/Tooltip/tooltip_runtime_ui_assets_preview_20260616.png
Assets/ArtSource/Production/Tooltip/runtime_ui_sources/
```

| 新方向参考 | 源文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 老街机检修终端三状态样张 | `Assets/ArtSource/Production/ArcadeDiceKing/DiceTooltipV1/arcade_dice_king_tooltip_three_state_contact_v1_20260717.png` | 不适用 | 用户已确认、不得直接切图 | `1672×941` 不透明接触图；覆盖基础、复杂动态和市场三个状态。带字内容只用于方向与容量评审，正式运行时必须重绘无字底件。SHA-256 `5A9DE659C3A1D7D6ED191FD3E78E8CC1F9FA18287BE08DADBCC1569CCC97A370`。 |

#### 老街机检修终端运行时资源 V1

| 资源 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 简版无字面板 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_arcade_panel_short.png` | `Art/UI/Tooltip/ui_tooltip_arcade_panel_short` | 已生成、待程序接入 | `896×392` RGBA，对应 `448×196` 虚拟尺寸。 |
| 中版无字面板 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_arcade_panel_medium.png` | `Art/UI/Tooltip/ui_tooltip_arcade_panel_medium` | 已生成、待程序接入 | `896×488` RGBA，对应 `448×244` 虚拟尺寸。 |
| 复杂版无字面板 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_arcade_panel_tall.png` | `Art/UI/Tooltip/ui_tooltip_arcade_panel_tall` | 已生成、待程序接入 | `896×576` RGBA，对应 `448×288` 虚拟尺寸。 |
| 数字面格 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_arcade_face_cell.png` | `Art/UI/Tooltip/ui_tooltip_arcade_face_cell` | 已生成、待程序接入 | `120×88` RGBA；运行时由程序绘制负数和一至四位数。 |
| 类型芯框 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_arcade_type_core_frame.png` | `Art/UI/Tooltip/ui_tooltip_arcade_type_core_frame` | 已生成、待程序接入 | `128×128` RGBA；中心透明，复用现有骰子类型图标。 |

生产源、可复现脚本和检查记录：

```text
Assets/ArtSource/Production/ArcadeDiceKing/DiceTooltipV1/RuntimeSourcesV1/arcade_tooltip_runtime_parts_chromakey_source_v1_20260717.png
Assets/ArtSource/Production/ArcadeDiceKing/DiceTooltipV1/RuntimeSourcesV1/build_arcade_tooltip_runtime_assets_v1.py
Assets/ArtSource/Production/ArcadeDiceKing/DiceTooltipV1/RuntimeSourcesV1/arcade_tooltip_runtime_assets_preview_v1_20260717.png
Assets/ArtSource/Production/ArcadeDiceKing/DiceTooltipV1/RuntimeSourcesV1/arcade_tooltip_runtime_assets_metrics_v1_20260717.json
```

V1 通过自动尺寸、透明像素和绿色残边检查；五张运行图均无业务文字、数字、家族名或价格。接触图不作为运行时资源，旧浅色资源在新方向完成 Unity 验收前保留。

源图副本：

```text
Assets/ArtSource/UI/
```

## 已生成改造道具图标

运行目录：

```text
Assets/Resources/Art/Items/
```

Unity 加载 key 规则：

```text
Art/Items/<file-name-without-extension>
```

| 资源 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 加印石图标 | `Assets/Resources/Art/Items/affix_add_stone.png` | `Art/Items/affix_add_stone` | 已生成 | 2026-06-11 由 F005 方向 A 接触图裁切抠透明；官署石印、绿色加号和新封印表达随机添加 1 条合法词缀；透明通道已校验。 |
| 剥印石图标 | `Assets/Resources/Art/Items/affix_remove_stone.png` | `Art/Items/affix_remove_stone` | 已生成 | 2026-06-11 由 F005 方向 A 接触图裁切抠透明；裂开石印、碎蜡封和刮片表达随机删除 1 条已有词缀；透明通道已校验。 |
| 换印石图标 | `Assets/Resources/Art/Items/affix_replace_stone.png` | `Art/Items/affix_replace_stone` | 已生成 | 2026-06-11 由 F005 方向 A 接触图裁切抠透明；双印章、环形箭头和新旧封印表达先删除再随机添加 1 条合法新词缀；透明通道已校验。 |

源图和接触图：

```text
Assets/ArtSource/Items/_f005_affix_stone_direction_a_contact_sheet_20260611.png
Assets/ArtSource/Items/_f005_affix_stone_final_icons_preview_20260611.png
Assets/ArtSource/Items/_source_chromakey/
```

## 其它已生成资源

| 资源 | 当前文件 | Unity 加载 key | 备注 |
|---|---|---|---|
| 桌面背景 | `Assets/Resources/Art/table_background.png` | `Art/table_background` | 2026-06-02 以 `Bright Ledger Boardgame` 风格重生成；用于主流程全屏桌面背景。 |
| 骰盅 | `Assets/Resources/Art/dice_cup.png` | `Art/dice_cup` | 2026-06-02 以 `Bright Ledger Boardgame` 风格重生成；透明道具图，用于摇骰阶段。 |

源图和旧资源位置：

```text
Assets/ArtSource/Backgrounds/table_background_generated_20260602.png
Assets/ArtSource/Props/_source_chromakey/dice_cup_source_20260602.png
Assets/ArtSource/Legacy/table_background_legacy_dark_realistic.png
Assets/ArtSource/Legacy/dice_cup_legacy_dark_realistic.png
```

## F021 主游戏实体面板 V1 三版接触图

以下三张为用户确认“六槽结算控制台”功能分区后生成的 `Ready` 状态评审源图，不是运行时资源。

| 方向 | 源图 | 状态 | 备注 |
|---|---|---|---|
| A 信息清晰仪表盘 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGamePanelV1/arcade_dice_king_main_game_panel_v1_a_clear_instrument_20260714.png` | 待用户选型 | 强调目标 / 当前、金币、单次投掷、单条规则与六槽的清晰层级。 |
| B 机械结算机 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGamePanelV1/arcade_dice_king_main_game_panel_v1_b_mechanical_settlement_20260714.png` | 待用户选型 | 强化六节点轨道、实体骰槽和规则插件的机械反馈。 |
| C 雨夜信号台 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGamePanelV1/arcade_dice_king_main_game_panel_v1_c_rain_signal_20260714.png` | 待用户选型 | 强化与已确认开始界面的冷蓝雨夜、暖琥珀机台过渡。 |

共同约束：同一机柜、同一正视近景、六颗实体骰、六节点左到右结算轨道、目标 / 当前双计数器、金币、单次投掷、单条规则插件和唯一实体主键；不使用皇冠、赔付表、`骰子 ×3`、赌场滚轴或额外操作键。三张均只位于 `Assets/ArtSource/`，用户确认方向前不得进入 `Assets/Resources/Art/`。

## F021 穿屏转场与独立骰王竞技台 V1 评审图

用户已确认主游戏不再受开始界面机台、CRT 比例和雨夜空间约束，改为通过全屏信号遮罩进入独立 `16:9` 主游戏场景。以下两张为方向评审源图，不是运行时资源；用户验收前不得拆入 `Assets/Resources/Art/`，也没有 Unity 加载 key。

| 评审项 | 源图 | 状态 | 备注 |
|---|---|---|---|
| 四帧穿屏启动分镜 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameArenaV1/arcade_dice_king_transition_storyboard_v1_20260715.png` | 用户确认可先使用、保留为当前基准 | `1672x941`；四格依次为雨夜机厅、六节点启动、全屏信号遮罩和独立竞技台。只确认转场关系，不代表末帧主游戏布局通过。SHA-256 `1A8DADCDC33142E493ED3F0BA54A2FBB74A3C8FEF4367457CC96DC23D451C181`。 |
| 主游戏三状态接触图 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameArenaV1/arcade_dice_king_main_game_arena_three_state_contact_v1_20260715.png` | 需修订、不得切图 | `1672x941`；用户指出缺少明确生命 / 金币分区，`1/1` 没有常驻价值，悬空结算轨道与后续状态灯均不需要，裸 `+4` 无法区分分数与临时骰数量。V2 先只重做 `Ready` 布局，结算位置后续仅由骰子本体抬升 / 回落表达。SHA-256 `CBCCBEDC6C9C916B0C9A726DB216F942255A1B687C2D3481B4DE9384E056F175`。 |
| 主游戏 Ready V2 与拖拽三步示意 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameArenaV2/arcade_dice_king_main_game_ready_layout_drag_contact_v2_20260715.png` | 需修订、不得切图 | 功能分区继续保留，但六个独立升降框、密集螺栓和厚重底座过于蒸汽工业，与已确认开始界面的旧街机黑玻璃 / 圆角喷漆金属语言不一致；拖动中的黑色硬空槽和竖直插入灯也显得僵硬。V3 改为连续低矮街机托盘、浅凹软槽和弹性让位预览。 |
| 主游戏 Ready V3 街机托盘与拖拽示意 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameArenaV3/arcade_dice_king_main_game_ready_arcade_tray_drag_contact_v3_20260715.png` | 功能与交互通过、风格需修订、不得切图 | 用户确认功能分区、连续托盘和“悬停—拖动—预落位”交互没有问题；但整体仍偏光滑现代控制台，缺少开始界面的老街机电子气味。后续保持布局与交互不变，只把外壳、面板和电子器件换成冲压喷漆钢板、老化塑料、烟熏玻璃、琥珀点阵、通风栅和克制掉漆语言。 |
| 主游戏 Ready V4 老街机电子换肤稿 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameArenaV4/arcade_dice_king_main_game_ready_old_arcade_electronics_contact_v4_20260715.png` | 界面外壳方向可继续、骰子层待重构、不得切图 | 用户评价整体“还行”，功能与交互继续冻结；但接触图中的整颗固定骰不能承接后续面值变化和大量个性化改造。后续保留 V4 老街机电子外壳，将骰子改为统一骰体、空白数字面、运行时数字、家族层、改造挂件和状态特效的模块化组合。 |

共同约束：开始界面只作为入口；第三帧全屏信号必须能够隐藏真正的场景替换；主游戏不保留窗雨、吊灯、完整机壳或持续顶箱。色彩、点阵和继电器语言继续提供连续性，六节点信号只允许短暂存在于转场，不进入主游戏常态 HUD。上述图片只用于评审，不定义最终文字、数字、动画参数或可切运行资源。
| 数字化模块骰系统 V1 样张 | Assets/ArtSource/Production/ArcadeDiceKing/NumericDiceSystemV1/arcade_dice_king_numeric_modular_dice_system_contact_v1_20260715.png | 需修订、不得切图 | 用户收紧信息职责：骰子本体只表达当前点数与种类，永久成长、挂件数量、双家族细节和六面分布不应直接堆在本体上。V2 需改为一至四位数字极限、不同种类识别及主界面实际尺寸测试；结果阶段不得露出其它骰面点数。 |
| 数字与种类实景 V2 样张 | Assets/ArtSource/Production/ArcadeDiceKing/NumericDiceSystemV2/arcade_dice_king_numeric_type_in_context_contact_v2_20260715.png | 数字方向可继续、识别体系待补齐、不得切图 | 用户评价整体“还可以”；一至四位数字和单面结果纪律可以继续，但外壳表达家族后，具体骰子类型仍缺少统一识别载体，关间市场的未投商品态也尚未定义。下一轮先确认“家族壳 + 类型芯 + 数字面 + 悬浮详情”的完整跨状态语法。 |
| 主游戏家族壳 / 类型芯三状态 V1 评审图 | Assets/ArtSource/Production/ArcadeDiceKing/DiceIdentityMarketSystemV1/arcade_dice_king_family_type_core_main_game_states_contact_v1_20260715.png | 显示方式与功能分区认可、美术风格需修订、不得切图 | `1672x941`；用户认可同一组六骰的跨状态信息分层、主游戏功能分区和呈现方式：`Ready` 放大类型芯、`Result` 数字接管中心、`Hover` 展开六面与效果详情。后续不重做结构，只调整整体美术风格与细节；运行时拆层仍未批准。SHA-256 `C055C4327E20A168595488C436E1BA211779AD871D661834FF87FC956EFD74F9`。 |
| 关间市场完整识别与交互 V1 评审图 | Assets/ArtSource/Production/ArcadeDiceKing/DiceIdentityMarketSystemV1/arcade_dice_king_interstage_market_identity_system_contact_v1_20260715.png | 商品态与功能分区认可、美术风格需修订、不得切图 | `1672x941`；用户认可左侧骰袋、右侧三货架、顶部资源结构，以及市场商品沿用待机态家族壳 + 放大类型芯、不伪造点数、悬浮详情承接完整信息的方式。后续只做美术风格适配和细节修订。SHA-256 `298335195A462EBD356C2CD4490B2A91572DEA6ADE4679350319D74883D524A0`。 |

## 关间市场三状态换肤 V2 样张（2026-07-17）

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 关间市场三状态接触图 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/InterStageMarketV2/arcade_dice_king_interstage_market_three_state_contact_v1_20260717.png` | 不适用 | 用户已通过、运行时 V1 已派生 | `1672x941`、`Format24bppRgb` 不透明源图；用户于 2026-07-17 通过该方向。上方为常态市场与悬浮详情，下方分别展示购买后留空货架、金币与容量变化，以及离场锁定后按骰袋顺序扫描、吸附和货架离场效果。布局冻结为左骰袋、右三货架、顶部资源和底部操作结构，材质采用当前主游戏的午夜旧机厅、老化深蓝塑料、烟熏玻璃和琥珀点阵语言，并继续减少独立军工箱体和硬轨道感。图片是批量生产母版，不直接作为运行时底图；无字底板与动态文字 / 骰子 / 状态层已经按下节资源包 V1 派生和接入。SHA-256 `1000CD6E6C6FC1FF3CB98C86D4863D4F3B656A0D9B29AD7E730BEC2489AAA386`。 |

## 关间市场运行时资源包 V1（2026-07-17）

| 资源 | 源文件 / 运行时文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 市场无字干净底板源图 | `Assets/ArtSource/Production/ArcadeDiceKing/InterStageMarketRuntimeV1/arcade_market_common_clean_plate_source_v1_20260717.png` | 不适用 | 用户通过方向后生产、源图保留 | `1672×941`、`Format24bppRgb`、2,391,782 字节；四个顶部空窗、左侧六个空槽和卖出托盘、右侧三格空商品柜、底部刷新 / 反馈 / 离开三段操作区。无文字、数字、骰子、图标、价格、Tooltip、高亮或动效。SHA-256 `18D4498706BF7B3E130555E063C91891D45CFEC7022E294CC29D07E4B6D02340`。 |
| 市场共用运行时底板 | `Assets/Resources/Art/Market/arcade_market_common_base.png` | `Art/Market/arcade_market_common_base` | 已接入、双分辨率 Play Mode 通过 | `1920×1080` 不透明 PNG、4,208,612 字节；由 `build_market_runtime_assets.ps1` 可复现生成。`DiceKingDemo.cs` 在关间市场和章节市场优先加载，资源缺失或词缀功能重新开启时回退旧市场。SHA-256 `4C4C3A8A2C027B6EB627051DFF78E368F9E57CDC4D1829DCDA4623E29A7BECCC`。 |
| 商品与骰袋家族壳 | 复用 `Assets/Resources/Art/MainGame/arcade_main_game_die_shell_*.png` | 复用既有 `Art/MainGame/arcade_main_game_die_shell_*` | 已接入 | 未锁结果只绘制家族壳 + 放大类型芯，不伪造点数；双家族继续左右拼壳，缺失家族回退中立壳。 |

生成提示、参考图职责、构建命令与哈希见 `Assets/ArtSource/Production/ArcadeDiceKing/InterStageMarketRuntimeV1/README.md`。Unity 于 2026-07-17 使用 `Assets/Editor/WabishMarketRuntimeCapture.cs` 对常态、购买后留空和离场锁定三状态生成 `1920×1080 / 1280×720` 六张截图并通过尺寸校验；详见 `Docs/QA/20260717_market_runtime_validation.md`。现有 Tooltip 与拖拽逻辑已复用，但真实鼠标停留和完整拖拽手势仍保留人工门禁。F021 V4 全局文字标准随后已接入本市场的动态文字入口；这只改变文字渲染，不改变本资源包的布局与流程。

## F021 通关与失败收束 V1 评审图

以 F021 已通过的主游戏功能分区、连续低矮托盘、旧娱乐街机外壳和“家族壳 + 类型芯 + 数字面”为共同母版，生成三张同规格结果收束关键帧。用户于 2026-07-15 确认三图没有问题，已冻结为通关、可重试失败和生命归零的静态视觉基准。三图都是 `1672x941` 不透明评审源图，不是运行时贴图；下一门禁为三条分支的动态分镜，动效验收前仍不拆入 `Assets/Resources/Art/`，也没有 Unity 加载 key。

| 状态 | 源图 | 状态 | 备注 |
|---|---|---|---|
| 通关收束 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameOutcomeV1/arcade_dice_king_stage_clear_review_v1_20260715.png` | 静态基准已通过、动态待评审、暂不切图 | 保留最终六骰；左侧目标 / 当前为 `15 / 18`，右侧结果终端显示通关及固定、利息、复利三项收入，底部为进入市场。暖色升亮只限积分塔、结果终端和金币区。SHA-256 `10ACBA2073CA0E423A63923F600FC55CC99DC4688E97EB90A8452DC1AC48124D`。 |
| 可重试失败 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameOutcomeV1/arcade_dice_king_stage_failed_retry_review_v1_20260715.png` | 静态基准已通过、动态待评审、暂不切图 | 目标 / 当前为 `15 / 12`，生命扣至 `2`，右侧显示未达目标和差 `3`分，底部为重试本关。锈红只用于目标标记、生命区和结果终端，没有全屏红罩。SHA-256 `8FE36210E11EBB667355435070869F5505D9872223289E56E1E97E62719816B8`。 |
| 生命归零 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameOutcomeV1/arcade_dice_king_run_over_review_v1_20260715.png` | 静态基准已通过、动态待评审、暂不切图 | 生命为 `0`，右侧显示本轮结束和差 `3`分，底部为返回开始界面。该图是玩家确认前的等待帧，最终骰子和分差仍可读，尚未执行断电与 CRT 收缩。SHA-256 `A99DD354E828EB1D9BEB0127FB59FACBE86D68C0009EC76CC8FD6FEACA8C154C`。 |

共同验收边界：不显示“本次预计 +N 分”总预览，不新增临时骰托盘、`1/1`、六节点线、槽位灯或额外操作键；右侧复用原规则终端承接结果，底部 `Space` 保持唯一分支操作。

## F021 主游戏共用运行时资源包 V1

用户于 2026-07-15 明确批准先制作并接入主游戏共用运行时底座，不等待通关 / 失败动态分镜。本包只承载 `Ready / Shaking / Stopping / ResultDecision / Scoring` 共用的中性外壳、空显示区和模块化骰体；通关暖光、失败锈红、生命扣除、重试扫描、断电收缩等分支专属表现继续后置。所有文字、数字、图标、积分填充、类型芯、骰面结果和交互反馈由 Unity 绘制，不烘入底图。

| 资源 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 主游戏共用无文字底板 | `Assets/Resources/Art/MainGame/arcade_main_game_common_base.png` | `Art/MainGame/arcade_main_game_common_base` | 已接入；R3 边框贴合回归中 | `1920x1080`；老娱乐街机电子外壳，包含空顶部三窗、空左侧积分塔、中央连续六槽、空右侧终端和空底部实体键。R3 运行候选为六个 `128×128` 物理槽、中心 `X=299+128n / Y=368`、净距 `0`。没有骰子、文字、图标、数值、灯条填充或通关 / 失败光效。资源缺失时必须回退旧主游戏绘制。 |
| 中性家族骰壳 | `Assets/Resources/Art/MainGame/arcade_main_game_die_shell_neutral.png` | `Art/MainGame/arcade_main_game_die_shell_neutral` | 已接入；R3 贴合回归中 | `512x512` RGBA；暖象牙 / 旧黄铜外壳，中央留空，由 Unity 绘制类型芯或一至四位数字。运行时使用公共有效区 `(32,27,448,458)`。 |
| 猪家族骰壳 | `Assets/Resources/Art/MainGame/arcade_main_game_die_shell_pig.png` | `Art/MainGame/arcade_main_game_die_shell_pig` | 已接入；R3 贴合回归中 | `512x512` RGBA；克制珊瑚红、耳角与鼻章只表达家族，不替代具体类型芯；沿用公共有效区。 |
| 龟家族骰壳 | `Assets/Resources/Art/MainGame/arcade_main_game_die_shell_turtle.png` | `Art/MainGame/arcade_main_game_die_shell_turtle` | 已接入；R3 贴合回归中 | `512x512` RGBA；旧青绿色和壳章只表达家族；沿用公共有效区。 |
| 恶魔家族骰壳 | `Assets/Resources/Art/MainGame/arcade_main_game_die_shell_devil.png` | `Art/MainGame/arcade_main_game_die_shell_devil` | 已接入；R3 贴合回归中 | `512x512` RGBA；深锈红、克制角与蝠章只表达家族；沿用公共有效区。 |
| 海盗家族骰壳 | `Assets/Resources/Art/MainGame/arcade_main_game_die_shell_pirate.png` | `Art/MainGame/arcade_main_game_die_shell_pirate` | 已接入；R3 贴合回归中 | `512x512` RGBA；旧海军蓝、铆钉与锚章只表达家族；沿用公共有效区。 |

源图与可复现构建脚本：

```text
Assets/ArtSource/Production/ArcadeDiceKing/MainGameRuntimeV1/arcade_main_game_common_clean_plate_source_v1_20260715.png
Assets/ArtSource/Production/ArcadeDiceKing/MainGameRuntimeV1/arcade_main_game_family_shells_chromakey_source_v1_20260715.png
Assets/ArtSource/Production/ArcadeDiceKing/MainGameRuntimeV1/build_runtime_assets.ps1
```

首轮生成提示以已通过的 V4 老街机外壳、家族壳 / 类型芯三状态和通关静态基准为参考，要求移除全部文字、骰子、图标、数值和状态光效；家族壳源图使用纯色抠图背景，构建脚本负责生成透明 `512x512` 运行文件。接入门禁为 Unity 导入 / 编译、`Resources.Load` 路径检查、`1920x1080` 与 `1280x720` Play Mode 截图，以及 `Ready / ResultDecision / Scoring` 常态交互检查。

2026-07-16 接入与运行验收完成：`DiceKingDemo.cs` 在共用底板存在时启用新 Run 绘制分支，缺失时回退旧界面；单家族使用对应壳，双家族使用两张现有壳左右拼接，类型图标缺失时使用短类型名。Unity 2019.4.33f1 已完成 0 error 编译与八张双分辨率截图，覆盖 Ready、ResultDecision、Scoring、五类单家族、双家族和 `123 / 1234` 数字容量。记录见 `Docs/QA/20260716_main_game_runtime_validation.md`。本状态表示资源已正确入库和运行，不等于后续通关 / 失败动态分支已批准。

2026-07-16 R2 尺寸复核：用户人工截图确认资源虽然已正确加载，但完整 `512×512` 透明画布被缩入逻辑槽后，骰壳有效像素偏小。PNG 与加载 key 均不变；运行时改用公共有效区 `(32,27,448,458)`，底图有效空槽冻结为 `112×112`、中心距 `128`、净距 `16`。本轮状态改为“资源已入库、R2 UV 运行回归中”，通过前不再沿用 V1 的视觉尺寸通过结论。

2026-07-16 R3 边框复核：用户继续指出 R2 骰壳未贴合底图卡槽边框，因此 R2 尺寸仅保留为历史尝试。PNG、加载 key 与公共 UV 不变；运行候选改为 `128×128` 物理槽、中心 `X=299+128n / Y=368`、净距 `0`，映射后可见壳约宽 `123.1–127.7`、高 `127.4–128`。资源本身无需重生成，当前门禁是 R3 双分辨率图和用户人工贴合度确认。

## F021 主游戏九状态接触图 V1（源图样张，2026-07-16）

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 主游戏九状态接触图 V1 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/arcade_dice_king_main_game_nine_state_contact_v1_20260716.png` | 不适用 | 需修订、保留对照 | 主体老街机风格获认可；文字仍混用平滑字体且六骰没有严格贴合当前 R3 尺寸，因此不进入后续动态门禁。 |
| 主游戏九状态接触图 V2 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/arcade_dice_king_main_game_nine_state_contact_v2_led_fit_20260716.png` | 不适用 | 用户已通过、动态母版 | 保持 V1 主体风格与九状态顺序；全部 UI 文字改为开始界面同系 LED 点阵字，六骰按 R3 `128×128` 连续物理槽和当前壳体有效尺寸重新贴合。 |

该文件只用于视觉方向和状态可读性评审，未放入 `Assets/Resources/Art/`，不代表运行时资源已经切分或接入。验收后再决定是否进入“投骰—停转”“逐骰结算—目标越线”“通关收入”“失败重试”“生命归零断电”五条动态样片。

V2 继续只用于源图评审；若用户确认字体与贴槽，才把 LED 字体规则和 R3 几何带入五条动态样片。正式运行时文字仍应由 Unity 动态绘制，不从接触图硬裁。

## F021 主游戏五条动态样片 V1（2026-07-16）

| 样片 | 源文件 | 状态 | 规格与边界 |
|---|---|---|---|
| 投骰—停转 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MotionPreviewsV1/arcade_dice_king_roll_to_stop_preview_v1_20260716.mp4` | 用户已通过（导演基准） | 5.00 秒；固定六槽内旋转、第三槽揭晓、左到右停转并锁定结果。 |
| 逐骰结算—目标越线 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MotionPreviewsV1/arcade_dice_king_scoring_target_cross_preview_v1_20260716.mp4` | 用户已通过（导演基准） | 5.60 秒；第二槽局部贡献与第四槽越线，越线后保持继续结算语义。 |
| 通关收入 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MotionPreviewsV1/arcade_dice_king_stage_clear_income_preview_v1_20260716.mp4` | 用户已通过（导演基准） | 5.43 秒；通关终端接管，固定 / 利息 / 复利三拍点亮，只停在进入市场操作。 |
| 失败重试 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MotionPreviewsV1/arcade_dice_king_stage_failed_retry_preview_v1_20260716.mp4` | 用户已通过（导演基准） | 4.77 秒；保留最终六骰和分差，确认后单条水平扫描复位到 `Ready`。 |
| 生命归零断电 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MotionPreviewsV1/arcade_dice_king_run_over_powerdown_preview_v1_20260716.mp4` | 用户已通过（导演基准） | 5.47 秒；分模块断电、CRT 水平线收缩、黑场结束。 |

共同规格为 `1920×1080`、30 fps、H.264 High、`yuv420p`、无声；五条均已完成全片解码校验，并于 2026-07-17 获用户确认通过。中段总览图为 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MotionPreviewsV1/arcade_dice_king_five_motion_previews_review_sheet_v1_20260716.png`，可复现脚本为 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/build_five_motion_previews_v1.ps1`。全部文件只位于 `Assets/ArtSource`，没有运行时加载 key；Unity 接入必须按分层和参数化时序重建，不能直接加载这些 MP4。

## F009 自动结算与分级破线动态样片 V1（2026-07-17）

本样片响应 F009 最新确认：取消第二次 `Space` 主动结算；加力窗口配置化且首版为 `0`；基础旋转、左到右停转、短暂静默读点、自动逐槽结算与五档分级反馈组成一条连续导演曲线。样片沿用 F021 已通过的老街机竞技台动态母版，只用于用户评审，不替换任何运行资源。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 自动结算与分级破线样片 V1 | `Assets/ArtSource/Production/F009/AutoSettlementScorePreviewV1/f009_auto_settlement_score_tiers_preview_v1_20260717.mp4` | 不适用 | 用户已退回 | `1280×720`、30 fps、6.2 秒、无声；显式百分比、逐颗大数字和逐档破线造成强顿挫与过强数值感，不得作为 Unity 接入参考。 |
| 对话快速预览 | `Assets/ArtSource/Production/F009/AutoSettlementScorePreviewV1/f009_auto_settlement_score_tiers_preview_v1_20260717.gif` | 不适用 | 用户已退回 | `960×540`、15 fps；只保留为 V1 失败证据。 |
| 九格节拍总览 | `Assets/ArtSource/Production/F009/AutoSettlementScorePreviewV1/f009_auto_settlement_score_tiers_review_v1_20260717.png` | 不适用 | 用户已退回 | 记录了准备、旋转、停转、静默锁定、自动入账和逐档破级；这些离散节点正是本轮退回原因。 |
| 五档终局总览 | `Assets/ArtSource/Production/F009/AutoSettlementScorePreviewV1/f009_score_tier_states_review_v1_20260717.png` | 不适用 | 用户已退回 | 五档作为显式终局卡会让玩家读等级；V2 只允许五档作为隐藏导演参数。 |
| 可复现预览源 | `Assets/ArtSource/Production/F009/AutoSettlementScorePreviewV1/preview.html`、`render_preview.js`、`build_preview.ps1` | 不适用 | 源文件 | 只生成评审 MP4、GIF 和总览图；不得被 Unity 直接加载。 |

2026-07-21 用户明确退回 V1：流程不够连续、顿挫感强，并且过度依赖明确分数与比例让玩家判断强弱。全部文件继续保留在 `Assets/ArtSource/` 作为历史失败证据，不删除、不进入运行时。当前门禁改为先确认 V2“连续能量潮 / 整机呼吸”，再制作不显示比例、档位名和逐颗大分数的三手对比样片；样片确认前不修改 `Assets/Resources/Art/`、Unity 代码、玩法、数据或存档。

## F009 连续能量潮三手对比样张 V2（2026-07-21）

本样张以 F021 午夜旧街机机台为结构参考，用相同机位、相同六槽和相同骰面并列展示明显未达、刚好过关与暴击三种整机负载。画面不显示比例、档位名、逐颗大分数、明确目标数字或主动结算提示，只通过连续能量流、回路闭合程度、积分塔填充、终端波形与整机余辉表达强弱。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 连续能量潮三手同机位样张 V2 | `Assets/ArtSource/Production/F009/ContinuousEnergyWavePreviewV2/f009_continuous_energy_wave_three_hand_contact_v2_20260721.png` | 不适用 | 用户已退回 | `1672×941` PNG；用户反馈整体青色配色太阴冷，暴击只是扩大亮度，没有形成激烈、直接的视觉冲击。只保留为失败证据。 |

2026-07-21 用户退回 V2。该图不得继续作为动态或 Unity 参考；后续改用暖色高能路线，并把暴击从“全机更亮”改为“连续蓄能末端的一次白热重击”。

## F009 暖色高能与暴击重击样张 V3（2026-07-21）

V3 保留 V2 的固定六槽、同机位、弱数值化和整机反馈原则，但将主能量从冷青改为暗铜、琥珀、金橙、白热与红热余波。暴击列不再只是扩大灯路覆盖，而是展示白热核心、金橙冲击波、整机缝隙点燃、火星与机柜回坐共同形成的单次直接重击。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 暖色高能三手同机位样张 V3 | `Assets/ArtSource/Production/F009/ContinuousEnergyWavePreviewV3/f009_continuous_energy_wave_warm_impact_contact_v3_20260721.png` | 不适用 | 需整体修订、保留对照 | `1672×941` PNG；暖色主导和单次暴击峰值意图保留，但外部黑房、管线、地面反射、熔融材质、体积光和全机橙白化偏离 F021 已通过的扁平旧街机体系，不得作为造型、材质、空间、动态或 Unity 参考。 |

2026-07-21 整体审核后，V3 改判为需整体修订。只保留暖色力量感、隐藏分档和单次重击意图；后续不得沿用其场景、材质、火星、熔融缝隙或全屏辉光。

## F009 旧街机过载整体修改方案 V4（2026-07-21）

V4 静态样张已被用户要求修订，当前状态为 `用户要求修订、保留对照`。本轮曾直接使用已批准的 `Assets/Resources/Art/MainGame/arcade_main_game_common_base.png` 和 F021 运行截图作为编辑目标，只叠加二维旧街机覆盖层，不重新设计整台机柜。主能量为琥珀和温白硬芯，热橙 / 珊瑚红只用于暴击瞬时边缘，青色只保留目标回路闭合语义，锈红只保留最终失败语义。

候选覆盖层为骰台下分段电流、既有积分塔连续充能、终端点阵波形、温白冲击扫描、金橙边缘残像和小幅整机回坐。禁用外部房间、管线、地面反射、熔融金属、烟尘粒子雨、持续高 Bloom、全机橙白化和新档位文字。两张静态评审板通过后才制作带声音的连续动态预览，动态通过后才允许生成运行资源或修改 Unity。

全部产物位于 `Assets/ArtSource/Production/F009/OldArcadeOverloadPreviewV4/`，没有 Unity 运行时加载 key。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 过关独立状态 | `f009_v4_pass_preview_20260721.png` | 不适用 | 用户要求修订、保留对照 | 薄琥珀闭环、稳定积分塔和紧凑终端波形；青色只剩极小目标闭合提示。 |
| 远超过独立状态 | `f009_v4_far_exceed_preview_20260721.png` | 不适用 | 用户要求修订、保留对照 | 双层琥珀灯路、局部温白硬芯和双波形；不触发整屏冲击扫描。 |
| 暴击主击独立状态 | `f009_v4_critical_impact_preview_20260721.png` | 不适用 | 用户要求修订、保留对照 | 温白横向硬扫描、金橙冲击带、终端过载波形和第四槽主焦点；无熔融机柜与粒子雨。 |
| 暴击预压独立状态 | `f009_v4_critical_precompress_preview_20260721.png` | 不适用 | 用户要求修订、保留对照 | 能量向第四槽内收、环境灯短降、终端波形压缩；尚未释放主击。 |
| 暴击余震独立状态 | `f009_v4_critical_aftershock_preview_20260721.png` | 不适用 | 用户要求修订、保留对照 | 主扫描已离场，保留断续灯路、衰减波形和第四槽余辉；已修正首稿错误的第五槽高亮。 |
| 强度分级贴合板 | `f009_v4_style_fit_review_board_20260721.png` | 不适用 | 用户要求修订、保留对照 | `2048×1280`；外部标签为过关、远超过、暴击，游戏画面内部不显示档位或百分比。SHA-256 `9ED8028E47B8D4B59EC28C14E00E33CB33BCC57E5E8124F7B20ECABC5E478B6E`。 |
| 暴击四帧评审板 | `f009_v4_critical_four_frame_review_board_20260721.png` | 不适用 | 用户要求修订、保留对照 | `2048×1280`；预压、主击、余震和恢复四格。SHA-256 `B38989DC1C47C679201DF8AAE70248B9A0D3B43E8F8CCE01D13F7FC59907F40E`。 |
| 拼板脚本与指标 | `build_f009_v4_preview_boards.py`、`f009_v4_preview_metrics_20260721.json` | 不适用 | 生产源文件 | 确定性生成两张评审板并记录输入尺寸、输出尺寸与哈希；不是 Unity 运行逻辑。 |

退回原因：共享台下灯路、槽间接力和贯穿骰列的冲击仍会让六颗骰子看起来彼此传导，形成杂乱关系网，并削弱左侧本关得分的主角地位。V4 不再进入动态验证。本轮没有修改 `Assets/Resources/`、Unity 代码、玩法、数据或存档。

## F009 单向投分 / 本关得分承击方案 V5 / V5R1 / V5R2（2026-07-21）

V5 / V5R1 / V5R2 已生成四张源区评审图，仍未生成运行资产。结算关系保持每个当前物理槽直接作用于左侧本关得分 / 积分塔。V5R1 实体骰离槽撞塔因动作过大、抢走得分注意力而退回；V5R2 改为六骰全程原位，当前骰只做低幅聚焦并析出小型积分光晕，单骰真实贡献主要通过积分塔填充、回弹与余辉连续表达。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| V5 四帧单向投分评审板 | `Assets/ArtSource/Production/F009/DirectScorePacketPreviewV5/f009_v5_direct_score_four_frame_review_20260721.png` | 不适用 | 样张待验收 | `1672×941`；第一槽预备、直接投分、积分塔吸收、余辉与第二槽预备交叠。SHA-256 `F17F44444DFED18B4781E90E93DC70E1E4BB24CACA236C2D967B8C6A4A9CF5C1`。 |
| V5 积分塔五档响应板 | `Assets/ArtSource/Production/F009/DirectScorePacketPreviewV5/f009_v5_score_tower_five_tier_review_20260721.png` | 不适用 | 样张待验收 | `1774×887`；五格仅用外部编号组织，强度全部集中在左侧积分塔。SHA-256 `21D892CE7990BE7CE7AA558163B1DF2D21F1ECD59DA6DCC091AEFC7BDB0F024C`。 |
| V5R1 单骰贡献重量评审板 | `Assets/ArtSource/Production/F009/DirectScorePacketPreviewV5/f009_v5r1_individual_die_impact_weight_review_20260721.png` | 不适用 | 用户退回、保留对照 | `1672×941`；实体骰离槽撞塔会分走结算注意力。SHA-256 `B6A2E211038DF3F06BCC437DB96F3BC5323AC54FF7CE3C8A5FBE55E4750BBAD5`。 |
| V5R2 原槽聚焦与积分光晕评审板 | `Assets/ArtSource/Production/F009/DirectScorePacketPreviewV5/f009_v5r2_stationary_die_score_halo_weight_review_20260721.png` | 不适用 | 样张待验收 | `1672×941`；六骰完整原位，当前奶油色 `3` 点骰只做低幅聚焦，小型点阵光晕与积分塔响应展示四个连续重量采样。SHA-256 `2DA8400CC45D681C1BF68261755B31660A891A8286E078644C151BA430AF80E6`。 |

上述评审图均以 F021 当前运行截图和共用母版为参考，通过内置 `imagegen` 生成；都不具有 Unity 加载 key，不得放入 `Assets/Resources/Art/` 或被代码引用。V5R2 用户确认后才制作带临时声音的连续动态样片。

## F021 主游戏 LED 字体校准板 V1（2026-07-17）

Unity 初步接入暴露出当前运行字库在低字号下明显比已批准预览更细、更小、更疏。V1 校准板以当前运行字库、字形映射、虚拟坐标、换行、对齐和 `Point` 采样规则为事实来源，分别比较批准参考、当前运行方案、A「10×10 粗灯珠」和 B「11×11 紧凑灯珠」。用户于 2026-07-17 退回 A / B：两者相比批准预览仍有差距且显示模糊。V1 全部保留为源区失败证据，没有运行时加载 key，不得据此替换 `Assets/Resources/Art/MainGame/Flow/` 中的正式字库。

| 资源 | 源文件 | 状态 | 备注 |
|---|---|---|---|
| `1920×1080` 校准板预览 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV1/led_font_calibration_board_v1_1920x1080_preview_20260717.png` | 用户退回、不得作为真值板 | 对照顶部主信息、积分塔、右侧终端、底部主按键和最小辅助字；发布预览存在整板 `LANCZOS` 二次缩小，不能用于锐度判断。 |
| `1280×720` 校准板预览 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV1/led_font_calibration_board_v1_1280x720_preview_20260717.png` | 用户退回、不得作为真值板 | 当前最小灯珠为 `0.875 px`，A 为 `2.20 px`，B 为 `2.00 px`；仅尺寸达标不足以证明清晰。 |
| A 完整 Ready 候选 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV1/led_font_candidate_a_coarse_ready_1920x1080_v1.png`、`led_font_candidate_a_coarse_ready_1280x720_v1.png` | 需重做、不得接入 | `10×10` 字图被缩放到多组非整数尺寸并叠加扩大辉光；人工验收仍显模糊。 |
| B 完整 Ready 候选 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV1/led_font_candidate_b_compact_ready_1920x1080_v1.png`、`led_font_candidate_b_compact_ready_1280x720_v1.png` | 需重做、不得接入 | `11×11` 字图同样没有建立完整的整数像素合同；人工验收仍显模糊。 |
| 候选源字图与映射 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV1/led_font_candidate_*_atlas_v1.png`、`led_font_candidate_*_map_v1.csv` | 评审源文件 | 只用于复现候选，不是正式运行资源；用户批准后再由正式字库生成器生产运行文件。 |
| 自动指标报告 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV1/led_font_calibration_metrics_v1_20260717.json` | 指标不完整、不得据此放行 | A / B 只通过核心灯珠、有效字高、区域装入与缺字；缺少锐度、非整数缩放和辉光占比门禁，人工验收失败优先。 |

可复现生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_led_font_calibration_v1.py`。详细记录：`Docs/QA/20260717_led_font_calibration_v1.md`。本轮没有调用图像生成模型，没有使用 Computer Use，也没有修改 Unity 代码、正式运行字库、玩法、数据或存档。

## F021 主游戏 LED 字体像素级校准 V2（2026-07-17）

V2 只修复 V1 的模糊验收链路，不扩张到正式运行资源。文字核心改为整数物理像素的逐灯珠绘制，完整帧直接输出目标分辨率；区域对照保持 1:1，放大检查只使用整数 `4× NEAREST`。无辉光硬核心与受限 `1px` 辉光分开交付，避免再次把字形与辉光混成一个变量。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| V2 无辉光完整 Ready | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV2/led_font_v2_hard_core_ready_1280x720.png`、`led_font_v2_hard_core_ready_1920x1080.png` | 不适用 | 清晰度通过、字形退回、不得接入 | 原生目标分辨率；无抗锯齿、无字图缩放、无辉光。像素链路保留，`7×7–10×10` 中文字形无效。 |
| V2 受限辉光完整 Ready | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV2/led_font_v2_controlled_halo_ready_1280x720.png`、`led_font_v2_controlled_halo_ready_1920x1080.png` | 不适用 | 清晰度通过、字形退回、不得接入 | 保留同一硬核心，只增加独立 `1px / Alpha 34` 外层；没有模糊滤镜。 |
| V2 1:1 区域对照 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV2/led_font_v2_*_comparison_1to1.png` | 不适用 | 评审源文件 | 分别覆盖顶部、积分塔、终端、按键和辅助字；纵向堆叠，裁切像素不重采样。 |
| V2 720p 像素放大检查 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV2/led_font_v2_1280x720_*_pixel_zoom4x.png` | 不适用 | 评审源文件 | 顶部、终端和按键使用整数 `4× NEAREST`，只用于查看灯珠几何。 |
| V2 自动报告 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV2/led_font_calibration_metrics_v2_20260717.json` | 不适用 | 清晰度指标通过、字形指标缺失 | 原生分辨率、1:1 裁切、整数灯珠、区域装入、缺字和辉光分层通过；未验证字形身份，不能放行。 |

可复现生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_led_font_calibration_v2.py`。详细记录：`Docs/QA/20260717_led_font_calibration_v2.md`。用户已确认清晰度，但字形退回；不生成运行时 atlas / map，不修改 Unity 代码。

## F021 主游戏 LED 字体字形识别压力板 V3（2026-07-17）

V3 只验证真实中文是否可读，不制作完整 Ready。候选使用 `16×16` Noto Sans SC 中等字重 hinted mask，720p 输出为 `2×2 px` 二值硬核心、`3 px` 点距、无辉光和无缩放；旧文字框不再反向压缩字形。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| V3 字形识别压力板 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRecognitionV3/led_font_v3_recognition_pressure_board_1280x720.png` | 不适用 | 字形通过、基线退回 | 原生 `1280×720`；用户已确认字形可识别，但逐字顶对齐不处于同一水平线。 |
| V3 1:1 盲读板 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRecognitionV3/led_font_v3_blind_reading_sheet_1to1.png` | 不适用 | 字形通过、基线退回 | 不依赖完整 UI；T1–T7 字形身份通过，排版基线转入 V4。 |
| V3 复杂字像素检查 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRecognitionV3/led_font_v3_complex_glyphs_zoom2x_nearest.png` | 不适用 | 评审源文件 | 只使用整数 `2× NEAREST`；覆盖 `章 / 额 / 规 / 检 / 骰 / 袋 / 调 / 整 / 结 / 算 / 顺 / 序 / 掷`。 |
| V3 自动报告 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRecognitionV3/led_font_recognition_metrics_v3_20260717.json` | 不适用 | 字形检查通过、缺少基线指标 | 同时回算旧框；积分塔、终端标题 / 正文和微提示需要在 V4 通过后回排。 |

可复现生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_led_font_recognition_v3.py`。详细记录：`Docs/QA/20260717_led_font_recognition_v3.md`。本包没有运行时 key，不得据此修改 Unity。

## F021 主游戏 LED 字体基线校准 V4（2026-07-17）

V4 响应用户“字形通过，但不处于同一水平线”的反馈，只修复纵向锚点。它复用 V3 的活动二值字形，不改字重、阈值、灯珠数、字宽、字符步进或辉光；中文、英文大写和数字统一落到逻辑第 `14` 行，短横 / 间隔点使用显式中线。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| V4 基线校准主板 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontBaselineV4/led_font_v4_baseline_calibration_board_1280x720.png` | 不适用 | 用户已通过、全局文字基准 | 左侧保留 V3 逐字顶对齐失败证据，右侧为 V4 唯一候选；原生 `1280×720`。 |
| V4 统一基线 1:1 验收条 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontBaselineV4/led_font_v4_unified_baseline_1to1.png` | 不适用 | 用户已通过 | 覆盖章节、生命 / 金币、目标 / 当前、`SPACE`、数字串与中英混排。 |
| V4 基线像素检查 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontBaselineV4/led_font_v4_baseline_zoom2x_nearest.png` | 不适用 | 评审源文件 | 只使用整数 `2× NEAREST`，绿线只用于暴露共享底线。 |
| V4 自动报告 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontBaselineV4/led_font_baseline_metrics_v4_20260717.json` | 不适用 | 自动与人工均通过 | 36 个中英数字符底线偏差 `0 px`；V3 活动字形哈希和亮灯数全部守恒。 |

可复现生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_led_font_baseline_v4.py`。详细记录：`Docs/QA/20260717_led_font_baseline_v4.md`。本包本身仍是源区验收证据；用户通过后派生的正式资源见下节。

## F021 全局 LED 文字运行资源 V4（2026-07-17）

用户已将 V4 指定为无特别说明时的默认全局文字标准。正式资源直接复用 V3 已通过的 `16×16` 二值字形与 V4 共享基线，并覆盖主菜单、开场、主流程、设置、市场、Tooltip、结果页与旧回退界面的动态文字。明确例外为骰面实体压印数字和 `DICE KING` 实体点阵招牌。

| 资源 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| V4 正式二值字形 atlas | `Assets/Resources/Art/MainGame/Flow/main_game_led_font_atlas.png` | `Art/MainGame/Flow/main_game_led_font_atlas` | 已生成并接入 | `576×486`、当前 `850` 字形；生成器按代码 / 数据字符集合动态重算，Alpha 仅 `0 / 255`，`Point`、无 mipmap、无压缩。 |
| V4 正式字形 map | `Assets/Resources/Art/MainGame/Flow/main_game_led_font_map.csv` | `Art/MainGame/Flow/main_game_led_font_map` | 已生成并接入 | 每字记录 `16` 行位图、活动宽度和锚点；当前代码 / 数据字符全覆盖，无重复 codepoint。 |
| V4 三档共享样式合同 | `Assets/Resources/Art/MainGame/Flow/main_game_led_font_styles.csv` | `Art/MainGame/Flow/main_game_led_font_styles` | 已生成并接入 | `Display / Compact / Micro` 的灯珠、点距、字间和行距由同一 CSV 提供给离线生成器与 C# 运行时，禁止两边复制不同常量。 |
| V4 双分辨率真值与上下文图 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRuntimeV4/` | 不适用 | 离线检查通过 | 包含全局真值板，以及主菜单、Run Ready、市场的 `1280×720 / 1920×1080` 同源上下文图。 |
| V4 自动指标 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRuntimeV4/led_font_runtime_metrics_v4_20260717.json` | 不适用 | 全部检查通过 | 字形守恒、底线偏差 `0`、二值 atlas、缺字、绘制入口白名单和命名样式均通过。 |

可复现生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_main_game_led_font_atlas.py`。运行时由 `MainGameLedFont.cs` 读取 map 与共享样式 CSV 后按当前屏幕物理像素整数取整，并使用 `Display / Compact / Micro` 三档命名样式；不再把整张字图缩到任意字号。离线上下文图同步执行运行时 Rect 裁切。验证记录：`Docs/QA/20260717_led_font_runtime_v4.md`。

## F021 骰子类型芯 V1（2026-07-17）

用户已批准“家族壳 + 机械类型芯 + 结果数字”的方向 A。首批 37 枚透明静态类型芯与对应活动遮罩已生成并通过资源合同和实际尺寸总表检查；当前进入 Unity 程序接入与 Play Mode 验收，双家族 9 枚仍不在本批范围。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 12 枚压力测试图 | `Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/arcade_dice_type_core_pressure_contact_v1_20260717.png` | 不适用 | 用户已通过、可批量生产 | 覆盖静态 `Ready`、身份动作峰值和结果缩小类型芯；家族壳与具体类型分层成立。 |
| 37 枚生产规格 | `Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/TYPE_CORE_PRODUCTION_SPEC.md` | 不适用 | 已冻结首批范围 | 基础 1、猪 7、恶魔 5、贡品 1、龟 7、海盗 8、中立 8；双家族另批。 |
| 基础＋中立生产板 | `Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/arcade_type_core_board_neutral_basic_v1_20260717.png` | 不适用 | 已通过、已派生运行资源 | 3×3，共 9 枚。 |
| 猪猪生产板 | `Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/arcade_type_core_board_pig_v1_20260717.png` | 不适用 | 已通过、已派生运行资源 | 上四下三，共 7 枚；小状态已统一猪猪壳。 |
| 恶魔＋贡品生产板 | `Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/arcade_type_core_board_devil_tribute_v1_20260717.png` | 不适用 | 已通过、已派生运行资源 | 3×2，共 6 枚；贡品保持无家族系统壳。 |
| 龟龟生产板 | `Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/arcade_type_core_board_turtle_v1_20260717.png` | 不适用 | 已通过、已派生运行资源 | 上四下三，共 7 枚。 |
| 海盗生产板 | `Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/arcade_type_core_board_pirate_v1_20260717.png` | 不适用 | 已通过、已派生运行资源 | 4×2，共 8 枚。 |
| 37 枚静态类型芯 | `Assets/Resources/Art/DiceTypes/arcade_type_core_<名称>.png` | `Art/DiceTypes/arcade_type_core_<名称>` | 已生成、待程序接入 | `512×512 RGBA`；`Ready` 与市场放大显示，结果态复用同图缩入数字下方。 |
| 37 枚活动遮罩 | `Assets/Resources/Art/DiceTypes/arcade_type_core_<名称>_activity.png` | `Art/DiceTypes/arcade_type_core_<名称>_activity` | 已生成、待程序接入 | `512×512 RGBA`；只保留琥珀、青色或家族活动节点，结果态禁用。 |
| 实际尺寸与极限数字总表 | `Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/RuntimeSourcesV1/qa/arcade_type_core_runtime_qa_80x76_v1_20260717.png` | 不适用 | 人工检查通过 | 同屏覆盖 37 枚 `80×76` Ready 芯与 `8888 + 结果小芯`；高风险近似组仍可区分。 |
| 自动资源指标 | `Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/RuntimeSourcesV1/qa/arcade_type_core_runtime_metrics_v1_20260717.json` | 不适用 | 37 / 37 通过 | 检查尺寸、RGBA、透明角、主体包围框、活动像素与文件数量。 |

正式名称集合为 `basic`、`pig_farmer`、`meat_pig`、`trade_pig`、`sow_pig`、`three_little_pigs`、`greedy_pig`、`feed_wholesaler`、`imp`、`devourer`、`demon`、`demon_bat`、`abyss_summon`、`tribute`、`money_turtle`、`tiny_turtle`、`double_turtle`、`lucky_turtle`、`magnet_turtle`、`rally_turtle`、`leader_turtle`、`refresh_pirate`、`plunder_pirate`、`crew_pirate`、`pirate_captain`、`training_pirate`、`treasure_pirate`、`robbery_pirate`、`pirate_king`、`lightfang`、`duet`、`trigger`、`crown`、`relief`、`airstrike`、`pact`、`stitch`。静态与活动资源分别使用上表两种加载 key；双家族继续使用既有家族通用回退，直到后续独立批次完成。

可复现后处理脚本：`Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/build_runtime_type_cores_v1.py`。色键原图、透明中间图和运行源图保存在 `RuntimeSourcesV1/`；脚本只负责统一 `512×512`、活动遮罩、运行文件与 QA 总表，不替代原始图像生成。

## F021 LED 语义亮度 V1 样张（2026-07-17）

本包只验证正式 V4 点阵字在真实主菜单、市场与 Tooltip 中的语义亮度、局部底板和状态差异；不生成新字形或新背景，不作为运行时加载资源。所有产物位于 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV1/`。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 语义角色候选合同 | `main_game_led_text_roles_v1.csv` | 不适用 | 需修订、保留对照 | 用户认可分级，但同级色相没有共同感知亮度目标，不得派生运行时副本。 |
| 主菜单上下文 | `led_text_brightness_v1_main_menu_{core,focus}_{1280x720,1920x1080}.png` | 不适用 | 分级认可、同级待修 | 选中标题与实体 `SPACE` 为 `L3`，副标题为 `L1`，无存档继续为可读 `Disabled`。 |
| 市场常态上下文 | `led_text_brightness_v1_market_{core,focus}_{1280x720,1920x1080}.png` | 不适用 | 分级认可、同级待修 | 商品名 `Display`、六面 `Compact`；米色键使用同材质浅铭牌带与深墨 V4 字。 |
| 市场 Tooltip 上下文 | `led_text_brightness_v1_market_tooltip_{core,focus}_{1280x720,1920x1080}.png` | 不适用 | 分级认可、同级待修 | 名称、六面与价格为主信息，家族 / 触发和规则为次信息。 |
| 状态与缩放压力 | `led_text_brightness_v1_state_strip_*`、`led_text_brightness_v1_*_core_90pct_1152x648.png` | 不适用 | 评审源文件 | 状态条覆盖焦点、主信息、次信息、禁用和浅键深字；`90%` 只验层级。 |
| 可复现生成器与报告 | `build_led_text_brightness_v1.py`、`led_text_brightness_metrics_v1_20260717.json` | 不适用 | 机器检查通过 | 正式 glyph 覆盖、关键角色几何、局部对比、裁切、双分辨率和辉光边界均通过。 |

本包没有调用图像生成模型，没有使用 Computer Use，没有修改 `Assets/Resources/`、Unity 代码、玩法、数据或存档。V1 只保留为分级基准与同级不一致证据。

## F021 LED 语义亮度 V2 同级等亮样张（2026-07-17）

本包复用 V1 的页面分级、正式 V4 字形、真实底板和实际 Rect，只新增 `OKLab L` 同级目标及统一暗底比较条。所有产物位于 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV2/`。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| V2 语义角色合同 | `main_game_led_text_roles_v2.csv` | 不适用 | 样张待验收 | `10` 项角色；琥珀锚点不变，焦点青、L1 暖白、失败和成功色完成同级校准。 |
| 同级亮度目标合同 | `main_game_led_text_brightness_groups_v2.csv` | 不适用 | 机器检查通过 | 每个角色登记 `brightness_grade / target_oklab_l / tolerance / anchor_role`；容差 `0.0002`。 |
| 同级统一底色比较 | `led_text_brightness_v2_same_grade_{core,focus}_{1280x360,1920x540}.png` | 不适用 | 样张待验收 | 同屏比较 L3 四色与 L1 两色；核心与辉光分开。 |
| 三种实景上下文 | `led_text_brightness_v2_{main_menu,market,market_tooltip}_{core,focus}_{1280x720,1920x1080}.png` | 不适用 | 样张待验收 | 页面结构和实际 Rect 与 V1 一致，只替换 V2 角色色。 |
| 状态与缩放压力 | `led_text_brightness_v2_state_strip_*`、`led_text_brightness_v2_*_core_90pct_1152x648.png` | 不适用 | 评审源文件 | `90%` 只验层级，不用于锐度判断。 |
| 可复现生成器与报告 | `build_led_text_brightness_v2.py`、`led_text_brightness_metrics_v2_20260717.json` | 不适用 | 机器检查通过 | `320` 次绘制；最大目标差 `0.000045263`、同级跨度 `0.000067127`；对比、裁切、Micro 和 glyph 均零失败。 |

V2 没有调用图像生成模型或 Computer Use，没有修改 `Assets/Resources/`、Unity 代码、玩法、数据或存档。用户视觉确认前不派生运行时资源。

## F021 市场浅色实体键帽实心字 V1 样张（2026-07-17）

本包只验证关间市场卖出、三枚购买和离开市场五处浅色实体键帽的非点阵文字。所有产物位于 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketPhysicalKeyLabelsV1/`，不具有运行时加载 key。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 市场完整样张 | `market_physical_key_labels_v1_full_{1280x720,1920x1080}.png` | 不适用 | 需修订、保留对照 | 用户退回新增纯色键帽内衬；实心字方向可保留。 |
| 原生 1:1 键帽条 | `market_physical_key_labels_v1_key_strip_{1280x260,1920x390}.png` | 不适用 | 失败证据 | 用于定位平色内衬遮住原键帽纹理的问题。 |
| 状态与长文案压力板 | `market_physical_key_labels_v1_state_stress_{1280x500,1920x750}.png` | 不适用 | 需修订、保留对照 | 状态和装入通过，但承载材质失败。 |
| 可复现生成器与报告 | `build_market_physical_key_labels_v1.py`、`market_physical_key_labels_metrics_v1_20260717.json` | 不适用 | 机器检查通过、人工失败 | 平色对比度通过不能覆盖材质连续性的人工失败。 |

当前字体来源为系统安装的 `Noto Sans SC` 可变字体，样张记录了文件哈希；它不是正式运行依赖。用户通过后必须将批准字形固化为仓库内字体或固体字图集，再登记运行时资源和绘制入口。本轮没有修改 `Assets/Resources/`、C#、玩法、经济、数据或存档，也没有启动 Unity。

## F021 市场浅色实体键帽实心字 V2 与运行资源（2026-07-17）

V2 删除 V1 的平色承载层，从现有市场无字底图恢复原始键帽材质并直接印入实心字。用户已通过样张并授权进入 Unity；样张位于 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketPhysicalKeyLabelsV2/`，正式字形已作为独立运行资源固化。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 市场完整样张 | `market_physical_key_labels_v2_full_{1280x720,1920x1080}.png` | 不适用 | 用户已通过 | 原键帽磨损、颗粒、凹凸与局部明暗全部恢复，无新增底板。 |
| 原生 1:1 键帽条 | `market_physical_key_labels_v2_key_strip_{1280x260,1920x390}.png` | 不适用 | 用户已通过 | 实心字直接落在原材质上的融合度和可读性获得确认。 |
| 状态与长文案压力板 | `market_physical_key_labels_v2_state_stress_{1280x500,1920x750}.png` | 不适用 | 用户已通过 | 状态变化均保留原纹理；最长文案装入通过。 |
| 可复现生成器与报告 | `build_market_physical_key_labels_v2.py`、`market_physical_key_labels_metrics_v2_20260717.json` | 不适用 | 机器检查通过 | 无纯色底板；纹理方差、双分辨率、对比、字高、装入与压力文案全部通过。 |
| 运行时静态字体子集 | `Assets/Resources/Art/MainGame/Flow/wabish_physical_key_sans_sc_semibold.ttf` | `Art/MainGame/Flow/wabish_physical_key_sans_sc_semibold` | 已生成并接入 | `19,788` bytes；家族名 `Wabish Physical Key Sans SC`，静态 `SemiBold / 600`，当前 51 个可达键帽字符全覆盖；SHA-256 `72e0a6b2…ac5c7c5`。 |
| 字体构建脚本与冻结清单 | `build_physical_key_runtime_font_v1.py`、`wabish_physical_key_sans_sc_semibold_manifest.json` | 不适用 | 重复生成一致 | 从批准样张字形固定 600 字重、裁切字符、改名并写入许可证元数据；连续两次生成哈希一致。 |
| 字体许可证 | `Assets/Resources/Licenses/WabishPhysicalKeySansSC_OFL.txt` | `Licenses/WabishPhysicalKeySansSC_OFL` | 已入库 | SIL Open Font License 1.1；修改后的主要字体名不使用上游保留名称 `Source`。 |
| Unity 双分辨率运行截图 | `Docs/QA/20260717_market_runtime_{normal,purchase,leave}_{1280x720,1920x1080}.png` | 不适用 | Play Mode 自动检查通过 | 常态、购买留空、离场锁定各两档；不使用 Computer Use。 |
| 运行时像素指标 | `Docs/QA/20260717_market_physical_key_runtime_metrics_v1.json` | 不适用 | 全部通过 | 启用 / 禁用精确实心色、字高、居中、三枚购买键同线、字体哈希、六图尺寸与纹理方差均通过。 |

运行时由 `DiceKingDemo.DrawArcadePhysicalKeyLabel` 只处理五处浅色键帽，并以当前 GUI 矩阵换算为物理像素后直接栅格化；720p 使用 `22 / 24 px`，1080p 使用 `33 / 36 px`。暗色刷新键与其它市场文字继续 V4 LED。市场布局、热区、玩法、经济、数据与存档均未改变。

## F021 LED 语义亮度 V2 正式运行资源（2026-07-17）

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 语义角色合同 | `Assets/Resources/Art/MainGame/Flow/main_game_led_text_roles.csv` | `Art/MainGame/Flow/main_game_led_text_roles` | 已生成并接入 | `10` 个角色；定义等级、几何首选 / 下限、核心色、Alpha、局部底色与用途。Python 与 C# 共读。 |
| 同级亮度合同 | `Assets/Resources/Art/MainGame/Flow/main_game_led_text_brightness_groups.csv` | `Art/MainGame/Flow/main_game_led_text_brightness_groups` | 已生成并接入 | 记录每级 `OKLab L` 目标、锚点与 `0.0002` 容差；同级最大实测跨度 `0.000067127`。 |
| `0.606×` 压力构建器 | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV2/build_led_text_runtime_stress_v1.py` | 不适用 | 自动检查通过 | 从 1080p 原图确定性生成 `1164×654` 三屏压力图、前后对照与 JSON；不作为运行时资源。 |
| 运行 QA | `Docs/QA/20260717_led_text_brightness_runtime_v1.md` | 不适用 | 自动通过、用户视觉待确认 | 覆盖主菜单、局内、市场原生双分辨率与 `0.606×` 压力；未使用 Computer Use。 |

历史 `main_game_led_text_roles_v2.csv` 与 `main_game_led_text_brightness_groups_v2.csv` 仅保留在样张目录作为候选快照；正式生成器已改读上述 Resources 文件。五处浅色市场实体键帽仍使用独立实心字体资源，不由本合同改写。

## F009 V4R6 固定正方体骰面滚轴（2026-07-21）

本版本已获用户确认并接入主局。骰子外壳固定为现有 `128×128` 正方体家族素材，不旋转、不缩放、不变成长方体；滚动期间只有中心点数层像老虎机滚轴一样上下移动。未新增运行时位图，既有 F021 家族外壳与类型芯继续作为正式资产来源。

| 资源 | 源文件 | 运行时加载 key | 状态 | 备注 |
|---|---|---|---|---|
| V4R6 接触效果图 | `Assets/ArtSource/Production/F009/FaceReelStopPreviewV4R6/f009_stationary_cube_face_reel_contact_v4r6_v1_20260721.png` | 不适用 | 用户已通过 | `1672×941`；SHA-256 `92E2B57189A945246CFA3E0F72FBE4BD04CCD174DAAF26FAA987F146DB90879E`；只作为方向与停靠节奏依据。 |
| 五类正方体骰壳 | `Assets/Resources/Art/MainGame/arcade_main_game_die_shell_{neutral,pig,turtle,devil,pirate}.png` | `Art/MainGame/arcade_main_game_die_shell_<family>` | 复用既有正式资源 | 全部 `512×512`，运行时落入 `128×128` 槽位；外壳在高速、减速与回弹阶段保持静止。 |
| 类型芯 | `Assets/Resources/Art/DiceTypes/arcade_type_core_<名称>.png` | `Art/DiceTypes/arcade_type_core_<名称>` | 复用既有正式资源 | 固定在骰面下部，仅数字层滚动，避免滚动过程丢失骰子身份。 |
| 动态数字滚轴 | `Assets/Scripts/DiceKingDemo.cs` | 不适用 | 已接入 | 通用序列不携带类型与最终结果信息；进入停靠段后才切入真实点数，并执行越界、回弹、锁定。 |
| Unity 运行 QA | `Docs/QA/20260721_f009_face_reel_runtime.md` | 不适用 | 双分辨率通过 | 覆盖高速滚动、从左到右停靠波和自动结算；截图为 `1280×720` 与 `1920×1080`。 |

旧条带滚动与整颗骰子旋转只保留为历史实现参考；F021 当前默认主局显示以本 V4R6 固定正方体方案为准。结算表现 V6B-R1 属于独立后续批次，不由本资产条目确认。
