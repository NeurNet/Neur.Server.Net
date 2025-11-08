using FluentValidation;
using Neur.Server.Net.API.Contracts.Models;
using Neur.Server.Net.Core.Interfaces;
using Neur.Server.Net.Core.Records;

namespace Neur.Server.Net.API.Validators;

public class CreateModelRequestValidator : AbstractValidator<CreateModelReqest> {
    public CreateModelRequestValidator() {
        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Name must not be empty")
            .MinimumLength(2).WithMessage("Name must be at least 2 characters long");
        RuleFor(x => x.version)
            .MinimumLength(1).WithMessage("Version must be at least 1 characters long");
        RuleFor(x => x.type)
            .NotEmpty().WithMessage("Type must not be empty")
            .Must(type => 
                Enum.IsDefined(typeof(ModelType), type)
            )
            .WithMessage("Invalid model type! Available types: 'text', 'code' or 'image'");
        RuleFor(x => x.status)
            .NotEmpty().WithMessage("Status must not be empty")
            .Must(status => 
                Enum.IsDefined(typeof(ModelStatus), status)
            )
            .WithMessage("Invalid model status! Available statuses: 'open' or 'locked'");
    }
}