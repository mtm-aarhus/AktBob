﻿using AktBob.CloudConvert.Handlers;
using AktBob.CloudConvert.Models;
using Ardalis.Result;
using FluentAssertions;
using NSubstitute;

namespace AktBob.CloudConvert.Tests.Unit.Handlers;

public class ConvertHtmlToPdfHandlerTests
{
    private readonly ConvertHtmlToPdfHandler _sut;
    private readonly ICloudConvertClient _cloudConvertClient = Substitute.For<ICloudConvertClient>();
    public ConvertHtmlToPdfHandlerTests()
    {
        _sut = new ConvertHtmlToPdfHandler(_cloudConvertClient);
    }

    [Fact]
    public async Task Handle_ShouldReturnJobId_WhenInvokedWithValidTasks()
    {
        // Arrange
        var tasks = new Dictionary<Guid, object>
        {
            { Guid.NewGuid(), new { SomeProperty = "Some value 1" } },
            { Guid.NewGuid(), new { SomeProperty = "Some value 2" } },
            { Guid.NewGuid(), new { SomeProperty = "Some value 3" } }
        };
        
        var expectedId = Guid.NewGuid();
        _cloudConvertClient
            .CreateJob(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(expectedId));

        // Act
        var result = await _sut.Handle(tasks, CancellationToken.None);

        // Assert
        result.Value.Should().Be(expectedId);
        result.Status.Should().Be(ResultStatus.Ok);
        await _cloudConvertClient.Received(1).CreateJob(Arg.Any<Payload>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenOneOrMoreTasksAreInvalid()
    {
        // Arrange
        var tasks = new Dictionary<Guid, object>
        {
            { Guid.NewGuid(), null! },
            { Guid.NewGuid(), new { SomeProperty = "Some value 2" } },
            { Guid.NewGuid(), new { SomeProperty = "Some value 3" } }
        };

        // Act
        var result = await _sut.Handle(tasks, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().NotBeEmpty();
        await _cloudConvertClient.DidNotReceive().CreateJob(Arg.Any<Payload>(), Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Handle_ShouldReturnError_WhenInvokedWithEmptyDictionary()
    {
        // Arrange
        var tasks = new Dictionary<Guid, object>();

        // Act
        var result = await _sut.Handle(tasks, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().NotBeEmpty();
        await _cloudConvertClient.DidNotReceive().CreateJob(Arg.Any<Payload>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenCreatingCloudConvertJobFails()
    {
        // Arrange
        var tasks = new Dictionary<Guid, object>
        {
            { Guid.NewGuid(), new { SomeProperty = "Some value 1" } },
            { Guid.NewGuid(), new { SomeProperty = "Some value 2" } },
            { Guid.NewGuid(), new { SomeProperty = "Some value 3" } }
        };
        _cloudConvertClient.CreateJob(Arg.Any<object>(), Arg.Any<CancellationToken>()).Returns(Result.Error());

        // Act
        var result = await _sut.Handle(tasks, CancellationToken.None);

        // Assert
        result.Status.Should().Be(ResultStatus.Error);
        result.Errors.Should().NotBeEmpty();
        await _cloudConvertClient.Received(1).CreateJob(Arg.Any<Payload>(), Arg.Any<CancellationToken>());
    }
}
