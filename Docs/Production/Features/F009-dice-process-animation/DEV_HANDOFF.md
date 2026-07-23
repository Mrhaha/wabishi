# F009 程序交接

状态：摇骰 V4R6 与结算 V6B-R5 已完成 Unity 首版接入；来源 / 回环触发语义修正已通过 Unity 2019.4.33 编译与双分辨率自动运行验证，待用户连续实机回归
功能：F009 骰子过程动画表现
关联主文档：PROJECT_CONTEXT.md、GAME_FLOW.md
实现事实来源：Assets/Scripts/DiceKingDemo.cs

## 输入决策

- F009 只做表现层，不改玩法规则。
- 表现层最终点数必须吸附到真实结果。
- 结算点名消费 F006 / F007 的提交态和展示节奏。
- 首版默认技术路线为 OnGUI 序列帧播放器，统一帧图资源已作为默认运行资源；真实 3D 相机、渲染纹理和更高规格专属资源仍后置。

## 实现目标

在当前单文件原型中新增或封装一层只读骰子视觉表现，让主流程能播放入场、固定槽位内序列帧旋转、左到右停转显点和结算点名。该层读取当前 `dice`、`rollPhase`、真实骰面、槽位顺序、`scoringDice`、`SettlementDisplayEvent` 和 `RunScoreCounterState` 相关展示状态，但不拥有玩法结果。

## V5 接入前置结论

V5R2 不再沿用 V4 的 `UnderTrayCharge`、槽间电流或贯穿骰列扫描，也不允许 V5R1 的实体骰离槽 / 撞塔 / 回程。未来实现保持六骰原槽绘制，只为当前骰叠加低幅来源聚焦，并生成一枚小型积分光晕直接进入左侧本关得分 / 积分塔；积分塔是唯一全局计分反馈锚点。

当前代码事实与目标流程仍有三处明确差异，本轮不修改代码：

1. `Assets/Resources/Data/roll_feedback_config.csv` 的 `input_window_duration` 当前仍为 `1.35`，目标首版值为 `0`。
2. `DiceKingDemo.cs` 当前仍在 `ResultDecision` 等待输入后调用 `BeginSettle()`，目标是结果锁定后自动交接，不保留第二次主动结算。
3. `AddSettlementSlotEvent()` 当前对 `ValueDelta` 使用 `Mathf.Max(0, valueDelta)`，会吞掉负贡献；V5 需要带符号展示值才能正确表现扣分包。

### 未来展示数据合同

建议新增只读的“物理槽贡献包”视图，不改变真实结算：

- `SourceSlotIndex`：来源物理槽。
- `SourceDieId`：来源实体骰。
- `SignedContribution`：本槽聚合后的带符号贡献。
- `ProgressScoreAfterImpact`：命中积分塔后的真实累计分。
- `LocalChainCount`：本槽局部连锁数量，只用于包体密度，不逐事件发弹丸。
- `IncludesTemporaryScore`：是否折入临时骰 / 附属得分。
- `LocalImpactWeight01`：由 `abs(SignedContribution)` 相对可配置关卡参考量，经 `sqrt` / `log` 一类压缩曲线并钳制到 `0..1`；必须保持同手内真实贡献强弱顺序，不显示给玩家。
- `FinalHandStrengthTier` / `FinalHandStrength01`：按最终已提交总分计算的整手五档与档内连续值，只驱动积分塔最终负载；不得反向覆盖单骰重量。
- `CrossesTarget`：本包是否首次跨过目标，只触发一次目标锁扣。

聚合规则必须以当前物理槽为单位：当前槽的点数、类型效果、遭遇修正和可归属临时得分合成一个包；本槽局部连锁清空后再提交。临时骰没有可靠来源时应阻塞 V5 正式接入，不能退回逐临时骰弹丸或新增托盘。

### 未来表现状态

- `SourceFocus`：当前骰保持原槽，只叠加 `40–60ms` 的紧凑边缘、骰面短亮和接触影收紧。
- `HaloCondense`：用约 `50–80ms` 在当前骰面上方凝成小型积分光晕。
- `HaloFlight`：单枚光晕约 `100–140ms` 飞向积分塔，只保留贴身短残影。
- `ScoreTowerImpact`：积分塔约 `100–160ms` 吸收和回弹；幅度读取 `LocalImpactWeight01`。
- `TowerAfterglow`：余辉与下一骰来源聚焦交叠 `60–80ms`。
- `CriticalImpact`：只在积分塔区域执行预压、主击、余震和恢复；不得横扫骰列。

同屏积分光晕上限固定为 `1`。下一骰来源聚焦可以和上一击余辉重叠，但下一枚光晕必须等上一枚被积分塔吸收后再生成，避免交叉飞行重新形成杂乱网络。

## 已确认行为

- `BeginShakeRoll()` 仍负责起摇和投骰配置快照。
- `BeginStopRoll()` / 停止阶段在加力窗口结束后锁定本次结果，随后只播放左到右停转显点。
- 真实骰面仍由现有随机和出千逻辑决定。
- `BeginSettle()` 仍只调用一次真实 `ScoreDice()`。
- F009 动画不得调用 `ScoreDice()`。
- F009 动画不得重复生成临时小骰、重复写钱包或重复应用成长。
- 若 F009 视觉层不可用，现有 2D 绘制路径必须可继续完成完整流程。

## 实现任务

| 任务 | 文件 / 数据 | 依赖 | 验收 | 状态 |
|---|---|---|---|---|
| 建立视觉层状态结构 | Assets/Scripts/DiceKingDemo.cs | `RollPhase`、当前骰袋 | 能记录每颗骰子的槽位、序列帧种子、目标结果和阶段 | 已实现待运行验证 |
| 接入小关入场 | Assets/Scripts/DiceKingDemo.cs | 进入小关 / `StartStage` 相关流程 | 六颗骰子按槽位落桌，不改变骰袋顺序 | 已实现待运行验证 |
| 接入投骰旋转 | Assets/Scripts/DiceKingDemo.cs | `BeginShakeRoll()`、`UpdateShakeRoll()`、`UpdateStopRoll()` | 加力只影响表现强度，不改随机 | 已实现待运行验证 |
| 接入停转显点吸附 | Assets/Scripts/DiceKingDemo.cs | 加力窗口结束后锁定的真实骰面 | 左到右停住并显点，最终点数、槽位和结果预览一致 | 已实现待运行验证 |
| 接入结算点名 | Assets/Scripts/DiceKingDemo.cs | `SettlementDisplayEvent`、`scoringDice` | 当前骰高亮 / 轻抬与左侧计分同步 | 已实现待运行验证 |
| 实现回退开关 | Assets/Scripts/DiceKingDemo.cs 或调试配置 | 当前 2D 绘制路径 | 关闭 F009 后主流程仍可玩 | 已实现待运行验证 |
| 验证不侵入规则 | 代码检查 / Play Mode | `ScoreDice()`、钱包、成长、临时小骰 | 无重复计分、无重复入账、无存档变化 | 静态已检查，运行待验证 |

V5 / V6 追加任务已按用户确认和 Unity 接入授权执行；以下状态覆盖早期暂停口径：

| 任务 | 文件 / 数据 | 关键验收 | 状态 |
|---|---|---|---|
| 取消第二次主动结算并将加力默认值改为 `0` | `DiceKingDemo.cs`、`roll_feedback_config.csv` | 一次 `Space` 后自动完成滚动、停靠和结算；基础滚动保留 `0.56s` | 已完成并验证 |
| 生成物理槽贡献包视图 | `DiceKingDemo.cs` | 六个物理槽各至多一个包，临时分可归属，负分不丢失 | 已完成 |
| 接入单向积分光晕 | 展示层 | 骰子本体原位；轨迹不连接骰子；同屏最多一枚，只命中积分塔 | 已完成 |
| 接入积分塔五档承击 | 展示层 / 表现配置 | 五档隐藏、连续插值；暴击只在积分塔产生一次主峰 | 已完成首版 |
| 接入实体骰离槽直击与回程 | 展示层 / 表现配置 | V5R1 已退回，不得实现 | 取消 |
| 接入单骰连续重量 | 展示层 / 表现配置 | 强弱顺序与真实贡献一致；局部峰值不冒充整手暴击 | 已完成 |
| 双分辨率与来源 / 回环验证 | Play Mode / 截图 | `1280×720 / 1920×1080`，覆盖普通触发、再触发和跨槽响应 | 已完成自动验证，连续实机待回归 |

## 代码影响

主要影响 `Assets/Scripts/DiceKingDemo.cs` 的展示层、投掷区绘制、投骰阶段更新和结算点名显示。允许新增内部数据结构或小型 helper 方法。首版不要求拆出新 C# 文件，但如果实现量明显膨胀，应优先隔离为只读视觉控制结构，避免继续扩大主逻辑耦合。

建议命名方向：

- `DiceVisualState`
- `DiceVisualController`
- `DiceVisualPhase`
- `BeginDiceVisualEnter()`
- `UpdateDiceVisuals()`
- `SyncDiceVisualsToResults()`
- `ApplySettlementVisualFocus()`

命名可按现有代码风格调整，但职责必须清楚：只管视觉。

## 数据影响

首版不要求新增数据文件。若需要调参，优先在代码中保留安全默认值；待原型确认后，再决定是否新增 `dice_process_animation_config.csv` 或扩展 `roll_feedback_config.csv`。

不得把 F009 视觉参数写入运行存档。

## 存档影响

无存档字段变化。不得修改 `SaveVersion`。继续游戏仍恢复到小关开始，不恢复小关中途动画状态。

## 界面影响

- 主投掷区增加视觉骰层。
- 结果决策阶段仍显示可读点数、类型色条、材质和状态标签。
- 结算阶段当前骰点名效果要和 F006 标签、金币飞字、倍率章共存。
- 视觉层不得遮挡左侧积分塔、资源牌、右侧规则和底部提示。

## 测试 / 验证计划

- 静态检查：确认 `ScoreDice()` 没有被动画层新增调用。
- 静态检查：确认没有新增 `PlayerPrefs` 字段和 `SaveVersion` 修改。
- 运行检查：普通基础骰自然投骰。
- 运行检查：高倍率手，确认停转阶段逐槽显示的点数和结算倍率一致。
- 运行检查：出千前后槽位不变。
- 运行检查：金币、龟龟、大树、贿赂场景不重复应用。
- 回退检查：关闭或禁用视觉层后现有 2D 结果可用。
- 截图 / 录屏：覆盖 `1280x720` 和 `1920x1080`。

## 文档同步

| 文档 | 是否需要更新 | 已确认来源 |
|---|---|---|
| PROJECT_CONTEXT.md | 实现完成后需要 | F009 已确认规则和实现事实 |
| GAME_FLOW.md | 实现完成后需要 | F009 已确认只读表现边界 |
| DICE_ARCHETYPES.md | 不需要，除非误改骰子效果 | 本包不改骰子规则 |
| ART_ASSETS.md | 仅最终资源接入后需要 | ART_BRIEF.md |
| Docs/DesignDialogues/F009-dice-process-animation.md | 已生成生产包后需要更新路径 | 本包路径 |

## 实现记录

2026-06-15：

- 已在 `Assets/Scripts/DiceKingDemo.cs` 接入首版程序 2.5D 骰子过程视觉层。
- 小关开始调用 `BeginDiceVisualEnter()`，在 `Ready` 阶段播放按槽位落桌。
- 起转、加力、停转和显点分别接入 `BeginDiceVisualRoll()`、视觉冲量、`BeginStopRoll()` 结果锁定和左到右序列帧停转。
- `DrawTableDice()` 在 F009 开启时绘制序列帧式骰子；`Shaking` / `Stopping` 阶段默认不绘制骰盅，骰子保持固定槽位播放旋转帧。
- 结算阶段复用 `SettlementDisplayEvent` 和 `IsScoreViewActive()`，当前入账骰会轻抬并保留原有分数飞字、路线标签和金币飞字。
- 调试期可用 `F6` 开关 F009 视觉层；关闭后回退到原 2D 展示路径。
- 本实现不新增运行时资源、不新增数据表、不修改 `PlayerPrefs`、不修改 `SaveVersion`、不新增 `ScoreDice()` 调用。

2026-06-15：

- 已按 `PLAYTEST_FEEDBACK.md` 的首版反馈修正 F009 表现层。
- 最新回炉方向取消骰盅阶段和抛投概念，`Shaking` / `Stopping` 阶段改为固定槽位内序列帧旋转。
- `DrawDiceProcessCube()` 曾用于程序 2.5D 验证；当前默认绘制路径已切到 `DrawDiceSequenceFrame()`，通过离散正面、侧面、背面、压缩面、接触影和类型运动档案表达旋转感。
- 程序点数贴面已改为定格可读优先：运动期不强行显示清晰点数，结果期使用更小的类型角标和色条，降低对点数区的遮挡。
- 已新增按骰子家族分组的程序运动档案：龟龟、大树、金币、奇偶、爆发和普通骰在轴向、速度、横纵摆幅、旋转幅度和停转收束上有基础差异。
- 本次修正仍不新增运行时资源、不新增数据表、不修改 `PlayerPrefs`、不修改 `SaveVersion`、不新增 `ScoreDice()` 调用。

2026-06-16：

- 已按用户反馈将 F009 从连续程序方块形变改为槽位内序列帧式旋转。
- `Shaking` 阶段每颗骰子按离散帧播放正面、侧面、背面、压缩面和接触影，不再对整颗骰子做横纵摇晃。
- 加力窗口结束后即锁定本次真实结果；`Stopping` 阶段按槽位从左到右逐步停住，停住的槽位立即显示真实点数。
- `FinishShakeRoll()` 不再重新掷骰，只进入 `ResultDecision`，避免停转显点和真实结果二次变化。
- 每次小关开始和未达标回到 `Ready` 时，骰子恢复为类型身份显示，下一次按 `Space` 再进入序列帧旋转。
- `stop_duration` 默认值调整为 `0.90`，让左到右停转过程可被看见；该参数仍只影响表现，不影响随机或结算。
- 本次修正仍不新增运行时资源、不新增数据表、不修改 `PlayerPrefs`、不修改 `SaveVersion`、不新增 `ScoreDice()` 调用。

2026-06-16：

- 已将用户确认的“桌面摩擦旋转 V1”接入运行时资源路径：24 帧 loop strip 位于 `Assets/Resources/Art/DiceRoll/f009_table_friction_spin_loop_strip_24f_256.png`，8 帧 stop preview strip 位于 `Assets/Resources/Art/DiceRoll/f009_table_friction_spin_stop_strip_8f_256.png`。
- `DiceKingDemo.cs` 的 F009 绘制层优先使用 `Resources.Load<Texture2D>` 读取上述 strip，并通过 `GUI.DrawTextureWithTexCoords` 按帧裁切；资源缺失时仍回退到原程序离散帧。
- `Shaking` 阶段使用 24 帧桌面摩擦旋转，`Stopping` 阶段按槽位停转进度使用 8 帧停转预览；达到停住阈值后立即绘制真实 `EffectiveValue` 点数，不使用 stop 预览里的示例点数作为结果。
- 按骰子家族保留不同基础帧速、相位和停转节奏，让龟龟、大树、金币、奇偶和爆发类骰子在同一套资源上仍有差异。
- 本次接入不新增数据表、不修改 `PlayerPrefs`、不修改 `SaveVersion`、不新增 `ScoreDice()` 调用；运行验收仍待 Unity Play Mode 和截图 / 录屏确认。

2026-06-16：

- 已按最新示意图反馈优化完整循环：`Ready` 和结算后重置阶段直接绘制 `DiceTypes` 原图，不再额外绘制投掷前骰框。
- 已移除结算队列中的单独结果定格事件，`BeginSettle()` 之后直接进入首个槽位入账 / 点名事件。
- 已按截图反馈修正 `Ready` 路径：F009 序列帧视觉层在 `Ready` 阶段显式绕过程序骰面帧，改为调用 `DrawReadyDieToken()` 直显类型原图。
- 已去掉可见槽位编号角标，并移除旧 `DrawSlotBadge()` 绘制方法；结算点名改由原槽位高亮、轻抬和分数飞字表达来源。
- 未达标且仍有出手机会时，`PrepareNextRollVisualReadyState()` 继续清理本手点面状态并触发 `BeginDiceVisualEnter()`，下一手回到类型原图展示。
- 本次仍不新增资源、不新增数据表、不修改 `PlayerPrefs`、不修改 `SaveVersion`、不新增 `ScoreDice()` 调用；运行验收仍待 Unity Play Mode 和截图 / 录屏确认。

2026-06-16：

- 已按用户确认的三阶段统一方向替换运行资源：`Ready` / 重置阶段优先读取 `Art/DiceRoll/f009_unified_ready_die_256`，`Shaking` 阶段优先读取 `Art/DiceRoll/f009_unified_spin_loop_strip_24f_256`，`Stopping` 阶段优先读取 `Art/DiceRoll/f009_unified_spin_stop_strip_8f_256`，`ResultDecision` 和 `Scoring` 的 1 到 6 点结果优先读取 `Art/DiceFaces/f009_unified_result_die_faces_6x256`。
- `DiceKingDemo.cs` 保留旧桌面摩擦 strip 作为 `Shaking` / `Stopping` 资源缺失时的回退；`7+` 点结果直接在运行时骰面底图上显示居中数字，避免成长骰高点数无法读清。
- `DrawReadyDieToken()` 改为优先绘制统一待机骰，非基础骰叠加小类型标记，不恢复外框或槽位数字。
- `DrawDiceSequenceFrame()` 改为在结果可见且点数为 1 到 6 时优先绘制统一结果骰面，再叠加原有类型保底标记；不改变停转吸附、出千、结算或分数来源。
- 已新增运行时资源和 `.meta`，并同步 `ART_ASSETS.md`、`PROJECT_CONTEXT.md`、`GAME_FLOW.md` 和 F009 包文档。
- 本次仍不新增数据表、不修改 `PlayerPrefs`、不修改 `SaveVersion`、不新增 `ScoreDice()` 调用；运行验收仍待 Unity Play Mode 和截图 / 录屏确认。

2026-06-16：

- 已按运行截图反馈修复统一资源图像不完整问题：同名覆盖 `f009_unified_ready_die_256.png`、`f009_unified_spin_loop_strip_24f_256.png`、`f009_unified_spin_stop_strip_8f_256.png` 和 `f009_unified_result_die_faces_6x256.png`。
- 资源重拆策略改为完整小画幅、安全边距、统一底色和边缘清理；旋转 loop 过滤会带相邻帧碎片的源帧，结果骰面扩大纵向安全区后缩放，避免最终点数顶部或边缘被裁切。
- `DrawDiceRollSpriteMarker()` 和 `DrawProcessDieTypeMarker()` 对 `DieType.Basic` 直接返回，不再给基础骰叠加底部类型色条，避免普通结果看起来像图像底部被截断。
- 本次仍不新增数据表、不修改 `PlayerPrefs`、不修改 `SaveVersion`、不新增 `ScoreDice()` 调用；运行验收仍待 Unity Play Mode 和截图 / 录屏确认。

## 阻塞项

V5R2 新接入阻塞于用户对“原槽聚焦 + 积分光晕入塔 + 单骰连续贡献重量”静态板及后续动态样片的确认。确认前不得修改 `input_window_duration`、主动结算状态机、展示事件结构或运行资源。进入实现后还必须先解决带符号贡献、临时骰来源聚合、`LocalImpactWeight01` 参考量和光晕层绘制所有权；否则不能正确覆盖负分、附属得分和六骰身份边界。旧统一资源首版的运行验收仍保留，但不能替代 V5R2 的新视觉门禁。

## 2026-07-21 V4R6 摇骰实现记录

本节只解除摇骰分支的旧暂停条件；结算 V5 / V6 视觉仍保持独立门禁。

- `DiceKingDemo.DrawDiceProcessToken()` 在 F021 主流程内直接把 `Shaking / Stopping` 路由到 `DrawArcadeRunFaceReelDie()`，不再对 `128×128` 物理槽应用旧 `DiceVisualAnimatedRect()`、整骰旋转矩阵或类型档案速度。
- `DrawArcadeRunFaceReelDie()` 先绘制现有家族壳与固定接触影，再用 `GUI.BeginGroup(reelWindow)` 裁切面值带；底部类型芯保持固定，只有数字层移动。
- `RollFeedbackConfig` 新增 `base_spin_duration=0.56`，并允许 `input_window_duration=0`；默认 `stop_duration=0.66`。`MainGameFlowPresentationProfile` 新增滚轴加速、快慢格速、结果接入点、下冲和回勾参数。
- `ResultDecision` 在关闭 V0.2 出千时仅承担 `0.20s` 稳定读点，之后自动调用既有 `BeginSettle()`；未新增随机、计分或存档入口。
- `WabishF009FaceReelCapture` 覆盖两档分辨率的高速、停靠波和自动结算截图。隔离 Unity 2019.4.33 批处理编译成功，运行状态记录为 `PASS`。
- 静态边界复核：没有新增 `ScoreDice()` 调用，没有修改 `CurrentSaveVersion` / `PlayerPrefs`，没有让动画候选值写回 `EffectiveValue`。

验证证据：`Docs/QA/20260721_f009_face_reel_runtime.md` 与同名前缀六张 PNG。

## 2026-07-22 V6B-R5 结算接入与来源语义修正记录

- `SettlementContributionRecord` 保留带符号贡献并新增 `SemanticLabel`；同来源、同阶段记录合并时同步合并机制语义。可归属临时贡献折入对应物理槽，不再在展示层把负值钳成零。
- `PrepareImpactSettlementDisplayEvents()` 使用只读贡献记录生成单枚积分光晕事件；光晕大小、密度、短尾和积分塔承击读取连续 `LocalImpactWeight01`，最终隐藏五档只控制整手塔体收束。
- 来源标签通过 `SettlementContributionDisplayLabel()`、`SettlementSemanticLabelForDie()` 和复制 / 响应 helper 生成。普通事件锚定真实来源槽；局部回环中同时绘制来源槽 `再触发 / 响应` 与低强度 `主槽·…` 上下文，右侧终端回声当前机制。
- 新结算路径继续抑制旧逐骰大号 `+N`、骰旁大幅抬升和底部重复横幅；没有增加骰间连线、共享轨迹、第二枚并发光晕或新的暂停节点。
- 自动验收入口为 `Assets/Editor/WabishF009SettlementSourceCapture.cs`，覆盖 `养猪·触发器`、`再触发·三只小猪`、`响应·盟约` 三类归因场景。
- 隔离 Unity 2019.4.33f1 编译为 `0 error`；原生 `1920×1080 / 1280×720` 共六张运行截图和 `0.606×` 显示压力检查均通过。详细记录见 `Docs/QA/20260722_f009_settlement_source_clarity.md` 与 `Docs/QA/20260722_f009_settlement_source_metrics.json`。
- 未修改随机、真实计分、钱包、长期成长、骰子效果、`PlayerPrefs` 或 `CurrentSaveVersion`。剩余门禁仅为用户在当前工程连续播放真实整手，确认来源标签不增噪且声音 / 节奏成立。
