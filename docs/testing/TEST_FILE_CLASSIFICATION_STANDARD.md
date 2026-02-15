# 测试文件分类标准

> **版本**: 1.0  
> **发布日期**: 2026-02-13  
> **维护者**: Architecture Team  
> **适用范围**: 所有测试项目（UnitTests, IntegrationTests, ArchitectureTests, SharedTestHelpers）

---

## 📋 目录

- [概述](#概述)
- [分类定义](#分类定义)
- [判定流程](#判定流程)
- [常见案例](#常见案例)
- [决策矩阵](#决策矩阵)
- [最佳实践](#最佳实践)

---

## 概述

### 目的

建立统一的测试文件分类标准，以便：
1. **明确文件归属**：每个测试文件都有明确的项目归属
2. **提升复用性**：通用工具可被多个项目使用
3. **降低耦合**：避免不必要的项目间依赖
4. **指导迁移**：为自动化迁移提供判定依据

### 适用场景

- ✅ 新建测试工具类
- ✅ 重构现有测试代码
- ✅ 自动化迁移脚本
- ✅ Code Review 评审
- ✅ 架构治理检查

---

## 分类定义

### 1. 通用测试工具（SharedTestHelpers）

**定义**：不依赖特定测试类型（单元/集成/架构），可被任何测试项目复用的基础设施代码。

**关键特征**：
- ✅ 无特定测试框架依赖（除 Xunit、FluentAssertions 等通用库）
- ✅ 不涉及业务逻辑验证
- ✅ 可独立使用，无需架构上下文
- ✅ 职责单一，功能明确

**典型场景**：
- 文件系统操作（读取、搜索、断言）
- 数据构建器（Builder Pattern）
- 断言消息格式化
- 测试环境配置（路径常量）
- 性能监控收集

**命名空间约定**：
```csharp
namespace Zss.BilliardHall.Tests.SharedTestHelpers;
namespace Zss.BilliardHall.Tests.SharedTestHelpers.FileSystem;
namespace Zss.BilliardHall.Tests.SharedTestHelpers.Testing;
```

**示例文件**：
- `FileSearchHelper.cs` - 文件搜索
- `FileContentAnalyzer.cs` - 内容分析
- `TestEnvironment.cs` - 路径常量
- `AssertionMessageBuilder.cs` - 消息格式化
- `TestDataBuilder.cs` - 数据构建

---

### 2. 架构专用工具（ArchitectureTests/Shared）

**定义**：强依赖架构验证概念，专为架构测试设计的工具类。

**关键特征**：
- ✅ 依赖 NetArchTest.Rules
- ✅ 操作架构规则（ArchitectureRuleSet、RuleId）
- ✅ 验证架构约束（模块边界、依赖方向）
- ✅ 仅在架构测试上下文中有意义

**典型场景**：
- NetArchTest 封装和扩展
- 架构规则集验证
- 程序集加载和分析（用于架构检查）
- 规则ID 断言

**命名空间约定**：
```csharp
namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;
namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Testing;
namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Assemblies;
```

**示例文件**：
- `NetArchTestHelper.cs` - NetArchTest 封装
- `RuleSetValidator.cs` - 规则集验证
- `RuleIdAssertions.cs` - 规则ID 断言
- `ModuleAssemblyData.cs` - 模块程序集加载（架构检查用）
- `HostAssemblyData.cs` - Host 程序集加载（架构检查用）

---

### 3. ADR 处理工具（SharedTestHelpers/Adr）

**定义**：专门处理 ADR（Architecture Decision Record）文档的工具，但不涉及架构代码验证。

**关键特征**：
- ✅ 解析/生成 ADR Markdown
- ✅ 管理 ADR 关系（depends-on, supersedes）
- ✅ 提取 Front Matter 元数据
- ✅ 可被测试和非测试场景使用

**典型场景**：
- ADR 文档解析
- Front Matter 元数据提取
- ADR 关系图生成
- ADR 文档构建

**命名空间约定**：
```csharp
namespace Zss.BilliardHall.Tests.SharedTestHelpers.Adr;
```

**示例文件**：
- `AdrParser.cs` - 解析 ADR Markdown
- `AdrRepository.cs` - ADR 文档仓库
- `FrontMatterParser.cs` - Front Matter 解析
- `AdrMarkdownBuilder.cs` - ADR 文档构建
- `AdrRelationshipValidator.cs` - 关系验证

**未来展望**：
如果 ADR 工具被非测试场景广泛使用（如文档生成脚本、CI报告），可考虑提取到独立项目：
```
src/tools/Zss.BilliardHall.Adr.Tools/
```

---

## 判定流程

### 决策问题清单

回答以下问题以确定分类：

| 问题 | 分类倾向 |
|------|----------|
| **1. 是否依赖 NetArchTest.Rules?** | 是 → 架构专用 |
| **2. 是否操作 ArchitectureRuleSet 或 RuleId?** | 是 → 架构专用 |
| **3. 是否解析/生成 ADR Markdown?** | 是 → ADR 工具 |
| **4. 是否验证架构约束（模块边界、依赖）?** | 是 → 架构专用 |
| **5. 是否依赖具体的业务实体或服务?** | 是 → 单元/集成专用 |
| **6. 是否管理外部资源（数据库、容器）?** | 是 → 集成测试专用 |
| **7. 是否可被多个测试项目复用?** | 是 → 通用工具 |
| **8. 是否纯文件/字符串/数据操作?** | 是 → 通用工具 |

**判定规则**：
- 如果 Q1 或 Q2 或 Q4 = 是 → **架构专用工具**
- 如果 Q3 = 是 且 Q4 = 否 → **ADR 处理工具**
- 如果 Q5 = 是 且 单元测试专用 → **单元测试工具**
- 如果 Q6 = 是 → **集成测试工具**
- 如果 Q7 = 是 或 Q8 = 是 → **通用测试工具**

---

## 常见案例

### 案例 1: FileSearchHelper - 通用工具

**代码**：
```csharp
public static class FileSearchHelper
{
    public static string[] GetAdrFiles(string directory)
    {
        return Directory.GetFiles(directory, "*.md", SearchOption.AllDirectories)
            .Where(f => Path.GetFileName(f).StartsWith("ADR-"))
            .ToArray();
    }
}
```

**分析**：纯文件操作，无架构逻辑

**归属**：`SharedTestHelpers/FileSystem` ✅

---

### 案例 2: NetArchTestHelper - 架构专用

**代码**：
```csharp
public static class NetArchTestHelper
{
    public static ConditionList ShouldResideInNamespace(
        this Types types, 
        string namespace)
    {
        return types.Should().ResideInNamespace(namespace);
    }
}
```

**分析**：依赖 NetArchTest.Rules，扩展架构测试 DSL

**归属**：`ArchitectureTests/Shared/Testing` ✅

---

### 案例 3: AdrParser - ADR 工具

**代码**：
```csharp
public static class AdrParser
{
    public static AdrDocument Parse(string markdown)
    {
        var frontMatter = FrontMatterParser.Parse(markdown);
        var sections = ExtractSections(markdown);
        return new AdrDocument(frontMatter, sections);
    }
}
```

**分析**：解析 ADR Markdown，无架构验证逻辑

**归属**：`SharedTestHelpers/Adr` ✅

---

## 决策矩阵

### 快速查找表

| 工具类名称 | 分类 | 归属项目 |
|-----------|------|---------|
| FileSearchHelper | 通用 | SharedTestHelpers/FileSystem |
| FileContentAnalyzer | 通用 | SharedTestHelpers/FileSystem |
| TestEnvironment | 通用 | SharedTestHelpers/Testing |
| AssertionMessageBuilder | 通用 | SharedTestHelpers/Testing |
| **NetArchTestHelper** | **架构** | **ArchitectureTests/Shared/Testing** |
| **RuleSetValidator** | **架构** | **ArchitectureTests/Shared/Testing** |
| **ModuleAssemblyData** | **架构** | **ArchitectureTests/Shared/Assemblies** |
| AdrParser | ADR | SharedTestHelpers/Adr |
| AdrRepository | ADR | SharedTestHelpers/Adr |
| FrontMatterParser | ADR | SharedTestHelpers/Adr |

---

## 最佳实践

### 1. 命名约定

**通用测试工具**：
- ✅ `*Helper.cs` - 辅助类
- ✅ `*Builder.cs` - 构建器
- ✅ `*Analyzer.cs` - 分析器
- ✅ `Test*.cs` - 测试环境/常量

**架构专用工具**：
- ✅ `*RuleSet*.cs` - 规则集相关
- ✅ `*RuleId*.cs` - 规则ID 相关
- ✅ `*AssemblyData.cs` - 程序集加载（架构用）

**ADR 工具**：
- ✅ `Adr*.cs` - ADR 开头
- ✅ `*FrontMatter*.cs` - Front Matter 相关

### 2. 依赖管理

**通用测试工具**：
```csharp
// ✅ 允许的依赖
using System.*;
using Xunit;
using FluentAssertions;

// ❌ 不允许的依赖
using NetArchTest.Rules;  // 架构专用
using Zss.BilliardHall.Modules.*;  // 业务逻辑
```

### 3. GlobalUsings 使用

**SharedTestHelpers/GlobalUsings.cs**：
```csharp
// ✅ 仅引入基础库和测试框架
global using System.*;
global using Xunit;
global using FluentAssertions;
// 不引入业务命名空间
```

**ArchitectureTests/GlobalUsings.cs**：
```csharp
// ✅ 引入架构测试必需的命名空间
global using NetArchTest.Rules;
global using Zss.BilliardHall.Specification.*;
global using Zss.BilliardHall.Tests.SharedTestHelpers;

// ⚠️ 按需引入，避免污染
// global using Zss.BilliardHall.Generators.*;
```

---

## 审查检查清单

Code Review 时，使用此检查清单验证分类是否正确：

### 新增文件检查

- [ ] 文件位于正确的项目中
- [ ] 命名空间符合约定
- [ ] 依赖关系正确（无逆向依赖）
- [ ] 使用合适的访问修饰符
- [ ] 有单元测试覆盖（如适用）

### 迁移文件检查

- [ ] 目标项目正确
- [ ] 命名空间已更新
- [ ] 引用已更新（using 语句）
- [ ] GlobalUsings 已调整
- [ ] 所有测试仍通过

---

## 附录

### 相关文档

- [PR-406-MIGRATION-ANALYSIS.md](../../PR-406-MIGRATION-ANALYSIS.md) - 迁移问题分析
- [src/tests/README.md](../../src/tests/README.md) - 测试项目结构

### 变更历史

| 版本 | 日期 | 变更 |
|------|------|------|
| 1.0 | 2026-02-13 | 初始版本，基于 PR-406 分析 |

---

**维护者**: Architecture Team  
**审查周期**: 每季度  
**下次审查**: 2026-05-13
