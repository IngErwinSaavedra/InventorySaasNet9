using System.Threading.Tasks;
using SharedKernel.DTOs;

namespace IdentityService.Application.Interfaces
{
    public interface IAuthService {
        Task RegisterUserAsync(RegisterRequestDto dto, string origin);
        Task<bool> ConfirmEmailAsync(string userId, string token);
        Task ForgotPasswordAsync(ForgotPasswordRequestDto dto, string callbackUrl);
        Task ResetPasswordAsync(ResetPasswordRequestDto dto);

        Task<AuthResponseDto> LoginAsync(LoginRequestDto dto);
        Task<AuthResponseDto> RefreshAccessTokenAsync(RefreshTokenRequestDto dto);
    }
}