using FluentValidation;
using TODO_List.Application.DTO;

namespace TODO_List.Application.ValidatorFastEndpoint
{
    public class GetTaskValidation : AbstractValidator<GetAllPaginatedRequest>
    {
        public GetTaskValidation()
        {
            RuleFor(x => x.page)
                .GreaterThan(0).WithMessage("Page number must be greater than 0.");
            RuleFor(x => x.pageSize)
                .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
        }
    }
}
