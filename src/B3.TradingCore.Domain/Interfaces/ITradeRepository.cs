using B3.TradingCore.Domain.Entities;

namespace B3.TradingCore.Domain.Interfaces;

/// <summary>
/// Contrato de persistência para operações da B3 (Inversão de Dependência - SOLID).
/// O Domínio define a necessidade, a Infraestrutura implementa a tecnologia (PostgreSQL/EF Core).
/// </summary>
public interface ITradeRepository
{
    Task<Trade> AddAsync(Trade trade, CancellationToken cancellationToken = default);
    Task<Trade?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Trade>> GetByAccountAsync(string accountId, CancellationToken cancellationToken = default);
    Task UpdateAsync(Trade trade, CancellationToken cancellationToken = default);
}
