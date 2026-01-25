# 配置文件索引

> ⚠️ **无裁决力声明**：本文档仅供参考，不具备架构裁决权。
> 所有架构决策以相关 ADR 正文为准。详见 [ADR 目录](../adr/README.md)。

**目录**：`docs/configuration/`  
**用途**：集中管理项目级配置文件的文档说明  
**最后更新**：2026-01-24

---

## 目录

- [概述](#概述)
- [配置文件列表](#配置文件列表)
- [配置层级关系](#配置层级关系)
- [快速导航](#快速导航)
- [相关文档](#相关文档)

---

## 概述

本目录包含 Zss.BilliardHall 项目中各类配置文件的详细文档。这些配置文件分为两类：

### 1. 格式和风格配置

- **EditorConfig**（`.editorconfig`）：代码格式、缩进、换行符等
- 管理范围：纯技术性格式规范
- 变更流程：遵循 ADR-0900（技术层 ADR）

### 2. 架构和构建配置

- **Directory.Build.props**：MSBuild 全局属性、命名空间推导
- **Directory.Packages.props**：中央包管理（CPM）
- **Directory.Build.targets**：MSBuild 构建目标
- 管理范围：架构约束、依赖管理、编译配置
- 变更流程：遵循 ADR-0900（宪法层或结构层 ADR）

---

## 配置文件列表

| 配置文件                       | 位置    | 用途                    | 详细文档                                                            | 相关 ADR   |
|----------------------------|-------|-----------------------|-----------------------------------------------------------------|----------|
| `.editorconfig`            | 仓库根目录 | 代码格式和风格规范             | [editorconfig.md](editorconfig.md)                              | ADR-0900 |
| `Directory.Build.props`    | 仓库根目录 | MSBuild 全局属性、命名空间自动推导 | [Directory.Build.props 内联文档](../../Directory.Build.props)       | ADR-0003 |
| `Directory.Packages.props` | 仓库根目录 | 中央包管理（CPM），统一依赖版本     | [Directory.Packages.props 内联文档](../../Directory.Packages.props) | ADR-0004 |
| `Directory.Build.targets`  | 仓库根目录 | MSBuild 构建目标和自定义任务    | [Directory.Build.targets 内联文档](../../Directory.Build.targets)   | -        |
| `.gitignore`               | 仓库根目录 | Git 忽略文件规则            | -                                                               | -        |
| `.gitattributes`           | 仓库根目录 | Git 属性配置（换行符等）        | -                                                               | -        |

---

## 配置层级关系

```
┌─────────────────────────────────────────────────────────────┐
│                      ADR 宪法层                              │
│  ADR-0001: 模块化单体与垂直切片                              │
│  ADR-0002: Platform/Application/Host 三层启动                │
│  ADR-0003: 命名空间规则                                      │
│  ADR-0004: 中央包管理                                        │
│  ADR-0005: CQRS 和 Handler 模式                              │
└─────────────────────────────────────────────────────────────┘
                              │
                              ├─────────────────────────────────┐
                              ▼                                 ▼
┌─────────────────────────────────────┐   ┌────────────────────────────────┐
│       架构约束配置                   │   │      格式风格配置              │
│  (强制执行架构规则)                  │   │   (辅助开发体验)               │
│                                     │   │                                │
│  • Directory.Build.props            │   │  • .editorconfig               │
│    - 命名空间自动推导 (ADR-0003)     │   │    - 代码格式规范              │
│    - 层级识别和验证                  │   │    - 缩进、换行、字符集        │
│                                     │   │                                │
│  • Directory.Packages.props         │   │  • .gitattributes              │
│    - 中央包管理 (ADR-0004)           │   │    - 换行符自动转换            │
│    - 依赖版本统一控制                │   │                                │
│                                     │   │                                │
│  • 架构测试                          │   │                                │
│    - 自动化验证架构约束              │   │                                │
│    - CI 门禁                         │   │                                │
└─────────────────────────────────────┘   └────────────────────────────────┘
```

### 关键原则

1. **EditorConfig 不包含架构约束**
  - ✅ 控制：文件格式、缩进、换行符
  - ❌ 不控制：命名空间、依赖、分层、CQRS 模式

2. **架构约束由 ADR + MSBuild + 测试强制执行**
  - `Directory.Build.props`：自动推导命名空间，防止手动覆盖
  - `Directory.Packages.props`：统一管理包版本，根据 ADR-0004 禁止项目级版本
  - 架构测试：验证所有 ADR 约束合规性

3. **配置文件变更应遵循 ADR-0900**
  - 格式配置（EditorConfig）：技术层 ADR，单人批准
  - 架构配置（MSBuild、CPM）：可能涉及宪法层，严格审查

---

## 快速导航

### 按场景查找

| 我想...         | 查看文档                                                                                                                 |
|---------------|----------------------------------------------------------------------------------------------------------------------|
| 配置代码编辑器的格式规范  | [editorconfig.md](editorconfig.md)                                                                                   |
| 理解命名空间如何自动推导  | [ADR-0003](../adr/constitutional/ADR-0003-namespace-rules.md) + [Directory.Build.props](../../Directory.Build.props) |
| 添加或更新 NuGet 包 | [ADR-0004](../adr/constitutional/ADR-0004-Cpm-Final.md) + [Directory.Packages.props](../../Directory.Packages.props) |
| 修改构建配置        | [Directory.Build.props](../../Directory.Build.props) 内联文档                                                            |
| 解决架构测试失败      | [架构测试失败诊断](../copilot/architecture-test-failures.md)                                                                 |
| 修改任何配置文件      | [ADR-0900](../adr/governance/ADR-0900-adr-process.md)                                                                |

### 按文件类型查找

| 文件类型            | 格式规范                                                     | 架构约束                                                               |
|-----------------|----------------------------------------------------------|--------------------------------------------------------------------|
| C# 源代码 (*.cs)   | [editorconfig.md § C# 源代码文件](editorconfig.md#c-源代码文件-cs) | [ADR-0003 命名空间](../adr/constitutional/ADR-0003-namespace-rules.md) |
| 项目文件 (*.csproj) | [editorconfig.md § 项目配置文件](editorconfig.md#项目和配置文件)      | [ADR-0004 包管理](../adr/constitutional/ADR-0004-Cpm-Final.md)        |
| Markdown (*.md) | [editorconfig.md § 文档文件](editorconfig.md#文档和数据文件)        | -                                                                  |
| YAML (*.yml)    | [editorconfig.md § 文档文件](editorconfig.md#文档和数据文件)        | -                                                                  |
| JSON (*.json)   | [editorconfig.md § 文档文件](editorconfig.md#文档和数据文件)        | -                                                                  |

---

## IDE 配置快速入门

### Visual Studio 2022

**自动支持 EditorConfig 和 MSBuild 配置，无需额外设置**

1. 打开项目
2. EditorConfig 自动生效
3. `Directory.Build.props` 自动推导命名空间

**推荐配置**：

- 启用"保存时代码清理"（包含 EditorConfig 偏好）
- 快捷键：`Ctrl+K, Ctrl+D`（格式化文档）

**详细指南**：[editorconfig.md § Visual Studio 2022](editorconfig.md#visual-studio-2022)

### JetBrains Rider

**自动支持 EditorConfig 和 MSBuild 配置**

1. 打开项目
2. 验证 `Settings → Editor → Code Style → Enable EditorConfig support` 已勾选
3. 使用 `Ctrl+Alt+L` 重新格式化代码

**详细指南**：[editorconfig.md § JetBrains Rider](editorconfig.md#jetbrains-rider)

### Visual Studio Code

**需要安装扩展**：

```bash
# 必需：EditorConfig 支持
扩展市场安装："EditorConfig for VS Code"

# 可选：C# 支持
扩展市场安装："C#" 或 "C# Dev Kit"
```

**详细指南**：[editorconfig.md § Visual Studio Code](editorconfig.md#visual-studio-code)

---

## 常见问题

### Q1：为什么 EditorConfig 不包含命名空间规则？

**A**：EditorConfig 是一个格式化工具，仅控制文件格式（缩进、换行、字符集）。架构约束（如命名空间、分层、依赖）由以下机制强制执行：

- **命名空间**：`Directory.Build.props` 自动推导 + ADR-0003 + 架构测试
- **分层依赖**：ADR-0001、ADR-0002 + 架构测试
- **包管理**：`Directory.Packages.props` + ADR-0004 + 架构测试

这种分离确保：

1. 每个工具专注其职责
2. 架构约束可以被自动化测试强制执行
3. 格式问题不会干扰架构验证

### Q2：修改配置文件需要走什么流程？

**A**：根据 [ADR-0900（ADR 新增与修订流程）](../adr/governance/ADR-0900-adr-process.md)：

| 配置文件                           | 变更级别    | 审批要求              | 公示期  |
|--------------------------------|---------|-------------------|------|
| `.editorconfig`                | 技术层     | Tech Lead/架构师单人批准 | 无    |
| `Directory.Build.props`（格式性变更） | 技术层     | Tech Lead/架构师单人批准 | 无    |
| `Directory.Build.props`（架构性变更） | 结构层或宪法层 | 严格审查              | 建议讨论 |
| `Directory.Packages.props`     | 技术层     | Tech Lead/架构师单人批准 | 无    |

**所有变更应**（根据 ADR-0900）：

1. 提交 PR 并说明变更原因
2. 更新相关文档
3. 通过 CI 验证
4. 合并后团队公告

### Q3：EditorConfig 规则未生效怎么办？

**A**：参考 [editorconfig.md § 问题 1：EditorConfig 规则未生效](editorconfig.md#问题-1editorconfig-规则未生效)

常见原因：

1. IDE 未启用 EditorConfig 支持
2. IDE 缓存未刷新
3. VS Code 未安装扩展

解决方案：

- Visual Studio：验证"代码清理"配置
- Rider：验证 `Enable EditorConfig support`
- VS Code：安装 `EditorConfig for VS Code` 扩展

### Q4：架构测试失败怎么办？

**A**：EditorConfig 不负责架构约束！请参考：

1. **架构测试失败诊断**：[architecture-test-failures.md](../copilot/architecture-test-failures.md)
2. **查阅对应 ADR**：
  - 命名空间问题 → [ADR-0003](../adr/constitutional/ADR-0003-namespace-rules.md)
  - 依赖问题 → [ADR-0001](../adr/constitutional/ADR-0001-modular-monolith-vertical-slice-architecture.md)
  - 包管理问题 → [ADR-0004](../adr/constitutional/ADR-0004-Cpm-Final.md)
3. **Copilot Prompts**：[docs/copilot/](../copilot/)

---

## 相关文档

### ADR 体系

- [ADR 索引](../adr/README.md)
- [宪法层 ADR](../adr/constitutional/)
- [ADR-0900：ADR 新增与修订流程](../adr/governance/ADR-0900-adr-process.md)

### 配置文件文档

- [EditorConfig 详细文档](editorconfig.md)
- [.editorconfig 文件](../../.editorconfig)
- [Directory.Build.props](../../Directory.Build.props)
- [Directory.Packages.props](../../Directory.Packages.props)

### Copilot 指南

- [Copilot 指令索引](../copilot/README.md)
- [架构测试失败诊断](../copilot/architecture-test-failures.md)
- [ADR Prompts 合集](../copilot/)

### 开发指南

- [快速入门](../QUICK-START.md)
- [架构指南](../architecture-guide.md)
- [测试指南](../TESTING-GUIDE.md)
- [CI/CD 指南](../ci-cd-guide.md)

---

## 版本历史

| 版本  | 日期         | 变更摘要                        | 负责人  |
|-----|------------|-----------------------------|------|
| 1.0 | 2026-01-24 | 创建配置文件索引，整合 EditorConfig 文档 | 架构团队 |

---

**本目录是 Zss.BilliardHall 项目配置文件的中心索引和导航入口。**
