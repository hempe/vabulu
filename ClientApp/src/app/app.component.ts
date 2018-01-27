import {} from 'chartjs-plugin-deferred/sr';
import 'rxjs/Rx';

import * as FileSaver from 'file-saver';

import { ActivatedRoute, Router } from '@angular/router';
import { Component, ElementRef, Renderer, ViewChild } from '@angular/core';
import { Http, RequestOptions, ResponseContentType } from '@angular/http';
import { array, makeid } from './common/helper';

import { ApiService } from './services/api';
import { ConfigurationService } from './services/configuration';
import { TranslateService } from '@ngx-translate/core';
import { PdfRenderService } from './services/pdf-render';

declare var Chart: any;
@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.css']
})
export class AppComponent {
    private iframeUrl = '/.auth/iframe';
    @ViewChild('iframe') public iframe: ElementRef;
    constructor(
        private configuraton: ConfigurationService,
        private http: Http,
        private router: Router,
        private route: ActivatedRoute,
        private api: ApiService,
        private renderer: Renderer,
        private pdfRender: PdfRenderService,
        public configuration: ConfigurationService
    ) {
        Chart.defaults.global.defaultFontFamily = 'roboto';

        this.api.init().then(x => {
            this.refreshIFrame();
            this.loading = false;
        });
    }

    @ViewChild('importFileInput') importFileInput: ElementRef;

    public download(type: string) {
        let options = new RequestOptions({
            responseType: ResponseContentType.Blob
        });

        if (type == 'pdf') {
            this.pdfRender.render('/api/export?format=html');
        } else {
            this.http
                .get(`/api/export?format=${type}`, options)
                .map(x => x.blob())
                .subscribe(blob => {
                    FileSaver.saveAs(blob, `export.${type}`);
                });
        }
    }

    public upload() {
        this.importFileInput.nativeElement.click();
    }

    public importFileInputChanged(fileInput: any) {
        if (fileInput.target.files && fileInput.target.files[0]) {
            var reader = new FileReader();
            reader.onload = (e: any) => {
                this.http
                    .post(`/api/import`, JSON.parse(e.target.result))
                    .subscribe(r => {
                        this.router.navigated = false;
                        this.router.navigate(['./'], {
                            relativeTo: this.route
                        });
                    });
                this.importFileInput.nativeElement.value = null;
            };
            reader.readAsText(fileInput.target.files[0], 'utf-8');
        }
    }

    private refreshIFrame() {
        setInterval(() => {
            if (
                this.configuration.loggedIn &&
                this.iframe &&
                this.iframe.nativeElement
            ) {
                this.iframe.nativeElement.src = `${
                    this.iframeUrl
                }?q=${makeid()}`;
                console.debug('Refresh iframe', this.iframe.nativeElement.src);
            }
        }, 60 * 1000);
    }

    public loading: boolean = true;

    public signOut() {
        this.api.signOut();
    }

    public goHome() {
        this.router.navigate(['/']);
    }

    public gotoProfile() {
        this.router.navigate(['/profile']);
    }
}
