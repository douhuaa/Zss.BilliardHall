using Zss.BilliardHall.Generators;
using Zss.BilliardHall.Tools.Governance.Cli.Commands;
using Zss.BilliardHall.Tools.Governance.Cli.Tests.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Tests.Commands;

public sealed class GenerateAgentCommandHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldGenerateYamlFile()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        const string outputDir = "/output";
        fileSystem.CreateDirectory(outputDir);

        var instructionGenerator = new AgentInstructionGenerator();
        var handler = new GenerateAgentCommandHandler(fileSystem, instructionGenerator);

        // Act
        var exitCode = await handler.ExecuteAsync(outputDir, adrNumber: 1);

        // Assert
        exitCode.Should().Be(0);
        var files = fileSystem.GetAllFiles();
        files.Should().ContainKey("/output/ADR-001-agent-instructions.yaml");
    }

    [Fact]
    public async Task ExecuteAsync_WithoutAdrNumber_ShouldGenerateMultipleFiles()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        const string outputDir = "/output";
        fileSystem.CreateDirectory(outputDir);

        var instructionGenerator = new AgentInstructionGenerator();
        var handler = new GenerateAgentCommandHandler(fileSystem, instructionGenerator);

        // Act
        var exitCode = await handler.ExecuteAsync(outputDir, adrNumber: null);

        // Assert
        exitCode.Should().Be(0);
        var files = fileSystem.GetAllFiles();
        files.Should().NotBeEmpty();
        files.Keys.Should().Contain(key => key.EndsWith("-agent-instructions.yaml"));
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentDirectory_ShouldCreateIt()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        const string outputDir = "/new/output";

        var instructionGenerator = new AgentInstructionGenerator();
        var handler = new GenerateAgentCommandHandler(fileSystem, instructionGenerator);

        // Act
        var exitCode = await handler.ExecuteAsync(outputDir, adrNumber: 1);

        // Assert
        exitCode.Should().Be(0);
        fileSystem.DirectoryExists(outputDir).Should().BeTrue();
    }
}
