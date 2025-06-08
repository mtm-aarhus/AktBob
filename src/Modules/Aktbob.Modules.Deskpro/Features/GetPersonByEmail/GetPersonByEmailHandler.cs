using AAK.Deskpro;
using AktBob.Shared;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetPersonByEmail;
internal class GetPersonByEmailHandler(IDeskproClient deskpro, IAppConfig appConfig) : IGetPersonByEmailHandler
{
    private readonly IDeskproClient _deskpro = deskpro;
    private readonly IAppConfig _appConfig = appConfig;

    public async Task<ErrorOr<PersonDto>> Handle(string email, CancellationToken cancellationToken)
    {
        var section = _appConfig.GetSection("GetPersonHandler:IgnoreEmails");
        var ignoreList = section?.Split(',') ?? Enumerable.Empty<string>();
        if (ignoreList.Contains(email))
        {
            return ErrorOrFactory.From(new PersonDto());
        }

        var persons = await _deskpro.GetPersonByEmail(email, cancellationToken);
        if (persons.FirstOrDefault() is null)
        {
            return Error.Failure("GetPersonByEmailHandler.Failure", $"Error getting person by email {email} from Deskpro.");
        }

        var person = persons.First();

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