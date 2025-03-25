using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers;

public class GetCustomFieldSpecificationsHandlerTests
{
    private GetCustomFieldSpecificationsHandler _sut;
    private IDeskproClient _deskproClient = Substitute.For<IDeskproClient>();

    public GetCustomFieldSpecificationsHandlerTests()
    {
        _sut = new GetCustomFieldSpecificationsHandler(_deskproClient);
    }

    [Fact]
    public async Task Handle_ShouldReturnSpecificationsDtoResult_WhenDeskproClientResponseIsSuccessful()
    {
        // Arrange
        var expected = new List<CustomFieldSpecificationDto>();
        _deskproClient.GetCustomFieldSpecifications(Arg.Any<CancellationToken>()).Returns(new List<CustomFieldSpecification>());

        // Act
        var result = await _sut.Handle(CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected);
        await _deskproClient.Received(1).GetCustomFieldSpecifications(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnErrorResultWithMessage_WhenDeskproClientThrowsHttpRequestException()
    {
        // Arrange
        _deskproClient.GetCustomFieldSpecifications(Arg.Any<CancellationToken>()).ThrowsAsync<HttpRequestException>();

        // Act
        var act = () => _sut.Handle(CancellationToken.None);
        var result = await act();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetCustomFieldSpecifications(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRethrowException_WhenDeskproClientThrowsAnyExceptionOtherThanHttpRequestException()
    {
        // Arrange
        _deskproClient.GetCustomFieldSpecifications(Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Handle(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _deskproClient.Received(1).GetCustomFieldSpecifications(Arg.Any<CancellationToken>());
    }
}
