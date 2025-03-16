using AktBob.Database.DataAccess;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using Dapper;
using FluentAssertions;
using FluentValidation;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using System.Data;
using System.Text.Json;

namespace AktBob.Database.Tests.Unit;

public class MesssageRepositoryTests
{
    private readonly ISqlDataAccess _dataAccess = Substitute.For<ISqlDataAccess>();
    private readonly MessageRepository _sut;

    public MesssageRepositoryTests()
    {
        _sut = new MessageRepository(_dataAccess);
    }

    // Add

    [Fact]
    public async Task Add_ShouldSetMessageIdAndReturnTrue_WhenMessageIsAdded()
    {
        // Arrange
        var message = new Message
        {
            TicketId = 1,
            DeskproMessageId = 1
        };

        var expectedMessageId = 123;

        _dataAccess
            .ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>())
            .Returns(1)
            .AndDoes(call =>
            {
                var passedParameters = call.Arg<DynamicParameters>();
                passedParameters.Add("Id", expectedMessageId, dbType: DbType.Int32, direction: ParameterDirection.Output);
            });

        // Act
        var result = await _sut.Add(message);

        // Assert
        result.Should().BeTrue();
        await _dataAccess.Received(1).ExecuteProcedure("dbo.spMessage_Insert", Arg.Any<DynamicParameters>());
        message.Id.Should().Be(expectedMessageId);
    }

    [Fact]
    public async Task Add_ShouldNotSetMessageIdReturnFalse_WhenMessageIsNotAdded()
    {
        // Arrange
        var message = new Message
        {
            TicketId = 1,
            DeskproMessageId = 1
        };

        _dataAccess
            .ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>())
            .Returns(1)
            .AndDoes(call =>
            {
                var passedParameters = call.Arg<DynamicParameters>();
                passedParameters.Add("Id", null, dbType: DbType.Int32, direction: ParameterDirection.Output);
            });

        // Act
        var result = await _sut.Add(message);

        // Assert
        result.Should().BeTrue();
        await _dataAccess.Received(1).ExecuteProcedure("dbo.spMessage_Insert", Arg.Any<DynamicParameters>());
        message.Id.Should().Be(default);
    }

    [Fact]
    public async Task Add_ShouldThrowValidationException_WhenMessageIsInvalid()
    {
        // Arrange
        var message = new Message
        {
            TicketId = default,
            DeskproMessageId = default
        };

        // Act
        var act = () => _sut.Add(message);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await _dataAccess.Received(0).ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>());
    }


    // Delete

    [Fact]
    public async Task Delete_ShouldReturnTrue_WhenMessageIsUpdated()
    {
        // Arrange
        var id = 1;

        _dataAccess
            .Execute(Arg.Any<string>(), Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(id)))
            .Returns(1);

        // Act
        var result = await _sut.Delete(id);

        // Assert
        result.Should().BeTrue();
        await _dataAccess.Received(1).Execute(
            Arg.Any<string>(),
            Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(id)));
    }

    [Fact]
    public async Task Delete_ShouldReturnFalse_WhenNoMessagesAreUpdated()
    {
        // Arrange
        var id = 1;

        _dataAccess
            .Execute(Arg.Any<string>(), Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(id)))
            .Returns(0);

        // Act
        var result = await _sut.Delete(id);

        // Assert
        result.Should().BeFalse();
        await _dataAccess.Received(1).Execute(
            Arg.Any<string>(),
            Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(id)));
    }


    // Get
    [Fact]
    public async Task Get_ShouldReturnMessage_WhenMessageIsFoundById()
    {
        // Arrange
        var id = 1;

        var expectedMessage = new Message
        {
            Id = id,
            DeskproMessageId = 1,
            GODocumentId = 1,
            MessageNumber = 1,
            TicketId = 1
        };

        _dataAccess
            .QuerySingle<Message>(Arg.Any<string>(), Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(id)))
            .Returns(expectedMessage);

        // Act
        var result = await _sut.Get(id);

        // Assert
        result.Should().Be(expectedMessage);
        result.Id.Should().Be(id);
        await _dataAccess.Received(1).QuerySingle<Message>(
            Arg.Any<string>(),
            Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(id)));
    }

    [Fact]
    public async Task Get_ShouldReturnNull_WhenMessageIsNotFoundById()
    {
        // Arrange
        var id = 1;

        _dataAccess
            .QuerySingle<Message>(Arg.Any<string>(), Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(id)))
            .ReturnsNull();

        // Act
        var result = await _sut.Get(id);

        // Assert
        result.Should().BeNull();
        await _dataAccess.Received(1).QuerySingle<Message>(
            Arg.Any<string>(),
            Arg.Is<object>(arg => arg.GetType().GetProperty("Id")!.GetValue(arg)!.Equals(id)));
    }


    // GetByDeskproMessageId

    [Fact]
    public async Task GetByDeskproMessageId_ShouldReturnMessage_WhenMessageIsFoundByDeskproMessageId()
    {
        // Arrange
        var deskproMessageId = 1;

        var expectedMessage = new Message
        {
            Id = 1,
            DeskproMessageId = deskproMessageId,
            GODocumentId = 1,
            MessageNumber = 1,
            TicketId = 1
        };

        _dataAccess
            .QuerySingle<Message>(Arg.Any<string>(), Arg.Is<object>(arg => arg.GetType().GetProperty("DeskproMessageId")!.GetValue(arg)!.Equals(deskproMessageId)))
            .Returns(expectedMessage);

        // Act
        var result = await _sut.GetByDeskproMessageId(deskproMessageId);

        // Assert
        result.Should().Be(expectedMessage);
        result.DeskproMessageId.Should().Be(deskproMessageId);
        await _dataAccess.Received(1).QuerySingle<Message>(
            Arg.Any<string>(),
            Arg.Is<object>(arg => arg.GetType().GetProperty("DeskproMessageId")!.GetValue(arg)!.Equals(deskproMessageId)));
    }

    [Fact]
    public async Task GetByDeskproMessageId_ShouldReturnNull_WhenMessageIsNotFoundByDeskproMessageId()
    {
        // Arrange
        var deskproMessageId = 1;

        _dataAccess
            .QuerySingle<Message>(Arg.Any<string>(), Arg.Is<object>(arg => arg.GetType().GetProperty("DeskproMessageId")!.GetValue(arg)!.Equals(deskproMessageId)))
            .ReturnsNull();

        // Act
        var result = await _sut.GetByDeskproMessageId(deskproMessageId);

        // Assert
        result.Should().BeNull();
        await _dataAccess.Received(1).QuerySingle<Message>(
            Arg.Any<string>(),
            Arg.Is<object>(arg => arg.GetType().GetProperty("DeskproMessageId")!.GetValue(arg)!.Equals(deskproMessageId)));
    }


    // Update

    [Fact]
    public async Task Update_ShouldReturnTrue_WhenMessageIsUpdated()
    {
        // Arrange
        var message = new Message
        {
            Id = 1,
            DeskproMessageId = 1,
            GODocumentId = 1,
            MessageNumber = 1,
            TicketId = 1
        };

        _dataAccess
            .Execute(Arg.Any<string>(), message)
            .Returns(1);

        var expectedMessage = JsonSerializer.Serialize(message);
        var messageCopy = JsonSerializer.Deserialize<Message>(expectedMessage);

        // Act
        var result = await _sut.Update(message);

        // Assert
        result.Should().BeTrue();
        JsonSerializer.Serialize(message).Should().Be(JsonSerializer.Serialize(messageCopy));
        await _dataAccess.Received(1).Execute(Arg.Any<string>(), message);
    }

    [Fact]
    public async Task Update_ShouldReturnFalse_WhenMessageIsNotUpdated()
    {
        // Arrange
        var message = new Message
        {
            Id = 1,
            DeskproMessageId = 1,
            GODocumentId = 1,
            MessageNumber = 1,
            TicketId = 1
        };

        var expectedMessage = JsonSerializer.Serialize(message);
        var messageCopy = JsonSerializer.Deserialize<Message>(expectedMessage);

        _dataAccess
            .Execute(Arg.Any<string>(), message)
            .Returns(0);

        // Act
        var result = await _sut.Update(message);

        // Assert
        result.Should().BeFalse();
        JsonSerializer.Serialize(message).Should().Be(JsonSerializer.Serialize(messageCopy));
        await _dataAccess.Received(1).Execute(
            Arg.Any<string>(),
            message);
    }

    [Fact]
    public async Task Update_ShouldThrowValidationException_WhenMessageIsNotValid()
    {
        // Arrange
        var message = new Message
        {
            TicketId = default,
            DeskproMessageId = default
        };

        var expectedMessage = JsonSerializer.Serialize(message);
        var messageCopy = JsonSerializer.Deserialize<Message>(expectedMessage);

        // Act
        var act = () => _sut.Update(message);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        await _dataAccess.Received(0).ExecuteProcedure(Arg.Any<string>(), Arg.Any<DynamicParameters>());
        JsonSerializer.Serialize(message).Should().Be(JsonSerializer.Serialize(messageCopy));

    }
}
