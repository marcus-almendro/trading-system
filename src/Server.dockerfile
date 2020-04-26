FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

# copy all files and folder and build Server
COPY ./TradingSystem.Application ./TradingSystem.Application
COPY ./TradingSystem.Domain ./TradingSystem.Domain
COPY ./TradingSystem.Infrastructure ./TradingSystem.Infrastructure
COPY ./TradingSystem.Server ./TradingSystem.Server
WORKDIR /app/TradingSystem.Server/
RUN dotnet publish -c Release -o out

# copy only the Server build to production image
FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=build-env /app/TradingSystem.Server/out ./
ENTRYPOINT ["dotnet", "TradingSystem.Server.dll"]
