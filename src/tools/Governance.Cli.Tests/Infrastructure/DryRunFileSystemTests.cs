using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Tests.Infrastructure;

public sealed class DryRunFileSystemTests
{
    [Fact]
    public async Task WriteAllTextAsync_ShouldNotWriteToInnerFileSystem()
    {
        // Arrange
        var innerFileSystem = new InMemoryFileSystem();
        var dryRunFileSystem = new DryRunFileSystem(innerFileSystem);
        const string path = "/test/file.txt";
        const string content = "Test content";

        // Act
        await dryRunFileSystem.WriteAllTextAsync(path, content);

        // Assert
        innerFileSystem.FileExists(path).Should().BeFalse();
    }

    [Fact]
    public async Task ReadAllTextAsync_ShouldReadFromInnerFileSystem()
    {
        // Arrange
        var innerFileSystem = new InMemoryFileSystem();
        const string path = "/test/file.txt";
        const string content = "Test content";
        innerFileSystem.AddFile(path, content);

        var dryRunFileSystem = new DryRunFileSystem(innerFileSystem);

        // Act
        var result = await dryRunFileSystem.ReadAllTextAsync(path);

        // Assert
        result.Should().Be(content);
    }

    [Fact]
    public void FileExists_ShouldCheckInnerFileSystem()
    {
        // Arrange
        var innerFileSystem = new InMemoryFileSystem();
        const string path = "/test/file.txt";
        innerFileSystem.AddFile(path, "content");

        var dryRunFileSystem = new DryRunFileSystem(innerFileSystem);

        // Act
        var exists = dryRunFileSystem.FileExists(path);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public void CreateDirectory_ShouldNotCreateInInnerFileSystem()
    {
        // Arrange
        var innerFileSystem = new InMemoryFileSystem();
        var dryRunFileSystem = new DryRunFileSystem(innerFileSystem);
        const string path = "/test/dir";

        // Act
        dryRunFileSystem.CreateDirectory(path);

        // Assert
        innerFileSystem.DirectoryExists(path).Should().BeFalse();
    }
}
