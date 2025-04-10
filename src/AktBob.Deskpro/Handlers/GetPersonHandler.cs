using AAK.Deskpro;
using AktBob.Shared;
using Microsoft.Extensions.Configuration;

namespace AktBob.Deskpro.Handlers;
internal class GetPersonHandler(IDeskproClient deskpro, IAppConfig appConfig, ILogger<GetPersonHandler> logger) : IGetPersonHandler
{
    private readonly IDeskproClient _deskpro = deskpro;
    private readonly IAppConfig _appConfig = appConfig;
    private readonly ILogger<GetPersonHandler> _logger = logger;

    public async Task<Result<PersonDto>> GetById(int personId, CancellationToken cancellationToken)
    {
        try
        {
            if (personId == 0)
            {
                return Result.Error($"Error getting person from Deskpro. Invalid ID #{personId}.");
            }

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
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result.Error($"Error getting Deskpro person {personId}: {ex}");
        }
        catch (Exception)
        {
            throw;
        }
    }

    public async Task<Result<PersonDto>> GetByEmail(string email, CancellationToken cancellationToken)
    {
        try
        {
            var section = _appConfig.GetSection("Deskpro:GetPersonHandler:IgnoreEmails");
            var ignoreList = section?.Split(',') ?? Enumerable.Empty<string>();
            if (ignoreList.Contains(email))
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
        catch (HttpRequestException ex)
        when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return Result.Error($"Error getting Deskpro person {email}: {ex}");
        }
        catch (Exception)
        {
            throw;
        }
    }
}