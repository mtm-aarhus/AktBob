using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetPersonById;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetPersonById;
public class GetPersonByIdHandlerLoggingTests
{
    private readonly GetPersonByIdHandlerLogging _sut;
    private readonly IGetPersonByIdHandler _inner = Substitute.For<IGetPersonByIdHandler>();
    private readonly FakeLogger<GetPersonByIdHandler> _logger = new FakeLogger<GetPersonByIdHandler>();

    public GetPersonByIdHandlerLoggingTests()
    {
        _sut = new GetPersonByIdHandlerLogging(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var personId = 1;
        var personDto = new PersonDto
        {
            Id = personId
        };

        var innerResult = ErrorOrFactory.From(personDto);
        var expectedResult = ErrorOrFactory.From(personDto);

        _inner.Handle(personId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(personId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).Handle(personId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var personId = 1;
        var personDto = new PersonDto
        {
            Id = personId
        };

        var error = Error.Failure().ToErrorOr<PersonDto>();
        _inner.Handle(personId, Arg.Any<CancellationToken>()).Returns(error);

        // Act
        var result = await _sut.Handle(personId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(error);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Warning);
        await _inner.Received(1).Handle(personId, Arg.Any<CancellationToken>());
    }
}
