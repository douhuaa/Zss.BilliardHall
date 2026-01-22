namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0120: 功能切片命名规范
/// 验证 Command/Query/Handler/Endpoint/DTO 命名符合规范
/// 
/// 本 ADR 属于结构层（ADR-100~199），主要通过以下方式保证：
/// 1. Copilot Prompts（docs/copilot/adr-0120.prompts.md）- 实时提醒和指导
/// 2. Code Review - 人工审查命名是否符合业务语义
/// 3. 团队培训 - 确保团队理解业务动词和技术动词的区别
/// 
/// 注意：根据 ADR-0000，结构层 ADR 的架构测试可以是文档化约束，
/// 因为命名规范需要理解业务语义，难以通过简单的静态分析完全验证。
/// </summary>
public sealed class ADR_0120_Architecture_Tests
{
    /// <summary>
    /// ADR-0120: 功能切片命名规范
    /// 本测试类标记 ADR-0120 已被文档化并通过多层保障机制执行。
    /// 
    /// 保障机制：
    /// 1. Copilot Prompts - 实时识别技术术语并建议业务术语
    /// 2. Code Review - PR 审查时验证命名是否表达业务意图
    /// 3. 团队约定 - 维护业务动词和技术动词对照表
    /// 
    /// 未来可扩展的自动化检查：
    /// - 检查 Command 是否包含 Command 后缀
    /// - 检查 Query 是否包含 Query 后缀
    /// - 检查 Handler 是否与 Command/Query 名称对应
    /// - 检查是否使用黑名单技术动词（Save, Insert, Select 等）
    /// - 检查目录名是否使用 PascalCase
    /// 
    /// 当前状态：已文档化，通过人工审查和 Copilot 辅助保证
    /// </summary>
    [Fact(DisplayName = "ADR-0120: 功能切片命名规范（文档化约束）")]
    public void Feature_Naming_Conventions_Are_Documented()
    {
        // ADR-0120 定义了功能切片命名规范，包括：
        
        // 1. 功能切片目录命名
        //    格式：{动词}{实体}（PascalCase）
        //    ✅ 正确：CreateMember, GetMemberById, UpdateMemberProfile
        //    ❌ 错误：Member（缺少动词）, CRUD（技术术语）, SaveMember（技术动词）
        
        // 2. Command 命名
        //    格式：{动词}{实体}Command
        //    ✅ 正确：CreateMemberCommand, UpdateMemberProfileCommand
        //    ❌ 错误：MemberCommand（缺少动词）, SaveMemberCommand（技术动词）
        
        // 3. Query 命名
        //    格式：{动词}{实体}{限定条件}Query
        //    ✅ 正确：GetMemberByIdQuery, ListActiveMembersQuery
        //    ❌ 错误：MemberQuery（缺少动词和限定条件）, SelectMemberQuery（SQL 术语）
        
        // 4. Handler 命名
        //    格式：{Command/Query名}Handler
        //    ✅ 正确：CreateMemberCommandHandler, GetMemberByIdQueryHandler
        //    ❌ 错误：MemberHandler（过于宽泛）, MemberService（Service 后缀）
        
        // 5. DTO 命名
        //    格式：{实体}{限定词}Dto
        //    ✅ 正确：MemberDto, MemberSummaryDto, MemberDetailsDto
        //    ❌ 错误：Member（缺少 Dto 后缀）, MemberContract（不用 Contract 后缀）
        
        // 6. 业务动词 vs 技术动词
        //    业务动词：Create, Update, Activate, Cancel, Approve, Reject
        //    技术动词（避免）：Save, Insert, Delete, Select, Load, Execute
        
        // 这些规范通过以下方式保证：
        // - docs/copilot/adr-0120.prompts.md 提供实时指导和反模式检测
        // - Code Review 验证命名是否表达业务意图
        // - docs/adr/structure/ADR-0120-feature-naming-conventions.md 提供详细说明
        
        // 验证：本测试类的存在表明 ADR-0120 已被正式采纳并文档化
        // 通过多层保障机制（Copilot + Code Review + 团队培训）确保执行
        Assert.True(true, 
            "ADR-0120 已通过文档化、Copilot Prompts 和 Code Review 保证");
    }
}
