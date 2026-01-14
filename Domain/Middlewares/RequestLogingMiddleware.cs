namespace TODO_List.Domain.Middlewares
{
    public class RequestLogingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLogingMiddleware> _logger;
        public RequestLogingMiddleware(RequestDelegate next, ILogger<RequestLogingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("Http {Method} {Path} Started: ", context.Request.Method, context.Request.Path);
            await _next(context);
            _logger.LogInformation("Http {Method} {Path} Completed With {StatusCode}: ", context.Request.Method, context.Request.Path, context.Response.StatusCode);
        }
    }
}
