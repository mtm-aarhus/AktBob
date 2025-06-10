using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetMessages;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetMessages;
public class GetMessagesHandlerTests
{
    private readonly GetMessagesHandler _sut;
    private readonly IDeskproClient _deskproClient = Substitute.For<IDeskproClient>();
    private readonly IGetPersonByIdHandler _getPersonHandler = Substitute.For<IGetPersonByIdHandler>();

    public GetMessagesHandlerTests()
    {
        _sut = new GetMessagesHandler(_deskproClient, _getPersonHandler);
    }

    [Fact]
    public async Task Handle_ShouldReturnCollectionWithMessageDtos_WhenDeskproClientResponseSuccessfully()
    {
        // Arrange
        var person = new PersonDto
        {
            Id = 1
        };

        var deskproMessages1 = new Messages
        {
            Data = new List<Message>
            {
                new Message { Id = 1, TicketId = 1, Person = new Person { Id = 1 } },
                new Message { Id = 2, TicketId = 1, Person = new Person { Id = 1 } },
                new Message { Id = 3, TicketId = 1, Person = new Person { Id = 1 } }
            },
            Pagination = new Pagination
            {
                Count = 3,
                CurrentPage = 1,
                PerPage = 3,
                Total = 5,
                TotalPages = 2
            }
        };

        var deskproMessages2 = new Messages
        {
            Data = new List<Message>
            {
                new Message { Id = 4, TicketId = 1, Person = new Person { Id = 1 } },
                new Message { Id = 5, TicketId = 1, Person = new Person { Id = 1 } }
            },
            Pagination = new Pagination
            {
                Count = 2,
                CurrentPage = 2,
                PerPage = 3,
                Total = 5,
                TotalPages = 2
            }
        };

        var expectedMessages = new List<MessageDto>();
        expectedMessages.AddRange(deskproMessages1.Data.Select(x => new MessageDto { Id = x.Id, TicketId = x.TicketId, Person = new PersonDto { Id = person.Id } }));
        expectedMessages.AddRange(deskproMessages2.Data.Select(x => new MessageDto { Id = x.Id, TicketId = x.TicketId, Person = new PersonDto { Id = person.Id } }));

        _deskproClient
            .GetTicketMessages(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(
                deskproMessages1,
                deskproMessages2);

        _getPersonHandler.Handle(1, Arg.Any<CancellationToken>()).Returns(person);

        // Act
        var result = await _sut.Handle(1, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedMessages);
        await _deskproClient
            .Received(2)
            .GetTicketMessages(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>());
        await _getPersonHandler.Received(5).Handle(1, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyCollection_WhenDeskproClientReturnsNoMessages()
    {
        // Arrange
        var person = new PersonDto
        {
            Id = 1
        };

        var deskproMessages = new Messages
        {
            Data = new List<Message>(),
            Pagination = new Pagination
            {
                Count = 0,
                CurrentPage = 1,
                Total = 0,
                TotalPages = 0
            }
        };

        var expectedMessages = new List<MessageDto>();

        _deskproClient
            .GetTicketMessages(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .Returns(deskproMessages);

        // Act
        var result = await _sut.Handle(1, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expectedMessages);
        await _deskproClient
            .Received(1)
            .GetTicketMessages(
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>());
        await _getPersonHandler.Received(0).Handle(1, Arg.Any<CancellationToken>());
    }


    [Fact]
    public async Task Handle_ShouldRethrowException_WhenDeskproClientThrowAnyException()
    {
        // Arrange
        _deskproClient
            .GetTicketMessages(Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<CancellationToken>())
            .ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Handle(1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
    }

}
