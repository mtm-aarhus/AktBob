using AktBob.Database.Contracts;
using AktBob.Database.Decorators;
using AktBob.Database.Entities;
using AktBob.Database.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AktBob.Database.Tests.Unit.Decorators;

public class CaseRepositoryExceptionDecoratorTests
{
    private readonly CaseRepositoryExceptionDecorator _sut;
    private readonly ICaseRepository _inner = Substitute.For<ICaseRepository>();
    private readonly FakeLogger<CaseRepository> _logger = new FakeLogger<CaseRepository>();

    public CaseRepositoryExceptionDecoratorTests()
    {
        _sut = new CaseRepositoryExceptionDecorator(_inner, _logger);
    }

    [Fact]
    public async Task Add_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var @case = new Case();
        _inner
            .Add(Arg.Any<Case>())
            .Returns(true);

        // Act
        var result = await _sut.Add(@case);

        // Assert
        result.Should().Be(true);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Add_ShouldLogErrorAndRethrowException_WhenInnerModuleFails()
    {
        // Arrange
        var @case = new Case();
        _inner
            .Add(Arg.Any<Case>())
            .ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Add(@case);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task Get_ShouldReturnResult_WhenInnerModuleSuceeds()
    {
        // Arrange
        var caseId = 1;
        var expectedCase = new Case
        {
            Id = caseId
        };

        _inner
            .Get(caseId)
            .Returns(expectedCase);

        // Act
        var result = await _sut.Get(caseId);

        // Assert
        result.Should().Be(expectedCase);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Get_ShouldLogErrorAndRethrowException_WhenInnerModuleFails()
    {
        // Arrange
        var caseId = 1;
       
        _inner
            .Get(caseId)
            .ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Get(caseId);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task GetAll_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var expectedCases = new List<Case>
        {
            new Case { Id = 1 },
            new Case { Id = 2 }
        };

        _inner
            .GetAll(Arg.Any<long?>(), Arg.Any<Guid?>())
            .Returns(expectedCases);

        // Act
        var result = await _inner.GetAll(null, null);

        // Assert
        result.Should().BeEquivalentTo(expectedCases);
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task GetAll_ShouldLogErrorAndRethrowException_WhenInnerModuleFails()
    {
        // Arrange
        _inner
            .GetAll(Arg.Any<long?>(), Arg.Any<Guid?>())
            .ThrowsAsync<Exception>();

        // Act
        var act = () =>_sut.GetAll(null, null);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }

    [Fact]
    public async Task Update_ShouldReturnResult_WhenInnerModuleSucceeds()
    {
        // Arrange
        var @case = new Case();

        _inner
            .Update(Arg.Any<Case>())
            .Returns(true);

        // Act
        var result = await _sut.Update(@case);

        // Assert
        result.Should().BeTrue();
        _logger.Collector.Count.Should().Be(0);
    }

    [Fact]
    public async Task Update_ShouldLogErrorAndRethrowException_WhenInnerModuleFails()
    {
        // Arrange
        var @case = new Case();

        _inner
            .Update(Arg.Any<Case>())
            .ThrowsAsync<Exception>();

        // Act
        var act = () => _sut.Update(@case);

        // Assert
        await act.Should().ThrowAsync<Exception>();
        _logger.Collector.LatestRecord.Level.Should().Be(LogLevel.Error);
    }
}
