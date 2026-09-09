namespace B3.TradingCore.Domain.Enums;

/// <summary>
/// Ciclo de vida da operação na Clearing da B3.
/// </summary>
public enum TradeStatus
{
    Executed = 1,           // Negócio fechado no pregão
    PendingSettlement = 2,  // Aguardando prazo de liquidação financeira (D+2)
    Settled = 3,            // Liquidado com sucesso na Clearing
    Cancelled = 4           // Cancelado/estornado
}
