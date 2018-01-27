import {
    ConnectionBackend,
    Headers,
    Http,
    Request,
    RequestOptions,
    RequestOptionsArgs,
    Response,
    XHRBackend
} from '@angular/http';

import { ConfigurationService } from './configuration';
import { CustomErrorStateMatcher } from './custom-error-state-matcher';
import { Injectable } from '@angular/core';
import { MatSnackBar } from '@angular/material';
import { Observable } from 'rxjs/Observable';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { TranslateService } from '@ngx-translate/core';

export function httpFactory(
    xhrBackend: XHRBackend,
    requestOptions: RequestOptions,
    configurationService: ConfigurationService,
    router: Router,
    snackBar: MatSnackBar,
    translateService: TranslateService
): Http {
    return new HttpInterceptor(
        xhrBackend,
        requestOptions,
        configurationService,
        router,
        snackBar,
        translateService
    );
}

@Injectable()
export class HttpInterceptor extends Http {
    //private router: Router
    constructor(
        backend: ConnectionBackend,
        defaultOptions: RequestOptions,
        private configurationService: ConfigurationService,
        private router: Router,
        private snackBar: MatSnackBar,
        private translateService: TranslateService
    ) {
        super(backend, defaultOptions);
    }

    request(
        url: string | Request,
        options?: RequestOptionsArgs
    ): Observable<Response> {
        return this.on401(
            super.request(url, this.getRequestOptionArgs(options))
        );
    }

    get(url: string, options?: RequestOptionsArgs): Observable<Response> {
        return this.on401(super.get(url, this.getRequestOptionArgs(options)));
    }

    post(
        url: string,
        body: string,
        options?: RequestOptionsArgs
    ): Observable<Response> {
        return this.on401(
            super.post(url, body, this.getRequestOptionArgs(options))
        );
    }

    put(
        url: string,
        body: string,
        options?: RequestOptionsArgs
    ): Observable<Response> {
        return this.on401(
            super.put(url, body, this.getRequestOptionArgs(options))
        );
    }

    delete(url: string, options?: RequestOptionsArgs): Observable<Response> {
        return this.on401(
            super.delete(url, this.getRequestOptionArgs(options))
        );
    }

    private on401(obj: Observable<Response>): Observable<Response> {
        let subject = new Subject<Response>();
        obj.subscribe(
            data => {
                CustomErrorStateMatcher.errors = {};
                subject.next(data);
            },
            err => {
                if (err && err.status == 401) {
                    this.configurationService.username = undefined;
                    this.configurationService.loggedIn = undefined;
                    //this.router.navigate(['/login']);
                }
                try {
                    console.error(err.status, err);
                    if (err.status == 0) {
                        this.alert('Server not reachable');
                    }
                    if (err.status >= 500) {
                        this.alert('Unkown error');
                    }
                    CustomErrorStateMatcher.errors = err.json();
                    console.error(err.json());
                } catch (ex) {}
                subject.error(err);
            },
            () => subject.complete()
        );

        return subject.asObservable();
    }

    private alert(msg) {
        let trx = this.translateService.get(msg).subscribe(x => {
            this.snackBar.open(x, undefined, {
                duration: 3000
            });
        });
    }
    private getRequestOptionArgs(
        options?: RequestOptionsArgs
    ): RequestOptionsArgs {
        /*
        if (options == null) {
            options = new RequestOptions();
        }
        if (options.headers == null) {
            options.headers = new Headers();
        }
        let query = (<any>options).query;
        if (query) {
            if (options.search === undefined) {
                let queryParameters = new URLSearchParams();
                queryParameters.set('query', <any>query);
            } else {
                (<URLSearchParams>options.search).set('query', <any>query);
            }
        }

        if (this.configuration.accessToken) {
            options.headers.append(
                'Authorization',
                'Token ' + this.configuration.accessToken
            );
        }*/

        //options.headers.append('Accept', '*/*');

        return options;
    }
}
