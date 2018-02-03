import { CalendarDateFormatter, DateFormatterParams } from 'angular-calendar';
import { getISOWeek } from 'date-fns';
import { DatePipe } from '@angular/common';
import { ConfigurationService } from './configuration';

export class CustomDateFormatter extends CalendarDateFormatter {
    constructor(private config: ConfigurationService) {
        super();
    }

    /**
     * The month view header week day labels
     */
    public monthViewColumnHeader({
        date,
        locale
    }: DateFormatterParams): string {
        return date.toLocaleString(this.config.language, {
            weekday: 'long'
        });
    }
    /**
     * The month view cell day number
     */
    public monthViewDayNumber({ date, locale }: DateFormatterParams): string {
        return date.toLocaleString(this.config.language, {
            day: '2-digit'
        });
    }
    /**
     * The month view title
     */
    public monthViewTitle({ date, locale }: DateFormatterParams): string {
        return `${date.toLocaleString(this.config.language, {
            year: 'numeric'
        })} ${date.toLocaleString(this.config.language, { month: 'long' })}`;
    }
    /**
     * The week view header week day labels
     */
    public weekViewColumnHeader({ date, locale }: DateFormatterParams): string {
        return date.toLocaleString(this.config.language, {
            weekday: 'long'
        });
    }
    /**
     * The week view sub header day and month labels
     */
    public weekViewColumnSubHeader({
        date,
        locale
    }: DateFormatterParams): string {
        return `${date.toLocaleString(this.config.language, {
            day: 'numeric'
        })} ${date.toLocaleString(this.config.language, { month: 'short' })}`;
    }
}
