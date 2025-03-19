using AktBob.Database.Dtos;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
using FluentAssertions;

namespace AktBob.Database.Tests.Unit.Extensions;

public class MessageExtensionsTests
{
    [Fact]
    public void ToDto_ShouldReturnDto_WhenInvoked()
    {
        // Arrange
        var message = new Message
        {
            DeskproMessageId = 123,
            GODocumentId = 123,
            Id = 123,
            MessageNumber = 123,
            TicketId = 123
        };

        // Act
        var result = message.ToDto();

        // Assert
        result.Should().BeOfType(typeof(MessageDto));
        result.DeskproMessageId.Should().Be(message.DeskproMessageId);
        result.GODocumentId.Should().Be(message.GODocumentId);
        result.Id.Should().Be(message.Id);
        result.MessageNumber.Should().Be(message.MessageNumber);
        result.TicketId.Should().Be(message.TicketId);
    }
}
