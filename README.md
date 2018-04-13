# Trading System Proof of Concept
This is a simple trading system based on Kafka for event sourcing, .Net Core 2 for server side processing (order book / matching end) and Socket.IO for client-side streaming.

To run this project:
1) start a local zookeeper using port 2181
2) start a local kafka node using port 9092
3) start the TradingSystem.Server using the command "dotnet run"
4) start the TradingSystem.Web using the command "node app.js" (you must "npm install" first)

Access your localhost:5000 via browser and have fun!
