# F001-B 程序交接

状态：已验收（用户确认）
功能：F001-B 龟龟骰规则细化
关联主文档：PROJECT_CONTEXT.md、GAME_FLOW.md、DICE_ARCHETYPES.md
实现事实来源：Assets/Scripts/DiceKingDemo.cs

## 输入决策

| 决策 | 状态 | 来源 | 程序影响 |
|---|---|---|---|
| 本切片不立即实现新增龟龟补强骰。 | 已确认 | FEATURE_BRIEF.md | 本轮不执行程序改动。 |
| 临时小骰不参与主牌型识别。 | 已确认 | GAME_FLOW.md、DICE_ARCHETYPES.md | 不需要改全局手牌表。 |
| 壳匠、巢穴、慢龟规则已确认。 | 已确认 | RULE_CARDS.md、用户确认 | 可以按首版规则进入实现准备。 |
| 三颗候选第一版不需要跨小关状态。 | 已确认 | RULE_CARDS.md、用户确认 | 存档只需要新增类型，不需要新状态字段。 |
| 临时小骰完整计分，显示超过 8 颗时折叠。 | 已确认 | UI_UX_SPEC.md、用户确认 | 计分和展示需要分离处理。 |
| 三颗最终图标已生成并登记。 | 已确认 | ART_ASSETS.md、ART_BRIEF.md | 程序可直接接入对应 `Resources.Load` key。 |

## 实现目标

用户确认后，程序需要把三颗龟龟补强骰接入当前单文件原型：新增类型、默认面组、市场配置、临时小骰链生成、预览期望、真实结算、界面标签、图标映射和文档同步。

## 已确认行为

- 当前 `DieType` 已支持新增类型 key 存档，旧数字序号兼容读取。
- 当前龟龟骰使用 `floor(value / 2)` 递减生成临时小骰。
- 当前龟龟预览使用期望估算，真实结算随机生成。
- 当前临时小骰追加到 `scoringDice`，只参与本次结算动画。

## 实现任务

| 任务 | 文件 / 数据 | 依赖 | 验收 | 状态 |
|---|---|---|---|---|
| 新增 `DieType` | Assets/Scripts/DiceKingDemo.cs | 已确认规则 | 追加 `Shellsmith / Nest / SlowTurtle`，旧类型 key 不变 | 已实现 |
| 抽出龟龟链生成逻辑 | Assets/Scripts/DiceKingDemo.cs | 慢龟规则已确认 | 普通龟龟和慢龟可共用真实结算与预览函数 | 已实现 |
| 实现壳匠计分 | Assets/Scripts/DiceKingDemo.cs | 壳匠规则已确认 | 按实际临时小骰数量给本骰 +1 单骰分 | 已实现 |
| 实现巢穴补骰 | Assets/Scripts/DiceKingDemo.cs | 巢穴规则已确认 | 按有产出链条和巢穴数量补 d1，不闭环 | 已实现 |
| 实现慢龟链 | Assets/Scripts/DiceKingDemo.cs | 慢龟规则已确认 | `ceil(value / 2)` 衰减，点数 1 停止 | 已实现 |
| 新增市场数据 | Assets/Resources/Data/dice_market_config.csv | 代码支持新类型 | 市场刷出新骰且价格不套利 | 已实现 |
| 新增图标映射 | Assets/Scripts/DiceKingDemo.cs | 图标资源已生成 | 三颗图标可加载，缺失时可回退 | 已实现 |
| 更新界面标签 | Assets/Scripts/DiceKingDemo.cs | UI_UX_SPEC.md 已确认 | 标签、预览、日志不溢出；超过 8 颗折叠显示 | 已实现，待界面实机确认 |
| 同步主文档 | DICE_ARCHETYPES.md、GAME_FLOW.md、PROJECT_CONTEXT.md | 已确认规则和实现完成 | 文档行为和代码一致 | 已同步 |

## 代码影响

- `DieType`：追加 `Shellsmith / Nest / SlowTurtle` 或项目一致的对应命名。
- `DefaultName`、`TypeName`、`RoundTag`、`MarketOfferHint`：新增三颗骰表达。
- 龟龟预览和结算：建议抽出通用函数，支持普通龟龟、慢龟、巢穴和壳匠数量统计。
- `ScoreOneDieValue`：壳匠作为本骰单骰分加成。
- `ScoreTemporaryTurtleDice`：需要记录本次龟龟系临时小骰实际数量、龟龟链产出数量、巢穴补骰数量。
- `PreviewTemporaryTurtleDiceScore`：需要额外估算临时小骰数量和壳匠预估加分。
- `DieTypeIconFileName`：新增三颗图标路径映射。

## 图标资源

| 骰子类型 | 运行路径 | Unity 加载 key |
|---|---|---|
| Shellsmith | `Assets/Resources/Art/DiceTypes/shellsmith_die_icon.png` | `Art/DiceTypes/shellsmith_die_icon` |
| Nest | `Assets/Resources/Art/DiceTypes/nest_die_icon.png` | `Art/DiceTypes/nest_die_icon` |
| SlowTurtle | `Assets/Resources/Art/DiceTypes/slow_turtle_die_icon.png` | `Art/DiceTypes/slow_turtle_die_icon` |

## 数据影响

市场行已写入 `Assets/Resources/Data/dice_market_config.csv`，后续仍可按实测调参：

| die_type | buy_price | sell_price | tier | weight_ch1_2 | weight_ch3_5 | weight_ch6_10 | 状态 |
|---|---:|---:|---|---:|---:|---:|---|
| Shellsmith | 10 | 5 | T2 | 5 | 8 | 8 | 待实测 |
| Nest | 12 | 5 | T3 | 2 | 5 | 7 | 待实测 |
| SlowTurtle | 11 | 5 | T2 | 4 | 7 | 8 | 待实测 |

可能需要新增或内置调参键：

| key | 建议值 | 作用 | 状态 |
|---|---:|---|---|
| shellsmith_score_per_temp_die | 1 | 每颗龟龟系临时小骰给壳匠的单骰加分 | 已确认 |
| nest_bonus_die_face | 1 | 巢穴补出的临时小骰点数 | 已确认 |
| temp_turtle_display_fold_threshold | 8 | 投掷区完整显示的临时小骰数量阈值，超过后折叠 | 已确认 |
| slow_turtle_decay_mode | 1 | 慢龟使用向上取半衰减；可作为代码常量 | 已确认 |

## 存档影响

- 三颗候选第一版不需要 per-die 持久字段。
- 新增类型应使用当前类型 key 存档路径。
- 临时小骰、巢穴补骰、壳匠本次数量和慢龟链都属于结算临时状态，不写入 `DiceData`。

## 界面影响

- 需要新增三颗龟龟补强骰市场短提示。
- 需要在结果预览显示小龟预估数量和壳匠预估加分。
- 需要在结算日志中区分普通龟龟、慢龟和巢穴补骰。
- 需要防止临时小骰过多导致投掷区显示拥挤；超过 8 颗时折叠显示，但计分和日志汇总覆盖全部临时小骰。

## 测试 / 验证计划

- 编译通过。
- 旧存档可继续读取。
- 新龟龟补强骰能保存和继续游戏恢复。
- 壳匠按实际临时小骰数量加分。
- 巢穴补骰数量不超过巢穴骰数量，补骰不触发巢穴闭环。
- 慢龟掷出 5 时第一层最大点数为 3，掷出 1 时不生成临时小骰。
- 临时小骰超过 8 颗时折叠显示，但完整计入单骰分。
- 临时小骰不参与实体骰主牌型识别。
- 出千后龟龟相关预览刷新。
- 1280x720 和 1920x1080 下标签不溢出。

## 文档同步

| 文档 | 是否需要更新 | 已确认来源 |
|---|---|---|
| DICE_ARCHETYPES.md | 已按确认规则更新；实现后再核对代码事实 | F001-B 用户确认 |
| GAME_FLOW.md | 已按显示折叠边界更新；实现后再核对代码事实 | F001-B 用户确认 |
| PROJECT_CONTEXT.md | 已补充龟龟路线首版确认方向；实现后再核对代码事实 | F001-B 用户确认 |
| ART_ASSETS.md | 已更新 | 美术产物 |

## 实现结果

- 已在 `Assets/Scripts/DiceKingDemo.cs` 接入 `Shellsmith / Nest / SlowTurtle` 类型、默认名称、短标签、市场提示、颜色、图标映射和存档 key 读取。
- 已实现普通龟龟和慢龟的共用链式临时小骰生成；普通龟龟使用 `floor(v / 2)`，慢龟使用 `ceil(v / 2)` 且 `v <= 1` 停止。
- 已实现巢穴按有产出链条补 d1 临时小骰，不触发闭环。
- 已实现壳匠按本次龟龟系临时小骰总数给自身单骰加分。
- 已实现展示层折叠：结算层保留全部临时小骰，投掷区完整显示前 8 颗临时小骰，额外临时小骰折叠成摘要行。

## 验证状态

- 已完成静态检索：无旧函数签名残留。
- 已确认三张图标资源存在。
- 已确认 `dice_market_config.csv` 可由 `Import-Csv` 解析，并包含 `Shellsmith / Nest / SlowTurtle` 三行。
- 已完成代码大括号数量平衡检查。
- 用户已确认 F001-B 验收完毕。

## 阻塞项

- 无。
