using Aktbob.Modules.Deskpro.Features.DownloadMessageAttachment;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using ErrorOr;
using Microsoft.Extensions.Logging;

namespace AktBob.Deskpro.Tests.Unit.Handlers.DownloadMessageAttachment;
public class DownloadMessageAttachmentHandlerLoggingTests
{
    private readonly DownloadMessageAttachmentHandlerLogging _sut;
    private readonly IDownloadMessageAttachmentHandler _inner = Substitute.For<IDownloadMessageAttachmentHandler>();
    private readonly FakeLogger<DownloadMessageAttachmentHandler> _logger = new FakeLogger<DownloadMessageAttachmentHandler>();

    public DownloadMessageAttachmentHandlerLoggingTests()
    {
        _sut = new DownloadMessageAttachmentHandlerLogging(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var downloadUrl = "download url";
        using Stream stream = new MemoryStream();
        var innerResult = ErrorOrFactory.From(stream);
        var expectedResult = ErrorOrFactory.From(stream);
        _inner.Handle(downloadUrl, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(downloadUrl, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).Handle(downloadUrl, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLogAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var downloadUrl = "download url";
        var error = Error.Failure().ToErrorOr<Stream>();
        _inner.Handle(downloadUrl, Arg.Any<CancellationToken>()).Returns(error);

        // Act
        var result = await _sut.Handle(downloadUrl, CancellationToken.None);

        // Assert
        result.Should().Be(error);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
        await _inner.Received(1).Handle(downloadUrl, Arg.Any<CancellationToken>());
    }
}
