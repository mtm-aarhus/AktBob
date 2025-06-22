using System.Collections.ObjectModel;
using Aktbob.Processors.OS2FormsSubmissions.OS2FormsModule.Contracts;
using AktBob.Shared;
using AktBob.Shared.Exceptions;
using AktBob.Shared.Extensions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Ardalis.GuardClauses;

namespace Aktbob.Processors.OS2FormsSubmissions;

public class EnsureSubmissions(
    ILogger<EnsureSubmissions> logger,
    IConfiguration configuration,
    IOS2FormsModule os2Forms,
    IMessageBus messageBus)
{
    [Function("ensure-submissions")]
    public async Task Run([TimerTrigger("%EnsureSubmission:TriggerTime%")] TimerInfo timerInfo, CancellationToken cancellationToken)
    {
        var webformId = Guard.Against.NullOrEmpty(configuration.GetValue<string>("WebformId"));
        var ensureSubmissionsQueueName = Guard.Against.NullOrEmpty(configuration.GetValue<string>("EnsureSubmission:ServiceBusQueueName"));
        
        // Get submission ids from OS2Forms
        var submissionIds = await os2Forms.GetSubmissions(webformId, cancellationToken);
        if (submissionIds.IsError) throw new BusinessException($"Error getting submissions from OS2Forms: {submissionIds.Errors.ToCommaDelimitedString()}");
        
        if (submissionIds.Value.Count == 0) return;
        
        // Enqueue jobs for every ID
        var jobs = new Collection<EnsureSubmissionJob>();
        jobs.AddRange(submissionIds.Value.Select(x => new EnsureSubmissionJob(x, 1)));
        await messageBus.SendMessages(ensureSubmissionsQueueName, jobs.ToArray<object>(), cancellationToken);
    }
}