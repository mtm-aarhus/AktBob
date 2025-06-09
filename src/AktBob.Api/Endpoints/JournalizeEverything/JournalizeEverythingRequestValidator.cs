using FastEndpoints;
using FluentValidation;

namespace AktBob.Api.Endpoints.JournalizeEverything;

internal class JournalizeEverythingRequestValidator : Validator<JournalizeEverythingRequest>
{
    public JournalizeEverythingRequestValidator()
    {
        RuleFor(x => x.DeskproId).NotNull();
    }
}
