using AktBob.OpenOrchestrator.Contracts;
using AktBob.Shared;
using NSubstitute;
using System.Text;

namespace AktBob.OpenOrchestrator.Tests.Unit;

public class OpenOrchestratorModuleTests
{
    private readonly OpenOrchestratorModule _sut;
    private readonly IJobDispatcher _jobDispatcher = Substitute.For<IJobDispatcher>();

    public OpenOrchestratorModuleTests()
    {
        _sut = new OpenOrchestratorModule(_jobDispatcher);
    }

    [Fact]
    public void CreateQueueItem_ShouldEncodePayloadAndInvokeDispatcher_WhenInvoked()
    {
        // Arrange
        var command = new CreateQueueItemCommand("queue name", "reference", "some value");
        var expectedBase64Payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(command.Payload));

        // Act
        _sut.CreateQueueItem(command);

        // Assert
        _jobDispatcher.Received(1).Dispatch(Arg.Is<CreateQueueItemJob>(job => 
            job.QueueName == command.QueueName
            && job.Reference == command.Reference
            && job.Payload == expectedBase64Payload));
    }
}
