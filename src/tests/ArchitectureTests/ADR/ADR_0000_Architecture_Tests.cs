using System.Reflection;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0000
/// 架构宪法测试：每一条 ADR 文档，必须有唯一、严格对应的架构测试类
/// </summary>
public sealed class ADR_0000_Architecture_Tests
{
    private const string AdrDocsPath = "docs/adr";
    private const string AdrFilePattern = "ADR-*.md";
    private const string TestNamespacePrefix = "Zss.BilliardHall.Tests.ArchitectureTests.ADR";
    private const string TypeSuffix = "_Architecture_Tests";

    /// <summary>
    /// 只以 ADR 编号作为测试类绑定依据：
    /// - 标题/slug 可演进
    /// - 编号不可变
    /// </summary>
    [Fact(DisplayName = "ADR-0000: 每条 ADR 必须有且仅有唯一对应的架构测试类")]
    public void Each_ADR_Must_Have_Exact_And_Unique_Architecture_Test()
    {
        var repoRoot = FindRepositoryRoot() ?? throw new InvalidOperationException("未找到仓库根（docs/adr 或 .git）");
        var adrDirectory = Path.Combine(repoRoot, AdrDocsPath);

        Assert.True(Directory.Exists(adrDirectory), $"未找到 ADR 文档目录：{AdrDocsPath}");

        var adrIds = LoadAdrIds(adrDirectory);
        Assert.NotEmpty(adrIds);

        var testTypes = LoadArchitectureTestTypes();

        var duplicates = adrIds
            .Select(adr => new
            {
                Adr = adr,
                Matches = testTypes
                    .Where(t => string.Equals(t.Name, ToExpectedTypeName(adr), StringComparison.OrdinalIgnoreCase))
                    .Select(t => t.FullName)
                    .ToList()
            })
            .Where(x => x.Matches.Count > 1)
            .ToList();

        var missing = adrIds
            .Where(adr => !testTypes.Any(t =>
                string.Equals(t.Name, ToExpectedTypeName(adr), StringComparison.OrdinalIgnoreCase)))
            .ToList();

        if (duplicates.Any() || missing.Any())
        {
            var messages = new List<string>();

            if (duplicates.Any())
            {
                messages.Add("❌ ADR-0000 架构违规（重复定义）");
                foreach (var d in duplicates)
                {
                    messages.Add($" - {d.Adr} 对应多个测试类型:");
                    messages.AddRange(d.Matches.Select(fullName => $"     • {fullName}"));
                }
            }

            if (missing.Any())
            {
                messages.Add("❌ ADR-0000 架构违规（缺失测试）");
                messages.AddRange(missing.Select(m => $" - {m}"));
            }

            messages.Add("每条 ADR 文档必须有且仅有一个对应的架构测试类，命名规范：ADR_xxxx_xxxx_Architecture_Tests");

            throw new Xunit.Sdk.XunitException(string.Join(Environment.NewLine, messages));
        }
    }

    private static string? FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current != null)
        {
            if (Directory.Exists(Path.Combine(current.FullName, "docs", "adr")) ||
                Directory.Exists(Path.Combine(current.FullName, ".git")))
                return current.FullName;

            current = current.Parent;
        }
        return null;
    }

    private static IReadOnlyList<string> LoadAdrIds(string adrDirectory)
    {
        return Directory.GetFiles(adrDirectory, AdrFilePattern)
            .Select(Path.GetFileNameWithoutExtension)
            .Where(file => !string.IsNullOrWhiteSpace(file) &&
                           System.Text.RegularExpressions.Regex.IsMatch(file!, @"^ADR-\d{4}", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            .Select(file => file!)
            .Distinct()
            .ToList();
    }

    private static IReadOnlyList<Type> LoadArchitectureTestTypes()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t =>
                t is { IsClass: true, IsSealed: true, IsPublic: true, Namespace: not null } &&
                t.Namespace.StartsWith(TestNamespacePrefix, StringComparison.OrdinalIgnoreCase) &&
                t.Name.EndsWith(TypeSuffix))
            .ToList();
    }

    private static string ToExpectedTypeName(string adrId)
    {
        var segments = adrId.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2)
            throw new InvalidOperationException($"ADR 文件名不合法: {adrId}");
        var number = segments[1];
        return $"ADR_{number}{TypeSuffix}";
    }
}
