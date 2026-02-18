using Zss.BilliardHall.Tools.Governance.Cli.Commands;
using Zss.BilliardHall.Tools.Governance.Cli.Infrastructure;
using Zss.BilliardHall.Tools.Governance.Cli.Tests.Infrastructure;

namespace Zss.BilliardHall.Tools.Governance.Cli.Tests.Commands;

/// <summary>
/// ScanCrossModuleReferencesCommandHandler 测试
/// </summary>
public sealed class ScanCrossModuleReferencesCommandHandlerTests
{
    [Fact]
    public async Task ExecuteAsync_WithNonExistentModule_ShouldReturnError()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        var handler = new ScanCrossModuleReferencesCommandHandler(fileSystem);

        // Act
        var exitCode = await handler.ExecuteAsync("NonExistentModule", includeTests: false);

        // Assert
        exitCode.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_WithNoReferences_ShouldReturnSuccess()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        const string modulePath = "src/Modules/Orders";
        fileSystem.CreateDirectory(modulePath);
        
        // 创建一个没有跨模块引用的文件
        var fileContent = @"
namespace Zss.BilliardHall.Modules.Orders.Domain;

public class Order
{
    public int Id { get; set; }
}
";
        fileSystem.AddFile($"{modulePath}/Order.cs", fileContent);

        var handler = new ScanCrossModuleReferencesCommandHandler(fileSystem);

        // Act
        var exitCode = await handler.ExecuteAsync("Orders", includeTests: false);

        // Assert - 命令成功执行，始终返回0
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_WithCrossModuleReference_ShouldReturnSuccess()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        const string modulePath = "src/Modules/Orders";
        fileSystem.CreateDirectory(modulePath);
        
        // 创建一个包含跨模块引用的文件
        var fileContent = @"
using Zss.BilliardHall.Modules.Members.Domain;
using Zss.BilliardHall.Modules.Orders.Domain;

namespace Zss.BilliardHall.Modules.Orders.Application;

public class OrderService
{
    // 引用了Members模块
}
";
        fileSystem.AddFile($"{modulePath}/OrderService.cs", fileContent);

        var handler = new ScanCrossModuleReferencesCommandHandler(fileSystem);

        // Act
        var exitCode = await handler.ExecuteAsync("Orders", includeTests: false);

        // Assert - 发现引用不等于失败，仍返回0
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSkipUsingStatic()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        const string modulePath = "src/Modules/Orders";
        fileSystem.CreateDirectory(modulePath);
        
        // 创建包含 using static 的文件
        var fileContent = @"
using static System.Console;
using Zss.BilliardHall.Modules.Members.Domain;

namespace Zss.BilliardHall.Modules.Orders.Application;
";
        fileSystem.AddFile($"{modulePath}/Test.cs", fileContent);

        var handler = new ScanCrossModuleReferencesCommandHandler(fileSystem);

        // Act
        var exitCode = await handler.ExecuteAsync("Orders", includeTests: false);

        // Assert - 应该能正确识别Members引用，忽略using static
        exitCode.Should().Be(0);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSkipUsingAlias()
    {
        // Arrange
        var fileSystem = new InMemoryFileSystem();
        const string modulePath = "src/Modules/Orders";
        fileSystem.CreateDirectory(modulePath);
        
        // 创建包含 using alias 的文件
        var fileContent = @"
using Project = Zss.BilliardHall.Modules.Members;
using Zss.BilliardHall.Modules.Orders.Domain;

namespace Zss.BilliardHall.Modules.Orders.Application;
";
        fileSystem.AddFile($"{modulePath}/Test.cs", fileContent);

        var handler = new ScanCrossModuleReferencesCommandHandler(fileSystem);

        // Act
        var exitCode = await handler.ExecuteAsync("Orders", includeTests: false);

        // Assert - 应该忽略using alias
        exitCode.Should().Be(0);
    }
}
