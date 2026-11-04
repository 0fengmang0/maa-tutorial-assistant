---
version: 1
slug: "src-mainwindow-xaml"
primary_target: "src/MainWindow.xaml"
related_targets: ["src/MainWindow.xaml.cs","src/App.cs"]
---

# 主窗口三栏改造

Scope: Windows WPF 主窗口；Operate。用户提供的 MAA 截图是布局与控件的视觉依据，保留助手现有的两项真实任务，不加入游戏任务。

## Direction contract

THESIS: 复现参考图的「左侧任务容器 / 中间选中任务详情 / 右侧日志」工作方式，替换原有双栏和底部横排操作。

OWN-WORLD: 保持 MAA 的 #1C1C1C 深色、细灰边框、蓝色勾选和齿轮。系统中文字体，紧凑的原生桌面密度，沿用 HandyControl。

STORY: 打开后等待；勾选任务，点击名称或齿轮查看详情，点击左下开始。运行中同一按钮变为停止，F8 仍可中断。加号追加恢复主题任务。

FIRST VIEWPORT: 默认 960×680 DIP，左侧约 220 DIP，居中详情约 240 DIP，右侧日志使用剩余宽度。任务容器底部是加号、全选和清空；容器外下方是完成后无动作和开始按钮。中间常规/高级设置切换，右侧全部/当前任务日志切换。

FORM: 用户明确指定参考图，没有候选方向或随机种子；代码实现。签名交互为点击任务后中间详情和当前任务日志同步切换。保持静态原生切换，不添加装饰动画。

FINISH: unreviewed and undocumented is unfinished; this build ends with the finish review, the verdict, DESIGN.md, and every shipping raster carrying its provenance

## Confirmed answers

- 用户确认改为打开后等待，点击「开始」才执行。
- 本轮不扩展 v6.21.4 的自动化适配；该版本隔离窗口属于用户手动确认。
- 原安装与用户数据保持原状；新界面测试不推进该窗口的教程。
