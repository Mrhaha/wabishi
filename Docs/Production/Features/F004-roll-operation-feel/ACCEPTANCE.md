# F004 验收记录

状态：F004-03 已实现待 Unity 验证
功能：F004 投骰操作手感

## 就绪度门禁

| 角色 | 就绪度 | 缺口 / 风险 | 建议补丁 | 阻塞问题 |
|---|---|---|---|---|
| 策划 | 就绪 | 建议默认值可作为首版默认，后续通过配置微调 | 已通过 `$wabish-packet-review` 写回补丁 | 无 |
| 美术 | 就绪 | 首版无新增必需资源；音效后置 | 复用现有骰盅资源 | 无 |
| 界面体验 | 就绪 | 设置页按钮和音效提示可后置 | 首版只做阶段提示和超时轻反馈 | 无 |
| 程序 | 就绪 | 随机隔离需要实现后验证 | 先做配置闭环和快照策略 | 无 |

## 验收清单

- [x] 生产包已从 F004 设计对话生成。
- [x] 已记录起摇、加力窗口、表现冲量、回落显示结果的规则。
- [x] 已记录配置化和运行时重载要求。
- [x] 已记录随机隔离要求。
- [x] 已拆成配置闭环、投骰状态机、界面表现、验证同步四个执行切片。
- [x] 已完成 `$wabish-packet-review` 审查。
- [x] 已按审查建议写回生产包补丁。
- [x] 已新增投骰表现配置和默认配置文件。
- [x] 已实现运行时重载。
- [x] 已实现本次投骰配置快照。
- [x] 已将旧的延迟停止逻辑替换为固定加力窗口。
- [x] 已实现窗口结束后的轻量“显示结果”反馈。
- [x] 已更新主流程提示、日志和设置说明旧文案。
- [x] 已通过静态检查确认配置缺失或非法时会保留上一次有效配置或使用安全默认值。
- [x] 已通过代码审查确认正在投骰时重载配置只替换全局配置，不改动本次投骰配置快照。
- [x] 已通过代码审查确认 F004-01 / F004-02 / F004-03 新增的配置读取、重载、状态机敲击处理、提示反馈和骰盅表现不会消耗 `UnityEngine.Random`。
- [ ] 如果实现环境提供确定性种子、`Random.state` 快照或调试钩子，已验证不同敲击次数不改变骰面结果。
- [ ] 如果实现环境提供确定性种子、`Random.state` 快照或调试钩子，已验证不同表现配置不改变骰面结果。
- [ ] 已完成 Unity Play Mode 或等效运行验证。
- [x] 已同步 `PROJECT_CONTEXT.md` 和 `GAME_FLOW.md` 的实现事实。

## 职能验证记录

2026-06-11：

- 用户要求使用 `$wabish-production-pipeline` 基于 `Docs/DesignDialogues/F004-roll-operation-feel.md` 生成生产包。
- 已创建 F004 生产包首版。
- 当前未改运行时代码、数据或资源。
- 已完成 `$wabish-packet-review` 执行审查。
- 用户批准按 `EXECUTION_REVIEW.md` 的“建议写回生产包的补丁”批量更新生产包文件，且明确不改代码。
- 已写回配置字段边界、运行时重载来源、随机隔离验收口径、旧文案接入点和配置闭环验证拆分。
- 已执行 `$wabish-dev-implementation` 的 F004-01 配置闭环。
- 已新增 `RollFeedbackConfig`、默认 `roll_feedback_config.csv`、`Application.persistentDataPath/roll_feedback_config.csv` 覆盖读取、`F5` 手动重载和 `BeginShakeRoll` 配置快照。
- 已确认 F004-01 不改投骰动画手感、不替换旧延迟停止逻辑、不改骰面随机、出千、分数、结算或存档。
- 已同步 Notion 状态页：`https://app.notion.com/p/37cf9be4dcfd81c58b6aef19f408f341`。
- 已执行 `$wabish-dev-implementation` 的 F004-02 投骰状态机。
- 已将旧的延迟停止逻辑替换为固定加力窗口、输入去抖、有效冲量上限、窗口结束进入回落和快照驱动回落时长。
- 已确认 F004-02 不新增随机消耗，不改变出千、分数、结算或存档。
- 已执行 `$wabish-dev-implementation` 的 F004-03 界面和表现反馈。
- 已替换运行时代码中的旧“延迟停止”文案，阶段提示、设置说明和底部主动作提示改为加力窗口和“显示结果”流程。
- 已接入有效敲击提示脉冲、窗口结束后的“显示结果”轻提示，以及配置快照驱动的骰盅横移、纵移、旋转和频率。
- 已确认 F004-03 不新增随机消耗，不改变出千、分数、结算或存档。
- 已同步 Notion 状态页：`https://app.notion.com/p/37cf9be4dcfd81c58b6aef19f408f341`。
- 当前包状态为 F004-03 已实现待 Unity 验证，推荐下一步执行 F004-04。

## 验证备注

- 创建生产包阶段不运行 Unity 验证。
- 创建生产包阶段不修改 `Assets/Scripts/DiceKingDemo.cs`，因此不能声明 F004 已实现。
- 主文档在设计对话阶段已经同步过确认规则；实现后仍需核对文档与代码一致。
- F004-01 阶段已完成静态检查：`roll_feedback_config.csv` 共 21 个数值字段且可解析；`git diff --check` 未发现空白错误。
- F004-02 阶段已完成代码审查：`UpdateShakeRoll` 和 `UpdateStopRoll` 不调用 `UnityEngine.Random`，敲击只改变表现状态字段。
- F004-03 阶段已完成静态检查：`Assets/Scripts/DiceKingDemo.cs` 中不再出现“延迟停止”旧文案。
- F004-03 阶段已完成代码审查：`UpdateShakeRoll`、`TryApplyShakeImpulse`、`BeginStopRoll`、`UpdateStopRoll`、`DrawPromptPulse` 和 `DrawDiceCup` 不调用 `UnityEngine.Random`。
- F004-03 阶段已完成配置检查：`roll_feedback_config.csv` 仍为 21 个可解析数值字段。
- F004-03 阶段已运行 `git diff --check -- Assets/Scripts/DiceKingDemo.cs`，只出现当前文件行尾转换提示，未发现空白错误。
- 当前环境没有可用的 `dotnet`、`csc` 或 Unity 命令行入口，因此尚未运行 Unity 编译或 Play Mode。
- 当前原型原本会在 `BeginShakeRoll` 为猪猪、大树和赌徒分配回合目标并消耗 `UnityEngine.Random`；F004-01 / F004-02 / F004-03 新增的配置读取、覆盖、重载、敲击状态机、提示反馈和骰盅表现没有新增随机消耗。

## 已知缺口

- Unity 编译和 Play Mode 尚未运行。
- 随机隔离尚未通过确定性种子、`Random.state` 快照或调试钩子做运行验证。
- 主流程 UI 尚未通过 Play Mode 或截图流程确认无遮挡、无溢出。
- 音效、桌面轻震和设置页重载按钮不作为首版必做；相关字段可先解析，实际表现可后置。

## 最终结论

F004-01 配置闭环、F004-02 投骰状态机和 F004-03 界面表现反馈已完成代码接入，但尚未运行 Unity 验证。下一步执行 F004-04，重点验证随机隔离、配置重载、主流程 UI 表现和 Unity Play Mode。
