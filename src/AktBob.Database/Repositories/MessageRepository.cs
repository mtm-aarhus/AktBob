using AktBob.Database.Contracts;
using AktBob.Database.DataAccess;
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

    public async Task<bool> Add(Message message)
    {
        // The stored procedure prevents from persisting duplicates, so we don't need to check this before called the database
        var parameters = new DynamicParameters();
        parameters.Add("TicketId", message.TicketId);
        parameters.Add("DeskproMessageId", message.DeskproMessageId);
        parameters.Add("Id", dbType: DbType.Int32, direction: ParameterDirection.Output);

        var rowsAffected = await _sqlDataAccess.ExecuteProcedure("dbo.spMessage_Insert", parameters);
        message.Id = parameters.Get<int?>("Id") ?? 0;
        return rowsAffected == 1;
    }

    public async Task<bool> Delete(int id) => await _sqlDataAccess.Execute("UPDATE Messages SET Deleted = 1 WHERE Id = @Id", new { Id = id }) == 1;
    
    public async Task<Message?> Get(int id) => await _sqlDataAccess.QuerySingle<Message>("SELECT * FROM v_Messages WHERE Id = @Id", new { Id = id });
    
    public async Task<Message?> GetByDeskproMessageId(int deskproMessageId) => await _sqlDataAccess.QuerySingle<Message>("SELECT * FROM v_Messages WHERE DeskproMessageId = @DeskproMessageId", new { DeskproMessageId = deskproMessageId });

    public async Task<bool> Update(Message message)
    {
        var sql = """
            UPDATE Messages SET
                TicketId = @TicketId,
                DeskproMessageId = @DeskproMessageId,
                GODocumentId = @GODocumentId,
                MessageNumber = @MessageNumber
            WHERE Id = @Id
            """;
            
        var rowsAffected = await _sqlDataAccess.Execute(sql, message);
        return rowsAffected == 1;
    }
}