docker build -t trading-system-server -f ./TradingSystem.Server/Dockerfile .
docker build -t trading-system-web ./TradingSystem.Web/
docker rmi $(docker images | grep "^<none>" | awk "{print $3}")
