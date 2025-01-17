using AktBob.CloudConvert.Contracts;
using AktBob.CloudConvert.Models;
using Ardalis.Result;
using MediatR;

namespace AktBob.CloudConvert;
internal class ConvertHtmlToPdfCommandHandler(ICloudConvertClient cloudConvertClient) : IRequestHandler<ConvertHtmlToPdfCommand, Result<ConvertHtmlToPdfResponseDto>>
{
    private readonly ICloudConvertClient _cloudConvertClient = cloudConvertClient;

    public async Task<Result<ConvertHtmlToPdfResponseDto>> Handle(ConvertHtmlToPdfCommand command, CancellationToken cancellationToken)
    {
        var importTasks = new Dictionary<Guid, ImportTask>();
        var convertTasks = new Dictionary<Guid, ConvertTask>();
        var tasks = new Dictionary<Guid, object>();


        // Import tasks
        foreach (var item in command.base64HTMLDocuments)
        {
            var id = Guid.NewGuid();
            var task = new ImportTask
            {
                File = item,
                Filename = $"{id}.html"
            };

            importTasks.Add(id, task);
            tasks.Add(id, task);
        }


        // Convert tasks
        foreach (var item in importTasks)
        {
            var id = Guid.NewGuid();
            var task = new ConvertTask
            {
                Input = [item.Key.ToString()]
            };

            convertTasks.Add(id, task);
            tasks.Add(id, task);
        }


        // Merge tasks
        var mergeTaskId = Guid.NewGuid();
        var mergeTask = new MergeTask
        {
            Input = convertTasks.Keys.Select(x => x.ToString()).ToArray()
        };


        // Export tasks
        var exportTaskId = Guid.NewGuid();
        var exportTask = new ExportTask
        {
            Input = [convertTasks.First().Key.ToString()]
        };


        // If there is more than one convert tasks, we need to utilize
        // the merge task to combine all converted tasks
        if (convertTasks.Count() > 1)
        {
            tasks.Add(mergeTaskId, mergeTask);
            exportTask.Input = [mergeTaskId.ToString()];
        }


        tasks.Add(exportTaskId, exportTask);

        var payload = new Payload
        {
            Tasks = tasks
        };
        
        var result = await _cloudConvertClient.CreateJob(payload, cancellationToken);

        if (result.IsSuccess)
        {
            return new ConvertHtmlToPdfResponseDto(result.Value);
        }

        return Result.Error();

    }
}
