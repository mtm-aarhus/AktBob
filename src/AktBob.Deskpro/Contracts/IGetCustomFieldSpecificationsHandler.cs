namespace AktBob.Deskpro.Contracts;
internal interface IGetCustomFieldSpecificationsHandler
{
    Task<Result<IReadOnlyCollection<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken);
}