import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class SymbolsService {

  public symbols: SymbolName[];

  constructor(private http: HttpClient) { }

  public getSymbols = (next?: (value: SymbolName[]) => void, forceReload?: boolean) => {
    if (!this.symbols || forceReload && forceReload.valueOf()) {
      this.http.get<string[]>('/orderbooks').subscribe(result => {
        this.symbols = result.map<SymbolName>(r => ({ name: r }));
        if (next) {
          next(this.symbols);
        }
      }, error => console.error(error));
    }

    if (next) {
      next(this.symbols);
    }
  }

  public putSymbol = (symbol: SymbolName, next: (value: ErrorCodeMsg) => void, error: (value: any) => void) => {
    this.http.post<ErrorCodeMsg>('/orderbooks', { Symbol: symbol }).subscribe((value) => {
      this.getSymbols(() => { }, true);
      next(value);
    }, error);
  }

}

export interface SymbolName {
  name: string;
}

interface ErrorCodeMsg {
  description: string;
}
