import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { SignalRService } from '../common/signalr.service';
import { OrderBookSideComponent } from '../order-book-side/order-book-side.component';
import { TradersService } from '../common/traders.service';

@Component({
  selector: 'app-order-book',
  templateUrl: './order-book.component.html',
  styleUrls: ['./order-book.component.css']
})
export class OrderBookComponent implements OnInit, AfterViewInit {
  @ViewChild('buyTable') buyTable: OrderBookSideComponent;
  @ViewChild('sellTable') sellTable: OrderBookSideComponent;
  buyColumns = ['orderId', 'trader', 'size', 'price'];
  sellColumns = ['price', 'size', 'trader', 'orderId'];
  signalRService: SignalRService;

  constructor(private tradersService: TradersService) {
    this.signalRService = new SignalRService();
  }

  ngOnInit() {
  }

  ngAfterViewInit() {
    setInterval(this.renderTables, 1000);
  }

  public symbolSelected = (symbol: string) => {
    this.signalRService.startConnection(symbol);
  }

  renderTables = () => {
    this.buyTable.dataSource = this.getBuyOrders();
    this.sellTable.dataSource = this.getSellOrders();
  }

  getBuyOrders = () => {
    const orders: Order[] = [];
    Object.keys(this.signalRService.buySide).slice().reverse().forEach(key => {
      Object.keys(this.signalRService.buySide[key]).forEach(key2 => {
        const oc = this.signalRService.buySide[key][key2];
        orders.push({ orderId: oc.id, size: oc.size, price: oc.price, trader: this.tradersService.traders[oc.traderId] });
      });
    });
    return orders;
  }

  getSellOrders = () => {
    const orders: Order[] = [];
    Object.keys(this.signalRService.sellSide).forEach(key => {
      Object.keys(this.signalRService.sellSide[key]).forEach(key2 => {
        const oc = this.signalRService.sellSide[key][key2];
        orders.push({ orderId: oc.id, size: oc.size, price: oc.price, trader: this.tradersService.traders[oc.traderId] });
      });
    });
    return orders;
  }
}

export interface Order {
  orderId: number;
  size: number;
  price: number;
  trader: string;
}
