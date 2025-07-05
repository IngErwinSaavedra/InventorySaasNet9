// src/InventoryService/InventoryService.Application/Services/ProductService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using InventoryService.Domain.Entities;
using InventoryService.Domain.Repositories;

namespace InventoryService.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<ProductResponseDto?> GetProductByIdAsync(Guid productId, Guid tenantId)
        {
            var product = await _productRepository.GetByIdAsync(productId, tenantId);
            return product == null ? null : MapProductToDto(product);
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllProductsAsync(Guid tenantId)
        {
            var products = await _productRepository.GetAllAsync(tenantId);
            return products.Select(MapProductToDto);
        }

        public async Task<ProductResponseDto> CreateProductAsync(CreateProductDto productDto, Guid tenantId)
        {
            var product = new Product
            {
                Id = Guid.NewGuid(),
                TenantId = tenantId, // Asignar TenantId
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                CreatedAt = DateTime.UtcNow // Asignar fecha de creación
            };

            await _productRepository.AddAsync(product);
            await _productRepository.SaveChangesAsync();
            
            return MapProductToDto(product);
        }

        public async Task<ProductResponseDto?> UpdateProductAsync(Guid productId, UpdateProductDto productDto, Guid tenantId)
        {
            var existingProduct = await _productRepository.GetByIdAsync(productId, tenantId);

            if (existingProduct == null)
            {
                return null;
            }

            existingProduct.Name = productDto.Name;
            existingProduct.Description = productDto.Description;
            existingProduct.Price = productDto.Price;
            existingProduct.Stock = productDto.Stock;
            existingProduct.UpdatedAt = DateTime.UtcNow; // Actualizar fecha de modificación

            await _productRepository.UpdateAsync(existingProduct);
            await _productRepository.SaveChangesAsync();

            return MapProductToDto(existingProduct);
        }

        public async Task<bool> DeleteProductAsync(Guid productId, Guid tenantId)
        {
            var product = await _productRepository.GetByIdAsync(productId, tenantId);
            if (product == null)
            {
                return false;
            }

            await _productRepository.DeleteAsync(productId, tenantId);
            await _productRepository.SaveChangesAsync();
            return true;
        }

        // Método de mapeo simple (considera usar AutoMapper para proyectos más grandes)
        private ProductResponseDto MapProductToDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                TenantId = product.TenantId,
                Name = product.Name ?? string.Empty,
                Description = product.Description ?? string.Empty,
                Price = product.Price,
                Stock = product.Stock,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}