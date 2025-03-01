using AAK.Deskpro.Models;

namespace AktBob.Deskpro;
internal static class Mappers
{
    public static PersonDto MapPerson(Person? person)
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
            PhoneNumbers = person.PhoneNumbers
        };
    }
}
