# ArchitectureTests.Specification 目录现状分析报告

**报告日期**: 2026-02-09  
**分析对象**: `douhuaa/Zss.BilliardHall` 仓库  
**分析目标**: ArchitectureTests.Specification 相关目录和文件

---

## 🎯 核心发现总结（Executive Summary）

### 关键发现

1. **ArchitectureTests.Specification 目录不存在**
   - 仓库中没有名为 `ArchitectureTests.Specification` 的目录
   - 没有使用 `.Specification` 命名空间的代码文件
   - 可能这是一个**计划中但尚未实现**的功能，或者是**历史遗留的引用**

2. **当前架构测试组织方式**
   - 所有架构测试位于：`/src/tests/ArchitectureTests/`
   - 主命名空间：`Zss.BilliardHall.Tests.ArchitectureTests`
   - 采用**三层分级架构**（自 2026-01-25）：
     - `Governance/` - 宪法层（治理原则）
     - `Enforcement/` - 执法层（硬约束）
     - `Heuristics/` - 启发层（风格建议）
     - `ADR/` - 传统层（每个 ADR 对应一个测试类）

3. **无 Adr907 相关内容**
   - 搜索整个仓库未发现 "907" 或 "0907" 相关文件
   - 现有 ADR 编号范围：0-8, 120-124, 201-360, 900-930
   - ADR 编号存在跳跃，907 不在已实现范围内

4. **构建状态良好**
   - ✅ 构建成功，无编译错误
   - ⚠️ 8 个警告（全部来自 `/src/tools/ArchitectureAnalyzers/` 项目）
   - ⚠️ 警告内容：RS1036, RS2008, CS8604（可空引用）

---

## 📂 目录结构完整清单

### 1. 主目录结构

```
/src/tests/ArchitectureTests/
  ├─ ADR/                    (26 个测试文件)
  ├─ Enforcement/            (4 个测试文件 + README)
  ├─ Governance/             (1 个测试文件 + README)
  ├─ Heuristics/             (1 个测试文件 + README)
  ├─ ArchitectureTests.csproj
  ├─ TestData.cs             (辅助数据提供器)
  └─ README.md
```

### 2. ADR 测试文件清单（26 个文件）

| 文件名 | 对应 ADR | 行数 | 主要测试内容 |
|--------|----------|------|--------------|
| `ADR_0000_Architecture_Tests.cs` | ADR-0000 | 313 | 架构测试元规则、一一映射 |
| `ADR_0001_Architecture_Tests.cs` | ADR-0001 | 265 | 模块隔离、垂直切片 |
| `ADR_0002_Architecture_Tests.cs` | ADR-0002 | 498 | Platform/Application/Host 边界 |
| `ADR_0003_Architecture_Tests.cs` | ADR-0003 | 336 | 命名空间规范 |
| `ADR_0004_Architecture_Tests.cs` | ADR-0004 | 432 | 中央包管理 (CPM) |
| `ADR_0005_Architecture_Tests.cs` | ADR-0005 | 539 | Handler 规则、CQRS |
| `ADR_0006_Architecture_Tests.cs` | ADR-0006 | 261 | 术语与编号规范 |
| `ADR_0007_Architecture_Tests.cs` | ADR-0007 | 323 | Agent 行为与权限 |
| `ADR_0008_Architecture_Tests.cs` | ADR-0008 | ~120 | 文档治理（已拆分到 Governance） |
| `ADR_0120_Architecture_Tests.cs` | ADR-0120 | 347 | 领域事件命名约定 |
| `ADR_0121_Architecture_Tests.cs` | ADR-0121 | 423 | 数据契约规范 |
| `ADR_0122_Architecture_Tests.cs` | ADR-0122 | ~180 | 查询对象规范 |
| `ADR_0123_Architecture_Tests.cs` | ADR-0123 | ~160 | 命令对象规范 |
| `ADR_0124_Architecture_Tests.cs` | ADR-0124 | ~150 | 事件对象规范 |
| `ADR_0201_Architecture_Tests.cs` | ADR-0201 | ~65 | Handler 生命周期 |
| `ADR_0210_Architecture_Tests.cs` | ADR-0210 | ~140 | Handler 事务边界 |
| `ADR_0220_Architecture_Tests.cs` | ADR-0220 | ~130 | Handler 异常处理 |
| `ADR_0240_Architecture_Tests.cs` | ADR-0240 | 295 | 异常体系规范 |
| `ADR_0301_Architecture_Tests.cs` | ADR-0301 | ~120 | 数据库访问规范 |
| `ADR_0340_Architecture_Tests.cs` | ADR-0340 | 274 | Repository 模式 |
| `ADR_0350_Architecture_Tests.cs` | ADR-0350 | ~110 | 实体与聚合根 |
| `ADR_0360_Architecture_Tests.cs` | ADR-0360 | ~100 | 值对象规范 |
| `ADR_0900_Architecture_Tests.cs` | ADR-0900 | 239 | ADR-Test-Prompts 三角映射 |
| `ADR_0930_Architecture_Tests.cs` | ADR-0930 | ~90 | 测试组织规范 |
| `ADR_910_Architecture_Tests.cs` | ADR-0910 | 472 | Copilot Instructions 规范 |
| `ADR_920_Architecture_Tests.cs` | ADR-0920 | 582 | Copilot Agents 规范 |

### 3. Enforcement 层测试（4 个文件）

| 文件名 | 行数 | 核心功能 |
|--------|------|----------|
| `AdrStructureTests.cs` | ~100 | ADR 文档必需章节验证 |
| `DocumentationDecisionLanguageTests.cs` | 145 | README 裁决语言检查 |
| `DocumentationAuthorityDeclarationTests.cs` | 142 | Instructions/Agents 权威声明 |
| `SkillsJudgmentLanguageTests.cs` | ~80 | Skills 判断性语言检查 |

### 4. Governance 层测试（1 个文件）

| 文件名 | 行数 | 核心功能 |
|--------|------|----------|
| `ADR_0008_Governance_Tests.cs` | ~180 | 治理边界定义验证 |

### 5. Heuristics 层测试（1 个文件）

| 文件名 | 行数 | 核心功能 |
|--------|------|----------|
| `DocumentationStyleHeuristicsTests.cs` | 210 | 文档风格建议（永不失败） |

### 6. 辅助文件（1 个文件）

| 文件名 | 行数 | 核心功能 |
|--------|------|----------|
| `TestData.cs` | 269 | `ModuleAssemblyData` 和 `HostAssemblyData` 提供器 |

---

## 🔍 文件详细分析

### TestData.cs - 数据提供器（269 行）

**主要类型**：
- `ModuleAssemblyData : IEnumerable<object[]>`
  - **静态属性**：`ModuleAssemblies` (List<Assembly>), `ModuleNames` (List<string>)
  - **核心方法**：`GetSolutionRoot()`, `GetModuleProjectFiles()`
  - **实现方式**：通过文件系统扫描 `/src/Modules/` 目录，加载模块 DLL

- `HostAssemblyData : IEnumerable<object[]>`
  - **静态属性**：`HostAssemblies` (List<Assembly>)
  - **核心方法**：类似 `ModuleAssemblyData`
  - **实现方式**：扫描 `/src/Host/` 目录，加载 Host DLL

**技术特征**：
- ✅ **真实实现**（无占位符）
- ⚠️ **反射使用**：大量使用 `Assembly.LoadFrom()`
- ⚠️ **文件系统访问**：`Directory.GetDirectories()`, `File.Exists()`
- ⚠️ **硬编码路径**：`"src/Modules"`, `"src/Host"`, `"Zss.BilliardHall.slnx"`
- ⚠️ **环境变量依赖**：`Configuration` 环境变量（Debug/Release）
- ⚠️ **TFM 硬编码**：`net10.0`, `net8.0`, `net7.0` 等

**潜在问题**：
1. **重复初始化风险**：静态构造函数在每个 AppDomain 加载时执行一次
2. **跨平台兼容性**：路径拼接可能存在 Windows/Linux 差异
3. **失败处理**：DLL 加载失败时仅使用 `Debug.WriteLine()`，可能导致测试无声失败

---

### ADR 测试类模式分析

#### 典型结构（以 ADR_0001_Architecture_Tests.cs 为例）

**类结构**：
```csharp
namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0001: 模块化单体与垂直切片架构决策（v4.0）
/// 验证模块隔离、垂直切片、契约使用等核心架构约束
/// 约束映射表...
/// </summary>
public sealed class ADR_0001_Architecture_Tests
{
    #region 1. 模块隔离约束 (ADR-0001.1, 0001.2, 0001.7)
    
    [Theory(DisplayName = "ADR-0001.1: 模块不应相互引用")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Reference_Other_Modules(Assembly moduleAssembly) { }
    
    // 更多测试方法...
    #endregion
    
    // 其他分组...
}
```

**共同特征**：
- ✅ 使用 `NetArchTest.Rules` 库进行架构验证
- ✅ 参数化测试：`[Theory]` + `[ClassData(typeof(ModuleAssemblyData))]`
- ✅ 清晰的失败消息：包含 ADR 编号、违规详情、修复建议、文档引用
- ✅ 分组注释：使用 `#region` 组织相关测试

**核心依赖**：
- `NetArchTest.Rules.Types` - 架构规则引擎
- `System.Reflection.Assembly` - 反射操作
- `System.Xml.XmlDocument` - 解析 .csproj 文件
- `System.IO.File/Directory` - 文件系统访问

---

### Enforcement 层测试模式

#### AdrStructureTests.cs（~100 行）

**测试方法**：
- `ADR_Documents_Must_Have_Required_Sections()`
  - **验证目标**：ADR 文档包含必需章节（状态、级别、决策）
  - **实现方式**：读取 `/docs/adr/` 目录下所有 `ADR-*.md` 文件，检查内容
  - **失败策略**：CI 阻断

**技术特征**：
- ⚠️ **文件系统访问**：`Directory.GetFiles()`, `File.ReadAllText()`
- ⚠️ **硬编码路径**：`"docs/adr"`, `"README"`, `"TEMPLATE"`
- ⚠️ **字符串匹配**：使用 `Contains()` 检查章节标题
- ✅ **性能优化**：`.Take(30)` 限制检查数量

**潜在问题**：
1. **文档格式依赖**：假设章节标题为中文或英文固定格式
2. **误报风险**：如果 ADR 使用变体标题（如"地位"而非"状态"），可能误报
3. **编码问题**：未显式指定文件编码，可能在 UTF-8 BOM 上出错

---

### Governance 层测试模式

#### ADR_0008_Governance_Tests.cs（~180 行）

**测试方法**（6 个）：
1. `ADR_0008_Document_Must_Exist()` - 验证 ADR-0008 文档存在
2. `ADR_0008_Must_Define_Three_Tier_Architecture()` - 验证三层架构定义
3. `ADR_0008_Must_Define_Authority_Boundary()` - 验证权威边界定义
4. `ADR_0008_Must_Define_Conflict_Resolution()` - 验证冲突解决机制
5. `ADR_0008_Prompts_File_Must_Exist()` - 验证 Prompts 文件存在
6. `ADR_0008_Prompts_Must_Reference_ADR()` - 验证 Prompts 引用 ADR

**技术特征**：
- ⚠️ **文件系统访问**：验证特定文档路径存在性
- ⚠️ **内容检查**：使用 `Contains()` 验证关键词
- ✅ **清晰的错误消息**：精确指出缺失的治理元素

---

### Heuristics 层测试模式

#### DocumentationStyleHeuristicsTests.cs（210 行）

**特殊性**：
- ✅ **永不失败**：所有测试使用 `[Fact(Skip = "Heuristic - 永不失败构建")]`
- ✅ **建议性质**：输出风格建议到控制台，但不阻断 CI

**测试方法**（3 个）：
1. `README_Files_Should_Have_Reasonable_Length()` - README 长度建议
2. `ADR_Documents_Should_Have_Examples()` - ADR 应包含示例
3. `Documentation_Should_Use_Consistent_Formatting()` - 格式一致性

---

## 🔬 反射使用情况

### 反射调用统计

- **总反射调用次数**：172 次
- **主要使用场景**：
  1. **程序集加载**：`Assembly.LoadFrom()`, `Assembly.GetName()`
  2. **类型检查**：`typeof()`, `Type.GetType()`, `GetTypes()`
  3. **成员访问**：`GetFields()`, `GetMethods()`, `GetProperties()`
  4. **属性读取**：`GetCustomAttributes()`, `IsDefined()`

### 高频反射使用位置

1. **TestData.cs**
   - `Assembly.LoadFrom()` - 动态加载模块 DLL
   - `Assembly.GetName()` - 获取程序集名称
   - 用途：测试参数化数据提供

2. **ADR_0201_Architecture_Tests.cs**
   - `GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)` - 检测 Handler 静态字段
   - 用途：验证生命周期规范

3. **ADR_0005_Architecture_Tests.cs**
   - `Types.InAssembly().GetTypes()` - 获取所有类型
   - `GetMethods()` - 检查 Handler 方法签名
   - 用途：验证 CQRS 规范

### 反射使用合理性评估

✅ **合理使用**：
- 架构测试本质上需要检查代码结构，反射是必要工具
- NetArchTest 库本身也基于反射实现

⚠️ **潜在风险**：
- **性能开销**：每次测试运行都要加载所有模块程序集
- **脆弱性**：依赖文件系统路径和 DLL 命名约定
- **调试困难**：反射调用失败时错误信息可能不清晰

💡 **改进建议**：
1. 考虑引入**缓存机制**：静态程序集列表可以序列化到临时文件
2. 考虑使用 **Roslyn Analyzers**：编译时分析，无需反射
3. 为反射调用添加**重试机制**和**详细日志**

---

## 🔗 外部依赖分析

### 文件系统访问（File/Directory I/O）

**统计**：约 60+ 处文件系统调用

**主要用途**：
1. **DLL 加载**（TestData.cs）
   - `Directory.GetDirectories()` - 扫描模块目录
   - `Directory.GetFiles()` - 查找 DLL 文件
   - `File.Exists()` - 验证文件存在

2. **文档验证**（Enforcement/Governance/Heuristics）
   - `Directory.GetFiles(adrDir, "ADR-*.md", SearchOption.AllDirectories)` - 扫描 ADR 文档
   - `File.ReadAllText()` - 读取文档内容

3. **项目文件检查**（ADR_0004）
   - `File.Load(csprojPath)` - 读取 .csproj XML
   - `File.Exists("Directory.Packages.props")` - 验证 CPM 配置

**风险评估**：
- ⚠️ **跨平台兼容性**：路径分隔符可能在 Windows/Linux 上不同
- ⚠️ **CI 环境依赖**：假设文件系统结构固定
- ⚠️ **并发安全性**：多个测试同时读取文件可能有竞争条件

### 网络访问

- ✅ **无网络依赖**：未发现 `HttpClient`, `SqlConnection`, `DbContext` 等外部连接

### 硬编码配置

**位置与内容**：

1. **TestData.cs**
   ```csharp
   var modulesDir = Path.Combine(root, "src", "Modules");
   var tfms = new[] { "net10.0", "net8.0", "net7.0", "net6.0", "net5.0" };
   var configuration = Environment.GetEnvironmentVariable("Configuration") ?? "Debug";
   ```

2. **AdrStructureTests.cs**
   ```csharp
   var adrDir = Path.Combine(repoRoot, "docs/adr");
   ```

3. **ADR_0001_Architecture_Tests.cs**
   ```csharp
   var allowedProjectNames = new HashSet<string> {
       "Zss.BilliardHall.Platform",
       "Zss.BilliardHall.BuildingBlocks",
   };
   ```

**问题**：
- ⚠️ **维护成本**：项目结构变更时需手动更新多处代码
- ⚠️ **可扩展性**：添加新模块或 TFM 需修改代码

**改进建议**：
- 将路径和配置提取到 `appsettings.json` 或 `TestConfiguration.cs`
- 使用约定优于配置（Convention over Configuration）

---

## ⚠️ 潜在编译问题

### 当前编译状态

✅ **构建成功**  
⚠️ **8 个警告**（全部来自 `/src/tools/ArchitectureAnalyzers/`）

### 警告详情

#### 1. RS1036（3 处）
```
A project containing analyzers or source generators should specify the property '<EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>'
```
**位置**：
- `EndpointBusinessLogicAnalyzer.cs`
- `StructuredExceptionAnalyzer.cs`
- `CrossModuleCallAnalyzer.cs`

**影响**：分析器项目配置不完整，可能影响 Roslyn 分析器的可靠性

#### 2. RS2008（3 处）
```
Enable analyzer release tracking for the analyzer project containing rule 'ADR0005_XX'
```
**位置**：同上三个分析器

**影响**：缺少发布跟踪文件，不影响功能但影响版本管理

#### 3. CS8604（2 处）
```
Possible null reference argument for parameter 'namespaceStr' in 'CrossModuleCallAnalyzer.ExtractModuleName(string namespaceStr)'
```
**位置**：`CrossModuleCallAnalyzer.cs` 第 68、69 行

**影响**：可能的空引用异常，建议添加空值检查

### ArchitectureTests 项目本身

- ✅ **无编译错误**
- ✅ **无编译警告**
- ✅ **所有依赖项正常解析**

### 潜在风险

1. **缺少类型定义**：无（所有类型都有定义）
2. **命名冲突**：无（命名空间清晰分离）
3. **方法签名不匹配**：无（xUnit 测试签名正确）
4. **依赖项缺失**：无（NuGet 包均正常引用）

---

## 📊 重构优先级与建议

### 🔴 高优先级（High Priority）

#### H1. 解决 ArchitectureAnalyzers 警告
**问题**：8 个警告可能影响 Roslyn 分析器稳定性  
**影响范围**：Roslyn 分析器功能  
**建议行动**：
1. 在 `ArchitectureAnalyzers.csproj` 添加 `<EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>`
2. 添加 `AnalyzerReleases.Unshipped.md` 和 `AnalyzerReleases.Shipped.md`
3. 修复 `CrossModuleCallAnalyzer.cs` 的空引用警告（添加 null 检查）

#### H2. 创建配置抽象层
**问题**：硬编码配置分散在多个文件中  
**影响范围**：可维护性、可扩展性  
**建议方案**：

**选项 A**：创建 `TestConfiguration.cs`
```csharp
public static class ArchitectureTestConfiguration
{
    public static string ModulesPath => "src/Modules";
    public static string HostPath => "src/Host";
    public static string AdrDocsPath => "docs/adr";
    public static string[] SupportedTfms => new[] { "net10.0", "net8.0", "net7.0" };
    public static HashSet<string> AllowedDependencies => new() { 
        "Zss.BilliardHall.Platform", 
        "Zss.BilliardHall.BuildingBlocks" 
    };
}
```

**选项 B**：使用 `appsettings.test.json`
```json
{
  "ArchitectureTests": {
    "ModulesPath": "src/Modules",
    "HostPath": "src/Host",
    "SupportedTfms": ["net10.0", "net8.0", "net7.0"],
    "AllowedDependencies": ["Zss.BilliardHall.Platform"]
  }
}
```

#### H3. 改进 TestData 错误处理
**问题**：DLL 加载失败仅记录日志，测试可能无声失败  
**影响范围**：测试可靠性  
**建议行动**：
```csharp
if (!ordered.Any())
{
    throw new InvalidOperationException(
        $"未找到模块输出 DLL: {moduleName}。\n" +
        $"路径: {moduleDir}\n" +
        $"请运行 `dotnet build` 或检查配置。\n" +
        $"环境变量 Configuration={configuration}");
}
```

---

### 🟡 中优先级（Medium Priority）

#### M1. 引入程序集缓存机制
**问题**：每次测试运行都要扫描和加载所有 DLL  
**影响范围**：测试性能  
**建议方案**：

```csharp
public static class AssemblyCache
{
    private static readonly string CacheFile = Path.Combine(
        Path.GetTempPath(), 
        "ArchitectureTests.AssemblyCache.json");
    
    public static List<Assembly> GetOrLoadModuleAssemblies()
    {
        if (IsCacheValid())
            return LoadFromCache();
        
        var assemblies = ScanAndLoadAssemblies();
        SaveToCache(assemblies);
        return assemblies;
    }
}
```

**性能预期**：首次运行后，后续测试启动时间减少 50-70%

#### M2. 拆分大型测试类（Partial Class）
**问题**：`ADR_920_Architecture_Tests.cs`（582 行）、`ADR_0005_Architecture_Tests.cs`（539 行）过长  
**影响范围**：可读性、可维护性  
**建议方案**：

```csharp
// ADR_0005_Architecture_Tests.Handler.cs
public sealed partial class ADR_0005_Architecture_Tests
{
    // Handler 相关测试
}

// ADR_0005_Architecture_Tests.Endpoint.cs
public sealed partial class ADR_0005_Architecture_Tests
{
    // Endpoint 相关测试
}

// ADR_0005_Architecture_Tests.Contract.cs
public sealed partial class ADR_0005_Architecture_Tests
{
    // Contract 相关测试
}
```

#### M3. 场景数据驱动化
**问题**：多个测试类重复扫描相同文档  
**影响范围**：测试效率  
**建议方案**：

创建 `DocumentationScanService.cs`：
```csharp
public class DocumentationScanService
{
    private static readonly Lazy<List<string>> _adrFiles = new(() => 
        Directory.GetFiles(GetAdrPath(), "ADR-*.md", SearchOption.AllDirectories)
            .ToList());
    
    public static IEnumerable<string> GetAllAdrFiles() => _adrFiles.Value;
}
```

---

### 🟢 低优先级（Low Priority）

#### L1. 提取共享测试基类
**问题**：重复的 `FindRepositoryRoot()` 实现  
**影响范围**：代码复用  
**建议方案**：

```csharp
public abstract class ArchitectureTestBase
{
    protected static string FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) || 
                Directory.Exists(Path.Combine(currentDir, "docs", "adr")))
                return currentDir;
            
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        throw new DirectoryNotFoundException("未找到仓库根目录");
    }
}
```

#### L2. 考虑引入测试数据生成器
**问题**：未来可能需要大量测试数据  
**影响范围**：测试覆盖率  
**建议方案**：

使用 **Source Generator** 在编译时生成测试数据：
```csharp
[Generator]
public class ArchitectureTestDataGenerator : ISourceGenerator
{
    public void Execute(GeneratorExecutionContext context)
    {
        // 扫描项目文件，生成测试数据类
    }
}
```

**优势**：
- 编译时生成，无运行时开销
- 类型安全
- 易于调试

#### L3. 文档格式标准化
**问题**：ADR 文档标题使用多种变体（"状态" vs "地位"）  
**影响范围**：文档一致性  
**建议方案**：

在 `AdrStructureTests.cs` 中使用正则表达式：
```csharp
var statusPattern = @"\*\*(?:状态|Status|地位)\*\*\s*[:：]";
if (!Regex.IsMatch(content, statusPattern, RegexOptions.Multiline))
{
    missingSections.Add("状态");
}
```

---

## 📋 可执行行动清单（Actionable Checklist）

### 第一阶段：紧急修复（1-2 天）

- [ ] **H1.1** 修复 ArchitectureAnalyzers 项目的 RS1036 警告
  - [ ] 在 `ArchitectureAnalyzers.csproj` 添加 `<EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>`
  - [ ] 测试构建确认警告消失

- [ ] **H1.2** 修复 ArchitectureAnalyzers 的 CS8604 空引用警告
  - [ ] 在 `CrossModuleCallAnalyzer.cs` 第 68、69 行添加 null 检查
  - [ ] 运行单元测试确认无影响

- [ ] **H1.3** 添加 AnalyzerReleases 文件
  - [ ] 创建 `AnalyzerReleases.Unshipped.md`
  - [ ] 创建 `AnalyzerReleases.Shipped.md`

- [ ] **H3** 改进 TestData.cs 错误处理
  - [ ] 将 `Debug.WriteLine()` 改为 `throw InvalidOperationException()`
  - [ ] 运行测试确认更清晰的错误消息

### 第二阶段：架构改进（3-5 天）

- [ ] **H2.1** 创建配置抽象层
  - [ ] 决定使用 `TestConfiguration.cs` 还是 `appsettings.test.json`
  - [ ] 实现配置类
  - [ ] 重构 TestData.cs 使用新配置
  - [ ] 重构所有硬编码路径

- [ ] **M1** 引入程序集缓存机制
  - [ ] 实现 `AssemblyCache.cs`
  - [ ] 修改 TestData.cs 使用缓存
  - [ ] 测试性能改进

- [ ] **M2** 拆分大型测试类
  - [ ] 拆分 `ADR_920_Architecture_Tests.cs`（582 行）
  - [ ] 拆分 `ADR_0005_Architecture_Tests.cs`（539 行）
  - [ ] 拆分 `ADR_0002_Architecture_Tests.cs`（498 行）

### 第三阶段：质量提升（持续）

- [ ] **M3** 场景数据驱动化
  - [ ] 创建 `DocumentationScanService.cs`
  - [ ] 重构所有文档扫描测试使用新服务

- [ ] **L1** 提取共享测试基类
  - [ ] 创建 `ArchitectureTestBase.cs`
  - [ ] 重构测试类继承基类

- [ ] **L2** 考虑测试数据生成器
  - [ ] 评估 Source Generator 可行性
  - [ ] 实现 POC（如果可行）

- [ ] **L3** 文档格式标准化
  - [ ] 更新 `AdrStructureTests.cs` 使用正则表达式
  - [ ] 标准化所有 ADR 文档标题

---

## ❓ 需人工确认的关键问题

### Q1. ArchitectureTests.Specification 的预期用途
**问题**：仓库中完全没有 `ArchitectureTests.Specification` 目录或引用，这是否意味着：
- [ ] **选项 A**：这是一个计划中但尚未实现的功能？
- [ ] **选项 B**：这是历史遗留的名称，现已用三层架构（Governance/Enforcement/Heuristics）替代？
- [ ] **选项 C**：这是一个误解，实际想查询的是其他内容？

**影响**：确定后续是否需要创建此目录/命名空间

---

### Q2. 生产 RuleSet 的实际类型名
**问题**：项目中使用 `NetArchTest.Rules` 库，但没有自定义 `RuleSet` 类。是否需要创建？
- [ ] **选项 A**：继续使用 `NetArchTest.Rules.Types` 的内置 API
- [ ] **选项 B**：创建自定义 `RuleSet` 基类封装常用规则
- [ ] **选项 C**：引入其他架构测试框架（如 ArchUnitNET）

**示例自定义 RuleSet**：
```csharp
public abstract class ArchitectureRuleSet
{
    public abstract string RuleId { get; }
    public abstract string Description { get; }
    public abstract ValidationResult Validate(Assembly assembly);
}
```

**建议**：如果当前 `NetArchTest` 满足需求，选项 A 最简单

---

### Q3. 测试中使用反射退化策略的接受度
**问题**：TestData.cs 使用大量反射和文件系统扫描，性能开销较大。是否接受以下"退化策略"：
- [ ] **策略 A**：保持现状，接受反射开销（简单但慢）
- [ ] **策略 B**：引入缓存，首次慢后续快（推荐）
- [ ] **策略 C**：迁移到 Roslyn Analyzers，完全编译时验证（最佳但工作量大）

**性能对比**（估算）：
| 策略 | 首次运行 | 后续运行 | 开发体验 | CI 时间 |
|------|----------|----------|----------|---------|
| A (现状) | ~3-5s | ~3-5s | 简单 | ~3-5s |
| B (缓存) | ~3-5s | ~0.5-1s | 稍复杂 | ~0.5-1s |
| C (Roslyn) | ~0.1s | ~0.1s | 复杂 | ~0.1s |

**建议**：先实施策略 B（中期），未来考虑策略 C（长期）

---

### Q4. 场景数据文件格式偏好（JSON vs YAML）
**问题**：如果要将测试场景数据外部化，偏好哪种格式？
- [ ] **JSON** - 标准、类型安全、C# 支持好
- [ ] **YAML** - 可读性强、支持注释、配置友好

**示例 JSON 场景数据**：
```json
{
  "moduleIsolationTests": {
    "allowedDependencies": [
      "Zss.BilliardHall.Platform",
      "Zss.BilliardHall.BuildingBlocks"
    ],
    "forbiddenPatterns": [
      "Zss.BilliardHall.Modules.*"
    ]
  }
}
```

**示例 YAML 场景数据**：
```yaml
moduleIsolationTests:
  allowedDependencies:
    - Zss.BilliardHall.Platform
    - Zss.BilliardHall.BuildingBlocks
  forbiddenPatterns:
    - Zss.BilliardHall.Modules.*
```

**建议**：JSON（更好的 C# 工具支持）

---

### Q5. ADR 编号跳跃的原因
**问题**：现有 ADR 编号存在跳跃（0-8, 120-124, 201-360, 900-930），缺失编号段是否：
- [ ] **保留用于未来扩展**？
- [ ] **已废弃/删除**？
- [ ] **按主题分组（如 1xx = 事件, 2xx = 生命周期, 3xx = 数据）**？

**影响**：确定 ADR 编号规范，指导未来 ADR 创建

---

### Q6. Adr907 的实际含义
**问题**：问题陈述中提到 "Adr907"，但仓库中不存在此编号的 ADR。可能是：
- [ ] **拼写错误**（如 ADR-0907）？
- [ ] **已计划但未创建**的 ADR？
- [ ] **外部文档引用**？

**建议行动**：明确 Adr907 的预期内容，如果需要则创建此 ADR

---

### Q7. 三层架构的演进方向
**问题**：当前三层架构（Governance/Enforcement/Heuristics）是否还需要进一步细化？
- [ ] **选项 A**：保持现状（3 层足够）
- [ ] **选项 B**：增加第 4 层（如 Performance 性能测试）
- [ ] **选项 C**：将 ADR 层融入三层架构（如 ADR 测试按层级归类）

**当前问题**：ADR 测试与三层架构并存，可能导致重复和混乱

---

## 📌 总结与建议

### 核心结论

1. **ArchitectureTests.Specification 目录不存在**
   - 这可能是计划中但未实现的功能
   - 或者是历史遗留的术语，已被三层架构替代

2. **当前架构测试组织良好**
   - 三层架构（Governance/Enforcement/Heuristics）清晰
   - ADR 一一对应测试类，覆盖全面
   - 构建成功，测试可运行

3. **存在一些技术债务**
   - 硬编码配置分散
   - 大型测试类需拆分
   - 反射使用可优化

4. **无严重问题**
   - 无编译错误
   - 无架构设计缺陷
   - 测试覆盖完整

### 推荐行动路径

**短期（1-2 周）**：
1. 修复 ArchitectureAnalyzers 警告（高优先级 H1）
2. 改进 TestData 错误处理（高优先级 H3）
3. 明确 ArchitectureTests.Specification 的用途（人工确认 Q1）

**中期（1-2 个月）**：
1. 创建配置抽象层（高优先级 H2）
2. 引入程序集缓存（中优先级 M1）
3. 拆分大型测试类（中优先级 M2）

**长期（持续）**：
1. 考虑迁移到 Roslyn Analyzers（策略 C）
2. 标准化文档格式（低优先级 L3）
3. 建立测试数据生成器（低优先级 L2）

---

## 📚 附录

### A. 测试统计摘要

| 指标 | 数量 |
|------|------|
| 总文件数 | 33 个 .cs 文件 |
| 总代码行数 | ~7,497 行 |
| ADR 测试类 | 26 个 |
| Enforcement 测试类 | 4 个 |
| Governance 测试类 | 1 个 |
| Heuristics 测试类 | 1 个 |
| 辅助类 | 2 个（ModuleAssemblyData, HostAssemblyData） |
| 反射调用次数 | 172 次 |
| 文件系统调用次数 | ~60 次 |
| 外部依赖 | NetArchTest.Rules, xUnit, FluentAssertions |

### B. 依赖关系图

```
ArchitectureTests.csproj
  ├─ NetArchTest.Rules (架构规则引擎)
  ├─ xUnit (测试框架)
  ├─ FluentAssertions (断言库)
  ├─ Microsoft.AspNetCore.Mvc.Testing (集成测试)
  ├─ Marten.AspNetCore (文档数据库)
  ├─ WolverineFx.Http (消息总线)
  ├─ Testcontainers (容器化测试)
  └─ 项目引用
      ├─ Platform.csproj
      ├─ Application.csproj
      ├─ Members.csproj (模块)
      ├─ Orders.csproj (模块)
      └─ Web.csproj (Host)
```

### C. 命名空间层级

```
Zss.BilliardHall.Tests.ArchitectureTests
  ├─ .ADR (26 个测试类)
  ├─ .Enforcement (4 个测试类)
  ├─ .Governance (1 个测试类)
  └─ .Heuristics (1 个测试类)
```

---

**报告生成时间**: 2026-02-09  
**报告版本**: 1.0  
**分析工具**: Manual inspection + CLI tools  
**下次更新时间**: 根据行动清单完成进度

