using AktBob.CloudConvert.Handlers.GenerateTasks;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.CloudConvert.Tests.Unit.Handlers.GenerateTasks;
public class GenerateTasksHandlerLoggingTests
{
    private readonly GenerateTasksHandlerLogging _sut;
    private readonly IGenerateTasksHandler _inner = Substitute.For<IGenerateTasksHandler>();
    private readonly FakeLogger<GenerateTasksHandler> _logger = new FakeLogger<GenerateTasksHandler>();

    public GenerateTasksHandlerLoggingTests()
    {
        _sut = new GenerateTasksHandlerLogging(_inner, _logger);
    }

    [Fact]
    public void GenerateTasks_ShouldLogInformation_WhenInvoked()
    {
        // Arrange
        var innerResult = new Dictionary<Guid, object>().AsReadOnly();
        var items = Enumerable.Empty<byte[]>();

        _inner
            .Handle(Arg.Any<IEnumerable<byte[]>>())
            .Returns(innerResult);

        // Act
        _sut.Handle(items);

        // Assert
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public void GenerateTasks_ShouldLogWarning_WhenResultIsNotSuccesful()
    {
        // Arrange
        var items = Enumerable.Empty<byte[]>();

        _inner
            .Handle(Arg.Any<IEnumerable<byte[]>>())
            .Returns(Error.Failure().ToErrorOr<IReadOnlyDictionary<Guid, object>>());

        // Act
        var result = _sut.Handle(items);

        // Assert
        result.IsError.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
    }
}
