using FluentValidation;
using TODO_List.Application.DTO;

namespace TODO_List.Application.ValidatorFastEndpoint
{
    public class UpdateTaskValidation : AbstractValidator<UpdateTaskRequest>
    {
        public UpdateTaskValidation()
        {
            RuleFor(x => x.tname)
                .NotEmpty().WithMessage("Task name cannot be empty.")
                .MaximumLength(100).WithMessage("Task name cannot exceed 100 characters.");
            RuleFor(x => x.tisCompleted)
                .NotNull().WithMessage("Completion status must be provided.");
        }
    }
}
