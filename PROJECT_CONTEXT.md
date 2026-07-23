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
- Tone: light, witty, and relaxed, but not childish. Current presentation uses a midnight old-arcade / retro-electronic world; `骰子王` is a circuit champion title rather than a monarch. Royal bureaucracy, ledgers, seals, crowns, and palace packaging are legacy exploration unless a later feature explicitly re-approves them.
- Core fantasy: the player is not simply gambling. They are building a dice engine that turns randomness into a controlled scoring system.
- `V0.1` 当前视为完整原型基线；`V0.2` 的首要目标是解决不好玩和引导弱的问题。后续设计应优先强化成长反馈、复利感、小连锁和大连锁，避免普通投掷成为没有反馈的空回合。
- 首个商业版本的范围原则已由用户在 2026-07-15 确认：完整性和局部细节打磨优先于继续扩充内容与体验。应尽快确认并冻结首发内容边界；冻结后，即使新骰子、新家族、新系统或额外体验本身质量很高，也默认进入后续版本，不得挤占核心闭环、引导、结算可读性、反馈、美术统一、稳定性、质量验证、市场页素材和发行准备。只有用于补齐首发闭环阻塞、修复明显体验缺口，或在不扩大范围的前提下替换现有内容，才允许进入首发范围。
- 2026-07-22 用户最终确认并已静态接入首发生命池：生命是本次游戏整轮共享的失败次数，从 `global.csv` 的 `starting_lives` 读取初始值，首版为 `3`，不会自动恢复。每次真实出手完整结算并把得分加入本关累计分后，累计仍未达目标就立即扣 `1` 点生命；扣后仍大于 `0` 才保留累计分、钱包金币和全部已提交长期成长，在同一关经失败扫描回到可排序的 `Ready` 续投；扣到 `0` 立即结束本轮并清档，不存在 `0` 生命再投一手。续投只重置本手锁定结果、临时单骰分、临时骰和展示队列；“本关一次”标记不重置，“每次结算”效果在新一手正常触发。`DiceKingDemo.cs` 已接入生命字段、失败分支、稳定续投检查点和 HUD，运行存档升至 `SaveVersion = 12`；完整 Play Mode 流程与数值曲线复测仍待完成。
- `V0.2` 首版方向已由用户在 2026-06-23 到 2026-07-17 确认，并由 `Docs/Production/Features/F015-v02-core-loop-feed-market/` 承接：删除出千和小关多次出手，关闭全局牌型计分，保留六槽和左到右结算，用饲料作为提高最低点的成长核心，把市场买入、卖出、刷新和离开作为构筑发动机。当前内容名单冻结为四家族核心、中立与双阵营；历史骰子不再是产品内容，不允许通过数据、存档或市场入口回流。
- `V0.2` 后续所有新增或重做结算必须遵守已确认的“左到右主游标 + 当前槽位局部连锁队列”：当前槽位连锁清空后才进入下一槽；该规则已写入 `Docs/Production/Features/F015-v02-core-loop-feed-market/RULE_CARDS.md`。
- `Docs/Production/Features/F016-v02-pig-family-feed-loop-test/` 是 `V0.2` 的猪猪家族单独测试包，已在 `Assets/Scripts/DiceKingDemo.cs` 和数据表中静态接入，待 Unity 验证：测试模式下市场只刷新养猪、肉猪、贸易猪、母猪、三只小猪、贪吃猪和批发商；该包用于验证饲料生产、肉猪放大、贪吃响应、三只小猪得分、贸易猪卖出转移和批发商提高饲料质量是否能形成好玩的内循环。F016 已补入市场动作队列：每次买入或卖出按槽位左到右扫描一轮触发，买入在新骰入袋后扫描，卖出用被卖骰原槽位参与本次虚拟扫描；饲料获得、肉猪翻倍、贸易猪转移、批发商质量提升和刷新涨价已接入短标签 / 日志 / 市场反馈，仍待 Unity 截图和 Play Mode 验证。
- `Docs/Production/Features/F017-v02-devil-family-devour-shop-test/` 是 `V0.2` 的恶魔家族吞吃与养市场生产包，当前已静态接入代码和数据，未生成美术资源，待 Unity 编译 / Play Mode 验证：恶魔家族以吞吃和市场最低点成长为核心，首批规则入口为小鬼、吞噬骰子、恶魔骰子、魔蝠骰子、深渊召唤和贡品骰子；贡品只能由深渊召唤产生，不能作为普通随机市场骰刷出。F017 已确认吞吃和后续吸附共用期望转移逻辑，首版整数面组使用来源骰当前六面平均值向下取整且最低 1；小鬼提高的市场基础最低点本轮持续；每颗魔蝠离开市场最多吞吃 1 颗市场骰；购买后货架留空且不自动补货，只有进入市场和付费刷新会整批补货，进入市场也算刷新；有深渊召唤时每次刷新固定 1 个贡品，多颗深渊召唤首版不叠加；恶魔测试池只随机刷五颗常规恶魔骰，贡品权重保持 0。吞吃后的目标骰会保留最近一次吞吃来源和加值，用于详情页确认贡品 / 魔蝠效果；有魔蝠这类离场效果时，点击离开市场后锁住购买 / 卖出 / 刷新 / 拖拽，按当前骰袋位置从左到右自动触发所有离场效果，逐个展示完毕后自动进入下一小关或完成本轮；随后主投掷区仍会显示市场反馈条，目标骰在该反馈窗口显示 `吞+N` 短标签。当前 `global.csv` 默认关闭 F017 恶魔测试池。
- `Docs/Production/Features/F018-v02-turtle-attachment-test/` 是 `V0.2` 的龟龟家族吸附测试包，当前已静态接入代码和数据，未生成最终美术资源，待 Play Mode 验证：六颗可吸附龟在离开市场进入下一小关前按当前骰袋左到右扫描，只吸附到后一个位置的非龟实体骰；后一个位置不是非龟实体骰时保留实体，不跨格查找，也不播放吸附表现。真实吸附会让来源龟化壳消失，从骰袋实体列表移入宿主吸附列表，并把来源骰当前期望转移值加到宿主全部面；市场表现为来源龟飞向后一位并消失，宿主高亮。首领龟不吸附到其它骰子，作为独立实体骰监听真实吸附事件，给自身获得同类型复制壳，首领之间和复制壳之间不递归触发。宿主结算时按吸附顺序执行龟壳效果：金钱龟进左到右钱包流，小小龟和磁力龟生成基础壳，幸运龟按最大面结算，呼朋唤友龟只读取其它骰子的真实吸附龟数量并给面组成长；双倍龟在吸附时一次性翻倍宿主当前最小面。真实吸附龟卖价随宿主卖出回收，基础壳和首领复制壳不计卖价。当前 `global.csv` 默认关闭所有单家族测试池，正式池包含这 7 颗龟龟核心骰。
- `Docs/Production/Features/F020-v02-neutral-family-council-test/` 的 8 颗中立骰已静态接入并通过 Unity 脚本编译：中立不作为第五个主体家族；主体家族只统计当前实体骰中的猪猪、恶魔、龟龟和海盗，吸附壳层不提供家族身份；二重唱和触发器只处理主体家族真实结算事件；光牙不回改本次锁定点数；王冠读取各主体家族最高最终有效点。
- 2026-07-17 双阵营 9 颗骰正式纳入首发名单：`黑帆魔蝠 / 驮粮龟 / 贡猪 / 黑市小鬼 / 血契船长 / 清仓猪 / 补给猪 / 共食魔龟 / 托底龟`。双阵营实体同时响应两个主体家族标签；吸附后只保留壳效果，不给宿主追加家族身份。同名完整叠加、黑市允许把刷新压成负费用返金、血契完整发布购买监听、共食分流不递归、托底新壳只影响后续结算。九颗进入默认 44 枚正式随机池；`v02_dual_family_market_only = 1` 只用于九骰隔离测试。
- 当前打开的 Unity Editor 已完成 `Assembly-CSharp` 与 `Assembly-CSharp-Editor` 重编译，无新增错误；双阵营效果仍需完成 Play Mode 功能矩阵与截图验收。
- `V0.2` 槽位交互首版改为市场式拖拽插入：`Ready` 投骰区和市场骰袋列表都可以按住骰子拖动排序，松手提交新的从左到右顺序，右键或 `Esc` 取消；单击只选中，不再用两次点击交换。排序只移动列表顺序，不改骰子 ID、饲料、成长、卖价、随机结果或市场货架。
- 2026-07-15 主游戏骰子本体的信息职责已收紧：常态与最终结果阶段只直接表达“当前点数 + 骰子种类”。成长、六面分布、吸附、词缀、来源和其它状态均由既有悬浮详情承接，不再堆叠在骰子本体上。结果阶段只显示本次最终有效点数，不在侧面或周围同时露出其它骰面点数。数字系统必须在主游戏实际尺寸下覆盖一至四位数，正式评审以最宽的 `888 / 8888` 作为极限样例。
- 2026-07-15 F021 已确认骰子跨状态目标识别语法：宽外壳只表达中立 / 猪猪 / 龟龟 / 恶魔 / 海盗家族，固定“类型芯”表达具体骰子类型，数字面只显示已锁定的当前有效点数，悬浮详情承接六面、完整效果、成长、吸附、来源和其它个体状态。`Ready` 与市场商品没有当前结果时放大类型芯且不得显示第一面、随机点数或其它伪结果；结果锁定后类型芯缩回底部固定插槽并让数字接管中心。双家族骰拼接两个家族壳段但仍只使用一个具体类型芯；中立不是第五个主体家族。用户已认可两张 V1 的显示方式和功能分区，但要求继续调整整体美术风格；尚未批准运行时拆层，当前 F009 点阵 / 小类型标记实现继续作为代码现状而非最终目标。运行时必须继续区分骰面有效点数与该骰真实贡献分：后者在 `Scoring` 中使用临时 `+N 分` 反馈，不塞入悬浮详情或冒充骰面点数。
- 2026-07-15 F021 主游戏后续范围进一步收紧：`ResultDecision` 暂不增加“本次预计 +N 分”总预览；来源骰下方的临时骰余波托盘、逐颗实体与超量折叠提案废弃，不为它们预留新的主界面区域。该决定只取消 F021 新展示方案，不自动删除现有临时骰玩法和计分。后续换肤已确认走旧娱乐街机方向，保持现有功能分区不动，减少军工 / 蒸汽 / 硬工业器件，增加老化塑料、烟熏玻璃、琥珀点阵、旧式贴花和半透明街机键帽。
- 2026-07-15 F021 通关与失败收束 V1 三张评审图已获用户确认，冻结为静态视觉基准：三分支均保留最终六骰和当前积分，右侧规则终端改显结果，底部 `Space` 作为唯一分支操作。通关显示固定金币、基础利息和复利并进入市场；剩余生命失败显示分差、扣除后生命并再次出手；生命归零显示本轮结束并返回开始界面。通关暖光和失败锈红只作用于局部结果区，不使用整屏弹窗、红洗或赌场式爆灯。评审图中的 `3 生命` 已由 2026-07-22 的规则确认并接入为首版配置值；动态 Play Mode 验收仍单独保留。
- 2026-07-16 用户已批准并完成 F021 主游戏共用运行时资源包 V1 接入：`Run` 的 `Ready / Shaking / Stopping / ResultDecision / Scoring` 在共用底板存在时切换到独立 16:9 老街机竞技台，顶部为章节 / 生命 / 金币，左侧为目标 / 当前与积分灯条，中央为可排序骰子，右侧为单终端，底部为阶段化 `Space`。中立、猪、恶魔、龟、海盗使用独立家族壳；双家族由两张现有壳左右拼接；待机显示类型图标或短类型名，结果只显示当前有效数字并已实测 `123 / 1234`。资源缺失时回退旧主游戏绘制或中立壳。Unity 2019.4.33f1 已在 `1920x1080 / 1280x720` 对 Ready、结果、结算和三 / 四位数字完成八图 Play Mode 验收；生命 HUD 与通关 / 失败分支的最新动态仍需补做 Play Mode 回归。
- 2026-07-16 F021-S2 V1 八图之后的人工检查发现中央六枚骰子与底图凹槽未重合。R1 把共用骰台区域从 `(188,278)` 改到 `(204,301)`，并把街机单行间距从 `18` 改为 `16`，使六骰中心落在底图 `X=299+128n / Y=369`；骰子尺寸与资源、拖拽 / 结算逻辑、玩法、数据、存档和随机均不改变。R1 双分辨率截图与拖拽人工检查完成前，不把 S2 标记为人工验收通过。
- 2026-07-16 F021-S2-R3 当前中央六槽候选契约：R2 的 `112×112` 内槽观感已被用户否定，运行时改按底图卡槽边框使用六个 `128×128` 物理槽，中心 `X=299+128n / Y=368`、中心距 `128`、逻辑净距 `0`。五张家族壳保留原 `512×512` PNG 和公共 UV `(32,27,448,458)`，映射后可见壳约宽 `123.1–127.7`、高 `127.4–128`；双家族、中心内容、拖拽和结算继续共用同一 `slotRects`，高亮与选择框必须槽内内缩。该候选待 R3 双分辨率截图和用户人工贴合度确认后再转为最终冻结值。
- 2026-07-16 F021 九状态接触图 V1 的主体老街机风格已获用户认可，但文字与骰子贴槽需要统一修订：主游戏所有 HUD、终端、结果、提示和按键文案改用开始界面 `DICE KING` 灯牌同系的真实 LED 点阵字，必须能看见规则灯珠与点距，不再混用平滑印刷字体；默认琥珀、目标越线可用青白、失败可用局部锈红。骰面锁定数字仍是非自发光实体压印数字，不纳入 LED 字体。所有九状态的六骰继续严格复用 R3 `128×128` 连续物理槽，壳体映射后必须贴近槽内沿，不允许为接触图缩成小图标或拉开独立间距。V2 源图进入样张复核，未切运行时资源。
- 2026-07-17 用户已通过 F021 主游戏五条无声动态样片：投骰—停转、逐骰结算—目标越线、通关三拍收入、失败扫描重试、生命归零分层断电。LED 点阵字、R3 六槽贴合、九状态同机位和五条节奏正式成为 Unity 表现层实现基准；短片仍只保存在 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MotionPreviewsV1/`，不得作为运行时视频直接播放。失败扫描重试与生命归零断电现已接入状态机：初始生命为配置值 `3` 且不会自动恢复，可重试失败保留本关累计分和已提交效果，扫描只复位每手状态；最新动态仍待 Play Mode 验收。
- 2026-07-17 F021 新增运行时 LED 字体质量门：美术预览通过不再等同于运行字体通过；所有字体评审先由离线脚本读取实际 atlas / map、运行坐标、缩放、换行、对齐与辉光生成同源真值板，并同时输出 `1280×720 / 1920×1080` 和机器指标。`1280×720` 下核心灯珠不得小于 `2 px`、主信息有效字高不得小于 `22 px`、正文不得小于 `18 px`，且区域不得越界或缺字。C# 最终使用命名样式而非散落任意字号，Unity 自动截帧只作为同源板通过后的最终冒烟；Computer Use 不作为常规验收依赖。V1 的 A / B 已被用户退回并只保留为失败证据。
- 2026-07-17 F021 LED 字体校准板 V1 已被用户退回：A / B 对比批准预览仍有差距且显示模糊，正式接入暂停。此前“核心灯珠不少于 `2 px`、字高与装入通过”的机器结果只属于必要条件，不能覆盖锐度。后续用于锐度验收的目标分辨率裁切必须 1:1 无重采样；放大检查只能使用整数倍 `Point / NEAREST`；总览缩略图只导航；低分辨率字图不得缩放到非整数像素尺寸；无辉光硬核心必须先单独通过，辉光占比和扩边再独立验收。V1 A / B 均保留为失败证据，不得接入正式运行字库。
- 2026-07-17 F021 LED 字体 V2 已由用户确认“清晰度没问题”，因此原生分辨率、整数灯珠、1:1 裁切、无二次缩放和核心 / 辉光分层规则冻结；但 V2 的 `7×7–10×10` 自动中文字形无法识别，整套字形不得接入。V2 当时至少要求分别记录像素清晰度、字形身份和区域装入；“缺字为零、装得下”不能证明字形可读。V3 随后以 `16×16` hinted mask、720p `2 px` 核心 / `3 px` 点距和真实复杂字盲读进入候选门，其字形身份结果已在下一条更新。
- 2026-07-17 用户已确认 F021 LED V3 字形可识别，并在 V4 复核中通过共享基线。复核确定 V3 的问题来自逐字顶对齐；V4 完整复用原活动字形，只以整数位移让中英数落到逻辑第 `14` 行、悬浮标点落到固定中线。字体验收固定拆为“像素清晰度 / 字形身份 / 基线排版 / 区域装入”四门，V2 清晰度、V3 字形与 V4 基线均已关闭。
- 2026-07-17 用户进一步将 V4 指定为默认全局文字标准。正式 `850` 字形 atlas / map、共享 `main_game_led_font_styles.csv` 中的 `Display / Compact / Micro` 三档命名样式和整数物理像素渲染器已接入；字形数量由生成器按当前代码 / 数据字符集合动态重算，当前缺字为 `0`。离线生成器与 C# 运行时读取同一命名样式合同，并使用同一文字框物理像素裁切，避免预览能装入而 Unity 被截断。`DiceKingDemo.cs` 的主菜单、开场、主流程、设置、市场、Tooltip、结算与旧回退界面动态文字统一走 `DrawStandardLabel`。明确例外包括骰面非自发光实体压印数字、已单独批准的 `DICE KING` 实体点阵招牌，以及下一条定义的浅色实体键帽实心字；后者尚未接入运行时。主菜单底图中的旧平滑菜单文字必须由不透明动态层覆盖；本次对关间市场只统一字体，不改变市场布局或流程。正式验收不依赖 Computer Use：同源生成器输出 `1280×720 / 1920×1080` 真值板和实际上下文图，Unity Roslyn 独立编译为 `0 error`；最新 Play Mode 交互仍需在编辑器完成资源刷新后单独复核。
- 2026-07-17 用户认可 F021 LED 的 `L3 / L2 / L1 / L0 / Disabled / InkOnLight` 信息分级，并新增全局“同级等亮”约束：同一亮度等级的琥珀、青色、暖白与成功 / 失败状态色必须使用同一目标感知亮度，色相只表达焦点或状态，不能形成第二套隐含层级。亮度以不透明硬核心的 `OKLab L` 为离线 / 运行时共享指标；焦点辉光为独立状态层，不参与核心亮度成立。V1 只保留为同级不一致证据；V2 同源样张已生成并通过机器门，尚待用户视觉确认，确认前不接入 Unity。
- 2026-07-17 用户已通过关间市场三状态换肤 V2 样张，冻结“午夜旧机厅·后台补给柜”为市场运行时美术基准：左侧连续骰袋、右侧三格商品柜、顶部资源窗、底部刷新 / 状态反馈 / 离开键；商品沿用家族壳 + 放大类型芯且不伪造点数。购买后原货格表现为空插槽，付费刷新表现为整柜回收与装载；离场时先锁定交互，再按骰袋从左到右执行扫描、吸附和货架离场效果。批准只覆盖视觉方向与三状态表现，不新增或修改市场经济、购买、刷新、卖出、存档和随机规则。随后已生产并接入 `Art/Market/arcade_market_common_base` 无字底板；Unity 动态绘制文字、数字、家族壳、类型芯、价格、悬浮候选、空货架、锁定与扫描，资源缺失或词缀功能重新开启时回退旧市场。Unity 2019.4.33f1 已对常态、购买留空和离场锁定在 `1920×1080 / 1280×720` 生成六张 Play Mode 截图并通过尺寸与画面结构检查；真实鼠标 Tooltip、完整拖拽手势和字体专项仍保留人工门禁。
- 2026-07-17 用户确认关间市场的浅米色 / 奶油色实体键帽不再使用 LED 点阵字：卖出、三枚购买和离开市场使用深墨实心印刷 / 压印字；暗色青色刷新键、商品名、六面、反馈、资源窗和 Tooltip 继续使用 V4 LED。该规则按承载材质分流，不是整页改字体。首版 `PhysicalKeyLabel` 单一样张位于 `Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/MarketPhysicalKeyLabelsV1/`，当前待用户验收；用户明确要求样张通过后才接入 Unity，因此正式字体资源、C# 路由和 Play Mode 冒烟均继续暂停。
- 2026-07-17 用户退回 `PhysicalKeyLabel V1` 的新增纯色承载层：实心字方向基本可用，但纯色矩形把原键帽的磨损、颗粒和局部光影盖掉，显得像后贴标签。后续浅色实体键帽不得为清字或提对比另铺平整底板；应从无字市场底图恢复原始键帽材质，再把实心字直接印在表面。V2 已按该约束生成原生双分辨率样张，当前待用户验收；V1 只保留为失败对照，仍未进入 Unity。
- 2026-07-17 用户随后通过 `PhysicalKeyLabel V2` 并授权进入 Unity。运行时已将批准的 600 字重裁成仓库内静态子集 `Wabish Physical Key Sans SC SemiBold`，加载 key 为 `Art/MainGame/Flow/wabish_physical_key_sans_sc_semibold`；卖出、三枚购买和离开市场使用原生物理像素实心深墨字，暗色刷新键与其它显示面继续 V4 LED。常态不新增底板，悬停只亮边，按下压暗并下移文字，禁用保持原材质与灰褐实心字。Unity 2019.4.33f1 已自动生成常态、购买留空、离场锁定的 `1280×720 / 1920×1080` 六图；字体加载、双分辨率字高、启用 / 禁用同级色、三枚购买键 `0 px` 中心线偏差和键帽纹理保留全部通过。该接入没有改变市场布局、热区、价格、规则、数据或存档，也未使用 Computer Use。
- 2026-07-22 用户确认关间市场按钮与短时提示采用统一状态合同：按钮家族只决定基础材质与尺寸，同一交互状态必须共享亮度、饱和度、凸凹、边光、位移和反馈节奏。卖出键固定显示“卖出”，三枚商品键固定显示“购买”，离场键固定显示“离开市场”；价格、成功结果与“骰袋已满 / 金币不足”等受阻原因不再替换按钮文案。浅色实体动作键统一使用可用、悬停、按下、受阻、受阻点击五种表现；受阻点击不执行动作，但保留说明命中层并调用统一短时提示。动作 Tip 的最终位置统一为全画面中间偏上，在 `1280×720` 虚拟画布中以约 `(640, 200)` 为面板中心，从终点下方 `24–32 px` 飞入，不再跟随具体按钮。提示约 `0.14–0.18s` 飞入、`0.45–0.55s` 停留、`0.22–0.28s` 上浮淡出，总长约 `0.85–1.00s`，重复触发时替换重播、不堆叠、不拦截输入。按钮状态必须读取真实动作资格，血契船长满袋仍可购买时继续显示统一可用态。三组样图已获用户通过并授权接入 Unity；本合同由 `Docs/Production/Features/F021-full-flow-rhythm-sample/` 的 `F021-S5-UI1` 承接。
- 2026-07-22 `F021-S5-UI1` 已静态接入 `Assets/Scripts/DiceKingDemo.cs`：`DrawArcadeMarketButton` 统一返回可执行或受阻结果，受阻键仍可说明点击但不会进入交易；卖出、购买、刷新和离场使用固定动作名，原因由独立解析入口交给统一 `ShowActionTip / DrawActionTip`。Tip 使用单槽替换，固定落在 `(640, 200)`，从下方 `28 px` 以 `0.16s / 0.50s / 0.25s` 飞入、停留和上浮淡出，并在 Tooltip 之后无输入热区绘制。商品资格继续读取 `CanBuyMarketOffer`，空货架不创建按钮，骰子和兼容改造道具商品共用“购买”；价格、经济、市场池、随机、骰子效果、数据与存档没有变化。Unity 2019 Roslyn 独立编译为 `0` 错误，Play Mode 双分辨率与真实点击回归仍待运行编辑器刷新后补验。
- 2026-07-23 用户已在 V7-R2 五档源样片交付后确认“可以，继续推进吧”，允许五档方向进入 Unity。V7-R1 继续冻结为最高暴击母版：逐击按游戏 `1.0x` 展开，最终以“锁死—吸光—近静默真空—单次击穿—余震”释放；近黑、近静默、全屏击穿和强后坐只属于最高档。其余隐藏四档固定为未达到反向泄压、过关单拍闭环、超过双拍继电、远超过可控整机过载，不能只做亮度缩放，也不能提前使用最高档的真空与击穿。

## Core Loop

The current end-to-end flow is tracked in `GAME_FLOW.md`. Keep this section as the high-level roguelite scoring loop only.

1. Enter a small encounter with a target score.
2. Roll the current dice bag for the encounter, up to six dice.
3. Roll once, then apply dice type effects, feed minimum-point growth, temporary dice effects, encounter rules, and any later modifiers through the left-to-right settlement queue. Global hand/poker scoring is disabled in V0.2.
4. If the score reaches the target, resolve stage-clear fixed gold and interest income after any roll-time wallet income, then enter the inter-stage dice market.
5. Inter-stage and chapter markets let the player buy dice, buy crafting items, use market-stage crafting items, sell dice, refresh offers, or leave if the dice bag is not empty.
6. Continue through configured normal encounters and boss encounters.
7. A failed hand consumes one shared run life after its score and effects commit. If lives remain, keep the stage's cumulative score and committed state, reset only per-hand state, and allow another hand; when lives reach zero, end the run and clear run progress.

## Confirmed Demo Scope

- The current prototype is implemented as a self-contained single-file demo in `Assets/Scripts/DiceKingDemo.cs`.
- The demo self-bootstraps at scene load via `RuntimeInitializeOnLoadMethod`, so `Assets/Scenes/SampleScene.unity` can be opened and played without manually wiring a component.
- 当前原型覆盖 `GAME_FLOW.md` 中确认的主流程：主菜单、设置、首次进入开场、默认初始骰袋、小关循环、单次出手结果决策、无出千结算、关间市场、章节市场、胜利和失败。
- 2026-07-21 设置页已接入 `Windowed / FullScreenWindow / ExclusiveFullScreen` 三种显示模式、窗口尺寸或独占全屏分辨率选择与十秒保留确认。Windows 独立包的窗口模式使用系统原生可缩放标题栏，双击标题栏按系统习惯最大化或还原；该最大化状态仍属于窗口模式，保留任务栏，不作为第四种显示模式。
- Prototype continue/save uses `PlayerPrefs`. Normal stage and market saves resume through the existing stage-entry path; a life-loss checkpoint resumes directly at a stable `Ready` state with cumulative score, post-deduction lives, wallet, committed die state, locked investments, and stage-once markers preserved.
- 当前 `DiceData` 写入骰子类型 key、当前面组、饲料计数、饲料价值、最近吞吃状态、龟龟吸附列表、实例卖价、本关投资锁金和本关一次性触发标记；运行存档使用 `SaveVersion = 12`，读取到旧版本运行记录时直接清空本轮记录，保留菜单设置。
- 生命续投检查点保存整轮剩余生命、本关累计分、`StageContinuationPending`、本关开始金币、投资锁金、已提交长期状态和本关一次性标记。失败手结算并扣命后立即写入；恢复时只清理每手状态，不恢复小关开始快照，也不撤销已提交金币与成长。
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
  - `DiceFaces/f009_unified_result_die_faces_6x256.png`: F009 统一 1 到 6 点结果骰面 strip，用于 `ResultDecision` 和 `Scoring` 阶段；`7+` 点使用运行时骰面底图加居中数字。
  - `DiceRoll/f009_table_friction_spin_loop_strip_24f_256.png`、`DiceRoll/f009_table_friction_spin_stop_strip_8f_256.png`: F009 旧版桌面摩擦 strip，仅作为统一资源缺失时的运行回退。
  - `UI/ui_panel_ledger.png`, `UI/ui_card_clip.png`, `UI/ui_small_panel.png`: generated parchment UI section/card/detail textures.
  - `UI/ui_button_primary.png`, `UI/ui_button_secondary.png`, `UI/ui_button_danger.png`, `UI/ui_button_disabled.png`: generated runtime button textures.
  - `UI/ui_icon_coin.png`, `UI/ui_icon_refresh.png`, `UI/ui_icon_settings.png`, `UI/ui_icon_close.png`, `UI/ui_icon_target.png`, `UI/ui_icon_sell.png`, `UI/ui_icon_bag.png`: generated runtime UI icons.
  - `Items/affix_add_stone.png`, `Items/affix_remove_stone.png`, `Items/affix_replace_stone.png`: F005 market crafting item icons.
  - Full generated-asset registry is tracked in `ART_ASSETS.md`.
- 当前运行时 `Assets/Resources/Art/DiceTypes/` 只保留当前名单仍会读取的基础、猪猪和龟龟通用图标；其余当前类型在专属资源补齐前使用家族壳、类型芯文字或程序化回退。历史接触图可以留在 `ArtSource` 作为美术档案，但不得注册为运行时骰子内容。
- Current runtime shader resources:
  - `Assets/Resources/Shaders/DiceMaterialOverlay.shader`: F002 旧材质表现 shader 保留为兼容和后续重设素材；当前 `DiceMaterialFeatureEnabled = false` 时运行时不加载、不绘制材质层。
- Current chapter structure:
  - Global economy constants are loaded from `Assets/Resources/Data/global.csv`.
  - Encounter structure is loaded from `Assets/Resources/Data/chapter_score_table.csv`.
  - Dice market prices, sell prices, tiers, and chapter-band offer weights are loaded from `Assets/Resources/Data/dice_market_config.csv`.
  - Market refresh costs and high-tier refresh pity are loaded from `Assets/Resources/Data/market_rule_config.csv`.
  - `Assets/Resources/Data/dice_material_config.csv` 保留旧材质价格、权重、显示名和短规则；当前材质总开关关闭时不加载、不参与市场、UI、价格或结算。
  - `global.csv` 中 `starting_lives = 3` 是整轮共享初始生命，失败扣除且不会自动恢复；`affix_feature_enabled = 0` 是 F013 词缀功能总开关，表示 F005 词缀和改造道具隐藏且不生效，但不删除数据或存档字段。
  - V0.2 四家族 + 中立 + 双阵营第二轮降压曲线已接入：`chapter_score_table.csv` 使用全 40 关曲线，所有关卡 `max_rolls = 1`、`max_cheats = 0`，`HandAudit` 已替换为 `None`，`LowFog` 只惩罚 1 / 2 点，`DoubleJudge` 只保留偶数 / 奇数单骰修正。各章四关目标依次为：`14/18/19/21`、`20/22/27/32`、`31/34/38/46`、`42/52/57/67`、`62/75/83/98`、`89/102/120/140`、`132/152/173/203`、`188/223/252/293`、`272/321/367/426`、`395/460/530/616`。本轮依据 10 次不启动 Unity 的固定种子自动流程样本，优先降低首章随机低点淘汰与中期 Boss 卡点，中后段整体约下调 8%–10%；猪猪长期叠面与小鬼龟龟等极端爆发留作效果上限问题，不用于反向抬高全局门槛。该曲线原按单手直接比较目标校准；生命累计续投已接入，必须重新记录每关耗命、续投差值与长期收益后复核整条曲线。
  - V0.2 正式市场生成规则已接入当前原型：`v02_core_families_market_only = 1` 时，随机货架只允许新版猪猪 7、恶魔 5、龟龟 7、海盗 8、中立 8 与双阵营 9，共 44 个类型。基础骰不进入随机市场；贡品仍只由深渊召唤固定生成。任何未列入当前名单的类型都会在配置解析、存档读取、实例创建和市场最终过滤处被拒绝。单家族、中立或双阵营测试开关显式开启时会临时覆盖正式白名单。
  - F013 正式经济候选已接入：`starting_gold = 8`，`stage_clear_base_gold = 3`，剩余出手和剩余出千金币奖励保持 `0`，基础利息为每 `6` 金 `+1`、每关封顶 `4`。
  - 正式市场继续读取 `dice_market_config.csv` 的章节段权重、价格、阶级和高阶保底；同一市场内付费刷新费用按 `market_refresh_cost_step` 递增，防止刷新海盗将无限刷新变成无成本循环。正式白名单与各专项测试池只生成骰子商品，不生成 F005 改造道具。
  - 2026-07-17 新版效果语义审计已统一负点数边界：最高面 / 最低面、饲料最低点、幸运龟、双倍龟与吞吃来源都读取真实长期面组，不再把每个面预先钳到 1；吞吃只对最终向下取整的平均转移值保底为 1。光牙同次结算不会把同一双阵营实体重复选作两个家族目标，奇数规则支持负奇数，`LowFog` 不再重复惩罚 0 / 负点数。
  - F004-01 投骰表现默认配置由 `Assets/Resources/Data/roll_feedback_config.csv` 读取；调试期可用 `Application.persistentDataPath/roll_feedback_config.csv` 覆盖，并用 `F5` 运行时重载当前全局配置。
  - F004-01 在 `BeginShakeRoll` 起摇时捕获本次投骰配置快照；后续重载只影响下一次投骰，不写入 `PlayerPrefs`，也不消耗 `UnityEngine.Random`。
  - F004-02 已将旧的“敲空格延迟停止”状态机替换为固定加力窗口：窗口内有效 `Space` 只按配置快照施加表现冲量、受去抖和上限约束；窗口结束进入回落，回落阶段额外 `Space` 不改变强度、时长或结果。
  - F004-03 已替换运行时代码中的旧投骰提示文案；配置层仍保留加力窗口、提示脉冲和旧骰盅运动参数，供回退路径或后续特殊表现复用。
  - F009 序列帧式固定槽位旋转首版已接入当前原型：主投掷区使用 OnGUI 序列帧播放器表现小关入场、槽位内旋转、左到右停转显点和结算点名；默认 F009 路径取消骰盅阶段，不再做抛投、跨桌位移或持续摇晃形变。`Ready` 和结算后重置阶段优先读取 `Assets/Resources/Art/DiceRoll/f009_unified_ready_die_256.png`，非基础骰由代码叠加小类型标记；`Shaking` 优先读取 `Assets/Resources/Art/DiceRoll/f009_unified_spin_loop_strip_24f_256.png` 的 24 帧统一旋转 strip；`Stopping` 优先读取 `Assets/Resources/Art/DiceRoll/f009_unified_spin_stop_strip_8f_256.png` 的 8 帧统一停转预览 strip；`ResultDecision` 和 `Scoring` 的 1 到 6 点结果优先读取 `Assets/Resources/Art/DiceFaces/f009_unified_result_die_faces_6x256.png`。旧桌面摩擦 strip 和程序骰面仍作为资源缺失回退；`7+` 点使用运行时骰面底图加居中数字。停转阶段按槽位从左到右逐步停住，停住的槽位立即切回真实 `EffectiveValue` 点数绘制，进入 `ResultDecision` 后保持稳定结果供结算选择；玩家确认结算后不再插入单独的“结果定格 / 账本定格”停顿，直接从原槽位高亮、轻抬和分数飞字进入左到右点名。视觉帧只读取 `RollPhase`、当前槽位、`EffectiveValue`、`SettlementDisplayEvent` 和计分展示状态，不调用 `ScoreDice()`，不写入 `PlayerPrefs`，不改变骰面概率、钱包、成长、临时小骰或结算公式。调试期可用 `F6` 开关该视觉层以回退到原 2D 展示路径；仍需 Unity Play Mode、录屏和 `1280x720` / `1920x1080` 截图验收。
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
  - Starting wallet and shared run lives are loaded from `global.csv`; current baselines are `starting_gold = 8` and `starting_lives = 3`.

## Design Pillars

- Dice should be readable by `type` first:
  - Faces: raw probability and base values.
  - Type: the concrete die identity within pig, devil, turtle, pirate, neutral, or dual-family content.
  - Family shell: the broad engine identity; dual-family dice visibly combine two family shells.
  - State: feed, face growth, devour history, turtle attachments, instance sell value, and other current long-term state.
- Dice types should create different play patterns through the current family engines. Material and affix systems are outside the active first-release content path.
- Future dice expansion has moved past the first internal route-completion pass. Stable follow-up direction is to break closed build walls by designing dice as more general mechanism providers: each new or reworked die should state its trigger condition, benefit type, and settlement interface, so it can be absorbed by multiple builds instead of only serving one fixed route. Type identity still remains the first readability layer, and individual new dice still require approved rule cards before implementation.
- V0.2 新骰设计优先使用已确认的公共触发点：最高点、回合结束、金币、槽位、市场买入、卖出、刷新、货架生成、离开市场，以及四家族特有触发点。牌型和出千不属于当前新增内容接口。
- 龟龟当前方向是实体吸附与壳效果：读取后一槽、非龟实体、真实吸附、其它骰真实龟数量、吞吃成长和饲料托底，不再以临时骰壳链作为家族核心。
- 猪猪、恶魔、龟龟、海盗分别承担饲料、吞吃、吸附、市场经济；中立连接多家族，双阵营桥接两个家族并提高跨家族成型率。
- F002 市场独特骰中的材质逻辑当前已整体软关闭，等待 V0.2 重新设计：市场新生成商品骰只由 `类型 + 随机面组` 构成，材质字段保留为 `None`；旧存档中的材质 key 可继续读取和保存，但不显示、不计价、不加载 shader、不触发分数或金币效果。卖出价仍按类型固定回收。
- F005 词缀和改造道具当前处于 F013 软关闭状态：配置、图标和存档字段继续保留，`affix_feature_enabled = 0` 时不进入市场、UI、预览、真实结算或钱包收益。后续若恢复词缀，需要另开包决定是否沿用旧存档内容。
- 当前内容名单已经冻结；新增骰子、新家族或恢复下线系统默认进入后续版本，不能通过关闭正式白名单重新回到首发市场。
- Rewards should support both committed routes and cross-route experimentation.
- Bosses should be rule distortions, not just larger target numbers.
- Dice archetypes are tracked in `DICE_ARCHETYPES.md`. Current confirmed archetype directions include:
  - Feed growth: pig dice produce, amplify, move, and monetize feed.
  - Devour growth: devil dice improve tavern dice and transfer their expected value into the bag.
  - Attachment growth: turtle dice become persistent shells on non-turtle hosts.
  - Market economy: pirate dice turn buying, selling, refresh, empty shelves, and wallet events into power.
  - Family council: neutral dice reward readable multi-family compositions.
  - Dual-family bridges: nine dice participate in two family identities and join the formal market.

## Current Dice Types

- 当前随机市场共 44 枚：猪猪 7、恶魔 5、龟龟 7、海盗 8、中立 8、双阵营 9。
- 猪猪：`PigFarmer / MeatPig / TradePig / SowPig / ThreeLittlePigs / GreedyPig / FeedWholesaler`。
- 恶魔：`Imp / Devourer / Demon / DemonBat / AbyssSummon`。
- 龟龟：`MoneyTurtle / TinyTurtle / DoubleTurtle / LuckyTurtle / MagnetTurtle / RallyTurtle / LeaderTurtle`。
- 海盗：`RefreshPirate / PlunderPirate / CrewPirate / PirateCaptain / TrainingPirate / TreasurePirate / RobberyPirate / PirateKing`。
- 中立：`Lightfang / Duet / Trigger / Crown / Relief / Airstrike / Pact / Stitch`。
- 双阵营：`BlackSailBat / PackTurtle / TributePig / BlackMarketImp / BloodPactCaptain / ClearancePig / SupplyPig / SharedFeastDemonTurtle / SafetyNetTurtle`。
- `Basic` 只用于初始骰袋、人口海盗招募与安全兜底；`Tribute` 只由深渊召唤生成。两者都不进入随机市场。
- 完整效果、家族身份与验收边界统一维护在 `DICE_ARCHETYPES.md`；本文件不再维护第二份逐骰规则，避免规则漂移。
- 历史骰子不属于当前名单；数据解析、存档读取、实例创建和市场最终过滤都必须拒绝名单外类型。

## Scoring Notes

- V0.2 使用直计：实体骰最终有效点数、当前骰类型效果、龟壳效果、长期面组成长、金币事件和遭遇规则相加；不计算全局牌型倍率。
- 每小关初始有一次真实出手；本手得分加入本关累计分后与 `target_score` 比较。累计未达标会扣除整轮共享生命，扣后仍有生命才在同一关追加一手并继续累计。
- 1 点有效点数对应 1 点基础单骰分；0 与负点数按真实值进入计分，不做逐面保底。
- `OddLedger`：负奇数也按奇数判断，奇数有效点数单骰分 `+1`。
- `LowFog`：只对有效点数恰为 1 或 2 的骰子单骰分 `-1`；0 与负点不重复受罚。
- `DoubleJudge`：偶数单骰分 `+1`，奇数单骰分 `-1`；不附带牌型奖励。
- 关卡目标按四家族、中立与双阵营的长期成长重新设定，完整 40 关曲线见 `DICE_ARCHETYPES.md` 与 `chapter_score_table.csv`。
- 组合核心来自饲料、吞吃、吸附、钱包流、槽位相邻、市场事件、四家族身份与双阵营桥接。
- 体验目标仍是 5 分钟理解基础、15 分钟形成路线、25 分钟出现一次可读的大幅成长或连锁。

## Economy Notes

- Current gold economy is configured in `Assets/Resources/Data/global.csv`.
- New runs currently start with `starting_gold = 8` as the F013 formal baseline; `1000` gold is no longer the active balance configuration.
- F013 后小关通关会发放 `stage_clear_base_gold = 3` 的固定金币；V0.2 首版只有一次出手且无出千，剩余出手和剩余出千金币奖励保持 `0`。
- 当前骰子产生的金币事件按骰袋槽位从左到右直接进入钱包；较早事件可以被后续槽位读取，后发生的事件不回溯修改已结算槽位。
- Current interest rule is easy to remember: every 6 gold held after本次出手收入和通关固定金币 gives +1 interest, capped at +4 per stage.
- 金钱龟、吞噬骰子、刷新海盗返利、劫掠海盗、财宝海盗、接济等当前金币来源都发布结构化入账事件；财宝海盗按事件次数响应，不按单次金币数量响应。
- 通关收入顺序为当前出手内金币、固定通关金币、基础利息；利息规则为每持有 6 金获得 1 金，每关最多 4 金。

## UX Notes

- The first playable screen should be the actual game experience, not a marketing page.
- UI should feel like a compact tabletop tool: restrained, readable, dense enough for repeated decisions.
- After main run UI changes, follow `Docs/UI_ACCEPTANCE_FEEDBACK_WORKFLOW.md` before calling the UI accepted. Static checks alone are not enough for UI acceptance.
- Formal UI design targets `1920x1080` at `16:9`, with a minimum practical window target of `1280x720`.
- Player windows may use custom resolutions; the core UI should preserve a 16:9 safe area and scale uniformly so controls do not overlap.
- Windows 包默认窗口还原尺寸为 `1280x720`，并允许系统原生调整尺寸。全屏窗口始终跟随桌面分辨率；独占全屏只使用运行时枚举出的受支持分辨率。切换显示模式或分辨率后必须在十秒内保留，否则自动恢复进入设置前的显示状态。
- The current OnGUI prototype keeps old `1280x720` authored coordinates and maps them into a `1920x1080` fit canvas with a `1.5x` prototype-to-design transform.
- Avoid in-game tutorial walls. Short actionable labels are better than explaining every system in paragraphs.
- Keep the current demo no-asset and self-contained unless a future task explicitly adds production UI/art.
- The start-of-run build selection screen has been removed. New game enters a single default starter dice bag.
- Confirmed target roll interaction follows `GAME_FLOW.md`: rolling is not triggered by a clickable button. The player presses `Space` once to start slot-fixed dice spinning; the force-input window remains configurable but its first-version target is `0`, so no repeated input is required or prompted. After the base spin, the dice stop from left to right, lock the real results, and automatically enter settlement without a second `Space`. Any future nonzero force window may affect presentation only and must not affect die-face probability, score, or settlement.
- 投骰表现参数必须配置化并支持调试期运行时重载；当前投骰使用开始时的配置快照，重载后的新配置从下一次投骰生效，且不得影响骰面概率、分数或结算。当前代码和 `roll_feedback_config.csv` 已把首版加力窗口设为 `0`，结果锁定后自动进入结算，不再保留第二次主动结算；未来若把窗口配置为非零，也只能改变表现。
- 当前 F004 / F009 摇骰已切换为 V4R6 固定骰体·骰面纵向滚轴：六个 `128×128` 槽位复用 F021 正方体家族壳，外壳、投影、坐标、尺寸和类型芯不动，只在正面裁切窗内播放与类型 / 属性 / 最终结果无关的通用 `1–6` 纵向面值带。默认 `base_spin_duration=0.56`、`input_window_duration=0`、`stop_duration=0.66`；真实结果在左到右停靠段从上缘接入，执行 `7 px` 下冲与 `2.5 px` 回勾，稳定约 `0.20s` 后自动调用既有结算，不显示第二次操作。旧骰盅、统一旋转 strip 和旧提示脉冲只保留非 F021 回退或历史资源。
- 投骰前的 `Ready` 阶段允许玩家通过拖拽插入调整骰子槽位；结果锁定后，投掷区保持当前骰袋槽位顺序，不按有效点数重排。V0.2 首版没有牌型表入口和出千选择，玩家按 `Space` 直接确认本次唯一结算。
- F007-03 静态主流程 UI 精简已接入 `Assets/Scripts/DiceKingDemo.cs`：保留 `骰子王` 标题和关卡信息；左侧上方显示积分塔，积分塔内目标分在上、当前分在下，下方显示基础分数字与 `直计` 章；金币、出手和直计状态集中到左侧下方资源牌；删除主流程常驻的当前骰袋、`6 / 6 全部上场`、构成、`基础 x6` 和本关关注；中央投掷区整体放大，静态层只保留骰子行、选中/结算高亮和连续留白；右侧只保留本关规则与单次出手提示；底部 `Space` 提示短而居中。该实现不新增运行时 UI 贴图，仍需 Unity Play Mode 和截图验收。
- F021 主游戏功能分区已通过 V3 评审：顶部只常驻章节 / 小关、剩余生命和金币；左侧目标 / 当前积分；中央六颗可排序实体骰；右侧单条规则；底部阶段化 `Space`。单次出手仍是玩法规则，但 `1/1` 没有决策价值，不再常驻显示。主游戏不绘制悬空结算线、六节点状态轨或骰槽结算灯；左到右结算只由当前骰和底座抬升、局部反馈、回落及下一骰抬升表达。V3 的连续托盘、来源骰影、邻骰滑动让位和柔和预落位交互已获用户认可，但画面风格仍需继续贴近开始界面的老街机电子语言，尚未接入代码。
- 2026-07-22 用户已确认 F009 V6B-R4 玻璃直吸收母版与 V6B-R5 隐藏五档，并授权接入 Unity。当前 `DiceKingDemo.cs` 已实现带符号贡献记录、临时贡献折入、连续单击重量、单枚积分光晕直触玻璃、积分塔承击 / 隐藏五档与程序音效；六骰全程原槽，左到右主游标仍先清空当前槽局部连锁队列。首轮实机反馈指出光效成立但来源和触发语义不清，现已在真实来源骰原槽补入非数值短标签，右侧终端同步当前机制；回环时来源骰显示 `再触发 / 响应`，当前处理槽保留低强度 `主槽` 上下文。不得出现常驻入口、骰子互联、共享灯路、长轨迹、实体骰飞行、逐骰大数值、百分比或档位名。隔离 Unity 2019.4.33 编译及 `1280×720 / 1920×1080` 的触发、再触发、跨槽响应截帧均通过，最终连续整手手感仍待用户在当前工程实机回归；本次未改变随机、真实计分、金币、成长或存档。
- F021 主游戏后续只做同布局换肤：使用旧街机的冲压喷漆钢板、分层圆角压边、老化黑色塑料、烟熏亚克力 / CRT 玻璃、琥珀点阵与少量青色磷光、通风栅和高接触边缘掉漆；避免 V2 的外露机械 / 蒸汽工业，也避免 V3 过于光滑、像现代汽车中控或新式外设。开始界面的窗雨、吊灯和完整机壳仍不进入独立主游戏场景。
- `Ready` 排序教学目标采用渐进提示：首次短文案、空闲一次性手势演示、悬停可抓取形变和拖动插入让位；任意输入终止演示，成功拖动后收起首次提示。演示不改变实际顺序、不写存档玩法状态、也不消耗玩法随机。
- F007-04 双数字计分器状态契约已接入 `Assets/Scripts/DiceKingDemo.cs`：左端积分塔统一消费 `RunScoreCounterState`，区分 Idle / Preview / Settling / Settled 阶段；结果决策阶段显示预览累计，结算阶段使用 `BeginSettle()` 在真实 `ScoreDice()` 后冻结的基础分、直计状态、本次总分和结算后总分。V0.2 已改为单次出手、无出千、无全局牌型倍率；当前运行存档版本为 `SaveVersion = 12`。
- F007-05 左到右结算跳涨演出已接入 `Assets/Scripts/DiceKingDemo.cs`：结算开始后根据 F007-04 提交态生成展示快照，基础分数字随当前槽位顺序逐颗累加，当前积分数字按直计分推进。该实现仍需 Unity Play Mode 和截图验收。
- F007-05 修正了预览阶段提前展示结算结果的问题：结果决策阶段，左端当前积分只显示已真实入账的小关累计分，不显示 `currentScore + previewRollScore`；基础分数字保持 `0`，直计章稳定显示，玩家确认结算后才通过逐步动画推进到最终分。
- F006-02 / F006-03 结算演出首轮已接入 `Assets/Scripts/DiceKingDemo.cs`：`BeginSettle()` 在唯一一次真实 `ScoreDice()` 完成后生成只读展示事件队列，`UpdateScoreReveal()` 直接按逐槽入账、家族效果短反馈和目标收束播放，不再插入单独的结果定格停顿；金币飞字、长期成长和达标 / 未达标横幅均只读已提交状态。当前未实现演出中按键跳过；设置页统一演出倍率只改变表现时间，不改真实计分、随机、数据表、存档或骰子效果边界，仍需 Unity Play Mode 和截图验收。
- 2026-07-23 F006 / F009 V7-R3 五档终局已沿既有 `TargetSettle` 队列接入：五档收束时长为 `1.70s / 1.55s / 1.65s / 1.85s / 2.20s`；最高档独占锁死、吸光、近黑真空、单次击穿和余震，并延迟结果文案。程序生成的蓄压 / 击穿 / 余震音色服从统一 `1.0x / 1.5x / 2.0x` 演出倍率；降闪设置缩减白闪、CRT、射线和整机后坐。Unity 2019.4.33f1 编译、五档真实 `TargetSettle` 分类 / 时长校验及 `1920×1080 / 1280×720 / 0.606×` 视觉证据通过；用户对完整整手、实际声音与连续耐受的实机回归仍待完成。本轮没有改 `ScoreDice()`、真实得分、奖励、随机、数据 CSV 或存档。
- 2026-07-23 F006-V7R3-FB-001 已收口普通通关跨阶段叠光：`TargetSettle` 激活时统一清除上一笔贡献的达标十字与积分塔扫描余辉；Pass 只在积分塔目标线附近保留一次暖色硬边机械接触，不再绘制目标图标青白漫射圆光或全高亮柱；完成后最多保留 `0.18s` 边框回声，不再保留 `0.52s` 塔内填充。提前达标、最后一枚刚好达标、贿赂补足、五档双分辨率、三档演出速度和降效模式均完成隔离 Unity 自动回归。该修复只改表现状态边界，未改五档阈值、Pass `1.55s`、`0.56` 揭示点、音效身份、计分、奖励、事件顺序、数据或存档；用户实机体验仍待回归。
- 2026-07-23 F006-V7R3-FB-002 已把积分塔结算中的贯穿实心长柱改为与既有 12 格灯体对齐的分段能量：每格保留断口并使用外晕、琥珀主体、温白热芯和移动继电头；BribeFinal、Exceed、FarExceed、Critical 蓄压 / 击穿、旧回退与非 Pass 残留统一遵守该合同。最高档贯屏直线色条同时改为分叉放电和多股冲击热丝，Pass 继续只使用目标线短接点。五档双分辨率、`0.606×`、真实 BribeFinal 与普通 Pass 三路径完成隔离 Unity 自动回归；未改档位时长、相位、声音、计分、阈值、奖励、数据或存档，用户实机体验仍待回归。
- 2026-07-23 F006-V7R3-FB-003 已增强普通逐次结算的积分塔边沿电流可读性：`SlotScore / BribeFinal` 入账后的约 `0.46s` 余辉保留低强度整框路径底色，并在左右边沿增加暖色外晕、主体、温白热核和四段短尾迹；正贡献自下向上，负贡献反向向下，左右轻微错相，端点只形成一次短接帽。真实 `SlotScore` 双分辨率早 / 中 / 晚三相位、五档终局、Critical 三相位、提前达标、最后一枚达标、BribeFinal、三档演出速度与降效模式均完成隔离 Unity 自动回归。该修复没有延长余辉、恢复实心长柱、扩大为整机反馈，也未改事件、计分、阈值、奖励、声音、数据或存档；用户实机体验仍待回归。
- 2026-07-23 F006-V7R3-FB-004 已把运行时第 2 / 3 / 4 档从共用琥珀机柜负载重构为三种主轮廓：Pass 仅在积分塔目标线做一次局部机械单扣；Exceed 使用上轨左至右去程、下轨右至左回程的空间分离双继电；FarExceed 让 12 格塔灯、骰盘、终端和机柜外框持续持压，并在峰值后局部回坐 / 卸压。Critical 原有吸光、近黑真空、全屏白金击穿、射线、碎片和余震保持独占。Unity 2019.4.33f1 隔离工程完成 13 相位、`1920×1080 / 1280×720` 共 26 张真实 `TargetSettle` 捕获，以及 `0.606×` 和降低特效接触板核对；程序与自动视觉门禁通过，正常速连续动态和声音仍待用户实机回归。本次未改五档阈值、终局时长、结果揭示点、声音 cue、真实计分、奖励、事件、数据或存档。
- 2026-07-23 F006-V7R3-FB-005 已只重构最高暴击的受力反差：旧对称击穿脉冲会在配置爆点前提前绘制射线、扩张与继电，现改为爆点前严格为零的单向释放；机柜内容先向下沉并纵向压缩，在积分塔小热核处硬锁，再从塔芯向上顶穿、上冲过冲、反向回落后进入余震。主射线和碎片限制在向上扇区，横向冲击缩短为次级脉冲；击穿音减少长低频倾泻并增加上扬瞬态。隔离 Unity 2019.4.33f1 已完成 18 相位、`1920×1080 / 1280×720` 共 36 张真实 `TargetSettle` 捕获，以及正常 / 降效位移方向数值校验；第 1–4 档世界变换保持为零。该修复保持 `2.20s`、真空 / 击穿 / 余震配置点、五档阈值、计分、奖励、事件、数据和存档不变，用户对正常速“火山压制后爆发”爽感仍需实机回归。
- 游戏内玩法表现统一使用设置页的 `1.0x / 1.5x / 2.0x` 三档“演出速度”：覆盖骰子入场、投骰、停转与结果揭晓，左到右结算、短标签、金币反馈和目标收束，通关收入、失败收束，以及离开市场效果队列。为兼容已有设置，倍率继续保存为 `PlayerPrefs` 的 `SettlementPlaybackSpeed`；主菜单雨水与吊灯、悬浮与按键呼吸、设置转场和显示设置十秒确认不受影响。倍率只压缩玩法表现持续时间，不改变真实计分、钱包、成长、随机、事件顺序或运行存档版本。
- F009 序列帧式固定槽位旋转首版已接入 `Assets/Scripts/DiceKingDemo.cs`：小关开始和每次未达标回到 `Ready` 时，六骰按槽位优先显示统一待机骰，非基础骰叠加小类型标记；按 `Space` 后每颗骰在原槽位播放统一旋转 strip，运动期隐藏可读点数；停转阶段从左到右逐颗显真实点数。程序按当前四家族、中立与双阵营身份提供视觉差异；资源缺失时回退到程序离散帧。
- 2026-07-21 上述 F009 旧旋转 strip 路径在 F021 主流程中已被 V4R6 覆盖：`DrawDiceProcessToken()` 对 `Shaking / Stopping` 直接使用固定正方体家族壳与面内滚轴，不再应用整骰矩阵旋转、缩放、位移或类型差异化速度。源区批准图为 `Assets/ArtSource/Production/F009/FaceReelStopPreviewV4R6/f009_stationary_cube_face_reel_contact_v4r6_v1_20260721.png`，运行不加载该图、不新增骰子位图；双分辨率证据和自动验收记录位于 `Docs/QA/20260721_f009_face_reel_runtime.md`。Unity 2019.4.33 隔离批处理编译与运行捕获通过；随机、计分、金币、效果、`PlayerPrefs` 和 `CurrentSaveVersion` 未改变。

## Art Direction Notes

- Use the project-specific Codex skill `$wabish-art-assets` for future generated art assets. A normal request can be just an asset type plus keywords.
- For uncertain directions, new visual families, batch art, replacement art, or assets intended for `Assets/Resources/Art/`, use the art skill's early direction gate first: present compact direction cards or one representative pilot/contact sheet, get user approval, then continue batch generation and runtime placement.
- 2026-07-14 开始界面专项方向已确认：F021 使用午夜旧机厅单机台，固定采用 A「CRT 存档槽」结构，并以 A 结构细化稿 C「首次启动」的留白、暖琥珀主卡、禁用继续游戏和单一确认键作为氛围母版。该方向替代 `MainMenu / Settings` 的王室账本包装，但不自动推翻尚未重做的主流程、结算和市场运行时美术。
- 2026-07-15 用户确认 F021 的开始界面到主游戏空间关系需要改为“雨夜机厅入口 → 穿屏启动过渡 → 独立骰王竞技台”：CRT 只作为入口和全屏遮罩，不再作为主游戏容器；主游戏按完整 `16:9` 画布重新构图，不受开始界面的机台、窗户、吊灯、CRT 比例或镜头轴线限制。连续性由深海军蓝、暖琥珀、低饱和青绿、点阵、六节点和继电器声音承担。该决定已确认空间与转场原则，但不等于两张 V1 评审图已经通过，也不自动批准运行时拆件或替换现有主流程资源。
- 开始界面氛围原则为“雨一直在，机器持续老化”：右侧窗雨和流滴承担持续空间动态；当前吊灯供电波动使用短间隔随机循环。2026-07-15 用户进一步确认 `DICE KING` 顶部灯牌需要增加电玩 / 霓虹招牌式闪光，具体灯语仍在版本评审；标题字形与可读性、菜单文字和当前焦点必须稳定，招牌峰值须与吊灯及未来 CRT 轻失焦 / 水平同步异常互斥。不得把故障做成机械固定节拍、恐怖惊吓、整屏黑白闪或影响输入的状态。
- 2026-07-15 右窗雨水表现进一步确认必须拆成两个独立层：玻璃之后持续、快速、略虚的窗外降雨，以及低频雨滴撞击玻璃、附着合并后缓慢汇流的近景玻璃层。两层不得使用相同速度和锐度；窗框内沿不得保留固定白色水珠链，所有动态遮罩必须向玻璃内部收缩，不能碰到窗框。
- 2026-07-15 右窗雨效参数载体进一步确认：先完成窗外雨幕和玻璃汇流样片验收，再进入 Unity 接入；雨幕密度、速度、远近层次、水滴数量、汇流速度和水流长度必须通过 Unity Inspector 直接查看和修改，优先使用可持久保存的 `ScriptableObject` 参数资源，不扩展 CSV。窗外雨的屏幕运动方向必须明确向下，雨丝轴线、亮头和速度向量保持一致。
- 2026-07-15 右窗效果一 V3 窗外持续降雨已获用户接受并冻结为运行背景基准。效果二 V3 的水珠材质、撞击、附着、停滞和汇流语言获条件接受，但固定四滴和固定路径不满足终版；用户随后确认直接进入 Unity 程序化实现，改为 `8–14` 颗可见水珠、`2–3` 个随机松散区域、邻近吸附、阈值触流和最多 `1–2` 条主流。
- 2026-07-15 右窗雨水正式运行切片已接入 `DiceKingDemo.cs`：`Art/MainMenu/arcade_main_menu_window_clean_patch` 覆盖旧底图中的静态雨痕与窗框白点，窗外远 / 近雨、玻璃撞击水滴、接触合并、主流、低概率支流、停滞、沿途接触吸收和淡出均由对象池式表现状态生成。窗外雨只沿屏幕坐标正 Y 向下移动；雨线旋转在缩放画布的局部坐标内完成，所有动态限制在右窗内缩多边形内，只在 `MainMenu` 使用 `Time.unscaledDeltaTime` 更新。
- 2026-07-15 人工验收 R1 后，玻璃水珠取消远距离、横向或向上的磁吸移动：普通水滴先附着，再按质量受重力向下滑；只有可见边缘真实接触才合并，并保留下方水珠；主流只捕获已流经路径上真实接触的下游水滴。用户随后确认 R1 人工回归基本无问题，右窗雨效切片通过。雨水参数不进入 `main_menu_visual_config.csv`。正式参数资产为 `Assets/Resources/Config/main_menu_rain_profile.asset`，加载 key 为 `Config/main_menu_rain_profile`；Inspector 直接提供雨幕密度 / 总速度、远近层数量 / 速度 / 长度 / 宽度 / 透明度、撞击频率、可见水滴范围、区域数、生成范围、邻滴撞击概率、附着时间、普通下滑速度、接触容差、合并重滴下滑速度 / 阈值、水流速度 / 长度 / 宽度 / 数量、支流 / 停滞概率和验收随机种子。`LockPreviewSeed` 默认开启，Play Mode 中 `F8` 可按当前资产重新开始同一验收序列；独立 `System.Random` 不消耗玩法随机。
- 2026-07-14 开始界面首个正式运行时切片已接入：`DiceKingDemo.cs` 通过 `Art/MainMenu/arcade_main_menu_lamp_on`、`arcade_main_menu_lamp_off`、`arcade_main_menu_lamp_glow` 和 `arcade_main_menu_lamp_core` 四层资源替换旧王室账本首屏，并保留旧首屏作为资源缺失兜底。当前完整机台图仍含首次启动基准文案，代码会为有存档状态、焦点和覆盖确认叠加动态内容；若后续需要本地化或全面改文案，仍应补无文字 clean plate。
- 吊灯表现定义为左上局部线路瞬断，不改变机台 CRT、`DICE KING` 顶箱、菜单输入或玩法随机。时序与间隔统一由 `Assets/Resources/Data/main_menu_visual_config.csv` 控制：`loop_enabled` 默认开启，首次等待 `1.00s`；复亮完成后随机等待 `0.70–1.30s`，以 `0.035s` 熄灭、`0.10s` 停黑、灯芯 / 近场 / 环境按 `0.035s / 0.07s / 0.14s` 恢复，完整起始到起始周期约 `0.98–1.58s`。视觉随机使用独立 `System.Random`，不得消耗骰子玩法的随机序列。设置页提供“开始界面吊灯屏闪”开关，菜单输入、覆盖确认、设置页和离开主菜单时必须暂停并复位该事件。
- Stable screen-level art direction for future main-flow, settlement, market, and menu presentation: `Pixel Ledger Scoring Machine` / `像素账本计分机风`. It keeps the bright royal-ledger tabletop tone of `Bright Ledger Boardgame` / `明亮账本桌游风`, but shifts full-screen presentation to polished 2D high-resolution pixel art for a `1920x1080`, `16:9` landscape canvas.
- `Bright Ledger Boardgame` / `明亮账本桌游风` remains the compatibility base for existing dice icons, parchment UI textures, royal-bureaucracy props, and current generated runtime assets. New screen-level concept art, UI replacement art, and animation targets should default to `Pixel Ledger Scoring Machine` unless a task explicitly says otherwise.
- Core screen composition: a royal ledger scoring machine embedded in a tabletop board, with a left-side score tower/gauge/pointer and lamps, a central six-slot dice tray, a right-side rule card, a bottom short `Space` action bar, coin-payout mouth/resource plate, and a separate inter-stage market with three offer shelves plus the six-slot dice bag.
- Screen-level motion should be driven by game state changes, not simple deformation: `Ready` is calm and readable; `ResultDecision` shows locked results without committing score; `Scoring` routes points left-to-right from the active slot into the score tower; target crossing uses pointer shake, bulbs, and burst feedback; `StageClear` / payout is the only phase that spits coins for fixed gold and interest; `InterStageMarket` unfolds shelves and market feedback separately from settlement.
- Avoid concept art that implies unavailable mechanics: no slot-machine reels, pull levers, roulette, poker-table layouts, fake jackpot labels, casino black-red-gold glam, vertical mobile composition, extra roll buttons, or cheating controls in the V0.2 default flow.
- Preferred motifs: dice, ledgers, wax seals, official stamps, small crowns, coin pouches, ribbons, stamped forms, desk props, and tidy tabletop game pieces.
- Tone should be bright, cute, witty, and relaxed, but not childish. Avoid chibi mascots, baby-face expressions, sticker-sheet looks, plush-toy rendering, casino glam, photorealistic 3D, neon sci-fi, and preschool styling.
- Production assets should favor chunky readable silhouettes, light clear colors, controlled dark outlines, minimal texture, visible material cues, and minimal/no generated text so Unity UI can own final copy.
- 骰子类型图标必须把机制符号融入骰子本体。优先使用骰面标记、骰边切角、骰角封章、绑带、嵌入标记、刻槽、点阵变形或表面镶嵌；避免把骰子放在大型外部道具、箭头、轨道、台阶、空位或说明性背景元素旁边，形成“骰子 + 场景说明”的构图。
- 运行时骰子在待机与市场阶段优先表达家族壳和具体类型芯；结果锁定后中心只显示最终有效点数，饲料、吞吃、吸附与其它个体状态进入悬浮详情或结算短反馈。基础骰使用更安静的中性色，双阵营骰拼接两个家族壳段但只显示一个类型芯。
- 2026-07-17 用户批准 F021 类型芯方向 A：家族壳继续只表达家族，中央使用嵌入骰面的粗轮廓机械类型芯表达具体骰子；静态剪影必须独立成立，低频动作只作记忆强化。首批范围冻结为基础骰、四家族、中立与专用贡品共 `37` 枚，双家族 `9` 枚另立后续批次。常态身份动作间隔 `3.5–5.5s`、同屏最多 `2` 枚，任意输入、拖拽、投骰、停转、结算和结果阶段暂停；结果锁定后复用同一类型芯缩入数字下方并停止活动。已批准压力图与完整生产规格位于 `Assets/ArtSource/Production/ArcadeDiceKing/DiceTypeCoresV1/`，正式透明资源和运行时映射尚未入库。
- F014 骰子悬浮窗的新视觉与信息架构已于 2026-07-17 获用户接受，并于 2026-07-23 完成新一轮静态接入：市场待购骰、市场骰袋和主投掷区实体骰共用横向“旧街机检修终端”，头部承载类型芯、名称、家族和必要触发；龟壳类不再重复显示“化壳时”。“实际六面”以当前 `die.Faces` 为基础逐面折算喂养最低点和幸运龟最大面，不纳入条件计分、邻位、钱包、遭遇、本次随机结果或单骰贡献分，也不写回玩法状态。六面最大绝对值 `<10,000 / 10,000–99,999 / >=100,000` 时分别使用 `1×6` 完整整数、`3×2` 千分位整数、`3×2` 三位有效数字科学计数与中文快速读取；唯一最高才显示提示，完整整数继续用于比较、结算和存档。规则正文通过 `{{kw:key}}` 引用词典显示名；`keyword_glossary_config.csv` 的 `show_explanation` 控制可见性、`accent_family` 控制家族色，当前仅 `feed / devour / shell` 生成“喂养 / 吞噬 / 化壳”彩色标签与解释，正文同词同色并加下划线；`market_floor / family / dual_family` 只替换正文或表达身份，不高亮、不解释。底部无通用“术语说明”标题，每词一个标签、解释另起一行；面板宽约 `448`，按内容选择 `196 / 244 / 288 / 352 / 416 / 480 / 568` 高度档。既有 hover 节奏和阶段抑制继续保留；Unity 2019.4 Roslyn 独立编译、配表覆盖、阈值反射测试和 LED 字库生成已通过，仍待 Play Mode `1280x720 / 1920x1080` 截图与交互验收。
- Runtime UI presentation uses scalable parchment panels/buttons drawn procedurally in OnGUI, with generated `Assets/Resources/Art/UI/` assets kept as style references, replacement candidates, and fixed-size UI icons. Do not stretch full decorative UI textures into arbitrary button or panel rectangles.
- The 2026-07-06 approved `Pixel Ledger Scoring Machine` flow references live under `Assets/ArtSource/Production/FlowContactSheets/` and are style / composition references only. They are not runtime resources until a later task explicitly slices assets into `Assets/Resources/Art/` and registers their load keys in `ART_ASSETS.md`.
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
- V0.2 新增或重做结算效果时，先检查 F015-R01 规则卡，明确主游标、局部连锁队列、响应排序和预览 / 真实结算边界。
- 实现生命池时必须把“提交本手分数与效果、未达标扣生命、写入稳定续投检查点、扣后归零清档”作为一个原子流程处理。续投保留本关累计分、已入钱包金币及全部已提交长期成长，只清理每手临时状态；不得恢复小关开始快照，也不得让退出重进绕过生命扣除或重复提交同一效果。
- Keep `PROJECT_CONTEXT.md`, `GAME_FLOW.md`, and `DICE_ARCHETYPES.md` synchronized whenever current code behavior changes.
- 主流程 LED 文字的正式运行合同由 `main_game_led_text_roles.csv` 与 `main_game_led_text_brightness_groups.csv` 统一管理；页面不得重新复制颜色或通过低 Alpha 改写同级权重。L2 / L3 只允许 Display / Compact，浅色市场实体键帽继续使用批准的原纹理实心字例外。
- UI 文字验收同时保留原生 `1280×720 / 1920×1080` 与真实反馈暴露的 `0.606×` 非整数压力。原生图判断锐度、基线与裁切；缩放图只判断信息层级和识别生存，不能互相替代。
