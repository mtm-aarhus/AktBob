using AktBob.OpenOrchestrator.Contracts;
using AktBob.OpenOrchestrator.Decorators;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.OpenOrchestrator.Tests.Unit.Decorators;

public class ModuleLoggingDecoratorTests
{
    private readonly ModuleLoggingDecorator _sut;
    private readonly IOpenOrchestratorModule _inner = Substitute.For<IOpenOrchestratorModule>();
    private readonly FakeLogger<OpenOrchestratorModule> _logger = new FakeLogger<OpenOrchestratorModule>();

    public ModuleLoggingDecoratorTests()
    {
        _sut = new ModuleLoggingDecorator(_inner, _logger);
    }

    [Fact]
    public void CreateQueueItem_ShouldLogInformationAndInvokeInnerModule_WhenInvoked()
    {
        // Arrange
        var command = new CreateQueueItemCommand(string.Empty, string.Empty, string.Empty);

        // Act
        _sut.CreateQueueItem(command);

        // Assert
        _logger.Collector.LatestRecord.Level.Should().Be(Microsoft.Extensions.Logging.LogLevel.Information);
        _inner.Received(1).CreateQueueItem(command);
    }
}
