import { Directive, ElementRef, HostListener, OnInit } from '@angular/core';
import { numberWithSeperator, toNumber } from '../../common/helper';

@Directive({ selector: '[decimalFormat]' })
export class DecimalFormatDirective implements OnInit {
    private el: HTMLInputElement;

    constructor(private elementRef: ElementRef) {
        this.el = this.elementRef.nativeElement;
    }

    ngOnInit() {
        this.el.value = numberWithSeperator(this.el.value);
    }

    @HostListener('change', ['$event.target.value'])
    onChange(value) {
        debugger;
    }

    @HostListener('focus', ['$event.target.value'])
    onFocus(value) {
        this.el.value = <any>toNumber(value); // opossite of transform
    }

    @HostListener('blur', ['$event.target.value'])
    onBlur(value) {
        this.el.value = numberWithSeperator(value);
    }
}
