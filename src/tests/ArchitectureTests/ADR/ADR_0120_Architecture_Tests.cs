using NetArchTest.Rules;
using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0120: 功能切片命名规范
/// 验证 Command/Query/Handler/Endpoint/DTO 命名符合规范
/// </summary>
public sealed class ADR_0120_Architecture_Tests
{
    [Theory(DisplayName = "ADR-0120.1: Command 类必须以 Command 结尾")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Commands_Must_End_With_Command(Assembly moduleAssembly)
    {
        var types = moduleAssembly.GetTypes()
            .Where(t => t.Namespace?.Contains(".Features.") == true)
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.Contains("Command") && !t.Name.Contains("Handler"))
            .ToList();

        var violations = new List<string>();

        foreach (var type in types)
        {
            if (!type.Name.EndsWith("Command"))
            {
                violations.Add($"{type.FullName}");
            }
        }

        Assert.True(violations.Count == 0,
            $"❌ ADR-0120 违规：以下 Command 类命名不符合规范：\n" +
            $"{string.Join("\n", violations)}\n" +
            $"正确格式：{{动词}}{{实体}}Command\n" +
            $"示例：CreateMemberCommand, UpdateMemberProfileCommand\n" +
            $"修复建议：确保 Command 类名以 'Command' 结尾。");
    }

    [Theory(DisplayName = "ADR-0120.2: Query 类必须以 Query 结尾")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Queries_Must_End_With_Query(Assembly moduleAssembly)
    {
        var types = moduleAssembly.GetTypes()
            .Where(t => t.Namespace?.Contains(".Features.") == true)
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.Contains("Query") && !t.Name.Contains("Handler"))
            .ToList();

        var violations = new List<string>();

        foreach (var type in types)
        {
            if (!type.Name.EndsWith("Query"))
            {
                violations.Add($"{type.FullName}");
            }
        }

        Assert.True(violations.Count == 0,
            $"❌ ADR-0120 违规：以下 Query 类命名不符合规范：\n" +
            $"{string.Join("\n", violations)}\n" +
            $"正确格式：{{动词}}{{实体}}{{限定条件}}Query\n" +
            $"示例：GetMemberByIdQuery, ListActiveMembersQuery\n" +
            $"修复建议：确保 Query 类名以 'Query' 结尾。");
    }

    [Theory(DisplayName = "ADR-0120.3: Handler 类必须以 Handler 结尾")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Handlers_Must_End_With_Handler(Assembly moduleAssembly)
    {
        var types = moduleAssembly.GetTypes()
            .Where(t => t.Namespace?.Contains(".Features.") == true)
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.Contains("Handler"))
            .ToList();

        var violations = new List<string>();

        foreach (var type in types)
        {
            if (!type.Name.EndsWith("Handler"))
            {
                violations.Add($"{type.FullName}");
            }
        }

        Assert.True(violations.Count == 0,
            $"❌ ADR-0120 违规：以下 Handler 类命名不符合规范：\n" +
            $"{string.Join("\n", violations)}\n" +
            $"正确格式：{{Command/Query名}}Handler\n" +
            $"示例：CreateMemberCommandHandler, GetMemberByIdQueryHandler\n" +
            $"修复建议：确保 Handler 类名以 'Handler' 结尾。");
    }

    [Theory(DisplayName = "ADR-0120.4: 禁止在 Features 中使用 Service 后缀")]
    [ClassData(typeof(ModuleAssemblyData))]
    public void Features_Should_Not_Use_Service_Suffix(Assembly moduleAssembly)
    {
        var result = Types.InAssembly(moduleAssembly)
            .That()
            .ResideInNamespaceContaining("Features")
            .ShouldNot()
            .HaveNameEndingWith("Service")
            .GetResult();

        Assert.True(result.IsSuccessful,
            $"❌ ADR-0120 违规：在垂直切片架构中禁止使用 Service 后缀。\n" +
            $"违规类型：{string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? Array.Empty<string>())}\n" +
            $"正确做法：使用 Handler 处理业务逻辑\n" +
            $"示例：CreateMemberCommandHandler 而非 MemberService\n" +
            $"修复建议：将 Service 重构为对应用例的 Handler。");
    }
}
