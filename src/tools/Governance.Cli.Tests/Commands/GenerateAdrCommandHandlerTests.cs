using Zss.BilliardHall.Generators;
using Zss.BilliardHall.Specification.Index;
using Zss.BilliardHall.Tools.Governance.Cli.Commands;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;
using Zss.BilliardHall.Tools.Governance.Cli.Tests.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Tests.Commands;

public sealed class GenerateAdrCommandHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidAdr_ShouldGenerateAndMergeDecision()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        const string adrPath = "/docs/adr/ADR-001.md";
        const string existingContent = @"---
title: Test ADR
---

# ADR-001: Test

## Context
Some context

## Consequences
Some consequences
";
        fileSystem.AddFile(adrPath, existingContent);

        var decisionGenerator = new AdrDecisionGenerator();
        var documentMerger = new AdrDocumentMerger(decisionGenerator);
        var pathValidator = new NoOpPathValidator();
        var handler = new GenerateAdrCommandHandler(fileSystem, decisionGenerator, documentMerger, pathValidator);

        // Act
        var exitCode = await handler.ExecuteAsync("1", adrPath);

        // Assert
        exitCode.Should().Be(0);
        var updatedContent = fileSystem.GetFileContent(adrPath);
        updatedContent.Should().Contain("## Decision");
    }

    [Fact]
    public async Task ExecuteAsync_WithNonExistentFile_ShouldReturnError()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        var decisionGenerator = new AdrDecisionGenerator();
        var documentMerger = new AdrDocumentMerger(decisionGenerator);
        var pathValidator = new NoOpPathValidator();
        var handler = new GenerateAdrCommandHandler(fileSystem, decisionGenerator, documentMerger, pathValidator);

        // Act
        var exitCode = await handler.ExecuteAsync("1", "/non/existent/file.md");

        // Assert
        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidAdrNumber_ShouldReturnError()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        const string adrPath = "/docs/adr/ADR-999.md";
        fileSystem.AddFile(adrPath, "# ADR-999");

        var decisionGenerator = new AdrDecisionGenerator();
        var documentMerger = new AdrDocumentMerger(decisionGenerator);
        var pathValidator = new NoOpPathValidator();
        var handler = new GenerateAdrCommandHandler(fileSystem, decisionGenerator, documentMerger, pathValidator);

        // Act
        var exitCode = await handler.ExecuteAsync("999", adrPath);

        // Assert
        exitCode.Should().Be(1);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("ADR-001")]
    public async Task ExecuteAsync_WithDifferentAdrFormats_ShouldWork(string adrInput)
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        const string adrPath = "/docs/adr/ADR-001.md";
        fileSystem.AddFile(adrPath, "# ADR-001\n## Context\nTest");

        var decisionGenerator = new AdrDecisionGenerator();
        var documentMerger = new AdrDocumentMerger(decisionGenerator);
        var pathValidator = new NoOpPathValidator();
        var handler = new GenerateAdrCommandHandler(fileSystem, decisionGenerator, documentMerger, pathValidator);

        // Act
        var exitCode = await handler.ExecuteAsync(adrInput, adrPath);

        // Assert
        exitCode.Should().Be(0);
    }
}
