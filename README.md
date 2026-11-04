<div align="center">

# MAA 教程助手

MAA Tutorial Assistant

![Windows x64](https://img.shields.io/badge/platform-Windows_x64-blue)
![.NET 8](https://img.shields.io/badge/.NET-8-512BD4)
[![MIT](https://img.shields.io/badge/license-MIT-green)](LICENSE)

**MAA 解放双手，我们先把双手从新手教程里解放出来。**

一个帮你通过自动化软件新手教程的自动化软件。

[快速开始](#快速开始) · [源码构建](#从源码构建) · [问题反馈](https://github.com/0fengmang0/maa-tutorial-assistant/issues)

</div>

基于 [MaaFramework](https://github.com/MaaXYZ/MaaFramework) 的 Windows 小工具，通过图像识别和窗口操作完成 MAA 新手教程。

毕竟，准备让软件替自己点鼠标的人，未必想先参加一场点鼠标考试。

## 亮点功能

- **自动通过教程**：找到已打开的 MAA 教程窗口，点击「开始」后完成引导任务。
- **自动切换深色**：执行前记录原主题，并将 MAA 切换为深色。
- **按需恢复主题**：通过「＋」添加恢复任务，完成后切回亮色、深色或与系统同步。
- **熟悉的三栏界面**：左侧管理任务，中间查看详情，右侧切换运行日志与当前任务日志。
- **随时停止和继续**：点击「停止」或按 **F8** 中断，处理后点击「开始」继续。

## 快速开始

当前适配 **MAA v6.21.3 简体中文**，运行环境为 Windows x64 和 [.NET 8 Windows Desktop Runtime x64](https://dotnet.microsoft.com/download/dotnet/8.0)。已验证 Windows 11、144 DPI。

1. 按下方说明[从源码构建](#从源码构建)，生成程序。
2. 打开一个 MAA 新手教程窗口。
3. 运行 `releases/current/MaaTutorialAssistant.exe`。
4. 默认勾选「通过 MAA 新手教程」；需要恢复主题时，点击左下「＋」添加并勾选「恢复原主题」。
5. 点击「开始」，等待教程完成。

运行时保持 MAA 窗口可见，并将鼠标键盘交给助手。需要中断时按 **F8**。

点击任务名称或齿轮可以切换详情；「全选」勾选已有任务，「清空」取消勾选。教程完成后，也可以单独添加恢复主题任务，再点击「开始」。原主题记录保存在程序目录的 `data/` 中。

## 从源码构建

准备 Windows x64、Git 和 [PowerShell 7](https://github.com/PowerShell/PowerShell/releases)，执行：

```powershell
git clone https://github.com/0fengmang0/maa-tutorial-assistant.git
Set-Location .\maa-tutorial-assistant
pwsh -NoProfile -File .\scripts\Build.ps1 -ForceRestore
```

脚本会准备固定版本的 .NET SDK、NuGet 依赖和 OCR 模型，产物位于 `releases/current`。运行时保留该目录的完整内容，并安装 .NET 8 Windows Desktop Runtime x64。

构建缓存默认放在系统临时目录，可通过 `-Scratch '缓存目录'` 指定位置。

## 参与开发

欢迎通过 [Issues](https://github.com/0fengmang0/maa-tutorial-assistant/issues) 反馈问题或提出适配需求，也欢迎提交 Pull Request。

反馈时附上 MAA 版本、界面语言、显示缩放和出错步骤，方便复现。

## 致谢

- [MaaAssistantArknights](https://github.com/MaaAssistantArknights/MaaAssistantArknights)：教程的主角，也是这个项目的灵感来源。
- [MaaFramework](https://github.com/MaaXYZ/MaaFramework) 与 [C# Binding](https://github.com/MaaXYZ/MaaFramework.Binding.CSharp)：图像识别与窗口操作。
- [HandyControl](https://github.com/HandyOrg/HandyControl)：WPF 界面控件。

## 许可

本项目采用 [MIT](LICENSE) 许可证。第三方组件的许可信息见 [THIRD_PARTY_NOTICES.md](THIRD_PARTY_NOTICES.md)。
