import { AfterViewInit, Component, OnInit, ViewChild, Inject } from '@angular/core';
import { MatTable } from '@angular/material/table';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { SymbolsService, SymbolName } from '../common/symbols.service';

export interface DialogNewSymbol {
  name: string;
}

@Component({
  selector: 'app-dialog-add-new-symbol',
  templateUrl: 'dialog-add-new-symbol.html',
})
export class DialogAddNewSymbolComponent {

  constructor(
    public dialogRef: MatDialogRef<DialogAddNewSymbolComponent>,
    @Inject(MAT_DIALOG_DATA) public data: DialogNewSymbol) { }

  onNoClick(): void {
    this.dialogRef.close();
  }
}

@Component({
  selector: 'app-symbols',
  templateUrl: './symbols.component.html',
  styleUrls: ['./symbols.component.css']
})
export class SymbolsComponent implements AfterViewInit, OnInit {
  @ViewChild(MatTable) table: MatTable<SymbolName>;
  displayedColumns = ['symbol'];
  errorMsg: string;
  symbol: SymbolName;

  constructor(private symbolsService: SymbolsService, public dialog: MatDialog, private snackBar: MatSnackBar) { }

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.reload();
  }

  reload() {
    this.symbolsService.getSymbols(symbols => {
      this.table.dataSource = symbols;
    }, true);
  }

  openDialog() {
    const dialogRef = this.dialog.open(DialogAddNewSymbolComponent, {
      width: '250px',
      data: { name: this.symbol }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.sendOrderBook(result);
      }
    });
  }

  sendOrderBook(symbol: SymbolName) {
    this.symbolsService.putSymbol(symbol, (result) => {
      this.showMessage(result.description);
      this.reload();
    }, error => this.showMessage(error.message));
  }

  showMessage(message: string) {
    this.snackBar.open(message, 'OK', {
      duration: 2000,
    });
  }
}

