using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace TODO_List.API.Filter
{
    public class ActionLogFilter : IActionFilter
    { 
        private readonly ILogger<ActionLogFilter> _logger;
        public ActionLogFilter(ILogger<ActionLogFilter> logger)
        {
            _logger = logger;
        }
        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation($"Action '{context.ActionDescriptor.DisplayName}' is starting at {DateTime.UtcNow}");
        }
        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation($"Action '{context.ActionDescriptor.DisplayName}' has finished at {DateTime.UtcNow}");
        }
    }
}
