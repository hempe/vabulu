import { Component, Input, OnInit, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { numberWithSeperator, toNumber } from '../../common/helper';

import { CustomErrorStateMatcher } from '../../services/custom-error-state-matcher';
import { ErrorStateMatcher } from '@angular/material';
import { Router } from '@angular/router';
import { retry } from 'rxjs/operators/retry';

const noop = () => {};
const inputTypes = [
    'button',
    'checkbox',
    'color',
    'date',
    'datetime-local',
    'file',
    'hidden',
    'image',
    'month',
    'number',
    'password',
    'radio',
    'range',
    'reset',
    'search',
    'submit',
    'tel',
    'text',
    'time',
    'url',
    'week'
];
export const CUSTOM_INPUT_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => FlatFieldComponent),
    multi: true
};

@Component({
    selector: 'flat-field',
    templateUrl: './flat-field.component.html',
    styleUrls: ['./flat-field.component.css'],
    providers: [CUSTOM_INPUT_CONTROL_VALUE_ACCESSOR]
})
export class FlatFieldComponent implements ControlValueAccessor, OnInit {
    ngOnInit(): void {
        this.matcher.name = this.name;
    }

    //The internal data model
    private innerValue: any = '';

    //Placeholders for the callbacks which are later provided
    //by the Control Value Accessor
    private onTouchedCallback: () => void = noop;
    private onChangeCallback: (_: any) => void = noop;

    constructor(private router: Router) {}

    //get accessor
    get value(): any {
        return this.innerValue;
    }

    //set accessor including call the onchange callback
    set value(v: any) {
        if (v !== this.innerValue) {
            this.innerValue = v;
            if (this.type == 'decimal') {
                v = toNumber(v);
            }
            this.onChangeCallback(v);
        }
    }

    //Set touched on blur
    onBlur() {
        if (this.type === 'decimal') {
            this.innerValue = numberWithSeperator(this.innerValue);
        }
        this.onTouchedCallback();
    }

    //From ControlValueAccessor interface
    writeValue(value: any) {
        if (this.type === 'decimal') {
            value = numberWithSeperator(value);
            if (value !== this.innerValue) {
                this.innerValue = value;
            }
        } else {
            if (value !== this.innerValue) {
                this.innerValue = value;
            }
        }
    }

    //From ControlValueAccessor interface
    registerOnChange(fn: any) {
        this.onChangeCallback = fn;
    }

    //From ControlValueAccessor interface
    registerOnTouched(fn: any) {
        this.onTouchedCallback = fn;
    }

    @Input() public icon: string;
    @Input() public name: string;
    @Input() public type: string;

    public matcher: CustomErrorStateMatcher = new CustomErrorStateMatcher();

    private isInputType(type: string): string {
        if (inputTypes.indexOf(type) >= 0) return type;
    }
}
