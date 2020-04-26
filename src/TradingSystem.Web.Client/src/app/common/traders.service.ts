import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class TradersService {

  constructor() { }

  // from some API
  public tradersAPI = [
    { id: 1, label: 'Bob' },
    { id: 2, label: 'John' },
    { id: 3, label: 'Alice' }
  ];

  // indexed for fast access
  public traders = {
    1: 'Bob',
    2: 'John',
    3: 'Alice'
  };
}
