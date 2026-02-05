using NetArchTest.Rules;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;
using System.Reflection;
using static Zss.BilliardHall.Tests.ArchitectureTests.Shared.AssertionMessageBuilder;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_003;

/// <summary>
/// ADR-003.7: 项目命名约束
/// 验证所有项目应遵循命名空间约定
/// </summary>
public sealed class ADR_003_7_Architecture_Tests
{
    private const string BaseNamespace = "Zss.BilliardHall";

    /// <summary>
    /// ADR-003_7_1: 所有项目应遵循命名空间约定
    /// </summary>
    [Fact(DisplayName = "ADR-003_7_1: 所有项目应遵循命名空间约定")]
    public void ADR_003_7_1_All_Projects_Should_Follow_Namespace_Convention()
    {
        var root = TestEnvironment.RepositoryRoot;
        var projectFiles = Directory
            .GetFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !p.Contains("/obj/") && !p.Contains("/bin/"))
            .ToList();

        projectFiles.Should().NotBeEmpty();

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

                isValidName.Should().BeTrue(
                    Build(
                        ruleId: "ADR-003_7_1",
                        summary: "项目命名不符合命名空间约定",
                        currentState: $"项目文件: {projectFile}\n项目名称: {projectName}\n相对路径: {relativePath}",
                        remediationSteps: new[]
                        {
                            $"确保 src/ 下的项目使用 '{BaseNamespace}.*' 命名约定",
                            $"或者确保项目的 RootNamespace 设置为 '{BaseNamespace}.*'",
                            "项目名应该与目录最后一级名称一致"
                        },
                        adrReference: "docs/copilot/adr-003.prompts.md (场景 1, 反模式 4)"));
            }
        }
    }
}
