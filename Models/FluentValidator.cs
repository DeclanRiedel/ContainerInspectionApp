using FluentValidation;
using ContainerInspectionApp.Models;

namespace ContainerInspectionApp.Validators
{
    public class ContainerValidator : AbstractValidator<Container>
    {
        public ContainerValidator()
        {
            RuleFor(x => x.ContainerId).NotEmpty().WithMessage("Container ID is required");
            RuleFor(x => x.ContainerType).NotEmpty().WithMessage("Container Type is required");
            RuleFor(x => x.ExtraInfo).MaximumLength(500).WithMessage("Extra Info must not exceed 500 characters");
        }
    }
}