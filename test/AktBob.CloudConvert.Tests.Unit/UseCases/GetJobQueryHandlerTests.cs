using AktBob.CloudConvert.Contracts;
using AktBob.CloudConvert.Contracts.DTOs;
using AktBob.CloudConvert.Models.JobResponse;
using AktBob.CloudConvert.UseCases;
using AktBob.Shared;
using AktBob.Shared.CQRS;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using System.Text;

namespace AktBob.CloudConvert.Tests.Unit.UseCases;

public class GetJobQueryHandlerTests
{
    private readonly FakeLogger<GetJobQueryHandler> _logger = new FakeLogger<GetJobQueryHandler>();
    private readonly ICloudConvertClient _cloudConvertClient = Substitute.For<ICloudConvertClient>();
    private readonly IQueryDispatcher _queryDispatcher = Substitute.For<IQueryDispatcher>();
    private readonly ITimeProvider _timeProvider = Substitute.For<ITimeProvider>();
    private readonly GetJobQueryHandler _sut;

    public GetJobQueryHandlerTests()
    {
        _sut = new GetJobQueryHandler(_cloudConvertClient, _logger, _timeProvider, _queryDispatcher);
    }

    [Fact]
    public async Task Handle_ShouldReturnBytes_WhenJobIsFinished()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var query = new GetJobQuery(jobId);
        
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
                                                Url = "http://localhost"
                                            }
                                        ]
                                    }
                                }
                            ]
                        } 
                    }
                ))
            );

        var fileContent = "some content";
        var fileBytes = Encoding.UTF8.GetBytes(fileContent);
        var stream = new MemoryStream(fileBytes);
        var fileDto = new FileDto(stream, "filename");
        var resultMock = Task.FromResult(Result.Success(fileDto));

        _queryDispatcher
            .Dispatch(Arg.Any<GetFileQuery>(), Arg.Any<CancellationToken>())
            .Returns(resultMock);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _timeProvider.Received(2).Delay(2000, Arg.Any<CancellationToken>());
        result.Value.Should().BeEquivalentTo(fileBytes);
    }
}
