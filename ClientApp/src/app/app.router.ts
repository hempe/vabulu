import { AppComponent } from './app.component';
import { TableListComponent } from './components/admin/table-list.component';
import { TableContentComponent } from './components/admin/table-content.component';
import { TableEntryComponent } from './components/admin/table-entry.component';
import { HomeComponent } from './components/home/home.component';
import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';

export const AppRoutes: Routes = [
    { path: 'home', component: HomeComponent },
    {
        path: 'admin',
        component: TableListComponent
    },
    {
        path: 'admin/:table',
        component: TableContentComponent
    },
    {
        path: 'admin/:table/:partitionKey/:rowKey',
        component: TableEntryComponent
    },
    { path: '**', component: HomeComponent }
];
