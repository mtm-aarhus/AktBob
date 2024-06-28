using Ardalis.Result;
using FilArkivCore.Web.Shared.Documents;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktBob.CheckOCRScreeningStatus.UseCases.RegisterDocuments;
internal class RegisterFilesCommandHandler : IRequestHandler<RegisterFilesCommand, Result>
{
    private readonly IData _data;
    private readonly IConfiguration _configuration;
    private readonly IFilArkiv _filArkiv;
    private readonly ILogger<RegisterFilesCommandHandler> _logger;

    public RegisterFilesCommandHandler(IData data, IConfiguration configuration, IFilArkiv filArkiv, ILogger<RegisterFilesCommandHandler> logger)
    {
        _data = data;
        _configuration = configuration;
        _filArkiv = filArkiv;
        _logger = logger;
    }

    public async Task<Result> Handle(RegisterFilesCommand request, CancellationToken cancellationToken)
    {
        var delayBetweenChecks = TimeSpan.FromMilliseconds(_configuration.GetValue<int?>("CheckOCRScreening:DelayBetweenChecksSMilliSeconds") ?? 10000);
        var files = new List<File>();

        var @case = _data.GetCase(request.CaseId);

        if (@case == null)
        {
            return Result.Error(new ErrorList([$"Unknown case Id {request.CaseId}"], string.Empty));
        }

        // Get data from FilArkiv
        bool moveToNextPage = true;
        int pageIndex = 0;

        while(moveToNextPage)
        {
            files.Clear();

            pageIndex++; // First page = pageIndex = 1

            var parameters = new DocumentParameters // PageSize: we just let FilArkiv decide
            {
                CaseId = request.CaseId.ToString(),
                Expand = "files",
                SkipTotalCount = true,
                PageIndex = pageIndex,
                PageSize = 1
            };

            var documents = await _filArkiv.FilArkivCoreClient.GetDocumentsAsync(parameters);

            if (!documents.Any() || documents.Count() < parameters.PageSize)
            {
                moveToNextPage = false;
            }


            // Add files to cached case object
            foreach (var document in documents)
            {
                foreach (var documentFile in document.Files)
                {
                    var file = new File(documentFile.Id, delayBetweenChecks);
                    files.Add(file);

                    _logger.LogInformation($"Case {request.CaseId} File {documentFile.Id} registered. Size: {documentFile.FileSize}, Checksum: {documentFile.FileDataChecksum}, FileName: '{documentFile.FileName}'");
                }
            }

            _data.AddFilesToCase(@case, files);
        }
       
        if (@case.Files.Any())
        {
            return Result.SuccessWithMessage($"Case {request.CaseId}: All files has been registered. {@case.Files.Count()} files total.");
        }


        return Result.Error(new ErrorList([$"No files found for Case Id {request.CaseId}"], string.Empty));
    }
}
