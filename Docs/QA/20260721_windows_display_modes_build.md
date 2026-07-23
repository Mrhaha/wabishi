# Windows 显示模式构建验证

## 构建结果

- 构建目标：`StandaloneWindows64`。
- 构建场景：`Assets/Scenes/SampleScene.unity`。
- 输出目录：`Builds/Windows/DiceKing-Windows-x64-display-modes/`。
- 压缩包：`Builds/Windows/DiceKing-Windows-x64-display-modes.zip`。
- Unity 构建结果：成功，`0` 个错误，`9` 个现有不可达代码警告。
- 构建内容总大小：`140357384` 字节；压缩包大小：`46066170` 字节。

## 完整性校验

- 压缩包条目数：`147`。
- 压缩包包含 `DiceKing.exe`、`DiceKing_Data` 和 `UnityPlayer.dll`。
- 压缩包 `SHA-256`：`B3D8F6680885764A0AF0DAC395968171E0E788749906FBD19D34F231B0A26A80`。
- `DiceKing.exe` 的 `SHA-256`：`4B7222B9B5C49D4C13B5A8DB6F5B3FC7119732D5982AB270EA343D70A3ED3E86`。

## 启动冒烟

- 以 `-batchmode -nographics` 启动成品并保持运行八秒，进程未提前退出。
- 启动日志确认 `DiceKingDemo` 完成初始化并加载 `Resources/Data/roll_feedback_config`。
- 启动日志未发现异常、错误、失败或崩溃记录。
- 冒烟日志：`Logs/DiceKing-Windows-x64-display-modes.smoke.log`。

## 尚未覆盖

- 本轮没有在可见窗口中人工切换三种显示模式和全部分辨率。
- Windows 标题栏双击最大化、再次双击还原、任务栏保留和十秒超时恢复仍需一次独立包人工操作回归。
- 因此本记录证明构建与无图形启动通过，不宣称显示设置界面已完成人工验收。
