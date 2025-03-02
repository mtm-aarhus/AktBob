using AktBob.CloudConvert.Handlers;
using AktBob.CloudConvert.Models.JobResponse;
using AktBob.Shared;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.CloudConvert.Tests.Unit.Handlers;

public class GetCloudConvertDownloadUrlHandlerTests
{
    private readonly FakeLogger<GetCloudConvertDownloadUrlHandler> _logger = new FakeLogger<GetCloudConvertDownloadUrlHandler>();
    private readonly ICloudConvertClient _cloudConvertClient = Substitute.For<ICloudConvertClient>();
    private readonly ITimeProvider _timeProvider = Substitute.For<ITimeProvider>();
    private readonly GetCloudConvertDownloadUrlHandler _sut;

    public GetCloudConvertDownloadUrlHandlerTests()
    {
        _sut = new GetCloudConvertDownloadUrlHandler(_cloudConvertClient, _logger, _timeProvider);
    }

    [Fact]
    public async Task Handle_ShouldReturnUrl_WhenJobIsFinished()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var expectedUrl = "the expected url";

        _cloudConvertClient
            .GetJob(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(
                Task.FromResult(Result.Success(new JobResponseRoot { Data = new JobResponseData { Id = jobId, Status = "processing" } })),
                Task.FromResult(Result.Success(
                    new JobResponseRoot
                    {
                        Data = new JobResponseData
                        {
                            Id = jobId,
                            Status = "finished",
                            Tasks =
                            [
                                new JobResponseTask
                                {
                                    Operation = "export/url",
                                    Result = new JobResponseResult
                                    {
                                        Files =
                                        [
                                            new JobResponseFiles
                                            {
                                                Url = expectedUrl
                                            }
                                        ]
                                    }
                                }
                            ]
                        }
                    }
                ))
            );

        // Act
        var result = await _sut.Handle(jobId, CancellationToken.None);

        // Assert
        await _timeProvider.Received(2).Delay(TimeSpan.FromSeconds(2), Arg.Any<CancellationToken>());
        result.Value.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task Handle_ShouldReturnResultError_WhenCloudConvertJobStatusIsError()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _cloudConvertClient
            .GetJob(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new JobResponseRoot { Data = new JobResponseData { Id = jobId, Status = "error" } }));

        // Act
        var result = await _sut.Handle(jobId, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnResultError_WhenCloudConvertClientResultIsNotSuccessful()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _cloudConvertClient
            .GetJob(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Error());

        // Act
        var result = await _sut.Handle(jobId, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().NotBeEmpty();
    }
}
