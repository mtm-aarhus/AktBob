using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Handlers.GetCustomFieldSpecifications;
internal interface IGetCustomFieldSpecificationsHandler
{
    Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken);
}