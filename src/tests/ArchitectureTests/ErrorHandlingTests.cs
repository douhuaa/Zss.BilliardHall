using NetArchTest.Rules;
using Xunit;
using Zss.BilliardHall.Modules.Orders;
using Zss.BilliardHall.Platform.Errors;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

public class ErrorHandlingTests
{
    [Fact]
    public void Orders_ErrorCodes_Must_Be_Registered()
    {
        // Assemble
        // Ensure registry is populated (simulate startup)
        var module = new OrdersErrorModule();
        // We need a clean registry or just adding to it.
        // Since ErrorRegistry is static, tests might interfere.
        // Architecture tests usually inspect structure provided by types, not runtime state.
        // BUT the user's test `Assert.NotNull(ErrorRegistry.Get(code))` REQUIRES registration.

        // Setup:
        try
        {
             module.Register();
        }
        catch (InvalidOperationException)
        {
            // Already registered in another test or context, fine.
        }

        var codes = typeof(OrdersErrorCodes)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly)
            .Select(f => f.GetValue(null)?.ToString())
            .Where(c => c != null)
            .Cast<string>()
            .ToList();

        // Act & Assert
        foreach (var code in codes)
        {
            // If checking runtime registration:
            var error = ErrorRegistry.Get(code);
            Assert.NotNull(error);
            Assert.Equal(code, error.Code);
        }
    }

    [Fact]
    public void Orders_Must_Not_Depend_On_Other_Modules_ErrorCodes()
    {
        // Ensure Orders does not depend on Members (if it exists) or others.
        // Currently we only have Orders and Members.

        var result = Types.InAssembly(typeof(OrdersErrorCodes).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Zss.BilliardHall.Modules.Members")
            .GetResult();

        Assert.True(result.IsSuccessful, "Orders module should not depend on Members module.");
    }
}

