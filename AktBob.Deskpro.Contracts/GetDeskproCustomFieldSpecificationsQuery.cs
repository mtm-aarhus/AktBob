using AktBob.Deskpro.Contracts.DTOs;
using Ardalis.Result;
using MassTransit.Mediator;

namespace AktBob.Deskpro.Contracts;
public record GetDeskproCustomFieldSpecificationsQuery() : Request<Result<IEnumerable<CustomFieldSpecificationDto>>>;