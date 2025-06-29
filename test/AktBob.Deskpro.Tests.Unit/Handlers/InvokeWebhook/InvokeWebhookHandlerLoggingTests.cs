using Aktbob.Modules.Deskpro.Features.InvokeWebhook;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Deskpro.Tests.Unit.Handlers.InvokeWebhook;
public class InvokeWebhookHandlerLoggingTests
{
    private readonly InvokeWebhookHandlerLogging _sut;
    private readonly IInvokeWebhookHandler _inner = Substitute.For<IInvokeWebhookHandler>();
    private readonly FakeLogger<InvokeWebhookHandler> _logger = new FakeLogger<InvokeWebhookHandler>();

    public InvokeWebhookHandlerLoggingTests()
    {
        _sut = new InvokeWebhookHandlerLogging(_inner, _logger);
    }

    [Fact]
    public void InvokeWebhook_ShouldInvokeInnerAndLogInformation_WhenInvoked()
    {
        // Arrange
        var webhookId = "webhookId";
        var payload = "payload";

        // Act
        _sut.Handle(webhookId, payload, CancellationToken.None);

        // Assert
        _inner.Received(1).Handle(webhookId, payload, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }
}
