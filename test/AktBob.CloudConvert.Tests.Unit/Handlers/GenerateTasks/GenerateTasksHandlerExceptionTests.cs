using AktBob.CloudConvert.Handlers.GenerateTasks;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.CloudConvert.Tests.Unit.Handlers.GenerateTasks;
public class GenerateTasksHandlerExceptionTests
{
    private readonly GenerateTasksHandlerException _sut;
    private readonly IGenerateTasksHandler _inner = Substitute.For<IGenerateTasksHandler>();
    private readonly FakeLogger<GenerateTasksHandler> _logger = new FakeLogger<GenerateTasksHandler>();

    public GenerateTasksHandlerExceptionTests()
    {
        _sut = new GenerateTasksHandlerException(_inner, _logger);
    }


    [Fact]
    public void GenerateTasks_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var items = new List<byte[]>();
        IReadOnlyDictionary<Guid, object> innerResult = new Dictionary<Guid, object>();

        _inner
            .Handle(items)
            .Returns(ErrorOrFactory.From(innerResult));

        // Act
        var result = _sut.Handle(items);

        // Assert
        result.Should().BeEquivalentTo(ErrorOrFactory.From(innerResult));
    }


    [Fact]
    public void GenerateTasks_ShouldReturnError_WhenInnerModuleFails()
    {
        // Arrange
        var items = new List<byte[]>();
        _inner
            .Handle(Arg.Any<IEnumerable<byte[]>>())
            .Throws<Exception>();

        // Act
        var result = _sut.Handle(items);

        // Assert
        result.IsError.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(Microsoft.Extensions.Logging.LogLevel.Critical);
    }
}
