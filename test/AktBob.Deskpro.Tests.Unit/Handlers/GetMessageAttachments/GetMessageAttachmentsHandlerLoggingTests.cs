using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetMessageAttachments;
using AktBob.Shared.Types.Deskpro;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetMessageAttachments;
public class GetMessageAttachmentsHandlerLoggingTests
{
    private readonly GetMessageAttachmentsHandlerLogging _sut;
    private readonly IGetMessageAttachmentsHandler _inner = Substitute.For<IGetMessageAttachmentsHandler>();
    private readonly FakeLogger<GetMessageAttachmentsHandler> _logger = new FakeLogger<GetMessageAttachmentsHandler>();

    public GetMessageAttachmentsHandlerLoggingTests()
    {
        _sut = new GetMessageAttachmentsHandlerLogging(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var messageId = MessageId.Create(1, 1);
        var innerResult = ErrorOrFactory.From<IReadOnlyCollection<AttachmentDto>>(new List<AttachmentDto>());
        var expectedResult = ErrorOrFactory.From<IReadOnlyCollection<AttachmentDto>>(new List<AttachmentDto>());
        _inner.Handle(messageId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(messageId, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(expectedResult.Value);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).Handle(messageId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var messageId = MessageId.Create(1, 1);
        var innerResult = Error.Failure().ToErrorOr<IReadOnlyCollection<AttachmentDto>>();
        var expectedResult = Error.Failure().ToErrorOr<IReadOnlyCollection<AttachmentDto>>();
        _inner.Handle(messageId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(messageId, CancellationToken.None);

        // Assert
        result.Value.Should().BeEquivalentTo(expectedResult.Value);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
        await _inner.Received(1).Handle(messageId, Arg.Any<CancellationToken>());
    }

}
