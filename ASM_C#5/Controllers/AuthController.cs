using ASM_C_5.Data;
using ASM_C_5.DTOS;
using ASM_C_5.DTOS.Responses;
using ASM_C_5.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ASM_C_5.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        private readonly ASM_C_5Context _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public AuthController(ASM_C_5Context context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(Login_DTO request)
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            if (user != null)
            {
                if (await _userManager.CheckPasswordAsync(user, request.Password))
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Sid, user.Id),
                        new Claim(ClaimTypes.Name, user.UserName!),
                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                    };

                    // Add role claims
                    foreach (var role in roles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    var token = GenerateJwtToken(authClaims);
                    var loginInfo = new LoginResponseDTO
                    {
                        Id = user.Id,
                        UserName = user.UserName!,
                        LastName = user.LastName,
                        FirstName = user.FirstName,
                        Token = token,
                    };
                    return Ok(new BaseResponse<LoginResponseDTO> { Data = loginInfo });
                }
            }

            return Unauthorized(new BaseResponse<string>
            {
                ErrorCode = 401,
                Message = "Invalid credentials"
            });
        }

        private string GenerateJwtToken(List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? ""));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
