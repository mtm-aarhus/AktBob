using AktBob.Database.Contracts;
using AktBob.Shared;
using AktBob.Shared.Contracts.Processors;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;

namespace AktBob.Api.Endpoints.Jobs;

internal static class CheckOcrScreeningStatus
{
    public static void MapCheckOcrScreeningEndpoint(this RouteGroupBuilder builder, string route) => builder
        .MapPost(route, EndpointHandler)
        .WithSummary("Check OCR screening")
        .WithDescription("Igangsætter baggrundsjob som checker OCR screening for den angivne FilArkiv case")
        .Produces(StatusCodes.Status204NoContent);
    
    private record CheckOcrScreeningRequest(Guid FilArkivCaseId, long PodioItemId);
    
    private static async Task<IResult> EndpointHandler(
        [FromBody]  CheckOcrScreeningRequest request,
        [FromServices] IConfiguration configuration,
        [FromServices] IMessageBus messageBus,
        [FromServices] ICaseRepository repository,
        CancellationToken cancellationToken)
    {
        var appId = Guard.Against.Null(configuration.GetValue<int?>("Podio:AktindsigtApp:Id"));
        var queueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("Queues:CheckOcrScreeningStatus"));
        var job = new OcrScreeningStatusRegisterFilesJob(request.FilArkivCaseId, request.PodioItemId);
        
        await messageBus.SendMessage(queueName, job, cancellationToken);
        await UpdateDatabaseSetFilArkivCaseId(request.PodioItemId, request.FilArkivCaseId, repository);

        return Results.NoContent();
    }
    
    private static async Task UpdateDatabaseSetFilArkivCaseId(long podioItemId, Guid filArkivCaseId, ICaseRepository caseRepository)
    {
        var cases = await caseRepository.GetAll(podioItemId, null);
        if (cases.FirstOrDefault() is null) return; // Case not found

        var @case = cases.First();
        @case.FilArkivCaseId = filArkivCaseId;

        await caseRepository.Update(@case);
    }
}