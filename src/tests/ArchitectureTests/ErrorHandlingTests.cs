using NetArchTest.Rules;
using Xunit;
using Zss.BilliardHall.Modules.Orders;
using Zss.BilliardHall.Platform.Errors;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

public class ErrorHandlingTests
{
    /// <summary>
    /// 验证 OrdersErrorCodes 中声明的所有常量均在 OrdersErrorModule 中被注册。
    /// 使用 ErrorRegistry.ResetForTesting() 实现测试隔离，避免静态状态干扰。
    /// </summary>
    [Fact]
    public void Orders_ErrorCodes_Must_Be_Registered()
    {
        // Arrange: 获取 OrdersErrorCodes 中所有公共常量字符串
        var codes = typeof(OrdersErrorCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly)
            .Select(f => f.GetValue(null)?.ToString())
            .Where(c => c != null)
            .Cast<string>()
            .ToList();

        // 重置为干净状态，确保测试隔离
        ErrorRegistry.ResetForTesting();
        try
        {
            new OrdersErrorModule().Register();

            // Assert: 所有 OrdersErrorCodes 常量均已在 OrdersErrorModule 中注册
            foreach (var code in codes)
            {
                var descriptor = ErrorRegistry.Get(code);
                Assert.NotNull(descriptor);
                Assert.Equal(code, descriptor.Code);
            }
        }
        finally
        {
            ErrorRegistry.ResetForTesting();
        }
    }

    [Fact]
    public void Orders_Must_Not_Depend_On_Members_Module()
    {
        var result = Types.InAssembly(typeof(OrdersErrorCodes).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Zss.BilliardHall.Modules.Members")
            .GetResult();

        Assert.True(result.IsSuccessful, "Orders module should not depend on Members module.");
    }
}



