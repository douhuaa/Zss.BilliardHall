using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// 命名空间约束测试
/// 确保所有类型命名空间以 Zss.BilliardHall 开头
/// 确保 Directory.Packages.props 存在于仓库根目录
/// </summary>
public class NamespaceTests
{
    private static readonly Assembly[] AllAssemblies = new[]
    {
        typeof(Platform.PlatformBootstrapper).Assembly,
        typeof(Application.ApplicationBootstrapper).Assembly,
        // Modules assemblies will be loaded dynamically
    };

    [Fact]
    public void All_Types_Should_Start_With_Base_Namespace()
    {
        // 所有类型命名空间都应以 Zss.BilliardHall 开头
        foreach (var assembly in GetAllProjectAssemblies())
        {
            var types = Types.InAssembly(assembly)
                .GetTypes()
                .Where(t => !t.Name.StartsWith("<")) // 排除编译器生成的类型
                .ToList();

            foreach (var type in types)
            {
                Assert.True(type.Namespace?.StartsWith("Zss.BilliardHall") == true,
                    $"程序集 {assembly.GetName().Name} 中存在不符合命名空间规范的类型: {type.FullName}。" +
                    $"修复建议：所有类型的命名空间都应以 'Zss.BilliardHall' 开头。");
            }
        }
    }

    [Fact]
    public void Platform_Types_Should_Have_Platform_Namespace()
    {
        // Platform 层的类型应该在 Zss.BilliardHall.Platform 命名空间下
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var types = Types.InAssembly(platformAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<")) // 排除编译器生成的类型
            .ToList();

        foreach (var type in types)
        {
            Assert.True(type.Namespace?.StartsWith("Zss.BilliardHall.Platform") == true,
                $"Platform 程序集中存在不在 Zss.BilliardHall.Platform 命名空间下的类型: {type.FullName}。" +
                $"修复建议：Platform 层的所有类型都应该在 'Zss.BilliardHall.Platform' 命名空间下。");
        }
    }

    [Fact]
    public void Application_Types_Should_Have_Application_Namespace()
    {
        // Application 层的类型应该在 Zss.BilliardHall.Application 命名空间下
        var applicationAssembly = typeof(Application.ApplicationBootstrapper).Assembly;
        var types = Types.InAssembly(applicationAssembly)
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<")) // 排除编译器生成的类型
            .ToList();

        foreach (var type in types)
        {
            Assert.True(type.Namespace?.StartsWith("Zss.BilliardHall.Application") == true,
                $"Application 程序集中存在不在 Zss.BilliardHall.Application 命名空间下的类型: {type.FullName}。" +
                $"修复建议：Application 层的所有类型都应该在 'Zss.BilliardHall.Application' 命名空间下。");
        }
    }

    [Fact]
    public void Module_Types_Should_Have_Module_Namespace()
    {
        // 模块的类型应该在 Zss.BilliardHall.Modules.{ModuleName} 命名空间下
        foreach (var moduleAssembly in ModuleAssemblyData.ModuleAssemblies)
        {
            var moduleName = moduleAssembly.GetName().Name!.Split('.').Last();
            var types = Types.InAssembly(moduleAssembly)
                .GetTypes()
                .Where(t => !t.Name.StartsWith("<")) // 排除编译器生成的类型
                .ToList();

            foreach (var type in types)
            {
                Assert.True(type.Namespace?.StartsWith($"Zss.BilliardHall.Modules.{moduleName}") == true,
                    $"模块 {moduleName} 程序集中存在不在 Zss.BilliardHall.Modules.{moduleName} 命名空间下的类型: {type.FullName}。" +
                    $"修复建议：模块 {moduleName} 的所有类型都应该在 'Zss.BilliardHall.Modules.{moduleName}' 命名空间下。");
            }
        }
    }

    [Fact]
    public void Directory_Packages_Props_Should_Exist_At_Repository_Root()
    {
        // Directory.Packages.props 应该存在于仓库根目录
        var root = ModuleIsolationTests.GetSolutionRoot();
        var directoryPackagesPropsPath = Path.Combine(root, "Directory.Packages.props");

        Assert.True(File.Exists(directoryPackagesPropsPath),
            $"未找到 Directory.Packages.props 文件，路径: {directoryPackagesPropsPath}。" +
            $"修复建议：在仓库根目录创建 Directory.Packages.props 文件以启用中央包管理。" +
            $"参考: https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management");
    }

    [Fact]
    public void Directory_Build_Props_Should_Exist_At_Repository_Root()
    {
        // Directory.Build.props 应该存在于仓库根目录
        var root = ModuleIsolationTests.GetSolutionRoot();
        var directoryBuildPropsPath = Path.Combine(root, "Directory.Build.props");

        Assert.True(File.Exists(directoryBuildPropsPath),
            $"未找到 Directory.Build.props 文件，路径: {directoryBuildPropsPath}。" +
            $"修复建议：在仓库根目录创建 Directory.Build.props 文件以统一项目配置。");
    }

    private static IEnumerable<Assembly> GetAllProjectAssemblies()
    {
        // 返回所有项目程序集
        yield return typeof(Platform.PlatformBootstrapper).Assembly;
        yield return typeof(Application.ApplicationBootstrapper).Assembly;

        // 添加所有模块程序集
        foreach (var moduleAssembly in ModuleAssemblyData.ModuleAssemblies)
        {
            yield return moduleAssembly;
        }
    }
}
