---
name: "Run Unit Tests"
description: "运行单元测试并分析结果"
version: "1.0"
risk_level: "低"
category: "测试执行"
required_agent: "test-generator"
---

# Run Unit Tests Skill

**类别**：测试执行  
**风险等级**：低  
**版本**：1.0

---

## 功能定义

### 用途

运行项目的单元测试，验证业务逻辑正确性，并提供详细的失败分析和覆盖率报告。

### 输入参数

- `module`：字符串，可选，指定模块名称（如 "Orders"）
- `useCase`：字符串，可选，指定用例名称
- `verbose`：布尔值，是否输出详细信息，默认 false
- `collectCoverage`：布尔值，是否收集覆盖率，默认 false

### 输出结果

```json
{
  "success": true,
  "summary": {
    "totalTests": 145,
    "passed": 145,
    "failed": 0,
    "skipped": 0,
    "duration_ms": 3456
  },
  "coverage": {
    "line": 85.3,
    "branch": 78.2,
    "method": 90.1
  },
  "failures": [],
  "metadata": {
    "timestamp": "2026-01-26T10:30:00Z",
    "testFramework": "xUnit",
    "projectPath": "src/tests/Modules.*.Tests"
  }
}
```

---

## 前置条件

### 必须满足的条件

- [ ] 项目可编译
- [ ] 单元测试项目存在
- [ ] 测试依赖已安装（xUnit, FluentAssertions, NSubstitute）

### 必须的 Agent 授权

- **需要**：`test-generator`
- **理由**：运行测试是低风险操作，结果分析需要测试专业知识

---

## 执行步骤

1. **验证环境**
  - 检查项目可编译
  - 验证测试框架可用

2. **运行测试**
  - 使用 `dotnet test` 命令
  - 应用过滤器（如指定模块）
  - 收集覆盖率（如需要）

3. **分析结果**
  - 提取失败测试
  - 分析失败原因
  - 统计覆盖率

4. **生成报告**
  - 汇总统计信息
  - 列出失败详情
  - 覆盖率报告

5. **记录日志**

---

## 测试执行命令

### 运行所有单元测试

```bash
dotnet test src/tests/ \
  --filter "Category!=Architecture&Category!=Integration" \
  --logger "console;verbosity=normal"
```

### 运行特定模块的测试

```bash
dotnet test src/tests/Modules.Orders.Tests/ \
  --logger "console;verbosity=normal"
```

### 运行特定用例的测试

```bash
dotnet test src/tests/Modules.Orders.Tests/ \
  --filter "FullyQualifiedName~CreateOrderHandlerTests" \
  --logger "console;verbosity=detailed"
```

### 收集覆盖率

```bash
dotnet test src/tests/ \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage
```

---

## 报告格式

### 控制台输出

```
=== 单元测试结果 ===

模块: Orders
总计: 45 个测试
✅ 通过: 45
❌ 失败: 0
⏭️ 跳过: 0

耗时: 1.2 秒

=== 覆盖率 ===
行覆盖率: 85.3%
分支覆盖率: 78.2%
方法覆盖率: 90.1%

---

运行完成。所有测试通过。
```

### 失败详情

```
❌ CreateOrderHandlerTests.Handle_InvalidMemberId_ThrowsException
   
   失败原因: 预期抛出 InvalidOperationException，但未抛出异常
   
   位置: CreateOrderHandlerTests.cs:45
   
   堆栈跟踪:
   at CreateOrderHandlerTests.Handle_InvalidMemberId_ThrowsException()
   
   建议:
   检查 Handler 是否正确验证 MemberId
```

---

## 覆盖率分析

### 覆盖率阈值

| 类型 | 最低阈值 | 推荐阈值 |
|-----|---------|---------|
| 行覆盖率 | 70% | 85% |
| 分支覆盖率 | 60% | 75% |
| 方法覆盖率 | 80% | 90% |

### 覆盖率不足警告

```
⚠️ 覆盖率不足

以下文件覆盖率低于阈值：

- CreateOrderHandler.cs: 65% (目标: 85%)
  缺失覆盖:
  - Line 45-50: 异常处理分支
  - Line 60-62: 边界情况

建议: 添加以下测试
- Handle_InvalidItems_ThrowsException
- Handle_EmptyItems_ThrowsException
```

---

## 验证规则

### 测试质量检查

- [ ] 所有测试已运行
- [ ] 覆盖率达标
- [ ] 失败测试有详细信息
- [ ] 报告格式清晰

---

## 危险信号

⚠️ **警告**：
- 覆盖率显著下降
- 大量测试失败
- 测试运行时间过长
- 测试不稳定（间歇性失败）

---

## 使用示例

### 示例 1：运行所有单元测试

**输入**：
```json
{
  "verbose": false,
  "collectCoverage": true
}
```

**输出**：
- 运行全部单元测试
- 收集覆盖率
- 简洁报告

### 示例 2：运行特定模块测试

**输入**：
```json
{
  "module": "Orders",
  "verbose": true
}
```

**输出**：
- 只运行 Orders 模块测试
- 详细输出
- 完整报告

### 示例 3：运行特定用例测试

**输入**：
```json
{
  "module": "Orders",
  "useCase": "CreateOrder",
  "verbose": true
}
```

**输出**：
- 只运行 CreateOrder 相关测试
- 详细输出
- 针对性反馈

---

## 参考资料

- [ADR-122：测试组织规范](../../../docs/adr/structure/ADR-122-test-organization-naming.md)
- [ARCHITECTURE-TEST-GUIDELINES.md](../../../docs/guidelines/ARCHITECTURE-TEST-GUIDELINES.md) - 架构测试编写指南
- [测试编写指令](../../instructions/test-generator.instructions.yaml)

---

## 最佳实践

### 测试命名规范

- 测试方法：`MethodName_Scenario_ExpectedResult`
- 测试类：`{ClassName}Tests`
- 测试文件：`{ClassName}Tests.cs`

### 测试组织

- 使用 Arrange-Act-Assert (AAA) 模式
- 每个测试只验证一个行为
- 使用 FluentAssertions 提高可读性
- 使用 NSubstitute 创建 Mock 对象

### 持续集成

```bash
# CI 管道中的单元测试
dotnet test src/tests/ \
  --filter "Category!=Architecture&Category!=Integration" \
  --logger "trx;LogFileName=unit-test-results.trx" \
  --logger "console;verbosity=normal" \
  --collect:"XPlat Code Coverage" \
  --results-directory ./TestResults
```

---

**维护者**：架构委员会  
**状态**：✅ Active  
**版本**：1.1  
**最后更新**：2026-02-06
