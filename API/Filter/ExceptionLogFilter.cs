using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace TODO_List.API.Filter
{
    public class ExceptionLogFilter:IExceptionFilter
    {
        private readonly ILogger<ExceptionLogFilter> _logger;
        public ExceptionLogFilter(ILogger<ExceptionLogFilter> logger)
        {
            _logger = logger;
        }
        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "An unhandled exception occurred while processing the request.");
            context.Result = new ObjectResult(new { error = "An unexpected error occurred. Please try again later.", 
                details = context.Exception.Message
            })
            {
                StatusCode = 500
            };
            context.ExceptionHandled = true;
        }
    }
}
