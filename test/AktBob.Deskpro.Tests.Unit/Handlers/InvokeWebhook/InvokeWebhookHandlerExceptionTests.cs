using AktBob.Deskpro.Handlers.InvokeWebhook;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers.InvokeWebhook;
public class InvokeWebhookHandlerExceptionTests
{
    private readonly InvokeWebhookHandlerException _sut;
    private readonly IInvokeWebhookHandler _inner = Substitute.For<IInvokeWebhookHandler>();
    private readonly FakeLogger<InvokeWebhookHandler> _logger = new FakeLogger<InvokeWebhookHandler>();

    public InvokeWebhookHandlerExceptionTests()
    {
        _sut = new InvokeWebhookHandlerException(_inner, _logger);
    }

    [Fact]
    public void Handle_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var webhookId = "webhook id";
        var payload = "payload";

        // Act
        _sut.Handle(webhookId, payload, CancellationToken.None);

        // Assert
        _inner.Received(1).Handle(webhookId, payload, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_LogAndReturn_WhenInnerModuleThrowsException()
    {
        // Arrange
        var webhookId = "webhook id";
        var payload = "payload";
        _inner
            .When(x => x.Handle(webhookId, payload, CancellationToken.None))
            .Do(x => throw new Exception());

        // Act
        var result = await _sut.Handle(webhookId, payload, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        _inner.Received(1).Handle(webhookId, payload, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
