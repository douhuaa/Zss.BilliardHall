# Instructions 文件规范

**版本**: 1.0  
**最后更新**: 2026-02-09  
**状态**: Active

---

## 概述

本文档定义 `.github/instructions/*.instructions.yaml` 文件的统一结构规范，确保所有指令文件格式一致、易于解析和维护。

---

## YAML Schema 定义

### 顶层结构

```yaml
instructions:
  - id: <string>              # 必需：指令唯一标识符
    description: <string>      # 必需：指令描述
    action: <string>           # 必需：执行的操作
    conditions: <array>        # 必需：触发条件列表
    output: <string>           # 必需：输出结果
    tools: <array>             # 可选：使用的工具列表
    feedback: <array>          # 可选：反馈机制列表
    guidelines: <array>        # 可选：指导原则列表
    commands: <object>         # 可选：标准化命令集
    dependencies: <array>      # 可选：依赖的其他指令
```

### 字段说明

#### 必需字段

##### `id` (string)
- **格式**：`{AgentPrefix}-{Number}`
- **示例**：`AG-001`, `TG-002`, `DM-003`
- **规则**：
  - Agent 前缀必须唯一且有意义
  - 编号使用 3 位数字，从 001 开始
  - 同一 Agent 的指令编号连续

**Agent 前缀映射**：
| Agent | 前缀 | 示例 |
|-------|------|------|
| Architecture Guardian | AG | AG-001 |
| ADR Reviewer | AR | AR-001 |
| Documentation Maintainer | DM | DM-001 |
| Expert Dotnet Engineer | DE | DE-001 |
| Handler Pattern Enforcer | HP | HP-001 |
| Module Boundary Checker | MB | MB-001 |
| Test Generator | TG | TG-001 |

##### `description` (string)
- **格式**：简短的描述，不超过 100 个字符
- **示例**：`"协调所有 Specialist Agent 输出，确保 ADR 遵守三态输出规则"`
- **规则**：
  - 使用中文
  - 清晰描述指令目的
  - 避免模糊或过于宽泛的表述

##### `action` (string)
- **格式**：具体的操作描述
- **示例**：`"审核所有 Agent 输出"`, `"生成对应测试方法"`
- **规则**：
  - 使用动词开头
  - 描述具体行为
  - 可以引用工具或资源

##### `conditions` (array of strings)
- **格式**：触发条件列表
- **示例**：
```yaml
conditions:
  - "PullRequest"
  - "CI pipeline"
  - "ManualReview"
```
- **规则**：
  - 至少包含一个条件
  - 使用 PascalCase 或标准术语
  - 避免重复或冗余条件

**标准条件**：
- `PullRequest` - PR 创建或更新时
- `CI pipeline` - CI 管道执行时
- `ManualReview` - 手动审查时
- `New ADR Added` - 新增 ADR 时
- `Documentation Updated` - 文档更新时
- `Code Modified` - 代码修改时

##### `output` (string)
- **格式**：期望的输出结果
- **示例**：`"Allowed / Blocked / Uncertain"`, `"Generated Tests"`
- **规则**：
  - 描述输出的格式或类型
  - 可以使用枚举格式（用 `/` 分隔）
  - 清晰明确

#### 可选字段

##### `tools` (array of strings)
- **格式**：使用的工具、资源或服务列表
- **示例**：
```yaml
tools:
  - "architecture-tests"
  - "ADR database"
  - "ArchitectureTests"
  - "Codegen Templates"
```
- **规则**：
  - 列出所有依赖的工具
  - 使用小写或 PascalCase
  - 包括文档路径、数据库、API 等

##### `feedback` (array of strings)
- **格式**：反馈机制或后续操作列表
- **示例**：
```yaml
feedback:
  - "生成 FailureObject"
  - "Escalate Fatal failures to ArchitectureCouncil"
  - "提交生成测试至测试项目"
```
- **规则**：
  - 描述执行后的反馈动作
  - 包括日志、通知、升级等
  - 清晰描述失败处理

##### `guidelines` (array of strings)
- **格式**：指导原则或规范列表
- **示例**：
```yaml
guidelines:
  - "测试文件路径：src/tests/ArchitectureTests/"
  - "命名规范：ADR_XXX_Y_Architecture_Tests.cs"
  - "严格遵循 ADR-907 v2.0 Rule/Clause 体系"
```
- **规则**：
  - 提供具体的规范或原则
  - 可以引用其他文档
  - 包括路径、命名等约定

##### `commands` (object)
- **格式**：标准化命令集合（键值对）
- **示例**：
```yaml
commands:
  run_all_tests: "dotnet test --filter Category=Architecture"
  run_specific_test: "dotnet test --filter FullyQualifiedName~ADR_{NUMBER}"
```
- **规则**：
  - 键使用 snake_case
  - 值为完整的命令字符串
  - 包含所有必要的参数和标志

##### `dependencies` (array of strings)
- **格式**：依赖的其他指令 ID 列表
- **示例**：
```yaml
dependencies:
  - "AG-001"  # 依赖架构守护的审核
  - "TG-002"  # 依赖测试生成指南
```
- **规则**：
  - 使用其他指令的 ID
  - 明确执行顺序
  - 避免循环依赖

---

## 完整示例

### 基础指令（仅必需字段）

```yaml
instructions:
  - id: HP-001
    description: "验证 Handler 模式的正确性"
    action: "检查 Handler 结构和约束"
    conditions:
      - "PullRequest"
      - "Code Modified"
    output: "Validation Report"
```

### 完整指令（包含所有字段）

```yaml
instructions:
  - id: TG-001
    description: "根据 ADR Clause 自动生成 Architecture Tests"
    action: "生成对应测试方法"
    conditions:
      - "PullRequest"
      - "New ADR Added"
    output: "Generated Tests"
    tools:
      - "ArchitectureTests"
      - "Codegen Templates"
      - "docs/guidelines/ARCHITECTURE-TEST-GUIDELINES.md"
    feedback:
      - "提交生成测试至测试项目"
      - "运行新生成的测试验证"
    guidelines:
      - "测试文件路径：src/tests/ArchitectureTests/"
      - "命名规范：ADR_XXX_Y_Architecture_Tests.cs"
      - "严格遵循 ADR-907 v2.0 Rule/Clause 体系"
    commands:
      run_all_architecture_tests: "dotnet test src/tests/ArchitectureTests/ --filter \"Category=Architecture\""
      run_specific_adr_tests: "dotnet test src/tests/ArchitectureTests/ --filter \"FullyQualifiedName~ADR_{NUMBER}\""
    dependencies:
      - "AG-001"  # 需要架构守护的预审批
```

---

## 现有指令文件对照

### 符合规范的指令

✅ **adr-reviewer.instructions.yaml**
- 包含所有必需字段
- 结构清晰
- 可以添加 `guidelines` 提高完整性

✅ **architecture-guardian.instructions.yaml**
- 包含所有必需字段
- 结构简洁
- 适合其协调角色

✅ **documentation-maintainer.instructions.yaml**
- 包含所有必需字段
- 使用了 `tools` 字段
- 可以考虑添加 `commands` 用于文档更新命令

✅ **expert-dotnet-software-engineer.instructions.yaml**
- 符合基础规范
- 结构清晰

✅ **handler-pattern-enforcer.instructions.yaml**
- 符合基础规范
- 结构简洁

✅ **module-boundary-checker.instructions.yaml**
- 符合基础规范
- 结构清晰

⚠️ **test-generator.instructions.yaml**
- 包含了扩展字段 `guidelines` 和 `commands`
- 这是一个**良好实践**，应该推广到其他指令
- 建议其他指令也添加相应的扩展字段

---

## 迁移指南

### 为现有指令添加扩展字段

**推荐优先级**：

**P1（高优先级）**：
1. `architecture-guardian.instructions.yaml` - 添加 `commands`（用于运行架构测试）
2. `adr-reviewer.instructions.yaml` - 添加 `guidelines`（ADR 审查规范）
3. `documentation-maintainer.instructions.yaml` - 添加 `commands`（文档更新命令）

**P2（中优先级）**：
4. `handler-pattern-enforcer.instructions.yaml` - 添加 `guidelines`（Handler 模式规范）
5. `module-boundary-checker.instructions.yaml` - 添加 `guidelines`（模块边界规则）

**P3（低优先级）**：
6. `expert-dotnet-software-engineer.instructions.yaml` - 保持当前结构即可

### 添加示例

#### 为 architecture-guardian 添加 commands

```yaml
commands:
  run_all_architecture_tests: "dotnet test src/tests/ArchitectureTests/ --filter \"Category=Architecture\""
  check_guardian_decision: "dotnet test src/tests/ArchitectureTests/ --filter \"FullyQualifiedName~GuardianDecision\""
```

#### 为 adr-reviewer 添加 guidelines

```yaml
guidelines:
  - "审查 ADR 结构符合 ADR-902 模板"
  - "验证 Rule/Clause 编号符合 ADR-907-A 标准"
  - "检查关系声明区完整性（ADR-940）"
  - "确认标题级别符合 ADR-946 约束"
```

---

## 验证工具

### JSON Schema（用于自动化验证）

```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "title": "Instructions Schema",
  "type": "object",
  "required": ["instructions"],
  "properties": {
    "instructions": {
      "type": "array",
      "items": {
        "type": "object",
        "required": ["id", "description", "action", "conditions", "output"],
        "properties": {
          "id": {
            "type": "string",
            "pattern": "^[A-Z]{2,3}-\\d{3}$"
          },
          "description": {
            "type": "string",
            "maxLength": 100
          },
          "action": {
            "type": "string"
          },
          "conditions": {
            "type": "array",
            "items": {
              "type": "string"
            },
            "minItems": 1
          },
          "output": {
            "type": "string"
          },
          "tools": {
            "type": "array",
            "items": {
              "type": "string"
            }
          },
          "feedback": {
            "type": "array",
            "items": {
              "type": "string"
            }
          },
          "guidelines": {
            "type": "array",
            "items": {
              "type": "string"
            }
          },
          "commands": {
            "type": "object",
            "patternProperties": {
              "^[a-z_]+$": {
                "type": "string"
              }
            }
          },
          "dependencies": {
            "type": "array",
            "items": {
              "type": "string",
              "pattern": "^[A-Z]{2,3}-\\d{3}$"
            }
          }
        }
      }
    }
  }
}
```

### 验证脚本（可选）

```bash
#!/bin/bash
# validate-instructions.sh

echo "验证 Instructions 文件..."

for file in .github/instructions/*.instructions.yaml; do
    echo "检查: $file"
    
    # 使用 yq 或 python 验证 YAML 格式
    # 验证必需字段存在
    # 验证字段格式
    
    echo "✅ $file 验证通过"
done
```

---

## 最佳实践

### 1. 保持一致性
- 所有指令文件使用相同的字段顺序
- 使用统一的命名约定
- 保持描述风格一致

### 2. 完整性
- 尽可能使用扩展字段
- 提供详细的 `guidelines`
- 包含标准化的 `commands`

### 3. 可维护性
- 定期审查和更新指令
- 保持与 ADR 同步
- 记录变更历史

### 4. 可读性
- 使用清晰的描述
- 避免过于技术化的术语
- 包含示例和说明

---

## 版本历史

| 版本 | 日期 | 变更说明 | 作者 |
|------|------|---------|------|
| 1.0 | 2026-02-09 | 初始版本：建立 Instructions 文件规范 | Copilot Agent |

---

## 相关文档

- [Instructions 文件目录](./instructions/)
- [Agent-Skills 权限映射](./AGENT-SKILLS-MAPPING.md)
- [Skills 体系规范](./skills/README.md)
- [ADR-007：Agent 行为与权限宪法](../docs/adr/constitutional/ADR-007-agent-behavior-permissions-constitution.md)

---

**维护责任**：架构委员会  
**审核周期**：每季度  
**状态**：✅ Active
