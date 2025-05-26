using AktBob.Shared;
using NSubstitute;
using AktBob.Deskpro.Handlers.GetPersonByEmail;
using AktBob.Deskpro.Handlers.GetPerson;
using ErrorOr;
using FluentAssertions;
using NSubstitute.ReturnsExtensions;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetPersonByEmail;
public class GetPersonByEmailHandlerCachingTests
{
    private readonly GetPersonByEmailHandlerCaching _sut;
    private readonly IGetPersonByEmailHandler _inner = Substitute.For<IGetPersonByEmailHandler>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();

    public GetPersonByEmailHandlerCachingTests()
    {
        _sut = new GetPersonByEmailHandlerCaching(_inner, _cache);
    }

    [Fact]
    public async Task Handle_ShouldReturnCachedPerson_WhenCacheIsHitAndDataIsNotNull()
    {
        // Arrange
        var personEmail = "email";
        var cacheKey = $"Deskpro_Person_{personEmail}";
        var cachedValue = new PersonDto();
        var expectedValue = ErrorOrFactory.From(cachedValue);
        _cache.Get<PersonDto>(Arg.Is(cacheKey)).Returns(cachedValue);

        // Act
        var result = await _sut.Handle(personEmail, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedValue);
        _cache.Received(1).Get<PersonDto>(Arg.Is(cacheKey));
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<Arg.AnyType>(), Arg.Any<TimeSpan>());
        await _inner.Received(0).Handle(Arg.Is(personEmail), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallInnerAndCacheResult_WhenCacheIsMiss()
    {
        // Arrange
        var personEmail = "email";
        var cacheKey = $"Deskpro_Person_{personEmail}";
        var person = new PersonDto();
        var innerResult = ErrorOrFactory.From(person);
        _cache.Get<PersonDto>(Arg.Is(cacheKey)).ReturnsNull();
        _inner.Handle(Arg.Is(personEmail), Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(personEmail, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(innerResult);
        await _inner.Received(1).Handle(Arg.Is(personEmail), Arg.Any<CancellationToken>());
        _cache.Received(1).Get<PersonDto>(Arg.Is(cacheKey));
        _cache.Received(1).Set(Arg.Is(cacheKey), Arg.Is(innerResult.Value), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task Handle_ShouldNotCache_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var personEmail = "email";
        var cacheKey = $"Deskpro_Person_{personEmail}";
        _cache.Get<PersonDto>(Arg.Is(cacheKey)).ReturnsNull();
        var innerResult = Error.Failure().ToErrorOr<PersonDto>();
        _inner.Handle(Arg.Is(personEmail), Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        await _sut.Handle(personEmail, CancellationToken.None);

        // Assert
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<Arg.AnyType>(), Arg.Any<TimeSpan>());
    }
}
