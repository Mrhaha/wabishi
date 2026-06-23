# F009 验收记录

状态：已接入统一资源首版，待运行验证
功能：F009 骰子过程动画表现

## 就绪度门禁

| 角色 | 就绪度 | 缺口 / 风险 | 建议补丁 | 阻塞问题 |
|---|---|---|---|---|
| 策划 | 就绪 | 已确认表现层边界和非目标 | 已按首版路线实现 | 无 |
| 美术 | 就绪待运行验证 | 统一资源已接入，但仍需 Play Mode 看真实尺寸和节奏 | 按截图反馈继续修裁切、透明边和点名高光 | 无 |
| 界面体验 | 就绪 | 需在运行后验证遮挡和节奏 | 按录屏反馈调节 | 无 |
| 程序 | 就绪 | OnGUI 序列帧骰子视觉层和 2D 回退已接入 | 运行验证后决定是否继续 F009-04 / F009-05 | 无 |

## 验收清单

- [x] 生产包已从 F009 设计对话生成。
- [x] 已记录首版目标：骰子入场、槽位内序列帧旋转、左到右停转显点、结算点名。
- [x] 已记录不改变随机、概率、出千、计分、金币、存档或骰子效果。
- [x] 已记录动画最终必须对齐真实结果。
- [x] 已记录物理模拟不决定骰面。
- [x] 已拆分策划、美术、界面体验、程序和验收。
- [x] 已拆成 F009-01 至 F009-06 执行切片。
- [x] 已完成 `$wabish-packet-review` 审查。
- [x] 已实现程序视觉层和 2D 回退。
- [x] 已实现小关入场骰子落桌。
- [x] 已实现自然投骰翻滚过程。
- [x] 已实现停转显点到真实点数。
- [x] 已实现结算点名与左侧计分同步。
- [x] 已验证动画层不调用 `ScoreDice()`。
- [x] 已验证动画层不重复写钱包、成长或临时小骰。
- [ ] 已验证出千前后槽位和结果一致。
- [ ] 已完成 Unity Play Mode 或等效运行验证。
- [ ] 已完成 `1280x720` 和 `1920x1080` 截图 / 录屏验收。
- [x] 已按实现事实同步 `PROJECT_CONTEXT.md` 和 `GAME_FLOW.md`。
- [x] 若接入最终资源，已同步 `ART_ASSETS.md`。

## 职能验证记录

2026-06-15：

- 用户要求将 F009 沉淀成正式生产包，拆给界面体验、美术、程序和验收。
- 已读取 `AGENTS.md`、`PROJECT_CONTEXT.md`、`GAME_FLOW.md`、`Docs/DesignDialogues/F009-dice-process-animation.md`、F004 / F006 / F007 相关记录和生产包规则。
- 已创建 F009 生产包首版。
- 当前未改运行时代码、数据或资源。
- 当前未同步 `PROJECT_CONTEXT.md` / `GAME_FLOW.md`，因为 F009 尚未实现。

2026-06-15：

- 已完成 `$wabish-packet-review` 审查，确认程序 3D / 2.5D 方块验证层作为首版默认路线。
- 已在 `Assets/Scripts/DiceKingDemo.cs` 接入程序 2.5D 骰子视觉层：小关入场、投骰翻滚、停转显点吸附、结算点名和 `F6` 回退开关。
- 静态检查确认没有新增 `ScoreDice()` 调用，没有新增 `PlayerPrefs` 写入点，没有修改 `CurrentSaveVersion`。
- 已同步 `PROJECT_CONTEXT.md` 和 `GAME_FLOW.md`。

2026-06-15：

- 已记录 F009 首版体验反馈到 `PLAYTEST_FEEDBACK.md`。
- 当前主要问题是：骰盅和骰子同时摇晃导致主焦点混乱；程序点数贴面与 2.5D 骰面适配不足；不同骰子类型缺少过程运动差异。
- 当前验收判断改为需小改。高优先级反馈解决前，不能把 F009 标记为已验收。

2026-06-15：

- 已接入 F009 首版反馈修正。
- 最新回炉方向已取消骰盅阶段和抛投概念，`Shaking` / `Stopping` 阶段改为固定槽位内原地立体翻转。
- 点数贴面已改为结果期定格可读优先，运动期不再强行显示清晰点数。
- 已新增按骰子家族分组的程序运动档案。
- 当前状态改为待运行验证；未完成 Play Mode 和截图 / 录屏前，仍不能标记为已验收。

2026-06-15：

- 历史首版曾接入固定槽位原地立体翻转：默认 F009 路径取消骰盅阶段，不抛投，骰子在原槽位内通过压缩面、侧面、背面、接触影和旋转轴表达 2.5D / 3D 翻转；当前已进一步收敛为序列帧式固定槽位旋转。
- 已同步 `PROJECT_CONTEXT.md`、`GAME_FLOW.md`、`Docs/DesignDialogues/F009-dice-process-animation.md` 和 F009 生产包文档。
- 已在 Notion 生产包页补充实现同步评论。
- `git diff --check` 通过；F009 未跟踪文档已额外检查无尾随空白；`Assets/Scripts/DiceKingDemo.cs` 大括号数量平衡。
- 当前 PATH 未找到 `Unity`、`dotnet`、`csc` 或 `mcs`，本轮未完成 Unity Play Mode、编译、截图或录屏验收。

2026-06-16：

- 已按用户反馈改为序列帧式固定槽位旋转：不再把主要表现做成骰子本体摇晃或连续方块形变。
- `Ready` 阶段显示每颗骰子的类型身份；未达标结算后回到 `Ready` 时也会清回类型身份显示。
- `Shaking` 阶段按每颗骰子的序列帧种子播放旋转帧；`Stopping` 阶段从左到右逐步停住，停住的槽位立即显示真实点数。
- 现有 F006 / F007 结算点名和左到右跳分仍作为结算阶段表现来源。
- `Assets/Resources/Data/roll_feedback_config.csv` 中 `stop_duration` 调整为 `0.90`，只影响停转表现时长。

2026-06-16：

- 已将本次结果锁定提前到加力窗口结束后的 `BeginStopRoll()`，仍不受连打影响，且早于结算。
- `FinishShakeRoll()` 不再重新掷骰，只负责进入结果确认，避免停转显点和真实结果出现二次变化。
- 停转阶段每个槽位达到停住阈值后直接显示真实 `EffectiveValue`，未停住前仍隐藏可读点数。

2026-06-16：

- 已找到 Unity Editor：`F:\unity\Unity20190433_release\Editor\Unity.exe`。
- 尝试使用 batchmode 执行 `DiceKingUiAcceptanceRunner.Run`，但项目已被另一个 Unity 实例打开，Unity 拒绝同一工程多实例运行。
- 未强制关闭已有 Unity 进程，避免破坏当前编辑器状态；Play Mode、截图和录屏验收仍待用户关闭当前 Unity 实例后执行。

2026-06-16：

- 已接入“桌面摩擦旋转 V1”运行时资源：24 帧 loop strip 和 8 帧 stop preview strip 已放入 `Assets/Resources/Art/DiceRoll/` 并由 F009 绘制层优先读取。
- 静态检查确认新增资源路径为 `Art/DiceRoll/f009_table_friction_spin_loop_strip_24f_256` 和 `Art/DiceRoll/f009_table_friction_spin_stop_strip_8f_256`，并登记到 `ART_ASSETS.md`。
- 停转预览只负责减速质感，最终点数仍使用真实 `EffectiveValue` 绘制；该项需要 Play Mode 录屏确认视觉切换是否自然。
- 本次仍未完成 Unity Play Mode、`1280x720` / `1920x1080` 截图或录屏验收，不能标记为 F009 已验收。

2026-06-16：

- 已按最新反馈移除结算开始前的单独结果定格停顿，结算确认后直接从当前槽位入账 / 点名事件开始。
- `Ready` 和未达标结算后重置阶段改为直接显示骰子类型原图，不再绘制额外投掷前骰框。
- 已按截图反馈修正 `Ready` 阶段仍出现外框的问题：F009 视觉层在待机状态直接走类型原图绘制，不再落到程序骰面帧。
- 已去掉可见槽位编号角标，并移除旧 `DrawSlotBadge()` 绘制方法；结算点名通过原槽位高亮、轻抬和分数飞字表达来源。
- 静态检查通过：代码中无 `SettlementEventKind.Freeze`、`SettlementFreezeDuration` 或 `账本定格` 残留；`git diff --check` 通过；`Assets/Scripts/DiceKingDemo.cs` 大括号数量平衡；最近 800 行 Unity Editor 日志未发现 `error CS`。
- 本次静态复查通过：旧数字角标绘制方法已无残留，旧数字来源提示口径已无残留；`git diff --check` 通过；`Assets/Scripts/DiceKingDemo.cs` 大括号数量平衡；最近 800 行 Unity Editor 日志未发现 `error CS`。
- 本次仍未完成 Unity Play Mode、`1280x720` / `1920x1080` 截图或录屏验收，不能标记为 F009 已验收。

2026-06-16：

- 已根据用户提供的三阶段截图生成统一骰子母版 V1 方向接触图：`Assets/ArtSource/Production/F009/f009_unified_die_visual_language_contact_sheet_v1_20260616.png`。
- 方向判断：待机、旋转、停转、结果点面和结算高亮已经能看成同一颗明亮账本桌游骰，优于当前三套视觉语言混用。
- 风险判断：V1 偏金边贵族骰，装饰强度高于基础骰应有气质；若进入运行资源生产，需要压低金属边和皇冠封印，保留统一壳体、温暖点数、椭圆阴影和 3/4 角度。
- 当前状态为方向待用户评审；未替换 `Assets/Resources/Art/DiceRoll/` 的运行时 strip，未改代码，未完成 Play Mode 验收。

2026-06-16：

- 已按统一母版方向生成三张阶段接触图：`f009_unified_stage_ready_contact_sheet_v1_20260616.png`、`f009_unified_stage_roll_stop_contact_sheet_v1_20260616.png`、`f009_unified_stage_result_scoring_contact_sheet_v1_20260616.png`。
- 待机 / 入场接触图：外框和槽位数字已消失，骰子以统一 3/4 奶油骰体和软阴影呈现，整体通过样张自检。
- 旋转 / 停转接触图：已避免纸片翻折和方形卡片感；风险是快速帧前两帧动感偏糊，后续拆 strip 时需要保留更稳定外轮廓，并压低金色角件。
- 结果 / 结算接触图：1 到 6 点面保持 3/4 实体骰，不再是程序方形卡片；风险是结算高光和星芒只能用于点名阶段，不能进入普通结果常态。
- 本次只生成 ArtSource 样张和文档记录；未替换运行时 strip，未改代码，未完成 Unity Play Mode 验收。

2026-06-16：

- 已按用户确认的三阶段统一方向替换到游戏中，新增运行时资源：`Assets/Resources/Art/DiceRoll/f009_unified_ready_die_256.png`、`Assets/Resources/Art/DiceRoll/f009_unified_spin_loop_strip_24f_256.png`、`Assets/Resources/Art/DiceRoll/f009_unified_spin_stop_strip_8f_256.png`、`Assets/Resources/Art/DiceFaces/f009_unified_result_die_faces_6x256.png`。
- 已新增运行预览图：`Assets/ArtSource/Production/F009/f009_unified_runtime_assets_preview_20260616.png` 和 `Assets/ArtSource/Production/F009/f009_unified_runtime_assets_checker_preview_20260616.png`。
- 已接入 `DiceKingDemo.cs`：`Ready` / 重置、`Shaking`、`Stopping`、`ResultDecision`、`Scoring` 均优先使用统一资源；旧桌面摩擦 strip 和程序点阵保留为回退。
- 静态检查确认 `ScoreDice()` 调用点仍为 `BeginSettle()` 内一次，`CurrentSaveVersion` 仍为 `4`，本次未新增 `PlayerPrefs` 写入字段。
- 已同步 `ART_ASSETS.md`、`PROJECT_CONTEXT.md`、`GAME_FLOW.md`、`ART_BRIEF.md` 和 `DEV_HANDOFF.md`。
- 已在 Notion 生产包页追加状态评论，记录统一资源接入、静态验证和待运行验收项。
- 本次仍未完成 Unity Play Mode、`1280x720` / `1920x1080` 截图或录屏验收，不能标记为 F009 已验收。

2026-06-16：

- 用户运行截图反馈：旋转动画和最后显示点数存在图像不完整问题。
- 已重拆并覆盖同名运行资源：待机骰、24 帧旋转 strip、8 帧停转 strip 和 1 到 6 点结果骰面 strip。
- 已改用完整小画幅、安全边距、统一底色和边缘清理；旋转 loop 过滤了会带相邻帧碎片的源帧，结果骰面扩大安全区后缩放，避免顶部、侧边或阴影被裁切。
- 已去掉基础骰在旋转和结果阶段额外叠加的底部类型色条，降低“图像被截断”的错觉；非基础骰仍保留小类型标记。
- 本次仍未完成 Unity Play Mode、`1280x720` / `1920x1080` 截图或录屏验收，需用户重新进入当前流程确认修复后的实际观感。

## 验证备注

- 当前实现阶段尚未运行 Unity 验证。
- 当前不能声明 F009 已验收。
- F009 是强动态表现包，最终必须用 Play Mode 录屏或等效运行验证。
- 主流程视觉改动后必须按 `Docs/UI_ACCEPTANCE_FEEDBACK_WORKFLOW.md` 做截图验收。

## 已知缺口

- 统一旋转和统一停转资源已接入，仍需运行回归确认 `Shaking` 阶段是否足够干净、`Stopping` 从左到右停下并显点是否自然。
- 统一结果骰面已接入，仍需截图确认 1 到 6 点和类型标记在 `1280x720` 与 `1920x1080` 下都不挤压。
- 按骰子类型分组的程序旋转档案已接入，仍需录屏确认差异是否可感知且不过度抢戏。
- 新的完整循环仍需录屏确认：统一待机骰、序列帧旋转、左到右停转显点、原槽位高亮 / 轻抬点名、结算后回到统一待机骰。
- 首版技术路线已按 OnGUI 序列帧骰子视觉层接入，仍需运行画面确认是否达到预期。
- 未来是否继续升级为真实 3D 相机或渲染纹理待原型反馈。
- 统一资源已按截图反馈改为完整裁切版；仍需在主投掷区真实底色上截图判断统一底色是否明显、旋转帧是否仍有残留碎片。
- 出千重摇、音效、震动和加速结算后置。

## 最终结论

F009 序列帧式固定槽位旋转首版已实现到代码并接入统一运行资源，当前处于待运行验证状态。下一步应在 Unity Play Mode 中检查普通投骰、左到右停转显点、出千前后、结算点名、`F6` 回退、`1280x720` 和 `1920x1080` 画面遮挡；验证通过后再决定是否进入 F009-04 出千重摇动画、透明边精修或更高规格 3D 表现。
