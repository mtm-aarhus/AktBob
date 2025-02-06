using Microsoft.Extensions.DependencyInjection;
using AktBob.Shared;
using AktBob.Shared.Contracts;
using AktBob.Podio.Contracts;
using AktBob.CheckOCRScreeningStatus.UseCases;

namespace AktBob.CheckOCRScreeningStatus.JobHandlers;
internal class CheckOCRScreeningStatusJobHandler : IJobHandler<CheckOCRScreeningStatusJob>
{
    private readonly ILogger<CheckOCRScreeningStatusJobHandler> _logger;
    private readonly IData _data;
    private readonly IConfiguration _configuration;


    public CheckOCRScreeningStatusJobHandler(ILogger<CheckOCRScreeningStatusJobHandler> logger, IData data, IConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _data = data;
        _configuration = configuration;
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    public async Task Handle(CheckOCRScreeningStatusJob job, CancellationToken cancellationToken = default)
    {
        using (var scope = ServiceProvider.CreateScope())
        {
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();


            // 1. Add case ID to cache
            _data.AddCase(job.FilArkivCaseId, job.PodioItemId);


            // 2. Register FilArkiv files in cache
            var registerFilesCommand = new RegisterFilesCommand(job.FilArkivCaseId);
            var registerFilesResult = await mediator.SendRequest(registerFilesCommand, cancellationToken);

            if (!registerFilesResult.IsSuccess)
            {
                await mediator.Send(new RemoveCaseFromCacheCommand(job.FilArkivCaseId));
                return;
            }


            // 3. Update database, set the FilArkivCaseId for this specific case
            var updateDatabaseCommand = new UpdateDatabaseCommand(job.FilArkivCaseId, job.PodioItemId);
            await mediator.Send(updateDatabaseCommand, cancellationToken);


            // 4. Check status for the FilArkiv files
            var files = _data.GetCase(job.FilArkivCaseId)?.Files;
            if (files == null || files.Count == 0)
            {
                _logger.LogWarning("No files registered for case {id}. Assuming this was intented.", job.FilArkivCaseId);
            }
            else
            {
                _logger.LogInformation("Start checking file statusses for case {id}", job.FilArkivCaseId);

                await Task.WhenAll(
                    _data.GetCase(job.FilArkivCaseId)!.Files.Select(
                        f => mediator.Send(new GetFileStatusQuery(f.FileId), cancellationToken)));

                _logger.LogInformation("Case {id}: OCRSceeningCompleted", job.FilArkivCaseId);
            }


            // 5. OCR screening finished -> update the Podio item
            var updatePodioItemCommand = new UpdatePodioItemCommand(job.FilArkivCaseId);
            await mediator.Send(updatePodioItemCommand);


            // 6. Post comment on the Podio item
            var podioAppId = _configuration.GetValue<int>("Podio:AppId");
            var commentText = "OCR screening af dokumenterne på FilArkiv er færdig.";

            var postCommentCommand = new PostItemCommentCommand(podioAppId, job.PodioItemId, commentText);
            var postCommentCommandResult = await mediator.SendRequest(postCommentCommand, cancellationToken);

            if (!postCommentCommandResult.IsSuccess)
            {
                _logger.LogWarning("Error posting comment on Podio item {id}", job.PodioItemId);
            }


            // 7. Remove data from cache
            var removeCaseFromCacheCommand = new RemoveCaseFromCacheCommand(job.FilArkivCaseId);
            await mediator.Send(removeCaseFromCacheCommand, cancellationToken);
        }
    }
}