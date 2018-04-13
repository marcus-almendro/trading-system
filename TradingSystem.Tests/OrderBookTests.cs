using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TradingSystem.Core;

namespace TradingSystem.Tests
{
    [TestClass]
    public class OrderBookTests
    {
        List<Order> _added, _updated, _deleted;
        List<Trade> _matched;
        OrderBook _orderBook;

        [TestInitialize]
        public void Init()
        {
            _added = new List<Order>();
            _updated = new List<Order>();
            _deleted = new List<Order>();
            _matched = new List<Trade>();
            _orderBook = new OrderBook();
            _orderBook.OnMessage += HandleMessage;
        }

        [TestMethod]
        public void TotalMatch()
        {
            _orderBook.Enter(new Order(OrderEntryType.Add, 0, Side.Buy, 100, 100, 0, 0));
            _orderBook.Enter(new Order(OrderEntryType.Add, 1, Side.Sell, 100, 100, 1, 0));

            Assert.AreEqual(_added[0], new Order(OrderEntryType.Add, 0, Side.Buy, 100, 100, 0, 0));
            Assert.AreEqual(_deleted[0], new Order(OrderEntryType.Delete, 0, Side.Buy, 100, 100, 0, 0));
            Assert.AreEqual(_matched[0], new Trade(1, 0, 1, Side.Sell, 100, 100, 0, 0, 1));
        }

        [TestMethod]
        public void PartialMatch()
        {
            _orderBook.Enter(new Order(OrderEntryType.Add, 0, Side.Buy, 100, 100, 0, 0));
            _orderBook.Enter(new Order(OrderEntryType.Add, 1, Side.Sell, 90, 50, 1, 0));

            Assert.AreEqual(_added[0], new Order(OrderEntryType.Add, 0, Side.Buy, 100, 100, 0, 0));
            Assert.AreEqual(_updated[0], new Order(OrderEntryType.Update, 0, Side.Buy, 100, 50, 0, 0));
            Assert.AreEqual(_matched[0], new Trade(1, 0, 1, Side.Sell, 100, 50, 50, 0, 1));
        }

        [TestMethod]
        public void OverMatchAfterUpdating()
        {
            _orderBook.Enter(new Order(OrderEntryType.Add, 0, Side.Buy, 100, 100, 0, 0));
            _orderBook.Enter(new Order(OrderEntryType.Update, 0, Side.Buy, 100, 50, 0, 0));
            _orderBook.Enter(new Order(OrderEntryType.Add, 1, Side.Sell, 90, 100, 1, 0));

            Assert.AreEqual(_added[0], new Order(OrderEntryType.Add, 0, Side.Buy, 100, 100, 0, 0));
            Assert.AreEqual(_updated[0], new Order(OrderEntryType.Update, 0, Side.Buy, 100, 50, 0, 0));
            Assert.AreEqual(_deleted[0], new Order(OrderEntryType.Delete, 0, Side.Buy, 100, 50, 0, 0));
            Assert.AreEqual(_added[1], new Order(OrderEntryType.Add, 1, Side.Sell, 90, 50, 1, 0));
            Assert.AreEqual(_matched[0], new Trade(1, 0, 1, Side.Sell, 100, 50, 0, 0, 1));
        }

        private void HandleMessage(object sender, Message m)
        {
            if (m.Type == MessageType.Order)
            {
                switch (m.Order.Action)
                {
                    case OrderEntryType.Add:
                        _added.Add(m.Order);
                        break;
                    case OrderEntryType.Update:
                        _updated.Add(m.Order);
                        break;
                    case OrderEntryType.Delete:
                        _deleted.Add(m.Order);
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                _matched.Add(m.Trade);
            }
        }
    }
}
