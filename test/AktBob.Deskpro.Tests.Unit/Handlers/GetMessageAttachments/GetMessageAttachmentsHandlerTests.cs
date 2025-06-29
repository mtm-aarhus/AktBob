using AAK.Deskpro;
using AAK.Deskpro.Models;
using Aktbob.Modules.Deskpro.Features.GetMessageAttachments;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers.GetMessageAttachments;
public class GetMessageAttachmentsHandlerTests
{
    private readonly GetMessageAttachmentsHandler _sut;
    private readonly IDeskproClient _deskproClient = Substitute.For<IDeskproClient>();

    public GetMessageAttachmentsHandlerTests()
    {
        _sut = new GetMessageAttachmentsHandler(_deskproClient);
    }

    [Fact]
    public async Task Handle_ShouldReturnAttachmentDtoCollection_WhenDeskproClientReturnsCollectionSuccessfully()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        var currentPage = 1;
        var expected = new List<AttachmentDto> { new AttachmentDto(), new AttachmentDto() };
        var attachments = new MessageAttachments
        {
            Attachments =
            [
                new MessageAttachment()
            ],
            Pagination = new Pagination
            {
                Count = 1,
                CurrentPage = currentPage++,
                PerPage = 1,
                Total = 2,
                TotalPages = 2
            }
        };
        _deskproClient.GetMessageAttachments(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(
                attachments,
                attachments);

        // Act
        var result = await _sut.Handle(ticketId, messageId, CancellationToken.None);

        // Assert
        result.IsError.Should().BeFalse();
        result.Value.Should().BeEquivalentTo(expected);
        await _deskproClient.Received(2).GetMessageAttachments(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRethrowException_WhenDeskproClientThrowsAnyException()
    {
        // Arrange
        var ticketId = 1;
        var messageId = 1;
        _deskproClient.GetMessageAttachments(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Handle(ticketId, messageId, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _deskproClient.Received(1).GetMessageAttachments(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
