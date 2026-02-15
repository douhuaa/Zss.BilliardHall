using Zss.BilliardHall.Tools.Governance.Cli.Commands;

namespace Zss.BilliardHall.Tools.Governance.Cli.Tests.Commands;

public sealed class ValidateCommandHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldValidateAllRuleSets_AndReturnSuccess()
    {
        // Arrange
        var handler = new ValidateCommandHandler();

        // Act
        var exitCode = await handler.ExecuteAsync();

        // Assert
        // 当前仓库的 RuleSetRegistry 应该是完整的，期望返回 0
        // 如果此测试失败，说明仓库中的 RuleSet 定义存在问题
        exitCode.Should().Be(0, "当前仓库的所有 RuleSet 应该通过完整性校验");
    }
}
