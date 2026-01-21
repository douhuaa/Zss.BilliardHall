using NetArchTest.Rules;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0003: 命名空间与项目边界规范
/// 验证所有类型的命名空间遵循 BaseNamespace 约定
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
            var types = Types.InAssembly(assembly)
                .GetTypes()
                .Where(t => !t.Name.StartsWith("<")) // 排除编译器生成的类型
                .Where(t => t.Name != "Program") // 排除顶级语句生成的 Program 类
                .ToList();

            foreach (var type in types)
            {
                Assert.True(type.Namespace?.StartsWith(BaseNamespace) == true,
                    $"❌ ADR-0003 违规: 程序集 {assembly.GetName().Name} 中存在不符合命名空间规范的类型: {type.FullName}。\n" +
                    $"修复建议：所有类型的命名空间都应以 '{BaseNamespace}' 开头。检查项目的 RootNamespace 设置。");
            }
        }
    }

    #endregion

    #region 2. Platform 命名空间约束

    [Fact(DisplayName = "ADR-0003.2: Platform 类型应在 Zss.BilliardHall.Platform 命名空间")]
    public void Platform_Types_Should_Have_Platform_Namespace()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var types = Types.InAssembly(platformAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<"))
            .ToList();

        foreach (var type in types)
        {
            Assert.True(type.Namespace?.StartsWith($"{BaseNamespace}.Platform") == true,
                $"❌ ADR-0003 违规: Platform 程序集中存在不在 {BaseNamespace}.Platform 命名空间下的类型: {type.FullName}。\n" +
                $"修复建议：Platform 层的所有类型都应该在 '{BaseNamespace}.Platform' 命名空间下。");
        }
    }

    #endregion

    #region 3. Application 命名空间约束

    [Fact(DisplayName = "ADR-0003.3: Application 类型应在 Zss.BilliardHall.Application 命名空间")]
    public void Application_Types_Should_Have_Application_Namespace()
    {
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var types = Types.InAssembly(applicationAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<"))
            .ToList();

        foreach (var type in types)
        {
            Assert.True(type.Namespace?.StartsWith($"{BaseNamespace}.Application") == true,
                $"❌ ADR-0003 违规: Application 程序集中存在不在 {BaseNamespace}.Application 命名空间下的类型: {type.FullName}。\n" +
                $"修复建议：Application 层的所有类型都应该在 '{BaseNamespace}.Application' 命名空间下。");
        }
    }

    #endregion

    #region 4. Modules 命名空间约束

    [Theory(DisplayName = "ADR-0003.4: Module 类型应在 Zss.BilliardHall.Modules.{ModuleName} 命名空间")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Module_Types_Should_Have_Module_Namespace(Assembly moduleAssembly)
    {
        var moduleName = moduleAssembly.GetName().Name!.Split('.').Last();
        var types = Types.InAssembly(moduleAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<"))
            .ToList();

        foreach (var type in types)
        {
            Assert.True(type.Namespace?.StartsWith($"{BaseNamespace}.Modules.{moduleName}") == true,
                $"❌ ADR-0003 违规: 模块 {moduleName} 程序集中存在不在 {BaseNamespace}.Modules.{moduleName} 命名空间下的类型: {type.FullName}。\n" +
                $"修复建议：模块 {moduleName} 的所有类型都应该在 '{BaseNamespace}.Modules.{moduleName}' 命名空间下。");
        }
    }

    #endregion

    #region 5. Host 命名空间约束

    [Theory(DisplayName = "ADR-0003.5: Host 类型应在 Zss.BilliardHall.Host.{HostName} 命名空间")]
    [ClassData(typeof(HostAssemblyData))]
    public void Host_Types_Should_Have_Host_Namespace(Assembly hostAssembly)
    {
        var hostName = hostAssembly.GetName().Name!.Split('.').Last();
        var types = Types.InAssembly(hostAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<"))
            .Where(t => t.Name != "Program") // 排除顶级语句生成的 Program 类
            .ToList();

        foreach (var type in types)
        {
            var expectedNamespace = $"{BaseNamespace}.Host.{hostName}";
            Assert.True(type.Namespace?.StartsWith(expectedNamespace) == true,
                $"❌ ADR-0003 违规: Host {hostName} 程序集中存在不在 {expectedNamespace} 命名空间下的类型: {type.FullName}。\n" +
                $"修复建议：Host {hostName} 的所有类型都应该在 '{expectedNamespace}' 命名空间下。");
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
            $"❌ ADR-0003 违规: 未找到 Directory.Build.props 文件，路径: {directoryBuildPropsPath}。\n" +
            $"修复建议：在仓库根目录创建 Directory.Build.props 文件以统一项目配置。");
    }

    [Fact(DisplayName = "ADR-0003.7: Directory.Build.props 应定义 BaseNamespace")]
    public void Directory_Build_Props_Should_Define_Base_Namespace()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var directoryBuildPropsPath = Path.Combine(root, "Directory.Build.props");

        if (!File.Exists(directoryBuildPropsPath))
        {
            Assert.Fail($"❌ ADR-0003 违规: Directory.Build.props 文件不存在。");
        }

        var content = File.ReadAllText(directoryBuildPropsPath);

        Assert.True(content.Contains("CompanyNamespace") || content.Contains("ProductNamespace") || content.Contains("BaseNamespace"),
            $"❌ ADR-0003 违规: Directory.Build.props 应定义 CompanyNamespace/ProductNamespace/BaseNamespace。\n" +
            $"修复建议：在 Directory.Build.props 中添加：\n" +
            $"  <CompanyNamespace>Zss</CompanyNamespace>\n" +
            $"  <ProductNamespace>BilliardHall</ProductNamespace>\n" +
            $"  <BaseNamespace>$(CompanyNamespace).$(ProductNamespace)</BaseNamespace>");
    }

    #endregion

    #region 7. 项目命名约束

    [Fact(DisplayName = "ADR-0003.8: 所有项目应遵循命名空间约定")]
    public void All_Projects_Should_Follow_Namespace_Convention()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var projectFiles = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories)
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
                var isValidName = projectName.StartsWith(BaseNamespace)
                    || projectName == "Platform"
                    || projectName == "Application"
                    || projectName == "Members"
                    || projectName == "Orders"
                    || projectName == "Web"
                    || projectName == "Worker"
                    || projectName == "ArchitectureTests"
                    || projectName == "ArchitectureAnalyzers";  // Level 2 enforcement tool

                Assert.True(isValidName,
                    $"❌ ADR-0003 违规: 项目 {projectFile} 的命名不符合约定。\n" +
                    $"项目名称: {projectName}\n" +
                    $"修复建议：src/ 下的项目应该使用 '{BaseNamespace}.*' 命名约定，或者确保其 RootNamespace 设置为 '{BaseNamespace}.*'。");
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
            var result = Types.InAssembly(moduleAssembly)
                .That().ResideInNamespaceStartingWith($"{BaseNamespace}.Modules")
                .ShouldNot().ResideInNamespaceContaining(pattern)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"❌ ADR-0003 违规: 模块 {moduleAssembly.GetName().Name} 不应包含命名空间模式: {pattern}。\n" +
                $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
                $"修复建议：避免使用 Util/Helper/Common/Shared 等命名空间，采用垂直切片组织代码。");
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
