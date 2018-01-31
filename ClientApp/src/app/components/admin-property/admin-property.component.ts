import { Colors, ConfigurationService } from '../../services/configuration';
import { Component, HostListener, ViewChild, OnInit } from '@angular/core';
import {
    array,
    numberWithSeperator,
    clone,
    last,
    isFunction
} from '../../common/helper';

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

export interface CalendarEventData {
    comment: string;
}

@Component({
    selector: 'admin-property',
    templateUrl: 'admin-property.component.html',
    styleUrls: ['./admin-property.component.css']
})
export class AdminPropertyComponent implements OnInit {
    @ViewChild('eventForm') public form: NgForm;

    private propertyId: string;
    private property: any;

    public get uploadUrl(): string {
        return `/api/admin/property/${this.propertyId}/images/upload`;
    }

    public head: MenuEntry = {
        icon: 'arrow_back',
        name: 'Properties',
        action: () => this.router.navigate(['../'], { relativeTo: this.route })
    };

    public viewDate: Date = new Date();

    private view: string = 'month';

    private newEvent(): CalendarEvent<CalendarEventData> {
        return this.fixEvent<CalendarEventData>(<any>{});
    }

    private _event: CalendarEvent<CalendarEventData>;
    public get event(): CalendarEvent<CalendarEventData> {
        return this._event;
    }
    public set event(val: CalendarEvent<CalendarEventData>) {
        this._event = this.fixEvent(clone(val));
        this.resetForm(this.form);
    }

    public colors: { name: string; value: EventColor }[];

    events: CalendarEvent<CalendarEventData>[] = [];

    public refresh: Subject<any> = new Subject();

    public eventTimesChanged(change: CalendarEventTimesChangedEvent): void {
        change.event.start = change.newStart;
        change.event.end = change.newEnd;
        this.refresh.next();
        this.saveEvent(change.event);
    }

    public getMonth(): string {
        return this.viewDate.toLocaleString(this.config.language, {
            month: 'long'
        });
    }

    public getYear(): string {
        return this.viewDate.getFullYear().toString();
    }

    public onPropertySubmit(form: NgForm) {
        this.http
            .post(`/api/admin/property`, this.property)
            .map(x => x.json())
            .subscribe(x => {
                this.property = x;
                this.resetForm(form);
            });
    }

    public onSubmit(form: NgForm): void {
        this.fixEvent(this.event);
        this.saveEvent(this.event);
    }

    public handleEvent(event: CalendarEvent, click: MouseEvent): void {
        if (click && isFunction(click.preventDefault)) {
            click.preventDefault();
            click.stopImmediatePropagation();
            click.stopPropagation();
        }
        this.event = event;
    }

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private http: Http,
        private config: ConfigurationService
    ) {
        this.colors = Object.keys(Colors).map(x => {
            return {
                name: x,
                value: <EventColor>{
                    secondary: Colors[x],
                    primary: Colors[x]
                }
            };
        });

        this.event = this.newEvent();
    }

    private fixEvent<T>(event: CalendarEvent<T> | any): CalendarEvent<T> {
        event.resizable = {
            beforeStart: true,
            afterEnd: true
        };
        event.draggable = true;
        if (!event.meta) event.meta = <T>{};
        if (!event.color)
            event.color = {
                primary: Colors.Amber,
                secondary: Colors.Amber
            };

        let col = this.colors.filter(
            x => x.value.primary == event.color.primary
        );
        if (col && col.length > 0) event.color = col[0].value;
        if (event.start && !event.end) event.end = clone(event.start);

        if (event.start) event.start = new Date(event.start);
        if (event.end) event.end = new Date(event.end);

        return event;
    }

    private dayClicked(day: any) {
        let event = this.newEvent();
        event.start = day;
        event.end = day;
        this.event = event;
    }

    private resetForm(form: NgForm) {
        if (form) {
            form.control.markAsUntouched();
            form.control.markAsPristine();
        }
    }

    private getEvents() {
        this.http
            .get(`/api/admin/property/${this.propertyId}/events`)
            .map(x => x.json())
            .subscribe(x => {
                this.events = x.map(x => this.fixEvent(x));
                this.refresh.next();
            });
    }

    private saveEvent(event: CalendarEvent<CalendarEventData>) {
        this.http
            .post(`/api/admin/property/${this.propertyId}/events`, event)
            .map(x => x.json())
            .subscribe(x => {
                this.event = x;
                this.resetForm(this.form);
                this.getEvents();
            });
    }

    public reloadFiles() {
        this.http
            .get(`/api/admin/property/${this.propertyId}/images`)
            .map(x => x.json())
            .subscribe(images => {
                this.images = images;
            });
    }

    ngOnInit(): void {
        this.propertyId = this.route.snapshot.params['propertyId'];

        this.http
            .get(`/api/admin/property/${this.propertyId}`)
            .map(x => x.json())
            .subscribe(x => {
                this.property = x;
                this.getEvents();
                this.reloadFiles();
            });
    }

    @ViewChild(NgxImageGalleryComponent)
    ngxImageGallery: NgxImageGalleryComponent;

    galleryConfiguration: GALLERY_CONF = {
        imageOffset: '0px',
        showDeleteControl: true,
        showImageTitle: false,
        showCloseControl: false,
        showExtUrlControl: false,
        inline: true,
        backdropColor: 'white'
    };

    images: GALLERY_IMAGE[] = [];

    deleteImage(index) {
        console.info('Delete image at index ', index);
        let img = this.images[index];
        try {
            this.http
                .delete(
                    `/api/admin/property/id_here/images/${last(
                        img.url.split('/')
                    )}`
                )
                .subscribe(x => this.reloadFiles());
        } catch (err) {}
        try {
            this.http
                .delete(
                    `/api/admin/property/id_here/images/${last(
                        img.thumbnailUrl.split('/')
                    )}`
                )
                .subscribe(x => this.reloadFiles());
        } catch (err) {}
    }
}
