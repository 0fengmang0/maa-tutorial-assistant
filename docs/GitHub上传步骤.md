# GitHub 上传步骤

本轮交付为已公开上传的**源码仓库**和本地源码 ZIP。源码包含原创 MIT 许可证、依赖声明、固定版本/模型哈希及构建脚本；没有把本机 MAA 安装、用户配置、主题恢复记录、日志、截图、开发 SDK 或二进制运行库上传。

2026-11-04 已按用户授权创建公开仓库 <https://github.com/0fengmang0/maa-tutorial-assistant>，配置 `origin` 并推送 `main`。本机不需要重复创建仓库或添加远程，可直接从下方「后续更新」继续；首次发布步骤供重建或参考。未创建二进制 Release。

正式项目目录：

`D:\AI_work_space\project_next\maa-tutorial-assistant`

准备好的源码包：

`releases\github\maa-tutorial-assistant-source.zip`

同目录 `SHA256SUMS.txt` 是 ZIP 的校验值，`SOURCE_INFO.txt` 记录精确源码提交。这三个文件一起保留。ZIP 不含 `.git`；如果直接从 ZIP 开始上传，Git 历史从新仓库首次提交开始。

## 推荐方式：把当前本地仓库推送到 GitHub

### 1. 新建一个空仓库

1. 登录 GitHub，打开 <https://github.com/new>。
2. Owner 选择你的账号。
3. Repository name 建议填 `maa-tutorial-assistant`。
4. Description 可填：`基于 MaaFramework 的 Windows MAA 新手教程助手，提供主题恢复与三栏任务界面。`
5. 选择 Public（公开）或 Private（私有）。
6. **不要**勾选自动添加 README、`.gitignore` 或 License：本地已有这些文件，保持远程为空可直接推送。
7. 点击 **Create repository**。
8. 复制新仓库的 HTTPS 地址，本项目为 `https://github.com/0fengmang0/maa-tutorial-assistant.git`。

### 2. 打开 PowerShell，进入正式项目目录

```powershell
Set-Location 'D:\AI_work_space\project_next\maa-tutorial-assistant'
git status --short
git branch --show-current
git log -1 --oneline
git remote -v
```

当前主分支为 `main`，`git status --short` 应为空。`origin` 已指向 `https://github.com/0fengmang0/maa-tutorial-assistant.git`；先确认地址，不要重复添加或盲目覆盖。

### 3. 添加你的远程地址并上传

以下只适用于尚未配置远程的新副本；本机已经执行完成，后续直接 `git push`。其他用户请替换账号和仓库名。

```powershell
git remote add origin https://github.com/0fengmang0/maa-tutorial-assistant.git
git push -u origin main
```

首次推送时按 Git Credential Manager 或 GitHub 的浏览器登录流程认证。GitHub 不接受账户密码作为 HTTPS Git 密码；不需要把令牌写入仓库 URL、源码或发给其他人。使用 GitHub Desktop 登录后也可以完成同一推送。

不要运行 `git add -A`，不要上传 `dependencies/`、`releases/`、`test-environments/` 或 `data/`。本次正式源码已经提交，执行上述推送即可。

### 4. 在 GitHub 上检查结果

刷新仓库页面，确认：

- 默认分支为 `main`。
- 首页显示中文 README。
- `src/`、`scripts/`、`docs/`、`licenses/`、`LICENSE`、`THIRD_PARTY_NOTICES.md` 均存在。
- 没有本机配置、隔离 MAA、副本日志、SDK、主题记录和 EXE/DLL。
- 仓库简介写明独立项目，不使用 MAA 官方 Logo，也不暗示官方出品。

### 5. 后续更新

只暂存你本次修改的正式文件。例如：

```powershell
git add README.md src/MainWindow.xaml src/MainWindow.xaml.cs
git commit -m "Update tutorial assistant interface"
git push
```

按实际修改调整文件列表。不要强制推送，也不要把他人的未提交修改合入自己的更新。

## 不使用 Git 命令：网页上传源码包中的文件

1. 把 `releases\github\maa-tutorial-assistant-source.zip` 解压到一个新目录。
2. 打开解压后的 `maa-tutorial-assistant` 文件夹。
3. 按前文新建空 GitHub 仓库。
4. 在仓库的 **uploading an existing file** 或 **Add file → Upload files** 中，拖入该文件夹**内部的文件和目录**。
5. 也带上 `.gitignore`、`.impeccable` 这些以点开头的正式项目文件；文件管理器中看不到时开启显示隐藏项目。
6. 检查网页待上传列表，提交说明填写 `Initial source release`，点击 **Commit changes**。
7. 检查仓库首页出现 `src/` 和 README。如果只看到一个 ZIP，说明你上传了压缩包本身；源码应先解压再上传，ZIP 可另作为 Release 附件。

这种方式没有保留本地历史；后续可以克隆网页仓库继续开发。不要随后把当前本地已有的另一段历史强制覆盖过去。

## 可选：创建一个源码 Release

1. 源码上传成功后，在仓库右侧 **Releases → Create a new release**。
2. 新建版本标签，例如 `v0.1.0-preview`，目标选择 `main`。
3. 标题可填 `MAA 教程助手 · 源码预览版`。
4. 发布说明写明：Windows x64、需要 .NET 8 Windows Desktop Runtime、自动化当前只验证 v6.21.3 简体中文、打开后手动点击开始。
5. 可以添加准备好的源码 ZIP、`SHA256SUMS.txt` 和 `SOURCE_INFO.txt`。GitHub 也会自动提供标签的 Source code 压缩包。
6. 预览版可勾选 **Set as a pre-release**，检查内容后自行点击 **Publish release**。

**本次源码包不包含可公开分发的 EXE 安装包。** `releases/current` 是已在本机运行的程序目录；其原生传递依赖的完整 NOTICE、许可证和对应源码提供方式仍有发布前事项，见 `THIRD_PARTY_NOTICES.md`。完成这些事项后再单独制作、上传二进制 Release；不要把整个本机运行目录或 MAA 安装一并上传。

## 源码包重建与校验

正式源码提交后，在项目根目录执行：

```powershell
pwsh -NoProfile -File .\scripts\Package-Source.ps1
Get-FileHash .\releases\github\maa-tutorial-assistant-source.zip -Algorithm SHA256
Get-Content .\releases\github\SHA256SUMS.txt
Get-Content .\releases\github\SOURCE_INFO.txt
```

脚本只打包当前 `HEAD` 已提交内容，工作区有正式未提交文件时会拒绝生成，避免漏包；不会自动提交、添加远程或推送。

## 其他电脑从源码构建

需要 Windows x64、Git 和 PowerShell 7。构建脚本会准备项目内固定版本 SDK、NuGet 依赖和 OCR 模型：

```powershell
git clone https://github.com/0fengmang0/maa-tutorial-assistant.git
Set-Location .\maa-tutorial-assistant
pwsh -NoProfile -File .\scripts\Build.ps1 -ForceRestore
```

默认构建缓存位于系统临时目录的 `maa-tutorial-assistant-build`，可用 `-Scratch '你的缓存目录'` 指定；不要求存在 `D:` 盘。运行产物位于 `releases/current`，运行还需要 .NET 8 Windows Desktop Runtime x64，必须保留整个目录。

若继续在本工作区开发，请遵从本工作区规则显式使用集中临时目录，例如：

```powershell
pwsh -NoProfile -File .\scripts\Build.ps1 -Scratch 'D:\AI_work_space\tmp\agent-scratch\你的本次任务目录' -ForceRestore
```

源码仓库公开性、默认分支和远端提交通过 GitHub API 核查；这不表示已进行跨电脑启动验证。当前账号为 `0fengmang0`，仓库地址见本文开头。
