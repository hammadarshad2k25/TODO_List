using FastEndpoints;

namespace TODO_List.API.FastEndpoints
{
    public class ExceptionEndpoint : EndpointWithoutRequest
    {
        public override void Configure()
        {
            Get("/api/GetException");
            AllowAnonymous();
        }
        public override async Task HandleAsync(CancellationToken ct)
        {
            throw new Exception("Test 500 Internal Server Error!"); 
        }
    }
}
