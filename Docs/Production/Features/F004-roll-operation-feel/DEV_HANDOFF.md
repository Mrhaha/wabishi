# F004 程序交接

状态：F004-03 已实现待 Unity 验证
功能：F004 投骰操作手感
关联主文档：PROJECT_CONTEXT.md、GAME_FLOW.md
实现事实来源：Assets/Scripts/DiceKingDemo.cs

## 输入决策

- 当前投骰从旧的“延长停止时间”改为“有限加力窗口 + 固定表现冲量 + 自然回落”。
- 投骰表现参数必须配置化，并支持运行时重载。
- 当前投骰使用开始时的配置快照，运行中重载的新配置从下一次投骰生效。
- 敲击次数、敲击节奏、表现强度和配置值不得影响骰面随机、出千、分数或结算。

## 实现目标

在 `Assets/Scripts/DiceKingDemo.cs` 中完成低风险改造：保留现有单文件原型结构，新增投骰表现配置结构、默认配置、CSV 加载、调试覆盖读取、手动重载入口、状态机改造和提示文案更新。

## 已确认行为

- `Ready` 中按 `Space` 起摇。
- `Shaking` 改为有限加力窗口。
- 加力窗口内每次有效 `Space` 增加表现冲量。
- 加力强度受上限约束。
- 窗口结束后进入 `Stopping`。
- `Stopping` 中的额外 `Space` 只触发轻量“显示结果”反馈。
- `FinishShakeRoll` 仍负责锁定结果。
- 投骰表现配置不进入存档。

## 实现任务

| 任务 | 文件 / 数据 | 依赖 | 验收 | 状态 |
|---|---|---|---|---|
| 新增投骰表现配置结构 | Assets/Scripts/DiceKingDemo.cs | FEATURE_BRIEF.md 建议字段 | 有安全默认值和范围钳制 | 已完成 |
| 新增默认配置文件 | Assets/Resources/Data/roll_feedback_config.csv | FEATURE_BRIEF.md 配置草案 | `Resources` 能读取默认配置 | 已完成 |
| 新增调试覆盖读取 | Application.persistentDataPath/roll_feedback_config.csv；StreamingAssets 可选 | 程序默认策略 | 修改外部配置后可手动重载 | 已完成 |
| 新增运行时重载入口 | Assets/Scripts/DiceKingDemo.cs | 输入策略 `F5` | 重载成功和失败都有日志 | 已完成 |
| 改造 `BeginShakeRoll` | Assets/Scripts/DiceKingDemo.cs | 配置结构 | 投骰开始创建配置快照 | 已完成配置快照 |
| 改造 `UpdateShakeRoll` | Assets/Scripts/DiceKingDemo.cs | 状态机规则 | 不再延长倒计时，改为固定窗口和冲量 | 已完成 |
| 改造 `BeginStopRoll` / `UpdateStopRoll` | Assets/Scripts/DiceKingDemo.cs | 回落配置 | 使用配置化回落时长和衰减 | 已完成 |
| 更新骰盅绘制参数 | Assets/Scripts/DiceKingDemo.cs | UI_UX_SPEC.md | 骰盅运动读取快照参数 | 已完成待 Unity 验证 |
| 更新阶段提示和设置说明 | Assets/Scripts/DiceKingDemo.cs | UI_UX_SPEC.md | 不再出现“延迟停止”旧文案 | 已完成待 Unity 验证 |
| 验证随机隔离 | Assets/Scripts/DiceKingDemo.cs 或人工检查 | ACCEPTANCE.md | 敲击和配置不影响骰面结果 | 代码审查已完成，待运行验证 |

## 代码影响

当前已知相关位置：

- `RollPhase.Ready / Shaking / Stopping`。
- 字段：`shakeTimer`、`shakePower`、`stopTimer`、`stopStartPower`。
- 已迁移旧常量：`ShakeStartTime`、`ShakeTapBonus`、`ShakeMaxTime`、`StopDuration` 不再作为投骰主逻辑来源，运行时改用配置快照。
- 方法：`BeginShakeRoll`、`UpdateShakeRoll`、`BeginStopRoll`、`UpdateStopRoll`、`FinishShakeRoll`。
- 旧文案：设置说明和阶段提示中的“延迟停止”相关内容。

当前 F004-03 已替换旧逻辑和旧文案接入点：

- `BeginShakeRoll` 中起摇日志使用“摇骰盅开始，敲空格加力。”。
- 设置页操作说明改为加力窗口和显示结果提示，不再描述延迟停止。
- `StagePrepFocusText` 中小关准备关注点改为“敲 Space 加力”和“显示结果”。
- 底部动作提示改为“敲 Space 加力”和“显示结果”。
- `ShakeStartTime / ShakeTapBonus / ShakeMaxTime / StopDuration` 已不再作为投骰表现主逻辑来源。
- 骰盅绘制中回落百分比、横向振幅、纵向振幅、旋转振幅和频率读取本次投骰配置快照，避免重载中途改变当前动画。

建议新增运行时字段：

- 当前有效全局配置。
- 本次投骰配置快照。
- 加力窗口计时。
- 有效冲量计数。
- 上次有效输入时间。
- 超时提示计时。

## 数据影响

建议新增默认配置：

```text
Assets/Resources/Data/roll_feedback_config.csv
```

首版最小实现要求：

- 默认配置从 `Resources.Load<TextAsset>("Data/roll_feedback_config")` 读取。
- 外部调试覆盖至少支持 `Application.persistentDataPath/roll_feedback_config.csv`。
- 启动和手动重载时，日志需要显示外部覆盖文件路径、读取结果和失败原因。
- 外部配置缺失、字段非法或数值越界时，使用上一次有效配置或代码安全默认值。
- `Application.streamingAssetsPath/Data/roll_feedback_config.csv` 可以作为额外覆盖源，但不是首版硬依赖。

建议字段：

```text
key,value,comment
start_response_time,0.15,起摇响应时间
input_window_duration,1.35,加力窗口时长
input_debounce,0.06,输入去抖
max_impulse_count,8,有效冲量上限
base_power,0.45,起摇基础强度
impulse_power,0.18,单次冲量强度
max_power,1.75,最大表现强度
power_decay_per_second,0.22,摇晃阶段自然衰减
stop_duration,0.45,回落时长
stop_min_power,0.02,回落最低强度
expired_prompt_duration,0.12,超时提示闪烁时长
cup_x_amplitude,32,骰盅横向振幅
cup_y_amplitude,7,骰盅纵向振幅
cup_rotation_amplitude,6,骰盅旋转振幅
cup_frequency,20,骰盅晃动频率
table_shake_amplitude,2,桌面轻震幅度
table_shake_duration,0.08,桌面轻震时长
prompt_pulse_scale,1.04,提示条脉冲缩放
prompt_pulse_duration,0.10,提示条脉冲时长
tap_sound_volume,0.65,有效敲击音量
expired_tap_volume,0.18,超时敲击音量
```

字段边界：

- `table_shake_amplitude` 和 `table_shake_duration` 首版必须可解析、钳制和保留，但实际桌面轻震表现可以不接入。
- `tap_sound_volume` 和 `expired_tap_volume` 首版必须可解析、钳制和保留，但实际音效资源和播放链路可以不接入。
- 未接入的可选表现不得影响 F004-01 到 F004-03 的程序验收。

调试覆盖建议读取优先级：

1. `Application.persistentDataPath` 下的 `roll_feedback_config.csv`。
2. `Resources.Load<TextAsset>("Data/roll_feedback_config")`。
3. 代码安全默认值。

如要额外支持 `Application.streamingAssetsPath/Data/roll_feedback_config.csv`，应插入在外部调试覆盖和 `Resources` 默认配置之间。实际实现可先使用较小范围，但必须满足运行时可重载 `Application.persistentDataPath/roll_feedback_config.csv` 修改后的配置。

## F004-01 实现记录

2026-06-11 已完成配置闭环首切：

- 新增 `RollFeedbackConfig` 结构，包含建议字段、安全默认值、范围钳制和克隆快照。
- 新增 `Assets/Resources/Data/roll_feedback_config.csv` 默认配置。
- 启动时读取 `Resources.Load<TextAsset>("Data/roll_feedback_config")`。
- 支持 `Application.persistentDataPath/roll_feedback_config.csv` 外部覆盖；启动和手动重载日志会输出覆盖路径。
- 支持运行中按 `F5` 手动重载；重载失败时保留上一次有效配置。
- `BeginShakeRoll` 会捕获本次投骰配置快照，后续重载不会改动该快照。
- 本切片未改投骰动画手感、旧延迟停止状态机或旧提示文案；这些仍属于 F004-02 和 F004-03。
- 本切片新增代码不调用 `UnityEngine.Random`，不会新增随机消耗。当前原型原本会在 `BeginShakeRoll` 为猪猪、大树和赌徒分配回合目标，这不属于本切片新增随机。

## F004-02 实现记录

2026-06-11 已完成投骰状态机首切：

- `BeginShakeRoll` 使用本次配置快照的 `input_window_duration` 作为固定加力窗口，不再使用旧的可延长摇晃时间。
- `BeginShakeRoll` 使用本次配置快照的 `base_power` 作为起摇基础强度。
- `UpdateShakeRoll` 中有效 `Space` 只在窗口内按 `input_debounce` 去抖，并受 `max_impulse_count` 限制。
- 每次有效敲击只按 `impulse_power` 提升表现强度，并受 `max_power` 上限约束；不改变窗口时长。
- 摇晃阶段强度按 `power_decay_per_second` 自然衰减，最低保持本次快照的 `base_power`。
- 加力窗口结束后进入 `Stopping`，不再通过继续敲击延长摇晃时间。
- `UpdateStopRoll` 使用本次配置快照的 `stop_duration` 和 `stop_min_power` 回落。
- 回落阶段额外 `Space` 只写入 `expired_prompt_duration` 计时，不改变 `shakePower`、`stopTimer`、窗口时长或骰面结果；可见提示留给 F004-03 接入。
- F004-02 新增代码不调用 `UnityEngine.Random`，敲击次数、敲击时机、配置值和表现强度不进入骰面随机、出千、分数或结算。

## F004-03 实现记录

2026-06-11 已完成界面和表现反馈首切：

- 设置页操作说明、阶段提示、投掷区关注点和底部主动作提示已替换为加力窗口和“显示结果”流程文案。
- 新增 `promptPulseTimer`，有效加力敲击按 `prompt_pulse_duration` 驱动阶段提示条轻脉冲。
- 加力窗口结束和回落阶段额外 `Space` 使用 `expired_prompt_duration` 驱动轻量“显示结果”反馈，不改变回落计时、表现强度或结果。
- `DrawDiceCup` 使用本次配置快照读取 `cup_x_amplitude`、`cup_y_amplitude`、`cup_rotation_amplitude` 和 `cup_frequency`，并保留 `stop_duration` 驱动回落百分比。
- F004-03 新增提示和骰盅表现代码不调用 `UnityEngine.Random`，不改骰面随机、出千、分数、结算或存档。
- 音效、桌面轻震和设置页重载按钮仍按生产包约定后置，不阻塞 F004-03。

## 存档影响

无存档格式变化。投骰表现配置不写入 `PlayerPrefs`，不改变 `DiceData`。

## 界面影响

- 底部阶段提示和设置页操作说明已替换旧文案。
- 加力窗口内提示条会按有效敲击轻脉冲。
- 回落阶段显示“显示结果”，额外 `Space` 只触发极轻提示。
- 调试重载日志保持短而清晰。
- 不新增教程墙。

## 测试 / 验证计划

- 静态检查：搜索旧常量和旧文案，确认主逻辑不再依赖旧硬编码。
- 配置检查：删除配置、写入非法值、写入极端值，确认有范围钳制和回退。
- 重载检查：修改覆盖配置后按 `F5`，下一次投骰使用新参数。
- 当前投骰快照检查：摇晃中重载配置，本次动画不跳变。
- 随机隔离代码审查：确认敲击输入、提示反馈、配置加载和重载不会在 `FinishShakeRoll` 前消耗 `UnityEngine.Random`。
- 随机隔离调试验证：如果当前实现没有正式确定性种子系统，使用临时 `Random.state` 快照、调试钩子或等效日志验证不同敲击次数和不同表现配置不改变骰面结果。
- 人工体验检查：起摇、连打、窗口结束、回落、结果决策顺序清楚。
- Unity 验证：有 Unity Play Mode 时运行主流程，检查投骰、出千、结算和下一次投骰均正常。

## 文档同步

| 文档 | 是否需要更新 | 已确认来源 |
|---|---|---|
| PROJECT_CONTEXT.md | 实现后需要核对 | F004 设计对话、FEATURE_BRIEF.md |
| GAME_FLOW.md | 实现后需要核对 | F004 设计对话、FEATURE_BRIEF.md |
| DICE_ARCHETYPES.md | 不需要 | 本包不涉及骰子效果 |
| ART_ASSETS.md | 只有新增资源时需要 | ART_BRIEF.md |
| ACCEPTANCE.md | 需要更新验证结果 | 实现和验收 |

## 阻塞项

无硬阻塞。音效、桌面轻震和设置页按钮均可后置。
