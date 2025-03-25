using AktBob.OpenOrchestrator.Contracts;
using AktBob.OpenOrchestrator.Decorators;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.OpenOrchestrator.Tests.Unit.Decorators;

public class ModuleExceptionDecoratorTests
{
    private readonly ModuleExceptionDecorator _sut;
    private readonly IOpenOrchestratorModule _inner = Substitute.For<IOpenOrchestratorModule>();
    private readonly FakeLogger<OpenOrchestratorModule> _logger = new FakeLogger<OpenOrchestratorModule>();

    public ModuleExceptionDecoratorTests()
    {
        _sut = new ModuleExceptionDecorator(_inner, _logger);
    }

    [Fact]
    public void CreateQueueItem_ShouldInvokeInnerModule_WhenInvoked()
    {
        // Arrange
        var command = new CreateQueueItemCommand(string.Empty, string.Empty, string.Empty);
            
        // Act
        _sut.CreateQueueItem(command);

        // Assert
        _inner.Received(1).CreateQueueItem(Arg.Any<CreateQueueItemCommand>());
        _logger.Collector.Count.Should().Be(0);

    }

    [Fact]
    public void CreateQueueItem_ShouldLogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var command = new CreateQueueItemCommand(string.Empty, string.Empty, string.Empty);
        _inner.When(x => x.CreateQueueItem(command)).Throw<Exception>();

        // Act
        var act = () => _sut.CreateQueueItem(command);

        // Assert
        act.Should().Throw<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
