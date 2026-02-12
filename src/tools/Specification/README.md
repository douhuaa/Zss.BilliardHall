# Zss.BilliardHall.Specification

## 目的（Purpose）

本库提供 Zss.BilliardHall 架构测试的规范模型（Specification Models），是一个**纯规范库**，不包含测试框架依赖。

**核心设计理念**：
- 将 ADR（架构决策记录）转化为可执行的规范模型
- 提供统一的规范定义入口，供测试、分析器、生成器等工具使用
- 保持与测试框架解耦，仅依赖 .NET 基础类库

## 公共 API

### ArchitectureTestSpecification

规范定义的主入口类，使用 partial class 组织为以下子模块：

#### 1. Namespaces - 命名空间规范
定义项目中的标准命名空间约定：
- `ArchitectureTests`: 架构测试命名空间前缀
- `Modules`: 模块命名空间前缀
- `Platform`: 平台命名空间
- `BuildingBlocks`: 构建块命名空间

#### 2. Adr - ADR 规范
定义 ADR 相关的所有规范：
- `Patterns`: ADR 命名和格式模式（正则表达式）
  - `TestClass`: ADR 测试类命名模式
  - `FileName`: ADR 文件命名模式
  - `Id`: ADR 编号模式
- `Paths`: ADR 文档路径约定
  - `Root`: 文档根目录
  - `Constitutional`: 宪法层文档路径
  - `Governance`: 治理层文档路径
  - `Technical`: 技术层文档路径
  - `Structure`: 结构层文档路径
- `KnownDocuments`: 已知的关键 ADR 文档路径（43+ 个 ADR）

#### 3. Semantics - 语义规范
定义语义块和关键词：
- `DecisionKeywords`: 裁决性关键词列表（已废弃，使用 DecisionLanguage 代替）
- `RequiredHeadings`: 关键语义块标题

#### 4. Output - 输出规范
定义标准输出格式，特别是三态输出模型：
- `States`: 三态输出状态
  - `Allowed`: ✅ 允许状态
  - `Blocked`: ⚠️ 阻止状态
  - `Uncertain`: ❓ 不确定状态
  - `FullIndicators`: 完整标识列表
  - `ShortForms`: 简写形式
  - `Emojis`: Emoji 标识

#### 5. Onboarding - Onboarding 规范
定义 Onboarding 文档的内容规范：
- `ProhibitedContent`: 禁止的内容类型
- `AllowedContent`: 允许的内容类型
- `CoreQuestions`: 三个核心问题

## 使用示例

### 基本使用

```csharp
using Zss.BilliardHall.Specification;

// 获取 ADR 根目录路径
var adrRoot = ArchitectureTestSpecification.Adr.Paths.Root;
// 输出: "docs/adr"

// 获取 ADR 编号模式
var adrIdPattern = ArchitectureTestSpecification.Adr.Patterns.Id;
// 输出: "^ADR-\d{4}$"

// 获取三态输出标识
var allowedState = ArchitectureTestSpecification.Output.States.Allowed;
// 输出: "✅ Allowed"
```

### 在测试中使用

```csharp
using Zss.BilliardHall.Specification;

public class AdrNamingTests
{
    [Fact]
    public void AdrFileName_Should_Match_Pattern()
    {
        var fileName = "ADR-0001-test.md";
        var pattern = ArchitectureTestSpecification.Adr.Patterns.FileName;
        
        Assert.Matches(pattern, fileName);
    }
}
```

### 在代码生成器中使用

```csharp
using Zss.BilliardHall.Specification;

public class AdrPathResolver
{
    public string GetAdrPath(string adrNumber, string category)
    {
        var basePath = category switch
        {
            "constitutional" => ArchitectureTestSpecification.Adr.Paths.Constitutional,
            "governance" => ArchitectureTestSpecification.Adr.Paths.Governance,
            "technical" => ArchitectureTestSpecification.Adr.Paths.Technical,
            _ => ArchitectureTestSpecification.Adr.Paths.Root
        };
        
        return Path.Combine(basePath, $"ADR-{adrNumber}.md");
    }
}
```

## 迁移注意点

### 从测试项目迁移到本库

本库的内容原本位于 `src/tests/ArchitectureTests/Specification` 目录，现已迁移为独立的工具库。

**命名空间变更**：
- **旧命名空间**: `Zss.BilliardHall.Tests.ArchitectureTests.Specification`
- **新命名空间**: `Zss.BilliardHall.Specification`

**迁移步骤**：

1. 添加项目引用：
```xml
<ItemGroup>
  <ProjectReference Include="..\..\tools\Specification\Zss.BilliardHall.Specification.csproj" />
</ItemGroup>
```

2. 更新 using 语句：
```csharp
// 旧代码
using Zss.BilliardHall.Tests.ArchitectureTests.Specification;

// 新代码
using Zss.BilliardHall.Specification;
```

### 保留在测试项目中的内容

以下内容**不属于**纯规范模型，仍保留在测试项目中：
- **Generation**: 架构测试代码生成工具（依赖 xUnit）
- **Generator**: 文档和指令生成器（测试辅助工具）
- **Tests**: 规范模型的单元测试
- **Tests/Infrastructure**: 测试辅助工具（RuleIdAssertions、RuleSetValidator、TestDataBuilder）

### 单向依赖原则

```
Generators/ArchitectureTests → Specification
```

**规则**：
- ✅ 测试项目可以引用 Specification
- ✅ 生成器可以引用 Specification
- ❌ Specification **不应**引用测试框架（xUnit、FluentAssertions）
- ❌ Specification **不应**引用仓库特定的实现

## 架构原则

1. **纯规范模型**：仅包含常量、枚举、只读集合等规范定义
2. **零测试依赖**：不引用任何测试框架
3. **最小依赖**：仅依赖 .NET 基础类库（System.*）
4. **向后兼容**：使用 `[Obsolete]` 标记废弃的 API
5. **文档优先**：所有公共 API 必须有 XML 文档注释

## 版本历史

### v1.0.0 (2026-02-12)
- 从 `src/tests/ArchitectureTests/Specification` 迁移到独立工具库
- 更新命名空间为 `Zss.BilliardHall.Specification`
- 移除测试框架依赖
- 迁移文件：
  - `ArchitectureTestSpecification.cs`
  - `_Adr.cs`
  - `_Namespaces.cs`
  - `_Semantics.cs`
  - `_Output.cs`
  - `_Onboarding.cs`

## 相关资源

- **测试项目**: `src/tests/ArchitectureTests`
- **ADR 文档**: `docs/adr`
- **架构分析器**: `src/tools/ArchitectureAnalyzers`
- **ADR 解析器**: `src/tools/AdrSemanticParser`
