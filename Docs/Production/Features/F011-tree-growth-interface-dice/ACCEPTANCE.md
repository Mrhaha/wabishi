# F011 验收记录

状态：程序已静态接入，待 Unity 运行验证
功能：F011 大树长期成长接口骰

## 就绪度门禁

| 角色 | 就绪度 | 缺口 / 风险 | 建议补丁 | 阻塞问题 |
|---|---|---|---|---|
| 策划 | 已确认 | 直接成长、旧树下架、肥料标准步长、根系标签和一次实现七颗已裁决 | 运行验证后回写调参结论 | 否 |
| 美术 | 已生成最终资源 | 第三版接触图已由用户接受，七颗最终透明图标已生成并登记 | 程序接入时使用已登记 key | 否 |
| 界面体验 | 已静态接入 | 短标签已接入；仍需截图确认文字不挤压、不误导 | Unity 截图验收 | 否 |
| 程序 | 已静态接入 | 七颗类型、成长队列、根系标签、旧树下架和市场数据已接入；未做运行验证 | Play Mode 样例、截图、保存恢复 | 否 |

## 验收清单

- [x] 策划规则已整理，待回答问题已明确记录。
- [x] 所需接触图样张已生成，最终资源已在接触图通过后生成。
- [x] 界面体验状态已静态接入，待截图验收。
- [ ] 程序行为通过 Unity 运行验证。
- [x] 执行顺序和职能依赖已记录。
- [x] 相关主文档已同步为静态实现事实。
- [x] 美术静态验证结果已记录；程序验证仍后置。

## 第一阶段验收

- [x] 已创建 F011 生产包。
- [x] 已整理七颗大树骰规则卡。
- [x] 已明确第一阶段不改代码、不改 CSV，最终资源需在接触图通过后再生成。
- [x] 已记录程序实现阻塞项。
- [x] 已设置接触图资源路径。
- [x] 已通过 `$wabish-packet-review` 审查并写回直接成长、旧树下架和肥料标准步长裁决。
- [x] 已生成第二版接触图：`Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_v2_20260616.png`。
- [x] 已记录用户反馈：第二版第二颗书本 / 账本式牌谱树不通过，其余六颗方向可保留。
- [x] 已生成牌谱树替代小样：`Assets/ArtSource/Production/F011/f011_pattern_tree_replacement_contact_sheet_20260616.png`。
- [x] 牌谱树替代方向已由用户确认：使用中间格纹骰面方案。
- [x] 已生成第三版完整接触图：`Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_v3_20260616.png`。
- [x] 接触图已由用户接受。
- [x] 已从第三版接触图提取七颗最终透明图标到 `Assets/Resources/Art/DiceTypes/`。
- [x] 已生成最终图标预览：`Assets/ArtSource/Production/F011/f011_tree_growth_interface_final_icons_preview_20260616.png`。
- [x] 已更新 `ART_ASSETS.md` 登记七颗最终图标和 Unity 加载 key。
- [x] 已静态接入七颗类型、市场数据、成长队列、短标签和图标加载。
- [x] 已将旧 `Tree / Gardener / Irrigation` 正式市场权重置为 `0 / 0 / 0`，并在正式市场候选和测试全池随机中过滤。
- [x] 已采用结构化运行时标签 `TypeTriggeredThisSettle` 供根系读取，不解析 `RoundNote`。
- [x] 已同步 `PROJECT_CONTEXT.md`、`GAME_FLOW.md`、`DICE_ARCHETYPES.md`。

## 职能验证记录

- 第二版记录：F011 七颗接触图第二版已生成到 `Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_v2_20260616.png`，曾用于区分度评审。该图不作为最终运行时资源。
- 第一版记录：`Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_20260616.png` 因七颗太类似进入历史样张，不作为当前推荐版本。
- 第二版反馈：用户明确指出第二个书本不行，其余还可以。第二版牌谱树不再作为当前候选，其余六颗可作为后续完整接触图的保留方向。
- 牌谱树替代小样：`Assets/ArtSource/Production/F011/f011_pattern_tree_replacement_contact_sheet_20260616.png` 已生成。三种方案均避免书本 / 账本主体，改为骰面点阵、连线、格纹或年轮分区表达指定牌型。
- 用户选择：牌谱树替代小样使用中间方案。
- 第三版完整接触图：`Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_v3_20260616.png` 已生成。快速检查无生成文字和水印，第二颗已改为骰面格纹方案，不再是书本 / 账本主体；七颗触发点轮廓仍可区分。
- 用户接受第三版接触图后，已生成七颗最终运行时图标：`point_seed_tree_die_icon.png`、`pattern_tree_die_icon.png`、`canopy_tree_die_icon.png`、`ring_tree_die_icon.png`、`fertilizer_tree_die_icon.png`、`pruning_tree_die_icon.png`、`root_tree_die_icon.png`。
- 最终图标均为 256x256 PNG，四角透明，`.meta` 已生成；最终预览见 `Assets/ArtSource/Production/F011/f011_tree_growth_interface_final_icons_preview_20260616.png`，源裁切图见 `Assets/ArtSource/Production/F011/final_icon_sources/`。

## 验证备注

本阶段完成美术资源静态检查和程序静态接入。运行时验证尚未完成，不能视为可玩验收通过。

## 已知缺口

- 仍需 Unity Play Mode 验证点籽、牌谱、冠层、年轮、肥料、修枝、根系七类样例。
- 肥料步长已裁决为标准金币步长，默认读取 `interest_gold_step`；仍需在正式经济配置下验证强度，不能用 1000 金测试起手做平衡结论。
- 根系触发标签已静态接入，但仍需验证相邻触发覆盖面和非触发排除项。
- 仍需截图确认市场卡、骰袋卡和桌面短标签可读。
- 仍需保存恢复验证，确认未新增保存字段且成长入档正常。

## 最终结论

F011 当前已完成包审查补丁、第三版接触图验收、最终图标生产、程序静态接入和主文档同步。剩余门禁是 Unity 运行验证、截图验收、肥料正式经济强度验证、根系标签样例验证和保存恢复验证。
