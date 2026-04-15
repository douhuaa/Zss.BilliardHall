using NetArchTest.Rules;
using Xunit;
using Zss.BilliardHall.Composition;
using Zss.BilliardHall.Modules.Members;
using Zss.BilliardHall.Modules.Members.Domain;
using Zss.BilliardHall.Modules.Orders;
using Zss.BilliardHall.Platform.Errors;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Zss.BilliardHall.Tests.ArchitectureTests;

[Collection(ErrorRegistryCollection.Name)]
public class ErrorHandlingTests
{
    [Fact]
    public void All_Discovered_ErrorCodes_Must_Be_Registered_By_ErrorModules()
    {
        RunWithIsolatedErrorRegistry(() =>
        {
            RegisterAllErrorModules();

            var errorCodeTypes = GetErrorCodeTypesFromDiscoveredAssemblies();
            var allCodes = errorCodeTypes.SelectMany(GetErrorCodes).Distinct(StringComparer.Ordinal).ToArray();
            var missingCodes = allCodes.Where(code => !ErrorRegistry.Contains(code)).ToArray();

            Assert.True(
                missingCodes.Length == 0,
                $"存在未注册错误码：{string.Join(", ", missingCodes)}");
        });
    }

    [Fact]
    public void Discovered_ErrorModules_Registration_Must_Not_Conflict()
    {
        RunWithIsolatedErrorRegistry(() =>
        {
            var ex = Record.Exception(RegisterAllErrorModules);

            Assert.Null(ex);

            var descriptors = ErrorRegistry.All.ToArray();
            var uniqueCodes = descriptors.Select(d => d.Code).Distinct(StringComparer.Ordinal).Count();
            Assert.Equal(uniqueCodes, descriptors.Length);
        });
    }

    [Fact]
    public void HostBootstrapper_Must_Use_GlobalExceptionMiddleware()
    {
        var repositoryRoot = FindRepositoryRoot();
        var hostBootstrapperPath = Path.Combine(repositoryRoot, "src", "Host", "Web", "HostBootstrapper.cs");

        Assert.True(File.Exists(hostBootstrapperPath), $"未找到文件：{hostBootstrapperPath}");

        var source = File.ReadAllText(hostBootstrapperPath);
        Assert.Contains("UseMiddleware<GlobalExceptionMiddleware>()", source, StringComparison.Ordinal);
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

    private static void RegisterAllErrorModules()
    {
        var moduleTypes = GetRelevantAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(IErrorModule).IsAssignableFrom(t))
            .ToArray();

        Assert.NotEmpty(moduleTypes);

        foreach (var moduleType in moduleTypes)
        {
            var module = Activator.CreateInstance(moduleType) as IErrorModule;
            Assert.NotNull(module);
            module!.Register();
        }
    }

    private static IEnumerable<Type> GetErrorCodeTypesFromDiscoveredAssemblies()
    {
        return GetRelevantAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: true, IsSealed: true } &&
                        t.Name.EndsWith("ErrorCodes", StringComparison.Ordinal));
    }

    private static IEnumerable<Assembly> GetRelevantAssemblies()
    {
        var configuration = new ConfigurationBuilder().Build();
        var moduleAssemblies = ModuleComposition.GetEnabledModules(configuration)
            .Select(m => m.GetType().Assembly);

        return moduleAssemblies
            .Append(typeof(PlatformErrorModule).Assembly)
            .Distinct();
    }

    private static IReadOnlyList<string> GetErrorCodes(Type errorCodeType)
    {
        return errorCodeType
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string))
            .Select(f => f.GetValue(null)?.ToString())
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Cast<string>()
            .ToList();
    }

    private static void RunWithIsolatedErrorRegistry(Action testAction)
    {
        ErrorRegistry.ResetForTesting();
        try
        {
            testAction();
        }
        finally
        {
            ErrorRegistry.ResetForTesting();
        }
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            var solution = Path.Combine(current.FullName, "Zss.BilliardHall.slnx");
            if (File.Exists(solution))
                return current.FullName;

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("未找到仓库根目录（包含 Zss.BilliardHall.slnx）。");
    }
}
