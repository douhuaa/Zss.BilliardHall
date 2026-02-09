namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Extensions;

/// <summary>
/// ADR 测试扩展方法
/// 提供常用的 ADR 测试辅助功能
/// </summary>
public static class AdrTestExtensions
{
    /// <summary>
    /// 验证 ADR 文档是否存在
    /// </summary>
    public static void AssertAdrExists(this IReadOnlyDictionary<string, AdrDocument> adrs, string adrId, string? because = null)
    {
        adrs.Should().ContainKey(adrId, because ?? $"ADR {adrId} 应该存在");
    }

    /// <summary>
    /// 验证 ADR 文档状态
    /// </summary>
    public static void AssertAdrStatus(this AdrDocument adr, string expectedStatus, string? because = null)
    {
        adr.Status.Should().Be(expectedStatus, because ?? $"ADR {adr.Id} 的状态应该是 {expectedStatus}");
    }

    /// <summary>
    /// 验证 ADR 文档有特定依赖
    /// </summary>
    public static void AssertDependsOn(this AdrDocument adr, string targetAdrId, string? because = null)
    {
        adr.DependsOn.Should().Contain(targetAdrId, 
            because ?? $"ADR {adr.Id} 应该依赖于 {targetAdrId}");
    }

    /// <summary>
    /// 验证 ADR 文档替代另一个 ADR
    /// </summary>
    public static void AssertSupersedes(this AdrDocument adr, string targetAdrId, string? because = null)
    {
        adr.Supersedes.Should().Contain(targetAdrId, 
            because ?? $"ADR {adr.Id} 应该替代 {targetAdrId}");
    }

    /// <summary>
    /// 获取所有指定状态的 ADR
    /// </summary>
    public static IEnumerable<AdrDocument> WithStatus(this IEnumerable<AdrDocument> adrs, string status)
    {
        return adrs.Where(a => a.Status?.Equals(status, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// 获取所有已接受的 ADR
    /// </summary>
    public static IEnumerable<AdrDocument> Accepted(this IEnumerable<AdrDocument> adrs)
    {
        return adrs.Where(a => 
            a.Status?.Equals("已接受", StringComparison.OrdinalIgnoreCase) == true ||
            a.Status?.Equals("accepted", StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// 获取所有已废弃的 ADR
    /// </summary>
    public static IEnumerable<AdrDocument> Deprecated(this IEnumerable<AdrDocument> adrs)
    {
        return adrs.Where(a => 
            a.Status?.Equals("已废弃", StringComparison.OrdinalIgnoreCase) == true ||
            a.Status?.Equals("deprecated", StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// 获取所有待定的 ADR
    /// </summary>
    public static IEnumerable<AdrDocument> Pending(this IEnumerable<AdrDocument> adrs)
    {
        return adrs.Where(a => 
            a.Status?.Equals("待定", StringComparison.OrdinalIgnoreCase) == true ||
            a.Status?.Equals("pending", StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// 获取所有正式的 ADR 文档
    /// </summary>
    public static IEnumerable<AdrDocument> OnlyAdrs(this IEnumerable<AdrDocument> adrs)
    {
        return adrs.Where(a => a.IsAdr);
    }

    /// <summary>
    /// 按 ID 过滤 ADR
    /// </summary>
    public static IEnumerable<AdrDocument> WithIdPattern(this IEnumerable<AdrDocument> adrs, string pattern)
    {
        return adrs.Where(a => a.Id.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取指定范围的 ADR（按编号）
    /// </summary>
    public static IEnumerable<AdrDocument> InRange(this IEnumerable<AdrDocument> adrs, int fromNumber, int toNumber)
    {
        return adrs.Where(a =>
        {
            // 从 ID 中提取编号（例如 "ADR-001" -> 1）
            var idParts = a.Id.Split('-');
            if (idParts.Length >= 2 && int.TryParse(idParts[1], out var number))
            {
                return number >= fromNumber && number <= toNumber;
            }
            return false;
        });
    }

    /// <summary>
    /// 验证 ADR 集合不为空
    /// </summary>
    public static void AssertNotEmpty(this IEnumerable<AdrDocument> adrs, string? because = null)
    {
        adrs.Should().NotBeEmpty(because ?? "ADR 集合不应为空");
    }

    /// <summary>
    /// 验证 ADR 集合数量
    /// </summary>
    public static void AssertCount(this IEnumerable<AdrDocument> adrs, int expectedCount, string? because = null)
    {
        adrs.Should().HaveCount(expectedCount, because ?? $"ADR 集合应该包含 {expectedCount} 个文档");
    }

    /// <summary>
    /// 获取 ADR 的所有前置依赖
    /// </summary>
    public static IEnumerable<string> GetAllDependencies(this AdrDocument adr)
    {
        return adr.DependsOn.Concat(adr.Supersedes).Distinct();
    }

    /// <summary>
    /// 获取 ADR 的所有后置引用
    /// </summary>
    public static IEnumerable<string> GetAllReferences(this AdrDocument adr)
    {
        return adr.DependedBy.Concat(adr.SupersededBy).Distinct();
    }

    /// <summary>
    /// 验证 ADR 是否有前端事项（Front Matter）
    /// </summary>
    public static void AssertHasFrontMatter(this AdrDocument adr, string? because = null)
    {
        adr.HasFrontMatter.Should().BeTrue(because ?? $"ADR {adr.Id} 应该有 Front Matter");
    }

    /// <summary>
    /// 验证 ADR 是否是正式 ADR
    /// </summary>
    public static void AssertIsAdr(this AdrDocument adr, string? because = null)
    {
        adr.IsAdr.Should().BeTrue(because ?? $"{adr.Id} 应该是正式的 ADR 文档");
    }

    /// <summary>
    /// 按架构层级过滤
    /// </summary>
    public static IEnumerable<AdrDocument> WithLevel(this IEnumerable<AdrDocument> adrs, string level)
    {
        return adrs.Where(a => a.Level?.Equals(level, StringComparison.OrdinalIgnoreCase) == true);
    }

    /// <summary>
    /// 获取治理层级的 ADR
    /// </summary>
    public static IEnumerable<AdrDocument> GovernanceLevel(this IEnumerable<AdrDocument> adrs)
    {
        return adrs.WithLevel("governance");
    }

    /// <summary>
    /// 获取技术层级的 ADR
    /// </summary>
    public static IEnumerable<AdrDocument> TechnicalLevel(this IEnumerable<AdrDocument> adrs)
    {
        return adrs.WithLevel("technical");
    }
}
