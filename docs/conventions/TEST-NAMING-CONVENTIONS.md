# 测试命名与组织规范

**版本**: 1.0  
**最后更新**: 2026-02-09  
**状态**: Active  
**依据**: ADR-122

---

## 概述

本文档定义项目中所有测试的命名和组织规范，确保测试代码的一致性和可维护性。

**适用范围**：
- 架构测试（Architecture Tests）
- 单元测试（Unit Tests）
- 集成测试（Integration Tests）

---

## 一、架构测试命名规范

### 1.1 测试文件命名

**格式**：`ADR_XXX_Y_Architecture_Tests.cs`

**说明**：
- `XXX`：ADR 编号（3 位数字，如 001, 122）
- `Y`：Rule 编号（1 位或多位数字）
- `Architecture_Tests`：固定后缀

**示例**：
```
ADR_001_1_Architecture_Tests.cs
ADR_122_1_Architecture_Tests.cs
ADR_900_3_Architecture_Tests.cs
```

**存放路径**：`src/tests/ArchitectureTests/`

### 1.2 测试方法命名

**格式**：`ADR_XXX_Y_Z_<DescriptiveName>`

**示例**：
```csharp
[Fact]
[Trait("Category", "Architecture")]
[Trait("ADR", "ADR-001")]
public void ADR_001_1_1_Modules_Should_Not_Reference_Other_Modules()
{
    // 测试实现...
}
```

---

## 二、单元测试命名规范

### 2.1 测试文件命名

**格式**：`{ClassName}Tests.cs`

**示例**：
```
CreateOrderHandlerTests.cs
OrderRepositoryTests.cs
OrderValidatorTests.cs
```

### 2.2 测试方法命名

**格式**：`MethodName_Scenario_ExpectedResult`

**示例**：
```csharp
[Fact]
public void Handle_ValidCommand_ReturnsOrderId()
{
    // Arrange, Act, Assert
}

[Fact]
public void Handle_InvalidCommand_ThrowsValidationException()
{
    // Arrange, Act, Assert
}
```

---

## 三、测试命令规范

### 运行所有架构测试

```bash
dotnet test src/tests/ArchitectureTests/ \
    --filter "Category=Architecture" \
    --logger "console;verbosity=detailed"
```

### 运行特定 ADR 的测试

```bash
dotnet test src/tests/ArchitectureTests/ \
    --filter "FullyQualifiedName~ADR_001" \
    --logger "console;verbosity=detailed"
```

### 运行所有单元测试

```bash
dotnet test src/tests/ \
    --filter "Category!=Architecture&Category!=Integration" \
    --logger "console;verbosity=normal"
```

---

## 相关文档

- [ADR-122：测试组织与命名](../adr/structure/ADR-122-test-organization-naming.md)
- [ADR-900：架构测试与 CI 治理元规则](../adr/governance/ADR-900-architecture-tests.md)
- [架构测试指南](../guidelines/ARCHITECTURE-TEST-GUIDELINES.md)

---

**维护责任**：Test Generator Agent  
**状态**：✅ Active
