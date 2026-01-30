# 自动化工具日志存储

> 依据 [ADR-970：自动化工具日志集成标准](../adr/governance/ADR-970-automation-log-integration-standard.md)

本目录用于存储自动化工具生成的结构化日志报告。

---

## 目录结构

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

---

## 日志格式

所有日志遵循 ADR-970.2 定义的标准 JSON 格式：

```json
{
  "type": "architecture-test | security-scan | build | test | validation",
  "timestamp": "2026-01-27T12:00:00Z",
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
    "author": "user"
  }
}
```

---

## 保留期策略

依据 ADR-970.1：

| 日志类型 | 保留期 | 清理策略 |
|---------|--------|---------|
| 架构测试报告 | 30 天 | 自动删除旧报告 |
| 依赖更新日志 | 180 天 | 自动删除旧日志 |
| 安全扫描报告 | 永久 | 不删除 |
| 构建日志 | 7 天 | 自动删除旧日志 |
| 测试报告 | 30 天 | 自动删除旧报告 |

---

## 使用方式

### 读取最新报告

```bash
# 架构测试最新结果
cat docs/reports/architecture-tests/latest.json | jq '.summary'

# 查看失败项
cat docs/reports/architecture-tests/latest.json | jq '.details[] | select(.severity == "error")'
```

### 生成报告

验证脚本支持 JSON 输出：

```bash
# 生成 JSON 格式报告
./scripts/validate-adr-consistency.sh --format json --output docs/reports/architecture-tests/$(date +%Y-%m-%d-%H-%M).json

# 默认文本模式（向后兼容）
./scripts/validate-adr-consistency.sh
```

---

## 与 ADR 关联

每个 `details` 条目包含 `adr` 字段，指向相关的 ADR 编号：

```json
{
  "test": "Modules_Should_Not_Reference_Other_Modules",
  "adr": "ADR-0001",
  "severity": "error",
  "message": "Modules.Orders references Modules.Members",
  "fix_guide": "docs/copilot/adr-0001.prompts.md#scenario-3"
}
```

这使得：
- ✅ 测试失败时立即知道相关 ADR
- ✅ 自动生成修复指南链接
- ✅ CI 可以自动通知和评论

---

## CI 集成

GitHub Actions 自动生成并上传报告：

```yaml
- name: Run Validation
  run: ./scripts/validate-adr-consistency.sh --format json --output reports/adr-validation.json

- name: Upload Report
  uses: actions/upload-artifact@v3
  with:
    name: adr-validation-report
    path: reports/adr-validation.json
```

---

## 相关资源

- [ADR-970：自动化工具日志集成标准](../adr/governance/ADR-970-automation-log-integration-standard.md)
- [ADR-0000：架构测试与 CI 治理元规则](../adr/governance/ADR-0000-architecture-tests.md)
- [脚本工具文档](../../scripts/README.md)

---

**维护**：架构委员会  
**最后更新**：2026-01-27  
**依据**：ADR-970
