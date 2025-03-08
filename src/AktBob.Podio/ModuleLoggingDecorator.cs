using AAK.Podio.Models;
using AktBob.Podio.Contracts;
using AktBob.Shared;
using AktBob.Shared.Extensions;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio;

internal class ModuleLoggingDecorator(IPodioModule inner, ILogger<ModuleLoggingDecorator> logger) : IPodioModule
{
    private readonly IPodioModule _inner = inner;
    private readonly ILogger<ModuleLoggingDecorator> _logger = logger;

    public async Task<Result<Item>> GetItem(PodioItemId podioItemId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting Podio item by id {podioItemId}", podioItemId);

        var result = await _inner.GetItem(podioItemId, cancellationToken);
        if (!result.IsSuccess)
        {
            _logger.LogWarning(result.Errors.AsString());
        }

        return result;
    }

    public void PostComment(PostCommentCommand command)
    {
        _logger.LogInformation("Enqueueing job: Post comment on Podio Item. {command}", command);
        _inner.PostComment(command);
    }

    public void UpdateTextField(UpdateTextFieldCommand command)
    {
        _logger.LogInformation("Enqueueing job: Update Podio text field. {command}", command);
        _inner.UpdateTextField(command);
    }
}
