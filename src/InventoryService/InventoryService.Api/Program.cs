// src/InventoryService/InventoryService.Api/Program.cs
using System.Security.Claims; // Necesario para ClaimsPrincipal
using System.Text;
using InventoryService.Application; // Para AddApplication()
using InventoryService.Infrastructure; // Para AddInfrastructure()
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models; // Necesario para AddSwaggerGen

// Necesario para el filtro de tenantId

var builder = WebApplication.CreateBuilder(args);

// 1. Configurar la base de datos (Inventario)
builder.Services.AddInfrastructure(builder.Configuration); // Extensión personalizada

// 2. Configurar autenticación JWT (para validar tokens del IdentityService)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"], // Debe coincidir con el Issuer de IdentityService
            ValidAudience = builder.Configuration["Jwt:Audience"], // Debe coincidir con la audiencia a la que está destinado este servicio
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not found in configuration."))) // Misma clave secreta que IdentityService
        };

        // Opcional: Si el claim de TenantId no es un claim estándar, puedes mapearlo
        // options.MapInboundClaims = false; // Deshabilita el mapeo de claims por defecto
    });

builder.Services.AddAuthorization(); // Habilitar autorización

builder.Services.AddControllers(); // Añadir controladores para los endpoints de productos

// 3. Añadir servicios de la capa de aplicación
builder.Services.AddApplication(); // Extensión personalizada

// 4. Configurar Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Configuración para que Swagger envíe JWT en los requests
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// 5. Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin() // **ADVERTENCIA: Cambiar a dominios específicos en producción**
                          .AllowAnyMethod()
                          .AllowAnyHeader());
});

var app = builder.Build();

// 6. Configurar el pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.RunAsync();