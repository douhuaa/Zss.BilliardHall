namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 测试常量定义（已废弃）
/// 
/// ⚠️ 此类已被重构为 ArchitectureTestSpecification，请使用新的规范模型。
/// 
/// 迁移指南：
/// - 所有命名空间常量 -> ArchitectureTestSpecification.Namespaces.*
/// - 所有 ADR 路径常量 -> ArchitectureTestSpecification.Adr.Paths.* 或 ArchitectureTestSpecification.Adr.KnownDocuments.*
/// - 所有 ADR 模式常量 -> ArchitectureTestSpecification.Adr.Patterns.*
/// - 所有语义常量 -> ArchitectureTestSpecification.Semantics.*
/// - 所有输出常量 -> ArchitectureTestSpecification.Output.States.*
/// - 所有 Onboarding 常量 -> ArchitectureTestSpecification.Onboarding.*
/// - BuildConfiguration 和 SupportedTargetFrameworks -> TestEnvironment.*
/// 
/// 此类保留作为向后兼容性桥接，但应被视为已废弃。
/// 新代码应直接使用 ArchitectureTestSpecification。
/// </summary>
[Obsolete("使用 ArchitectureTestSpecification 代替。此类仅为向后兼容保留。", false)]
public static class TestConstants
{
    // 此类已完全重构为 ArchitectureTestSpecification
    // 如需使用测试规范，请使用 ArchitectureTestSpecification
    // 如需使用环境配置，请使用 TestEnvironment
}
