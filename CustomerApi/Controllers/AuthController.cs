using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CustomerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly string _key = "c8279fdf3b4f47eba9a604a453c2fc37";

        [HttpPost("login")]
        [ProducesResponseType(typeof(string), 200)]
        [ProducesResponseType(401)]
        public IActionResult Login([FromBody] LoginModel login)
        {
            if (login.Username == "user" && login.Password == "password")
            {
                var token = GenerateJwtToken(new[] { "User" });
                return Ok(new { token });
            }
            else if (login.Username == "admin" && login.Password == "password")
            {
                var token = GenerateJwtToken(new[] { "Admin" });
                return Ok(new { token });
            }
            return Unauthorized();
        }

        private string GenerateJwtToken(string[] roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Role, string.Join(",", roles))
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
