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
    selector: 'menu-entry',
    templateUrl: 'entry.component.html',
    styleUrls: ['./entry.component.css']
})
export class MenuEntryComponent {
    @Input()
    @HostBinding('style.color')
    public color: string;
}
