using System.Text.Json.Serialization;
using B3.TradingCore.Application.DTOs;
using B3.TradingCore.Application.Services;
using B3.TradingCore.Domain.Enums;
using B3.TradingCore.Infrastructure;
using B3.TradingCore.Infrastructure.Persistence;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Configura Controllers e serialização de Enums como strings legíveis no JSON
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 2. Swagger / OpenAPI com documentação institucional Vortex Labs & B3
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "B3 TradingCore API · Vortex Labs",
        Version = "v1.0.0",
        Description = "Motor institucional de Pós-Negociação, Liquidação D+2 e Clearing de Ordens da B3 em C# e .NET 8 com Clean Architecture, PostgreSQL, Redis e RabbitMQ. Projeto em produção no ecossistema Vortex Labs.",
        Contact = new OpenApiContact
        {
            Name = "João Paulo Gregório · Vortex Software",
            Email = "contato@vortexsoftware.tech",
            Url = new Uri("https://vortexsoftware.tech")
        }
    });
});

// 3. Injeção de Dependências das Camadas de Arquitetura Limpa
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddScoped<ITradingService, TradingService>();

// 4. Configuração de CORS para permitir consumo por frontends e dashboards
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

// 5. Inicialização automática do schema PostgreSQL e seed de cotações B3
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = services.GetRequiredService<TradingDbContext>();
        db.Database.EnsureCreated();
        logger.LogInformation("Banco de dados PostgreSQL (B3 TradingCore) inicializado com sucesso.");

        var tradingService = services.GetRequiredService<ITradingService>();
        var existingTrades = await tradingService.GetTradesByAccountAsync("CC-98765-VORTEX");
        if (existingTrades.Count == 0)
        {
            logger.LogInformation("Realizando carga inicial de demonstração da B3...");
            await tradingService.ExecuteTradeAsync(new CreateTradeRequest("CC-98765-VORTEX", "PETR4", MarketType.Equities, OrderSide.Buy, 38.50m, 500));
            await tradingService.ExecuteTradeAsync(new CreateTradeRequest("CC-98765-VORTEX", "VALE3", MarketType.Equities, OrderSide.Buy, 62.10m, 300));
            await tradingService.ExecuteTradeAsync(new CreateTradeRequest("CC-98765-VORTEX", "ITUB4", MarketType.Equities, OrderSide.Buy, 34.80m, 1000));
            await tradingService.ExecuteTradeAsync(new CreateTradeRequest("CC-98765-VORTEX", "BBAS3", MarketType.Equities, OrderSide.Buy, 27.90m, 800));
            logger.LogInformation("Carga inicial de operações B3 concluída com sucesso.");
        }
    }
    catch (Exception ex)
    {
        logger.LogWarning(ex, "Aviso na inicialização de dados/cache B3: {Message}", ex.Message);
    }
}

// 6. Pipeline HTTP
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "B3 TradingCore v1 - Vortex Labs");
    c.RoutePrefix = string.Empty; // Swagger abre direto na raiz http://b3.vortexsoftware.tech
    c.DocumentTitle = "B3 TradingCore API · Vortex Labs";
});

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
