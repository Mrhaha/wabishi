# F013 软关闭静态验证记录

日期：2026-06-16
范围：F013-01 / F013-02 / F013-04
状态：静态检查通过，Unity Play Mode 未执行

## 本轮改动

- 在 `Assets/Resources/Data/global.csv` 新增 `affix_feature_enabled,0`。
- 在 `Assets/Scripts/DiceKingDemo.cs` 新增 `affixFeatureEnabled`，并从 `global.csv` 读取。
- 关闭状态下，市场不生成、不购买、不使用 `CraftingItem`。
- 关闭状态下，市场不绘制改造道具栏、目标选择面板或“仍可买改造道具”提示。
- 关闭状态下，骰袋和结果短标签不显示词缀槽位或词缀列表。
- 关闭状态下，`AffixScoreBonusForDie` 返回 0，`WalletIncomeForDie` 跳过 `SuffixAffixes`。
- `DiceData` 的前缀/后缀字段和三类改造道具持有数量继续保存和读取，未升级 `SaveVersion`。

## 已执行检查

- `git diff --check`：通过，无空白错误。
- `Import-Csv Assets\Resources\Data\global.csv`：通过。
- `Import-Csv Assets\Resources\Data\*.csv`：全部可解析。
- 静态入口检查：
  - `ShouldMakeCraftingItemOffer` 受 `affixFeatureEnabled` 保护。
  - `CanBuyMarketOffer` 拒绝关闭状态下的 `CraftingItem`。
  - `CanUseCraftingItemOnDie` 拒绝关闭状态下的改造使用。
  - `DrawMarket` 关闭状态下清空 `activeCraftingItemKey`，不绘制道具栏。
  - `DrawCompactDie` 关闭状态下显示面组和材质，不显示词缀槽位。
  - `AffixOrRoundTag` / `AffixShortText` 关闭状态下回退到普通回合短标签。
  - `AffixScoreBonusForDie` / `WalletIncomeForDie` 关闭状态下不应用 F005 词缀收益。

## 未执行检查

- 未执行 Unity Play Mode。
- 未截图验证 `1280x720` / `1920x1080` 市场、结果决策和结算界面。
- 未用构造旧存档实机验证带词缀骰和三类道具数量。
- 未执行 F013-03 无词缀数值重平衡。

## 工具缺口

- 当前 PATH 未找到 `dotnet`、`csc` 或 `Unity` 命令，无法在本轮命令行完成 C# 编译或 Unity 运行验证。

## 结论

F013 软关闭前三步已完成静态接入。当前可以进入 Unity Play Mode 验证；通过后再推进 F013-03 数值重平衡。
