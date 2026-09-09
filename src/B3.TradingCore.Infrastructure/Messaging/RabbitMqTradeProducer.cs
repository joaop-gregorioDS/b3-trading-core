using System.Text;
using System.Text.Json;
using B3.TradingCore.Application.DTOs;
using B3.TradingCore.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace B3.TradingCore.Infrastructure.Messaging;

/// <summary>
/// Produtor de mensagens para o RabbitMQ.
/// Publica o evento de execução de ordem da B3 para ser processado de forma desacoplada
/// pelo motor de compensação e liquidação (Clearing).
/// </summary>
public class RabbitMqTradeProducer : ITradeMessageProducer, IDisposable
{
    private readonly ILogger<RabbitMqTradeProducer> _logger;
    private readonly IConnection? _connection;
    private readonly IModel? _channel;
    private const string ExchangeName = "b3.trading.exchange";
    private const string QueueName = "b3.clearing.queue";
    private const string RoutingKey = "trade.executed";

    public RabbitMqTradeProducer(IConfiguration configuration, ILogger<RabbitMqTradeProducer> logger)
    {
        _logger = logger;

        try
        {
            var hostName = configuration["RabbitMQ:HostName"] ?? "localhost";
            var port = int.TryParse(configuration["RabbitMQ:Port"], out var p) ? p : 5672;
            var userName = configuration["RabbitMQ:UserName"] ?? "guest";
            var password = configuration["RabbitMQ:Password"] ?? "guest";

            var factory = new ConnectionFactory
            {
                HostName = hostName,
                Port = port,
                UserName = userName,
                Password = password,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declara a exchange de trading
            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);

            // Declara a fila de liquidação
            _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false);

            // Vincula a fila à exchange com a routing key
            _channel.QueueBind(queue: QueueName, exchange: ExchangeName, routingKey: RoutingKey);

            _logger.LogInformation("Conectado com sucesso ao RabbitMQ no host {Host}.", hostName);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("RabbitMQ não disponível no momento ({Message}). Mensagens serão registradas em log.", ex.Message);
        }
    }

    public Task PublishTradeExecutedAsync(TradeResponse trade, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(trade);
        var body = Encoding.UTF8.GetBytes(json);

        if (_channel is not null && _channel.IsOpen)
        {
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.Type = "TradeExecutedEvent";

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: RoutingKey,
                basicProperties: properties,
                body: body
            );

            _logger.LogInformation("Evento TradeExecuted [{Id}] para {Ticker} publicado no RabbitMQ.", trade.Id, trade.Ticker);
        }
        else
        {
            _logger.LogInformation("[Simulação RabbitMQ Offline] Evento de trade despachado: {Json}", json);
        }

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
    }
}
