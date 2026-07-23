# F006-V7R3-FB-003 普通结算积分塔边沿电流回归

日期：2026-07-23  
Unity：2019.4.33f1  
验证工程：隔离副本，不占用当前已打开的主 Unity Editor  
代码哈希：`71F1B4B6D2F2C3465EC9025DA016D4A89F714252694BCAE61D62D117E3AE4C59`

## 反馈与目标

原始反馈：

> 普通结算时会有积分塔上下类似电流流过边沿的效果对吧，现在有点淡，看到不是那么清楚

目标是提高普通 `SlotScore / BribeFinal` 入账后约 `0.46s` 塔框余辉的可追踪性，同时保持它低于 `TargetSettle` 五档终局，不恢复全高实心光柱，也不增加全屏闪光或整机负载。

## 实现

- 将原先的二次方快速衰减改为平滑衰减；持续时间仍读取既有 `SettlementTowerAfterglowDuration = 0.46s`，没有加长。
- 保留低强度整框余辉作为路径底色。
- 在左右边沿增加暖色外晕、主体、温白热核和四段短尾迹。
- 正贡献从下向上流动；负贡献继续反向向下。
- 左右边沿有约 `0.045` 归一化进度错位，避免读成静态发亮边框。
- 电流抵达端点时只形成一次短接帽，不扩展为整塔泛白。

## 普通结算专项捕获

捕获脚本：`Assets/Editor/WabishF006TowerEdgeCurrentCapture.cs`

- 使用真实 `StartDefaultBuild → ResultDecision → BeginSettle → SlotScore` 路径建立普通逐次结算。
- 固定局部冲击权重为 `0.34`，分别捕获余辉归一化进度 `0.16 / 0.46 / 0.76`。
- `1920×1080` 与 `1280×720` 六张捕获均成功。
- 运动序列可读为“塔底启动 → 沿左右边沿上行 → 塔顶附近衰减”。

代表证据：

- `Docs/QA/20260723_f006_tower_edge_current_motion_strip_1920x1080.png`
- `Docs/QA/20260723_f006_tower_edge_current_early_1920x1080.png`
- `Docs/QA/20260723_f006_tower_edge_current_mid_1920x1080.png`
- `Docs/QA/20260723_f006_tower_edge_current_late_1920x1080.png`
- `Docs/QA/20260723_f006_tower_edge_current_early_1280x720.png`
- `Docs/QA/20260723_f006_tower_edge_current_mid_1280x720.png`
- `Docs/QA/20260723_f006_tower_edge_current_late_1280x720.png`

## 可读性抽样

在中段电流头对应的左右边沿 ROI 进行 sRGB 感知亮度抽样：

| 分辨率 | 版本 | ROI 平均亮度 | 95 分位 | 热核高亮均值 | 最大值 |
|---|---|---:|---:|---:|---:|
| `1920×1080` | 修改前 | 8.98 | 28.84 | 38.27 | 40.99 |
| `1920×1080` | 修改后 | 27.09 | 89.35 | 124.34 | 130.84 |
| `1280×720` | 修改前 | 8.08 | 24.15 | 35.78 | 37.99 |
| `1280×720` | 修改后 | 24.64 | 88.78 | 122.74 | 129.91 |

结果表明两种目标分辨率的移动热核都从背景边框中稳定分离；亮度提升集中在短电流头与尾迹，没有形成全高实心色柱。

## 隔离回归

- 普通 `SlotScore` 双分辨率三相位：`PASS`。
- 五档 `TargetSettle` 分类、时长与关键相位双分辨率回归：`PASS`。
- Critical 真空 / 击穿 / 余震：`PASS`。
- 提前达标、最后一枚刚好达标、`BribeFinal` Pass：`PASS`。
- Pass `1.55s / 0.56` 揭示合同：`PASS`。
- `1.0x / 1.5x / 2.0x` 与降效模式：`PASS`。
- Pass 完成后不超过 `0.18s` 的边框回声：`PASS`。
- 编译日志未出现 C# 编译错误。

状态文件：

- `Docs/QA/20260723_f006_tower_edge_current_status.txt`
- `Docs/QA/20260723_f006_tower_edge_current_five_tier_regression_status.txt`
- `Docs/QA/20260723_f006_tower_edge_current_pass_bribe_regression_status.txt`

## 结论

F006-V7R3-FB-003 已实现并通过自动运行与截图回归。普通逐次结算的积分塔边沿电流已明显可见，且没有侵入 Pass 单焦点或五档终局强度。当前状态为“实现与自动验证通过，待用户在当前工程实机回归”，不代替最终主观体验验收。
