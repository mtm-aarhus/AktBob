using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Decorators;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AktBob.Deskpro.Tests.Unit.Decorators;
public class ModuleLoggingDecoratorTests
{
    private readonly ModuleLoggingDecorator _sut;
    private readonly IDeskproModule _inner = Substitute.For<IDeskproModule>();
    private readonly FakeLogger<DeskproModule> _logger = new FakeLogger<DeskproModule>();

    public ModuleLoggingDecoratorTests()
    {
        _sut = new ModuleLoggingDecorator(_inner, _logger);
    }

    [Fact]
    public async Task GetCustomFieldSpecifications_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var content = new List<CustomFieldSpecificationDto>();
        var innerResult = Result.Success<IReadOnlyCollection<CustomFieldSpecificationDto>>(content);
        var expectedResult = Result.Success<IReadOnlyCollection<CustomFieldSpecificationDto>>(content);
        _inner.GetCustomFieldSpecifications(Arg.Any<CancellationToken>()).Returns(Task.FromResult(innerResult));

        // Act
        var result = await _sut.GetCustomFieldSpecifications(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).GetCustomFieldSpecifications(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetCustomFieldSpecifications_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var innerResult = Result<IReadOnlyCollection<CustomFieldSpecificationDto>>.Error();
        _inner.GetCustomFieldSpecifications(Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetCustomFieldSpecifications(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(innerResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        await _inner.Received(1).GetCustomFieldSpecifications(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMessage_ShouldLogInformationAndResultReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var innerResult = Result.Success(new MessageDto());
        var expectedResult = Result.Success(new MessageDto());
        _inner.GetMessage(ticketId, messageId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetMessage(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.GetMessage(ticketId, messageId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMessage_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var innerResult = Result<MessageDto>.Error();
        var expectedResult = Result<MessageDto>.Error();
        _inner.GetMessage(ticketId, messageId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetMessage(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        await _inner.GetMessage(ticketId, messageId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DownloadMessageAttachment_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var downloadUrl = "download url";
        using Stream stream = new MemoryStream();
        var innerResult = Result.Success(stream);
        var expectedResult = Result.Success(stream);
        _inner.DownloadMessageAttachment(downloadUrl, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.DownloadMessageAttachment(downloadUrl, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).DownloadMessageAttachment(downloadUrl, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DownloadMessageAttachment_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var downloadUrl = "download url";
        var innerResult = Result<Stream>.Error();
        var expectedResult = Result<Stream>.Error();
        _inner.DownloadMessageAttachment(downloadUrl, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.DownloadMessageAttachment(downloadUrl, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        await _inner.Received(1).DownloadMessageAttachment(downloadUrl, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMessageAttachments_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var innerResult = Result<IReadOnlyCollection<AttachmentDto>>.Success(new List<AttachmentDto>());
        var expectedResult = Result<IReadOnlyCollection<AttachmentDto>>.Success(new List<AttachmentDto>());
        _inner.GetMessageAttachments(ticketId, messageId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetMessageAttachments(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).GetMessageAttachments(ticketId, messageId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMessageAttachments_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var innerResult = Result<IReadOnlyCollection<AttachmentDto>>.Error();
        var expectedResult = Result<IReadOnlyCollection<AttachmentDto>>.Error();
        _inner.GetMessageAttachments(ticketId, messageId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetMessageAttachments(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        await _inner.Received(1).GetMessageAttachments(ticketId, messageId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMessages_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var ticketId = 1;
        var innerResult = Result<IReadOnlyCollection<MessageDto>>.Success(new List<MessageDto>());
        var expectedResult = Result<IReadOnlyCollection<MessageDto>>.Success(new List<MessageDto>());
        _inner.GetMessages(ticketId, CancellationToken.None).Returns(innerResult);
        
        // Act
        var result = await _sut.GetMessages(ticketId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).GetMessages(ticketId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMessages_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var innerResult = Result<IReadOnlyCollection<MessageDto>>.Error();
        var expectedResult = Result<IReadOnlyCollection<MessageDto>>.Error();
        _inner.GetMessages(ticketId, CancellationToken.None).Returns(innerResult);

        // Act
        var result = await _sut.GetMessages(ticketId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        await _inner.Received(1).GetMessages(ticketId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPerson_ById_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var personId = 1;
        var personDto = new PersonDto
        {
            Id = personId
        };

        var innerResult = Result.Success(personDto);
        var expectedResult = Result.Success(personDto);

        _inner.GetPerson(personId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetPerson(personId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).GetPerson(personId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPerson_ById_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var personId = 1;
        var personDto = new PersonDto
        {
            Id = personId
        };

        var innerResult = Result<PersonDto>.Error();
        var expectedResult = Result<PersonDto>.Error();

        _inner.GetPerson(personId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetPerson(personId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        await _inner.Received(1).GetPerson(personId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPerson_ByEmail_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var email = "email";
        var personDto = new PersonDto
        {
            Email = email
        };

        var innerResult = Result.Success(personDto);
        var expectedResult = Result.Success(personDto);

        _inner.GetPerson(email, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetPerson(email, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).GetPerson(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPerson_ByEmail_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var email = "email";
        var personDto = new PersonDto
        {
            Email = email
        };

        var innerResult = Result<PersonDto>.Error();
        var expectedResult = Result<PersonDto>.Error();

        _inner.GetPerson(email, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetPerson(email, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        await _inner.Received(1).GetPerson(email, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTicket_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var ticketId = 1;
        var ticketDto = new TicketDto
        {
            Id = ticketId
        };

        var innerResult = Result.Success(ticketDto);
        var expectedResult = Result.Success(ticketDto);

        _inner.GetTicket(ticketId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetTicket(ticketId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).GetTicket(ticketId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTicket_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var ticketDto = new TicketDto
        {
            Id = ticketId
        };

        var innerResult = Result<TicketDto>.Error();
        var expectedResult = Result<TicketDto>.Error();

        _inner.GetTicket(ticketId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetTicket(ticketId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        await _inner.Received(1).GetTicket(ticketId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTicketsByFieldSearch_ShouldLogInformationAndReturnInnerResult_WhenInvoked()
    {
        // Arrange
        int[] fields = [1, 2, 3];
        var searchValue = "search value";

        var collection = new List<TicketDto>();
        var innerResult = Result<IReadOnlyCollection<TicketDto>>.Success(collection);
        var expectedResult = Result<IReadOnlyCollection<TicketDto>>.Success(collection);

        _inner.GetTicketsByFieldSearch(fields, searchValue, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetTicketsByFieldSearch(fields, searchValue, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
        await _inner.Received(1).GetTicketsByFieldSearch(fields, searchValue, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTicketsByFieldSearch_ShouldLogDebugAndReturnInnerResult_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        int[] fields = [1, 2, 3];
        var searchValue = "search value";

        var collection = new List<TicketDto>();
        var innerResult = Result<IReadOnlyCollection<TicketDto>>.Error();
        var expectedResult = Result<IReadOnlyCollection<TicketDto>>.Error();

        _inner.GetTicketsByFieldSearch(fields, searchValue, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetTicketsByFieldSearch(fields, searchValue, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Debug);
        await _inner.Received(1).GetTicketsByFieldSearch(fields, searchValue, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void InvokeWebhook_ShouldInvokeInnerAndLogInformation_WhenInvoked()
    {
        // Arrange
        var webhookId = "webhookId";
        var payload = "payload";

        // Act
        _inner.InvokeWebhook(webhookId, payload);

        // Assert
        _inner.Received(1).InvokeWebhook(webhookId, payload);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Information);
    }
}