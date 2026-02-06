namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification;

using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Index;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.Rules;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0001;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0002;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0003;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0120;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0201;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0900;
using Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR0907;

/// <summary>
/// ArchitectureTestSpecification - 架构规则集定义（轻量级聚合器）
/// 
/// ⚠️ 重要变更：
/// 此文件已从"规则定义文件"升级为"规则聚合器"
/// 真正的规则定义已迁移到 /Specification/RuleSets/{ADRxxxx}/ 目录下
/// 
/// 新架构：
/// - /RuleSets/{ADRxxxx}/AdrxxxxRuleSet.cs - 规则定义（每个 ADR 一个独立文件）
/// - /Index/RuleSetRegistry.cs - 规则集注册表（统一访问入口）
/// - /Index/AdrRuleIndex.cs - 规则索引（快速查询）
/// - 本文件 - 向后兼容层（保留旧 API）
/// 
/// 推荐使用方式：
/// ✅ 新代码：使用 RuleSetRegistry.Get(adrNumber) 获取规则集
/// ⚠️ 旧代码：可继续使用 ArchitectureRules.AdrXXX，但建议迁移
/// 
/// 设计目标：
/// 1. 保持向后兼容性，避免破坏现有测试
/// 2. 为新代码提供清晰的迁移路径
/// 3. 支持未来的规则集扩展（100+ ADR）
/// </summary>
public static partial class ArchitectureTestSpecification
{
    /// <summary>
    /// 架构规则集合（向后兼容层）
    /// 
    /// ⚠️ 注意：此类仅为向后兼容保留
    /// 新代码请使用 RuleSetRegistry.Get(adrNumber) 代替
    /// </summary>
    public static class ArchitectureRules
    {
        #region Constitutional ADRs (宪法层 ADR-001 ~ ADR-008)

        /// <summary>
        /// ADR-001：模块化单体与垂直切片架构
        /// ⚠️ 向后兼容属性，新代码请使用 RuleSetRegistry.Get(1)
        /// </summary>
        public static ArchitectureRuleSet Adr001 => Adr0001RuleSet.RuleSet;

        /// <summary>
        /// ADR-002：Platform/Application/Host 启动引导
        /// ⚠️ 向后兼容属性，新代码请使用 RuleSetRegistry.Get(2)
        /// </summary>
        public static ArchitectureRuleSet Adr002 => Adr0002RuleSet.RuleSet;

        /// <summary>
        /// ADR-003：命名空间规则
        /// ⚠️ 向后兼容属性，新代码请使用 RuleSetRegistry.Get(3)
        /// </summary>
        public static ArchitectureRuleSet Adr003 => Adr0003RuleSet.RuleSet;

        #endregion

        #region Governance ADRs (治理层 ADR-900 ~ ADR-999)

        /// <summary>
        /// ADR-900：架构测试与 CI 治理元规则
        /// ⚠️ 向后兼容属性，新代码请使用 RuleSetRegistry.Get(900)
        /// </summary>
        public static ArchitectureRuleSet Adr900 => Adr0900RuleSet.RuleSet;

        /// <summary>
        /// ADR-907：ArchitectureTests 执法治理体系
        /// ⚠️ 向后兼容属性，新代码请使用 RuleSetRegistry.Get(907)
        /// </summary>
        public static ArchitectureRuleSet Adr907 => Adr0907RuleSet.RuleSet;

        #endregion

        #region Runtime ADRs (运行时 ADR-201 ~ ADR-240)

        /// <summary>
        /// ADR-201：Handler 生命周期管理
        /// ⚠️ 向后兼容属性，新代码请使用 RuleSetRegistry.Get(201)
        /// </summary>
        public static ArchitectureRuleSet Adr201 => Adr0201RuleSet.RuleSet;

        #endregion

        #region Structure ADRs (结构层 ADR-120 ~ ADR-124)

        /// <summary>
        /// ADR-120：领域事件命名规范
        /// ⚠️ 向后兼容属性，新代码请使用 RuleSetRegistry.Get(120)
        /// </summary>
        public static ArchitectureRuleSet Adr120 => Adr0120RuleSet.RuleSet;

        #endregion

        #region Helper Methods

        /// <summary>
        /// 获取指定 ADR 的规则集
        /// ⚠️ 向后兼容方法，新代码请使用 RuleSetRegistry.Get(adrNumber)
        /// </summary>
        /// <param name="adrNumber">ADR 编号</param>
        /// <returns>规则集，如果不存在则返回 null</returns>
        public static ArchitectureRuleSet? GetRuleSet(int adrNumber)
        {
            return RuleSetRegistry.Get(adrNumber);
        }

        /// <summary>
        /// 获取所有已定义的规则集
        /// ⚠️ 向后兼容方法，新代码请使用 RuleSetRegistry.GetAllRuleSets()
        /// </summary>
        public static IEnumerable<ArchitectureRuleSet> GetAllRuleSets()
        {
            return RuleSetRegistry.GetAllRuleSets();
        }

        /// <summary>
        /// 获取所有已定义的 ADR 编号
        /// ⚠️ 向后兼容方法，新代码请使用 RuleSetRegistry.GetAllAdrNumbers()
        /// </summary>
        public static IEnumerable<int> GetAllAdrNumbers()
        {
            return RuleSetRegistry.GetAllAdrNumbers();
        }

        #endregion
    }
}
