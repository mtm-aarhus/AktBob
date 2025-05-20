using AktBob.CloudConvert.Handlers.ConvertHtmlToPdf;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.CloudConvert.Tests.Unit.Handlers.ConvertHtmlToPdf;
public class ConvertHtmlToPdfHandlerLoggingTests
{
    private readonly IConvertHtmlToPdfHandler _inner = Substitute.For<IConvertHtmlToPdfHandler>();
    private readonly FakeLogger<ConvertHtmlToPdfHandler> _logger = new FakeLogger<ConvertHtmlToPdfHandler>();
    private readonly ConvertHtmlToPdfHandlerLogging _sut;

    public ConvertHtmlToPdfHandlerLoggingTests()
    {
        _sut = new ConvertHtmlToPdfHandlerLogging(_inner, _logger);
    }

    [Fact]
    public async Task ConvertHtmlToPdf_ShouldReturnResult_WhenInvoked()
    {
        // Arrange
        var innerResult = ErrorOrFactory.From(Guid.Empty);
        var tasks = new Dictionary<Guid, object>();

        _inner
            .Handle(Arg.Any<IReadOnlyDictionary<Guid, object>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        var result = await _sut.Handle(tasks, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Should().Be(innerResult);
    }

    [Fact]
    public async Task ConvertHtmlToPdf_ShouldLogInformation_WhenInvoked()
    {
        // Arrange
        var innerResult = ErrorOrFactory.From(Guid.Empty);
        var tasks = new Dictionary<Guid, object>();

        _inner
            .Handle(Arg.Any<IReadOnlyDictionary<Guid, object>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        await _sut.Handle(tasks, CancellationToken.None);

        // Assert
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

}
