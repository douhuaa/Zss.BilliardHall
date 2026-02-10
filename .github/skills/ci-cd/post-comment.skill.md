---
name: "Post Comment"
description: "在 GitHub PR 或 Issue 上发布评论"
version: "1.0"
risk_level: "低"
category: "CI/CD 集成"
required_agent: "adr-reviewer"
dependencies: []  # 无前置依赖，可独立执行
post_execution: []  # 无后续操作
---

# Post Comment Skill

**类别**：CI/CD 集成  
**风险等级**：低  
**版本**：1.0

---

## 功能定义

### 用途

在 GitHub Pull Request 或 Issue 上自动发布格式化的评论，用于反馈代码审查、测试结果或架构合规性检查。

### 输入参数

- `target`：字符串枚举，"pr" / "issue"
- `number`：数字，PR 或 Issue 编号
- `commentType`：字符串枚举，"architecture-review" / "test-result" / "general"
- `content`：对象，评论内容（标题、摘要、详情等）
- `tag`：字符串，可选，标签（如 "architecture", "testing"）

### 输出结果

```json
{
  "success": true,
  "comment": {
    "id": 123456789,
    "url": "https://github.com/douhuaa/Zss.BilliardHall/pull/152#issuecomment-123456789",
    "createdAt": "2026-01-26T10:30:00Z"
  },
  "summary": {
    "target": "pr",
    "number": 152,
    "commentType": "architecture-review"
  }
}
```

---

## 前置条件

### 必须满足的条件

- [ ] GitHub API 访问权限
- [ ] PR/Issue 存在
- [ ] 评论内容已格式化

### 必须的 Agent 授权

- **需要**：`adr-reviewer` 或 `architecture-guardian`
- **理由**：公开评论需要确保内容准确和专业

---

## 执行步骤

1. **验证输入参数**
  - 检查目标类型有效
  - 验证编号存在
  - 验证内容格式

2. **格式化评论**
  - 根据 commentType 选择模板
  - 填充内容
  - 添加标签和标题

3. **发布评论**
  - 使用 GitHub API
  - 处理错误
  - 记录结果

4. **验证发布**
  - 检查评论已发布
  - 获取评论 URL

5. **记录日志**

---

## 评论模板

### 架构审查评论

```markdown
## 🏛️ 架构审查报告

由 @architecture-guardian 自动生成

---

### ✅ 合规方面

{compliantItems}

---

### ⚠️ 需要关注

{concernItems}

---

### ❌ 检测到违规

{violationItems}

---

### 📚 相关资源

- [ADR-{number}](link)
- [Prompts](link)

---

<sub>
此评论由架构守护者自动生成 | 
基于 ADR-007 | 
[了解更多](docs/adr/constitutional/ADR-007-...)
</sub>
```

### 测试结果评论

```markdown
## 🧪 测试结果

---

### 📊 总览

| 类型 | 总数 | ✅ 通过 | ❌ 失败 | ⏭️ 跳过 |
|-----|------|--------|--------|--------|
| 单元测试 | {total} | {passed} | {failed} | {skipped} |
| 架构测试 | {total} | {passed} | {failed} | {skipped} |

---

### ❌ 失败详情

{failureDetails}

---

<sub>
此评论由测试生成器自动生成 | 
[查看完整报告](link)
</sub>
```

### 一般评论

```markdown
## {title}

{content}

---

<sub>
由 {agent} 生成 | {timestamp}
</sub>
```

---

## 评论内容规范

### Markdown 格式

- 使用清晰的标题层级
- 使用表格展示数据
- 使用表情符号增强可读性
- 使用链接引用资源

### 专业性要求

- 语言清晰、准确
- 避免模糊表述
- 提供具体建议
- 引用 ADR 正文

---

## 验证规则

### 评论质量检查

- [ ] 格式正确
- [ ] 内容完整
- [ ] 链接有效
- [ ] 引用准确

### 避免垃圾评论

- [ ] 不重复发布相同内容
- [ ] 不发布空评论
- [ ] 不发布模糊信息

---

## 错误处理

### 常见错误

| 错误 | 原因 | 处理方式 |
|------|------|---------|
| 403 Forbidden | 权限不足 | 检查 API token |
| 404 Not Found | PR/Issue 不存在 | 验证编号 |
| 422 Validation Failed | 内容格式错误 | 修正格式 |

---

## 使用示例

### 示例 1：发布架构审查评论

**输入**：
```json
{
  "target": "pr",
  "number": 152,
  "commentType": "architecture-review",
  "content": {
    "compliant": ["模块隔离正确", "CQRS 分离清晰"],
    "concerns": ["缺少单元测试"],
    "violations": [],
    "relatedAdrs": ["ADR-001", "ADR-005"]
  },
  "tag": "architecture"
}
```

**输出**：
- 发布格式化的架构审查评论
- 包含合规、关注点和违规信息
- 链接到相关 ADR

### 示例 2：发布测试结果评论

**输入**：
```json
{
  "target": "pr",
  "number": 152,
  "commentType": "test-result",
  "content": {
    "unitTests": {
      "total": 145,
      "passed": 145,
      "failed": 0
    },
    "architectureTests": {
      "total": 15,
      "passed": 13,
      "failed": 2
    },
    "failures": [
      {
        "test": "ADR_001_Test",
        "reason": "..."
      }
    ]
  }
}
```

**输出**：
- 发布测试结果摘要
- 表格展示统计
- 详细失败信息

---

## 危险信号

⚠️ **警告**：
- 评论频率过高（spam）
- 评论内容错误或误导
- 评论未经 Agent 授权

---

## 参考资料

- [GitHub API 文档](https://docs.github.com/en/rest)
- [ADR-007：Agent 行为规范](../../../docs/adr/constitutional/ADR-007-agent-behavior-permissions-constitution.md)

---

**维护者**：架构委员会  
**状态**：✅ Active
