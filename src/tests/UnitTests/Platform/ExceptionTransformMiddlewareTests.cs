using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Zss.BilliardHall.Platform.Exceptions;
using Zss.BilliardHall.Platform.Infrastructure;

namespace Zss.BilliardHall.Tests.UnitTests.Platform;

public sealed class ExceptionTransformMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNext()
    {
        // Arrange
        var nextCalled = false;
        RequestDelegate next = _ =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };
        var middleware = new ExceptionTransformMiddleware(next, []);
        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenNonPostgresException_RethrowsOriginal()
    {
        // Arrange
        var originalException = new InvalidOperationException("测试异常");
        RequestDelegate next = _ => throw originalException;
        var middleware = new ExceptionTransformMiddleware(next, []);
        var context = new DefaultHttpContext();

        // Act
        var act = () => middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("测试异常");
    }

    [Fact]
    public async Task InvokeAsync_WhenNonPostgresException_DoesNotCallTransformers()
    {
        // Arrange
        var transformerCalled = false;
        var mockTransformer = new TrackingTransformer(() => transformerCalled = true);
        var originalException = new InvalidOperationException("测试异常");
        RequestDelegate next = _ => throw originalException;
        var middleware = new ExceptionTransformMiddleware(next, [mockTransformer]);
        var context = new DefaultHttpContext();

        // Act
        var act = () => middleware.InvokeAsync(context);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
        transformerCalled.Should().BeFalse("普通异常不应触发 transformer");
    }

    // 注意：无法直接测试 PostgresException 场景，因为 PostgresException 的构造函数是 internal 的
    // PostgresException 转换的集成测试应通过端到端测试覆盖

    private sealed class TestDomainException(string errorCode, string message)
        : DomainException(errorCode, message);

    private sealed class TrackingTransformer(Action onTransform) : IPostgresExceptionTransformer
    {
        public DomainException? TryTransform(Exception ex)
        {
            onTransform();
            return null;
        }
    }
}
