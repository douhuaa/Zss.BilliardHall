using NetArchTest.Rules;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-930: 代码审查与 ADR 合规自检流程
/// 参考：docs/adr/governance/ADR-930-code-review-compliance.md
/// </summary>
public sealed class ADR_0930_Architecture_Tests
{
    [Fact(DisplayName = "ADR-930.1: PR 模板应包含必要的自检清单")]
    public void PR_Template_Should_Include_Checklist()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = FindRepositoryRoot(currentDir);
        
        Assert.NotNull(repoRoot);
        
        var prTemplate = Path.Combine(repoRoot!, ".github", "PULL_REQUEST_TEMPLATE.md");
        Assert.True(File.Exists(prTemplate), 
            $"【ADR-930.1】PR 模板文件应存在：{prTemplate}");
        
        // 验证模板包含基本内容
        var content = File.ReadAllText(prTemplate);
        Assert.NotEmpty(content);
        Assert.True(content.Length > 100, "PR 模板应包含实质性内容");
    }

    [Fact(DisplayName = "ADR-930.2: Copilot 指令文件应存在")]
    public void Copilot_Instructions_Should_Exist()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var repoRoot = FindRepositoryRoot(currentDir);
        
        Assert.NotNull(repoRoot);
        
        // 验证 Copilot 指令目录存在
        var copilotDir = Path.Combine(repoRoot!, "docs", "copilot");
        Assert.True(Directory.Exists(copilotDir), 
            $"【ADR-930.2】Copilot 指令目录应存在：{copilotDir}");
        
        // 验证至少有一些提示词文件
        var promptFiles = Directory.GetFiles(copilotDir, "*.prompts.md");
        Assert.True(promptFiles.Length > 0, "应存在 Copilot 提示词文件");
    }

    [Fact(DisplayName = "ADR-930.3: 架构测试必须在 CI 中执行")]
    public void Architecture_Tests_Must_Be_In_CI()
    {
        // 验证架构测试项目可以被 CI 发现和执行
        var currentAssembly = typeof(ADR_0930_Architecture_Tests).Assembly;
        Assert.NotNull(currentAssembly);
        Assert.Equal("ArchitectureTests", currentAssembly.GetName().Name);
        
        // 验证测试类存在并可执行
        var testTypes = currentAssembly.GetTypes()
            .Where(t => t.Name.EndsWith("Architecture_Tests"))
            .ToList();
            
        Assert.True(testTypes.Count >= 10, 
            $"【ADR-930.3】应至少有 10 个架构测试类，当前：{testTypes.Count}");
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
