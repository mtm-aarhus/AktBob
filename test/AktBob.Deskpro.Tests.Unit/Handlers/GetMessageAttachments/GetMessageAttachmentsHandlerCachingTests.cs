using AktBob.Shared;
using NSubstitute;
using AktBob.Deskpro.Handlers.GetMessageAttachments;
using NSubstitute.ReturnsExtensions;
using System.Collections.ObjectModel;
using ErrorOr;
using FluentAssertions;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetMessageAttachments;
public class GetMessageAttachmentsHandlerCachingTests
{
    private readonly GetMessageAttachmentsHandlerCaching _sut;
    private readonly IGetMessageAttachmentsHandler _inner = Substitute.For<IGetMessageAttachmentsHandler>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();

    public GetMessageAttachmentsHandlerCachingTests()
    {
        _sut = new GetMessageAttachmentsHandlerCaching(_inner, _cache);
    }

    [Fact]
    public async Task Handle_ShouldReturnCachedValule_WhenCacheIsHit()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var cacheKey = $"Deskpro_MessageAttachments_{ticketId}_{messageId}";
        var cachedValue = new Collection<AttachmentDto>();
        var expectedResult = ErrorOrFactory.From<IReadOnlyCollection<AttachmentDto>>(cachedValue);
        _cache.Get<IReadOnlyCollection<AttachmentDto>>(Arg.Is(cacheKey)).Returns(cachedValue);

        // Act
        var result = await _sut.Handle(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _cache.Received(1).Get<IReadOnlyCollection<AttachmentDto>>(Arg.Is(cacheKey));
        _cache.Received(0).Set(Arg.Any<string>(), Arg.Any<IReadOnlyCollection<AttachmentDto>>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task Handle_ShouldReturnInnerResultAndCacheResult_WhenCacheIsMiss()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var cacheKey = $"Deskpro_MessageAttachments_{ticketId}_{messageId}";
        _cache.Get<IReadOnlyCollection<AttachmentDto>>(Arg.Is(cacheKey)).ReturnsNull();
        var innerResult = ErrorOrFactory.From<IReadOnlyCollection<AttachmentDto>>(new Collection<AttachmentDto>());
        _inner.Handle(Arg.Is(ticketId), Arg.Is(messageId), Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().Be(innerResult);
        await _inner.Received(1).Handle(Arg.Is(ticketId), Arg.Is(messageId), Arg.Any<CancellationToken>());
        _cache.Received(1).Get<IReadOnlyCollection<AttachmentDto>>(Arg.Is(cacheKey));
        _cache.Received(1).Set(Arg.Is(cacheKey), Arg.Is(innerResult.Value), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task Handle_ShouldNotCache_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var cacheKey = $"Deskpro_MessageAttachments_{ticketId}_{messageId}";
        _cache.Get<IReadOnlyCollection<AttachmentDto>>(Arg.Is(cacheKey)).ReturnsNull();
        var innerResult = Error.Failure().ToErrorOr<IReadOnlyCollection<AttachmentDto>>();
        _inner.Handle(Arg.Is(ticketId), Arg.Is(messageId), Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        await _sut.Handle(ticketId, messageId, CancellationToken.None);

        // Assert
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<Arg.AnyType>(), Arg.Any<TimeSpan>());
    }
}
