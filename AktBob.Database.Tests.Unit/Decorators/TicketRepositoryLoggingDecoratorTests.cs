using AktBob.Database.Contracts;
using AktBob.Database.Decorators;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace AktBob.Database.Tests.Unit.Decorators;
public class TicketRepositoryLoggingDecoratorTests
{
    private readonly TicketRepositoryLoggingDecorator _sut;
    private readonly ITicketRepository _inner = Substitute.For<ITicketRepository>();
    private readonly FakeLogger<TicketRepository> _logger = new FakeLogger<TicketRepository>();

    public TicketRepositoryLoggingDecoratorTests()
    {
        _sut = new TicketRepositoryLoggingDecorator(_inner, _logger);
    }

    [Fact]
    public async Task Add_ShouldLogInformationAndReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var ticket = new Ticket();
        _inner.Add(Arg.Any<Ticket>()).Returns(true);

        // Act
        var result = await _sut.Add(ticket);

        // Assert
        result.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        _logger.Collector.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Add_ShouldLogDebugAndReturnResult_WhenInnerModuleFails()
    {
        // Arrange
        var ticket = new Ticket();
        _inner.Add(Arg.Any<Ticket>()).Returns(false);

        // Act
        var result = await _sut.Add(ticket);

        // Assert
        result.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        _logger.Collector.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task Get_ShouldLogInformationAndReturnResult_WhenInnerModuleReturnsTicket()
    {
        // Arrange
        var expectedTicket = new Ticket();
        _inner.Get(Arg.Any<int>()).Returns(expectedTicket);

        // Act
        var result = await _sut.Get(1);

        // Assert
        result.Should().Be(expectedTicket);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        _logger.Collector.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Get_ShouldLogDebugAndReturnResult_WhenInnerModuleReturnsNull()
    {
        // Arrange
        _inner.Get(Arg.Any<int>()).ReturnsNull();

        // Act
        var result = await _sut.Get(1);

        // Assert
        result.Should().BeNull();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        _logger.Collector.Count.Should().BeGreaterThan(1);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(123, null, null)]
    [InlineData(null, 12312312312, null)]
    [InlineData(null, null, "5020E9AA-FCEB-4AE3-B5FE-49965021F9BA")]
    [InlineData(null, 12312312312, "5020E9AA-FCEB-4AE3-B5FE-49965021F9BA")]
    [InlineData(123, 12312312312, null)]
    [InlineData(123, 12312312312, "5020E9AA-FCEB-4AE3-B5FE-49965021F9BA")]
    [InlineData(123, null, "5020E9AA-FCEB-4AE3-B5FE-49965021F9BA")]
    public async Task GetAll_ShouldLogInformationAndReturnResult_WhenInnerModuleReturnsCollectionWithTickets(int? deskproId, long? podioItemId, string? filArkivCaseId)
    {
        // Arrange
        var expectedTickets = new List<Ticket>
        {
            new Ticket(),
            new Ticket()
        };
        Guid? parsedFilArkivCaseId = !string.IsNullOrEmpty(filArkivCaseId) ? Guid.Parse(filArkivCaseId) : null;
        _inner.GetAll(Arg.Any<int?>(), Arg.Any<long?>(), Arg.Any<Guid?>()).Returns(expectedTickets);

        // Act
        var result = await _sut.GetAll(deskproId, podioItemId, parsedFilArkivCaseId);

        // Assert
        result.Should().BeSameAs(expectedTickets);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        _logger.Collector.Count.Should().BeGreaterThan(0);
    }

    [Theory]
    [InlineData(null, null, null)]
    [InlineData(123, null, null)]
    [InlineData(null, 12312312312, null)]
    [InlineData(null, null, "5020E9AA-FCEB-4AE3-B5FE-49965021F9BA")]
    [InlineData(null, 12312312312, "5020E9AA-FCEB-4AE3-B5FE-49965021F9BA")]
    [InlineData(123, 12312312312, null)]
    [InlineData(123, 12312312312, "5020E9AA-FCEB-4AE3-B5FE-49965021F9BA")]
    [InlineData(123, null, "5020E9AA-FCEB-4AE3-B5FE-49965021F9BA")]
    public async Task GetAll_ShouldLogDebugAndReturnResult_WhenInnerModuleReturnsEmptyCollection(int? deskproId, long? podioItemId, string? filArkivCaseId)
    {
        // Arrange
        var expectedTickets = new List<Ticket>();
        Guid? parsedFilArkivCaseId = !string.IsNullOrEmpty(filArkivCaseId) ? Guid.Parse(filArkivCaseId) : null;
        _inner.GetAll(Arg.Any<int?>(), Arg.Any<long?>(), Arg.Any<Guid?>()).Returns(expectedTickets);

        // Act
        var result = await _sut.GetAll(deskproId, podioItemId, parsedFilArkivCaseId);

        // Assert
        result.Should().BeEmpty();
        result.Should().BeSameAs(expectedTickets);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        _logger.Collector.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task GetByDeskproTicketId_ShouldLogInformationAndReturnResult_WhenInnerModuleReturnsTicket()
    {
        // Arrange
        var expectedTicket = new Ticket();
        _inner.GetByDeskproTicketId(Arg.Any<int>()).Returns(expectedTicket);

        // Act
        var result = await _sut.GetByDeskproTicketId(1);

        // Assert
        result.Should().Be(expectedTicket);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        _logger.Collector.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetByDeskproTicketId_ShouldLogDebugAndReturnResult_WhenInnerModuleReturnsNull()
    {
        // Arrange
        _inner.GetByDeskproTicketId(Arg.Any<int>()).ReturnsNull();

        // Act
        var result = await _sut.GetByDeskproTicketId(1);

        // Assert
        result.Should().BeNull();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        _logger.Collector.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task GetByPodioItemId_ShouldLogInformationAndReturnResult_WhenInnerModuleReturnsTicket()
    {
        // Arrange
        var expectedTicket = new Ticket();
        _inner.GetByPodioItemId(Arg.Any<long>()).Returns(expectedTicket);

        // Act
        var result = await _sut.GetByPodioItemId(12312312);

        // Assert
        result.Should().Be(expectedTicket);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        _logger.Collector.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetByPodioItemId_ShouldLogDebugAndReturnResult_WhenInnerModuleReturnsNull()
    {
        // Arrange
        _inner.GetByPodioItemId(Arg.Any<long>()).ReturnsNull();

        // Act
        var result = await _sut.GetByPodioItemId(123123212);

        // Assert
        result.Should().BeNull();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        _logger.Collector.Count.Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task Update_ShouldLogInformationAndReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var ticket = new Ticket();
        _inner.Update(Arg.Any<Ticket>()).Returns(true);

        // Act
        var result = await _sut.Update(ticket);

        // Assert
        result.Should().BeTrue();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        _logger.Collector.Count.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Update_ShouldLogDebugAndReturnResult_WhenInnerModuleFails()
    {
        // Arrange
        var ticket = new Ticket();
        _inner.Update(Arg.Any<Ticket>()).Returns(false);

        // Act
        var result = await _sut.Update(ticket);

        // Assert
        result.Should().BeFalse();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        _logger.Collector.Count.Should().BeGreaterThan(1);
    }
}
