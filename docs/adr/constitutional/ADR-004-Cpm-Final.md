---
adr: ADR-004
title: "中央包管理（CPM）规范"
status: Final
level: Constitutional
deciders: "Architecture Board"
date: 2026-02-04
version: "3.0"
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---


# ADR-004：中央包管理（CPM）规范

> ⚖️ **本 ADR 是架构宪法的核心，定义中央包管理的唯一裁决源。**

---

## Focus（聚焦内容）

仅定义适用于全生命周期自动化裁决/阻断的**包管理约束**：

- 所有依赖包通过 Directory.Packages.props 集中管理
- 层级依赖规则细化：Platform、Application、Modules、Host 各自边界
- 禁止项目文件手动指定包版本
- 所有规则必须架构测试覆盖

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|--------------------------|----------------------------------|-----------------------|
| CPM                      | Central Package Management，中央包管理 | CPM                   |
| Directory.Packages.props | NuGet 配置文件，集中定义全局依赖包版本           | Directory.Packages.props |
| 传递依赖固定                   | 通过 CPM 禁止传递依赖的漂移                 | Transitive Dependency Lock |
| 层级依赖                     | 不同层项目允许的包类型约束                    | Layered Dependencies  |
| 包分组                      | 依赖包按技术栈、场景分隔分组                   | Package Grouping      |

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-004 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-004_<Rule>_<Clause>
> ```

---

### ADR-004.1：CPM 基础设施约束（Rule）

#### ADR-004.1.1 Directory.Packages.props 必须存在

- 仓库根目录必须包含 Directory.Packages.props 文件
- 该文件是启用 Central Package Management (CPM) 的基础

**判定**：
- ❌ 仓库根目录缺少 Directory.Packages.props 文件
- ✅ Directory.Packages.props 文件存在

#### ADR-004.1.2 CPM 必须启用

- Directory.Packages.props 必须包含 `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`
- 启用 CPM 确保所有包版本集中管理

**判定**：
- ❌ 未设置 ManagePackageVersionsCentrally
- ❌ ManagePackageVersionsCentrally 设置为 false
- ✅ ManagePackageVersionsCentrally 设置为 true

#### ADR-004.1.3 传递依赖固定建议启用

- 建议启用 `<CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>`
- 固定传递依赖版本，避免间接依赖升级导致的破坏性变更
- 此条款为建议性质，不作为强制要求

**判定**：
- ⚠️ 未启用传递依赖固定（建议启用）
- ✅ 已启用传递依赖固定

---

### ADR-004.2：项目依赖管理约束（Rule）

#### ADR-004.2.1 项目文件禁止手动指定包版本

- 项目文件（.csproj）中的 PackageReference 不得包含 Version 属性
- 所有包版本必须在 Directory.Packages.props 中定义
- 禁止使用 VersionOverride 覆盖中央包版本

**判定**：
- ❌ 项目文件中存在 `<PackageReference Include="..." Version="..." />`
- ❌ 项目文件中存在 `<PackageReference Update="..." VersionOverride="..." />`
- ✅ 所有项目文件不包含包版本信息

#### ADR-004.2.2 所有使用的包必须在 CPM 中定义

- Directory.Packages.props 必须定义所有项目引用的包
- 不允许存在未在 CPM 中声明的包引用

**判定**：
- ❌ 项目引用的包未在 Directory.Packages.props 中定义
- ✅ 所有包引用都在 CPM 中有定义

---

### ADR-004.3：层级依赖与分组约束（Rule）

#### ADR-004.3.1 包应按功能分组

- Directory.Packages.props 中应使用 `<ItemGroup Label="分组名称">` 对包进行逻辑分组
- 常见分组包括：Logging、Testing、Wolverine Framework、Marten、Aspire 等
- 此条款为建议性质，有助于快速定位和管理相关包

**判定**：
- ⚠️ 未使用 Label 属性进行包分组（建议使用）
- ✅ 已使用 Label 属性进行包分组

#### ADR-004.3.2 Platform 项目不引用业务包

- Platform 层项目只能引用技术基础包（如 Serilog、OpenTelemetry、HealthChecks）
- 禁止引用业务相关的 NuGet 包（如 FluentValidation、MediatR）
- Platform 层定位为技术底座，不包含业务逻辑

**判定**：
- ❌ Platform 项目引用了业务包
- ✅ Platform 项目仅引用技术基础包

#### ADR-004.3.3 测试框架版本统一

- 所有测试项目必须使用相同版本的测试框架（xUnit、NUnit 或 MSTest）
- 相关测试包（如 Microsoft.NET.Test.Sdk、FluentAssertions）版本必须一致

**判定**：
- ❌ 测试框架存在多个版本
- ✅ 所有测试项目使用统一的测试框架版本

#### ADR-004.3.4 层级依赖规则

- **Platform**：仅技术底座包（Logging、OpenTelemetry、基础异常处理）
- **Application**：装配与 Pipeline 包（Wolverine、Marten）
- **Modules**：业务依赖、DTO、协议、契约
- **Host**：仅调用 Bootstrapper，不依赖业务包
- **Tests**：被测模块 + Platform/Application

**判定**：
- ❌ 层级依赖不符合规范
- ✅ 所有层级依赖符合约束

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-004 各条款（Clause）的执法方式及执行级别。
>
> 所有规则通过 `src/tests/ArchitectureTests/ADR-004/ADR_004_X_Architecture_Tests.cs` 强制验证。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-004.1.1** | L1 | ArchitectureTests 验证 Directory.Packages.props 文件存在性 | §ADR-004.1.1 |
| **ADR-004.1.2** | L1 | ArchitectureTests 验证 ManagePackageVersionsCentrally 设置 | §ADR-004.1.2 |
| **ADR-004.1.3** | L2 | ArchitectureTests 建议性检查，不阻断构建 | §ADR-004.1.3 |
| **ADR-004.2.1** | L1 | ArchitectureTests 扫描所有 .csproj 文件，检查 PackageReference 的 Version 属性 | §ADR-004.2.1 |
| **ADR-004.2.2** | L1 | ArchitectureTests 对比项目引用与 CPM 定义，检测未声明的包 | §ADR-004.2.2 |
| **ADR-004.3.1** | L2 | ArchitectureTests 建议性检查，验证 Label 属性使用情况 | §ADR-004.3.1 |
| **ADR-004.3.2** | L1 | ArchitectureTests 验证 Platform 项目的包引用类型 | §ADR-004.3.2 |
| **ADR-004.3.3** | L1 | ArchitectureTests 验证测试框架包的版本一致性 | §ADR-004.3.3 |
| **ADR-004.3.4** | L1 | ArchitectureTests 验证各层级的依赖规则 | §ADR-004.3.4 |

**有一项违规视为架构违规，CI 自动阻断。**

---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **具体包版本号选择**：不约束应该使用哪个具体版本号（如 8.0.0 vs 8.0.1），仅确保版本集中管理
- **包的安全漏洞扫描**：不涉及如何检测和修复包的安全漏洞，这由安全流程管理
- **私有 NuGet 源配置**：不约束是否使用私有源、如何配置源优先级等
- **包升级审批流程**：不规定谁有权限升级包、升级需要哪些审批，这由治理流程定义
- **包缓存和离线构建**：不涉及如何优化包下载、缓存策略或离线场景构建
- **包许可证合规性**：不检查包的开源许可证是否符合公司政策
- **多目标框架兼容性**：不约束如何处理针对不同 .NET 版本的包兼容性问题
- **包性能基准测试**：不涉及包对构建时间、运行时性能的影响评估

---

## Prohibited（禁止行为）


以下行为明确禁止：

### 包版本管理违规

- ❌ **在项目文件中指定包版本**：禁止在 `.csproj` 中使用 `<PackageReference Include="..." Version="x.y.z" />`
- ❌ **覆盖中央包版本**：禁止在项目文件中使用 `<PackageReference Update="..." VersionOverride="..." />`
- ❌ **删除或修改 Directory.Packages.props**：未经架构委员会批准，不得删除或修改根目录的 Directory.Packages.props
- ❌ **禁用 CPM**：禁止在项目文件中添加 `<ManagePackageVersionsCentrally>false</ManagePackageVersionsCentrally>`

### 层级依赖违规

- ❌ **Platform 依赖业务包**：Platform 项目禁止引用任何业务相关的 NuGet 包（如 FluentValidation、MediatR 等）
- ❌ **Host 直接依赖模块包**：Host 项目禁止引用模块特定的包，只能依赖 Platform 和 Application
- ❌ **模块依赖 Host 包**：Modules 禁止引用 ASP.NET Core、Kestrel 等 Host 专属包
- ❌ **测试项目引用生产包**：测试项目禁止引用生产环境专用的包（如监控、APM 等）

### 包分组与配置违规

- ❌ **不使用包分组注释**：Directory.Packages.props 中的包必须按技术栈分组并添加注释
- ❌ **测试框架版本不统一**：所有测试项目必须使用相同版本的 xUnit、NUnit 或 MSTest
- ❌ **传递依赖版本不固定**：禁止让传递依赖自动升级，必须在 Directory.Packages.props 中显式声明
- ❌ **包引用缺少用途说明**：新增包时必须在 Directory.Packages.props 中添加注释说明用途

### 架构测试规避

- ❌ **注释架构测试**：禁止注释或删除 ADR_004_Architecture_Tests.cs 中的测试
- ❌ **添加测试排除项**：禁止通过 `[Fact(Skip = "...")]` 或条件编译跳过包管理测试
- ❌ **修改测试阈值**：禁止修改测试中的层级依赖规则（如允许 Platform 依赖更多包）

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-900
- [ADR-002：平台、应用与主机启动器架构](./ADR-002-platform-application-host-bootstrap.md) - 包管理规范基于层级装配边界
- [ADR-003：命名空间与项目结构规范](./ADR-003-namespace-rules.md) - 包管理依赖命名空间结构

**被依赖（Depended By）**：
- [ADR-005：应用内交互模型与执行边界](./ADR-005-Application-Interaction-Model-Final.md) - 运行时依赖语义基于包管理规则

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：

- 无

---

## References（非裁决性参考）

**官方文档**：
- [NuGet Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management) - NuGet CPM 官方文档
- [MSBuild Directory.Packages.props](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management#enabling-central-package-management) - CPM 配置详解
- [NuGet Package Versioning](https://learn.microsoft.com/en-us/nuget/concepts/package-versioning) - 包版本管理最佳实践
- [Transitive Dependencies](https://learn.microsoft.com/en-us/nuget/concepts/dependency-resolution) - 传递依赖解析机制

**依赖管理最佳实践**：
- [Dependency Management Best Practices](https://www.thoughtworks.com/insights/blog/dependency-management) - ThoughtWorks 依赖管理指南
- [Managing .NET Dependencies at Scale](https://devblogs.microsoft.com/dotnet/managing-package-dependency-updates-at-scale/) - 大规模依赖管理

**相关内部文档**：
- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md) - 了解测试执行机制
- [ADR-002：平台、应用与主机启动器架构](./ADR-002-platform-application-host-bootstrap.md) - 了解层级职责划分
- [ADR-003：命名空间与项目结构规范](./ADR-003-namespace-rules.md) - 了解项目组织结构

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                         |
|-----|------------|----------------------------------------------|
| 3.0 | 2026-02-04 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系。重构 Decision 章节为 3 个 Rule（Rule 1: CPM 基础设施约束，Rule 2: 项目依赖管理约束，Rule 3: 层级依赖与分组约束），共 9 个 Clause。更新 Enforcement 表格，架构测试将拆分为多个文件。 |
| 2.0 | 2026-01-29 | 同步 ADR-902/940/0006 标准：添加 Front Matter、术语表英文对照 |
| 1.0 | 2026-01-26 | 裁决型重构，移除冗余                                   |
