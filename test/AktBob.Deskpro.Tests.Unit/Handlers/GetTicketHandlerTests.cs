using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers;
using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AktBob.Deskpro.Tests.Unit.Handlers;
public class GetTicketHandlerTests
{
    private readonly GetTicketHandler _sut;
    private readonly IDeskproClient _deskproClient = Substitute.For<IDeskproClient>();

    public GetTicketHandlerTests()
    {
        _sut = new GetTicketHandler(_deskproClient);
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenTicketIsNull()
    {
        // Arrange
        var ticketId = 1;
        _deskproClient.GetTicketById(ticketId, Arg.Any<CancellationToken>()).ReturnsNull();

        // Act
        var result = await _sut.Handle(ticketId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetTicketById(ticketId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnTicketDto_WhenTicketExists()
    {
        // Arrange
        var ticketId = 1;
        var createdAt = DateTime.UtcNow;
        var expected = new TicketDto 
        { 
            Id = ticketId,
            CreatedAt = createdAt,
            Person = new PersonDto(),
            Agent = new PersonDto()
        };

        _deskproClient
            .GetTicketById(ticketId, Arg.Any<CancellationToken>())
            .Returns(new Ticket 
            { 
                Id = ticketId,
                CreatedAt = createdAt
            });

        // Act
        var result = await _sut.Handle(ticketId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected);
        await _deskproClient.Received(1).GetTicketById(ticketId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenDeskproClientThrowsHttpRequestExceptionWithNotFound()
    {
        // Arrange
        var ticketId = 1;
        var exception = new HttpRequestException(null, null, statusCode: System.Net.HttpStatusCode.NotFound);
        _deskproClient.GetTicketById(ticketId, Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        // Act
        var act = () => _sut.Handle(ticketId, CancellationToken.None);
        var result = await act();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetTicketById(ticketId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRethrowException_WhenDeskproClientThrowsHttpRequestExceptionWithAnyOtherStatusCodeThanNotFound()
    {
        // Arrange
        var ticketId = 1;
        var exception = new HttpRequestException(null, null, statusCode: System.Net.HttpStatusCode.Forbidden);
        _deskproClient.GetTicketById(ticketId, Arg.Any<CancellationToken>()).ThrowsAsync(exception);

        // Act
        var act = () => _sut.Handle(ticketId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
        await _deskproClient.Received(1).GetTicketById(ticketId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRethrowException_WhenAnyExceptionOtherThanHttpRequestExceptionIsThrown()
    {
        // Arrange
        var ticketId = 1;
        _deskproClient.GetTicketById(ticketId, Arg.Any<CancellationToken>()).Throws<Exception>();

        // Act
        var act = () => _sut.Handle(ticketId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _deskproClient.Received(1).GetTicketById(ticketId, Arg.Any<CancellationToken>());
    }
}
