using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-940: ADR 关系与溯源管理
/// 验证 ADR 文档之间的关系声明双向一致性
/// 
/// 这是治理测试，不是工具脚本：
/// - ❌ 不返回 bool
/// - ❌ 不打印日志  
/// - ✅ 直接 Assert，失败即裁决
/// </summary>
public sealed class AdrRelationshipConsistencyTests
{
    private readonly IReadOnlyDictionary<string, AdrDocument> _adrs;

    public AdrRelationshipConsistencyTests()
    {
        var repoRoot = FindRepositoryRoot() 
            ?? throw new InvalidOperationException("未找到仓库根目录（无法定位 docs/adr 或 .git）");
        
        var adrPath = Path.Combine(repoRoot, "docs", "adr");
        var repo = new AdrRepository(adrPath);
        
        _adrs = repo.LoadAll().ToDictionary(a => a.Id);
    }

    /// <summary>
    /// ADR-940.3: A 依赖 B ⇔ B 被 A 依赖（双向一致性）
    /// </summary>
    [Fact(DisplayName = "ADR-940.3: 依赖关系必须双向一致")]
    public void DependsOn_Must_Be_Declared_Bidirectionally()
    {
        var violations = new List<string>();

        foreach (var adr in _adrs.Values)
        {
            foreach (var target in adr.DependsOn)
            {
                // 检查目标 ADR 是否存在
                if (!_adrs.TryGetValue(target, out var targetAdr))
                {
                    violations.Add(
                        $"❌ {adr.Id} 依赖不存在的 ADR：{target}\n" +
                        $"   文件：{adr.FilePath}"
                    );
                    continue;
                }

                // 检查反向关系
                if (!targetAdr.DependedBy.Contains(adr.Id))
                {
                    violations.Add(
                        $"❌ 依赖关系不一致：\n" +
                        $"   {adr.Id} 声明依赖 {target}\n" +
                        $"   但 {target} 未声明被 {adr.Id} 依赖\n" +
                        $"   修复：在 {target}.md 的 **Depended By** 中添加 {adr.Id}"
                    );
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// ADR-940.3: A 被 B 依赖 ⇔ B 依赖 A（双向一致性）
    /// </summary>
    [Fact(DisplayName = "ADR-940.3: 被依赖关系必须双向一致")]
    public void DependedBy_Must_Be_Declared_Bidirectionally()
    {
        var violations = new List<string>();

        foreach (var adr in _adrs.Values)
        {
            foreach (var target in adr.DependedBy)
            {
                // 检查目标 ADR 是否存在
                if (!_adrs.TryGetValue(target, out var targetAdr))
                {
                    violations.Add(
                        $"❌ {adr.Id} 被不存在的 ADR 依赖：{target}\n" +
                        $"   文件：{adr.FilePath}"
                    );
                    continue;
                }

                // 检查反向关系
                if (!targetAdr.DependsOn.Contains(adr.Id))
                {
                    violations.Add(
                        $"❌ 被依赖关系不一致：\n" +
                        $"   {adr.Id} 声明被 {target} 依赖\n" +
                        $"   但 {target} 未声明依赖 {adr.Id}\n" +
                        $"   修复：在 {target}.md 的 **Depends On** 中添加 {adr.Id}"
                    );
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// ADR-940.3: A 替代 B ⇔ B 被 A 替代（双向一致性）
    /// </summary>
    [Fact(DisplayName = "ADR-940.3: 替代关系必须双向一致")]
    public void Supersedes_Must_Be_Declared_Bidirectionally()
    {
        var violations = new List<string>();

        foreach (var adr in _adrs.Values)
        {
            foreach (var target in adr.Supersedes)
            {
                // 检查目标 ADR 是否存在
                if (!_adrs.TryGetValue(target, out var targetAdr))
                {
                    violations.Add(
                        $"❌ {adr.Id} 替代不存在的 ADR：{target}\n" +
                        $"   文件：{adr.FilePath}"
                    );
                    continue;
                }

                // 检查反向关系
                if (!targetAdr.SupersededBy.Contains(adr.Id))
                {
                    violations.Add(
                        $"❌ 替代关系不一致：\n" +
                        $"   {adr.Id} 声明替代 {target}\n" +
                        $"   但 {target} 未声明被 {adr.Id} 替代\n" +
                        $"   修复：在 {target}.md 的 **Superseded By** 中添加 {adr.Id}"
                    );
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// ADR-940.3: A 被 B 替代 ⇔ B 替代 A（双向一致性）
    /// </summary>
    [Fact(DisplayName = "ADR-940.3: 被替代关系必须双向一致")]
    public void SupersededBy_Must_Be_Declared_Bidirectionally()
    {
        var violations = new List<string>();

        foreach (var adr in _adrs.Values)
        {
            foreach (var target in adr.SupersededBy)
            {
                // 检查目标 ADR 是否存在
                if (!_adrs.TryGetValue(target, out var targetAdr))
                {
                    violations.Add(
                        $"❌ {adr.Id} 被不存在的 ADR 替代：{target}\n" +
                        $"   文件：{adr.FilePath}"
                    );
                    continue;
                }

                // 检查反向关系
                if (!targetAdr.Supersedes.Contains(adr.Id))
                {
                    violations.Add(
                        $"❌ 被替代关系不一致：\n" +
                        $"   {adr.Id} 声明被 {target} 替代\n" +
                        $"   但 {target} 未声明替代 {adr.Id}\n" +
                        $"   修复：在 {target}.md 的 **Supersedes** 中添加 {adr.Id}"
                    );
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// 查找仓库根目录
    /// </summary>
    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, "docs", "adr")) ||
                Directory.Exists(Path.Combine(currentDir, ".git")))
            {
                return currentDir;
            }
            
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        
        return null;
    }
}
