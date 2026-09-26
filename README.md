# B3 TradingCore · Vortex Labs

[![Live Demo](https://img.shields.io/badge/Live%20Demo-b3.vortexsoftware.tech-0A84FF?style=for-the-badge&logo=google-cloud&logoColor=white)](https://b3.vortexsoftware.tech)
![.NET 8](https://img.shields.io/badge/.NET%208-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![C# 12](https://img.shields.io/badge/C%23%2012-239120?style=for-the-badge&logo=csharp&logoColor=white)
![PostgreSQL 16](https://img.shields.io/badge/PostgreSQL%2016-336791?style=for-the-badge&logo=postgresql&logoColor=white)
![Redis 7](https://img.shields.io/badge/Redis%207-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ%203-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)

Motor institucional de pós-negociação, liquidação D+2 e clearing de ordens da B3 em **C# e .NET 8** com Clean Architecture, PostgreSQL, Redis e RabbitMQ. Projeto em produção integrado ao ecossistema de inovação **Vortex Labs**.

---

### 🌐 Swagger OpenAPI Interativo em Produção

Acesse a documentação interativa, explore os endpoints e execute operações em tempo real diretamente na infraestrutura da **Vortex Labs**:

👉 **[https://b3.vortexsoftware.tech](https://b3.vortexsoftware.tech)**

> **Escopo:** Este projeto é uma simulação técnica institucional para demonstração de engenharia de software financeiro de alta performance e resiliência. Não é um sistema oficial da B3 S.A. nem se conecta diretamente ao pregão em tempo real.

---

## O que a aplicação demonstra

- **Validação e Execução de Trades:** Recebe e valida operações de compra e venda (`Equities`, `Futures`, `Options`) via API REST assíncrona.
- **Cálculo Financeiro e Clearing:** Cálculo automático de volume financeiro, validações de preço e agendamento de liquidação em D+2 (`SettlementDate`).
- **Persistência Relacional:** Persistência no PostgreSQL 16 com Entity Framework Core e índices relacionais por conta e ticker.
- **Cache de Baixa Latência:** Cache de cotações em tempo real e consolidação de volume no Redis 7.
- **Mensageria Orientada a Eventos:** Publicação de eventos `TradeExecutedEvent` em fila assíncrona RabbitMQ para processamento desacoplado de compensação.
- **Documentação Interativa:** Swagger UI institucional customizado para o ecossistema Vortex Labs na raiz do serviço (`/`).

---

## Arquitetura de Microsserviço B3

```text
Client / Swagger UI (HTTPS)
           │
           ▼
Host Nginx Proxy (b3.vortexsoftware.tech)
           │ (Loopback :5050)
           ▼
ASP.NET Core 8 API ─── Application Services
                             │
              ┌──────────────┼─────────────┐
              ▼              ▼             ▼
       PostgreSQL 16      Redis 7       RabbitMQ 3
      (Trades / Pos)    (Cotações)    (Eventos Clearing)
```

- `B3.TradingCore.Domain`: Entidade `Trade`, enums (`MarketType`, `OrderSide`, `TradeStatus`) e contratos de repositório.
- `B3.TradingCore.Application`: Serviços de orquestração de negócios, DTOs e validações.
- `B3.TradingCore.Infrastructure`: Camada de persistência (EF Core), cache de baixa latência (StackExchange.Redis) e mensageria (RabbitMQ.Client).
- `B3.TradingCore.Api`: Controladores HTTP, OpenAPI/Swagger customizado e injeção de dependências.

---

## Endpoints Disponíveis

| Método | Rota | Descrição |
|---|---|---|
| `POST` | `/api/trades` | Valida, persiste no PostgreSQL e publica evento no RabbitMQ. |
| `GET` | `/api/trades/{id}` | Consulta detalhes e status de um trade por UUID. |
| `GET` | `/api/trades/account/{accountId}` | Lista histórico de operações de uma conta (ex: `CC-98765-VORTEX`). |
| `GET` | `/api/trades/quote/{ticker}` | Consulta cotação recente em cache Redis (ex: `PETR4`, `VALE3`, `ITUB4`, `BBAS3`). |

---

### Exemplo de Requisição via cURL (Produção)

```bash
# Consultar cotação em tempo real da Petrobras (PETR4)
curl -s https://b3.vortexsoftware.tech/api/trades/quote/PETR4

# Consultar histórico de trades da conta demo Vortex
curl -s https://b3.vortexsoftware.tech/api/trades/account/CC-98765-VORTEX
```

---

## Executar Localmente

**Pré-requisitos:** Docker Compose e .NET 8 SDK.

1. Inicie PostgreSQL, Redis e RabbitMQ:
   ```bash
   docker compose up -d
   ```

2. Inicie a API .NET:
   ```bash
   dotnet run --project src/B3.TradingCore.Api/B3.TradingCore.Api.csproj
   ```

3. Acesse o Swagger local em: `http://localhost:5000`

---

## Limites e Considerações do Protótipo

- O cálculo de `SettlementDate` soma dias corridos (`AddDays`); calendário de feriados bancários e dias úteis da B3 pode ser integrado em versão futura.
- O serviço publica no RabbitMQ; consumidores dedicados de pós-clearing podem ser plugados via filas AMQP.
- Resiliência com fallback automático para execução mesmo em caso de latência momentânea de mensageria.

---

## Licença

MIT © [João Paulo Gregório](https://github.com/joaop-gregorioDS) · [Vortex Software](https://vortexsoftware.tech)
