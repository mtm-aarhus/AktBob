using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using AktBob.Shared.DataAccess;
using Dapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Data;
using System.Text.Json;

namespace AktBob.Database.Tests.Unit.Repositories;

public class CaseRepositoryTests
{
    private readonly CaseRepository _sut;
    private readonly ISqlDataAccess<IDatabaseSqlConnectionFactory> _dataAccess = Substitute.For<ISqlDataAccess<IDatabaseSqlConnectionFactory>>();

    public CaseRepositoryTests()
    {
        _sut = new CaseRepository(_dataAccess);
    }


    // Add

    [Fact]
    public async Task Add_ShouldSetCaseIdAndReturnTrue_WhenCaseIsAdded()
    {
        // Arrange
        var @case = new Case
        {
            TicketId = 1,
            PodioItemId = 1,
            CaseNumber = "case number"
        };

        var expectedCaseId = 123;

        _dataAccess
            .ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>())
            .Returns(1)
            .AndDoes(call =>
            {
                var passedParameters = call.Arg<DynamicParameters>();
                passedParameters.Add("Id", expectedCaseId, dbType: DbType.Int32, direction: ParameterDirection.Output);
            });

        // Act
        var result = await _sut.Add(@case);

        // Assert
        result.Should().BeTrue();
        await _dataAccess.Received(1).ExecuteProcedure("spCase_Create", Arg.Any<DynamicParameters>());
        @case.Id.Should().Be(expectedCaseId);
    }

    [Fact]
    public async Task Add_ShouldNotSetCaseIdAndReturnFalse_WhenCaseWasNotAdded()
    {
        // Arrange
        var @case = new Case
        {
            TicketId = 1,
            PodioItemId = 1,
            CaseNumber = "case number"
        };

        _dataAccess
            .ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>())
            .Returns(0)
            .AndDoes(call =>
            {
                var passedParameters = call.Arg<DynamicParameters>();
                passedParameters.Add("Id", null, dbType: DbType.Int32, direction: ParameterDirection.Output);
            });

        // Act
        var result = await _sut.Add(@case);

        // Assert
        result.Should().BeFalse();
        await _dataAccess.Received(1).ExecuteProcedure("spCase_Create", Arg.Any<DynamicParameters>());
        @case.Id.Should().Be(default);
    }

    [Fact]
    public async Task Add_ShouldThrowValidationException_WhenCaseIsInvalid()
    {
        // Arrange
        var @case = new Case
        {
            TicketId = default,
            PodioItemId = 1,
            CaseNumber = "case number"
        };

        // Act
        var act = () => _sut.Add(@case);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await _dataAccess.Received(0).ExecuteProcedure("spCase_Create", Arg.Any<DynamicParameters>());
    }


    // Get

    [Fact]
    public async Task Get_ShouldReturnCase_WhenIdMatchesExistingCase()
    {
        // Arrange
        var id = 1;
        var expectedCase = new Case
        {
            Id = id,
            CaseNumber = "case number",
            PodioItemId = 1,
            TicketId = 1
        };

        _dataAccess
            .QuerySingle<Case>(Arg.Any<string>(), Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(id)))
            .Returns(expectedCase);

        // Act
        var result = await _sut.Get(id);

        // Assert
        result.Should().Be(expectedCase);
        await _dataAccess.Received(1).QuerySingle<Case>(
            Arg.Any<string>(),
            Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(id)));

    }

    [Fact]
    public async Task Get_ShouldReturnNull_WhenIdDoesNotMatchesExistingCase()
    {
        // Arrange
        var id = 1;

        _dataAccess
            .QuerySingle<Case>(Arg.Any<string>(), Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(id)))
            .ReturnsNull();

        // Act
        var result = await _sut.Get(id);

        // Assert
        result.Should().BeNull();
        await _dataAccess.Received(1).QuerySingle<Case>(
            Arg.Any<string>(),
            Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(id)));
    }


    // GetAll

    [Fact]
    public async Task GetAll_ShouldReturnAllCases_WhenArgumentsAreNull()
    {
        // Arrange
        IReadOnlyCollection<Case> expected = new List<Case>
        {
            new Case(),
            new Case()
        };

        _dataAccess
            .Query<Case>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(expected);

        // Act
        var result = await _sut.GetAll(podioItemId: null, filArkivCaseId: null);

        // Assert
        result.Should().BeEquivalentTo(expected);
        result.Count().Should().Be(expected.Count());
        await _dataAccess.Received(1).Query<Case>(Arg.Any<string>(), Arg.Any<object>());
    }

    [Fact]
    public async Task GetAll_ShouldReturnCases_WhenPodioItemIdIsNotNull()
    {
        // Arrange
        IReadOnlyCollection<Case> expected = new List<Case>
        {
            new Case()           
        };

        _dataAccess
            .Query<Case>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(expected);

        // Act
        var result = await _sut.GetAll(podioItemId: 1, filArkivCaseId: null);

        // Assert
        result.Should().BeEquivalentTo(expected);
        result.Count().Should().Be(expected.Count());
        await _dataAccess.Received(1).Query<Case>(Arg.Any<string>(), Arg.Any<object>());
    }

    [Fact]
    public async Task GetAll_ShouldReturnCases_WhenFilArkivCaseIdIsNotNull()
    {
        // Arrange
        IReadOnlyCollection<Case> expected = new List<Case>
        {
            new Case()
        };

        _dataAccess
            .Query<Case>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(expected);

        // Act
        var result = await _sut.GetAll(podioItemId: null, filArkivCaseId: Guid.Empty);

        // Assert
        result.Should().BeEquivalentTo(expected);
        result.Count().Should().Be(expected.Count());
        await _dataAccess.Received(1).Query<Case>(Arg.Any<string>(), Arg.Any<object>());
    }

    [Fact]
    public async Task GetAll_ShouldReturnCases_WhenPodioItemIdAndFilArkivIdAreNotNull()
    {
        // Arrange
        IReadOnlyCollection<Case> expected = new List<Case>
        {
            new Case()
        };

        _dataAccess
            .Query<Case>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(expected);

        // Act
        var result = await _sut.GetAll(podioItemId: 1, filArkivCaseId: Guid.Empty);

        // Assert
        result.Should().BeEquivalentTo(expected);
        result.Count().Should().Be(expected.Count());
        await _dataAccess.Received(1).Query<Case>(Arg.Any<string>(), Arg.Any<object>());
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyEnumerable_WhenNoCasesAreFoundByPodioItemId()
    {
        // Arrange
        _dataAccess
            .Query<Case>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(new List<Case>());

        // Act
        var result = await _sut.GetAll(podioItemId: 1, filArkivCaseId: null);

        // Assert
        result.Should().BeEmpty();
        await _dataAccess.Received(1).Query<Case>(Arg.Any<string>(), Arg.Any<object>());
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyEnumerable_WhenNoCasesAreFoundByFilArkivCaseId()
    {
        // Arrange
        _dataAccess
            .Query<Case>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(new List<Case>());

        // Act
        var result = await _sut.GetAll(podioItemId: null, filArkivCaseId: Guid.Empty);

        // Assert
        result.Should().BeEmpty();
        await _dataAccess.Received(1).Query<Case>(Arg.Any<string>(), Arg.Any<object>());
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyEnumerable_WhenNoCasesAreFoundByPodioItemIdAndFilArkivCaseId()
    {
        // Arrange
        _dataAccess
            .Query<Case>(Arg.Any<string>(), Arg.Any<object>())
            .Returns(new List<Case>());

        // Act
        var result = await _sut.GetAll(podioItemId: 1, filArkivCaseId: Guid.Empty);

        // Assert
        result.Should().BeEmpty();
        await _dataAccess.Received(1).Query<Case>(Arg.Any<string>(), Arg.Any<object>());
    }


    // Update

    [Fact]
    public async Task Update_ShouldReturnTrue_WhenCaseIsUpdated()
    {
        // Arrange
        var @case = new Case
        {
            Id = 1,
            TicketId = 1,
            PodioItemId = 1,
            CaseNumber = "case number",
            FilArkivCaseId = Guid.Empty,
            SharepointFolderName = string.Empty
        };

        var expectedCase = JsonSerializer.Serialize(@case);
        var caseCopy = JsonSerializer.Deserialize<Case>(expectedCase);

        _dataAccess
            .Execute(Arg.Any<string>(), @case)
            .Returns(1);

        // Act
        var result = await _sut.Update(@case);

        // Assert
        result.Should().BeTrue();
        JsonSerializer.Serialize(@case).Should().Be(JsonSerializer.Serialize(caseCopy));
        await _dataAccess.Received(1).Execute(Arg.Any<string>(), @case);
    }

    [Fact]
    public async Task Update_ShouldReturnFalse_WhenCaseIsNotUpdated()
    {
        // Arrange
        var @case = new Case
        {
            Id = 1,
            TicketId = 1,
            PodioItemId = 1,
            CaseNumber = "case number",
            FilArkivCaseId = Guid.Empty,
            SharepointFolderName = string.Empty
        };

        var expectedCase = JsonSerializer.Serialize(@case);
        var caseCopy = JsonSerializer.Deserialize<Case>(expectedCase);

        _dataAccess
            .Execute(Arg.Any<string>(), @case)
            .Returns(0);

        // Act
        var result = await _sut.Update(@case);

        // Assert
        result.Should().BeFalse();
        await _dataAccess.Received(1).Execute(Arg.Any<string>(), @case);
        JsonSerializer.Serialize(@case).Should().Be(JsonSerializer.Serialize(caseCopy));

    }

    [Fact]
    public async Task Update_ShouldThrowInvalidException_WhenCaseIsInvalid()
    {
        // Arrange
        var @case = new Case
        {
            Id = 1,
            TicketId = 1,
            PodioItemId = 0, // <-- should fail validation
            CaseNumber = "case number",
            FilArkivCaseId = Guid.Empty,
            SharepointFolderName = string.Empty
        };

        var expectedCase = JsonSerializer.Serialize(@case);
        var caseCopy = JsonSerializer.Deserialize<Case>(expectedCase);

        // Act
        var act = () => _sut.Update(@case);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await _dataAccess.Received(0).Execute(Arg.Any<string>(), Arg.Any<object>());
        JsonSerializer.Serialize(@case).Should().Be(JsonSerializer.Serialize(caseCopy));

    }
}