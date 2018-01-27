import {
    Component,
    EventEmitter,
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
    selector: 'menu-header',
    templateUrl: 'header.component.html',
    styleUrls: ['./header.component.css']
})
export class MenuHeaderComponent {}
