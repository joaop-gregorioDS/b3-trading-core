using System.Text.Json.Serialization;
using B3.TradingCore.Application.Services;
using B3.TradingCore.Infrastructure;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Configura Controllers e serialização de Enums como strings legíveis no JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 2. Swagger / OpenAPI com documentação institucional da B3
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "B3 TradingCore API",
        Version = "v1",
        Description = "Motor de Pós-Negociação, Liquidação e Clearing de Ordens da B3 em .NET 8 / C#.",
        Contact = new OpenApiContact
        {
            Name = "João Paulo Gregório",
            Email = "joaop.gregorio@outlook.com",
            Url = new Uri("https://github.com/joaop-gregorioDS")
        }
    });
});

// 3. Injeção de Dependências das Camadas de Arquitetura Limpa
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<ITradingService, TradingService>();

// 4. Configuração de CORS para permitir consumo por frontends (React / Next.js)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// 5. Pipeline HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "B3 TradingCore v1");
    c.RoutePrefix = string.Empty; // Swagger abre direto na raiz http://localhost:5000
});

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
