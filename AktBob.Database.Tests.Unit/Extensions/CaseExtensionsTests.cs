using AktBob.Database.Dtos;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
using FluentAssertions;

namespace AktBob.Database.Tests.Unit.Extensions;

public class CaseExtensionsTests
{
    [Fact]
    public void ToDto_ShouldReturnCaseDto_WhenInvoked()
    {
        // Arrange
        var @case = new Case
        {
            FilArkivCaseId = Guid.NewGuid(),
            CaseNumber = "case number",
            Id = 123,
            PodioItemId = 123,
            SharepointFolderName = "sharepoint folder name",
            TicketId = 123
        };

        // Act
        var result = @case.ToDto();

        // Assert
        result.Should().BeOfType(typeof(CaseDto));
        result.FilArkivCaseId.Should().Be(@case.FilArkivCaseId);
        result.CaseNumber.Should().Be(@case.CaseNumber);
        result.Id.Should().Be(@case.Id);
        result.PodioItemId.Should().Be(@case.PodioItemId);
        result.SharepointFolderName.Should().Be(@case.SharepointFolderName);
        result.TicketId.Should().Be(@case.TicketId);
    }

    [Fact]
    public void ToDto_ShouldReturnCaseDtoEnumerable_WhenInvoked()
    {
        // Arrange
        var cases = new List<Case>
        {
            new Case
            {
                FilArkivCaseId = Guid.NewGuid(),
                CaseNumber = "case number 1",
                Id = 123,
                PodioItemId = 123,
                SharepointFolderName = "sharepoint folder name 1",
                TicketId = 123
            },
            new Case
            {
                FilArkivCaseId = Guid.NewGuid(),
                CaseNumber = "case number 2",
                Id = 987,
                PodioItemId = 987,
                SharepointFolderName = "sharepoint folder name 2",
                TicketId = 987
            }
        };

        // Act
        var result = cases.ToDto().ToList();

        // Assert
        result.Count().Should().Be(cases.Count());
        result.Should().BeOfType(typeof(List<CaseDto>));
        
        foreach (var resultCase in result)
        {
            var originalCase = cases.First(c => c.Id == resultCase.Id);

            resultCase.FilArkivCaseId.Should().Be(originalCase.FilArkivCaseId);
            resultCase.CaseNumber.Should().Be(originalCase.CaseNumber);
            resultCase.Id.Should().Be(originalCase.Id);
            resultCase.PodioItemId.Should().Be(originalCase.PodioItemId);
            resultCase.SharepointFolderName.Should().Be(originalCase.SharepointFolderName);
            resultCase.TicketId.Should().Be(originalCase.TicketId);
        }
    }
}
