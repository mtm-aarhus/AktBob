using AktBob.CloudConvert.Handlers.GetDownloadUrl;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.CloudConvert.Tests.Unit.Handlers.GetDownloadUrl;
public class GetDownloadUrlHandlerExceptionTests
{
    private readonly GetDownloadUrlHandlerException _sut;
    private readonly IGetDownloadUrlHandler _inner = Substitute.For<IGetDownloadUrlHandler>();
    private readonly FakeLogger<GetDownloadUrlHandler> _logger = new FakeLogger<GetDownloadUrlHandler>();

    public GetDownloadUrlHandlerExceptionTests()
    {
        _sut = new GetDownloadUrlHandlerException(_inner, _logger);
    }

    [Fact]
    public async Task GetDowloadUrl_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var innerResult = ErrorOrFactory.From("locahost");

        _inner
            .Handle(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        var result = await _sut.Handle(Guid.Empty, CancellationToken.None);

        // Assert
        result.Should().Be(innerResult);
    }


    [Fact]
    public async Task GetDownloadUrl_ShouldReturnError_WhenInnerModuleFails()
    {
        // Arrange
        _inner
            .Handle(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ThrowsAsync<Exception>();

        // Act
        var result = await _sut.Handle(Guid.Empty, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(Microsoft.Extensions.Logging.LogLevel.Critical);
    }
}
