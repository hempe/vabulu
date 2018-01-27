import {
    Directive,
    ElementRef,
    HostBinding,
    Input,
    Renderer
} from '@angular/core';
@Directive({
    selector: '[flexContainer]'
})
export class FlexContainerDirective {
    @HostBinding('style.display') display: string = 'flex';
    @HostBinding('style.flex-flow') flexFlow: string = 'row wrap';
}

@Directive({
    selector: '[flex]'
})
export class FlexDirective {
    @HostBinding('style.flex-basis') flexBasis: string = '100%';

    @HostBinding('style.flex-grow') flexGrow: number = 10;

    @Input() flex: number = 0;
    ngOnInit() {
        if (this.flex) {
            this.flexBasis = this.flex / 10 * 100 + '%';
        } else {
            this.flex = 1;
            this.flexBasis = 'auto';
        }
    }
}

@Directive({
    selector: '[flexBreak]'
})
export class FlexBreakDirective {
    @Input()
    @HostBinding('style.flex-basis')
    flexBasis: string = '100%';
}
