import { Colors, ConfigurationService } from '../../services/configuration';
import { Component, HostListener } from '@angular/core';
import { array, numberWithSeperator } from '../../common/helper';

import { Color } from 'ng2-charts';
import { Http } from '@angular/http';
import { MenuEntry } from '../view-wrapper/view-wrapper.component';
import { MouseService } from '../../services/mouse';
import { Router } from '@angular/router';
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
export interface CalendarEventData {
    comment: string;
}

@Component({
    selector: 'home',
    templateUrl: 'home.component.html',
    styleUrls: ['./home.component.css']
})
export class HomeComponent {
    public head: MenuEntry = {
        icon: 'home',
        name: 'Home'
    };

    public viewDate: Date = new Date();
    private view: string = 'week';

    private newEvent(): CalendarEvent<CalendarEventData> {
        return this.fixEvent<CalendarEventData>(<any>{});
    }

    public event: CalendarEvent<CalendarEventData>;
    public colors: { name: string; value: EventColor }[];

    events: CalendarEvent<CalendarEventData>[];

    public refresh: Subject<any> = new Subject();

    public eventTimesChanged(change: CalendarEventTimesChangedEvent): void {
        change.event.start = change.newStart;
        change.event.end = change.newEnd;
        /* hier sollte ich speichern */
        this.refresh.next();
    }

    public getMonth(): string {
        return this.viewDate.toLocaleString(this.config.language, {
            month: 'long'
        });
    }

    public getYear(): string {
        return this.viewDate.getFullYear().toString();
    }

    public updateEvent(): void {
        this.refresh.next();
        this.event = this.newEvent();
    }

    public handleEvent(event: CalendarEvent): void {
        this.event = this.fixEvent(event);
    }

    constructor(
        private router: Router,
        private http: Http,
        private config: ConfigurationService
    ) {
        debugger;
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
        this.events = [
            this.fixEvent({
                start: subDays(startOfDay(new Date()), 1),
                end: addDays(new Date(), 1),
                title: 'A 3 day event'
            }),
            this.fixEvent({
                start: startOfDay(new Date()),
                title: 'An event with no end date',
                color: {
                    primary: Colors.Blue,
                    secondary: Colors.Blue
                }
            })
        ];
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

        return event;
    }

    ngOnInit(): void {}
}
