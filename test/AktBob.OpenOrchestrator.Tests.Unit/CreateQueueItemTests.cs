using AktBob.Shared.Exceptions;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using System.Text;

namespace AktBob.OpenOrchestrator.Tests.Unit;

public class CreateQueueItemTests
{
    private readonly CreateQueueItem _sut;
    private readonly IServiceScopeFactory _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
    private readonly IServiceScope _serviceScope = Substitute.For<IServiceScope>();
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
    private readonly ICreateQueueItemHandler _createQueueItemHandler = Substitute.For<ICreateQueueItemHandler>();
    private readonly FakeLogger<CreateQueueItem> _logger = new FakeLogger<CreateQueueItem>();

    public CreateQueueItemTests()
    {
        _serviceScopeFactory.CreateScope().Returns(_serviceScope);
        _serviceScope.ServiceProvider.Returns(_serviceProvider);
        _serviceProvider.GetService<ICreateQueueItemHandler>().Returns(_createQueueItemHandler);
        _sut = new CreateQueueItem(_serviceScopeFactory, _logger);
    }

    [Fact]
    public async Task Handle_ShouldInvokeServiceHandlerWithDecodedPayloadAndLogInformation_WhenInvoked()
    {
        // Arrange
        var encodedPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes("some payload"));
        var job = new CreateQueueItemJob("queue name", "reference", encodedPayload);
        
        var decodedPayload = Encoding.UTF8.GetString(Convert.FromBase64String(job.Payload));

        _createQueueItemHandler
            .Handle(
                Arg.Any<string>(),
                decodedPayload,
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        // Act
        await _sut.Handle(job, CancellationToken.None);

        // Assert
        _logger.Collector.LatestRecord.Level.Should().Be(Microsoft.Extensions.Logging.LogLevel.Information);
        await _createQueueItemHandler.Received(1).Handle(
            job.QueueName,
            decodedPayload,
            job.Reference,
            Arg.Any<CancellationToken>());

    }

    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenServiceHandlerFails()
    {
        // Arrange
        var encodedPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes("some payload"));
        var job = new CreateQueueItemJob("queue name", "reference", encodedPayload);

        _createQueueItemHandler
            .Handle(
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(Result.Error());

        // Act
        var act = () => _sut.Handle(job, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
        _logger.Collector.Count.Should().Be(0);
        await _createQueueItemHandler.Received(1).Handle(
            job.QueueName,
            Arg.Any<string>(),
            job.Reference,
            Arg.Any<CancellationToken>());
    }
}
