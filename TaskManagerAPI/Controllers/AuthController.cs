
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManagerAPI.Models;
using TaskManagerAPI.Models.Requests;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private static readonly List<User> _users = new List<User>();
        private static int _nextUserId = 1;
        private readonly IConfiguration _config;

        public AuthController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("register")]
        public ActionResult<AuthResponse> Register([FromBody] RegisterRequest request)
        {
            if (_users.Any(u => u.Email == request.Email))
            {
                return BadRequest(new { error = "Пользователь с таким email уже зарегистрирован" });
            }

            string passwordHash = HashPassword(request.Password);

            var user = new User
            {
                Id = _nextUserId++,
                Email = request.Email,
                PasswordHash = passwordHash,
                Name = request.Name
            };

            _users.Add(user);

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60"))
            });
        }
        [HttpPost("login")]
        public ActionResult<AuthResponse> Login([FromBody] LoginRequest request)
        {
            var user = _users.FirstOrDefault(u => u.Email == request.Email);

            if (user == null || user.PasswordHash != HashPassword(request.Password))
            {
                return Unauthorized(new { error = "Неверный email или пароль" });
            }

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                Email = user.Email,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpiryMinutes"] ?? "60"))
            });
        }
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "12345678912345678912345678912345"));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(60),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}