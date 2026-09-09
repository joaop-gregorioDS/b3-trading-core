using B3.TradingCore.Application.DTOs;
using B3.TradingCore.Application.Interfaces;
using B3.TradingCore.Domain.Entities;
using B3.TradingCore.Domain.Interfaces;

namespace B3.TradingCore.Application.Services;

/// <summary>
/// Orquestrador de casos de uso de trading.
/// Recebe as ordens, valida regras B3, persiste no banco, atualiza o Redis e despacha para o RabbitMQ.
/// </summary>
public class TradingService : ITradingService
{
    private readonly ITradeRepository _tradeRepository;
    private readonly ITradingCacheService _cacheService;
    private readonly ITradeMessageProducer _messageProducer;

    public TradingService(
        ITradeRepository tradeRepository,
        ITradingCacheService cacheService,
        ITradeMessageProducer messageProducer)
    {
        _tradeRepository = tradeRepository;
        _cacheService = cacheService;
        _messageProducer = messageProducer;
    }

    public async Task<TradeResponse> ExecuteTradeAsync(CreateTradeRequest request, CancellationToken cancellationToken = default)
    {
        // 1. Cria a entidade no Domínio (onde as regras e invariantes são validadas)
        var trade = new Trade(
            request.AccountId,
            request.Ticker,
            request.MarketType,
            request.Side,
            request.Price,
            request.Quantity,
            DateTimeOffset.UtcNow
        );

        // 2. Persistência transacional no banco de dados (PostgreSQL)
        var savedTrade = await _tradeRepository.AddAsync(trade, cancellationToken);
        var response = TradeResponse.FromEntity(savedTrade);

        // 3. Atualização de cache no Redis (Cotação mais recente e volume financeiro)
        await _cacheService.CacheLatestTradeAsync(response, cancellationToken);
        await _cacheService.IncrementDailyVolumeAsync(response.Ticker, response.TotalAmount, cancellationToken);

        // 4. Publicação assíncrona do evento na mensageria (RabbitMQ) para o motor de Clearing
        await _messageProducer.PublishTradeExecutedAsync(response, cancellationToken);

        return response;
    }

    public async Task<TradeResponse?> GetTradeByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var trade = await _tradeRepository.GetByIdAsync(id, cancellationToken);
        return trade is null ? null : TradeResponse.FromEntity(trade);
    }

    public async Task<IReadOnlyList<TradeResponse>> GetTradesByAccountAsync(string accountId, CancellationToken cancellationToken = default)
    {
        var trades = await _tradeRepository.GetByAccountAsync(accountId, cancellationToken);
        return trades.Select(TradeResponse.FromEntity).ToList();
    }

    public async Task<TradeResponse?> GetLatestTickerQuoteAsync(string ticker, CancellationToken cancellationToken = default)
    {
        // Busca primeiro no Redis (baixa latência)
        return await _cacheService.GetLatestTradeAsync(ticker, cancellationToken);
    }
}
