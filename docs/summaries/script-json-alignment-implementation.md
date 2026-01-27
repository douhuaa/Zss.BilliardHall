# 脚本 JSON 输出对齐实施总结

> **依据**：[ADR-970：自动化工具日志集成标准](../adr/governance/ADR-970-automation-log-integration-standard.md)  
> **日期**：2026-01-27  
> **状态**：核心对齐已完成 ✅

---

## 概述

本次实施将现有的 ADR 验证脚本对齐到 ADR-970 定义的标准化 JSON 输出格式，实现了自动化工具日志的结构化存储和关联机制。

---

## 已完成的工作

### 1. 基础设施建设 ✅

#### 1.1 JSON 输出库
- **文件**：`scripts/lib/json-output.sh`
- **功能**：
  - 提供统一的 JSON 输出格式化函数
  - 符合 ADR-970.2 标准 Schema
  - 支持自动时间戳、Git 元数据、ADR 关联
- **使用**：所有脚本可通过 `source scripts/lib/json-output.sh` 复用

#### 1.2 日志存储目录
- **结构**：`docs/reports/` 按类型分类
  ```
  docs/reports/
  ├── architecture-tests/     # 架构测试报告
  ├── dependencies/           # 依赖更新日志
  ├── security/               # 安全扫描报告
  ├── builds/                 # 构建日志
  └── tests/                  # 测试执行报告
  ```
- **规范**：
  - `.gitkeep` 保留目录结构
  - `.gitignore` 排除实际日志文件
  - `latest.json` 符号链接指向最新报告

#### 1.3 文档支持
- **`docs/reports/README.md`**：日志存储使用指南
- **`scripts/README.md`**：JSON 输出使用说明
- **ADR-970**：更新实施工具清单

---

### 2. 核心脚本对齐 ✅

#### 2.1 validate-adr-consistency.sh
**对齐前**：
```bash
./scripts/validate-adr-consistency.sh
# 输出彩色文本
```

**对齐后**：
```bash
# 文本模式（默认，向后兼容）
./scripts/validate-adr-consistency.sh

# JSON 模式
./scripts/validate-adr-consistency.sh --format json

# JSON 保存到文件
./scripts/validate-adr-consistency.sh --format json --output docs/reports/architecture-tests/adr-consistency.json
```

**JSON 输出示例**：
```json
{
  "type": "adr-validation",
  "timestamp": "2026-01-27T02:51:23Z",
  "source": "validate-adr-consistency",
  "version": "1.0.0",
  "status": "success",
  "summary": {
    "total": 43,
    "passed": 43,
    "failed": 0,
    "warnings": 0
  },
  "details": [
    {
      "test": "ADR_Numbering_Format",
      "adr": "ADR-0001",
      "severity": "info",
      "message": "编号格式正确：0001",
      "file": "/path/to/ADR-0001-*.md",
      "fix_guide": "docs/adr/constitutional/ADR-0006-terminology-numbering-constitution.md"
    }
  ],
  "metadata": {
    "branch": "main",
    "commit": "abc123",
    "author": "user"
  }
}
```

#### 2.2 validate-three-way-mapping.sh
**对齐前**：
```bash
./scripts/validate-three-way-mapping.sh
# 输出彩色文本和修正清单
```

**对齐后**：
```bash
# 文本模式（默认，向后兼容）
./scripts/validate-three-way-mapping.sh

# JSON 模式
./scripts/validate-three-way-mapping.sh --format json --output docs/reports/architecture-tests/three-way-mapping.json
```

**JSON 输出特点**：
- 包含所有映射检查详情
- 标识缺失测试、缺失 Prompts、孤立文件
- 每个条目关联相应的 ADR 编号
- 提供修复指南链接

---

### 3. ADR-970 符合性 ✅

#### 3.1 标准字段（ADR-970.2）
所有 JSON 输出包含：
- ✅ `type` - 日志类型
- ✅ `timestamp` - ISO 8601 时间戳
- ✅ `source` - 工具名称
- ✅ `version` - 工具版本
- ✅ `status` - 整体状态（success/failure/warning）
- ✅ `summary` - 汇总统计
- ✅ `details` - 详细结果数组
- ✅ `metadata` - Git 上下文信息

#### 3.2 details 字段（ADR-970.2）
每个详情条目包含：
- ✅ `test` - 测试名称
- ✅ `adr` - 关联的 ADR 编号（如适用）
- ✅ `severity` - 严重程度（error/warning/info）
- ✅ `message` - 错误/警告消息
- ✅ `file` - 文件路径（可选）
- ✅ `line` - 行号（可选）
- ✅ `fix_guide` - 修复指南链接（可选）

#### 3.3 存储位置（ADR-970.1）
符合标准化目录结构：
- ✅ `docs/reports/architecture-tests/` - 架构测试报告
- ✅ 时间戳文件名：`YYYY-MM-DD-HH-MM.json`
- ✅ `latest.json` 符号链接

---

## 向后兼容性 ✅

### 默认行为
- **未指定 `--format`**：保持原有文本输出
- **所有现有调用**：无需修改，继续正常工作
- **彩色输出**：文本模式下保留所有彩色图标和格式

### 渐进式采用
- 现有 CI/CD 流程：无需立即修改
- 可逐步切换到 JSON 模式
- 两种模式可并行使用

---

## 测试验证 ✅

### 功能测试
```bash
# 测试文本模式
./scripts/validate-adr-consistency.sh
# ✅ 通过：输出正常彩色文本

# 测试 JSON 模式
./scripts/validate-adr-consistency.sh --format json
# ✅ 通过：输出有效 JSON

# 测试文件保存
./scripts/validate-adr-consistency.sh --format json --output /tmp/test.json
# ✅ 通过：文件创建，latest.json 链接正确

# 测试 JSON 格式
cat /tmp/test.json | jq '.'
# ✅ 通过：有效的 JSON，包含所有必需字段
```

### 架构测试
```bash
dotnet test src/tests/ArchitectureTests/
# ✅ 173/175 测试通过
# ⚠️ 2 个失败与既有文档问题相关（非本 PR 引入）
```

---

## 未完成的工作（待后续实施）

### 其他脚本 JSON 支持
以下脚本可按需对齐：
- `validate-adr-test-mapping.sh`
- `validate-governance-compliance.sh`
- `verify-adr-relationships.sh`
- `verify-adr-947-compliance.sh`
- `verify-adr-heading-semantics.sh`
- `check-relationship-consistency.sh`
- `detect-circular-dependencies.sh`
- `generate-health-report.sh`
- `validate-adr-version-sync.sh`
- `verify-all.sh`
- `adr-cli.sh`

### CI/CD 集成
- GitHub Actions Workflow 集成
- 自动报告生成和上传
- PR 评论通知
- 报告格式转换脚本

---

## 使用指南

### 本地使用
```bash
# 默认文本模式（日常使用）
./scripts/validate-adr-consistency.sh

# 生成 JSON 报告（CI 或分析）
./scripts/validate-adr-consistency.sh --format json --output docs/reports/architecture-tests/$(date +%Y-%m-%d-%H-%M).json

# 查看最新报告
cat docs/reports/architecture-tests/latest.json | jq '.summary'

# 查看失败项
cat docs/reports/architecture-tests/latest.json | jq '.details[] | select(.severity == "error")'
```

### CI/CD 使用（计划中）
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

## 核心成果

1. ✅ **符合 ADR-970 标准**
   - JSON 格式完全符合规范
   - 包含所有必需字段
   - 自动 ADR 关联

2. ✅ **向后兼容**
   - 默认保持文本输出
   - 现有调用无需修改
   - 渐进式采用

3. ✅ **可扩展**
   - 通用库可被所有脚本复用
   - 一致的命令行接口
   - 标准化的输出格式

4. ✅ **文档完整**
   - 使用指南
   - 示例和最佳实践
   - ADR-970 更新

---

## 后续建议

### 短期（1-2 周）
1. 根据需求对齐其他关键脚本
2. 在 CI/CD 中试点 JSON 输出
3. 收集使用反馈

### 中期（1-2 月）
1. 完成所有验证脚本的对齐
2. 实现 CI Workflow 自动化
3. 开发报告可视化工具

### 长期（3-6 月）
1. 实现报告聚合和分析
2. 建立趋势监控
3. 集成到团队仪表板

---

## 参考资源

- [ADR-970：自动化工具日志集成标准](../adr/governance/ADR-970-automation-log-integration-standard.md)
- [scripts/README.md](../../scripts/README.md) - 工具使用指南
- [docs/reports/README.md](../reports/README.md) - 日志存储说明
- [scripts/lib/json-output.sh](../../scripts/lib/json-output.sh) - JSON 输出库

---

**维护**：架构委员会  
**完成日期**：2026-01-27  
**状态**：✅ 核心对齐完成
