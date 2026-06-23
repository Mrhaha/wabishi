# F013 程序交接

状态：F013-03 首轮候选已静态接入，整体运行待验证
功能：F013 骰子重平衡与词缀临时隐藏
关联主文档：PROJECT_CONTEXT.md、GAME_FLOW.md、DICE_ARCHETYPES.md
实现事实来源：Assets/Scripts/DiceKingDemo.cs

## 输入决策

- F013 需要先软关闭词缀和改造道具，再做无词缀基线重平衡。
- 软关闭应保留 F005 存档、配置和资源兼容。
- 用户已于 2026-06-16 接受软关闭默认值：隐藏期间不显示也不生效，旧存档词缀和三类改造道具数量保留但暂不生效。
- `starting_gold = 1000` 是旧测试配置，不是正式平衡基准；F013 首轮候选已改为 `starting_gold = 8`。
- F013 执行前 `market_test_random_refresh = 1` 会因测试随机逻辑屏蔽改造道具，但那只是副作用，不能作为正式隐藏方案；当前已新增独立 `affix_feature_enabled` 软关闭开关。
- 用户后续要求市场随机刷新去除基础骰，并提高 F008 / F010 / F011 本轮重点骰的刷出概率，同时每次付费刷新尽量和上一批货架不同。

## 实现目标

1. 增加或等价实现词缀功能总开关，建议从 `global.csv` 读取 `affix_feature_enabled`。
2. 开关关闭时，市场不生成、不展示、不购买、不使用改造道具。
3. 开关关闭时，词缀不显示、不参与预览、不参与真实结算、不产生钱包收益。
4. 保留词缀和道具存档字段，不升级 `SaveVersion`，不清空旧数据。
5. 在无词缀状态下重调关卡目标、市场价格、刷新费用、经济参数和权重。

## 已确认行为

- 当前 F005 相关字段、CSV 和资源已经存在。
- 当前 `DiceData` 保存类型、面组、成长、材质、前缀和后缀。
- 当前三类改造道具持有数量通过 `PlayerPrefs` 保存。
- 当前 `affix_feature_enabled = 0` 时，正式市场和测试随机市场都不会生成改造道具。
- 当前 `affix_feature_enabled = 0` 时，词缀前缀不影响单骰分，后缀不影响钱包金币；开关恢复为 `1` 时才回到 F005 入口和效果。

## 实现任务

| 任务 | 文件 / 数据 | 依赖 | 验收 | 状态 |
|---|---|---|---|---|
| 增加词缀总开关 | Assets/Scripts/DiceKingDemo.cs、Assets/Resources/Data/global.csv | F013 D3-D5 | `affix_feature_enabled = 0` 时关闭全部玩家可见入口和效果 | 已静态接入 |
| 关闭市场改造道具 offer | DiceKingDemo.cs 的 `ShouldMakeCraftingItemOffer`、`MakeRandomMarketOffer`、`BuyOffer` | 总开关 | 正式市场和测试市场都不生成改造道具 | 已静态接入，待 Play Mode 验证 |
| 隐藏市场道具 UI | `DrawMarket`、`DrawCraftingInventoryPanel`、`DrawCraftingTargetPanel`、`DrawCraftingMarketOffer` | UI_UX_SPEC.md | 截图中无改造道具栏和目标选择 | 已静态接入，待截图 |
| 隐藏骰子词缀展示 | `DrawCompactDie`、`AffixSlotSummary`、`AffixOrRoundTag`、`AffixShortText` | UI_UX_SPEC.md | 骰袋和结果骰不显示词缀槽位或词缀标签 | 已静态接入，待截图 |
| 关闭词缀分数和金币效果 | `RefreshHandPreview`、`ScoreDice`、`AffixScoreBonusForDie`、`WalletIncomeForDie` | 总开关 | 旧存档带词缀时预览和真实结算都不加词缀收益 | 已静态接入，待旧存档验证 |
| 保留存档兼容 | `SerializeDice`、`ParseAffixes`、`LoadCraftingItemInventory`、`SaveRun` | F005 兼容 | 旧词缀字段和道具数量可读写但隐藏期不生效 | 已静态确认，未改存档结构 |
| 首轮重平衡数据 | global.csv、chapter_score_table.csv、dice_market_config.csv、market_rule_config.csv、dice_material_config.csv | F013 无词缀基线 | CSV 可解析，目标曲线和市场购买力有候选说明 | 已静态接入，待实机样本 |
| 市场刷新二次调整 | DiceKingDemo.cs、dice_market_config.csv | F013 无词缀基线 | 基础骰不作为随机商品；F008/F010/F011 开放后权重提高；付费刷新优先避开上一批 | 已静态接入，待 Play Mode 市场刷新样本 |
| 文档同步 | PROJECT_CONTEXT.md、GAME_FLOW.md、DICE_ARCHETYPES.md、ACCEPTANCE.md | 实现结果 | 主文档不再把词缀写成当前可见体验 | 已同步 |

## 代码影响

建议增加一个运行时字段，例如：

```text
affixFeatureEnabled
```

读取来源建议为 `global.csv` 的 `affix_feature_enabled`，缺失时默认 `false`，直到用户明确恢复 F005。

需要特别处理的代码入口：

- `LoadGlobalConfig` 或等价配置读取：读取开关。
- `EnterMarket`：关闭时清空 `activeCraftingItemKey`。
- `ShouldMakeCraftingItemOffer`：关闭时直接返回 `false`。
- `CanBuyMarketOffer`：关闭时拒绝 `CraftingItem`。
- `DrawMarket`：关闭时不绘制改造栏，不显示“仍可买改造道具”。
- `DrawCompactDie`：关闭时不要显示 `AffixSlotSummary`。
- `AffixOrRoundTag`：关闭时直接走 `RoundTag`。
- `AffixScoreBonusForDie`：关闭时返回 `0`。
- `WalletIncomeForDie`：关闭时跳过 `SuffixAffixes`。
- `RefreshHandPreview` 和真实 `ScoreDice`：关闭时 `previewAffixScoreBonus`、`lastAffixScoreBonus` 应保持 `0`。

## 数据影响

需要新增或调整：

| 文件 | 影响 | 备注 |
|---|---|---|
| Assets/Resources/Data/global.csv | 已接入 `starting_gold = 8`、`stage_clear_base_gold = 3`、利息 `6` 金一档封顶 `4`、`market_test_random_refresh = 0`、`affix_feature_enabled = 0` | 1000 金不再是活动基线 |
| Assets/Resources/Data/chapter_score_table.csv | 已接入平滑梯度版无词缀目标曲线；前三章为第一章 `60 / 74 / 70 / 70`、第二章 `78 / 70 / 86 / 82`、第三章 `100 / 88 / 82 / 94`，第四章从 `104` 起平滑递增，第十章为 `1141 / 1261 / 1393 / 1539` | 重点看前三章是否有梯度但不劝退，以及第三章到第四章是否不再断崖 |
| Assets/Resources/Data/dice_market_config.csv | 已重调价格、卖价、阶级和权重；`Basic` 权重为 `0/0/0`，F008/F010/F011 开放后权重二次提高 | F008/F010/F011 后需要统一看强度和刷出率 |
| Assets/Resources/Data/market_rule_config.csv | 已重调刷新费用和高阶保底：`1-2` 章 `1/2/2`，`3-5` 章 `2/3/2`，`6-10` 章 `3/4/2` | 与正式起手金币联动 |
| Assets/Resources/Data/dice_material_config.csv | 已重调材质价格和权重 | 材质保留为当前二级独特性 |
| Assets/Resources/Data/dice_affix_config.csv | 不删除 | 隐藏期间可保持原值 |
| Assets/Resources/Data/dice_crafting_item_config.csv | 不删除 | 可选把权重置 0 作保险，但代码开关必须是主控 |

## 存档影响

- 建议不升级 `CurrentSaveVersion`。
- 不删除 `DiceData` 中前缀和后缀字段。
- 不删除 `AffixAddStone`、`AffixRemoveStone`、`AffixReplaceStone`。
- 关闭状态下读到旧词缀和旧道具数量也不生效、不显示。
- 后续恢复词缀时，另开生产包决定是否继续沿用旧存档内容。

## 界面影响

- 市场右侧货架区域不再出现 `CraftingItem` 商品。
- 市场底部不再出现改造道具栏。
- 左侧骰袋卡副信息不再显示词缀槽位。
- 结果骰短标签优先显示类型运行状态，不显示词缀列表。
- 结算摘要不再出现“词缀 +N”。

## 测试 / 验证计划

静态检查：

- `Import-Csv` 检查所有改动 CSV 可解析。
- `git diff --check` 检查空白。
- 用旧存档或构造数据确认带词缀的 `DiceData` 可读取。

Unity Play Mode：

- 新游戏进入市场，确认不会刷出改造道具。
- 关闭 `market_test_random_refresh` 后刷新多个市场，确认仍不会刷出改造道具。
- 构造带词缀骰，确认预览和真实结算无词缀分数或金币。
- 构造持有三类道具数量，确认市场不显示、不使用。
- 截图 `1280x720` 和 `1920x1080` 的市场、结果决策和结算状态。

平衡验证：

- 记录 6 基础骰前 3 章通关压力。
- 记录第一章结束前玩家可购买的骰子数量和质量。
- 记录 F008/F010/F011 关键骰在无词缀环境下的强度样本。
- 正式市场连续付费刷新样本中不应出现 `Basic` 商品；相邻两次付费刷新应优先看到不同类型，候选不足时允许回退。

## 文档同步

| 文档 | 是否需要更新 | 已确认来源 |
|---|---|---|
| PROJECT_CONTEXT.md | 实现后需要 | F013 D3-D6 |
| GAME_FLOW.md | 实现后需要 | F013 市场和结算变化 |
| DICE_ARCHETYPES.md | 实现后需要 | F013 无词缀基线和市场数据 |
| ART_ASSETS.md | 不需要 | 本包不新增资源 |
| Docs/DesignDialogues/F013-dice-balance-affix-hide.md | 实现后需要 | 生产包状态 |

## 阻塞项

无。

非阻塞提醒：

- 正式起手金币和目标曲线候选值由 F013 执行阶段提出，最终需要用户验收。
- 市场重平衡建议默认使用 `market_test_random_refresh = 0` 的正式市场规则采样；如果执行阶段临时开启测试随机，必须在验收记录中分开标注。

## 本轮执行记录

- 2026-06-16：已完成 F013-01 / F013-02 / F013-04 的静态接入。
- 2026-06-16：新增静态验证记录 `Docs/QA/F013/20260616_f013_soft_close_static_validation.md`。
- 2026-06-16：`git diff --check` 通过，`Assets/Resources/Data/*.csv` 均可被 `Import-Csv` 解析。
- 2026-06-16：当前 PATH 未找到 `dotnet`、`csc` 或 `Unity` 命令，Unity Play Mode、截图和旧存档构造验证未执行。
- 2026-06-16：用户反馈软关闭已验证；已开始并静态接入 F013-03 无词缀数值基线首轮候选。
- 2026-06-16：按用户后续反馈静态接入市场刷新二次调整：基础骰退出随机货架，F008/F010/F011 开放后权重提高，付费刷新优先避开上一批货架类型。
