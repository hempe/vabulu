import {
    Component,
    EventEmitter,
    HostBinding,
    HostListener,
    Input,
    OnDestroy,
    OnInit,
    Output
} from '@angular/core';
import { array, numberWithSeperator, toSum } from '../../../common/helper';

import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';

@Component({
    selector: 'menu-container',
    templateUrl: 'container.component.html',
    styleUrls: ['./container.component.css']
})
export class MenuContainerComponent {
    private _noTop = false;
    @HostBinding('style.padding-top') paddingTop: string = '24px';
    @Input()
    public set noTop(value: boolean) {
        this._noTop = value == true || <any>value == 'true';
        this.paddingTop = this._noTop ? '0px' : '24px';
    }
    public get noTop(): boolean {
        return this._noTop;
    }
}
