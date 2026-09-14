# B3.TradingCore 📈

Motor corporativo de **Pós-Negociação, Liquidação e Clearing de Ordens da B3** em **C# / .NET 8**, construído sob os princípios de **Clean Architecture**, **SOLID** e mensageria distribuída.

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4.svg?style=flat-square&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12.0-239120.svg?style=flat-square&logo=c-sharp&logoColor=white)](https://docs.microsoft.com/dotnet/csharp/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-316192.svg?style=flat-square&logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Redis](https://img.shields.io/badge/Redis-7-DC382D.svg?style=flat-square&logo=redis&logoColor=white)](https://redis.io/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3-FF6600.svg?style=flat-square&logo=rabbitmq&logoColor=white)](https://www.rabbitmq.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=flat-square)](LICENSE)

---

## 🏛️ Visão Geral da Arquitetura

O sistema atua como o backend de pós-negociação para operações executadas no pregão da B3 (Ações à vista, Mini Índice e Mini Dólar):

```
       [ Client / Simulador ]
                 │ (HTTP / JSON)
                 ▼
       ┌─────────────────────┐
       │   B3.TradingCore    │ (ASP.NET Core Web API .NET 8)
       └──────────┬──────────┘
                  │
        ┌─────────┼─────────┐
        │         │         │
        ▼         ▼         ▼
  ┌──────────┐ ┌─────┐ ┌──────────┐
  │PostgreSQL│ │Redis│ │ RabbitMQ │
  │ (Trans.) │ │(Cot)│ │(Clearing)│
  └──────────┘ └─────┘ └──────────┘
```

1. **Entrada de Ordens:** Recebe operações de compra/venda (`POST /api/trades`).
2. **Domínio B3:** Valida regras de mercado e calcula automaticamente a data de liquidação da Clearing (**D+2** para ações, **D+1** para derivativos).
3. **Persistência Transacional:** Grava o trade no **PostgreSQL** com precisão decimal (`18,2`).
4. **Cache em Memória:** Atualiza a cotação recente e acumula o volume financeiro diário no **Redis**.
5. **Mensageria Assíncrona:** Publica o evento `TradeExecutedEvent` na exchange do **RabbitMQ** para processamento desacoplado pela câmara de compensação.

---

## 📑 Endpoints da API (Swagger / OpenAPI)

A API expõe contratos RESTful documentados para integração direta com frontends e sistemas de mercado:

| Método | Rota | Descrição |
| :--- | :--- | :--- |
| `POST` | `/api/trades` | Submete uma nova ordem de compra/venda para validação e liquidação |
| `GET` | `/api/trades` | Lista o histórico consolidado de operações de pós-negociação |
| `GET` | `/api/trades/{id}` | Consulta detalhes, status de execução e data de liquidação de um trade |
| `GET` | `/api/trades/account/{accountId}` | Extrato analítico de operações por conta de custódia |

---

## 🚀 Como Executar Localmente

Caso queira executar a infraestrutura e a aplicação na sua máquina local:

### 1. Subir a Infraestrutura (PostgreSQL + Redis + RabbitMQ)
Com o Docker aberto, execute na raiz do repositório:
```bash
docker compose up -d
```
> **Serviços provisionados:**
> - **PostgreSQL:** `localhost:5432` *(Database: `b3_trading`)*
> - **Redis:** `localhost:6379`
> - **RabbitMQ Dashboard:** `http://localhost:15672` *(Credenciais: `guest` / `guest`)*

### 2. Rodar a API .NET 8
```bash
dotnet run --project src/B3.TradingCore.Api/B3.TradingCore.Api.csproj
```

Após iniciar a API, acesse a documentação interativa no navegador da sua máquina:
- **Swagger UI:** `http://localhost:5000` *(ou `https://localhost:5001`)*

---

## 🧪 Exemplo de Requisição (Compra de Ações B3)

Execute via Swagger ou cURL:

```bash
curl -X POST "http://localhost:5000/api/trades" \
     -H "Content-Type: application/json" \
     -d '{
       "accountId": "CC-98765-BTG",
       "ticker": "PETR4",
       "marketType": "Equities",
       "side": "Buy",
       "price": 38.50,
       "quantity": 100
     }'
```

**Resposta (201 Created):**
```json
{
  "id": "e7b0c95a-4712-4f8a-a82f-2d8c3653198e",
  "accountId": "CC-98765-BTG",
  "ticker": "PETR4",
  "marketType": "Equities",
  "side": "Buy",
  "price": 38.50,
  "quantity": 100,
  "totalAmount": 3850.00,
  "status": "Executed",
  "executedAt": "2026-09-09T10:55:00Z",
  "settlementDate": "2026-09-11T10:55:00Z"
}
```

---

## 📂 Estrutura de Camadas (Clean Architecture)

- **`src/B3.TradingCore.Domain`:** Entidades puras (`Trade`), Enums (`OrderSide`, `MarketType`, `TradeStatus`) e Contratos de repositório. Zero dependências externas.
- **`src/B3.TradingCore.Application`:** Casos de uso (`TradingService`), DTOs imutáveis (`CreateTradeRequest`, `TradeResponse`) e interfaces de mensageria e cache.
- **`src/B3.TradingCore.Infrastructure`:** Implementações concretas de banco (`Entity Framework Core` + `PostgreSQL`), Cache (`StackExchange.Redis`) e Mensageria (`RabbitMQ.Client`).
- **`src/B3.TradingCore.Api`:** Controllers REST, documentação Swagger/OpenAPI e injeção de dependências.

---

## ⚖️ Licença

Distribuído sob a licença MIT. Veja [`LICENSE`](LICENSE) para mais detalhes.

Desenvolvido por **João Paulo Gregório de Souza** | **Vortex Software LTDA**.
