using System.IO;
using System.Linq;
using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// 基础设施约束测试
/// 确保项目遵循命名空间和 CPM 约定
/// </summary>
public class InfrastructureTests
{
    [Fact]
    public void Repository_Should_Have_DirectoryPackagesProps()
    {
        // 确保仓库根目录存在 Directory.Packages.props 文件（CPM 校验）
        var root = ModuleIsolationTests.GetSolutionRoot();
        var cpmFile = Path.Combine(root, "Directory.Packages.props");
        
        Assert.True(File.Exists(cpmFile), 
            $"仓库根目录必须存在 Directory.Packages.props 文件以启用 Central Package Management (CPM)。" +
            $"预期路径: {cpmFile}。" +
            $"修复建议：在仓库根目录创建 Directory.Packages.props 文件并配置 ManagePackageVersionsCentrally=true。");
    }

    [Fact]
    public void All_Types_Should_Have_Proper_RootNamespace()
    {
        // 确保所有类型的命名空间都以 "Zss.BilliardHall" 开头
        // 使用已加载的模块和 Host 程序集进行测试
        var assembliesToCheck = new List<System.Reflection.Assembly>();
        
        // 添加模块程序集
        assembliesToCheck.AddRange(ModuleAssemblyData.ModuleAssemblies);
        
        // 添加 Host 程序集
        assembliesToCheck.AddRange(HostAssemblyData.HostAssemblies);
        
        // 添加 Platform 程序集
        assembliesToCheck.Add(typeof(Platform.PlatformBootstrapper).Assembly);

        // 如果没有程序集，跳过测试（可能是在测试发现阶段）
        if (assembliesToCheck.Count == 0)
        {
            return;
        }

        foreach (var assembly in assembliesToCheck)
        {
            var types = assembly.GetTypes()
                .Where(t => !t.IsNested) // 排除嵌套类型
                .Where(t => !t.Name.StartsWith("<")) // 排除编译器生成的类型
                .Where(t => !string.IsNullOrEmpty(t.Namespace))
                .ToList();

            var badTypes = types
                .Where(t => !t.Namespace!.StartsWith("Zss.BilliardHall"))
                .ToList();

            Assert.True(badTypes.Count == 0,
                $"程序集 {assembly.GetName().Name} 中发现不符合命名空间约定的类型：" +
                $"{string.Join(", ", badTypes.Select(t => $"{t.FullName} (命名空间: {t.Namespace})"))}。" +
                $"修复建议：所有类型的命名空间必须以 'Zss.BilliardHall' 开头。检查项目的 RootNamespace 设置。");
        }
    }

    [Fact]
    public void All_Projects_Should_Have_Consistent_RootNamespace()
    {
        // 确保所有项目的 RootNamespace 遵循约定
        var root = ModuleIsolationTests.GetSolutionRoot();
        var projectFiles = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !p.Contains("/obj/") && !p.Contains("/bin/"))
            .ToList();

        Assert.NotEmpty(projectFiles);

        foreach (var projectFile in projectFiles)
        {
            var projectName = Path.GetFileNameWithoutExtension(projectFile);
            var projectDir = Path.GetDirectoryName(projectFile)!;
            
            // 根据项目路径推断预期的 RootNamespace
            var relativePath = Path.GetRelativePath(root, projectDir);
            
            // 基本检查：如果项目在 src/ 下，应该遵循 Zss.BilliardHall.* 约定
            if (relativePath.StartsWith("src/"))
            {
                // 项目名称应该以 Zss.BilliardHall 开头或者匹配特定模式
                var isValidName = projectName.StartsWith("Zss.BilliardHall") 
                    || projectName == "Platform" 
                    || projectName == "Application"
                    || projectName == "Members"
                    || projectName == "Orders"
                    || projectName == "Web"
                    || projectName == "Worker"
                    || projectName == "ArchitectureTests";

                Assert.True(isValidName,
                    $"项目 {projectFile} 的命名不符合约定。" +
                    $"项目名称: {projectName}。" +
                    $"修复建议：src/ 下的项目应该使用 'Zss.BilliardHall.*' 命名约定，" +
                    $"或者确保其 RootNamespace 设置为 'Zss.BilliardHall.*'。");
            }
        }
    }

    [Fact]
    public void CPM_Should_Be_Enabled()
    {
        // 验证 Directory.Packages.props 内容包含 CPM 启用配置
        var root = ModuleIsolationTests.GetSolutionRoot();
        var cpmFile = Path.Combine(root, "Directory.Packages.props");
        
        Assert.True(File.Exists(cpmFile), "Directory.Packages.props 文件不存在");

        var content = File.ReadAllText(cpmFile);
        
        Assert.True(content.Contains("ManagePackageVersionsCentrally"),
            $"Directory.Packages.props 必须包含 ManagePackageVersionsCentrally 设置。" +
            $"修复建议：在 Directory.Packages.props 中添加 <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>。");
        
        Assert.True(content.Contains("true"),
            $"Directory.Packages.props 中的 ManagePackageVersionsCentrally 应该设置为 true。");
    }
}
