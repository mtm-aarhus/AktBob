using AAK.Deskpro;
using AktBob.Shared.Contracts.Modules.Deskpro.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetCustomFieldSpecifications;
internal class GetCustomFieldSpecificationsHandler(IDeskproClient deskproClient) : IGetCustomFieldSpecificationsHandler
{
    private readonly IDeskproClient _deskproClient = deskproClient;

    public async Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken)
    {
        var dto = await _deskproClient.GetCustomFieldSpecifications(cancellationToken);
        return dto.Select(x => new CustomFieldSpecificationDto(x.Id, x.Title, x.Choices)).ToList();
    }
}