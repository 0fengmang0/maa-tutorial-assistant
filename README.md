# MAA 教程助手

基于 MaaFramework 的 Windows x64 本地工具。启动即寻找已经打开的 MAA 教程窗口，保存原主题、通过设置强制切为暗色，逐页完成教程及任务演示。通过 HUD 的「＋」可追加「恢复原主题」，精确恢复亮色、暗色或与系统同步。

## 使用

1. 打开 **MAA v6.21.3 简体中文新手教程**，只保留一个 MAA 窗口。
2. 运行 `releases/current/MaaTutorialAssistant.exe`，它会自动执行。
3. 若需要结束后恢复 MAA 原主题，点击助手「＋」→「恢复原主题」。不添加则保持暗色。
4. **F8** 或「停止」中断；「重试」继续。运行期间程序会占用鼠标与键盘，让 MAA 保持可见，并避免同时操作其他窗口。

本机已安装 .NET 8 Windows Desktop Runtime。分发给其他电脑时也需要该 x64 运行时；本构建没有打包整套运行时。禁止把 EXE 单独移出其运行库和 resources 所在目录。

助手 HUD 固定暗色。教程完成后仍可通过加号恢复主题；同一个 MAA 进程中的中断/重启会复用原主题记录。MAA 退出重启后，旧记录仍保留，但不会自动套用到另一个进程。

## 兼容边界

- 已针对本机 v6.21.3、中文、144 DPI、实际多屏环境验证。**v6.21.4、其他语言和其他缩放比例尚未验证**。
- 采用 MaaFramework ScreenDC 前台截图、OCR/ColorMatch、Win32 输入；后台截图在本机初始化失败，因此运行会将 MAA 置前。
- 不连接游戏，不运行游戏任务，不修改教程完成标记。开发测试使用 `test-environments/maa-v6.21.3` 的独立安装，并禁用游戏核心初始化。
- 目标丢失、多开、版本不符、OCR 不符、操作超时均停止后续输入。异常证据与原主题记录保留在运行目录的 `data/`。

## 构建与复现

在 PowerShell 7 中运行：

```powershell
./scripts/Build.ps1 -ForceRestore
./scripts/Prepare-TestFixture.ps1 -Source '你的 MAA v6.21.3 安装路径'
./scripts/Reset-TestFixture.ps1 -Theme Light -Launch
```

关闭隔离副本后才可重置。Prepare 只首次运行，附原安装全部文件 SHA256 清单；后续使用 Reset。SDK 8.0.427、NuGet 锁文件、模型固定提交与哈希均纳入复现路径。

仅对带项目标记的隔离副本运行无界面回归：

```powershell
$run = 'D:\AI_work_space\tmp\agent-scratch\20261104-194014-maa-tutorial-dev\my-run'
New-Item -ItemType Directory -Path $run -Force | Out-Null
$process = Start-Process ./releases/current/MaaTutorialAssistant.exe -ArgumentList '--run-fixture',$run,'--restore' -WindowStyle Hidden -RedirectStandardOutput "$run/host.stdout.log" -RedirectStandardError "$run/host.stderr.log" -Wait -PassThru
if ($process.ExitCode -ne 0) { throw 'Regression failed; inspect the run log.' }
./scripts/Verify-Run.ps1 -ResultDirectory $run -OriginalTheme Light -Restore -CheckOriginal
```

不加 `--restore` 可验证结束保持暗色。`--theme-test <目录>` 只验证主题切换与恢复。`--probe <输出.json>` 导出唯一 MAA 的截图/OCR。`--preview-ui` 仅显示 HUD。

## 目录与记录

- `src/`、`scripts/`：正式源码与构建/验证入口。
- `dependencies/`：开发 SDK 和模型；`releases/current/`：本机运行构建。二者不混入 Git。
- `test-environments/`：隔离 MAA、原文件哈希基线与私有验收证据，不公开。
- [任务书](docs/任务书.md)、[开发过程](docs/开发过程.md)、[项目状态](docs/项目状态.md)、[技术方案](docs/技术方案.md)：范围与真实进度。
- [第三方声明](THIRD_PARTY_NOTICES.md)：许可、来源与公开分发前仍需完成的事项。

原创助手源码使用 MIT；未复制 MAA Logo、源码或 XAML，不代表官方。当前仅在本机交付，无远程仓库、推送或公开发布。
