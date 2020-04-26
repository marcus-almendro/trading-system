import { Component } from '@angular/core';
import { StateService } from '../common/state.service';
import { TradersService } from '../common/traders.service';

@Component({
  selector: 'app-traders',
  templateUrl: './traders.component.html',
  styleUrls: ['./traders.component.css']
})
export class TradersComponent {

  constructor(public stateService: StateService, public traderService: TradersService) { }

}
