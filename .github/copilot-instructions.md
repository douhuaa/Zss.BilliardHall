# Copilot Instructions for Zss.BilliardHall

## 项目概览
# Copilot Instructions for Zss.BilliardHall

目的：帮助 AI 代理快速定位本仓库的关键点并安全、可重复地执行常见 .NET 开发任务。

## 快速检查列表
- 这是一个 Visual Studio/.NET 项目（见 `.gitignore` 的条目）。
- 常用命令：`dotnet restore`、`dotnet build`、`dotnet test`。
- 若仓库使用 ABP 框架（未在本仓库中自动检测到），参见下方“ABP 专用提示”。

## 项目高层（大图）
- 构建系统：MSBuild（可用 Visual Studio 或 `dotnet build`）。
- 测试：MSTest（测试输出 `TestResult.xml` 被忽略）。
- 依赖：通过 NuGet 管理，建议在 CI 前运行 `dotnet restore`。

## 开发 / 调试 / CI 工作流
- 本地编译：

```powershell
dotnet restore
dotnet build -c Debug
```

- 运行测试：

```powershell
dotnet test
```

- CI 注意：仓库可能配置为在 TeamCity 上运行；避免提交 TestResult、bin/obj 等产物（参考 `.gitignore`）。

## 项目约定（从仓库可发现的模式）
- 源码与测试分离，测试产物不入库。
- `Directory.Build.rsp` 用于全局 MSBuild 设置（不会被忽略，仓库保留）。
- 若存在自定义构建/辅助脚本，请优先检查根目录下的 `build/`、`scripts/` 或 `.ps1` 文件。

## 如何检测是否为 ABP 框架项目（必要时）
AI 代理在对仓库做大改动前，应确认是否使用了 ABP 框架。可以通过下列快速搜索判断：

```powershell
# 在仓库根目录运行（PowerShell）
Select-String -Path "**/*.csproj","**/*.cs" -Pattern "Volo.Abp|ApplicationModule|AbpModule|Volo" -SimpleMatch -List
```

如果输出包含 `PackageReference Include="Volo.Abp`、或类名以 `*ApplicationModule` 结尾，说明仓库基于 ABP。

## 若仓库使用 ABP：简要指南
- 首先安装 ABP CLI（仅当你需要使用 CLI 时）：

```powershell
dotnet tool install -g Volo.Abp.Cli
# 或更新
dotnet tool update -g Volo.Abp.Cli
```

- 常用检查命令：

```powershell
abp --help
abp -v
```

- 参考：ABP CLI 文档 https://abp.io/docs/latest/cli （在使用 CLI 执行生成/迁移前请先在本地或 CI 环境备份/分支）

## 安全与变更策略
- 任何对项目结构或全局配置（如 `Directory.Build.rsp`、NuGet 源、CI 配置）的更改，都应在单独分支并附带简短说明。
- 对数据库迁移、模板生成（若为 ABP）或批量替换类名的操作，先创建小型验证 PR，确保 CI 和测试通过。

## 需要补充的信息（给维护者的快速请求）
请提供下列信息以便完善本指南：
- 实际的源码目录（例如 `src/`、`services/`、`tests/`）
- 是否使用 ABP（是/否），若是，主要模块名或根模块类的位置
- 是否有自定义构建脚本或特殊 CI 流程

---
如果你希望我把 ABP 相关的更多自动化步骤写入本文件（例如常用 `abp` 子命令的模板），或根据仓库实际文件把文件里“检测”一节自动化，我可以继续扫描并更新文档。
