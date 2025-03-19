using AktBob.Database.Entities;

namespace AktBob.Database.Contracts;
public interface ICaseRepository
{
    Task<bool> Add(Case @case);
    Task<Case?> Get(int id);
    Task<IReadOnlyCollection<Case>> GetAll(long? podioItemId, Guid? filArkivCaseId);
    Task<bool> Update(Case @case);
}