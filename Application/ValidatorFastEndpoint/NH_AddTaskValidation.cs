using FluentValidation;
using TODO_List.Application.DTO;

namespace TODO_List.Application.ValidatorFastEndpoint
{
    public class NH_AddTaskValidation : AbstractValidator<NH_TaskRequestDTO>
    {
        public NH_AddTaskValidation()
        {
            RuleFor(x => x.tname)
                            .NotEmpty().WithMessage("Task name must not be empty.")
                            .MaximumLength(100).WithMessage("Task name must not exceed 100 characters.");
            RuleFor(x => x.tisCompleted)
                .Equal(false).WithMessage("New task cannot be marked completed.");
        }
    }
}
