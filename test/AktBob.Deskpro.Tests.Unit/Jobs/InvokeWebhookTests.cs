using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Jobs;
using AktBob.Shared.Exceptions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AktBob.Deskpro.Tests.Unit.Jobs;
public class InvokeWebhookTests
{
    private readonly InvokeWebhook _sut;
    private readonly IServiceScopeFactory _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
    private readonly IServiceScope _serviceScope = Substitute.For<IServiceScope>();
    private readonly IServiceProvider _serviceProvider = Substitute.For<IServiceProvider>();
    private readonly IInvokeWebhookHandler _invokeWebhookHandler = Substitute.For<IInvokeWebhookHandler>();

    public InvokeWebhookTests()
    {
        _serviceScopeFactory.CreateScope().Returns(_serviceScope);
        _serviceScope.ServiceProvider.Returns(_serviceProvider);
        _serviceProvider.GetService<IInvokeWebhookHandler>().Returns(_invokeWebhookHandler);
        _sut = new InvokeWebhook(_serviceScopeFactory);
    }

    [Fact]
    public async Task Handle_ShouldCallHandlerWithCorrectArguments_WhenInvokedWithValidJob()
    {
        // Arrange
        var payload = "payload";
        var bytes = Encoding.UTF8.GetBytes(payload);
        var encodedPayload = Convert.ToBase64String(bytes);
        var job = new InvokeWebhookJob("webhookId", encodedPayload);

        // Act
        await _sut.Handle(job, CancellationToken.None);

        // Assert
        await _invokeWebhookHandler.Received(1).Handle(job.WebhookId, payload, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldThrowBusinessException_WhenPayloadIsNotValidUTF8()
    {
        // Arrange
        var invalidBase64 = "!!invalid!!";
        var job = new InvokeWebhookJob("webhookId", invalidBase64);

        // Act
        var act = () => _sut.Handle(job, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
        await _invokeWebhookHandler.Received(0).Handle(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());

    }
}

