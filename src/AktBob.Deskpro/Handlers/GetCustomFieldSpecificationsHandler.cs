using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers;
internal class GetCustomFieldSpecificationsHandler(IDeskproClient deskproClient) : IGetCustomFieldSpecificationsHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<Result<IEnumerable<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken)
    {
        var dto = await _deskproClient.GetCustomFieldSpecifications(cancellationToken);
        var specifications = dto.Select(x => new CustomFieldSpecificationDto(x.Id, x.Title, x.Choices));

        if (specifications is null)
        {
            return Result.Error("Error getting custom field specification from Deskpro. The Deskpro client returned null.");
        }
        
        return Result.Success(specifications);
    }
}