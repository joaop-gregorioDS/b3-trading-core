using B3.TradingCore.Application.DTOs;

namespace B3.TradingCore.Application.Interfaces;

/// <summary>
/// Contrato para publicação assíncrona de eventos na mensageria (RabbitMQ).
/// Permite o desacoplamento entre o pregão e o motor de liquidação (Clearing).
/// </summary>
public interface ITradeMessageProducer
{
    Task PublishTradeExecutedAsync(TradeResponse trade, CancellationToken cancellationToken = default);
}
