using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetMessageAttachments;
using AktBob.Shared.Types.Deskpro;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Collections.ObjectModel;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetMessageAttachments;
public class GetMessageAttachmentsHandlerExceptionTests
{
    private readonly GetMessageAttachmentsHandlerException _sut;
    private readonly IGetMessageAttachmentsHandler _inner = Substitute.For<IGetMessageAttachmentsHandler>();
    private readonly FakeLogger<GetMessageAttachmentsHandler> _logger = new FakeLogger<GetMessageAttachmentsHandler>();

    public GetMessageAttachmentsHandlerExceptionTests()
    {
        _sut = new GetMessageAttachmentsHandlerException(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var messageId = MessageId.Create(1, 1);
        var collection = new Collection<AttachmentDto>();
        var innerResult = ErrorOrFactory.From<IReadOnlyCollection<AttachmentDto>>(collection);
        var expecetedResult = ErrorOrFactory.From<IReadOnlyCollection<AttachmentDto>>(collection);
        _inner.Handle(messageId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expecetedResult);
        await _inner.Received(1).Handle(messageId, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_LogAndReturnErrorA_WhenInnerModuleThrowsException()
    {
        // Arrange
        var messageId = MessageId.Create(1, 1);
        _inner.Handle(messageId, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var result = await _sut.Handle(messageId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        await _inner.Received(1).Handle(messageId, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
