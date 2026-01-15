//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;
//using Microsoft.IdentityModel.Tokens;
//using System.Text;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Cryptography;
//using TODO_List.Application.DTO;
//using TODO_List.Infrastructure.Storage;

//namespace TODO_List.API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class AuthController : ControllerBase
//    {
//        private readonly IConfiguration _config;
//        public AuthController(IConfiguration config)
//        {
//            _config = config;
//        }
//        [HttpPost("Login")]
//        public IActionResult Login([FromBody] LoginModel lM)
//        { 
//            if (lM.UserName == "Admin" && lM.Password == "12345")
//            {
//                var token = GenerateJwtToken(lM.UserName);
//                var refreshToken = GenerateRefreshToken();
//                RefreshTokenStore.StoreToken(lM.UserName, refreshToken);
//                return Ok(new { token , refreshToken });
//            }
//            return Unauthorized("Invalid credentials");
//        }
//        [HttpPost("RefreshToken")]
//        public IActionResult RefreshToken([FromBody] RefreshDTO request)
//        {
//            if (!RefreshTokenStore.ValidateToken(request.UserName, request.RenewToken))
//            {
//                return Unauthorized("Invalid refresh token");
//            }
//            var newJwtToken = GenerateJwtToken(request.UserName);
//            var refreshToken = GenerateRefreshToken();
//            RefreshTokenStore.StoreToken(request.UserName, refreshToken);
//            return Ok(new { token = newJwtToken, RenewToken = refreshToken });
//        }
//        private string GenerateJwtToken(string userName)
//        {
//            var jwtSettings = _config.GetSection("Jwt");
//            var Claims = new[]
//            {
//                new Claim(ClaimTypes.Name, userName),
//                new Claim(ClaimTypes.Role, "Admin")
//            };
//            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
//            var creds = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);
//            var Token = new JwtSecurityToken(
//                issuer: jwtSettings["Issuer"],
//                audience: jwtSettings["Audience"],
//                claims: Claims,
//                expires: DateTime.Now.AddMinutes(Convert.ToDouble(jwtSettings["ExpireMinutes"])),
//                signingCredentials: creds
//            );
//            return new JwtSecurityTokenHandler().WriteToken(Token);
//        }
//        private string GenerateRefreshToken()
//        {
//            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
//        }
//    }
//}
