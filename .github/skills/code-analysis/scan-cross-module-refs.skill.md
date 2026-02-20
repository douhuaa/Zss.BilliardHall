---
name: "Scan Cross-Module References"
description: "扫描跨模块引用"
version: "1.1"
risk_level: "低"
category: "代码分析"
required_agent: "module-boundary-checker"
dependencies:
  - "verify-project-builds"  # 前置：确保项目可编译以加载程序集
post_execution:
  - "post-comment"  # 可选：将扫描结果发布到 PR
---

# Scan Cross-Module References Skill

**类别**：代码分析  
**风险等级**：低  
**版本**：1.1

---

## 功能定义

### 用途

扫描项目中的跨模块引用，提取引用事实，由 Agent 根据 ADR 进行判定。

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
      "targetNamespace": "Zss.BilliardHall.Modules.Members.Domain",
      "targetLayer": "Domain",
      "derivedSeverity": "High",
      "severitySource": "ADR-001.2"
    }
  ],
  "summary": {
    "totalViolations": 1,
    "byLayer": {
      "Domain": 1,
      "UseCases": 0,
      "Infrastructure": 0,
      "Contracts": 0
    }
  },
  "metadata": {
    "scanTimestamp": "2026-01-25T10:30:00Z",
    "filesScanned": 42,
    "scanDurationMs": 234
  }
}
```

**关键说明**：
- `derivedSeverity`：基于当前 ADR（如 ADR-001.2）推导的严重性，**不是 Skill 固化的真理**
- `severitySource`：严重性判定依据的 ADR 条款，便于追溯
- 最终判定由 Agent 根据最新 ADR 执行

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

3. **提取引用事实**
  - 引用类型（DirectReference / NamespaceReference / AssemblyReference）
  - 目标模块名称
  - 目标命名空间
  - 目标层级（Domain / UseCases / Infrastructure / Contracts）

4. **推导严重性（Informational）**
  - 基于当前 ADR-001.2 推导严重性
  - 标注推导依据（ADR 条款）
  - **注意**：此推导仅供参考，Agent 可根据最新 ADR 重新判定

5. **生成报告**
  - 按文件分组
  - 按层级分类统计
  - 记录扫描元数据

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

## RuleSet API 集成

### 使用 RuleSetRegistry 判定跨模块引用规则

扫描工具提取引用事实后，应使用 RuleSetRegistry 进行判定：

```csharp
using Zss.BilliardHall.Specification.Index;

// 获取 ADR-001 规则集（模块边界规则）
var adr001 = RuleSetRegistry.GetStrict(1);
Console.WriteLine($"📋 基于 ADR-{adr001.AdrNumber:D3} 判定跨模块引用");

// 获取模块边界规则
var moduleBoundaryRule = adr001.GetRule(2); // 模块物理隔离规则
var domainIsolationClause = adr001.GetClause(2, 1); // Domain 层隔离条款

// 判定引用是否违规
foreach (var reference in references)
{
    if (reference.TargetArea == "Domain")
    {
        var ruleId = new ArchitectureRuleId(1, 2, 1);
        Console.WriteLine($"❌ 违反 {ruleId}：禁止跨模块引用 Domain 层");
        Console.WriteLine($"   文件: {reference.FilePath}");
        Console.WriteLine($"   目标: {reference.TargetNamespace}");
    }
    else if (reference.TargetArea == "Contracts")
    {
        // Contracts 引用可能合法，需进一步检查
        var contractsClause = adr001.GetClause(3, 1); // Contracts 使用条款
        Console.WriteLine($"⚠️  检查 Contracts 引用是否符合 {new ArchitectureRuleId(1, 3, 1)}");
    }
}
```

### 动态严重性推导（基于 RuleSet）

使用 RuleSet 动态推导严重性，而非硬编码：

```csharp
// 从 RuleSet 获取规则严重程度
var rule = adr001.GetRule(2);
var severity = rule.Severity; // RuleSeverity.Constitutional

// 根据严重程度判定
if (severity == RuleSeverity.Constitutional)
{
    Console.WriteLine("🔴 严重违规：违反宪法级规则，必须修复");
}
else if (severity == RuleSeverity.Governance)
{
    Console.WriteLine("🟡 警告：违反治理规则，需要审查");
}
```

### 参考实现

参见 `src/tools/Governance.Cli/Commands/ScanCrossModuleReferencesCommandHandler.cs`：
- 提取跨模块引用事实
- 不硬编码严重性判定
- 输出建议时引用 ADR 条款

---

## 严重性推导规则（Informational）

> ⚠️ **重要声明**：以下严重性推导基于 ADR-001.2（当前版本），仅供 Agent 参考。
> 
> Agent 应根据**最新 ADR 正文**进行最终裁决，而非依赖此推导结果。

### 推导逻辑（基于 ADR-001.2）

- **High**：直接引用 Domain 层（`*.Domain.*`）
  - 依据：ADR-001.2 明确禁止跨模块 Domain 引用
  
- **Medium**：引用非 Contracts 的其他层（`*.UseCases.*`, `*.Infrastructure.*`）
  - 依据：ADR-001.2 要求模块内部实现隐藏
  
- **Low**：引用 Contracts 层（`*.Contracts.*`）
  - 依据：ADR-001.3 允许 Contracts 跨模块使用，但需验证使用方式

### 例外情况（需 Agent 二次判断）

- Application 层编排多个模块的 Contracts
- 测试代码引用（`tests/**/*.cs`）
- Platform 层引用（允许）

**如果 ADR-001 修订，此推导逻辑可能过时，Agent 不应盲目信任。**

---

## 错误处理

### 错误 1：模块不存在

```json
{
  "error": "ModuleNotFound",
  "message": "找不到模块 'Orders'",
  "suggestions": ["检查模块名称拼写", "确认模块已创建"]
}
```

### 错误 2：无权限访问

```json
{
  "error": "PermissionDenied",
  "message": "无权限读取文件",
  "suggestions": ["检查文件权限", "确认 Agent 授权"]
}
```

---

## 性能指标

| 项目规模 | 文件数 | 预期时间 |
|---------|-------|---------|
| 小型    | < 100 | < 1 秒  |
| 中型    | 100-500 | 1-3 秒 |
| 大型    | > 500 | 3-10 秒 |

**优化建议**：
- 增量扫描（仅变更文件）
- 并行处理
- 结果缓存

---

## 版本历史

| 版本  | 日期         | 变更说明 |
|-----|------------|------|
| 1.2 | 2026-02-20 | 集成 RuleSetRegistry API，使用动态规则判定 |
| 1.1 | 2026-01-25 | 重构：严重性改为推导值，移除人类文档化内容 |
| 1.0 | 2026-01-25 | 初始版本 |

---

**维护者**：架构委员会  
**状态**：✅ Active  
**版本**：1.2  
**最后更新**：2026-02-20  
**变更**：集成 RuleSetRegistry API，使用动态规则判定替代硬编码严重性  
**版本**：1.2  
**最后更新**：2026-02-20  
**变更**：集成 RuleSetRegistry API，使用动态规则判定替代硬编码严重性

---

## 附录：使用指导（Optional - 供人类参考）

> **注意**：以下内容仅供人类开发者理解 Skill 用途，**不作为 Agent 判定依据**。
> 
> Agent 应基于 ADR 正文和 Skill 输出的事实数据进行判定。

### 典型使用流程

1. Agent 调用此 Skill 获取引用事实
2. Agent 根据最新 ADR 判定每个引用
3. Agent 生成符合三态格式的响应（✅ Allowed / ⚠️ Blocked / ❓ Uncertain）

### 修复建议来源

修复建议应由 Agent 根据 ADR 生成，常见方案包括：
- 使用领域事件异步通信（ADR-001.4）
- 使用契约查询传递数据（ADR-001.3）
- 使用原始类型传递标识（ADR-001.3）

详见：`docs/copilot/adr-001.prompts.md`
