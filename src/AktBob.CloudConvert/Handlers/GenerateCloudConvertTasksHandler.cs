using AktBob.CloudConvert.Models;

namespace AktBob.CloudConvert.Handlers;
internal class GenerateCloudConvertTasksHandler(ILogger<GenerateCloudConvertTasksHandler> logger) : IGenerateCloudConvertTasksHandler
{
    private readonly ILogger<GenerateCloudConvertTasksHandler> _logger = logger;

    public Result<Dictionary<Guid, object>> Handle(IEnumerable<byte[]> items)
    {
        var importTasks = new Dictionary<Guid, ImportTask>();
        var convertTasks = new Dictionary<Guid, ConvertTask>();
        var tasks = new Dictionary<Guid, object>();

        if (!items.Any())
        {
            _logger.LogError("No items was provided. Cannot generate CloudConvert tasks");
            return Result.Error();
        }

        // Import tasks
        foreach (var item in items)
        {
            var id = Guid.NewGuid();
            var task = new ImportTask
            {
                File = Convert.ToBase64String(item),
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
        return tasks;
    }
}
