using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproCustomFieldSpecificationsQuery() : IQuery<Result<IEnumerable<CustomFieldSpecificationDto>>>;