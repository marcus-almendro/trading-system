FROM microsoft/dotnet:2.0-sdk AS build-env
WORKDIR /app

# copy all files and folder and build Server
COPY ./ ./
WORKDIR /app/TradingSystem.Server/
RUN dotnet publish -c Release -o out

# copy only the Server build to production image
FROM microsoft/dotnet:2.0-runtime 
WORKDIR /app
COPY --from=build-env /app/TradingSystem.Server/out ./
ENTRYPOINT ["dotnet", "TradingSystem.Server.dll"]
