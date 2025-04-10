using AktBob.Email.Contracts;
using AktBob.Email.Decorators;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Email.Tests.Unit.Decorators;
public class ModuleLoggingDecoratorTests
{
    private readonly ModuleLoggingDecorator _sut;
    private readonly IEmailModule _inner = Substitute.For<IEmailModule>();
    private readonly FakeLogger<EmailModule> _logger = new FakeLogger<EmailModule>();

    public ModuleLoggingDecoratorTests()
    {
        _sut = new ModuleLoggingDecorator(_inner, _logger);
    }

    [Fact]
    public void Send_ShouldCallInnerAndLogInformation_WhenInvoked()
    {
        // Arrange
        var to = "to";
        var subject = "subject";
        var body = "body";

        // Act
        _sut.Send(to, subject, body, false);

        // Assert
        _inner.Received(1).Send(Arg.Is(to), Arg.Is(subject), Arg.Is(body));
        _logger.Collector.LatestRecord.Level.Should().Be(Microsoft.Extensions.Logging.LogLevel.Information);
    }
}
