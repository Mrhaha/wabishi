# 关间商店分类图文界面接入 Unity

日期：2026-11-09

来源：`Docs/DesignDialogues/F021-arcade-cabinet-main-menu.md` 第七十四至七十八轮。

状态：运行代码与资源已接入，独立编译、编辑器反射验证、受控 GUI 事件回归与二十四张原生双分辨率截图检查通过；物理系统鼠标、普通组件自动派发的完整连续操作和主观观感仍保留用户实机回归。

## 已接入内容

- 普通与章节市场共用左侧自有骰袋、上排骰子、下排法术的布局，每类每页四件，按真实商品列表分页。
- 所有商品价格居中在上、购买键在下，键帽复用原无字底板纹理的九宫采样，保留旧街机外缘。
- 法术卡常态为插画、名称、价格与购买操作，完整 `RuleText` 仅在独立悬停详情中多行显示。
- 原货架索引和本批显示类别分开保存，购买后原位留空、当前页不跳。翻页不改变真实列表、不填参与玩法计数的假空货架。
- 定向换货目标复用分类分页，支持原索引超过第三格的目标；同数量整刷与法术变骰均重建必要的展示映射。
- 持有法术面板同步图文卡与悬停详情，保留施放、目标步骤、返回和取消。
- 关闭面板、交易、刷新、变类、离场、重开和投骰会清理旧悬停。打开目标面板时底层市场只绘制，不再创建会吞掉上层选择的按钮热区。

## 保持的规则与范围

当前正式供给仍为每批三个真实商品，其中一至两个法术，其余骰子。本轮只让展示层支持任意实际列表与分页，没有改变供给配表、骰子池、经济、牌效、触发顺序、随机或 `SaveVersion = 14`。

三栏插画以 `Art/Market/spell_card_art_pilot_strip` 加载，原图 `2172×724`。当前只有冲关增幅、连锁分红、回声指令三张批准原型插画，其余暂复用，不宣称八张正式插画已经完成。

主文件修改前原样快照为 `Temp/20261109_market_ui_before.cs`；本轮相对快照的差异记录为 `runtime-changes.patch`，与用户此前已有改动区分。

## 验证环境

- Unity：`2019.4.33f1`，可执行文件 `F:/unity/Unity20190433_release/Editor/Unity.exe`。
- 隔离工程：系统临时目录中的 `wabish-market-ui-20261109`。仅同步当前运行脚本、资源、场景、必要编辑器入口与项目设置，不导入源区海量评审文件。
- 隔离公司／产品：`CodexWabishMarketUiQA / WabishMarketUi20261109`，不复用正式存档命名空间。
- 临时测试实例在初始化前设置 `suppressSave = true`。原生截图样本的钱包与商品数为验证注入状态，不是平衡参数变更。

## 已通过门禁

- Unity 随附 Roslyn、Mono `4.7.1-api` 对运行脚本与新验证入口编译为零错误，只保留九处既有 `CS0162` 警告。
- V4 字库重建为 `973` 字形，源字符覆盖与原生字库合同检查均为真；具体为 `font-contract-metrics.json`。新旧共有字形的位图与活动宽度全部不变；生成器移除了当前源不再引用的“视”，并补入十三个当前源字符，详见 `capture-metrics.json`。
- `Validate` 在实际 Unity 编辑器反射环境中通过七十二组数量组合：两类数量分别取零、一、四、六、八、三十二，分别覆盖普通与章节市场。
- 反射验证覆盖完整分页与边界、原始索引、价格纵排安全框、真实购买扣款与留空、同总数整刷重新分类、第十六个货位定向换货、满袋资格与血契船长例外、八张法术完整悬停几何、禁止存档及选定存档键前后不变。实际计数与检查列表见 `validation.json`。

## 图形验证的输入边界

批处理隐藏 Game View 的 OS 鼠标不在窗口内。最初直接 `GameView.SendEvent` 无法令重绘持续保持鼠标位置，记录在 `pointer-third-attempt.jsonl`；不同 `MonoBehaviour.OnGUI` 之间也会重设原始鼠标状态。

因此验证入口使用测试专用受控 GUI 宿主：临时实例正常初始化后停止自动绘制，由测试组件在真实 Unity GUI 上下文中提供原始输入并调用同一生产 `OnGUI`。生产候选、计时、绘制、`GUI.Button` 与交易函数保持原样；不赋值悬停结果、不直接调用提交方法伪造鼠标测试通过。

这验证生产界面在受控输入下的行为与原生渲染，不等于 OS 鼠标路线或普通 `MonoBehaviour` 自动派发已完成人工验收。完整连续拖拽、实际系统鼠标、声音和最终主观观感仍应在用户正常编辑器中试玩。

## 最终图形与事件结果

- 最新 `capture.json`：`Passed = true`、`PlayMode = true`、七十二组数量组合、全部断言检查通过，以及二十四张原生截图；断言次数含重复帧条件，实际计数以该文件为准。
- 原生 `1280×720 / 1920×1080` 分别覆盖普通市场、章节市场、四件、六件末页、八件首末页、三十二件末页、购买留空、最长法术悬停、末页换货选择、离场锁定与持有法术面板。
- 两档分辨率的受控原始 `MouseDown` 均被生产按钮消费，控制捕获由零变为三十二；`MouseUp` 继续被消费并自行释放控制，原索引十五从法术变成骰子，法术袋从一张变为空，步骤从选货架返回普通状态。没有直接调用提交方法作为此事件检查的兜底。实际事件记录为 `gui_input_events.jsonl`。
- 两档最长法术悬停前后，完整货架引用与类别、金币及法术袋均相同，排除悬停副作用；完整详情在画面内展开，不遮挡购买键。
- 二十四张截图的原生尺寸全部正确。另导出三张 `1164×654` 非整数压力图和两张原生一比一局部裁切；压力图仅供识别与层级复核，不替代原生锐度判断。
- 十六个跨分辨率商品价格区域均保留普通档 `(255, 192, 74)` 不透明核心，字形合同与旧共有字形守恒检查均通过。

输入诊断额外发现，GUI 矩阵处理后按钮事件位置可能与已经归一化的悬停位置分离。现已在根级市场按钮中将有效鼠标事件临时对齐到同一虚拟指针，再交给原生 `GUI.Button` 判定；`finally` 仅恢复位置和 `GUI.enabled`，保留事件消费与原生控制捕获，不修改点击结果或事务。该入口明确不适用于分组、滚动或旋转子空间。

最后的画面复核还将离场反馈移到法术区标题条，避免覆盖上排购买键；短反馈按实际 LED 字宽装入该行，完整法术 `RuleText` 不受此缩略处理影响。修正后已重新完成编译、全部图形捕获与反射验证。

坐标路径的辅助代码审查参考 Unity 官方 [GUI 控件实现](https://raw.githubusercontent.com/Unity-Technologies/UnityCsReference/2019.4/Modules/IMGUI/GUI.cs) 与 [GUIUtility 命中测试](https://raw.githubusercontent.com/Unity-Technologies/UnityCsReference/2019.4/Modules/IMGUI/GUIUtility.cs)；本机最终行为仍以本目录的实际事件记录和结果为证据。

## 关键文件

- `Assets/Scripts/DiceKingDemo.cs`：主实现与根级坐标一致性处理。
- `Assets/Editor/WabishMarketVariableUiCapture.cs`：隔离验证入口与测试专用 GUI 宿主。
- `Assets/Resources/Art/Market/spell_card_art_pilot_strip.png`：已登记的三栏原型插画。
- `validation.json / capture.json / capture-metrics.json`：最新逻辑、图形与像素检查结果。
- `4dice_4spells_1280x720.png / spell_hover_longest_1920x1080.png / owned_spell_cards_1280x720.png`：代表性原生图。

## 复现

```powershell
& F:\unity\wabish\Docs\QA\MarketVariableShelvesUnity20261109\compile.ps1 -IncludeValidation
& F:\unity\wabish\Docs\QA\MarketVariableShelvesUnity20261109\prepare-isolated.ps1
& F:\unity\wabish\Docs\QA\MarketVariableShelvesUnity20261109\run-unity.ps1 -Mode Validate
& F:\unity\wabish\Docs\QA\MarketVariableShelvesUnity20261109\run-unity.ps1 -Mode Capture
```

图形捕获必须保留图形设备，不能加 `-nographics`，且只允许独立工程与独立公司／产品命名。首次隔离导入与每次运行的日志均保留在本目录，失败尝试按序另存，不将失败截图当作最终通过证据。

Notion 未同步：当前没有可用连接工具。
