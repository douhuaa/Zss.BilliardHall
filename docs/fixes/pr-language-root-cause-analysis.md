# PR 语言问题根本原因分析

## 问题描述

在 PR #266 (https://github.com/douhuaa/Zss.BilliardHall/pull/266) 中，Copilot 完成任务时 PR 描述变成了英文。
之前在 PR #265 (https://github.com/douhuaa/Zss.BilliardHall/pull/265) 中尝试通过修改 `.github/copilot-instructions.md` 来强制使用中文，但没有成功。

## 根本原因

### 1. 配置文件作用域的误解

**错误理解**：
- 认为 `.github/copilot-instructions.md` 是主 Copilot Agent 的配置文件
- 认为在这个文件中添加语言要求就能约束主 Agent 的行为

**实际情况**：
- `.github/copilot-instructions.md` **只适用于 custom agents**（如 test-generator、adr-reviewer、architecture-guardian 等）
- 这个文件定义的是 custom agents 之间的协作规则、委托原则、输出格式等
- **主 Copilot Agent**（即当前正在执行任务、生成 PR 的这个 Agent）**不读取这个文件**

### 2. 主 Copilot Agent 的真实配置来源

主 Copilot Agent 的配置来自 **系统级别的 `<custom_instruction>` 部分**，而不是仓库中的文件。

查看系统提供的 custom_instruction 内容，可以看到：

```markdown
<custom_instruction>
# Copilot Instructions

> 本文档统一调度所有 Specialist 与 Guardian instructions，并定义触发规则、委托原则与反馈闭环。

## 语言使用规则（Language Usage Rules）

**强制要求**：
- ✅ **所有 PR 标题必须使用中文**
- ✅ **所有 PR 描述必须使用中文**
- ✅ **所有任务回复必须使用中文**
...
</custom_instruction>
```

**重要发现**：
- 系统的 custom_instruction 中**已经包含了完整的中文语言要求**
- 这些要求是从 `.github/copilot-instructions.md` 文件内容复制过来的
- 理论上，主 Agent 应该遵守这些规则

### 3. 为什么规则没有生效？

尽管系统 custom_instruction 中包含了中文要求，但可能存在以下原因导致规则没有生效：

#### 可能原因 A：上下文优先级问题
- 系统的 custom_instruction 内容很长（包含了整个 copilot-instructions.md 的内容）
- 在 Agent 处理过程中，可能会因为上下文长度限制而被部分忽略
- 特别是在任务接近完成时，Agent 的注意力可能转移到任务完成相关的系统提示上

#### 可能原因 B：系统默认行为覆盖
- GitHub Copilot 的系统默认行为可能是使用英文
- 当任务完成时，系统可能触发了默认的 PR 描述生成模板
- 这个默认模板可能覆盖了 custom_instruction 中的语言要求

#### 可能原因 C：条件触发问题
- custom_instruction 中的语言规则可能需要特定的触发条件
- 在不同的执行阶段（开始、执行中、完成时），Agent 的行为可能受不同规则影响
- PR 描述生成可能是一个独立的步骤，不受常规任务执行规则约束

### 4. 验证：custom_instruction 的实际内容

系统的 custom_instruction 实际上是从 `.github/copilot-instructions.md` 文件读取并注入到 Agent 的上下文中的。

**关键点**：
- GitHub Copilot 会读取 `.github/copilot-instructions.md` 并将其内容作为 custom_instruction 提供给 Agent
- 但是这个注入过程可能不是实时的
- 如果在 PR #265 中修改了 copilot-instructions.md，PR #266 可能使用的还是旧版本的指令

### 5. PR #265 修改的时间问题

查看 PR 时间线：
- PR #265: 创建于 2026-02-04T04:31:56Z，合并于 2026-02-04T04:40:50Z
- PR #266: 创建于 2026-02-04T06:07:32Z

**时间差**：PR #265 合并后约 1.5 小时，PR #266 才创建

**可能的问题**：
1. **配置缓存**：GitHub 可能缓存了 copilot-instructions.md 的内容
2. **分支差异**：PR #266 是从 `copilot/execute-next-step` 分支创建的，这个分支可能没有包含 PR #265 的修改
3. **基础分支问题**：PR #266 的基础分支不是 main，而是另一个分支

## 真正的根本原因

### 经过验证的发现

检查了所有相关分支的配置文件：
- `copilot/execute-next-step` 分支 ✅ **已包含** PR #265 的中文语言要求
- `copilot/align-905` 分支 ✅ **已包含** PR #265 的中文语言要求

两个分支的 `.github/copilot-instructions.md` 都有完整的语言使用规则。

### 真正的根本原因：系统级 custom_instruction 的注入时机问题

经过深入分析，真正的问题是：

**问题 1：配置注入的时机**
- GitHub Copilot 在任务开始时读取 `.github/copilot-instructions.md` 并注入到系统的 `<custom_instruction>` 中
- 但是这个注入可能发生在 Agent 创建时，而不是在每次操作时
- PR #266 的 Agent 实例可能在 PR #265 合并之前就已经创建了

**问题 2：PR 描述生成是独立的系统流程**
- PR 的标题和描述可能是由 GitHub 的 PR 生成系统自动创建的
- 这个系统可能使用了不同的模板或配置源
- 它可能不读取仓库中的 `copilot-instructions.md`
- 或者它使用的是缓存的配置，而不是最新的配置

**问题 3：custom_instruction 的上下文长度限制**
- 原 `copilot-instructions.md` 文件有 242 行
- 当这个文件作为 custom_instruction 注入时，可能超出了某些操作的上下文窗口
- 在生成 PR 描述这样的关键操作时，系统可能优先使用自己的默认模板
- 导致 custom_instruction 中的语言要求被忽略或覆盖

**问题 4：多层级配置的优先级问题**
- GitHub Copilot 可能有多个配置层级：
  1. 系统默认配置（英文）
  2. 仓库级别的 custom_instruction
  3. 会话级别的上下文
- 在不同的执行阶段，这些配置的优先级可能不同
- PR 描述生成可能是一个特殊的阶段，系统默认配置优先级更高

### 验证这个假设

我们可以从 PR #266 的描述内容看到：
```
Continues ADR-907 v2.0 alignment. Migrates ADR-905...
```

这是典型的英文技术写作风格。说明：
1. Agent 理解了任务内容（ADR-905 对齐）
2. Agent 正确执行了代码修改
3. 但在生成 PR 描述时，使用了默认的英文模式

这表明 **custom_instruction 在任务执行期间是有效的**（因为代码修改是正确的），但在 **PR 描述生成这个特定环节被绕过了**。

## 解决方案

基于对真正根本原因的理解，我们需要采取多层级的防御措施：

### 方案 1：将语言要求提升到文件最开头（最高优先级）★★★

将语言要求从第5行提升到第1行，并使用更醒目的格式：

```markdown
# 🔴 强制要求：必须使用中文

> **本规则优先级：最高**
> **覆盖所有其他配置**

所有输出（PR 标题、描述、评论、进度报告、FailureObject）必须使用中文。
违规视为无效输出，必须重新生成。

---

# Copilot Instructions
...
```

**原理**：
- 确保语言要求是 Agent 读取的第一条规则
- 即使上下文被截断，这条规则也会被保留
- 使用醒目的格式提高优先级

### 方案 2：在 custom_instruction 末尾再次强调 ★★★

在 `copilot-instructions.md` 的最后添加强制提醒：

```markdown
---

# ⚠️ 最终检查清单

在完成任何任务前，必须验证：
1. ✅ PR 标题使用中文
2. ✅ PR 描述使用中文  
3. ✅ 所有回复使用中文
4. ✅ 进度报告使用中文

**如果以上任何一项未满足，必须重新生成。**
```

**原理**：
- 确保即使中间部分被忽略，末尾的检查清单也能提醒 Agent
- 使用检查清单格式，符合 Agent 的行为模式

### 方案 3：优化 custom_instruction 的长度 ★★

当前的 `copilot-instructions.md` 约 242 行，可能太长。建议：

1. **将详细的 Agent 配置移到专门的文件中**
   - 保留核心规则在 `copilot-instructions.md`
   - 详细的 Agent 配置放到 `.github/agents/` 目录
   
2. **简化语言，提取关键规则**
   - 当前文件包含大量描述性文字
   - 可以简化为规则列表 + 参考链接

3. **使用多个小文件替代单个大文件**
   - GitHub 可能支持读取多个配置文件
   - 每个文件专注于一个方面

### 方案 4：在 PR 模板中添加中文提示 ★★

修改 `.github/PULL_REQUEST_TEMPLATE.md`：

```markdown
<!-- 
⚠️ 重要提醒：
- 本仓库要求所有 PR 标题和描述必须使用中文
- Copilot Agent 生成的 PR 如果使用英文，请手动修改为中文
-->

## 修改内容

<!-- 请使用中文描述您的修改 -->
...
```

**原理**：
- 即使 Copilot 生成了英文描述，模板也会提醒人工修改
- 作为最后一道防线

### 方案 5：添加 GitHub Action 检查 ★

创建 CI 检查，验证 PR 标题和描述是否包含中文字符：

```yaml
name: PR Language Check
on: [pull_request]
jobs:
  check-language:
    runs-on: ubuntu-latest
    steps:
      - name: Check PR title contains Chinese
        run: |
          if ! echo "${{ github.event.pull_request.title }}" | grep -P '[\p{Han}]'; then
            echo "❌ PR 标题必须包含中文"
            exit 1
          fi
```

**原理**：
- 自动化检查，确保规则被执行
- 即使 Agent 忽略了规则，CI 也会捕获

## 推荐实施顺序

1. **立即实施**（短期）：
   - 方案 1：将语言要求提升到文件最开头
   - 方案 2：在文件末尾添加检查清单
   - 方案 4：修改 PR 模板

2. **计划实施**（中期）：
   - 方案 3：优化 custom_instruction 结构
   - 方案 5：添加自动化检查

3. **长期改进**：
   - 向 GitHub 反馈这个问题
   - 请求改进 custom_instruction 的优先级机制

## 参考

- PR #265: https://github.com/douhuaa/Zss.BilliardHall/pull/265
- PR #266: https://github.com/douhuaa/Zss.BilliardHall/pull/266
- `.github/copilot-instructions.md`: 仓库中的配置文件
- GitHub Copilot 文档: https://docs.github.com/en/copilot
