# F022 程序交接

状态：已提案，需用户决策
功能：交易托盘、酒馆法术与资源循环
实现事实来源：`Assets/Scripts/DiceKingDemo.cs`
当前存档版本：`12`
建议目标存档版本：`13`

## 交接结论

F022 不是在现有 `BuyOffer()` 后补一个临时列表。它要求先把市场重构成可回滚的领域动作，再接托盘、法术和供给导演。若继续把副作用散落在按钮函数中，满架替换、清仓、黑帆刷新、换班恢复和来源递归都无法可靠验收。

程序实现顺序必须是：

1. 事件、来源、实例与事务骨架。
2. 两格托盘和采购 / 首次装入分离。
3. 法术货位、法术架与基础五张法术。
4. 资源合同、两颗主动骰与批量命令。
5. 中高阶法术、供给导演与连续状态记录。
6. 正式 UI、美术、存档恢复和行为门。

## 当前实现事实

当前 `DiceKingDemo.cs`：

- `MarketOfferKind` 只有 `Empty / Die / CraftingItem`。
- `MarketActionKind` 只有 `BuyDie / SellDie`。
- `marketOffers` 只有三格，没有独立法术位、交易托盘和法术架。
- `BuyOffer()` 在一次调用中扣款、克隆商品、直接加入 `dice`、触发买入效果并清空货架。
- `CanBuyMarketOffer()` 用骰袋容量阻止普通购买，满袋只为旧血契逻辑开例外。
- 贡品购买后自动吞给系统选择的最低点目标。
- 抢劫海盗在离店队列中随机获得货架骰并直接加入骰袋。
- 黑帆和魔蝠自动按第一个可用货架吞吃。
- 人口海盗直接把招募骰加入骰袋，未记录来源递归。
- 贸易猪出售后随机选择饲料继承者。
- `TurtleAttachment` 只有类型、基础转移、卖价和来源，没有稳定壳实例 ID 或内部已用状态。
- `SaveRun()` 只保存骰袋、金币、关卡、生命和少量全局状态，不保存市场货架、托盘、法术、刷新计数、导演记忆、随机状态或未完成事务。
- `HasCurrentRunSaveVersion()` 会清除低于当前版本的运行存档。

F022 必须显式替换这些路径，不能让旧自动逻辑与新动作同时触发。

## 建议领域模型

命名可按项目代码风格调整，但语义不得压回一个按钮函数。

### 市场商品

```text
MarketDieOffer
  OfferId
  DieInstance
  LockedPrice
  GeneratedChapter
  GeneratedTier
  IsHighTierGuarantee
  DiscountTag
  Source
  FunctionBucket

MarketSpellOffer
  OfferId
  SpellDefinitionId
  LockedPrice
  Source
  FunctionBucket
```

骰子三格和法术一格使用独立字段；不要把法术混入 `marketOffers` 后再用同一权重抽取。

### 交易托盘

```text
MarketTraySlot[2]
  SlotIndex
  DieInstance
  EntryKind = Purchase | Generated | Recovery | Stolen
  SourceActionId
  MayFirstInstall
  MustClearBeforeLeave
```

- `Purchase`：支付货架价格取得。
- `Generated`：人口招募等明确生成。
- `Recovery`：换班凭证等明确回收。
- `Stolen`：抢劫海盗主动移动货架。
- 来源不因归位、组装或 UI 移动丢失。

### 骰子实例补充字段

```text
Die
  PersistentInstanceId
  OriginKind
  FirstInstallCommitted
  ActiveUseMarketId
  FeedLedger
  TurtleAttachments
```

当前运行 `Id` 可以继续做会话内定位，但事务和存档应使用稳定实例 ID。`FeedLedger` 至少保留饲料次数、累计最低点价值和每次已经结算的质量；只存 `FeedCount / FeedValue` 无法证明转栏没有追溯重算。

### 真实龟壳

```text
TurtleAttachment
  AttachmentId
  Type
  FaceGain
  SellValue
  Source
  InternalState
  UsedThisStageOrSettle
```

- 真实壳、基础壳、首领复制壳必须可区分。
- 外壳转运移动同一个 `AttachmentId`，不能删除后新建一个满状态壳。
- 已用状态随壳移动，不能因换宿主重置。

### 法术

```text
SpellDefinition
  Id
  DisplayName
  BasePrice
  Tier
  PrimaryBucket
  SecondaryBucket
  EarlyWeight
  MidWeight
  LateWeight
  BossMultiplier

SpellRackSlot[3]
  SlotIndex
  SpellInstanceId
  DefinitionId
  AcquiredMarketId
  AcquiredActionId
```

法术实例本身无需随机面组，但应有实例 ID，以支持替换、保存窗口和恢复日志。

### 供给记忆

```text
MarketDirectorMemory
  RecentSpellIds
  RecentSpellPrimaryBuckets
  RejectedSpellIds
  MissingFunctionCounters
  BridgeGap
  LastInterventionReason
```

自然生成结果、法术纠偏和骰位纠偏必须分开记录。

## 事件语法

建议把当前 `MarketActionKind` 替换或扩展为明确事件：

| 事件 | 何时发布 | 不应误触发 |
|---|---|---|
| `DiePurchased` | 支付货架价格并进入托盘 | 生成、抢劫、回收 |
| `DieFirstInstalled` | 从托盘首次进入骰袋 | 回收归位、直接化壳 |
| `DieReturned` | 特殊回收骰归位 | 首次装入 |
| `DieSold` | 玩家主动出售资产 | 清算、吞吃、丢弃 |
| `AssetLiquidated` | 加急清算 | 出售响应、人口计数 |
| `PaidRefreshRequested` | 玩家确认付费整刷 | 单格换货、定向招募 |
| `ShelfGenerated` | 任意货架生成 | 不自动等于付费刷新 |
| `SpellPurchased` | 法术进入法术架 | 施放、替换掉的旧牌 |
| `SpellCast` | 法术提交并消耗 | 购买 |
| `FeedGranted` | 每次真实喂养 | 转栏迁移 |
| `DevourResolved` | 一次真实吞吃完成 | 清算、化壳、丢弃 |
| `RealShellAttached` | 一次真实化壳 | 复制壳、移壳 |
| `ShellMoved` | 外壳转运 | 真实化壳、首次装入 |
| `GoldIncome` | 一笔真实金币入账 | 支付、免费零售 |
| `DieRecovered` | 骰袋旧骰进入托盘 | 采购 |
| `MarketLeaveCommitted` | 离店目标和清盘通过后 | 离开按钮第一次点击 |

所有子事件携带：

- `RootActionId`
- `SourceKind`
- `SourceInstanceId`
- `MarketId`
- `SequenceIndex`
- `IsPreview`

预览永远不能写钱包、骰面、饲料、壳、货架、随机游标或存档。

## 原子事务

### 状态

```text
None -> Prepared -> Committed
                \-> Cancelled
Prepared/Committed + reload -> RecoverToBefore | RecoverToAfter
```

### `Prepared`

提交前冻结：

- 根动作 ID 和类型。
- 完整前态摘要或可恢复快照。
- 所有涉及的实例 ID、货架 ID、托盘格、法术格和目标。
- 锁定价格、额外支付、处理顺序。
- 随机种子 / 随机游标。
- 预期后态校验摘要。

### `Committed`

- 不再接受玩家输入。
- 按冻结顺序执行。
- 每个子事件可记录展示，但不得再次决定随机或目标。
- 完成后保存完整后态，再清除事务日志。

### 恢复

- 读取到 `Prepared` 且没有完整后态：恢复完整前态。
- 读取到 `Committed` 且完整后态存在：恢复完整后态。
- 不允许继续半条队列。
- 固定种子只允许 before / after 两种结果，不得重复扣款、吞吃、分账、首次装入、招募或壳复制。

首轮必须使用该事务的动作：

- 法术架满购买替换。
- 清仓祭典。
- 换班凭证。
- 黑帆付费刷新。
- 魔蝠离店。
- 外壳转运。
- 贸易猪转栏目标出售。
- 托盘直接化壳。

## 核心动作顺序

### 采购骰子

```text
Validate gold + empty tray
Prepare(price, offer, tray slot)
Deduct gold
Move exact offer instance to tray
Empty shelf
Publish DiePurchased
Resolve purchase responses
Commit + save
```

- 不检查骰袋是否已满。
- 差异化显示价格沿用商品生成时锁价。
- 采购后恶魔骰子可以抬高仍在货架的商品；已经进托盘的商品不追溯。
- 补给猪写在商品上的饲料与其它个体状态完整进入托盘。

### 首次装入

```text
Validate tray instance + empty bag slot
Prepare(instance, target slot)
Move to bag slot
If !FirstInstallCommitted:
  set true
  publish DieFirstInstalled
else:
  publish DieReturned
Resolve
Commit + save
```

普通装入不能原子挤出旧骰。满袋换代应先通过出售 / 消耗产生空槽；“换班凭证”是特例。

### 主动出售

- 一项组合宿主只发布一次 `DieSold` 和一笔 `GoldIncome`。
- 真实龟壳回收值并入组合资产卖价。
- 复制壳没有卖价。
- `CrewRecruit` 来源骰仍发布普通出售与金币，但不推进任何人口海盗计数。
- 贡品 `0` 金出售发布 `DieSold`，不发布 `GoldIncome`。

### 付费刷新

```text
Compute and lock current net price
Prepare optional BatchMarketCommand for BlackSail
On confirm:
  apply locked payment/refund
  resolve assigned old-shelf devours in bag order
  clear remaining die shelves
  clear/replace spell offer
  increment paid refresh/original price counters
  generate 3 die offers under current rules
  generate 1 spell under independent rules
  apply tavern minimum and SupplyPig exactly once per generated event
  evaluate director
  commit + save
```

- 黑市折扣只作用下一刷。
- 高阶保底只由付费整刷推进。
- 单格换货和定向招募不推进刷新价或高阶保底。
- 当前负刷新价返金继续作为一笔金币事件。

### 离店

```text
Require bag non-empty
Require both tray slots empty
Prepare DemonBat assignments and remaining leave targets
Commit MarketLeave
Lock all market mutation input
Resolve fixed leave queue
Save stable next-state
```

离店提交后不再弹出目标选择，也不生成新托盘物。

## 主动骰候选

### 抢劫海盗

- 骰袋实例，每市场一次。
- 目标：一个非空骰子货架与一个空托盘格。
- 效果：把原商品实例移动到托盘并清空货架。
- 不支付价格，不算采购，不触发恶魔骰子采购响应。
- 保留商品当前面组、饲料、价格和个体状态。
- 不再在离店时随机获骰或递归触发。

### 血契船长

- 骰袋实例，每市场一次。
- 目标：一枚托盘骰和一颗骰袋目标。
- 效果：把托盘骰作为市场来源真实吞吃，随后获得 `1` 金。
- 材料无卖价、不发布出售；吞噬骰子、黑市小鬼、共食等按来源正常响应。
- 取消、目标失效或没有材料不消耗次数。
- 不再提供满袋采购后自动吞吃。

## 资源合同

### 贸易猪

设结构回收值 `R`、饲料次数 `C`、饲料累计最低点价值 `V`：

- 现金兑现：`gold = R + C`，来源和饲料账本销毁。
- 饲料转栏：`gold = R`，把完整 `C / V / 各次质量账本` 移到骰袋或托盘合法目标。
- 转栏不是获得饲料，不发布 `FeedGranted`。
- 两种路线只发布一次主动出售和一笔金币事件。
- 目标在提交前失效则整项取消。
- 无饲料时普通出售，不显示二选一。
- 加急清算只取 `X = R + C`，得到 `X + min(X, 6)`，不打开转栏。

### 饲料进入吞吃

来源长期六面为 `Fi`，长期最小面为 `Fmin`：

```text
feedFloor = Fmin + V
edibleFace[i] = max(Fi, feedFloor)
devourValue = max(1, floor(sum(edibleFace) / 6))
```

不读取：

- 龟壳持续效果。
- 本手随机结果和临时分。
- 邻位、钱包或遭遇条件。

来源被吞吃后长期面组与饲料账本一起销毁。普通龟龟化壳仍只读取长期面组；驮粮龟是饲料进入壳的专属转换器。

### 托盘龟

四类出口：

1. 装入为实体。
2. 直接化壳。
3. 主动出售。
4. 作为明确材料消耗。

直接化壳：

- 目标是骰袋或另一托盘中的普通非龟实体。
- 不允许贡品等一次性专用材料作为宿主。
- 发布一次 `RealShellAttached`，不发布首次装入、第二次采购或出售。
- 先写入来源龟当前长期面组转移，再挂持续壳、真实壳卖价和内部状态。
- 来源实体、家族身份和独立计分消失。
- 每颗首领龟复制一次同类型壳；复制壳无基础转移、无卖价、不递归。

### 驮粮龟

真实化壳完成基础转移后，宿主全部六面额外增加：

```text
floor(source.FeedValue / 2)
```

这是面组写入，不是获得饲料。来源饲料随龟消失。

### 贡品

- 只由深渊召唤等明确效果生成。
- 使用普通面组估值，基础显示价 `max(2, standardValue)`。
- 生成时锁价；之后市场最低点提高不回算。
- 显式折价可以降到 `1`。
- 采购后进入托盘，不能装入。
- 奉献发布市场来源真实吞吃；或 `0` 金主动出售。

### 外壳转运

- 选择一层真实壳和另一合法非龟宿主。
- 移动相同 `AttachmentId`、类型、持续效果、内部状态、回收卖价、来源和使用计数。
- 旧宿主减去、新宿主增加相同回收值，总值守恒。
- 不移动初次化壳已写入的面组、历史永久成长、其它壳、饲料和个体状态。
- 不算真实化壳、采购、装入或出售；不触发首领龟。
- 复制壳与基础壳不可移动。

### 人口海盗

- 每两次符合条件的玩家主动出售生成一颗基础骰到第一个空托盘。
- 托盘满时生成失败，不建立隐藏队列。
- 招募骰 `OriginKind = CrewRecruit`。
- `CrewRecruit` 来源骰出售仍有卖价、普通出售和金币事件，但不推进任何人口海盗计数。
- 正常资产、贡品、采购骰、特殊回收骰和组合宿主出售继续计数。

## 批量命令

```text
BatchMarketCommand
  RootActionId
  CommandKind
  LockedPrice
  SourceInstanceIds
  Assignments
  OrderedOfferIds
  OrderedTargetIds
  RngState
  State
```

### 黑帆刷新

- 每颗黑帆选择一个唯一非空旧货架或跳过。
- 本次净价在面板前锁定。
- 提交后按骰袋顺序吞吃。
- 响应结清后清余货、涨价、生成新货。

### 魔蝠离店

- 每颗魔蝠选择一个唯一非空货架或跳过。
- 托盘清空后才能准备。
- 提交后锁定离店，不再输入。

### 清仓祭典

- 所有非空货架必须各指定一个合法骰袋目标。
- 来源不能跳过，目标可以重复。
- 按货架从左到右真实吞吃。
- 不补货、不算采购或刷新。

## 十二张法术实现合同

| ID | 法术 | 价格 | 实现合同 |
|---|---|---:|---|
| `spell_single_restock` | 单格换货 | `2` | 选择骰货架，丢弃旧货或补空位，按当前章节池生成新骰；不算付费刷新、高阶保底和刷新涨价；丢弃无退出事件 |
| `spell_flawed_discount` | 瑕疵折价 | `2` | 非空且未折价商品锁价 `-4`、最低 `1`；采购后六个长期面各 `-1`；每件最多一标 |
| `spell_feed_bag` | 饲料袋 | `3` | 骰袋 / 托盘目标依次两次当前质量真实喂养；逐次发布 `FeedGranted` |
| `spell_dismantle_permit` | 拆解许可 | `3` | 消耗托盘材料，无卖价；骰袋目标全六面 `+max(1,floor(materialTransfer/2))`；发布带来源的真实吞吃 |
| `spell_rush_liquidation` | 加急清算 | `4` | 清算骰袋资产，获得 `X + min(X,6)`；一笔合并金币事件；不算出售 |
| `spell_shell_transfer` | 外壳转运 | `4` | 移动一个真实壳实例、内部状态和卖价；历史加面不动，不触发真实化壳 |
| `spell_shelf_fertilize` | 货架施肥 | `5` | 本市场最低点 `+2`；立即修改现货长期面组并影响后续生成；锁价不回算；不影响托盘 / 骰袋 |
| `spell_targeted_recruit` | 定向招募 | `5` | 选择五个身份之一和骰货架；从当前章节该身份合法候选按权重生成；含双阵营，不含基础 / 贡品；不保证卡名 |
| `spell_black_market_split` | 黑市分账 | `5` | 到离店前，每次市场来源真实吞吃额外 `+2` 金；多张相加为同一次吞吃的一笔合并金币事件 |
| `spell_coin_feed` | 金币饲料 | `4+` | 先付 `4`，再选额外 `2/4/6`；每额外 `2` 金依次两次真实喂养；最少总价 `6` |
| `spell_clearance_rite` | 清仓祭典 | `8` | 使用批量命令，把全部非空货架按左到右真实吞吃；不补货 |
| `spell_shift_voucher` | 换班凭证 | `6` | 原子交换一枚托盘骰与一枚骰袋骰；新骰首次装入旧槽，旧骰进入同一托盘格并标记回收；不二次采购，旧骰归位不重放 |

### 法术权重

| 法术 | 第 1–2 章 | 第 3–5 章 | 第 6–10 章 |
|---|---:|---:|---:|
| 单格换货 | `18` | `14` | `8` |
| 瑕疵折价 | `18` | `14` | `8` |
| 饲料袋 | `10` | `8` | `5` |
| 拆解许可 | `10` | `12` | `8` |
| 加急清算 | `8` | `10` | `10` |
| 外壳转运 | `0` | `8` | `8` |
| 货架施肥 | `0` | `8` | `8` |
| 定向招募 | `2` | `10` | `8` |
| 黑市分账 | `0` | `8` | `10` |
| 金币饲料 | `0` | `4` | `10` |
| 清仓祭典 | `0` | `0` | `8` |
| 换班凭证 | `0` | `0` | `9` |
| 总权重 | `66` | `96` | `100` |

Boss 市场：

- 基础操作乘 `0.75`。
- 资源桥乘 `1.25`。
- 高阶改道乘 `1.50`。
- 原权重为 `0` 时仍为 `0`。
- 仍只有一个法术货位。

## 法术架

- 固定三格。
- 不提供常驻免费丢弃。
- 法术货位在满架时仍显示并刷新。
- 满架暂停连续空转保护。

满架购买替换：

```text
Prepare(oldRackSlot, newOffer, price, walletAfter)
On commit:
  deduct price
  destroy old spell without gameplay event
  place new spell in same slot
  empty spell offer
  publish one SpellPurchased for new spell
  save complete after-state
```

取消或失败时钱包、旧牌和商店位全部保持前态。

## 软供给导演

### 功能桶

`Immediate / Ignite / Sustain / Convert / Search / CashOut / Pivot / Temptation`

每个骰子与法术必须有一个主桶，可有一个次桶。方向计数只看当前可支付的完整链，不因标签数量虚增。

### 法术可用倍率

- 当前可执行：`1.00`
- 严格可保存：`0.70`
- 纯诱惑：`0.35`
- 非法：`0`

严格保存同时满足：

1. 有持久、已命名输入。
2. 最迟两次未来市场内有明确窗口。
3. 固定收入与利息后能支付法术及完整后续成本。
4. 相对只持币有非支配理由。

### 连续空转保护

- 仅法术架未满时工作。
- 连续两次出现纯诱惑且都被玩家跳过后，下一次只保证不同主功能且至少达到严格保存。
- 不保证卡名、家族或当前构筑答案。
- 执行一次后清零。

### 记忆

- 最近两次同名都出现：权重乘 `0.35`。
- 最近两次主功能相同：第三次同功能乘 `0.65`。
- 法术架已有同名：乘 `0.25`。
- 连续两次看见并刷新掉同名：下一次乘 `0.50`。
- 公开状态连续两个市场确实缺搜索、转向或退出：对应功能最高乘 `1.50`。

### 纠偏顺序

1. 原始三骰和一法术完整生成。
2. 读取钱包、利息、刷新价、骰袋 / 托盘 / 法术架容量、可卖资产、公开资源和记忆。
3. 生成当前可支付、用途明确的完整行动链。
4. 按第一个不可逆承诺动作去重；依赖移除同一关键资产的路线不能虚报两个方向。
5. 正常资源少于两方向且法术架未满：最多重新选择一次合法法术。
6. 仍不足：最多替换一颗可用性最低的非高阶保底骰。
7. 停止。

### `BridgeGap`

- 只识别公开资源的两端与缺失中段。
- 连续两个市场缺可执行中段时记录。
- 原市场已有两方向时不介入。
- 只在导演本来要纠偏、同资格候选同分时破同分。
- 不增加纠偏预算，不保证卡名或家族。
- 法术架满或玩家近期拒绝同功能时不追发。
- 转换执行或任一端消失后清零。

## 防止无限与重复状态

允许长循环，不设置无意义硬次数上限；只切断真无限和重复写入：

- 付费刷新原始价格持续递增，折扣只作用下一次净价。
- 货架商品被吞吃 / 采购后留空，不自动补货。
- 人口招募只补一半出售数量，`CrewRecruit` 不再生第二代。
- 法术会消耗，法术位每次整刷只补一张。
- 托盘只有两格且必须清盘。
- 主动能力按实例每市场一次。
- 同一事务子事件以 `RootActionId + SequenceIndex` 去重。
- 若提交后状态摘要与先前同一根动作状态完全重复，调试构建记录循环告警并停止自动队列，不向正式玩家静默发资源。

## 数据文件建议

新增：

- `Assets/Resources/Data/spell_market_config.csv`
- `Assets/Resources/Data/market_function_bucket_config.csv`
- `Assets/Resources/Data/market_director_config.csv`

建议字段：

```text
spell_market_config.csv
id,name,price,tier,primary_bucket,secondary_bucket,early_weight,mid_weight,late_weight,boss_multiplier,enabled

market_function_bucket_config.csv
content_kind,content_id,primary_bucket,secondary_bucket,chapter_min,chapter_max

market_director_config.csv
key,value
```

现有 `dice_market_config.csv` 的价格、章节权重和高阶保底首轮不改；功能桶可以使用独立映射，避免把设计标签混入现行解析后同时改变基础供给。

## 代码影响

优先拆分而非继续扩大单文件按钮逻辑：

| 区域 | 当前入口 | F022 目标 |
|---|---|---|
| 市场状态 | `marketOffers` 与散落字段 | 三骰、一法术、两托盘、三法术架、事务、导演记忆 |
| 购买 | `BuyOffer()` | `Prepare/CommitDiePurchase()` 与 `Prepare/CommitSpellPurchase()` |
| 装入 | 不存在独立动作 | `Prepare/CommitFirstInstall()`、`CommitReturn()` |
| 出售 | `SellSelectedMarketDie()` | 资产退出模式与来源感知出售 |
| 刷新 | `RefreshMarketOffers()` | 锁价、批量命令、生成与导演 |
| 离店 | `ActivateMarketLeave()` | 托盘清盘门与目标准备 |
| 吞吃 | `ResolveDevour()` 自动最低点目标 | 显式目标、来源、可吞吃面组与根动作 |
| 化壳 | 离店位置自动化壳 | 托盘直接化壳、壳实例、移壳 |
| 招募 | 直接进骰袋 | 进入托盘与来源递归排除 |
| 存档 | `SerializeDice()` + PlayerPrefs 字段 | 版本化完整运行状态和事务日志 |
| UI | `DrawArcadeMarket()` | 灰盒新增区域与统一弹层 |

若不立即拆文件，至少要在 `DiceKingDemo.cs` 内建立清楚的领域段和纯函数，禁止 UI 绘制函数直接改变钱包、货架、托盘或随机状态。

## 存档与迁移

### 建议版本

- `CurrentSaveVersion = 13`。
- 保留设置、音量、显示模式、开场状态。
- 由于 V12 没有市场状态和稳定实例来源，默认清除正在进行的 V12 运行存档，不尝试在市场中猜测托盘 / 法术状态。
- 若需要保留 V12，只允许从稳定非市场 `Ready` 状态迁移，法术架为空、导演记忆为空；不能迁移半个市场。

### V13 最低保存项

- 当前模式、关卡、钱包、生命、本关累计分和现有长期字段。
- 骰袋顺序与每个稳定实例的首次装入、来源、饲料账本、壳实例和主动使用状态。
- 三格骰货架完整实例与锁价。
- 一格法术货位。
- 两格交易托盘。
- 三格法术架。
- 本市场刷新原价、净折扣、已付刷新次数、市场最低点、持续法术。
- 法术与骰子供给记忆、`BridgeGap`。
- 当前稳定随机种子 / 游标。
- 事务日志与 before / after 校验摘要。

建议使用一个版本化 JSON 运行状态写入 PlayerPrefs，而不是继续向 `DiceData` 的 `~` 分隔字段无限追加。旧设置键可以保留。

### 保存时点

- 每个原子动作完整提交后立即保存。
- `Prepared` 日志在提交前持久化。
- 完整后态保存后再删除事务日志。
- 预览、悬停、目标切换和取消不写运行存档。

## 确定性日志

每个市场输出一条可机器读取记录：

- 种子、章节、市场类型。
- 入店钱包、利息、刷新价、骰袋、托盘、法术架。
- 原始三骰与法术。
- 自然法术可用分类。
- 方向签名与被排除的假方向。
- 法术纠偏、骰位纠偏、`BridgeGap` 原因。
- 每个玩家动作、钱包变化、来源、目标和根动作 ID。
- 事务 before / after 摘要。
- 离店状态与停止原因。

自然供给和导演结果必须分列，不能只记录最终市场。

## 自动测试

### 单元 / 纯逻辑

- 法术三段权重与 Boss 倍率，`0` 权重不提前解锁。
- 差异化骰价与折价后锁价。
- 贸易猪 `R/C/V` 三条路径。
- 可吞吃面组公式。
- 真实壳移动守恒和已用状态。
- 贡品最低二金与显式折价到一金。
- `CrewRecruit` 来源排除。
- 严格保存四项分类。
- 方向签名去重和 `BridgeGap` 破同分。

### 事务

- 在 `Prepared`、扣款后、第一子事件后、最后子事件前模拟重载。
- 清仓、换班、满架替换、黑帆刷新、魔蝠离店和移壳均只能恢复 before / after。
- 不重复首次装入、吞吃、分账、招募、金币和壳复制。

### 回归

- 现行三个骰子货架、买走留空、整刷补齐。
- 现行刷新基础价与递增。
- 负刷新价返金。
- 高阶保底不被单格法术推进或覆盖。
- 市场最低点、补给猪和现有商品个体状态。
- 骰袋排序、Tooltip、卖出、离场队列。
- 生命池、累计续投、通关收入和利息。
- F021 LED、实体键帽和短时提示。

## 手工验证

- 满袋时先买新骰进托盘，比较后卖旧并装新。
- 托盘两格被组装体与回收骰占满时，所有阻塞原因正确。
- 抢劫海盗和血契船长取消目标不消耗次数，成功后回收也不刷新次数。
- 贸易猪现金、转栏保留、转栏后吞吃在不同固定状态反转。
- 托盘龟选择实体与直接化壳，至少一次保留实体是合理答案。
- 贡品奉献、零售和延后 / 放弃均出现。
- 三格法术架满时可放弃、先施旧牌或原子替换。
- 黑帆、魔蝠与清仓都只打开一次面板。
- 高阶恶魔 / 海盗循环能连续多步，但最终由价格、食物、托盘或主动停止收束。

## 不得实现

- 冻结或免费保留货架。
- 第二个法术货位。
- 任意骰袋骰免费回托盘。
- 满架随时免费丢法术。
- 导演保证指定卡名、完整两件套或当前家族核心。
- 导演修改高阶保底商品。
- 清仓、黑帆、魔蝠逐颗弹窗。
- 回收后重放旧骰首次装入。
- 把法术加入三骰权重池。
- 把当前所有骰一次性改成主动。

## 当前角色就绪度

程序：阻塞。

代码落点、数据模型、事务、存档和测试合同已经明确；真正阻塞是 `BG3-D` 至 `BG3-K` 仍为候选，尚未获用户批准。批准后建议先实现 `F022-S1` 的纯领域层和确定性测试，不先接正式 UI。
