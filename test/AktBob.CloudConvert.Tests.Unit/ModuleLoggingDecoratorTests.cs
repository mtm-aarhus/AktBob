using AktBob.CloudConvert.Contracts;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.CloudConvert.Tests.Unit;

public class ModuleLoggingDecoratorTests
{
    private readonly ModuleLoggingDecorator _sut;
    private readonly ICloudConvertModule _inner = Substitute.For<ICloudConvertModule>();
    private readonly FakeLogger<CloudConvertModule> _logger = new FakeLogger<CloudConvertModule>();

    public ModuleLoggingDecoratorTests()
    {
        _sut = new ModuleLoggingDecorator(_inner, _logger);
    }

    [Fact]
    public async Task ConvertHtmlToPdf_ShouldReturnResult_WhenInvoked()
    {
        // Arrange
        var innerResult = Result.Success(Guid.Empty);
        var tasks = new Dictionary<Guid, object>();

        _inner
            .ConvertHtmlToPdf(Arg.Any<IReadOnlyDictionary<Guid, object>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        var result = await _sut.ConvertHtmlToPdf(tasks, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Should().Be(innerResult);
    }

    [Fact]
    public async Task ConvertHtmlToPdf_ShouldLogInformation_WhenInvoked()
    {
        // Arrange
        var innerResult = Result.Success(Guid.Empty);
        var tasks = new Dictionary<Guid, object>();

        _inner
            .ConvertHtmlToPdf(Arg.Any<IReadOnlyDictionary<Guid, object>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        await _sut.ConvertHtmlToPdf(tasks, CancellationToken.None);

        // Assert
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task ConvertHtmlToPdf_ShouldLogWarning_WhenResultIsNotSuccesful()
    {
        // Arrange
        var tasks = new Dictionary<Guid, object>();

        _inner
            .ConvertHtmlToPdf(Arg.Any<IReadOnlyDictionary<Guid, object>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<Guid>.Error()));

        // Act
        var result = await _sut.ConvertHtmlToPdf(tasks, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
    }


    [Fact]
    public void GenerateTasks_ShouldReturnResult_WhenInvoked()
    {
        // Arrange
        var innerResult = new Dictionary<Guid, object>();
        var items = Enumerable.Empty<byte[]>();

        _inner
            .GenerateTasks(Arg.Any<IEnumerable<byte[]>>())
            .Returns(innerResult);

        // Act
        var result = _sut.GenerateTasks(items);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(innerResult);
    }

    [Fact]
    public void GenerateTasks_ShouldLogInformation_WhenInvoked()
    {
        // Arrange
        var innerResult = new Dictionary<Guid, object>().AsReadOnly();
        var items = Enumerable.Empty<byte[]>();

        _inner
            .GenerateTasks(Arg.Any<IEnumerable<byte[]>>())
            .Returns(innerResult);

        // Act
        _sut.GenerateTasks(items);

        // Assert
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public void GenerateTasks_ShouldLogWarning_WhenResultIsNotSuccesful()
    {
        // Arrange
        var items = Enumerable.Empty<byte[]>();

        _inner
            .GenerateTasks(Arg.Any<IEnumerable<byte[]>>())
            .Returns(Result<IReadOnlyDictionary<Guid, object>>.Error());

        // Act
        var result = _sut.GenerateTasks(items);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
    }


    [Fact]
    public async Task GetDownloadUrl_ShouldReturnResult_WhenInvoked()
    {
        // Arrange
        var innerResult = Result.Success(string.Empty);

        _inner
            .GetDownloadUrl(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        var result = await _sut.GetDownloadUrl(Guid.Empty, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Should().Be(innerResult);
    }

    [Fact]
    public async Task GetDownloadUrl_ShouldLogInformation_WhenInvoked()
    {
        // Arrange
        var innerResult = Result.Success(string.Empty);
        
        _inner
            .GetDownloadUrl(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        await _sut.GetDownloadUrl(Guid.Empty, CancellationToken.None);

        // Assert
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task GetDownloadUrl_ShouldLogWarning_WhenResultIsNotSuccesful()
    {
        // Arrange
        _inner
            .GetDownloadUrl(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result<string>.Error());

        // Act
        var result = await _sut.GetDownloadUrl(Guid.Empty, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
    }


    [Fact]
    public async Task DownloadFile_ShouldReturnResult_WhenInvoked()
    {
        // Arrange
        var innerResult = Result.Success(new byte[] {});

        _inner
            .DownloadFile(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        var result = await _sut.DownloadFile(string.Empty, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Should().Be(innerResult);
    }

    [Fact]
    public async Task DownloadFile_ShouldLogInformation_WhenInvoked()
    {
        // Arrange
        var innerResult = Result.Success(new byte[] { });

        _inner
            .DownloadFile(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        await _sut.DownloadFile(string.Empty, CancellationToken.None);

        // Assert
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }

    [Fact]
    public async Task DownloadFile_ShouldLogWarning_WhenResultIsNotSuccesful()
    {
        // Arrange
        _inner
            .DownloadFile(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Result<byte[]>.Error());

        // Act
        var result = await _sut.DownloadFile(string.Empty, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
    }
}
