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
    selector: 'user-list',
    templateUrl: 'user-list.component.html',
    styleUrls: ['user-list.component.css']
})
export class UserListComponent implements OnInit, OnDestroy {
    public columns: DataSourceColumn[] = [
        { key: 'userName', name: 'userName', type: 'text' }
    ];

    public dataSource: DataSourceFactory<any, any>;
    public head: MenuEntry = {};
    private _data: { id: string; userName: string }[] = undefined;
    private data: EventEmitter<
        { id: string; userName: string }[]
    > = new EventEmitter<any>();

    private url = `/api/admin/user`;
    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private config: ConfigurationService,
        private http: Http
    ) {
        this.head = {
            icon: 'home',
            name: 'Home',
            action: () => this.router.navigate(['/'])
        };
    }

    ngOnInit() {
        this.dataSource = ref => new ListDataSource(this.getData(), ref);
    }

    ngOnDestroy(): void {}

    private getData(): Observable<{ id: string; userName: string }[]> {
        this.http
            .get(this.url)
            .map(x => x.json())
            .subscribe(x => {
                this._data = x;
                this.data.emit(this._data);
            });
        return this.data.asObservable();
    }

    public selected(row: { id: string; userName: string }) {
        this.router.navigate(['user', row.id]);
    }
}
