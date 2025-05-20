using AktBob.CloudConvert.Handlers;
using AktBob.CloudConvert.Models.JobResponse;
using AktBob.Shared;
using ErrorOr;
using FluentAssertions;
using NSubstitute;

namespace AktBob.CloudConvert.Tests.Unit.Handlers;

public class GetDownloadUrlHandlerTests
{
    private readonly ICloudConvertClient _cloudConvertClient = Substitute.For<ICloudConvertClient>();
    private readonly ITimeProvider _timeProvider = Substitute.For<ITimeProvider>();
    private readonly GetDownloadUrlHandler _sut;

    public GetDownloadUrlHandlerTests()
    {
        _sut = new GetDownloadUrlHandler(_cloudConvertClient, _timeProvider);
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
                Task.FromResult(ErrorOrFactory.From(new JobResponseRoot { Data = new JobResponseData { Id = jobId, Status = "processing" } })),
                Task.FromResult(ErrorOrFactory.From(
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
            .Returns(ErrorOrFactory.From(new JobResponseRoot { Data = new JobResponseData { Id = jobId, Status = "error" } }));

        // Act
        var result = await _sut.Handle(jobId, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        result.Errors.Should().NotBeEmpty();
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldReturnResultError_WhenCloudConvertClientResultIsNotSuccessful()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        _cloudConvertClient
            .GetJob(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Error.Failure());

        // Act
        var result = await _sut.Handle(jobId, CancellationToken.None);

        // Assert
        result.Value.Should().BeNull();
        result.Errors.Should().NotBeEmpty();
        result.IsError.Should().BeTrue();
    }
}
