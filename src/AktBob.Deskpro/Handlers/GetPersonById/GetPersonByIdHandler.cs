using AAK.Deskpro;
using AktBob.Deskpro.Contracts;
using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetPersonById;
internal class GetPersonByIdHandler(IDeskproClient deskpro) : IGetPersonByIdHandler
{
    private readonly IDeskproClient _deskpro = deskpro;

    public async Task<ErrorOr<PersonDto>> Handle(int personId, CancellationToken cancellationToken)
    {
        if (personId == 0)
        {
            return Error.Failure("GetPersonByIdHandler.Failure", $"Error getting person from Deskpro: Invalid PersonId ({personId}).");
        }

        var person = await _deskpro.GetPersonById(personId, cancellationToken);
        if (person is null)
        {
            return Error.Failure("GetPersonByIdHandler.Failure", $"Error getting person {personId} from Deskpro.");
        }

        return new PersonDto
        {
            Id = person.Id,
            IsAgent = person.IsAgent,
            DisplayName = person.DisplayName,
            Email = person.Email,
            FirstName = person.FirstName,
            LastName = person.LastName,
            FullName = person.FullName,
            PhoneNumbers = person.PhoneNumbers,
            TeamId = person.TeamId
        };

    }
}