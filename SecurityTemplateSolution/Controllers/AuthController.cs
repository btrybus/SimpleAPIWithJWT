using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SimpleAPIWithJWT.Models;
using SimpleAPIWithJWT.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SimpleAPIWithJWT.Controllers
{
    [Authorize]
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly SecurityDbContext _context;
        private readonly TokenSettings _tokenSettings;
        public AuthController(SecurityDbContext context,
            IOptions<TokenSettings> tokenSettings)
        {
            _context = context;
            _tokenSettings = tokenSettings.Value;
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public ActionResult Register([FromBody] AuthRequest request)
        {
            var user = new User
            {
                Login = request.Login
            };
            AuthService.CreatePasswordHash(request.Password,
                out byte[] passwordHash,
                out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;
            _context.Add(user);
            _context.SaveChanges();
            return Ok(new { user.Id, user.Login });
        }

        [HttpGet("users")]
        public ActionResult GetUsers()
        {
            var users = _context.Users.Select(u => new { u.Id, u.Login }).ToList();
            return Ok(users);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public ActionResult Login([FromBody] AuthRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Błędne dane");

            var user = _context.Users.FirstOrDefault(x =>
                x.Login == request.Login);
            if (user == null)
            {
                BadRequest("Nieprawidłowa nazwa użytkownika");
            }
            if (!AuthService.VerifyPasswordHash(request.Password,
                user.PasswordHash,
                user.PasswordSalt))
            {
                return BadRequest("Nieprawidłowe hasło");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_tokenSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Login.ToString()),
                    new Claim(ClaimTypes.Role, "admin"),
                }),
                Expires = DateTime.UtcNow.AddHours(6),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return Ok(new { Token = tokenString });
        }       


    }
}
