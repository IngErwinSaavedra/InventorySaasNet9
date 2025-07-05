using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Repositories
{
    public interface IProductRepository
    {
        Task<Product?> GetByIdAsync(Guid productId, Guid TenantId);
        Task<IEnumerable<Product>> GetAllAsync(Guid TenantId);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid productId, Guid TenantId);
        Task SaveChangesAsync();
    }
}