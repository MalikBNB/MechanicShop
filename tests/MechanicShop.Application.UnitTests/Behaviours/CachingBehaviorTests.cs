using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Application.Common.Interfaces;
using MechanicShop.Domain.Common.Results;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using static MechanicShop.Application.UnitTests.Behaviours.CachingBehaviorTests;

namespace MechanicShop.Application.UnitTests.Behaviours;

public class CachingBehaviorTests
{
    private readonly HybridCache _cache = Substitute.For<HybridCache>();
    private readonly ILogger<CachingBehavior<CachedQuery, Result<string>>> _logger = Substitute.For<ILogger<CachingBehavior<CachedQuery, Result<string>>>>();

    private readonly CachingBehavior<CachedQuery, Result<string>> _sut = Substitute.For<CachingBehavior<CachedQuery, Result<string>>>();

    public CachingBehaviorTests()
    {
        _sut = new CachingBehavior<CachedQuery, Result<string>>(_cache, _logger);
    }

    [Fact]
    public async Task Handle_WhenNotCachedQuery_ShouldSkipCacheAndReturnResult()
    {
        // Arrange
        var unCachedQueryRequest = new NonCachedQuery();
        var behavior = new CachingBehavior<NonCachedQuery, string>(_cache, Substitute.For<ILogger<CachingBehavior<NonCachedQuery, string>>>());

        // Act
        var result = await behavior.Handle(unCachedQueryRequest, _ => Task.FromResult("OK"), CancellationToken.None);

        Assert.Equal("OK", result);

        // NSubstitute meaning:
        //  * DidNotReceive() → method must never be called
        //  * Arg.Any<T>() → ignore parameter values [“I don’t care about the parameter values — the method must not be called with ANY values.”].
        // Checking behavior: “Verify that the following method call NEVER happened.”
        // If SetAsync was called even once → ❌ test fails.   
        await _cache.DidNotReceive().SetAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<HybridCacheEntryOptions>(),
            Arg.Any<string[]>(),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_WhenCachedQueryAndResultIsSuccess_ShouldCacheResult()
    {
        // Arrange
        var request = new CachedQuery();
        var response = (Result<string>)"test-value";

        string? actualKey = null;
        object? actualValue = null;
        HybridCacheEntryOptions? actualOptions = null;
        string[]? actualTags = null;
        CancellationToken actualToken = default;

        _cache.SetAsync(
            Arg.Do<string>(k => actualKey = k),
            Arg.Do<object>(v => actualValue = v),
            Arg.Do<HybridCacheEntryOptions>(o => actualOptions = o),
            Arg.Do<string[]>(t => actualTags = t),
            Arg.Do<CancellationToken>(c => actualToken = c)).Returns(ValueTask.CompletedTask);

        // Act
        var result = await _sut.Handle(request, _ => Task.FromResult(response), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(request.CacheKey, actualKey);

        var typed = Assert.IsType<Result<string>>(actualValue);
        Assert.True(typed.IsSuccess);
        Assert.Equal("test-value", typed.Value);

        Assert.Equal(request.Expiration, actualOptions!.Expiration);
        Assert.Equal(request.Tags, actualTags);
    }

    [Fact]
    public async Task Handle_WhenCachedQueryAndResultIsError_ShouldNotCacheResult()
    {
        // Arrange
        var request = new CachedQuery();
        var errorResult = (Result<string>)Error.Validation("code", "message");

        // Act
        var result = await _sut.Handle(request, _ => Task.FromResult(errorResult), CancellationToken.None);

        // Assert
        Assert.True(result.IsError);

        // Inspect received calls manually
        var calls = _cache.ReceivedCalls();
        var setCalls = calls.Where(call =>
            call.GetMethodInfo().Name == nameof(HybridCache.SetAsync) &&
            call.GetMethodInfo().IsGenericMethod &&
            call.GetMethodInfo().GetGenericArguments().FirstOrDefault() == typeof(Result<string>));

        Assert.Empty(setCalls); // ✅ no call to SetAsync<Result<string>>
    }

    public class NonCachedQuery;

    public class CachedQuery : ICachedQuery
    {
        public string CacheKey => "test-key";
        public TimeSpan Expiration => TimeSpan.FromMinutes(5);
        public string[] Tags => ["unit-test"];
    }
}