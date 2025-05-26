using AAK.FilArkiv;
using AktBob.FilArkiv.Contracts.DTOs;
using ErrorOr;

namespace AktBob.FilArkiv.Handlers.GetDocumentsByCaseId;
internal class GetDocumentsByCaseIdHandler : IGetDocumentsByCaseIdHandler
{
    private readonly IFilArkiv _filArkiv;

    public GetDocumentsByCaseIdHandler(IFilArkiv filArkiv)
    {
        _filArkiv = filArkiv;
    }
    public async Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> Handle(Guid caseId, CancellationToken cancellationToken = default)
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