# F022 法术货架正文可读性验证记录

日期：2026-08-01  
状态：代码、全牌装入、同源真值板与独立编译通过，待真实 Play Mode 回归  
反馈：`F022-FIRST-PLAYABLE-FB-002`

## 问题

用户截图中法术名称可见，但效果正文只显示“下一次结算结束、达标判定前，额……”的开头。根因位于 `DrawArcadeMarketOffer()`：法术正文先被 `TooltipTrim(..., 34)` 截短，再通过不换行的 `DrawLedRoleText()` 绘制在 `127×86` 的窄正文框中，超出部分被物理裁切。

## 修正

- 法术卡宽度由 `155` 调整为 `229` 虚拟像素，正文框变为 `201×86`。
- 正文直接读取完整 `SpellDefinition.RuleText`，不再使用 `TooltipTrim`。
- 正文改用 `DrawLedRoleWrappedText()`，继续服从 `SecondaryWarm` 的 L1 角色与正式 LED 字库。
- 名称、价格和购买键的语义角色、位置与交互未改变；牌效、价格、刷新概率、法术袋和存档未改变。

## 八张首片装入结果

按运行时 `Micro` 几何的实际字形宽度、`201` 像素正文宽度、`16` 像素首行高度和 `18` 像素行距计算：

| 法术 | 行数 | 正文高度 | `86+1` 像素容差内装入 |
|---|---:|---:|---|
| 冲关增幅 | 2 | 34 | 通过 |
| 连锁分红 | 3 | 52 | 通过 |
| 回声指令 | 3 | 52 | 通过 |
| 定向换货 | 4 | 70 | 通过 |
| 精密校准 | 2 | 34 | 通过 |
| 饲料袋 | 2 | 34 | 通过 |
| 削峰套现 | 3 | 52 | 通过 |
| 超额悬赏 | 3 | 52 | 通过 |

失败数为 `0`。最长的定向换货仍保留 `17` 像素以上的纵向余量。

## 同源文字门禁

- 生成器：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/RuntimeTools/build_main_game_led_font_atlas.py`
- `1280×720` 市场真值板：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRuntimeV4/led_font_runtime_v4_market_1280x720.png`
- `1920×1080` 市场真值板：`Assets/ArtSource/Production/ArcadeDiceKing/MainGameFlowV1/LedFontRuntimeV4/led_font_runtime_v4_market_1920x1080.png`
- 两张板同时放入冲关增幅与最长定向换货，名称、完整正文与价格均完整显示。
- 字库保持 `961` 个字形，源字符缺字为 `0`，指标文件全部检查为真。

## 编译

- 使用 Unity 2019.4.33f1 对应 Mono `4.7.1-api` 与 UnityEngine 程序集编译 `Assets/Scripts` 下五个运行脚本。
- 结果：退出码 `0`，错误 `0`；仅有既有 `CS0162` 不可达代码警告。

## 仍需回归

1. 在当前 Unity Play Mode 分别以 `1280×720 / 1920×1080` 查看一张与两张法术货架。
2. 逐张刷新八张首片法术，确认真实卡面没有裁切或遮挡。
3. 点击购买并悬浮货架，确认扩宽卡面没有改变购买键与悬浮命中区。
4. 补查 `1920×1080 @ 0.606×`，只判断名称与正文的识别生存。

## 结论

造成“看不见效果”的单行裁切路径已移除。代码、全牌文字装入、双分辨率同源板和独立编译均通过；未运行当前工程的自动 Play Mode 截图，因此反馈状态保持“待回归”，不宣称界面已最终验收。
