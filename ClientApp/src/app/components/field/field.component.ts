import {
    Component,
    EventEmitter,
    Input,
    OnInit,
    Output,
    forwardRef
} from '@angular/core';
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
    useExisting: forwardRef(() => FieldComponent),
    multi: true
};

@Component({
    selector: 'field',
    templateUrl: './field.component.html',
    styleUrls: ['./field.component.css'],
    providers: [CUSTOM_INPUT_CONTROL_VALUE_ACCESSOR]
})
export class FieldComponent implements ControlValueAccessor, OnInit {
    ngOnInit(): void {
        this.matcher.name = this.name;
    }

    public ace = {
        options: {
            maxLines: 1000,
            printMargin: false,
            showInvisibles: false,
            displayIndentGuides: true,
            showGutter: false
        }
    };

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
            this.change.emit(v);
        }
    }

    get text(): string {
        return JSON.stringify(this.innerValue, null, '\t');
    }

    set text(v: string) {
        if (v !== this.innerValue) {
            try {
                let ob = JSON.parse(v);
                if (ob) {
                    this.innerValue = ob;
                    this.onChangeCallback(ob);
                    this.change.emit(ob);
                }
            } catch (e) {}
        }
    }

    //Set touched on blur
    onBlur() {
        if (this.type === 'decimal') {
            this.innerValue = numberWithSeperator(this.innerValue);
        }
        this.onTouchedCallback();
        this.blur.emit();
    }

    onFocus() {
        this.focus.emit();
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

    @Output() public blur: EventEmitter<{}> = new EventEmitter();
    @Output() public focus: EventEmitter<{}> = new EventEmitter();
    @Output() public change: EventEmitter<any> = new EventEmitter();

    @Input() public min: string;
    @Input() public max: string;
    @Input() public icon: string;
    @Input() public name: string;
    @Input() public type: string;
    @Input() public options: { name: string; value: string }[];
    @Input() public color: string = 'inherit';

    public matcher: CustomErrorStateMatcher = new CustomErrorStateMatcher();

    private isInputType(type: string): string {
        if (inputTypes.indexOf(type) >= 0) return type;
    }
}
