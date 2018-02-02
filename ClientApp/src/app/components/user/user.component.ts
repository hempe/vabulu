import { Colors, ConfigurationService } from '../../services/configuration';
import { Component, HostListener, ViewChild, OnInit } from '@angular/core';
import { array, numberWithSeperator, clone, last } from '../../common/helper';

import { Color } from 'ng2-charts';
import { Http } from '@angular/http';
import { MenuEntry } from '../view-wrapper/view-wrapper.component';
import { MouseService } from '../../services/mouse';
import { Router, ActivatedRoute } from '@angular/router';
import {
    CalendarEvent,
    CalendarEventTimesChangedEvent
} from 'angular-calendar';

import {
    startOfDay,
    endOfDay,
    subDays,
    addDays,
    endOfMonth,
    isSameDay,
    isSameMonth,
    addHours
} from 'date-fns';

import { Subject } from 'rxjs';
import { EventColor } from 'calendar-utils';
import { NgForm } from '@angular/forms';
import { Observable } from 'rxjs/Observable';

import {
    NgxImageGalleryComponent,
    GALLERY_IMAGE,
    GALLERY_CONF
} from 'ngx-image-gallery';
import { isFunction } from 'util';
import { Confirmation } from '../confirmation/confirmation.component';

export interface CalendarEventData {
    comment: string;
}

@Component({
    selector: 'user',
    templateUrl: 'user.component.html',
    styleUrls: ['./user.component.css']
})
export class UserComponent implements OnInit {
    @ViewChild('f') public form: NgForm;

    private userId: string;
    private user: any;
    private roles: string[];
    private rolesChecked: any = {};

    public head: MenuEntry = {
        icon: 'arrow_back',
        name: 'Users',
        action: () => this.router.navigate(['../'], { relativeTo: this.route })
    };

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private http: Http,
        private config: ConfigurationService,
        private confirmation: Confirmation
    ) {}

    ngOnInit(): void {
        this.userId = this.route.snapshot.params['userId'];
        this.http
            .get(`/api/admin/user/roles`)
            .map(x => x.json())
            .subscribe(roles => {
                this.roles = roles;
                this.http
                    .get(`/api/admin/user/${this.userId}`)
                    .map(x => x.json())
                    .subscribe(x => this.setUser(x));
            });
    }

    public hasRole(role: string): boolean {
        return this.user.roles.indexOf(role) >= 0;
    }

    public onSubmit(form: NgForm): void {
        this.prepairForSubmit();
        this.http
            .post(`/api/admin/user`, this.user)
            .map(x => x.json())
            .subscribe(x => {
                this.setUser(x);
                this.resetForm(this.form);
            });
    }

    private delete() {
        this.confirmation.confirm('Confirm.DeleteUser').subscribe(x => {
            if (x) {
                this.http
                    .delete(`/api/admin/user/${this.user.id}`)
                    .subscribe(x => {
                        this.router.navigate(['../'], {
                            relativeTo: this.route
                        });
                    });
            }
        });
    }

    private setUser(user: any) {
        this.user = user;
        this.rolesChecked = {};
        this.roles.forEach(r => {
            this.rolesChecked[r] = this.hasRole(r);
        });
    }

    private prepairForSubmit() {
        this.user.roles = [];
        this.roles.forEach(r => {
            if (this.rolesChecked[r]) this.user.roles.push(r);
        });
    }

    private resetForm(form: NgForm) {
        if (form) {
            form.control.markAsUntouched();
            form.control.markAsPristine();
        }
    }
}
