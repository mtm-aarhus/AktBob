using Ardalis.GuardClauses;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Data;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using AktBob.Database.Entities;
using AktBob.Database.Extensions;
using AktBob.Deskpro.Contracts;
using MassTransit.Mediator;
using MassTransit;

namespace AktBob.Database.UseCases.Messages.PostMessage;
internal class PostMessageBackgroundJob : BackgroundService
{
    private readonly ILogger<PostMessageBackgroundJob> _logger;
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<Guid, DeskproTicketWithNewMessage> _deskproTicketsWithNewMessage;

    public PostMessageBackgroundJob(ILogger<PostMessageBackgroundJob> logger, IMediator mediator, IConfiguration configuration, ConcurrentDictionary<Guid, DeskproTicketWithNewMessage> deskproTicketsWithNewMessage)
    {
        _logger = logger;
        _mediator = mediator;
        _configuration = configuration;
        _deskproTicketsWithNewMessage = deskproTicketsWithNewMessage;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var registeredAtIntervalSeconds = _configuration.GetValue<int?>("WaitSecondsBeforeProcessingNewDeskproMessage") ?? 120;

        while (!stoppingToken.IsCancellationRequested)
        {
            // Get all registered tickets with new messages that have been waiting for at little while....
            // We need to wait since the newest message is not immediatly avaible from the Deskpro API
            var deskproTickets = _deskproTicketsWithNewMessage.Where(x => x.Value.RegisteredAt < DateTime.UtcNow.AddSeconds(-registeredAtIntervalSeconds));
            var deskproTicketIds = deskproTickets.Select(x => x.Value.DeskproTicketId).ToList();
            var keys = deskproTickets.Select(x => x.Key).ToArray();

            foreach (var key in keys)
            {
                if (_deskproTicketsWithNewMessage.TryGetValue(key, out DeskproTicketWithNewMessage? item))
                {
                    _deskproTicketsWithNewMessage.TryRemove(new KeyValuePair<Guid, DeskproTicketWithNewMessage>(key, item));
                }
            }

            foreach (var deskproTicketId in deskproTicketIds)
            {
                var getMessagesQuery = new GetDeskproMessagesQuery(deskproTicketId);
                var getMessagesResult = await _mediator.SendRequest(getMessagesQuery, stoppingToken);
                
                if (!getMessagesResult.IsSuccess)
                {
                    _logger.LogError("Error getting messages from Deskpro Ticket {id}", deskproTicketId);
                    continue;
                }

                // Persist the Deskpro ticket ID and message ID in the database
                var connectionString = Guard.Against.NullOrEmpty(_configuration.GetConnectionString("Database"));

                using (var connection = new SqlConnection(connectionString))
                {
                    // Get the ticket object in the database by the Deskpro ticket ID
                    var getDatabaseTicketParameters = new DynamicParameters();
                    getDatabaseTicketParameters.Add(Constants.T_TICKETS_DESKPRO_ID, deskproTicketId);

                    var databaseTickets = await connection.QueryAsync<Ticket>(Constants.SP_TICKET_GET_BY_DESKPRO_ID, getDatabaseTicketParameters, commandType: CommandType.StoredProcedure);

                    if (databaseTickets != null)
                    {
                        if (databaseTickets.Count() > 1)
                        {
                            _logger.LogError("More than 1 entity in Tickets table with Deskpro ticket ID {id}", deskproTicketId);
                            continue;
                        }

                        var databaseTicket = databaseTickets.First();

                        foreach (var message in getMessagesResult.Value)
                        {
                            // The stored procedure prevents from persisting duplicates, so we don't need to check this before called the database
                            var parameters = new DynamicParameters();
                            parameters.Add(Constants.T_MESSAGES_TICKET_ID, databaseTicket.Id);
                            parameters.Add(Constants.T_MESSAGES_DESKPRO_ID, message.Id);
                            parameters.Add(Constants.T_MESSAGES_HASH, message.Content.GetHash());
                            parameters.Add(Constants.T_MESSAGES_ID, dbType: DbType.Int32, direction: ParameterDirection.Output);

                            await connection.QueryAsync(Constants.SP_MESSAGE_CREATE, parameters, commandType: CommandType.StoredProcedure);
                        }
                    }
                }
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}
