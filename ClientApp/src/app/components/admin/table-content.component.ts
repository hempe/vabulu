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

interface Table {
    partitionKey: string;
    rowKey: string;
    table: string;
    data: Map<string, any>;
}
@Component({
    selector: 'table-content',
    templateUrl: 'table-content.component.html',
    styleUrls: ['table-content.component.css']
})
export class TableContentComponent implements OnInit, OnDestroy {
    public columns: DataSourceColumn[] = [];

    public dataSource: DataSourceFactory<any, any>;
    public head: MenuEntry = {};
    private _data: Table[] = undefined;
    private data: EventEmitter<Table[]> = new EventEmitter<any>();

    private url = ``;

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
        let table = this.route.snapshot.params['table'];
        let headerUrl = `/api/admin/tables/headers/${table}`;
        this.head.name = table;

        this.http
            .get(headerUrl)
            .map(x => x.json())
            .subscribe((x: Map<string, any>[]) => {
                this.columns = Object.keys(x).map(
                    c =>
                        <DataSourceColumn>{
                            key: c,
                            name: c,
                            type: 'text'
                        }
                );

                console.info(this.columns);
                this.url = `/api/admin/tables/${table}`;
                console.info('and the url is ', this.url);
                this.dataSource = ref =>
                    new ListDataSource(this.getData(), ref);
            });
    }

    ngOnDestroy(): void {}

    private getData(): Observable<Table[]> {
        this.http
            .get(this.url)
            .map(x => x.json())
            .subscribe(x => {
                this._data = x.map(c =>
                    Object.assign(
                        {
                            rowKey: c.rowKey,
                            partitionKey: c.partitionKey,
                            table: c.table
                        },
                        c.data
                    )
                );
                console.info(this._data);
                this.data.emit(this._data);
            });
        return this.data.asObservable();
    }

    public selected(row: any) {
        console.info('content', row);
        this.router.navigate([
            'admin',
            row.table,
            row.partitionKey,
            row.rowKey
        ]);
    }
}
