using AAK.Deskpro;
using Microsoft.Extensions.Configuration;

namespace AktBob.Deskpro.Handlers;
internal class GetPersonHandler(IDeskproClient deskpro, IConfiguration configuration, ILogger<GetPersonHandler> logger) : IGetPersonHandler
{
    private readonly IDeskproClient _deskpro = deskpro;
    private readonly IConfiguration _configuration = configuration;
    private readonly ILogger<GetPersonHandler> _logger = logger;

    public async Task<Result<PersonDto>> Handle(int personId, CancellationToken cancellationToken)
    {
        var person = await _deskpro.GetPersonById(personId, cancellationToken);
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

    public async Task<Result<PersonDto>> Handle(string email, CancellationToken cancellationToken)
    {
        var skip = _configuration.GetSection("Deskpro:GetPersonHandler:IgnoreEmails").Get<IEnumerable<string>>() ?? Enumerable.Empty<string>();
        if (skip.Contains(email))
        {
            _logger.LogDebug("Email address {email} found on ignore list, returning an empty PersonDto result", email);
            return Result.Success();
        }

        var persons = await _deskpro.GetPersonByEmail(email, cancellationToken);

        if (persons is null)
        {
            return Result.Error($"Error getting person by email {email} from Deskpro.");
        }

        var person = persons.FirstOrDefault();

        if (person is null)
        {
            return Result.Error($"Error getting person by email {email} from Deskpro.");

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