# MAA 教程助手

基于 [MaaFramework](https://github.com/MaaXYZ/MaaFramework) 的 Windows 本地工具，帮助用户通过 MAA 新手教程，并按需恢复原主题。独立项目，非 MAA 官方产品。

## 功能

- MAA 风格深色三栏界面：左侧勾选任务、中间切换详情、右侧查看运行或当前任务日志。
- **打开后等待，点击「开始」才执行**，自动寻找已打开的一个 MAA 教程窗口。
- 操作前记录原主题，通过 MAA 设置切为深色，再依靠截图、识别与窗口输入完成教程。
- 通过左下「＋」追加「恢复原主题」任务，支持亮色、深色和与系统同步；未勾选则保持深色。
- 运行中点击「停止」或按 **F8** 中断，处理问题后点击「开始」继续。
- 本地运行，不连接游戏，不运行游戏任务，不用改写完成标记替代教程操作。

## 兼容范围

| 项目 | 当前情况 |
| --- | --- |
| 系统 | Windows x64，需要 .NET 8 Windows Desktop Runtime x64 |
| 已有完整流程验证 | MAA **v6.21.3 简体中文**，本机 Windows 11、144 DPI、多屏环境 |
| MAA v6.21.4 | 不在当前自动化支持范围；仅另开过隔离教程供手动查看 |
| 其他电脑、缩放、语言或版本 | 未验证 |

前台截图和输入会占用鼠标键盘，运行时请保持 MAA 可见，避免同时操作其他窗口。目标丢失、多开、识别不符和超时会停止后续输入，保留原主题记录。

## 使用

1. 打开一个 **MAA v6.21.3 简体中文新手教程**。
2. 运行 `releases/current/MaaTutorialAssistant.exe`。保留 EXE 同目录全部运行库和 `resources`，不要单独移动 EXE。
3. 默认勾选「通过 MAA 新手教程」。需要恢复主题时，点击左下「＋」添加并勾选「恢复原主题」。
4. 点击左下「开始」。运行中该按钮变为「停止」，也可使用 F8。
5. 点击任务名称或齿轮切换中间详情；右侧可切换「运行日志」和「当前任务」。

「全选」勾选已有任务，「清空」取消全部勾选；没有任务被勾选时不能开始。教程完成后仍可添加恢复任务，再点击开始执行。助手 HUD 始终保持深色。

原主题记录保存在运行目录 `data/`。同一个 MAA 进程中会复用未完成记录，避免用已切换的深色覆盖原选项；MAA 重启后不会自动将旧记录应用到新进程。请保留这些记录。

## 从源码构建

需要 Windows x64、Git 和 **PowerShell 7**。构建脚本会准备项目内 SDK 8.0.427、锁定的 NuGet 依赖和固定哈希的 OCR 模型；首次构建需要访问 Microsoft、NuGet 与 GitHub。

```powershell
pwsh -NoProfile -File .\scripts\Build.ps1 -ForceRestore
```

产物位于 `releases/current`。默认缓存位于系统临时目录的 `maa-tutorial-assistant-build`，可通过 `-Scratch '缓存目录'` 指定，不要求存在 D 盘。继续在原工作区开发时，应按工作区规则显式传入集中临时目录。

本机运行目录不包含整套 .NET 运行时，运行前需要安装 .NET 8 Windows Desktop Runtime x64。当前没有对外发布已完成依赖合规整理的二进制安装包。

## 开发验证

准备一个 **v6.21.3** 的隔离副本，仅首次运行 Prepare，关闭该副本后可重复 Reset：

```powershell
.\scripts\Prepare-TestFixture.ps1 -Source '你的 MAA v6.21.3 安装路径' -Scratch '你的临时目录'
.\scripts\Reset-TestFixture.ps1 -Theme Light -Launch -Scratch '你的临时目录'
```

隔离入口禁止游戏核心初始化和自动任务。命令行 `--run-fixture <输出目录> [--restore]` 只允许操作带项目标记的副本；`--theme-test <目录>` 验证主题；`--probe <输出.json>` 导出窗口识别；`--preview-ui` 仅预览界面，不执行任务。

历史完整流程证据与本轮前端人工验收的边界分别登记在项目文档中，不将构建成功或一张截图视为跨环境验收。

## 文档和发布

- [GitHub 上传详细步骤](docs/GitHub上传步骤.md)：创建空仓库、Git 推送、网页上传、源码 Release 与校验。
- [给介绍视频 Agent 的提示词](docs/介绍视频Agent提示词.md)：可直接复制的项目功能介绍。
- [任务书](docs/任务书.md)、[开发过程](docs/开发过程.md)、[项目状态](docs/项目状态.md)、[验收记录](docs/验收记录.md)。
- [技术方案](docs/技术方案.md)、[第三方组件声明](THIRD_PARTY_NOTICES.md)。

源码打包：正式改动提交后运行 `pwsh -NoProfile -File .\scripts\Package-Source.ps1`，在 `releases/github` 生成源码 ZIP、SHA256 和提交信息。源码包排除开发 SDK、二进制、MAA 安装、用户数据、日志和私有证据；不会自动上传或推送。

## 许可

原创助手源码使用 [MIT](LICENSE)。MaaFramework、Binding 和其他依赖适用各自许可证，见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md) 和 `licenses/`。未复制 MAA 源码、XAML、Logo 或游戏素材。包含原生依赖的公开运行包仍有许可和对应源码提供方式的整理事项，不能用本项目 MIT 许可证替代。
