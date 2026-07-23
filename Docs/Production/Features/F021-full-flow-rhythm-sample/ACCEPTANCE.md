# F021 验收记录

状态：F021-S1 运行实现已通过人工回归；F021-S2-R3 待完整回归；五条结果动态已通过；LED V4 与市场实体键帽 V2 已接入；F021-S5-UI1 已静态接入并通过独立编译，Tip 位置为全局中上，待 Play Mode 回归
功能：F021 全流程节奏样片

## 就绪度门禁

| 角色 | 就绪度 | 缺口 / 风险 | 建议补丁 | 阻塞问题 |
|---|---|---|---|---|
| 策划 | 就绪 | 交付格式未最终选择 | 先按关键帧加可动效版本推进 | 无 |
| 美术 | 就绪 | 首轮可能在完成度和速度之间摇摆 | 先做五锚点接触图，再补过渡和合成样片 | 无 |
| 界面体验 | 就绪 | 需要在样片中控制信息密度 | 按状态只保留一个主焦点 | 无 |
| 程序 | 需补充 | 样片冻结前不适合实现 | 样片满意后再拆正式实现切片 | 无 |

## 验收清单

- [ ] 策划规则已确认，或待回答问题已明确记录。
- [ ] 所需美术资源已完成，或已明确后置。
- [ ] 界面体验状态已实现，或已明确后置。
- [ ] 程序行为符合已确认规则。
- [ ] 执行顺序和职能依赖已记录。
- [ ] 相关主文档已同步。
- [ ] 验证结果已记录。

## 样片专项验收

- [ ] 样片覆盖主菜单、开始新游戏、主流程、投骰、停转、结算、胜场、轻面板、市场购买、离开市场和下一关 Ready。
- [ ] 五锚点接触图已先行通过：主菜单、Ready、超额井喷、轻结算面板、市场。
- [ ] 样片有明确美术表现，不是纯线框、纯灰盒或纯几何示意。
- [ ] 画面遵守 `Pixel Ledger Scoring Machine` 风格。
- [ ] 达标越线后继续结算，最终分明显超过目标分。
- [ ] 最终分定格后再触发金币效果。
- [ ] 轻结算面板只显示必要奖励和分数信息。
- [ ] 市场只演示购买一个骰子和离开市场。
- [ ] 样片资源保存在 `Assets/ArtSource/Production/F021/`。
- [ ] 样片未被登记为最终运行时资源。
- [ ] 五锚点通过前，没有进入全量过渡帧和样片合成。

## 职能验证记录

### 2026-07-08 美术首轮方向接触图

- 角色：`$wabish-art-production`
- 状态：样张待验收
- 范围：仅生成五锚点方向接触图，不生成最终运行时资源，不改代码。
- 生成位置：`Assets/ArtSource/Production/F021/`
- 方向 A：`Assets/ArtSource/Production/F021/f021_anchor_contact_sheet_direction_a_pixel_ledger_machine_20260708.png`
- 方向 B：`Assets/ArtSource/Production/F021/f021_anchor_contact_sheet_direction_b_bright_boardgame_20260708.png`
- 方向 C：`Assets/ArtSource/Production/F021/f021_anchor_contact_sheet_direction_c_paper_mechanical_theater_20260708.png`

快速检查：

- 三套接触图均覆盖主菜单、Ready、超额井喷、轻结算面板和市场五个锚点。
- 三套均保持积分塔、六槽骰台、规则牌、金币奖励和三格市场货架等核心识别。
- 井喷表现使用积分塔、灯泡、纸带、盖章和金币口，不使用老虎机、拉杆、轮盘或赌场化奖励机。
- 市场画面与结算画面分离，保留三格货架、购买高亮和骰袋。
- 生成图中的文字和数字只作为抽象占位，不作为最终 UI 文案或数值来源。
- 三套接触图是方向样张，不是完整 `1920x1080` 单锚点关键帧，也未进入 `Assets/Resources/Art/`。

### 2026-07-08 美术比例修正

- 触发原因：首轮 A / B / C 三张接触图把五个锚点横向并排塞进同一张图，导致每个锚点被压成竖向窄画幅，不符合已确认的 `16:9` 横版屏幕需求。
- 用户反馈：B 版较符合方向，但比例需要修正。
- 修正策略：以 B 版明亮桌游账本风为主方向，重新生成五张独立 `16:9` 横版锚点方向图，并额外拼一张保持横版比例的五锚点总览图。
- 状态：样张待验收。

修正后文件：

- `Assets/ArtSource/Production/F021/f021_b_v2_landscape_main_menu_anchor_20260708.png`
- `Assets/ArtSource/Production/F021/f021_b_v2_landscape_ready_anchor_20260708.png`
- `Assets/ArtSource/Production/F021/f021_b_v2_landscape_score_burst_anchor_20260708.png`
- `Assets/ArtSource/Production/F021/f021_b_v2_landscape_clear_panel_anchor_20260708.png`
- `Assets/ArtSource/Production/F021/f021_b_v2_landscape_market_anchor_20260708.png`
- `Assets/ArtSource/Production/F021/f021_b_v2_landscape_five_anchor_contact_sheet_20260708.png`

快速检查：

- 五张独立锚点图均为约 `16:9` 横版比例，实际生成为 `1672x941`。
- 总览接触图中每个缩略锚点保持横版比例，不再使用竖条分栏。
- 主菜单、Ready、超额井喷、轻结算面板和市场均保留 B 版的明亮账本桌游气质。
- 轻结算面板仍需用户确认是否继续减轻；市场锚点可作为当前较优方向。
- 修正图仍仅为 `Assets/ArtSource/Production/F021/` 下的源图样张，未进入 `Assets/Resources/Art/`，未改代码。

### 2026-07-13 开始界面专项接触图与界面动效状态图

- 角色：`$wabish-art-production`、`$wabish-art-assets`
- 状态：样张待验收
- 范围：仅细化 `F021-S1` 的开始界面美术、菜单状态和开机待机表现，不生成运行时切片，不改代码。
- 美术接触图：`Assets/ArtSource/Production/MainMenuStart/main_menu_start_art_ui_ue_contact_sheet_v1_20260713.png`
- 界面动效状态图：`Assets/ArtSource/Production/MainMenuStart/main_menu_start_ui_ue_state_sheet_v1_20260713.png`

本轮锁定的样张内容：

- 采用左侧待机计分塔、中央一字六骰、右侧四行账页菜单的横版构图。
- 四行菜单图标依次表达开始新游戏、继续游戏、设置和退出游戏；最终文字仍由 Unity 绘制。
- 无存档状态只禁用继续游戏；有效存档状态高亮继续游戏并预留紧凑摘要牌。
- 覆盖存档使用独立警示弹层，包含返回和危险确认两个动作。
- 开机表现拆为冷启动、短校准和可操作待机三个同机位状态；骰子全程静止，灯光不代表计分、达标或奖励。
- 菜单开机表现属于 `MainMenu` 内部视觉子状态，不替代首次新游戏后的黑幕睁眼 `Opening`，不写入 `SeenOpening`。
- 六颗待机骰使用打散且含重复值的点数组合，不形成 `1-2-3-4-5-6` 顺子展示，也不暗示牌型玩法。

静态复核：

- 美术方向、四菜单结构、六骰数量、无文字底图、覆盖确认和开机状态表达通过。
- 两张图片均为 `1672x941` 不透明评审图，只能作为构图、状态和后续透明拆件生产依据，不能直接裁切为运行时素材。
- 正式界面仍需由 Unity 补齐按钮文案、存档摘要、覆盖后果说明和键鼠 / 手柄焦点行为。

### 2026-07-13 用户评审：开始界面入口布局与主流程转场

- 当前美术状态：需修订
- 用户提出的问题：开始新游戏、继续游戏、设置和退出全部放在右侧，不符合常见开始界面的主操作阅读习惯；现有构图若继续衔接主游戏 `Ready`，需要明显移动或替换中央机台、右侧菜单和骰台，容易形成突兀的界面重排。
- 当前判断：两项问题来自同一结构原因，即开始界面过度复用主流程的左右分栏，把主菜单做成了右侧次级栏，同时没有保证主菜单和 `Ready` 共用完全相同的固定底板。
- 现有两张 `20260713` 样张继续保留为问题定位和美术语言参考，不作为可冻结的运行时母版，不进入透明拆件或 `Assets/Resources/Art/`。

待用户确认的建议修订方向：

- 主菜单改为位于画面中央的王室公文浮层；开始新游戏和继续游戏回到视觉中心，设置和退出降低层级。
- 浮层背后直接使用与主游戏 `Ready` 相同机位、相同尺寸和相同组件位置的计分机底板；左侧积分塔、中央六骰托盘和右侧规则牌不在转场中换位。
- 继续游戏或已看过开场的新游戏只移除中央菜单浮层，再点亮积分塔、填入右侧规则牌并出现底部 `Space` 提示，以共享组件状态变化完成转场。
- 首次新游戏仍先进入独立黑幕睁眼 `Opening`，再回到同一 `Ready` 底板；菜单开机表现不能替代 `Opening`。
- 在用户确认上述入口结构前，不生成下一版接触图。

### 2026-07-13 用户确认新版开始界面生产

- 用户已确认采用“中央王室公文菜单浮层 + 与 `Ready` 完全共用固定底板”的修订方向，并要求生成新版接触图和转场分镜。
- 新版接触图必须保持左侧积分塔、中央六骰、右侧规则牌、底部操作条和镜头位置不变；主菜单只增加中央上半部公文浮层以及既有组件的待机遮罩。
- 转场分镜分别表现继续游戏 / 已看开场的新游戏的直接进入路径，以及首次新游戏经墨迹黑幕进入 `Opening` 后再到 `Ready` 的路径。
- 当前状态：新版样张生产中；仍不生成运行时透明切片，不改代码。

### 2026-07-13 新版中央公文菜单接触图与转场分镜

- 角色：`$wabish-art-production`、`$wabish-art-assets`
- 状态：样张待验收
- 范围：只交付开始界面新版美术关键帧和转场导演分镜；不生成运行时透明切片，不修改 Unity 代码或玩法数据。
- 新版接触图：`Assets/ArtSource/Production/MainMenuStart/main_menu_center_decree_contact_keyframe_v2_20260713.png`
- 新版转场分镜：`Assets/ArtSource/Production/MainMenuStart/main_menu_to_ready_transition_storyboard_v2_20260713.png`
- 早期转场分镜：`Assets/ArtSource/Production/MainMenuStart/main_menu_to_ready_transition_storyboard_v1_20260713.png`，仅保留为迭代对照，不作为导演基准。

静态复核：

- 两张新版图片均为 `1672x941` 不透明源图评审资产，保存在 `Assets/ArtSource/Production/MainMenuStart/`，未进入 `Assets/Resources/Art/`。
- 新版接触图通过：中央王室公文浮层恢复了主操作焦点；浮层位于六骰上方，不遮挡玩法身份；左侧积分塔、中央六槽、右侧规则牌和底部操作条与 `Ready` 共用固定机位。
- 新版分镜通过：上排用“继续游戏”与“已看开场的新游戏”两个入口图标汇流到同一直接转场；下排只表现首次新游戏经盖章、墨迹吞黑、纯黑 `Opening` 和睁眼渐显进入 `Ready`。
- 分镜中的浮层只向上收起，墨迹和黑幕只作为覆盖层变化；固定底板组件不换位、不缩放、不推镜，睁眼画面不使用金边或传送门式轮廓。
- 最终 `Ready` 保持当前分为 `0`、无胜利满灯、无金币喷发；右侧规则内容和底部操作提示仅在进入可操作状态后出现。
- 直接路径只共用视觉动画，不合并数据动作：继续游戏需要在浮层遮挡期间恢复真实存档骰袋，已看开场的新游戏需要建立默认六骰；输入在转场期间保持锁定，且数据动作不得重复触发。
- 生成图中的细小文字、数字和图标只作为导演占位，不作为最终 Unity 文案、数值或可直接裁切的运行时资源。

### 2026-07-13 用户复评：开始界面继续简化

- 当前美术状态：需修订。
- 用户反馈：中央公文浮层虽然解决了右侧菜单和底板换位问题，但作为开始界面仍然元素过多、结构过重；现阶段应先比较多个更简化的静态首屏方向，再决定后续过渡和界面动效。
- 处理结论：`main_menu_center_decree_contact_keyframe_v2_20260713.png` 和两版转场分镜继续保留为迭代记录，不作为可冻结母版；所有开始转场、`Opening` 衔接和界面动效生产立即暂停。
- 本轮只生成一张四方向极简开始界面接触图，不生成转场分镜、不生成运行时透明切片、不修改 Unity 代码或玩法数据。

四方向共同约束：

- 四版只比较构图和信息密度，继续使用同一 `Pixel Ledger Scoring Machine` 屏幕级美术基因。
- 首屏最多使用一个主容器、一个主视觉和三件装饰物，留白不少于约 `45%`。
- 保留 `开始新游戏 / 继续游戏 / 设置 / 退出游戏` 四项功能；开始新游戏为第一主操作，继续游戏为第二操作，设置和退出降为弱入口。
- 标题和正式按钮文字仍由 Unity 绘制；接触图只预留文字安全区，不生成长文字、存档摘要或覆盖提示。
- 完整积分塔、六槽骰台、右侧规则牌、金币口、底部 `Space` 条和桌面散件全部移出首屏，不再用完整 `Ready` 机台承担主菜单。
- 四个方向依次为：一纸王令、皇冠骰徽、封存账本、极简待机表；用户选出相对确定的方向后，才继续单张细化、菜单状态和转场设计。

生成结果：

- 当前状态：样张待验收。
- 四方向接触图：`Assets/ArtSource/Production/MainMenuStart/main_menu_minimal_four_direction_contact_sheet_v3_20260713.png`
- 图片规格：`1672x941`、不透明 PNG，仅作为方向评审源图；未进入 `Assets/Resources/Art/`。
- A「一纸王令」：中央窄公文承担唯一主容器，两个主行和两个弱入口集中在同一张纸上。
- B「皇冠骰徽」：左侧单颗皇冠骰作为玩法识别，右侧窄菜单承担四项入口。
- C「封存账本」：合上的皇家账本作为唯一主物件，四个书签边签承担菜单层级。
- D「极简待机表」：小型审核牌只保留一根指针和一盏待机灯，下方集中四项入口。
- 四版均删除完整 `Ready` 机台、六骰阵列、右侧规则牌、金币口和底部操作条；生成图只保留 `A / B / C / D` 方向编号，不生成菜单文字或伪文案。
- 静态美术复核：整体 `PASS`。A 的窄竖公文略像移动端表单，B 的大骰略接近标题宣传画，C 的两个小书签入口可发现性较弱；三项均属于选中方向后再处理的轻微警告，不妨碍本轮比较。D 在保留少量计分机识别与极简信息密度之间最稳定。
- 本轮到此停止；用户选出相对确定方向前，不生成单张精修稿、菜单状态、转场分镜或界面动效。

### 2026-07-13 街机主题重新探索

- 用户明确旧有“王室、账本”关键词属于历史负担，本轮方向探索不再继承这些叙事与装饰约束；此前所有开始界面和转场样张只保留为历史迭代记录。
- 保留游戏名与核心设定“骰子王”，但“王”解释为六骰街机巡回赛的最高冠军称号，不代表君王、宫殿或贵族体系。
- 背景母题暂按“复古未来城市中的六骰街机巡回赛”制作方向样张：玩家携带六颗骰子挑战不同机厅，六槽竞技台负责读取骰子、执行左到右结算并显示目标与当前分。
- 当前状态：三方向样张已生成，待用户选型与评审。
- 本轮范围：生成约三套主题一致但表现不同的开始界面与 `Ready` 主游戏界面接触图；不生成转场、结算爆发、市场画面、运行时透明切片，不修改 Unity 代码或玩法数据。

三套方向：

- A「明亮城市巡回」：明亮、热血、像街机体育巡回赛，强调排行榜、场馆灯牌与标准六槽竞技台。
- B「午夜旧机厅」：雨夜、CRT 暖光和最后一台老机的轻悬疑，强调机台记忆与街角机厅氛围。
- C「霓虹大秀」：用百老汇式顶箱、追逐灯泡、观众剪影和大型电子分牌强化大秀感，但不使用赌场滚轴、拉杆、赔付线或 `777` 中奖组合。

共同功能约束：

- 每套接触图同时展示开始界面和 `Ready` 主游戏界面，两者必须属于同一场馆与同一机台视觉系统。
- 开始界面保留标题安全区、开始新游戏、继续游戏、设置和退出游戏四项入口；不把完整主游戏信息提前塞入首屏。
- `Ready` 必须清楚保留目标分、当前分、金币 / 资源、一次出手状态、中央六颗实体骰、从左到右槽位顺序、单张规则插件和底部短 `Space` 提示。
- `Ready` 不显示出千、牌型表、额外投骰按钮、胜利满灯或金币喷发；六骰保持稳定待机，不预演结算结果。
- 三套图片只作为 `Assets/ArtSource/Production/` 下的方向评审源图，用户选定方向前不进入 `Assets/Resources/Art/`。

生成结果：

- A「明亮城市巡回」：`Assets/ArtSource/Production/ArcadeDiceKing/arcade_dice_king_direction_a_city_circuit_contact_sheet_v1_20260713.png`，`1717x916`、不透明 PNG。
- B「午夜旧机厅」：`Assets/ArtSource/Production/ArcadeDiceKing/arcade_dice_king_direction_b_last_arcade_contact_sheet_v1_20260713.png`，`1774x887`、不透明 PNG。
- C「霓虹大秀」：`Assets/ArtSource/Production/ArcadeDiceKing/arcade_dice_king_direction_c_neon_revue_contact_sheet_v1_20260713.png`，`1672x941`、不透明 PNG。
- 三张均为“开始界面 + Ready 主游戏界面”的并排方向接触图；只用于比较世界气味、构图、机台语言和 UI 层级，不作为可直接裁切的运行时素材。
- 本轮未生成转场、市场、结算爆发或透明拆件，未修改 Unity 代码与玩法数据。

静态方向复核：

- A 条件通过：明快、六骰与逐槽关系清楚，玩法辨识最稳；后续需补清目标分 / 当前分双数字关系，弱化赛道、方格旗等赛车联想。
- B 需修订后再冻结：开始界面最克制、故事钩子最强，目标分 / 当前分也最清楚；但资源区 `骰子 x3` 容易误读为三次投掷，右侧图标表与投币式机柜组合有老虎机赔付表联想。
- C 条件通过且最贴“百老汇 / 电子大秀”偏好：六骰、逐槽灯、规则插件和底部操作入口清楚；后续需明确目标分 / 当前分与“一关一次投掷”，并控制观众、追光和操作键的节奏游戏联想。
- 三版开始界面都通过本轮简化目标：主游戏 HUD 没有提前塞入首屏，四项菜单只保留层级占位；B 留白最克制，A 的背景信息相对最多。
- 三版目前都重复使用皇冠图形。它可被解释为排行榜冠军标志，但仍可能重新触发王室误读；选型后的精修版应将皇冠限制为一次性的冠军徽记，或改为高分名牌 / 六点星 / 冠军灯牌。
- 当前方向建议排序为 `C -> A -> B`：C 最贴用户希望的电子百老汇感，A 最稳妥易读，B 的叙事气味最强但系统与赌场误读风险最高。

### 2026-07-13 午夜旧机厅开始界面三版细化

- 角色：`$wabish-art-production`、`$wabish-art-assets`、`$imagegen`
- 用户选择：以 B「午夜旧机厅」作为当前深化母体，并认可“一屏、两层、一个键”进入三版开始界面比较。
- 当前状态：样张待验收。
- 本轮范围：只生成开始界面；不生成主游戏、六骰行、目标分、规则插件、转场分镜或运行时透明拆件，不修改 Unity 代码和玩法数据。

生成结果：

- A「CRT 存档槽」：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuBRefinement/arcade_dice_king_b_main_menu_refine_a_crt_save_slot_v1_20260713.png`
- B「机框系统键」：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuBRefinement/arcade_dice_king_b_main_menu_refine_b_bezel_system_controls_v1_20260713.png`
- C「机械记忆终端」：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuBRefinement/arcade_dice_king_b_main_menu_refine_c_mechanical_memory_terminal_v1_20260713.png`
- 三张均为 `1672x941` 不透明 PNG，只作为方向评审源图，未进入 `Assets/Resources/Art/`。

共同通过项：

- 三版都只保留单台 `DICE KING` 旧街机、两级主入口、设置 / 退出弱入口和一个通用确认键，没有把四项功能堆到右侧竖栏。
- 三版均删除皇冠、账本、王室装饰、`骰子 x3`、赔付表、滚轴、拉杆和追灯节拍；没有显示主游戏六骰、目标分或规则插件。
- 午夜雨夜、熄灭的周边机台、深海军蓝磨损金属和暖琥珀显示继续维持 B 方向的“最后一台旧机”气味。
- 生成图中的空白横线、背景机台招牌和输入符号只用于构图占位，存在少量伪文字；最终中文标签、存档摘要和输入提示必须由 Unity 绘制。

分版复核：

- A：界面体验 `PASS`。继续存档主卡、开始新游戏次入口、设置 / 退出页脚层级最清楚，键鼠与手柄焦点最容易统一；风险是四项都落在屏内，实体街机因果略弱。
- B：综合 `PASS / WARN`。开始 / 继续属于屏幕内容，设置 / 退出属于机框实体系统键，物理街机可信度和故事气味最强；风险是焦点跨越屏幕与实体机框两层，中央发光卡仍略像现代界面卡片。
- C：结构 `FAIL`、材质可保留。机械槽与黑玻璃有气味，但更像工业终端或双硬盘仓，无法清楚承载存档摘要，并会暗示不存在的插卡操作；不建议作为下一轮结构母版。
- 下一轮只需在 A 与 B 中选型：A 偏可用性，B 偏实体街机可信度。若选 B，应把中央卡片改为嵌入式单行琥珀选择窗并补小型换项控制。

### 2026-07-13 用户选择 A 并启动运行时试制方案

- 用户选择 A「CRT 存档槽」作为后续细化、UI 切图和主菜单完善的结构母版。
- 当前美术状态：结构方向已选，运行时资源与实现方案待用户批准执行。
- 本轮只形成方案，未生成新的运行时切图，未修改 `Assets/Scripts/DiceKingDemo.cs`。
- 当前接触图不能直接裁切：占位横线、选中光、CRT 扫描线、玻璃反光和背景伪文字已经互相烘焙，正式生产必须先补无文字、无选中光的 clean plate。

推荐运行结构：

- 静态大层：机厅环境底图、机台外壳、熄灭顶箱与 Space 键槽。
- 动态 CRT 层：主卡、次卡、设置、退出、存档摘要、焦点游标、禁用与按下状态、覆盖 / 退出确认和设置页。
- 效果层：CRT 玻璃、扫描线 / 噪点、顶箱发光、Space 键帽与键光、可选雨线和地面反光呼吸。
- 运行资源计划约 13 到 16 个，独立放入 `Assets/Resources/Art/UI/MainMenuArcade/`；源文件放入 `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuBRefinement/RuntimeSources/`。

推荐菜单规则：

- 有有效存档：大主卡为继续游戏并显示最多两行摘要，次卡为开始新游戏，默认焦点为继续游戏。
- 无存档：大主卡为开始新游戏，次卡为禁用的继续游戏，默认焦点为开始新游戏。
- 设置与退出位于 CRT 页脚，使用图标和短文字；覆盖存档与退出均使用同一 CRT 模态框，默认焦点为取消。
- 中文文案、存档数值、焦点、禁用、按下、输入提示和模态内容全部由 Unity 绘制，不烘入纹理。

当前实现风险：

- `DrawMainMenu()` 仍是羊皮纸面板和四个纵向鼠标按钮，需要替换为 CRT 菜单结构。
- `DrawSceneBackdrop()` 当前对所有模式共用桌面背景，必须按主菜单 / 设置单独绘制街机底图，不能影响主游戏和市场。
- 主菜单尚无键盘 / 手柄焦点索引；接入前不能显示方向键与 `Space` 导航提示。
- 新游戏覆盖目前只是二次点击同一按钮，没有取消按钮；需要真正的模态状态。
- 设置页仍是旧羊皮纸样式，应在同一 CRT 内原位显示。

建议验收顺序：先完成无存档、有存档和覆盖确认三种状态，在 `1920x1080` 与 `1280x720` 截图验收；通过后再做设置页、退出确认、雨景与发光细化。

### 2026-07-13 A 结构三版细化接触图

- 角色：`$wabish-art-production`、`$wabish-art-assets`、`$imagegen`
- 用户要求：以已确认的 A「CRT 存档槽」为唯一结构母版，生成三版不同的细化开始界面接触图；本轮暂时不切图。
- 当前状态：三张样张已生成，待用户选型。
- 本轮范围：只生成完整开始界面源图；不生成 clean plate、透明切片、主游戏界面或转场，不修改 Unity 代码和玩法数据。

生成结果：

- A「暖琥珀存档卡」：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuARefinementV2/arcade_dice_king_main_menu_a_refine_v2_amber_save_card_20260713.png`
- B「冷青档案终端」：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuARefinementV2/arcade_dice_king_main_menu_b_refine_v2_teal_archive_20260713.png`
- C「首次启动」：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuARefinementV2/arcade_dice_king_main_menu_c_refine_v2_first_run_20260713.png`
- 三张均为 `1672x941`、`Format24bppRgb` 不透明 PNG，只位于 `Assets/ArtSource/Production/`，未进入 `Assets/Resources/Art/`。

共同通过项：

- 三版都保持单台正视旧街机、`DICE KING` 顶箱、“大主卡 + 次入口 + 页脚设置 / 退出 + 单一实体确认键”的同一结构，没有回到右侧竖排菜单或四项等权标签。
- A / B 展示有存档状态，C 展示无存档状态；菜单位置不变，只替换主入口内容、禁用信息与默认焦点。
- A / B 的继续游戏摘要明确写出“从本关开始”，不暗示恢复到小关中途；C 使用“开启新的六骰挑战”，不暗示只投一颗骰子。
- 三版均未出现六骰行、目标分、规则插件、皇冠、账本、王室装饰、滚轴、拉杆、`777`、赔付表或节奏轨道。
- 菜单文字已用于细化稿的信息层级审核，但仍属于接触图内容；正式中文、存档数值、焦点、禁用与输入提示必须由 Unity 动态绘制。

分版用途：

- A：暖琥珀主卡的动作优先级最直观，用于比较更亲切、明确的继续游戏入口。
- B：冷青磷光承载摘要、琥珀承载焦点；顶部无功能刻度已删除，用于比较更技术化、更像旧机记录终端的气味。
- C：留白最多并明确显示“暂无运行记录”；主图标已改为新记录符号，用于验证首次启动时开始新游戏自然升为主卡的状态规则。
- 最终静态复核：A / B / C 均为 `PASS`；B 远景机台屏幕已压暗为无字黑玻璃，三版未发现会阻断选型的伪文字或主题误读。

本轮停止条件：等待用户选择视觉密度与 CRT 材质方向；选型前不拆图、不生成运行时资源、不接入 `DiceKingDemo.cs`。

### 2026-07-14 用户选择 C 与氛围细化方案

- 用户确认 A 结构细化稿中的 C「首次启动」可以继续细化；C 成为开始界面的视觉密度与氛围母版，A / B 只保留为对照参考。
- 用户希望加入右侧窗外持续雨滴、雨点击窗后的向下流痕、室内灯光偶发屏闪，以及主机 CRT 偶发成像模糊或信号异常。
- 当前状态：视觉方向已选，氛围细化方案待评审；本轮未生成新图、未切运行时资源、未修改 Unity 代码。

推荐导演规则：

> 历史提案：以下低频组合规则在用户否决组合动态后，已由后文第 13–16 轮的“单独吊灯断电 + 快速随机循环”替代，仅保留决策过程。

- 约 80% 的待机时间只保留雨与极弱 CRT 底噪，约 15% 出现外围氛围事件，真正的灯闪或 CRT 信号故障低于约 5%。
- 远雨持续低对比循环；玻璃流滴每 4 到 9 秒低频生成，单条流动约 5 到 10 秒，最多同时 1 到 2 条。
- 吊灯每 22 到 45 秒允许一次 0.25 到 0.55 秒的轻微掉亮，亮度变化约 6% 到 10%，不完全熄灭；`DICE KING` 顶箱保持稳定。
- CRT 轻失焦每 28 到 60 秒随机一次，持续约 0.25 到 0.45 秒且峰值不超过约 0.8 像素；水平同步错位每 45 到 90 秒最多一次，持续约 0.12 到 0.22 秒且位移不超过 2 像素；两者互斥。
- 菜单进入后的前 8 秒、任意输入后的 1 秒、设置 / 确认弹层和转场期间暂停全部低频故障。
- 近景大流滴、吊灯波动和 CRT 异常同一时刻最多出现一种；不使用固定节拍循环，不逐步加强故障，不出现黑帧、白闪、整屏熄灭或异常人影。

推荐生产门禁：

1. 先生成一张 C 氛围定稿关键帧，确认常态雨夜、灯光层级和 CRT 常态可读性。
2. 再生成一张六格动效状态图，覆盖常态雨夜、窗面流滴、吊灯掉亮、CRT 轻失焦、水平同步错位和确认保护状态。
3. 两张评审图通过后，才制作 clean plate、透明雨水图集、独立发光层和 CRT 覆盖层。
4. 首轮不做 `RenderTexture` 或 CRT Shader；真实局部模糊、色散和曲面扭曲后置。

候选验收：

- 30 秒待机录屏中持续可见雨与少量流痕，灯闪和 CRT 异常各最多出现 0 到 1 次。
- 任一 CRT 异常造成的文字不可读时间不超过 0.2 秒，偏移不超过 2 像素，输入与焦点不丢失。
- 窗雨不越过窗框，吊灯、顶箱和 CRT 外溢光不同时闪；画面不形成节拍游戏或恐怖故障片气质。
- `1920x1080` 与 `1280x720` 下中文、焦点、禁用态和输入提示都保持清楚。
- 提供减少闪烁选项时，保留雨景但关闭随机灯闪和强 CRT 错位。

### 2026-07-14 C 常态雨夜氛围预览

- 角色：`$wabish-art-production`、`$wabish-art-assets`、`$imagegen`
- 当前状态：静态预览已获用户接受，动态预览可以启动。
- 用户要求：先生成一张预览图，效果满意后再继续动态预览和正式资源制作。
- 验收结果：用户于 2026-07-14 确认可以按既定方案继续；构图、视觉密度、冷暖关系、雨窗材质、菜单层级与 CRT 常态基准冻结。
- 生成文件：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_atmosphere_preview_v1_20260714.png`
- 图片规格：`1672x941`、`Format24bppRgb` 不透明 PNG；仅位于 `Assets/ArtSource/Production/`，未进入 `Assets/Resources/Art/`。

静态检查：

- `PASS`：保持单台正视机柜、大主卡、禁用次入口、页脚设置 / 退出和单一实体确认键。
- `PASS`：右侧窗体出现分层雨幕、玻璃水珠、汇流和下滑水痕，水迹没有越过窗框进入机台或菜单。
- `PASS`：冷蓝雨夜与暖琥珀 CRT / 吊灯形成明确层级，周边机台保持熄灭，`DICE KING` 顶箱稳定。
- `PASS`：开始新游戏、六骰挑战、禁用继续游戏、设置、退出和输入提示保持可读；未生成额外主游戏 HUD、王室、账本、赌场或节奏轨道元素。
- `WARN`：静态关键帧只能评审常态雨夜、材质和灯光，不能证明灯闪、CRT 失焦、同步错位的发生频率、互斥与恢复节奏。

门禁结果：30 秒同机位分层合成动态预览已经生成，进入下一节的用户验收；动态预览通过前不制作正式 clean plate、透明切片或 Unity 实现。

### 2026-07-14 C 开始界面动态氛围预览

- 角色：`$wabish-art-production`、`$wabish-art-assets`
- 当前状态：用户已拒绝，组合动态方案停止。
- 用户要求：开始制作已经批准的动态预览，本轮仍不切正式运行时资源、不修改 Unity。
- 视频：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_motion_preview_v1_20260714.mp4`
- 海报：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_motion_preview_v1_poster_20260714.png`
- 合成规则：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/motion_preview_filtergraph_v1.ffgraph`
- 规格：`1920x1080`、30 fps、30.00 秒、H.264 High、`yuv420p`、无声、3,393,031 字节。
- SHA-256：`B3ECC22818A18592FEDD4A1F6B29E69E806F5F4FB7505FB2D0BE10D71F16F548`

时序：

- `0–8s`：仅持续雨幕、窗面水珠运动和极弱 CRT 底噪，菜单优先可读。
- `8–14s`：演示一次玻璃流滴，未与灯光或 CRT 异常重叠。
- `16.00–16.45s`：演示一次轻微吊灯掉亮，使用两个很短的波动组成一个事件，顶箱保持稳定。
- `26.00–26.18s`：演示一次温和 CRT 轻失焦、色偏重影和局部水平错位，随后立即恢复。

检查：

- `PASS`：构图、菜单文字、焦点、机柜轮廓、冷蓝雨夜和暖琥珀主层级与已接受关键帧一致。
- `PASS`：远雨持续运动，流滴、灯光掉亮和 CRT 异常互斥，没有固定节拍递进、黑帧、白闪、整屏熄灭或恐怖惊吓。
- `PASS`：`DICE KING` 顶箱、菜单焦点和输入提示保持稳定；CRT 峰值约 0.18 秒，`1920x1080` 与 `1280x720` 缩放抽查均可辨认中文。
- `PASS`：文件可完整解码，时长、帧率、画幅和编码符合评审交付要求。
- `WARN`：本视频是无声导演样片，事件时间点用于展示，不代表未来运行时固定在相同时刻；正式实现仍需使用独立视觉随机源和交互保护窗口。

用户反馈：吊灯效果只是简单局部明暗变化，没有形成“断电后重新亮起”的明确因果；同时不再接受雨、流滴、灯光和 CRT 多项效果放在同一轮评审。该视频只保留为历史记录，不进入 clean plate 或运行时资源生产。

### 2026-07-14 吊灯单效果修订门禁

- 当前状态：方案已获用户确认，5 秒专用样片已生成并等待验收。
- 本轮范围：只讨论左上昏黄吊灯的断电—复亮，不讨论雨水、玻璃流滴、CRT、顶箱或其它环境动态。
- 失败根因：灯芯、灯罩光晕、左墙受光和地面暖反射均烘在同一张常亮底图里，后期局部压暗不能真实移除光照贡献。
- 推荐资源结构：一张不含吊灯暖光的灯灭底图，加灯芯、近场光晕、墙面 / 地面环境反射三个透明叠加层。
- 推荐时序：`0.06s` 熄灭、`0.28s` 停黑、`0.06s` 点亮、`0.24s` 暖光恢复，总长约 `0.64s`；不使用连续抖闪、白闪、整屏黑帧或镜头震动。
- 运行逻辑边界：定义为吊灯局部线路瞬断，机台 CRT、`DICE KING` 顶箱、菜单焦点和输入状态不受影响。

下一门禁：用户验收 5 秒吊灯专用预览；该预览通过后才制作吊灯正式候选资源。

### 2026-07-14 吊灯断电—复亮专用预览

- 角色：`$wabish-art-production`、`$wabish-art-assets`、`$imagegen`
- 当前状态：用户评价“还可以”并批准进入 Unity 正式实现。
- 灯灭源状态：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_lamp_off_state_v1_20260714.png`
- 5 秒预览：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_lamp_power_cut_preview_v1_20260714.mp4`
- 六帧评审图：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_lamp_power_cut_review_sheet_v1_20260714.png`
- 合成规则：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/lamp_power_cut_preview_filtergraph_v1.ffgraph`
- 视频规格：`1920x1080`、30 fps、5.00 秒、H.264 High、无声、891,033 字节；SHA-256 `5384068D24B4C6375457FE63698929571F622795BF0223C310F5A2FB6D362861`。

时序：

- `0–2.00s`：吊灯稳定常亮，其它画面静止。
- `2.00–2.06s`：灯芯、近场光晕和左侧环境暖光迅速切断。
- `2.06–2.34s`：保持灯灭；左侧仍由冷蓝雨夜环境光提供轮廓，不形成整屏黑帧。
- `2.34–2.40s`：灯芯先恢复；近场光晕约 `0.14s` 回来，墙面与湿地环境反射约 `0.30s` 完成回暖。
- 恢复后：回到稳定常亮，不追加抖闪。

检查：

- `PASS`：灯灭状态真实移除了左上灯芯、灯罩光晕、左墙暖受光和左侧湿地暖反射，断电因果可识别。
- `PASS`：复亮顺序为灯芯—近场光晕—环境反射，不是整块画面的简单明暗插值。
- `PASS`：正式合成只采用灯灭源图的左侧受光区域；中央中文菜单、焦点、`DICE KING` 顶箱、右侧雨窗和机位均保持原始关键帧，不随事件变化。
- `PASS`：视频可完整解码，画幅、帧率、时长和编码满足评审要求。
- `WARN`：灯灭源状态与软遮罩只用于验证视觉因果，不是可直接加载的 Unity 运行时资源；通过后仍需重绘灯灭底图、灯芯、近场光晕和环境反射四层候选资源。

门禁结果：吊灯断电识别、`0.28s` 停黑时长和三层复亮节奏通过，可以进入开始界面 Unity 接入；雨水与 CRT 动效仍不在本轮范围。

### 2026-07-14 F021-S1 Unity 开始界面接入

- 角色：`$wabish-dev-implementation`、`$wabish-art-production`
- 当前状态：代码与运行时资源已接入、Unity 编译通过，待 Play Mode 视觉与交互验收。
- 代码：`Assets/Scripts/DiceKingDemo.cs`
- 参数：`Assets/Resources/Data/main_menu_visual_config.csv`
- 运行时资源：`Assets/Resources/Art/MainMenu/arcade_main_menu_lamp_on.png`、`arcade_main_menu_lamp_off.png`、`arcade_main_menu_lamp_glow.png`、`arcade_main_menu_lamp_core.png`

已实现：

- `MainMenu` 优先加载新午夜旧机厅全屏画面；常亮底图缺失时回退旧王室账本菜单，灯灭层缺失时保留新画面并禁用吊灯事件。
- 鼠标热区、方向键和 `Space / Enter` 共用菜单焦点；无存档默认开始新游戏，有存档默认继续游戏，继续卡动态显示章节 / 关卡摘要，开始新游戏保留二次覆盖确认。
- 吊灯使用常亮、灯灭、近场光晕、灯芯四层重建专用样片节奏；事件只在 `MainMenu` 空闲时运行，覆盖确认、设置页、离开主菜单和输入保护期间保持常亮。
- `main_menu_visual_config.csv` 提供 12 个可调项：总开关、循环开关、首次延迟、随机间隔上下限、熄灭、停黑、灯芯、近场、环境恢复、断电强度和输入保护；`F7` 支持运行中重新加载。
- 视觉事件使用独立 `System.Random`，不消耗玩法投骰随机。设置页新增“开始界面吊灯屏闪”开关，并保存到 `DiceKingDemo.MenuLampFlickerEnabled`；不提高运行存档版本。

静态 / 编辑器检查：

- `PASS`：Unity 2019.4.33f1 已导入四张 PNG、CSV 与对应 `.meta`，四个 `Resources.Load` key 和实际路径一致。
- `PASS`：`Assembly-CSharp.dll` 与 `Assembly-CSharp-Editor.dll` 在当前 Unity Editor 中完成编译，无新增编译错误；现有两个不可达代码警告仍保留。
- `PASS`：CSV 共 12 行，所有代码要求的 key 均存在；参数范围在加载时钳制，间隔与三段恢复顺序会自动归一化。
- `PASS`：四张运行时图为 `1920x1080`，灯芯与近场层保留透明通道，主界面常亮 / 灯灭状态路径完整。
- `WARN`：自动化窗口捕获接口未能读取当前 Unity 窗口，因此本轮没有完成 Play Mode 截图、真实鼠标热区或 `1920x1080 / 1280x720` 视觉验收；不能据此宣称 UI 最终通过。
- `WARN`：当前完整基准图包含首次启动文案；代码已动态覆盖有存档卡片、焦点和覆盖确认，但全面本地化或大幅改文案前仍需制作无文字 clean plate。

2026-07-14 快速循环修订：用户要求吊灯可循环且闪烁更快。运行时新增 `loop_enabled` 显式开关；默认首次等待改为 `1.00s`，单次事件约 `0.275s`，复亮后随机等待 `0.70–1.30s`，完整起始到起始周期约 `0.98–1.58s`。菜单输入保护改为 `0.50s`；关闭循环时每次进入主菜单只演示一次，关闭总开关或玩家设置开关时始终常亮。

- `PASS`：CSV 共 12 行且无缺失 key；按配置复算得到单次事件 `0.275s`、完整周期 `0.975–1.575s`。
- `PASS`：修改后的 `DiceKingDemo.cs` 使用 Unity 2019.4.33f1 随附 Roslyn 与对应 UnityEngine 引用完成独立编译，0 个错误；只保留当前代码中已有的不可达代码警告。
- `WARN`：当前 Unity Editor 未自动刷新到本次文件时间戳，因此仍不能宣称最新节奏已完成 Play Mode 视觉验收；聚焦 Editor 后会按正常资源刷新流程导入。

下一门禁：在 Unity Play Mode 检查新首屏、1 秒首次断电演示、约 1.0–1.6 秒快速随机循环、两个开关、鼠标 / 键盘操作、覆盖确认，以及进入设置 / 游戏后吊灯保持常亮；通过后再决定是否继续雨水或 CRT 单效果。

### 2026-07-14 窗外雨幕与玻璃汇流单效果方案门禁

- 角色：`$wabish-design-dialogue`、`$wabish-art-production`
- 当前状态：方案已执行，专用样片待验收；尚未生成运行时资源。
- 本轮范围：只讨论右侧窗框内的窗外持续降雨和玻璃汇流水痕；吊灯在预览中固定常亮，CRT、菜单、焦点、机台和其它环境保持静止。
- 现状风险：当前全屏运行时底图已烘入密集静态雨线和固定水痕，不能直接追加动态雨层；首轮必须先制作只替换右窗可视区的局部干净窗景，并保留窗框、街灯和湿地反射。
- 推荐层级：局部干净窗景、窗外远雨循环、窗外近雨循环、少量静态微水珠、单条主汇流 / 可选细支流、窗框遮罩与玻璃反光。
- 深度规则：窗外雨快速、低对比、略虚；玻璃水痕慢、清晰、带停滞与折射高光。雨线和水痕不得越过右侧窗框，也不得覆盖机台或菜单。
- 专用预览建议为 `1920x1080`、30 fps、8 秒、无声：前 2 秒仅连续雨幕，随后 0.5 秒聚滴，约 4 秒向下汇流，最后 1.5 秒水尾淡出；窗外雨全程不断。
- 首轮只交付局部干净窗景、8 秒专用预览与六帧评审图，全部进入 `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/`；不进入 `Assets/Resources/Art/`，不修改 Unity 代码或配置。
- 预览候选验收：远 / 近雨循环无明显接缝；一眼可区分窗外雨和贴玻璃水痕；汇流最多一主一支且不形成白色面条；`1280x720` 下仍有水感但不变成噪点；菜单与 `DICE KING` 顶箱始终稳定。

下一门禁：右窗单效果预览已生成，进入下一节样片验收；样片通过后才生产正式循环纹理、汇流序列、配置项和 Unity 接入方案。

### 2026-07-14 窗外雨幕与玻璃汇流专用预览

- 角色：`$wabish-art-production`、`$wabish-art-assets`
- 当前状态：用户要求修订，V1 只保留对照；未接入 Unity。
- 主预览：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_rain_window_preview_v1_20260714.mp4`，`1920x1080`、30 fps、8 秒、H.264 High、无声、240 帧。
- 评审图：`arcade_dice_king_main_menu_c_rain_window_review_sheet_v1_20260714.png` 为六帧全景，`arcade_dice_king_main_menu_c_rain_window_detail_review_sheet_v1_20260714.png` 为六帧右窗局部放大；二者与视频位于同一目录。
- 样片源：`arcade_dice_king_main_menu_c_window_clean_base_v1_20260714.png`、`arcade_dice_king_main_menu_c_window_clean_patch_v1_20260714.png`、`window_clean_patch_filtergraph_v1.ffgraph` 和 `rain_window_preview_filtergraph_v1.ffgraph`。
- 制作边界：采用局部遮罩滤波清理右窗内已烘焙雨线，再叠加远雨、近雨和一主一支玻璃汇流；未生成式重绘整屏，灯光、CRT、菜单、焦点和机台结构保持母版状态。
- 时序：`0–2.0s` 连续双层雨；`2.0–2.5s` 聚滴；`2.5–3.3s` 慢滑；`3.3–4.1s` 加速；`4.1–4.7s` 停滞；`4.7–6.5s` 再滑；`6.5–8.0s` 水尾淡出。
- `PASS`：远 / 近雨在运动中可区分，最终合成未见明显横向循环接缝。
- `PASS`：水痕采用暗部折射主体与断续高光，最多一主一支，没有形成均匀白色面条。
- `PASS`：所有雨水均限制在右窗内部，菜单、顶箱、吊灯与 CRT 全程稳定。
- `PASS`：已完成全片 240 帧解码检查，并在 `1280x720` 缩放下确认雨水可辨、中文和焦点仍清楚。
- `WARN`：局部干净窗景为导演样片所用的受控滤波派生图，不是最终运行时 clean plate；本轮没有向 `Assets/Resources/Art/` 添加雨水资源，也没有修改 Unity 代码或配置。
- `FAIL`：用户复评发现窗框上下边沿保留了一圈不明白色点；它们来自母版烘焙高亮水珠，V1 没有清理窗框内沿。
- `FAIL`：用户需要“窗外持续降雨”和“雨滴撞击玻璃后附着汇流”两个独立效果，V1 在同一条样片中合成后主要只读出玻璃流痕，不能分别验收。

下一门禁：V1 停止；按用户反馈生成 V2 边沿去白点基底、窗外降雨独立样片和玻璃撞击汇流独立样片。

### 2026-07-15 右窗双效果 V2 修订预览

- 角色：`$wabish-art-production`、`$wabish-art-assets`
- 当前状态：两个 V2 样片分别待用户验收；未接入 Unity。
- 边沿去点源：`arcade_dice_king_main_menu_c_window_clean_base_v2_20260715.png`、`arcade_dice_king_main_menu_c_window_clean_patch_v2_20260715.png` 和 `window_clean_patch_filtergraph_v2.ffgraph`，统一位于 `Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/`。
- 效果一视频：`arcade_dice_king_main_menu_c_outside_rain_preview_v2_20260715.mp4`，`1920x1080`、30 fps、6 秒、H.264 High、无声、180 帧；SHA-256 `7D0E2D58C670EB4CC67BC216C4F8F72F77C7D45D88F91694F22DAF69A57B4CCC`。
- 效果二视频：`arcade_dice_king_main_menu_c_glass_flow_preview_v2_20260715.mp4`，`1920x1080`、30 fps、10 秒、H.264 High、无声、300 帧；SHA-256 `42C43A45EA90BCC30CC2E363F5AF2E73732BA4D6D23447FFD72B8096FCE295A6`。
- 评审图：`arcade_dice_king_main_menu_c_outside_rain_review_sheet_v2_20260715.png`、`arcade_dice_king_main_menu_c_glass_flow_review_sheet_v2_20260715.png` 和 `arcade_dice_king_main_menu_c_rain_two_effect_detail_review_sheet_v2_20260715.png`。
- 合成规则：`outside_rain_preview_filtergraph_v2.ffgraph` 与 `glass_flow_preview_filtergraph_v2.ffgraph`；前者只生成窗外远 / 近雨，后者在同一持续雨景上增加撞击、附着、合并和汇流。
- 效果一时序：6 秒内远雨与近雨持续运动；远雨密、细、低对比，近雨疏、长、速度更快；不出现玻璃水珠和汇流。
- 效果二时序：`0–2.0s` 只有窗外降雨；`2.0–3.32s` 四次撞击环与附着水珠；`4.0–5.4s` 慢滑，`5.4–6.4s` 加速，`6.4–7.0s` 停滞，`7.0–9.0s` 再滑，`9.0–10.0s` 水尾淡出。
- `PASS`：V2 已清除顶部、底部和左侧窗框内沿的连续白点；窗框结构仍可读，没有改动机台、菜单或标题。
- `PASS`：所有动态遮罩向玻璃内部收缩，窗外雨线、撞击环、附着水珠和汇流均不接触窗框。
- `PASS`：两个效果分为两段独立视频；窗外雨依靠速度与虚实表现深度，玻璃层依靠圆形撞击、附着水珠、暗部折射和慢速汇流表现材质。
- `PASS`：两段视频均完成全片解码；`1280x720` 抽查下主汇流仍可辨认，中文、焦点、吊灯和 `DICE KING` 顶箱保持稳定。
- `WARN`：V2 清理基底仍是导演样片使用的局部派生图，不是最终运行时 clean plate；本轮没有向 `Assets/Resources/Art/` 添加资源，也没有修改 Unity 代码或配置。

下一门禁：用户分别接受效果一和效果二后，才生产正式循环纹理、撞击 / 汇流序列、参数表和 Unity 接入；任一项需修订时只修改对应样片。

### 2026-07-15 窗外持续降雨 V3 向下修订

- 角色：`$wabish-art-production`、`$wabish-art-assets`
- 当前状态：用户已接受效果一 V3，冻结为效果二背景基准；未接入 Unity。
- 用户决策：雨效参数后续必须在 Unity Inspector 中直接查看和修改，不放入 CSV；只有效果一与效果二分别验收通过后才进入 Unity 接入和参数资源制作。
- 视频：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_outside_rain_preview_v3_20260715.mp4`，`1920x1080`、30 fps、6 秒、H.264 High、无声、180 帧、2,167,582 字节；SHA-256 `70DD8B9ED814F7577D2E39FA09451443B3A2FAF03DC7AD98FC5E1EF2979484A7`。
- 评审图：`arcade_dice_king_main_menu_c_outside_rain_review_sheet_v3_20260715.png` 为 `1472x564` 六帧全景，`arcade_dice_king_main_menu_c_outside_rain_detail_review_sheet_v3_20260715.png` 为 `1247x1544` 六帧右窗局部。
- 合成规则：`outside_rain_preview_filtergraph_v3.ffgraph`；继续使用 V2 去白点窗景和内缩遮罩，只把远雨、近雨的纵向滚动从 `+0.0038 / +0.0105` 改为 `-0.0038 / -0.0105`，没有改变密度、长度、透明度或两层相对速度。
- `PASS`：单标记方向复核确认当前合成器正值滚动向屏幕上方、负值滚动向屏幕下方；V3 两层雨幕的屏幕速度约为向下 `205px/s` 与 `567px/s`，雨丝倾斜与运动方向一致。
- `PASS`：视频完成全片 180 帧解码；菜单、标题、吊灯和机台没有随雨幕变化，动态仍限制在右窗内缩区域，窗框内沿固定白点没有复发。
- `PASS`：`1280x720` 抽查下远 / 近雨仍可辨认，中文与焦点保持清楚。
- `WARN`：V3 仍是导演样片，不是运行时纹理或参数实现；本轮没有修改 `Assets/Scripts/DiceKingDemo.cs`、`main_menu_visual_config.csv` 或 `Assets/Resources/Art/`。

用户验收：窗外没有问题。V3 的向下方向、当前密度、速度与远近层次通过，效果一门禁关闭；下一步只验收玻璃水滴和汇流。

### 2026-07-15 玻璃撞击与汇流 V3 独立预览

- 角色：`$wabish-art-production`、`$wabish-art-assets`
- 当前状态：效果二 V3 样片待用户单独验收；未接入 Unity。
- 视频：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_glass_flow_preview_v3_20260715.mp4`，`1920x1080`、30 fps、10 秒、H.264 High、无声、300 帧、3,945,728 字节；SHA-256 `272DDC2FC948306B1523C86405D6A48D93B2FDFF93A2DD68E7A87FA7343BC807`。
- 评审图：`arcade_dice_king_main_menu_c_glass_flow_review_sheet_v3_20260715.png` 为 `2880x1080` 六节点全景，`arcade_dice_king_main_menu_c_glass_flow_detail_review_sheet_v3_20260715.png` 为 `1215x1520` 六节点右窗局部。
- 合成规则：`glass_flow_preview_filtergraph_v3.ffgraph`；相对 V2 只把窗外远 / 近雨改成已通过的向下方向，四次撞击、四颗附着水珠、主 / 支流形状、聚合速度和最大长度均保持一致。
- 时序：`0–2.0s` 只有已通过窗外雨；`2.0–3.32s` 四次撞击和附着；`4.0–5.4s` 慢滑，`5.4–6.4s` 加速，`6.4–7.0s` 停滞，`7.0–9.0s` 再滑，`9.0–10.0s` 淡出。
- `PASS`：视频完成全片 300 帧解码；窗外雨方向正确，撞击、附着、初流、停滞和长流节点均在评审图中可辨。
- `PASS`：水层继续限制在右窗内缩区域，未触碰窗框；菜单、标题、吊灯和机台全程稳定。
- `PASS`：全景评审单帧缩放到 `960x540` 时主水流仍可辨认，满足低于 `1280x720` 的静态可读检查。
- `WARN`：水滴数量、汇流速度和水流长度仍是导演样片固定值；只有用户通过后才拆成 Unity Inspector 参数。本轮没有修改 Unity 代码、CSV 或 `Assets/Resources/Art/`。

下一门禁：用户单独判断效果二的撞击辨识、水滴数量、聚合 / 下流速度、停滞节奏和最大水流长度。通过后，效果一与效果二门禁才全部关闭，并可进入 Unity Inspector 参数化实现。

### 2026-07-14 主游戏画面细化母体选择

- 角色：`$wabish-art-production`
- 当前状态：方向已选择，V1 细化方案待评审。
- 用户选择：使用早期 B「午夜旧机厅」接触图中的“全机位开始界面 + 推近后的实体六槽机台”作为主游戏画面细化母体。
- 本轮范围：只形成细化方案，不生成新图、不切运行时资源、不修改 Unity 代码或玩法数据。

确认保留：

- 同一机柜轮廓、同一正视轴线和从全机位到近机位的镜头关系。
- 深海军蓝磨损金属、暖琥珀灯、雨夜冷蓝反射和实体六槽控制台。
- 上部计数器、中部六槽、底部单一实体确认键的机台层级。

明确不继承：

- `骰子 ×3`，它会错误暗示三次投掷或三颗骰子。
- 皇冠与多行等级表，它们会强化王室和老虎机赔付表联想。
- 原图中的具体数字、结果点数、灯位和按钮语义，它们不是当前真实数据或运行状态。

V1 方案门禁：

- 开始界面黑色显示区解释为带显示能力的烟熏玻璃盖板；菜单熄灭后沿同轴推近并开启盖板，显露后方六槽竞技台。
- 目标计数器在上、当前计数器在下；金币和单次投掷状态替代 `骰子 ×3`。
- 右栏改为单张规则插件，不再使用五行等级 / 赔付表结构。
- 六槽上方横条改为六节点左到右结算轨道；`Ready` 安静、`Scoring` 逐槽点亮。
- 下一张样张先交付一张完整 `Ready` 精修稿，并附开始、开启中段、主游戏点亮三格转场说明；通过后才补 `Scoring / StageClear`。

### 2026-07-14 主游戏实体面板功能映射

- 角色：`$wabish-art-production`
- 当前状态：主游戏实体面板结构已确认，V1 功能细化方案待评审。
- 用户澄清：附件近景机台就是后续主游戏界面本体，不是 CRT 内的平面程序页。
- 本轮范围：按当前真实游戏内容定义功能区域；不生成新图、不切运行时资源、不修改代码或玩法数据。

常驻功能映射：

- 左上双计数器：目标分在上、当前分在下，结果锁定阶段不提前增长。
- 中上资源模块：金币数字与单次投掷 / 当前阶段状态，替换 `骰子 ×3` 和三灯误导。
- 右侧插件位：一张本关规则插件，承载章节 / 小关信息、规则图标和一条短规则，不使用多行奖励 / 赔付表。
- 中央横轨：六节点左到右结算轨道，每个节点与一颗实体骰对齐；当前槽局部连锁清空后才推进下一节点。
- 中央六槽：只承载六颗实体骰，`Ready` 支持拖拽插入排序；结果锁定后不重排。
- 底部实体键：全流程唯一主操作 `Space`，根据阶段显示起转、确认结算、进入市场或完成本轮。

按状态出现的附属层：

- 市场反馈在进入小关的 `Ready` 阶段使用短点阵条临时显示，随后收起。
- 饲料、吞吃、吸附和成长只显示必要短标签；详细信息继续由悬浮窗承接。
- 临时小骰不占六个实体槽；结算中从来源槽下方展开余波托盘，完整参与计分，超过展示上限时折叠为汇总。
- `Shaking / Stopping / Scoring` 抑制长悬浮窗，保持当前主焦点唯一。

下一门禁：用户接受 V1 功能分区后，生成一张完整 `Ready` 精修稿；通过后再生成 `Scoring` 图验证局部连锁和临时小骰余波层。

### 2026-07-14 主游戏实体面板 V1 三版接触图

- 角色：`$wabish-art-production`、`$wabish-art-assets`、`$imagegen`
- 当前状态：三版 `Ready` 接触图已生成，待用户选型。
- 本轮范围：只生成主游戏实体面板评审源图；不生成 `Scoring`、`StageClear`、透明切片或运行时资源，不修改 Unity 代码和玩法数据。

生成结果：

- A「信息清晰仪表盘」：`Assets/ArtSource/Production/ArcadeDiceKing/MainGamePanelV1/arcade_dice_king_main_game_panel_v1_a_clear_instrument_20260714.png`
- B「机械结算机」：`Assets/ArtSource/Production/ArcadeDiceKing/MainGamePanelV1/arcade_dice_king_main_game_panel_v1_b_mechanical_settlement_20260714.png`
- C「雨夜信号台」：`Assets/ArtSource/Production/ArcadeDiceKing/MainGamePanelV1/arcade_dice_king_main_game_panel_v1_c_rain_signal_20260714.png`

共同生产约束：

- 三版保持同一台 `DICE KING` 深海军蓝旧机柜、同一正视推近机位、目标 / 当前双计数器、金币、单次投掷、单张规则插件、六节点轨道、六颗实体骰和唯一底部主键。
- 三版只比较信息清晰度、机械反馈强度与雨夜氛围强度，不重新探索主题。
- 不继承皇冠、`骰子 ×3`、三次投掷灯、五行赔付表、赌场滚轴、拉杆或额外操作键。
- 下一门禁：用户选择 A / B / C 或提出混合修订；选定后先生成一张完整 `Ready` 精修稿，再用 `Scoring` 图验证逐槽结算、局部连锁与临时小骰余波层。

## 验证备注

F021-S1 已从样片门禁进入正式实现验收；开始界面吊灯和右窗雨水均以 Unity 运行时实现为准。右窗雨效 R1 已获用户人工回归通过；其它主流程、结算、胜场和市场切片仍处于样片 / 方向阶段，不因该雨效切片通过而自动通过。

## 已知缺口

- 右窗雨效 R1 的人工回归已经完成；自动化截图 / 录屏缺失保留为制作过程限制，不再阻塞该切片。
- `DICE KING` 顶部招牌闪光尚未选定表现版本，当前不生成素材、不接入运行时。
- 其它主流程锚点尚未冻结为最终运行时资源，仍按各自样片门禁推进。

## 最终结论

F021-S1 开始界面吊灯与右窗随机雨水已完成代码、Inspector 参数资产和运行时资源接入；右窗雨线越界与远距离磁吸完成 R1 修复并通过用户人工回归。F021 整体生产包仍在进行中；`DICE KING` 招牌闪光及其它流程切片不因雨效通过而自动验收。

### 2026-07-15 右窗随机多点雨水 Unity 接入验收记录

- 角色：`$wabish-dev-implementation`、`$wabish-art-production`
- 用户决策：保留玻璃 V3 的撞击、附着、停滞和汇流语言，否定固定四滴 / 固定路径；直接实现 Unity 随机多点方案，不再制作固定位置 V4 离线视频。
- 参数脚本：`Assets/Scripts/MainMenuRainProfile.cs`。
- Inspector 参数资产：`Assets/Resources/Config/main_menu_rain_profile.asset`，加载 key `Config/main_menu_rain_profile`，SHA-256 `04A218D3ABA359D9316A410E8580F55A7A2EEF97E6A9A018C42E7A11ABE9838F`。默认远层速度 `170–240px/s`、近层速度 `480–650px/s`，贴近用户已接受 V3 的约 `205px/s` 与 `567px/s` 基准。
- 运行时窗体补片：`Assets/Resources/Art/MainMenu/arcade_main_menu_window_clean_patch.png`，加载 key `Art/MainMenu/arcade_main_menu_window_clean_patch`，`405x900`、不透明 PNG、365,294 字节，SHA-256 `F3366F2EF0337C55FD3FC319794327E9474CBFEF68DD215896DF91111EB8CCD9`。
- `PASS`：右窗补片源自用户已接受的 V2 去白点版本；运行时绘制顺序会在四层吊灯画面后覆盖旧静态雨痕和窗框白点，再绘制程序雨水。补片缺失时程序雨水关闭，避免叠在旧烘焙雨痕上。
- `PASS`：窗外雨使用远 / 近两个固定容量池；更新代码只执行 `Y += movement`，明确沿屏幕向下，越过玻璃下沿后从对应斜顶边重生。密度、总速度、横风和两层数量 / 速度 / 长度 / 宽度 / 透明度均在 Inspector 可调。
- `PASS`：玻璃层进入主菜单时预热 `8–14` 颗水珠，随机分布在 `2–3` 个松散区域；后续按撞击频率补充。只有邻近水珠聚合质量达到 `FlowThreshold` 才触流，同屏主流受 `MaxConcurrentFlows` 限制，默认最多两条；支持支流、停滞、吸收水珠、增粗、延长和淡出循环。
- `PASS`：窗外雨与玻璃水分别由 `OutsideRainEnabled`、`GlassWaterEnabled` 控制；所有状态只在 `MainMenu` 以 `Time.unscaledDeltaTime` 更新。雨水使用独立 `System.Random`，`LockPreviewSeed` 默认开启，`F8` 可按 Inspector 当前值重启同一序列，不消耗玩法随机。
- `PASS`：Unity 2019.4.33f1 随附 Roslyn 独立编译 `DiceKingDemo.cs` 与 `MainMenuRainProfile.cs`，0 个错误；仅保留当前工程既有的 7 个不可达代码警告。
- `PASS`：已打开 Unity Editor 导入 `MainMenuRainProfile.cs`、`main_menu_rain_profile.asset` 和右窗补片，并于 2026-07-15 12:49:55 重编译 `Library/ScriptAssemblies/Assembly-CSharp.dll`；本轮导入日志没有新增 `error CS`。
- `PASS`：`git diff --check` 通过；雨水配置没有写入 `main_menu_visual_config.csv`，吊灯 CSV 边界保持不变；不改变运行存档版本、`PlayerPrefs`、玩法状态或骰子数据。
- `WARN`：12:49:55 编辑器编译后又把默认远 / 近雨速校准到 V3 基准；校准后的源码已再次通过独立 Roslyn 编译，但当前 Editor 没有自动刷新到新的文件时间戳。正式录屏前需先聚焦 Unity，等待资源刷新完成。
- `WARN`：未完成 Game View 动态截图、30 秒随机分布观察、实际聚合频率和水流长度视觉判断；也未验证 `1920x1080` / `1280x720` 的低分辨率线宽和水滴可读性。

下一门禁：在 Play Mode 打开 `Assets/Resources/Config/main_menu_rain_profile.asset`，录制至少 30 秒；确认雨只向下、远近层不粘连、可见水珠约 `8–14`、区域位置持续变化、不是每滴都触流、主流不超过两条、窗框没有白点或越界。通过后再冻结默认参数。

### 2026-07-15 人工验收反馈与 R1 修复

- 用户截图确认两项 S2：窗外雨丝 / 水流线段越出右窗并覆盖中央游玩机器；玻璃小水滴会跨距离追踪大水滴，包含横向或向上的吸附运动，不符合垂直玻璃上的重力规律。
- 根因一：雨丝和水流共用 `DrawMainMenuLine`，旧实现调用 `GUIUtility.RotateAroundPivot(angle, start)`；当前 OnGUI 已存在 `fitMatrix * prototypeMatrix` 外层缩放，该 API 的枢轴空间使旋转线段产生数百像素位移。圆形水珠无需旋转，因此截图中仍位于右窗。
- 修复一：线段改为 `oldMatrix * Matrix4x4.TRS(start, rotation, 1)` 的局部矩阵，再从局部原点绘制；现有窗面多边形端点裁切保持不变。雨丝和玻璃流线不再借用全局绕点旋转。
- 根因二：旧聚合在 `MergeRadius` 内让小滴用 `Vector2.MoveTowards` 追踪大滴，移动向量没有重力方向约束。
- 修复二：删除远距离追踪；普通水滴仅在附着期结束后沿屏幕正 Y 向下，质量越大下滑越快；只有两滴可见椭圆轮廓真实接触时才合并，保留下方水滴；主流只捕获自身已流经的下游路径上、与流线轮廓接触的水滴。
- Inspector 参数语义已同步：`MergeRadius` 改为额外接触容差，`MergeSpeed` 改为合并重滴向下滑速上限；新增 `ContactImpactChance`、`SurfaceHoldRange`、`SurfaceCreepSpeedRange`。默认接触容差由 `30` 调整为 `2` 设计像素。
- `PASS`：Unity 2019.4.33f1 随附 Roslyn 独立编译两份脚本，0 个错误；保留工程原有 7 个不可达代码警告。
- `PASS`：聚焦当前 Unity Editor 后完成资源刷新，`Library/ScriptAssemblies/Assembly-CSharp.dll` 于 2026-07-15 13:07:24 重编译；Editor 日志无新增 `error CS`。
- `PASS`：`git diff --check` 通过；代码中已不存在开始界面玻璃水珠的 `MoveTowards` 或旧 `attractionRadius` 路径。
- `WARN`：本轮无法自动取得修复后的 Game View 截图；上述静态与编译检查不能替代 30 秒动态人工回归。

回归标准以 `PLAYTEST_FEEDBACK.md` 的 `F021-RG-001` 至 `F021-RG-006` 为准。

### 2026-07-15 右窗雨效 R1 人工回归结论

- 用户结论：人工验收基本没问题，可以继续优化游戏开始界面。
- `PASS`：`F021-RG-001` 至 `F021-RG-006` 均关闭；两项 S2 反馈状态改为已通过。
- `PASS`：右窗雨效切片不再阻塞后续开始界面工作。
- 范围边界：新提出的 `DICE KING` 电玩 / 霓虹招牌闪光属于独立美术表现切片，当前只进行版本方案评审，不回开雨效缺陷，也不代表 F021 全包通过。

### 2026-07-15 DICE KING A 点阵补亮动态预览 V1

- 角色：`$wabish-art-production`、`$wabish-art-assets`。
- 用户选择：A「点阵灯泡补亮」进入动态样片门禁；确认后才允许制作运行时分层和接入 Unity。
- 静态样张反馈：红框内暗点是上一版为表达补亮峰值刻意设置的半亮灯珠，不是文件损坏；但单帧被合理地读成永久坏点，因此该静态图不再作为 A 方案通过依据。
- 修订结果：V1 稳定态保持全部灯珠完整常亮；四颗非 `I` 字灯珠仅在约 `0.15s` 的事件中短暂压暗，随后按 `DICE → KING` 出现受控暖琥珀补亮并完整恢复。
- 动态样片：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_preview_v1_20260715.mp4`。
- 局部循环：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_preview_v1_20260715.gif`。
- 评审图：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_review_sheet_v1_20260715.png`、`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_detail_review_sheet_v1_20260715.png`。
- `PASS`：样片为 `1920x1080`、30 fps、5 秒、150 帧、H.264 High、无声；标题完整可读，事件前后均回到同一常亮状态。
- `PASS`：合成直接使用现有运行首屏母版，画面变化限制在顶箱文字区域；没有生成新中文、没有叠加吊灯断电、CRT 故障、菜单动画或转场。
- `PASS`：本轮只新增 `Assets/ArtSource/` 评审文件和合成规则，未替换运行资源、未改 Unity 代码、CSV、Inspector 参数或玩法数据。
- `WARN`：V1 视觉节奏仍待用户人工验收；在用户明确接受前，A 不得标记为 `已批准入库`，也不得进入运行时拆分。

### 2026-07-15 DICE KING A 点阵补亮动态预览 V2

- 用户反馈：V1 总体过快，只像一次亮度闪动，未形成明确的停灭和电流不稳定感；同时要求未来 Unity 接入可参数化调节。V1 状态改为 `需修订、保留对照`。
- V2 修订：供电事件由约 `0.62s` 拉长到约 `1.63s`；最低亮度约 `20%`，近灭停留约 `0.20–0.26s`，包含两次失败回亮、`DICE → KING` 分区补亮和约 `0.30s` 缓慢收束。
- V2 取消单颗灯珠随机坏点，改为同一电路分区统一降亮；所有灯珠在事件结束后完整常亮。
- 动态样片：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_preview_v2_20260715.mp4`。
- 局部循环：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_preview_v2_20260715.gif`。
- 评审图：`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_review_sheet_v2_20260715.png`、`Assets/ArtSource/Production/ArcadeDiceKing/MainMenuCAtmosphere/arcade_dice_king_main_menu_c_marquee_a_dot_relight_detail_review_sheet_v2_20260715.png`。
- `PASS`：样片为 `1920x1080`、30 fps、6 秒、180 帧、H.264 High、无声；稳定态、压暗、近灭、失败回亮、分区补亮和恢复节点均可独立检查。
- `PASS`：画面变化仍限制在顶箱文字区域，不影响中文菜单、焦点、吊灯、CRT、右窗、机柜或玩法状态。
- `PASS`：本轮没有创建 Unity 脚本、`ScriptableObject`、运行时贴图、CSV 字段或 Inspector 参数；候选参数映射只记录在 `ART_BRIEF.md`。
- `WARN`：V2 仍待用户人工验收；只有节奏通过后，才允许把候选参数转为 `MainMenuMarqueeProfile` 并交由 `wabish-dev-implementation` 接入。

### 2026-07-16 F021-S2 主游戏共用运行时包 V1

- 角色：`$wabish-art-production`、`$imagegen`、`$wabish-dev-implementation`。
- 范围：只覆盖 `Ready / Shaking / Stopping / ResultDecision / Scoring` 共用竞技台与模块化骰壳；不覆盖生命规则、通关 / 失败专属动态、市场换肤或新玩法。
- `PASS`：运行资源包含一张 `1920x1080` 无文字底板和五张 `512x512` RGBA 家族壳，全部登记在 `ART_ASSETS.md`，源图与 `build_runtime_assets.ps1` 可复现。
- `PASS`：`Resources.Load` key 已接入；底板缺失回退旧 Run UI，单家族壳缺失回退中立壳。
- `PASS`：中立、猪、恶魔、龟、海盗单家族，以及恶魔 / 海盗双家族左右拼壳均在 Play Mode 正确显示。
- `PASS`：待机中心显示具体类型图标或短类型名；结果中心只显示当前有效数字；`123 / 1234` 在 `1920x1080` 与 `1280x720` 均完整可读。
- `PASS`：Ready、ResultDecision、Scoring 两档截图均无关键区裁切；结算强调已改为暗色 CRT 条、琥珀描边与少量青色，不再使用旧浅金实心条。
- `PASS`：Unity 2019.4.33f1 最新编译为 0 error；保留工程已有 7 条 `CS0162` 警告；`git diff --check` 通过。
- `PASS`：自动验收器同时读取八张 PNG 的真实像素尺寸，避免分辨率切换异步导致的假通过。
- `WARN`：生命池尚未实现，顶部按设计显示 `生命 --`；评审图中的 `3` 不作为规则值。
- `WARN`：通关暖光、失败锈红、重试扫描和生命归零断电继续后置，不因共用包通过而自动批准。
- 证据：`Docs/QA/20260716_main_game_runtime_validation.md` 与同目录八张 `20260716_main_game_runtime_*.png`。

### 2026-07-16 F021-S2-R1 六槽二维对齐修复

- 角色：`$wabish-playtest-feedback`、`$wabish-dev-implementation`。
- 用户反馈：`Ready` 截图中六枚骰子与底图凹槽未重合；纵向偏差明显，复核还发现轻微左偏和节距差。
- 分类：`F021-20260716-FB-001`，程序缺陷，`S2 高`，当前状态为“已修复待回归”。
- 根因：共用底板已经换成实体六槽构图，但 `DrawArcadeRun` 仍沿用旧区域 `(188,278)` 和街机单行间距 `18`；V1 中心为 `X=278+130n / Y=346`，底图中心为 `X=299+128n / Y=369`。
- 修复：共用骰台区域改为 `new Rect(204f, 301f, 830f, 220f)`，街机单行间距改为 `16f`；骰子尺寸、家族壳、类型芯、数字、拖拽与结算规则均不改变。
- `PASS`：`git diff --check -- Assets/Scripts/DiceKingDemo.cs` 通过。
- `PARTIAL`：用户提供的 R1 局部截图确认六骰中心与节距已经统一，但暴露骰壳有效像素相对空槽偏小；尺寸问题转入 R2，双分辨率和拖拽检查继续保留。
- 回归标准：`PLAYTEST_FEEDBACK.md` 的 `F021-RG-007` 至 `F021-RG-009`。

### 2026-07-16 F021-S2-R2 槽位尺寸契约与骰壳适配

- 角色：`$wabish-playtest-feedback`、`$wabish-dev-implementation`。
- 用户反馈：底图 UI 空槽需要规定正式尺寸并与骰子尺寸适配；R1 截图中中心虽已靠拢，骰壳仍明显没有装满有效槽。
- 分类：`F021-20260716-FB-002`，界面体验，`S2 高`，当前状态“待回归”；自动验收脚本编译阻断另记为 `F021-20260716-FB-003`，程序缺陷，`S1 阻断`，当前状态“已通过”。
- 尺寸冻结：内部设计坐标下，有效槽 `112×112`、中心距 `128`、净距 `16`、六槽中心 `X=299+128n / Y=369`；最大高亮 `128×128`。
- 根因：五张 `512×512` 家族壳有效 Alpha 只有宽 `431–447`、高 `456–458`；旧版把整个透明画布缩放到 `112×112`，可见壳只有约 `94–98 × 100`。
- 修复：单家族与双家族统一裁切公共 UV `(32,27,448,458)`；预计可见壳提升为宽 `107.8–111.8`、高 `111.5–112`。中心内容框同步换算为 `18.6%, 15.3%, 62.9%, 59.2%`。
- `PASS`：不修改底图或五张 PNG，不扩大命中框，不改变家族、数字、拖拽、结算、玩法、数据、存档和随机。
- `PASS`：自动验收器修正为只复用 `FixedResolution` Game View 项，避免把 `1280×720` 比例项误当固定尺寸并得到 `1532×793`。
- `PASS`：修复自动验收器的 `CS0136` 局部变量重名；`Assembly-CSharp-Editor.dll` 于 2026-07-16 15:20 重新生成，Editor 日志无新增 `error CS`。
- `PASS`：自动任务完整生成八张 `1920×1080 / 1280×720` 截图；五类壳、双家族拼缝、类型芯与 `123 / 1234` 均完成自动画面检查，状态文件为 `PASS`。
- `PENDING`：用户确认槽位尺寸观感，并人工检查拖拽幽灵、插入标记和结算高亮边界；自动截图不替代交互验收。
- 回归标准：`PLAYTEST_FEEDBACK.md` 的 `F021-RG-010` 至 `F021-RG-015`。

### 2026-07-16 F021-S2-R3 底图边框贴合与截图器稳定化

- 角色：`$wabish-playtest-feedback`、`$wabish-dev-implementation`。
- 用户反馈：R2 仍未对齐底图边框；随后自动验收切换 `1280×720` 时抛出 `Expected 1280x720, current 875x30`。
- 分类：`F021-20260716-FB-004`，界面体验，`S2 高`；`F021-20260716-FB-005`，程序缺陷，`S2 高`。两项当前均为“修复已实现，待完整回归”。
- R3 候选：六个物理槽 `128×128`、中心 `X=299+128n / Y=368`、净距 `0`；公共 UV 不变，所有交互和结算边框改为槽内内缩。
- `PASS`：`DiceKingDemo.cs` 静态几何为 `(235+128n,304,128,128)`，六槽中心与底图候选锚点一致；`git diff --check` 通过。
- `PASS`：截图器稳定等待修复通过独立 Roslyn 编译；主体修复已触发 Unity 重新生成 `Assembly-CSharp-Editor.dll`，无新增 `error CS`。
- `PENDING`：新一次性请求尚待当前 Unity 编辑器重新获得焦点并消费；八张 R3 图片和最终状态文件未生成前，不把自动验收标记为通过。
- `PENDING`：用户确认边框贴合度，并人工检查 Ready 拖拽幽灵、插入标记与 Scoring 高亮均留在各自物理槽内。

### 2026-07-16 F021 主游戏九状态接触图 V1

- 角色：`$wabish-art-production`、`$wabish-art-assets`、`$imagegen`。
- 交付：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/arcade_dice_king_main_game_nine_state_contact_v1_20260716.png`。
- 覆盖：Ready、投骰峰值、第三槽停转、ResultDecision、第二槽局部结算、结算中越过目标、StageClear、可重试 StageFailed、RunOver。
- `PASS`：源图保留在 `Assets/ArtSource`，未进入 `Assets/Resources/Art`，未修改 Unity 代码、玩法、数据与存档。
- `PASS`：范围不包含关间市场；StageClear 只显示 `SPACE 进入市场` 去向。
- `PENDING`：用户检查九格是否保持同一机位与六槽几何、各状态是否一眼可辨、骰面数字与贡献分是否分离、越过目标后是否仍有继续结算感、三种结果分支是否保持克制局部色彩。
- 下一门禁：用户接受后才制作五条分段动态样片；若修订则继续停留在源图样张阶段。

### 2026-07-16 F021 主游戏九状态接触图 V2

- 用户反馈：V1 主体风格通过；全部文字需换为开始界面同系 LED 点阵灯字，六骰位置与尺寸需符合当前骰子并完全贴槽。
- 交付：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/arcade_dice_king_main_game_nine_state_contact_v2_led_fit_20260716.png`。
- `PASS`：没有重做九状态顺序、机位、老娱乐街机材质或结果分支层级。
- `PASS`：生产约束冻结为全 UI 离散 LED 灯珠点阵；骰面数字继续保留实体压印样式。
- `PASS`：生产几何冻结为六个 `128×128` 连续物理槽、中心 `X=299+128n / Y=368`、净距 `0`；活动骰只允许在本槽内做受限抬升。
- `PASS`：V1 保留为需修订对照，V2 仅进入 `Assets/ArtSource`，未切运行时资源或修改 Unity 代码。
- `PASS`：用户确认接触图 V2 通过；离散 LED 点阵字、六槽贴合、骰子尺寸和九状态可读性进入动态母版。
- 下一门禁：分别验收五条分段动态样片；全部通过前不切结果动态运行资源。

### 2026-07-16 F021 主游戏五条动态样片 V1

- 角色：`$wabish-art-production`。
- 输出目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MotionPreviewsV1/`。
- `PASS`：五条均为 `1920×1080`、30 fps、H.264 High、`yuv420p`、无声，并通过完整解码校验。
- `PASS`：投骰—停转保持六骰固定槽位并从左到右揭晓；没有抛骰、改位或整屏震动。
- `PASS`：逐骰结算—目标越线保留当前槽局部焦点，越线后没有提前停止余下结算。
- `PASS`：通关收入按固定、利息、复利三拍显示；只停在进入市场操作，没有市场画面。
- `PASS`：失败重试保留最终六骰与分差，确认后用单条水平扫描复位；没有倒放逐骰计分。
- `PASS`：生命归零先保留结果等待确认，再按模块断电、CRT 水平线和黑场收束；没有死亡弹窗或整屏红洗。
- `PASS`：五条全部位于 `Assets/ArtSource`，未进入 `Assets/Resources/Art`，未修改 Unity 代码、玩法、数据、存档或关间市场。
- `PASS`：用户于 2026-07-17 确认五条短片均无问题；节奏、局部焦点、收入分拍、扫描速度和断电顺序正式成为 Unity 实现基准。
- 下一门禁：准备动态层拆分与 `wabish-dev-implementation` 交接；接入时以运行时状态与参数驱动复现，不直接播放评审 MP4。关间市场继续排除，生命池数值规则另行确认。

### 2026-07-17 F021 主游戏 LED 字体校准板 V1

- 角色：`$wabish-art-production`、`$wabish-dev-implementation`。
- 用户反馈：当前运行时点阵文字普遍偏细、偏小且不清楚，批准预览与实际实现差异明显；后续验收不希望依赖 Computer Use 进入 Unity 手工截图。
- 根因：正式字库使用 `32×32` 字图块和约 `2 px` 灯珠，但终端正文及辅助文字仍存在 `14 px` 目标字号；在 `1280×720` 下核心灯珠只有 `0.875 px`。此前预览和运行时没有共用一套可测字体合同。
- 生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_led_font_calibration_v1.py`。
- 输出目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV1/`。
- `PASS`：生成器直接读取当前运行字形映射，复用字符步距、虚拟坐标、换行、对齐、辉光比例和 `Point` 采样规则；没有另做一套 AI 预览字体。
- `PASS`：接触板分别使用实际 `1920×1080` 与 `1280×720` 输入，对照批准参考、当前运行、A「10×10 粗灯珠」和 B「11×11 紧凑灯珠」。
- `PASS`：A 的最小核心灯珠为 `2.20 px`，B 为 `2.00 px`；两者的主信息有效字高不低于 `22 px`，正文有效字高不低于 `18 px`，所有代表文字均装入当前区域且缺字数为 `0`。机器报告均为 `PASS`。
- `FAIL`：当前运行方案最小核心灯珠为 `0.875 px`，不再作为可接受视觉基准；不能只增加辉光并继续保留 `14 px` 正文。
- `PASS`：新增产物全部位于 `Assets/ArtSource`；没有替换正式 `Resources` 字库，没有修改 `MainGameLedFont.cs`、`DiceKingDemo.cs`、玩法、数据或存档，也没有启动 Unity 或使用 Computer Use。
- `PENDING`：用户选择 A / B，或只指出需要调整的变量族。确认后才生成正式运行字库、把任意数字字号改为命名样式，并通过编辑器脚本自动执行双分辨率 Unity 冒烟截帧。
- 证据：`Docs/QA/20260717_led_font_calibration_v1.md` 与 `led_font_calibration_metrics_v1_20260717.json`。

### 2026-07-17 F021 LED 字体校准板 V1 退回

- 用户原始反馈：A / B 对比批准预览仍有差距，至少显示很模糊，不能继续推进。
- 分类：`F021-LEDFONT-V1-FB-001` 为美术表现 `S2 高`；`F021-LEDFONT-V1-FB-002` 为验收缺口 `S2 高`。当前均为“待修复”。
- `FAIL`：A / B 的机器 `PASS` 不再有效作为放行依据；人工锐度验收失败时不得进入正式字库替换或 Unity 接入。
- 已确认根因一：发布用校准板把 `3840×2160` 总板通过 `LANCZOS` 再缩成 `1920×1080`，即使局部先用 `Point` 放大，最终展示仍被二次滤波。
- 已确认根因二：A / B 把 `10×10 / 11×11` 低分辨率字图缩放到多组非整数目标尺寸，并额外绘制扩大 `1.35` 设计像素的低透明辉光，造成灯珠复制宽度不均与边界发虚。
- 当前门禁：正式接入暂停；V1 A / B 标记为“需重做”。V2 必须先提供 1:1 未缩放裁切、整数倍最近邻像素检查图、无辉光核心图和独立辉光对照，并补充非整数采样、核心锐度与辉光占比机器检查。

### 2026-07-17 F021 LED 字体像素级校准 V2

- 角色：`$wabish-art-production`、`$wabish-playtest-feedback`。
- 输出目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontCalibrationV2/`。
- `PASS`：完整帧直接输出原生 `1280×720 / 1920×1080`，没有发布用二次缩小。
- `PASS`：十张区域对照均为纵向堆叠的 1:1 原始裁切；三张 720p 放大检查图仅使用整数 `4× NEAREST`。
- `PASS`：灯珠核心逐个绘制，宽高与点距均为整数物理像素；不缩放整张字图，不使用抗锯齿或模糊滤镜。
- `PASS`：无辉光硬核心先独立成立；受限辉光仅为独立 `1px / Alpha 34` 外层，关闭辉光不影响字形完整性。
- `PASS`：顶部、积分塔、终端、按键和辅助字在两档分辨率均装入当前区域，代表文案缺字为 `0`。
- `PASS`：用户确认清晰度没有问题；V1 的发虚与二次缩放问题关闭。
- `FAIL`：用户同时指出字形无法识别内容；`7×7–10×10` 中文逻辑网格不能作为正式字形。
- 当前不生成正式 atlas / map，不修改 Unity 代码；字形识别转入 V3。
- 证据：`Docs/QA/20260717_led_font_calibration_v2.md` 与 `led_font_calibration_metrics_v2_20260717.json`。

### 2026-07-17 F021 LED 字体字形识别压力板 V3

- 角色：`$wabish-art-production`、`$wabish-playtest-feedback`。
- 用户原始反馈：清晰度没问题，但是字形无法识别内容了。
- 输出目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRecognitionV3/`。
- `PASS`：主验收板为原生 `1280×720`，不缩放、不插值、不加辉光。
- `PASS`：中文结构由 `Noto Sans SC` 中等字重的目标尺寸 hinted mask 直接生成，固定保留 `16×16` 逻辑结构；输出阶段完全二值化。
- `PASS`：720p 统一使用 `2×2 px` 核心和 `3 px` 点距，不为装入旧框退回 `1 px` 细灯珠。
- `PASS`：真实文案 T1–T7 与 `章 / 额 / 规 / 检 / 骰 / 袋 / 调 / 整 / 结 / 算 / 顺 / 序 / 掷` 复杂字压力集均已进入样张；复杂字检查只用整数 `2× NEAREST`。
- `WARN`：按该可读字形回算，顶部三窗和主操作键可保留；积分塔目标 / 当前、终端标题 / 正文和排序微提示需要回排。区域装入不再优先于识字。
- `PASS`：用户确认 V3 字形通过；T1–T7 与复杂字压力集的字形身份门关闭。
- `FAIL`：用户同时指出字符不处于同一水平线；V3 的逐字顶对齐绘制不能作为正式排版，旧框回排继续暂停。
- `PASS`：V3 仍只位于 `Assets/ArtSource`；没有运行时 key，没有替换正式字库，没有修改 `MainGameLedFont.cs` 或 `DiceKingDemo.cs`，没有使用 Computer Use。
- 证据：`Docs/QA/20260717_led_font_recognition_v3.md` 与 `led_font_recognition_metrics_v3_20260717.json`。

### 2026-07-17 F021 LED 字体基线校准 V4

- 角色：`$wabish-art-production`、`$wabish-playtest-feedback`。
- 用户原始反馈：字形通过，但是排榜有问题，不处于同一水平线。
- 输出目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontBaselineV4/`。
- `PASS`：V4 直接复用 V3 活动二值字形；测试字符的活动位图哈希和亮灯数全部一致，没有重画、缩放或重采样。
- `PASS`：36 个中文、英文大写和数字样本统一落在逻辑第 `14` 行；最大底线偏差为 `0` 个逻辑行、720p `0 px`。
- `PASS`：短横 / 间隔点使用显式中线锚点；逗号 / 句号使用统一下沿，避免把悬浮标点错误压到底线。
- `PASS`：主板为原生 `1280×720`；V4-only 板为 1:1；放大检查只使用整数 `2× NEAREST`；无辉光、无抗锯齿。
- `PASS`：用户确认混合排版视觉上处于同一水平线，并指定 V4 为没有特别说明时的全局默认文字标准。
- `PASS`：没有修改运行资源、`MainGameLedFont.cs`、`DiceKingDemo.cs`、玩法、数据或存档；没有启动 Unity 或使用 Computer Use。
- 证据：`Docs/QA/20260717_led_font_baseline_v4.md` 与 `led_font_baseline_metrics_v4_20260717.json`。

### 2026-07-17 F021 LED V4 全局运行接入

- 用户原始结论：`没问题，后续这个作为文字标准，没有特别说明，所有出现的文字格式都需要按照这个标准实现，可以检查现在实现的内容中出现的问题并全部按照标准修一边`。
- `PASS`：正式 atlas / map 当前按代码 / 数据字符集合覆盖 `850` 字形；Alpha 仅 `0 / 255`，codepoint 无重复，每字位图固定 `16` 行，字符缺失为 `0`。
- `PASS`：V3 已批准字形活动位图保持不变；V4 已批准样本共享底线偏差为 `0`。
- `PASS`：运行时只有 `Display / Compact / Micro` 三档整数几何，纹理强制 `Point`，无抗锯齿、无运行辉光、无任意比例字图缩放。
- `PASS`：三档几何由 `main_game_led_font_styles.csv` 共享给离线生成器与 C# 运行时；离线上下文图执行与 Unity 相同的文字框物理像素裁切。
- `PASS`：`DiceKingDemo.cs` 的 `87` 个动态标签 / 开关文案入口均经过标准绘制；主菜单、开场、设置、主流程、关间市场、Tooltip、结果页和旧回退 UI 已覆盖。
- `PASS`：原始 `GUI.Label` 仅保留骰面实体数字阴影 / 本体及字库资源加载失败回退三处白名单；唯一原始 `GUI.Toggle` 是无文字控制框，文案仍由标准入口绘制。
- `PASS`：全部原始 `GUI.Button` 均为 `GUIContent.none` 热区；不存在其它 `GUI / GUILayout` 文字控件旁路。
- `PASS`：全部运行脚本及场景 / Resources prefab 均已扫描；不存在绕过标准入口的序列化 `Text / TextMeshPro / TMP_Text` 文本组件。
- `PASS`：主菜单底图的旧平滑菜单字已用不透明动态层覆盖；`DICE KING` 实体招牌与骰面实体数字作为明确例外保留。
- `PASS`：原生 `1280×720 / 1920×1080` 文字真值板以及主菜单 / Ready / 市场上下文对照通过；所有文字来自正式 map 和同一整数几何。
- `PASS`：Unity 2019.4.33f1 随附 Roslyn 独立编译全部 `Assets/Scripts/*.cs`，`0 error`；工程已有 `7` 条 `CS0162` 警告保持不变。
- `PASS`：本轮没有使用 Computer Use；市场只统一文字渲染，没有改变布局、流程、交互或规则。
- `PENDING`：当前已打开的 Unity Editor 尚未完成本轮资源重新导入后的 Play Mode 画面、鼠标热区与 Tooltip 交互回归，不能把离线结果冒充为人工交互通过。
- 证据：`Docs/QA/20260717_led_font_runtime_v4.md` 与 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRuntimeV4/`。

### 2026-07-17 F021 骰子类型芯方向 A 压力图

- 角色：`$wabish-art-production`、`$wabish-art-assets`、`$imagegen`。
- `PASS`：用户确认压力图没有问题并批准继续；家族壳与具体类型芯的两层识别关系通过方向门禁。
- `PASS`：12 枚压力对象覆盖基础、四家族与中立，并同时显示静态 `Ready`、身份动作峰值和结果缩小芯。
- `PASS`：修订版在结果数字下方补入具体类型小芯，不再只剩家族徽章。
- `PASS`：源图只位于 `Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/`，未覆盖现有运行资源、代码、玩法、数据或存档。
- `PASS`：首批生产范围按当前名单冻结为 `37` 枚；双家族 `9` 枚单独后置，避免扩大本轮。
- `PASS`：五张分家族生产板已生成，数量为 `9 / 7 / 6 / 7 / 8`，合计正好覆盖 37 枚；家族内高风险近似类型已建立不同静态剪影。
- `PASS`：猪猪小状态壳与恶魔家族徽章的生成偏差已定向修订；贡品继续使用无家族系统壳。
- `WARN`：生产板中的结果小芯可用于首轮缩小检查，但不替代逐枚资源进入实际 `80×76` 内容框后的最终验收。
- `PENDING`：37 张透明静态图、对应活动遮罩、运行时加载映射和双分辨率 Unity 回归尚未完成。

### 2026-07-17 F021 LED 语义亮度层级 V1 提案与样张

- 角色：`$wabish-playtest-feedback`、`$wabish-art-production`。
- 体验证据：用户提供的当前主菜单与 `1280×720` 关间市场运行截图。
- 用户结论：整体方向可继续，但开始界面主要菜单字过暗；市场骰子名称及部分文字暗到难以看见，需要按信息重要度建立亮度 / 样式分类。
- `PASS`：V4 字形身份、像素清晰度、共享基线和三档整数几何继续冻结，本轮不把亮度问题误判为需要重新造字。
- `FAIL`：当前运行文字只有几何样式，没有全局语义亮度角色；关键标题、商品名、说明和禁用状态仍靠调用点任意颜色 / Alpha 表达。
- `FAIL`：市场商品名使用 `17` 的旧字号提示，当前会优先选择 `Compact`；在 `1280×720` 只有 `1px` 核心，未体现其 `PrimaryInfo` 身份。
- `FAIL`：离线 V4 上下文与 C# 分别维护颜色常量，现有机器报告没有检查核心 / 局部背景对比度、关键角色或禁用态可读下限。
- `PASS`：用户认可 V1 的信息分级与页面映射。
- `FAIL`：V1 没有约束同一分级内不同色相的感知亮度；最低对比度通过不能证明同级等亮。V1 只保留为问题证据，不能接入运行时。

| 验收点 | 通过标准 | 状态 |
|---|---|---|
| 字形守恒 | V3 活动字形、V4 基线、三档整数灯珠全部不变 | 已冻结 |
| 角色单一来源 | Python 与 C# 读取同一角色合同，配置哈希一致 | 离线候选已建立；C# 待样张通过 |
| 主信息可读 | `L3 / L2` 对局部底色至少 `7:1`，Alpha `1.0`，不得使用 `Micro` | 机器通过；人工待验收 |
| 次信息可读 | `L1` 至少 `4.5:1`；规则 / 六面在 720p 可辨 | 机器通过；人工待验收 |
| 禁用仍可识别 | 至少 `3:1`，同字形同尺寸，并有面板 / 状态辅助 | 机器通过；人工待验收 |
| 实体浅色键 | `InkOnLight` 至少 `7:1`；可用 / 禁用不只靠透明度区分 | 浅铭牌带机器通过；人工待验收 |
| 辉光边界 | 硬核心先通过；只有 `L3` 可用独立 `1px`、Alpha `<=0.18` 外层 | 机器通过；人工待验收 |
| 上下文覆盖 | 主菜单、市场常态、市场 Tooltip 同时输出 720p / 1080p 1:1 | 已生成；人工待验收 |
| 缩放压力 | `90%` 显示模拟中主次、商品名和主操作仍可识别；不承担锐度验收 | 已生成；人工待验收 |
| 最终冒烟 | 同源板通过后只做一次自动 Unity 双分辨率与真实交互回归 | 样张通过后执行 |

样张证据：

- 目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV1/`。
- 角色合同：`main_game_led_text_roles_v1.csv`；状态入口共 `10` 项。
- 上下文：主菜单、市场常态、市场 Tooltip 各有 `core / focus` 与 `1280×720 / 1920×1080`，合计 `12` 张；另有双分辨率状态条和三张 `90%` 压力图。
- 报告：`led_text_brightness_metrics_v1_20260717.json`。关键 `L2 / L3` 使用 `Micro = 0`、局部对比度失败 `0`、框外裁切 `0`、正式 V4 glyph 缺失 `0`、辉光 Alpha 上限 `0.18`。
- 当前状态：`分级认可、同级亮度需修订`；未修改 C#、正式 Resources、玩法、经济、数据或存档，也未使用 Computer Use 或图像生成模型。

- 本轮没有修改代码、正式 atlas / map、市场布局、玩法、经济、数据或存档；也没有使用 Computer Use。

### 2026-07-17 F021 LED 语义亮度 V2 同级等亮样张

- 角色：`$wabish-art-production`、`$wabish-art-assets`。
- 用户输入：分级认可；同一分级必须亮度一致。
- 范围：只修正硬核心亮度合同与同源样张；V4 字形、几何、页面分级、市场布局和交互全部不变，不接入 Unity。
- `PASS`：10 个角色全部映射到 `brightness_grade`；`L3` 四种色相和 `L1` 两种色相分别共用同一 `OKLab L` 目标。
- `PASS`：RGB 量化后最大目标差 `0.000045263`、同级最大跨度 `0.000067127`，均低于 `0.0002` 容差。
- `PASS`：共 `320` 次绘制检查；关键角色误用 `Micro = 0`、局部对比度失败 `0`、框外裁切 `0`、正式 glyph 缺失 `0`、核心 Alpha 全部 `1.0`、焦点 / 结果辉光参数组内一致。
- `PENDING`：统一暗底比较条、主菜单、市场与 Tooltip 的人工观感待用户确认；确认前不生成运行时副本。

样张证据：

- 目录：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedTextBrightnessV2/`。
- 合同：`main_game_led_text_roles_v2.csv`、`main_game_led_text_brightness_groups_v2.csv`。
- 同级比较：`led_text_brightness_v2_same_grade_{core,focus}_{1280x360,1920x540}.png`。
- 实景上下文：主菜单、市场常态、市场 Tooltip 的 `core / focus` 与 `1280×720 / 1920×1080`，另有状态条和三张 `90%` 层级压力图。
- 报告：`led_text_brightness_metrics_v2_20260717.json`，`same_grade_equal_core_pass = true`。
- 当前状态：`样张待验收`；未使用图像生成模型或 Computer Use，未修改正式 Resources、C#、玩法、数据或存档。

### 2026-07-17 F021 市场浅色实体键帽实心字 V1 样张（退回）

- 用户输入：高亮浅色键帽不再使用点阵字；先出样张，用户同意之后再进入 Unity。
- 范围：卖出、三枚购买、离开市场；暗色刷新键与其它玻璃显示区继续 V4 LED。布局、热区、商品、价格、市场规则与经济均不变。
- `PASS`：原生 `1280×720 / 1920×1080` 分别直接栅格化，文字绘制后没有缩放或滤波。
- `PASS`：五处启用态完全共用 `Noto Sans SC 600`、`#180E08` 深墨和 `#D6B88C` 浅键底；无点阵、辉光、描边、阴影或磨损遮罩。
- `PASS`：启用态对比度 `10.0385:1`、禁用态 `5.0193:1`；五处实际文案和 `购买 999 金 / 卖出 999 金 / 交互已锁定` 压力文案全部装入且不裁切。
- `PASS`：提供全屏、1:1 键帽条、常态 / 悬停 / 按下 / 禁用状态板、可复现脚本和 JSON 指标。
- `FAIL`：用户认为实心字方向尚可，但新增纯色底板与原键帽纹理和明暗不连续，显得突兀。V1 只保留为问题对照，不得进入运行时。

样张证据：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketPhysicalKeyLabelsV1/`；机器报告：`market_physical_key_labels_metrics_v1_20260717.json`。

### 2026-07-17 F021 市场浅色实体键帽实心字 V2 样张与运行接入

- 修订范围：只删除 V1 新增的平色承载层；字形、字重、字号、字色和五处材质分流范围不变。
- `PASS`：五处键帽全部从 `arcade_market_common_base.png` 恢复原始无字表面；没有新增纯色矩形、铭牌、磨皮层、辉光、描边或阴影。
- `PASS`：双分辨率十个键帽区域均保留可测纹理方差；实心字仍分别原生绘制，完成后无缩放或滤波。
- `PASS`：最暗 `5%` 背景对比为 `3.69:1–4.82:1`，中位背景为 `5.00:1–5.93:1`；所有实际文案、有效字高与最长文案压力均通过。
- `PASS`：用户确认 V2，没有再要求纯色承载层修订，并授权进入 Unity。
- `PASS`：批准的 600 字重已固化为仓库内静态子集；51 个当前可达键帽字符全覆盖，字体家族 / 字重 / 许可证元数据正确，连续两次生成 SHA-256 一致。
- `PASS`：运行时只路由卖出、三枚购买和离开市场到实心字；暗色刷新键与其它显示区继续 V4 LED。正常态没有新增底板。
- `PASS`：Unity 2019.4.33f1 自动生成 normal / purchase / leave × `1280×720 / 1920×1080` 六图并通过状态报告；正式字体资源加载断言通过。
- `PASS`：启用字精确共用 `#180E08`，禁用字精确共用 `#30261D`；720p 核心字高为 `19 / 22 px`，1080p 为 `29 / 33 px`，达到合同下限。
- `PASS`：三枚购买键文字中心线跨度在两档分辨率均为 `0 px`；五处启用 / 禁用键帽的非文字亮度标准差均高于 `10`，证明原纹理未被平色覆盖。
- `PASS`：实现与验证未使用 Computer Use；没有修改市场布局、热区、价格、规则、经济、数据、随机或存档。

样张证据：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketPhysicalKeyLabelsV2/`；机器报告：`market_physical_key_labels_metrics_v2_20260717.json`。

运行证据：`Docs/QA/20260717_market_runtime_{normal,purchase,leave}_{1280x720,1920x1080}.png`、`Docs/QA/20260717_market_runtime_capture_status.txt`、`Docs/QA/20260717_market_physical_key_runtime_metrics_v1.json` 与 `Docs/QA/20260717_market_physical_key_labels_runtime_v1.md`。

### 2026-07-17 F021 LED 语义亮度 V2 运行接入与 `0.606×` 回归

- `PASS`：正式角色与同级亮度表进入 `Resources/Art/MainGame/Flow/`，Python 与 `MainGameLedFont` 共读同源 CSV；共 `10` 个角色，核心 Alpha 全部为 `1.0`。
- `PASS`：主菜单、局内和市场关键文字已改为语义角色入口；L2 / L3 只允许 Display 或 Compact，不再静默降为 Micro。
- `PASS`：Unity Play Mode 自动输出主游戏 / 主菜单 `10` 张和市场 `6` 张原生截图，`1280×720 / 1920×1080` 全部通过尺寸与资源加载检查。
- `PASS`：从 1080p 原图确定性生成三张 `0.606×`、`1164×654` 压力图；`3` 屏 `10` 个关键模块均检测到正式角色核心色和缩放后高亮色素。
- `PASS`：同级亮度离线 `320` 次检查失败 `0`；最大目标差 `0.000045263`、最大跨度 `0.000067127`。
- `PASS`：浅色实体键帽继续使用批准的实心字与原始纹理；专用运行验证失败 `0`，三枚购买键同线跨度 `0 px`。
- `PASS`：Unity 2019 Roslyn 静态编译错误 `0`；未使用 Computer Use，未修改玩法、经济、数据、随机或存档。
- `PENDING`：自动状态不能替代视觉验收；本轮三张运行结果仍待用户确认后再标记最终视觉通过。

运行证据：`Docs/QA/20260717_led_text_brightness_runtime_v1.md`、`Docs/QA/20260717_led_brightness_runtime_{main_menu,run_ready,market}_0606.png`、`Docs/QA/20260717_led_brightness_runtime_0606_before_after.png` 与 `Docs/QA/20260717_led_brightness_runtime_0606_metrics.json`。

### 2026-07-22 F021-S5-UI1 市场按钮与短时提示门禁

- 用户已确认固定动作文案、按钮同状态同表现、受阻说明点击、Tip 飞入后淡出和真实特殊资格映射。
- 本轮验收对象为三张源区样图，不是运行时资源或代码实现。

| 验收点 | 通过标准 | 状态 |
|---|---|---|
| 五状态完整 | 可用、悬停、按下、受阻、受阻点击均有独立画面 | 用户通过 |
| 同状态同构 | 同家族同状态的材质、文字、边光、凹凸和位移一致 | 用户通过 |
| 状态对比 | 玩家不读 Tip 也能快速区分可执行与受阻 | 用户通过 |
| 固定动作文案 | 卖出只显示“卖出”，商品键只显示“购买” | 用户通过 |
| Tip 位置 | 全画面中间偏上，`1280×720` 最终面板中心约 `(640, 200)`，不随按钮改变 | 用户确认修订 |
| Tip 节奏 | 飞入、完整停留、上浮淡出四节点可区分，总体不拖沓 | 用户通过 |
| 重复触发 | 规格明确为替换重播、不堆叠、不拦截输入 | 静态实现通过，待运行验证 |
| 混合资格 | 满袋时普通商品受阻，真实允许购买的特殊商品保持可用 | 用户通过 |
| 风格一致 | 保留老街机补给柜、原键帽纹理、V4 LED 与实心字材质分流 | 用户通过 |
| 范围守恒 | 样图只进入 `Assets/ArtSource/`，未改代码、正式资源、规则、经济、随机或存档 | 通过 |

用户已通过三张样图；`$wabish-packet-review` 已确认位置修订不会造成玩法、数据、存档或资源阻塞，本切片获准进入 `$wabish-dev-implementation`。

样图证据：

- `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketButtonActionSystemV1/market_button_five_state_contact_v1_20260722.png`
- `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketButtonActionSystemV1/market_action_tip_motion_storyboard_v1_20260722.png`
- `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketButtonActionSystemV1/market_mixed_eligibility_context_v1_20260722.png`
- `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketButtonActionSystemV1/README.md`

用户结论：除 Tip 位置统一改为全画面中间偏上外，其余样图没有问题，可以接入 Unity。图像生成编辑对部分非目标商品名与数字有重构，因此这些背景内容不计入文字、价格或数据验收；正式运行文字由 Unity 确定性绘制。

### 2026-07-22 F021-S5-UI1 Unity 静态接入

| 验收点 | 状态 | 证据 |
|---|---|---|
| 固定“卖出 / 购买 / 刷新 / 离开市场” | PASS | `DrawArcadeMarket` 静态合同检查 |
| 可用、悬停、按下、受阻、受阻点击共用状态入口 | PASS | `DrawArcadeMarketButton` |
| 受阻点击只解释原因、不执行交易 | PASS | 按钮入口无交易调用；返回 `Blocked` 后只显示 Tip |
| 逐商品读取真实购买资格 | PASS | 继续调用 `CanBuyMarketOffer` |
| 空货架无伪按钮，合法兼容商品仍可购买 | PASS | `HasArcadeMarketPurchaseAction` |
| 全局中上 Tip 与批准时序 | PASS | `(640, 200)`；`0.16s / 0.50s / 0.25s` |
| 单槽替换、重复重播、顶层非阻塞绘制 | PASS | `ShowActionTip / DrawActionTip / OnGUI` |
| Unity 2019 Roslyn 独立编译 | PASS | `0` 错误；`9` 个既有 `CS0162` 警告 |
| 720p / 1080p Play Mode 状态矩阵 | PENDING | 当前运行编辑器未刷新最新脚本，未覆盖历史截图 |

静态验证证据：`Docs/QA/20260722_market_button_action_tip_static_validation.md`。自动检查不替代 Play Mode 视觉和真实点击回归；在运行截图完成前，不将本切片标记为最终动态验收通过。
