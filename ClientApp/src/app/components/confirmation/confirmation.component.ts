import { Component, Inject, Injectable } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogRef } from '@angular/material';

import { Observable } from 'rxjs/Observable';

@Injectable()
export class Confirmation {
    constructor(private dialog: MatDialog) {}

    public confirm(messageKey: string): Observable<boolean> {
        let dialogRef = this.dialog.open(ConfirmationDialog, {
            data: {
                messageKey: messageKey
            }
        });
        return dialogRef.afterClosed();
    }
}

@Component({
    selector: 'confirmation',
    templateUrl: 'confirmation.component.html'
})
export class ConfirmationDialog {
    constructor(
        public dialogRef: MatDialogRef<ConfirmationDialog>,
        @Inject(MAT_DIALOG_DATA) public data: any
    ) {}

    onNoClick(): void {
        this.dialogRef.close();
    }
}
