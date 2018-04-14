echo stoping all containers...
docker stop $(docker ps -a -q)

echo removing containers...
docker rm $(docker ps -a -q)

echo starting zookeeper...
docker run -d \
    --net=host \
    -e ZOOKEEPER_CLIENT_PORT=2181 \
    confluentinc/cp-zookeeper:4.0.0
sleep 3

echo starting kafka...
docker run -d \
    --net=host \
    -e KAFKA_ZOOKEEPER_CONNECT=localhost:2181 \
    -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://localhost:9092 \
    -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 \
    confluentinc/cp-kafka:4.0.0
sleep 3

echo starting server...
docker run -d --net=host trading-system-server
sleep 10

echo starting web...
docker run -d --net=host trading-system-web

echo ok!
