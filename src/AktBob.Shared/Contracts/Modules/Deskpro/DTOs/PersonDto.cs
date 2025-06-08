using System.Collections.ObjectModel;

namespace AktBob.Shared.Contracts.Modules.Deskpro.DTOs;
public record PersonDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsAgent { get; set; }
    public int? TeamId { get; set; }
    public IEnumerable<string> PhoneNumbers { get; set; } = new Collection<string>();
}
