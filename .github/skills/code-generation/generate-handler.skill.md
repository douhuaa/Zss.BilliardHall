---
name: "Generate Handler"
description: "生成符合规范的 Handler 代码"
version: "1.0"
risk_level: "高"
category: "代码生成"
required_agent: "architecture-guardian"
dependencies:
  - "verify-module-structure"  # 前置：验证模块结构存在
  - "check-naming-conventions"  # 前置：检查命名规范
post_execution:
  - "run-architecture-tests"  # 建议：生成后运行架构测试验证
---

# Generate Handler Skill

**类别**：代码生成  
**风险等级**：高  
**版本**：1.0

---

## 功能定义

### 用途

根据用例需求生成符合 ADR-005 规范的 Handler 代码，确保遵循 CQRS 原则和垂直切片架构。

### 输入参数

- `module`：字符串，模块名称（如 "Orders"）
- `useCase`：字符串，用例名称（如 "CreateOrder"）
- `handlerType`：字符串枚举，"Command" 或 "Query"
- `returnType`：字符串，返回类型（Command 必须是 void/Task/ID，Query 必须是 DTO）
- `dependencies`：字符串数组，依赖项列表（如 ["IOrderRepository", "IEventBus"]）

### 输出结果

```json
{
  "generated": true,
  "files": [
    {
      "path": "src/Modules/Orders/UseCases/CreateOrder/CreateOrderHandler.cs",
      "content": "...",
      "type": "Handler"
    }
  ],
  "summary": {
    "handlerType": "Command",
    "returnType": "Guid",
    "dependencies": 2,
    "linesOfCode": 45
  },
  "validation": {
    "followsCQRS": true,
    "followsVerticalSlice": true,
    "architectureTestsPass": true
  }
}
```

---

## 前置条件

### 必须满足的条件

- [ ] 模块已存在
- [ ] 用例目录结构已创建
- [ ] 已定义 Command/Query 类型
- [ ] 依赖项接口已定义

### 必须的 Agent 授权

- **需要**：`architecture-guardian`
- **理由**：生成 Handler 代码直接影响架构合规性，必须由架构守护者授权

---

## 执行步骤

1. **验证输入参数**
  - 检查模块存在
  - 验证 handlerType 有效
  - 验证 returnType 符合 CQRS 规则

2. **检查文件冲突**
  - 检查 Handler 文件是否已存在
  - 如存在，询问是否覆盖

3. **生成 Handler 代码**
  - 根据 handlerType 选择模板
  - 注入依赖项
  - 设置返回类型
  - 添加标准注释

4. **生成目录结构**
  - 确保垂直切片目录存在
  - 创建 Handler 文件

5. **验证生成结果**
  - 检查语法正确性
  - 验证符合架构规范
  - 运行架构测试

6. **记录日志**
  - 记录生成参数
  - 记录生成时间
  - 记录验证结果

---

## 代码模板

### Command Handler 模板

```csharp
using Zss.BilliardHall.BuildingBlocks.Application.Messaging;

namespace Zss.BilliardHall.Modules.{Module}.UseCases.{UseCase};

/// <summary>
/// {UseCase} 命令处理器
/// </summary>
public class {UseCase}Handler : ICommandHandler<{UseCase}>
{
    private readonly {Dependencies}

    public {UseCase}Handler({DependencyParams})
    {
        {DependencyAssignments}
    }

    public async Task<{ReturnType}> Handle({UseCase} command)
    {
        // 1. 加载/创建聚合根
        
        // 2. 执行业务逻辑（在领域模型中）
        
        // 3. 保存更改
        
        // 4. 发布领域事件（可选）
        
        return {ReturnValue};
    }
}
```

### Query Handler 模板

```csharp
using Zss.BilliardHall.BuildingBlocks.Application.Messaging;

namespace Zss.BilliardHall.Modules.{Module}.UseCases.{UseCase};

/// <summary>
/// {UseCase} 查询处理器
/// </summary>
public class {UseCase}Handler : IQueryHandler<{UseCase}, {ReturnType}>
{
    private readonly {Dependencies}

    public {UseCase}Handler({DependencyParams})
    {
        {DependencyAssignments}
    }

    public async Task<{ReturnType}> Handle({UseCase} query)
    {
        // 1. 执行查询
        
        // 2. 映射到 DTO
        
        return dto;
    }
}
```

---

## 验证规则

### CQRS 验证

**Command Handler 必须**：
- [ ] 返回 void/Task/ID 类型
- [ ] 不返回 DTO
- [ ] 可以发布事件

**Query Handler 必须**：
- [ ] 返回 DTO
- [ ] 不修改状态
- [ ] 不发布事件

### 垂直切片验证

- [ ] Handler 位于用例目录
- [ ] Handler 是该用例的唯一权威
- [ ] 不创建横向 Service 层

---

## 回滚机制

### 如何回滚

1. 备份已生成的文件列表
2. 删除生成的 Handler 文件
3. 恢复之前的文件（如有覆盖）
4. 清理生成的目录（如为空）

### 回滚验证

- [ ] 生成的文件已删除
- [ ] 原文件已恢复（如有）
- [ ] 架构测试仍然通过

---

## 危险信号

🚨 **必须阻止**：
- Command Handler 返回 DTO
- Query Handler 修改状态
- Handler 位于横向 Service 层
- 缺少必要的依赖项

---

## 使用示例

### 示例 1：生成 Command Handler

**输入**：
```json
{
  "module": "Orders",
  "useCase": "CreateOrder",
  "handlerType": "Command",
  "returnType": "Guid",
  "dependencies": ["IOrderRepository", "IEventBus"]
}
```

**输出**：
- 生成 `CreateOrderHandler.cs`
- 包含标准 Command Handler 结构
- 返回订单 ID

### 示例 2：生成 Query Handler

**输入**：
```json
{
  "module": "Orders",
  "useCase": "GetOrderById",
  "handlerType": "Query",
  "returnType": "OrderDto",
  "dependencies": ["IOrderRepository"]
}
```

**输出**：
- 生成 `GetOrderByIdHandler.cs`
- 包含标准 Query Handler 结构
- 返回 OrderDto

---

## RuleSet API 集成

### 使用 RuleSetRegistry 查询 Handler 约束

生成 Handler 代码时，应从 RuleSetRegistry 获取 CQRS 和垂直切片规则：

```csharp
using Zss.BilliardHall.Specification.Index;

// 获取 ADR-005 规则集（应用内交互模型 - Handler 规范）
var adr005 = RuleSetRegistry.GetStrict(5);
Console.WriteLine($"📋 基于 ADR-{adr005.AdrNumber:D3} 生成 Handler");
Console.WriteLine($"   CQRS 规则数: {adr005.RuleCount}");

// 获取 ADR-001 规则集（垂直切片架构）
var adr001 = RuleSetRegistry.GetStrict(1);
// 使用规则验证 Handler 位置和边界
```

### Handler 规则验证

根据 RuleSet 验证 Handler 是否符合规范：

```csharp
// 验证 Command Handler 返回类型（基于 ADR-005）
var commandRule = adr005.GetRule(2); // Command Handler 规则
if (handlerType == "Command")
{
    // 检查返回类型必须是 void/Task/ID
    var validReturnTypes = new[] { "void", "Task", "Guid", "int", "long" };
    if (!validReturnTypes.Contains(returnType))
    {
        // 违反规则
        var ruleId = new ArchitectureRuleId(5, 2, 1);
        Console.WriteLine($"❌ 违反 {ruleId}：Command Handler 不能返回 DTO");
    }
}

// 验证 Query Handler 返回类型（基于 ADR-005）
var queryRule = adr005.GetRule(3); // Query Handler 规则
if (handlerType == "Query")
{
    // 检查返回类型必须是 DTO
    if (returnType.EndsWith("Command") || returnType == "void")
    {
        var ruleId = new ArchitectureRuleId(5, 3, 1);
        Console.WriteLine($"❌ 违反 {ruleId}：Query Handler 必须返回 DTO");
    }
}
```

### 参考实现

当前没有专用的 GenerateHandlerCommandHandler，但可以参考：
- `GenerateTestCommandHandler.cs` - 使用 RuleSetRegistry 的模式
- `RunArchitectureTestsCommandHandler.cs` - RuleId 验证模式
- `IRuleSetQueryService` - 统一查询接口

---

## 审计日志

```json
{
  "timestamp": "2026-01-26T10:30:00Z",
  "skill": "generate-handler",
  "agent": "architecture-guardian",
  "user": "developer@example.com",
  "parameters": {
    "module": "Orders",
    "useCase": "CreateOrder",
    "handlerType": "Command"
  },
  "result": "success",
  "filesGenerated": 1,
  "duration_ms": 345
}
```

---

## 参考资料

- **RuleSetRegistry API**：`src/tools/Specification/Index/RuleSetRegistry.cs`
- **IRuleSetQueryService**：`src/tools/Specification/Services/IRuleSetQueryService.cs`
- [ADR-005：应用内交互模型](../../../docs/adr/constitutional/ADR-005-Application-Interaction-Model-Final.md)
- [ADR-001：垂直切片架构](../../../docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)
- [后端开发指令](../../instructions/backend.instructions.md)

---

**维护者**：架构委员会  
**状态**：✅ Active  
**版本**：1.1  
**最后更新**：2026-02-20  
**变更**：集成 RuleSetRegistry API，移除硬编码规则引用
