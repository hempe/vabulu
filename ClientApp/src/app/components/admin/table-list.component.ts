import { ActivatedRoute, Router } from '@angular/router';
import {
    Component,
    ElementRef,
    EventEmitter,
    OnDestroy,
    OnInit,
    ViewChild
} from '@angular/core';
import {
    DataSourceColumn,
    DataSourceFactory,
    ListDataSource
} from '../../services/data-source-wrapper';
import { FormControl, FormGroup } from '@angular/forms';
import { Observable, Subject, Subscription } from 'rxjs';
import { array, clone, guid, toNumber } from '../../common/helper';

import { ConfigurationService } from '../../services/configuration';
import { Http } from '@angular/http';
import { KeyboardService } from '../../services/keyboard';
import { MatPaginator } from '@angular/material';
import { MenuEntry } from '../view-wrapper/view-wrapper.component';

@Component({
    selector: 'table-list',
    templateUrl: 'table-list.component.html',
    styleUrls: ['table-list.component.css']
})
export class TableListComponent implements OnInit, OnDestroy {
    public columns: DataSourceColumn[] = [
        { key: 'name', name: 'name', type: 'text' }
    ];

    public dataSource: DataSourceFactory<any, any>;
    public head: MenuEntry = {};
    private _data: { name: string }[] = undefined;
    private data: EventEmitter<{ name: string }[]> = new EventEmitter<any>();

    private url = `/api/admin/tables`;
    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private config: ConfigurationService,
        private http: Http
    ) {
        this.head = {
            icon: 'arrow_back',
            name: 'Admin',
            action: () =>
                this.router.navigate(['../'], { relativeTo: this.route })
        };
    }

    ngOnInit() {
        this.dataSource = ref => new ListDataSource(this.getData(), ref);
    }

    ngOnDestroy(): void {}

    private getData(): Observable<{ name: string }[]> {
        this.http
            .get(this.url)
            .map(x => x.json())
            .map((x: string[]) => x.map(c => <any>{ name: c }))
            .subscribe(x => {
                this._data = x;
                this.data.emit(this._data);
            });
        return this.data.asObservable();
    }

    public selected(row: { name: string }) {
        this.router.navigate(['admin', row.name]);
    }
}
