using FastEndpoints;
using System.Net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using FastEndpoints.Security;
using TODO_List.Application.DTO;

namespace TODO_List.API.FastEndpoints
{
    public class LoginFastEndPoint : Endpoint<LoginModel>
    {
        public override void Configure()
        {
            Post("/api/LoginFastEndPoint");
            AllowAnonymous();
        }
        public override async Task HandleAsync(LoginModel req, CancellationToken ct)
        {
            if (req.UserName != "Admin" || req.Password != "12345")
            {
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await HttpContext.Response.WriteAsync("UserName and Password does not match.", ct);
                return;
            }
            var token = JwtBearer.CreateToken(options =>
            {
                options.SigningKey = "a-string-secret-at-least-256-bits-long";
                options.ExpireAt = DateTime.UtcNow.AddHours(1);
                options.Issuer = "https://localhost:7054";
                options.Audience = "https://localhost:7054";
                options.User.Roles.Add("Admin");
                options.User["UserName"] = req.UserName;
            });
            await HttpContext.Response.WriteAsJsonAsync(new { Token = token }, cancellationToken: ct);
        }
    }
}
