# ABP 专用指令（供 AI 代理和维护者）

目的：为可能使用 ABP 框架的仓库提供可执行、可验的检测与操作步骤，包含常见 CLI / 数据库迁移 / 生成器使用的安全指南。

## 快速清单（开始前）

- [ ] 先检测仓库是否使用 ABP（见“检测”小节）。
- [ ] 在进行代码生成或迁移前创建分支并确保 CI 能通过（见“安全策略”）。
- [ ] 若要运行迁移或生成器，确保本地有正确的 .NET SDK、数据库凭据与环境配置。

## 一、如何检测仓库是否基于 ABP

- 在仓库根目录（PowerShell）运行：

```powershell
# 查找 ABP 相关包或代码标识
Select-String -Path "**/*.csproj","**/*.cs" -Pattern "Volo.Abp|ApplicationModule|AbpModule|Volo" -SimpleMatch -List
```

- 常见判断依据：
  - `.csproj` 中出现 `PackageReference Include="Volo.Abp`。
  - 任意 `.cs` 文件中存在类继承自 `AbpModule` 或命名包含 `ApplicationModule`。

如果没有这些痕迹，则项目很可能不是 ABP 项目——在此情况下仅按普通 .NET 指南操作。

## 二、安装 ABP CLI（可选，在需要使用生成/模版时）

- 安装或更新 ABP CLI：

```powershell
dotnet tool install -g Volo.Abp.Cli
# 或（已安装时更新）
dotnet tool update -g Volo.Abp.Cli
```

- 验证：

```powershell
abp --help
abp -v
```

- 备注：仅在需要执行 ABP 专用生成/迁移/模板时安装 CLI；普通开发不依赖全局 CLI。

## 三、常见安全与操作守则

- 任何自动化生成、模板变更或数据库迁移前务必：
  1. 新建分支（例如 `feature/abp-migration-xxx`）并推送。
  2. 在本地或临时环境运行迁移，确认无副作用。
  3. 提交前运行 `dotnet build` 和受影响项目的 `dotnet test`。
- 避免直接在主分支或远端生产环境运行自动迁移脚本。

## 四、EF Core 迁移（常见于 ABP 项目）

- ABP 项目通常把迁移放在单独的 DbMigrator 或 *EntityFrameworkCore 项目中。
- 增加迁移示例（替换占位符）：

```powershell
# -p 指定迁移项目（包含 Migrations 的项目），-s 指定启动项目（host / DbMigrator）
dotnet ef migrations add InitialCreate -p src/YourProject.EntityFrameworkCore -s src/YourProject.DbMigrator

# 将迁移应用到数据库
dotnet ef database update -p src/YourProject.EntityFrameworkCore -s src/YourProject.DbMigrator
```

- 如果出现多个 startup，请在 `-s` 指定正确的启动项目并确保该启动项目能读取到正确的连接字符串（通常在 `appsettings.Development.json` 或 `appsettings.json`）。

## 五、常见 ABP CLI 注意点（谨慎使用）

- ABP CLI 提供模板生成 / 模块添加等功能，但不同 ABP 版本命令细节可能变化。始终先运行 `abp --help` 并阅读官方文档：<https://abp.io/docs/latest/cli>
- 在使用任何 `abp` 生成命令前：备份/分支 + 本地验证 + 查看生成变更的 diff。

## 六、代码结构与约定（可在仓库中查找的典型位置）

- 模块类：通常在 `*.Application`、`*.Domain`、`*.EntityFrameworkCore` 等项目中，可查找继承自 `AbpModule` 的类。
- 数据迁移：`*.EntityFrameworkCore/Migrations` 或 `DbMigrator` 项目。
- 配置：`appsettings.json` / `appsettings.Development.json` 存放连接字符串与环境配置。
- 全局 MSBuild 设置：`Directory.Build.rsp`（若存在，请勿随意修改）。

## 七、示例工作流（生成 + 迁移 + 验证）

- 检测并确认：运行检测命令，确认 ABP 包存在。
- 新建分支：

```powershell
git checkout -b feature/abp-add-migration
```

- 安装/更新 ABP CLI（如需）：

```powershell
dotnet tool update -g Volo.Abp.Cli || dotnet tool install -g Volo.Abp.Cli
```

- 添加迁移（示例占位符见上文）。
- 在本地 Db 环境运行 `dotnet ef database update`，验证表与种子数据。
- 运行受影响项目的单元/集成测试。
- 提交并发起 PR，CI 验证通过后合并。

## 八、常见问题与排查要点

- "找不到启动项目"：确认 `-s` 指定的项目存在且能被编译。
- 迁移后应用不生效：检查连接字符串、环境文件加载顺序，或迁移程序集是否正确。
- 版本不兼容：ABP/EFCore/SDK 版本不一致时可能出现运行时错误，优先查看 `*.csproj` 中的包版本。

## 九、可选自动化建议（小步试验）

- 可在 CI 中添加一个只读检查任务：在分支上运行 `dotnet build` + `dotnet test`，以及一个快速 `dotnet ef migrations script`（仅生成 SQL，不执行），以提前发现迁移冲突。

---

如果你希望我把某些常用 ABP CLI 子命令模板（例如模块生成、代理生成、微服务模板）写入本文件，请告诉我你使用的 ABP 版本或提供仓库内的 `*.csproj` 示例，我会据此补充精确的命令模板。
