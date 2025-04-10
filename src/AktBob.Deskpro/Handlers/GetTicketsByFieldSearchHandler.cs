﻿using AAK.Deskpro;
using AAK.Deskpro.Models;
using AktBob.Shared.Extensions;
using System.Collections.ObjectModel;

namespace AktBob.Deskpro.Handlers;
internal class GetTicketsByFieldSearchHandler(IDeskproClient deskpro) : IGetTicketsByFieldSearchHandler
{
    private readonly IDeskproClient _deskpro = deskpro;

    public async Task<Result<IReadOnlyCollection<TicketDto>>> Handle(int[] fields, string searchValue, CancellationToken cancellationToken)
    {
        try
        {
            ICollection<Ticket> ticketsList = new Collection<Ticket>();

            foreach (var field in fields)
            {
                var tickets = await _deskpro.GetTicketsByFieldValue(field, searchValue, cancellationToken);

                if (tickets is not null)
                {
                    ticketsList!.AddRange(tickets);
                }
            }

            if (!ticketsList.Any())
            {
                return Result.Error($"No Deskpro tickets found by searching fields (fields: {string.Join(", ", fields.Select(x => x.ToString()))}) search value: '{searchValue}'.");
            }

            var dto = ticketsList.Select(t => new TicketDto
            {
                Id = t.Id,
                CreatedAt = (DateTime)t.CreatedAt!,
                Agent = Mappers.MapPerson(t.Agent),
                Person = Mappers.MapPerson(t.Person),
                AgentTeamId = t.AgentTeamId,
                Auth = t.Auth,
                Department = t.Department,
                Ref = t.Ref,
                Subject = t.Subject,
                Fields = t.Fields.Select(f => new FieldDto
                {
                    Id = f.Id,
                    Values = f.Values
                }) ?? Enumerable.Empty<FieldDto>()
            }).ToList();

            return Result.Success<IReadOnlyCollection<TicketDto>>(dto);
        }
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result.Error($"No tickets found by field search: {ex}");
        }
        catch (Exception)
        {
            throw;
        }
    }
}
