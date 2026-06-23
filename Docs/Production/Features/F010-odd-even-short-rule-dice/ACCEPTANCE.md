# F010 验收记录

状态：已确认
功能：F010 奇偶短规则骰
最后更新：2026-06-16

## 就绪度门禁

| 角色 | 就绪度 | 缺口 / 风险 | 建议补丁 | 阻塞问题 |
|---|---|---|---|---|
| 策划 | 就绪 | 规则短卡已锁定，但细节边界需审查 | 使用 `$wabish-packet-review` 审查默认值 | 无 |
| 美术 | 就绪 | 第二版接触图已由用户接受，正在准备最终资源 | 生成最终透明图标并登记 | 无 |
| 界面体验 | 已验证 | 1280x720 / 1920x1080 QA 截图已生成 | 后续如改 UI 再走截图反馈 | 无 |
| 程序 | 已验证 | 七颗触发样例、预览 / 真实结算一致性和出千路径已通过 Unity Play Mode | 后续只做数值调参或用户验收反馈 | 无 |

## 第一阶段验收清单

- [x] 生产包已基于 D1 创建。
- [x] 范围锁定七颗奇偶短规则骰。
- [x] 第一阶段明确不改代码。
- [x] 第一阶段明确不生成最终运行时资源。
- [x] 已创建规则卡。
- [x] 已创建接触图美术需求。
- [x] 程序实现已被接触图门禁阻塞。
- [x] 已完成 `$wabish-packet-review` 审查。
- [x] 已生成接触图。
- [x] 用户已接受接触图。

## 后续验收清单

- [x] 接触图只放入 `Assets/ArtSource/Production/F010/`。
- [x] 接触图中七颗机制可被一眼区分。
- [x] 接触图不含生成文字。
- [x] 最终图标生成后登记 `ART_ASSETS.md`。
- [x] 七颗骰已接入类型枚举、显示名、短规则、图标 key 和市场配置。
- [x] `补全骰` 代码路径只影响全奇 / 全偶副牌型识别，不改有效点数。
- [x] `复核骰` 代码路径只在自然结果锁定后触发，出千后不再复核。
- [x] `转号骰` 代码路径在出千选择和确认时过滤无相反奇偶面的情况。
- [x] 主文档已同步当前实现事实和运行验证结论。
- [x] 程序实现后七颗骰预览和真实结算一致，已通过 Unity Play Mode 验证。
- [x] 出千相关三颗取消、未选择、确认路径已通过 Unity Play Mode 验证。
- [x] 1280x720 / 1920x1080 截图已生成并确认非空、尺寸正确。

## 职能验证记录

- 2026-06-15：使用 `$wabish-production-pipeline` 基于 `Docs/DesignDialogues/D1-future-dice-types.md` 创建 F010 生产包。当前只完成生产包、规则卡和接触图需求编译，未改代码、未改数据、未生成资源。
- 2026-06-15：使用 `$wabish-packet-review` 完成执行审查，结论为接触图阶段可执行，程序实现当时仍阻塞。
- 2026-06-15：使用 `$wabish-art-production` 路由 `$wabish-art-assets` 生成第一版七格接触图：`Assets/ArtSource/Production/F010/f010_odd_even_short_rule_contact_sheet_20260615.png`。该图为驳回样张，不是最终运行时资源。
- 2026-06-15：用户驳回第一版接触图版式，原因是横向拉伸过重，不能像之前接触图一样看到原始图标效果。第二版改为近正方形九宫格参考版式。
- 2026-06-15：已生成第二版近正方形九宫格接触图：`Assets/ArtSource/Production/F010/f010_odd_even_short_rule_contact_sheet_v2_20260615.png`。尺寸 `1254x1254`，已由用户接受。
- 2026-06-15：用户确认第二版接触图通过。F010-03 接触图评审完成，允许进入 F010-04 最终透明图标生成和 F010-05 程序实现。
- 2026-06-15：已从第二版接触图生成七颗 256x256 透明 PNG，写入 `Assets/Resources/Art/DiceTypes/`；最终预览为 `Assets/ArtSource/Production/F010/f010_odd_even_short_rule_final_icons_preview_20260615.png`，源裁切保留在 `Assets/ArtSource/Production/F010/final_icon_sources/`，并已登记 `ART_ASSETS.md`。
- 2026-06-15：已接入七颗 F010 骰到 `Assets/Scripts/DiceKingDemo.cs` 和 `Assets/Resources/Data/dice_market_config.csv`。静态检查确认七个类型 key、36 行市场 CSV、七张 256x256 RGBA 透明运行时图标和 `.meta` 均存在；`git diff --check` 无报错；当日运行验证未完成。
- 2026-06-16：使用现有 Unity 2019.4.33f1 编辑器运行 `Assets/Editor/DiceKingF010ValidationRunner.cs`，完成 F010 Play Mode 验证。报告：`Docs/QA/F010/20260616_f010_validation_report.txt`。截图：`Docs/QA/F010/20260616_f010_result_1920x1080.png`、`Docs/QA/F010/20260616_f010_cheat_1920x1080.png`、`Docs/QA/F010/20260616_f010_result_1280x720.png`。

## 验证备注

当前已完成包审查、第二版接触图验收、最终透明图标生成、资源登记、程序静态接入、Unity Play Mode 样例验证与截图验收。

## 已知缺口

- 第一版接触图已生成，但因版式拉伸过重未被接受；第二版接触图已生成并由用户接受。
- 固定分口径、复核触发时机、补全边界和 `转号骰` 面组限制已通过首轮 Play Mode 样例验证；后续缺口仅是用户实玩反馈和数值调参。

## 最终结论

F010 已完成包审查、第二版接触图验收、最终图标入库、程序静态接入和 Unity Play Mode 验证。当前状态为可进入用户实玩验收。
