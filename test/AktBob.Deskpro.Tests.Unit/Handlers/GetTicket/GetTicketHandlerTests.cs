using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetTicket;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetTicket;
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
        result.IsError.Should().BeTrue();
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
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expected);
        await _deskproClient.Received(1).GetTicketById(ticketId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRethrowException_WhenAnyExceptionIsThrown()
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
