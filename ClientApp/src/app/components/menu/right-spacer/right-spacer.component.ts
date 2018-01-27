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
    selector: 'menu-right-spacer',
    templateUrl: 'right-spacer.component.html',
    styleUrls: ['./right-spacer.component.css']
})
export class MenuRightSpacerComponent {}
