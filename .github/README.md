# GitHub 配置文件说明

本目录包含项目的 GitHub 相关配置文件。

## 文件说明

### Copilot 相关文件

#### 1. [copilot-instructions.md](./copilot-instructions.md)
**GitHub Copilot 代码审查指令**

- **用途**: 为 GitHub Copilot 提供项目架构规范和代码审查上下文
- **内容**: 
  - 垂直切片架构原则
  - 命名和代码风格规范
  - 日志与安全检查清单
  - Wolverine + Marten 使用规范
- **更新频率**: 当架构规范发生变化时更新
- **版本**: 1.1.0

#### 2. [copilot-templates.md](./copilot-templates.md)
**GitHub Copilot 常用指令模板**

- **用途**: 提供完整的 Copilot 提示词模板，帮助快速生成符合项目规范的代码
- **内容**:
  - 完整功能切片模板（Command + Handler + Endpoint + Validator）
  - Command、Query、Event 各类模板
  - Saga 工作流模板
  - 领域模型模板
  - 测试模板
- **使用场景**: 创建新功能时参考使用
- **版本**: 1.0.0

#### 3. [copilot-quick-start.md](./copilot-quick-start.md)
**Copilot 快速开始指南**

- **用途**: 5分钟快速上手 Copilot 模板
- **内容**:
  - 最常用的 7 个场景示例
  - 提示词模板变量说明
  - 工作流建议
  - 常见问题解答
- **适合人群**: 新加入项目的开发者
- **版本**: 1.0.0

### 其他配置文件

#### 4. [pull_request_template.md](./pull_request_template.md)
**PR 模板**

- **用途**: 统一 Pull Request 的描述格式
- **内容**: PR 描述、变更清单、测试说明等标准模板

#### 5. [workflows/](./workflows/)
**GitHub Actions 工作流**

- **用途**: CI/CD 自动化流程配置
- **内容**: 构建、测试、部署等自动化脚本

---

## 如何使用 Copilot 模板

### 快速开始（推荐）

1. **阅读** [copilot-quick-start.md](./copilot-quick-start.md)（5分钟）
2. **参考** 快速开始指南中的场景示例
3. **复制** 提示词到代码编辑器
4. **生成** 代码并检查调整

### 深入学习

1. **详细模板** 参考 [copilot-templates.md](./copilot-templates.md)
2. **架构规范** 参考 [copilot-instructions.md](./copilot-instructions.md)
3. **框架文档** 参考 [docs/03_系统架构设计/](../docs/03_系统架构设计/)

---

## 常见工作流

### 场景 1: 创建新功能

1. 打开 [copilot-quick-start.md](./copilot-quick-start.md)
2. 找到"创建完整功能切片"模板
3. 替换变量，生成代码
4. 手动补充核心业务逻辑

### 场景 2: 添加查询功能

1. 使用"创建查询"模板
2. 指定 Query 和 DTO
3. 生成 Handler 和 Endpoint

### 场景 3: 处理事件

1. 使用"创建事件"模板
2. 使用"创建事件处理器"模板
3. 实现具体业务逻辑

---

## 文档关系图

```
.github/
├── copilot-quick-start.md       ← 从这里开始（5分钟）
│   └── 引用 → copilot-templates.md    ← 详细模板和示例
│       └── 遵循 → copilot-instructions.md  ← 架构规范和约束
│
└── 配合使用 → docs/03_系统架构设计/
    ├── Wolverine快速上手指南.md
    └── Wolverine模块化架构蓝图.md
```

---

## 维护说明

### 何时更新这些文档

| 文档 | 更新时机 | 负责人 |
|------|---------|--------|
| copilot-instructions.md | 架构规范变更时 | 架构组 |
| copilot-templates.md | 新增通用模板时 | 开发团队 |
| copilot-quick-start.md | 新增常用场景时 | 开发团队 |

### 如何更新

1. 提交 PR 修改对应文档
2. 在 PR 描述中说明变更原因
3. 通过 Code Review
4. 合并后通知团队

---

## 反馈与改进

如果你发现：
- 模板不够准确
- 缺少常用场景
- 文档有错误

请：
1. 提交 Issue 说明问题
2. 或直接提交 PR 改进

---

**最后更新**: 2026-01-13  
**维护者**: 开发团队
