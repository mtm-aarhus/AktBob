using AktBob.CloudConvert.Handlers;
using AktBob.CloudConvert.Models;
using FluentAssertions;

namespace AktBob.CloudConvert.Tests.Unit.Handlers;

public class GenerateTasksHandlerTests
{
    private readonly GenerateTasksHandler _sut;

    public GenerateTasksHandlerTests()
    {
        _sut = new GenerateTasksHandler();
    }

    [Fact]
    public void Handle_ShouldReturnDictionaryOfTasks_WhenItemsEnumerableIsNotEmpty()
    {
        // Arrange
        IEnumerable<byte[]> items = [[0x20]];

        // Act
        var result = _sut.Handle(items);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeOfType<Dictionary<Guid, object>>();
    }

    [Fact]
    public void Handle_ShouldReturnDictionaryContainingAMergeTask_WhenItemsEnumerableHaveMoreThanOneItem()
    {
        // Arrange
        IEnumerable<byte[]> items = [[0x20], [0x20]];

        // Act
        var result = _sut.Handle(items);

        // Assert
        result.Value.Values.Where(x => x.GetType() == typeof(MergeTask)).Should().HaveCount(1);
    }

    [Fact]
    public void Handle_ShouldReturnDictionaryContaningCorrectNumberAndTypesOfTasks_WhenItemsEnumerableIsNotEmpty()
    {
        // Arrange
        IEnumerable<byte[]> items = [[0x20], [0x20]];

        // Act
        var result = _sut.Handle(items);

        // Assert
        result.Value.Values.Where(x => x.GetType() == typeof(ImportTask)).Should().HaveCount(items.Count());
        result.Value.Values.Where(x => x.GetType() == typeof(ConvertTask)).Should().HaveCount(items.Count());
        result.Value.Values.Where(x => x.GetType() == typeof(ExportTask)).Should().HaveCount(1);
    }

    [Fact]
    public void Handle_ShouldReturnError_WhenItemsEnumerableIsEmpty()
    {
        // Arrange
        IEnumerable<byte[]> items = Enumerable.Empty<byte[]>();

        // Act
        var result = _sut.Handle(items);

        // Assert
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Error);
    }
}
