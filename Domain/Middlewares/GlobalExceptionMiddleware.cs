namespace TODO_List.Domain.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLogingMiddleware> _logger;
        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<RequestLogingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError("Unhandled Exception Occured. Path: {Path}", context.Request.Path);
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Internal Server Error"
                });
            }
        }
    }
}
