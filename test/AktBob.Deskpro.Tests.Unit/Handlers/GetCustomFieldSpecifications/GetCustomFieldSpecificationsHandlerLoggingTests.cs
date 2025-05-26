using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetCustomFieldSpecifications;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetCustomFieldSpecifications;
public class GetCustomFieldSpecificationsHandlerLoggingTests
{
    private readonly GetCustomFieldSpecificationsHandlerLogging _sut;
    private readonly IGetCustomFieldSpecificationsHandler _inner = Substitute.For<IGetCustomFieldSpecificationsHandler>();
    private readonly FakeLogger<GetCustomFieldSpecificationsHandler> _logger = new FakeLogger<GetCustomFieldSpecificationsHandler>();

    public GetCustomFieldSpecificationsHandlerLoggingTests()
    {
        _sut = new GetCustomFieldSpecificationsHandlerLogging(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var content = new List<CustomFieldSpecificationDto>();
        var innerResult = ErrorOrFactory.From<IReadOnlyCollection<CustomFieldSpecificationDto>>(content);
        var expectedResult = ErrorOrFactory.From <IReadOnlyCollection<CustomFieldSpecificationDto>>(content);
        _inner.Handle(Arg.Any<CancellationToken>()).Returns(Task.FromResult(innerResult));

        // Act
        var result = await _sut.Handle(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).Handle(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCustomFieldSpecifications_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var innerResult = Error.Failure().ToErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>();
        _inner.Handle(Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(innerResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
        await _inner.Received(1).Handle(Arg.Any<CancellationToken>());
    }
}
