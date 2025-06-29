using AktBob.Shared;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Collections.ObjectModel;
using Aktbob.Modules.Deskpro.Features.GetCustomFieldSpecifications;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using ErrorOr;
using FluentAssertions;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetCustomFieldSpecifications;
public class GetCustomFieldSpecificationsHandlerCachingTests
{
    private readonly GetCustomFieldSpecificationsHandlerCaching _sut;
    private readonly IGetCustomFieldSpecificationsHandler _inner = Substitute.For<IGetCustomFieldSpecificationsHandler>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();

    public GetCustomFieldSpecificationsHandlerCachingTests()
    {
        _sut = new GetCustomFieldSpecificationsHandlerCaching(_inner, _cache);
    }

    [Fact]
    public async Task Handle_ShouldReturnCachedValue_WhenCachedHasData()
    {
        // Arrange
        var cacheKey = "Deskpro_CustomFieldSpecifications";
        var collection = new Collection<CustomFieldSpecificationDto>
        {
            new CustomFieldSpecificationDto(1, "title", new Dictionary<int, string>())
        };

        var expectedValue = ErrorOrFactory.From<IReadOnlyCollection<CustomFieldSpecificationDto>>(collection);
        _cache.Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey)).Returns(collection);

        // Act
        var result = await _sut.Handle(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedValue);
        _cache.Received(1).Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey));
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<IReadOnlyCollection<CustomFieldSpecificationDto>>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task Handle_ShouldCallInnerAndCacheResult_WhenCacheDoesNotHaveData()
    {
        // Arrange
        var cacheKey = "Deskpro_CustomFieldSpecifications";
        var collection = new Collection<CustomFieldSpecificationDto>
        {
            new CustomFieldSpecificationDto(1, "title", new Dictionary<int, string>())
        };
        var innerResult = ErrorOrFactory.From<IReadOnlyCollection<CustomFieldSpecificationDto>>(collection);
        _inner.Handle(Arg.Any<CancellationToken>()).Returns(innerResult);
        _cache.Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey)).ReturnsNull();

        // Act
        await _sut.Handle(CancellationToken.None);

        // Assert
        await _inner.Received(1).Handle(Arg.Any<CancellationToken>());
        _cache.Received(1).Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey));
        _cache.Received(1).Set(Arg.Is(cacheKey), Arg.Is(innerResult.Value), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task Handle_ShouldCallInnerAndCacheResult_WhenCacheIsHitButDataIsEmpty()
    {
        // Arrange
        var cacheKey = "Deskpro_CustomFieldSpecifications";
        var collection = new Collection<CustomFieldSpecificationDto>
        {
            new CustomFieldSpecificationDto(1, "title", new Dictionary<int, string>())
        };
        Collection<CustomFieldSpecificationDto> cachedValue = [];
        var innerResult = ErrorOrFactory.From<IReadOnlyCollection<CustomFieldSpecificationDto>>(collection);
        _inner.Handle(Arg.Any<CancellationToken>()).Returns(innerResult);
        _cache.Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey)).Returns(cachedValue);

        // Act
        await _sut.Handle(CancellationToken.None);

        // Assert
        await _inner.Received(1).Handle(Arg.Any<CancellationToken>());
        _cache.Received(1).Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey));
        _cache.Received(1).Set(Arg.Is(cacheKey), Arg.Is(innerResult.Value), Arg.Any<TimeSpan>());
    }


    [Fact]
    public async Task Handle_ShouldNotCache_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var cacheKey = "Deskpro_CustomFieldSpecifications";
        var innerResult = Error.Failure().ToErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>();
        _inner.Handle(Arg.Any<CancellationToken>()).Returns(innerResult);
        _cache.Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey)).ReturnsNull();

        // Act
        await _sut.Handle(CancellationToken.None);

        // Assert
        await _inner.Received(1).Handle(Arg.Any<CancellationToken>());
        _cache.Received(1).Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey));
        _cache.Received(0).Set(Arg.Any<string>(), Arg.Any<Arg.AnyType>, Arg.Any<TimeSpan>());
    }
}
