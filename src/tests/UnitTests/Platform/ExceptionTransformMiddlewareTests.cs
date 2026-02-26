using Npgsql;
using Zss.BilliardHall.Platform.Exceptions;
using Zss.BilliardHall.Platform.Infrastructure;

namespace Zss.BilliardHall.Tests.UnitTests.Platform;

public sealed class ExceptionTransformMiddlewareTests
{
    private sealed class TestDomainException(Exception inner)
        : DomainException("TEST_ERROR", "测试领域异常", inner);

    private sealed class AlwaysMatchTransformer(DomainException result) : IPostgresExceptionTransformer
    {
        public DomainException? TryTransform(PostgresException ex) => result;
    }

    private sealed class NeverMatchTransformer : IPostgresExceptionTransformer
    {
        public DomainException? TryTransform(PostgresException ex) => null;
    }

    // PostgresException 的公共构造函数：(messageText, severity, invariantSeverity, sqlState, ..., constraintName, ...)
    private static PostgresException CreatePostgresException(string sqlState = "23505", string constraintName = "test_constraint")
        => new(
            "test", "ERROR", "ERROR", sqlState,
            null, null, 0, 0, null, null, null, null, null, null,
            constraintName, null, null, null);

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
        var expected = new TestDomainException(pg);
        var sut = new ExceptionTransformMiddleware([new AlwaysMatchTransformer(expected)]);

        var act = async () => await sut.InvokeAsync(() => throw pg);

        var thrown = await act.Should().ThrowAsync<TestDomainException>();
        thrown.Which.Should().BeSameAs(expected);
    }

    #endregion

    #region 包装异常中含 PostgresException（验证 FindPostgresException 遍历内层）

    [Fact]
    public async Task InvokeAsync_WhenWrappedPostgresException_FindsInnerAndTransforms()
    {
        var pg = CreatePostgresException();
        var wrapper = new Exception("外部包装异常", pg);
        var expected = new TestDomainException(pg);
        var sut = new ExceptionTransformMiddleware([new AlwaysMatchTransformer(expected)]);

        var act = async () => await sut.InvokeAsync(() => throw wrapper);

        var thrown = await act.Should().ThrowAsync<TestDomainException>();
        thrown.Which.Should().BeSameAs(expected);
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
        var domainEx = new TestDomainException(pg);
        var sut = new ExceptionTransformMiddleware([new AlwaysMatchTransformer(domainEx)]);

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
}
