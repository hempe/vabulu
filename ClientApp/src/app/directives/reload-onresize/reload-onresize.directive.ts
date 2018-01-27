import { Directive, HostBinding, HostListener, Input } from '@angular/core';

@Directive({
    selector: '[reloadOnResize]'
})
export class ReloadOnResizeDirective {
    timer: any;

    @HostBinding('style.display') display = 'block';

    @HostListener('window:resize', ['$event'])
    onResize(event) {
        if (this.timer) return;
        this.display = 'none';
        this.timer = setTimeout(() => {
            this.display = 'block';
            if (this.timer) clearTimeout(this.timer);
            this.timer = undefined;
        }, 1000);
    }
}
