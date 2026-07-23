# F009 自动结算与分级破线预览 V1

状态：样张待验收

## 目的

在不修改 Unity 代码和运行资源的前提下，验证以下导演节奏：

- 第一次按 `Space` 后直接启动完整流程。
- 加力窗口首版为 `0`，不等待连打；基础旋转仍正常播放。
- 六骰从左到右停转，锁定后短暂静默读点。
- 不显示第二次 `Space` 结算提示，自动进入逐槽结算。
- 用目标分 `15`、最终分 `48` 的演示手依次经过过关、超过、远超过和暴击，最终显示目标的 `320%`。
- 未达区间只显示蓄压，不提前播放失败结论。

## 产物

- `f009_auto_settlement_score_tiers_preview_v1_20260717.mp4`：正式评审视频，`1280×720`、30 fps、无声。
- `f009_auto_settlement_score_tiers_preview_v1_20260717.gif`：对话内快速预览。
- `f009_auto_settlement_score_tiers_review_v1_20260717.png`：九格节拍总览。
- `f009_score_tier_states_review_v1_20260717.png`：未达、过关、超过、远超过、暴击五个终局状态总览。
- `preview.html`：确定性视觉预览源。
- `render_preview.js`：浏览器逐帧渲染器。
- `build_preview.ps1`：MP4、GIF 和总览图构建脚本。

## 边界

- 全部产物只位于 `Assets/ArtSource/`，没有 Unity 运行时加载 key。
- 预览沿用 F021 已通过的老街机竞技台动态母版，不替换运行资源。
- 用户确认预览后，才进入 `$wabish-dev-implementation` 接入 Unity。
