# B3 TradingCore

Protótipo de portfólio de uma API de negociação e pós-negociação, desenvolvido em **C# e .NET 8**. O projeto demonstra organização em camadas, validação de regras de domínio, persistência de operações e publicação de eventos.

> **Escopo:** este projeto é uma simulação técnica e não está publicado on-line. Não é um sistema da B3, não se conecta à bolsa ou a corretoras e não realiza negociação, clearing ou liquidação reais.

## O que a aplicação demonstra

- Receber e validar operações de compra e venda por uma API REST.
- Calcular valor total a partir de preço e quantidade.
- Persistir operações no PostgreSQL usando Entity Framework Core.
- Consultar trades por identificador e por conta.
- Manter cotação recente e volume diário no Redis.
- Publicar `TradeExecutedEvent` no RabbitMQ para processamento desacoplado.
- Expor documentação interativa com Swagger/OpenAPI.

## Arquitetura

```text
Client / Swagger
      │ HTTP + JSON
      ▼
ASP.NET Core API ─── Application Services
                         │
          ┌──────────────┼─────────────┐
          ▼              ▼             ▼
      PostgreSQL       Redis       RabbitMQ
      (EF Core)         cache       producer
```

- `B3.TradingCore.Domain`: entidade `Trade`, enums e contrato de repositório.
- `B3.TradingCore.Application`: serviço de aplicação, DTOs e interfaces.
- `B3.TradingCore.Infrastructure`: persistência, cache e produtor RabbitMQ.
- `B3.TradingCore.Api`: controllers, Swagger e injeção de dependências.

## Tecnologias

`C# 12` · `.NET 8` · `ASP.NET Core` · `Entity Framework Core` · `PostgreSQL 16` · `Redis 7` · `RabbitMQ 3` · `Docker Compose`

## Endpoints disponíveis

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/trades` | Valida, persiste e publica um evento para o trade. |
| `GET` | `/api/trades/{id}` | Consulta um trade pelo UUID. |
| `GET` | `/api/trades/account/{accountId}` | Lista trades de uma conta. |
| `GET` | `/api/trades/quote/{ticker}` | Consulta a cotação recente disponível no cache. |

O Swagger abre na raiz da API, normalmente em `http://localhost:5000` ou `https://localhost:5001`.

### Exemplo de criação

```json
{
  "accountId": "CC-98765-BTG",
  "ticker": "PETR4",
  "marketType": "Equities",
  "side": "Buy",
  "price": 38.50,
  "quantity": 100
}
```

## Executar localmente

Pré-requisitos: Docker Compose e .NET 8 SDK.

1. Inicie PostgreSQL, Redis e RabbitMQ:

   ```bash
   docker compose up -d
   ```

2. Inicie a API:

   ```bash
   dotnet run --project src/B3.TradingCore.Api/B3.TradingCore.Api.csproj
   ```

O Compose local usa credenciais de desenvolvimento definidas no próprio arquivo; não reutilize essa configuração em um ambiente público.

## Limites atuais do protótipo

- O cálculo de `SettlementDate` soma dias corridos (`AddDays`); calendário de feriados e dias úteis não está implementado.
- O serviço publica no RabbitMQ, mas este repositório não inclui um consumidor de Clearing.
- Não há idempotência de criação nem pipeline de testes automatizados versionado neste repositório.
- O Redis e o RabbitMQ têm caminhos de fallback para execução quando os serviços não estão disponíveis; isso serve à demonstração, não define garantias de produção.

## Licença

MIT. Consulte [`LICENSE`](LICENSE).
