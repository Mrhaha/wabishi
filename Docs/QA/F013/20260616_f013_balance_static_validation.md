# F013 无词缀数值基线静态验证记录

日期：2026-06-16
范围：F013-03
状态：静态检查通过，Unity Play Mode 未执行

## 本轮改动

- `Assets/Resources/Data/global.csv`：正式基线改为 `starting_gold = 8`、`stage_clear_base_gold = 3`、利息每 `6` 金 `+1` 且封顶 `4`，并将 `market_test_random_refresh` 设为 `0`。
- `Assets/Resources/Data/chapter_score_table.csv`：接入 40 关平滑梯度版无词缀目标曲线；前三章为第一章 `60 / 74 / 70 / 70`，第二章 `78 / 70 / 86 / 82`，第三章 `100 / 88 / 82 / 94`；第三章后平滑递增，第四章为 `104 / 115 / 127 / 140`，第十章为 `1141 / 1261 / 1393 / 1539`。
- `Assets/Resources/Data/dice_market_config.csv`：重调买价、卖价、阶级和章节段权重。
- `Assets/Resources/Data/market_rule_config.csv`：刷新费用改为 `1-2` 章 `1 / 2 / 2`，`3-5` 章 `2 / 3 / 2`，`6-10` 章 `3 / 4 / 2`。
- `Assets/Resources/Data/dice_material_config.csv`：重调材质价格和章节段权重。
- 主文档和 F013 生产包已同步为“首轮候选已静态接入，整体运行待验证”。

## 基线判断

- 粗模拟 50000 次纯基础 6 骰、3 次出手、不计出千时，平均小关累计分约 `95.71`。
- 同一粗模拟下，目标 `72` 的通过率约 `81.1%`，目标 `96` 的通过率约 `45.0%`，目标 `120` 的通过率约 `17.2%`。
- 粗模拟 12000 次规则均值：`None` 约 `95.84`，`OddLedger` 约 `109.14`，`HandAudit` 约 `101.75`，`LowFog` 约 `86.71`，`DoubleJudge` 约 `98.31`。
- 以上只用于解释首轮曲线，不等于实机平衡验收；实机仍需要出千、市场买卖、材质、F008、F010、F011 和玩家决策样本。

## 已执行检查

- `git diff --check`：通过，无空白错误。
- `Import-Csv Assets\Resources\Data\*.csv`：全部可解析。
- 数据行数：
  - `global.csv`：30 行。
  - `chapter_score_table.csv`：40 行。
  - `dice_market_config.csv`：43 行。
  - `market_rule_config.csv`：3 行。
  - `dice_material_config.csv`：5 行。
- 关键值断言：
  - `starting_gold = 8`。
  - `stage_clear_base_gold = 3`。
  - `interest_gold_step = 6`。
  - `interest_gold_cap = 4`。
  - `market_test_random_refresh = 0`。
  - `affix_feature_enabled = 0`。
  - 第一章目标为 `60 / 74 / 70 / 70`。
  - 第二章目标为 `78 / 70 / 86 / 82`。
  - 第三章目标为 `100 / 88 / 82 / 94`。
  - 第四章目标为 `104 / 115 / 127 / 140`。
  - 第十章目标为 `1141 / 1261 / 1393 / 1539`。
  - 40 个 `target_score` 均大于 0。
- 文档旧事实扫描：未再发现旧曲线 `72 / 84 / 99 / 115`、旧终局 `2053 / 2402 / 2813 / 3285`、旧经济“正式候选尚未生成”等过期表述。

## 未执行检查

- 未执行 Unity Play Mode。
- 未截图验证 `1280x720` / `1920x1080` 市场、结果决策和结算界面。
- 未用构造旧存档实机验证带词缀骰和三类道具数量。
- 未实机跑前 3 章基础通关压力、第一章可购买骰子数量、F008/F010/F011 关键骰强度样本。

## 工具缺口

- 当前 PATH 未找到 `dotnet`、`csc`、`Unity` 或 `Unity.exe` 命令，无法在本轮命令行完成 C# 编译或 Unity 运行验证。

## 结论

F013-03 无词缀数值基线首轮候选已完成静态接入。当前可以进入 Unity Play Mode 和前 3 章实机样本验证；如果样本显示第 1 章过紧、金币雪球过强或 F011 成长过猛，再进行第二轮 CSV 调参。
