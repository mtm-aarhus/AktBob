using AAK.Deskpro;
using AktBob.Deskpro.Handlers;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using System.Text;

namespace AktBob.Deskpro.Tests.Unit.Handlers;
public class DownloadMessageAttachmentHandlerTests
{
    private readonly DownloadMessageAttachmentHandler _sut;
    private readonly IDeskproClient _deskproClient = Substitute.For<IDeskproClient>();

    public DownloadMessageAttachmentHandlerTests()
    {
        _sut = new DownloadMessageAttachmentHandler(_deskproClient);
    }

    [Fact]
    public async Task Handle_ShouldReturnStreamResult_WhenDeskproClientResponseIsSuccessful()
    {
        // Arrange
        var bytes = Encoding.UTF8.GetBytes("some value");
        using var stream = new MemoryStream(bytes);
        _deskproClient.DownloadAttachment(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(stream);

        // Act
        var result = await _sut.Handle(string.Empty, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(stream);
        await _deskproClient.Received(1).DownloadAttachment(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnErrorResultWithMessage_WhenDeskproClientResponseIsNull()
    {
        // Arrange
        _deskproClient.DownloadAttachment(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _sut.Handle(string.Empty, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).DownloadAttachment(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnErrorResultWithMessage_WhenDeskproClientThrowsHttpRequestException()
    {
        // Arrange
        _deskproClient.DownloadAttachment(Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync<HttpRequestException>();

        // Act
        var act = () => _sut.Handle(string.Empty, CancellationToken.None);
        var result = await act();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).DownloadAttachment(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRethrowException_WhenDeskproClientThrowsAnyExceptionOtherThanHttpRequestException()
    {
        // Arrange
        _deskproClient.DownloadAttachment(Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Handle(string.Empty, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _deskproClient.Received(1).DownloadAttachment(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
