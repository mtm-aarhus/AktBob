using AAK.Deskpro;

namespace AktBob.Deskpro.Handlers.GetCustomFieldSpecifications;
internal class GetCustomFieldSpecificationsHandler(IDeskproClient deskproClient) : IGetCustomFieldSpecificationsHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken)
    {
        var dto = await _deskproClient.GetCustomFieldSpecifications(cancellationToken);
        var specifications = dto.Select(x => new CustomFieldSpecificationDto(x.Id, x.Title, x.Choices)).ToList();
        return specifications;
    }
}