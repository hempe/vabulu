import { AppComponent } from './app.component';
import { TableListComponent } from './components/admin/table-list.component';
import { TableContentComponent } from './components/admin/table-content.component';
import { TableEntryComponent } from './components/admin/table-entry.component';
import { UserComponent } from './components/user/user.component';
import { UserListComponent } from './components/user/user-list.component';
import { PropertyComponent } from './components/property/property.component';
import { PropertyListComponent } from './components/property/property-list.component';
import { AdminPropertyComponent } from './components/admin-property/admin-property.component';
import { AdminPropertyListComponent } from './components/admin-property/admin-property-list.component';
import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';

export const AppRoutes: Routes = [
    { path: 'home', component: PropertyListComponent },
    { path: 'p', component: PropertyListComponent },
    { path: 'p/:propertyId', component: PropertyComponent },
    { path: 'user', component: UserListComponent },
    { path: 'user/:userId', component: UserComponent },
    { path: 'property', component: AdminPropertyListComponent },
    { path: 'property/:propertyId', component: AdminPropertyComponent },
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
    { path: '**', component: PropertyListComponent }
];
