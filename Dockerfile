# Estágio 1: Build da aplicação .NET 8
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copia a solution e os arquivos de projeto para cache eficiente de dependências
COPY B3.TradingCore.sln .
COPY src/B3.TradingCore.Domain/B3.TradingCore.Domain.csproj src/B3.TradingCore.Domain/
COPY src/B3.TradingCore.Application/B3.TradingCore.Application.csproj src/B3.TradingCore.Application/
COPY src/B3.TradingCore.Infrastructure/B3.TradingCore.Infrastructure.csproj src/B3.TradingCore.Infrastructure/
COPY src/B3.TradingCore.Api/B3.TradingCore.Api.csproj src/B3.TradingCore.Api/

RUN dotnet restore B3.TradingCore.sln

# Copia todo o código-fonte e compila os binários em Release
COPY src/ src/
RUN dotnet publish src/B3.TradingCore.Api/B3.TradingCore.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

# Estágio 2: Runtime enxuto e seguro (ASP.NET 8 Alpine)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS runtime
WORKDIR /app

# Instala curl para healthcheck
RUN apk add --no-cache curl

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5000

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD curl -f http://localhost:5000/swagger/index.html || exit 1

ENTRYPOINT ["dotnet", "B3.TradingCore.Api.dll"]
