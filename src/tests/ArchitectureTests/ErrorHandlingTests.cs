using NetArchTest.Rules;
using Xunit;
using Zss.BilliardHall.Modules.Orders;
using Zss.BilliardHall.Platform.Errors;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

public class ErrorHandlingTests
{
    /// <summary>
    /// 通过反射验证 OrdersErrorCodes 中声明的所有常量均在 OrdersErrorModule.Register() 中有对应的注册调用，
    /// 避免使用静态 ErrorRegistry 导致测试间相互干扰。
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

        // Act: 获取 OrdersErrorModule.Register() 方法体的 IL 字节，检查每个错误码是否出现在方法中
        // 通过读取方法实现，将其序列化为字符串，然后检查每个错误码是否存在
        // 更简单的方式：用临时隔离的注册表执行注册，检查所有码均已注册
        var tempErrors = new System.Collections.Generic.HashSet<string>();
        var originalRegister = typeof(ErrorRegistry)
            .GetMethod(nameof(ErrorRegistry.Register), BindingFlags.Public | BindingFlags.Static)!;

        // 由于 ErrorRegistry 是静态的，这里通过反射读取 OrdersErrorModule 的注册方法内嵌的常量引用
        // 用以验证每个 OrdersErrorCodes 常量都在 OrdersErrorModule 中被引用
        var moduleAssembly = typeof(OrdersErrorModule).Assembly;
        var moduleIl = typeof(OrdersErrorModule)
            .GetMethod(nameof(IErrorModule.Register))!
            .GetMethodBody()!
            .GetILAsByteArray()!;

        // 将 IL 字节转换为字符串以便简单搜索不适用；改为解析 LocalVariables 和字符串引用
        // 最可靠的架构验证：通过检查 OrdersErrorModule 的元数据确认其注册了所有错误码
        // 这里改用：在隔离方式下运行注册（仅当未冻结时），通过 internal reset 或反射
        // 由于 ErrorRegistry 不暴露 Reset，改用对 _errors 字典的反射访问实现测试隔离

        var errorsField = typeof(ErrorRegistry)
            .GetField("_errors", BindingFlags.NonPublic | BindingFlags.Static)!;
        var frozenField = typeof(ErrorRegistry)
            .GetField("_frozen", BindingFlags.NonPublic | BindingFlags.Static)!;

        var dict = (System.Collections.Concurrent.ConcurrentDictionary<string, ErrorDescriptor>)errorsField.GetValue(null)!;
        var wasFrozen = (bool)frozenField.GetValue(null)!;

        // 临时解冻以注册测试数据（仅在测试中使用反射访问）
        frozenField.SetValue(null, false);
        try
        {
            var module = new OrdersErrorModule();
            foreach (var code in codes)
            {
                dict.TryRemove(code, out _);
            }
            module.Register();

            // Assert: 所有 OrdersErrorCodes 常量都已注册
            foreach (var code in codes)
            {
                Assert.True(dict.ContainsKey(code), $"错误码 '{code}' 未在 OrdersErrorModule 中注册。");
            }
        }
        finally
        {
            // 恢复原始冻结状态并清理测试数据
            foreach (var code in codes)
            {
                dict.TryRemove(code, out _);
            }
            frozenField.SetValue(null, wasFrozen);
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


