using System;
using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Data
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(255);
                entity.Property(p => p.Price).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(p => p.Stock).IsRequired();
                entity.Property(p => p.TenantId).IsRequired(); // TenantId es requerido
                entity.Property(p => p.CreatedAt).HasDefaultValueSql("now()").ValueGeneratedOnAdd(); // Auto-generar fecha de creación
                entity.Property(p => p.UpdatedAt).ValueGeneratedOnUpdate(); // Auto-generar fecha de actualización

                // Asegura que las consultas por TenantId sean eficientes
                entity.HasIndex(p => p.TenantId);
            });

            // Si quieres añadir un índice único combinado (ej., Name y TenantId deben ser únicos)
            modelBuilder.Entity<Product>()
                .HasIndex(p => new { p.TenantId, p.Name })
                .IsUnique();
        }

        
    }
}