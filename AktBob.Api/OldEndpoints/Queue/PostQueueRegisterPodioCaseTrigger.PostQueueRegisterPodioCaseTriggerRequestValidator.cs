//namespace AktBob.ExternalQueue.Endpoints;
using FastEndpoints;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AktBob.ExternalQueue.Endpoints;
internal class PostQueueRegisterPodioCaseTriggerRequestValidator : Validator<PostQueueRegisterPodioCaseTriggerRequest>
{
    public PostQueueRegisterPodioCaseTriggerRequestValidator()
    {
        RuleFor(x => x.PodioItemId).NotNull();
    }
}
