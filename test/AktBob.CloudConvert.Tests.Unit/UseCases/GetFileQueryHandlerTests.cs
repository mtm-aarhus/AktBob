using AktBob.CloudConvert.Contracts;
using AktBob.CloudConvert.Contracts.DTOs;
using AktBob.CloudConvert.UseCases;
using Ardalis.Result;
using FluentAssertions;
using NSubstitute;

namespace AktBob.CloudConvert.Tests.Unit.UseCases;

public class TestableGetFileQueryHandler : GetFileQueryHandler
{
    public TestableGetFileQueryHandler(ICloudConvertClient cloudConvertClient) : base(cloudConvertClient)
    {
    }

    public new Task<Result<FileDto>> Handle(GetFileQuery query, CancellationToken cancellationToken)
    {
        return base.Handle(query, cancellationToken);
    }
}

public class GetFileQueryHandlerTests
{
    private readonly ICloudConvertClient _cloudConvertClient = Substitute.For<ICloudConvertClient>();
    private readonly TestableGetFileQueryHandler _sut;
    public GetFileQueryHandlerTests()
    {
        _sut = new TestableGetFileQueryHandler(_cloudConvertClient);
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
        var query = new GetFileQuery(string.Empty);
        var expectedDto = new FileDto(file.Stream, file.Filename);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);

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
        var query = new GetFileQuery(string.Empty);

        // Act
        var result = await _sut.Handle(query, CancellationToken.None);
        
        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(ResultStatus.Error);
    }

}
