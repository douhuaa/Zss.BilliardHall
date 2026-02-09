# 共享测试辅助工具

本目录包含用于架构测试的共享辅助类和工具。

> **重要更新（2026-02-09）**：FileSystemTestHelper 已重构为三个专用类，提升单一职责原则遵循度和性能。详见 [重构总结](REFACTORING_SUMMARY.md)。

---

## 📋 工具类概览

| 工具类 | 职责 | 主要方法数 | 状态 |
|--------|------|-----------|------|
| **FileAssertionHelper** | 文件/目录断言 | 4 | ✅ 推荐使用 |
| **FileContentAnalyzer** | 内容分析（关键词、模式、表格） | 8 | ✅ 推荐使用 |
| **FileSearchHelper** | 文件搜索和路径操作 | 7 | ✅ 推荐使用 |
| **FileSystemTestHelper** | 向后兼容桥接 | 16 | ⚠️ 已废弃 |
| **AssemblyLoaderBase** | 程序集加载基类 | 4 | ✅ 内部使用 |
| **ModuleAssemblyData** | 模块程序集数据 | - | ✅ 使用中 |
| **HostAssemblyData** | Host 程序集数据 | - | ✅ 使用中 |
| **TestEnvironment** | 环境路径常量 | - | ✅ 使用中 |
| **AssertionMessageBuilder** | 断言消息构建 | 7 | ✅ 使用中 |
| **NetArchTestHelper** | NetArchTest 封装 | 5 | ✅ 使用中 |
| **AdrMarkdownBuilder** | ADR 文档构建 | 10+ | ✅ 使用中 |
| **AdrParser** | ADR 文档解析 | - | ✅ 使用中 |
| **AdrTestFixture** | ADR 测试固件 | - | ✅ 使用中 |

---

## 🎯 核心工具类详解

### 1. FileAssertionHelper（文件/目录断言）

**用途**：提供文件和目录存在性、内容断言功能

**主要方法**：

```csharp
// 断言文件存在
FileAssertionHelper.AssertFileExists(filePath, "❌ 文件不存在");

// 断言目录存在
FileAssertionHelper.AssertDirectoryExists(dirPath, "❌ 目录不存在");

// 断言文件包含特定内容
FileAssertionHelper.AssertFileContains(filePath, "期望内容", "❌ 缺少必需内容");

// 断言文件内容长度
FileAssertionHelper.AssertFileContentLength(filePath, 100, "❌ 文件内容过短");
```

**示例**：
```csharp
[Fact(DisplayName = "ADR-001_1_1: ADR 文档存在性检查")]
public void ADR_001_1_1_Document_Exists()
{
    var adrFile = FileSearchHelper.GetAbsolutePath("docs/adr/ADR-001.md");
    
    FileAssertionHelper.AssertFileExists(adrFile,
        AssertionMessageBuilder.BuildFileNotFoundMessage(
            ruleId: "ADR-001_1_1",
            filePath: adrFile,
            fileDescription: "ADR-001 文档",
            remediationSteps: new[] { "创建 ADR-001.md 文档" },
            adrReference: "docs/adr/governance/ADR-001.md"
        ));
}
```

---

### 2. FileContentAnalyzer（内容分析）

**用途**：分析文件内容，包括关键词检查、模式匹配、表格检测

**主要方法**：

```csharp
// 检查是否包含所有关键词
bool hasAll = FileContentAnalyzer.FileContainsAllKeywords(
    filePath, 
    new[] { "关键词1", "关键词2" }, 
    ignoreCase: true);

// 检查是否包含任一关键词
bool hasAny = FileContentAnalyzer.FileContainsAnyKeyword(
    filePath, 
    new[] { "关键词1", "关键词2" });

// 获取缺失的关键词
var missing = FileContentAnalyzer.GetMissingKeywords(
    filePath, 
    new[] { "必需词1", "必需词2" });

// 检查是否包含表格
bool hasTable = FileContentAnalyzer.FileContainsTable(
    filePath, 
    headerPattern: "列名1");

// 统计模式出现次数（流式读取，性能优化）
int count = FileContentAnalyzer.CountPatternOccurrences(
    filePath, 
    @"ADR-\d{3,4}", 
    excludeCodeBlocks: true);

// 获取匹配的行
var lines = FileContentAnalyzer.GetMatchingLines(
    filePath, 
    @"TODO:");
```

**性能特点**：
- ✅ 使用流式读取（StreamReader）处理大文件
- ✅ 大文件（>1MB）性能提升 60%+
- ✅ 避免重复读取文件

**示例**：
```csharp
[Fact(DisplayName = "ADR 文档必需章节检查")]
public void ADR_Document_Required_Sections()
{
    var adrFile = FileSearchHelper.GetAbsolutePath("docs/adr/ADR-001.md");
    
    var requiredSections = new[] { "## 决策", "## 背景", "## 后果" };
    var missing = FileContentAnalyzer.GetMissingKeywords(adrFile, requiredSections);
    
    missing.Should().BeEmpty(
        $"ADR 文档缺少必需章节：{string.Join(", ", missing)}");
}
```

---

### 3. FileSearchHelper（文件搜索和路径操作）

**用途**：文件搜索、路径转换、ADR/Agent 文件专用搜索

**主要方法**：

```csharp
// 获取目录中的文件
var files = FileSearchHelper.GetFilesInDirectory(
    dirPath, 
    "*.cs", 
    SearchOption.AllDirectories);

// 获取子目录
var subdirs = FileSearchHelper.GetSubdirectories(dirPath);

// 路径转换
var absolutePath = FileSearchHelper.GetAbsolutePath("docs/adr");
var relativePath = FileSearchHelper.GetRelativePath(absolutePath);

// 读取文件内容（带验证）
var content = FileSearchHelper.ReadFileContent(filePath);

// 获取 ADR 文件（使用 AdrFileFilter 过滤）
var adrFiles = FileSearchHelper.GetAdrFiles(
    subfolder: "constitutional",
    excludeTimeline: true);

// 获取 Agent 配置文件
var agentFiles = FileSearchHelper.GetAgentFiles(
    includeSystemAgents: false,
    excludeGuardian: true);
```

**示例**：
```csharp
[Fact(DisplayName = "所有 ADR 文档包含 Front Matter")]
public void All_ADR_Documents_Have_FrontMatter()
{
    var adrFiles = FileSearchHelper.GetAdrFiles();
    
    foreach (var file in adrFiles)
    {
        var content = FileSearchHelper.ReadFileContent(file);
        content.Should().StartWith("---", 
            $"{FileSearchHelper.GetRelativePath(file)} 缺少 Front Matter");
    }
}
```

---

### 4. AssemblyLoaderBase（程序集加载基类）

**用途**：提供统一的程序集加载逻辑，供 ModuleAssemblyData 和 HostAssemblyData 继承

**设计模式**：Template Method Pattern

**主要方法**（protected，供子类使用）：

```csharp
// 解析程序集路径候选列表
protected static List<string> ResolveAssemblyPathCandidates(
    string projectDir,
    string projectName,
    string configuration,
    string[] tfms);

// 加载单个程序集
protected static Assembly? LoadAssembly(
    string dllPath,
    string? expectedName = null);

// 批量加载程序集
protected static List<Assembly> LoadAssembliesFromDirectories(
    IEnumerable<string> directories,
    string configuration,
    string[] tfms,
    Func<string, string, bool>? nameValidator = null);

// 验证程序集列表非空
protected static void ValidateAssembliesNotEmpty(
    IReadOnlyList<Assembly> assemblies,
    string assemblyType);
```

**代码重复消除**：
- 消除 ModuleAssemblyData 和 HostAssemblyData 之间 ~70% 代码重复
- 统一路径解析逻辑（40+ 行 → 基类）
- 统一加载逻辑和错误处理

---

## 🔄 向后兼容桥接

### FileSystemTestHelper（已废弃）

**状态**：⚠️ 标记为 `Obsolete`，保留作为向后兼容桥接

**迁移指南**：

```csharp
// ❌ 旧代码（仍可用，但不推荐）
using static FileSystemTestHelper;
AssertFileExists(path, message);
var hasKeywords = FileContainsAllKeywords(path, keywords);

// ✅ 新代码（推荐）
using static FileAssertionHelper;
using static FileContentAnalyzer;
using static FileSearchHelper;

AssertFileExists(path, message);           // 断言
var hasKeywords = FileContainsAllKeywords(path, keywords);  // 内容分析
var files = GetAdrFiles();                 // 搜索
```

**选择合适的工具类**：

| 场景 | 推荐类 | 示例方法 |
|------|--------|----------|
| 文件/目录断言 | `FileAssertionHelper` | `AssertFileExists`, `AssertFileContains` |
| 关键词/模式检查 | `FileContentAnalyzer` | `FileContainsAllKeywords`, `CountPatternOccurrences` |
| 文件搜索/路径 | `FileSearchHelper` | `GetAdrFiles`, `GetRelativePath` |

---

## 🛠️ 其他辅助工具

### TestEnvironment

提供仓库路径常量，避免重复查找：

```csharp
var repoRoot = TestEnvironment.RepositoryRoot;
var adrPath = TestEnvironment.AdrPath;
var modulesPath = TestEnvironment.ModulesPath;
var configuration = TestEnvironment.BuildConfiguration;
```

---

### AssertionMessageBuilder

提供统一的断言消息格式：

```csharp
var message = AssertionMessageBuilder.Build(
    ruleId: "ADR-XXX_Y_Z",
    summary: "简短问题描述",
    currentState: "当前状态说明",
    remediationSteps: new[] { "步骤1", "步骤2", "步骤3" },
    adrReference: "docs/adr/ADR-XXX.md"
);

// 使用 NetArchTest 结果构建消息
var message2 = AssertionMessageBuilder.BuildFromArchTestResult(
    ruleId: "ADR-001_2_1",
    summary: "模块隔离违规",
    failingTypeNames: result.FailingTypeNames,
    remediationSteps: new[] { "移除跨模块引用", "使用领域事件通信" },
    adrReference: "docs/adr/constitutional/ADR-001.md"
);
```

---

### NetArchTestHelper

封装 NetArchTest 的常用模式：

```csharp
// 验证命名空间规则
NetArchTestHelper.AssertNamespaceConvention(
    assembly,
    expectedNamespacePrefix: "Zss.BilliardHall.Modules.Members",
    ruleId: "ADR-003_1_1",
    adrReference: "docs/adr/constitutional/ADR-003.md"
);

// 验证依赖规则
NetArchTestHelper.AssertNoDependencyOn(
    assembly,
    forbiddenDependencies: new[] { "System.Web", "Microsoft.AspNetCore.Mvc" },
    ruleId: "ADR-005_3_1",
    adrReference: "docs/adr/constitutional/ADR-005.md"
);
```

---

### AdrMarkdownBuilder

用于测试中构建 ADR 文档：

```csharp
var adrContent = new AdrMarkdownBuilder()
    .WithId("ADR-001")  // ✅ 新方法（推荐，带格式验证）
    .WithTitle("模块化单体架构")
    .WithStatus("Final")
    .WithDependsOn("ADR-002", "ADR-003")  // ✅ 统一命名
    .WithRelatedTo("ADR-005")
    .WithDecision("采用模块化单体架构...")
    .Build();

// ⚠️ 旧方法（向后兼容，但已标记 Obsolete）
// .DependsOn("ADR-002")  // 使用 WithDependsOn 代替
// .RelatedTo("ADR-005")  // 使用 WithRelatedTo 代替
```

---

## 📈 最佳实践

### 1. 优先使用专用辅助类

```csharp
// ❌ 避免：直接使用原生方法
if (!File.Exists(filePath))
    throw new FileNotFoundException(...);
var content = File.ReadAllText(filePath);

// ✅ 推荐：使用专用辅助类
FileAssertionHelper.AssertFileExists(filePath, message);
var content = FileSearchHelper.ReadFileContent(filePath);
```

### 2. 根据职责选择合适的工具类

| 操作类型 | 推荐工具类 |
|---------|-----------|
| 文件/目录断言 | `FileAssertionHelper` |
| 内容关键词检查 | `FileContentAnalyzer` |
| 模式匹配/表格检测 | `FileContentAnalyzer` |
| 文件搜索 | `FileSearchHelper` |
| 路径转换 | `FileSearchHelper` |
| 程序集加载 | `ModuleAssemblyData` / `HostAssemblyData` |
| 断言消息 | `AssertionMessageBuilder` |
| ADR 文档构建 | `AdrMarkdownBuilder` |

### 3. 使用相对路径提高可移植性

```csharp
// ✅ 推荐
var file = FileSearchHelper.GetAbsolutePath("docs/adr/ADR-001.md");

// ❌ 避免
var file = "/home/user/project/docs/adr/ADR-001.md";  // 硬编码绝对路径
```

### 4. 提供详细错误信息

```csharp
// ✅ 推荐：使用 AssertionMessageBuilder
var message = AssertionMessageBuilder.BuildFileNotFoundMessage(
    ruleId: "ADR-XXX_Y_Z",
    filePath: file,
    fileDescription: "ADR 文档",
    remediationSteps: new[] { "创建文档", "填写内容" },
    adrReference: "docs/adr/governance/ADR-XXX.md"
);
FileAssertionHelper.AssertFileExists(file, message);

// ❌ 避免：简单错误消息
FileAssertionHelper.AssertFileExists(file, "文件不存在");
```

### 5. 性能优化建议

```csharp
// ✅ 推荐：使用流式读取处理大文件
var count = FileContentAnalyzer.CountPatternOccurrences(
    largeFile,
    pattern,
    excludeCodeBlocks: true);  // 流式读取，性能提升 60%+

// ❌ 避免：全量加载大文件
var content = File.ReadAllText(largeFile);  // 可能导致内存压力
var count = Regex.Matches(content, pattern).Count;
```

---

## 🔄 迁移指南（从 FileSystemTestHelper）

### 快速查找替换

| 旧方法 | 新方法 | 所属类 |
|--------|--------|--------|
| `AssertFileExists` | `AssertFileExists` | `FileAssertionHelper` |
| `AssertDirectoryExists` | `AssertDirectoryExists` | `FileAssertionHelper` |
| `AssertFileContains` | `AssertFileContains` | `FileAssertionHelper` |
| `FileContainsAllKeywords` | `FileContainsAllKeywords` | `FileContentAnalyzer` |
| `FileContainsTable` | `FileContainsTable` | `FileContentAnalyzer` |
| `CountPatternOccurrences` | `CountPatternOccurrences` | `FileContentAnalyzer` |
| `GetFilesInDirectory` | `GetFilesInDirectory` | `FileSearchHelper` |
| `GetAbsolutePath` | `GetAbsolutePath` | `FileSearchHelper` |
| `GetRelativePath` | `GetRelativePath` | `FileSearchHelper` |
| `GetAdrFiles` | `GetAdrFiles` | `FileSearchHelper` |
| `ReadFileContent` | `ReadFileContent` | `FileSearchHelper` |

### 迁移示例

**迁移前**：
```csharp
using static FileSystemTestHelper;

[Fact]
public void Test_ADR_Document()
{
    var file = GetAbsolutePath("docs/adr/ADR-001.md");
    AssertFileExists(file, "文件不存在");
    var hasKeywords = FileContainsAllKeywords(file, new[] { "决策", "后果" });
    hasKeywords.Should().BeTrue();
}
```

**迁移后**：
```csharp
using static FileSearchHelper;
using static FileAssertionHelper;
using static FileContentAnalyzer;

[Fact]
public void Test_ADR_Document()
{
    var file = GetAbsolutePath("docs/adr/ADR-001.md");
    AssertFileExists(file, "文件不存在");
    var hasKeywords = FileContainsAllKeywords(file, new[] { "决策", "后果" });
    hasKeywords.Should().BeTrue();
}
```

---

## 📚 维护说明

### 添加新方法

如需添加新的辅助方法，请：

1. **选择合适的类**：根据职责选择 FileAssertionHelper / FileContentAnalyzer / FileSearchHelper
2. **添加参数验证**：所有公共方法必须验证参数（null/empty检查）
3. **编写 XML 注释**：包括参数说明、返回值、异常说明
4. **更新 README**：在本文档中添加使用示例
5. **添加单元测试**：验证新方法的正确性（建议）

### 性能考虑

- 大文件操作使用流式读取（StreamReader）
- 避免重复读取文件（提取私有辅助方法）
- 使用 Lazy<T> 延迟加载（如 TestEnvironment）

---

## 📄 相关文档

- [重构总结](REFACTORING_SUMMARY.md) - 详细的重构说明和性能对比
- [架构测试指南](/docs/guidelines/ARCHITECTURE-TEST-GUIDELINES.md) - 测试编写最佳实践
- [ADR-900](/docs/adr/governance/ADR-900.md) - 架构测试元规则

---

**最后更新**: 2026-02-09  
**维护者**: Architecture Team
