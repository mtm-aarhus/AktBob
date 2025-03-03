namespace AktBob.Deskpro.Contracts;
internal interface IGetDeskproCustomFieldSpecificationsHandler
{
    Task<Result<IEnumerable<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken);
}