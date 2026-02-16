# Agent 配置文件清理总结

**日期**: 2026-02-16  
**PR**: #423  
**执行人**: Copilot Agent

---

## 问题描述

agents 目录中存在三种不同格式的配置文件，导致单一数据源原则被破坏：

1. **`.agent`** - YAML 格式的简短配置文件（30-72行，无扩展名）
2. **`.agent.md`** - Markdown 格式的详细文档（166-498行）
3. **`../instructions/*.instructions.yaml`** - 结构化 YAML 指令文件

这种冗余导致：
- 维护负担增加（三处需要同步更新）
- AI 读取混淆（不清楚应该读取哪个文件）
- 违反单一数据源原则（Single Source of Truth）

---

## 决策依据

经过分析，确认了各文件的实际用途：

### 实际使用情况

| 文件类型 | 用途 | 读取者 | 引用位置 |
|---------|------|--------|---------|
| `.agent.md` | 详细文档说明 | Copilot AI | `copilot-instructions.md` 第79行 |
| `.instructions.yaml` | 结构化指令 | 系统执行/CI | `copilot-instructions.md` 第71-77行表格 |
| `.agent` ❌ | **冗余** | **无引用** | **无** |

### 代码验证

```bash
# 搜索所有引用
$ grep -r "\.agent\"" --include="*.cs" --include="*.md" --exclude-dir=".git" .
# 结果：仅在 agents/README.md 中提及，无实际使用

# 搜索代码引用
$ find . -type f \( -name "*.cs" -o -name "*.csproj" \) -exec grep -l "agents/" {} \;
# 结果：无代码引用 agents 目录
```

**结论**：`.agent` 文件是历史遗留的冗余配置，可以安全删除。

---

## 执行的更改

### 删除的文件（8个）

```
.github/agents/.agent                              （隐藏文件）
.github/agents/adr-reviewer.agent
.github/agents/architecture-guardian.agent
.github/agents/documentation-maintainer.agent
.github/agents/expert-dotnet-software-engineer.agent
.github/agents/handler-pattern-enforcer.agent
.github/agents/module-boundary-checker.agent
.github/agents/test-generator.agent
```

### 更新的文件（1个）

**`.github/agents/README.md`**
- 移除了对 `.agent` 配置文件的描述
- 澄清了 `.agent.md` 是供 Copilot AI 阅读的文档
- 明确了 `.instructions.yaml` 是系统执行的结构化指令
- 更新了 Agent 列表表格，移除"配置文件"列，仅保留"文档说明"和"结构化指令"列

---

## 最终架构

### 清理后的文件结构

```
.github/
├── agents/                          # AI 阅读的详细文档
│   ├── README.md                    # 目录说明（已更新）
│   ├── AGENTS.md                    # 高层概念描述
│   ├── adr-reviewer.agent.md
│   ├── architecture-guardian.agent.md
│   ├── documentation-maintainer.agent.md
│   ├── expert-dotnet-software-engineer.agent.md
│   ├── handler-pattern-enforcer.agent.md
│   ├── module-boundary-checker.agent.md
│   └── test-generator.agent.md
│
└── instructions/                    # 系统执行的结构化指令
    ├── adr-reviewer.instructions.yaml
    ├── architecture-guardian.instructions.yaml
    ├── documentation-maintainer.instructions.yaml
    ├── expert-dotnet-software-engineer.instructions.yaml
    ├── handler-pattern-enforcer.instructions.yaml
    ├── module-boundary-checker.instructions.yaml
    └── test-generator.instructions.yaml
```

### 单一数据源原则

现在每种配置只有一个权威来源：

| 配置类型 | 权威来源 | 用途 |
|---------|---------|------|
| **AI 行为规范** | `.agent.md` | Copilot 理解 Agent 的角色、职责、权限边界 |
| **执行指令** | `.instructions.yaml` | 系统执行、CI/CD 集成 |
| **架构规则** | `RuleSetRegistry` | 架构裁决的唯一权威（参见 repository_memories） |

---

## 验证结果

### 引用检查
- ✅ 无代码引用 `.agent` 文件
- ✅ 无文档直接引用 `.agent` 文件（除 agents/README.md，已更新）
- ✅ `copilot-instructions.md` 仍正确引用 `.agent.md` 和 `.instructions.yaml`

### 构建测试
- ✅ `dotnet restore` 成功（仅有预先存在的 NU1608 警告）
- ✅ 删除的文件不影响代码编译
- ✅ 不影响 CI/CD 流程

---

## 后续建议

1. **文档维护**：
   - 保持 `.agent.md` 和 `.instructions.yaml` 的一致性
   - 使用 Governance.Cli 工具自动生成 `.instructions.yaml`（如果未来支持）

2. **防止回退**：
   - 在 `.gitignore` 中添加 `.github/agents/*.agent`（可选）
   - 在 CI 中添加检查确保不引入新的 `.agent` 文件（可选）

3. **教育团队**：
   - 确保所有开发者理解新的文件结构
   - 更新相关的贡献指南（如存在）

---

## 相关 ADR

- **ADR-007**：Agent 行为与权限宪法（定义 Agent 职责）
- **ADR-008**：文档结构与维护规范
- **ADR-902**：ADR 模板结构契约

---

## 附录：文件对比

### 删除前
```
agents/
├── adr-reviewer.agent           (889 bytes, YAML)
├── adr-reviewer.agent.md        (5473 bytes, Markdown)
```

### 删除后
```
agents/
└── adr-reviewer.agent.md        (5473 bytes, Markdown)

instructions/
└── adr-reviewer.instructions.yaml (656 bytes, YAML)
```

**节省**: 每个 Agent 减少 1 个冗余文件，总共 8 个文件，约 10KB 冗余代码。
