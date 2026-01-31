using NetArchTest.Rules;
using FluentAssertions;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-360: CI/CD Pipeline 流程标准化
/// 参考：docs/adr/technical/ADR-360-cicd-pipeline-standardization.md
/// </summary>
public sealed class ADR_0360_Architecture_Tests
{
    [Fact(DisplayName = "ADR-360.1: GitHub Workflows 配置文件应存在")]
    public void GitHub_Workflows_Configuration_Should_Exist()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = FindRepositoryRoot(currentDir);
        
        repoRoot.Should().NotBeNull();
        
        var workflowsDir = Path.Combine(repoRoot!, ".github", "workflows");
        Directory.Exists(workflowsDir).Should().BeTrue($"【ADR-360.1】GitHub Workflows 目录应存在：{workflowsDir}");
        
        // 验证至少有一个 workflow 文件
        var workflowFiles = Directory.GetFiles(workflowsDir, "*.yml")
            .Concat(Directory.GetFiles(workflowsDir, "*.yaml"))
            .ToList();
            
        workflowFiles.Should().NotBeEmpty();
    }

    [Fact(DisplayName = "ADR-360.2: PR 模板应存在")]
    public void Pull_Request_Template_Should_Exist()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = FindRepositoryRoot(currentDir);
        
        repoRoot.Should().NotBeNull();
        
        var prTemplate = Path.Combine(repoRoot!, ".github", "PULL_REQUEST_TEMPLATE.md");
        File.Exists(prTemplate).Should().BeTrue($"【ADR-360.2】PR 模板文件应存在：{prTemplate}");
    }

    [Fact(DisplayName = "ADR-360.3: 架构测试项目应存在并可被 CI 执行")]
    public void Architecture_Tests_Should_Be_Executable()
    {
        // 验证架构测试项目可以被发现和执行
        var currentAssembly = typeof(ADR_0360_Architecture_Tests).Assembly;
        currentAssembly.Should().NotBeNull();
        currentAssembly.GetName().Name.Should().Be("ArchitectureTests");
    }

    private static string? FindRepositoryRoot(string startPath)
    {
        var dir = new DirectoryInfo(startPath);
        while (dir != null)
        {
            if (Directory.Exists(Path.Combine(dir.FullName, ".github")) ||
                File.Exists(Path.Combine(dir.FullName, "Directory.Build.props")))
            {
                return dir.FullName;
            }
            dir = dir.Parent;
        }
        return null;
    }
}
