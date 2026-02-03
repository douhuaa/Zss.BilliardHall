# ADR 语义模型解析器

## 概述

ADR 语义模型解析器是一个基于 Markdig 的 .NET 库，用于将 ADR（架构决策记录）Markdown 文档解析为结构化的语义模型。

## 功能特性

- ✅ **完整的 ADR 解析**：提取 ADR 编号、标题、状态、级别等元数据
- ✅ **关系声明解析**：解析依赖、被依赖、替代、被替代和相关关系
- ✅ **术语表提取**：自动提取术语定义和英文对照
- ✅ **快速参考表提取**：提取约束编号和测试方式
- ✅ **JSON 序列化**：支持单个或批量 ADR 的 JSON 序列化
- ✅ **命令行工具**：提供易用的 CLI 工具

## 项目结构

```
AdrSemanticParser/
├── Zss.BilliardHall.AdrSemanticParser/    # 核心库
│   ├── Models/
│   │   └── AdrSemanticModel.cs            # 数据模型
│   ├── AdrParser.cs                       # 解析器
│   └── AdrSerializer.cs                   # 序列化器
├── AdrParserCli/                          # 命令行工具
│   └── Program.cs
└── Tests/                                 # 单元测试
    ├── AdrParserTests.cs
    └── AdrSerializerTests.cs
```

## 快速开始

### 作为库使用

```csharp
using Zss.BilliardHall.AdrSemanticParser;

// 创建解析器
var parser = new AdrParser();

// 解析单个文件
var model = await parser.ParseFileAsync("docs/adr/ADR-001.md");

// 输出 JSON
var serializer = new AdrSerializer();
var json = serializer.Serialize(model);
Console.WriteLine(json);
```

### 使用 CLI 工具

```bash
# 构建 CLI 工具
dotnet build src/tools/AdrSemanticParser/AdrParserCli/AdrParserCli.csproj

# 解析单个 ADR
dotnet run --project src/tools/AdrSemanticParser/AdrParserCli/AdrParserCli.csproj \
  -- parse docs/adr/ADR-001.md output.json

# 批量解析
dotnet run --project src/tools/AdrSemanticParser/AdrParserCli/AdrParserCli.csproj \
  -- batch docs/adr all-adrs.json
```

## 数据模型

### AdrSemanticModel

主要的 ADR 语义模型，包含以下字段：

```json
{
  "id": "ADR-940",
  "title": "ADR 关系与溯源管理宪法",
  "status": "✅ Accepted",
  "level": "治理层 / 架构元规则",
  "scope": "所有 ADR 文档",
  "version": "1.0",
  "effectiveDate": "即刻",
  "relationships": {
    "dependsOn": [
      {
        "id": "ADR-008",
        "title": "文档编写与维护宪法",
        "reason": "基于文档规范",
        "relativePath": "../constitutional/ADR-008.md"
      }
    ],
    "dependedBy": [],
    "supersedes": [],
    "supersededBy": [],
    "related": []
  },
  "glossary": [
    {
      "term": "关系声明",
      "definition": "ADR 文档中声明与其他 ADR 关系的章节",
      "englishTerm": "Relationship Declaration"
    }
  ],
  "quickReference": [],
  "filePath": "docs/adr/governance/ADR-940.md"
}
```

## 支持的 ADR 格式

解析器支持标准的 ADR 格式：

### 元数据

```markdown
# ADR-XXXX：标题

**状态**：✅ Final  
**级别**：架构约束  
**适用范围**：所有模块  
**版本**：1.0  
**生效时间**：即刻
```

### 关系声明

```markdown
## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-001：模块化单体架构](./ADR-001.md) - 基于模块隔离规则

**被依赖（Depended By）**：
- [ADR-002：平台架构](./ADR-002.md)

**替代（Supersedes）**：无

**被替代（Superseded By）**：无

**相关（Related）**：
- [ADR-003：命名规范](./ADR-003.md) - 相关约束
```

### 术语表

```markdown
## 术语表（Glossary）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 模块 | 独立的业务单元 | Module |
| 契约 | 数据传输对象 | Contract |
```

### 快速参考表

```markdown
## 快速参考表

| 约束编号 | 约束描述 | 测试方式 | 测试用例 | 必须遵守 |
|---------|---------|---------|---------|----------|
| ADR-001.1 | 模块不可相互引用 | L1 - NetArchTest | Test_Name | ✅ |
```

## API 参考

### AdrParser

```csharp
// 解析 Markdown 字符串
AdrSemanticModel Parse(string markdown, string? filePath = null)

// 解析文件
Task<AdrSemanticModel> ParseFileAsync(string filePath, CancellationToken ct = default)
```

### AdrSerializer

```csharp
// 序列化为 JSON
string Serialize(AdrSemanticModel model)

// 序列化到文件
Task SerializeToFileAsync(AdrSemanticModel model, string filePath, CancellationToken ct = default)

// 反序列化
AdrSemanticModel? Deserialize(string json)

// 从文件反序列化
Task<AdrSemanticModel?> DeserializeFromFileAsync(string filePath, CancellationToken ct = default)

// 批量序列化
string SerializeBatch(IEnumerable<AdrSemanticModel> models)
```

## 运行测试

```bash
dotnet test src/tools/AdrSemanticParser/Tests/Zss.BilliardHall.AdrSemanticParser.Tests.csproj
```

## 技术栈

- **.NET 9.0**：目标框架
- **Markdig**：Markdown 解析库
- **System.Text.Json**：JSON 序列化
- **xUnit + FluentAssertions**：单元测试

## 使用场景

1. **ADR 关系分析**：分析 ADR 之间的依赖关系
2. **文档验证**：验证 ADR 格式是否符合规范
3. **自动化工具集成**：将 ADR 数据集成到 CI/CD 流程
4. **知识图谱构建**：基于 ADR 关系构建知识图谱
5. **文档搜索**：提供结构化搜索功能

## 示例输出

解析 ADR-940 后的 JSON 输出（部分）：

```json
{
  "id": "ADR-940",
  "title": "ADR 关系与溯源管理宪法",
  "status": "✅ Accepted（已采纳）",
  "level": "治理层 / 架构元规则",
  "relationships": {
    "dependsOn": [
      {
        "id": "ADR-008",
        "title": "文档编写与维护宪法",
        "reason": "基于文档规范"
      }
    ]
  },
  "glossary": [
    {
      "term": "关系声明",
      "definition": "ADR 文档中声明与其他 ADR 关系的章节",
      "englishTerm": "Relationship Declaration"
    }
  ]
}
```

## 许可证

本项目遵循仓库的整体许可证。

## 维护者

- Zss.BilliardHall 架构委员会

## 版本历史

- **1.0.0** (2026-01-26) - 初始版本
  - 基础 ADR 解析功能
  - 关系声明提取
  - 术语表和快速参考表提取
  - CLI 工具
  - 单元测试覆盖
