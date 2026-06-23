# Dice King Project Context

This file records stable design decisions and implementation notes for future conversations in this Unity project.

For complete gameplay flow, read `GAME_FLOW.md` before making design or implementation changes.
For confirmed dice archetype designs, read `DICE_ARCHETYPES.md`.

## New Session Bootstrap

At the start of a new project conversation, read these files in order:

1. `AGENTS.md`
2. `PROJECT_CONTEXT.md`
3. `GAME_FLOW.md`
4. `DICE_ARCHETYPES.md` when working on dice types, scoring, markets, or dice effects

Current implementation source of truth is `Assets/Scripts/DiceKingDemo.cs`. If code behavior and docs disagree, inspect the code first, then update docs and implementation together.

## Product Direction

- Working title: `骰子王`.
- Genre target: roguelite score-builder built around dice rolls, inspired by the score-engine feel of `Balatro`, but using readable dice types, long-term dice state, and board-like encounter rules.
- Tone: light, witty, and relaxed, but not childish. Prefer dry humor, tabletop tavern, odd royal bureaucracy, ledgers, seals, dice cups, and small-citizen absurdity. Avoid overly cute or preschool wording.
- Core fantasy: the player is not simply gambling. They are building a dice engine that turns randomness into a controlled scoring system.

## Core Loop

The current end-to-end flow is tracked in `GAME_FLOW.md`. Keep this section as the high-level roguelite scoring loop only.

1. Enter a small encounter with a target score.
2. Roll the current dice bag for the encounter, up to six dice.
3. Roll, apply dice type effects, temporary dice effects, hand scoring, encounter rules, and any later modifiers.
4. If the score reaches the target, resolve stage-clear fixed gold and interest income after any roll-time wallet income, then enter the inter-stage dice market.
5. Inter-stage and chapter markets let the player buy dice, buy crafting items, use market-stage crafting items, sell dice, refresh offers, or leave if the dice bag is not empty.
6. Continue through configured normal encounters and boss encounters.
7. Any small-stage failure ends the run immediately and clears run progress.

## Confirmed Demo Scope

- The current prototype is implemented as a self-contained single-file demo in `Assets/Scripts/DiceKingDemo.cs`.
- The demo self-bootstraps at scene load via `RuntimeInitializeOnLoadMethod`, so `Assets/Scenes/SampleScene.unity` can be opened and played without manually wiring a component.
- 当前原型覆盖 `GAME_FLOW.md` 中确认的主流程：主菜单、设置、首次进入开场、默认初始骰袋、小关循环、结果决策、每关一次最多三颗实体骰的出千改点、关间市场、章节市场、胜利和失败。
- Prototype continue/save uses `PlayerPrefs` and restores to the start of the saved stage with current gold, dice collection, die faces, dice type, dice material, growth counters, settings, and opening-seen state. It does not restore a partially played stage.
- 当前 `DiceData` 写入骰子类型 key、当前面组、大树成长层数、材质 key、前缀词缀列表和后缀词缀列表；F005 道具持有和流式钱包接入后运行存档使用 `SaveVersion = 4`，读取到旧版本运行记录时直接清空本轮记录，保留菜单设置。
- F013 词缀与改造道具软关闭已接入，并已由用户完成软关闭验证：`global.csv` 中 `affix_feature_enabled = 0` 时，F005 词缀和三类改造道具保留配置、资源和存档兼容，但市场不生成/不显示/不购买/不使用改造道具，骰袋和结果不显示词缀，前缀不加分，后缀不加钱包金币。F013 无词缀基线重平衡首轮候选已静态接入，仍需 Unity Play Mode、截图和前 3 章样本验证。
- Current generated art assets live under `Assets/Resources/Art/`:
  - `table_background.png`: regenerated full-screen tabletop UI background in the current bright flat style.
  - `dice_cup.png`: regenerated transparent dice cup prop used during rolling in the current bright flat style.
  - `DiceTypes/basic_die_icon.png`
  - `DiceTypes/piggy_die_icon.png`
  - `DiceTypes/turtle_die_icon.png`
  - `DiceTypes/shellsmith_die_icon.png`
  - `DiceTypes/nest_die_icon.png`
  - `DiceTypes/slow_turtle_die_icon.png`
  - `DiceTypes/double_die_icon.png`
  - `DiceTypes/odd_die_icon.png`
  - `DiceTypes/even_die_icon.png`
  - `DiceTypes/lone_witness_die_icon.png`
  - `DiceTypes/stamp_die_icon.png`
  - `DiceTypes/half_step_die_icon.png`
  - `DiceTypes/track_die_icon.png`
  - `DiceTypes/parity_neighbor_diff_die_icon.png`
  - `DiceTypes/parity_neighbor_same_die_icon.png`
  - `DiceTypes/parity_complete_die_icon.png`
  - `DiceTypes/parity_review_die_icon.png`
  - `DiceTypes/parity_flip_score_die_icon.png`
  - `DiceTypes/parity_hold_score_die_icon.png`
  - `DiceTypes/parity_turner_die_icon.png`
  - `DiceTypes/tree_die_icon.png`
  - `DiceTypes/gardener_die_icon.png`
  - `DiceTypes/irrigation_die_icon.png`
  - `DiceTypes/gambler_die_icon.png`
  - `DiceTypes/treasury_die_icon.png`
  - `DiceTypes/bribe_die_icon.png`
  - `DiceTypes/investment_die_icon.png`
  - `DiceFaces/runtime_die_face_base.png`: generated blank die face used by runtime pip rendering.
  - `DiceRoll/f009_unified_ready_die_256.png`: F009 统一待机骰，用于 `Ready` 和结算后重置阶段。
  - `DiceRoll/f009_unified_spin_loop_strip_24f_256.png`: F009 统一 24 帧旋转循环 strip，用于 `Shaking` 阶段固定槽位内旋转。
  - `DiceRoll/f009_unified_spin_stop_strip_8f_256.png`: F009 统一 8 帧停转预览 strip，用于 `Stopping` 阶段减速显点前的过程表现。
  - `DiceFaces/f009_unified_result_die_faces_6x256.png`: F009 统一 1 到 6 点结果骰面 strip，用于 `ResultDecision` 和 `Scoring` 阶段；`7+` 点仍回退到程序点阵。
  - `DiceRoll/f009_table_friction_spin_loop_strip_24f_256.png`、`DiceRoll/f009_table_friction_spin_stop_strip_8f_256.png`: F009 旧版桌面摩擦 strip，仅作为统一资源缺失时的运行回退。
  - `UI/ui_panel_ledger.png`, `UI/ui_card_clip.png`, `UI/ui_small_panel.png`: generated parchment UI section/card/detail textures.
  - `UI/ui_button_primary.png`, `UI/ui_button_secondary.png`, `UI/ui_button_danger.png`, `UI/ui_button_disabled.png`: generated runtime button textures.
  - `UI/ui_icon_coin.png`, `UI/ui_icon_refresh.png`, `UI/ui_icon_settings.png`, `UI/ui_icon_close.png`, `UI/ui_icon_target.png`, `UI/ui_icon_sell.png`, `UI/ui_icon_bag.png`: generated runtime UI icons.
  - `Items/affix_add_stone.png`, `Items/affix_remove_stone.png`, `Items/affix_replace_stone.png`: F005 market crafting item icons.
  - Full generated-asset registry is tracked in `ART_ASSETS.md`.
- F008 金币收益族九宫格接触图已生成在 `Assets/ArtSource/Production/F008/f008_gold_income_dice_contact_sheet_20260615.png`；九颗最终透明骰子类型图标已生成到 `Assets/Resources/Art/DiceTypes/`，最终预览图在 `Assets/ArtSource/Production/F008/f008_gold_income_dice_final_icons_preview_20260615.png`。
- F010 奇偶短规则骰第二版接触图已由用户接受，七颗最终透明骰子类型图标已生成到 `Assets/Resources/Art/DiceTypes/`，最终预览图在 `Assets/ArtSource/Production/F010/f010_odd_even_short_rule_final_icons_preview_20260615.png`。
- F011 大树长期成长接口骰第三版接触图已由用户接受，七颗最终透明骰子类型图标已生成并接入加载 key：`point_seed_tree_die_icon.png`、`pattern_tree_die_icon.png`、`canopy_tree_die_icon.png`、`ring_tree_die_icon.png`、`fertilizer_tree_die_icon.png`、`pruning_tree_die_icon.png`、`root_tree_die_icon.png`；最终预览图在 `Assets/ArtSource/Production/F011/f011_tree_growth_interface_final_icons_preview_20260616.png`。
- Current runtime shader resources:
  - `Assets/Resources/Shaders/DiceMaterialOverlay.shader`: shared F002 dice material shader used to render material differences without generating separate dice art for each material.
- Current chapter structure:
  - Global economy constants are loaded from `Assets/Resources/Data/global.csv`.
  - Encounter structure is loaded from `Assets/Resources/Data/chapter_score_table.csv`.
  - Dice market prices, sell prices, tiers, and chapter-band offer weights are loaded from `Assets/Resources/Data/dice_market_config.csv`.
  - Market refresh costs and high-tier refresh pity are loaded from `Assets/Resources/Data/market_rule_config.csv`.
  - Dice material price modifiers, sell modifiers, chapter-band weights, display names, and short rules are loaded from `Assets/Resources/Data/dice_material_config.csv`.
  - `global.csv` 中 `affix_feature_enabled` 是 F013 词缀功能总开关；当前值为 `0`，表示 F005 词缀和改造道具隐藏且不生效，但不删除数据或存档字段。
  - F013 无词缀基线目标曲线已改为平滑梯度版：`chapter_score_table.csv` 使用全 40 关曲线。前三章按“6 基础公平骰 + 3 次出手 + 不出千”的单关数学基线重调为明显递减，目标为第一章 `60 / 74 / 70 / 70`，第二章 `78 / 70 / 86 / 82`，第三章 `100 / 88 / 82 / 94`；对应单关不出千通过率约从 `95.43%` 降到 `51.62%`，每小关均高于 `50%`。第三章后目标从 `94` 平滑接到第四章 `104 / 115 / 127 / 140`，终章为 `1141 / 1261 / 1393 / 1539`，不再保留旧版第三章到第四章断崖和旧终局峰值。
  - F003 市场生成规则已接入当前原型：抽类型、材质和面组模板前按当前章节过滤；每次市场后台保留 1 个隐藏轻度倾向槽位，按本市场刚购买路线优先、当前骰袋唯一主路线次之、否则通用池的顺序推断路线，并以 `70% / 30%` 尝试路线池 / 通用池。
  - F013 正式经济候选已接入：`starting_gold = 8`，`stage_clear_base_gold = 3`，剩余出手和剩余出千金币奖励保持 `0`，基础利息为每 `6` 金 `+1`、每关封顶 `4`。
  - F013 正式市场基线已接入：`market_test_random_refresh = 0`，市场读取章节开放、路线倾向、高阶保底和 `dice_market_config.csv` 权重；基础骰权重为 `0 / 0 / 0` 且代码层排除，不作为随机市场商品；付费刷新会优先避开上一批货架中出现过的骰子类型；改造道具是否生成不依赖测试开关，而由 `affix_feature_enabled` 独立控制。
  - F004-01 投骰表现默认配置由 `Assets/Resources/Data/roll_feedback_config.csv` 读取；调试期可用 `Application.persistentDataPath/roll_feedback_config.csv` 覆盖，并用 `F5` 运行时重载当前全局配置。
  - F004-01 在 `BeginShakeRoll` 起摇时捕获本次投骰配置快照；后续重载只影响下一次投骰，不写入 `PlayerPrefs`，也不消耗 `UnityEngine.Random`。
  - F004-02 已将旧的“敲空格延迟停止”状态机替换为固定加力窗口：窗口内有效 `Space` 只按配置快照施加表现冲量、受去抖和上限约束；窗口结束进入回落，回落阶段额外 `Space` 不改变强度、时长或结果。
  - F004-03 已替换运行时代码中的旧投骰提示文案；配置层仍保留加力窗口、提示脉冲和旧骰盅运动参数，供回退路径或后续特殊表现复用。
  - F009 序列帧式固定槽位旋转首版已接入当前原型：主投掷区使用 OnGUI 序列帧播放器表现小关入场、槽位内旋转、左到右停转显点和结算点名；默认 F009 路径取消骰盅阶段，不再做抛投、跨桌位移或持续摇晃形变。`Ready` 和结算后重置阶段优先读取 `Assets/Resources/Art/DiceRoll/f009_unified_ready_die_256.png`，非基础骰由代码叠加小类型标记；`Shaking` 优先读取 `Assets/Resources/Art/DiceRoll/f009_unified_spin_loop_strip_24f_256.png` 的 24 帧统一旋转 strip；`Stopping` 优先读取 `Assets/Resources/Art/DiceRoll/f009_unified_spin_stop_strip_8f_256.png` 的 8 帧统一停转预览 strip；`ResultDecision` 和 `Scoring` 的 1 到 6 点结果优先读取 `Assets/Resources/Art/DiceFaces/f009_unified_result_die_faces_6x256.png`。旧桌面摩擦 strip 和程序点阵仍作为资源缺失或 `7+` 点回退。停转阶段按槽位从左到右逐步停住，停住的槽位立即切回真实 `EffectiveValue` 点数绘制，进入 `ResultDecision` 后保持稳定结果供出千或结算选择；玩家确认结算后不再插入单独的“结果定格 / 账本定格”停顿，直接从原槽位高亮、轻抬和分数飞字进入左到右点名。视觉帧只读取 `RollPhase`、当前槽位、`EffectiveValue`、`SettlementDisplayEvent` 和计分展示状态，不调用 `ScoreDice()`，不写入 `PlayerPrefs`，不改变 `SaveVersion`、骰面概率、出千、钱包、成长、临时小骰或结算公式。调试期可用 `F6` 开关该视觉层以回退到原 2D 展示路径；仍需 Unity Play Mode、录屏和 `1280x720` / `1920x1080` 截图验收。
  - Dice face template rules for F002 are currently code constants in `DiceKingDemo.cs`; they can move to CSV after playtest tuning.
  - Current configured run has 10 chapters.
  - Each chapter has 3 normal encounters and 1 boss encounter.
  - Chapter targets use the point-value scoring scale where die face value is the base score, not `value * 10`.
  - If the CSV is missing, empty, or invalid, code falls back to four first-chapter encounters.
  - If `global.csv` is missing, empty, or invalid, code uses fallback economy constants only to keep the prototype playable.
  - If market CSVs are missing, empty, or invalid, code uses fallback market configs only to keep the prototype playable.
- Current start:
  - Formal design starts from 6 basic dice.
  - Non-basic dice are acquired through inter-stage market/events.
  - The old material/trait starter routes are removed from the active prototype flow.
  - Starting wallet is loaded from `global.csv`; current F013 formal baseline is `starting_gold = 8`.

## Design Pillars

- Dice should be readable by `type` first:
  - Faces: raw probability and base values.
  - Type: the main mechanical identity, such as piggy, turtle, double, odd, even, basic, tree, gambler, treasury, bribe, and investment.
  - Material: a secondary shop uniqueness axis for concrete market dice, never the first identity layer.
  - State: long-term counters or round configuration, such as piggy target face, tree growth, tree hit face, gambler threshold, stage-start wallet snapshot, and investment budget.
- Dice types should create different play patterns. F002 material is intentionally narrow: one material per shop-generated die, with no multi-affix, reforging, negative affixes, or large trait matrix in the first implementation pass.
- Future dice expansion has moved past the first internal route-completion pass. Stable follow-up direction is to break closed build walls by designing dice as more general mechanism providers: each new or reworked die should state its trigger condition, benefit type, and settlement interface, so it can be absorbed by multiple builds instead of only serving one fixed route. Type identity still remains the first readability layer, and individual new dice still require approved rule cards before implementation.
- 后续骰子设计优先使用已确认的通用触发点白名单：指定点数、指定牌型、最高点数、回合结束、金币、出千和槽位触发。新骰子或重做骰子应优先从这些触发点中选择或组合，再定义收益类型和结算接口；如果需要新增白名单外触发，必须在设计对话或生产包中说明原因，避免随意设计孤立条件。
- 龟龟后续优化的确认方向是“临时小骰余波 + 结算连锁表演”的通用机制提供者：龟龟应更多读取非龟骰、相邻槽位、牌型成立、金币支付、出千改点、高面命中或其它 build 成功事件，再转成临时小骰余波；避免重新变成多龟龟互相套娃的封闭套装。
- F011 大树重设方向是“长期成长接口”的通用机制提供者：新大树类骰读取上述触发点，触发后直接真实成长，表现为触发骰自身全部面 `+1` 和现有 `Growth +1`；不新增发芽、芽点、成长进度或 `GrowthProgress`。不把主要收益做成临时小骰，也不直接写成“成长一次给金币”。旧 `Tree / Gardener / Irrigation` 已从正式市场权重和测试全池随机中下架，但代码、旧图标和旧结算逻辑保留用于兼容旧存档与历史引用。
- F001-B 龟龟路线首版规则已接入当前原型并由用户确认验收；本轮进一步按后续壳链底层方向更新生成逻辑：`Turtle` 用 `floor(v / 2)` 得到起始壳链种子，`SlowTurtle` 用 `ceil(v / 2)` 得到起始壳链种子，之后都按 `dN -> d(N-1) -> ... -> d1` 逐级生成临时小骰。`Shellsmith` 按本次龟龟系临时小骰数量给自身每颗 `+1` 单骰分，`Nest` 按有产出的龟龟链补 d1 临时小骰且不闭环。
- F001-C 奇偶路线第一批补强骰已接入当前原型并由用户确认验收：`LoneWitness` 自然结果锁定后落单自动重摇一次，`Stamp` 有同点伙伴时固定 6 分，`HalfStep` 只在顺子判定中可低一格借位，`Track` 在实体骰包含 `2 / 4 / 6` 时固定 8 分。
- F010 奇偶短规则骰已接入当前原型代码、数据和最终透明图标，并于 2026-06-16 通过 Unity Play Mode 验证：`ParityNeighborDiff` 左邻奇偶不同则本骰 6 分，`ParityNeighborSame` 左邻奇偶相同则本骰 6 分，`ParityComplete` 只差本骰时补全全奇 / 全偶副牌型，`ParityReview` 自然结果破坏全奇 / 全偶时自动重摇一次，`ParityFlipScore` 出千后奇偶变了则本骰 8 分，`ParityHoldScore` 出千后奇偶没变则本骰 6 分，`ParityTurner` 读取全局出千改点结果，不再覆盖为相反奇偶面随机重摇。QA 报告见 `Docs/QA/F010/20260616_f010_validation_report.txt`。
- F011 大树长期成长接口骰已静态接入当前原型代码、市场数据和最终透明图标，待 Unity 运行验证：`PointSeedTree / PatternTree / CanopyTree / RingTree / FertilizerTree / PruningTree / RootTree` 分别读取指定点数、指定牌型、最高点数、回合结束、当前金币、出千改善和相邻类型触发标签。旧 `Tree / Gardener / Irrigation` 不再作为市场新刷内容，只作为兼容对象保留。
- F002 商店独特骰首版已接入当前原型：商店生成的商品骰由 `类型 + 随机面组 + 单一材质` 组成；骰子类型先决定合法面组家族，卖出价仍按类型固定回收，材质用共享 shader 做运行时表现，并保留材质名文本保底，不为每种材质重复生成骰子图片。
- F005 词缀和改造道具当前处于 F013 软关闭状态：配置、图标和存档字段继续保留，`affix_feature_enabled = 0` 时不进入市场、UI、预览、真实结算或钱包收益。后续若恢复词缀，需要另开包决定是否沿用旧存档内容。
- F008 金币收益族泛用化已接入当前原型代码和数据：`BountyGold / TopGold / HandTax / Collection / CompoundInterest / LeadTicket / ShellTax / CounterGold / LumberGold` 归入金币市场路线；出手内收益按当前槽位左到右写入钱包，复利只在通关收益阶段追加，伐木只削本次左邻单骰分。
- F008 九颗最终运行时图标已接入 `Assets/Resources/Art/DiceTypes/`，代码按 `Art/DiceTypes/<file>` 加载；若资源缺失仍会回退到程序化骰面。
- F008 金币收益族、F010 奇偶短规则骰和 F011 新大树接口骰已进入 F013 正式市场基线，并在本轮提高开放后的刷新权重；`market_test_random_refresh = 0` 时按章节开放、路线倾向和 `dice_market_config.csv` 权重刷出，不再默认走测试期全池随机。
- Rewards should support both committed routes and cross-route experimentation.
- Bosses should be rule distortions, not just larger target numbers.
- Dice archetypes are tracked in `DICE_ARCHETYPES.md`. Current confirmed archetype directions include:
  - Piggy economy: hit a random target face to generate wallet value for the dice market.
  - Turtle iteration: start a shrinking shell chain that generates temporary dice for extra single-die score.
  - Odd/even control: shape the current dice-bag hand toward parity and repeated-value outcomes.
  - Tree growth: F011 reads visible trigger points such as specified point, specified hand, highest face, real settle, wallet size, cheat improvement, and adjacent type triggers, then directly increases the triggering tree die's own faces.
  - Double/gambler burst: provide direct high-score choices with clear expected value boundaries.
  - Gold operation: 国库骰奖励持有本金，贿赂骰用可用钱包补最终分，投资骰在小关开始预付预算换取持续单骰加分；F008 新增点数、牌型、出千、小骰数量、槽位和利息触发的金币收益骰。

## Current Dice Types

- Basic: no special effect; current role is starter dice and fallback compatibility, not a random market refresh offer.
- Piggy: each roll gets a random target face; hitting it earns gold.
- Turtle：使用有效点数的 `floor(v / 2)` 作为壳链种子，并按 `dN -> d(N-1) -> ... -> d1` 生成龟系临时小骰。
- Shellsmith：龟龟路线收益骰；本次每生成 1 颗龟龟系临时小骰，每颗 `Shellsmith` 自身获得 `+1` 单骰分。
- Nest：龟龟路线补骰骰；有产出的 `Turtle` / `SlowTurtle` 链会按 `Nest` 数量补 d1 临时小骰，不触发闭环。
- SlowTurtle：龟龟路线来源骰；使用有效点数的 `ceil(v / 2)` 作为壳链种子，并按同一套逐级衰减壳链生成龟系临时小骰。
- Double: doubles its own base die score.
- Odd: rolls only odd values.
- Even: rolls only even values.
- LoneWitness：奇数路线追同稳定骰；自然结果锁定后，如果本骰有效点数没有其它实体骰同点，本骰自动重摇自己 1 次并接受新结果；出千确认后的强制结果不触发。
- Stamp：奇数路线同点兑现骰；有任意其它实体骰与本骰同点时，本骰基础单骰分先替换为 6，再接受遭遇规则修正。
- HalfStep：偶数路线顺子辅助骰；只在顺子识别时可用原有效点数或低一格借位值，每颗骰一次判定只能服务一个值，不改变显示点数、有效点数或单骰分。
- Track：偶数路线目标兑现骰；当前实体骰结果同时包含 `2 / 4 / 6` 时，本骰基础单骰分先替换为 8，再接受遭遇规则修正。
- F010 奇偶短规则骰：`ParityNeighborDiff` 左邻奇偶不同固定 6 分；`ParityNeighborSame` 左邻奇偶相同固定 6 分；`ParityComplete` 只补全全奇 / 全偶副牌型；`ParityReview` 自然破坏全奇 / 全偶时重摇一次；`ParityFlipScore` 出千变奇偶固定 8 分；`ParityHoldScore` 出千守奇偶固定 6 分；`ParityTurner` 读取全局出千改点结果，不再改写候选池。
- 旧大树三件套兼容对象：`Tree` 每次出手随机命中点数并在命中后永久成长；`Gardener` 放大自然命中的旧大树成长；`Irrigation` 用自身点数补未自然命中的旧大树成长。三者已从市场新刷内容下架。
- F011 新大树接口骰：`PointSeedTree` 命中指定点成长；`PatternTree` 命中指定牌型成长；`CanopyTree` 掷出自身最高面成长；`RingTree` 每次真实结算成长一次；`FertilizerTree` 按当前金币和 `interest_gold_step` 直接成长多次且无单次上限；`PruningTree` 本骰出千后结果改善、倍率改善或帮助通关时成长；`RootTree` 左右相邻实体骰本次触发自身类型效果时成长一次。
- Gambler: each roll gets a random threshold from 1 to 5; values below it score zero, equal values score normally, and values above it use an expectation-preserving multiplier.
- Treasury：读取小关开始钱包快照，按 `floor(wallet / 10)` 给本骰单骰分加分；F013 当前单颗上限为 `6`。
- Bribe：基础本次分确定后，只在能补到通关时花可用钱包补最终分；每 1 金 +3 最终分，每颗贿赂骰每次结算最多花 2 金。
- Investment：小关开始投骰前自动锁定并预付预算；每颗投资骰最多锁 2 金，每锁 1 金让本骰在本小关内每次结算 +2 单骰分。
- F008 金币收益族：`BountyGold` 命中自身面组悬赏点 +2 金；`TopGold` 最大面 +1 金；`HandTax` 成牌型 +1/+2 金；`Collection` 每小关首次真实结算 +1 金；`CompoundInterest` 通关时按基础利息追加且总额封顶；`LeadTicket` 本骰被确认出千 +2 金；`ShellTax` 临时小骰数达 3 时 +2 金；`CounterGold` 左邻本次入账时 +1 金；`LumberGold` 左邻本次单骰分 -1 并 +2 金。
- 商店材质：官铁让本骰单骰分 `+1`；鎏印在掷出当前面组最高点时让钱包 `+1` 金；明釉在掷出当前面组最高点时本骰单骰分 `+2`；铅封在本骰被确认出千后本次单骰分 `+2`；铜骨在本骰与任意其它实体骰同点时本骰单骰分 `+1`。材质不改变有效点数或牌型识别，临时小骰不携带材质。

## Scoring Notes

- Current scoring implementation baseline:
  - Roll score uses `round(sum of six individual die scores * final hand multiplier)`.
  - Stage score is cumulative across the available rolls; configured `target_score` is compared against the cumulative stage score.
  - One face point is one base score point; the old `value * 10` prototype scale is not used.
  - The six dice are evaluated as one poker-like hand.
  - Each settlement takes only one exclusive main hand multiplier by priority.
  - All odd / all even are side hands; they use `max(main hand multiplier, parity multiplier)` instead of multiplying with the main hand.
  - Current hand scoring intentionally uses a short integer-multiplier table: six-kind, five-kind, four-kind, straight, three-kind, all-odd, all-even, or no hand.
  - Pair, two-pair, three-pair, full-house, double-triple, small-straight, and large-straight subtypes are intentionally not separate scoring hands.
  - The fair-dice baseline average multiplier is about `x1.52`.
  - The full multiplier table and probability notes are tracked in `GAME_FLOW.md`.
- 贿赂骰补分发生在基础本次分之后，是最终分补差，不参与牌型倍率；国库骰和投资骰是单骰分加成，会进入基础本次分并参与最终倍率。
- F005 词缀配置仍支持每颗实体骰最多 2 前缀 + 2 后缀，但当前 F013 默认软关闭；关闭期间旧存档词缀会被读取并保存，但不显示、不提供单骰分加成、不提供钱包金币。临时小骰仍不继承、不触发词缀。
- F001-C 固定分顺序采用方案甲：盖章骰和轨道骰先把本骰基础单骰分替换为固定分，再按有效点数接受遭遇规则修正；半步骰只扩展本次顺子识别的候选值，不新增全局牌型。
- F010 固定分顺序沿用 F001-C 方案甲：异邻、同邻、翻号和守号先替换本骰基础单骰分，再按有效点数接受遭遇规则修正；补全只影响全奇 / 全偶副牌型识别，复核只在自然开盅后触发，转号只读取本骰是否被确认出千改点，不再改写全局出千候选池。
- F011 成长顺序沿用真实结算后入档：旧大树仍按自然命中优先并兼容园丁 / 灌溉；新大树在 `ScoreDice()` 真实结算中收集触发并进入同一个 `pendingTreeGrowth` 队列，结算展示完成后统一让本骰面组和 `Growth` 入档。根系只读取非根系骰的结构化类型触发标签，不解析 `RoundNote` 文本，也不让根系成长反向触发其它根系。
- Individual die scoring should combine:
  - Individual die value.
  - Dice type effects.
  - Temporary small dice effects.
  - Long-term dice state such as tree growth or piggy counters.
  - Encounter rule modifiers.
  - Later relic or modifier systems if reintroduced.
- Current encounter rules:
  - `OddLedger`: odd-valued dice get `+1` single-die score.
  - `HandAudit`: any recognized main hand multiplier above `x1.00` adds `+2` rule score before the final multiplier.
  - `LowFog`: dice with effective value `<= 2` lose `1` single-die score, floored at `0`.
  - `DoubleJudge`: even dice get `+1` single-die score, odd dice lose `1` single-die score, and straight adds `+4` rule score before the final multiplier.
- Useful combo families:
  - Three/four of a kind.
  - Five-run-or-better straights.
  - All odd / all even.
- The fun target is for players to understand basics in 5 minutes, form a route in 15 minutes, and hit a memorable score spike in 25 minutes.

## Economy Notes

- Current gold economy is configured in `Assets/Resources/Data/global.csv`.
- New runs currently start with `starting_gold = 8` as the F013 formal baseline; `1000` gold is no longer the active balance configuration.
- F013 后小关通关会发放 `stage_clear_base_gold = 3` 的固定金币；剩余出手和剩余出千金币奖励仍保持 `0`。
- F013 软关闭期间，猪猪、鎏印和 F008 金币收益族仍按当前骰袋槽位从左到右直接进入钱包；F005 金币后缀保留在存档中但不生效。
- Current interest rule is easy to remember: every 6 gold held after本次出手收入和通关固定金币 gives +1 interest, capped at +4 per stage.
- Piggy dice add `piggy_gold_per_hit = 1` directly to wallet per hit. Future relics that create gold should specify whether they enter the same left-to-right wallet stream or only modify stage-clear economy.
- 鎏印材质命中当前面组最高点时直接让钱包 `+1` 金，不进入本次分数。
- 国库骰读取小关开始钱包快照，投资在小关开始扣钱包；贿赂在确认结算后扣可用钱包，但其可用上限按当前骰袋槽位的左到右顺序计算，较早金币来源可以被较晚贿赂读取，较晚金币来源不会回头影响较早贿赂。
- F008 出手内金币收益沿用同一左到右钱包流；柜台骰只读左邻入账，伐木骰只削本次左邻单骰分。复利骰在通关阶段读取已封顶基础利息，当前每颗最多 +2、全部合计最多 +4，不递归计息。

## UX Notes

- The first playable screen should be the actual game experience, not a marketing page.
- UI should feel like a compact tabletop tool: restrained, readable, dense enough for repeated decisions.
- After main run UI changes, follow `Docs/UI_ACCEPTANCE_FEEDBACK_WORKFLOW.md` before calling the UI accepted. Static checks alone are not enough for UI acceptance.
- Formal UI design targets `1920x1080` at `16:9`, with a minimum practical window target of `1280x720`.
- Player windows may use custom resolutions; the core UI should preserve a 16:9 safe area and scale uniformly so controls do not overlap.
- The current OnGUI prototype keeps old `1280x720` authored coordinates and maps them into a `1920x1080` fit canvas with a `1.5x` prototype-to-design transform.
- Avoid in-game tutorial walls. Short actionable labels are better than explaining every system in paragraphs.
- Keep the current demo no-asset and self-contained unless a future task explicitly adds production UI/art.
- The start-of-run build selection screen has been removed. New game enters a single default starter dice bag.
- Confirmed roll interaction follows `GAME_FLOW.md`: rolling is not triggered by a clickable button. The player presses `Space` once to start slot-fixed dice spinning, then repeatedly taps `Space` during a limited input window to add fixed presentation impulses. These taps affect dice visual intensity, audio, vibration, and prompt feedback only; they do not affect die-face probability. After the window closes, further `Space` input only gives a light "stopping / reveal" prompt, and the dice stop in place before locking the result.
- 投骰表现参数必须配置化并支持调试期运行时重载；当前投骰使用开始时的配置快照，重载后的新配置从下一次投骰生效，且不得影响骰面概率、出千、分数或结算。
- 当前 F004 / F009 投骰表现已接入加力窗口提示、窗口结束后的“停转显点”轻提示和提示脉冲计时；F009 默认由固定槽位内的序列帧式骰子旋转承担主运动，旧骰盅运动参数仅保留为回退路径或后续特殊表现素材。
- 投骰前的 `Ready` 阶段允许玩家调整骰子槽位；结果锁定后，投掷区保持当前骰袋槽位顺序，不按有效点数重排，牌型表按钮仍可查看示例和倍率。玩家可以按 `Space` 结算，或进入出千选择 1 到 3 颗实体骰点数 `+1`，最高不超过该骰当前面组最大点数，出千确认前可以取消。出千确认后，本回合目标、阈值、钱包快照和投资预算保持固定，新结果必须接受并立即进入结算。
- F007-03 静态主流程 UI 精简已接入 `Assets/Scripts/DiceKingDemo.cs`：保留 `骰子王` 标题和关卡信息；左侧上方显示积分塔，积分塔内目标分在上、当前分在下，下方显示倍率前基础分数字与倍率章；金币、出手和出千集中到左侧下方资源牌；删除主流程常驻的当前骰袋、`6 / 6 全部上场`、构成、`基础 x6` 和本关关注；中央投掷区整体放大，静态层只保留骰子行、选中/结算高亮和连续留白；右侧只保留本关规则与牌型表；底部 `Space` 提示短而居中。该实现不新增运行时 UI 贴图，仍需 Unity Play Mode 和截图验收。
- F007-04 双数字计分器状态契约已接入 `Assets/Scripts/DiceKingDemo.cs`：左端积分塔统一消费 `RunScoreCounterState`，区分 Idle / Preview / Settling / Settled 阶段；结果决策阶段显示预览累计，结算阶段使用 `BeginSettle()` 在真实 `ScoreDice()` 后冻结的基础分、倍数、倍率后贿赂补分、本次总分和结算后总分。真实计分公式、骰面概率、目标曲线、出千逻辑、数据表和存档版本未改变。
- F007-05 左到右结算跳涨演出已接入 `Assets/Scripts/DiceKingDemo.cs`：结算开始后根据 F007-04 提交态生成展示快照，基础分数字随当前槽位顺序逐颗累加，倍数在三同 / 四同 / 五同 / 六同、五连顺子以及全奇 / 全偶成立节点跳涨，当前积分数字随 `基础分 x 当前展示倍数` 推进；贿赂补分作为最终倍率后补分单独落到当前积分，不进入倍数。该实现不改真实计分公式、骰面概率、目标曲线、出千逻辑、数据表或存档版本，仍需 Unity Play Mode 和截图验收。
- F007-05 修正了预览阶段提前展示结算结果的问题：结果决策和出千选择阶段，左端当前积分只显示已真实入账的小关累计分，不显示 `currentScore + previewRollScore`；基础分数字保持 `0`，倍数保持 `×1`，玩家确认结算后才通过逐步动画推进到最终分。未达标回到等待投骰时，基础分和倍数保留上一手结算结果；下一次按 `Space` 起摇时才清零。
- F006-02 / F006-03 结算演出首轮已接入 `Assets/Scripts/DiceKingDemo.cs`：`BeginSettle()` 在唯一一次真实 `ScoreDice()` 完成后生成只读展示事件队列，`UpdateScoreReveal()` 直接按逐槽入账、路线短反馈、倍率盖章、贿赂补分和目标收束播放，不再插入单独的结果定格停顿；金币飞字、折叠余骰、成长待入档、倍率章和达标 / 未达标横幅均只读已提交状态。未实现 F006-04 加速，不改真实计分、随机、数据表、存档或骰子效果边界，仍需 Unity Play Mode 和截图验收。
- F009 序列帧式固定槽位旋转首版已接入 `Assets/Scripts/DiceKingDemo.cs`：小关开始和每次未达标回到 `Ready` 时，六骰按槽位优先显示统一待机骰，非基础骰叠加小类型标记，不套额外骰框，也不显示槽位编号角标；按 `Space` 后每颗骰在原槽位优先播放 F009 统一旋转 strip，运动期隐藏可读点数；加力窗口结束后锁定真实结果，停转阶段从左到右播放统一停转预览 strip 并逐颗显真实点数；1 到 6 点结果和结算点名优先显示统一结果骰面，`7+` 点回退到程序点阵。确认结算后直接用原槽位高亮、轻抬和分数飞字点名。程序已按骰子家族接入基础帧速、相位和停转差异，让龟龟、大树、金币、奇偶和爆发类骰子有不同旋转节奏。资源缺失时仍回退到旧桌面摩擦 strip 或程序离散帧；运行验收和截图验收仍未完成。

## Art Direction Notes

- Use the project-specific Codex skill `$wabish-art-assets` for future generated art assets. A normal request can be just an asset type plus keywords.
- For uncertain directions, new visual families, batch art, replacement art, or assets intended for `Assets/Resources/Art/`, use the art skill's early direction gate first: present compact direction cards or one representative pilot/contact sheet, get user approval, then continue batch generation and runtime placement.
- Stable visual direction: `Bright Ledger Boardgame` / `明亮账本桌游风`, a bright 2D flat board-game illustration style with clean rounded shapes, soft cel shading, subtle paper texture, and playful royal-bureaucracy props.
- Preferred motifs: dice, ledgers, wax seals, official stamps, small crowns, coin pouches, ribbons, stamped forms, desk props, and tidy tabletop game pieces.
- Tone should be bright, cute, witty, and relaxed, but not childish. Avoid chibi mascots, baby-face expressions, sticker-sheet looks, plush-toy rendering, casino glam, photorealistic 3D, neon sci-fi, and preschool styling.
- Production assets should favor chunky readable silhouettes, light clear colors, controlled dark outlines, minimal texture, visible material cues, and minimal/no generated text so Unity UI can own final copy.
- 骰子类型图标必须把机制符号融入骰子本体。优先使用骰面标记、骰边切角、骰角封章、绑带、嵌入标记、刻槽、点阵变形或表面镶嵌；避免把骰子放在大型外部道具、箭头、轨道、台阶、空位或说明性背景元素旁边，形成“骰子 + 场景说明”的构图。
- 运行时骰子表现先用生成的骰子类型图标原图作为投掷前和重置后的身份展示，不额外套骰框；出现结果后切换到生成的空白骰面底图并由代码绘制点数。结果骰保留更清晰的类型色条、类型图标和短状态标签，用于表达出千改点选择、大树成长、猪猪目标、赌徒阈值和龟龟临时小骰。基础骰结果使用更安静的中性色标记，避免重复显示“基础”抢占注意力；非基础骰保留更强的类型身份。主投掷结果不使用数字徽章替代点数。
- 后续商店购买骰子的视觉方向已确认：在不大改当前主场景布局的前提下，商店骰和投掷区非基础骰优先使用更厚实的 3/4 桌游骰表现，强化彩色材质、圆角倒角、厚点数、清晰接触阴影和材质表面线索；主界面仍沿用现有左侧积分塔、中央投掷区、右侧规则牌和底部 `Space` 条，悬浮窗只作为轻量桌游详情层，不替代常驻界面或大卡片。商店待购骰和主投掷区骰共用同一套组件骨架；后续排版默认先走最小满足版：竖版紧凑简介卡，不显示右侧“面 / 效 / 质”页签，不表现纸页厚度、书脊、账册装订或翻页感；默认只显示骰子名称、六格点面、骰子效果、品质效果和卖出价；骰子效果说明由 `Assets/Resources/Data/dice_type_config.csv` 的 `tooltip_effect` 提供，只用于展示，不参与规则结算；统计、边界标签、价格规则解释和额外状态先不显示，待验证阅读缺口后再逐项加回。F014 已按该方向生成悬浮窗 UI 运行时素材，路径为 `Assets/Resources/Art/UI/Tooltip/`；程序已在 `Assets/Scripts/DiceKingDemo.cs` 静态接入统一 hover 绘制、`0.12s` 停留延迟、`0.10s` 淡入、`0.08s` 淡出、相邻目标软替换和 `Shaking / Stopping / Scoring / StageFailed` 阶段抑制；侧栏和页签素材保留但当前运行时不加载、不绘制。该实现不改变规则、市场、计分、出千、钱包、成长、存档或词缀开关，仍需 Unity Play Mode 与截图验收。
- Runtime UI presentation uses scalable parchment panels/buttons drawn procedurally in OnGUI, with generated `Assets/Resources/Art/UI/` assets kept as style references, replacement candidates, and fixed-size UI icons. Do not stretch full decorative UI textures into arbitrary button or panel rectangles.
- Source images, chroma-key intermediates, contact sheets, and old visual explorations should stay under `Assets/ArtSource/`, not under `Assets/Resources/Art/`.

## Near-Term Optimization Points

- Replace `OnGUI` prototype UI with proper Unity UI Toolkit or uGUI once mechanics stabilize.
- Split `DiceKingDemo.cs` into separate runtime modules:
  - Data definitions.
  - Run state.
  - Scoring engine.
  - Reward generation.
  - UI presentation.
- Add deterministic seed support for reproducible balancing.
- Add automated scoring tests outside scene UI.
- Improve the current dice market:
  - Continue tuning CSV offer weights, target scores, discounts, and chapter-specific market differences after F003 playtest data.
  - Add lock/favorite protection before selling important growth dice.
  - Add clearer route guidance for why a market offer helps the current bag.
- Add encounter preview text that highlights why a route may care about the rule.
- Tune chapter targets after playtesting the default starter dice bag across the new dice type routes.
- Track per-run stats: highest score, best combo, total gold earned, and route-defining dice.

## Implementation Cautions

- This is currently a prototype, not a production architecture.
- Keep future changes scoped: avoid introducing a large framework before the scoring and reward loop are validated.
- When adding mechanics, prefer adding a small number of readable route interactions over many isolated effects.
- If a change affects scoring, update this file with the intent and add or update tests when a test harness exists.
- If a change affects a dice archetype, update `DICE_ARCHETYPES.md` with the route intent, settlement boundary, and anti-abuse rule.
- Keep `PROJECT_CONTEXT.md`, `GAME_FLOW.md`, and `DICE_ARCHETYPES.md` synchronized whenever current code behavior changes.
