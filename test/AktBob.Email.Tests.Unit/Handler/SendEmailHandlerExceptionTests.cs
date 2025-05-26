using AktBob.Email.Handler;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Email.Tests.Unit.Handler;
public class SendEmailHandlerExceptionTests
{
    private readonly SendEmailHandlerException _sut;
    private readonly ISendEmailHandler _inner = Substitute.For<ISendEmailHandler>();
    private readonly FakeLogger<SendEmailHandler> _logger = new FakeLogger<SendEmailHandler>();

    public SendEmailHandlerExceptionTests()
    {
        _sut = new SendEmailHandlerException(_inner, _logger);
    }

    [Fact]
    public void Send_ShouldCallInner_WhenInvoked()
    {
        // Arrange
        var to = "to";
        var subject = "subject";
        var body = "body";

        // Act
        _sut.Handle(to, subject, body);

        // Assert
        _inner.Received(1).Handle(Arg.Is(to), Arg.Is(subject), Arg.Is(body));
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public void Send_ShouldLogErrorAndRethrowException_WhenInnerSendThrowsException()
    {
        // Arrange
        var to = "to";
        var subject = "subject";
        var body = "body";
        _inner
            .When(x => x.Handle(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()))
            .Do(call => throw new Exception());

        // Act
        var act = () => _sut.Handle(to, subject, body);

        // Assert
        act.Should().Throw<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(Microsoft.Extensions.Logging.LogLevel.Error);
    }
}