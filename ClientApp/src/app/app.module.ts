import 'chartjs-plugin-deferred';

import { ErrorStateMatcher, MatDialog, MatSnackBar } from '@angular/material';
import {
    FlexBreakDirective,
    FlexContainerDirective,
    FlexDirective
} from './directives/flex/flex.directive';
import { Http, HttpModule, RequestOptions, XHRBackend } from '@angular/http';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import {
    MissingTranslationHandler,
    TranslateLoader,
    TranslateModule,
    TranslateService
} from '@ngx-translate/core';
import { Router, RouterModule } from '@angular/router';

import { AceEditorModule } from 'ng2-ace-editor';
import { ApiService } from './services/api';
import { AppComponent } from './app.component';
import { AppRoutes } from './app.router';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { BrowserModule } from '@angular/platform-browser';
import { CalendarModule } from 'angular-calendar';
import { TableListComponent } from './components/admin/table-list.component';
import { TableContentComponent } from './components/admin/table-content.component';
import { TableEntryComponent } from './components/admin/table-entry.component';
import { CdkTableModule } from '@angular/cdk/table';
import { ChartsModule } from 'ng2-charts';
import { ConfigurationService } from './services/configuration';
import { CustomErrorStateMatcher } from './services/custom-error-state-matcher';
import { DataSourceTableComponent } from './components/data-source-table/data-source-table.component';
import { DecimalFormatDirective } from './directives/decimal-format/decimal-format.directive';
import { ErrorComponent } from './components/error/error.component';
import { FieldComponent } from './components/field/field.component';
import { FileUploadModule } from 'ng2-file-upload';
import { FlatFieldComponent } from './components/flat-field/flat-field.component';
import { FormsModule } from '@angular/forms';

import {
    PropertyComponent,
    ImageFullscreenDialog
} from './components/property/property.component';
import { PropertyListComponent } from './components/property/property-list.component';

import { UserComponent } from './components/user/user.component';
import { UserListComponent } from './components/user/user-list.component';

import { AdminPropertyComponent } from './components/admin-property/admin-property.component';
import { AdminPropertyListComponent } from './components/admin-property/admin-property-list.component';
import { KeyboardService } from './services/keyboard';
import { LoginComponent } from './components/login/login.component';
import { MatFileComponent } from './components/matfile/matfile.component';
import { MaterialModule } from './app.material';
import { MenuModule } from './components/menu/menu.module';
import { MouseService } from './services/mouse';
import { NgModule } from '@angular/core';
import { NgxImageGalleryModule } from 'ngx-image-gallery';
import { NumberWithSeperatorPipe } from './common/helper';
import { ReloadOnResizeDirective } from './directives/reload-onresize/reload-onresize.directive';
import { ResizeService } from './services/resize';
import { TranslateHttpLoader } from '@ngx-translate/http-loader';
import { ViewWrapperComponent } from './components/view-wrapper/view-wrapper.component';
import { WarnMissingTranslationHandler } from './services/warn-missing-translation-handler';
import { httpFactory } from './services/http-interceptor';
import { PdfRenderService } from './services/pdf-render';
import { Autosize } from '../../node_modules/angular2-autosize/src/autosize.directive';
import {
    ConfirmationDialog,
    Confirmation
} from './components/confirmation/confirmation.component';
import { CalendarDateFormatter } from 'angular-calendar';
import { CustomDateFormatter } from './services/CustomDateFormatter';

@NgModule({
    declarations: [
        AppComponent,
        ConfirmationDialog,
        ImageFullscreenDialog,
        ViewWrapperComponent,
        FieldComponent,
        FlatFieldComponent,
        LoginComponent,
        UserComponent,
        UserListComponent,
        PropertyComponent,
        PropertyListComponent,
        AdminPropertyComponent,
        AdminPropertyListComponent,
        ReloadOnResizeDirective,
        FlexContainerDirective,
        FlexDirective,
        FlexBreakDirective,
        DecimalFormatDirective,
        ErrorComponent,
        NumberWithSeperatorPipe,
        DataSourceTableComponent,
        TableListComponent,
        TableContentComponent,
        TableEntryComponent,
        MatFileComponent,
        Autosize
    ],
    entryComponents: [
        ConfirmationDialog,
        ImageFullscreenDialog,
        MatFileComponent
    ],
    imports: [
        BrowserModule,
        HttpModule,
        FormsModule,
        BrowserAnimationsModule,
        CalendarModule.forRoot(),
        MaterialModule,
        MenuModule,
        NgxImageGalleryModule,
        FileUploadModule,
        CdkTableModule,
        ChartsModule,
        RouterModule.forRoot(
            AppRoutes,
            { enableTracing: true } // <-- debugging purposes only
        ),
        AceEditorModule,
        /** NGX Translate */
        HttpClientModule,
        TranslateModule.forRoot({
            missingTranslationHandler: {
                provide: MissingTranslationHandler,
                useClass: WarnMissingTranslationHandler
            },
            loader: {
                provide: TranslateLoader,
                useFactory: HttpLoaderFactory,
                deps: [HttpClient]
            }
        })
        /*
        TranslateModule.forRoot({
            loader: {
                provide: TranslateLoader,
                useFactory: HttpLoaderFactory,
                deps: [HttpClient]
            }
        })
        */
    ],
    providers: [
        ConfigurationService,
        MouseService,
        PdfRenderService,
        ResizeService,
        KeyboardService,
        ApiService,
        Confirmation,
        {
            provide: Http,
            useFactory: httpFactory,
            deps: [
                XHRBackend,
                RequestOptions,
                ConfigurationService,
                Router,
                MatSnackBar,
                TranslateService
            ]
        },
        {
            provide: CalendarDateFormatter,
            useClass: CustomDateFormatter,
            deps: [ConfigurationService]
        },
        { provide: ErrorStateMatcher, useClass: CustomErrorStateMatcher }
    ],
    bootstrap: [AppComponent]
})
export class AppModule {}

export function HttpLoaderFactory(http: HttpClient) {
    return new TranslateHttpLoader(http, './api/i18n/', '');
}
