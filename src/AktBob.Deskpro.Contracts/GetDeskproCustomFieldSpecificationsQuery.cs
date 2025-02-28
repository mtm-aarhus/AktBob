using AktBob.Deskpro.Contracts.DTOs;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproCustomFieldSpecificationsQuery() : IRequest<Result<IEnumerable<CustomFieldSpecificationDto>>>;