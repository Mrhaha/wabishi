# F013 市场刷新二次调整静态验证记录

日期：2026-06-16
范围：F013-03 / F013-05 市场刷新二次调整

## 本轮改动

- `Assets/Scripts/DiceKingDemo.cs`
  - 正式市场候选过滤排除 `DieType.Basic`。
  - 付费刷新会读取上一批货架骰子类型，优先避开这些类型重新生成 3 个货架。
  - 避重候选不足时回退到正常池，避免刷空；高阶保底仍可回退保证候选。
  - 测试随机池继续排除基础骰和旧大树三件套。
- `Assets/Resources/Data/dice_market_config.csv`
  - `Basic` 权重改为 `0 / 0 / 0`。
  - 提高 F008 金币收益族、F010 奇偶短规则骰、F011 新大树接口骰在各自开放后的权重。
- 主文档和 F013 生产包已同步基础骰下架、重点骰权重提高和付费刷新避重规则。

## 静态检查

- `Import-Csv Assets\Resources\Data\dice_market_config.csv`
  - 结果：通过。
  - 行数：43。
  - `Basic` 权重：`0 / 0 / 0`。
  - F008 / F010 / F011 重点行：23 行均存在。
- 代码关键路径搜索：
  - `BuildMarketOffers` 会在付费刷新前收集上一批货架类型。
  - `PickMarketDieConfigAvoidingPrevious` 和 `CombinedExcludedTypes` 已接入正式和测试随机选取路径。
  - `CanPickMarketDie` 过滤 `config.Type == DieType.Basic`。
  - `TestRandomMarketDieCandidates` 继续过滤 `DieType.Basic` 和旧大树三件套。
- 文档旧描述搜索：
  - 未发现基础骰仍进入第 1 章市场池的旧描述。
  - 未发现基础骰仍作为便宜市场填充物的旧定位。
  - 未发现基础骰旧三段权重表格描述。
- `git diff --check`
  - 结果：通过。

## 未执行

- 未执行 Unity Play Mode。
- 未执行正式市场连续刷新样本。
- 未执行 1280x720 / 1920x1080 市场截图。

原因：当前 PATH 未找到 `dotnet`、`csc` 或 `Unity` 命令；本轮只能完成静态校验。

## 运行验证建议

- 新游戏进入正式市场，连续付费刷新 10 次，确认不出现 `Basic` 商品。
- 记录相邻两批货架类型，确认候选充足时优先不同。
- 记录第 2 章和第 3 章市场样本，确认 F008 / F010 / F011 比旧权重更容易刷到。
- 触发高阶保底后确认避重不会阻断 T2 / T3 货架。

## 结论

F013 市场刷新二次调整已完成静态接入和静态校验。当前不能宣称运行验收完成，仍需 Unity Play Mode 的正式市场连续刷新样本确认。
