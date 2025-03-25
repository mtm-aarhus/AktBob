using AktBob.Database.Dtos;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
using FluentAssertions;

namespace AktBob.Database.Tests.Unit.Extensions;

public class TicketExtensionsTests
{
    [Fact]
    public void ToDto_ShouldReturnTicketDto_WhenInvoked()
    {
        // Arrange
        var ticket = new Ticket
        {
            CaseNumber = "case number",
            Cases = new List<Case>
            {
                new Case
                {
                    FilArkivCaseId = Guid.NewGuid(),
                    CaseNumber = "case number",
                    Id = 123,
                    PodioItemId = 123,
                    SharepointFolderName = "sharepoint folder name",
                    TicketId = 123
                }
            },
            CaseUrl = "case url",
            DeskproId = 123,
            Id = 123,
            SharepointFolderName = "sharepoint folder name"
        };

        // Act
        var result = ticket.ToDto();

        // Assert
        result.Should().BeOfType(typeof(TicketDto));
        result.CaseNumber.Should().Be(ticket.CaseNumber);
        result.CaseUrl.Should().Be(ticket.CaseUrl);
        result.DeskproId.Should().Be(ticket.DeskproId);
        result.Id.Should().Be(ticket.Id);
        result.SharepointFolderName.Should().Be(ticket.SharepointFolderName);
        result.Cases.ToList().Should().BeOfType(typeof(List<CaseDto>));
        result.Cases.Count().Should().Be(1);
    }

    [Fact]
    public void ToDto_ShouldReturnTicketDtoEnumerable_WhenInvoked()
    {
        // Arrange
        var tickets = new List<Ticket>
        {
            new Ticket
            {
                CaseNumber = "case number 1",
                Cases = new List<Case>
                {
                    new Case
                    {
                        FilArkivCaseId = Guid.NewGuid(),
                        CaseNumber = "case number",
                        Id = 123,
                        PodioItemId = 123,
                        SharepointFolderName = "sharepoint folder name",
                        TicketId = 123
                    }
                },
                CaseUrl = "case url 1",
                DeskproId = 123,
                Id = 123,
                SharepointFolderName = "sharepoint folder name 1"
            },
            new Ticket
            {
                CaseNumber = "case number 2",
                Cases = new List<Case>
                {
                    new Case
                    {
                        FilArkivCaseId = Guid.NewGuid(),
                        CaseNumber = "case number",
                        Id = 987,
                        PodioItemId = 987,
                        SharepointFolderName = "sharepoint folder name",
                        TicketId = 987
                    }
                },
                CaseUrl = "case url 2",
                DeskproId = 987,
                Id = 987,
                SharepointFolderName = "sharepoint folder name 2"
            }
        };

        // Act
        var result = tickets.ToDto().ToList();

        // Assert
        result.Should().BeOfType(typeof(List<TicketDto>));
        result.Count().Should().Be(2);

    }
}
