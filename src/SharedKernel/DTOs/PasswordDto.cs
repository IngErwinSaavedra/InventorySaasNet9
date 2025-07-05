using System.ComponentModel.DataAnnotations;

namespace SharedKernel.DTOs;

public record ForgotPasswordRequestDto(
    [Required] [EmailAddress] string Email
);

public record ResetPasswordRequestDto(
    [Required] string UserId,
    [Required] [EmailAddress] string Email,
    [Required] string Token,
    [Required] [MinLength(6)] string NewPassword
);


    
    
    