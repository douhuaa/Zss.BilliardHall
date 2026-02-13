namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

public sealed class Adr_Modules_NoMagicScan_Tests
{
    [Theory(DisplayName = "ApplicationBootstrapper 禁止通过目录扫描加载 Modules.*.dll（必须显式 Modules:Assemblies）")]
    [InlineData("EnumerateFiles")]
    [InlineData("SearchOption.TopDirectoryOnly")]
    [InlineData("Zss.BilliardHall.Modules.*.dll")]
    public void ApplicationBootstrapper_Should_Not_Contain_Magic_Scan_Tokens(string forbiddenToken)
    {
        var path = Path.Combine(TestEnvironment.SourceRoot, "Application", "ApplicationBootstrapper.cs");

        // 如果你们测试项目的工作目录不同，可改为 TestEnvironment/RepositoryRoot（看你们 Shared 里是否已有）
        File.Exists(path).Should().BeTrue($"测试需要能读取源码文件：{path}");

        var content = File.ReadAllText(path);
        content.Should().NotContain(forbiddenToken, "模块加载必须显式声明，禁止目录扫描兜底");
    }
}
