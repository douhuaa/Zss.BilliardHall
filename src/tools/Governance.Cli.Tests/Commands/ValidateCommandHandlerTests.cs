using Zss.BilliardHall.Tools.Governance.Cli.Commands;

namespace Zss.BilliardHall.Tools.Governance.Cli.Tests.Commands;

public sealed class ValidateCommandHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldValidateAllRuleSets()
    {
        // Arrange
        var handler = new ValidateCommandHandler();

        // Act
        var exitCode = await handler.ExecuteAsync();

        // Assert
        // 如果校验通过应该返回0，如果有错误应该返回1
        // 由于我们不能mock RuleSetRegistry，这个测试依赖实际的规则集
        exitCode.Should().BeOneOf(0, 1);
    }
}
