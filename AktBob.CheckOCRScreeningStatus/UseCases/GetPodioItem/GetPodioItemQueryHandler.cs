using AktBob.CheckOCRScreeningStatus.DTOs;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Configuration;

namespace AktBob.CheckOCRScreeningStatus.UseCases.GetPodioItem;
internal class GetPodioItemQueryHandler : IRequestHandler<GetPodioItemQuery, Result<PodioItemDto>>
{
    private readonly IAktBobApi _aktBobApi;
    private readonly IConfiguration _configuration;

    public GetPodioItemQueryHandler(IAktBobApi aktBobApi, IConfiguration configuration)
    {
        _aktBobApi = aktBobApi;
        _configuration = configuration;
    }

    public async Task<Result<PodioItemDto>> Handle(GetPodioItemQuery request, CancellationToken cancellationToken)
    {
        var appId = _configuration.GetValue<int>("Podio:AppId");
        return await _aktBobApi.GetPodioItem(appId, request.ItemId, cancellationToken);
    }
}
