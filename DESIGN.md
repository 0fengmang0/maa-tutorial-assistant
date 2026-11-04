---
name: MAA 教程助手
description: 延续 MAA 深色 WPF 任务面板的 Windows 本地助手
colors:
  accent-line: "#178DD4"
  accent-content: "#49ABDF"
  window: "#1C1C1C"
  text: "#EEEEEE"
  border: "#414141"
  text-muted: "#B3B3B3"
  log-surface: "#202020"
  log-text: "#D9D9D9"
typography:
  progress-title:
    fontSize: "18px"
  section-label:
    fontSize: "15px"
  task-status:
    fontSize: "12px"
rounded:
  task-row: "3px"
spacing:
  status-gap: "6px"
  task-gap: "8px"
  inset: "12px"
  heading-gap: "14px"
  column-gap: "18px"
components:
  section-header:
    textColor: "{colors.accent-content}"
    typography: "{typography.section-label}"
    padding: "8px 12px"
  task-row:
    backgroundColor: "{colors.window}"
    textColor: "{colors.text}"
    rounded: "{rounded.task-row}"
    padding: "{spacing.inset}"
  task-status:
    textColor: "{colors.text-muted}"
    typography: "{typography.task-status}"
  progress-heading:
    textColor: "{colors.text}"
    typography: "{typography.progress-title}"
  log-view:
    backgroundColor: "{colors.log-surface}"
    textColor: "{colors.log-text}"
    padding: "{spacing.inset}"
  add-button:
    height: "34px"
  stop-button:
    width: "150px"
  retry-button:
    width: "100px"
---

# HUD 界面约束

## Overview

**Creative North Star: "MAA 深色任务面板"**

### 视觉依据

继承用户当前 MAA v6.21.3 的原生深色 WPF 界面。深灰底色、细边框、蓝色强调和紧凑任务布局服务于本地执行状态的持续阅读；本次是在既有身份内实现助手，没有替换视觉世界。

实际实现使用 WPF 与 HandyControl 3.5.1，加载其 `SkinDark.xaml` 和 `Theme.xaml`。界面名称为「MAA 教程助手」，不使用 MAA Logo，未复制 MAA XAML。规范来自 `src/MainWindow.xaml`、`src/MainWindow.xaml.cs`、`src/App.cs` 和完成态截图；未显式设置的控件细节仍由当前 HandyControl 深色主题与 WPF 继承。

**Key Characteristics:**
- 深色窗口与轻微分层的日志面板。
- 稀疏蓝色强调，优先突出当前步骤与任务状态。
- 紧凑原生桌面控件，保持两项任务的清晰范围。

## Colors

蓝色只承担标题、分隔线和加号提示；其余阅读区域由中性深灰与浅色文字组成。前置令牌为源码中明确指定的值，不包含尚未展开的 HandyControl 主题内部色阶。

### Primary

- `accent-line`：标题区底部分隔线。
- `accent-content`：分区标题与加号笔画；二者沿用同一强调色。

### Neutral

- `window` / `text`：窗口与任务正文的默认底色、前景色。
- `border`：任务行与日志面板的细描边。
- `text-muted`：任务次级状态与底部本地运行说明。
- `log-surface` / `log-text`：独立日志阅读面，保留与窗口轻微的明度差。

**The Restrained Accent Rule.** 用文字表达完成、停止与失败；不要给每种运行状态新增一套强调色。

## Typography

字体族、默认正文大小、字重、行高和控件文字由 WPF / HandyControl 主题继承；本项目未声明独立字体资产或固定中文字体族。保持系统中文桌面文字的密度，后续变更须先核查实际继承资源。

- `progress-title`：右侧当前步骤，可换行。
- `section-label`：顶部「教程助手」分区标签。
- `task-status`：任务行第二行的小号次级文字。

令牌只记录 XAML 已明确设置的三个字号，不推定标题栏、日志或按钮的字体值。

## Layout

### 主界面

以下尺寸为 WPF DIP。前置令牌为可移植格式写作 `px`，在此项目中按同数值 DIP 理解；不将它们视为截图中的物理像素。默认窗口居中，大小（800×600 DIP），最小（720×500 DIP）；最小尺寸是代码约束，尚不代表该尺寸已独立视觉验收。没有移动端断点。

| 区域 | 当前实现 |
| --- | --- |
| 外围容器 | 四边边距（12 DIP）；行高为（42 / 剩余空间 / 56 DIP）。 |
| 顶部 | 窗口标题栏之外的分区标签，边距（左右12、上下8 DIP）；底部分隔线（1 DIP）。当前没有导航分页。 |
| 主区域 | 距顶部（18 DIP）；列宽为（240 / 18 / 剩余空间 DIP）。 |
| 任务列表 | 任务行内边距（12 DIP），状态距名称（6 DIP）；恢复行距上一行（8 DIP），初始折叠。 |
| 添加任务 | 距任务行（12 DIP），按钮高（34 DIP）；加号尺寸（12×12 DIP）、笔画（1.5 DIP）。按钮宽度由主题布局决定。 |
| 当前步骤与日志 | 当前步骤下边距（14 DIP）；日志占剩余高度，内边距（12 DIP），自动换行、垂直滚动。 |
| 底部 | 上边距（12 DIP）；停止按钮宽（150 DIP），重试按钮宽（100 DIP），重试左边距（12 DIP）；说明文字靠右居中。 |

左侧默认只有「通过 MAA 新手教程」；「＋」菜单追加「恢复原主题」。右侧持续显示当前步骤和日志。底部提供「停止（F8）」与「重试」，启动自动执行默认任务。

## Elevation & Depth

任务行以窗口底色和细边框划分；日志面板用轻微色差形成阅读层级。项目没有声明自定义阴影、发光、过渡或动画令牌；窗口、按钮、菜单及键盘焦点的主题效果由 HandyControl 提供，不把截图中的主题效果推广成新的自定义阴影系统。

## Shapes

任务行使用轻微圆角（`rounded.task-row`）与细边框（1 DIP）。按钮、日志控件、窗口和菜单的圆角及状态样式继承当前主题，不能将任务行圆角机械地套用到所有控件。

## Components

### 任务行

紧凑双行内容：第一行任务名，第二行低对比度状态。恢复行初始隐藏；添加后显示，且加号禁用以防重复添加。任务已完成时保留行和结果。

### 按钮与菜单

停止和重试均使用 HandyControl 默认深色按钮；没有另造主按钮变体。运行中启用停止、禁用重试；退出运行后禁用停止、启用重试。加号用内联路径绘制，点击打开单项原生上下文菜单。悬停、按下、禁用与键盘焦点外观继续继承主题，项目未覆盖其颜色和动画参数。

### 日志与当前步骤

日志为只读 `TextBox`，按时间追加、自动滚至末尾，并允许垂直滚动和文字选择。当前步骤可换行，不依赖颜色单独传递状态。底部保留「MaaFramework · 本地运行」说明。

### 状态反馈

窗口寻找、记录主题、切换深色、教程执行、恢复与异常均写入日志。停止主状态为「已停止」，未完成的教程行显示「已停止，可重试」；失败主状态为「需要处理后重试」，具体原因留在日志。

教程完成后显示「教程已完成」；恢复完成后显示「教程完成，原主题已恢复」，恢复行显示「已恢复」。精确原选项（包括「与系统同步」）在日志中显示，不把恢复行误记为包含该选项。

`.impeccable/design.json` 提供自包含 HTML/CSS 文档预览；前置令牌记录源码明确值。sidecar 的八阶 OKLCH 色带仅为文档面板合成预览，不是项目已使用的主题色阶。预览对原生按钮与菜单的翻译不承担 HandyControl 像素或交互一致性证明，真实 WPF 截图是视觉验收依据。

## Do's and Don'ts

### Do

- **Do** 保留原生深色任务面板、浅色正文与稀疏蓝色强调。
- **Do** 以任务行、当前步骤和可滚动日志同时呈现进度。
- **Do** 保留继承的原生控件状态与键盘焦点，新增覆写前核查 HandyControl 资源。
- **Do** 分别记录功能验收和真实 Windows 截图的视觉验收范围。

### Don't

- **Don't** 使用 MAA Logo、复制其 XAML 或宣称官方关系。
- **Don't** 添加无真实功能的分页，或把轻量两任务范围扩成新的工作台。
- **Don't** 把单张完成态截图推广为所有运行状态、DPI、语言与版本均已视觉通过。

### 验证要求

完成态实际截图为 `.impeccable/review/windows.png`。独立视觉审阅对默认（800×600 DIP）Windows 完成态给出可交付结论，无实质修复项；本机为 Windows 11 build 22631、144 DPI。运行中、停止与错误态没有获得该审阅者的视觉结论，其他缩放、语言和版本也不在此结论内。

保持与实际 MAA 截图比较布局、配色、控件和窗口缩放的验收要求；后续扩展状态或环境时补对应真实截图。功能运行证据见 `PRODUCT.md` 与项目状态记录，不能由 HTML 文档预览代替。
