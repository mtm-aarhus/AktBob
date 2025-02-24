using AktBob.CloudConvert.Contracts;
using AktBob.CloudConvert.Contracts.DTOs;
using AktBob.CloudConvert.Models.JobResponse;
using AktBob.CloudConvert.UseCases;
using AktBob.Shared;
using Ardalis.Result;
using FluentAssertions;
using MassTransit;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AktBob.CloudConvert.Tests.Unit.UseCases;

public class TestableGetJobQueryHandler : GetJobQueryHandler
{
    public TestableGetJobQueryHandler(ICloudConvertClient cloudConvertClient, ILogger<GetJobQueryHandler> logger, IMediator mediator, ITimeProvider timeProvider) : base(cloudConvertClient, logger, mediator, timeProvider)
    {
    }

    public new Task<Result<byte[]>> Handle(GetJobQuery request, CancellationToken cancellationToken)
    {
        return base.Handle(request, cancellationToken);
    }
}

public class GetJobQueryHandlerTests
{
    private readonly FakeLogger<GetJobQueryHandler> _logger = new FakeLogger<GetJobQueryHandler>();
    private readonly ICloudConvertClient _cloudConvertClient = Substitute.For<ICloudConvertClient>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly ITimeProvider _timeProvider = Substitute.For<ITimeProvider>();
    private readonly TestableGetJobQueryHandler _sut;

    public GetJobQueryHandlerTests()
    {
        _sut = new TestableGetJobQueryHandler(_cloudConvertClient, _logger, _mediator, _timeProvider);
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
                Task.FromResult(Result.Success(new JobResponseRoot { Data = new JobResponseData { Id = jobId, Status = "finished" } }))
            );

        var fileDto = new FileDto(new MemoryStream(), "filename");

        // Mock the expected Result<FileDto>
        var resultMock = Task.FromResult(Result.Success(fileDto));



        _mediator
            .SendRequest(Arg.Any<GetFileQuery>(), Arg.Any<CancellationToken>())
            .ReturnsForAll(resultMock);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

        // Assert
        await _timeProvider.Received(3).Delay(2000, Arg.Any<CancellationToken>());
    }
}
