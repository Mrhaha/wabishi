# F001-D 程序交接

状态：已实现待验证
功能：大树路线第一批补强骰
实现事实来源：Assets/Scripts/DiceKingDemo.cs
最后更新：2026-06-10

## 已实现内容

- 新增 `DieType.Gardener` 和 `DieType.Irrigation`，枚举追加在末尾以保护旧数字序号存档。
- 新增默认名称、短名、颜色、图标资源 key、市场建议和骰袋路线摘要。
- 市场 CSV 增加园丁骰和灌溉骰。
- 大树成长队列支持同一棵树在一次结算中成长多次，并汇总日志。
- 园丁按骰袋中园丁数量放大每棵自然命中大树的成长次数。
- 灌溉按自身有效点数补未自然命中大树 1 次成长。

## 关键边界

- 园丁不直接加分，只增加自然命中大树的成长次数。
- 灌溉补成长不是自然命中，不触发园丁。
- 灌溉每颗每次最多浇中 1 棵树，同一棵树每次最多被灌溉 1 次。
- 不新增存档字段；继续复用大树 `Faces` 和 `Growth`。
- 本版不生成正式图标资源，缺图时使用运行时回退。

## 改动文件

- Assets/Scripts/DiceKingDemo.cs
- Assets/Resources/Data/dice_market_config.csv
- PROJECT_CONTEXT.md
- GAME_FLOW.md
- DICE_ARCHETYPES.md
- Docs/Production/Features/F001-D-tree-dice/*

## 验证要求

- 在 Unity 中买入园丁、灌溉和大树后验证市场、保存、继续游戏。
- 验证 1 棵自然命中大树 + 2 颗园丁会让该树成长 3 次。
- 验证灌溉命中未自然命中大树时只成长 1 次，且不触发园丁。
- 验证出千重摇灌溉骰后按新点数追大树命中点。
- 验证缺少正式图标时不会阻塞运行。
