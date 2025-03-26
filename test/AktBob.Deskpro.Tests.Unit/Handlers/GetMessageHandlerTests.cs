using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers;
public class GetMessageHandlerTests
{
    private readonly GetMessageHandler _sut;
    private readonly IDeskproClient _deskproClient = Substitute.For<IDeskproClient>();

    public GetMessageHandlerTests()
    {
        _sut = new GetMessageHandler(_deskproClient);
    }

    [Fact]
    public async Task Handle_ShouldReturnMessageDtoResult_WhenDeskproClientReturnsMessage()
    {
        // Arrange
        var messageId = 1;
        var deskproMessage = new Message { Id = messageId };
        var expectedMessage = new MessageDto {Id = messageId };

        _deskproClient
            .GetMessage(Arg.Any<int>(), messageId, Arg.Any<CancellationToken>())
            .Returns(deskproMessage);


        // Act
        var result = await _sut.Handle(1, messageId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedMessage);
        await _deskproClient.Received(1).GetMessage(Arg.Any<int>(), messageId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnErrorResultWithMessage_WhenDeskproClientReturnsNull()
    {
        // Arrange
        _deskproClient.GetMessage(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _sut.Handle(1, 1, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetMessage(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());

    }

    [Fact]
    public async Task Handle_ShouldReturnErrorResultWithMessage_WhenDeskproClientThrowsHttpRequestException()
    {
        // Arrange
        _deskproClient.GetMessage(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ThrowsAsync<HttpRequestException>();

        // Act
        var act = () => _sut.Handle(1, 1, CancellationToken.None);
        var result = await act();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetMessage(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShoudlRethrowException_WhenDeskproClientThrowsAnyExceptionOtherThanHttpRequestExption()
    {
        // Arrange
        _deskproClient.GetMessage(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Handle(1, 1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _deskproClient.Received(1).GetMessage(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
