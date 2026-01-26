---
name: "Run Architecture Tests"
description: "运行架构测试并分析结果"
version: "1.0"
risk_level: "低"
category: "测试执行"
required_agent: "test-generator"
---

# Run Architecture Tests Skill

**类别**：测试执行  
**风险等级**：低  
**版本**：1.0

---

## 功能定义

### 用途

运行项目的架构测试，验证代码是否符合 ADR 约束，并提供详细的失败分析。

### 输入参数

- `adrFilter`：字符串，可选，指定 ADR 编号（如 "0001"），不指定则运行所有架构测试
- `verbose`：布尔值，是否输出详细信息，默认 false
- `failFast`：布尔值，是否在首次失败后停止，默认 false

### 输出结果

```json
{
  "success": false,
  "summary": {
    "totalTests": 15,
    "passed": 13,
    "failed": 2,
    "skipped": 0,
    "duration_ms": 2345
  },
  "failures": [
    {
      "test": "ADR_0001_Modules_Should_Not_Reference_Other_Modules",
      "adr": "ADR-0001",
      "message": "模块 Orders 直接引用了模块 Members（ADR-0001）",
      "violations": [
        {
          "file": "UseCases/CreateOrder/CreateOrderHandler.cs",
          "line": 15,
          "type": "using Zss.BilliardHall.Modules.Members.Domain"
        }
      ],
      "fixSuggestion": "使用领域事件、契约查询或原始类型进行模块间通信"
    }
  ],
  "metadata": {
    "timestamp": "2026-01-26T10:30:00Z",
    "testFramework": "xUnit + NetArchTest",
    "projectPath": "src/tests/ArchitectureTests"
  }
}
```

---

## 前置条件

### 必须满足的条件

- [ ] 项目可编译
- [ ] 架构测试项目存在
- [ ] 测试依赖已安装（xUnit, NetArchTest.Rules）

### 必须的 Agent 授权

- **需要**：`test-generator` 或 `architecture-guardian`
- **理由**：运行测试是低风险操作，但分析结果需要专业知识

---

## 执行步骤

1. **验证环境**
  - 检查项目可编译
  - 验证测试框架可用

2. **运行测试**
  - 使用 `dotnet test` 命令
  - 应用过滤器（如指定 ADR）
  - 收集测试结果

3. **分析结果**
  - 提取失败测试
  - 关联到 ADR 正文
  - 提取违规详情

4. **生成报告**
  - 汇总统计信息
  - 列出失败详情
  - 提供修复建议

5. **记录日志**

---

## 测试执行命令

### 运行所有架构测试

```bash
dotnet test src/tests/ArchitectureTests/ \
  --filter "Category=Architecture" \
  --logger "console;verbosity=detailed"
```

### 运行特定 ADR 的测试

```bash
dotnet test src/tests/ArchitectureTests/ \
  --filter "FullyQualifiedName~ADR_0001" \
  --logger "console;verbosity=detailed"
```

### 快速失败模式

```bash
dotnet test src/tests/ArchitectureTests/ \
  --filter "Category=Architecture" \
  --logger "console;verbosity=minimal" \
  -- xUnit.StopOnFail=true
```

---

## 失败分析

### 关联 ADR 正文

当测试失败时，提取 ADR 编号并关联到具体条款：

```
测试：ADR_0001_Modules_Should_Not_Reference_Other_Modules
失败：模块 Orders 引用了 Members

关联 ADR：ADR-0001.2.1
约束：模块间禁止直接引用
参考：docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md
```

### 提供修复建议

根据失败类型提供具体建议：

**模块引用违规**：
```
修复方案：
1. 使用领域事件（异步）
2. 使用契约查询（只读）
3. 使用原始类型（ID）

参考：docs/copilot/adr-0001.prompts.md
```

---

## 报告格式

### 控制台输出

```
=== 架构测试结果 ===

总计: 15 个测试
✅ 通过: 13
❌ 失败: 2
⏭️ 跳过: 0

耗时: 2.3 秒

=== 失败详情 ===

❌ ADR_0001_Modules_Should_Not_Reference_Other_Modules
   违反: ADR-0001.2.1 - 模块间禁止直接引用
   位置: Orders/UseCases/CreateOrder/CreateOrderHandler.cs:15
   内容: using Zss.BilliardHall.Modules.Members.Domain
   
   修复建议:
   使用以下方式之一进行模块间通信：
   - 领域事件（推荐，异步）
   - 契约查询（同步，只读）
   - 原始类型（传递 ID）
   
   参考: docs/copilot/adr-0001.prompts.md

---

运行完成。2 个测试失败。
```

---

## 验证规则

### 测试有效性检查

- [ ] 所有架构测试已运行
- [ ] 失败测试关联到 ADR
- [ ] 提供了修复建议
- [ ] 报告格式清晰

---

## 危险信号

⚠️ **警告**：
- 大量测试失败（可能是系统性问题）
- 测试无法运行（环境问题）
- 失败无法关联到 ADR（测试问题）

---

## 使用示例

### 示例 1：运行所有架构测试

**输入**：
```json
{
  "verbose": true
}
```

**输出**：
- 运行全部架构测试
- 详细输出
- 完整报告

### 示例 2：运行特定 ADR 的测试

**输入**：
```json
{
  "adrFilter": "0001",
  "verbose": false
}
```

**输出**：
- 只运行 ADR-0001 相关测试
- 简洁输出
- 仅显示失败

### 示例 3：快速失败模式

**输入**：
```json
{
  "failFast": true
}
```

**输出**：
- 在首次失败后停止
- 快速反馈
- 节省时间

---

## 参考资料

- [ADR-0000：架构测试与 CI 治理](../../../docs/adr/constitutional/ADR-0000-architecture-testing-ci-governance-constitution.md)
- [架构测试失败诊断](../../../docs/copilot/architecture-test-failures.md)
- [测试编写指令](../../instructions/testing.instructions.md)

---

**维护者**：架构委员会  
**状态**：✅ Active
