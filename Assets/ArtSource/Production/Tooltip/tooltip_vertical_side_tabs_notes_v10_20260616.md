# 骰子悬浮窗 UI V10：干净竖向侧栏页签

状态：用户已接受，已进入 F014 UI 素材生产。

## 本轮修正

- 保留竖版紧凑简介和竖向页签。
- 删除纸页厚度、书脊、账册装订、多层纸张、铆钉和翻页暗示。
- 页签改为干净的侧边索引控件：贴在面板侧边轨道上，像游戏 UI 的垂直页签，不像纸张从后面伸出。
- 商店待购骰和主投掷区骰继续共用同一套悬浮窗骨架，只改变出现位置和触发来源。

## 最小字段

- 骰子名称。
- 六格点面。
- 骰子效果。
- 品质效果。
- 卖出价格。

统计信息、边界标签、价格规则解释和额外状态暂不默认显示，等确认阅读缺口后再逐项加回。

## 组件规格倾向

- 主体建议约 `350 x 400`，按实际分辨率等比缩放。
- 侧边页签建议约 `42` 宽，使用独立按钮块和细轨道，不做纸页厚度。
- 默认页签可停在“点面”，后续如果内容增多再允许切换“骰效 / 质效”内容。
- 悬浮窗只覆盖信息，不接管点击输入；商店和投掷区的样式必须一致。

## 避免项

- 不做多层纸边。
- 不做底部页线。
- 不做书脊或装订铆钉。
- 不做账册翻页感。
- 不把字段重新扩成大卡片说明。

## 参考文件

- V10 接触图：`Assets/ArtSource/Production/Tooltip/tooltip_vertical_side_tabs_clean_v10_20260616.png`
- V9 历史稿：`Assets/ArtSource/Production/Tooltip/tooltip_vertical_book_tabs_v9_20260616.png`
- F014 生产包：`Docs/Production/Features/F014-dice-hover-tooltip-ui/`
- 运行时 UI 素材预览：`Assets/ArtSource/Production/Tooltip/tooltip_runtime_ui_assets_preview_20260616.png`
