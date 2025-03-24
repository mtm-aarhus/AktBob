using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers;
internal class GetCustomFieldSpecificationsHandler(IDeskproClient deskproClient) : IGetCustomFieldSpecificationsHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<Result<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken)
    {
        try
        {
            var dto = await _deskproClient.GetCustomFieldSpecifications(cancellationToken);
            var specifications = dto.Select(x => new CustomFieldSpecificationDto(x.Id, x.Title, x.Choices)).ToList();
            return Result.Success<IReadOnlyCollection<CustomFieldSpecificationDto>>(specifications);
        }
        catch (HttpRequestException ex)
        {
            return Result.Error($"Error getting custom field specification from Deskpro. {ex}");
        }
        catch (Exception)
        {
            throw;
        }
    }
}