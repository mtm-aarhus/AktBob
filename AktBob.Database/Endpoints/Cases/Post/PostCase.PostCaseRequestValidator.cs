using FastEndpoints;
using FluentValidation;

namespace AktBob.Database.Endpoints.Cases.Post;
internal class PostCaseRequestValidator : Validator<PostCaseRequest>
{
    public PostCaseRequestValidator()
    {
        RuleFor(x => x.TicketId).NotEmpty();
        RuleFor(x => x.PodioItemId).NotEmpty();
        RuleFor(x => x.CaseNumber).NotEmpty();
    }
}
