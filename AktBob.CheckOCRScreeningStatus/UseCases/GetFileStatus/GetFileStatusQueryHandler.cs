using Ardalis.Result;
using FilArkivCore.Web.Shared.FileProcess;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AktBob.CheckOCRScreeningStatus.UseCases.GetFileStatus;
internal class GetFileStatusQueryHandler : IRequestHandler<GetFileStatusQuery, Result>
{
    private readonly IData _data;
    private readonly IConfiguration _configuration;
    private readonly IFilArkiv _filArkiv;
    private readonly ILogger<GetFileStatusQueryHandler> _logger;

    public GetFileStatusQueryHandler(IData data, IConfiguration configuration, IFilArkiv filArkiv, ILogger<GetFileStatusQueryHandler> logger)
    {
        _data = data;
        _configuration = configuration;
        _filArkiv = filArkiv;
        _logger = logger;
    }


    public async Task<Result> Handle(GetFileStatusQuery request, CancellationToken cancellationToken)
    {
        var random = new Random();

        var file = _data.GetFile(request.FileId);

        if (file == null)
        {
            return Result.Error(new ErrorList([$"File {request.FileId} not found in cache"], string.Empty));
        }

        var counter = 0;

        var stopWatch = new Stopwatch();
        stopWatch.Start();

        while (!file.HasBeenScreened)
        {
            // Wait before each check to reduce throttling
            var randomDelayTime = TimeSpan.FromMilliseconds(random.Next(0, _configuration.GetValue<int?>("CheckOCRScreening:MaxRandomDelayTimeMilliseconds") ?? 3000));
            var delay = file.DelayBetweenChecks + randomDelayTime;
            
            _logger.LogInformation($"File {request.FileId} Waiting {delay.TotalMilliseconds} ms before next check");

            await Task.Delay(delay, cancellationToken);

            counter++;

            // Get file status from FilArkiv
            var parameters = new FileProcessStatusFileParameters
            {
                FileId = request.FileId
            };

            var fileProcessStatus = await _filArkiv.FilArkivCoreClient.GetFileProcessStatusFileAsync(parameters);

            _logger.LogInformation($"File {request.FileId} IsBeingProcessed: {fileProcessStatus.IsBeingProcessed} ('{fileProcessStatus.FileName}')");

            if (!fileProcessStatus.IsBeingProcessed)
            {
                // File has been screeened
                _data.FileHasBeenScreened(file);         
            }
        }

        stopWatch.Stop();
        
        return Result.SuccessWithMessage($"File {request.FileId} has been screened. Elapsed time: {stopWatch.ElapsedMilliseconds} milliseconds. Checked {counter} time{(counter > 1 ? "s" : string.Empty)}.");
    }
}
