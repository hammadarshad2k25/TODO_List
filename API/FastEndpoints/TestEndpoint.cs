using FastEndpoints;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace TODO_List.API.FastEndpoints
{
    public class TestEndpoint : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/api/Test");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var response = new
            {
                status = "Ok",
                serverTime = DateTime.UtcNow
            };
            HttpContext.Response.StatusCode = StatusCodes.Status200OK;
            HttpContext.Response.ContentType = "application/json";
            HttpContext.Response.Headers["Cache-Control"] = "public,max-age=60";
            await HttpContext.Response.WriteAsJsonAsync(response, ct);  
        }
    }
}
