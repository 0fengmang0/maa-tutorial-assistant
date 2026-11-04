---
name: MAA 教程助手
description: 延续 MAA 深色 WPF 任务面板的 Windows 本地助手
colors:
  accent: "#008DDB"
  tab-selected: "#009DEB"
  window: "#1C1C1C"
  text: "#EEEEEE"
  border: "#414146"
  text-muted: "#B3B3B3"
  control-border: "#303034"
  hover: "#262626"
  button-hover: "#292929"
  button-pressed: "#333333"
  icon-muted: "#888888"
typography:
  body:
    fontFamily: "Microsoft YaHei UI, Segoe UI"
    fontSize: "13px"
  help:
    fontSize: "13px"
    lineHeight: "21px"
  status:
    fontSize: "13px"
    lineHeight: "20px"
rounded:
  task-selector: "2px"
  button: "3px"
  panel: "4px"
spacing:
  compact: "4px"
  inset: "8px"
  task-inset: "10px"
  section-gap: "12px"
  column-gap: "40px"
components:
  task-selector:
    backgroundColor: "transparent"
    textColor: "{colors.text}"
    rounded: "{rounded.task-selector}"
    padding: "0 4px"
    height: "32px"
  button-flat:
    backgroundColor: "transparent"
    textColor: "{colors.text-muted}"
    rounded: "{rounded.button}"
    padding: "4px 8px"
  button-flat-hover:
    backgroundColor: "{colors.button-hover}"
  button-flat-pressed:
    backgroundColor: "{colors.button-pressed}"
  run-button:
    backgroundColor: "transparent"
    textColor: "{colors.text-muted}"
    rounded: "{rounded.button}"
    padding: "4px 8px"
    width: "108px"
    height: "52px"
  field-readonly:
    backgroundColor: "{colors.window}"
    textColor: "{colors.text-muted}"
    padding: "6px 8px"
  segment:
    backgroundColor: "transparent"
    textColor: "{colors.text}"
    padding: "2px 6px"
    height: "30px"
  segment-selected:
    textColor: "{colors.tab-selected}"
  log-view:
    backgroundColor: "transparent"
    textColor: "{colors.text-muted}"
    padding: "{spacing.inset}"
  status-panel:
    backgroundColor: "transparent"
    textColor: "{colors.text-muted}"
    typography: "{typography.status}"
    rounded: "{rounded.panel}"
    padding: "{spacing.inset}"
---

# Design System: MAA 教程助手

## Overview

**Creative North Star: "MAA 深色任务面板"**

继承用户指定 MAA 截图的原生深色桌面界面。深灰底色、细边框、蓝色强调和紧凑任务布局服务于本地任务的选择、说明阅读和运行反馈；本轮在既有视觉世界中将主窗口改为三栏。

实际实现使用 Windows WPF 与 HandyControl 3.5.1，加载其 `SkinDark.xaml` 和 `Theme.xaml`。规范以 `src/MainWindow.xaml`、`src/MainWindow.xaml.cs` 和 `src/App.cs` 的当前实现为依据；未覆写的窗口、复选框、菜单及文本框细节仍继承当前原生主题。没有新字体资产、MAA Logo 或官方代码复用。

**Key Characteristics:**

- 深色平面容器，以细灰边框划分任务、字段、日志与状态。
- 蓝色用于选中齿轮、分段标签与键盘焦点，运行结果由文字表达。
- 紧凑中文桌面文字，围绕两项真实任务组织选择、详情和日志。

## Colors

强调色保持清晰的 MAA 蓝，主体依靠深灰与浅灰建立阅读层次；精确值以 YAML 令牌为准。

### Primary

- `accent`：选中任务的齿轮、选中分段边框和自定义按钮、任务选择器的键盘焦点边框。
- `tab-selected`：选中「常规设置 / 高级设置」与「运行日志 / 当前任务」的文字。

### Neutral

- `window` / `text`：窗口与只读字段底色，以及正文文字。
- `border`：任务容器、分段标签、日志和状态框的细边界。
- `text-muted`：字段值、日志、状态和扁平按钮文字。
- `control-border`：只读字段和扁平按钮的低对比度边界。
- `hover`：任务选择器和分段标签的悬停底色。
- `button-hover` / `button-pressed`：扁平按钮的悬停与按下底色。
- `icon-muted`：未选中齿轮与圆圈加号的笔画。

**The Restrained Accent Rule.** 用文字表达完成、停止与失败；不要给每种运行状态新增一套强调色。

## Typography

窗口明确声明系统字体序列 `Microsoft YaHei UI, Segoe UI` 和正文（13 DIP）；没有独立展示字体或打包字体文件。字体字重、默认行高和未覆写控件细节继续继承 WPF / HandyControl。任务名称、字段、按钮和分段标签沿用正文大小，不再使用旧版大号当前步骤标题。

- `body`：所有主要控件的基础文字。
- `help`：中间任务说明，明确行高，允许换行和垂直滚动。
- `status`：右侧主状态，明确行高并允许换行。

**The Native Density Rule.** 保持系统中文桌面文字的紧凑密度，用区域、留白和状态表达层级，不以装饰标题扩大界面。

## Layout

以下尺寸为 WPF DIP。YAML 和 HTML 文档预览中的 `px` 按同数值 DIP 映射，不代表屏幕截图中的物理像素。平台为 Windows 原生 WPF；没有 Web、iOS、Android 布局或移动端断点。

默认窗口居中，大小（960×680 DIP），最小（860×560 DIP）。最小尺寸是代码约束，本轮没有该尺寸的独立视觉验收。

| 区域 | 当前实现 |
| --- | --- |
| 外围容器 | 左、上、右、下边距为（36 / 20 / 20 / 24 DIP）。 |
| 三栏 | 左侧任务列（220 DIP），中间详情列（240 DIP），右侧日志列使用剩余宽度；列间各（40 DIP）。 |
| 左侧任务容器 | 细边框、轻圆角；内部左、上、右、下边距为（10 / 12 / 10 / 10 DIP）。任务区占剩余高度，底部操作行高（32 DIP）。 |
| 任务行 | 勾选列宽（24 DIP），任务选择器高（32 DIP），齿轮列宽（24 DIP），齿轮（18×18 DIP）。 |
| 左下操作 | 容器底部为圆圈加号、全选和清空；容器外保留「完成后 / 无动作」，开始按钮位于左下（108×52 DIP）。 |
| 中间详情 | 上部为可垂直滚动的只读字段；分段标签高（30 DIP）；下部说明区高（180 DIP），长说明独立滚动。 |
| 右侧日志 | 顶部分段标签与日志相距（5 DIP）；日志内边距（8 DIP），文本区域最小高（98 DIP）、最大高（350 DIP），超过时垂直滚动。 |
| 右侧状态 | 独立边框状态框位于日志下方，相距（4 DIP），内边距（8 DIP），文字允许换行。 |

**The Three Regions Rule.** 任务选择留在左侧，所选任务详情留在中间，日志和主状态留在右侧；切换任务不改变三个区域的位置。

## Elevation & Depth

界面用平面底色、细描边和留白区分层次。日志与状态使用透明底色继承窗口，不再保留旧版独立日志底色。项目没有声明自定义阴影、发光、过渡或动画令牌；窗口、复选框和菜单的主题效果仍由 HandyControl 提供，不将其推广为新的自定义阴影系统。

## Shapes

任务选择器、扁平按钮和面板分别使用轻微圆角（`rounded.task-selector`、`rounded.button`、`rounded.panel`）。常态描边为（1 DIP），分段标签键盘聚焦时描边变为（2 DIP）。齿轮与圆圈加号是 XAML 几何图形，不使用图标字体或 Logo。只读字段、窗口和菜单未覆写的形状继续继承原生主题。

## Components

### 任务行与添加菜单

复选框决定是否执行，任务名称与齿轮组成独立的详情选择器。选中任务的齿轮填充蓝色；悬停显示轻微灰色底，键盘聚焦显示蓝色边框。恢复行初始折叠；「＋」菜单添加后显示、勾选并选中恢复任务，同时禁用该菜单项以防重复添加，加号按钮本身仍可打开菜单。

「全选」勾选已存在的任务；「清空」只取消勾选，不移除任务行。运行中禁用勾选框、全选和清空，详情选择和添加尚未存在的恢复任务仍可用。

### 扁平按钮与开始 / 停止

使用透明常态底、低对比度边框和次级文字。悬停、按下依次使用两个灰色底色；键盘聚焦使用蓝色边框；禁用时整体不透明度为（0.45）。该自定义模板应用于加号、全选、清空和开始 / 停止。

打开后等待，左下显示「开始」。至少一项任务已勾选时可开始；清空全部勾选后禁用。运行中同一按钮显示「停止」，也可按 F8 中断；请求停止时显示「正在停止」并暂时禁用。停止或异常后恢复为「开始」，继续入口不再是独立重试按钮。空闲时添加恢复任务后等待开始；运行中添加则排在教程之后。

### 只读详情与说明分段

中间字段用标签和可换行只读文本框展示任务事实与状态，不暗示可编辑。教程详情包含目标窗口、语言、运行主题、任务状态和完成后主题；恢复详情包含主题来源、恢复选项、执行顺序和状态。

「常规设置 / 高级设置」实际切换所选任务的说明文字，不提供额外参数编辑。选中标签文字与边框变蓝，悬停使用轻灰底，键盘聚焦加粗描边。

### 日志与主状态

「运行日志」显示全部运行记录；「当前任务」按当前所选任务筛选，切换任务时同步更新。日志是只读文本框，按时间记录、自动滚至末尾，允许文字选择、换行和垂直滚动。没有匹配记录时显示「当前任务暂无日志。点击开始后在此显示执行记录。」

主状态独立置于日志下方，初始为「等待开始」。停止后显示「已停止，点击开始可继续」，异常后显示「需要处理后重新开始」，原因留在日志。教程完成和主题恢复分别显示对应完成文字；精确原主题选项保留于日志与恢复说明。

### 文档预览与验证边界

`.impeccable/design.json` 提供自包含 HTML/CSS 组件预览；它翻译当前 XAML 的明确值，不是运行中的 WPF 控件，也不证明 HandyControl 的像素或交互一致性。八阶 OKLCH 色带仅是文档面板的合成预览，不是新增运行主题。

本轮已编译并实际启动，界面操作读取到加号恢复行、任务详情、高级说明、当前任务日志筛选和清空后开始禁用。用户确认当前功能完整并要求收尾，随后用 Esc 停止界面操作；本轮以用户验收结束继续界面审阅，没有留存新版截图、没有新增自动化回归，也没有新版独立视觉审阅的可交付结论。

历史 `.impeccable/review/windows.png` 是旧版双栏（800×600 DIP）完成态，只能连同旧版独立视觉审阅作为历史记录。它不证明当前三栏、运行中、停止、错误、最小尺寸或其他 DPI 的视觉结果。本机历史验证环境为 Windows 11 build 22631、144 DPI；自动化核心仍只验证 MAA v6.21.3，不能推定 v6.21.4 支持。功能证据的具体边界见 `PRODUCT.md` 与项目状态文档。

## Do's and Don'ts

### Do

- **Do** 保留原生深色任务面板、浅色正文与稀疏蓝色强调。
- **Do** 保留任务勾选与详情选择的不同职责，以及详情和当前任务日志的同步切换。
- **Do** 保留可滚动说明、可选择日志和自定义控件的键盘焦点反馈。
- **Do** 分别记录功能验收、用户界面验收和独立视觉审阅的实际覆盖范围。

### Don't

- **Don't** 使用 MAA Logo、复制其官方代码或宣称官方关系。
- **Don't** 添加无真实功能的分页，或把轻量两任务范围扩成新的工作台。
- **Don't** 把旧版截图、HTML 预览或用户收尾确认写成新版独立视觉审阅结论。
- **Don't** 将单一环境的结果推广为所有运行状态、DPI、语言或 MAA 版本均已通过。
