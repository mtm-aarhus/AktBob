using Aktbob.Modules.Deskpro.Features.GetPersonByEmail;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetPersonByEmail;
public class GetPersonByEmailHandlerLoggingTests
{
    private readonly GetPersonByEmailHandlerLogging _sut;
    private readonly IGetPersonByEmailHandler _inner = Substitute.For<IGetPersonByEmailHandler>();
    private readonly FakeLogger<GetPersonByEmailHandler> _logger = new FakeLogger<GetPersonByEmailHandler>();

    public GetPersonByEmailHandlerLoggingTests()
    {
        _sut = new GetPersonByEmailHandlerLogging(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var email = "email";
        var personDto = new PersonDto
        {
            Email = email
        };

        var innerResult = ErrorOrFactory.From(personDto);
        var expectedResult = ErrorOrFactory.From(personDto);

        _inner.Handle(email, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(email, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).Handle(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var email = "email";
        var personDto = new PersonDto
        {
            Email = email
        };

        var error = Error.Failure().ToErrorOr<PersonDto>();
        
        _inner.Handle(email, Arg.Any<CancellationToken>()).Returns(error);

        // Act
        var result = await _sut.Handle(email, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(error);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        await _inner.Received(1).Handle(email, Arg.Any<CancellationToken>());
    }
}
