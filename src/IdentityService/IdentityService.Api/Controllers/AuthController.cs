using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using IdentityService.Api.Data;
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities; // Si usas la entidad Tenant
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SharedKernel.DTOs; // Para acceder a IdentityDbContext para Tenants

namespace IdentityService.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IdentityDbContext _context; // Para gestionar Tenants
        private readonly IAuthService _authService;

        public AuthController(UserManager<ApplicationUser> userManager,
                              SignInManager<ApplicationUser> signInManager,
                              IConfiguration configuration,
                              IdentityDbContext context,
                              IAuthService authService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _context = context;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var newTenant = new Tenant(model.CompanyName, "Free");
            _context.Tenants.Add(newTenant);
            await _context.SaveChangesAsync();

            // Delegar el resto al servicio AuthService
            await _authService.RegisterUserAsync(model, $"{Request.Scheme}://{Request.Host}");

            return Ok(new { Message = "Usuario registrado correctamente. Revisa tu correo para confirmar tu cuenta." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users
                .Where(u => u.Email == model.Email)
                .Include(u => u.Tenant) // Incluir la relación con Tenant
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return Unauthorized(new { Message = "Invalid credentials" });
            }

            if (user.Tenant == null)
            {
                // Log del error para diagnóstico
                Console.WriteLine($"Error: User with Email '{model.Email}' has no associated Tenant.");
                return StatusCode(500, new { Message = "Error: User has no associated tenant." });
            }

            if (!user.EmailConfirmed)
            {
                return Unauthorized(new { Message = "Debes confirmar tu correo electrónico antes de iniciar sesión." });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);

            if (!result.Succeeded)
            {
                return Unauthorized(new { Message = "Credenciales inválidas." });
            }
            
            var token = GenerateJwtToken(user);
            return Ok(new
            {
                Token = token,
                UserId = user.Id,
                TenantId = user.TenantId,
                Plan = user.Tenant.InitialPlan // Si el plan está asociado al tenant
            });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("tenant_id", user.TenantId.ToString()), // ¡CRÍTICO para multi-tenancy!
                new Claim("plan", user.Tenant.InitialPlan) // Si el plan está asociado al tenant
            };

            // Puedes añadir roles si los usas
            // var roles = await _userManager.GetRolesAsync(user);
            // foreach (var role in roles)
            // {
            //     claims.Add(new Claim(ClaimTypes.Role, role));
            // }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(7)); // Token válido por 7 días

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("Faltan parámetros.");
            
            var result = await _authService.ConfirmEmailAsync(userId, token);
            
            if (!result)
            {
                return BadRequest("Token inválido o expirado.");
            }

            return Ok("Correo confirmado correctamente.");
        }
        
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return Ok(new { Message = "Si tu correo está registrado, recibirás un enlace para restablecer tu contraseña." });
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = System.Web.HttpUtility.UrlEncode(token);
            var origin = $"{Request.Scheme}://{Request.Host}";
            var callbackUrl = $"{origin}/api/auth/reset-password?email={user.Email}&token={encodedToken}";

            // Aquí deberías enviar un correo con el callbackUrl
            await _authService.ForgotPasswordAsync(model, callbackUrl);

            return Ok(new { Message = "Se ha enviado un correo con instrucciones para restablecer la contraseña." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new { Message = "Usuario no encontrado." });

            var decodedToken = System.Web.HttpUtility.UrlDecode(model.Token);
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return BadRequest(new { Errors = errors });
            }

            return Ok(new { Message = "Contraseña restablecida correctamente." });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var authResponse = await _authService.RefreshTokenAccessTokenAsync(model);
                return Ok(authResponse);
            }
            catch (ApplicationException ex)
            {
                return Unauthorized(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Ocurrió un error inesperado durante el refresco del token." });
            }
        }
        
    }
}