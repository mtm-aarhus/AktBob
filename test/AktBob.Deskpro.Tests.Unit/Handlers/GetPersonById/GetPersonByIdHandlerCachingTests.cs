using AktBob.Shared;
using NSubstitute;
using ErrorOr;
using AktBob.Deskpro.Handlers.GetPersonById;
using FluentAssertions;
using AktBob.Deskpro.Contracts;
using NSubstitute.ReturnsExtensions;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetPersonById;
public class GetPersonByIdHandlerCachingTests
{
    private readonly GetPersonByIdHandlerCaching _sut;
    private readonly IGetPersonByIdHandler _inner = Substitute.For<IGetPersonByIdHandler>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();

    public GetPersonByIdHandlerCachingTests()
    {
        _sut = new GetPersonByIdHandlerCaching(_inner, _cache);
    }

    [Fact]
    public async Task GetPerson_ById_ShouldReturnCachedPerson_WhenCacheIsHit()
    {
        // Arrange
        var personId = 1;
        var cacheKey = $"Deskpro_Person_{personId}";
        var cachedValue = new PersonDto();
        var expectedValue = ErrorOrFactory.From(cachedValue);
        _cache.Get<PersonDto>(Arg.Is(cacheKey)).Returns(cachedValue);

        // Act
        var result = await _sut.Handle(personId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedValue);
        _cache.Received(1).Get<PersonDto>(Arg.Is(cacheKey));
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<Arg.AnyType>(), Arg.Any<TimeSpan>());
        await _inner.Received(0).Handle(Arg.Is(personId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPerson_ById_ShouldReturnInnerResultAndCacheResult_WhenCacheIsMiss()
    {
        // Arrange
        var personId = 1;
        var cacheKey = $"Deskpro_Person_{personId}";
        var person = new PersonDto();
        var innerResult = ErrorOrFactory.From(person);
        _cache.Get<PersonDto>(Arg.Is(cacheKey)).ReturnsNull();
        _inner.Handle(Arg.Is(personId), Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(personId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(innerResult);
        await _inner.Received(1).Handle(Arg.Is(personId), Arg.Any<CancellationToken>());
        _cache.Received(1).Get<PersonDto>(Arg.Is(cacheKey));
        _cache.Received(1).Set(Arg.Is(cacheKey), Arg.Is(innerResult.Value), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task GetPerson_ById_ShouldNotCache_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var personId = 1;
        var cacheKey = $"Deskpro_Person_{personId}";
        _cache.Get<PersonDto>(Arg.Is(cacheKey)).ReturnsNull();
        var innerResult = Error.Failure().ToErrorOr<PersonDto>();
        _inner.Handle(Arg.Is(personId), Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        await _sut.Handle(personId, CancellationToken.None);

        // Assert
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<Arg.AnyType>(), Arg.Any<TimeSpan>());
    }
}
