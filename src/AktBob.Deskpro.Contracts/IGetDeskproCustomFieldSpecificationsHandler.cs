using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public interface IGetDeskproCustomFieldSpecificationsHandler
{
    Task<Result<IEnumerable<CustomFieldSpecificationDto>>> Handle(CancellationToken cancellationToken);
}