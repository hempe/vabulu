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
    selector: 'menu-right',
    templateUrl: 'right.component.html',
    styleUrls: ['./right.component.css']
})
export class MenuRightComponent {
    private _noTop = true;
    @Input()
    public set noTop(value: boolean) {
        this._noTop = value == true || <any>value == 'true';
    }
    public get noTop(): boolean {
        return this._noTop;
    }
}
