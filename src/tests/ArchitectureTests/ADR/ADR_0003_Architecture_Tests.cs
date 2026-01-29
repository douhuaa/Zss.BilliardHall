using NetArchTest.Rules;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

// Version: 1.0
// ADR: ADR-0003
/// <summary>
/// ADR-0003: 命名空间与项目边界规范
/// 验证所有类型的命名空间遵循 BaseNamespace 约定
/// 
/// 测试映射：
/// - All_Types_Should_Start_With_Base_Namespace → ADR-0003.1 (所有类型应以 BaseNamespace 开头)
/// - Platform_Types_Should_Have_Platform_Namespace → ADR-0003.2 (Platform 类型应在 Zss.BilliardHall.Platform 命名空间)
/// - Application_Types_Should_Have_Application_Namespace → ADR-0003.3 (Application 类型应在 Zss.BilliardHall.Application 命名空间)
/// - Module_Types_Should_Have_Module_Namespace → ADR-0003.4 (Module 类型应在 Zss.BilliardHall.Modules.{ModuleName} 命名空间)
/// - Host_Types_Should_Have_Host_Namespace → ADR-0003.5 (Host 类型应在 Zss.BilliardHall.Host.{HostName} 命名空间)
/// - Directory_Build_Props_Should_Exist_At_Repository_Root → ADR-0003.6 (Directory.Build.props 应存在于仓库根目录)
/// - Directory_Build_Props_Should_Define_Base_Namespace → ADR-0003.7 (Directory.Build.props 应定义 BaseNamespace)
/// - All_Projects_Should_Follow_Namespace_Convention → ADR-0003.8 (所有项目应遵循命名空间约定)
/// - Modules_Should_Not_Contain_Irregular_Namespace_Patterns → ADR-0003.9 (模块不应包含不规范的命名空间模式)
/// </summary>
public sealed class ADR_0003_Architecture_Tests
{
    private const string BaseNamespace = "Zss.BilliardHall";

    #region 1. 基础命名空间约束

    [Fact(DisplayName = "ADR-0003.1: 所有类型应以 BaseNamespace 开头")]
    public void All_Types_Should_Start_With_Base_Namespace()
    {
        foreach (var assembly in GetAllProjectAssemblies())
        {
            var types = Types
                .InAssembly(assembly)
                .GetTypes()
                .Where(t => !t.Name.StartsWith("<")) // 排除编译器生成的类型
                .Where(t => t.Name != "Program") // 排除顶级语句生成的 Program 类
                .ToList();

            foreach (var type in types)
            {
                Assert.True(type.Namespace?.StartsWith(BaseNamespace) == true,
                $"❌ ADR-0003.1 违规: 所有类型的命名空间都应以 BaseNamespace 开头\n\n" +
                $"程序集: {assembly.GetName().Name}\n" +
                $"违规类型: {type.FullName}\n" +
                $"当前命名空间: {type.Namespace}\n" +
                $"期望开头: {BaseNamespace}\n\n" +
                $"修复建议:\n" +
                $"1. 检查项目的 RootNamespace 是否由 Directory.Build.props 正确推导\n" +
                $"2. 确保项目目录结构符合规范（Platform/Application/Modules/Host/Tests）\n" +
                $"3. 删除项目文件中的手动 RootNamespace 设置\n\n" +
                $"参考: docs/copilot/adr-0003.prompts.md (场景 1, 场景 2)");
            }
        }
    }

    #endregion

    #region 2. Platform 命名空间约束

    [Fact(DisplayName = "ADR-0003.2: Platform 类型应在 Zss.BilliardHall.Platform 命名空间")]
    public void Platform_Types_Should_Have_Platform_Namespace()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var types = Types
            .InAssembly(platformAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<"))
            .ToList();

        foreach (var type in types)
        {
            Assert.True(type.Namespace?.StartsWith($"{BaseNamespace}.Platform") == true,
            $"❌ ADR-0003.2 违规: Platform 程序集中的类型应在 {BaseNamespace}.Platform 命名空间下\n\n" +
            $"违规类型: {type.FullName}\n" +
            $"当前命名空间: {type.Namespace}\n" +
            $"期望命名空间前缀: {BaseNamespace}.Platform\n\n" +
            $"修复建议:\n" +
            $"1. 确保类型定义在正确的命名空间中\n" +
            $"2. Platform 层的所有代码都应该在 {BaseNamespace}.Platform 命名空间下\n" +
            $"3. 如果是子命名空间，应该是 {BaseNamespace}.Platform.* 格式\n\n" +
            $"参考: docs/copilot/adr-0003.prompts.md (场景 2)");
        }
    }

    #endregion

    #region 3. Application 命名空间约束

    [Fact(DisplayName = "ADR-0003.3: Application 类型应在 Zss.BilliardHall.Application 命名空间")]
    public void Application_Types_Should_Have_Application_Namespace()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var types = Types
            .InAssembly(applicationAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<"))
            .ToList();

        foreach (var type in types)
        {
            Assert.True(type.Namespace?.StartsWith($"{BaseNamespace}.Application") == true,
            $"❌ ADR-0003.3 违规: Application 程序集中的类型应在 {BaseNamespace}.Application 命名空间下\n\n" +
            $"违规类型: {type.FullName}\n" +
            $"当前命名空间: {type.Namespace}\n" +
            $"期望命名空间前缀: {BaseNamespace}.Application\n\n" +
            $"修复建议:\n" +
            $"1. 确保类型定义在正确的命名空间中\n" +
            $"2. Application 层的所有代码都应该在 {BaseNamespace}.Application 命名空间下\n" +
            $"3. 如果是子命名空间，应该是 {BaseNamespace}.Application.* 格式\n\n" +
            $"参考: docs/copilot/adr-0003.prompts.md (场景 2)");
        }
    }

    #endregion

    #region 4. Modules 命名空间约束

    [Theory(DisplayName = "ADR-0003.4: Module 类型应在 Zss.BilliardHall.Modules.{ModuleName} 命名空间")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Module_Types_Should_Have_Module_Namespace(Assembly moduleAssembly)
    {
        var moduleName = moduleAssembly.GetName()
            .Name!
            .Split('.')
            .Last();
        var types = Types
            .InAssembly(moduleAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<"))
            .ToList();

        foreach (var type in types)
        {
            Assert.True(type.Namespace?.StartsWith($"{BaseNamespace}.Modules.{moduleName}") == true,
            $"❌ ADR-0003.4 违规: Module 程序集中的类型应在 {BaseNamespace}.Modules.{{ModuleName}} 命名空间下\n\n" +
            $"模块名: {moduleName}\n" +
            $"违规类型: {type.FullName}\n" +
            $"当前命名空间: {type.Namespace}\n" +
            $"期望命名空间前缀: {BaseNamespace}.Modules.{moduleName}\n\n" +
            $"修复建议:\n" +
            $"1. 确保类型定义在正确的命名空间中\n" +
            $"2. 模块 {moduleName} 的所有代码都应该在 {BaseNamespace}.Modules.{moduleName} 命名空间下\n" +
            $"3. 如果是子命名空间，应该是 {BaseNamespace}.Modules.{moduleName}.* 格式（如 .Domain, .UseCases）\n\n" +
            $"参考: docs/copilot/adr-0003.prompts.md (场景 2)");
        }
    }

    #endregion

    #region 5. Host 命名空间约束

    [Theory(DisplayName = "ADR-0003.5: Host 类型应在 Zss.BilliardHall.Host.{HostName} 命名空间")]
    [ClassData(typeof(HostAssemblyData))]
    public void Host_Types_Should_Have_Host_Namespace(Assembly hostAssembly)
    {
        var hostName = hostAssembly.GetName()
            .Name!
            .Split('.')
            .Last();
        var types = Types
            .InAssembly(hostAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<"))
            .Where(t => t.Name != "Program") // 排除顶级语句生成的 Program 类
            .ToList();

        foreach (var type in types)
        {
            var expectedNamespace = $"{BaseNamespace}.Host.{hostName}";
            Assert.True(type.Namespace?.StartsWith(expectedNamespace) == true,
            $"❌ ADR-0003.5 违规: Host 程序集中的类型应在 {BaseNamespace}.Host.{{HostName}} 命名空间下\n\n" +
            $"Host 名: {hostName}\n" +
            $"违规类型: {type.FullName}\n" +
            $"当前命名空间: {type.Namespace}\n" +
            $"期望命名空间前缀: {expectedNamespace}\n\n" +
            $"修复建议:\n" +
            $"1. 确保类型定义在正确的命名空间中\n" +
            $"2. Host {hostName} 的所有代码都应该在 {expectedNamespace} 命名空间下\n" +
            $"3. 如果是子命名空间，应该是 {expectedNamespace}.* 格式\n\n" +
            $"参考: docs/copilot/adr-0003.prompts.md (场景 1, 场景 2)");
        }
    }

    #endregion

    #region 6. Directory.Build.props 约束

    [Fact(DisplayName = "ADR-0003.6: Directory.Build.props 应存在于仓库根目录")]
    public void Directory_Build_Props_Should_Exist_At_Repository_Root()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var directoryBuildPropsPath = Path.Combine(root, "Directory.Build.props");

        Assert.True(File.Exists(directoryBuildPropsPath),
        $"❌ ADR-0003.6 违规: Directory.Build.props 文件应存在于仓库根目录\n\n" +
        $"期望路径: {directoryBuildPropsPath}\n" +
        $"当前状态: 文件不存在\n\n" +
        $"修复建议:\n" +
        $"1. 在仓库根目录创建 Directory.Build.props 文件\n" +
        $"2. 在文件中定义 BaseNamespace（CompanyNamespace + ProductNamespace）\n" +
        $"3. 参考其他项目的 Directory.Build.props 模板\n\n" +
        $"参考: docs/copilot/adr-0003.prompts.md (场景 1)");
    }

    [Fact(DisplayName = "ADR-0003.7: Directory.Build.props 应定义 BaseNamespace")]
    public void Directory_Build_Props_Should_Define_Base_Namespace()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var directoryBuildPropsPath = Path.Combine(root, "Directory.Build.props");

        if (!File.Exists(directoryBuildPropsPath))
        {
            Assert.Fail($"❌ ADR-0003.6 违规: Directory.Build.props 文件不存在");
        }

        var content = File.ReadAllText(directoryBuildPropsPath);

        Assert.True(content.Contains("CompanyNamespace") || content.Contains("ProductNamespace") || content.Contains("BaseNamespace"),
        $"❌ ADR-0003.7 违规: Directory.Build.props 应定义 BaseNamespace 相关属性\n\n" +
        $"文件路径: {directoryBuildPropsPath}\n" +
        $"当前状态: 未找到 CompanyNamespace/ProductNamespace/BaseNamespace 定义\n\n" +
        $"修复建议:\n" +
        $"1. 在 Directory.Build.props 中添加 BaseNamespace 定义\n" +
        $"2. 使用以下格式:\n" +
        $"   <CompanyNamespace>Zss</CompanyNamespace>\n" +
        $"   <ProductNamespace>BilliardHall</ProductNamespace>\n" +
        $"   <BaseNamespace>$(CompanyNamespace).$(ProductNamespace)</BaseNamespace>\n" +
        $"3. 确保所有项目都依赖这个统一的 BaseNamespace\n\n" +
        $"参考: docs/copilot/adr-0003.prompts.md (场景 1, FAQ Q2)");
    }

    #endregion

    #region 7. 项目命名约束

    [Fact(DisplayName = "ADR-0003.8: 所有项目应遵循命名空间约定")]
    public void All_Projects_Should_Follow_Namespace_Convention()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var projectFiles = Directory
            .GetFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !p.Contains("/obj/") && !p.Contains("/bin/"))
            .ToList();

        Assert.NotEmpty(projectFiles);

        foreach (var projectFile in projectFiles)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectFile);
            var relativePath = Path.GetRelativePath(root, Path.GetDirectoryName(projectFile)!);

            // 基本检查：如果项目在 src/ 下，应该遵循 Zss.BilliardHall.* 约定
            if (relativePath.StartsWith("src/"))
            {
                var isValidName = projectName.StartsWith(BaseNamespace) ||
                                  projectName == "Platform" ||
                                  projectName == "Application" ||
                                  projectName == "Members" ||
                                  projectName == "Orders" ||
                                  projectName == "Web" ||
                                  projectName == "Worker" ||
                                  projectName == "ArchitectureTests" ||
                                  projectName == "ArchitectureAnalyzers" ||  // Level 2 enforcement tool
                                  projectName == "AdrParserCli";  // CLI tool in tools directory

                Assert.True(isValidName,
                $"❌ ADR-0003.8 违规: 项目命名不符合命名空间约定\n\n" +
                $"项目文件: {projectFile}\n" +
                $"项目名称: {projectName}\n" +
                $"相对路径: {relativePath}\n\n" +
                $"修复建议:\n" +
                $"1. 确保 src/ 下的项目使用 '{BaseNamespace}.*' 命名约定\n" +
                $"2. 或者确保项目的 RootNamespace 设置为 '{BaseNamespace}.*'\n" +
                $"3. 项目名应该与目录最后一级名称一致\n\n" +
                $"参考: docs/copilot/adr-0003.prompts.md (场景 1, 反模式 4)");
            }
        }
    }

    #endregion

    #region 8. 禁止的命名空间模式

    [Theory(DisplayName = "ADR-0003.9: 模块不应包含不规范的命名空间模式")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Contain_Irregular_Namespace_Patterns(Assembly moduleAssembly)
    {
        // 禁止的命名空间模式（已在 ADR-0001 中覆盖，这里作为 ADR-0003 的补充验证）
        var forbiddenPatterns = new[] { ".Util", ".Utils", ".Helper", ".Helpers", ".Common", ".Shared" };

        foreach (var pattern in forbiddenPatterns)
        {
            var result = Types
                .InAssembly(moduleAssembly)
                .That()
                .ResideInNamespaceStartingWith($"{BaseNamespace}.Modules")
                .ShouldNot()
                .ResideInNamespaceContaining(pattern)
                .GetResult();

            Assert.True(result.IsSuccessful,
            $"❌ ADR-0003.9 违规: 模块不应包含不规范的命名空间模式\n\n" +
            $"模块: {moduleAssembly.GetName().Name}\n" +
            $"禁止的模式: {pattern}\n" +
            $"违规类型:\n{string.Join("\n", result.FailingTypes?.Select(t => $"  - {t.FullName}") ?? Array.Empty<string>())}\n\n" +
            $"修复建议:\n" +
            $"1. 避免使用 Util/Helper/Common/Shared 等不规范的命名空间\n" +
            $"2. 采用垂直切片组织代码（按用例组织，而非按技术层次）\n" +
            $"3. 技术性抽象移到 Platform 或 BuildingBlocks\n\n" +
            $"参考: docs/copilot/adr-0003.prompts.md (反模式 3, FAQ Q4)");
        }
    }

    #endregion

    #region Helper Methods

    private static IEnumerable<Assembly> GetAllProjectAssemblies()
    {
        yield return typeof(Platform.PlatformBootstrapper).Assembly;
        yield return typeof(Application.ApplicationBootstrapper).Assembly;

        foreach (var moduleAssembly in ModuleAssemblyData.ModuleAssemblies)
        {
            yield return moduleAssembly;
        }

        foreach (var hostAssembly in HostAssemblyData.HostAssemblies)
        {
            yield return hostAssembly;
        }
    }

    #endregion

}
