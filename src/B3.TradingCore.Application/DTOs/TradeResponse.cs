using B3.TradingCore.Domain.Entities;
using B3.TradingCore.Domain.Enums;

namespace B3.TradingCore.Application.DTOs;

/// <summary>
/// DTO de saída que representa a operação confirmada e enviada para compensação.
/// </summary>
public record TradeResponse(
    Guid Id,
    string AccountId,
    string Ticker,
    MarketType MarketType,
    OrderSide Side,
    decimal Price,
    int Quantity,
    decimal TotalAmount,
    TradeStatus Status,
    DateTimeOffset ExecutedAt,
    DateTimeOffset SettlementDate
)
{
    public static TradeResponse FromEntity(Trade trade) =>
        new(
            trade.Id,
            trade.AccountId,
            trade.Ticker,
            trade.MarketType,
            trade.Side,
            trade.Price,
            trade.Quantity,
            trade.TotalAmount,
            trade.Status,
            trade.ExecutedAt,
            trade.SettlementDate
        );
}
