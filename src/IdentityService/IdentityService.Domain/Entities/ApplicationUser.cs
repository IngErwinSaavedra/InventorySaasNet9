using System;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public Guid TenantId { get; set; }
        public Tenant Tenant { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? EmailVerificationToken { get; set; }
        
        public ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}