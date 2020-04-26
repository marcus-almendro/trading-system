FROM node:13-alpine AS build-ng
WORKDIR /app

COPY ./TradingSystem.Web.Client/ ./
RUN npm install -g @angular/cli
RUN npm install
RUN ng build --prod

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build-env
WORKDIR /app

COPY ./TradingSystem.Application ./TradingSystem.Application
COPY ./TradingSystem.Domain ./TradingSystem.Domain
COPY ./TradingSystem.Infrastructure ./TradingSystem.Infrastructure
COPY ./TradingSystem.WebApp ./TradingSystem.WebApp
WORKDIR /app/TradingSystem.WebApp/
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
WORKDIR /app
COPY --from=build-ng /app/dist/trading-system-web-client/ ./dist
COPY --from=build-env /app/TradingSystem.WebApp/out ./
ENTRYPOINT ["dotnet", "TradingSystem.WebApp.dll"]
