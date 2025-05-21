using AktBob.Shared.Types.Deskpro;

namespace AktBob.Api.Endpoints.CreateCleanUpQueueItems;

internal record CreateCleanUpQueueItemsRequest(TicketId DeskproTicketId);