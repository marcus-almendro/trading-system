import { Component, OnInit, ViewChild, AfterViewInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HttpClient } from '@angular/common/http';
import { SelectSymbolComponent } from '../select-symbol/select-symbol.component';
import { StateService } from '../common/state.service';

@Component({
  selector: 'app-create-order',
  templateUrl: './create-order.component.html',
  styleUrls: ['./create-order.component.css']
})
export class CreateOrderComponent implements OnInit, AfterViewInit {
  @ViewChild(SelectSymbolComponent) selectSymbolComponent: SelectSymbolComponent;
  selectedSymbol: string;
  side: string;
  price: number;
  size: number;

  constructor(private http: HttpClient, private snackBar: MatSnackBar, private stateService: StateService) { }

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.selectedSymbol = this.selectSymbolComponent.symbol;
  }

  symbolSelected = (symbol: string) => {
    this.selectedSymbol = symbol;
  }

  public sendOrder() {
    const order: NewOrder = {
      Symbol: this.selectedSymbol,
      Type: this.side === '1' ? 0 : 1,
      Price: this.price,
      Size: this.size,
      TraderId: this.stateService.traderId
    };
    this.http.post<ErrorCodeMsg>('/orders', order).subscribe(result => {
      this.showMessage(result.description);
    }, error => this.showMessage(error.message));
  }

  showMessage(message: string) {
    this.snackBar.open(message, 'OK', {
      duration: 2000,
    });
  }
}

interface ErrorCodeMsg {
  description: string;
}

interface NewOrder {
  Symbol: string;
  Type: number;
  Price: number;
  Size: number;
  TraderId: number;
}

