using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetPersonByEmail;
using AktBob.Shared;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetPersonByEmail;
public class GetPersonByEmailHandlerTests
{
    private readonly GetPersonByEmailHandler _sut;
    private readonly IDeskproClient _deskproClient = Substitute.For<IDeskproClient>();
    private readonly IAppConfig _appConfig = Substitute.For<IAppConfig>();

    public GetPersonByEmailHandlerTests()
    {
        _sut = new GetPersonByEmailHandler(_deskproClient, _appConfig);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenEmailIsInIgnoreList()
    {
        // Arrange
        var email = "somebody@somewhere.com";
        var ignoreList =  $"{email}";
        var expected = new PersonDto();

        _appConfig.GetSection("Deskpro:GetPersonHandler:IgnoreEmails").Returns(ignoreList);

        // Act
        var result = await _sut.Handle(email, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expected);
        await _deskproClient.Received(0).GetPersonByEmail(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenDeskproReturnsNull()
    {
        // Arrange
        var email = "somebody@somewhere.com";
        _deskproClient.GetPersonByEmail(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsNull();
        _appConfig.GetSection(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _sut.Handle(email, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetPersonByEmail(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenDeskproReturnsEmptyList()
    {
        // Arrange
        var email = "somebody@somewhere.com";
        _deskproClient.GetPersonByEmail(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new List<Person>());
        _appConfig.GetSection(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _sut.Handle(email, CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetPersonByEmail(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccessResultWithDto_WhenDeskproReturnsValidPerson()
    {
        // Arrange
        var email = "somebody@somewhere.com";
        var expectedDto = new PersonDto { Email = email };
        _deskproClient.GetPersonByEmail(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new List<Person> { new Person { Email = email} });
        _appConfig.GetSection(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _sut.Handle(email, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedDto);
        await _deskproClient.Received(1).GetPersonByEmail(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRethrowException_WhenAnyExceptionIsThrown()
    {
        // Arrange
        var email = "somebody@somewhere.com";
        _deskproClient.GetPersonByEmail(Arg.Any<string>(), Arg.Any<CancellationToken>()).Throws<Exception>();

        // Act
        var act = () => _sut.Handle(email, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _deskproClient.Received(1).GetPersonByEmail(email, Arg.Any<CancellationToken>());
    }
}
