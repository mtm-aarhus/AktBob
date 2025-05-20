using AktBob.CloudConvert.Handlers.ConvertHtmlToPdf;
using AktBob.CloudConvert.Handlers.DownloadFile;
using AktBob.CloudConvert.Handlers.GenerateTasks;
using AktBob.CloudConvert.Handlers.GetDownloadUrl;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace AktBob.CloudConvert.Tests.Unit;

public class CloudConvertModuleTests
{
    private readonly CloudConvertModule _sut;
    private readonly IConvertHtmlToPdfHandler _convertHtmlToPdfHandler = Substitute.For<IConvertHtmlToPdfHandler>();
    private readonly IGetDownloadUrlHandler _getDownloadUrlHandler = Substitute.For<IGetDownloadUrlHandler>();
    private readonly IDownloadFileHandler _downloadFileHandler = Substitute.For<IDownloadFileHandler>();
    private readonly IGenerateTasksHandler _generateTasksHandler = Substitute.For<IGenerateTasksHandler>();

    public CloudConvertModuleTests()
    {
        _sut = new CloudConvertModule(
            _convertHtmlToPdfHandler,
            _getDownloadUrlHandler,
            _downloadFileHandler,
            _generateTasksHandler);
    }

    [Fact]
    public async Task ConvertHtmlToPdf_ShouldInvokeHandlerAndReturnResult_WhenInvoked()
    {
        // Arrange
        var expected = ErrorOrFactory.From(Guid.Parse("0C482CE0-24C6-4967-A4E1-3DF54B1E40B5"));
        var tasks = new Dictionary<Guid, object>();

        _convertHtmlToPdfHandler
            .Handle(
                Arg.Any<IReadOnlyDictionary<Guid, object>>(),
                Arg.Any<CancellationToken>())
            .Returns(expected);

        // Act
        var result = await _sut.ConvertHtmlToPdf(tasks, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        await _convertHtmlToPdfHandler.Received(1).Handle(tasks, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void GenerateTasks_ShouldInvokeHandlerAndReturnResult_WhenInvoked()
    {
        // Arrange
        IReadOnlyDictionary<Guid, object> dict = new Dictionary<Guid, object>();
        var expected = ErrorOrFactory.From(dict);

        var items = new List<byte[]>();
        _generateTasksHandler.Handle(items).Returns(expected);

        // Act
        var result = _sut.GenerateTasks(items);

        // Assert
        result.Should().Be(expected);
        _generateTasksHandler.Received(1).Handle(items);
    }

    [Fact]
    public async Task GetDownloadUrl_ShouldInvokeHandlerAndReturnResult_WhenInvoked()
    {
        // Arrange
        var expected = ErrorOrFactory.From("some value");
        _getDownloadUrlHandler.Handle(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(expected);

        // Act
        var result = await _sut.GetDownloadUrl(Guid.Empty, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        await _getDownloadUrlHandler.Received(1).Handle(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DownloadFile_ShouldInvokeHanlderAndReturnResult_WhenInvoked()
    {
        // Arrange
        var expected = ErrorOrFactory.From(new byte[] { 0x20 });
        _downloadFileHandler.Handle(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(expected);

        // Act
        var result = await _sut.DownloadFile(string.Empty, CancellationToken.None);

        // Assert
        result.Should().Be(expected);
        await _downloadFileHandler.Received(1).Handle(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }
}
