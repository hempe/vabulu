import {
    Component,
    ViewChild,
    Input,
    AfterViewInit,
    ElementRef,
    Inject,
    OnInit
} from '@angular/core';
import { CustomErrorStateMatcher } from '../../services/custom-error-state-matcher';

@Component({
    selector: '[error]',
    templateUrl: 'error.component.html',
    styleUrls: ['./error.component.css']
})
export class ErrorComponent implements OnInit {
    ngOnInit(): void {}

    private el: ElementRef;
    constructor(@Inject(ElementRef) el: ElementRef) {
        this.el = el;
    }

    @Input() public name: string;

    public errors() {
        if (this.name == 'eMail') {
            console.info('lets check the email');
        }
        if (!CustomErrorStateMatcher.errors) return [];
        if (!this.name) return [];
        return CustomErrorStateMatcher.getErrors(this.name);
    }
}
