import { Injectable } from '@angular/core';
import {
    FormGroupDirective,
    NgForm,
    FormControl,
    AbstractControl,
    FormGroup,
    ValidationErrors
} from '@angular/forms';
import { ErrorStateMatcher } from '@angular/material';

/** Error state matcher that matches when a control is invalid and dirty. */
@Injectable()
export class CustomErrorStateMatcher implements ErrorStateMatcher {
    public name: string;
    static getErrors(name: string): any[] {
        if (CustomErrorStateMatcher.errors)
            return CustomErrorStateMatcher.errors[name];
    }

    static errors: any = {};
    isErrorState(
        control: FormControl | null,
        form: FormGroupDirective | NgForm | null
    ): boolean {
        let hasError = control.invalid && (control.dirty || control.touched);
        /*
        if (hasError) {
            let errors = [];
            Object.keys(control.errors).forEach(error => {
                if (control.errors[error]) errors.push(error);
            });
            console.info('errors', errors);
        }
        */
        return hasError
            ? true
            : CustomErrorStateMatcher.errors &&
                  CustomErrorStateMatcher.errors[this.name] &&
                  CustomErrorStateMatcher.errors[this.name].length > 0;
    }
}
