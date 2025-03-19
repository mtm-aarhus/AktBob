using AktBob.Database.Contracts;
using AktBob.Database.Decorators;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;

namespace AktBob.Database.Tests.Unit.Decorators;
public class TicketRepositoryExceptionDecoratorTests
{
    private readonly TicketRepositoryExceptionDecorator _sut;
    private readonly ITicketRepository _inner = Substitute.For<ITicketRepository>();
    private readonly FakeLogger<TicketRepository> _logger = new FakeLogger<TicketRepository>();
    public TicketRepositoryExceptionDecoratorTests()
    {
        _sut = new TicketRepositoryExceptionDecorator(_inner, _logger);
    }

    [Fact]
    public async Task Add_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var ticket = new Ticket();
        _inner.Add(Arg.Any<Ticket>()).Returns(true);

        // Act
        var result = await _sut.Add(ticket);

        // Assert
        result.Should().BeTrue();
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Add_ShouldLogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var ticket = new Ticket();
        _inner.Add(Arg.Any<Ticket>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Add(ticket);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task Get_ShouldReturnResult_WhenInnerModuleSucceedsWithTicket()
    {
        // Arrange
        var expectedTicket = new Ticket();
        _inner.Get(Arg.Any<int>()).Returns(expectedTicket);

        // Act
        var result = await _sut.Get(1);

        // Assert
        result.Should().Be(expectedTicket);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Get_ShouldReturnResult_WhenInnerModuleSucceedsWithNull()
    {
        // Arrange
        _inner.Get(Arg.Any<int>()).ReturnsNull();

        // Act
        var result = await _sut.Get(1);

        // Assert
        result.Should().BeNull();
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Get_ShouldLogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        _inner.Get(Arg.Any<int>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Get(1);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
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
    public async Task GetAll_ShouldReturnResult_WhenInnerModuleSucceeds(int? deskproId, long? podioItemId, string? filArkivCaseId)
    {
        // Arrange
        var expectedTickets = new List<Ticket>();
        Guid? parsedFilArkivCaseId = !string.IsNullOrEmpty(filArkivCaseId) ? Guid.Parse(filArkivCaseId) : null;
        _inner.GetAll(Arg.Any<int?>(), Arg.Any<long?>(), Arg.Any<Guid?>()).Returns(expectedTickets);

        // Act
        var result = await _sut.GetAll(deskproId, podioItemId, parsedFilArkivCaseId);

        // Assert
        result.Should().BeSameAs(expectedTickets);
        _logger.Collector.Count.Should().Be(0);
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
    public async Task GetAll_ShouldLogErrorAndRethrowException_WhenInnerModuleThrowsExecption(int? deskproId, long? podioItemId, string? filArkivCaseId)
    {
        // Arrange
        Guid? parsedFilArkivCaseId = !string.IsNullOrEmpty(filArkivCaseId) ? Guid.Parse(filArkivCaseId) : null;
        _inner.GetAll(Arg.Any<int?>(), Arg.Any<long?>(), Arg.Any<Guid?>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetAll(deskproId, podioItemId, parsedFilArkivCaseId);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task GetByDeskproTicketId_ShouldReturnResult_WhenInnerModuleReturnsTicket()
    {
        // Arrange
        var id = 1;
        var expectedTicket = new Ticket { DeskproId = id };
        _inner.GetByDeskproTicketId(Arg.Any<int>()).Returns(expectedTicket);

        // Act
        var result = await _sut.GetByDeskproTicketId(id);

        // Assert
        result.Should().Be(expectedTicket);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetByDeskproTicketId_ShouldReturnResult_WhenInnerModuleReturnsNull()
    {
        // Arrange
        _inner.GetByDeskproTicketId(Arg.Any<int>()).ReturnsNull();

        // Act
        var result = await _sut.GetByDeskproTicketId(1);

        // Assert
        result.Should().BeNull();
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetByDeskproTicketId_ShouldLogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        _inner.GetByDeskproTicketId(Arg.Any<int>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetByDeskproTicketId(1);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task GetByPodioItemId_ShouldReturnResult_WhenInnerModuleReturnsTicket()
    {
        // Arrange
        var expectedTicket = new Ticket();
        _inner.GetByPodioItemId(Arg.Any<long>()).Returns(expectedTicket);

        // Act
        var result = await _sut.GetByPodioItemId(12312312);

        // Assert
        result.Should().Be(expectedTicket);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetByPodioItemId_ShouldReturnResult_WhenInnerModuleReturnsNull()
    {
        // Arrange
        _inner.GetByPodioItemId(Arg.Any<long>()).ReturnsNull();

        // Act
        var result = await _sut.GetByPodioItemId(12312312);

        // Assert
        result.Should().BeNull();
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetByPodioItemId_ShouldLogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        _inner.GetByPodioItemId(Arg.Any<long>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetByPodioItemId(12312312);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task Update_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var ticket = new Ticket();
        _inner.Update(Arg.Any<Ticket>()).Returns(true);

        // Act
        var result = await _sut.Update(ticket);

        // Assert
        result.Should().BeTrue();
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Update_ShouldLogErrorAndRethroException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var ticket = new Ticket();
        _inner.Update(Arg.Any<Ticket>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Update(ticket);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
