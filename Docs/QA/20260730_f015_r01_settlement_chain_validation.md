# F015-R01 分波结算与离场连锁验证记录

日期：2026-07-30  
状态：静态与编译通过，待 Play Mode 连锁日志验收

## 验证范围

- 主结算按阶段开始时锁定的骰子实例 ID 从左到右推进。
- 当前根骰自身结算完成后，响应按波次和当前骰袋位置执行；波内新响应只进入下一波。
- 同骰同事件去重、执行前条件复核与异常连锁安全上限。
- 离店锁定实例队列、多只龟顺序化壳、不回查，以及抢劫海盗队尾追加例外。
- 真实吸附后遗产龟、空舱龟、首领龟的跨类型位置排序。
- Unity Console / `Player.log` 的真实规则层结构化 `[CHAIN]` 日志。

## 实现结果

| 项目 | 结果 | 证据 |
|---|---|---|
| 主结算根顺序 | PASS（静态） | `ScoreDice()` 在进入根循环前快照当前骰子 ID；每个根调用 `EndGameplayChainRoot()` 清空全部响应波后才进入下一根。 |
| 波内 / 次波边界 | PASS（静态） | 非执行期响应进入 `PendingResponses`；当前波执行期间统一进入 `NextResponses`，波末再排序和执行。 |
| 响应排序 | PASS（静态） | 每波开始按响应骰当前 `DiceIndexById` 排序，同位置使用稳定入队序。 |
| 去重与失效跳过 | PASS（静态） | `eventId + responderId + responseType` 组合键去重；执行前调用响应自己的 `IsValid`，失效写 `response_skip`。 |
| 多波响应迁移 | PASS（静态） | 触发器 / 回声、贪吃、财宝、吞噬骰子 / 黑市小鬼、轮作，以及真实吸附响应均接入统一队列。 |
| 离场根顺序 | PASS（静态） | 点击离店时锁定 `marketLeaveRootDieIds`；游标不再依赖移除后变化的实时列表下标。 |
| 化壳边界 | PASS（静态） | 每只龟只在轮到时检查紧邻后一位；根结束前清空吸附响应；已跳过根不回查。 |
| 抢劫例外 | PASS（静态） | 抢到的新骰只在抢劫海盗分支明确追加当前离场队列末尾。 |
| 吸附响应跨类型排序 | PASS（静态） | 遗产龟、空舱龟、首领龟共享 `turtle_absorbed` 事件并由同一波排序，不再按类型分段调用。 |
| 结构化日志 | PASS（静态） | 生命周期覆盖 `chain_begin / root_begin / wave_scan / response_enqueue / response_execute / state_delta / response_skip / wave_end / root_end / chain_end`；字段包含连锁、序号、阶段、根、波、事件、来源、目标、结果、变化与原因。 |
| 故障保护 | PASS（静态） | 每条连锁最多执行 `256` 个响应动作；触发或异常时打印 `guard_abort` 和当前 / 下一波队列快照。 |
| 独立 C# 编译 | PASS | 使用 Unity 2019.4.33f1 对应 Roslyn 响应文件编译全部运行脚本，返回码 `0`、无 C# 错误；仅有既有常量分支的 `CS0162` 警告。 |
| Unity 隔离导入编译 | PASS | 最小隔离工程由 Unity 2019.4.33f1 批处理导入并生成 `Assembly-CSharp.dll`；日志无 `error CS`、`Compilation failed` 或 `Scripts have compiler errors`，包含 `Exiting batchmode successfully`。 |
| 补丁空白检查 | PASS | `git diff --check -- Assets/Scripts/DiceKingDemo.cs` 返回码 `0`。 |

Unity 编译日志：`Logs/F006_chain_isolated_unity_compile_20260730.log`。

## 待 Play Mode 验收

1. 构造第一波左侧响应产生第二波、第一波右侧仍待执行的场景，确认日志顺序为“第一波全部完成 → 第二波”。
2. 构造至少三波连锁，确认不会只扫第一遍，也不会由函数嵌套立即插队。
3. 构造“龟甲、宿主甲、龟乙、宿主乙”，确认两次化壳与各自响应完整分隔。
4. 构造“龟甲、龟乙、宿主”，确认龟甲先跳过、龟乙化壳，之后不回查龟甲。
5. 让遗产龟、空舱龟、首领龟交错排列，确认同一吸附波严格按当前骰袋位置执行。
6. 保存一份完整 `Player.log`，仅凭 `[CHAIN]` 行还原主根、波次、来源、响应者、状态变化和跳过原因。

## 结论

F015-R01 的代码结构、日志合同、独立 C# 编译与隔离 Unity 编译已通过。真实运行顺序与日志内容仍需上述 Play Mode 场景确认，因此本记录不把 F015 全包或 F015-R01 的最终玩法验收标记为完成。
