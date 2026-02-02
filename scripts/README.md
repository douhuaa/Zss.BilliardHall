# ADR 自动化工具集

> 依据 [ADR-970：自动化工具日志集成标准](../docs/adr/governance/ADR-970-automation-log-integration-standard.md)

本目录包含用于管理和维护 ADR 治理体系的自动化工具。所有工具遵循最小化变更原则，确保 ADR 文档、架构测试和 Copilot Prompts 的一致性。

---

## 🆕 JSON 输出支持

**所有验证脚本现已支持结构化 JSON 输出（依据 ADR-970.2）**：

### 使用方法

```bash
# 默认文本模式（向后兼容）
./scripts/validate-adr-consistency.sh

# JSON 格式输出到控制台
./scripts/validate-adr-consistency.sh --format json

# JSON 格式保存到文件
./scripts/validate-adr-consistency.sh --format json --output docs/reports/architecture-tests/adr-consistency.json
```

### JSON 输出格式

符合 ADR-970.2 标准：

```json
{
  "type": "adr-validation | three-way-mapping | ...",
  "timestamp": "2026-01-27T12:00:00Z",
  "source": "validate-adr-consistency",
  "version": "1.0.0",
  "status": "success | failure | warning",
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
      "severity": "info | warning | error",
      "message": "详细消息",
      "file": "path/to/file",
      "fix_guide": "docs/adr/..."
    }
  ],
  "metadata": {
    "branch": "main",
    "commit": "abc123",
    "author": "user"
  }
}
```

### 已支持 JSON 输出的脚本

- ✅ `validate-adr-consistency.sh` - ADR 一致性检查
- ✅ `validate-three-way-mapping.sh` - 三位一体映射验证
- 🚧 其他脚本正在对齐中...

---

## 工具概览

### 1. ADR 一致性检查工具

**脚本**：`validate-adr-consistency.sh` / `validate-adr-consistency.ps1`

**用途**：验证 ADR 文档的编号、目录和内容的三元一致性

**检查项**：
- ✅ ADR 编号格式（4位数字）
- ✅ ADR 编号与目录层级对应
- ✅ 元数据完整性（状态、级别等）
- ✅ 文件命名规范
- ✅ 编号连续性（检测跳号）

**使用方法**：
```bash
# 文本模式（默认）
./scripts/validate-adr-consistency.sh

# JSON 模式
./scripts/validate-adr-consistency.sh --format json

# JSON 保存到文件
./scripts/validate-adr-consistency.sh --format json --output docs/reports/architecture-tests/adr-consistency.json
```

**输出示例（文本）**：
```
✅ 编号格式正确：0001
✅ 目录位置正确：constitutional (范围: 0001-0099)
✅ 元数据完整
```

**JSON 输出**：支持 ✅（依据 ADR-970.2）

---

### 2. 三位一体映射验证工具

**脚本**：`validate-three-way-mapping.sh`

**用途**：验证 ADR、架构测试、Copilot Prompts 三者的映射关系

**检查项**：
- ✅ ADR 与测试文件的映射
- ✅ ADR 与 Prompt 文件的映射
- ✅ 根据 ADR-900，检查需要测试覆盖的 ADR 是否有测试
- ✅ 孤立的测试和 Prompt 文件
- ✅ 映射覆盖率统计

**使用方法**：
```bash
# 文本模式（默认）
./scripts/validate-three-way-mapping.sh

# JSON 模式
./scripts/validate-three-way-mapping.sh --format json --output docs/reports/architecture-tests/three-way-mapping.json
```

**输出**：
- 映射关系分析
- 问题修正清单
- 健康度报告

**JSON 输出**：支持 ✅（依据 ADR-970.2）

---

### 3. ADR 管理 CLI

**脚本**：`adr-cli.sh`

**用途**：提供统一的 ADR 创建、查询和管理入口

**功能**：
- ✅ 创建新 ADR（自动分配编号）
- ✅ 查询下一个可用编号
- ✅ 列出所有 ADR
- ✅ 运行验证

**使用方法**：

#### 创建新 ADR
```bash
./scripts/adr-cli.sh create constitutional "模块隔离约束"
```

自动执行：
1. 分配下一个可用编号
2. 从模板创建 ADR 文档
3. 自动填充元数据
4. 创建对应的 Prompt 文件
5. 提示创建测试文件

#### 查询下一个可用编号
```bash
./scripts/adr-cli.sh next-number structure
# 输出：0101
```

#### 列出 ADR
```bash
# 列出所有 ADR
./scripts/adr-cli.sh list

# 列出指定层级
./scripts/adr-cli.sh list constitutional
```

#### 运行验证
```bash
./scripts/adr-cli.sh validate
```

---

### 4. ADR 健康报告生成器

**脚本**：`generate-health-report.sh`

**用途**：生成 ADR 治理体系的综合健康度报告

**报告内容**：
- 📊 ADR 文档统计（按层级、状态）
- 📈 架构测试覆盖率
- 🗺️ Copilot Prompts 映射率
- ✅ 编号一致性状态
- 💡 改进建议

**使用方法**：
```bash
# 生成到默认位置（docs/adr-health-report.md）
./scripts/generate-health-report.sh

# 指定输出文件
./scripts/generate-health-report.sh /path/to/output.md
```

**建议频率**：每月生成一次

---

### 5. 可裁决性速查工具

**脚本**：`generate-quick-reference.sh`

**用途**：从 ADR 中提取红线约束和需要测试的条款，生成速查手册

**提取内容**：
- 🔴 红线约束（MUST/MUST NOT）
- 🟡 建议约束（SHOULD）
- ✅ 根据 ADR-900，提取需要测试覆盖的条款
- 🚧 人工审核门控点

**使用方法**：
```bash
# 输出到控制台
./scripts/generate-quick-reference.sh

# 输出到文件
./scripts/generate-quick-reference.sh docs/adr-quick-reference.md
```

---

### 6. 传统映射验证工具

**脚本**：`validate-adr-test-mapping.sh` / `validate-adr-test-mapping.ps1`

**状态**：保留用于向后兼容，建议使用新的三位一体映射工具

**用途**：验证 ADR 与架构测试的映射关系

---

## 日常使用工作流

### 场景 1：创建新 ADR

```bash
# 1. 使用 CLI 创建 ADR
./scripts/adr-cli.sh create structure "领域事件命名规范"

# 2. 编辑生成的 ADR 文档
vim docs/adr/structure/ADR-0120-domain-event-naming.md

# 3. 如需测试，创建测试文件
vim src/tests/ArchitectureTests/ADR/ADR_0120_Architecture_Tests.cs

# 4. 完善 Prompt 文件
vim docs/copilot/adr-120.prompts.md

# 5. 运行验证
./scripts/adr-cli.sh validate
```

### 场景 2：修改现有 ADR

```bash
# 1. 修改 ADR 文档
vim docs/adr/constitutional/ADR-0001-*.md

# 2. 同步更新测试（如需要）
vim src/tests/ArchitectureTests/ADR/ADR_0001_Architecture_Tests.cs

# 3. 同步更新 Prompt
vim docs/copilot/adr-1.prompts.md

# 4. 运行验证
./scripts/validate-three-way-mapping.sh
```

### 场景 3：定期维护

```bash
# 每周：运行一致性检查
./scripts/validate-adr-consistency.sh
./scripts/validate-three-way-mapping.sh

# 每月：生成健康报告
./scripts/generate-health-report.sh

# 根据需要：更新速查手册
./scripts/generate-quick-reference.sh docs/adr-quick-reference.md
```

---

## CI/CD 集成

所有验证工具已集成到 CI/CD 流程中：

### GitHub Actions 工作流

在 `.github/workflows/architecture-tests.yml` 中：

1. **ADR 一致性检查**：确保编号、目录、元数据一致
2. **三位一体映射验证**：确保 ADR/测试/Prompt 映射完整
3. **架构测试执行**：运行所有架构测试

### Pull Request 检查

PR 模板中包含：
- [ ] 运行 ADR 一致性验证
- [ ] 运行三位一体映射验证
- [ ] 更新相关 Prompt 文件

---

## 层级编号规范

基于 [ADR-0006 术语与编号宪法](../docs/adr/constitutional/ADR-0006-terminology-numbering-constitution.md)：

| 层级 | 编号范围 | 目录 | 用途 |
|-----|---------|------|------|
| 宪法层 | 0001-0099 | `constitutional/` | 系统基础约束 |
| 结构层 | 0100-0199 | `structure/` | 静态组织与命名 |
| 运行层 | 0200-0299 | `runtime/` | 运行时模型 |
| 技术层 | 0300-0399 | `technical/` | 具体实现 |
| 治理层 | 0000, 0900-0999 | `governance/` | 流程、测试管理 |

---

## 错误诊断

### 问题：编号格式错误

**症状**：
```
❌ 编号格式错误：应为4位数字（如 0001），当前为 1
```

**解决**：
确保 ADR 文件名使用4位编号格式：`ADR-0001-*.md`

### 问题：目录位置错误

**症状**：
```
❌ 目录位置错误：ADR-0150 不在 constitutional 的编号范围内
```

**解决**：
将 ADR 移动到正确的目录：
```bash
mv docs/adr/constitutional/ADR-0150-*.md docs/adr/structure/
```

### 问题：缺少测试文件

**症状**：
```
⚠️ ADR-0001：需要测试但缺少测试文件
```

**解决**：
创建对应的测试文件：
```bash
touch src/tests/ArchitectureTests/ADR/ADR_0001_Architecture_Tests.cs
```

### 问题：孤立的测试或 Prompt 文件

**症状**：
```
⚠️ 测试文件 ADR_0120_Architecture_Tests.cs：对应的 ADR 不存在
```

**解决方案**：
1. 创建对应的 ADR 文档，或
2. 删除/重命名孤立文件

---

## 工具开发指南

### 添加新工具

1. 在 `scripts/` 目录创建脚本
2. 使用一致的输出格式（颜色、图标）
3. 提供清晰的错误消息
4. 支持 `--help` 参数
5. 更新本 README

### 输出规范

使用统一的颜色和图标：
- ✅ 成功：绿色
- ❌ 错误：红色
- ⚠️ 警告：黄色
- ℹ️ 信息：青色
- 🔍 调试：灰色

### 脚本模板

```bash
#!/bin/bash
set -e

# 定义路径
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(dirname "$SCRIPT_DIR")"

# 颜色定义
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m'

# 输出函数
function log_success() { echo -e "${GREEN}✅ $1${NC}"; }
function log_error() { echo -e "${RED}❌ $1${NC}"; }
function log_warning() { echo -e "${YELLOW}⚠️  $1${NC}"; }
function log_info() { echo -e "${CYAN}ℹ️  $1${NC}"; }

# 主逻辑
function main() {
    log_info "开始执行..."
    # 工具逻辑
}

main "$@"
```

---

## 相关资源

### 文档
- [ADR 目录](../docs/adr/README.md)
- [ADR 流程规范](../docs/adr/governance/ADR-0900-adr-process.md)
- [架构测试宪法](../docs/adr/governance/ADR-900-architecture-tests.md)
- [Copilot 治理体系](../docs/copilot/README.md)

### 模板
- [ADR 模板](../docs/templates/adr-template.md)
- [Copilot Prompt 模板](../docs/templates/copilot-pormpts-template.md)

---

## 常见问题

### Q: 为什么需要这么多工具？

A: 每个工具专注于特定的验证场景，组合使用可以全面保障 ADR 治理体系的质量。

### Q: 可以只运行某个工具吗？

A: 可以。但建议至少运行一致性检查和三位一体映射验证。

### Q: 工具会修改我的文件吗？

A: 不会。所有工具都是只读的，只进行检查和报告。修改需要手动进行。

### Q: CI 失败时如何处理？

A: 查看 CI 日志中的具体错误，使用对应的工具在本地重现和修复。

---

## 贡献指南

欢迎改进工具！提交 PR 前请确保：

1. 遵循现有的代码风格
2. 添加必要的错误处理
3. 更新此 README
4. 测试脚本在 Linux/macOS 和 Windows 上的兼容性

---

**维护者**：架构委员会  
**最后更新**：2026-01-26  
**版本**：1.0
