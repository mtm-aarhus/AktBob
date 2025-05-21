using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetPerson;
using AktBob.Deskpro.Handlers.GetPersonByEmail;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetPersonByEmail;
public class GetPersonByEmailHandlerExceptionTests
{
    private readonly GetPersonByEmailHandlerException _sut;
    private readonly IGetPersonByEmailHandler _inner = Substitute.For<IGetPersonByEmailHandler>();
    private readonly FakeLogger<GetPersonByEmailHandler> _logger = new FakeLogger<GetPersonByEmailHandler>();

    public GetPersonByEmailHandlerExceptionTests()
    {
        _sut = new GetPersonByEmailHandlerException(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var personEmail = "email";
        var personDto = new PersonDto { Email = personEmail };
        var innerResult = ErrorOrFactory.From(personDto);
        var expectedResult = ErrorOrFactory.From(personDto);
        _inner.Handle(personEmail, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(personEmail, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).Handle(personEmail, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_LogAndReturnError_WhenInnerModuleThrowsException()
    {
        // Arrange
        var personEmail = "email";
        _inner.Handle(personEmail, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var result = await _sut.Handle(personEmail, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        await _inner.Received(1).Handle(personEmail, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
