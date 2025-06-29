using AAK.Deskpro;
using AAK.Deskpro.Models;
using Aktbob.Modules.Deskpro.Features.GetPersonById;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetPersonById;
public class GetPersonByIdHandlerTests
{
    private readonly GetPersonByIdHandler _sut;
    private readonly IDeskproClient _deskproClient = Substitute.For<IDeskproClient>();

    public GetPersonByIdHandlerTests()
    {
        _sut = new GetPersonByIdHandler(_deskproClient);
    }

    [Fact]
    public async Task GetById_ShouldReturnErrorWithMessage_WhenPersonIdIsZero()
    {
        // Arrange
        var personId = 0;

        // Act
        var result = await _sut.Handle(personId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(0).GetPersonById(Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_ShouldReturnErrorWithMessage_WhenPersonIsNullFromDeskpro()
    {
        // Arrange
        var personId = 1;
        _deskproClient.GetPersonById(personId, Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _sut.Handle(personId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetPersonById(personId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_ShouldReturnDto_WhenValidPersonIsReturnedFromDeskpro()
    {
        // Arrange
        var personId = 1;
        var expectedDto = new PersonDto { Id = personId };
        _deskproClient.GetPersonById(personId, Arg.Any<CancellationToken>()).Returns(new Person { Id = personId });

        // Act
        var result = await _sut.Handle(personId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedDto);
        await _deskproClient.Received(1).GetPersonById(personId, Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task GetById_ShouldRethrowException_WhenAnyExceptionIsThrown()
    {
        // Arrange
        var personId = 1;
        _deskproClient.GetPersonById(personId, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Handle(personId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _deskproClient.Received(1).GetPersonById(personId, Arg.Any<CancellationToken>());
    }
}
