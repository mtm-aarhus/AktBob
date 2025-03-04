namespace AktBob.Deskpro.Contracts;
internal interface IGetCustomFieldSpecificationsHandler
{
    Task<Result<IEnumerable<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken);
}