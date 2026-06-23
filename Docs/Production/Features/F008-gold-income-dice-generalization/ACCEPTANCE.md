# F008 验收记录

状态：已实现待验证
功能：F008 金币收益族泛用化
最后更新：2026-06-15

## 就绪度门禁

| 角色 | 就绪度 | 缺口 / 风险 | 建议补丁 | 阻塞问题 |
|---|---|---|---|---|
| 策划 | 就绪 | 九颗骰规则卡、收益、上限和首版裁决已补齐 | 后续按切片执行 | 无 |
| 美术 | 已生成待截图 | 九颗最终透明 PNG 已写入运行目录，透明通道和 `.meta` 静态检查通过 | Unity 市场卡和主流程截图验收 | 本机未找到 Unity Editor CLI |
| 界面体验 | 已实现待截图 | 短标签、状态契约、灰态和反馈时机已接入代码 | Unity Play Mode 截图验收 | 本机未找到 Unity Editor CLI |
| 程序 | 已实现待运行验证 | 代码键、市场建议、触发、预览、真实结算和存档边界已接入 | Unity 样例验证 | 本机未找到 Unity Editor CLI |

## 验收清单

- [x] 生产包已基于 D1 生成。
- [x] 九颗金币收益族骰子已纳入范围。
- [x] F008-01 规则卡审查已完成。
- [x] 每颗骰的一句话规则已确认。
- [x] 每颗骰的触发时机已确认。
- [x] 每颗骰的金币收益和上限已确认。
- [x] 每颗骰是否进入左到右钱包流已确认。
- [x] 出千相关交互已定义。
- [x] 临时小骰数量读取和预览估算已定义。
- [x] 牌型读取不新增全局牌型。
- [x] 稳定经济类不成为无脑必买的首版上限已定义。
- [x] 槽位类触发玩家可理解的左邻规则已定义。
- [x] 削点类不会意外变成强控点工具的边界已定义。
- [x] 所需美术资源已完成，或已明确后置。
- [x] 界面体验状态已实现，或已明确后置。
- [ ] 程序行为符合已确认规则。
- [x] 执行顺序和职能依赖已记录。
- [x] 相关主文档已同步。
- [x] 验证结果已记录。

## 职能验证记录

- 2026-06-13：使用 `$wabish-production-pipeline` 基于 `Docs/DesignDialogues/D1-future-dice-types.md` 创建 F008 生产包。当前只完成生产包编译，未实现代码、数据或资源。
- 2026-06-15：使用 `$wabish-packet-review` 完成 F008-01 规则卡审查，补齐九颗金币收益族骰子的首版规则、收益、上限、市场建议、预览边界、出千交互和存档影响。未修改代码、数据或资源。
- 2026-06-15：使用 `$wabish-art-production` / `$wabish-art-assets` 生成九颗金币收益族骰子的九宫格接触图，路径为 `Assets/ArtSource/Production/F008/f008_gold_income_dice_contact_sheet_20260615.png`。后续已从接触图提取九颗 256x256 透明 PNG，写入 `Assets/Resources/Art/DiceTypes/`。
- 2026-06-15：使用 `$wabish-dev-implementation` 接入九颗骰子的 `DieType`、市场配置、全局数值键、预览/真实结算、通关复利、左到右钱包流、短标签、市场建议和主文档同步。修改范围包括 `Assets/Scripts/DiceKingDemo.cs`、`Assets/Resources/Data/global.csv`、`Assets/Resources/Data/dice_market_config.csv`、`DICE_ARCHETYPES.md`、`GAME_FLOW.md`、`PROJECT_CONTEXT.md`、`ART_ASSETS.md` 和本生产包文档。
- 2026-06-15：根据测试期曝光需求改为全池随机刷新：`market_test_random_refresh = 1` 时，3 个市场货架只从全部已实现非基础骰类型里等概率随机，不读取章节开放、路线倾向、高阶保底或刷新权重，也暂不刷改造道具；关闭为 `0` 后恢复正式市场规则。
- 2026-06-15：静态验证通过：F008 九颗市场 CSV 行可被 `Import-Csv` 解析；新增全局数值键可被 `Import-Csv` 解析；悬赏、柜台、伐木三颗的旧短图标键未残留；`DiceKingDemo.cs` 大括号和小括号计数平衡。运行验证未完成：本机未找到 Unity Editor CLI、`dotnet`、`csc` 或 `mcs`。
- 2026-06-15：上一轮刷新调参静态验证通过：`dice_market_config.csv` 九颗 F008 行精确匹配新权重；`market_rule_config.csv` 第 1-2 章高阶保底精确匹配 `1`；`DiceKingDemo.cs` 缺省回退权重与 CSV 一致。该规则当前被 `market_test_random_refresh = 1` 测试随机刷新覆盖，保留为关闭测试开关后的正式规则基础。
- 2026-06-15：美术最终资源静态验证通过：九颗 F008 骰子图标均为 256x256 RGBA 透明 PNG，均存在对应 `.meta`；最终图标预览已写入 `Assets/ArtSource/Production/F008/f008_gold_income_dice_final_icons_preview_20260615.png`。
- 2026-06-15：程序实现静态审计通过：九颗 `DieType`、市场配置、图标 key、悬赏目标、收账重置、铅票出千标记、壳税门槛、柜台左邻读取、伐木本次分削减和复利总上限均能在 `DiceKingDemo.cs` 中对应到实现；括号计数平衡。运行验证未完成：本机未找到 Unity Editor CLI。
- 2026-06-15：测试随机刷新静态验证通过：`global.csv` 可解析且 `market_test_random_refresh = 1`；九颗 F008 市场行仍存在；`DiceKingDemo.cs` 已接入测试开关、全非基础骰随机候选池、改造道具屏蔽、高阶保底屏蔽和 UI 提示；括号计数平衡。运行验证未完成：本机未找到 Unity Editor CLI、`dotnet`、`csc` 或 `mcs`。

## 验证备注

F008-01 已完成，九颗骰首轮代码和数据已接入。后续验收重点转为：在 Unity Play Mode 中验证预览和真实结算口径一致、钱包流顺序正确、复利不递归、伐木不改变牌型、市场出现和存档读取正常。

## 已知缺口

- 九颗骰最终透明图标已生成，但仍需 Unity 市场卡和主流程截图确认小尺寸实际观感。
- 界面体验已接入代码，但仍需截图验收。
- 程序已接入代码和 CSV，但未做 Unity 编译、Play Mode 或运行样例。
- 市场当前打开 `market_test_random_refresh = 1` 的测试期随机刷新；正式调参前需要改回 `0`，再按测试数据调整市场权重和章节开放。

## 最终结论

F008 九颗金币收益族骰子的首轮代码、数据和最终透明图标已接入，但仍处于待运行验证状态。下一步是 Unity Play Mode 样例验证、市场截图和主流程截图验收。
