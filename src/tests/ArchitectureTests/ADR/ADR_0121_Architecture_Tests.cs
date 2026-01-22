using NetArchTest.Rules;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0121: 契约（Contract）命名规范
/// 验证契约接口和 DTO 命名符合规范
/// </summary>
public sealed class ADR_0121_Architecture_Tests
{
    [Fact(DisplayName = "ADR-0121.1: 查询接口必须以 Queries 结尾（复数）")]
    public void Query_Interfaces_Must_End_With_Queries_Plural()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        
        var queryInterfaces = platformAssembly.GetTypes()
            .Where(t => t.IsInterface)
            .Where(t => t.Namespace?.Contains(".Contracts") == true)
            .Where(t => t.Name.Contains("Query") || t.Name.Contains("Queries"))
            .Where(t => t.Name != "IQuery" && t.Name != "IContract") // 排除标记接口
            .ToList();

        var violations = new List<string>();

        foreach (var iface in queryInterfaces)
        {
            if (iface.Name.Contains("Query") && !iface.Name.EndsWith("Queries"))
            {
                violations.Add($"{iface.FullName}");
            }
        }

        Assert.True(violations.Count == 0,
            $"❌ ADR-0121 违规：以下查询接口命名不符合规范：\n" +
            $"{string.Join("\n", violations)}\n" +
            $"正确格式：I{{实体}}Queries（使用复数）\n" +
            $"示例：IMemberQueries, IOrderQueries\n" +
            $"修复建议：将接口名从 Query 改为 Queries（复数形式）。");
    }

    [Fact(DisplayName = "ADR-0121.2: 查询接口必须继承 IQuery 标记接口")]
    public void Query_Interfaces_Must_Implement_IQuery()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        
        var iQueryType = platformAssembly.GetTypes()
            .FirstOrDefault(t => t.IsInterface && t.Name == "IQuery");
        
        if (iQueryType == null) return; // IQuery 不存在时跳过

        var queryInterfaces = platformAssembly.GetTypes()
            .Where(t => t.IsInterface)
            .Where(t => t.Namespace?.Contains(".Contracts") == true)
            .Where(t => t.Name.EndsWith("Queries"))
            .ToList();

        var violations = new List<string>();

        foreach (var iface in queryInterfaces)
        {
            if (!iQueryType.IsAssignableFrom(iface))
            {
                violations.Add($"{iface.FullName}");
            }
        }

        Assert.True(violations.Count == 0,
            $"❌ ADR-0121 违规：以下查询接口未继承 IQuery：\n" +
            $"{string.Join("\n", violations)}\n" +
            $"修复建议：确保所有查询接口继承 IQuery 标记接口。\n" +
            $"示例：public interface IMemberQueries : IQuery");
    }

    [Fact(DisplayName = "ADR-0121.3: 契约 DTO 必须实现 IContract 标记接口")]
    public void Contract_DTOs_Must_Implement_IContract()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        
        var iContractType = platformAssembly.GetTypes()
            .FirstOrDefault(t => t.IsInterface && t.Name == "IContract");
        
        if (iContractType == null) return; // IContract 不存在时跳过

        var dtoTypes = platformAssembly.GetTypes()
            .Where(t => t.Namespace?.Contains(".Contracts") == true)
            .Where(t => (t.IsClass || t.IsValueType) && !t.IsInterface)
            .Where(t => t.Name.EndsWith("Dto"))
            .ToList();

        var violations = new List<string>();

        foreach (var dto in dtoTypes)
        {
            if (!iContractType.IsAssignableFrom(dto))
            {
                violations.Add($"{dto.FullName}");
            }
        }

        Assert.True(violations.Count == 0,
            $"❌ ADR-0121 违规：以下契约 DTO 未实现 IContract：\n" +
            $"{string.Join("\n", violations)}\n" +
            $"修复建议：确保所有契约 DTO 实现 IContract 标记接口。\n" +
            $"示例：public record MemberDto : IContract");
    }

    [Fact(DisplayName = "ADR-0121.4: 契约接口禁止使用 Service 后缀")]
    public void Contract_Interfaces_Should_Not_Use_Service_Suffix()
    {
        var platformAssembly = typeof(Platform.PlatformBootstrapper).Assembly;
        
        var result = Types.InAssembly(platformAssembly)
            .That()
            .ResideInNamespaceContaining("Contracts")
            .And()
            .AreInterfaces()
            .ShouldNot()
            .HaveNameEndingWith("Service")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0121 违规：契约接口不应使用 Service 后缀。\n" +
            $"违规类型：{string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}\n" +
            $"正确做法：使用 Queries 后缀表示查询接口\n" +
            $"示例：IMemberQueries 而非 IMemberService\n" +
            $"修复建议：将接口名从 Service 改为 Queries。");
    }
}
