using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

/// <summary>
/// Platform 层约束测试
/// 确保 Platform 只提供技术能力，不包含业务逻辑
/// </summary>
public class PlatformLayerTests
{
    private static readonly Assembly PlatformAssembly = typeof(Platform.PlatformDefaults).Assembly;

    [Fact]
    public void Platform_Should_Not_Contain_Business_Named_Types()
    {
        // Platform 不应包含业务相关的命名
        var businessKeywords = new[] 
        { 
            "Member", "Order", "Payment", "Billing", "Invoice", 
            "Customer", "Product", "Reservation", "Table" 
        };

        foreach (var keyword in businessKeywords)
        {
            var result = Types.InAssembly(PlatformAssembly)
                .That().DoNotResideInNamespace("Zss.BilliardHall.Platform.Contracts")
                .ShouldNot().HaveNameMatching($".*{keyword}.*")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Platform 层不应包含业务相关类型名称: {keyword}。" +
                $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}。" +
                $"修复建议：Platform 只提供技术能力（日志、事务、序列化等），业务逻辑应放在模块中。");
        }
    }

    [Fact]
    public void Platform_Should_Only_Contain_Technical_Capabilities()
    {
        // Platform 应该只包含技术相关的命名
        var allowedPrefixes = new[]
        {
            "Platform", "Configuration", "Logging", "Transaction", "Exception",
            "Serialization", "Validation", "Http", "Database", "Messaging",
            "Contract", "Query", "Event", "Infrastructure", "Extension"
        };

        var types = Types.InAssembly(PlatformAssembly)
            .That().AreClasses()
            .Or().AreInterfaces()
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<")) // 排除编译器生成的类型
            .ToList();

        foreach (var type in types)
        {
            var hasAllowedPrefix = allowedPrefixes.Any(prefix => 
                type.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) ||
                type.Namespace?.Contains("Contracts") == true);

            if (!hasAllowedPrefix && !type.Name.StartsWith("I")) // I 开头的接口也允许
            {
                Assert.Fail(
                    $"Platform 层类型 {type.FullName} 命名不符合技术能力规范。" +
                    $"修复建议：Platform 类型应使用技术相关命名（如 *Configuration, *Logger, *Serializer 等），" +
                    $"或放在 Contracts 命名空间中。");
            }
        }
    }

    [Fact]
    public void Platform_Should_Not_Reference_Any_Modules()
    {
        // Platform 不应引用任何业务模块
        var result = Types.InAssembly(PlatformAssembly)
            .ShouldNot().HaveDependencyOnAny(
                ModuleAssemblyData.ModuleNames.Select(m => $"Zss.BilliardHall.Modules.{m}").ToArray())
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Platform 层不应依赖任何业务模块。" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}。" +
            $"修复建议：Platform 是基础层，应该被模块依赖，而不是依赖模块。");
    }

    [Fact]
    public void Platform_Should_Not_Reference_Application_Or_Host()
    {
        // Platform 不应引用 Application 或 Host
        var result = Types.InAssembly(PlatformAssembly)
            .ShouldNot().HaveDependencyOnAny(
                "Zss.BilliardHall.Application",
                "Zss.BilliardHall.Host",
                "Zss.BilliardHall.WebHost",
                "Zss.BilliardHall.Worker")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"Platform 层不应依赖 Application 或 Host 层。" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}。" +
            $"修复建议：依赖关系应该是 Host -> Application -> Modules -> Platform，Platform 是最底层。");
    }

    [Fact]
    public void Contracts_Should_Be_Simple_Data_Structures()
    {
        // 契约应该是简单的数据结构，不包含业务方法
        var contractTypes = Types.InAssembly(PlatformAssembly)
            .That().ResideInNamespace("Zss.BilliardHall.Platform.Contracts")
            .And().AreClasses()
            .GetTypes();

        foreach (var contractType in contractTypes)
        {
            // 排除标记接口和抽象类
            if (contractType.IsAbstract || contractType.IsInterface)
                continue;

            var publicMethods = contractType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName) // 排除属性的 get/set 方法
                .Where(m => m.DeclaringType == contractType) // 只检查当前类型声明的方法
                .ToList();

            Assert.True(publicMethods.Count == 0,
                $"契约类型 {contractType.FullName} 包含业务方法: {string.Join(", ", publicMethods.Select(m => m.Name))}。" +
                $"修复建议：契约应该是简单的数据结构（DTO/Record），只包含属性，不包含业务逻辑方法。");
        }
    }
}
