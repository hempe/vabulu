import { Injectable } from '@angular/core';

@Injectable()
export class MouseService {
    private x: number = null;
    private y: number = null;

    constructor() {
        document.addEventListener(
            'mousemove',
            e => this.onMouseUpdate(e),
            false
        );
        document.addEventListener(
            'mouseenter',
            e => this.onMouseUpdate(e),
            false
        );
    }
    private onMouseUpdate(e) {
        this.x = e.pageX;
        this.y = e.pageY;
    }

    public getX() {
        return this.x;
    }

    public getY() {
        return this.y;
    }
}
