# F006 普通通关异常光晕修复运行验证

日期：2026-07-23  
范围：F006-V7R3-FB-001  
环境：Unity 2019.4.33f1，隔离工程，同步当前 `Scripts / Resources/Data / Resources/Config`，`1920×1080`，真实结算展示事件  
结论：代码修复与自动化运行回归通过；用户在当前工程中的最终体验回归仍待完成。

## 修复内容

- `TargetSettle` 激活时统一清空上一笔贡献留下的达标十字脉冲和积分塔扫描余辉，并把余辉权重归零。
- Pass 不再在目标图标上绘制青白漫射圆光；闭环反馈改为积分塔目标线附近的暖色硬边接触。
- Pass 原有全高暖白亮丝改为目标线附近 `14 px` 的短接触闪光。
- Pass 完成残留由通用 `0.52s` 塔内填充改为最多 `0.18s` 的低强度边框回声；其它档位继续使用原残留规则。
- 未修改五档阈值、Pass `1.55s` 总时长、`0.56` 结果揭示点、音效身份、真实计分、奖励、事件顺序、数据或存档。

## 自动化回归

专项捕获脚本使用真实 `BeginSettle()`、真实展示事件队列和真实 `TargetSettle`，结果状态文件为：

- `20260723_f006_pass_glow_fix_regression_status.txt`

最终同版复跑日志没有 C# 编译错误、旧术语表格式错误或捕获脚本失败。

| 验证点 | 场景 | 结果 |
|---|---|---|
| 档位起点确定性 | 提前达标 `24 / 20`、最后一枚刚好达标 `20 / 20`、贿赂 `2 金 → +8 分` 补到 `20 / 20` | 三条路径进入 Pass 时 `mainGameTargetCrossStartedAt` 与 `settlementTowerAfterglowStartedAt` 均已失效，余辉权重为 `0` |
| Pass 时间合同 | 三条路径及 `1.0x / 1.5x / 2.0x` | 隐藏档位均为 Pass；基础时长保持 `1.55s`，显示时长继续按统一倍率压缩 |
| 结果揭示 | Pass 终局 | `0.55` 尚未揭示，`0.56` 开始揭示 |
| 贿赂路径 | 六骰基础分 `12`，贿赂花费 `2` 金 | 生成真实 `BribeFinal`，补分 `8`，最终分 `20` |
| 残留收口 | StageClear 完成后 `0.06s / 0.22s` | `0.06s` 只保留微弱边框回声；`0.22s` 已完全结束，无塔内琥珀填充 |
| 其它档位 | Miss / Exceed / FarExceed / Critical，双分辨率 | 五档分类、五档时长与最高档真空 / 击穿 / 余震捕获全部通过 |
| 降效设置 | Pass `0.34`，减少闪烁 / 镜头运动开启 | 档位、时间合同与单焦点结构不变 |

五档复跑状态：

- `20260723_f009_settlement_finale_runtime_status.txt`
- `PASS: five hidden result tiers and the critical vacuum / breakthrough / aftershock phases were classified, timed and captured at 1920x1080 and 1280x720.`

## 视觉证据

- `20260723_f006_pass_glow_early_p034_contact_1920x1080.png`：提前达标的闭环接触。
- `20260723_f006_pass_glow_lasthit_p012_1920x1080.png`：最后一枚达标进入 Pass 初段，无旧十字或塔扫描。
- `20260723_f006_pass_glow_lasthit_p034_contact_1920x1080.png`：最后一枚达标的暖色硬边接触。
- `20260723_f006_pass_glow_bribe_p034_contact_1920x1080.png`：贿赂补足后的同一闭环接触。
- `20260723_f006_pass_glow_stageclear_p006_border_echo_1920x1080.png`：完成后 `0.06s` 的边框回声。
- `20260723_f006_pass_glow_stageclear_p022_clear_1920x1080.png`：完成后 `0.22s`，残留已结束。

与修复前 `20260723_f006_pass_glow_finale_p034_latch_1920x1080.png` 对比，修复后不再出现目标图标上的青白横光 / 圆光，也不再出现贯穿积分塔的暖白竖柱。

## 验收边界

- 本轮自动回归可以关闭 F006-V7R3-FB-001 的程序与画面结构问题。
- 用户仍需在当前工程用正常构筑确认普通 Pass 的实际观感，以及完整整手声音、连续触发耐受和降闪设置；在该回归前不把 V7-R3 标记为最终体验验收完成。
