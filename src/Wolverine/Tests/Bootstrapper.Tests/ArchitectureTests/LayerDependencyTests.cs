using NetArchTest.Rules;
using Zss.BilliardHall.Modules.Members;

namespace Zss.BilliardHall.Wolverine.Bootstrapper.Tests.ArchitectureTests;

public class LayerDependencyTests
{
    [Fact]
    public void Platform_Should_Not_Depend_On_Application()
    {
        var result = Types
            .InAssembly(typeof(PlatformDefaults).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Zss.BilliardHall.Application")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    public class HandlerConventionsTests
    {
        [Fact]
        public void Handlers_Should_Have_Transactional_On_AtLeastOneMethod()
        {
            var assembly = typeof(Member).Assembly;

            var handlers = assembly.GetTypes().Where(t => t.Name.EndsWith("Handler")).ToArray();

            var missing = new List<string>();

            foreach (var handler in handlers)
            {
                var methods = handler.GetMethods(
                    System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.NonPublic
                        | System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.Static
                );

                var hasTransactional = methods.Any(m =>
                    m.GetCustomAttributes(false)
                        .Any(a =>
                            a.GetType().Name == "TransactionalAttribute"
                            || a.GetType().Name == "Transactional"
                        )
                );

                if (!hasTransactional)
                    missing.Add(handler.FullName ?? handler.Name);
            }

            Assert.True(
                missing.Count == 0,
                $"Handlers missing [Transactional] attribute on any method: {string.Join(", ", missing)}"
            );
        }
    }

    [Fact]
    public void Application_Should_Not_Depend_On_Host()
    {
        var result = Types
            .InAssembly(typeof(ApplicationOptions).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Zss.BilliardHall.Host")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}

public class HostBoundaryTests
{
    [Fact]
    public void Host_Should_Not_Reference_Domain_Entities_Directly()
    {
        var result = Types
            .InAssembly(typeof(HttpHost).Assembly)
            .ShouldNot()
            .HaveDependencyOn("Zss.BilliardHall.Domain")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}

public class ApplicationBoundaryTests
{
    [Fact]
    public void Handlers_Should_Not_Depend_On_AspNet()
    {
        var result = Types
            .InAssembly(typeof(Member).Assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Or()
            .HaveNameEndingWith("Command")
            .Or()
            .HaveNameEndingWith("Aggregate")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Endpoints_Should_Depend_On_AspNet()
    {
        var result = Types
            .InAssembly(typeof(Member).Assembly)
            .That()
            .HaveNameEndingWith("Endpoint")
            .Should()
            .HaveDependencyOn("Microsoft.AspNetCore")
            .GetResult();

        Assert.True(result.IsSuccessful);
    }
}

public class VerticalSliceRulesTests
{
    private static readonly Type AssemblyMarker = typeof(Program);

    [Fact]
    public void Handlers_Should_Reside_In_Features_Namespace()
    {
        var result = Types
            .InAssembly(AssemblyMarker.Assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .Should()
            .ResideInNamespaceContaining("Features") // 要求 Handler 位于 Features.* 下
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            FormatFailMessage("Handler 应位于 Features 命名空间", result)
        );
    }

    [Fact]
    public void Application_And_Domain_Should_Not_Reside_In_Infrastructure()
    {
        var result = Types
            .InAssembly(AssemblyMarker.Assembly)
            .That()
            .ResideInNamespaceContaining("Application") // 匹配 Application 层命名空间
            .Or()
            .ResideInNamespaceContaining("Domain") // 或 Domain 层
            .ShouldNot()
            .ResideInNamespaceContaining("Infrastructure") // 禁止依赖或混入 Infrastructure 命名空间
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            FormatFailMessage("Application/Domain 层不应在 Infrastructure 命名空间下出现", result)
        );
    }

    [Fact]
    public void Repositories_Should_Reside_In_Infrastructure_Persistence()
    {
        var result = Types
            .InAssembly(AssemblyMarker.Assembly)
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .ResideInNamespaceContaining("Infrastructure") // 可更严格地改为 "Infrastructure.Persistence"
            .GetResult();

        Assert.True(
            result.IsSuccessful,
            FormatFailMessage(
                "Repository 类型应位于 Infrastructure（或 Infrastructure.Persistence）",
                result
            )
        );
    }



    // 帮助打印失败信息，列出违规类型
    private static string FormatFailMessage(string title, NetArchTest.Rules.TestResult result)
    {
        if (result.IsSuccessful)
            return title + "：通过";
        var failing = result.FailingTypes.Select(t =>
            $"{t.FullName} ({t.Assembly.GetName().Name})"
        );
        return $"{title}：失败，以下类型未通过断言：\n" + string.Join("\n", failing);
    }
}
