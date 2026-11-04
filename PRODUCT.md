# Product

<!-- impeccable:product-schema 1 -->

## Platform

Windows 原生桌面，首版 x64；当前交付为本机运行构建，尚未公开发布。

## Stack

用户指定自动化核心使用 MaaFramework。现已实现并运行的技术栈为 C#、.NET 8 WPF、HandyControl 3.5.1、Maa.Framework.Binding.Native 5.10.0 与 Maa.Framework.Runtime.win-x64 5.10.5。当前构建依赖外部安装的 .NET 8 Windows Desktop Runtime x64。

## Users

已经启动 MAA、正在其新手教程中的 Windows 用户。

## Product Purpose

用户点开助手后，自动查找其已经打开的 MAA 教程窗口，完成教程。保持轻量的两项任务范围：默认通过教程，助手「＋」菜单允许追加一项恢复原主题任务。

## Operating Context

当前适配对象为 MAA v6.21.3 简体中文新手教程。已验证环境为本机 Windows 11（build 22631）、144 DPI 和实际多屏环境；其他版本、语言与缩放比例尚未验证。

正式运行入口为 `releases/current/MaaTutorialAssistant.exe`，必须与同目录运行库、resources 一起使用。开发验证使用项目内 `test-environments/maa-v6.21.3` 独立副本，保留用户原安装与业务数据。

## Capabilities and Constraints

- 默认只有一项通过教程任务，启动后自动进入检测与执行流程。
- 自动化识别、截图和输入使用 MaaFramework。
- 操作前记录目标 MAA 的原始主题选项，包括「跟随系统」，再通过其设置切换至深色。
- 教程识别仅适配深色；浅色界面只承担切换主题所需的有限操作。
- 恢复原主题任务通过助手的「＋」按需添加，作用对象为被控制的 MAA。
- 恢复任务最多一项；可在教程运行中添加，也可在教程完成后添加并执行。
- 助手本身的 HUD 保持深色；恢复任务不切换助手自己的主题。
- 运行中支持停止按钮与全局 F8；重试复用同一 MAA 进程的原主题记录，包括 `SyncWithOs`。MAA 重启后不会将旧记录自动套用到新进程。
- 当前使用前台截图和输入，会将 MAA 置前；运行时须保持 MAA 可见，避免同时操作其他窗口。
- 目标丢失、多开、版本不符、识别不符或操作超时会停止后续输入，保留日志与原主题记录供处理后重试。
- 不通过修改 MAA 配置、完成标记、调试标记或私有接口替代教程界面操作。
- 保留现有用户数据；开发验证只写隔离数据。

## Brand Commitments

用户要求前端 HUD 与当前 MAA 保持一致。继承其深色窗口、蓝色强调色、任务列表、状态日志和停止/重试操作方式；默认任务启动即执行，不另行设计新的视觉世界。不使用 MAA Logo，未复制 MAA XAML，不宣称官方或合作关系。

## Evidence on Hand

已完成三次隔离的「亮色 → 暗色 → 教程 → 亮色」完整运行，均通过实际 MAA `GuideStep7`。另已验证真实 GUI 通过「＋」添加恢复任务、教程运行中全局 F8 停止、重试保留 `OriginalTheme=SyncWithOs`，以及 GUI 完成教程后恢复「与系统同步」。测试证据保留于项目的私有 `test-environments/` 与工作区测试记录；具体记录见项目状态文档。

实际完成态截图为 `.impeccable/review/windows.png`。独立视觉审阅对默认 800×600 DIP Windows 完成态给出可交付结论，无实质修复项；该审阅未覆盖运行中、停止与错误态的视觉表现。功能验证与这项视觉审阅分别成立，不据此推定其他 DPI、语言、版本或公开分发合规已经通过。

尚无独立性能基准或跨电脑分发验收；公开分发前事项以 `THIRD_PARTY_NOTICES.md` 为准。

## Product Principles

- 一次启动，自动寻找目标；找不准就停止输入。
- 原始主题在切换之前保存，恢复精确的原选项。
- 从屏幕识别进度，以实际界面变化判断成功。
- 限制功能与依赖，记录每个交付步骤的实际验证。
