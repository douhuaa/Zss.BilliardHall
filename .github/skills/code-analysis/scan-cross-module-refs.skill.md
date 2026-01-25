---
name: "Scan Cross-Module References"
description: "扫描跨模块引用"
version: "1.0"
risk_level: "低"
category: "代码分析"
required_agent: "module-boundary-checker"
---

# Scan Cross-Module References Skill

**类别**：代码分析  
**风险等级**：低  
**版本**：1.0

---

## 功能定义

### 用途

扫描项目中的跨模块引用，检测模块边界违规。

### 输入参数

- `sourceModule`：字符串，要检查的模块名称（如 "Orders"）
- `targetModules`：字符串数组，可选，要检查的目标模块列表（如 ["Members", "Products"]，不指定则检查所有模块）
- `includeTests`：布尔值，是否包含测试代码，默认 false

### 输出结果

```json
{
  "sourceModule": "Orders",
  "violations": [
    {
      "file": "UseCases/CreateOrder/CreateOrderHandler.cs",
      "line": 15,
      "type": "DirectReference",
      "targetModule": "Members",
      "targetType": "Zss.BilliardHall.Modules.Members.Domain.Member",
      "severity": "High"
    }
  ],
  "summary": {
    "totalViolations": 1,
    "highSeverity": 1,
    "mediumSeverity": 0,
    "lowSeverity": 0
  }
}
```

---

## 前置条件

### 必须满足的条件

- [ ] 项目已成功编译
- [ ] 存在 source 模块
- [ ] 有访问项目文件的权限

### 必须的 Agent 授权

- **需要**：`module-boundary-checker` 或 `architecture-guardian`
- **理由**：此 Skill 用于检测模块边界违规，必须由负责模块边界的 Agent 授权

---

## 执行步骤

1. **验证输入参数**
  - 检查 sourceModule 是否存在
  - 验证 targetModules 列表有效性

2. **扫描 using 语句**
  - 读取 sourceModule 下所有 .cs 文件
  - 提取 `using` 语句
  - 识别跨模块引用

3. **分析引用类型**
  - Direct Reference：直接引用其他模块的类型
  - Namespace Reference：引用其他模块的命名空间
  - Assembly Reference：引用其他模块的程序集

4. **评估严重性**
  - High：直接引用 Domain 层
  - Medium：引用非 Contracts 的公共类型
  - Low：引用 Contracts（需进一步验证使用方式）

5. **生成报告**
  - 按文件分组
  - 按严重性排序
  - 提供修复建议

6. **记录日志**
```json
{
  "timestamp": "2026-01-25T10:30:00Z",
  "skill": "scan-cross-module-refs",
  "agent": "module-boundary-checker",
  "parameters": {
    "sourceModule": "Orders",
    "targetModules": ["Members"]
  },
  "result": "success",
  "violationsFound": 1
}
```

---

## 使用示例

### 示例 1：检查单个模块

```
Agent: module-boundary-checker
Skill: scan-cross-module-refs
Parameters:
  sourceModule: "Orders"
  targetModules: ["Members"]
```

输出：
```
⚠️ 发现 1 个跨模块引用违规

文件：UseCases/CreateOrder/CreateOrderHandler.cs:15
违规：Direct Reference
目标：Zss.BilliardHall.Modules.Members.Domain.Member
严重性：High

建议：
- 使用契约查询：GetMemberById
- 使用领域事件通信
- 使用原始类型（Guid memberId）
```

### 示例 2：检查所有模块

```
Agent: architecture-guardian
Skill: scan-cross-module-refs
Parameters:
  sourceModule: "Orders"
```

输出：
```
✅ 未发现跨模块引用违规

已检查：
- Orders → Members: 0 违规
- Orders → Products: 0 违规
- Orders → Platform: 0 违规（允许）
```

---

## 检测规则

### 规则 1：禁止的 using 语句

```csharp
// ❌ High Severity
using Zss.BilliardHall.Modules.Members.Domain;
using Zss.BilliardHall.Modules.Members.UseCases;
using Zss.BilliardHall.Modules.Members.Infrastructure;

// ⚠️ Medium Severity（需进一步检查）
using Zss.BilliardHall.Modules.Members.Contracts;

// ✅ 允许
using Zss.BilliardHall.Platform;
using Zss.BilliardHall.Application;
```

### 规则 2：允许的例外

```csharp
// ✅ 允许：在 Application 层编排多个模块
// 文件：Application/Features/SomeFeature/Handler.cs
using Zss.BilliardHall.Modules.Orders.Contracts;
using Zss.BilliardHall.Modules.Members.Contracts;

// ✅ 允许：测试代码
// 文件：tests/**/*.cs
using Zss.BilliardHall.Modules.Orders.Domain;
```

---

## 错误处理

### 错误 1：模块不存在

```
❌ 错误：找不到模块 "Orders"

可能原因：
- 模块名称拼写错误
- 模块尚未创建
- 项目结构变更

建议：
- 检查 src/Modules/ 目录
- 确认模块名称正确
```

### 错误 2：无权限访问

```
❌ 错误：无权限读取文件

需要的权限：
- 读取项目文件
- 访问源代码目录

建议：
- 检查文件权限
- 确认 Agent 有正确的授权
```

---

## 性能考虑

- **小型项目**（< 100 文件）：< 1 秒
- **中型项目**（100-500 文件）：1-3 秒
- **大型项目**（> 500 文件）：3-10 秒

如果扫描时间过长，考虑：
- 只扫描变更的文件
- 并行处理多个文件
- 缓存扫描结果

---

## 版本历史

| 版本  | 日期         | 变更说明 |
|-----|------------|------|
| 1.0 | 2026-01-25 | 初始版本 |

---

**维护者**：架构委员会  
**状态**：✅ Active
