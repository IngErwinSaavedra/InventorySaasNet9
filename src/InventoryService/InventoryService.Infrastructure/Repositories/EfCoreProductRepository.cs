using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Repositories;
using InventoryService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Repositories
{
    public class EfCoreProductRepository : IProductRepository
    {
        private readonly InventoryDbContext _context;

        public EfCoreProductRepository(InventoryDbContext context)
        {
            _context = context;
        }

        public async Task<Product?> GetByIdAsync(Guid productId, Guid tenantId)
        {
            // IMPORTANTE: Siempre filtrar por TenantId
            return await _context.Products
                                 .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId);
        }

        public async Task<IEnumerable<Product>> GetAllAsync(Guid tenantId)
        {
            // IMPORTANTE: Siempre filtrar por TenantId
            return await _context.Products
                                 .Where(p => p.TenantId == tenantId)
                                 .ToListAsync();
        }

        public async Task AddAsync(Product product)
        {
            await _context.Products.AddAsync(product);
        }

        public async Task UpdateAsync(Product product)
        {
            await Task.Run(() => _context.Products.Update(product));
        }

        public async Task DeleteAsync(Guid productId, Guid tenantId) // <-- Coincide con la interfaz
        {
            // Primero, encuentra el producto por Id y TenantId
            var productToDelete = await _context.Products
                                                .FirstOrDefaultAsync(p => p.Id == productId && p.TenantId == tenantId);

            if (productToDelete != null)
            {
                _context.Products.Remove(productToDelete);
                // No llamas a SaveChangesAsync aquí porque el repositorio se encarga de ello
                // a través del método SaveChangesAsync()
            }
            // Considera manejar el caso donde el producto no se encuentra (ej. lanzar una excepción)
            // Depende de la lógica de tu aplicación si quieres que la eliminación de algo que no existe sea un error o una operación no exitosa
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}