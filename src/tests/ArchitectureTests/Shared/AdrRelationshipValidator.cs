namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// ADR 关系验证辅助类
/// 提供通用的关系一致性验证逻辑，避免重复代码
/// </summary>
public static class AdrRelationshipValidator
{
    /// <summary>
    /// 验证双向关系的一致性
    /// </summary>
    /// <param name="adrs">所有 ADR 文档</param>
    /// <param name="forwardRelation">前向关系名称（如 "DependsOn"）</param>
    /// <param name="backwardRelation">反向关系名称（如 "DependedBy"）</param>
    /// <param name="getForwardTargets">获取前向关系目标的委托</param>
    /// <param name="getBackwardTargets">获取反向关系目标的委托</param>
    /// <returns>违规描述列表</returns>
    public static List<string> ValidateBidirectionalRelationship(
        IReadOnlyDictionary<string, AdrDocument> adrs,
        string forwardRelation,
        string backwardRelation,
        Func<AdrDocument, IEnumerable<string>> getForwardTargets,
        Func<AdrDocument, IEnumerable<string>> getBackwardTargets)
    {
        var violations = new List<string>();

        foreach (var adr in adrs.Values)
        {
            var targets = getForwardTargets(adr);

            foreach (var target in targets)
            {
                // 检查目标 ADR 是否存在
                if (!adrs.TryGetValue(target, out var targetAdr))
                {
                    violations.Add(
                        $"❌ {adr.Id} 声明 {forwardRelation} {target}，" +
                        $"但 {target} 不存在或已删除\n" +
                        $"   文件：{adr.FilePath}\n" +
                        $"   修复：删除对 {target} 的引用或创建该 ADR"
                    );
                    continue;
                }

                // 检查反向关系
                var backRefs = getBackwardTargets(targetAdr);
                if (!backRefs.Contains(adr.Id))
                {
                    violations.Add(
                        $"❌ {forwardRelation} 关系不一致：\n" +
                        $"   {adr.Id} → {target}（声明了 {forwardRelation}）\n" +
                        $"   但 {target} 未声明 {backwardRelation}\n" +
                        $"   修复：在 {target}.md 中添加对应的 {backwardRelation} 声明：{adr.Id}"
                    );
                }
            }
        }

        return violations;
    }

    /// <summary>
    /// 根据关系类型获取目标列表
    /// </summary>
    public static IEnumerable<string> GetRelationshipTargets(
        AdrDocument adr,
        string relationshipType)
    {
        return relationshipType switch
        {
            "DependsOn" => adr.DependsOn,
            "DependedBy" => adr.DependedBy,
            "Supersedes" => adr.Supersedes,
            "SupersededBy" => adr.SupersededBy,
            "Related" => adr.Related,
            _ => Enumerable.Empty<string>()
        };
    }

    /// <summary>
    /// 验证对称关系（如 Related）
    /// </summary>
    public static List<string> ValidateSymmetricRelationship(
        IReadOnlyDictionary<string, AdrDocument> adrs,
        string relationName,
        Func<AdrDocument, IEnumerable<string>> getTargets)
    {
        var violations = new List<string>();

        foreach (var adr in adrs.Values)
        {
            var targets = getTargets(adr);

            foreach (var target in targets)
            {
                if (!adrs.TryGetValue(target, out var targetAdr))
                {
                    violations.Add(
                        $"❌ {adr.Id} 声明 {relationName} {target}，但 {target} 不存在\n" +
                        $"   文件：{adr.FilePath}"
                    );
                    continue;
                }

                var targetRefs = getTargets(targetAdr);
                if (!targetRefs.Contains(adr.Id))
                {
                    violations.Add(
                        $"❌ {relationName} 关系不对称：\n" +
                        $"   {adr.Id} → {target}\n" +
                        $"   但 {target} 未声明 {relationName} {adr.Id}\n" +
                        $"   修复：在 {target}.md 中添加 {relationName}: {adr.Id}"
                    );
                }
            }
        }

        return violations;
    }
}
