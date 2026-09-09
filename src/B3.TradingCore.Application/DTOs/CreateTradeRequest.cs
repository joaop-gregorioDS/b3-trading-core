using B3.TradingCore.Domain.Enums;

namespace B3.TradingCore.Application.DTOs;

/// <summary>
/// DTO de entrada para registrar uma operação da B3 vinda da boleta/sistema externo.
/// Usamos 'record' no C# por ser imutável e leve para transferência de dados.
/// </summary>
public record CreateTradeRequest(
    string AccountId,
    string Ticker,
    MarketType MarketType,
    OrderSide Side,
    decimal Price,
    int Quantity
);
