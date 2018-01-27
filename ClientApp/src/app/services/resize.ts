import { EventEmitter } from '@angular/core';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';

@Injectable()
export class ResizeService {
    private _resized: EventEmitter<{}> = new EventEmitter();
    public get resized(): Observable<{}> {
        return this._resized.asObservable();
    }
    constructor() {
        window.addEventListener('resize', e => this.onResize(e), false);
    }
    private onResize(e) {
        this._resized.emit();
    }
}
