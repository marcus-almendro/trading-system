import { Component, OnInit, Output } from '@angular/core';
import { SymbolsService } from '../common/symbols.service';
import { EventEmitter } from '@angular/core';

@Component({
  selector: 'app-select-symbol',
  templateUrl: './select-symbol.component.html',
  styleUrls: ['./select-symbol.component.css']
})
export class SelectSymbolComponent implements OnInit {

  @Output() symbolSelected = new EventEmitter();

  symbol: string;

  constructor(public symbolsService: SymbolsService) { }

  ngOnInit(): void {
    this.symbolsService.getSymbols();
  }

  setSymbol(symbol: string) {
    this.symbol = symbol;
    this.symbolSelected.emit(symbol);
  }
}
