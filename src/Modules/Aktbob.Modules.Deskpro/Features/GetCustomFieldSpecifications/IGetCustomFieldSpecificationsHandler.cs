namespace Aktbob.Modules.Deskpro.Features.GetCustomFieldSpecifications;
internal interface IGetCustomFieldSpecificationsHandler
{
    Task<ErrorOr<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken);
}