namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Builders;

/// <summary>
/// 测试数据构建器基类
/// 提供流畅的 API 用于构建测试数据
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TBuilder">构建器类型（用于支持流畅 API）</typeparam>
public abstract class TestDataBuilder<TEntity, TBuilder> 
    where TEntity : class
    where TBuilder : TestDataBuilder<TEntity, TBuilder>
{
    protected TEntity Entity { get; set; }

    protected TestDataBuilder()
    {
        Entity = CreateDefault();
    }

    /// <summary>
    /// 创建默认实体实例
    /// 子类必须实现此方法以提供默认值
    /// </summary>
    protected abstract TEntity CreateDefault();

    /// <summary>
    /// 构建最终实体
    /// </summary>
    public virtual TEntity Build()
    {
        return Entity;
    }

    /// <summary>
    /// 返回当前构建器实例（用于流畅 API）
    /// </summary>
    protected TBuilder This => (TBuilder)this;
}

/// <summary>
/// 示例：用户测试数据构建器
/// 实际项目中根据领域模型创建具体的 Builder
/// </summary>
/// <example>
/// <code>
/// var user = new UserBuilder()
///     .WithName("张三")
///     .WithEmail("zhangsan@example.com")
///     .WithAge(30)
///     .Build();
/// </code>
/// </example>
public class SampleEntityBuilder : TestDataBuilder<SampleEntity, SampleEntityBuilder>
{
    protected override SampleEntity CreateDefault()
    {
        return new SampleEntity
        {
            Id = Guid.NewGuid(),
            Name = "默认名称",
            CreatedAt = DateTime.UtcNow
        };
    }

    public SampleEntityBuilder WithId(Guid id)
    {
        Entity.Id = id;
        return This;
    }

    public SampleEntityBuilder WithName(string name)
    {
        Entity.Name = name;
        return This;
    }

    public SampleEntityBuilder WithCreatedAt(DateTime createdAt)
    {
        Entity.CreatedAt = createdAt;
        return This;
    }
}

/// <summary>
/// 示例实体（仅用于演示 Builder 模式）
/// </summary>
public class SampleEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
