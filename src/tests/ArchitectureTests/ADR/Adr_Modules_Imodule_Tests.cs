namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

public sealed class Adr_Modules_Imodule_Tests
{
    [Theory(DisplayName = "Modules: 每个模块程序集必须且只能有一个 IModule 实现")]
    [MemberData(nameof(AllModuleAssemblies))]
    public void Each_Module_Assembly_Should_Have_Exactly_One_IModule(Assembly moduleAssembly)
    {
        var moduleTypes = SafeGetTypes(moduleAssembly)
            .Where(t => t is { IsAbstract: false, IsInterface: false } && typeof(IModule).IsAssignableFrom(t))
            .ToArray();

        moduleTypes.Length.Should().Be(
        1,
        $"模块程序集 {moduleAssembly.GetName().Name} 必须且只能有一个 IModule。当前发现：\n{string.Join('\n', moduleTypes.Select(t => " - " + t.FullName))}");
    }

    public static IEnumerable<object[]> AllModuleAssemblies()
        => ModuleAssemblyData.ModuleAssemblies.Select(a => new object[] { a });

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try { return assembly.GetTypes(); }
        catch (ReflectionTypeLoadException ex) { return ex.Types.Where(t => t is not null)!; }
    }
}
public sealed class Adr_Modules_NoMagicScan_Tests
{
    [Theory(DisplayName = "ApplicationBootstrapper 禁止通过目录扫描加载 Modules.*.dll（必须显式 Modules:Assemblies）")]
    [InlineData("EnumerateFiles")]
    [InlineData("SearchOption.TopDirectoryOnly")]
    [InlineData("Zss.BilliardHall.Modules.*.dll")]
    public void ApplicationBootstrapper_Should_Not_Contain_Magic_Scan_Tokens(string forbiddenToken)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..",
        "src", "Application", "ApplicationBootstrapper.cs");

        // 如果你们测试项目的工作目录不同，可改为 TestEnvironment/RepositoryRoot（看你们 Shared 里是否已有）
        File.Exists(path).Should().BeTrue($"测试需要能读取源码文件：{path}");

        var content = File.ReadAllText(path);
        content.Should().NotContain(forbiddenToken, "模块加载必须显式声明，禁止目录扫描兜底");
    }
}
