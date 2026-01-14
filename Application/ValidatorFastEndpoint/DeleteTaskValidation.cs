using FluentValidation;
using TODO_List.Application.DTO;

namespace TODO_List.Application.ValidatorFastEndpoint
{
    public class DeleteTaskValidation : AbstractValidator<DeleteTaskRequest>
    {
        public DeleteTaskValidation()
        {
            RuleFor(x => x.tid)
                .GreaterThan(0).WithMessage("Task ID must be greater than zero.");
        }
    }
}
