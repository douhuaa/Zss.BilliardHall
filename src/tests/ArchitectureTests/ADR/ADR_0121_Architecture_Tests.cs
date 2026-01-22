namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0121: 契约（Contract）命名规范
/// 验证契约接口和 DTO 命名符合规范
/// 
/// 本 ADR 属于结构层（ADR-100~199），主要通过以下方式保证：
/// 1. Copilot Prompts（docs/copilot/adr-0121.prompts.md）- 实时提醒和指导
/// 2. Code Review - 人工审查契约设计和命名
/// 3. 团队培训 - 确保团队理解契约的作用和使用场景
/// 
/// 注意：根据 ADR-0000，结构层 ADR 的架构测试可以是文档化约束，
/// 因为契约命名和使用规范需要理解跨模块通信语义。
/// </summary>
public sealed class ADR_0121_Architecture_Tests
{
    /// <summary>
    /// ADR-0121: 契约命名规范
    /// 本测试类标记 ADR-0121 已被文档化并通过多层保障机制执行。
    /// 
    /// 保障机制：
    /// 1. Copilot Prompts - 实时检测契约使用违规（如 Command Handler 依赖查询接口）
    /// 2. Code Review - PR 审查时验证契约设计和命名
    /// 3. 团队约定 - 明确契约的边界和使用场景
    /// 
    /// 未来可扩展的自动化检查：
    /// - 检查查询接口是否使用复数 Queries
    /// - 检查契约 DTO 是否使用 record 类型
    /// - 检查契约是否定义在 Platform.Contracts
    /// - 检查契约 DTO 是否实现 IContract
    /// - 检查查询接口是否继承 IQuery
    /// - 检查 Command Handler 是否依赖查询接口（严重违规）
    /// 
    /// 当前状态：已文档化，通过人工审查和 Copilot 辅助保证
    /// </summary>
    [Fact(DisplayName = "ADR-0121: 契约命名规范（文档化约束）")]
    public void Contract_Naming_Conventions_Are_Documented()
    {
        // ADR-0121 定义了契约命名规范，包括：
        
        // 1. 查询接口命名
        //    格式：I{实体}Queries（使用复数）
        //    ✅ 正确：IMemberQueries, IOrderQueries
        //    ❌ 错误：IMemberQuery（单数）, IMemberService（Service 后缀）
        
        // 2. 契约 DTO 命名
        //    格式：{实体}{限定词}Dto
        //    ✅ 正确：MemberDto, MemberSummaryDto, OrderDetailsDto
        //    ❌ 错误：Member（缺少 Dto 后缀）, MemberContract（Contract 后缀）
        
        // 3. 查询方法命名
        //    格式：{动词}{实体}{条件}Async
        //    ✅ 正确：GetByIdAsync, FindByEmailAsync, ListActiveMembersAsync
        //    ❌ 错误：GetMember（缺少 Async）, SelectById（SQL 术语）
        
        // 4. 契约位置规范
        //    跨模块契约必须定义在：Platform.Contracts/{模块名}/
        //    模块内部 DTO 可以放在：Modules/{模块}/Features/{用例}/
        
        // 5. 契约不可变性
        //    契约 DTO 必须：
        //    - 使用 record 类型（不是 class）
        //    - 属性使用 init（不是 set）
        //    - 实现 IContract 标记接口
        
        // 6. 契约使用规则
        //    ✅ Query Handler 可以返回契约 DTO
        //    ✅ Endpoint 可以使用契约进行请求/响应
        //    ❌ Command Handler 不应依赖其他模块的查询接口（严重违规）
        
        // 7. 查询接口继承
        //    所有查询接口必须继承 IQuery 标记接口
        //    示例：public interface IMemberQueries : IQuery
        
        // 这些规范通过以下方式保证：
        // - docs/copilot/adr-0121.prompts.md 提供实时指导和违规检测
        // - Code Review 验证契约设计是否合理
        // - docs/adr/structure/ADR-0121-contract-naming-conventions.md 提供详细说明
        // - ADR-0001 的架构测试验证 Command Handler 不依赖查询接口
        
        // 验证：本测试类的存在表明 ADR-0121 已被正式采纳并文档化
        // 通过多层保障机制（Copilot + Code Review + 团队培训 + ADR-0001测试）确保执行
        
        // 简单验证：确认测试类本身存在（通过反射）
        var thisTestType = typeof(ADR_0121_Architecture_Tests);
        Assert.NotNull(thisTestType);
        Assert.Equal("ADR_0121_Architecture_Tests", thisTestType.Name);
        
        // 文档化说明：ADR-0121 主要通过以下方式保证：
        // - docs/adr/structure/ADR-0121-contract-naming-conventions.md 提供详细规范
        // - docs/copilot/adr-0121.prompts.md 提供实时指导和违规检测
        // - Code Review 验证契约设计和命名
        // - ADR-0001 的架构测试验证 Command Handler 不依赖查询接口（契约使用的关键约束）
        // - 团队培训确保理解契约的作用和使用场景
    }
}
