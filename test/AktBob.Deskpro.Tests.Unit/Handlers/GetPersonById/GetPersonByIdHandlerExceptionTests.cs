using Aktbob.Modules.Deskpro.Features.GetPersonById;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using ErrorOr;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetPersonById;
public class GetPersonByIdHandlerExceptionTests
{
    private readonly GetPersonByIdHandlerException _sut;
    private readonly IGetPersonByIdHandler _inner = Substitute.For<IGetPersonByIdHandler>();
    private readonly FakeLogger<GetPersonByIdHandler> _logger = new FakeLogger<GetPersonByIdHandler>();

    public GetPersonByIdHandlerExceptionTests()
    {
        _sut = new GetPersonByIdHandlerException(_inner, _logger);
    }

    [Fact]
    public async Task Handle_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var personId = 1;
        var personDto = new PersonDto { Id = personId };
        var innerResult = ErrorOrFactory.From(personDto);
        var expectedResult = ErrorOrFactory.From(personDto);
        _inner.Handle(personId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.Handle(personId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).Handle(personId, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Handle_LogAndReturnError_WhenInnerModuleThrowsException()
    {
        // Arrange
        var personId = 1;
        _inner.Handle(personId, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var result = await _sut.Handle(personId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        await _inner.Received(1).Handle(personId, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }   
}
