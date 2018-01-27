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
    selector: 'menu-input',
    templateUrl: 'input.component.html',
    styleUrls: ['./input.component.css']
})
export class MenuInputComponent {}
