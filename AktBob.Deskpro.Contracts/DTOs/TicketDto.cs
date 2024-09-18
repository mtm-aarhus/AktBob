namespace AktBob.Deskpro.Contracts.DTOs;
public class TicketDto
{
    public int Id { get; set; }
    public string Ref { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
    public int Department { get; set; }
    public PersonDto? Person { get; set; }
    public PersonDto? Agent { get; set; }
    public int? AgentTeamId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public IEnumerable<FieldDto> Fields { get; set; } = Enumerable.Empty<FieldDto>();
}
