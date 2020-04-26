import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { LayoutModule } from '@angular/cdk/layout';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { DashboardComponent } from './dashboard/dashboard.component';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatCardModule } from '@angular/material/card';
import { MatMenuModule } from '@angular/material/menu';
import { MatInputModule } from '@angular/material/input';
import { SymbolsComponent, DialogAddNewSymbolComponent } from './symbols/symbols.component';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule } from '@angular/material/paginator';
import { MatSortModule } from '@angular/material/sort';
import { GridsterModule } from 'angular-gridster2';
import { TradersComponent } from './traders/traders.component';
import { StateService } from './common/state.service';
import { SignalRService } from './common/signalr.service';
import { HttpClientModule } from '@angular/common/http';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { LastTradesComponent } from './last-trades/last-trades.component';
import { OrderBookComponent } from './order-book/order-book.component';
import { OrderBookSideComponent } from './order-book-side/order-book-side.component';
import { TradersService } from './common/traders.service';
import { MatDialogModule } from '@angular/material/dialog';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { FormsModule } from '@angular/forms';
import { CreateOrderComponent } from './create-order/create-order.component';
import { SelectSymbolComponent } from './select-symbol/select-symbol.component';
import { SymbolsService } from './common/symbols.service';
import { MatRadioModule } from '@angular/material/radio';

@NgModule({
  declarations: [
    AppComponent,
    DashboardComponent,
    SymbolsComponent,
    TradersComponent,
    LastTradesComponent,
    OrderBookComponent,
    OrderBookSideComponent,
    DialogAddNewSymbolComponent,
    CreateOrderComponent,
    SelectSymbolComponent
  ],
  imports: [
    CommonModule,
    BrowserModule,
    HttpClientModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    LayoutModule,
    MatToolbarModule,
    MatButtonModule,
    MatSidenavModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatListModule,
    MatInputModule,
    MatGridListModule,
    MatCardModule,
    MatMenuModule,
    MatSnackBarModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatRadioModule,
    MatDialogModule,
    GridsterModule,
    FormsModule
  ],
  providers: [StateService, TradersService, SignalRService, SymbolsService],
  bootstrap: [AppComponent]
})
export class AppModule { }
