using Zss.BilliardHall.Specification.Services;
using Zss.BilliardHall.Tools.Governance.Cli.Commands;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;
using Zss.BilliardHall.Tools.Governance.Cli.Tests.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Tests.Commands;

/// <summary>
/// UpdateDocumentationCommandHandler 测试
/// </summary>
public sealed class UpdateDocumentationCommandHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldGenerateAdrIndex()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        const string adrPath = "docs/adr";
        fileSystem.CreateDirectory(adrPath);

        var queryService = new RuleSetQueryService();
        var handler = new UpdateDocumentationCommandHandler(fileSystem, queryService);

        // Act
        var exitCode = await handler.ExecuteAsync(adrPath);

        // Assert
        exitCode.Should().Be(0);
        var files = fileSystem.GetAllFiles();
        files.Should().ContainKey("docs/adr/README.md");
        
        var content = files["docs/adr/README.md"];
        content.Should().Contain("## ADR 索引");
        content.Should().Contain("ADR-001"); // 应该包含至少一个ADR
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentPath_ShouldReturnError()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        var queryService = new RuleSetQueryService();
        var handler = new UpdateDocumentationCommandHandler(fileSystem, queryService);

        // Act
        var exitCode = await handler.ExecuteAsync("non/existent/path");

        // Assert
        exitCode.Should().Be(1);
    }
}
