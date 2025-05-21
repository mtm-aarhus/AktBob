using AktBob.Shared;
using NSubstitute;
using AktBob.Deskpro.Handlers.GetMessage;
using ErrorOr;
using FluentAssertions;
using NSubstitute.ReturnsExtensions;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Shared.Types.Deskpro;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetMessage;
public class GetMessageHandlerCachingTests
{
    private readonly GetMessageHandlerCaching _sut;
    private readonly IGetMessageHandler _inner = Substitute.For<IGetMessageHandler>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();

    public GetMessageHandlerCachingTests()
    {
        _sut = new GetMessageHandlerCaching(_inner, _cache);
    }

    [Fact]
    public async Task Handle_ShouldReturnCachedValue_WhenCacheIsHitAndValueIsNotNull()
    {
        // Arrange
        var messageId = MessageId.Create(1, 1);
        var cacheKey = $"Deskpro_Message_{messageId.TicketId}_{messageId.Id}";
        var cachedValue = new MessageDto();
        var expectedResult = ErrorOrFactory.From(cachedValue);
        _cache.Get<MessageDto>(Arg.Is(cacheKey)).Returns(cachedValue);

        // Act
        var result = await _sut.Handle(messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _cache.Received(1).Get<MessageDto>(Arg.Is(cacheKey));
        _cache.Received(0).Set(Arg.Any<string>(), Arg.Any<Arg.AnyType>(), Arg.Any<TimeSpan>());
        await _inner.Received(0).Handle(Arg.Any<MessageId>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldCallInnerAndCacheResult_WhenCacheIsMiss()
    {
        // Arrange
        var messageId = MessageId.Create(1, 1);
        var cacheKey = $"Deskpro_Message_{messageId.TicketId}_{messageId.Id}";
        var message = new MessageDto();
        var innerResult = ErrorOrFactory.From(message);
        _inner.Handle(Arg.Is(messageId), Arg.Any<CancellationToken>()).Returns(innerResult);
        _cache.Get<MessageDto>(Arg.Is(cacheKey)).ReturnsNull();

        // Act
        await _sut.Handle(messageId, CancellationToken.None);

        // Assert
        _cache.Received(1).Get<MessageDto>(Arg.Is(cacheKey));
        _cache.Received(1).Set(Arg.Is(cacheKey), Arg.Is(message), Arg.Any<TimeSpan>());
        await _inner.Received(1).Handle(Arg.Is(messageId), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Handle_ShouldNotCache_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var messageId = MessageId.Create(1, 1);
        var cacheKey = $"Deskpro_Message_{messageId.TicketId}_{messageId.Id}";
        var innerResult = Error.Failure().ToErrorOr<MessageDto>();
        _inner.Handle(Arg.Is(messageId), Arg.Any<CancellationToken>()).Returns(innerResult);
        _cache.Get<MessageDto>(Arg.Is(cacheKey)).ReturnsNull();

        // Act
        await _sut.Handle(messageId, CancellationToken.None);

        // Assert
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<MessageDto>(), Arg.Any<TimeSpan>());
    }
}
