using NetArchTest.Rules;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0001: 模块化单体与垂直切片架构决策
/// 验证模块隔离、垂直切片、契约使用等核心架构约束
/// </summary>
public sealed class ADR_0001_Architecture_Tests
{
    #region 1. 模块隔离约束

    [Theory(DisplayName = "ADR-0001.1: 模块不应相互引用")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Reference_Other_Modules(Assembly moduleAssembly)
    {
        var moduleName = moduleAssembly.GetName().Name!.Split('.').Last();
        foreach (var other in ModuleAssemblyData.ModuleNames.Where(m => m != moduleName))
        {
            var result = Types.InAssembly(moduleAssembly)
                .ShouldNot()
                .HaveDependencyOn($"Zss.BilliardHall.Modules.{other}")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"❌ ADR-0001 违规: 模块 {moduleName} 不应依赖模块 {other}。\n" +
                $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
                $"修复建议：将共享逻辑移至 Platform/BuildingBlocks，或改为消息通信（Publish/Invoke），或由 Bootstrapper/Coordinator 做模块级协调。");
        }
    }

    [Theory(DisplayName = "ADR-0001.2: 模块项目文件不应引用其他模块")]
    [MemberData(nameof(GetModuleProjectFiles))]
    public void Module_Csproj_Should_Not_Reference_Other_Modules(string csprojPath)
    {
        var allowedProjectNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Zss.BilliardHall.Platform",
            "Zss.BilliardHall.BuildingBlocks",
        };

        var doc = new System.Xml.XmlDocument();
        doc.Load(csprojPath);
        var mgr = new System.Xml.XmlNamespaceManager(doc.NameTable);
        mgr.AddNamespace("msb", doc.DocumentElement!.NamespaceURI);

        var projectName = Path.GetFileNameWithoutExtension(csprojPath);
        var references = doc.SelectNodes("//msb:ProjectReference", mgr);
        if (references == null) return;

        foreach (System.Xml.XmlNode reference in references)
        {
            var include = reference?.Attributes?["Include"]?.Value;
            if (string.IsNullOrEmpty(include)) continue;

            var refPath = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(csprojPath)!, include));
            var refName = Path.GetFileNameWithoutExtension(refPath);

            if (string.Equals(refName, projectName, StringComparison.OrdinalIgnoreCase))
                continue;

            if (allowedProjectNames.Contains(refName))
                continue;

            Assert.Fail(
                $"❌ ADR-0001 违规: 模块 {projectName} 不应引用其他模块或非白名单项目: {refName}。\n" +
                $"项目路径: {csprojPath}\n" +
                $"引用路径: {include}\n" +
                $"修复建议：将共享代码移至 Platform/BuildingBlocks，或改用消息通信（Publish/Invoke）。");
        }
    }

    public static IEnumerable<object[]> GetModuleProjectFiles()
    {
        var root = ModuleIsolationTests.GetSolutionRoot();
        var modulesDir = Path.Combine(root, "src", "Modules");
        if (!Directory.Exists(modulesDir)) yield break;

        var csprojs = Directory.GetFiles(modulesDir, "*.csproj", SearchOption.AllDirectories);
        foreach (var csproj in csprojs)
        {
            yield return new object[] { csproj };
        }
    }

    #endregion

    #region 2. 垂直切片架构约束

    [Theory(DisplayName = "ADR-0001.3: 模块不应包含传统分层命名空间")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Contain_Traditional_Layering_Namespaces(Assembly moduleAssembly)
    {
        var forbidden = new[] { ".Application", ".Domain", ".Infrastructure", ".Repository", ".Service", ".Shared", ".Common" };
        foreach (var ns in forbidden)
        {
            var result = Types.InAssembly(moduleAssembly)
                .That().ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
                .ShouldNot().ResideInNamespaceContaining(ns)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"❌ ADR-0001 违规: 模块 {moduleAssembly.GetName().Name} 禁止出现命名空间: {ns}。\n" +
                $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
                $"修复建议：将相关代码移回模块切片内部或抽象到 Platform（仅当满足 BuildingBlocks 准入规则）。");
        }
    }

    [Theory(DisplayName = "ADR-0001.4: 模块不应包含 Repository/Service 等横向抽象")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Not_Contain_Repository_Or_Service_Semantics(Assembly moduleAssembly)
    {
        var forbidden = new[] { "Repository", "Service", "Manager", "Store" };
        foreach (var word in forbidden)
        {
            var result = Types.InAssembly(moduleAssembly)
                .That().ResideInNamespaceStartingWith("Zss.BilliardHall.Modules")
                .ShouldNot().HaveNameMatching($".*{word}.*")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"❌ ADR-0001 违规: 模块 {moduleAssembly.GetName().Name} 禁止出现类型名包含: {word}。\n" +
                $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
                $"修复建议：把实现放到模块内的领域/领域服务或改为消息交互。");
        }
    }

    [Theory(DisplayName = "ADR-0001.5: Handler 应该自包含，不依赖横向 Service")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Should_Not_Depend_On_Horizontal_Services(Assembly moduleAssembly)
    {
        var handlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            var dependencies = handler.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.Name.EndsWith("Service") &&
                           t.Namespace?.StartsWith("Zss.BilliardHall.Modules") == true)
                .ToList();

            Assert.True(dependencies.Count == 0,
                $"❌ ADR-0001 违规: Handler {handler.FullName} 依赖了横向 Service: {string.Join(", ", dependencies.Select(d => d.Name))}。\n" +
                $"修复建议：在垂直切片架构中，Handler 应直接包含业务逻辑，避免抽象为横向 Service。");
        }
    }

    [Theory(DisplayName = "ADR-0001.6: Handler 不应直接调用其他 Handler")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Should_Not_Call_Other_Handlers_Directly(Assembly moduleAssembly)
    {
        var handlers = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("Handler")
            .GetTypes();

        foreach (var handler in handlers)
        {
            var dependencies = handler.GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .Where(t => t.Name.EndsWith("Handler") &&
                           t.Namespace?.StartsWith("Zss.BilliardHall.Modules") == true)
                .ToList();

            Assert.True(dependencies.Count == 0,
                $"❌ ADR-0001 违规: Handler {handler.FullName} 直接依赖了其他 Handler: {string.Join(", ", dependencies.Select(d => d.Name))}。\n" +
                $"修复建议：Handler 之间不应直接调用。如需组合逻辑，使用 Mediator/EventBus 进行解耦。");
        }
    }

    #endregion

    #region 3. 契约使用规则约束

    [Theory(DisplayName = "ADR-0001.7: Command Handler 不应依赖 IQuery 接口")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void CommandHandlers_Should_Not_Depend_On_IQuery_Interfaces(Assembly moduleAssembly)
    {
        var result = Types.InAssembly(moduleAssembly)
            .That().HaveNameEndingWith("CommandHandler")
            .Or().HaveNameEndingWith("Command.Handler")
            .ShouldNot().HaveDependencyOn("Zss.BilliardHall.Platform.Contracts.IQuery")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0001 违规: 模块 {moduleAssembly.GetName().Name} 中的 Command Handler 不应依赖 IQuery 接口。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}。\n" +
            $"修复建议：Command Handler 应通过领域事件、命令或维护本地状态副本来获取必要信息，而不是查询其他模块的契约数据做业务决策。");
    }

    [Theory(DisplayName = "ADR-0001.8: Platform 不应依赖模块契约")]
    [MemberData(nameof(GetPlatformAssembly))]
    public void Platform_Should_Not_Depend_On_Module_Contracts(Assembly platformAssembly)
    {
        var result = Types.InAssembly(platformAssembly)
            .That().ResideInNamespace("Zss.BilliardHall.Platform")
            .And().DoNotResideInNamespace("Zss.BilliardHall.Platform.Contracts")
            .ShouldNot().HaveDependencyOnAny(
                ModuleAssemblyData.ModuleNames.Select(m => $"Zss.BilliardHall.Modules.{m}").ToArray())
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0001 违规: Platform 层不应依赖任何模块的契约或实现。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
            $"修复建议：Platform 只提供技术能力，不应包含业务逻辑或依赖业务契约。");
    }

    public static IEnumerable<object[]> GetPlatformAssembly()
    {
        yield return new object[] { typeof(Platform.PlatformBootstrapper).Assembly };
    }

    #endregion

    #region 4. 模块通信约束

    [Theory(DisplayName = "ADR-0001.9: 模块只能依赖 Platform，不能依赖 Application/Host")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Modules_Should_Only_Depend_On_Platform(Assembly moduleAssembly)
    {
        var result = Types.InAssembly(moduleAssembly)
            .ShouldNot().HaveDependencyOnAny(
                "Zss.BilliardHall.Application",
                "Zss.BilliardHall.Host")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0001 违规: 模块 {moduleAssembly.GetName().Name} 不应依赖 Application 或 Host。\n" +
            $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}。\n" +
            $"修复建议：将需要共享的契约放到 Platform/BuildingBlocks，或通过消息通信解耦。");
    }

    #endregion

    #region 5. Platform 层限制

    [Fact(DisplayName = "ADR-0001.10: Platform 不应包含业务相关命名")]
    public void Platform_Should_Not_Contain_Business_Named_Types()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var businessKeywords = new[]
        {
            "Member", "Order", "Payment", "Billing", "Invoice",
            "Customer", "Product", "Reservation", "Table"
        };

        foreach (var keyword in businessKeywords)
        {
            var result = Types.InAssembly(platformAssembly)
                .That().DoNotResideInNamespace("Zss.BilliardHall.Platform.Contracts")
                .ShouldNot().HaveNameMatching($".*{keyword}.*")
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"❌ ADR-0001 违规: Platform 层不应包含业务相关类型名称: {keyword}。\n" +
                $"违规类型: {string.Join(", ", result.FailingTypes?.Select(t => t.Name) ?? Array.Empty<string>())}。\n" +
                $"修复建议：Platform 只提供技术能力（日志、事务、序列化等），业务逻辑应放在模块中。");
        }
    }

    [Fact(DisplayName = "ADR-0001.11: Contracts 应该是简单的数据结构")]
    public void Contracts_Should_Be_Simple_Data_Structures()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        var contractTypes = Types.InAssembly(platformAssembly)
            .That().ResideInNamespace("Zss.BilliardHall.Platform.Contracts")
            .And().AreClasses()
            .GetTypes();

        foreach (var contractType in contractTypes)
        {
            if (contractType.IsAbstract || contractType.IsInterface)
                continue;

            var publicMethods = contractType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => !m.IsSpecialName)
                .Where(m => m.DeclaringType == contractType)
                .ToList();

            Assert.True(publicMethods.Count == 0,
                $"❌ ADR-0001 违规: 契约类型 {contractType.FullName} 包含业务方法: {string.Join(", ", publicMethods.Select(m => m.Name))}。\n" +
                $"修复建议：契约应该是简单的数据结构（DTO/Record），只包含属性，不包含业务逻辑方法。");
        }
    }

    #endregion
}
