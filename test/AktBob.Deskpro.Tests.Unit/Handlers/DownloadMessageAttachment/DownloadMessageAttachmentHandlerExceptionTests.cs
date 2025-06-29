using Aktbob.Modules.Deskpro.Features.DownloadMessageAttachment;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using ErrorOr;
using NSubstitute.ExceptionExtensions;
using Microsoft.Extensions.Logging;

namespace AktBob.Deskpro.Tests.Unit.Handlers.DownloadMessageAttachment;
public class DownloadMessageAttachmentHandlerExceptionTests
{
    private readonly DownloadMessageAttachmentHandlerException _sut;
    private readonly IDownloadMessageAttachmentHandler _inner = Substitute.For<IDownloadMessageAttachmentHandler>();
    private readonly FakeLogger<DownloadMessageAttachmentHandler> _logger = new FakeLogger<DownloadMessageAttachmentHandler>();

    public DownloadMessageAttachmentHandlerExceptionTests()
    {
        _sut = new DownloadMessageAttachmentHandlerException(_inner, _logger);
    }


    [Fact]
    public async Task Handle_ReturnInnerResult_WhenInnerResponseIsSuccessful()
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
        await _inner.Received(1).Handle(downloadUrl, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_LogAndReturnError_WhenInnerModuleThrowsException()
    {
        // Arrange
        var downloadUrl = "download url";
        _inner.Handle(downloadUrl, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var result = await _sut.Handle(downloadUrl, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        await _inner.Received(1).Handle(downloadUrl, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
