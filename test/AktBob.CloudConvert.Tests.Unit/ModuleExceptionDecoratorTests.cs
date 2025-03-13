using AktBob.CloudConvert.Contracts;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.CloudConvert.Tests.Unit;

public class ModuleExceptionDecoratorTests
{
    private readonly ModuleExceptionDecorator _sut;
    private readonly ICloudConvertModule _inner = Substitute.For<ICloudConvertModule>();
    private readonly FakeLogger<CloudConvertModule> _logger = new FakeLogger<CloudConvertModule>();

    public ModuleExceptionDecoratorTests()
    {
        _sut = new ModuleExceptionDecorator(_inner, _logger);
    }


    [Fact]
    public async Task ConvertHtmlToPdf_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var tasks = new Dictionary<Guid, object>();
        var innerResult = Result.Success(jobId);

        _inner
            .ConvertHtmlToPdf(tasks, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        var act = () => _sut.ConvertHtmlToPdf(tasks, CancellationToken.None);
        var result = await act();
        
        // Assert
        result.Should().Be(innerResult);
        await act.Should().NotThrowAsync();
    }


    [Fact]
    public async Task ConvertHtmlToPdf_ShouldThrowException_WhenInnerModuleFails()
    {
        // Arrange
        var tasks = new Dictionary<Guid, object>();
        _inner
           .ConvertHtmlToPdf(Arg.Any<IReadOnlyDictionary<Guid, object>>(), Arg.Any<CancellationToken>())
           .ThrowsAsync(new Exception());

        // Act
        var act = () => _sut.ConvertHtmlToPdf(tasks, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }


    [Fact]
    public void GenerateTasks_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var items = new List<byte[]>();
        IReadOnlyDictionary<Guid, object> innerResult = new Dictionary<Guid, object>();

        _inner
            .GenerateTasks(items)
            .Returns(Result.Success(innerResult));

        // Act
        var result = _sut.GenerateTasks(items);

        // Assert
        result.Should().BeEquivalentTo(Result.Success(innerResult));
    }

    
    [Fact]
    public void GenerateTasks_ShouldThrowException_WhenInnerModuleFails()
    {
        // Arrange
        var items = new List<byte[]>();
        _inner
            .GenerateTasks(Arg.Any<IEnumerable<byte[]>>())
            .Throws<Exception>();

        // Act
        var act = () => _sut.GenerateTasks(items);

        // Assert
        act.Should().Throw<Exception>();
    }


    [Fact]
    public async Task GetDowloadUrl_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var innerResult = Result.Success("locahost");

        _inner
            .GetDownloadUrl(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        var result = await _sut.GetDownloadUrl(Guid.Empty, CancellationToken.None);

        // Assert
        result.Should().Be(innerResult);
    }

    
    [Fact]
    public async Task GetDownloadUrl_ShouldThrowException_WhenInnerModuleFails()
    {
        // Arrange
        _inner
            .GetDownloadUrl(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetDownloadUrl(Guid.Empty, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }


    [Fact]
    public async Task DownloadFile_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var innerResult = Result.Success(new byte[] {});
        _inner
            .DownloadFile(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(innerResult));

        // Act
        var result = await _sut.DownloadFile(string.Empty, CancellationToken.None);

        // Assert
        result.Should().Be(innerResult);
    }

   
    [Fact]
    public async Task DownloadFile_ShouldThrowException_WhenInnerModuleFails()
    {
        // Arrange
        _inner
            .DownloadFile(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.DownloadFile(string.Empty, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

}
