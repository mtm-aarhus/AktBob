using FilArkivCore.Web.Shared.FileProcess;
using MassTransit.Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace AktBob.CheckOCRScreeningStatus.UseCases.GetFileStatus;
public class GetFileStatusQueryHandler : MediatorRequestHandler<GetFileStatusQuery>
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


    protected override async Task Handle(GetFileStatusQuery query, CancellationToken cancellationToken)
    {
        var random = new Random();
        var delayTimeOffset = TimeSpan.FromMilliseconds(_configuration.GetValue<int?>("CheckOCRScreening:DelayBetweenChecksSMilliSeconds") ?? 10000);

        var file = _data.GetFile(query.FileId);

        if (file == null)
        {
            _logger.LogError("File {id} not found in cache", query.FileId);
            return;
        }

        var counter = 0;
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        while (!file.HasBeenScreened)
        {
            // Wait before each check to reduce throttling
            var randomDelayTime = TimeSpan.FromMilliseconds(random.Next(0, _configuration.GetValue<int?>("CheckOCRScreening:MaxRandomDelayTimeMilliseconds") ?? 3000));
            var delay = delayTimeOffset + randomDelayTime;
            
            _logger.LogInformation($"File {query.FileId} Waiting {delay.TotalMilliseconds} ms before next check");

            await Task.Delay(delay, cancellationToken);

            counter++;

            // Get file status from FilArkiv
            var parameters = new FileProcessStatusFileParameters
            {
                FileId = query.FileId
            };

            var response = await _filArkiv.FilArkivCoreClient.GetFileProcessStatusFileAsync(parameters);

            _logger.LogInformation($"File {query.FileId} IsBeingProcessed: {response.IsBeingProcessed} ('{response.FileName}')");

            if (!response.IsBeingProcessed && !response.FileProcessStatusResponses.Any(x => x.FinishedAt == null))
            {
                _data.FileHasBeenScreened(file);         
            }
        }

        stopWatch.Stop();

        _logger.LogInformation("File {fileId} has been screened. Elapsed time: {milliseconds} milliseconds. Checked {counter} time(s)", query.FileId, stopWatch.ElapsedMilliseconds, counter);
        return;
    }
}
