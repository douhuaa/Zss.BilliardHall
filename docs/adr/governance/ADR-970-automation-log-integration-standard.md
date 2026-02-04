---
adr: ADR-970
title: "自动化工具日志集成标准"
status: Accepted
level: Governance
deciders: "Architecture Board & DevOps Team"
date: 2026-02-03
version: "2.0"
maintainer: "Architecture Board & DevOps Team"
primary_enforcement: L1
reviewer: "待定"
supersedes: null
superseded_by: null
---


# ADR-970：自动化工具日志集成标准

> ⚖️ **本 ADR 是自动化工具日志管理的唯一裁决源，定义测试报告、CI 日志、工具输出的结构化存储和关联机制。**

**状态**：✅ Accepted  
## Focus（聚焦内容）

- 日志分类与存储位置
- 结构化日志格式（JSON）
- 日志与 ADR 关联机制
- 保留期策略
- 日志分发标准

---

---

## Glossary（术语表）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 结构化日志 | 使用 JSON 格式的标准化日志 | Structured Log |
| 架构测试报告 | 架构测试的执行结果 | Architecture Test Report |
| 依赖更新日志 | 依赖包变更的记录 | Dependency Update Log |
| 安全扫描报告 | CodeQL 等安全工具的输出 | Security Scan Report |
| 日志关联 | 日志与对应 ADR 的链接 | Log Correlation |
| 保留期 | 日志保留时长 | Retention Period |

---

---

## Decision（裁决）

> ⚠️ **本节为唯一裁决来源，所有条款具备执行级别。**
> 
> 🔒 **统一铁律**：
> 
> ADR-970 中，所有可执法条款必须具备稳定 RuleId，格式为：
> ```
> ADR-970_<Rule>_<Clause>
> ```

---

### ADR-970_1：日志分类与存储位置（Rule）

#### ADR-970_1_1 必须按类型存储在标准位置
所有自动化工具日志 **必须**按类型存储在标准位置。

#### ADR-970_1_2 存储结构
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

#### ADR-970_1_3 保留期策略
**保留期策略**：

| 日志类型 | 保留期 | 清理策略 | 原因 |
|---------|--------|---------|------|
| 架构测试报告 | 30 天 | 自动删除旧报告 | 频繁执行，历史价值有限 |
| 依赖更新日志 | 180 天 | 自动删除旧日志 | 需追溯依赖变更历史 |
| 安全扫描报告 | 永久 | 不删除 | 安全合规要求 |
| 构建日志 | 7 天 | 自动删除旧日志 | 仅用于近期故障排查 |
| 测试报告 | 30 天 | 自动删除旧报告 | 频繁执行，历史价值有限 |

#### ADR-970_1_4 核心原则
**核心原则**：
> 分类存储，明确保留，易于查找。

**判定**：
- ❌ 日志散落各处
- ❌ 无保留期策略，占用大量空间
- ✅ 标准化存储和保留

---

### ADR-970_2：结构化日志格式（Rule）

#### ADR-970_2_1 必须使用 JSON 格式
所有日志 **必须**使用 JSON 格式，包含标准字段。

#### ADR-970_2_2 标准 JSON 架构
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
      "adr": "ADR-001",
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

#### ADR-970_2_3 字段说明
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

#### ADR-970_2_4 示例
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
      "adr": "ADR-001",
      "severity": "error",
      "message": "Modules.Orders references Modules.Members",
      "file": "src/Modules/Orders/UseCases/CreateOrder/CreateOrderHandler.cs",
      "line": 15,
      "fix_guide": "docs/copilot/adr-001.prompts.md#scenario-3"
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

### ADR-970_3：日志与 ADR 关联机制（Rule）

#### ADR-970_3_1 测试失败日志必须自动链接到 ADR
测试失败日志 **必须**自动链接到对应的 ADR 和修复指南。

#### ADR-970_3_2 关联规则
**关联规则**：
1. **测试名称映射**：测试名称必须包含 ADR 编号
   - 格式：`ADR_XXXX_Test_Name`
   - 示例：`ADR_001_Modules_Should_Not_Reference_Other_Modules`

2. **日志中的 ADR 字段**：
   - 从测试名称自动提取 ADR 编号
   - 填充 `details[].adr` 字段

3. **修复指南链接**：
   - 映射到 `docs/copilot/adr-XXXX.prompts.md`
   - 具体到场景或章节（如适用）

#### ADR-970_3_3 ADR 中的反向链接
**ADR 中的反向链接**：

每个 ADR **必须**包含相关测试和日志位置：

```markdown
### Enforcement Section Example

| 规则编号 | 执行级别 | 测试/手段 | 说明 |
|---------|---------|----------|------|
| ADR-001.1 | L1 | `Modules_Should_Not_Reference_Other_Modules` | 模块隔离测试 |

**相关日志**：
- 测试报告：`docs/reports/architecture-tests/latest.json`
- 修复指南：`docs/copilot/adr-001.prompts.md`
```

#### ADR-970_3_4 CI 失败通知增强
**CI 失败通知增强**：

当测试失败时，CI 通知 **应该**包含：
```
❌ 架构测试失败

测试：Modules_Should_Not_Reference_Other_Modules
ADR：ADR-001 - 模块化单体与垂直切片架构
错误：Modules.Orders references Modules.Members

📖 了解详情：
- ADR 正文：docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md
- 修复指南：docs/copilot/adr-001.prompts.md#scenario-3
- 测试报告：docs/reports/architecture-tests/latest.json
```

**核心原则**：
> 测试失败即知道查看哪个 ADR，ADR 即知道查看哪个测试。

**判定**：
- ❌ 测试失败但不知道查看哪个 ADR
- ❌ ADR 中未列出相关测试
- ✅ 双向关联，快速定位

---

### ADR-970_4：自动化报告生成（Rule）

#### ADR-970_4_1 CI 必须自动生成结构化日志
CI **必须**自动生成结构化日志并存储。

#### ADR-970_4_2 CI Workflow 示例
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
        uses: actions/github-script@v3
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

#### ADR-970_4_3 脚本职责
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

### ADR-970_5：日志分发与访问（Rule）

#### ADR-970_5_1 日志必须易于访问
日志 **必须**易于访问，并在适当时自动分发。

#### ADR-970_5_2 访问方式
**访问方式**：
1. **本地访问**：`docs/reports/` 目录
2. **CI Artifacts**：GitHub Actions Artifacts
3. **PR 评论**：失败时自动评论
4. **通知**：严重失败时通知团队

#### ADR-970_5_3 分发规则
**分发规则**：

| 事件 | 分发方式 | 接收方 | 内容 |
|------|---------|--------|------|
| 架构测试失败 | PR 评论 | PR 作者 + Reviewers | 失败详情 + 修复链接 |
| 安全扫描发现高危 | Issue + Email | 架构委员会 | 漏洞详情 + 修复建议 |
| 依赖重大更新 | PR 评论 | PR 作者 | 变更说明 + 影响评估 |
| 构建失败 | PR 评论 | PR 作者 | 失败日志摘要 |

#### ADR-970_5_4 访问权限
**访问权限**：
- `docs/reports/` 目录在仓库中，所有成员可访问
- 敏感安全报告可配置访问限制

**核心原则**：
> 主动分发关键信息，降低查找成本。

**判定**：
- ❌ 发展者需主动去 CI 查看日志
- ❌ 关键失败无人知晓
- ✅ 自动分发到相关方

---

## Enforcement（执法模型）

> 📋 **Enforcement 映射说明**：
> 
> 下表展示了 ADR-970 各条款（Clause）的执法方式及执行级别。

| 规则编号 | 执行级 | 执法方式 | Decision 映射 |
|---------|--------|---------|--------------|
| **ADR-970_1_1** | L1 | 文档扫描日志存储位置 | §ADR-970_1_1 |
| **ADR-970_1_2** | L1 | 文档扫描存储结构 | §ADR-970_1_2 |
| **ADR-970_1_3** | L1 | 文档扫描保留期策略 | §ADR-970_1_3 |
| **ADR-970_1_4** | L1 | 文档扫描核心原则 | §ADR-970_1_4 |
| **ADR-970_2_1** | L1 | 文档扫描 JSON 格式要求 | §ADR-970_2_1 |
| **ADR-970_2_2** | L1 | 文档扫描标准架构 | §ADR-970_2_2 |
| **ADR-970_2_3** | L1 | 文档扫描字段说明 | §ADR-970_2_3 |
| **ADR-970_2_4** | L1 | 文档扫描示例 | §ADR-970_2_4 |
| **ADR-970_3_1** | L1 | 文档扫描 ADR 关联机制 | §ADR-970_3_1 |
| **ADR-970_3_2** | L1 | 文档扫描关联规则 | §ADR-970_3_2 |
| **ADR-970_3_3** | L1 | 文档扫描反向链接 | §ADR-970_3_3 |
| **ADR-970_3_4** | L1 | 文档扫描 CI 通知增强 | §ADR-970_3_4 |
| **ADR-970_4_1** | L1 | 文档扫描自动化生成 | §ADR-970_4_1 |
| **ADR-970_4_2** | L1 | 文档扫描 CI Workflow | §ADR-970_4_2 |
| **ADR-970_4_3** | L1 | 文档扫描脚本职责 | §ADR-970_4_3 |
| **ADR-970_5_1** | L1 | 文档扫描日志访问 | §ADR-970_5_1 |
| **ADR-970_5_2** | L1 | 文档扫描访问方式 | §ADR-970_5_2 |
| **ADR-970_5_3** | L1 | 文档扫描分发规则 | §ADR-970_5_3 |
| **ADR-970_5_4** | L1 | 文档扫描访问权限 | §ADR-970_5_4 |

---

## Relationships（关系声明）

**Depends On**：
- [ADR-900：架构测试与 CI 治理元规则](./ADR-900-architecture-tests.md) - 日志集成基于 CI 治理机制
- [ADR-907：ArchitectureTests 执法治理体系](./ADR-907-architecture-tests-enforcement-governance.md) - 测试报告日志关联
- [ADR-940：ADR 关系与溯源管理](./ADR-940-adr-relationship-traceability-management.md) - 日志与 ADR 关联机制

**Depended By**：
- 无

**Supersedes**：
- 无

**Superseded By**：
- 无

**Related**：
- [ADR-975：文档质量监控](./ADR-975-documentation-quality-monitoring.md) - 文档质量日志
- [ADR-980：ADR 生命周期同步机制](./ADR-980-adr-lifecycle-synchronization.md) - 生命周期日志

---

## History（版本历史）


| 版本  | 日期         | 变更说明   |
|-----|------------|--------|
| 2.0 | 2026-02-03 | 对齐 ADR-907 v2.0，引入 Rule/Clause 双层编号体系 | 架构委员会 |
| 1.0 | 2026-01-29 | 初始版本 |
