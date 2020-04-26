import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { DashboardComponent } from './dashboard/dashboard.component';
import { SymbolsComponent } from './symbols/symbols.component';
import { CreateOrderComponent } from './create-order/create-order.component';
import { OrderBookComponent } from './order-book/order-book.component';
import { LastTradesComponent } from './last-trades/last-trades.component';
import { TradersComponent } from './traders/traders.component';

const routes: Routes = [
  {
    path: '',
    component: DashboardComponent,
    data: {
      title: 'Dashboard'
    }
  },
  {
    path: 'symbols',
    component: SymbolsComponent,
    data: {
      title: 'Symbols'
    }
  },
  {
    path: 'new-order',
    component: CreateOrderComponent,
    data: {
      title: 'New Order'
    }
  },
  {
    path: 'order-book',
    component: OrderBookComponent,
    data: {
      title: 'Order Book'
    }
  },
  {
    path: 'last-trades',
    component: LastTradesComponent,
    data: {
      title: 'Last Trades'
    }
  },
  {
    path: 'login-simulation',
    component: TradersComponent,
    data: {
      title: 'Login Simulation'
    }
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes, { useHash: true })],
  exports: [RouterModule]
})
export class AppRoutingModule { }
