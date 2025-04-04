using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Decorators;
using Ardalis.Result;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Collections.ObjectModel;

namespace AktBob.Deskpro.Tests.Unit.Decorators;
public class ModuleExceptionDecoratorTests
{
    private readonly ModuleExceptionDecorator _sut;
    private readonly IDeskproModule _inner = Substitute.For<IDeskproModule>();
    private readonly FakeLogger<DeskproModule> _logger = new FakeLogger<DeskproModule>();

    public ModuleExceptionDecoratorTests()
    {
        _sut = new ModuleExceptionDecorator(_inner, _logger);
    }

    [Fact]
    public async Task GetCustomFieldSpecifications_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var collection = new List<CustomFieldSpecificationDto>();
        var innerResult = Result<IReadOnlyCollection<CustomFieldSpecificationDto>>.Success(collection);
        var expectedResult = Result<IReadOnlyCollection<CustomFieldSpecificationDto>>.Success(collection);
        _inner.GetCustomFieldSpecifications(Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetCustomFieldSpecifications(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).GetCustomFieldSpecifications(Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetCustomFieldSpecifications_LogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        _inner.GetCustomFieldSpecifications(Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetCustomFieldSpecifications(CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _inner.Received(1).GetCustomFieldSpecifications(Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task GetMessage_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var dto = new MessageDto();
        var innerResult = Result.Success(dto);
        var expectedResult = Result.Success(dto);

        _inner.GetMessage(ticketId, messageId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetMessage(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).GetMessage(ticketId, messageId, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetMessage_LogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        _inner.GetMessage(ticketId, messageId, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetMessage(ticketId, messageId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _inner.Received(1).GetMessage(ticketId, messageId, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task DownloadMessageAttachment_ReturnInnerResult_WhenInnerResponseIsSuccessful()
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
        await _inner.Received(1).DownloadMessageAttachment(downloadUrl, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task DownloadMessageAttachment_LogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var downloadUrl = "download url";
        _inner.DownloadMessageAttachment(downloadUrl, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.DownloadMessageAttachment(downloadUrl, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _inner.Received(1).DownloadMessageAttachment(downloadUrl, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task GetMessageAttachments_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var collection = new Collection<AttachmentDto>();
        var innerResult = Result<IReadOnlyCollection<AttachmentDto>>.Success(collection);
        var expecetedResult = Result<IReadOnlyCollection<AttachmentDto>>.Success(collection);
        _inner.GetMessageAttachments(ticketId, messageId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetMessageAttachments(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expecetedResult);
        await _inner.Received(1).GetMessageAttachments(ticketId, messageId, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetMessageAttachments_LogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        _inner.GetMessageAttachments(ticketId, messageId, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetMessageAttachments(ticketId, messageId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _inner.Received(1).GetMessageAttachments(ticketId, messageId, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task GetMessages_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var collection = new Collection<MessageDto>();
        var innerResult = Result<IReadOnlyCollection<MessageDto>>.Success(collection);
        var expectedResult = Result<IReadOnlyCollection<MessageDto>>.Success(collection);
        _inner.GetMessages(ticketId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetMessages(ticketId, CancellationToken.None);
                
        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).GetMessages(ticketId, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetMessages_LogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var ticketId = 1;
        _inner.GetMessages(ticketId, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetMessages(ticketId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _inner.Received(1).GetMessages(ticketId, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task GetPerson_ById_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var personId = 1;
        var personDto = new PersonDto { Id = personId };
        var innerResult = Result.Success(personDto);
        var expectedResult = Result.Success(personDto);
        _inner.GetPerson(personId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetPerson(personId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).GetPerson(personId, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0); 
    }

    [Fact]
    public async Task GetPerson_ById_LogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var personId = 1;
        _inner.GetPerson(personId, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetPerson(personId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _inner.Received(1).GetPerson(personId, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task GetPerson_ByEmail_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var personEmail = "email";
        var personDto = new PersonDto { Email = personEmail };
        var innerResult = Result.Success(personDto);
        var expectedResult = Result.Success(personDto);
        _inner.GetPerson(personEmail, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetPerson(personEmail, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).GetPerson(personEmail, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetPerson_ByEmail_LogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var personEmail = "email";
        _inner.GetPerson(personEmail, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetPerson(personEmail, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _inner.Received(1).GetPerson(personEmail, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task GetTicket_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var ticketDto = new TicketDto { Id = ticketId };
        var innerResult = Result.Success(ticketDto);
        var expectedResult = Result.Success(ticketDto);
        _inner.GetTicket(ticketId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetTicket(1, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).GetTicket(ticketId, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetTicket_LogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var ticketId = 1;
        _inner.GetTicket(ticketId, Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetTicket(ticketId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _inner.Received(1).GetTicket(ticketId, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task GetTicketsByFieldSearch_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        int[] fields = { 1, 2, 3 };
        var searchValue = "search value";
        var collection = new Collection<TicketDto> { new TicketDto { Id = 1 } };
        var innerResult = Result<IReadOnlyCollection<TicketDto>>.Success(collection);
        var expectedResult = Result<IReadOnlyCollection<TicketDto>>.Success(collection);
        _inner.GetTicketsByFieldSearch(fields, searchValue, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetTicketsByFieldSearch(fields, searchValue, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).GetTicketsByFieldSearch(fields, searchValue, Arg.Any<CancellationToken>());
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetTicketsByFieldSearch_LogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        int[] fields = { 1, 2, 3 };
        var searchValue = "search value";
        _inner.GetTicketsByFieldSearch(Arg.Any<int[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.GetTicketsByFieldSearch(fields, searchValue, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _inner.Received(1).GetTicketsByFieldSearch(fields, searchValue, Arg.Any<CancellationToken>());
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public void InvokeWebhook_ReturnInnerResult_WhenInnerResponseIsSuccessful()
    {
        // Arrange
        var webhookId = "webhook id";
        var payload = "payload";

        // Act
        _sut.InvokeWebhook(webhookId, payload);

        // Assert
        _inner.Received(1).InvokeWebhook(webhookId, payload);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public void InvokeWebhook_LogErrorAndRethrowException_WhenInnerModuleThrowsException()
    {
        // Arrange
        var webhookId = "webhook id";
        var payload = "payload";
        _inner
            .When(x => x.InvokeWebhook(webhookId, payload))
            .Do(x => throw new Exception());

        // Act
        var act = () => _sut.InvokeWebhook(webhookId, payload);

        // Assert
        act.Should().Throw<Exception>();
        _inner.Received(1).InvokeWebhook(webhookId, payload);
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
