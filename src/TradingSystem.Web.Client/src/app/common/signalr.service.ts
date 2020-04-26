import { Injectable } from '@angular/core';
import * as signalR from '@aspnet/signalr';

@Injectable()
export class SignalRService {
  hub: signalR.HubConnection;
  count = 0;
  buySide: { [price: number]: { [orderId: number]: OrderCreated } } = {};
  sellSide: { [price: number]: { [orderId: number]: OrderCreated } } = {};
  allOrders: { [orderId: number]: OrderCreated } = {};
  sync: boolean;
  tempEventsSnp: { [msgId: number]: DomainEventCollection } = {};
  tempEventsInc: { [msgId: number]: DomainEventCollection } = {};
  lastTrades: Trade[] = [];
  handlers: { [evtType: number]: (e: DomainEvent) => void };

  constructor() {
    this.handlers = {
      3: this.createOrder,
      4: this.updateOrder,
      5: this.deleteOrder,
      6: this.addTrade
    };
  }

  public startConnection = (symbol: string) => {
    this.buySide = {};
    this.sellSide = {};
    this.allOrders = {};
    this.lastTrades = [];

    if (this.hub) {
      console.log('stopping old hub');
      this.hub.stop();
    }

    const hubConnection = new signalR.HubConnectionBuilder().withUrl('/events').build();

    this.sync = true;

    hubConnection.start()
      .then(() => hubConnection.invoke('GetSnapshot', symbol).catch(err => {
        return console.error(err.toString());
      }))
      .catch(err => console.log('Error while starting connection: ' + err));

    hubConnection.on('snp_msg', (data) => {
      this.tempEventsSnp[data.messageId] = data;
    });

    hubConnection.on('snp_end', () => {
      console.log('snp end');
      this.sync = false;
      console.log('temp snp length:' + Object.keys(this.tempEventsSnp).length);
      let incMsgWhileSnp = 0;
      Object.keys(this.tempEventsSnp).forEach(t => {
        this.handleMessage(this.tempEventsSnp[t], 'snp');
        if (this.tempEventsInc[t]) {
          delete this.tempEventsInc[t];
          incMsgWhileSnp++;
        }
      });
      console.log('incMsgWhileSnp: ' + incMsgWhileSnp);
      console.log('temp inc length:' + Object.keys(this.tempEventsInc).length);
      Object.keys(this.tempEventsInc).forEach(t => this.handleMessage(this.tempEventsInc[t], 'inc'));

      this.tempEventsSnp = {};
      this.tempEventsInc = {};
    });

    hubConnection.on('inc_msg', (data) => {
      if (!this.sync) {
        this.handleMessage(data, 'inc');
      } else {
        this.tempEventsInc[data.messageId] = data;
      }
    });

    this.hub = hubConnection;
  }

  handleMessage = (e: DomainEventCollection, source: string) => {
    this.count++;
    if (source === 'inc' && (this.sync || this.count % 1000 === 0)) {
      console.log(source + ':' + e.messageId + '/' + this.count + '/' + this.sync);
    }
    e.events.forEach(ev => this.handlers[ev.eventTypeCase](ev));
  }

  createOrder = (e: DomainEvent) => {
    const bookSide = e.orderCreated.type === 0 ? this.buySide : this.sellSide;
    if (!bookSide[e.orderCreated.price]) {
      bookSide[e.orderCreated.price] = [];
    }
    bookSide[e.orderCreated.price][e.orderCreated.id] = e.orderCreated;
    this.allOrders[e.orderCreated.id] = e.orderCreated;
  }

  updateOrder = (e: DomainEvent) => {
    this.allOrders[e.orderUpdated.id].size = e.orderUpdated.size;
  }

  deleteOrder = (e: DomainEvent) => {
    const bookSide = this.allOrders[e.orderDeleted.id].type === 0 ? this.buySide : this.sellSide;
    delete bookSide[e.orderDeleted.price][e.orderDeleted.id];
    if (Object.keys(bookSide[e.orderDeleted.price]).length === 0) {
      delete bookSide[e.orderDeleted.price];
    }
    delete this.allOrders[e.orderDeleted.id];
  }

  addTrade = (e: DomainEvent) => {
    this.lastTrades.unshift(e.trade);
    if (this.lastTrades.length > 10) {
      this.lastTrades.pop();
    }
  }
}

export interface OrderCreated {
  id: number;
  price: number;
  size: number;
  traderId: number;
  type: number;
}

interface OrderUpdated {
  id: number;
  price: number;
  size: number;
  traderId: number;
  type: number;
}
interface OrderDeleted {
  id: number;
  price: number;
  size: number;
  traderId: number;
  type: number;
}
export interface Trade {
  price: number;
  executedSize: number;
}
interface DomainEvent {
  creationDate: number;
  eventTypeCase: number;
  orderCreated: OrderCreated;
  orderDeleted: OrderDeleted;
  trade: Trade;
  orderUpdated: OrderUpdated;
  symbol: string;
}
interface DomainEventCollection {
  messageId: number;
  events: DomainEvent[];
}
