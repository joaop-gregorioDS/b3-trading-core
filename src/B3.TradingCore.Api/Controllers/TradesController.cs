using B3.TradingCore.Application.DTOs;
using B3.TradingCore.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace B3.TradingCore.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TradesController : ControllerBase
{
    private readonly ITradingService _tradingService;
    private readonly ILogger<TradesController> _logger;

    public TradesController(ITradingService tradingService, ILogger<TradesController> logger)
    {
        _tradingService = tradingService;
        _logger = logger;
    }

    /// <summary>
    /// Registra uma nova operação executada na B3, grava no banco e despacha para Clearing via RabbitMQ.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(TradeResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExecuteTrade([FromBody] CreateTradeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _tradingService.ExecuteTradeAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Erro de validação ao processar ordem B3.");
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Consulta o detalhe de uma operação pelo seu identificador único (UUID).
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TradeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var trade = await _tradingService.GetTradeByIdAsync(id, cancellationToken);
        if (trade is null)
            return NotFound(new { message = $"Operação com ID '{id}' não foi encontrada." });

        return Ok(trade);
    }

    /// <summary>
    /// Retorna o extrato de operações executadas de uma conta do investidor.
    /// </summary>
    [HttpGet("account/{accountId}")]
    [ProducesResponseType(typeof(IEnumerable<TradeResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByAccount(string accountId, CancellationToken cancellationToken)
    {
        var trades = await _tradingService.GetTradesByAccountAsync(accountId, cancellationToken);
        return Ok(trades);
    }

    /// <summary>
    /// Obtém a cotação mais recente de um ticker da B3 via cache Redis de baixa latência.
    /// </summary>
    [HttpGet("quote/{ticker}")]
    [ProducesResponseType(typeof(TradeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetQuote(string ticker, CancellationToken cancellationToken)
    {
        var quote = await _tradingService.GetLatestTickerQuoteAsync(ticker, cancellationToken);
        if (quote is null)
            return NotFound(new { message = $"Nenhuma cotação em cache encontrada para o papel '{ticker.ToUpperInvariant()}'." });

        return Ok(quote);
    }
}
