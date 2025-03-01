using AktBob.Database.Contracts;
using AktBob.Database.Entities;
using System.Data;

namespace AktBob.Database.Repositories;
internal class MessageRepository : IMessageRepository
{
    private readonly ISqlDataAccess _sqlDataAccess;

    public MessageRepository(ISqlDataAccess sqlDataAccess)
    {
        _sqlDataAccess = sqlDataAccess;
    }

    public async Task<int> Add(Message message)
    {
        // The stored procedure prevents from persisting duplicates, so we don't need to check this before called the database
        var parameters = new DynamicParameters();
        parameters.Add("TicketId", message.TicketId);
        parameters.Add("DeskproMessageId", message.DeskproMessageId);
        parameters.Add("Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rowsAffected = await _sqlDataAccess.ExecuteProcedure("dbo.spMessage_Insert", parameters);

        if (rowsAffected == 0)
        {
            // TODO
            throw new Exception($"Error inserting message {message}");
        }

        var id = parameters.Get<int>("Id");
        return id;
    }

    public async Task<int> Delete(int id) => await _sqlDataAccess.Execute("UPDATE Messages SET Deleted = 1 WHERE Id = @Id", new { Id = id });
    
    public async Task<Message?> GetById(int id) => await _sqlDataAccess.QuerySingle<Message>("SELECT * FROM v_Messages WHERE Id = @Id", new { Id = id });
    
    public async Task<Message?> GetByDeskproMessageId(int deskproMessageId) => await _sqlDataAccess.QuerySingle<Message>("SELECT * FROM v_Messages WHERE DeskproMessageId = @DeskproMessageId", new { DeskproMessageId = deskproMessageId });

    public async Task<int> Update(Message message)
    {
        var sql = @"
            UPDATE Messages SET
                TicketId = @TicketId,
                DeskproId = @DeskproId,
                GODocumentId = @GODocumentId,
                Deleted = @Deleted,
                MessageNumber = @MessageNumber
            WHERE Id = @Id";
            
        return await _sqlDataAccess.Execute(sql, message);
    }
}