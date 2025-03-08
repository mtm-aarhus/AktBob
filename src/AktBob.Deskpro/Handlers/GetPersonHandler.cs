using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers;
internal class GetPersonHandler(IDeskproClient deskpro) : IGetPersonHandler
{
    private readonly IDeskproClient _deskpro = deskpro;

    public async Task<Result<PersonDto>> Handle(int personId, CancellationToken cancellationToken)
    {
        var person = await _deskpro.GetPersonById(personId);
        if (person is null)
        {
            return Result.Error($"Error getting person {personId} from Deskpro.");
        }

        var dto = new PersonDto
        {
            Id = person.Id,
            IsAgent = person.IsAgent,
            DisplayName = person.DisplayName,
            Email = person.Email,
            FirstName = person.FirstName,
            LastName = person.LastName,
            FullName = person.FullName,
            PhoneNumbers = person.PhoneNumbers
        };

        return Result.Success(dto);
    }
}