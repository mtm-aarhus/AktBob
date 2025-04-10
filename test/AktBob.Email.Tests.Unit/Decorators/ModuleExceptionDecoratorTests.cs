using AktBob.Email.Contracts;
using AktBob.Email.Decorators;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Email.Tests.Unit.Decorators;
public class ModuleExceptionDecoratorTests
{
    private readonly ModuleExceptionDecorator _sut;
    private readonly IEmailModule _inner = Substitute.For<IEmailModule>();
    private readonly FakeLogger<EmailModule> _logger = new FakeLogger<EmailModule>();

    public ModuleExceptionDecoratorTests()
    {
        _sut = new ModuleExceptionDecorator(_inner, _logger);
    }

    [Fact]
    public void Send_ShouldCallInner_WhenInvoked()
    {
        // Arrange
        var to = "to";
        var subject = "subject";
        var body = "body";

        // Act
        _sut.Send(to, subject, body);

        // Assert
        _inner.Received(1).Send(Arg.Is(to), Arg.Is(subject), Arg.Is(body));
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
            .When(x => x.Send(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()))
            .Do(call => throw new Exception());

        // Act
        var act = () => _sut.Send(to, subject, body);

        // Assert
        act.Should().Throw<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(Microsoft.Extensions.Logging.LogLevel.Error);
    }
}