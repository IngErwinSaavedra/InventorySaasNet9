// src/InventoryService/InventoryService.Api/Controllers/ProductsController.cs
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using InventoryService.Application.DTOs;
using InventoryService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Api.Controllers
{
    [Authorize] // Requiere autenticación JWT
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // Helper para obtener el TenantId del JWT
        private Guid GetTenantId()
        {
            // Asegúrate de que el claim "tenant_id" esté presente en el JWT
            // El valor del claim siempre es una cadena, así que lo convertimos a Guid
            var tenantIdClaim = User.FindFirst("tenant_id")?.Value;
            if (string.IsNullOrEmpty(tenantIdClaim))
            {
                throw new UnauthorizedAccessException("Tenant ID claim not found in JWT.");
            }
            return Guid.Parse(tenantIdClaim);
        }

        // GET api/products
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), 200)]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> Get()
        {
            var tenantId = GetTenantId();
            var products = await _productService.GetAllProductsAsync(tenantId);
            return Ok(products);
        }

        // GET api/products/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProductResponseDto>> Get(Guid id)
        {
            var tenantId = GetTenantId();
            var product = await _productService.GetProductByIdAsync(id, tenantId);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        // POST api/products
        [HttpPost]
        [ProducesResponseType(typeof(ProductResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProductResponseDto>> Post([FromBody] CreateProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenantId = GetTenantId();
            var createdProduct = await _productService.CreateProductAsync(productDto, tenantId);
            
            return CreatedAtAction(nameof(Get), new { id = createdProduct.Id }, createdProduct);
        }

        // PUT api/products/{id}
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ProductResponseDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProductResponseDto>> Put(Guid id, [FromBody] UpdateProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tenantId = GetTenantId();
            var updatedProduct = await _productService.UpdateProductAsync(id, productDto, tenantId);
            if (updatedProduct == null)
            {
                return NotFound();
            }
            return Ok(updatedProduct);
        }

        // DELETE api/products/{id}
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Delete(Guid id)
        {
            var tenantId = GetTenantId();
            var deleted = await _productService.DeleteProductAsync(id, tenantId);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent(); // 204 No Content
        }
    }
}