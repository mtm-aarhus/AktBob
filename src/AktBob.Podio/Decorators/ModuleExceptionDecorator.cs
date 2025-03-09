using AAK.Podio.Models;
using AktBob.Podio.Contracts;
using AktBob.Shared;
using Microsoft.Extensions.Logging;

namespace AktBob.Podio.Decorators;

internal class ModuleExceptionDecorator(IPodioModule inner, ILogger<ModuleExceptionDecorator> logger) : IPodioModule
{
    private readonly IPodioModule _inner = inner;
    private readonly ILogger<ModuleExceptionDecorator> _logger = logger;

    public async Task<Result<Item>> GetItem(PodioItemId podioItemId, CancellationToken cancellationToken)
    {
        try
        {
            return await _inner.GetItem(podioItemId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in {name}", nameof(GetItem));
            throw;
        }
    }

    public void PostComment(PostCommentCommand command)
    {
        try
        {
            _inner.PostComment(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueueing job: Post comment on Podio Item ({command})", command);
            throw;
        }
    }

    public void UpdateTextField(UpdateTextFieldCommand command)
    {
        try
        {
            _inner.UpdateTextField(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enqueueing job: Update Podio text field ({command})", command);
            throw;
        }
    }
}
