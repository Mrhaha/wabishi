# 法术卡面插画评审示意

日期：2026-11-09

关联：`Docs/DesignDialogues/F021-arcade-cabinet-main-menu.md` 第七十六轮。

状态：用户已授权用于当前原型的 Unity 运行接入；三张示意可随本次已确认界面使用，不代表八张法术正式卡画已经完成或通过终稿验收。源图继续保存在本目录，运行副本见 `Assets/Resources/Art/Market/spell_card_art_pilot_strip.png`。

用户已明确要求法术卡常态为卡面插画与必要文字，完整牌效仅在悬停详情中显示。本轮先以三张代表插画检验现有午夜旧街机风格，未批量绘制八张正式卡画。

## 文件与内容

- `spell_card_art_three_panel_pilot_v1_20261109.png`：`2172×724` 不透明 PNG，三栏等宽插画，从左到右为冲关增幅、连锁分红与回声指令。插画不包含卡名、规则、价格或购买键。
- `generation_request.json`：内置 `image_gen.imagegen` 实际提示词及风格参考关系。
- 当前原型按三栏读取同一原图，其余尚无专属插画的法术暂复用对应示意。用户已批准按该原型修改 Unity；复用仅用于本轮原型接入，不代表插画含义或后续正式资源分配已经冻结。
- 运行副本：`Assets/Resources/Art/Market/spell_card_art_pilot_strip.png`，加载 key 为 `Art/Market/spell_card_art_pilot_strip`。副本与源图字节一致，SHA-256 为 `524FD1B0FF684819B3080DCB5246AF6081F45CA242A4B94C35BC5214122C7D61`；导入设为无压缩、无 mipmap、Clamp、保留原图尺寸，最大边限制 `4096`，不做非二次幂缩放。
- 原型复用映射：冲关增幅／精密校准／超额悬赏使用左栏，连锁分红／饲料袋／削峰套现使用中栏，回声指令／定向换货使用右栏。未开放的托盘法术不因本资源进入运行目录而启用。

## 快速复核

- 通过：旧街机深蓝、青绿、奶油色骰体与琥珀机制光，与现有材质衔接。
- 通过：三栏分别具有增幅、连锁产金、回声的可辨主体，没有文字、水印或伪骰面结果。
- 通过：卡名、价格、规则与动作均保持为独立绘制内容，卡画不烘焙动态文字。
- 当前仅批准按原型使用三张示意，后续逐牌插画、细节密度和正式卡面裁切仍需各自评审，不作为完整正式插画包。

常态与悬停示例位于 `Docs/QA/MarketVariableShelvesStress20261109/captures-illustrated/`，完整验证见同目录 `SPELL_CARD_HOVER_REPORT.md`。

已在 `ART_ASSETS.md` 登记本轮源图与运行副本。程序接入、导入后的资源加载和 Play Mode 验证由本轮 Unity 实现任务记录；本资源准备步骤不提前宣称运行界面已通过，也未修改真实牌效、配表、经济、随机或存档。Notion 未同步。
