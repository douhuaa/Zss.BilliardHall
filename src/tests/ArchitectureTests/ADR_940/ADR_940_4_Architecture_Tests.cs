namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-940_4: ADR 依赖关系循环检测（Rule）
/// 检测 ADR 依赖关系中的循环依赖
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-940_4_1: ADR 依赖关系不得形成循环
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-940-adr-relationship-and-traceability.md
///
/// 实现说明：
/// - 使用 AdrTestFixture 统一加载 ADR 文档，避免重复代码
/// </summary>
public sealed class ADR_940_4_Architecture_Tests : IClassFixture<AdrTestFixture>
{
    private readonly IReadOnlyDictionary<string, AdrDocument> _adrs;

    public ADR_940_4_Architecture_Tests(AdrTestFixture fixture)
    {
        _adrs = fixture.AllAdrs;
    }

    /// <summary>
    /// ADR-940_4_1: ADR 依赖关系不得形成循环
    /// 验证 ADR 文档之间的依赖关系不存在循环引用（§ADR-940_4_1）
    /// </summary>
    [Fact(DisplayName = "ADR-940_4_1: ADR 依赖关系不得存在循环")]
    public void ADR_940_4_1_Dependencies_Must_Not_Form_Cycles()
    {
        var cycles = DetectCycles();

        cycles.Count.Should().Be(0, $"发现 {cycles.Count} 个循环依赖：\n\n" + string.Join("\n\n", cycles.Select(c => string.Join(" → ", c))));
    }

    /// <summary>
    /// 使用 DFS 检测循环依赖
    /// </summary>
    private List<List<string>> DetectCycles()
    {
        var cycles = new List<List<string>>();
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();
        var path = new List<string>();

        foreach (var adrId in _adrs.Keys)
        {
            if (!visited.Contains(adrId))
            {
                DfsDetectCycle(adrId, visited, recursionStack, path, cycles);
            }
        }

        return cycles;
    }

    /// <summary>
    /// 深度优先搜索检测循环
    /// </summary>
    private void DfsDetectCycle(
        string current,
        HashSet<string> visited,
        HashSet<string> recursionStack,
        List<string> path,
        List<List<string>> cycles)
    {
        visited.Add(current);
        recursionStack.Add(current);
        path.Add(current);

        if (_adrs.TryGetValue(current, out var adr))
        {
            foreach (var dependency in adr.DependsOn)
            {
                if (!_adrs.ContainsKey(dependency))
                    continue; // 跳过不存在的 ADR

                if (recursionStack.Contains(dependency))
                {
                    // 发现循环
                    var cycleStart = path.IndexOf(dependency);
                    if (cycleStart >= 0)
                    {
                        var cycle = path.Skip(cycleStart).ToList();
                        cycles.Add(cycle);
                    }
                }
                else if (!visited.Contains(dependency))
                {
                    DfsDetectCycle(dependency, visited, recursionStack, path, cycles);
                }
            }
        }

        path.RemoveAt(path.Count - 1);
        recursionStack.Remove(current);
    }
}
