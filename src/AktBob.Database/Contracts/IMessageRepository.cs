using AktBob.Database.Entities;

namespace AktBob.Database.Contracts;
public interface IMessageRepository
{
    Task<bool> Add(Message message);
    Task<bool> Delete(int id);
    Task<Message?> GetByDeskproMessageId(int deskproMessageId);
    Task<Message?> Get(int id);
    Task<bool> Update(Message message);
}
