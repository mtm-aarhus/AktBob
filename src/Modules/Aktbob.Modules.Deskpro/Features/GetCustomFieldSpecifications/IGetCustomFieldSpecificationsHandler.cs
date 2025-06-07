using Aktbob.Modules.Deskpro.Contracts.DTOs;

namespace Aktbob.Modules.Deskpro.Features.GetCustomFieldSpecifications;
internal interface IGetCustomFieldSpecificationsHandler
{
    Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken);
}