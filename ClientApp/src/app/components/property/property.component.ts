import { Colors, ConfigurationService } from '../../services/configuration';
import {
    Component,
    HostListener,
    ViewChild,
    OnInit,
    Inject
} from '@angular/core';
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
import { MatDialogRef, MAT_DIALOG_DATA, MatDialog } from '@angular/material';

export interface CalendarEventData {
    comment: string;
}

@Component({
    selector: 'property',
    templateUrl: 'property.component.html',
    styleUrls: ['./property.component.css']
})
export class PropertyComponent implements OnInit {
    public head: MenuEntry = {
        icon: 'arrow_back',
        name: 'Properties',
        action: () => this.router.navigate(['/'])
    };

    public propertyId: string;
    private property: any;

    public viewDate: Date = new Date();
    public events: CalendarEvent<CalendarEventData>[] = [];
    public refresh: Subject<any> = new Subject();

    public getMonth(): string {
        return this.viewDate.toLocaleString(this.config.language, {
            month: 'long'
        });
    }

    public getYear(): string {
        return this.viewDate.getFullYear().toString();
    }

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private http: Http,
        private config: ConfigurationService,
        private dialog: MatDialog
    ) {}

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

        if (event.start && !event.end) event.end = clone(event.start);

        if (event.start) event.start = new Date(event.start);
        if (event.end) event.end = new Date(event.end);

        return event;
    }

    private getEvents() {
        this.http
            .get(`/api/property/${this.propertyId}/events`)
            .map(x => x.json())
            .subscribe(x => {
                this.events = x.map(x => this.fixEvent(x));
                this.refresh.next();
            });
    }

    public reloadFiles() {
        this.http
            .get(`/api/property/${this.propertyId}/images`)
            .map(x => x.json())
            .subscribe(images => {
                this.images = images;
            });
    }

    public galleryImageClicked(index: number) {
        let dialogRef = this.dialog.open(ImageFullscreenDialog, {
            data: {
                src: this.images[index].url
            }
        });
    }

    ngOnInit(): void {
        this.propertyId = this.route.snapshot.params['propertyId'];

        this.http
            .get(`/api/property/${this.propertyId}`)
            .map(x => x.json())
            .subscribe(x => {
                this.property = x;
                this.getEvents();
                this.reloadFiles();
            });
    }

    conf: GALLERY_CONF = {
        imageOffset: '0px',
        showDeleteControl: false,
        showImageTitle: false,
        showCloseControl: false,
        showExtUrlControl: false,
        inline: true,
        backdropColor: 'white'
    };

    images: GALLERY_IMAGE[] = [];
}

@Component({
    template: '<img [src]="data.src" style="width:100%;height:100%" />'
})
export class ImageFullscreenDialog {
    constructor(
        public dialogRef: MatDialogRef<ImageFullscreenDialog>,
        @Inject(MAT_DIALOG_DATA) public data: any
    ) {}

    onNoClick(): void {
        this.dialogRef.close();
    }
}
