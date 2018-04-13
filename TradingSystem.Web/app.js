var express = require('express');
var grpc = require('grpc');
var app = express();
var http = require('http').Server(app);
var io = require('socket.io')(http);
var kafka = require('kafka-node'),
  Consumer = kafka.Consumer,
  client = new kafka.Client(),
  consumer = new Consumer(
    client,
    [
      { topic: 'MarketData' }
    ],
    {
      groupId: 'kafka-node-group',
      autoCommit: false
    }
  );
var svc = grpc.load(__dirname + '/proto/Service.proto').TradingSystem.Core;
var apiClient = new svc.Service('localhost:23456', grpc.credentials.createInsecure());

app.use(express.static('public'));

app.get('/', function (req, res) {
  res.sendFile(__dirname + '/public/index.html');
});

consumer.on('message', function (message) {
  var msg = JSON.parse(message.value);
  msg.Id = message.offset;
  io.emit('msg', msg);
});

io.on('connection', function (socket) {
  console.log('a user connected');

  socket.on('disconnect', function () {
    console.log('user disconnected');
  });

  socket.on('new order', function (order) {
    console.log('entering order: ');
    console.log(order);
    apiClient.EnterOrder(order, function (err, data) {
      if (err) {
        socket.emit('API_ERR', err);
      } else {
        socket.emit('API_OK', data);
      }
    });
  });

  socket.on('snapshot request', function () {
    var offset = new kafka.Offset(client);
    offset.fetch([{ topic: 'MarketData', partition: 0, time: -1 }], function (err, data) {
      var latestOffset = data['MarketData']['0'][0];
      console.log("Consumer current offset: " + latestOffset);
      var msgArr = [];

      if (latestOffset == 0) {
        socket.emit('snp_msg', msgArr);
        return;
      }

      var clientTemp = new kafka.Client(),
        consumerTemp = new Consumer(
          clientTemp,
          [
            { topic: 'MarketData', offset: 0 }
          ],
          {
            groupId: 'kafka-node-group',
            autoCommit: false,
            fromOffset: true
          }
        );
      consumerTemp.on('message', function (message) {
        var msg = JSON.parse(message.value);
        msg.Id = message.offset;
        msgArr.push(msg);
        if (msg.Id == latestOffset - 1) {
          socket.emit('snp_msg', msgArr);
          consumerTemp.close();
        }
      });

    });
  });
});

http.listen(5000, function () {
  console.log('listening on *:5000');
});
