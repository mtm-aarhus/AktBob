using AktBob.CloudConvert.Handlers.ConvertHtmlToPdf;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.CloudConvert.Tests.Unit.Handlers.ConvertHtmlToPdf;
public class ConvertHtmlToPdfHandlerExceptionTests
{
    private readonly ConvertHtmlToPdfHandlerException _sut;
    private readonly IConvertHtmlToPdfHandler _inner = Substitute.For<IConvertHtmlToPdfHandler>();
    private readonly FakeLogger<ConvertHtmlToPdfHandler> _logger = new FakeLogger<ConvertHtmlToPdfHandler>();

    public ConvertHtmlToPdfHandlerExceptionTests()
    {
        _sut = new ConvertHtmlToPdfHandlerException(_inner, _logger);
    }

    [Fact]
    public async Task ConvertHtmlToPdf_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var tasks = new Dictionary<Guid, object>();
        var innerResult = ErrorOrFactory.From(jobId);

        _inner
            .Handle(tasks, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        var act = () => _sut.Handle(tasks, CancellationToken.None);
        var result = await act();

        // Assert
        result.Should().Be(innerResult);
        await act.Should().NotThrowAsync();
    }


    [Fact]
    public async Task ConvertHtmlToPdf_ShouldReturnError_WhenInnerModuleFails()
    {
        // Arrange
        var tasks = new Dictionary<Guid, object>();
        _inner
           .Handle(Arg.Any<IReadOnlyDictionary<Guid, object>>(), Arg.Any<CancellationToken>())
           .ThrowsAsync(new Exception());

        // Act
        var result = await _sut.Handle(tasks, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Critical);
    }
}
