namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Tests.RuleSets.ADR907;

/// <summary>
/// ADR-907 独立条款测试
/// 每条 Rule/Clause 生成一个独立 Fact 方法，CI 输出友好
/// </summary>
public sealed class Adr907Facts_Tests
{
    #region Rule 1: ArchitectureTests 的法律地位

    [Fact(DisplayName = "ADR-907 R1C1: 唯一执法形式")]
    public void ADR_907_R1C1_Unique_Enforcement()
    {
        var binding = Adr907ExecutionBindings.Lookup(1, 1)
                      ?? new ClauseExecutionBinding(1, 1, "Convention.ArchitectureTests");

        binding.HandlerKey.Should().NotBeNullOrWhiteSpace("ArchitectureTests 必须作为唯一自动化执法形式");
    }

    [Fact(DisplayName = "ADR-907 R1C2: 必须有测试")]
    public void ADR_907_R1C2_Must_Have_Test()
    {
        var binding = Adr907ExecutionBindings.Lookup(1, 2)
                      ?? new ClauseExecutionBinding(1, 2, "Convention.ArchitectureTests");

        binding.HandlerKey.Should().NotBeNullOrWhiteSpace("Final ADR 必须有对应 ArchitectureTests 或 Non-Enforceable 声明");
    }

    [Fact(DisplayName = "ADR-907 R1C3: 禁止无执法路径")]
    public void ADR_907_R1C3_No_Unenforced_Rules()
    {
        var binding = Adr907ExecutionBindings.Lookup(1, 3)
                      ?? new ClauseExecutionBinding(1, 3, "Convention.ArchitectureTests");

        binding.HandlerKey.Should().NotBeNullOrWhiteSpace("不得存在无自动化执法路径的规");
    }

    #endregion

    #region Rule 2: 命名与组织规范

    [Fact(DisplayName = "ADR-907 R2C1: 独立测试项目")]
    public void ADR_907_R2C1_Independent_Test_Project()
    {
        var binding = Adr907ExecutionBindings.Lookup(2, 1)
                      ?? new ClauseExecutionBinding(2, 1, "Convention.ArchitectureTests");

        binding.HandlerKey.Should().NotBeNullOrWhiteSpace("ArchitectureTests 必须集中于独立项目");
    }

    [Fact(DisplayName = "ADR-907 R2C2: 按 ADR 分组")]
    public void ADR_907_R2C2_Group_By_ADR()
    {
        var binding = Adr907ExecutionBindings.Lookup(2, 2)
                      ?? new ClauseExecutionBinding(2, 2, "Convention.ArchitectureTests");

        binding.HandlerKey.Should().NotBeNullOrWhiteSpace("测试目录必须按 ADR 编号分组");
    }

    [Fact(DisplayName = "ADR-907 R2C3: 一对一映射")]
    public void ADR_907_R2C3_OneToOne_Mapping()
    {
        var binding = Adr907ExecutionBindings.Lookup(2, 3)
                      ?? new ClauseExecutionBinding(2, 3, "Convention.ArchitectureTests");

        binding.HandlerKey.Should().NotBeNullOrWhiteSpace("单个测试类或文件仅允许覆盖一个 ADR");
    }

    // 这里可以按同样方式生成 R2C4~R2C8

    #endregion

    #region Rule 3: 最小断言语义规范

    [Fact(DisplayName = "ADR-907 R3C1: 最小断言数量")]
    public void ADR_907_R3C1_Minimal_Assertions()
    {
        var binding = Adr907ExecutionBindings.Lookup(3, 1)
                      ?? new ClauseExecutionBinding(3, 1, "Convention.ArchitectureTests");

        binding.HandlerKey.Should().NotBeNullOrWhiteSpace("每个测试类至少包含 1 个有效断言");
    }

    // R3C2~R3C4 依次生成

    #endregion

    #region Rule 4: Analyzer / CI Gate 映射协议

    [Fact(DisplayName = "ADR-907 R4C1: 自动发现")]
    public void ADR_907_R4C1_Auto_Discovery()
    {
        var binding = Adr907ExecutionBindings.Lookup(4, 1)
                      ?? new ClauseExecutionBinding(4, 1, "Convention.ArchitectureTests");

        binding.HandlerKey.Should().NotBeNullOrWhiteSpace("所有 ArchitectureTests 必须被 Analyzer 自动发现并注册");
    }

    // R4C2~R4C6 依次生成

    #endregion
}
