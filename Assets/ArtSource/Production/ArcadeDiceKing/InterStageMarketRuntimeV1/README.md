# 关间市场运行时资源包 V1

日期：2026-07-17  
状态：用户通过三状态样张后生产，已接入 Unity 并完成双分辨率 Play Mode 截图

## 文件

- 源图：`arcade_market_common_clean_plate_source_v1_20260717.png`
  - `1672×941`
  - `Format24bppRgb`
  - SHA-256 `18D4498706BF7B3E130555E063C91891D45CFEC7022E294CC29D07E4B6D02340`
- 构建脚本：`build_market_runtime_assets.ps1`
- 运行时输出：`Assets/Resources/Art/Market/arcade_market_common_base.png`
  - `1920×1080`
  - 不透明 PNG
  - SHA-256 `4C4C3A8A2C027B6EB627051DFF78E368F9E57CDC4D1829DCDA4623E29A7BECCC`
  - Resources key：`Art/Market/arcade_market_common_base`

## 生成提示

参考图：

- 已通过的关间市场三状态接触图 V1。
- 当前主游戏 `Ready` 的 Unity 运行截图。

实际生成提示：

> Create one production-ready 16:9 full-screen CLEAN PLATE for a Unity 2D game inter-stage dice market. Use the APPROVED three-state market contact sheet as the exact functional/layout reference and the current main-game runtime screenshot as the material/style reference. The result is one single normal-state empty arcade supply cabinet background, straight-on orthographic UI view, no perspective tilt.
>
> STYLE: midnight old neighborhood arcade / retro electronics, worn navy-blue painted plastic and restrained dark metal, smoked glass, small amber indicator lights, subtle cyan service lights, fine scratches and age, warm off-white physical keycaps. Cute-but-not-childish, polished 2D flat game UI with tactile depth. Reduce military crates, weapons, factory rails, hard industrial tracks and steampunk clutter. It must clearly belong to the same machine family as the provided main-game screen.
>
> EXACT STATIC LAYOUT: one continuous outer cabinet frame. Across the top, four blank smoked-glass display windows with generous empty dark interiors for dynamic text: a title window, a gold/resource window, a bag-capacity window, and a refresh-cost window. In the main body, a narrow continuous vertical bag cabinet on the left with exactly six evenly spaced empty recessed sockets and one blank physical sell/reclaim tray directly beneath it. On the right, one continuous merchandise cabinet with exactly three large equally sized empty product wells in one horizontal row; each well has a spacious empty center for a dynamic dice shell, a blank label region below, and a blank physical purchase-key area at the bottom. Along the bottom, exactly three separated control modules: a blank refresh key on the left, a long blank smoked-glass feedback/status terminal in the center, and a large blank leave-market key on the right. Keep all wells, windows and keys clean, readable, symmetrical and comfortably separated.
>
> CRITICAL CLEAN-PLATE RULES: absolutely no letters, no Chinese, no English, no numbers, no currency symbols, no icons, no dice, no family shells, no type cores, no faces, no prices, no tooltips, no arrows, no hover outline, no sold-out text, no scanning line, no particle trail, no progress bar, no selection glow, no animated state, no characters. Only the static cabinet, blank displays, empty sockets, empty wells, blank keycaps and subtle neutral ambient lights. Do not create a contact sheet or multiple panels. No watermark. Fill the entire 16:9 canvas edge-to-edge.

## 构建

在项目根目录运行：

```powershell
& Assets/ArtSource/Production/ArcadeDiceKing/InterStageMarketRuntimeV1/build_market_runtime_assets.ps1
```

脚本只执行可复现的高质量缩放和尺寸校验，不烘焙文字、数字、骰子、价格、Tooltip、高亮或动画。

## 运行时边界

- 底板只负责静态机柜。
- 顶部资源、六格骰袋、三格商品、购买留空、刷新反馈、离场锁定、扫描线和 Tooltip 全由 Unity 动态绘制。
- 商品复用主游戏五张家族壳；未锁结果时只显示类型芯，不伪造点数。
- 资源缺失或词缀功能重新启用时回退旧市场界面。
- 不修改市场经济、随机、价格、买卖、刷新、离场效果、存档或结算规则。
