# F021 程序交接

状态：F021-S1 已实现并人工回归；F021-S2-R3 待完整回归；五条结果动态已通过；LED V4 字形标准已完成正式接入；市场浅色实体键帽 V2 已接入；F021-S5-UI1 已静态接入并通过独立编译，待 Play Mode 回归
功能：F021 全流程节奏样片
关联主文档：PROJECT_CONTEXT.md、GAME_FLOW.md、ART_ASSETS.md
实现事实来源：Assets/Scripts/DiceKingDemo.cs

## 2026-07-14 F021-S1 开始界面正式实现覆盖

- 用户确认吊灯专用样片“还可以”，并要求直接在 Unity 实现、保留可调参数、用本次新主界面替换旧开始界面。
- 本次实现范围只包含 `MainMenu`：四层街机底图、吊灯状态机、菜单鼠标 / 键盘热区、有存档动态覆盖、设置页屏闪开关和资源缺失回退。
- 旧“本阶段不改代码、不把样片放入 `Assets/Resources/Art/`”约束只对尚未冻结的其它 F021 切片继续有效，不再阻塞 `F021-S1`。
- 运行时资源加载 key：`Art/MainMenu/arcade_main_menu_lamp_on`、`Art/MainMenu/arcade_main_menu_lamp_off`、`Art/MainMenu/arcade_main_menu_lamp_glow`、`Art/MainMenu/arcade_main_menu_lamp_core`。
- 参数表：`Assets/Resources/Data/main_menu_visual_config.csv`；支持 `F7` 在运行中重新加载。`loop_enabled` 独立控制循环，默认开启；默认完整周期约 `0.98–1.58s`，并保留首次延迟、随机间隔、断电与三段复亮时长等参数。设置项 `MenuLampFlickerEnabled` 只控制表现，不提高运行存档版本。
- 灯光事件使用独立 `System.Random`，不会消耗或改变玩法投骰随机；确认覆盖、设置页、输入保护和离开主菜单时会复位为常亮。

## 2026-07-15 F021-S1 右窗随机雨水实现

- 用户接受窗外雨幕 V3 的向下方向与远近层次；玻璃 V3 的材质和节奏作为条件基准，但固定四滴 / 固定路径被否定。用户随后确认跳过固定 V4 视频，直接实现随机多点水珠。
- 新增 Inspector 参数类与资产：`Assets/Scripts/MainMenuRainProfile.cs`、`Assets/Resources/Config/main_menu_rain_profile.asset`，加载 key 为 `Config/main_menu_rain_profile`。雨水参数不写入 `main_menu_visual_config.csv`。
- 新增运行时干净窗体：`Assets/Resources/Art/MainMenu/arcade_main_menu_window_clean_patch.png`，加载 key 为 `Art/MainMenu/arcade_main_menu_window_clean_patch`；它覆盖旧四层全屏图中的静态雨痕与窗框白点。资源缺失时不追加程序雨水，避免在旧雨痕上重复叠加。
- 窗外雨由远 / 近两个固定容量池组成，屏幕坐标 Y 持续递增；密度、总速度、两层数量 / 速度 / 长度 / 宽度 / 透明度和横风均可在 Inspector 修改。雨线在内缩窗面多边形内分段裁切。
- 玻璃层默认预热 `8–14` 颗水珠并分布在 `2–3` 个随机松散区域，按撞击频率持续补充；R1 后水珠不再远距离互相追踪，只在可见边缘真实接触时合并，普通水滴受重力缓慢向下，质量越大下滑越快，达到质量阈值后才转为主流。同屏默认最多两条主流，支持低概率支流、停滞、接触吸收沿途下游水珠、延长、淡出和继续循环。
- 雨水只在 `MainMenu` 以 `Time.unscaledDeltaTime` 更新，使用独立 `System.Random`；`LockPreviewSeed` 默认开启，`F8` 可从当前 Inspector 资产重启同一验收序列。离开主菜单停止更新，不改变输入、存档版本、玩法状态或投骰随机。

## 2026-07-15 人工验收 R1 修正

- 雨线越过右窗的根因是 `GUIUtility.RotateAroundPivot` 在已有 `fitMatrix * prototypeMatrix` 上按错误枢轴空间叠加，圆形水珠未旋转所以仍留在窗区。`DrawMainMenuLine` 现改为 `oldMatrix * localLineMatrix`，线段起点与旋转都在原型局部坐标完成。
- 旧玻璃聚合会让小滴在 `MergeRadius` 内使用 `MoveTowards` 追踪大滴，因此可能侧移或向上。R1 删除该追踪，只保留向下重力、接触合并和水流下游接触捕获。
- `MergeRadius` 现表示可见边缘的额外接触容差，不再表示吸附半径；`MergeSpeed` 现表示合并变重后的向下滑速上限。Inspector 新增 `ContactImpactChance`、`SurfaceHoldRange` 与 `SurfaceCreepSpeedRange`。
- 独立 Roslyn 和当前 Unity Editor 均已重新编译，0 个新增错误；动态画面仍需按 `PLAYTEST_FEEDBACK.md` 的 `F021-RG-001` 至 `F021-RG-006` 人工回归。

## 2026-07-16 F021-S2 主游戏共用运行时包

- 新增一张 `1920x1080` 共用无文字底板和中立、猪、恶魔、龟、海盗五张 `512x512` RGBA 家族壳；源图与可复现构建脚本位于 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameRuntimeV1/`。
- `DiceKingDemo.cs` 在 `GameMode.Run` 且共用底板加载成功时启用新布局，覆盖 `Ready / Shaking / Stopping / ResultDecision / Scoring`；底板缺失时完整回退旧 Run UI，单个家族壳缺失时回退中立壳。
- 顶部显示章节 / 小关、`生命 --` 和金币；生命占位是有意边界，因为生命池、存档迁移和失败重试尚未实现，不能写死评审图示例值。
- 单家族使用对应壳；双家族通过两张现有壳左右拼接。待机中心使用现有具体类型图标，缺图标时用短类型名；结果中心只显示当前有效数字，字号同时按高度和字符串宽度收缩，支持三 / 四位数字。
- 既有拖拽排序、悬浮详情、投骰随机、结果锁定、只读结算事件队列和市场 / 存档规则未改。结算浅金色旧条已换为暗色 CRT 信息条、琥珀描边和青色细线。
- 自动验收器 `Assets/Editor/WabishMainGameRuntimeCapture.cs` 可在已打开的 Unity 中自动进入 Play Mode、设置固定 Game View 尺寸、生成八张截图并读取 PNG 实际尺寸复核；完整结果见 `Docs/QA/20260716_main_game_runtime_validation.md`。

## 2026-07-16 F021-S2-R1 六槽二维对齐

- 用户人工检查 V1 `Ready` 截图后指出骰子与底图槽位未重合。纵向偏差最明显；按底图像素复核后，V1 六骰中心 `X=278+130n / Y=346` 相对凹槽 `X=299+128n / Y=369` 还包含轻微左偏和节距差。
- 实现把共用骰台区域改为 `new Rect(204f, 301f, 830f, 220f)`，并把街机单行间距由 `18f` 改为 `16f`。六个 `112` 像素骰子的运行时中心现为 `X=299+128n / Y=369`。
- 所有状态仍由 `DrawTableDice` 生成同一组 `slotRects`，所以拖拽命中、幽灵、插入标记、投骰、结果数字和结算高亮会一起移动，不产生状态分叉。
- 本修复不改运行资源、骰子尺寸、数据、存档、随机或结算顺序；`git diff --check -- Assets/Scripts/DiceKingDemo.cs` 已通过。
- 自动回归请求已排队；Unity 编辑器尚未响应外部 Refresh，因此 `F021-RG-007` 至 `F021-RG-009` 保持待回归。

## 2026-07-16 F021-S2-R2 槽位尺寸与骰壳有效像素适配

- 用户检查 R1 局部截图后指出：位置已经靠拢，但底图空槽没有形成明确尺寸契约，骰子在槽内仍明显偏小。
- 源因不是 `DrawTableDice` 的 `112×112` 逻辑框，而是五张 `512×512` 家族壳带有公共透明边距；Alpha 有效区宽 `431–447`、高 `456–458`，旧 `ScaleToFit` 后实际可见壳只有约 `94–98 × 100`。
- 新增运行常量：有效槽 `112×112`、净距 `16`、顶部内边距 `12`；六槽中心继续为 `X=299+128n / Y=369`，最大 `128×128` 高亮不侵入相邻槽。
- 单家族与双家族统一裁切源图公共 UV `(32,27,448,458)` 再映射到逻辑槽；预计五类可见壳提升到宽 `107.8–111.8`、高 `111.5–112`。
- 类型芯 / 结果数字框同步换算到裁切后局部坐标 `18.6%, 15.3%, 62.9%, 59.2%`，避免外壳放大但中心内容仍沿用旧整图位置。
- `WabishMainGameRuntimeCapture` 的 Game View 尺寸查找只接受 `FixedResolution`。旧实现可能误选同为 `1280×720` 的 `16:9` 比例项，导致实际 `Screen` 为 `1532×793` 并中断回归。
- 初次兼容修正把循环内布尔值和方法外层枚举对象都命名为 `fixedResolution`，触发 `CS0136`。现已分别改为 `isFixedResolution` 和 `fixedResolutionKind`；Unity 重新生成 `Assembly-CSharp-Editor.dll` 后自动完成八图，尺寸选择行为不变。
- 不修改 PNG、底图、家族归属、骰子容量、玩法、数据、存档、随机或结算顺序。

## 2026-07-16 F021-S2-R3 底图边框贴合与验收器稳定化

- 用户人工截图确认 R2 仍未贴合底图边框，因此 `112×112 / 净距 16 / Y=369` 不再是当前运行契约。`DiceKingDemo.cs` 现使用六个 `128×128` 物理槽、中心 `X=299+128n / Y=368`、横向净距 `0`；少于六枚时仍从第一个物理槽左对齐，不重新居中。
- 结算缩放幅度在街机单行中降为旧增量的 `37.5%`；结算高亮内缩 `1px`、Ready 选中框内缩 `2px`，拖拽幽灵与插入标记复用同一 `slotRects`，避免跨入相邻槽或再次偏离底图。
- `WabishMainGameRuntimeCapture.cs` 不再在首个尺寸失配帧抛错：目标尺寸需连续稳定两帧才截图；失配时每 `0.5s` 重新选择固定分辨率，`10s` 后才以含当前尺寸的错误退出。
- 验收请求现由编辑器常驻监听 `Temp/WabishMainGameRuntimeCapture.request`，不再要求为了消费请求而额外修改源码。分辨率重试主体已由当前 Unity 重新编译；常驻监听补丁通过独立 Roslyn 语法编译，完整八图仍待编辑器刷新后重跑。

## 输入决策

- F021 原始阶段只生产有美术表现的粗样片；用户已分别批准 `F021-S1` 开始界面、`F021-S2` 主游戏共用底座，以及 2026-07-17 通过的五条主游戏动态进入 Unity 实现交接。关间市场换肤与其它未批准切片仍不直接实现。
- 后续正式实现必须遵守现有主流程状态、F009 固定槽位序列帧投骰、F006 / F007 左到右结算展示和只读结算提交态。
- 样片素材先放在 `Assets/ArtSource/Production/F021/`，不作为运行时资源加载。

## 实现目标

`F021-S1` 已把开始界面拆成 Unity 可加载层并以可调状态机替换旧首屏；`F021-S2` 已把共用主游戏竞技台和模块化家族壳接入现有 Run 状态机。两者都不改变玩法数据、运行存档版本或随机。五条结果与过程动态现已通过美术门禁，成为下一阶段表现层实现范围；生命池玩法数据仍需独立确认，关间市场继续后置。

## 已确认行为

- 结果锁定后不按点数重排，继续按当前槽位左到右结算。
- 越线达标后继续播放剩余结算，最终分明显超过目标分后才定格。
- 金币效果在最终分定格后作为奖励兑现表现触发，不绑定新增规则、指定骰子或数据表。
- 轻结算面板显示过关、最终分、目标分、金币来源和当前金币。
- 市场首版只演示购买一个骰子并离开市场。

## 实现任务

| 任务 | 文件 / 数据 | 依赖 | 验收 | 状态 |
|---|---|---|---|---|
| 替换开始界面并接入吊灯断电状态机 | `Assets/Scripts/DiceKingDemo.cs`、`Assets/Resources/Art/MainMenu/`、`main_menu_visual_config.csv` | 吊灯专用样片已接受 | 新首屏可操作，灯光参数可调，资源缺失可回退 | 已实现待 Play Mode 验收 |
| 接入右窗双层雨幕与随机玻璃汇流 | `Assets/Scripts/DiceKingDemo.cs`、`MainMenuRainProfile.cs`、`main_menu_rain_profile.asset`、右窗干净补片 | 窗外雨 V3 已接受；玻璃 V3 为条件语言基准 | 参数在 Inspector 可见；雨线只在右窗；水珠只向下并接触合并；最多两条主流 | R1 已实现待 30 秒 Play Mode 回归 |
| 样片冻结后检查现有状态机承接点 | `Assets/Scripts/DiceKingDemo.cs` | F021 样片验收 | 能对应 Ready、投骰、结果锁定、逐骰结算与三种结果表现；市场仅保留既有入口 | 交接检查完成 |
| 拆分主游戏共用运行时资源加载边界 | `ART_ASSETS.md`、`Assets/Resources/Art/MainGame/` | 用户批准 F021-S2 共用底座 | 六个运行时资源有路径、加载 key 与缺失回退 | 已完成并验证 |
| 接入三种结果演出状态参数 | 建议新增表现 `ScriptableObject` 与 `Assets/Resources/Art/MainGame/Flow/` | 五条动态已通过 | 通关三拍收入、失败扫描复位、生命归零分层断电均可调且不改计分 | 待开始 |
| 接入市场购买和离开转场表现 | `Assets/Scripts/DiceKingDemo.cs` | 样片定稿 | 购买、离开和下一关 Ready 回环可录屏验证 | 后置 |

## 2026-07-17 五条动态通过后的 Unity 接入边界

- 不直接播放五个评审 MP4。按运行时职责拆出共用底层、LED 点阵文本、六槽状态、积分塔 / 目标标记、右侧终端、局部色层、扫描线和 CRT 断电遮罩，并让动态数值继续由 Unity 绘制。
- 第一段接入沿用现有 `Ready → Shaking → Stopping → ResultDecision → Scoring` 状态机，复现槽内点火 / 旋转、左到右停转、局部结算焦点和目标越线后继续结算。
- 第二段增加 `StageClear / StageFailed / RunOver` 表现承接：通关固定 / 利息 / 复利三拍；失败保留最终结果并以单条扫描复位；生命归零按模块断电后收成 CRT 水平线。
- 动效参数使用表现专用 `ScriptableObject`，至少覆盖投骰点火、逐槽停转间隔、结算焦点、越线脉冲、收入分拍、扫描速度、模块断电间隔和 CRT 收束；不新增玩法 CSV。
- 关间市场继续排除，`StageClear` 只交给既有 `Space 进入市场` 去向。首发初始生命数与恢复来源确认前，只实现失败 / 归零的表现接口和可视壳，不落地扣命、重试快照或存档迁移。

## 代码影响

当前已影响 `MainMenu` 绘制、输入与设置保存，以及 `Run` 的共用视觉绘制；不改变 `Opening / Run / InterStageMarket / ChapterShop` 的玩法状态推进、结算或数据行为。其它后续可能影响：

- 主菜单开始新游戏转场。
- 主流程投骰和停转演出参数。
- 结算展示事件队列。
- 胜利后的轻结算面板。
- 市场进入、购买反馈和离开转场。

## 数据影响

`Assets/Resources/Data/main_menu_visual_config.csv` 只配置开始界面吊灯表现；`Assets/Resources/Config/main_menu_rain_profile.asset` 只配置右窗雨水表现，并通过 Unity Inspector 持久化。主游戏共用包只新增美术资源和绘制分支，不新增 CSV、ScriptableObject 或玩法数据字段；QA 的 `123 / 1234` 只在验收脚本中临时写入并在真实结算前恢复。

## 存档影响

不改变运行存档版本与 `DiceData`。设置项新增 `DiceKingDemo.MenuLampFlickerEnabled`，只记录玩家是否允许开始界面吊灯屏闪。

## 界面影响

当前正式影响主菜单、设置页，以及主流程 `Ready / Shaking / Stopping / ResultDecision / Scoring` 的视觉层；胜利 / 失败专属动态和市场仍只受已确认规划影响。

## 测试 / 验证计划

后续正式实现时至少需要：

- Play Mode 录屏覆盖主菜单到下一关 Ready 的完整闭环。
- `1920x1080` 截图验收。
- `1280x720` 截图验收。
- 验证表现层不改变随机、分数、金币、市场购买、存档。
- 验证动画关闭或资源缺失时仍能回退到可玩路径。
- 右窗专项录制至少 30 秒，验证向下雨幕、远近层次、`8–14` 颗随机水珠、无横向 / 向上磁吸、接触合并、阈值触流、最多两条主流、固定种子复现和窗框 / 机台边界。

F021-S2 共用主游戏包 V1 已完成基础 Unity、资源回退和双分辨率技术验收。用户随后依次指出二维对齐、槽位尺寸和 R2 边框贴合问题；当前 R3 以底图实际 `128` 节拍冻结候选物理槽，并修复了验收器把 `875×30` 临时布局尺寸当成最终分辨率的问题。九状态接触图 V2 与五条动态样片现已通过，结果分支表现进入下一次 Unity 实现范围；关间市场仍不在本轮范围。R3 八图重跑、边框贴合与拖拽人工检查仍需在完整回归中覆盖。

## 文档同步

| 文档 | 是否需要更新 | 已确认来源 |
|---|---|---|
| PROJECT_CONTEXT.md | 已同步 F021-S2 与 LED V4 全局标准 | 用户批准、运行接入与离线验证 |
| GAME_FLOW.md | 已同步 F021-S2 与 LED V4 全局范围 | 用户批准、流程文字合同 |
| ART_ASSETS.md | 已登记主游戏资源与 LED V4 atlas / map | F021 正式资源切片 |
| DICE_ARCHETYPES.md | 当前不需要 | 未改变骰子规则 |

## 阻塞项

`F021-S1` 当前无代码实现阻塞并已获用户人工回归。`F021-S2-R3` 代码与验收器修复已完成静态 / 编译检查，但当前 Unity 失焦未消费新请求，因此 `F021-20260716-FB-004` 与 `F021-20260716-FB-005` 保持“待完整回归”，不能提前标记通过。LED V4 运行接入已完成，不再阻塞代码；只保留最新 Unity Play Mode 画面、鼠标热区与 Tooltip 交互回归。五条动态的美术门禁已经解除；首发初始生命数和恢复来源仍阻塞可重试失败 / 生命归零的最终逻辑、重试快照与存档迁移。市场换肤继续不在本轮范围。

## 2026-07-17 LED 字体校准 V1 历史接入边界

- 本轮只新增离线生成器、候选源字库、双分辨率对照板和 JSON 报告；没有修改 `Assets/Resources/Art/MainGame/Flow`、`MainGameLedFont.cs` 或 `DiceKingDemo.cs`，因此当前游戏画面尚未换字。
- V1 原计划在用户选择 A / B 后按独立字体切片接入；该计划已因用户退回而失效，A / B 均不得生成正式 atlas / map。
- 正式门禁以 `1280×720` 为下限：最小核心灯珠不得小于 `2 px`，主信息有效字高不得小于 `22 px`，正文不得小于 `18 px`，代表区域不得越界或缺字；`1920×1080` 同时生成结果，不能只看高分辨率。
- 每轮只调整一种变量族：先点阵密度 / 粗细，再字号 / 字距 / 行距，最后颜色 / 辉光。禁止用增加辉光掩盖灯珠核心不足，也禁止继续把高密度字图任意缩到非整数尺寸。
- 离线同源板通过后，复用 `Assets/Editor/WabishMainGameRuntimeCapture.cs` 自动注入状态并生成双分辨率 PNG 与状态报告，只做一次最终 Unity 冒烟。Computer Use 和人工进编辑器截图不属于日常字体迭代链路。
- 当前结论：V1 A / B 均需重做，不再推荐 A。正式运行字库继续保持不变。

## 2026-07-17 LED 字体像素级校准 V2 接入边界

- V2 只用于清晰度验收，不是新的正式字库实现；`LedFontCalibrationV2` 没有运行时加载 key。
- 正式实现不能继续使用“整张低分辨率字图缩放到任意 `fontSize`”作为基础。目标实现需按命名样式直接确定物理灯珠核心、点距和网格，并让每颗灯珠落在整数物理像素。
- 无辉光硬核心必须先通过；辉光由独立可关闭层实现，默认上限为 `1px / Alpha 34`，不能通过扩大整张字图制造辉光。
- 运行代码仍需收口为 `HUD 主信息 / HUD 次信息 / 积分塔正文 / 终端标题 / 终端正文 / 主操作键 / 微提示` 命名样式；每个样式分别定义 720p 与 1080p 物理合同，不再传入任意字号。
- 用户已确认 V2 不再模糊，但字形无法识别；因此清晰度门已关闭，字形门仍阻塞。V2 不得生成正式 atlas / map，不修改 `MainGameLedFont.cs`、`DiceKingDemo.cs` 或运行资源。

## 2026-07-17 LED 字体字形识别 V3 接入边界

- V3 只用于字形盲读，不是完整 UI 候选；`LedFontRecognitionV3` 没有运行时加载 key。
- 正式字形不得再把复杂中文自动压到 `7×7–10×10`。当前 V3 以 `16×16` hinted mask、720p `2×2 px` 核心 / `3 px` 点距作为识别候选。
- 字形识别优先于旧文字框装入。当前回算明确要求调整积分塔目标 / 当前、终端标题 / 正文和排序微提示；顶部三窗与主操作键可保留。
- 用户已通过 V3 盲读，但指出混排基线不齐；因此不能直接进入文字框回排。
- 最终仍只运行一次无 Computer Use 的自动 Unity 冒烟：脚本注入确定状态并输出双分辨率 PNG 与状态报告；日常字形迭代继续留在离线同源链路。

## 2026-07-17 LED 字体基线 V4 接入边界

- `LedFontBaselineV4` 仍是源区校准产物，没有运行时加载 key；本轮不得修改 `MainGameLedFont.cs`、`DiceKingDemo.cs`、正式 atlas / map 或命名样式。
- 后续正式渲染不能沿用 V3 的“按活动边界减去 `min_y` 后逐字顶对齐”。每个 glyph 记录固定纵向锚点；中文、英文大写和数字必须共享底线，悬浮标点使用显式中线锚点。
- 正式字库生成时必须保留 V3 活动字形哈希 / 亮灯数，并增加底线偏差为 `0` 的自动检查，不能仅检查缺字与 Rect 装入。
- 用户通过 V4 后才由界面体验制作一次积分塔、终端和微提示文字框回排图；回排通过后才允许生成正式 atlas / map 和双分辨率命名样式合同。

## 2026-07-17 LED V4 正式全局接入

- 用户已通过 V4 并指定其为默认全局文字标准；本节覆盖上方“V4 仍为源区产物 / 不进入 Unity”的历史门禁。
- 正式加载 key 为 `Art/MainGame/Flow/main_game_led_font_atlas`、`Art/MainGame/Flow/main_game_led_font_map` 与 `Art/MainGame/Flow/main_game_led_font_styles`。map 当前按代码 / 数据字符集合生成 `850` 字形，保存每字 `16` 行二值位图、活动宽度、锚点与上下边界；字符缺失为 `0`。三档几何由共享样式 CSV 同时约束 Python 真值与 C# 运行时。
- `MainGameLedFont.cs` 从 map 重建灯珠并以当前 GUI 矩阵换算物理屏幕比例，缓存 1:1 文本纹理；所有灯珠坐标与尺寸使用整数取整，纹理强制 `Point`，无抗锯齿、无运行辉光、无任意连续字号缩放。
- 命名样式只允许 `Display / Compact / Micro`。调用方传入的旧语义字号仅用于优先选择档位，并按真实 Rect 逐档装入，不能生成第四套临时几何。
- `DiceKingDemo.cs` 的 `87` 个动态标签 / 开关文案入口已经统一路由至 `DrawStandardLabel`；主菜单、设置、主流程、新旧市场、Tooltip、结果页与旧回退 UI 均覆盖。原始 `GUI.Label` 只保留骰面实体数字阴影 / 本体和字库加载失败安全回退三处白名单。
- 主菜单旧底图中的平滑菜单字由不透明动态层覆盖后再绘 V4；`DICE KING` 实体招牌与骰面实体数字是唯一批准例外。
- 生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_main_game_led_font_atlas.py`。验证：`Docs/QA/20260717_led_font_runtime_v4.md`。
- 已通过原生 `1280×720 / 1920×1080` 离线真值、atlas / map 完整性、全入口静态审计和 Unity 2019.4.33f1 随附 Roslyn 编译（`0 error`，保留已有 `7` 条 `CS0162` 警告）。没有使用 Computer Use；当前仍需在 Unity 完成最新资源导入后的 Play Mode 画面与交互回归。

## 2026-07-17 LED 语义亮度 V2 程序交接（样张通过后执行）

### 当前根因

- `MainGameLedFont` 目前只定义 `Display / Compact / Micro` 几何，不知道文字是主操作、商品名、说明还是禁用项。
- 几何由旧 `requestedSize` 和 Rect 装入隐式选择；例如市场骰子名称传入 `17`，会优先落到 `Compact`，在 `1280×720` 使用 `1px` 核心，信息重要度没有参与选择。
- 调用点仍散落传入颜色和 Alpha；市场 `dimAmber`、禁用按钮以及主菜单次文字存在多套独立透明度，没有统一可读下限。
- `MainGameLedFont.Draw / DrawWrapped` 的辉光参数当前未参与绘制；因此不得继续由调用方假设“传了 glow 就会变亮”。
- 离线 V4 生成器与 C# 共享字形 / 几何，但主菜单和市场颜色仍在 Python 与 C# 各自硬编码，无法保证预览与运行一致。

### 推荐实现

1. 新增共享 `main_game_led_text_roles.csv` 与同级亮度字段，至少包含 `role / brightness_grade / target_oklab_l / tolerance / preferred_geometry / allow_compact / allow_micro / core_rgba / halo_rgba / halo_px / min_contrast / min_alpha / background_mode`。
2. 在 `MainGameLedFont` 增加 `TextRole` 与显式 `TextStyle` 入口；角色先决定语义样式，Rect 只允许在合同范围内降档。`FocusAction / PrimaryInfo` 禁止降到 `Micro`。
3. 保留现有 overload 作为迁移期适配，但所有正式调用必须映射到命名角色；自动审计拒绝新增任意颜色的正式文字调用。
4. 把焦点辉光实现为可关闭的第二张硬边文本纹理或逐点外圈层，只对 `L3` 生效；先画通过验收的硬核心，再画最多 `1` 物理像素的外层。不得使用 bilinear / blur。
5. 米色实体键不进入 V4 LED 字形渲染；复用已批准的 `PhysicalKeyLabel` 实心字入口与深墨核心。禁用键切换为独立灰褐实心字和材质状态，不能只乘低 Alpha。
6. 离线生成器直接读取同一 CSV，输出主菜单、市场、Tooltip 和全局真值；报告写入角色配置哈希，C# 启动或 QA 同步校验。

### 已完成的离线输入

- V1 角色合同只保留为同级不一致证据，不得作为运行输入。
- V2 候选角色合同：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV2/main_game_led_text_roles_v2.csv`；同级目标合同：同目录 `main_game_led_text_brightness_groups_v2.csv`；共 `10` 个角色 / 状态入口。
- 可复现生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV2/build_led_text_brightness_v2.py`。
- 机器报告：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV2/led_text_brightness_metrics_v2_20260717.json`；同级最大目标差 `0.000045263`、最大跨度 `0.000067127`。
- 离线门当前机器项均通过，但 `runtime_sync.status` 明确为 `not-applicable-until-sample-approved`；不得在用户验收前把源区 CSV 复制进 `Resources` 或改 C#。

### 页面迁移顺序

1. 主菜单：选中 / 未选中 / 禁用 / 页脚 / 实体 `SPACE`。
2. 市场：顶部资源、三格商品名与面组、买入 / 卖出 / 刷新 / 离开、中央反馈。
3. Tooltip：名称、家族 / 触发、六面、规则、价格与状态行。
4. 主游戏、设置、开场、结果页和旧回退 UI 全量角色审计。

### 验收与回退

- 先离线生成双分辨率 1:1 硬核心图、焦点辉光对照和 `90%` 层级压力图；未通过前不改 Unity。
- 关键角色局部对比度：`L3 / L2 >= 7:1`、`L1 >= 4.5:1`、`Disabled >= 3:1`、`InkOnLight >= 7:1`。
- 自动检查所有主操作 / 商品名 / 关键数值没有误用 `Micro`，主信息 Alpha 不低于合同值，并检查每个角色实际 `OKLab L` 与等级目标的偏差不超过 `0.0002`。
- 离线通过后再执行一次自动 `1280×720 / 1920×1080` Play Mode 截帧和真实鼠标 / 键盘交互冒烟；日常调光不使用 Computer Use。
- 角色配置或新渲染层加载失败时，回退到 V4 现有硬核心绘制，不能回退到系统平滑字体。

本节当前已有可执行离线输入，但不代表运行实现已获授权；没有修改 `MainGameLedFont.cs`、`DiceKingDemo.cs`、正式资源、玩法、数据或存档。

## 2026-07-17 市场 PhysicalKeyLabel V2 Unity 接入

- 授权来源：用户通过 V2 原材质样张并回复可以继续；该授权只覆盖五处浅色实体键帽文字，不覆盖语义亮度 V2 或其它市场改造。
- 运行资源：`Resources/Art/MainGame/Flow/wabish_physical_key_sans_sc_semibold`；静态字体族 `Wabish Physical Key Sans SC`、`SemiBold / 600`。构建脚本、字符清单、输入 / 输出哈希和 OFL 许可证均已入库。
- `DiceKingDemo.DrawArcadeMarketButton` 仅在 `lightKey=true` 时调用 `DrawArcadePhysicalKeyLabel`；`lightKey=false` 的刷新键继续 `DrawRunText -> MainGameLedFont`。
- 专用入口先把虚拟安全框乘当前 `GUI.matrix` 转成屏幕物理矩形，再临时使用单位矩阵以 22 / 24 px 或 33 / 36 px 原生栅格化；长文案只按整数像素向下装入。
- 常态不画新底板；悬停 / 按下 / 禁用只添加状态性边缘或低 Alpha 材质处理。资源缺失时保留 UI 字体安全回退并发出警告，但 QA 必须判失败，不允许静默回到 LED。
- `WabishMarketRuntimeCapture` 已增加字体资源反射断言，并自动输出两档分辨率下的 normal / purchase / leave 六图。`validate_market_physical_key_runtime_v1.py` 检查字体哈希、精确字色、字高、居中、购买键同线和纹理方差，当前全部通过。
- 不修改市场布局、按钮 Rect、鼠标热区、购买 / 卖出 / 刷新 / 离场逻辑、经济、数据、随机或存档。`GAME_FLOW.md` 与 `DICE_ARCHETYPES.md` 无需更新。

## 2026-07-17 LED 语义亮度 V2 运行合同

- 正式加载 key 为 `Art/MainGame/Flow/main_game_led_text_roles` 与 `Art/MainGame/Flow/main_game_led_text_brightness_groups`。离线样张目录中的 V2 CSV 只保留历史快照，不再作为权威输入。
- `MainGameLedFont.TextRole` 固定覆盖 `FocusAmber / FocusTeal / PrimaryAmber / SecondaryAmber / SecondaryWarm / Ambient / Disabled / InkOnLight / Warning / Success`；资源缺项时 `IsReady=false`，不得静默以旧任意色继续运行。
- 调用方使用 `DrawLedRoleText / DrawLedRoleWrappedText`。同级共用核心亮度与 Alpha；页面只决定语义角色和安全框，不得重新乘低 Alpha 或复制颜色常量。
- L2 / L3 的声明几何下限为 `Compact`。装不下时返回下限并暴露布局问题，不允许继续降到 `Micro`；L1 才可按合同降为 `Micro`。
- 当前接入覆盖主菜单、局内顶部 / 积分塔 / 终端 / 主操作和市场资源窗 / 商品名 / 六面 / 刷新 / 反馈，以及 Tooltip 关键行。五处浅色实体键帽仍由 `DrawArcadePhysicalKeyLabel` 独占。
- 回归固定输出原生 `1280×720 / 1920×1080`，再从 1080p 原图生成 `0.606×` 双线性压力图。原生图判断锐度、基线与裁切；压力图只判断层级和识别生存。
- 自动结果当前通过；用户视觉确认前不得把 F021 LED 亮度标为最终验收通过。

## 2026-07-22 F021-S5-UI1 程序交接

### 当前实现事实

- `DrawArcadeMarket` 已将卖出、购买、刷新和离场文案固定为动作名；价格与结构化反馈继续由商品信息、资源窗和 `rewardBanner` 承接。
- `DrawArcadeMarketButton` 统一返回 `None / Activated / Blocked`。受阻键保留透明说明命中层，但不会在按钮入口调用交易方法；受阻点击只写入短促回弹状态并交给调用方显示原因。
- 三枚商品键继续逐格读取 `CanBuyMarketOffer`；普通满袋、贡品和血契船长等特殊资格没有被视觉层简化重写。空货架不生成按钮，兼容改造道具商品复用同一“购买”动作入口。
- `ShowActionTip / DrawActionTip` 是当前统一动作 Tip 入口；成功与警告共用单槽、替换键和时序，只改变文案与语义色。
- Tip 在 `1280×720` 虚拟画布以 `(640, 200)` 为最终面板中心，从下方 `28 px` 飞入，`0.16s / 0.50s / 0.25s` 完成飞入、停留和上浮淡出；它在 Tooltip 之后绘制且没有输入热区。

### 建议实现边界

1. 新增统一按钮视觉状态入口，至少覆盖 `可用 / 悬停 / 按下 / 受阻 / 受阻点击`；业务层只提供动作名、资格结果、受阻原因和提示锚点，绘制层不得按原因自行换色或换文案。
2. 将按钮“可否执行”和“是否接受说明点击”拆开。受阻键不调用 `BuyOffer / SellSelectedMarketDie / RefreshMarketOffers / ActivateMarketLeave`，但允许命中后触发统一提示。
3. 新增统一短时提示视图模型和绘制 / 更新入口，至少包含文案、语义类型、位置预设、替换键、阶段时间和总时长；市场首版固定使用全画面 `UpperCenter`，在 `1280×720` 虚拟画布以约 `(640, 200)` 为最终面板中心，从下方 `24–32 px` 飞入。使用非缩放时间，不写入存档、不消费玩法随机。
4. 卖出、购买、刷新、离场的动作名由固定映射提供；价格继续由商品信息、顶部资源和现有结构化反馈承接。空货架不创建伪购买动作。
5. 购买资格继续调用 `CanBuyMarketOffer`。另提供确定性的受阻原因解析，优先顺序建议为交互锁定、空货架 / 非法商品、缺少贡品目标、金币不足、普通满袋；解析只生成提示，不参与资格本身。
6. 成功动作完成后调用同一提示入口；现有 `rewardBanner`、日志和市场事件仍保留，不由短时提示替代。

### 实现任务

| 任务 | 文件 / 数据 | 依赖 | 验收 | 状态 |
|---|---|---|---|---|
| 统一按钮视觉状态与说明命中 | `Assets/Scripts/DiceKingDemo.cs` | 状态样图通过 | 同状态像素与动效一致，受阻点击不执行动作 | 已实现；静态 PASS，待 Play Mode |
| 固定市场动作文案并拆出原因解析 | `Assets/Scripts/DiceKingDemo.cs` | `GAME_FLOW.md` 已确认规则 | 按钮不再显示价格 / 原因，特殊资格不回归 | 已实现；静态 PASS |
| 实现统一短时提示控件 | `Assets/Scripts/DiceKingDemo.cs` | 动效关键帧通过；位置以用户最新修订为准 | 全局中上飞入、停留、淡出、替换重播和不拦截输入通过 | 已实现；静态 PASS，待动态确认 |
| 自动状态矩阵与双分辨率截图 | `Assets/Editor/` 下市场验收工具 | 编辑器退出旧 Play 状态并刷新最新脚本 | 720p / 1080p 覆盖五状态、成功 / 受阻、混合资格 | 待 Play Mode |

### 数据与存档

不修改市场数据表、价格、骰子资格、经济、随机或 `SaveVersion`。若使用表现配置，只保存时序与位移等视觉参数，不把业务原因或玩法资格复制进配置。

### 用户授权

用户已通过三组状态样图，唯一修订是动作 Tip 统一放在整个游戏界面中间偏上；其它视觉与交互规格均通过，并明确授权接入 Unity。该决定关闭本切片的程序前置阻塞。

### 2026-07-22 实现验证

- Unity 2019.4.33f1 自带 Roslyn 对 `Assets/Scripts/*.cs` 独立编译：`0` 错误，`9` 个既有 `CS0162` 警告。
- 静态合同检查覆盖固定文案、资格真值、空货架、兼容商品、受阻无交易、结果映射、全局中上位置、时序、替换重播与顶层绘制，全部通过。
- 运行中的 Unity 窗口无法由桌面控制器激活，因此没有继续发送输入或覆盖历史截图；Play Mode 状态矩阵仍单独待验。
- 验证记录：`Docs/QA/20260722_market_button_action_tip_static_validation.md`。
