# F014 验收记录

状态：程序已静态接入，待 Unity 运行验收
功能：骰子悬浮窗 UI

## 就绪度门禁

| 角色 | 就绪度 | 缺口 / 风险 | 建议补丁 | 阻塞问题 |
|---|---|---|---|---|
| 策划 | 已确认 | 字段范围已收敛到最小满足版 | 后续只按阅读缺口加字段 | 否 |
| 美术 | 已完成素材 | 固定面板素材可能需要程序侧缩放验证；页签素材保留但本版不用 | 必要时保留程序化面板，使用胶囊和标签素材 | 否 |
| 界面体验 | 已完成规格 | 已移除右侧“面 / 效 / 质”页签；时机和渐变已补齐 | 后续信息增多再评估分页或新增字段 | 否 |
| 程序 | 已静态接入 | Unity Play Mode、动态行为和截图尚未验收 | 按验收清单做市场与主投掷区截图验证 | 否 |

## 验收清单

- [x] V10 方向已由用户接受。
- [x] 已生成运行时 UI 素材。
- [x] 已生成素材预览。
- [x] 已登记 `ART_ASSETS.md`。
- [x] 已创建美术、界面体验、程序交接和执行计划文档。
- [x] 已补充出现时机、淡入淡出、目标切换和阶段抑制规范。
- [x] 程序接入 `DiceKingDemo.cs`。
- [x] 骰子效果说明已改为 `Assets/Resources/Data/dice_type_config.csv` 数据来源。
- [x] 右侧“面 / 效 / 质”页签已从运行时绘制移除。
- [ ] 市场 hover 截图验收。
- [ ] 主投掷区 hover 截图验收。
- [ ] 悬浮窗动态行为验收。
- [ ] `1280x720` / `1920x1080` 适配验收。

## 美术验证记录

- V10 接触图：`Assets/ArtSource/Production/Tooltip/tooltip_vertical_side_tabs_clean_v10_20260616.png`。
- 运行时素材预览：`Assets/ArtSource/Production/Tooltip/tooltip_runtime_ui_assets_preview_20260616.png`。
- 运行时素材目录：`Assets/Resources/Art/UI/Tooltip/`。
- 源图目录：`Assets/ArtSource/Production/Tooltip/runtime_ui_sources/`。
- 资源内不包含业务文字、点面数字或规则长说明。
- 当前运行时保持单层浮窗，不显示侧边竖向页签，没有纸页厚度、书脊、账册装订和翻页感。

## 程序待验收项

- 市场三个货架 hover 可显示。
- 当前骰袋列表 hover 可显示。
- 主投掷区实体骰 hover 可显示。
- 临时小骰不显示。
- 鼠标扫过目标不足 `0.12s` 时不显示。
- 鼠标稳定停留后 `0.10s` 左右完成淡入。
- 鼠标离开后 `0.08s` 左右完成淡出。
- 已显示时横向切换相邻骰子不重复等待和重淡入。
- `Shaking` / `Stopping` 阶段不闪烁显示。
- `Scoring` 默认不显示，不遮挡结算飞字。
- 文本不溢出。
- 靠右货架可向左避边。
- 悬浮窗不接管点击。
- 右侧“面 / 效 / 质”页签不出现。
- 骰子效果行显示 CSV 的具体效果说明，不退回短标签。

## 静态验证记录

- 已通过 `git diff --check`。
- 已检查当前运行时 tooltip 资源 key 与文档清单一致。
- 已确认 `dice_type_config.csv` 覆盖当前 43 个 `DieType`，无缺失、无额外类型、无空字段。
- 已确认 `DiceKingDemo.cs` 不再包含右侧页签运行时绘制和页签素材加载引用。
- 已确认 F014 文档无尾随空白，运行时素材目录包含 9 张 PNG 和对应 `.meta`。
- 已做 `DiceKingDemo.cs` 大括号数量检查。
- 当前环境未在 PATH 中找到 `dotnet`、`csc` 或 `Unity`，尚未做编译或 Play Mode 验收。
- Notion cockpit 已同步到“CSV 效果说明与页签移除静态接入完成，待 Unity 验收”。

## 最终结论

F014 当前完成了设计确认、UI 素材生产、资源登记、出现时机规范、执行审查和 `DiceKingDemo.cs` 静态接入。下一步进入 Unity Play Mode，按市场、当前骰袋和主投掷区 hover 清单做动态行为与截图验收。
