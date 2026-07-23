# F009 / F006 五档结算终局运行验证

日期：2026-07-23  
运行环境：Unity 2019.4.33f1，隔离工程 Play Mode  
实现来源：`Assets/Scripts/DiceKingDemo.cs`、`Assets/Scripts/MainGameFlowPresentationProfile.cs`  
自动捕获入口：`Assets/Editor/WabishF009SettlementFinaleCapture.cs`

## 本轮结论

- Unity 编译通过，运行时与 Editor 脚本均为 `0 error`。
- 真实 `TargetSettle` 路径已自动覆盖隐藏五档：未达到、过关、超过、远超过、最高暴击。
- 五档终局时长分别为 `1.70s / 1.55s / 1.65s / 1.85s / 2.20s`；逐骰贡献仍由既有 `1.0x` 时标驱动，设置页 `1.5x / 2.0x` 同步压缩视觉与程序音效。
- 最高暴击的 `2.20s` 终局保留独占结构：锁死、吸光、近黑真空、单次击穿、余震；结果文案在击穿后才揭示。
- 前四档分别使用反向泄压、单拍闭环、双拍继电、可控整机过载，没有使用最高档专属的近黑真空和全屏击穿。
- `1920×1080` 与 `1280×720` 共 14 张真实运行截帧均成功生成；另生成 7 张 `0.606×` 压力缩放图。目检未发现主骰、积分塔、右侧终端或底部阶段牌裁切。
- 本轮只改展示层、表现配置与程序生成音效；没有改 `ScoreDice()`、真实得分、奖励、随机、金币、成长、数据 CSV、存档字段或存档版本。

自动化验证已通过，不等于用户对整手动态、扬声器声压与重复耐受的最终主观验收。

## 五档运行证据

| 档位 | 运行语义 | 1920×1080 | 1280×720 | 0.606× |
|---|---|---|---|---|
| 未达到 | 反向泄压 | `20260723_f009_settlement_finale_miss_reverse_release_1920x1080.png` | `20260723_f009_settlement_finale_miss_reverse_release_1280x720.png` | `20260723_f009_settlement_finale_miss_reverse_release_0606.png` |
| 过关 | 单拍闭环 | `20260723_f009_settlement_finale_pass_single_latch_1920x1080.png` | `20260723_f009_settlement_finale_pass_single_latch_1280x720.png` | `20260723_f009_settlement_finale_pass_single_latch_0606.png` |
| 超过 | 双拍继电 | `20260723_f009_settlement_finale_exceed_double_relay_1920x1080.png` | `20260723_f009_settlement_finale_exceed_double_relay_1280x720.png` | `20260723_f009_settlement_finale_exceed_double_relay_0606.png` |
| 远超过 | 可控整机过载 | `20260723_f009_settlement_finale_farexceed_cabinet_overload_1920x1080.png` | `20260723_f009_settlement_finale_farexceed_cabinet_overload_1280x720.png` | `20260723_f009_settlement_finale_farexceed_cabinet_overload_0606.png` |
| 最高暴击 | 真空前态 | `20260723_f009_settlement_finale_critical_vacuum_1920x1080.png` | `20260723_f009_settlement_finale_critical_vacuum_1280x720.png` | `20260723_f009_settlement_finale_critical_vacuum_0606.png` |
| 最高暴击 | 单次击穿 | `20260723_f009_settlement_finale_critical_breakthrough_1920x1080.png` | `20260723_f009_settlement_finale_critical_breakthrough_1280x720.png` | `20260723_f009_settlement_finale_critical_breakthrough_0606.png` |
| 最高暴击 | 余震回亮 | `20260723_f009_settlement_finale_critical_aftershock_1920x1080.png` | `20260723_f009_settlement_finale_critical_aftershock_1280x720.png` | `20260723_f009_settlement_finale_critical_aftershock_0606.png` |

## 运行门禁

| 门禁 | 结果 | 证据 |
|---|---|---|
| Unity 运行时脚本编译 | 通过 | `20260723_f009_settlement_finale_compile.log` |
| Unity Editor 捕获脚本编译 | 通过 | `20260723_f009_settlement_finale_compile.log` |
| 五档实际分类 | 通过 | 捕获器以目标分 `20` 和六颗固定基础骰构造五档，并校验实际 `FeedbackTier` |
| 档位终局时长 | 通过 | 捕获器校验五档事件时长与表现配置一致 |
| 最高档三关键相位 | 通过 | 真空、击穿、余震在两个分辨率分别捕获 |
| 双分辨率布局 | 通过 | 14 张运行截图已目检，无关键区裁切 |
| `0.606×` 压力观察 | 通过 | 7 张缩放图已生成，主要文字和动作锚点仍可辨认 |
| 真实计分 / 奖励 / 存档不变 | 静态通过 | 终局导演只读取已提交的 `SettlementDisplayEvent`；未新增 `ScoreDice()` 调用或存档字段 |

状态文件：`20260723_f009_settlement_finale_runtime_status.txt`。  
原始运行日志：`20260723_f009_settlement_finale_capture.log`。

## 仍需用户实机回归

- 用正常构筑完整打一手，确认逐骰入账到最高档 `2.20s` 终局的总节奏仍具有“压抑后突然击中”的爽点。
- 开启声音确认真空前声场下潜、击穿瞬间和余震尾音在实际扬声器上不会糊成连续噪声。
- 连续触发低档与高档，确认前四档不过度拖慢、最高档仍稀有且具有质变。
- 打开“减少闪烁 / 减少镜头运动”后，确认信息与声音身份保留，同时白闪、CRT 扰动、射线和整机后坐明显减弱。
