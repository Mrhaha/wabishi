# 购买按钮与商品自适应排布验证说明

日期：2026-11-10

本目录承接购买键可购买时高亮、受阻时整键灰暗，以及每页不足四件时在整行内均匀对称分布的修复。实际执行状态与画面结论以本目录后续生成的 `REPORT.md`、`validation.json` 和 `capture.json` 为准；本说明不代表验证已经通过。

## 验证入口与范围

沿用 `Assets/Editor/WabishMarketVariableUiCapture.cs`，保留原有反射、受控原始输入与生产界面绘制路径。

- 普通与章节市场分别覆盖两类商品数量 `0 / 1 / 2 / 3 / 4 / 5 / 6 / 8 / 32`，共 `162` 组组合。
- 每页按真实原索引检查最多四件、商品身份、价格在上、购买键在下，以及所有卡片和热区均在安全区内。
- 每行围绕虚拟坐标 `X = 740` 对称摆放；相邻商品中心等距、没有重叠；一件与末页剩一件均居中。
- 一至五件及八件的真实购买分别检查扣款、原货位留空、页码不变、剩余商品与本行几何不移动，没有生成参与玩法计数的假空位。
- 购买资格覆盖金币足够、金币恰等于价格、金币不足、骰袋已满、法术袋已满、满骰袋仍可买法术，以及血契船长的满袋购骰例外。
- 原有同总数整刷重新分类、第十六货位换货、完整法术悬停安全区、受控原始鼠标事件与禁止存档检查继续执行。

## 原生截图

共 `23` 个状态，分别捕获 `1280×720 / 1920×1080`，合计 `46` 张。

本次关键新增状态：

| 文件前缀 | 复核内容 |
|---|---|
| `1dice_1spell_centered` | 两类各一件均居中 |
| `2dice_2spells_distributed` | 两类各两件均匀对称 |
| `3dice_3spells_distributed` | 两类各三件均匀对称 |
| `3dice_no_spells_affordable` | 当前暂停法术供给配置下的三骰高亮购买键 |
| `3dice_mixed_affordability` | 钱包六金、价格十二／六／五金，同屏对照受阻／可买／可买 |
| `3dice_no_gold` | 金币不足，全部购买键受阻 |
| `3dice_bag_full` | 普通满袋，全部普通骰购买受阻 |
| `3dice_blood_pact_full` | 血契船长满袋购骰例外，可购买键仍高亮 |
| `3dice_purchase_empty` | 三件中已购买一件，保留原空位和剩余商品位置 |
| `5dice_5spells_first` | 五件第一页保持原四件布局 |
| `5dice_5spells_last` | 五件末页只剩一件时居中 |

`capture.json` 的 `AdaptiveRows` 记录实际反射得到的行边界、数量、页码；`PurchaseEligibility` 记录各截图状态中真实的 `CanBuyMarketOffer` 结果。视觉检查必须将该结果与截图中的高亮／灰暗逐项对照，不能只凭截图生成成功判定通过。

## 复现

```powershell
& F:\unity\wabish\Docs\QA\MarketAdaptiveRows20261110\compile.ps1 -IncludeValidation
& F:\unity\wabish\Docs\QA\MarketAdaptiveRows20261110\prepare-isolated.ps1
& F:\unity\wabish\Docs\QA\MarketAdaptiveRows20261110\run-unity.ps1 -Mode Validate
& F:\unity\wabish\Docs\QA\MarketAdaptiveRows20261110\run-unity.ps1 -Mode Capture
```

隔离工程继续复用系统临时目录下的 `wabish-market-ui-20261109`，公司／产品保持 `CodexWabishMarketUiQA / WabishMarketUi20261109`，与捕获入口的命名空间保护匹配。证据全部输出到本目录，不覆盖 2026-11-09 的旧结果。

图形捕获不得添加 `-nographics`；未执行 `Capture` 时只能宣称编译与反射检查状态，不能宣称完成 Play Mode 或图形验收。受控输入没有覆盖操作系统鼠标、普通组件自动派发的完整连续交互、声音或最终主观观感。
