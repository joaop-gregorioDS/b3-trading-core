using B3.TradingCore.Domain.Entities;
using B3.TradingCore.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace B3.TradingCore.Infrastructure.Persistence;

/// <summary>
/// Implementação do repositório de Trades usando Entity Framework Core e PostgreSQL.
/// </summary>
public class TradeRepository : ITradeRepository
{
    private readonly TradingDbContext _context;

    public TradeRepository(TradingDbContext context)
    {
        _context = context;
    }

    public async Task<Trade> AddAsync(Trade trade, CancellationToken cancellationToken = default)
    {
        await _context.Trades.AddAsync(trade, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return trade;
    }

    public async Task<Trade?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Trades
            .AsNoTracking() // Não rastreia para máxima performance de leitura
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Trade>> GetByAccountAsync(string accountId, CancellationToken cancellationToken = default)
    {
        var normalizedAccount = accountId.Trim().ToUpperInvariant();
        return await _context.Trades
            .AsNoTracking()
            .Where(t => t.AccountId == normalizedAccount)
            .OrderByDescending(t => t.ExecutedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(Trade trade, CancellationToken cancellationToken = default)
    {
        _context.Trades.Update(trade);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
