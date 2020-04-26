import { Component, OnInit, ViewChild } from '@angular/core';
import { MatTable } from '@angular/material/table';
import { SignalRService, Trade } from '../common/signalr.service';

@Component({
  selector: 'app-last-trades',
  templateUrl: './last-trades.component.html',
  styleUrls: ['./last-trades.component.css']
})
export class LastTradesComponent implements OnInit {
  @ViewChild(MatTable) table: MatTable<Trade>;
  signalRService: SignalRService;
  displayedColumns = ['price', 'executedSize'];
  timeout: NodeJS.Timeout;

  constructor() {
    this.signalRService = new SignalRService();
  }

  ngOnInit() {
  }

  renderTable = () => {
    this.table.dataSource = this.signalRService.lastTrades;
    this.table.renderRows();
  }

  public symbolSelected = (symbol: string) => {
    if (this.timeout) {
      clearInterval(this.timeout);
    }
    this.signalRService.startConnection(symbol);
    this.timeout = setInterval(this.renderTable, 1000);
  }
}
