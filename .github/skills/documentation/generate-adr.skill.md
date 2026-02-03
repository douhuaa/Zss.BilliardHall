---
name: "Generate ADR"
description: "生成符合规范的 ADR 文档"
version: "1.0"
risk_level: "高"
category: "文档生成"
required_agent: "adr-reviewer"
---

# Generate ADR Skill

**类别**：文档生成  
**风险等级**：高  
**版本**：1.0

---

## 功能定义

### 用途

根据 ADR-0900 规范生成标准化的 ADR 文档模板，确保结构完整、格式正确。

### 输入参数

- `adrNumber`：字符串，ADR 编号（如 "0001"）
- `title`：字符串，ADR 标题
- `level`：字符串枚举，"宪法" / "结构" / "运行" / "技术" / "治理"
- `category`：字符串，类别（如 "constitutional", "structure"）
- `relatedAdrs`：字符串数组，相关 ADR 列表

### 输出结果

```json
{
  "generated": true,
  "files": [
    {
      "path": "docs/adr/constitutional/ADR-001-modular-monolith.md",
      "content": "...",
      "type": "ADR"
    }
  ],
  "summary": {
    "adrNumber": "0001",
    "level": "宪法",
    "sectionsIncluded": [
      "元数据",
      "本章聚焦内容",
      "决策",
      "理由",
      "影响",
      "相关 ADR"
    ]
  },
  "reminders": [
    "需要补充对应的架构测试",
    "需要创建 Copilot Prompts"
  ]
}
```

---

## 前置条件

### 必须满足的条件

- [ ] ADR 编号未被使用
- [ ] 类别目录存在
- [ ] 标题符合命名规范

### 必须的 Agent 授权

- **需要**：`adr-reviewer`
- **理由**：生成 ADR 直接影响架构决策记录的完整性

---

## 执行步骤

1. **验证输入参数**
  - 检查 ADR 编号唯一性
  - 验证级别有效
  - 验证类别目录存在

2. **生成文件名**
  - 格式：`ADR-{Number}-{kebab-case-title}.md`
  - 确定目标路径

3. **生成文档内容**
  - 使用标准模板
  - 填充元数据
  - 添加必需章节
  - 插入占位符

4. **创建文件**
  - 写入目标路径
  - 设置文件权限

5. **生成提醒**
  - 需要补充架构测试
  - 需要创建 Prompts
  - 需要更新索引

6. **记录日志**

---

## ADR 文档模板

```markdown
# ADR-{Number}：{Title}

**状态**：🚧 草稿  
**级别**：{Level}  
**日期**：{Date}  
**作者**：{Author}

---

## 本章聚焦内容

<!-- 简述本 ADR 关注的问题域和决策范围 -->

---

## 决策

### 核心约束

<!-- 【必须架构测试覆盖】标注需要测试的约束 -->

#### ✅ 允许的行为

<!-- 列出明确允许的模式和实践 -->

#### ❌ 禁止的行为

<!-- 列出明确禁止的模式和实践 -->

### 正确模式

```csharp
// ✅ 正确示例
```

### 错误模式

```csharp
// ❌ 错误示例
```

---

## 理由

### 为什么做这个决策

<!-- 解释决策背景和动机 -->

### 考虑的替代方案

<!-- 列出考虑过但未采纳的方案 -->

### 选择的理由

<!-- 解释为什么选择当前方案 -->

---

## 影响

### 对现有代码的影响

<!-- 描述对现有代码的影响 -->

### 对开发流程的影响

<!-- 描述对开发流程的影响 -->

### 风险和缓解措施

<!-- 识别风险并提供缓解措施 -->

---

## 相关 ADR

<!-- 列出相关 ADR 及其关系 -->

- [ADR-XXXX：相关标题](./ADR-XXXX-related.md) - 关系说明

---

## 版本历史

| 版本 | 日期 | 变更说明 |
|-----|------|---------|
| 1.0 | {Date} | 初始版本 |

---

**维护者**：{Maintainer}  
**审核人**：{Reviewer}  
**状态**：🚧 草稿
```

---

## 验证规则

### ADR 结构检查

- [ ] 文件名符合规范
- [ ] 包含所有必需章节
- [ ] 使用简体中文
- [ ] 包含代码示例
- [ ] 标注测试覆盖要求

### 元数据检查

- [ ] 状态标签正确
- [ ] 级别正确
- [ ] 日期格式正确
- [ ] 作者信息完整

---

## 回滚机制

### 如何回滚

1. 删除生成的 ADR 文件
2. 清理空的类别目录
3. 验证索引未更新

### 回滚验证

- [ ] ADR 文件已删除
- [ ] 其他 ADR 不受影响
- [ ] 编号可重用

---

## 危险信号

🚨 **必须阻止**：
- ADR 编号重复
- 缺少必需章节
- 格式不符合规范

---

## 生成后清单

生成 ADR 后必须完成：

- [ ] 补充决策内容
- [ ] 添加代码示例
- [ ] 标注【必须架构测试覆盖】
- [ ] 创建对应的架构测试
- [ ] 创建 Copilot Prompts
- [ ] 更新 docs/adr/README.md
- [ ] 更新类别 README
- [ ] 提交 PR 审查

---

## 使用示例

### 示例 1：生成宪法级 ADR

**输入**：
```json
{
  "adrNumber": "0010",
  "title": "Event Sourcing Pattern",
  "level": "宪法",
  "category": "constitutional",
  "relatedAdrs": ["ADR-005", "ADR-001"]
}
```

**输出**：
- 生成 `docs/adr/constitutional/ADR-0010-event-sourcing-pattern.md`
- 包含标准结构
- 提醒补充测试和 Prompts

---

## 参考资料

- [ADR-0900：ADR 流程](../../../docs/adr/governance/ADR-0900-adr-workflow-final.md)
- [ADR-008：文档编写规范](../../../docs/adr/constitutional/ADR-008-documentation-writing-maintenance-constitution.md)
- [ADR 模板](../../../docs/templates/adr-template.md)

---

**维护者**：架构委员会  
**状态**：✅ Active
