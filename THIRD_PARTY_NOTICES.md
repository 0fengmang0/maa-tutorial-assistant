# 第三方组件与分发边界

本助手的原创源码使用 MIT 许可证。它是独立程序，通过 MaaFramework 操作另一个未修改的 MAA 进程；未复制 MAA 源码、XAML、Logo 或游戏素材，不代表 MAA 官方。

| 组件 | 固定版本/来源 | 许可证 |
| --- | --- | --- |
| MaaFramework Windows x64 | 5.10.5，提交 `13ff95cdfa2b20fa6bdd361779278115bc03c0b4` | LGPL-3.0-only |
| MaaFramework C# Binding | 5.10.0，提交 `27c69a5b8ff41b6002ead71f403a16f442a7168e` | LGPL-3.0-only |
| HandyControl | 3.5.1 | MIT |
| MaaCommonAssets 中文 PP-OCRv4 | 提交 `dabcd4681ac990dc4361de26416d986abd80e4aa` | 仓库 MIT；模型为 PP-OCRv4 衍生资源 |
| .NET Windows Desktop | 用户系统的 .NET 8 运行时，不随本构建打包 | Microsoft/.NET 相应许可证 |

许可证正文见 `licenses/`，模型哈希见 `docs/dependency-models.json`。MaaFramework 与 Binding 使用独立 DLL 动态加载，不限制用户替换接口兼容的修改版本，也不限制为调试库修改而进行逆向工程。

源码与构建依据：

- [MaaFramework 对应源码](https://github.com/MaaXYZ/MaaFramework/tree/13ff95cdfa2b20fa6bdd361779278115bc03c0b4)，[构建说明](https://github.com/MaaXYZ/MaaFramework/blob/v5.10.5/docs/zh_cn/2.1-集成文档.md)。
- [Binding 对应源码](https://github.com/MaaXYZ/MaaFramework.Binding.CSharp/tree/27c69a5b8ff41b6002ead71f403a16f442a7168e)。
- [HandyControl](https://github.com/HandyOrg/HandyControl/tree/v3.5.1)。
- [识别模型](https://github.com/MaaXYZ/MaaCommonAssets/tree/dabcd4681ac990dc4361de26416d986abd80e4aa/OCR/ppocr_v4/zh_cn)。

当前交付是本地开发验证版本，未上传或公开发布。对外分发包含的原生库还带有 OpenCV、ONNX Runtime、DirectML、FastDeploy 等传递组件；正式发布前须按选定二进制发行物补齐其完整许可证/NOTICE 和对应源码提供方式，再打包发布。这份清单不宣称已完成公众分发合规审计。

如果后续复制或修改 MAA 本身的 AGPL-3.0-only 源码，应重新评估衍生作品的许可与对应源码义务；不能仅保留本助手 MIT 文件。自动操作教程这一用途本身不是本清单列出的禁止条款。MAA 的在线服务和第三方素材仍受各自条款约束。
