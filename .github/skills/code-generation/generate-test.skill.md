---
name: "Generate Test"
description: "生成符合规范的测试代码"
version: "1.0"
risk_level: "中"
category: "代码生成"
required_agent: "test-generator"
---

# Generate Test Skill

**类别**：代码生成  
**风险等级**：中  
**版本**：1.0

---

## 功能定义

### 用途

根据源代码自动生成符合项目规范的测试代码，包括单元测试、架构测试和集成测试。

### 输入参数

- `testType`：字符串枚举，"Unit" / "Architecture" / "Integration"
- `targetFile`：字符串，目标源文件路径
- `targetClass`：字符串，目标类名
- `scenarios`：字符串数组，可选，测试场景列表
- `includeEdgeCases`：布尔值，是否包含边界情况，默认 true

### 输出结果

```json
{
  "generated": true,
  "files": [
    {
      "path": "tests/Modules.Orders.Tests/UseCases/CreateOrder/CreateOrderHandlerTests.cs",
      "content": "...",
      "scenarios": 5
    }
  ],
  "summary": {
    "testType": "Unit",
    "testCount": 5,
    "coverageScenarios": [
      "正常流程",
      "边界情况",
      "异常处理"
    ]
  }
}
```

---

## 前置条件

### 必须满足的条件

- [ ] 目标源文件存在
- [ ] 项目可编译
- [ ] 测试框架已配置（xUnit + FluentAssertions + NSubstitute）

### 必须的 Agent 授权

- **需要**：`test-generator`
- **理由**：生成测试代码需要测试生成器的专业知识和规范验证

---

## 执行步骤

1. **分析目标代码**
  - 解析类结构
  - 识别方法签名
  - 提取依赖项

2. **确定测试场景**
  - 正常流程
  - 边界情况
  - 异常处理
  - 特殊业务规则

3. **生成测试代码**
  - 选择合适模板
  - 生成 Arrange-Act-Assert 结构
  - 添加 Mock/Stub

4. **创建测试文件**
  - 确保测试目录镜像源代码结构
  - 生成测试类文件

5. **验证生成结果**
  - 检查测试可编译
  - 运行生成的测试
  - 验证测试覆盖率

6. **记录日志**

---

## 测试模板

### Handler 单元测试模板

```csharp
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace Zss.BilliardHall.Tests.Modules.{Module}.UseCases.{UseCase};

public class {ClassName}Tests
{
    [Fact]
    public async Task Handle_ValidInput_ReturnsExpectedResult()
    {
        // Arrange
        var {dependencies} = Substitute.For<{IDependency}>();
        var handler = new {ClassName}({dependencies});
        var {input} = new {InputType}(...);
        
        // Act
        var result = await handler.Handle({input});
        
        // Assert
        result.Should().NotBeNull();
        // 添加更多断言
    }
    
    [Fact]
    public async Task Handle_InvalidInput_ThrowsException()
    {
        // Arrange
        var handler = new {ClassName}(...);
        var {input} = new {InputType}(...); // 无效输入
        
        // Act & Assert
        await handler.Invoking(h => h.Handle({input}))
            .Should().ThrowAsync<{ExceptionType}>();
    }
    
    [Fact]
    public async Task Handle_EdgeCase_HandlesCorrectly()
    {
        // Arrange
        
        // Act
        
        // Assert
    }
}
```

### 领域模型测试模板

```csharp
using FluentAssertions;
using Xunit;

namespace Zss.BilliardHall.Tests.Modules.{Module}.Domain;

public class {ClassName}Tests
{
    [Fact]
    public void Method_ValidScenario_ProducesExpectedResult()
    {
        // Arrange
        var {object} = new {ClassName}(...);
        
        // Act
        {object}.{Method}(...);
        
        // Assert
        {object}.{Property}.Should().Be(...);
    }
    
    [Fact]
    public void Method_InvalidInput_ThrowsException()
    {
        // Arrange
        var {object} = new {ClassName}(...);
        
        // Act & Assert
        {object}.Invoking(o => o.{Method}(...))
            .Should().Throw<{ExceptionType}>();
    }
}
```

---

## 验证规则

### 测试质量检查

- [ ] 使用 Arrange-Act-Assert 模式
- [ ] 测试方法命名清晰：`Method_Scenario_ExpectedResult`
- [ ] 使用 FluentAssertions
- [ ] 每个测试只测试一件事
- [ ] 包含正常流程测试
- [ ] 包含边界情况测试
- [ ] 包含异常处理测试

### 测试结构检查

- [ ] 测试文件名：`{ClassName}Tests.cs`
- [ ] 测试目录镜像源代码结构
- [ ] 测试类命名：`{ClassName}Tests`
- [ ] 测试方法命名规范

---

## 回滚机制

### 如何回滚

1. 删除生成的测试文件
2. 清理空的测试目录
3. 验证原有测试仍可运行

### 回滚验证

- [ ] 生成的测试文件已删除
- [ ] 原有测试通过
- [ ] 测试覆盖率未降低

---

## 危险信号

⚠️ **警告**：
- 生成的测试过于简单
- 缺少边界情况测试
- Mock 使用不当
- 测试依赖具体实现

---

## 使用示例

### 示例 1：为 Handler 生成单元测试

**输入**：
```json
{
  "testType": "Unit",
  "targetFile": "src/Modules/Orders/UseCases/CreateOrder/CreateOrderHandler.cs",
  "targetClass": "CreateOrderHandler",
  "includeEdgeCases": true
}
```

**输出**：
- 生成 `CreateOrderHandlerTests.cs`
- 包含 5 个测试方法
- 覆盖正常流程、边界情况、异常处理

### 示例 2：为领域模型生成测试

**输入**：
```json
{
  "testType": "Unit",
  "targetFile": "src/Modules/Orders/Domain/Order.cs",
  "targetClass": "Order",
  "scenarios": ["ApplyDiscount", "Cancel", "Complete"]
}
```

**输出**：
- 生成 `OrderTests.cs`
- 包含指定场景的测试
- 包含边界和异常测试

---

## 参考资料

- [ADR-0000：架构测试](../../../docs/adr/constitutional/ADR-0000-architecture-testing-ci-governance-constitution.md)
- [ADR-0122：测试组织](../../../docs/adr/structure/ADR-0122-testing-organization.md)
- [测试编写指令](../../instructions/testing.instructions.md)

---

**维护者**：架构委员会  
**状态**：✅ Active
