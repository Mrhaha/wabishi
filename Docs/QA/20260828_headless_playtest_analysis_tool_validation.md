# 2026-08-28 后台试玩分析工具验证

## 结论

- Unity 编辑器入口、隔离工程启动器、无窗口真实规则运行器、逐步决策日志、单局 CSV、汇总报告、`[CHAIN]` 日志和日志打开入口已接入。
- Unity 2019.4.33f1 在主工程编辑器保持打开时，成功从系统临时目录启动第二个隔离工程；后台进程使用 `-batchmode -nographics` 与隐藏窗口，当前桌面未出现第二个 Unity 或游戏窗口。
- 首次隔离工程全量导入完成；同一隔离工程增量运行可直接复用 `Library`。
- 最终隐藏进程冒烟退出码为 `0`，Unity 日志没有 `error CS`、`Compilation failed`、脚本编译错误、反射缺失或运行异常。

## 验证范围

### 编译与进程

- Unity 路径：`F:/unity/Unity20190433_release/Editor/Unity.exe`。
- 参数：`-batchmode -nographics -executeMethod WabishHeadlessPlaytestRunner.RunFromCommandLine`。
- 主工程 Unity 进程在整个验证期间保持运行；隔离工程没有触发同工程多实例锁。
- 最终使用 PowerShell `Start-Process -WindowStyle Hidden -PassThru -Wait` 验证与编辑器启动器相同的隐藏进程语义，退出码 `0`。
- 设置普通日志与警告不附带调用堆栈后，单种子五策略的后台 Unity 日志约为 `0.8 MB`；真实 `[CHAIN]` 仍完整保留在独立日志。

### 玩法路径

最终冒烟使用 `1` 个固定种子、`5` 种策略，共 `5` 局，覆盖：

- 开始默认六骰。
- 投骰、锁点、真实 `ScoreDice` 与立即完成表现层。
- 达标、通关收入和进入市场。
- 市场购买、卖出、刷新和离开。
- 法术购买、市场施法、投掷前施法与目标选择。
- 骰袋排序。
- 左到右离场队列。
- 未达标扣命、累计续投与生命归零。
- `[CHAIN]` 真实规则日志。

最终样本结果：

- 逐步决策 `377` 条，全部为有效 JSONL。
- 单局汇总 `5` 行，CSV 可正常解析。
- 运行异常 `0`。
- 动作上限 `0`。
- 连锁警告 `0`。

另一次固定种子冒烟生成 `339` 条决策，实际覆盖了冲关增幅、连锁分红、定向换货和精密校准，以及购买、卖出、刷新、排序、失败续投和离场。

## 存档隔离

- 后台运行在 `StartDefaultBuild()` 前设置 `suppressSave = true`。
- `SaveRun()` 已有抑制分支。
- 本次补齐 `ClearSave()` 的同一抑制分支；生命归零和最终通关不会删除正式 `PlayerPrefs`。
- 隔离测试不修改正式玩法数值、随机规则、存档版本或市场配置。

## 输出合同

正式入口输出到：

`Docs/QA/Playtests/yyyyMMdd/`

命名前缀：

`yyyyMMdd_HHmmss`

同秒重复时由启动器检查现有同前缀文件，并依次追加 `_2 / _3 / ...`。

编辑器窗口保存当前输出前缀与后台进程 ID；运行完成后，“打开本次 / 最近分析日志”按钮直接打开对应 `*_playtest_summary.md`，另有日志目录和后台 Unity 日志入口。

## 未验证范围

- 本轮没有在用户当前主编辑器中人工点击新窗口；编辑器窗口与启动器已随同隔离工程完成脚本编译，真实按钮点击仍保留为首次使用冒烟。
- 没有执行默认 `50 × 5 = 250` 局完整样本，只验证单种子五策略的完整控制流。
- 无窗口运行不验证动画、声音、鼠标手感、文字可读性或主观爽感。

## 相关文件

- `Assets/Editor/WabishPlaytestAnalysisWindow.cs`
- `Assets/Editor/WabishHeadlessPlaytestRunner.cs`
- `Assets/Scripts/DiceKingDemo.cs`
- `Docs/QA/Playtests/README.md`
