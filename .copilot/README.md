# Copilot 指令文件系统 (Copilot Instruction File System)

这个目录包含了针对智慧台球厅管理系统的详细 GitHub Copilot 指令文件，旨在实现机器可读优先、人机混合协作、流程自动化的开发目标。

## 目录结构 (Directory Structure)

```
.copilot/
├── README.md                    # 本文件
├── schemas/                     # 机器可读的数据架构定义
│   ├── entities.json           # 业务实体架构
│   ├── api-responses.json      # API 响应格式
│   ├── database-schema.json    # 数据库架构
│   └── config-schema.json      # 配置文件架构
├── patterns/                   # 代码模式和约定
│   ├── coding-patterns.md      # 代码编写模式
│   ├── api-patterns.md         # API 设计模式
│   ├── database-patterns.md    # 数据库设计模式
│   ├── testing-patterns.md     # 测试模式
│   ├── frontend-patterns.md    # 前端开发模式
│   └── security-patterns.md    # 安全模式
├── workflows/                  # 工作流和自动化
│   ├── README.md              # 工作流说明
│   ├── development.md         # 开发工作流
│   ├── testing.md             # 测试工作流
│   ├── deployment.md          # 部署工作流
│   └── maintenance.md         # 维护工作流
└── templates/                 # 代码生成模板
    ├── controller-template.md  # 控制器模板
    ├── service-template.md     # 服务层模板
    ├── repository-template.md  # 数据访问模板
    ├── dto-template.md         # DTO 模板
    ├── component-template.md   # 前端组件模板
    └── test-template.md        # 测试模板
```

## 使用指南 (Usage Guidelines)

### 1. 机器可读优先 (Machine-Readable First)

所有架构定义和配置都以 JSON/YAML 格式提供，便于 Copilot 理解和处理：

```json
{
  "$schema": "./schemas/entities.json#",
  "entity": "BilliardTable",
  "properties": {
    "id": "string",
    "number": "number",
    "type": "TableType",
    "status": "TableStatus"
  }
}
```

### 2. 人机混合协作 (Human-AI Collaboration)

每个模式文件都包含：
- 机器可读的规则定义
- 人类可读的解释和示例
- 上下文感知的使用场景

### 3. 流程自动化 (Process Automation)

工作流文件定义了：
- 自动化开发流程
- 代码质量检查
- 测试和部署策略
- 持续集成/持续部署

## 快速开始 (Quick Start)

1. **创建新的 API 端点**：
   ```bash
   # Copilot 将根据 api-patterns.md 和相关模板生成代码
   # 提示词：基于 BilliardTable 实体创建完整的 CRUD API
   ```

2. **添加新的业务实体**：
   ```bash
   # 参考 entities.json 架构
   # 提示词：为台球厅预约系统创建 Reservation 实体及相关代码
   ```

3. **实现前端组件**：
   ```bash
   # 使用 component-template.md 和 frontend-patterns.md
   # 提示词：创建台球桌状态显示组件，支持实时更新
   ```

## 文件更新策略 (File Update Strategy)

- **schemas/**: 当业务模型变更时更新
- **patterns/**: 当发现新的最佳实践时更新
- **workflows/**: 当流程优化时更新
- **templates/**: 当代码结构标准化时更新

## 贡献指南 (Contributing)

当添加新的指令文件时，请确保：
1. 遵循现有的文件命名约定
2. 提供清晰的机器可读格式
3. 包含详细的使用示例
4. 添加适当的验证规则

---

> 本指令系统设计用于提高开发效率，确保代码质量，并促进团队协作。请根据项目需求持续优化和完善。