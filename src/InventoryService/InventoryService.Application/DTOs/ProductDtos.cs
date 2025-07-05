using System;
using System.ComponentModel.DataAnnotations;

namespace InventoryService.Application.DTOs
{
    // DTO para la creación de un producto
    public class CreateProductDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        [MaxLength(1000)]
        public string? Description { get; set; } // Puede ser nulo
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }

    // DTO para la actualización de un producto
    public class UpdateProductDto
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }
        
        [MaxLength(1000)]
        public string? Description { get; set; }
        
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }
    }

    // DTO para la respuesta de un producto
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}