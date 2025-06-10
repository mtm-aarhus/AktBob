using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers.GetTicketsByFieldSearch;
using AktBob.Shared.Extensions;
using ErrorOr;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Collections.ObjectModel;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetTicketsByFieldSearch;
public class GetTicketsByFieldSearchHandlerTests
{
    private readonly GetTicketsByFieldSearchHandler _sut;
    private readonly IDeskproClient _deskproClient = Substitute.For<IDeskproClient>();

    public GetTicketsByFieldSearchHandlerTests()
    {
        _sut = new GetTicketsByFieldSearchHandler(_deskproClient);
    }

    [Theory]
    [InlineData(new int[] { 1 })]
    [InlineData(new int[] { 1, 2, 3 })]
    public async Task Handle_ShouldReturnSuccessWithTicketDtos_WhenTicketsAreFound(int[] fields)
    {
        // Arrange
        var searchValue = "search value";
        var expectedCollection = new Collection<TicketDto>();
        var now = DateTime.UtcNow;
        foreach (var field in fields)
        {
            expectedCollection.AddRange(new Collection<TicketDto>
            {
                new TicketDto { Id = field * 1 },
                new TicketDto { Id = field * 2 }
            });

            _deskproClient
                .GetTicketsByFieldValue(Arg.Is(field), Arg.Is(searchValue), Arg.Any<CancellationToken>())
                .Returns(new Collection<Ticket>
                {
                    new Ticket { Id = field * 1, CreatedAt = now, Fields = new Collection<Field>() },
                    new Ticket { Id = field * 2, CreatedAt = now, Fields = new Collection<Field>() }
                });
        }
        var expectedResult = ErrorOrFactory.From<IReadOnlyCollection<TicketDto>>(expectedCollection);

        // Act
        var result = await _sut.Handle(fields, searchValue, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Count().Should().Be(expectedCollection.Count);
    }

    [Theory]
    [InlineData(new int[] { })]
    [InlineData(new int[] { 1 })]
    [InlineData(new int[] { 1, 2, 3 })]
    public async Task Handle_ShouldReturnError_WhenNoTicketsAreFound(int[] fields)
    {
        // Arrange
        var collection = new Collection<Ticket>();
        _deskproClient.GetTicketsByFieldValue(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(collection);

        // Act
        var result = await _sut.Handle(fields, "search value", CancellationToken.None);

        // Assert
        result.IsError.Should().BeTrue();
        await _deskproClient.Received(fields.Length).GetTicketsByFieldValue(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData(new int[] { }, "search value")]
    [InlineData(new int[] { 1 }, "search value")]
    [InlineData(new int[] { 1, 2, 3 }, "search value")]
    public async Task Handle_ShouldCallGetTicketsByFieldValue_ForEachField(int[] fields, string searchValue)
    {
        // Arrange
        var expectedCollection = new Collection<TicketDto>();
        var now = DateTime.UtcNow;
        foreach (var field in fields)
        {
            expectedCollection.AddRange(new Collection<TicketDto>
            {
                new TicketDto { Id = field * 1 },
                new TicketDto { Id = field * 2 }
            });
            _deskproClient
                .GetTicketsByFieldValue(Arg.Is(field), Arg.Is(searchValue), Arg.Any<CancellationToken>())
                .Returns(new Collection<Ticket>
                {
                    new Ticket { Id = field * 1, CreatedAt = now, Fields = new Collection<Field>() },
                    new Ticket { Id = field * 2, CreatedAt = now, Fields = new Collection<Field>() }
                });
        }
        var expectedResult = ErrorOrFactory.From<IReadOnlyCollection<TicketDto>>(expectedCollection);

        // Act
        var result = await _sut.Handle(fields, searchValue, CancellationToken.None);

        // Assert
        if (fields.Count() == 0)
        {
            await _deskproClient.Received(0).GetTicketsByFieldValue(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        foreach (var field in fields)
        {
            await _deskproClient.Received(1).GetTicketsByFieldValue(Arg.Is(field), Arg.Is(searchValue), Arg.Any<CancellationToken>());
        }
    }

    
    [Theory]
    [InlineData(new int[] { })]
    [InlineData(new int[] { 1 })]
    [InlineData(new int[] { 1, 2, 3 })]
    public async Task Handle_ShouldRethrow_WhenAnyExceptionIsThrown(int[] fields)
    {
        // Arrange
        _deskproClient.GetTicketsByFieldValue(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Handle(fields, "search value", CancellationToken.None);

        // Assert
        if (fields.Length > 0 )
        {
            await act.Should().ThrowAsync<Exception>();
        }
        else
        {
            var result = await act();
            result.IsError.Should().BeTrue();
        }
    }
}
