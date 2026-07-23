# F021 美术需求

状态：主风格与五条动态已确认；F021-S2-R3 待完整回归；LED V4 继续用于暗色显示面；市场浅色实体键帽 V2 已接入；F021-S5-UI1 状态样图待验收
功能：F021 全流程节奏样片
关联美术文档：PROJECT_CONTEXT.md、ART_ASSETS.md

## 输入决策

- 当前屏幕级风格为 `Pixel Ledger Scoring Machine`，继承 `Bright Ledger Boardgame` 的明亮账本桌游基底。
- 粗样片必须有一定美术素材表现，不能以纯线框、几何块或抽象占位作为主视觉。
- 粗样片素材先作为源图和样片资产保存，不直接进入运行时资源目录。

## 2026-07-16 主游戏共用运行时资源覆盖

- 用户已单独批准 `F021-S2` 共用底板进入正式运行时生产；本节覆盖旧“只保留源图、不进入 Resources”限制，但只覆盖共用竞技台和模块化骰壳。
- 正式资源为一张 `1920x1080` 无文字底板，以及中立、猪、恶魔、龟、海盗五张 `512x512` RGBA 家族壳。文字、数字、积分灯条、具体类型、交互与结算反馈继续由 Unity 绘制。
- 双家族不新增第六张贴图，由程序把两张现有家族壳左右拼接；待机中心显示具体类型图标或短类型名，结果中心只显示当前有效数字。
- 通关暖光、失败锈红、生命扣除、重试扫描和生命归零断电仍是后续分支资源，不得烘入共用底板。
- 源图、派生脚本、运行路径和加载 key 以 `ART_ASSETS.md` 的“F021 主游戏共用运行时资源包 V1”为准。

## 2026-07-16 主游戏槽位与骰壳尺寸契约

- R2 的 `112×112` 内槽贴合度已被用户否定；R3 以底图连续卡槽边框为准，当前候选有效空槽为内部设计坐标 `128×128`。
- 六槽中心固定 `X=299+128n / Y=368`，中心距 `128`、逻辑净距 `0`；底图或骰壳资源不得自行改变这套横向节拍。
- 当前五张 `512×512` 家族壳不重生成。运行时公共有效区仍为 `(32,27,448,458)`；裁切后五类可见壳约宽 `123.1–127.7`、高 `127.4–128`。
- 后续个性化骰壳仍以 `512×512` RGBA 交付，但映射到 `128×128` 物理槽后主体必须贴近卡槽内沿；装饰角、耳、锚章可以有少量家族差异，主体不得重新缩回透明画布中央。
- 双家族资源继续由两张现有壳左右拼接，分割线必须使用公共有效区的中线，不能使用整张 PNG 的透明画布中线。
- 类型芯和数字不是骰壳位图的一部分；Unity 中心内容框固定为裁切后局部比例 `18.6%, 15.3%, 62.9%, 59.2%`。

## 2026-07-14 开始界面专项覆盖

- 本节只覆盖 `F021-S1` 开始界面；主流程、结算和市场仍保留原生产包事实，等待后续单独重做。
- 用户已确认午夜旧机厅单机台方向：固定采用 A「CRT 存档槽」结构，并以 A 结构细化稿 C「首次启动」作为视觉密度与氛围母版。该首屏不再使用王室、账本、皇冠、封印或公文包装。
- 常态画面以深海军蓝磨损机壳、暖琥珀 CRT、右侧雨窗和安静留白为主；动态遵循“雨一直在，机器偶尔老化”。
- C 常态关键帧继续作为构图母版，但 30 秒组合动态预览已被用户拒绝；后续不再把雨、流滴、吊灯与 CRT 合在同一轮评审，改为一个表现一个表现地单独确认。第一项吊灯断电—复亮的 5 秒专用预览已获用户接受，并已拆成 Unity 运行时四层。
- `DICE KING` 顶箱、菜单文字和当前焦点必须稳定；吊灯与 CRT 异常互斥，不出现固定节拍、整屏闪烁、赌场追灯或恐怖惊吓。
- 当前 `OnGUI` 首轮使用局部 clean plate、程序绘制雨线 / 水珠 / 水流和独立发光层；不再制作固定雨水视频或透明雨水图集。真实 `RenderTexture` / Shader 模糊、色散和弯曲后置。
- 历史动态预览：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_motion_preview_v1_20260714.mp4`；当前状态为用户已拒绝，不作为正式资源依据。
- 已接受样片：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_lamp_power_cut_preview_v1_20260714.mp4`；只演示左上吊灯局部线路瞬断、停黑和分层复亮，其它画面锁定。
- 运行时资源：`Assets/Resources/Art/MainMenu/arcade_main_menu_lamp_on.png`、`arcade_main_menu_lamp_off.png`、`arcade_main_menu_lamp_glow.png`、`arcade_main_menu_lamp_core.png`。本次按用户“新主界面直接替换旧首屏”的要求，以完整基准图派生原型四层；若后续需要全面改文案或本地化，仍需补无文字 clean plate。

## 2026-07-15 右窗双效果修订要求

- 右窗雨水必须拆成两个可独立判断的表现：效果一是玻璃之后持续、快速、略虚的窗外降雨；效果二是雨滴撞击玻璃后形成可见水珠，数滴低频合并并缓慢向下汇流。
- 窗外雨不得用静止白点代替，也不得和玻璃水痕具有相同速度、锐度或亮度；玻璃撞击先出现短促小环和附着水珠，再进入慢速汇流。
- 当前母版窗框上下内沿烘入的连续白色水珠必须在样片和正式资源中移除；动态雨线、水珠、撞击环和水痕遮罩必须向玻璃内部收缩，不接触窗框。
- 第一轮 V1 雨水预览只保留为修订对照。V2 分别交付 6 秒窗外降雨样片和 10 秒玻璃撞击汇流样片；两项都获用户接受前，不生产运行时图集、不配置参数、不接入 Unity。

## 2026-07-15 效果一 V3 与参数载体修订

- 用户指出 V2 窗外雨幕被读成向上运动；复核确认离线合成器的正值纵向滚动会让纹理向屏幕上方移动。V3 必须反转为负值滚动，使远雨和近雨都明确向下。
- V3 只修正运动方向，继续使用 V2 去白点窗景、内缩遮罩、雨幕密度、远近速度差、长度和透明度，避免在同一轮同时改变多项视觉变量。
- 本轮只交付效果一 V3 视频和评审图，仍位于 `Assets/ArtSource/`；不修改效果二，不创建运行时资源，不修改 Unity 代码或现有配置。
- 后续参数不得放入 CSV。效果一与效果二都验收通过后，再使用 Unity 可序列化的 Inspector 参数资源，优先采用 `ScriptableObject`，直接显示并持久保存雨幕密度、速度、远近层次、水滴数量、汇流速度和水流长度。
- 效果一 V3 已获用户接受，向下雨幕冻结为效果二评审背景。效果二必须另交 10 秒玻璃撞击汇流专用样片；不得用效果一通过替代玻璃层验收。

## 2026-07-15 随机多点玻璃层运行时冻结

- 效果二 V3 的水珠材质、撞击、附着、停滞和汇流语言作为条件基准；固定四滴和固定路径不作为终版。正式运行改为 `8–14` 颗可见水珠、`2–3` 个松散随机区域、邻近聚合和阈值触流。
- 已批准运行时 clean plate 为 `Assets/Resources/Art/MainMenu/arcade_main_menu_window_clean_patch.png`，加载 key 为 `Art/MainMenu/arcade_main_menu_window_clean_patch`；它必须覆盖旧全屏底图里的静态雨痕和窗框白点后，才允许程序雨水绘制。
- 雨线、水珠、撞击环、主流和低概率支流均由 Unity 程序绘制，不新增雨水位图或序列帧。颜色保持低对比冷青灰、暗部折射主体和克制高光，不做均匀白线、发光面条或覆盖窗框的水珠链。
- 参数资产为 `Assets/Resources/Config/main_menu_rain_profile.asset`。美术调节只改 Inspector 中的密度、速度、远近层、透明度、水珠尺寸 / 数量、聚合距离、流速、长度、宽度、支流 / 停滞概率和随机种子，不修改吊灯 CSV。
- 当前资源与代码已入库并通过编辑器导入 / 编译；最终美术门禁是 `1920x1080`、`1280x720` 和至少 30 秒 Play Mode 录屏，重点检查窗框边界、低分辨率线宽、随机分布和主流数量。

## 2026-07-15 DICE KING A 点阵补亮动态样片门禁

- 用户选择 A「点阵灯泡补亮」作为首个招牌候选，并要求先看动态预览，确认后再进入资源拆分和 Unity 接入。
- 上一张静态峰值帧中的少量半亮灯珠被读成常驻坏点，因此不再用静态坏点图代表最终状态。稳定态必须保持 `DICE KING` 全部方形灯珠完整可读；局部压暗只允许在动态事件中短暂出现，并必须以完整常亮收尾。
- V1 样片规格为 `1920x1080`、30 fps、5 秒、H.264、无声。`0.00–1.50s` 完整常亮；`1.50–1.70s` 四颗非 `I` 字灯珠短暂压暗；`1.66–1.92s` 依次补亮 `DICE` 与 `KING`；`1.92–2.12s` 收束，之后保持完整常亮直到循环结束。
- V1 直接复用当前运行首屏母版，只有顶箱灯珠发生变化；中文菜单、吊灯、CRT、右窗雨景、机柜和构图不参与合成。不得把压暗解释成拼写错误、整字熄灭或长期损坏。
- 动态样片：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_preview_v1_20260715.mp4`。
- 顶箱循环预览：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_preview_v1_20260715.gif`。
- 六节点全景 / 局部评审图：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_review_sheet_v1_20260715.png`、`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_detail_review_sheet_v1_20260715.png`。
- 合成规则：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/dice_king_marquee_a_dot_relight_preview_filtergraph_v1.ffgraph`。
- V1 反馈为整体表达过快，无法清楚形成停灭和电流不稳定感，当前状态改为 `需修订、保留对照`。全部文件只放在 `Assets/ArtSource/`；未改 `Assets/Resources/Art/`、Unity 代码、CSV 或 Inspector 参数。

### V2 慢速供电波动与参数化映射

- V2 样片规格为 `1920x1080`、30 fps、6 秒、H.264、无声。供电事件从 `1.15s` 开始，到 `2.78s` 完全恢复，总长约 `1.63s`；最低亮度约为常亮的 `20%`，近灭停留约 `0.20–0.26s`，包含两次失败回亮、`DICE → KING` 分区恢复和约 `0.30s` 的缓慢稳定收束。
- V2 不再随机关闭单颗灯珠，改为分区内所有方形灯珠同步降亮。暗态仍保留暗铜灯座和极弱余辉，避免再次形成永久坏点或拼写损坏。
- 动态样片：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_preview_v2_20260715.mp4`。
- 顶箱循环预览：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_preview_v2_20260715.gif`。
- 八节点全景 / 局部评审图：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_review_sheet_v2_20260715.png`、`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_detail_review_sheet_v2_20260715.png`。
- 合成规则：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/dice_king_marquee_a_dot_relight_preview_filtergraph_v2.ffgraph`。
- 当前状态为 `样张待验收`。V2 只在 `Assets/ArtSource/`，未修改运行资源、Unity 代码或配置。

V2 通过后，运行时优先使用独立 `ScriptableObject`，候选文件为 `Assets/Resources/Config/main_menu_marquee_profile.asset`，候选加载 key 为 `Config/main_menu_marquee_profile`。不扩展 CSV。Inspector 至少暴露以下参数组：

| 参数组 | 候选字段 | V2 默认含义 |
|---|---|---|
| 开关与循环 | `Enabled`、`LoopEnabled`、`PlayOnEnter`、`EventIntervalRange` | 是否启用、是否循环、进菜单是否演示一次、待机事件间隔 |
| 压暗与停灭 | `BrownoutDuration`、`MinimumBrightness`、`NearOffHoldDuration` | 两段电压下坠时长、最低亮度约 `0.20`、近灭停留约 `0.20–0.26s` |
| 不稳定脉冲 | `FailedPulseCount`、`FailedPulseDurationRange`、`FailedPulseBrightnessRange` | 失败回亮次数、单次持续和回亮幅度 |
| 分区复亮 | `RelightOrder`、`SectionStagger`、`RelightOvershoot` | `DICE → KING` 顺序、约 `0.20s` 分区间隔和回弹峰值 |
| 稳定收束 | `SettleDuration`、`GlowIntensity` | 约 `0.30s` 收束与最终辉光强度 |
| 调度与可访问性 | `PauseOnInput`、`MutualExclusionCooldown`、`ReducedFlashingScale` | 输入期间稳定、与吊灯 / CRT 峰值互斥、减少闪烁倍率 |

## 视觉意图

用接近最终风格的关键帧和动效素材证明完整游戏循环的观感：主菜单像一台待机的皇家账本计分机，主流程像明亮桌游机器，结算像账本过账并最终超额爆发，市场像货架展开的构筑空间。

## 资源清单

| 资源 | 用途 | 尺寸 / 格式 | 目标路径 | 来源决策 | 状态 |
|---|---|---|---|---|---|
| 主菜单关键帧 | 首屏、标题、开始新游戏 | `1920x1080` PNG | `Assets/ArtSource/Production/F021/main_menu_keyframe.png` | D1 / D7 / D10 | 第一轮待生产 |
| Ready 主流程关键帧 | 积分塔、六槽骰台、规则牌、底部 `Space` 条 | `1920x1080` PNG | `Assets/ArtSource/Production/F021/ready_main_run_keyframe.png` | D1 / D7 / D10 | 第一轮待生产 |
| 超额井喷关键帧 | 最终分明显超过目标，灯泡爆亮和盖章 | `1920x1080` PNG | `Assets/ArtSource/Production/F021/score_burst_keyframe.png` | D2 / D3 / D7 / D10 | 第一轮待生产 |
| 轻结算面板关键帧 | 过关、最终分、目标分、金币汇总 | `1920x1080` PNG | `Assets/ArtSource/Production/F021/clear_panel_keyframe.png` | D6 / D7 / D10 | 第一轮待生产 |
| 市场关键帧 | 三格货架、骰袋、购买一个骰子 | `1920x1080` PNG | `Assets/ArtSource/Production/F021/market_keyframe.png` | D4 / D7 / D10 | 第一轮待生产 |
| 开始转场关键帧 | 从菜单推入主桌面 | `1920x1080` PNG 或序列帧 | `Assets/ArtSource/Production/F021/start_transition/` | D1 / D7 | 第二轮待生产 |
| 投骰停转关键帧 | 槽位内旋转和逐颗显点 | `1920x1080` PNG 或序列帧 | `Assets/ArtSource/Production/F021/roll_stop_keyframes/` | D1 / D7 | 第二轮待生产 |
| 结算蓄势关键帧 | 左到右入账、积分塔预热 | `1920x1080` PNG | `Assets/ArtSource/Production/F021/scoring_charge_keyframe.png` | D2 / D3 / D7 | 第二轮待生产 |
| 金币效果关键帧 | 最终分定格后的奖励兑现效果 | `1920x1080` PNG 或局部序列帧 | `Assets/ArtSource/Production/F021/gold_effect_keyframes/` | D5 / D7 / D11 | 第二轮待生产 |
| 离开市场回环关键帧 | 货架收起并回到下一关 Ready | `1920x1080` PNG | `Assets/ArtSource/Production/F021/market_exit_keyframe.png` | D1 / D4 / D7 | 第二轮待生产 |
| 样片合成文件 | 评审用节奏样片 | MP4 / GIF / 分镜长图 | `Assets/ArtSource/Production/F021/preview/` | Q1 | 第三轮待确认格式 |

## 首轮生产批次

第一轮只生产五锚点接触图，不生产完整视频、不生产全量过渡帧、不切运行时资源。

| 锚点 | 画面目标 | 验收重点 |
|---|---|---|
| 主菜单 | 皇家账本计分机待机首屏 | 风格成立，不像宣传页或赌场机器 |
| Ready | 左侧积分塔、中央六槽骰台、右侧规则牌和底部 `Space` 条 | 主流程可读，状态平静 |
| 超额井喷 | 最终分明显超过目标分 | 井喷来自账本和积分塔，不像老虎机 |
| 轻结算面板 | 过关、最终分、目标分、金币汇总 | 信息轻，不抢胜场爽点 |
| 市场 | 三格货架、骰袋、购买一个骰子 | 市场像构筑空间，和结算屏分离 |

## 风格关键词

- 像素账本计分机
- 明亮皇家账本
- 桌游骰台
- 积分塔、指针、灯泡、盖章、纸带、金币口
- 轻松、机敏、可爱但不幼态
- 高分爆发但不赌场化

## 必须保留

- `1920x1080`、`16:9` 横版构图。
- 左侧积分塔 / 指针 / 灯泡。
- 中央六槽骰台。
- 右侧规则牌。
- 底部短 `Space` 条。
- 市场作为独立屏幕，包含三格货架和六槽骰袋。
- 结果必须看起来来自骰子和账本机器，不来自老虎机或赌场奖励机。

## 避免

- 纯线框、纯几何块、纯灰盒样片作为主视觉。
- 赌场黑红金、老虎机、拉杆、轮盘、假 jackpot。
- 幼态吉祥物、贴纸感、毛绒玩具、霓虹科幻、写实 3D。
- 在图中生成长文字、价格、规则说明或不可控数字文案。
- 把接触图直接放进 `Assets/Resources/Art/`。

## 生成 / 来源备注

本包可以使用 `$wabish-art-assets` 生成样片关键帧或方向样张。由于屏幕级风格已经确认，首轮不需要重新做 4 个风格方向；应直接按 `Pixel Ledger Scoring Machine` 做五锚点接触图。如果某张关键帧出现明显风格偏移，再局部回到方向评审。

## 放置 / 运行时用途

所有样片图、关键帧、动效源文件和合成文件先放在：

```text
Assets/ArtSource/Production/F021/
```

本阶段不放入：

```text
Assets/Resources/Art/
```

后续若样片冻结并进入正式运行资源生产，再按拆分结果登记到 `ART_ASSETS.md`。

## 实现备注

- 关键帧可以先表现完整画面，不要求每个资源已可独立切片。
- 首轮不要求逐帧动画、复杂序列帧或最终 runtime 切片。
- 动效可以先用有限帧、简单层运动说明或样片合成表达，不要求首轮直接可 Unity 接入。
- 若后续进入正式实现，需要把背景、积分塔、灯泡、指针、骰台、市场货架、金币、盖章等拆成运行时层。

## 阻塞项

无。交付格式待确认但不阻塞首轮关键帧生产。

## 验收

- 样片不是纯示意图，至少主菜单、Ready、井喷、轻结算面板和市场五个关键画面有明确美术完成度。
- 画面符合 `Pixel Ledger Scoring Machine`，没有赌场化或幼态化偏移。
- 玩家不读说明也能看出主流程、结算胜利和市场是同一套游戏。
- 超额井喷画面能传达最终分明显超过目标分。
- 轻结算面板清楚但不抢走胜场爽点。
- 五锚点接触图未通过前，不进入全量过渡帧和样片合成。

## 2026-07-16 主游戏九状态接触图生产覆盖

- 当前方向以项目最新确认的“午夜旧娱乐街机 / 老电子设备”覆盖本文件早期 `Pixel Ledger Scoring Machine` 口径；禁止账本、纸张、王室、赌场、蒸汽机械与现代汽车仪表台语汇。
- 已生成同一正视竞技台的三乘三九状态源图，覆盖 Ready、投骰峰值、第三槽停转、ResultDecision、第二槽局部结算、结算中越过目标、StageClear、可重试 StageFailed、RunOver。
- 固定布局继续服从当前 Unity 运行截图与 R3 六槽几何：顶部状态、左积分塔、中央连续六个 `128×128` 物理槽、右单终端、底部唯一 Space 键。
- 骰子继续服从家族壳 / 类型芯 / 锁定数字语法；贡献分独立显示，不覆盖骰面数字；确认结算前不预演总分，越过目标后仍播完余下结算。
- 关间市场不在图内，只允许 StageClear 的 `SPACE 进入市场` 去向；源图不进入运行时目录。
- 源图：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/arcade_dice_king_main_game_nine_state_contact_v1_20260716.png`。
- 当前状态：样张待用户验收；接受后才进入五条分段动态样片。

## 2026-07-16 九状态接触图 V2 定向修订

- 用户已认可 V1 的主体老街机风格；本轮只修订文字系统与骰子贴槽，不改变九状态顺序、机位、材质和结果色彩层级。
- 所有 HUD、规则终端、结算、结果、提示和按键文案改为开始界面 `DICE KING` 灯牌同系的离散 LED 点阵字。中文按规则点阵构字，英文与数字使用统一灯珠网格，禁止平滑印刷字体；骰面数字仍是非自发光实体压印字。
- 六骰严格映射到内部设计坐标 `(235+128n,304,128,128)`，中心 `X=299+128n / Y=368`，逻辑净距 `0`。壳体主体必须贴近槽内沿，动态模糊、停转、结果和局部结算都不得另行缩放或漂移。
- V1 状态改为“需修订、保留对照”；V2 源图为 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/arcade_dice_king_main_game_nine_state_contact_v2_led_fit_20260716.png`。
- 当前状态：V2 样张待用户验收；通过前不进入动态样片或运行时资源切分。

## 2026-07-16 五条主游戏动态样片 V1

- 九状态接触图 V2 已获用户通过，状态改为“静态基准已通过”；LED 点阵字、R3 `128×128` 连续六槽、同一机位和局部结果色彩全部冻结。
- 已分别制作五条 `1920×1080`、30 fps、H.264 High、无声短片：投骰—停转、逐骰结算—目标越线、通关收入、失败重试、生命归零断电。
- 投骰—停转只在固定六槽中表现旋转和揭晓；逐骰结算在目标越线后继续；通关收入按固定 / 利息 / 复利三拍；失败重试用一条水平扫描清场；生命归零按模块顺序断电并以 CRT 水平线收束。
- 关间市场继续排除；通关短片只停在 `SPACE 进入市场`。五条短片均为源区评审资产，不包含音效，不作为 Unity 可直接播放或切分的正式运行资源。
- 输出：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MotionPreviewsV1/`。
- 可复现脚本：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/build_five_motion_previews_v1.ps1`。
- 当前状态：五条动态样张已于 2026-07-17 获用户全部通过；美术门禁关闭，下一阶段进入结果动态层拆分和 Unity 实现交接。短片只作为节奏与画面基准，不作为运行时视频资源。

## 2026-07-17 主游戏 LED 字体校准 V1

- 本轮不重新探索主体美术风格，只解决批准预览与实际运行字体重量不一致的问题；校准范围覆盖顶部 HUD、左侧积分塔、右侧终端、底部主操作和微提示。
- 对照板必须直接读取运行字形映射并按实际坐标、换行、字距、对齐、缩放与辉光合成，不允许再用独立排版软件或 AI 生成文字模拟 Unity 效果。
- 当前运行方案在 `1280×720` 的最小核心灯珠约为 `0.875 px`，标记为 `FAIL`。A「10×10 粗灯珠」最小核心 `2.20 px`，整体更接近批准参考；B「11×11 紧凑灯珠」最小核心 `2.00 px`，细节更整齐。两者均通过字高与区域装载门禁。
- 输出目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV1/`；生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_led_font_calibration_v1.py`。
- 用户已退回 A / B：两者相对批准预览仍有差距且显示模糊。两套候选均改为“需重做”，不具有运行时加载 key，也不得生成正式图集。

## 2026-07-17 主游戏 LED 字体像素级校准 V2

- 本轮只验证清晰度，不同时改变琥珀色相、区域布局或主游戏状态内容。
- 视觉核心改为开始界面 `DICE KING` 招牌同系的独立方形灯珠：高亮硬核心、明确点距、无平滑边缘；中文使用同一离散灯珠语法，不再把平滑字体整体缩小后伪装成点阵。
- 完整帧、1:1 裁切与整数 `4× NEAREST` 检查图必须分开交付；缩略总览不得承担清晰度验收。
- 无辉光硬核心与受限 `1px` 辉光是两个独立层。只有硬核心先通过后，才判断辉光是否需要保留。
- 输出目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV2/`。用户已确认清晰度没有问题，但字形无法识别；V2 仅保留像素清晰度链路，`7×7–10×10` 中文字形全部退回。

## 2026-07-17 主游戏 LED 字体字形识别压力板 V3

- 本轮只验证“能否读出内容”，不生成完整 Ready，不调整辉光、色相或动态节奏。
- 中文字形从 V2 的 `7×7–10×10` 自动压缩轮廓改为 `16×16` 目标尺寸 hinted mask；评审输出使用 `Noto Sans SC` 中等字重、二值灯珠、无抗锯齿。
- 720p 样张统一使用 `2×2 px` 核心和 `3 px` 点距。旧文字框不再反向决定字形密度；若装不下，后续回排积分塔、终端标题 / 正文和排序微提示。
- 固定压力字符为 `章 / 额 / 规 / 检 / 骰 / 袋 / 调 / 整 / 结 / 算 / 顺 / 序 / 掷`，同时提交七组真实游戏文案的 1:1 盲读板。
- 输出目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRecognitionV3/`。用户已确认字形身份通过，但混排基线失败；正式资源生产继续暂停。

## 2026-07-17 主游戏 LED 字体基线校准 V4

- 用户已通过 V3 字形身份，但指出字符排版不在同一水平线；V3 字形结构冻结，不再重新选字体或重画复杂字。
- V4 只移动完整二值活动字形：中文、英文大写和数字共享逻辑第 `14` 行底线；`- / ·` 等悬浮标点固定在逻辑中线；`， / 。` 使用统一下沿。
- 主板同时保留左侧 V3 顶对齐失败证据和右侧 V4 单一候选，另有 V4-only 1:1 验收条与整数 `2× NEAREST` 检查图；这不是 A / B 方向选择。
- 自动报告要求 V3 活动字形哈希与亮灯数全部守恒、共享底线偏差为 `0 px`、所有位移为整数并装入 `16×16`。
- 输出目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontBaselineV4/`；生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_led_font_baseline_v4.py`；QA：`Docs/QA/20260717_led_font_baseline_v4.md`。
- 用户已确认同线排版通过，并指定 V4 为没有特别说明时的全局默认文字标准；本段“无运行时 key / 不进入 Unity”的旧门禁已由下节正式接入覆盖。

## 2026-07-17 全局 LED 文字运行资源 V4

- 正式资源为 `Assets/Resources/Art/MainGame/Flow/main_game_led_font_atlas.png`、`main_game_led_font_map.csv` 与共享 `main_game_led_font_styles.csv`，当前按代码 / 数据字符集合生成 `850` 字形；字形沿用已通过的 V3 二值结构，基线沿用已通过的 V4 规则。
- 美术侧冻结三条约束：灯珠必须落在整数物理像素、边缘二值且无抗锯齿、默认无辉光。字号变化只能选择 `Display / Compact / Micro` 命名档，不重采样字形。
- 全局动态文字均服从该标准；骰面实体压印数字与 `DICE KING` 实体招牌是唯一已登记例外。新增特殊字效若需偏离，必须先单独提出并验收。
- 双分辨率文字真值板及主菜单 / Ready / 市场上下文对照位于 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRuntimeV4/`；完整验证见 `Docs/QA/20260717_led_font_runtime_v4.md`。

## 2026-07-17 LED 语义亮度 V1 美术样张（分级认可、同级亮度需修订）

- 本轮不改变 V4 字形、灯珠结构、字距、基线和三档几何；只为既有点阵文字建立可复用的硬核心亮度、局部底板和状态色规则。
- 不做“全屏所有字一起变亮”。主信息与可交互文字升亮，说明文字保持清楚但克制，装饰文字继续压暗；让层级来自语义而不是每个页面临时调色。
- 暗底琥珀首选 `#FFD36A / #FFC04A / #D99A4A / #806B4E` 四档；焦点青为 `#78F3E6`，失败锈红只用于局部结果，米色实体键使用 `#180E08` 深墨点阵字。
- 主信息硬核心 Alpha 固定 `1.0`。受限辉光只允许 `L3` 焦点使用独立 `1` 物理像素、Alpha 不高于 `0.18` 的外层；不得模糊、放大整张字图或用辉光补细字。
- 主菜单选中标题、市场骰子名、Tooltip 名称和当前主操作必须从实际背景中明确跳出；商品六面、规则正文和操作说明可次一级，但不能与机台纹理融在一起。
- 禁用态保留同一字形与尺寸，通过低饱和色、面板暗化和短原因共同表达；不能把文字 Alpha 压到近乎消失。
- 首轮只制作基于实际底板和运行 Rect 的单一“修订目标”同源板；现状证据沿用用户截图与既有 QA capture，不把全屏缩放对照板再作为锐度依据。不生成新的背景、按钮或字体位图，不调用图像生成模型，也不进入 `Assets/Resources/Art/`。
- 用户已指示开始制作。当前产物位于 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV1/`，包含三种真实上下文的双分辨率 `core / focus` 图、状态条、`90%` 压力图、角色 CSV、可复现脚本与机器报告。
- 市场商品名保持正式 V4 字形并升为 `Display`；名称 / 六面在原货架信息区内重新分配高度，不移动骰子、购买区或热区。米色实体键使用平整浅铭牌带承载 `#180E08` 深墨字，避免纹理暗斑吞字。
- `led_text_brightness_metrics_v1_20260717.json` 当前记录 `260` 次双分辨率 / 双状态绘制检查、关键 `L2 / L3` 误用 `Micro = 0`、对比度失败 `0`、裁切失败 `0`、正式字形缺失 `0`。
- 用户认可信息分级，但同级色相存在明显亮度差；V1 当前状态为 `需修订、保留对照`。没有调用图像生成模型，没有新增或覆盖运行时美术，没有修改 Unity 代码或配置。

## 2026-07-17 LED 语义亮度 V2 同级等亮样张（待验收）

- 保留 V1 全部分级、尺寸、字形和页面布局；以 `OKLab L` 校准硬核心，颜色只保留焦点 / 状态色相职责。
- `L3` 四种色相共用目标 `0.884625`；`L1` 两种色相共用目标 `0.732400`。RGB 量化容差为 `0.0002`，实际最大同级跨度为 `0.000067127`。
- 重点修订：焦点青 `#78F1E4`；L1 暖白 `#BDA57A`；失败 `#FCCDBC`；成功 `#91EDDF`。琥珀锚点与其它单角色等级不变。
- 新增统一暗底 `same_grade` 比较条，先直接比较 L3 四色和 L1 两色，再复核主菜单、市场与 Tooltip 的真实底板。核心 / 焦点仍分图，双分辨率和 `90%` 层级压力图继续保留。
- `led_text_brightness_metrics_v2_20260717.json` 记录 `320` 次绘制检查、角色覆盖 `10/10`、关键 `Micro = 0`、对比度失败 `0`、裁切失败 `0`、同级等亮通过。
- 当前状态为 `样张待验收`；产物只位于 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV2/`，没有调用图像生成模型或 Computer Use，也没有修改运行资源、C#、玩法、数据或存档。

## 2026-07-17 骰子类型芯方向 A 与首批生产范围

- 用户已通过 12 枚压力测试图，确认“家族壳表达家族、中央机械类型芯表达具体类型、结果数字接管中心后类型芯缩入底槽”的方向。
- 首批范围为基础骰、猪猪 7、恶魔 5、专用贡品 1、龟龟 7、海盗 8、中立 8，共 `37` 枚；双家族 `9` 枚另立后续批次，不在本轮混做。
- 类型芯是骰面内部的粗轮廓机械插芯，不是完整透视骰子、角色、外置道具或微缩场景；静态剪影必须在约 `80×76` 像素成立。
- 低频动作只强化身份记忆，常态间隔 `3.5–5.5s`、同屏最多 `2` 枚；输入、拖拽、投骰、停转、结算和结果阶段暂停。结果小芯完全复用静态图并停止活动。
- 压力图：`Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/arcade_dice_type_core_pressure_contact_v1_20260717.png`。
- 完整生产规格：`Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/TYPE_CORE_PRODUCTION_SPEC.md`。
- 五张分组生产板已生成并通过内部家族内区分检查，路径与顺序见生产规格；下一步生产 37 张透明静态图和对应活动遮罩，并在实际 `80×76` 内容框通过后进入运行时登记。

## 2026-07-17 市场浅色实体键帽实心字 V1（退回）

- 本轮只重绘用户标注的卖出、三枚购买和离开市场键帽文字；市场底板、键帽外框、商品、商品名、六面、刷新键、反馈和 Tooltip 均复用现状。
- 推荐字形为 `Noto Sans SC 600` 实心字。启用态所有键完全共用 `#180E08`，字重与字色不随悬停或按下改变；悬停只亮边缘，按下整体下移，禁用态改灰褐但保持实心清楚。
- `1280×720` 购买 / 卖出使用 `22 px`，离开市场使用 `24 px`；`1920×1080` 分别以 `33 / 36 px` 原生绘制，不对低分辨率文字做放大。
- 输出目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketPhysicalKeyLabelsV1/`。启用态对比度 `10.0385:1`，禁用态 `5.0193:1`；双分辨率与最长文案压力均装入。
- 当前只作为失败对照；新增纯色承载层已被用户退回，不得进入运行时。

## 2026-07-17 市场浅色实体键帽实心字 V2（已通过并接入）

- V1 退回原因：为覆盖旧点阵字新增的平色内衬遮住原键帽磨损和纹理，与既有画面材质不连续。
- V2 从市场无字底图恢复卖出、三枚购买和离开市场的完整原始键帽区域；不新增纯色矩形、铭牌、磨皮层或统一亮度层。
- 字形、字重、字号和 `#180E08` 字色保持 V1 不变，只修复承载材质。悬停只亮原边缘；按下只压暗原材质并同步移动文字；禁用仍保留纹理。
- 输出目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketPhysicalKeyLabelsV2/`。双分辨率纹理保留、字高、装入与压力文案机器检查通过。
- 原纹理的最暗 `5%` 区域对比为 `3.69:1–4.82:1`，中位背景为 `5.00:1–5.93:1`；不再为了追求平色 `7:1` 牺牲材质一致性。用户已确认 V2 消除贴标签感并授权接入。
- 批准的 600 字重已改名并裁成静态仓库子集 `Wabish Physical Key Sans SC SemiBold`，运行资源为 `Assets/Resources/Art/MainGame/Flow/wabish_physical_key_sans_sc_semibold.ttf`；许可证随包进入 `Assets/Resources/Licenses/`。
- Unity 自动六图与运行像素报告均通过：启用字精确共用 `#180E08`，禁用字精确共用 `#30261D`，三枚购买键中心线偏差为 `0 px`，五处常态 / 禁用表面均保持明显纹理方差。

## 2026-07-17 LED 语义亮度 V2 运行落地

- V4 字形、16×16 活动掩码、统一基线和三档整数灯珠几何不变；本轮只把已认可的层级与同级等亮合同落到真实页面。
- 正式核心色为 L3 琥珀 `#FFD36A` / 青 `#78F1E4`，L2 `#FFC04A`，L1 琥珀 `#D99A4A` / 暖白 `#BDA57A`，L0 `#806B4E`，Disabled `#8C8068`；所有核心 Alpha 为 `1.0`。
- 主信息不通过额外辉光、描边或全屏提亮实现；同级等亮依靠核心色本身成立。页面材质、机台锈蚀和暗底不做平整化处理。
- 关键信息扩大到正式 Display / Compact 安全框；商品名保持在原名牌区，不遮挡骰子；浅色实体键帽维持批准的原纹理实心字。
- 自动原生双分辨率和 `0.606×` 压力图均通过内部检查；最终视觉状态仍以用户本轮确认结果为准。

## 2026-07-22 F021-S5-UI1 状态样图批次

### 视觉意图

在既有“午夜旧机厅·后台补给柜”和 `PhysicalKeyLabel V2` 原材质上统一动作按钮状态，使玩家无需阅读原因文案就能区分可执行与受阻，并能通过按钮锚定的短时提示理解操作结果。

### 源区资源清单

| 资源 | 用途 | 尺寸 / 格式 | 目标路径 | 来源决策 | 状态 |
|---|---|---|---|---|---|
| 按钮五状态接触图 | 比较可用、悬停、按下、受阻、受阻点击 | 横版 PNG | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketButtonActionSystemV1/market_button_five_state_contact_v1_20260722.png` | D13 / D17 | 样张待验收 |
| 提示动效关键帧图 | 展示点击、飞入、停留、淡出四节点，并覆盖成功 / 受阻语义 | 横版 PNG | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketButtonActionSystemV1/market_action_tip_motion_storyboard_v1_20260722.png` | D14 / D15 / D17 | 样张待验收 |
| 满袋混合资格场景图 | 验证普通商品受阻而真实特殊资格仍可用 | `16:9` PNG | `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketButtonActionSystemV1/market_mixed_eligibility_context_v1_20260722.png` | D16 / D17 | 样张待验收 |

### 必须保留

- 既有市场机位、深海军蓝磨损机壳、烟熏玻璃、琥珀 LED、克制青色与浅米色原纹理键帽。
- 浅色键帽继续使用深墨实心字，不新增纯色承载层，不改成 LED 点阵字。
- 可用态保持凸起和材质能量；受阻态整键压暗、降饱和、取消暖边并轻微内陷。
- 提示使用局部烟熏玻璃浮层，锚定触发键，不变成顶部横幅或常驻状态条。

### 避免

- 不在按钮上显示价格、差额、满袋原因或成功结果。
- 不为不同受阻原因制作不同的禁用键外观。
- 不使用全屏扫光、多次弹跳、旋转、强震动、赌场追灯或现代扁平系统通知。
- 生成样图中的商品名与数字不作为文字 / 数据验收；正式文案仍由 Unity 确定性绘制。

### 门禁

三张样图全部只进入源区，用户通过前不登记 `ART_ASSETS.md`、不创建 `Resources.Load` key、不修改正式市场底板或运行代码。

### 2026-07-22 内部复核

- `PASS`：按钮五状态接触图固定使用“购买”，可用、悬停、按下、受阻和受阻点击具有同构结构与可辨差异。
- `PASS`：Tip 分镜同时覆盖“骰袋已满”和“已卖出”，飞入起点与触发按钮关联，停留和上浮淡出节点可区分。
- `PASS`：满袋混合资格图中普通商品受阻，血契船长仍按真实资格保持可用。
- `PASS`：三张图只进入 `Assets/ArtSource/`，未进入正式资源或代码。
- `WARN`：生成编辑重构了部分非目标商品名与数字；本批不用于文字、价格或数据验收。
- 当前门禁仍为 `样张待用户验收`，不得因内部 `PASS` 自动进入运行时。
