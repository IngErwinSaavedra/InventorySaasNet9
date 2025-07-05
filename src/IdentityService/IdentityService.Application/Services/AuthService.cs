
using IdentityService.Application.Interfaces;
using IdentityService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SharedKernel.DTOs;


namespace IdentityService.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(UserManager<ApplicationUser> userManager, IEmailSender emailSender,
        IApplicationDbContext context, IConfiguration configuration)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _context = context;
        _configuration = configuration;
    }

    public async Task RegisterUserAsync(RegisterRequestDto dto, string origin)
    {
        var tenant = new Tenant(dto.LastName, "free");
        _context.Tenants.Add(tenant);
        await _context.SaveChangesAsync(CancellationToken.None);

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            TenantId = tenant.Id
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            throw new ApplicationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var callbackUrl = $"{origin}/api/auth/confirm-email?userId={user.Id}&token={encodedToken}";


        var html = $@"
            <h1>Confirma tu cuenta</h1>
            <p>Gracias por registrarte en nuestra plataforma.</p>
            <p>Haz clic en el siguiente enlace para confirmar tu correo:</p>
            <p><a href='{callbackUrl}'>{callbackUrl}</a></p>
        ";

        await _emailSender.SendEmailAsync(dto.Email, "Confirma tu cuenta", html);
    }

    public async Task<bool> ConfirmEmailAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            Console.WriteLine("Usuario no encontrado");
            return false;
        }

        var result = await _userManager.ConfirmEmailAsync(user, Uri.UnescapeDataString(token));
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error confirmando email: {error.Description}");
            }
        }
        else
        {
            Console.WriteLine("Email confirmado correctamente");
            Console.WriteLine($"Email confirmado correctamente para {user.Email}");
            Console.WriteLine($"Campo EmailConfirmed en memoria: {user.EmailConfirmed}");

            // *** AÑADE ESTA LÍNEA ***
            var saveResult = await _context.SaveChangesAsync(CancellationToken.None);
            Console.WriteLine($"SaveChangesAsync resultado: {saveResult}");

            // Verifica el valor en la base de datos también
            var userInDb = await _userManager.FindByIdAsync(userId);
            Console.WriteLine($"EmailConfirmed en DB (tras guardar y refrescar): {userInDb?.EmailConfirmed}");
        }

        return result.Succeeded;
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequestDto dto, string origin)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var resetLink = $"{origin}/api/auth/reset-password?userId={user.Id}&token={encodedToken}";
        var html = $@"
            <h1>Restablece tu contraseña</h1>
            <p>Haz clic en el enlace para restablecer tu contraseña:</p>
            <p><a href='{resetLink}'>{resetLink}</a></p>
        ";
        try
        {
            await _emailSender.SendEmailAsync(user.Email, "Restablecer contraseña", html);
            Console.WriteLine($"Correo de restablecimiento enviado a: {user.Email}"); // Agrega un log de éxito
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al enviar correo de restablecimiento: {ex.Message}");
            // Registra la excepción completa para más detalles
            Console.WriteLine(ex.ToString());
        }
        
    }

    public async Task ResetPasswordAsync(ResetPasswordRequestDto dto)
    {
        if (_userManager == null)
        {
            Console.WriteLine("Error: UserManager no está inicializado.");
            return; // O lanza una excepción
        }
        
        var user = await _userManager.FindByIdAsync(dto.UserId); // Declara y asigna el valor a 'user'
        if (user == null)
        {
            Console.WriteLine($"Error: No se encontró usuario con ID: {dto.UserId}");
            return;
        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
        if (!result.Succeeded)
        {
            Console.WriteLine("Error al restablecer la contraseña:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"- {error.Description}");
            }
            // Considera lanzar una excepción o registrar los errores.
            return;
        }
    }
    
    
}