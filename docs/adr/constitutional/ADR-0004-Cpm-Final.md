---
adr: ADR-0004
title: "中央包管理（CPM）规范"
status: Final
level: Constitutional
deciders: "Architecture Board"
date: 2026-01-29
version: "2.0"
maintainer: "Architecture Board"
primary_enforcement: L1
reviewer: "Architecture Board"
supersedes: null
superseded_by: null
---


# ADR-0004：中央包管理（CPM）规范

> ⚖️ **本 ADR 是架构宪法的核心，定义中央包管理的唯一裁决源。**

---

## Focus（聚焦内容）

仅定义适用于全生命周期自动化裁决/阻断的**包管理约束**：

- 所有依赖包通过 Directory.Packages.props 集中管理
- 层级依赖规则细化：Platform、Application、Modules、Host 各自边界
- 禁止项目文件手动指定包版本
- 所有规则必须架构测试覆盖

---

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

---

## Decision（裁决）

### 包集中管理与防御（ADR-0004.1, 0004.2, 0004.3, 0004.9）

**规则**：
- 必须启用 CPM
- Directory.Packages.props 必须存在
- 禁止项目文件手动指定 Version
- 禁止私自覆盖中央包版本

**判定**：
- ❌ 未启用 CPM
- ❌ 缺少 Directory.Packages.props
- ❌ 项目文件包含 Version 属性
- ❌ 项目覆盖中央包版本
- ✅ 所有包版本集中管理

### 层级依赖规则（ADR-0004.4, 0004.8）

**规则**：
- Platform：仅技术底座包（Logging、OpenTelemetry、基础异常处理）
- Application：装配与 Pipeline 包（Wolverine、Marten）
- Modules：业务依赖、DTO、协议、契约
- Host：仅调用 Bootstrapper，不依赖业务包
- Tests：被测模块 + Platform/Application

**判定**：
- ❌ Platform 依赖业务包
- ❌ 层级依赖不符合规范
- ✅ 所有层级依赖正确

### 包分组与统一（ADR-0004.5, 0004.6, 0004.7）

**规则**：
- Directory.Packages.props 必须包含包分组
- 所有测试项目使用相同测试框架版本
- 所有使用的包必须集中声明

**判定**：
- ❌ Directory.Packages.props 缺少分组
- ❌ 测试框架版本不一致
- ❌ 存在未声明的包依赖
- ✅ 包分组完整、版本统一

---

---

## Enforcement（执法模型）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0004_Architecture_Tests.cs` 强制验证。

**有一项违规视为架构违规，CI 自动阻断。**

---
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

- ❌ **注释架构测试**：禁止注释或删除 ADR_0004_Architecture_Tests.cs 中的测试
- ❌ **添加测试排除项**：禁止通过 `[Fact(Skip = "...")]` 或条件编译跳过包管理测试
- ❌ **修改测试阈值**：禁止修改测试中的层级依赖规则（如允许 Platform 依赖更多包）


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-0000
- [ADR-0002：平台、应用与主机启动器架构](./ADR-0002-platform-application-host-bootstrap.md) - 包管理规范基于层级装配边界
- [ADR-0003：命名空间与项目结构规范](./ADR-0003-namespace-rules.md) - 包管理依赖命名空间结构

**被依赖（Depended By）**：
- [ADR-0005：应用内交互模型与执行边界](./ADR-0005-Application-Interaction-Model-Final.md) - 运行时依赖语义基于包管理规则

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- 无

---

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
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 了解测试执行机制
- [ADR-0002：平台、应用与主机启动器架构](./ADR-0002-platform-application-host-bootstrap.md) - 了解层级职责划分
- [ADR-0003：命名空间与项目结构规范](./ADR-0003-namespace-rules.md) - 了解项目组织结构


---

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                         |
|-----|------------|----------------------------------------------|
| 2.0 | 2026-01-29 | 同步 ADR-902/940/0006 标准：添加 Front Matter、术语表英文对照 |
| 1.0 | 2026-01-26 | 裁决型重构，移除冗余                                   |

---
