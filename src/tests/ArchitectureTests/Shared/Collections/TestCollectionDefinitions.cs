namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Collections;

/// <summary>
/// xUnit 测试集合定义
/// 用于在同一集合内的测试之间共享 Fixture 实例
/// </summary>
[CollectionDefinition(CollectionNames.IntegrationTests)]
public class IntegrationTestCollection : ICollectionFixture<Fixtures.SharedTestFixture>
{
    // 此类无需任何实现
    // xUnit 会自动为集合内的所有测试类注入 SharedTestFixture
}

/// <summary>
/// 测试集合名称常量
/// </summary>
public static class CollectionNames
{
    /// <summary>
    /// 集成测试集合
    /// 使用此集合的测试类将共享同一个 SharedTestFixture 实例
    /// </summary>
    public const string IntegrationTests = "IntegrationTests";

    /// <summary>
    /// 独立测试集合
    /// 用于需要完全隔离的测试（每个测试类使用独立的 Fixture）
    /// </summary>
    public const string IsolatedTests = "IsolatedTests";
}

/// <summary>
/// 使用示例：
/// 
/// [Collection(CollectionNames.IntegrationTests)]
/// public class MyIntegrationTests
/// {
///     private readonly SharedTestFixture _fixture;
///     
///     public MyIntegrationTests(SharedTestFixture fixture)
///     {
///         _fixture = fixture;
///     }
///     
///     [Fact]
///     public async Task Should_Save_Document()
///     {
///         // 使用 _fixture.DocumentStore 进行测试
///         await using var session = _fixture.DocumentStore.LightweightSession();
///         // ...
///     }
/// }
/// </summary>
