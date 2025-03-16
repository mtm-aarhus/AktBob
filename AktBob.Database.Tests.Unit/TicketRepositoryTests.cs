using AktBob.Database.DataAccess;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using Dapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AktBob.Database.Tests.Unit;

public class TicketRepositoryTests
{
    private readonly TicketRepository _sut;
    private readonly ISqlDataAccess _dataAccess = Substitute.For<ISqlDataAccess>();

    public TicketRepositoryTests()
    {
        _sut = new TicketRepository(_dataAccess);
    }

    // Add

    [Fact]
    public async Task Add_ShouldSetTicketIdAndReturnTrue_WhenTicketIsAdded()
    {
        // Arrange
        var ticketId = 1;
        var ticket = new Ticket
        {
            DeskproId = 1
        };

        _dataAccess
            .ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>())
            .Returns(1)
            .AndDoes(call =>
            {
                var passedParameters = call.Arg<DynamicParameters>();
                passedParameters.Add("Id", ticketId, dbType: DbType.Int32, direction: ParameterDirection.Output);
            });

        // Act
        var result = await _sut.Add(ticket);

        // Assert
        result.Should().BeTrue();
        ticket.Id.Should().Be(ticketId);
        await _dataAccess.Received(1).ExecuteProcedure("spTicket_Create", Arg.Any<DynamicParameters>());
    }

    [Fact]
    public async Task Add_ShouldNotSetTicketIdAndReturnFalse_WhenTicketIsNotAdded()
    {
        // Arrange
        var ticket = new Ticket
        {
            DeskproId = 1
        };

        _dataAccess
            .ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>())
            .Returns(0);
            
        // Act
        var result = await _sut.Add(ticket);

        // Assert
        result.Should().BeFalse();
        ticket.Id.Should().Be(default);
        await _dataAccess.Received(1).ExecuteProcedure("spTicket_Create", Arg.Any<DynamicParameters>());
    }

    [Fact]
    public async Task Add_ShouldThrowValidationException_WhenTicketIsInvalid()
    {
        // Arrange
        var ticket = new Ticket
        {
            DeskproId = default
        };

        // Act
        var act = () => _sut.Add(ticket);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        ticket.Id.Should().Be(default);
        await _dataAccess.Received(0).ExecuteProcedure("spTicket_Create", Arg.Any<DynamicParameters>());
    }


    // Get

    [Fact]
    public async Task Get_ShouldReturnTicket_WhenTicketIsFound()
    {
        // Arrange
        var ticketId = 1;
        IEnumerable<Ticket> expectedTickets = new List<Ticket> {
            new Ticket
            {
                Id = ticketId,
                DeskproId = 1
            }
        };

        _dataAccess
            .Query(
                Arg.Any<string>(),
                Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(ticketId)),
                "TicketId",
                Arg.Any<Func<Ticket, Case, Ticket>>())
            .Returns(expectedTickets);

        // Act
        var result = await _sut.Get(ticketId);

        // Assert
        await _dataAccess.Received(1).Query(
            Arg.Any<string>(),
            Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(ticketId)),
            "TicketId",
            Arg.Any<Func<Ticket, Case, Ticket>>());
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTickets.First());
    }

    [Fact]
    public async Task Get_ShouldReturnNull_WhenTicketIsNotFound()
    {

    }


    // GetByDeskproTicketId

    [Theory]
    public async Task GetByDeskproTicketId_ShouldReturnTicket_WhenTicketIsFound(int? deskproId, long? podioItemId, Guid? filArkivCaseId)
    {

    }

    [Theory]
    public async Task GetByDeskproTicketId_ShouldReturnNull_WhenTicketIsNotFound(int? deskproId, long? podioItemId, Guid? filArkivCaseId)
    {

    }


    // GetByPodioItemId

    [Fact]
    public async Task GetByPodioItemId_ShouldReturnTicket_WhenTicketIsFound()
    {

    }

    [Fact]
    public async Task GetByPodioItemId_ShouldReturnNull_WhenTicketIsNotFound()
    {

    }


    // GetAll

    [Fact]
    public async Task GetAll_ShouldReturnTickets_WhenFound()
    {

    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyCollection_WhenNoTicketsAreFoundWithoutCondition()
    {

    }

    
    // Update

    [Fact]
    public async Task Update_ShouldReturnTrue_WhenTicketIsUpdated()
    {

    }

    [Fact]
    public async Task Update_ShouldReturnFalse_WhenTicketIsNotUpdated()
    {

    }

    [Fact]
    public async Task Update_ShouldThrowValidationException_WhenTicketIsNotValid()
    {

    }

}
