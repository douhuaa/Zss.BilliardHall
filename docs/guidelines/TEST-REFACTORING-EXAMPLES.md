# 测试代码重构示例

> **文档版本**: 1.0  
> **最后更新**: 2026-02-05  
> **文档定位**: 指导文档，提供测试代码重构的实践示例

## 文档目的

本文档提供测试代码重构的实际示例，展示如何使用新增的常量和辅助方法来简化测试编写，提高代码质量和可维护性。

---

## 重构示例 1：使用 ADR 文档路径常量

### 重构前（❌ 不推荐）

```csharp
[Fact(DisplayName = "ADR-960_1_2: Onboarding 不得定义新架构约束")]
public void ADR_960_1_2_Onboarding_Must_Not_Define_New_Constraints()
{
    var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
    
    // 硬编码的文档路径
    var adr960Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-960-onboarding-documentation-governance.md");
    
    File.Exists(adr960Path).Should().BeTrue(
        $"❌ ADR-960_1_2 违规：ADR-960 文档不存在\n\n" +
        $"修复建议：确保 ADR-960 存在以定义 Onboarding 文档规范\n\n" +
        $"参考：docs/adr/governance/ADR-960-onboarding-documentation-governance.md §1.2");
}
```

**问题**：
- 文档路径硬编码，难以维护
- 路径在多个测试中重复
- 断言消息手工拼接，容易出错

### 重构后（✅ 推荐）

```csharp
[Fact(DisplayName = "ADR-960_1_2: Onboarding 不得定义新架构约束")]
public void ADR_960_1_2_Onboarding_Must_Not_Define_New_Constraints()
{
    // 使用预定义的常量
    var adr960Path = FileSystemTestHelper.GetAbsolutePath(TestConstants.Adr960Path);
    
    // 使用 AssertionMessageBuilder 构建标准化的错误消息
    var fileNotFoundMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
        ruleId: "ADR-960_1_2",
        filePath: adr960Path,
        fileDescription: "ADR-960 文档",
        remediationSteps: new[]
        {
            "确保 ADR-960 存在以定义 Onboarding 文档规范",
            "在 ADR-960 中明确 Onboarding 的非裁决性定位"
        },
        adrReference: TestConstants.Adr960Path);
    
    File.Exists(adr960Path).Should().BeTrue(fileNotFoundMessage);
}
```

**优势**：
- ✅ 使用 `TestConstants.Adr960Path` 常量，路径集中管理
- ✅ 使用 `AssertionMessageBuilder` 构建标准化错误消息
- ✅ 代码简洁清晰，易于维护

---

## 重构示例 2：使用 GetAdrFiles 辅助方法

### 重构前（❌ 不推荐）

```csharp
[Fact(DisplayName = "ADR-946_1_1: ADR 文件必须有且仅有一个 # 标题")]
public void ADR_946_1_1_ADR_Must_Have_Exactly_One_H1_Title()
{
    var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
    var adrPath = Path.Combine(repoRoot, "docs/adr");
    
    if (!Directory.Exists(adrPath))
    {
        throw new DirectoryNotFoundException($"ADR 目录不存在: {adrPath}");
    }
    
    // 手动过滤文件
    var adrFiles = Directory.GetFiles(adrPath, "*.md", SearchOption.AllDirectories)
        .Where(f => !f.Contains("README", StringComparison.OrdinalIgnoreCase))
        .Where(f => !f.Contains("TIMELINE", StringComparison.OrdinalIgnoreCase))
        .Where(f => !f.Contains("CHECKLIST", StringComparison.OrdinalIgnoreCase))
        .Where(f => Path.GetFileName(f).StartsWith("ADR-", StringComparison.OrdinalIgnoreCase))
        .ToList();
    
    // 验证逻辑...
}
```

**问题**：
- 文件过滤逻辑重复出现在多个测试中
- 代码冗长，降低可读性
- 难以统一修改过滤规则

### 重构后（✅ 推荐）

```csharp
[Fact(DisplayName = "ADR-946_1_1: ADR 文件必须有且仅有一个 # 标题")]
public void ADR_946_1_1_ADR_Must_Have_Exactly_One_H1_Title()
{
    // 使用辅助方法获取所有 ADR 文件
    var adrFiles = FileSystemTestHelper.GetAdrFiles();
    
    var violations = new List<string>();
    
    foreach (var adrFile in adrFiles)
    {
        // 验证逻辑...
    }
    
    // 使用 AssertionMessageBuilder 构建格式化的错误消息
    var message = AssertionMessageBuilder.BuildFormatViolationMessage(
        ruleId: "ADR-946_1_1",
        summary: "以下 ADR 文件违反标题级别即语义级别规则",
        violations: violations,
        remediationSteps: new[]
        {
            "确保每个 ADR 文件有且仅有一个 # 级别标题（文档标题）",
            "所有语义块（如 Decision、Relationships）使用 ## 级别",
            "检查是否有误将语义块标题设置为 # 级别"
        },
        adrReference: TestConstants.Adr946Path);
    
    violations.Should().BeEmpty(message);
}
```

**优势**：
- ✅ 使用 `FileSystemTestHelper.GetAdrFiles()` 简化文件获取
- ✅ 代码更简洁，可读性更高
- ✅ 过滤规则统一管理，易于维护

---

## 重构示例 3：使用常量替代硬编码的关键词列表

### 重构前（❌ 不推荐）

```csharp
public sealed class ADR_960_1_Architecture_Tests
{
    // 在每个测试类中重复定义
    private static readonly string[] DecisionKeywords = new[]
    {
        "必须", "禁止", "不得", "强制", "不允许"
    };
    
    [Fact(DisplayName = "ADR-960_1_1: Onboarding 文档不得包含裁决性语言")]
    public void ADR_960_1_1_Onboarding_Must_Not_Contain_Decision_Language()
    {
        // 使用本地定义的关键词
        foreach (var keyword in DecisionKeywords)
        {
            // 检查逻辑...
        }
    }
}
```

**问题**：
- 关键词列表在多个测试类中重复定义
- 修改关键词需要同步更新多个位置
- 不同测试可能使用不一致的关键词列表

### 重构后（✅ 推荐）

```csharp
public sealed class ADR_960_1_Architecture_Tests
{
    [Fact(DisplayName = "ADR-960_1_1: Onboarding 文档不得包含裁决性语言")]
    public void ADR_960_1_1_Onboarding_Must_Not_Contain_Decision_Language()
    {
        var docsPath = FileSystemTestHelper.GetAbsolutePath("docs");
        
        // 使用统一的常量
        foreach (var keyword in TestConstants.DecisionKeywords)
        {
            // 检查逻辑...
        }
    }
}
```

**优势**：
- ✅ 使用 `TestConstants.DecisionKeywords` 统一管理
- ✅ 修改一处，全局生效
- ✅ 确保所有测试使用一致的关键词列表

---

## 重构示例 4：使用 AssertionMessageBuilder 构建复杂的错误消息

### 重构前（❌ 不推荐）

```csharp
[Fact]
public void ADR_960_1_3_Onboarding_Responsibilities_Must_Be_Defined()
{
    var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
    var adr960Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-960-onboarding-documentation-governance.md");
    
    File.Exists(adr960Path).Should().BeTrue(
        $"❌ ADR-960_1_3 违规：ADR-960 文档不存在");
    
    var content = File.ReadAllText(adr960Path);
    
    var hasResponsibilityDefinition = content.Contains("唯一合法职责", StringComparison.OrdinalIgnoreCase) ||
                                     content.Contains("告诉你", StringComparison.OrdinalIgnoreCase);
    
    hasResponsibilityDefinition.Should().BeTrue(
        $"❌ ADR-960_1_3 违规：ADR-960 必须明确定义 Onboarding 的唯一合法职责\n\n" +
        $"参考：docs/adr/governance/ADR-960-onboarding-documentation-governance.md §1.3");
}
```

**问题**：
- 错误消息格式不一致（第一个缺少完整信息）
- 缺少当前状态描述和修复建议
- 消息格式手工拼接，容易出错

### 重构后（✅ 推荐）

```csharp
[Fact(DisplayName = "ADR-960_1_3: Onboarding 唯一职责必须明确定义")]
public void ADR_960_1_3_Onboarding_Responsibilities_Must_Be_Defined()
{
    var adr960Path = FileSystemTestHelper.GetAbsolutePath(TestConstants.Adr960Path);
    
    // 使用专门的模板方法构建文件不存在的错误消息
    var fileNotFoundMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
        ruleId: "ADR-960_1_3",
        filePath: adr960Path,
        fileDescription: "ADR-960 文档",
        remediationSteps: new[]
        {
            "创建 ADR-960 文档",
            "定义 Onboarding 的唯一合法职责"
        },
        adrReference: TestConstants.Adr960Path);
    
    File.Exists(adr960Path).Should().BeTrue(fileNotFoundMessage);
    
    var content = File.ReadAllText(adr960Path);
    
    var hasResponsibilityDefinition = content.Contains("唯一合法职责", StringComparison.OrdinalIgnoreCase) ||
                                     content.Contains("告诉你", StringComparison.OrdinalIgnoreCase);
    
    // 使用标准模板构建完整的错误消息
    var message = AssertionMessageBuilder.Build(
        ruleId: "ADR-960_1_3",
        summary: "ADR-960 必须明确定义 Onboarding 的唯一合法职责",
        currentState: "文档中未找到职责定义（应包含'唯一合法职责'或'告诉你'等关键词）",
        remediationSteps: new[]
        {
            "在 ADR-960 中添加 Onboarding 的职责定义章节",
            "明确说明 Onboarding 的唯一合法职责是什么",
            "确保职责定义清晰、具体、可验证"
        },
        adrReference: TestConstants.Adr960Path,
        includeClauseReference: true);
    
    hasResponsibilityDefinition.Should().BeTrue(message);
}
```

**优势**：
- ✅ 使用 `BuildFileNotFoundMessage` 和 `Build` 模板方法
- ✅ 错误消息格式统一、完整
- ✅ 包含必需的字段：RuleId、当前状态、修复建议、参考文档
- ✅ 代码清晰，易于理解和维护

---

## 重构示例 5：使用 GetAgentFiles 辅助方法

### 重构前（❌ 不推荐）

```csharp
[Fact]
public void ADR_007_1_1_Agent_Positioning_Must_Be_Tool()
{
    var repoRoot = TestEnvironment.RepositoryRoot;
    var agentPath = Path.Combine(repoRoot, ".github/agents");
    
    if (!Directory.Exists(agentPath)) return;
    
    // 手动过滤 Agent 文件
    var systemAgents = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "expert-dotnet-software-engineer.agent.md",
        "README.md"
    };
    
    var agentFiles = Directory.GetFiles(agentPath, "*.agent.md", SearchOption.AllDirectories)
        .Where(f => !systemAgents.Contains(Path.GetFileName(f)))
        .ToArray();
    
    // 验证逻辑...
}
```

**问题**：
- Agent 文件过滤逻辑重复
- 系统 Agent 列表硬编码
- 代码冗长

### 重构后（✅ 推荐）

```csharp
[Fact(DisplayName = "ADR-007_1_1: Agent 定位必须为工具")]
public void ADR_007_1_1_Agent_Positioning_Must_Be_Tool()
{
    // 使用辅助方法获取 Agent 文件（自动排除系统 Agent）
    var agentFiles = FileSystemTestHelper.GetAgentFiles(
        includeSystemAgents: false,
        excludeGuardian: false);
    
    if (!agentFiles.Any()) return;
    
    var violations = new List<string>();
    
    // 验证逻辑...
    
    var message = AssertionMessageBuilder.BuildWithViolations(
        ruleId: "ADR-007_1_1",
        summary: "以下 Agent 文件违反了定位规则",
        failingTypes: violations,
        remediationSteps: new[]
        {
            "Agent 应定位为工具，而非决策者",
            "移除所有声称拥有裁决权的表述",
            "确保 Agent 配置明确引用 ADR 作为权威来源"
        },
        adrReference: TestConstants.Adr007Path);
    
    violations.Should().BeEmpty(message);
}
```

**优势**：
- ✅ 使用 `FileSystemTestHelper.GetAgentFiles()` 简化文件获取
- ✅ 支持灵活的过滤选项（includeSystemAgents, excludeGuardian）
- ✅ 代码更简洁，可读性更高

---

## 可用的常量和辅助方法速查

### TestConstants 常量

#### ADR 目录路径
```csharp
TestConstants.AdrDocsPath              // "docs/adr"
TestConstants.AdrConstitutionalPath    // "docs/adr/constitutional"
TestConstants.AdrGovernancePath        // "docs/adr/governance"
TestConstants.AdrTechnicalPath         // "docs/adr/technical"
TestConstants.AdrStructurePath         // "docs/adr/structure"
TestConstants.CasesPath                // "docs/cases"
TestConstants.AgentFilesPath           // ".github/agents"
```

#### 常用 ADR 文档路径
```csharp
TestConstants.Adr007Path    // ADR-007：Agent 行为与权限宪法
TestConstants.Adr008Path    // ADR-008：文档治理宪法
TestConstants.Adr946Path    // ADR-946：ADR 标题级别语义约束
TestConstants.Adr951Path    // ADR-951：案例库管理
TestConstants.Adr960Path    // ADR-960：Onboarding 文档治理
TestConstants.Adr965Path    // ADR-965：Onboarding 互动式学习路径
TestConstants.Adr004Path    // ADR-004：中央包管理 (CPM) 规范
```

#### 其他常量
```csharp
TestConstants.DecisionKeywords       // 裁决性关键词列表
TestConstants.KeySemanticHeadings    // 关键语义块标题列表
```

### FileSystemTestHelper 辅助方法

#### 文件和目录操作
```csharp
FileSystemTestHelper.GetAbsolutePath(relativePath)           // 获取绝对路径
FileSystemTestHelper.GetRelativePath(absolutePath)           // 获取相对路径
FileSystemTestHelper.AssertFileExists(filePath, message)     // 断言文件存在
FileSystemTestHelper.AssertDirectoryExists(dirPath, message) // 断言目录存在
FileSystemTestHelper.ReadFileContent(filePath)               // 读取文件内容
```

#### 文件遍历
```csharp
FileSystemTestHelper.GetAdrFiles(subfolder, excludeReadme, excludeTimeline, excludeChecklist)
FileSystemTestHelper.GetAgentFiles(includeSystemAgents, excludeGuardian)
FileSystemTestHelper.GetFilesInDirectory(path, pattern, option)
FileSystemTestHelper.GetSubdirectories(directoryPath)
```

#### 内容检查
```csharp
FileSystemTestHelper.FileContentMatches(filePath, pattern)      // 检查文件内容是否匹配正则
FileSystemTestHelper.GetMatchingLines(filePath, pattern)        // 获取匹配的行
FileSystemTestHelper.CountPatternOccurrences(filePath, pattern) // 统计模式出现次数
FileSystemTestHelper.AssertFileContains(filePath, content, msg) // 断言文件包含内容
```

### AssertionMessageBuilder 模板方法

#### 基础模板
```csharp
AssertionMessageBuilder.Build(ruleId, summary, currentState, remediationSteps, adrReference)
AssertionMessageBuilder.BuildSimple(ruleId, summary, currentState, remediation, adrReference)
```

#### 专用模板
```csharp
AssertionMessageBuilder.BuildFileNotFoundMessage(ruleId, filePath, fileDescription, remediationSteps, adrReference)
AssertionMessageBuilder.BuildDirectoryNotFoundMessage(ruleId, directoryPath, directoryDescription, remediationSteps, adrReference)
AssertionMessageBuilder.BuildContentMissingMessage(ruleId, filePath, missingContent, remediationSteps, adrReference)
AssertionMessageBuilder.BuildFormatViolationMessage(ruleId, summary, violations, remediationSteps, adrReference)
AssertionMessageBuilder.BuildWithViolations(ruleId, summary, failingTypes, remediationSteps, adrReference)
AssertionMessageBuilder.BuildFromArchTestResult(ruleId, summary, failingTypeNames, remediationSteps, adrReference)
```

---

## 重构检查清单

在重构测试代码时，请检查以下各项：

### 常量使用
- [ ] 使用 `TestConstants` 中的 ADR 文档路径常量
- [ ] 使用 `TestConstants.DecisionKeywords` 替代硬编码的关键词列表
- [ ] 使用 `TestConstants.KeySemanticHeadings` 替代硬编码的语义块标题
- [ ] 删除测试类中的重复常量定义

### 辅助方法使用
- [ ] 使用 `FileSystemTestHelper.GetAbsolutePath()` 替代 `Path.Combine(repoRoot, ...)`
- [ ] 使用 `FileSystemTestHelper.GetAdrFiles()` 替代手动过滤 ADR 文件
- [ ] 使用 `FileSystemTestHelper.GetAgentFiles()` 替代手动过滤 Agent 文件
- [ ] 使用 `FileSystemTestHelper.AssertFileExists()` 替代 `File.Exists().Should().BeTrue()`
- [ ] 使用 `FileSystemTestHelper.ReadFileContent()` 替代 `File.ReadAllText()`

### 断言消息
- [ ] 使用 `AssertionMessageBuilder` 模板方法构建错误消息
- [ ] 确保所有错误消息包含必需字段：RuleId、当前状态、修复建议、参考文档
- [ ] 使用合适的模板方法：BuildFileNotFoundMessage、BuildContentMissingMessage 等
- [ ] 删除手工拼接的错误消息字符串

### 代码质量
- [ ] 删除不再需要的本地变量（如 `repoRoot`）
- [ ] 删除重复的代码逻辑
- [ ] 确保代码简洁、清晰、易于理解
- [ ] 添加必要的代码注释

---

## 重构示例 6：使用 FileContainsAnyKeyword 和 GetMissingKeywords

### 重构前（❌ 不推荐）

```csharp
[Fact]
public void ADR_007_2_1_Agent_Responses_Must_Include_Three_State_Indicators()
{
    var agentFiles = GetAgentFiles();
    if (agentFiles.Length == 0) return;

    var violations = new List<string>();

    foreach (var file in agentFiles)
    {
        var content = File.ReadAllText(file);
        var fileName = Path.GetFileName(file);

        var missingStates = new List<string>();

        // 检查是否提及三态输出
        if (!content.Contains("✅", StringComparison.OrdinalIgnoreCase) &&
            !content.Contains("Allowed", StringComparison.OrdinalIgnoreCase))
        {
            missingStates.Add("✅ Allowed");
        }

        if (!content.Contains("⚠️", StringComparison.OrdinalIgnoreCase) &&
            !content.Contains("Blocked", StringComparison.OrdinalIgnoreCase))
        {
            missingStates.Add("⚠️ Blocked");
        }

        if (!content.Contains("❓", StringComparison.OrdinalIgnoreCase) &&
            !content.Contains("Uncertain", StringComparison.OrdinalIgnoreCase))
        {
            missingStates.Add("❓ Uncertain");
        }

        if (missingStates.Count >= 2)
        {
            violations.Add($"  • {fileName} 缺少三态标识: {string.Join(", ", missingStates)}");
        }
    }
    
    // 手工拼接的错误消息...
}
```

**问题**：
- 重复的 Contains 检查逻辑
- 硬编码的三态标识
- 冗长的代码

### 重构后（✅ 推荐）

```csharp
[Fact(DisplayName = "ADR-007_2_1: Agent 响应必须包含三态标识")]
public void ADR_007_2_1_Agent_Responses_Must_Include_Three_State_Indicators()
{
    var agentFiles = FileSystemTestHelper.GetAgentFiles(
        includeSystemAgents: false,
        excludeGuardian: false);

    if (!agentFiles.Any()) return;

    var violations = new List<string>();

    foreach (var file in agentFiles)
    {
        var fileName = Path.GetFileName(file);
        var missingStates = new List<string>();

        // 使用常量检查三态标识
        foreach (var indicator in TestConstants.ThreeStateIndicators)
        {
            var hasIndicator = FileSystemTestHelper.FileContainsAnyKeyword(
                file,
                new[] { indicator, indicator.Split(' ')[1] }, // 检查完整形式和简写形式
                ignoreCase: true);

            if (!hasIndicator)
            {
                missingStates.Add(indicator);
            }
        }

        if (missingStates.Count >= 2)
        {
            violations.Add($"{fileName} 缺少三态标识: {string.Join(", ", missingStates)}");
        }
    }

    var message = AssertionMessageBuilder.BuildWithViolations(
        ruleId: "ADR-007_2_1",
        summary: "以下 Agent 文件未实现三态输出规范",
        failingTypes: violations,
        remediationSteps: new[]
        {
            "确保 Agent 响应明确标识 ✅ Allowed、⚠️ Blocked 或 ❓ Uncertain",
            "在 Agent 配置中定义三态输出规范",
            "每种判定结果都应使用相应的标识"
        },
        adrReference: TestConstants.Adr007Path);

    violations.Should().BeEmpty(message);
}
```

**优势**：
- ✅ 使用 TestConstants.ThreeStateIndicators 常量
- ✅ 使用 FileContainsAnyKeyword 简化关键词检查
- ✅ 代码更简洁清晰
- ✅ 使用 AssertionMessageBuilder 构建标准错误消息

---

## 重构示例 7：使用 FileContainsTable 检测 Markdown 表格

### 重构前（❌ 不推荐）

```csharp
[Fact]
public void ADR_960_2_1_Onboarding_Must_Follow_Content_Type_Restrictions()
{
    var repoRoot = TestEnvironment.RepositoryRoot;
    var adr960Path = Path.Combine(repoRoot, "docs/adr/governance/ADR-960-onboarding-documentation-governance.md");

    File.Exists(adr960Path).Should().BeTrue("ADR-960 文档不存在");

    var content = File.ReadAllText(adr960Path);

    // 手工检查表格
    var hasContentTypeTable = content.Contains("| 内容类型", StringComparison.OrdinalIgnoreCase) &&
                             content.Contains("是否允许出现在 Onboarding", StringComparison.OrdinalIgnoreCase);

    hasContentTypeTable.Should().BeTrue("必须定义内容类型限制表");
}
```

**问题**：
- 手工检查表格存在性，不够可靠
- 硬编码的文件路径
- 简单的断言消息

### 重构后（✅ 推荐）

```csharp
[Fact(DisplayName = "ADR-960_2_1: Onboarding 必须遵循内容类型限制")]
public void ADR_960_2_1_Onboarding_Must_Follow_Content_Type_Restrictions()
{
    var adr960Path = FileSystemTestHelper.GetAbsolutePath(TestConstants.Adr960Path);

    var fileNotFoundMessage = AssertionMessageBuilder.BuildFileNotFoundMessage(
        ruleId: "ADR-960_2_1",
        filePath: adr960Path,
        fileDescription: "ADR-960 文档",
        remediationSteps: new[]
        {
            "创建 ADR-960 文档",
            "在文档中定义 Onboarding 的内容类型限制表"
        },
        adrReference: TestConstants.Adr960Path);

    File.Exists(adr960Path).Should().BeTrue(fileNotFoundMessage);

    // 使用专用方法检测表格
    var hasContentTypeTable = FileSystemTestHelper.FileContainsTable(
        adr960Path, 
        "是否允许出现在 Onboarding");

    var tableMessage = AssertionMessageBuilder.Build(
        ruleId: "ADR-960_2_1",
        summary: "ADR-960 必须定义 Onboarding 的内容类型限制表",
        currentState: "文档中未找到内容类型限制表",
        remediationSteps: new[]
        {
            "在 ADR-960 中添加内容类型限制表格",
            "表格应包含'内容类型'和'是否允许出现在 Onboarding'列",
            "明确列出允许和禁止的内容类型"
        },
        adrReference: TestConstants.Adr960Path,
        includeClauseReference: true);

    hasContentTypeTable.Should().BeTrue(tableMessage);

    // 使用 GetMissingKeywords 检查必需的内容类型
    var missingContentTypes = FileSystemTestHelper.GetMissingKeywords(
        adr960Path,
        TestConstants.ProhibitedContentTypesInOnboarding,
        ignoreCase: true);

    missingContentTypes.Should().BeEmpty();
}
```

**优势**：
- ✅ 使用 FileContainsTable 可靠地检测 Markdown 表格
- ✅ 使用 GetMissingKeywords 检查缺失的内容类型
- ✅ 使用常量定义内容类型列表
- ✅ 完整的错误消息和修复建议

---

## 更新：新增辅助方法（第二版）

### FileSystemTestHelper 新增方法（v2.0）

```csharp
// 检查文件是否包含所有关键词
FileSystemTestHelper.FileContainsAllKeywords(filePath, keywords, ignoreCase)

// 检查文件是否包含任一关键词
FileSystemTestHelper.FileContainsAnyKeyword(filePath, keywords, ignoreCase)

// 获取文件中缺失的关键词列表
FileSystemTestHelper.GetMissingKeywords(filePath, requiredKeywords, ignoreCase)

// 检查文件是否包含 Markdown 表格
FileSystemTestHelper.FileContainsTable(filePath, headerPattern)
```

### TestConstants 新增常量（v2.0）

```csharp
// 三态输出标识
TestConstants.ThreeStateIndicators       // ["✅ Allowed", "⚠️ Blocked", "❓ Uncertain"]
TestConstants.ThreeStateShortForms       // ["Allowed", "Blocked", "Uncertain"]
TestConstants.ThreeStateEmojis          // ["✅", "⚠️", "❓"]

// 内容类型限制
TestConstants.ProhibitedContentTypesInOnboarding  // 禁止的内容类型
TestConstants.AllowedContentTypesInOnboarding     // 允许的内容类型

// Onboarding 核心问题
TestConstants.OnboardingCoreQuestions    // ["我是谁", "我先看什么", "我下一步去哪"]

// 更多 ADR 路径
TestConstants.Adr900Path    // ADR-900 架构测试元规则
TestConstants.Adr901Path    // ADR-901 架构测试反作弊机制
TestConstants.Adr902Path    // ADR-902 ADR 文档质量规范
TestConstants.Adr907Path    // ADR-907 ArchitectureTests 执法治理体系
TestConstants.Adr907APath   // ADR-907-A 对齐执行标准
```

---

## 重构检查清单（更新版）

在重构测试代码时，请检查以下各项：

### 常量使用
- [ ] 使用 `TestConstants` 中的 ADR 文档路径常量
- [ ] 使用 `TestConstants.DecisionKeywords` 替代硬编码的关键词列表
- [ ] 使用 `TestConstants.KeySemanticHeadings` 替代硬编码的语义块标题
- [ ] 使用 `TestConstants.ThreeStateIndicators` 等新增常量
- [ ] 删除测试类中的重复常量定义

### 辅助方法使用
- [ ] 使用 `FileSystemTestHelper.GetAbsolutePath()` 替代 `Path.Combine(repoRoot, ...)`
- [ ] 使用 `FileSystemTestHelper.GetAdrFiles()` 替代手动过滤 ADR 文件
- [ ] 使用 `FileSystemTestHelper.GetAgentFiles()` 替代手动过滤 Agent 文件
- [ ] 使用 `FileSystemTestHelper.AssertFileExists()` 替代 `File.Exists().Should().BeTrue()`
- [ ] 使用 `FileSystemTestHelper.ReadFileContent()` 替代 `File.ReadAllText()`
- [ ] 使用 `FileContainsAnyKeyword()` 简化关键词检查（新增）
- [ ] 使用 `GetMissingKeywords()` 检查缺失的关键词（新增）
- [ ] 使用 `FileContainsTable()` 检测 Markdown 表格（新增）

### 断言消息
- [ ] 使用 `AssertionMessageBuilder` 模板方法构建错误消息
- [ ] 确保所有错误消息包含必需字段：RuleId、当前状态、修复建议、参考文档
- [ ] 使用合适的模板方法：BuildFileNotFoundMessage、BuildContentMissingMessage 等
- [ ] 删除手工拼接的错误消息字符串

### 代码质量
- [ ] 删除不再需要的本地变量（如 `repoRoot`）
- [ ] 删除重复的代码逻辑
- [ ] 删除本地定义的辅助方法（如 GetAgentFiles）
- [ ] 确保代码简洁、清晰、易于理解
- [ ] 添加必要的代码注释

---

## 结语

本文档提供了测试代码重构的实际示例，展示了如何使用新增的常量和辅助方法来简化测试编写。

遵循这些示例将有助于：
- ✅ 减少代码重复，提高可维护性
- ✅ 统一测试代码风格和格式
- ✅ 简化测试编写，提高开发效率
- ✅ 确保测试质量和一致性

如有问题或建议，请通过 Issue 或 PR 提出。
