using System;

namespace IdentityService.Domain.Entities
{
    public interface IUser
    {
        Guid Id { get; set; }
    }
}