using System.ComponentModel.DataAnnotations;

namespace SharedKernel.DTOs
{
    public record RegisterRequestDto(
        [Required] string Email,
        [Required] string Password,
        [Required] string CompanyName,
        string FirstName,
        string LastName
    );  

    public record LoginRequestDto(
        [Required] string Email,
        [Required] string Password
    );

    public record UpdateProfileRequestDto(
        string? FirstName,
        string? LastName,
        string? PhoneNumber
    );

    public record ChangePasswordRequestDto(
        [Required] string CurrentPassword,
        [Required] string NewPassword
    );
}

