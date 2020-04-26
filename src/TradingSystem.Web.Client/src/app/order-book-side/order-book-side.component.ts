import { Component, OnInit, ViewChild, Input } from '@angular/core';
import { Order } from '../order-book/order-book.component';
import { MatTable } from '@angular/material/table';

@Component({
  selector: 'app-order-book-side',
  templateUrl: './order-book-side.component.html',
  styleUrls: ['./order-book-side.component.css']
})
export class OrderBookSideComponent implements OnInit {
  @ViewChild(MatTable) table: MatTable<Order>;
  @Input() displayedColumns: string[] = [];
  @Input() headerName = 'asdf';

  constructor() { }

  ngOnInit() {
  }

  set dataSource(dataSource: Order[]) {
    this.table.dataSource = dataSource;
    this.table.renderRows();
  }
}
