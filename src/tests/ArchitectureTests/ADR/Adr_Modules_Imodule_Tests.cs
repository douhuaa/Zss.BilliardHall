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
