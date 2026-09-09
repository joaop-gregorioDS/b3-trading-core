namespace B3.TradingCore.Domain.Enums;

/// <summary>
/// Tipo de mercado na B3.
/// </summary>
public enum MarketType
{
    Equities = 1,  // Mercado à vista de Ações (ex: PETR4, VALE3)
    Futures = 2,   // Mercado Futuro / Derivativos (ex: WIN, WDO)
    Options = 3    // Mercado de Opções (ex: PETRK30)
}
