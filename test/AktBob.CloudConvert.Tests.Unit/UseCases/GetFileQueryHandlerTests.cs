using AktBob.CloudConvert.Contracts.DTOs;
using AktBob.CloudConvert.Handlers;
using Ardalis.Result;
using FluentAssertions;
using NSubstitute;

namespace AktBob.CloudConvert.Tests.Unit.UseCases;

public class GetFileQueryHandlerTests
{
    private readonly ICloudConvertClient _cloudConvertClient = Substitute.For<ICloudConvertClient>();
    private readonly GetCloudConvertFileHandler _sut;
    public GetFileQueryHandlerTests()
    {
        _sut = new GetCloudConvertFileHandler(_cloudConvertClient);
    }

    [Fact]
    public async Task Handle_ShouldReturnFileDto_WhenQueryIsValid()
    {
        // Arrange
        var filename = "filename.txt";
        var file = new Models.File
        {
            Stream = new MemoryStream(),
            Filename = filename
        };
        _cloudConvertClient.GetFile(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Result.Success(file));
        var expectedDto = new FileDto(file.Stream, file.Filename);

        // Act
        var result = await _sut.Handle("http://localhost", CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        result.Value.Filename.Should().Be(filename);
        result.Value.Stream.Should().BeSameAs(file.Stream);
    }


    [Fact]
    public async Task Handle_ShouldReturnError_WhenResultIsNotSuccessful()
    {
        // Arrange
        _cloudConvertClient.GetFile(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(Result.Error());

        // Act
        var result = await _sut.Handle("http://localhost", CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

}
