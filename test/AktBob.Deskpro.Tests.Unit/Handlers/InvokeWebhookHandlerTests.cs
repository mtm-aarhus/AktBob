using AAK.Deskpro;
using AktBob.Deskpro.Handlers;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers;
public class InvokeWebhookHandlerTests
{
    private readonly InvokeWebhookHandler _sut;
    private readonly IDeskproClient _deskproClient = Substitute.For<IDeskproClient>();

    public InvokeWebhookHandlerTests()
    {
        _sut = new InvokeWebhookHandler(_deskproClient);
    }

    [Fact]
    public async Task Handle_ShouldPostWebhook_WhenInvoked()
    {
        // Arrange
        var webhookId = "webhookId";
        var payload = "payload";

        // Act
        await _sut.Handle(webhookId, payload, CancellationToken.None);

        // Assert
        await _deskproClient.Received(1).PostWebhook(webhookId, payload, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRethrowException_WhenDeskproClientThrowsException()
    {
        // Arrange
        var webhookId = "webhookId";
        var payload = "payload";
        _deskproClient.PostWebhook(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Handle(webhookId, payload, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _deskproClient.Received(1).PostWebhook(webhookId, payload, Arg.Any<CancellationToken>());
    }
}
