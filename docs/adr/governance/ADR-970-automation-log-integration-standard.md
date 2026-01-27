# ADR-970：自动化工具日志集成标准

> ⚖️ **本 ADR 是自动化工具日志管理的唯一裁决源，定义测试报告、CI 日志、工具输出的结构化存储和关联机制。**

**状态**：✅ Accepted  
**版本**：1.0
**级别**：工具集成标准 / 治理层  
**适用范围**：所有自动化工具的日志输出  
**生效时间**：即刻

---

## 聚焦内容（Focus）

- 日志分类与存储位置
- 结构化日志格式（JSON）
- 日志与 ADR 关联机制
- 保留期策略
- 日志分发标准

---

## 术语表（Glossary）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 结构化日志 | 使用 JSON 格式的标准化日志 | Structured Log |
| 架构测试报告 | 架构测试的执行结果 | Architecture Test Report |
| 依赖更新日志 | 依赖包变更的记录 | Dependency Update Log |
| 安全扫描报告 | CodeQL 等安全工具的输出 | Security Scan Report |
| 日志关联 | 日志与对应 ADR 的链接 | Log Correlation |
| 保留期 | 日志保留时长 | Retention Period |

---

## 决策（Decision）

### 日志分类与存储位置（ADR-970.1）

**规则**：

所有自动化工具日志 **必须**按类型存储在标准位置。

**存储结构**：
```
docs/reports/
├── architecture-tests/       # 架构测试报告
│   ├── YYYY-MM-DD-HH-MM.json
│   └── latest.json          # 最新报告的符号链接
│
├── dependencies/             # 依赖更新日志
│   ├── YYYY-MM-DD-updates.json
│   └── dependency-graph.json
│
├── security/                 # 安全扫描报告
│   ├── codeql/
│   │   ├── YYYY-MM-DD-scan.json
│   │   └── latest.json
│   └── dependency-check/
│       ├── YYYY-MM-DD-scan.json
│       └── latest.json
│
├── builds/                   # 构建日志
│   └── YYYY-MM-DD-HH-MM.json
│
└── tests/                    # 测试执行报告
    ├── unit/
    ├── integration/
    └── e2e/
```

**保留期策略**：

| 日志类型 | 保留期 | 清理策略 | 原因 |
|---------|--------|---------|------|
| 架构测试报告 | 30 天 | 自动删除旧报告 | 频繁执行，历史价值有限 |
| 依赖更新日志 | 180 天 | 自动删除旧日志 | 需追溯依赖变更历史 |
| 安全扫描报告 | 永久 | 不删除 | 安全合规要求 |
| 构建日志 | 7 天 | 自动删除旧日志 | 仅用于近期故障排查 |
| 测试报告 | 30 天 | 自动删除旧报告 | 频繁执行，历史价值有限 |

**核心原则**：
> 分类存储，明确保留，易于查找。

**判定**：
- ❌ 日志散落各处
- ❌ 无保留期策略，占用大量空间
- ✅ 标准化存储和保留

---

### 结构化日志格式（ADR-970.2）

**规则**：

所有日志 **必须**使用 JSON 格式，包含标准字段。

**标准 JSON 架构**：
```json
{
  "type": "architecture-test | security-scan | build | test",
  "timestamp": "2026-01-26T13:00:00Z",
  "source": "tool-name",
  "version": "tool-version",
  "status": "success | failure | warning",
  "summary": {
    "total": 100,
    "passed": 95,
    "failed": 5,
    "warnings": 0
  },
  "details": [
    {
      "test": "test-name",
      "adr": "ADR-0001",
      "severity": "error | warning | info",
      "message": "error message",
      "file": "path/to/file.cs",
      "line": 42,
      "fix_guide": "path/to/guide"
    }
  ],
  "metadata": {
    "branch": "main",
    "commit": "abc123",
    "pr": 123,
    "author": "user"
  }
}
```

**字段说明**：

| 字段 | 必须 | 类型 | 说明 |
|------|------|------|------|
| `type` | ✅ | string | 日志类型 |
| `timestamp` | ✅ | ISO 8601 | 生成时间 |
| `source` | ✅ | string | 工具名称 |
| `version` | ✅ | string | 工具版本 |
| `status` | ✅ | enum | 整体状态 |
| `summary` | ✅ | object | 汇总统计 |
| `details` | ✅ | array | 详细结果 |
| `metadata` | ❌ | object | 上下文信息 |

**details 字段说明**：

| 字段 | 必须 | 类型 | 说明 |
|------|------|------|------|
| `test` | ✅ | string | 测试名称 |
| `adr` | ⚠️ | string | 关联的 ADR 编号（如适用） |
| `severity` | ✅ | enum | 严重程度 |
| `message` | ✅ | string | 错误/警告消息 |
| `file` | ❌ | string | 文件路径 |
| `line` | ❌ | number | 行号 |
| `fix_guide` | ❌ | string | 修复指南链接 |

**示例：架构测试报告**：
```json
{
  "type": "architecture-test",
  "timestamp": "2026-01-26T13:00:00Z",
  "source": "NetArchTest",
  "version": "1.3.2",
  "status": "failure",
  "summary": {
    "total": 50,
    "passed": 48,
    "failed": 2,
    "warnings": 0
  },
  "details": [
    {
      "test": "Modules_Should_Not_Reference_Other_Modules",
      "adr": "ADR-0001",
      "severity": "error",
      "message": "Modules.Orders references Modules.Members",
      "file": "src/Modules/Orders/UseCases/CreateOrder/CreateOrderHandler.cs",
      "line": 15,
      "fix_guide": "docs/copilot/adr-0001.prompts.md#scenario-3"
    }
  ],
  "metadata": {
    "branch": "feature/new-order",
    "commit": "abc123def456",
    "pr": 456,
    "author": "developer"
  }
}
```

**核心原则**：
> 标准化格式，易于解析和分析。

**判定**：
- ❌ 纯文本日志，难以解析
- ❌ 格式不一致
- ✅ 标准 JSON 格式

---

### 日志与 ADR 关联机制（ADR-970.3）

**规则**：

测试失败日志 **必须**自动链接到对应的 ADR 和修复指南。

**关联规则**：
1. **测试名称映射**：测试名称必须包含 ADR 编号
   - 格式：`ADR_XXXX_Test_Name`
   - 示例：`ADR_0001_Modules_Should_Not_Reference_Other_Modules`

2. **日志中的 ADR 字段**：
   - 从测试名称自动提取 ADR 编号
   - 填充 `details[].adr` 字段

3. **修复指南链接**：
   - 映射到 `docs/copilot/adr-XXXX.prompts.md`
   - 具体到场景或章节（如适用）

**ADR 中的反向链接**：

每个 ADR **应该**包含相关测试和日志位置：

```markdown
## 执法模型（Enforcement）

| 规则编号 | 执行级别 | 测试/手段 | 说明 |
|---------|---------|----------|------|
| ADR-0001.1 | L1 | `Modules_Should_Not_Reference_Other_Modules` | 模块隔离测试 |

**相关日志**：
- 测试报告：`docs/reports/architecture-tests/latest.json`
- 修复指南：`docs/copilot/adr-0001.prompts.md`
```

**CI 失败通知增强**：

当测试失败时，CI 通知 **应该**包含：
```
❌ 架构测试失败

测试：Modules_Should_Not_Reference_Other_Modules
ADR：ADR-0001 - 模块化单体与垂直切片架构
错误：Modules.Orders references Modules.Members

📖 了解详情：
- ADR 正文：docs/adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md
- 修复指南：docs/copilot/adr-0001.prompts.md#scenario-3
- 测试报告：docs/reports/architecture-tests/latest.json
```

**核心原则**：
> 测试失败即知道查看哪个 ADR，ADR 即知道查看哪个测试。

**判定**：
- ❌ 测试失败但不知道查看哪个 ADR
- ❌ ADR 中未列出相关测试
- ✅ 双向关联，快速定位

---

### 自动化报告生成（ADR-970.4）

**规则**：

CI **必须**自动生成结构化日志并存储。

**CI Workflow 示例**：
```yaml
name: Architecture Tests

on: [push, pull_request]

jobs:
  architecture-tests:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Run Architecture Tests
        run: dotnet test src/tests/ArchitectureTests/ --logger "json;LogFilePath=reports/architecture-tests.json"
      
      - name: Convert to Standard Format
        run: |
          node scripts/convert-test-report.js \
            --input reports/architecture-tests.json \
            --output docs/reports/architecture-tests/$(date +%Y-%m-%d-%H-%M).json \
            --format standard
      
      - name: Update Latest Symlink
        run: |
          ln -sf $(date +%Y-%m-%d-%H-%M).json docs/reports/architecture-tests/latest.json
      
      - name: Upload Report
        uses: actions/upload-artifact@v3
        with:
          name: architecture-test-report
          path: docs/reports/architecture-tests/latest.json
      
      - name: Comment on PR
        if: failure() && github.event_name == 'pull_request'
        uses: actions/github-script@v6
        with:
          script: |
            const report = require('./docs/reports/architecture-tests/latest.json');
            const failures = report.details.filter(d => d.severity === 'error');
            const comment = failures.map(f => 
              `❌ **${f.test}**\n` +
              `ADR: [${f.adr}](${f.fix_guide})\n` +
              `错误: ${f.message}\n`
            ).join('\n---\n');
            
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: comment
            });
```

**脚本职责**：
- `scripts/convert-test-report.js`：转换工具原始输出到标准格式
- 提取 ADR 编号
- 生成修复指南链接
- 添加元数据

**核心原则**：
> 自动生成，自动关联，自动通知。

**判定**：
- ❌ 手动整理测试报告
- ❌ CI 失败但不生成报告
- ✅ 自动生成标准化报告

---

### 日志分发与访问（ADR-970.5）

**规则**：

日志 **必须**易于访问，并在适当时自动分发。

**访问方式**：
1. **本地访问**：`docs/reports/` 目录
2. **CI Artifacts**：GitHub Actions Artifacts
3. **PR 评论**：失败时自动评论
4. **通知**：严重失败时通知团队

**分发规则**：

| 事件 | 分发方式 | 接收方 | 内容 |
|------|---------|--------|------|
| 架构测试失败 | PR 评论 | PR 作者 + Reviewers | 失败详情 + 修复链接 |
| 安全扫描发现高危 | Issue + Email | 架构委员会 | 漏洞详情 + 修复建议 |
| 依赖重大更新 | PR 评论 | PR 作者 | 变更说明 + 影响评估 |
| 构建失败 | PR 评论 | PR 作者 | 失败日志摘要 |

**访问权限**：
- `docs/reports/` 目录在仓库中，所有成员可访问
- 敏感安全报告可配置访问限制

**核心原则**：
> 主动分发关键信息，降低查找成本。

**判定**：
- ❌ 开发者需主动去 CI 查看日志
- ❌ 关键失败无人知晓
- ✅ 自动分发到相关方

---

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md) - 基于其 CI 测试机制
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md) - 基于其文档组织

**被依赖（Depended By）**：
- 无

**替代（Supersedes）**：
- 无

**被替代（Superseded By）**：
- 无

**相关（Related）**：
- [ADR-940：ADR 关系与溯源管理宪法](../governance/ADR-940-adr-relationship-traceability-management.md) - 日志与 ADR 关联
- [ADR-980：ADR 生命周期一体化同步机制](../governance/ADR-980-adr-lifecycle-synchronization.md) - 版本同步检测

---

## 执法模型（Enforcement）

| 规则编号 | 执行级别 | 测试/手段 | 说明 |
|---------|---------|----------|------|
| ADR-970.1 | L2 | Code Review | 人工审查日志存储位置 |
| ADR-970.2 | L1 | JSON Schema Validation | 自动验证日志格式 |
| ADR-970.3 | L1 | CI Script | 自动提取 ADR 编号和关联 |
| ADR-970.4 | L1 | CI Workflow | 自动生成报告 |
| ADR-970.5 | L1 | CI Workflow | 自动分发通知 |

---

## 破例与归还（Exception）

### 允许破例的前提

破例 **仅在以下情况允许**：
- 工具不支持 JSON 输出（需转换脚本）
- 遗留工具迁移期（6 个月宽限）
- 外部工具集成复杂度过高（需评估）

### 破例要求

每个破例 **必须**：
- 记录在 Issue 中，说明原因和预期完成时间
- 标记 `log-format-exception` 标签
- 提供临时解决方案
- 架构委员会批准

**未记录的破例 = 未授权治理违规。**

---

## 变更政策（Change Policy）

### 变更规则

本 ADR 属于 **治理层工具集成规则**：
- 修改需架构委员会同意
- 需更新所有相关 CI Workflow 和脚本
- 需保持向后兼容（至少 3 个月）

### 失效与替代

- 本 ADR 一旦被替代，**必须**迁移所有现有日志到新格式
- 不允许"隐性废弃"

---

## 明确不管什么（Non-Goals）

本 ADR **不负责**：
- 日志内容的详细程度（由工具决定）
- 日志的可视化展示（可另行开发工具）
- 应用运行时日志（仅管理 CI 工具日志）
- 日志分析和统计（可另行开发）
- 长期归档策略（超过保留期的日志）

---

## 非裁决性参考（References）

### 相关 ADR
- [ADR-0000：架构测试与 CI 治理宪法](../governance/ADR-0000-architecture-tests.md)
- [ADR-0008：文档编写与维护宪法](../constitutional/ADR-0008-documentation-governance-constitution.md)
- [ADR-940：ADR 关系与溯源管理宪法](../governance/ADR-940-adr-relationship-traceability-management.md)

### 实施工具

**已实施**（2026-01-27）：
- `scripts/lib/json-output.sh` - 通用 JSON 输出库（依据 ADR-970.2）
- `scripts/validate-adr-consistency.sh` - ADR 一致性验证（支持 JSON 输出）
- `scripts/validate-three-way-mapping.sh` - 三位一体映射验证（支持 JSON 输出）
- `docs/reports/` - 标准化日志存储目录（依据 ADR-970.1）

**待实施**：
- `scripts/convert-test-report.js` - 报告格式转换（计划中）
- `.github/workflows/` - CI Workflows 集成（计划中）
- JSON Schema 定义文件（计划中）
- 其他验证脚本的 JSON 输出支持（进行中）

### 背景材料
- [ADR-Documentation-Governance-Gap-Analysis.md](../proposals/ADR-Documentation-Governance-Gap-Analysis.md) - 原始提案

---

## 版本历史（Version History）

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|----------|------|
| 1.0 | 2026-01-26 | 初版：定义自动化工具日志集成标准 | GitHub Copilot |

---

**维护**：架构委员会 & DevOps Team  
**审核**：待定  
**状态**：✅ Accepted
**版本**：1.0
