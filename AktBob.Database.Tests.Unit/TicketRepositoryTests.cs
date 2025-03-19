using AktBob.Database.DataAccess;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using Dapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using System.Data;
using System.Text.Json;

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

        var expectedTicket = new Ticket { Id = 1, CaseNumber = "Ticket A" };
        var expectedCase1 = new Case { Id = 101,  PodioItemId = 123, TicketId = 1 };
        var expectedCase2 = new Case { Id = 102, PodioItemId = 456, TicketId = 1 };

        var sqlCondition = "t.Id = @Id";
        
        var ticketCasePairs = new List<(Ticket, Case)>
        {
            (expectedTicket, expectedCase1),
            (expectedTicket, expectedCase2)
        };

        _dataAccess
            .Query(
                Arg.Any<string>(),
                Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(ticketId)),
                "TicketId",
                Arg.Any<Func<Ticket, Case, Ticket>>())
            .Returns(call =>
            {
                var mappingFunc = call.Arg<Func<Ticket, Case, Ticket>>();
                var ticketDictionary = new Dictionary<int, Ticket>();

                var tickets = ticketCasePairs
                    .Select(pair => mappingFunc(pair.Item1, pair.Item2))
                    .Where(ticket => ticket != null)
                    .ToList();

                return tickets;
            });

        // Act
        var result = await _sut.Get(ticketId);

        // Assert
        await _dataAccess.Received(1).Query(
            Arg.Is<string>(arg => arg.Contains(sqlCondition)),
            Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(ticketId)),
            "TicketId",
            Arg.Any<Func<Ticket, Case, Ticket>>());
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTicket);
        result.Cases.Count().Should().Be(2);
    }

    [Fact]
    public async Task Get_ShouldReturnNull_WhenTicketIsNotFound()
    {
        // Arrange
        var ticketId = 1;
        var sqlCondition = "t.Id = @Id";
        var ticketCasePairs = new List<(Ticket, Case)>();

        _dataAccess
            .Query(
                Arg.Any<string>(),
                Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(ticketId)),
                "TicketId",
                Arg.Any<Func<Ticket, Case, Ticket>>())
            .Returns(call =>
            {
                var mappingFunc = call.Arg<Func<Ticket, Case, Ticket>>();
                var ticketDictionary = new Dictionary<int, Ticket>();

                var tickets = ticketCasePairs
                    .Select(pair => mappingFunc(pair.Item1, pair.Item2))
                    .Where(ticket => ticket != null)
                    .ToList();

                return tickets;
            });

        // Act
        var result = await _sut.Get(ticketId);

        // Assert
        await _dataAccess.Received(1).Query(
            Arg.Is<string>(arg => arg.Contains(sqlCondition)),
            Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(ticketId)),
            "TicketId",
            Arg.Any<Func<Ticket, Case, Ticket>>());
        result.Should().BeNull();
    }


    // GetByDeskproTicketId

    [Fact]
    public async Task GetByDeskproTicketId_ShouldReturnTicket_WhenTicketIsFound()
    {
        // Arrange
        var deskproId = 123;
        var sqlCondition = "t.DeskproId = @DeskproId";
        var expectedTicket = new Ticket { Id = 1, DeskproId = 123, CaseNumber = "Ticket A" };
        var expectedCase1 = new Case { Id = 101, PodioItemId = 123, TicketId = 1 };
        var expectedCase2 = new Case { Id = 102, PodioItemId = 456, TicketId = 1 };

        // Mock database return values
        var ticketCasePairs = new List<(Ticket, Case)>
        {
            (expectedTicket, expectedCase1),
            (expectedTicket, expectedCase2)
        };

        _dataAccess
            .Query(
                Arg.Any<string>(),
                Arg.Is<object>(arg => arg.GetType().GetProperty("DeskproId")!.GetValue(arg)!.Equals(deskproId)),
                "TicketId",
                Arg.Any<Func<Ticket, Case, Ticket>>())
            .Returns(call =>
            {
                var mappingFunc = call.Arg<Func<Ticket, Case, Ticket>>();
                var ticketDictionary = new Dictionary<int, Ticket>();

                var tickets = ticketCasePairs
                    .Select(pair => mappingFunc(pair.Item1, pair.Item2))
                    .Where(ticket => ticket != null)
                    .ToList();

                return tickets;
            });

        // Act
        var result = await _sut.GetByDeskproTicketId(deskproId);

        // Assert
        await _dataAccess.Received(1).Query(
            Arg.Is<string>(arg => arg.Contains(sqlCondition)),
            Arg.Is<object>(arg => arg.GetType().GetProperty("DeskproId")!.GetValue(arg)!.Equals(deskproId)),
            "TicketId",
            Arg.Any<Func<Ticket, Case, Ticket>>());
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTicket);
        result.Cases.Count().Should().Be(2);
    }

    [Fact]
    public async Task GetByDeskproTicketId_ShouldReturnNull_WhenTicketIsNotFound()
    {
        // Arrange
        var deskproId = 123;
        var sqlCondition = "t.DeskproId = @DeskproId";
        var ticketCasePairs = new List<(Ticket, Case)>();

        _dataAccess
            .Query(
                Arg.Any<string>(),
                Arg.Is<object>(arg => arg.GetType().GetProperty("DeskproId")!.GetValue(arg)!.Equals(deskproId)),
                "TicketId",
                Arg.Any<Func<Ticket, Case, Ticket>>())
            .Returns(call =>
            {
                var mappingFunc = call.Arg<Func<Ticket, Case, Ticket>>();
                var ticketDictionary = new Dictionary<int, Ticket>();

                var tickets = ticketCasePairs
                    .Select(pair => mappingFunc(pair.Item1, pair.Item2))
                    .Where(ticket => ticket != null)
                    .ToList();

                return tickets;
            });

        // Act
        var result = await _sut.GetByDeskproTicketId(deskproId);

        // Assert
        await _dataAccess.Received(1).Query(
            Arg.Is<string>(arg => arg.Contains(sqlCondition)),
            Arg.Is<object>(arg => arg.GetType().GetProperty("DeskproId")!.GetValue(arg)!.Equals(deskproId)),
            "TicketId",
            Arg.Any<Func<Ticket, Case, Ticket>>());
        result.Should().BeNull();
    }


    // GetByPodioItemId

    [Theory]
    [InlineData(123)]
    [InlineData(456)]
    public async Task GetByPodioItemId_ShouldReturnTicket_WhenTicketIsFound(long podioItemId)
    {
        // Arrange
        var expectedTicket = new Ticket { Id = 1, DeskproId = 123, CaseNumber = "Ticket A" };
        var expectedCase1 = new Case { Id = 101, PodioItemId = 123, TicketId = 1 };
        var expectedCase2 = new Case { Id = 102, PodioItemId = 456, TicketId = 1 };

        var sqlCondition = "c.PodioItemId = @PodioItemId";
        var ticketCasePairs = new List<(Ticket, Case)>
        {
            (expectedTicket, expectedCase1),
            (expectedTicket, expectedCase2)
        };

        _dataAccess
            .Query(
                Arg.Any<string>(),
                Arg.Is<object>(arg => arg.GetType().GetProperty("PodioItemId")!.GetValue(arg)!.Equals(podioItemId)),
                "TicketId",
                Arg.Any<Func<Ticket, Case, Ticket>>())
            .Returns(call =>
            {
                var mappingFunc = call.Arg<Func<Ticket, Case, Ticket>>();
                var ticketDictionary = new Dictionary<int, Ticket>();

                var tickets = ticketCasePairs
                    .Select(pair => mappingFunc(pair.Item1, pair.Item2))
                    .Where(ticket => ticket != null)
                    .ToList();

                return tickets;
            });

        // Act
        var result = await _sut.GetByPodioItemId(podioItemId);

        // Assert
        await _dataAccess.Received(1).Query(
            Arg.Is<string>(arg => arg.Contains(sqlCondition)),
            Arg.Is<object>(arg => arg.GetType().GetProperty("PodioItemId")!.GetValue(arg)!.Equals(podioItemId)),
            "TicketId",
            Arg.Any<Func<Ticket, Case, Ticket>>());
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedTicket);
        result.Cases.Count().Should().Be(2);
    }

    [Fact]
    public async Task GetByPodioItemId_ShouldReturnNull_WhenTicketIsNotFound()
    {
        // Arrange
        long podioItemId = 123;
        var sqlCondition = "c.PodioItemId = @PodioItemId";
        var ticketCasePairs = new List<(Ticket, Case)>();

        _dataAccess
            .Query(
                Arg.Any<string>(),
                Arg.Is<object>(arg => arg.GetType().GetProperty("PodioItemId")!.GetValue(arg)!.Equals(podioItemId)),
                "TicketId",
                Arg.Any<Func<Ticket, Case, Ticket>>())
            .Returns(call =>
            {
                var mappingFunc = call.Arg<Func<Ticket, Case, Ticket>>();
                var ticketDictionary = new Dictionary<int, Ticket>();

                var tickets = ticketCasePairs
                    .Select(pair => mappingFunc(pair.Item1, pair.Item2))
                    .Where(ticket => ticket != null)
                    .ToList();

                return tickets;
            });

        // Act
        var result = await _sut.GetByPodioItemId(podioItemId);

        // Assert
        await _dataAccess.Received(1).Query(
            Arg.Is<string>(arg => arg.Contains(sqlCondition)),
            Arg.Is<object>(arg => arg.GetType().GetProperty("PodioItemId")!.GetValue(arg)!.Equals(podioItemId)),
            "TicketId",
            Arg.Any<Func<Ticket, Case, Ticket>>());
        result.Should().BeNull();
    }


    // GetAll
    [Theory]
    [InlineData(null, null, null, new string[] {})]
    [InlineData(123, null, null, new[] { "t.DeskproId = @DeskproId" })]
    [InlineData(null, 12312312312, null, new[] { "c.PodioItemId = @PodioItemId" })]
    [InlineData(null, 98798798798, null, new[] { "c.PodioItemId = @PodioItemId" })]
    [InlineData(null, null, "1866CBE9-5B44-4A5B-9F92-A906C3345D6C", new[] { "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(null, null, "D06FD2B7-D109-4E36-846C-FB1B1F5C1211", new[] { "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(123, 12312312312, null, new[] { "t.DeskproId = @DeskproId", "c.PodioItemId = @PodioItemId" })]
    [InlineData(123, 98798798798, null, new[] { "t.DeskproId = @DeskproId", "c.PodioItemId = @PodioItemId" })]
    [InlineData(123, null, "1866CBE9-5B44-4A5B-9F92-A906C3345D6C", new[] { "t.DeskproId = @DeskproId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(123, null, "D06FD2B7-D109-4E36-846C-FB1B1F5C1211", new[] { "t.DeskproId = @DeskproId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(null, 12312312312, "1866CBE9-5B44-4A5B-9F92-A906C3345D6C", new[] { "c.PodioItemId = @PodioItemId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(null, 98798798798, "1866CBE9-5B44-4A5B-9F92-A906C3345D6C", new[] { "c.PodioItemId = @PodioItemId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(null, 12312312312, "D06FD2B7-D109-4E36-846C-FB1B1F5C1211", new[] { "c.PodioItemId = @PodioItemId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(null, 98798798798, "D06FD2B7-D109-4E36-846C-FB1B1F5C1211", new[] { "c.PodioItemId = @PodioItemId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(123, 12312312312, "D06FD2B7-D109-4E36-846C-FB1B1F5C1211", new[] { "t.DeskproId = @DeskproId", "c.PodioItemId = @PodioItemId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(123, 98798798798, "D06FD2B7-D109-4E36-846C-FB1B1F5C1211", new[] { "t.DeskproId = @DeskproId", "c.PodioItemId = @PodioItemId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    public async Task GetAll_ShouldReturnTickets_WhenFound(int? deskproId, long? podioItemId, string? filArkivCaseId, string[] sqlConditions)
    {
        // Arrange
        var expectedTicket = new Ticket { Id = 1, DeskproId = 123, CaseNumber = "Ticket A" };
        var expectedCase1 = new Case { Id = 101, PodioItemId = 12312312312, TicketId = 1, FilArkivCaseId = Guid.Parse("1866CBE9-5B44-4A5B-9F92-A906C3345D6C") };
        var expectedCase2 = new Case { Id = 102, PodioItemId = 98798798798, TicketId = 1, FilArkivCaseId = Guid.Parse("D06FD2B7-D109-4E36-846C-FB1B1F5C1211") };

        // Mock database return values
        var ticketCasePairs = new List<(Ticket, Case)>
        {
            (expectedTicket, expectedCase1),
            (expectedTicket, expectedCase2)
        };

        _dataAccess
            .Query(
                Arg.Any<string>(),
                Arg.Any<object>(),
                "TicketId",
                Arg.Any<Func<Ticket, Case, Ticket>>())
            .Returns(call =>
            {
                var mappingFunc = call.Arg<Func<Ticket, Case, Ticket>>();
                var ticketDictionary = new Dictionary<int, Ticket>();

                var tickets = ticketCasePairs
                    .Select(pair => mappingFunc(pair.Item1, pair.Item2))
                    .Where(ticket => ticket != null)
                    .ToList();

                return tickets;
            });

        Guid? parsedFilArkivCaseId = filArkivCaseId != null ? Guid.Parse(filArkivCaseId) : null;

        
        
        // Act
        var result = await _sut.GetAll(deskproId, podioItemId, parsedFilArkivCaseId);

        // Assert
        await _dataAccess.Received(1).Query(
            Arg.Is<string>(arg => sqlConditions.All(value => arg.Contains(value))),
            Arg.Is<object>(arg => MatchesAnonymousObject(arg, deskproId, podioItemId, parsedFilArkivCaseId)),            
            "TicketId",
            Arg.Any<Func<Ticket, Case, Ticket>>());
        result.Should().NotBeNull();
        result.Count().Should().Be(1);
        result.First().Should().Be(expectedTicket);
        result.First().Cases.Count().Should().Be(2);
    }

    bool MatchesAnonymousObject(object arg, int? deskproId, long? podioItemId, Guid? parsedFilArkivCaseId)
    {
        var deskproIdValue = arg.GetType().GetProperty("DeskproId")!.GetValue(arg);
        var podioItemIdValue = arg.GetType().GetProperty("PodioItemId")!.GetValue(arg);
        var filArkivCaseIdValue = arg.GetType().GetProperty("FilArkivCaseId")!.GetValue(arg);

        return
            (deskproIdValue == null && deskproId == null || deskproIdValue?.Equals(deskproId) == true) &&
            (podioItemIdValue == null && podioItemId == null || podioItemIdValue?.Equals(podioItemId) == true) &&
            (filArkivCaseIdValue == null && parsedFilArkivCaseId == null || filArkivCaseIdValue?.Equals(parsedFilArkivCaseId) == true);
    }

    [Theory]
    [InlineData(null, null, null, new string[] {})]
    [InlineData(123, null, null, new[] { "t.DeskproId = @DeskproId" })]
    [InlineData(null, 12312312312, null, new[] { "c.PodioItemId = @PodioItemId" })]
    [InlineData(null, 98798798798, null, new[] { "c.PodioItemId = @PodioItemId" })]
    [InlineData(null, null, "1866CBE9-5B44-4A5B-9F92-A906C3345D6C", new[] { "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(null, null, "D06FD2B7-D109-4E36-846C-FB1B1F5C1211", new[] { "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(123, 12312312312, null, new[] { "t.DeskproId = @DeskproId", "c.PodioItemId = @PodioItemId" })]
    [InlineData(123, 98798798798, null, new[] { "t.DeskproId = @DeskproId", "c.PodioItemId = @PodioItemId" })]
    [InlineData(123, null, "1866CBE9-5B44-4A5B-9F92-A906C3345D6C", new[] { "t.DeskproId = @DeskproId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(123, null, "D06FD2B7-D109-4E36-846C-FB1B1F5C1211", new[] { "t.DeskproId = @DeskproId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(null, 12312312312, "1866CBE9-5B44-4A5B-9F92-A906C3345D6C", new[] { "c.PodioItemId = @PodioItemId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(null, 98798798798, "1866CBE9-5B44-4A5B-9F92-A906C3345D6C", new[] { "c.PodioItemId = @PodioItemId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(null, 12312312312, "D06FD2B7-D109-4E36-846C-FB1B1F5C1211", new[] { "c.PodioItemId = @PodioItemId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(null, 98798798798, "D06FD2B7-D109-4E36-846C-FB1B1F5C1211", new[] { "c.PodioItemId = @PodioItemId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(123, 12312312312, "D06FD2B7-D109-4E36-846C-FB1B1F5C1211", new[] { "t.DeskproId = @DeskproId", "c.PodioItemId = @PodioItemId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    [InlineData(123, 98798798798, "D06FD2B7-D109-4E36-846C-FB1B1F5C1211", new[] { "t.DeskproId = @DeskproId", "c.PodioItemId = @PodioItemId", "c.FilArkivCaseId = @FilArkivCaseId" })]
    public async Task GetAll_ShouldReturnEmptyCollection_WhenNoTicketsAreFound(int? deskproId, long? podioItemId, string? filArkivCaseId, string[] sqlConditions)
    {
        // Mock database return values
        var ticketCasePairs = new List<(Ticket, Case)>();

        _dataAccess
            .Query(
                Arg.Any<string>(),
                Arg.Any<object>(),
                "TicketId",
                Arg.Any<Func<Ticket, Case, Ticket>>())
            .Returns(call =>
            {
                var mappingFunc = call.Arg<Func<Ticket, Case, Ticket>>();
                var ticketDictionary = new Dictionary<int, Ticket>();

                var tickets = ticketCasePairs
                    .Select(pair => mappingFunc(pair.Item1, pair.Item2))
                    .Where(ticket => ticket != null)
                    .ToList();

                return tickets;
            });

        Guid? parsedFilArkivCaseId = filArkivCaseId != null ? Guid.Parse(filArkivCaseId) : null;



        // Act
        var result = await _sut.GetAll(deskproId, podioItemId, parsedFilArkivCaseId);

        // Assert
        await _dataAccess.Received(1).Query(
            Arg.Is<string>(arg => sqlConditions.All(value => arg.Contains(value))),
            Arg.Is<object>(arg => MatchesAnonymousObject(arg, deskproId, podioItemId, parsedFilArkivCaseId)),
            "TicketId",
            Arg.Any<Func<Ticket, Case, Ticket>>());
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    
    // Update

    [Fact]
    public async Task Update_ShouldReturnTrue_WhenTicketIsUpdated()
    {
        // Arrange
        var ticket = new Ticket
        {
            Id = 1,
            CaseNumber = "case number",
            DeskproId = 123
        };

        _dataAccess
            .Execute(Arg.Any<string>(), ticket)
            .Returns(1);

        var expectedTicket = JsonSerializer.Serialize(ticket);
        var ticketCopy = JsonSerializer.Deserialize<Ticket>(expectedTicket);

        // Act
        var result = await _sut.Update(ticket);

        // Assert
        result.Should().BeTrue();
        JsonSerializer.Serialize(ticket).Should().Be(JsonSerializer.Serialize(ticketCopy));
        await _dataAccess.Received(1).Execute(Arg.Any<string>(), ticket);
    }

    [Fact]
    public async Task Update_ShouldReturnFalse_WhenTicketIsNotUpdated()
    {
        // Arrange
        var ticket = new Ticket
        {
            Id = 1,
            CaseNumber = "case number",
            DeskproId = 123
        };

        var expectedTicket = JsonSerializer.Serialize(ticket);
        var ticketCopy = JsonSerializer.Deserialize<Ticket>(expectedTicket);

        _dataAccess
            .Execute(Arg.Any<string>(), ticket)
            .Returns(0);

        // Act
        var result = await _sut.Update(ticket);

        // Assert
        result.Should().BeFalse();
        JsonSerializer.Serialize(ticket).Should().Be(JsonSerializer.Serialize(ticketCopy));
        await _dataAccess.Received(1).Execute(
            Arg.Any<string>(),
            ticket);
    }

    [Fact]
    public async Task Update_ShouldThrowValidationException_WhenTicketIsNotValid()
    {
        // Arrange
        var ticket = new Ticket
        {
            DeskproId = default
        };

        var expectedTicket = JsonSerializer.Serialize(ticket);
        var ticketCopy = JsonSerializer.Deserialize<Ticket>(expectedTicket);

        // Act
        var act = () => _sut.Update(ticket);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await _dataAccess.Received(0).ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>());
        JsonSerializer.Serialize(ticket).Should().Be(JsonSerializer.Serialize(ticketCopy));

    }

}
