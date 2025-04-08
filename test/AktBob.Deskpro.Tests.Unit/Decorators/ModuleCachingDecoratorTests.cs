using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Decorators;
using AktBob.Shared;
using Ardalis.Result;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Collections.ObjectModel;

namespace AktBob.Deskpro.Tests.Unit.Decorators;
public class ModuleCachingDecoratorTests
{
    private readonly ModuleCachingDecorator _sut;
    private readonly IDeskproModule _inner = Substitute.For<IDeskproModule>();
    private readonly ICacheService _cache = Substitute.For<ICacheService>();

    public ModuleCachingDecoratorTests()
    {
        _sut = new ModuleCachingDecorator(_inner, _cache);
    }

    [Fact]
    public async Task DownloadMessageAttachment_ShouldReturnInnerResult_WhenInvoked()
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
    }

    [Fact]
    public async Task GetMessages_ShouldReturnInnerResult_WhenInvoked()
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
    }

    [Fact]
    public async Task GetTicket_ShouldReturnInnerResult_WhenInvoked()
    {
        // Arrange
        var ticketId = 1;
        var ticketDto = new TicketDto { Id= ticketId };
        var innerResult = Result.Success(ticketDto);
        var expectedResult = Result.Success(ticketDto);
        _inner.GetTicket(ticketId, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetTicket(ticketId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).GetTicket(ticketId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTicketsByFieldSearch_ShouldReturnInnerResult_WhenInvoked()
    {
        // Arrange
        int[] fields = { 1, 2, 3 };
        var searchValue = "search value";
        var collection = new Collection<TicketDto>();
        var innerResult = Result<IReadOnlyCollection<TicketDto>>.Success(collection);
        var expectedResult = Result<IReadOnlyCollection<TicketDto>>.Success(collection);
        _inner.GetTicketsByFieldSearch(fields, searchValue, Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetTicketsByFieldSearch(fields, searchValue, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        await _inner.Received(1).GetTicketsByFieldSearch(fields, searchValue, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void InvokeWebhook_ShouldInvokeInner_WhenInvoked()
    {
        // Arrange
        var webhookId = "webhook id";
        var payload = "payload";

        // Act
        _sut.InvokeWebhook(webhookId, payload);

        // Assert
        _inner.Received(1).InvokeWebhook(webhookId, payload);
    }

    [Fact]
    public async Task GetCustomFieldSpecifications_ShouldReturnCachedValue_WhenCachedHasData()
    {
        // Arrange
        var cacheKey = "Deskpro_CustomFieldSpecifications";
        var collection = new Collection<CustomFieldSpecificationDto>
        {
            new CustomFieldSpecificationDto(1, "title", new Dictionary<int, string>())
        };
        
        var expectedValue = Result<IReadOnlyCollection<CustomFieldSpecificationDto>>.Success(collection);
        _cache.Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey)).Returns(collection);

        // Act
        var result = await _sut.GetCustomFieldSpecifications(CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedValue);
        _cache.Received(1).Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey));
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<IReadOnlyCollection<CustomFieldSpecificationDto>>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task GetCustomFieldSpecifications_ShouldCallInnerAndCacheResult_WhenCacheDoesNotHaveData()
    {
        // Arrange
        var cacheKey = "Deskpro_CustomFieldSpecifications";
        var collection = new Collection<CustomFieldSpecificationDto>
        {
            new CustomFieldSpecificationDto(1, "title", new Dictionary<int, string>())
        };
        var innerResult = Result<IReadOnlyCollection<CustomFieldSpecificationDto>>.Success(collection);
        _inner.GetCustomFieldSpecifications(Arg.Any<CancellationToken>()).Returns(innerResult);
        _cache.Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey)).ReturnsNull();

        // Act
        await _sut.GetCustomFieldSpecifications(CancellationToken.None);

        // Assert
        await _inner.Received(1).GetCustomFieldSpecifications(Arg.Any<CancellationToken>());
        _cache.Received(1).Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey));
        _cache.Received(1).Set(Arg.Is(cacheKey), Arg.Is(innerResult.Value), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task GetCustomFieldSpecifications_ShouldCallInnerAndCacheResult_WhenCacheIsHitButDataIsEmpty()
    {
        // Arrange
        var cacheKey = "Deskpro_CustomFieldSpecifications";
        var collection = new Collection<CustomFieldSpecificationDto>
        {
            new CustomFieldSpecificationDto(1, "title", new Dictionary<int, string>())
        };
        Collection<CustomFieldSpecificationDto> cachedValue = [];
        var innerResult = Result<IReadOnlyCollection<CustomFieldSpecificationDto>>.Success(collection);
        _inner.GetCustomFieldSpecifications(Arg.Any<CancellationToken>()).Returns(innerResult);
        _cache.Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey)).Returns(cachedValue);

        // Act
        await _sut.GetCustomFieldSpecifications(CancellationToken.None);

        // Assert
        await _inner.Received(1).GetCustomFieldSpecifications(Arg.Any<CancellationToken>());
        _cache.Received(1).Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey));
        _cache.Received(1).Set(Arg.Is(cacheKey), Arg.Is(innerResult.Value), Arg.Any<TimeSpan>());
    }


    [Fact]
    public async Task GetCustomFieldSpecifications_ShouldNotCache_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var cacheKey = "Deskpro_CustomFieldSpecifications";
        var innerResult = Result<IReadOnlyCollection<CustomFieldSpecificationDto>>.Error();
        _inner.GetCustomFieldSpecifications(Arg.Any<CancellationToken>()).Returns(innerResult);
        _cache.Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey)).ReturnsNull();

        // Act
        await _sut.GetCustomFieldSpecifications(CancellationToken.None);

        // Assert
        await _inner.Received(1).GetCustomFieldSpecifications(Arg.Any<CancellationToken>());
        _cache.Received(1).Get<IReadOnlyCollection<CustomFieldSpecificationDto>>(Arg.Is(cacheKey));
        _cache.Received(0).Set(Arg.Any<string>(), Arg.Any<Arg.AnyType>, Arg.Any<TimeSpan>());
    }
    
    [Fact]
    public async Task GetMessage_ShouldReturnCachedValue_WhenCacheIsHitAndValueIsNotNull()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var cacheKey = $"Deskpro_Message_{ticketId}_{messageId}";
        var cachedValue = new MessageDto();
        var expectedResult = Result.Success(cachedValue);
        _cache.Get<MessageDto>(Arg.Is(cacheKey)).Returns(cachedValue);

        // Act
        var result = await _sut.GetMessage(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _cache.Received(1).Get<MessageDto>(Arg.Is(cacheKey));
        _cache.Received(0).Set(Arg.Any<string>(), Arg.Any<Arg.AnyType>(), Arg.Any<TimeSpan>());
        await _inner.Received(0).GetMessageAttachments(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetMessage_ShouldCallInnerAndCacheResult_WhenCacheIsMiss()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var cacheKey = $"Deskpro_Message_{ticketId}_{messageId}";
        var message = new MessageDto();
        var innerResult = Result.Success(message);
        _inner.GetMessage(Arg.Is(ticketId), Arg.Is(messageId), Arg.Any<CancellationToken>()).Returns(innerResult);
        _cache.Get<MessageDto>(Arg.Is(cacheKey)).ReturnsNull();

        // Act
        await _sut.GetMessage(ticketId, messageId, CancellationToken.None);

        // Assert
        _cache.Received(1).Get<MessageDto>(Arg.Is(cacheKey));
        _cache.Received(1).Set(Arg.Is(cacheKey), Arg.Is(message), Arg.Any<TimeSpan>());
        await _inner.Received(1).GetMessage(Arg.Is(ticketId), Arg.Is(messageId), Arg.Any<CancellationToken>());
    }

    
    [Fact]
    public async Task GetMessage_ShouldNotCache_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var cacheKey = $"Deskpro_Message_{ticketId}_{messageId}";
        var innerResult = Result<MessageDto>.Error();
        _inner.GetMessage(Arg.Is(ticketId), Arg.Is(messageId), Arg.Any<CancellationToken>()).Returns(innerResult);
        _cache.Get<MessageDto>(Arg.Is(cacheKey)).ReturnsNull();

        // Act
        await _sut.GetMessage(ticketId, messageId, CancellationToken.None);

        // Assert
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<MessageDto>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task GetMessageAttachments_ShouldReturnCachedValule_WhenCacheIsHit()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var cacheKey = $"Deskpro_MessageAttachments_{ticketId}_{messageId}";
        var cachedValue = new Collection<AttachmentDto>();
        var expectedResult = Result<IReadOnlyCollection<AttachmentDto>>.Success(cachedValue);
        _cache.Get<IReadOnlyCollection<AttachmentDto>>(Arg.Is(cacheKey)).Returns(cachedValue);

        // Act
        var result = await _sut.GetMessageAttachments(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedResult);
        _cache.Received(1).Get<IReadOnlyCollection<AttachmentDto>>(Arg.Is(cacheKey));
        _cache.Received(0).Set(Arg.Any<string>(), Arg.Any<IReadOnlyCollection<AttachmentDto>>(), Arg.Any<TimeSpan>());    
    }

    [Fact]
    public async Task GetMessageAttachments_ShouldReturnInnerResultAndCacheResult_WhenCacheIsMiss() 
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var cacheKey = $"Deskpro_MessageAttachments_{ticketId}_{messageId}";
        _cache.Get<IReadOnlyCollection<AttachmentDto>>(Arg.Is(cacheKey)).ReturnsNull();
        var innerResult = Result<IReadOnlyCollection<AttachmentDto>>.Success(new Collection<AttachmentDto>());
        _inner.GetMessageAttachments(Arg.Is(ticketId), Arg.Is(messageId), Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetMessageAttachments(ticketId, messageId, CancellationToken.None);

        // Assert
        result.Should().Be(innerResult);
        await _inner.Received(1).GetMessageAttachments(Arg.Is(ticketId), Arg.Is(messageId), Arg.Any<CancellationToken>());
        _cache.Received(1).Get<IReadOnlyCollection<AttachmentDto>>(Arg.Is(cacheKey));
        _cache.Received(1).Set(Arg.Is(cacheKey), Arg.Is(innerResult.Value), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task GetMessageAttachments_ShouldNotCache_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var cacheKey = $"Deskpro_MessageAttachments_{ticketId}_{messageId}";
        _cache.Get<IReadOnlyCollection<AttachmentDto>>(Arg.Is(cacheKey)).ReturnsNull();
        var innerResult = Result<IReadOnlyCollection<AttachmentDto>>.Error();
        _inner.GetMessageAttachments(Arg.Is(ticketId), Arg.Is(messageId), Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        await _sut.GetMessageAttachments(ticketId, messageId, CancellationToken.None);

        // Assert
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<Arg.AnyType>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task GetPerson_ById_ShouldReturnCachedPerson_WhenCacheIsHit()
    {
        // Arrange
        var personId = 1;
        var cacheKey = $"Deskpro_Person_{personId}";
        var cachedValue = new PersonDto();
        var expectedValue = Result.Success(cachedValue);
        _cache.Get<PersonDto>(Arg.Is(cacheKey)).Returns(cachedValue);

        // Act
        var result = await _sut.GetPerson(personId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedValue);
        _cache.Received(1).Get<PersonDto>(Arg.Is(cacheKey));
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<Arg.AnyType>(), Arg.Any<TimeSpan>());
        await _inner.Received(0).GetPerson(Arg.Is(personId), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPerson_ById_ShouldReturnInnerResultAndCacheResult_WhenCacheIsMiss()
    {
        // Arrange
        var personId = 1;
        var cacheKey = $"Deskpro_Person_{personId}";
        var person = new PersonDto();
        var innerResult = Result.Success(person);
        _cache.Get<PersonDto>(Arg.Is(cacheKey)).ReturnsNull();
        _inner.GetPerson(Arg.Is(personId), Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetPerson(personId, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(innerResult);
        await _inner.Received(1).GetPerson(Arg.Is(personId), Arg.Any<CancellationToken>());
        _cache.Received(1).Get<PersonDto>(Arg.Is(cacheKey));
        _cache.Received(1).Set(Arg.Is(cacheKey), Arg.Is(innerResult.Value), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task GetPerson_ById_ShouldNotCache_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var personId = 1;
        var cacheKey = $"Deskpro_Person_{personId}";
        _cache.Get<PersonDto>(Arg.Is(cacheKey)).ReturnsNull();
        var innerResult = Result<PersonDto>.Error();
        _inner.GetPerson(Arg.Is(personId), Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        await _sut.GetPerson(personId, CancellationToken.None);

        // Assert
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<Arg.AnyType>(), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task GetPerson_ByEmail_ShouldReturnCachedPerson_WhenCacheIsHitAndDataIsNotNull()
    {
        // Arrange
        var personEmail = "email";
        var cacheKey = $"Deskpro_Person_{personEmail}";
        var cachedValue = new PersonDto();
        var expectedValue = Result.Success(cachedValue);
        _cache.Get<PersonDto>(Arg.Is(cacheKey)).Returns(cachedValue);

        // Act
        var result = await _sut.GetPerson(personEmail, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(expectedValue);
        _cache.Received(1).Get<PersonDto>(Arg.Is(cacheKey));
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<Arg.AnyType>(), Arg.Any<TimeSpan>());
        await _inner.Received(0).GetPerson(Arg.Is(personEmail), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetPerson_ByEmail_ShouldCallInnerAndCacheResult_WhenCacheIsMiss()
    {
        // Arrange
        var personEmail = "email";
        var cacheKey = $"Deskpro_Person_{personEmail}";
        var person = new PersonDto();
        var innerResult = Result.Success(person);
        _cache.Get<PersonDto>(Arg.Is(cacheKey)).ReturnsNull();
        _inner.GetPerson(Arg.Is(personEmail), Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        var result = await _sut.GetPerson(personEmail, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(innerResult);
        await _inner.Received(1).GetPerson(Arg.Is(personEmail), Arg.Any<CancellationToken>());
        _cache.Received(1).Get<PersonDto>(Arg.Is(cacheKey));
        _cache.Received(1).Set(Arg.Is(cacheKey), Arg.Is(innerResult.Value), Arg.Any<TimeSpan>());
    }

    [Fact]
    public async Task GetPerson_ByEmail_ShouldNotCache_WhenInnerResultIsNotSuccessful()
    {
        // Arrange
        var personEmail = "email";
        var cacheKey = $"Deskpro_Person_{personEmail}";
        _cache.Get<PersonDto>(Arg.Is(cacheKey)).ReturnsNull();
        var innerResult = Result<PersonDto>.Error();
        _inner.GetPerson(Arg.Is(personEmail), Arg.Any<CancellationToken>()).Returns(innerResult);

        // Act
        await _sut.GetPerson(personEmail, CancellationToken.None);

        // Assert
        _cache.Received(0).Set(Arg.Is(cacheKey), Arg.Any<Arg.AnyType>(), Arg.Any<TimeSpan>());
    }
}
