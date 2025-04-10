using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers;
using AktBob.Shared;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers;
public class GetPersonHandlerTests
{
    private readonly GetPersonHandler _sut;
    private readonly IDeskproClient _deskproClient = Substitute.For<IDeskproClient>();
    private readonly IAppConfig _appConfig = Substitute.For<IAppConfig>();
    private readonly FakeLogger<GetPersonHandler> _logger = new FakeLogger<GetPersonHandler>();

    public GetPersonHandlerTests()
    {
        _sut = new GetPersonHandler(_deskproClient, _appConfig, _logger);
    }

    [Fact]
    public async Task GetById_ShouldReturnErrorWithMessage_WhenPersonIdIsZero()
    {
        // Arrange
        var personId = 0;

        // Act
        var result = await _sut.GetById(personId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
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
        var result = await _sut.GetById(personId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
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
        var result = await _sut.GetById(personId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
        await _deskproClient.Received(1).GetPersonById(personId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_ShouldReturnErrorWithMessage_WhenHttpRequestExceptionWithNotFoundStatusCodeIsThrown()
    {
        // Arrange
        var personId = 1;
        var exception = new HttpRequestException(null, null, statusCode: System.Net.HttpStatusCode.NotFound);
        _deskproClient.GetPersonById(Arg.Any<int>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        // Act
        var act = () => _sut.GetById(personId, CancellationToken.None);
        var result = await act();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetPersonById(personId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_ShouldRethrowException_WhenHttpRequestExceptionIsNotNotFound()
    {
        // Arrange
        var personId = 1;
        var exception = new HttpRequestException(null, null, statusCode: System.Net.HttpStatusCode.Forbidden);
        _deskproClient.GetPersonById(Arg.Any<int>(), Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        // Act
        var act = () => _sut.GetById(personId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        await _deskproClient.Received(1).GetPersonById(personId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetById_ShouldRethrowException_WhenAnyExceptionOtherThanHttpRequestExceptionIsThrown()
    {
        // Arrange
        var personId = 1;
        _deskproClient.GetPersonById(personId, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetById(personId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _deskproClient.Received(1).GetPersonById(personId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByEmail_ShouldReturnEmptyResult_WhenEmailIsInIgnoreList()
    {
        // Arrange
        var email = "somebody@somewhere.com";
        var ignoreList =  $"{email}";

        _appConfig.GetSection("Deskpro:GetPersonHandler:IgnoreEmails").Returns(ignoreList);

        // Act
        var result = await _sut.GetByEmail(email, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
        await _deskproClient.Received(0).GetPersonByEmail(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByEmail_ShouldReturnError_WhenDeskproReturnsNull()
    {
        // Arrange
        var email = "somebody@somewhere.com";
        _deskproClient.GetPersonByEmail(Arg.Any<string>(), Arg.Any<CancellationToken>()).ReturnsNull();
        _appConfig.GetSection(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _sut.GetByEmail(email, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetPersonByEmail(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByEmail_ShouldReturnError_WhenDeskproReturnsEmptyList()
    {
        // Arrange
        var email = "somebody@somewhere.com";
        _deskproClient.GetPersonByEmail(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new List<Person>());
        _appConfig.GetSection(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _sut.GetByEmail(email, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetPersonByEmail(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByEmail_ShouldReturnSuccessResultWithDto_WhenDeskproReturnsValidPerson()
    {
        // Arrange
        var email = "somebody@somewhere.com";
        var expectedDto = new PersonDto { Email = email };
        _deskproClient.GetPersonByEmail(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(new List<Person> { new Person { Email = email} });
        _appConfig.GetSection(Arg.Any<string>()).ReturnsNull();

        // Act
        var result = await _sut.GetByEmail(email, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
        await _deskproClient.Received(1).GetPersonByEmail(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByEmail_ShouldReturnError_WhenHttpRequestExceptionWithNotFoundIsThrown()
    {
        // Arrange
        var email = "somebody@somewhere.com";
        var exception = new HttpRequestException(null, null, statusCode: System.Net.HttpStatusCode.NotFound);
        _deskproClient.GetPersonByEmail(Arg.Any<string>(), Arg.Any<CancellationToken>()).Throws(exception);

        // Act
        var act = () => _sut.GetByEmail(email, CancellationToken.None);
        var result = await act();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetPersonByEmail(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByEmail_ShouldRethrowException_WhenHttpRequestExceptionStatusCodeIsNotFound()
    {
        // Arrange
        var email = "somebody@somewhere.com";
        var exception = new HttpRequestException(null, null, statusCode: System.Net.HttpStatusCode.Forbidden);
        _deskproClient.GetPersonByEmail(Arg.Any<string>(), Arg.Any<CancellationToken>()).Throws(exception);

        // Act
        var act = () => _sut.GetByEmail(email, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        await _deskproClient.Received(1).GetPersonByEmail(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetByEmail_ShouldRethrowException_WhenNonHttpRequestExceptionIsThrown()
    {
        // Arrange
        var email = "somebody@somewhere.com";
        _deskproClient.GetPersonByEmail(Arg.Any<string>(), Arg.Any<CancellationToken>()).Throws<Exception>();

        // Act
        var act = () => _sut.GetByEmail(email, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _deskproClient.Received(1).GetPersonByEmail(email, Arg.Any<CancellationToken>());
    }
}
