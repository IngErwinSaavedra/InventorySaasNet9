using InventoryService.Application.Interfaces;
using InventoryService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryService.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IProductService, ProductService>();
            
            // Si usaras MediatR, lo configurarías aquí
            // services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProductService).Assembly)); // O la asamblea de tus handlers

            return services;
        }
    }
}