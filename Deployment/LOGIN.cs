using FastEndpoints;
using FastEndpoints.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using TODO_List.Application.DTO;

namespace TODO_List.Deployment
{
    public class LOGIN : Endpoint<LoginModel>
    {
        public override void Configure()
        {
            Post("/api/LOGIN");
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

            // Read JWT settings from environment variables
            var secret = Environment.GetEnvironmentVariable("JWT_KEY");
            var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER");
            var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE");
            var expireHours = int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRE_HOURS"), out var h) ? h : 1;

            var token = JwtBearer.CreateToken(options =>
            {
                options.SigningKey = secret;
                options.ExpireAt = DateTime.UtcNow.AddHours(expireHours);
                options.Issuer = issuer;
                options.Audience = audience;
                options.User.Roles.Add("Admin");
                options.User["UserName"] = req.UserName;
            });

            await HttpContext.Response.WriteAsJsonAsync(new { Token = token }, cancellationToken: ct);
        }
    }
}
