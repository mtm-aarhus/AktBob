using AktBob.Email.Handler;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Email.Tests.Unit.Handler;

public class SendEmailHandlerLoggingTests
{
    private readonly SendEmailHandlerLogging _sut;
    private readonly ISendEmailHandler _inner = Substitute.For<ISendEmailHandler>();
    private readonly FakeLogger<SendEmailHandler> _logger = new FakeLogger<SendEmailHandler>();

    public SendEmailHandlerLoggingTests()
    {
        _sut = new SendEmailHandlerLogging(_inner, _logger);
    }

    [Fact]
    public void Send_ShouldCallInnerAndLogInformation_WhenInvoked()
    {
        // Arrange
        var to = "to";
        var subject = "subject";
        var body = "body";

        // Act
        _sut.Handle(to, subject, body, false);

        // Assert
        _inner.Received(1).Handle(Arg.Is(to), Arg.Is(subject), Arg.Is(body));
        _logger.Collector.LatestRecord.Level.Should().Be(Microsoft.Extensions.Logging.LogLevel.Information);
    }
}