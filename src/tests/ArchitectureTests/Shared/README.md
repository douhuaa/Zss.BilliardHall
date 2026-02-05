# 共享测试辅助工具

本目录包含用于架构测试的共享辅助类和工具。

## FileSystemTestHelper

`FileSystemTestHelper` 提供统一的文件系统操作方法，用于简化测试中的文件和目录操作。

### 主要功能

#### 1. 文件存在性断言

```csharp
// 断言文件存在，如果不存在则抛出带有详细信息的异常
var testFile = FileSystemTestHelper.GetAbsolutePath("src/tests/MyTest.cs");
FileSystemTestHelper.AssertFileExists(testFile, "❌ 测试文件不存在");
```

#### 2. 目录存在性断言

```csharp
// 断言目录存在
var docsPath = FileSystemTestHelper.GetAbsolutePath("docs");
FileSystemTestHelper.AssertDirectoryExists(docsPath, "文档目录不存在");
```

#### 3. 安全读取文件内容

```csharp
// 读取文件内容（自动检查文件是否存在）
var content = FileSystemTestHelper.ReadFileContent(filePath);
```

#### 4. 文件内容断言

```csharp
// 断言文件包含特定内容
FileSystemTestHelper.AssertFileContains(filePath, "expectedText", "文件应包含 expectedText");

// 断言文件内容长度
FileSystemTestHelper.AssertFileContentLength(filePath, 100, "文件内容过短");
```

#### 5. 目录文件遍历

```csharp
// 获取目录中的所有 .cs 文件
var files = FileSystemTestHelper.GetFilesInDirectory(
    directoryPath, 
    "*.cs", 
    SearchOption.AllDirectories
);

// 获取子目录列表
var subdirs = FileSystemTestHelper.GetSubdirectories(directoryPath);
```

#### 6. 路径转换

```csharp
// 将相对路径转换为绝对路径
var absolutePath = FileSystemTestHelper.GetAbsolutePath("docs/adr");

// 将绝对路径转换为相对路径
var relativePath = FileSystemTestHelper.GetRelativePath(absolutePath);
```

### 完整使用示例

**场景：验证 ADR 文档存在且包含必需内容**

```csharp
[Fact(DisplayName = "ADR-XXX_Y_Z: 文档规范检查")]
public void ADR_XXX_Y_Z_Document_Standards()
{
    // 1. 构建文档路径（使用相对路径）
    var adrFile = FileSystemTestHelper.GetAbsolutePath("docs/adr/ADR-XXX.md");
    
    // 2. 断言文件存在
    FileSystemTestHelper.AssertFileExists(adrFile,
        $"❌ ADR-XXX_Y_Z 违规：ADR 文档不存在\n\n" +
        $"修复建议：创建 docs/adr/ADR-XXX.md 文档\n\n" +
        $"参考：docs/adr/governance/ADR-008.md");
    
    // 3. 验证文件内容长度
    FileSystemTestHelper.AssertFileContentLength(adrFile, 500, 
        "ADR 文档内容过短，应包含详细说明");
    
    // 4. 验证文件包含必需内容
    FileSystemTestHelper.AssertFileContains(adrFile, "## 决策", 
        "ADR 文档必须包含'决策'章节");
    
    FileSystemTestHelper.AssertFileContains(adrFile, "## 后果", 
        "ADR 文档必须包含'后果'章节");
}
```

**场景：验证目录结构和文件分布**

```csharp
[Fact(DisplayName = "目录结构验证")]
public void Directory_Structure_Validation()
{
    // 1. 获取目录路径
    var casesDir = FileSystemTestHelper.GetAbsolutePath("docs/cases");
    
    // 2. 验证目录存在
    if (!Directory.Exists(casesDir))
    {
        Console.WriteLine("⚠️ 目录尚未创建，跳过测试");
        return;
    }
    
    // 3. 获取所有子目录
    var subdirs = FileSystemTestHelper.GetSubdirectories(casesDir);
    
    // 4. 验证每个子目录都有 README.md
    var missingReadmes = new List<string>();
    foreach (var subdir in subdirs)
    {
        var readmePath = Path.Combine(subdir, "README.md");
        if (!File.Exists(readmePath))
        {
            missingReadmes.Add(Path.GetFileName(subdir));
        }
    }
    
    missingReadmes.Should().BeEmpty(
        $"以下子目录缺少 README.md：{string.Join(", ", missingReadmes)}");
}
```

**场景：遍历文件并验证内容**

```csharp
[Fact(DisplayName = "文档内容合规性检查")]
public void Document_Content_Compliance()
{
    var docsPath = FileSystemTestHelper.GetAbsolutePath("docs");
    
    // 获取所有 markdown 文件
    var mdFiles = FileSystemTestHelper.GetFilesInDirectory(
        docsPath, 
        "*.md", 
        SearchOption.AllDirectories
    );
    
    var violations = new List<string>();
    
    foreach (var file in mdFiles)
    {
        var content = FileSystemTestHelper.ReadFileContent(file);
        var relativePath = FileSystemTestHelper.GetRelativePath(file);
        
        // 检查是否包含违规内容
        if (content.Contains("禁用词"))
        {
            violations.Add($"{relativePath} - 包含禁用词");
        }
    }
    
    violations.Should().BeEmpty("发现文档包含禁用词");
}
```

## 其他辅助工具

### TestEnvironment

提供仓库路径常量，避免重复查找：

```csharp
var repoRoot = TestEnvironment.RepositoryRoot;
var adrPath = TestEnvironment.AdrPath;
var modulesPath = TestEnvironment.ModulesPath;
```

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
```

## 最佳实践

1. **优先使用辅助方法**：避免直接使用 `File.Exists()`, `File.ReadAllText()` 等原生方法
2. **使用相对路径**：通过 `GetAbsolutePath()` 转换，提高可移植性
3. **提供详细错误信息**：使用 `AssertionMessageBuilder` 构建标准化错误消息
4. **适当处理空目录**：使用辅助方法返回的空集合，避免 null 检查

## 迁移指南

### 迁移前

```csharp
var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
var file = Path.Combine(repoRoot, "docs/adr/ADR-XXX.md");
File.Exists(file).Should().BeTrue("文件不存在");
var content = File.ReadAllText(file);
content.Should().Contain("决策");
```

### 迁移后

```csharp
var file = FileSystemTestHelper.GetAbsolutePath("docs/adr/ADR-XXX.md");
FileSystemTestHelper.AssertFileExists(file, "文件不存在");
FileSystemTestHelper.AssertFileContains(file, "决策", "应包含决策章节");
```

## 维护说明

如需添加新的文件系统操作辅助方法，请：

1. 在 `FileSystemTestHelper` 类中添加静态方法
2. 确保方法具有清晰的 XML 注释
3. 在本 README 中添加使用示例
4. 在至少 2-3 个测试类中验证新方法的可用性
