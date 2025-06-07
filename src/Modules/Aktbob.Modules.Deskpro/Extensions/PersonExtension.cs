using AAK.Deskpro.Models;

namespace Aktbob.Modules.Deskpro.Extensions;
internal static class PersonExtension
{
    public static PersonDto ToDto(this Person? person)
    {
        if (person == null)
        {
            return new PersonDto();
        }

        return new PersonDto
        {
            IsAgent = person.IsAgent,
            DisplayName = person.DisplayName,
            Email = person.Email,
            FirstName = person.FirstName,
            FullName = person.FullName,
            Id = person.Id,
            LastName = person.LastName,
            PhoneNumbers = person.PhoneNumbers,
            TeamId = person.TeamId
        };
    }
}
