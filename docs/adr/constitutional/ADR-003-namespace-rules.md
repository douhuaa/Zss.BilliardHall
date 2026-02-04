---
adr: ADR-003
title: "命名空间与项目边界规范"
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


# ADR-003：命名空间与项目边界规范

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

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-003 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-003_<Rule>_<Clause>
> ```

---

### ADR-003_1：基础命名空间约束（Rule）

#### ADR-003_1_1 所有类型必须以 BaseNamespace 开头

- 所有项目的类型命名空间必须以 BaseNamespace 开头
- BaseNamespace 为 `Zss.BilliardHall`

**判定**：
- ❌ 类型命名空间不以 BaseNamespace 开头
- ✅ 所有类型命名空间符合规范

---

### ADR-003_2：Platform 命名空间约束（Rule）

#### ADR-003_2_1 Platform 类型必须在 Platform 命名空间

- Platform 类型必须以 `Zss.BilliardHall.Platform` 为前缀
- 确保 Platform 层边界清晰

**判定**：
- ❌ Platform 类型命名空间不符合规范
- ✅ Platform 类型命名空间正确

---

### ADR-003_3：Application 命名空间约束（Rule）

#### ADR-003_3_1 Application 类型必须在 Application 命名空间

- Application 类型必须以 `Zss.BilliardHall.Application` 为前缀
- 确保 Application 层边界清晰

**判定**：
- ❌ Application 类型命名空间不符合规范
- ✅ Application 类型命名空间正确

---

### ADR-003_4：Modules 命名空间约束（Rule）

#### ADR-003_4_1 Module 类型必须在对应模块命名空间

- Module 类型必须对应 `Zss.BilliardHall.Modules.{ModuleName}`
- 每个模块有独立的命名空间

**判定**：
- ❌ Module 类型命名空间不符合规范
- ✅ Module 类型命名空间正确

---

### ADR-003_5：Host 命名空间约束（Rule）

#### ADR-003_5_1 Host 类型必须在对应 Host 命名空间

- Host 类型必须对应 `Zss.BilliardHall.Host.{HostName}`
- 每个 Host 有独立的命名空间

**判定**：
- ❌ Host 类型命名空间不符合规范
- ✅ Host 类型命名空间正确

---

### ADR-003_6：Directory.Build.props 约束（Rule）

#### ADR-003_6_1 Directory.Build.props 必须存在

- Directory.Build.props 必须存在于仓库根目录
- 统一管理 BaseNamespace 定义

**判定**：
- ❌ 缺少 Directory.Build.props
- ✅ Directory.Build.props 存在

#### ADR-003_6_2 Directory.Build.props 必须定义 BaseNamespace

- Directory.Build.props 必须定义 BaseNamespace 属性
- 值为 `Zss.BilliardHall`

**判定**：
- ❌ Directory.Build.props 未定义 BaseNamespace
- ✅ Directory.Build.props 正确定义 BaseNamespace

---

### ADR-003_7：项目命名约束（Rule）

#### ADR-003_7_1 所有项目必须遵循命名空间约定

- 项目命名必须与目录结构和命名空间对应
- 通过 MSBuild 自动推导 RootNamespace

**判定**：
- ❌ 项目命名不符合规范
- ✅ 项目命名符合命名空间约定

---

### ADR-003_8：禁止的命名空间模式（Rule）

#### ADR-003_8_1 禁止不规范命名空间模式

- 不得出现不规范命名空间（Common、Shared、Utils）
- 使用明确的层级命名

**判定**：
- ❌ 存在不规范命名空间
- ✅ 所有命名空间符合规范

---

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-003 各条款（Clause）的执法方式及执行级别。
>
> 所有规则通过 `src/tests/ArchitectureTests/ADR-003/` 目录下的测试强制验证。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-003_1_1** | L1 | ArchitectureTests 验证所有类型命名空间以 BaseNamespace 开头 | §ADR-003_1_1 |
| **ADR-003_2_1** | L1 | ArchitectureTests 验证 Platform 类型命名空间 | §ADR-003_2_1 |
| **ADR-003_3_1** | L1 | ArchitectureTests 验证 Application 类型命名空间 | §ADR-003_3_1 |
| **ADR-003_4_1** | L1 | ArchitectureTests 验证 Module 类型命名空间 | §ADR-003_4_1 |
| **ADR-003_5_1** | L1 | ArchitectureTests 验证 Host 类型命名空间 | §ADR-003_5_1 |
| **ADR-003_6_1** | L1 | ArchitectureTests 验证 Directory.Build.props 存在 | §ADR-003_6_1 |
| **ADR-003_6_2** | L1 | ArchitectureTests 验证 Directory.Build.props 定义 BaseNamespace | §ADR-003_6_2 |
| **ADR-003_7_1** | L1 | ArchitectureTests 验证项目命名约定 | §ADR-003_7_1 |
| **ADR-003_8_1** | L1 | ArchitectureTests 验证不存在不规范命名空间 | §ADR-003_8_1 |

### 执行级别说明
- **L1（阻断级）**：违规直接导致 CI 失败、阻止合并/部署

**有一项 L1 违规视为架构违规，CI 自动阻断。**

---
---

## Non-Goals（明确不管什么）

本 ADR 明确不涉及以下内容：

- **代码风格与格式化**：不规定命名约定（如 PascalCase、camelCase）、缩进、换行等代码格式规则，这些由代码规范（如 .editorconfig）管理
- **业务逻辑命名**：不规定业务类型、方法、变量的具体命名方式，仅管理命名空间结构
- **包版本管理**：不管理 NuGet 包的版本选择和依赖关系，这些由 [ADR-004](./ADR-004-Cpm-Final.md) 管理
- **依赖注入配置**：不规定服务注册、生命周期管理等 DI 细节，这些由 [ADR-002](./ADR-002-platform-application-host-bootstrap.md) 和 [ADR-005](./ADR-005-Application-Interaction-Model-Final.md) 管理
- **模块间通信方式**：不规定模块如何通信（事件、契约），这些由 [ADR-001](./ADR-001-modular-monolith-vertical-slice-architecture.md) 管理
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

- ❌ **排除架构测试**：尝试通过修改测试代码、添加忽略标记或修改 CI 配置来规避 ADR-003 的架构测试
- ❌ **注释掉失败的测试**：当架构测试失败时，注释掉测试而不是修复违规代码
- ❌ **使用反射绕过检查**：使用反射或其他动态技术绕过命名空间约束


---

---

## Relationships（关系声明）

**依赖（Depends On）**：
- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md) - 本 ADR 的测试执行基于 ADR-900
- [ADR-002：平台、应用与主机启动器架构](./ADR-002-platform-application-host-bootstrap.md) - 命名空间规范基于三层体系
- [ADR-001：模块化单体与垂直切片架构](../constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)

**被依赖（Depended By）**：
- [ADR-004：中央包管理与层级依赖规则](./ADR-004-Cpm-Final.md) - 包管理依赖命名空间规范
- [ADR-005：应用内交互模型与执行边界](./ADR-005-Application-Interaction-Model-Final.md) - 运行时注册依赖命名空间映射
- [ADR-122：测试代码组织与命名规范](../structure/ADR-122-test-organization-naming.md)
- [ADR-121：契约（Contract）与 DTO 命名组织规范](../structure/ADR-121-contract-dto-naming-organization.md)

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-006：术语与编号宪法](./ADR-006-terminology-numbering-constitution.md) - 命名规范

---

---

## References（非裁决性参考）

### 官方文档

- [MSBuild Directory.Build.props](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory) - MSBuild 自定义属性和目录级配置
- [.NET Project Structure Best Practices](https://learn.microsoft.com/en-us/dotnet/core/extensions/project-structure) - .NET 项目结构最佳实践
- [C# Namespaces](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/namespaces) - C# 命名空间基础

### 相关 ADR

- [ADR-900：架构测试与 CI 治理元规则](../governance/ADR-900-architecture-tests.md) - 了解架构测试的执行机制
- [ADR-001：模块化单体与垂直切片架构](./ADR-001-modular-monolith-vertical-slice-architecture.md) - 了解模块物理隔离的原因
- [ADR-002：平台、应用与主机启动器架构](./ADR-002-platform-application-host-bootstrap.md) - 了解三层体系的职责划分
- [ADR-004：中央包管理与层级依赖规则](./ADR-004-Cpm-Final.md) - 了解包管理如何与命名空间协同工作
- [ADR-006：术语与编号宪法](./ADR-006-terminology-numbering-constitution.md) - 了解术语的标准定义

### 设计模式与理念

- [Screaming Architecture](https://blog.cleancoder.com/uncle-bob/2011/09/30/Screaming-Architecture.html) - Robert C. Martin 关于目录结构应体现业务意图的理念
- [Package by Feature](https://phauer.com/2020/package-by-feature/) - 按功能组织代码而非按层组织


---

---

## History（版本历史）

| 版本  | 日期         | 变更说明                                         | 修订人 |
|-----|------------|----------------------------------------------|----|
| 3.0 | 2026-02-04 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | Architecture Board |
| 2.0 | 2026-01-29 | 同步 ADR-902/940/0006 标准：添加 Front Matter、术语表英文对照 | Architecture Board |
| 1.0 | 2026-01-26 | 裁决型重构，移除冗余                                   | Architecture Board |

---
