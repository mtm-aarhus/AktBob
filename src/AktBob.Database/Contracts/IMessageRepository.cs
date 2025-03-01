using AktBob.Database.Entities;

namespace AktBob.Database.Contracts;
public interface IMessageRepository
{
    Task<int> Add(Message entity);
    Task<int> Delete(int id);
    Task<Message?> GetByDeskproMessageId(int deskproMessageId);
    Task<Message?> GetById(int id);
    Task<int> Update(Message entity);
}
