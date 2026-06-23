# 美术资源登记表

本文件记录已生成美术资源、项目路径、预期 Unity `Resources.Load` 加载 key 和替换备注。

## 当前风格

- 技能：`$wabish-art-assets`
- 风格指南：`C:\Users\admin\.codex\skills\wabish-art-assets\references\art-style-guide.md`
- 风格名：`Bright Ledger Boardgame` / `明亮账本桌游风`

## 运行时美术实现约定

当前主流程美术应优先使用项目内生成资源作为可见美术来源：

1. 代码使用的背景和道具必须放在 `Assets/Resources/Art/` 下，并通过 Unity `Resources.Load` 加载。
2. 源图、抠图中间件、接触图和旧探索图必须放在 `Assets/ArtSource/` 下，避免作为运行时 `Resources` 打包。
3. 骰子运行时表现使用三层结构：
   - 基础骰面：来自 `Assets/Resources/Art/DiceFaces/` 的生成空白骰面。
   - 动态结果：点数由代码直接绘制到骰面上，包括 `7+` 成长点数的自适应点阵。
   - 类型身份：来自 `Assets/Resources/Art/DiceTypes/` 的生成骰子类型图标，用作小角标。
   - 状态标签：由界面文字显示目标点、阈值、成长或临时标记。
4. 主投掷结果不使用数值徽章；可见结果必须是实际点数骰面。
5. 新增骰子类型图标必须把机制符号融入骰子本体。优先使用骰面标记、骰边切角、骰角封章、绑带、嵌入标记、刻槽、点阵变形或表面镶嵌；避免“骰子主体 + 外部场景道具”的构图。
6. 程序化骰面只作为生成运行时骰面资源缺失时的兜底，不应成为主流程默认骰子身份。
7. 新增主流程美术资源必须通过 `$wabish-art-assets` 生成，或明确匹配同一风格指南，并在代码引用前登记到本文件。
8. 主流程界面控件使用不拉伸的 `OnGUI` 绘制路径：
   - 可缩放羊皮纸面板和按钮由代码程序化绘制，避免装饰和纸纹被强行拉伸。
   - `Assets/Resources/Art/UI/` 下的生成纹理保留为风格参考、替换候选和小图标资源。
   - 小型生成图标以固定尺寸用于顶部信息和市场操作提示。

## 已生成骰子类型图标

最终运行目录：

```text
Assets/Resources/Art/DiceTypes/
```

Unity 加载 key 规则：

```text
Art/DiceTypes/<file-name-without-extension>
```

| 骰子类型 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 基础骰 | `Assets/Resources/Art/DiceTypes/basic_die_icon.png` | `Art/DiceTypes/basic_die_icon` | 已生成 | 稳定起始骰图标，透明通道已校验。 |
| 猪猪骰 | `Assets/Resources/Art/DiceTypes/piggy_die_icon.png` | `Art/DiceTypes/piggy_die_icon` | 已生成 | 经济骰图标，透明通道已校验。 |
| 龟龟骰 | `Assets/Resources/Art/DiceTypes/turtle_die_icon.png` | `Art/DiceTypes/turtle_die_icon` | 已生成 | 迭代和临时小骰路线图标，透明通道已校验。 |
| 壳匠骰 | `Assets/Resources/Art/DiceTypes/shellsmith_die_icon.png` | `Art/DiceTypes/shellsmith_die_icon` | 已生成 | 2026-06-05 由 F001-B 第四版接触图裁切抠透明；以骰子为主体，壳片计数汇入加分章，表达临时小骰数量转分；透明通道已校验。 |
| 巢穴骰 | `Assets/Resources/Art/DiceTypes/nest_die_icon.png` | `Art/DiceTypes/nest_die_icon` | 已生成 | 2026-06-05 由 F001-B 第四版接触图裁切抠透明；以骰子为主体，半环巢托中冒出单点小骰，表达补 d1 临时小骰；透明通道已校验。 |
| 慢龟骰 | `Assets/Resources/Art/DiceTypes/slow_turtle_die_icon.png` | `Art/DiceTypes/slow_turtle_die_icon` | 已生成 | 2026-06-05 由 F001-B 第四版接触图裁切抠透明；以骰子为主体，逐级小骰链和沙漏表达慢衰长链；透明通道已校验。 |
| 双倍骰 | `Assets/Resources/Art/DiceTypes/double_die_icon.png` | `Art/DiceTypes/double_die_icon` | 已生成 | 直接翻倍得分图标，透明通道已校验。 |
| 奇数骰 | `Assets/Resources/Art/DiceTypes/odd_die_icon.png` | `Art/DiceTypes/odd_die_icon` | 已生成 | 奇数控面路线图标，透明通道已校验。 |
| 偶数骰 | `Assets/Resources/Art/DiceTypes/even_die_icon.png` | `Art/DiceTypes/even_die_icon` | 已生成 | 偶数控面路线图标，透明通道已校验。 |
| 孤证骰 | `Assets/Resources/Art/DiceTypes/lone_witness_die_icon.png` | `Art/DiceTypes/lone_witness_die_icon` | 已生成 | 2026-06-10 由 F001-C 第二版接触图裁切抠透明；以骰子为主体，缺席同点凹槽、回环刻线和角落复核章内嵌骰体，表达落单后重找同点；透明通道已校验。 |
| 盖章骰 | `Assets/Resources/Art/DiceTypes/stamp_die_icon.png` | `Art/DiceTypes/stamp_die_icon` | 已生成 | 2026-06-10 由 F001-C 第二版接触图裁切抠透明；以骰子为主体，骰面同点窗口和蜡封嵌件表达同点确认兑现；透明通道已校验。 |
| 半步骰 | `Assets/Resources/Art/DiceTypes/half_step_die_icon.png` | `Art/DiceTypes/half_step_die_icon` | 已生成 | 2026-06-10 由 F001-C 第二版接触图裁切抠透明；以骰子为主体，骰边半格切角和向低一格刻线表达顺子借位；透明通道已校验。 |
| 轨道骰 | `Assets/Resources/Art/DiceTypes/track_die_icon.png` | `Art/DiceTypes/track_die_icon` | 已生成 | 2026-06-10 由 F001-C 第二版接触图裁切抠透明；以骰子为主体，骰面点阵轨槽和终点嵌件表达 2/4/6 轨道齐备；透明通道已校验。 |
| 异邻骰 | `Assets/Resources/Art/DiceTypes/parity_neighbor_diff_die_icon.png` | `Art/DiceTypes/parity_neighbor_diff_die_icon` | 已生成 | 2026-06-15 由 F010 第二版九宫格接触图提取为 256x256 透明 PNG；左侧异色邻位槽和奇偶双色嵌件表达左邻奇偶不同。透明通道已静态校验。 |
| 同邻骰 | `Assets/Resources/Art/DiceTypes/parity_neighbor_same_die_icon.png` | `Art/DiceTypes/parity_neighbor_same_die_icon` | 已生成 | 2026-06-15 由 F010 第二版九宫格接触图提取为 256x256 透明 PNG；同色成对嵌件表达左邻奇偶相同。透明通道已静态校验。 |
| 补全骰 | `Assets/Resources/Art/DiceTypes/parity_complete_die_icon.png` | `Art/DiceTypes/parity_complete_die_icon` | 已生成 | 2026-06-15 由 F010 第二版九宫格接触图提取为 256x256 透明 PNG；合拢奇偶环和封印表达补全全奇或全偶。透明通道已静态校验。 |
| 复核骰 | `Assets/Resources/Art/DiceTypes/parity_review_die_icon.png` | `Art/DiceTypes/parity_review_die_icon` | 已生成 | 2026-06-15 由 F010 第二版九宫格接触图提取为 256x256 透明 PNG；内嵌回环刻线和复核章表达破坏全奇或全偶时重摇一次。透明通道已静态校验。 |
| 翻号骰 | `Assets/Resources/Art/DiceTypes/parity_flip_score_die_icon.png` | `Art/DiceTypes/parity_flip_score_die_icon` | 已生成 | 2026-06-15 由 F010 第二版九宫格接触图提取为 256x256 透明 PNG；双色翻面印和封章表达出千后奇偶变化给分。透明通道已静态校验。 |
| 守号骰 | `Assets/Resources/Art/DiceTypes/parity_hold_score_die_icon.png` | `Art/DiceTypes/parity_hold_score_die_icon` | 已生成 | 2026-06-15 由 F010 第二版九宫格接触图提取为 256x256 透明 PNG；同色锁扣和稳定封签表达出千后奇偶保持给分。透明通道已静态校验。 |
| 转号骰 | `Assets/Resources/Art/DiceTypes/parity_turner_die_icon.png` | `Art/DiceTypes/parity_turner_die_icon` | 已生成 | 2026-06-15 由 F010 第二版九宫格接触图提取为 256x256 透明 PNG；奇偶分界转盘和换色机关表达出千重摇必变奇偶。透明通道已静态校验。 |
| 大树骰 | `Assets/Resources/Art/DiceTypes/tree_die_icon.png` | `Art/DiceTypes/tree_die_icon` | 已生成 | 自成长骰图标，透明通道已校验。 |
| 园丁骰 | `Assets/Resources/Art/DiceTypes/gardener_die_icon.png` | `Art/DiceTypes/gardener_die_icon` | 已生成 | 2026-06-10 由 F001-D 方向 A 接触图裁切抠透明；以骰子为主体，枝条刻槽、嫩芽嵌件和绿色镶边表达自然命中后的额外成长；透明通道已校验。 |
| 灌溉骰 | `Assets/Resources/Art/DiceTypes/irrigation_die_icon.png` | `Art/DiceTypes/irrigation_die_icon` | 已生成 | 2026-06-10 由 F001-D 方向 A 接触图裁切抠透明；以骰子为主体，水流刻槽、水滴嵌件和点数孔连接表达掷中命中点后补成长；透明通道已校验。 |
| 点籽树 | `Assets/Resources/Art/DiceTypes/point_seed_tree_die_icon.png` | `Art/DiceTypes/point_seed_tree_die_icon` | 已生成 | 2026-06-16 由 F011 第三版完整接触图提取为 256x256 透明 PNG；靶心点籽和种子槽表达命中指定点成长。透明通道已静态校验。 |
| 牌谱树 | `Assets/Resources/Art/DiceTypes/pattern_tree_die_icon.png` | `Art/DiceTypes/pattern_tree_die_icon` | 已生成 | 2026-06-16 由 F011 第三版完整接触图提取为 256x256 透明 PNG；格纹骰面和藤蔓连线表达指定牌型成长。透明通道已静态校验。 |
| 冠层树 | `Assets/Resources/Art/DiceTypes/canopy_tree_die_icon.png` | `Art/DiceTypes/canopy_tree_die_icon` | 已生成 | 2026-06-16 由 F011 第三版完整接触图提取为 256x256 透明 PNG；层叠树冠和顶冠印记表达自身最高面成长。透明通道已静态校验。 |
| 年轮树 | `Assets/Resources/Art/DiceTypes/ring_tree_die_icon.png` | `Art/DiceTypes/ring_tree_die_icon` | 已生成 | 2026-06-16 由 F011 第三版完整接触图提取为 256x256 透明 PNG；木桩年轮和时砂章表达每次出手后成长。透明通道已静态校验。 |
| 肥料树 | `Assets/Resources/Art/DiceTypes/fertilizer_tree_die_icon.png` | `Art/DiceTypes/fertilizer_tree_die_icon` | 已生成 | 2026-06-16 由 F011 第三版完整接触图提取为 256x256 透明 PNG；土壤根须和金币肥料表达按当前金币存量成长，不表现为金币收益骰。透明通道已静态校验。 |
| 修枝树 | `Assets/Resources/Art/DiceTypes/pruning_tree_die_icon.png` | `Art/DiceTypes/pruning_tree_die_icon` | 已生成 | 2026-06-16 由 F011 第三版完整接触图提取为 256x256 透明 PNG；切开的骰面、枝条和剪枝切口表达出千变好成长。透明通道已静态校验。 |
| 根系树 | `Assets/Resources/Art/DiceTypes/root_tree_die_icon.png` | `Art/DiceTypes/root_tree_die_icon` | 已生成 | 2026-06-16 由 F011 第三版完整接触图提取为 256x256 透明 PNG；横向根须和左右接口表达相邻槽位触发。透明通道已静态校验。 |
| 赌徒骰 | `Assets/Resources/Art/DiceTypes/gambler_die_icon.png` | `Art/DiceTypes/gambler_die_icon` | 已生成 | 风险爆发骰图标，透明通道已校验。 |
| 国库骰 | `Assets/Resources/Art/DiceTypes/treasury_die_icon.png` | `Art/DiceTypes/treasury_die_icon` | 已生成 | 2026-06-04 重生成；以骰子本体为主，骰面集成国库门、金币点数和王冠蜡封，透明通道已校验。 |
| 贿赂骰 | `Assets/Resources/Art/DiceTypes/bribe_die_icon.png` | `Art/DiceTypes/bribe_die_icon` | 已生成 | 2026-06-04 重生成；以骰子本体为主，边缘附加钱袋与信封，骰面保留蜡封和印章补账提示，透明通道已校验。 |
| 投资骰 | `Assets/Resources/Art/DiceTypes/investment_die_icon.png` | `Art/DiceTypes/investment_die_icon` | 已生成 | 2026-06-04 重生成；以骰子本体为主，骰面集成增长图形、金币嫩芽和预算绑带，透明通道已校验。 |
| 悬赏骰 | `Assets/Resources/Art/DiceTypes/bounty_gold_die_icon.png` | `Art/DiceTypes/bounty_gold_die_icon` | 已生成 | 2026-06-15 由 F008 九宫格接触图提取为 256x256 透明 PNG；目标章和金币章表达命中悬赏点。透明通道已静态校验。 |
| 顶金骰 | `Assets/Resources/Art/DiceTypes/top_gold_die_icon.png` | `Art/DiceTypes/top_gold_die_icon` | 已生成 | 2026-06-15 由 F008 九宫格接触图提取为 256x256 透明 PNG；顶面金边和小冠表达最高点给金。透明通道已静态校验。 |
| 牌税骰 | `Assets/Resources/Art/DiceTypes/hand_tax_die_icon.png` | `Art/DiceTypes/hand_tax_die_icon` | 已生成 | 2026-06-15 由 F008 九宫格接触图提取为 256x256 透明 PNG；手牌税章表达牌型触发金币。透明通道已静态校验。 |
| 收账骰 | `Assets/Resources/Art/DiceTypes/collection_die_icon.png` | `Art/DiceTypes/collection_die_icon` | 已生成 | 2026-06-15 由 F008 九宫格接触图提取为 256x256 透明 PNG；账页、封章和抽屉金币表达小关首次收账。透明通道已静态校验。 |
| 复利骰 | `Assets/Resources/Art/DiceTypes/compound_interest_die_icon.png` | `Art/DiceTypes/compound_interest_die_icon` | 已生成 | 2026-06-15 由 F008 九宫格接触图提取为 256x256 透明 PNG；环形金币和螺旋封章表达通关利息追加。透明通道已静态校验。 |
| 铅票骰 | `Assets/Resources/Art/DiceTypes/lead_ticket_die_icon.png` | `Art/DiceTypes/lead_ticket_die_icon` | 已生成 | 2026-06-15 由 F008 九宫格接触图提取为 256x256 透明 PNG；重摇箭头、铅封和票据章表达出千重摇兑现。透明通道已静态校验。 |
| 壳税骰 | `Assets/Resources/Art/DiceTypes/shell_tax_die_icon.png` | `Art/DiceTypes/shell_tax_die_icon` | 已生成 | 2026-06-15 由 F008 九宫格接触图提取为 256x256 透明 PNG；龟壳格和金币铆钉表达临时小骰达标收税。透明通道已静态校验。 |
| 柜台骰 | `Assets/Resources/Art/DiceTypes/counter_gold_die_icon.png` | `Art/DiceTypes/counter_gold_die_icon` | 已生成 | 2026-06-15 由 F008 九宫格接触图提取为 256x256 透明 PNG；内嵌柜台和侧边槽表达左邻入账联动。透明通道已静态校验。 |
| 伐木骰 | `Assets/Resources/Art/DiceTypes/lumber_gold_die_icon.png` | `Art/DiceTypes/lumber_gold_die_icon` | 已生成 | 2026-06-15 由 F008 九宫格接触图提取为 256x256 透明 PNG；裂痕和金币章表达削分换金。透明通道已静态校验。 |

源图副本目录：

```text
Assets/ArtSource/DiceTypes/_source_chromakey/
```

接触图：

```text
Assets/ArtSource/DiceTypes/_dice_type_contact_sheet.png
Assets/ArtSource/DiceTypes/_gold_dice_contact_sheet_20260604.png
Assets/ArtSource/DiceTypes/_f001b_turtle_function_symbol_contact_sheet_20260605.png
Assets/ArtSource/DiceTypes/_f001c_odd_even_embedded_symbol_contact_sheet_20260610.png
Assets/ArtSource/DiceTypes/_f001c_odd_even_final_icons_preview_20260610.png
Assets/ArtSource/DiceTypes/_f001d_tree_gardener_irrigation_direction_a_contact_sheet_20260610.png
Assets/ArtSource/DiceTypes/_f001d_tree_final_icons_preview_20260610.png
Assets/ArtSource/Production/F008/f008_gold_income_dice_contact_sheet_20260615.png
Assets/ArtSource/Production/F008/f008_gold_income_dice_final_icons_preview_20260615.png
Assets/ArtSource/Production/F010/f010_odd_even_short_rule_contact_sheet_20260615.png
Assets/ArtSource/Production/F010/f010_odd_even_short_rule_contact_sheet_v2_20260615.png
Assets/ArtSource/Production/F010/f010_odd_even_short_rule_final_icons_preview_20260615.png
Assets/ArtSource/Production/F010/final_icon_sources/
Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_20260616.png
Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_v2_20260616.png
Assets/ArtSource/Production/F011/f011_pattern_tree_replacement_contact_sheet_20260616.png
Assets/ArtSource/Production/F011/f011_tree_growth_interface_contact_sheet_v3_20260616.png
Assets/ArtSource/Production/F011/f011_tree_growth_interface_final_icons_preview_20260616.png
Assets/ArtSource/Production/F011/final_icon_sources/
```

## 已生成运行时骰面资源

| 资源 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 空白运行时骰面 | `Assets/Resources/Art/DiceFaces/runtime_die_face_base.png` | `Art/DiceFaces/runtime_die_face_base` | 已生成 | 中性生成骰面底图，用于所有运行时投掷结果的代码绘制点数。 |
| F009 统一待机骰 | `Assets/Resources/Art/DiceRoll/f009_unified_ready_die_256.png` | `Art/DiceRoll/f009_unified_ready_die_256` | 已接入待运行验证 | `Ready` 和结算后重置阶段的默认骰体；已按截图反馈重拆为完整裁切版，非基础骰由代码叠加小类型标记。 |
| F009 统一旋转循环 strip | `Assets/Resources/Art/DiceRoll/f009_unified_spin_loop_strip_24f_256.png` | `Art/DiceRoll/f009_unified_spin_loop_strip_24f_256` | 已接入待运行验证 | 24 帧 256 像素横条，用于 `Shaking` 阶段固定槽位内旋转；已按截图反馈过滤边缘碎片源帧。 |
| F009 统一停转预览 strip | `Assets/Resources/Art/DiceRoll/f009_unified_spin_stop_strip_8f_256.png` | `Art/DiceRoll/f009_unified_spin_stop_strip_8f_256` | 已接入待运行验证 | 8 帧 256 像素横条，用于 `Stopping` 阶段减速预览；已按截图反馈改为完整源帧重组，停住后切到真实结果骰面。 |
| F009 统一结果骰面 strip | `Assets/Resources/Art/DiceFaces/f009_unified_result_die_faces_6x256.png` | `Art/DiceFaces/f009_unified_result_die_faces_6x256` | 已接入待运行验证 | 1 到 6 点结果骰面横条，用于 `ResultDecision` 和 `Scoring` 阶段；已按截图反馈扩大安全边距，`7+` 仍回退到程序点阵。 |
| F009 桌面摩擦旋转循环 strip 旧版 | `Assets/Resources/Art/DiceRoll/f009_table_friction_spin_loop_strip_24f_256.png` | `Art/DiceRoll/f009_table_friction_spin_loop_strip_24f_256` | 保留为回退 | 旧 24 帧桌面摩擦旋转 strip；统一旋转资源缺失时由代码回退读取。 |
| F009 桌面摩擦停转预览 strip 旧版 | `Assets/Resources/Art/DiceRoll/f009_table_friction_spin_stop_strip_8f_256.png` | `Art/DiceRoll/f009_table_friction_spin_stop_strip_8f_256` | 保留为回退 | 旧 8 帧停转预览 strip；统一停转资源缺失时由代码回退读取。 |

源图副本：

```text
Assets/ArtSource/DiceFaces/_source_chromakey/runtime_die_face_base_source.png
Assets/ArtSource/Production/F009/f009_table_friction_spin_loop_strip_24f_256_20260616.png
Assets/ArtSource/Production/F009/f009_table_friction_spin_stop_strip_8f_256_20260616.png
Assets/ArtSource/Production/F009/f009_table_friction_spin_combined_preview_32f_20260616.gif
Assets/ArtSource/Production/F009/f009_table_friction_spin_loop_contact_24f_20260616.png
Assets/ArtSource/Production/F009/f009_table_friction_spin_stop_contact_8f_20260616.png
Assets/ArtSource/Production/F009/f009_unified_die_visual_language_contact_sheet_v1_20260616.png
Assets/ArtSource/Production/F009/f009_unified_stage_ready_contact_sheet_v1_20260616.png
Assets/ArtSource/Production/F009/f009_unified_stage_roll_stop_contact_sheet_v1_20260616.png
Assets/ArtSource/Production/F009/f009_unified_stage_result_scoring_contact_sheet_v1_20260616.png
Assets/ArtSource/Production/F009/f009_unified_runtime_assets_preview_20260616.png
Assets/ArtSource/Production/F009/f009_unified_runtime_assets_checker_preview_20260616.png
```

## 已生成界面资源

运行目录：

```text
Assets/Resources/Art/UI/
```

Unity 加载 key 规则：

```text
Art/UI/<file-name-without-extension>
```

| 资源 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 账本面板 | `Assets/Resources/Art/UI/ui_panel_ledger.png` | `Art/UI/ui_panel_ledger` | 已生成 | 主羊皮纸区块面板。 |
| 夹纸卡片 | `Assets/Resources/Art/UI/ui_card_clip.png` | `Art/UI/ui_card_clip` | 已生成 | 市场货架卡和大物件卡底。 |
| 小面板 | `Assets/Resources/Art/UI/ui_small_panel.png` | `Art/UI/ui_small_panel` | 已生成 | 骰子行、命令条和小详情块。 |
| 主按钮 | `Assets/Resources/Art/UI/ui_button_primary.png` | `Art/UI/ui_button_primary` | 已生成 | 风格参考和替换候选；当前 `OnGUI` 按钮边框由代码程序化绘制以避免拉伸。 |
| 次按钮 | `Assets/Resources/Art/UI/ui_button_secondary.png` | `Art/UI/ui_button_secondary` | 已生成 | 风格参考和替换候选；当前 `OnGUI` 按钮边框由代码程序化绘制以避免拉伸。 |
| 危险按钮 | `Assets/Resources/Art/UI/ui_button_danger.png` | `Art/UI/ui_button_danger` | 已生成 | 风格参考和替换候选；当前 `OnGUI` 按钮边框由代码程序化绘制以避免拉伸。 |
| 禁用按钮 | `Assets/Resources/Art/UI/ui_button_disabled.png` | `Art/UI/ui_button_disabled` | 已生成 | 风格参考和替换候选；当前 `OnGUI` 按钮边框由代码程序化绘制以避免拉伸。 |
| 金币图标 | `Assets/Resources/Art/UI/ui_icon_coin.png` | `Art/UI/ui_icon_coin` | 已生成 | 金币和经济提示图标。 |
| 刷新图标 | `Assets/Resources/Art/UI/ui_icon_refresh.png` | `Art/UI/ui_icon_refresh` | 已生成 | 市场刷新提示图标。 |
| 设置图标 | `Assets/Resources/Art/UI/ui_icon_settings.png` | `Art/UI/ui_icon_settings` | 已生成 | 设置入口图标。 |
| 关闭图标 | `Assets/Resources/Art/UI/ui_icon_close.png` | `Art/UI/ui_icon_close` | 已生成 | 关闭和返回提示图标。 |
| 目标图标 | `Assets/Resources/Art/UI/ui_icon_target.png` | `Art/UI/ui_icon_target` | 已生成 | 目标分和进度提示图标。 |
| 出售图标 | `Assets/Resources/Art/UI/ui_icon_sell.png` | `Art/UI/ui_icon_sell` | 已生成 | 出售操作提示图标。 |
| 骰袋图标 | `Assets/Resources/Art/UI/ui_icon_bag.png` | `Art/UI/ui_icon_bag` | 已生成 | 骰袋容量提示图标。 |

### 骰子悬浮窗 UI 素材

运行目录：

```text
Assets/Resources/Art/UI/Tooltip/
```

Unity 加载 key 规则：

```text
Art/UI/Tooltip/<file-name-without-extension>
```

| 资源 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 悬浮窗主面板 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_panel_clean.png` | `Art/UI/Tooltip/ui_tooltip_panel_clean` | 已生成 | F014 竖版单层浮窗空面板，文字和点面由程序绘制；避免纸页厚度和账册装订。 |
| 悬浮窗侧栏细轨 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_side_rail.png` | `Art/UI/Tooltip/ui_tooltip_side_rail` | 已生成 | 侧边页签的细轨道，不表现书脊或纸页。 |
| 悬浮窗当前页签 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_tab_active.png` | `Art/UI/Tooltip/ui_tooltip_tab_active` | 已生成 | 当前页签底图，文字由程序绘制。 |
| 悬浮窗骰效页签 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_tab_idle_blue.png` | `Art/UI/Tooltip/ui_tooltip_tab_idle_blue` | 已生成 | 骰效页签底图，文字由程序绘制。 |
| 悬浮窗质效页签 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_tab_idle_green.png` | `Art/UI/Tooltip/ui_tooltip_tab_idle_green` | 已生成 | 品质效果页签底图，文字由程序绘制。 |
| 悬浮窗卖价胶囊 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_price_chip.png` | `Art/UI/Tooltip/ui_tooltip_price_chip` | 已生成 | 卖价字段底图和金币符号，具体价格由程序绘制。 |
| 悬浮窗骰效标签 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_label_chip_blue.png` | `Art/UI/Tooltip/ui_tooltip_label_chip_blue` | 已生成 | 骰子效果字段标签底图，文字由程序绘制。 |
| 悬浮窗质效标签 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_label_chip_green.png` | `Art/UI/Tooltip/ui_tooltip_label_chip_green` | 已生成 | 品质效果字段标签底图，文字由程序绘制。 |
| 悬浮窗点面格 | `Assets/Resources/Art/UI/Tooltip/ui_tooltip_face_cell.png` | `Art/UI/Tooltip/ui_tooltip_face_cell` | 已生成 | 单个点面格底图，点数 pip 由程序绘制。 |

源图和接触图：

```text
Assets/ArtSource/Production/Tooltip/tooltip_vertical_side_tabs_clean_v10_20260616.png
Assets/ArtSource/Production/Tooltip/tooltip_runtime_ui_assets_preview_20260616.png
Assets/ArtSource/Production/Tooltip/runtime_ui_sources/
```

源图副本：

```text
Assets/ArtSource/UI/
```

## 已生成改造道具图标

运行目录：

```text
Assets/Resources/Art/Items/
```

Unity 加载 key 规则：

```text
Art/Items/<file-name-without-extension>
```

| 资源 | 当前文件 | Unity 加载 key | 状态 | 备注 |
|---|---|---|---|---|
| 加印石图标 | `Assets/Resources/Art/Items/affix_add_stone.png` | `Art/Items/affix_add_stone` | 已生成 | 2026-06-11 由 F005 方向 A 接触图裁切抠透明；官署石印、绿色加号和新封印表达随机添加 1 条合法词缀；透明通道已校验。 |
| 剥印石图标 | `Assets/Resources/Art/Items/affix_remove_stone.png` | `Art/Items/affix_remove_stone` | 已生成 | 2026-06-11 由 F005 方向 A 接触图裁切抠透明；裂开石印、碎蜡封和刮片表达随机删除 1 条已有词缀；透明通道已校验。 |
| 换印石图标 | `Assets/Resources/Art/Items/affix_replace_stone.png` | `Art/Items/affix_replace_stone` | 已生成 | 2026-06-11 由 F005 方向 A 接触图裁切抠透明；双印章、环形箭头和新旧封印表达先删除再随机添加 1 条合法新词缀；透明通道已校验。 |

源图和接触图：

```text
Assets/ArtSource/Items/_f005_affix_stone_direction_a_contact_sheet_20260611.png
Assets/ArtSource/Items/_f005_affix_stone_final_icons_preview_20260611.png
Assets/ArtSource/Items/_source_chromakey/
```

## 其它已生成资源

| 资源 | 当前文件 | Unity 加载 key | 备注 |
|---|---|---|---|
| 桌面背景 | `Assets/Resources/Art/table_background.png` | `Art/table_background` | 2026-06-02 以 `Bright Ledger Boardgame` 风格重生成；用于主流程全屏桌面背景。 |
| 骰盅 | `Assets/Resources/Art/dice_cup.png` | `Art/dice_cup` | 2026-06-02 以 `Bright Ledger Boardgame` 风格重生成；透明道具图，用于摇骰阶段。 |

源图和旧资源位置：

```text
Assets/ArtSource/Backgrounds/table_background_generated_20260602.png
Assets/ArtSource/Props/_source_chromakey/dice_cup_source_20260602.png
Assets/ArtSource/Legacy/table_background_legacy_dark_realistic.png
Assets/ArtSource/Legacy/dice_cup_legacy_dark_realistic.png
```
