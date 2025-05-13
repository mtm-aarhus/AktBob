using AAK.FilArkiv;
using AktBob.FilArkiv.Contracts;
using Ardalis.Result;

namespace AktBob.FilArkiv.Handlers;
internal class GetDocumentsHandler : IGetDocumentsHandler
{
    private readonly IFilArkiv _filArkiv;

    public GetDocumentsHandler(IFilArkiv filArkiv)
    {
        _filArkiv = filArkiv;
    }
    public async Task<Result<IReadOnlyCollection<DocumentDto>>> Handle(Guid caseId, CancellationToken cancellationToken = default)
    {
        var documents = await _filArkiv.GetCaseDocumentOverview(caseId, cancellationToken);
        return documents.Select(d => new DocumentDto
        {
            Id = d.Id,
            CaseId = caseId,
            DocumentDate = d.DocumentDate,
            DocumentNumber = d.DocumentNumber ?? 0,
            Title = d.Title,
            Files = d.Files.Select(f => new FileDto
            {
                Id = f.Id,
                DocumentId = f.DocumentId,
                FileName = f.FileName,
            }).ToArray()
        }).ToArray();
    }
}
