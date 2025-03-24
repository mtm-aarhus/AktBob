using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Deskpro.Contracts.DTOs;
using AktBob.Deskpro.Handlers;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Deskpro.Tests.Unit.Handlers;
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
        var result = await _sut.Handle(1, 1, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expected);
        await _deskproClient.Received(2).GetMessageAttachments(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnErrorResultWithMessage_WhenDeskproClientThrowsHttpRequestException()
    {
        // Arrange
        _deskproClient.GetMessageAttachments(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ThrowsAsync<HttpRequestException>();

        // Act
        var act = () => _sut.Handle(1, 1, CancellationToken.None);
        var result = await act();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().NotBeEmpty();
        await _deskproClient.Received(1).GetMessageAttachments(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRethrowException_WhenDeskproClientThrowsAnyExceptionOtherThanHttpRequestException()
    {
        // Arrange
        _deskproClient.GetMessageAttachments(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Handle(1, 1, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        await _deskproClient.Received(1).GetMessageAttachments(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }
}
