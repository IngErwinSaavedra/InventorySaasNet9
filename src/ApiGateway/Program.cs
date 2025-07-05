using Yarp.ReverseProxy.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ApiGateway.Infrastructure.YarpCustomTransforms;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms<CustomHeaderTransformProvider>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.Audience = builder.Configuration["Jwt:Audience"];
        options.RequireHttpsMetadata = builder.Environment.IsProduction();
    });

// Configuración de Autorización (Puerto de Autorización con Adaptadores de Políticas)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin")); // Política que requiere el rol "Admin"
    options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser()); // Requiere cualquier usuario autenticado
    // Aquí puedes añadir más políticas de autorización según tus necesidades,
    // que pueden usar adaptadores más complejos si las reglas no son triviales
});

// Configuración de CORS para el frontend de React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        builder => builder.WithOrigins("http://localhost:3000") // URL de tu app de React
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()); // Importante si manejas credenciales (cookies, auth headers)
});

// Opcional: Configuración de Rate Limiting (Adaptador de Rate Limiting)
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter(policyName: "fixed", opt =>
    {
        opt.PermitLimit = 10; // 10 solicitudes
        opt.Window = TimeSpan.FromSeconds(60); // Por minuto
    });
});

// Para Swagger/OpenAPI (documentación del Gateway)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- 2. Configuración del Pipeline HTTP (Conexión de Adaptadores en el Flujo) ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // Redirige a HTTPS en producción

// Habilitar CORS
app.UseCors("AllowReactApp");

// Habilitar autenticación y autorización. ¡El orden importa!
// Deben ir antes de MapReverseProxy para que las solicitudes se autentiquen/autoricen primero.
app.UseAuthentication();
app.UseAuthorization();

// Opcional: Habilitar Rate Limiting
app.UseRateLimiter();

// Mapear el proxy inverso de YARP. Este es el "corazón" del Gateway.
app.MapReverseProxy();

app.Run();