namespace DefaultNamespace;

public record RefreshTokenRequestDto(
    [Required] string AccessToken,
    [Required] string RefreshToken
    );