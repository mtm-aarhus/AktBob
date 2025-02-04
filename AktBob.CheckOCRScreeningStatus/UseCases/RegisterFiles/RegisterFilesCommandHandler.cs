using Ardalis.Result;
using FilArkivCore.Web.Shared.Documents;
using MassTransit.Mediator;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.UseCases.RegisterFiles;
public class RegisterFilesCommandHandler : MediatorRequestHandler<RegisterFilesCommand, Result>
{
    private readonly IData _data;
    private readonly IFilArkiv _filArkiv;
    private readonly ILogger<RegisterFilesCommandHandler> _logger;

    public RegisterFilesCommandHandler(IData data, IFilArkiv filArkiv, ILogger<RegisterFilesCommandHandler> logger)
    {
        _data = data;
        _filArkiv = filArkiv;
        _logger = logger;
    }

    protected override async Task<Result> Handle(RegisterFilesCommand request, CancellationToken cancellationToken)
    {
        var filesList = new List<File>();

        var @case = _data.GetCase(request.CaseId);

        if (@case == null)
        {
            return Result.Error(new ErrorList([$"Unknown case Id {request.CaseId}"], string.Empty));
        }

        // Get data from FilArkiv
        bool moveToNextPage = true;
        int pageIndex = 0;

        while (moveToNextPage)
        {
            filesList.Clear();

            pageIndex++; // First page = pageIndex = 1

            var documentOverviewParameters = new DocumentOverviewParameters
            {
                CaseId = request.CaseId.ToString(),
                PageIndex = pageIndex,
                PageSize = 100
            };

            var documentOverview = await _filArkiv.FilArkivCoreClient.GetCaseDocumentOverviewListAsync(documentOverviewParameters);

            if (documentOverview == null)
            {
                _logger.LogWarning("FilArkiv: case {id} not found", request.CaseId);
                return Result.Error();
            }

            if (!documentOverview.HasNextPage)
            {
                moveToNextPage = false;
            }


            // Add files to cached case object
            foreach (var document in documentOverview.Items)
            {
                foreach (var documentFile in document.Files)
                {
                    var file = new File(documentFile.Id);
                    filesList.Add(file);

                    _logger.LogInformation($"Case {request.CaseId} File {documentFile.Id} registered. Size: {documentFile.FileSize}, FileName: '{documentFile.FileName}'");
                }
            }

            _data.AddFilesToCase(@case, filesList);
        }

        if (@case.Files.Any())
        {
            _logger.LogInformation("Case {caseId}: All files has been registered. {count} files total.", request.CaseId, @case.Files.Count());
            return Result.Success();
        }

        _logger.LogWarning("No files found for Case Id {caseId}", request.CaseId);
        return Result.Success();
    }
}