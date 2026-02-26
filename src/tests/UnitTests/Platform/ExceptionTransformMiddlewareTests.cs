using Npgsql;
using Zss.BilliardHall.Platform.Exceptions;
using Zss.BilliardHall.Platform.Infrastructure;

namespace Zss.BilliardHall.Tests.UnitTests.Platform;

public sealed class ExceptionTransformMiddlewareTests
{
    private sealed class TestDomainException(Exception inner)
        : DomainException("TEST_ERROR", "测试领域异常", inner);

    private sealed class AnotherDomainException(Exception inner)
        : DomainException("ANOTHER_ERROR", "另一个领域异常", inner);

    // 每次调用都创建新实例（与真实 transformer 行为一致）
    private sealed class AlwaysMatchTransformer : IPostgresExceptionTransformer
    {
        public DomainException? TryTransform(PostgresException ex) => new TestDomainException(ex);
    }

    private sealed class AnotherAlwaysMatchTransformer : IPostgresExceptionTransformer
    {
        public DomainException? TryTransform(PostgresException ex) => new AnotherDomainException(ex);
    }

    private sealed class NeverMatchTransformer : IPostgresExceptionTransformer
    {
        public DomainException? TryTransform(PostgresException ex) => null;
    }

    private static PostgresException CreatePostgresException(string sqlState = "23505", string constraintName = "test_constraint")
        => new PostgresException(
            messageText: "test",
            severity: "ERROR",
            invariantSeverity: "ERROR",
            sqlState: sqlState,
            detail: null,
            hint: null,
            position: 0,
            internalPosition: 0,
            internalQuery: null,
            where: null,
            schemaName: null,
            tableName: null,
            columnName: null,
            dataTypeName: null,
            constraintName: constraintName,
            file: null,
            line: null,
            routine: null);

    #region 无 PostgresException 时不转换

    [Fact]
    public async Task InvokeAsync_WhenNonPostgresException_RethrowsOriginal()
    {
        var original = new InvalidOperationException("无关异常");
        var sut = new ExceptionTransformMiddleware([new NeverMatchTransformer()]);

        var act = async () => await sut.InvokeAsync(() => throw original);

        var thrown = await act.Should().ThrowAsync<InvalidOperationException>();
        thrown.Which.Should().BeSameAs(original);
    }

    #endregion

    #region 直接 PostgresException 命中

    [Fact]
    public async Task InvokeAsync_WhenDirectPostgresException_TransformsAndThrowsDomain()
    {
        var pg = CreatePostgresException();
        var sut = new ExceptionTransformMiddleware([new AlwaysMatchTransformer()]);

        var act = async () => await sut.InvokeAsync(() => throw pg);

        var thrown = await act.Should().ThrowAsync<TestDomainException>();
        thrown.Which.InnerException.Should().BeSameAs(pg);
    }

    #endregion

    #region 包装异常中含 PostgresException（验证 FindPostgresException 遍历内层）

    [Fact]
    public async Task InvokeAsync_WhenWrappedPostgresException_FindsInnerAndTransforms()
    {
        var pg = CreatePostgresException();
        var wrapper = new Exception("外部包装异常", pg);
        var sut = new ExceptionTransformMiddleware([new AlwaysMatchTransformer()]);

        var act = async () => await sut.InvokeAsync(() => throw wrapper);

        // Transformer 接收到的是内层 pg，生成的 DomainException.InnerException 应为 pg
        var thrown = await act.Should().ThrowAsync<TestDomainException>();
        thrown.Which.InnerException.Should().BeSameAs(pg);
    }

    #endregion

    #region 无命中时重抛原始异常

    [Fact]
    public async Task InvokeAsync_WhenNoTransformerMatches_RethrowsOriginal()
    {
        var pg = CreatePostgresException();
        var sut = new ExceptionTransformMiddleware([new NeverMatchTransformer()]);

        var act = async () => await sut.InvokeAsync(() => throw pg);

        var thrown = await act.Should().ThrowAsync<PostgresException>();
        thrown.Which.Should().BeSameAs(pg);
    }

    #endregion

    #region 转换后 InnerException 为原始 PostgresException（可观测性）

    [Fact]
    public async Task InvokeAsync_WhenTransformed_DomainExceptionInnerIsOriginalPostgres()
    {
        var pg = CreatePostgresException();
        var sut = new ExceptionTransformMiddleware([new AlwaysMatchTransformer()]);

        var act = async () => await sut.InvokeAsync(() => throw pg);

        var thrown = await act.Should().ThrowAsync<TestDomainException>();
        thrown.Which.InnerException.Should().BeSameAs(pg);
    }

    #endregion

    #region 空 transformer 列表时重抛

    [Fact]
    public async Task InvokeAsync_WhenNoTransformers_RethrowsPostgresException()
    {
        var pg = CreatePostgresException();
        var sut = new ExceptionTransformMiddleware([]);

        var act = async () => await sut.InvokeAsync(() => throw pg);

        await act.Should().ThrowAsync<PostgresException>();
    }

    #endregion

    #region 多个 transformer 时使用第一个匹配结果（短路）

    [Fact]
    public async Task InvokeAsync_WithMultipleTransformers_UsesFirstMatch()
    {
        var pg = CreatePostgresException();
        var sut = new ExceptionTransformMiddleware([
            new AlwaysMatchTransformer(),
            new AnotherAlwaysMatchTransformer()  // 不应被执行
        ]);

        var act = async () => await sut.InvokeAsync(() => throw pg);

        // 只有第一个 transformer 的结果（TestDomainException）会被抛出
        var thrown = await act.Should().ThrowAsync<TestDomainException>();
        thrown.Which.InnerException.Should().BeSameAs(pg);
    }

    #endregion
}
