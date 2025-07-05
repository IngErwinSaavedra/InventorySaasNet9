using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InventoryService.Application.DTOs;

namespace InventoryService.Application.Interfaces
{
    public interface IProductService
    {
        Task<ProductResponseDto?> GetProductByIdAsync(Guid productId, Guid tenantId);
        Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(Guid tenantId);
        Task<ProductResponseDto> CreateProductAsync(CreateProductDto productDto, Guid tenantId);
        Task<ProductResponseDto?> UpdateProductAsync(Guid productId, UpdateProductDto productDto, Guid tenantId);
        Task<bool> DeleteProductAsync(Guid productId, Guid tenantId);
    }
}