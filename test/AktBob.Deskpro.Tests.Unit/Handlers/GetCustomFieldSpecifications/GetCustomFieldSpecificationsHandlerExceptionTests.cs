using Aktbob.Modules.Deskpro.Features.GetCustomFieldSpecifications;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetCustomFieldSpecifications;
public class GetCustomFieldSpecificationsHandlerExceptionTests
{
    private readonly GetCustomFieldSpecificationsHandlerException _sut;
    private readonly IGetCustomFieldSpecificationsHandler _inner = Substitute.For<IGetCustomFieldSpecificationsHandler>();
    private readonly FakeLogger<GetCustomFieldSpecificationsHandler> _logger = new FakeLogger<GetCustomFieldSpecificationsHandler>();

    public GetCustomFieldSpecificationsHandlerExceptionTests()
    {
        _sut = new GetCustomFieldSpecificationsHandlerException(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var collection = new List<CustomFieldSpecificationDto>();
        var innerResult = ErrorOrFactory.From<IReadOnlyCollection<CustomFieldSpecificationDto>>(collection);
        var expectedResult = ErrorOrFactory.From<IReadOnlyCollection<CustomFieldSpecificationDto>>(collection);
        _inner.Handle(Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).Handle(Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_LogAndReturnError_WhenInnerModuleThrowsException()
    {
        // Arrange
        _inner.Handle(Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var result = await _sut.Handle(CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        await _inner.Received(1).Handle(Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
