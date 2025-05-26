using AktBob.CloudConvert.Handlers.GetDownloadUrl;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.CloudConvert.Tests.Unit.Handlers.GetDownloadUrl;
public class GetDownloadUrlHandlerLoggingTests
{
    private readonly GetDownloadUrlHandlerLogging _sut;
    private readonly IGetDownloadUrlHandler _inner = Substitute.For<IGetDownloadUrlHandler>();
    private readonly FakeLogger<GetDownloadUrlHandler> _logger = new FakeLogger<GetDownloadUrlHandler>();

    public GetDownloadUrlHandlerLoggingTests()
    {
        _sut = new GetDownloadUrlHandlerLogging(_inner, _logger);
    }

    [Fact]
    public async Task GetDownloadUrl_ShouldReturnResult_WhenInvoked()
    {
        // Arrange
        var innerResult = ErrorOrFactory.From(string.Empty);

        _inner
            .Handle(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        var result = await _sut.Handle(Guid.Empty, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Should().Be(innerResult);
    }

    [Fact]
    public async Task GetDownloadUrl_ShouldLogInformation_WhenInvoked()
    {
        // Arrange
        var innerResult = ErrorOrFactory.From(string.Empty);

        _inner
            .Handle(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        await _sut.Handle(Guid.Empty, CancellationToken.None);

        // Assert
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task GetDownloadUrl_ShouldReturnErroring_WhenResultIsNotSuccesful()
    {
        // Arrange
        _inner
            .Handle(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure().ToErrorOr<string>());

        // Act
        var result = await _sut.Handle(Guid.Empty, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
    }

}
