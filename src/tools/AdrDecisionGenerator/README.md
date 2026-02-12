# Zss.BilliardHall.AdrDecisionGenerator

## 📖 概述

AdrDecisionGenerator 是一个专门用于生成和管理 Architecture Decision Record (ADR) 文档的工具类库。该类库实现了垂直切片架构的模块化设计，将决策生成功能从测试项目中独立出来，提供可复用的 ADR 文档生成能力。

## 🎯 主要功能

### 1. AdrDecisionGenerator
从架构规则集 (`ArchitectureRuleSet`) 生成 Markdown 格式的 Decision 章节。

**特性：**
- 支持自定义生成选项（标题层级、Markdown 转义等）
- 性能优化：O(M log M + N) 复杂度
- 跨平台一致性：统一行尾为 LF
- 防御式编程：完整的参数验证

### 2. AdrDocumentMerger
将生成的 Decision 章节与现有 ADR 文档合并，保留其他章节（Front Matter、Context、Consequences 等）。

**特性：**
- 使用 Markdig 解析 Markdown 文档
- 智能章节排序
- 保留 YAML Front Matter
- 非破坏性合并

### 3. FrontMatterParser
解析 Markdown 文件的 YAML Front Matter。

**特性：**
- 快速解析模式（前 N 行）
- 完整解析模式（所有字段）
- 高性能：避免全文件读取

## 📦 依赖项

- **Markdig**: Markdown 解析和渲染
- **YamlDotNet**: YAML 序列化（为未来扩展预留）
- **ArchitectureTests**: 访问架构规则定义

## 🚀 使用示例

### 生成 Decision 章节

```csharp
using Zss.BilliardHall.AdrDecisionGenerator;

// 创建生成器
var generator = new AdrDecisionGenerator();

// 从规则集生成 Decision 章节
string decisionContent = generator.GenerateDecisionSection(ruleSet);

// 使用自定义选项
var options = new DecisionGenerationOptions 
{
    IncludeSectionHeader = true,
    HeaderLevelOffset = 1,
    EscapeMarkdown = true
};
string customContent = generator.GenerateDecisionSection(ruleSet, options);
```

### 合并 ADR 文档

```csharp
using Zss.BilliardHall.AdrDecisionGenerator;

// 创建合并器
var generator = new AdrDecisionGenerator();
var merger = new AdrDocumentMerger(generator);

// 读取现有 ADR 文档
string existingAdr = File.ReadAllText("ADR-001.md");

// 合并新的 Decision 章节
string mergedContent = merger.MergeDecisionSection(existingAdr, ruleSet);

// 保存合并后的文档
File.WriteAllText("ADR-001.md", mergedContent);
```

### 解析 Front Matter

```csharp
using Zss.BilliardHall.AdrDecisionGenerator;

// 从文件快速解析
var frontMatter = FrontMatterParser.ParseFromFileQuick("ADR-001.md");
if (frontMatter.HasFrontMatter)
{
    Console.WriteLine($"ADR: {frontMatter.AdrField}");
    Console.WriteLine($"Type: {frontMatter.TypeField}");
}

// 从文本完整解析
string content = File.ReadAllText("ADR-001.md");
var fullData = FrontMatterParser.ParseFromText(content);
```

## 🏗️ 架构设计

### 设计原则

- **单一职责原则（SRP）**: 每个类专注于单一功能
- **开闭原则（OCP）**: 通过接口扩展，避免修改现有代码
- **依赖倒置原则（DIP）**: 依赖抽象而非具体实现
- **垂直切片架构**: 功能独立，减少全局依赖

### 命名空间结构

```
Zss.BilliardHall.AdrDecisionGenerator
├── IAdrDecisionGenerator (接口)
├── AdrDecisionGenerator (实现)
├── DecisionGenerationOptions (配置)
├── IAdrDocumentMerger (接口)
├── AdrDocumentMerger (实现)
└── FrontMatterParser (工具)
```

## 🔧 配置选项

### DecisionGenerationOptions

| 选项 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `IncludeSectionHeader` | `bool` | `true` | 是否包含 "## Decision" 标题 |
| `IncludeWarningNote` | `bool` | `true` | 是否包含警告说明 |
| `HeaderLevelOffset` | `int` | `0` | 标题层级偏移量（0-2） |
| `AddBlankLinesBetweenClauses` | `bool` | `false` | 是否在条款之间添加空行 |
| `EscapeMarkdown` | `bool` | `true` | 是否转义 Markdown 特殊字符 |

## 📝 开发笔记

### 性能优化

- **条款检索优化**: 从 O(N*M) 降低到 O(M log M + N)
- **Front Matter 快速解析**: 只读取前 50 行，避免全文件扫描
- **预编译正则表达式**: 使用 `RegexOptions.Compiled`

### 跨平台兼容性

- 所有生成的内容使用 LF (`\n`) 作为行尾
- 通过 `NormalizeNewlines()` 统一处理

### 安全性

- 完整的参数验证（`ArgumentNullException.ThrowIfNull`）
- Markdown 特殊字符转义，防止生成破坏性内容
- 防御式编程：早期返回，避免深层嵌套

## 📚 相关文档

- [ADR-001: 模块化单体架构](../../../docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)
- [Architecture Decision Records](../../../docs/adr/)

## 🤝 贡献指南

本类库遵循以下开发规范：

1. **代码风格**: 遵循 .NET 编码规范
2. **测试覆盖**: 所有公共 API 需要单元测试
3. **文档完整**: 公共方法需要 XML 注释
4. **版本兼容**: 保持向后兼容性

## 📄 许可证

本项目采用与主项目相同的许可证。

---

**最后更新**: 2026-02-12  
**版本**: 1.0.0  
**状态**: ✅ 可用
