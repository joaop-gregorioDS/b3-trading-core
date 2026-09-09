using B3.TradingCore.Domain.Enums;

namespace B3.TradingCore.Domain.Entities;

/// <summary>
/// Representa uma operação executada na B3 e pronta para compensação/liquidação.
/// </summary>
public class Trade
{
    public Guid Id { get; private set; }
    public string AccountId { get; private set; } = string.Empty;
    public string Ticker { get; private set; } = string.Empty;
    public MarketType MarketType { get; private set; }
    public OrderSide Side { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    public decimal TotalAmount => Price * Quantity;
    public TradeStatus Status { get; private set; }
    public DateTimeOffset ExecutedAt { get; private set; }
    public DateTimeOffset SettlementDate { get; private set; }

    // Construtor protegido exigido por ORMs (Entity Framework Core)
    protected Trade() { }

    public Trade(
        string accountId,
        string ticker,
        MarketType marketType,
        OrderSide side,
        decimal price,
        int quantity,
        DateTimeOffset executedAt)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("A conta do investidor é obrigatória.", nameof(accountId));

        if (string.IsNullOrWhiteSpace(ticker))
            throw new ArgumentException("O ticker do ativo na B3 é obrigatório.", nameof(ticker));

        if (price <= 0)
            throw new ArgumentOutOfRangeException(nameof(price), "O preço de execução deve ser maior que zero.");

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantity), "A quantidade negociada deve ser positiva.");

        Id = Guid.NewGuid();
        AccountId = accountId.Trim().ToUpperInvariant();
        Ticker = ticker.Trim().ToUpperInvariant();
        MarketType = marketType;
        Side = side;
        Price = price;
        Quantity = quantity;
        Status = TradeStatus.Executed;
        ExecutedAt = executedAt;

        // Regra da B3: Ações liquidam em D+2 úteis; Futuros em D+1
        SettlementDate = marketType == MarketType.Equities 
            ? executedAt.AddDays(2) 
            : executedAt.AddDays(1);
    }

    /// <summary>
    /// Marca o negócio como liquidado pela Clearing da B3.
    /// </summary>
    public void MarkAsSettled()
    {
        if (Status == TradeStatus.Cancelled)
            throw new InvalidOperationException("Uma operação cancelada não pode ser liquidada.");

        Status = TradeStatus.Settled;
    }

    /// <summary>
    /// Cancela a operação antes da liquidação final.
    /// </summary>
    public void Cancel()
    {
        if (Status == TradeStatus.Settled)
            throw new InvalidOperationException("Operações já liquidadas na Clearing não podem ser canceladas diretamente.");

        Status = TradeStatus.Cancelled;
    }
}
