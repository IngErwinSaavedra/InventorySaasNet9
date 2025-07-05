namespace SharedKernel.DTOs;

public record AuthResponseDto(
    [Required] string Token, // El JWT (Access Token)
    [Required] string RefreshToken, // El Refresh Token
    [Required] string UserId,
    [Required] Guid TenantId,       // Asumiendo que TenantId es un Guid
    [Required] string Plan,
    [Required] string Email,        // Útil devolver el email del usuario autenticado
    string? FirstName,    // Puede ser nulo
    string? LastName      // Puede ser nulo
    
    );