using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Tests.Infrastructure;

public sealed class RepositoryPathValidatorTests
{
    [Fact]
    public void IsPathSafe_WithPathPrefixCollision_ShouldReturnFalse()
    {
        // Arrange
        var validator = new RepositoryPathValidator();
        
        // 模拟一个路径前缀碰撞的场景
        // 例如，如果仓库根目录是 /home/repo，则 /home/repo2 不应该通过验证
        var currentDir = Directory.GetCurrentDirectory();
        var parentDir = Directory.GetParent(currentDir)?.FullName;
        
        if (parentDir != null)
        {
            // 创建一个相邻目录路径（如 /home/repo -> /home/repo2）
            var siblingPath = currentDir + "2";
            
            // Act
            var result = validator.IsPathSafe(siblingPath, out var errorMessage);
            
            // Assert
            result.Should().BeFalse("路径前缀碰撞应该被拒绝");
            errorMessage.Should().Contain("必须在仓库根目录下");
        }
    }

    [Fact]
    public void IsPathSafe_WithRepositoryRoot_ShouldReturnTrue()
    {
        // Arrange
        var validator = new RepositoryPathValidator();
        var currentDir = Directory.GetCurrentDirectory();
        
        // Act
        var result = validator.IsPathSafe(currentDir, out var errorMessage);
        
        // Assert
        result.Should().BeTrue("仓库根目录本身应该通过验证");
        errorMessage.Should().BeEmpty();
    }

    [Fact]
    public void IsPathSafe_WithSubdirectory_ShouldReturnTrue()
    {
        // Arrange
        var validator = new RepositoryPathValidator();
        var currentDir = Directory.GetCurrentDirectory();
        var subDir = Path.Combine(currentDir, "src", "tools");
        
        // Act
        var result = validator.IsPathSafe(subDir, out var errorMessage);
        
        // Assert
        result.Should().BeTrue("仓库子目录应该通过验证");
        errorMessage.Should().BeEmpty();
    }

    [Fact]
    public void IsPathSafe_WithPathTraversalPattern_ShouldReturnFalse()
    {
        // Arrange
        var validator = new RepositoryPathValidator();
        var pathWithTraversal = "../outside";
        
        // Act
        var result = validator.IsPathSafe(pathWithTraversal, out var errorMessage);
        
        // Assert
        result.Should().BeFalse("包含路径遍历模式的路径应该被拒绝");
        errorMessage.Should().NotBeEmpty();
    }
}
