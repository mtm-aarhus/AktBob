using AktBob.DatabaseAPI.Contracts.DTOs;
using AktBob.DatabaseAPI.Contracts.Queries;
using Ardalis.Result;
using MediatR;

namespace AktBob.DatabaseAPI.UseCases;
internal class GetMessageByDeskproMessageIdQueryHandler(IDatabaseApi databaseApi) : IRequestHandler<GetMessageByDeskproMessageIdQuery, Result<MessageDto>>
{
    private readonly IDatabaseApi _databaseApi = databaseApi;

    public async Task<Result<MessageDto>> Handle(GetMessageByDeskproMessageIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _databaseApi.GetMessageByDeskproMessageId(request.DeskproMessageId, cancellationToken);

        if (!result.IsSuccess)
        {
            return Result.Error();
        }

        if (result.Value.Count() == 0)
        {
            return Result.NotFound();
        }

        return result.Value.First();
    }
}
