using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityService.Domain.Entities;

public class RefreshToken
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public string Token { get; set; }
    
    [Required]
    public string JwtId { get; set; }
    
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    public DateTime ExpiryDate { get; set; } // Fecha de expiración del refresh token

    public bool Used { get; set; } = false; // Indica si el token ya fue utilizado
    public bool Invalidated { get; set; } = false; // Indica si el token ha sido invalidado (ej. por seguridad)

    [Required]
    public string UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } // Navegación al usuario asociado
}