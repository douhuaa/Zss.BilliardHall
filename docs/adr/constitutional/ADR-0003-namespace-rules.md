---
adr: ADR-0003
title: "命名空间与项目边界规范"
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


# ADR-0003：命名空间与项目边界规范

> ⚖️ **本 ADR 是架构宪法的核心，定义命名空间与项目边界的唯一裁决源。**

---

## Focus（聚焦内容）

仅定义适用于全生命周期自动化裁决/阻断的**命名空间约束**：

- BaseNamespace 固定与统一定义
- 目录结构与 RootNamespace 自动映射
- 项目命名与命名空间边界原则
- 所有规则必须架构测试覆盖

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|-----------------------|--------------------------------------|----------------------|
| BaseNamespace         | 公司+产品根命名空间（`Zss.BilliardHall`）       | Base Namespace       |
| RootNamespace         | 项目根命名空间，由BaseNamespace及目录自动推导        | Root Namespace       |
| Directory.Build.props | MSBuild全局配置文件，统一定义BaseNamespace      | Directory.Build.props |
| MSBuild 推导            | 通过 MSBuild 条件和目录映射自动赋值 RootNamespace | MSBuild Inference    |

---

---

## Decision（裁决）

### 命名空间自动推导与一致性（ADR-0003.1, 0003.2, 0003.3, 0003.4, 0003.5）

**规则**：
- 所有项目必须通过 Directory.Build.props 定义 BaseNamespace
- 目录结构直接推导 RootNamespace
- 类型命名空间必须与项目层级匹配

**判定**：
- ❌ 类型命名空间不以 BaseNamespace 开头
- ❌ Platform 类型不以 `Zss.BilliardHall.Platform` 为前缀
- ❌ Application 类型不以 `Zss.BilliardHall.Application` 为前缀
- ❌ Module 类型不对应 `Zss.BilliardHall.Modules.{Name}`
- ❌ Host 类型不对应 `Zss.BilliardHall.Host.{Name}`
- ✅ 所有命名空间自动映射正确

### 防御规则（ADR-0003.6, 0003.7, 0003.8, 0003.9）

**规则**：
- Directory.Build.props 必须存在于仓库根目录
- Directory.Build.props 必须定义 BaseNamespace
- 项目命名需遵循命名空间映射
- 不得出现不规范命名空间（Common、Shared、Utils）

**判定**：
- ❌ 缺少 Directory.Build.props
- ❌ Directory.Build.props 未定义 BaseNamespace
- ❌ 项目命名不符合规范
- ❌ 存在不规范命名空间
- ✅ 所有配置和命名正确

---

---

## Enforcement（执法模型）

所有规则通过 `src/tests/ArchitectureTests/ADR/ADR_0003_Architecture_Tests.cs` 强制验证。

**有一项违规视为架构违规，CI 自动阻断。**

---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **代码风格与格式化**：不规定命名约定（如 PascalCase、camelCase）、缩进、换行等代码格式规则，这些由代码规范（如 .editorconfig）管理
- **业务逻辑命名**：不规定业务类型、方法、变量的具体命名方式，仅管理命名空间结构
- **包版本管理**：不管理 NuGet 包的版本选择和依赖关系，这些由 [ADR-0004](./ADR-0004-Cpm-Final.md) 管理
- **依赖注入配置**：不规定服务注册、生命周期管理等 DI 细节，这些由 [ADR-0002](./ADR-0002-platform-application-host-bootstrap.md) 和 [ADR-0005](./ADR-0005-Application-Interaction-Model-Final.md) 管理
- **模块间通信方式**：不规定模块如何通信（事件、契约），这些由 [ADR-0001](./ADR-0001-modular-monolith-vertical-slice-architecture.md) 管理
- **文件组织结构**：不规定文件在命名空间内的具体组织方式（如按用例、按层），仅管理命名空间边界
- **多语言项目**：本 ADR 仅适用于 C# 项目，不涉及前端、脚本或其他语言项目的命名空间规范
- **测试命名空间详细规则**：测试项目的详细命名规范由 [ADR-122](../structure/ADR-122-test-organization-naming.md) 管理
- **运行时命名空间验证**：本 ADR 仅在编译时和架构测试中验证，不涉及运行时的命名空间检查

---

## Prohibited（禁止行为）


以下行为明确禁止：

### 命名空间配置违规

- ❌ **手动覆盖 RootNamespace**：在项目文件（.csproj）中手动设置 `<RootNamespace>`，违反自动推导原则
- ❌ **硬编码 BaseNamespace**：在代码中硬编码完整命名空间字符串，应通过 MSBuild 自动推导
- ❌ **删除或修改 Directory.Build.props**：未经架构委员会批准，不得删除或修改仓库根目录的 Directory.Build.props 文件

### 不规范命名空间模式

- ❌ **使用通用命名空间**：使用 `Common`、`Shared`、`Utils`、`Helpers` 等违反垂直切片原则的命名空间
- ❌ **跨层命名空间引用**：命名空间层级与项目层级不匹配

### 项目命名违规

- ❌ **项目名与目录名不一致**：项目文件名必须与其所在目录的最后一级名称完全一致
- ❌ **项目命名不符合层级规范**：项目名称不遵循 BaseNamespace + 层级路径的规范

### 目录结构违规

- ❌ **绕过标准目录层级**：在 src/ 下创建不符合 Platform/Application/Modules/Host/Tests 层级的目录
- ❌ **模块目录扁平化**：将多个模块代码混合在同一目录中，违反物理隔离原则

### 架构测试规避

- ❌ **排除架构测试**：尝试通过修改测试代码、添加忽略标记或修改 CI 配置来规避 ADR-0003 的架构测试
- ❌ **注释掉失败的测试**：当架构测试失败时，注释掉测试而不是修复违规代码
- ❌ **使用反射绕过检查**：使用反射或其他动态技术绕过命名空间约束


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理元规则](../governance/ADR-0000-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-0000
- [ADR-0002：平台、应用与主机启动器架构](./ADR-0002-platform-application-host-bootstrap.md) - 命名空间规范基于三层体系
- [ADR-0001：模块化单体与垂直切片架构](../constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)

**被依赖（Depended By）**：
- [ADR-0004：中央包管理与层级依赖规则](./ADR-0004-Cpm-Final.md) - 包管理依赖命名空间规范
- [ADR-0005：应用内交互模型与执行边界](./ADR-0005-Application-Interaction-Model-Final.md) - 运行时注册依赖命名空间映射
- [ADR-122：测试代码组织与命名规范](../structure/ADR-122-test-organization-naming.md)
- [ADR-121：契约（Contract）与 DTO 命名组织规范](../structure/ADR-121-contract-dto-naming-organization.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-0006：术语与编号宪法](./ADR-0006-terminology-numbering-constitution.md) - 命名规范

---

---

## References（非裁决性参考）

### 官方文档

- [MSBuild Directory.Build.props](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory) - MSBuild 自定义属性和目录级配置
- [.NET Project Structure Best Practices](https://learn.microsoft.com/en-us/dotnet/core/extensions/project-structure) - .NET 项目结构最佳实践
- [C# Namespaces](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/namespaces) - C# 命名空间基础

### 相关 ADR

- [ADR-0000：架构测试与 CI 治理元规则](../governance/ADR-0000-architecture-tests.md) - 了解架构测试的执行机制
- [ADR-0001：模块化单体与垂直切片架构](./ADR-0001-modular-monolith-vertical-slice-architecture.md) - 了解模块物理隔离的原因
- [ADR-0002：平台、应用与主机启动器架构](./ADR-0002-platform-application-host-bootstrap.md) - 了解三层体系的职责划分
- [ADR-0004：中央包管理与层级依赖规则](./ADR-0004-Cpm-Final.md) - 了解包管理如何与命名空间协同工作
- [ADR-0006：术语与编号宪法](./ADR-0006-terminology-numbering-constitution.md) - 了解术语的标准定义

### 设计模式与理念

- [Screaming Architecture](https://blog.cleancoder.com/uncle-bob/2011/09/30/Screaming-Architecture.html) - Robert C. Martin 关于目录结构应体现业务意图的理念
- [Package by Feature](https://phauer.com/2020/package-by-feature/) - 按功能组织代码而非按层组织


---

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                         |
|-----|------------|----------------------------------------------|
| 2.0 | 2026-01-29 | 同步 ADR-902/940/0006 标准：添加 Front Matter、术语表英文对照 |
| 1.0 | 2026-01-26 | 裁决型重构，移除冗余                                   |

---
