using AAK.FilArkiv;
using AktBob.Shared.Contracts.Modules.FilArkiv.Contracts.DTOs;
using ErrorOr;

namespace Aktbob.Modules.FilArkiv.Features.GetDocumentsByCaseId;
internal class GetDocumentsByCaseIdHandler(IFilArkiv filArkiv) : IGetDocumentsByCaseIdHandler
{
    public async Task<ErrorOr<IReadOnlyCollection<DocumentDto>>> Handle(Guid caseId, CancellationToken cancellationToken = default)
    {
        var documents = await filArkiv.GetCaseDocumentOverview(caseId, cancellationToken);
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