import { ApiService, Provider } from '../../services/api';

import { Component } from '@angular/core';
import { Http } from '@angular/http';
import { MenuEntry } from '../view-wrapper/view-wrapper.component';
import { OnInit } from '@angular/core/src/metadata/lifecycle_hooks';
import { NgForm } from '@angular/forms';
import { CustomErrorStateMatcher } from '../../services/custom-error-state-matcher';
import { isNullOrWhitespace } from '../../common/helper';
import { Router, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'login',
    templateUrl: 'login.component.html',
    styleUrls: ['./login.component.css']
})
export class LoginComponent implements OnInit {
    constructor(
        private api: ApiService,
        private router: Router,
        private route: ActivatedRoute
    ) {}

    public hasForgot = false;

    public providers: Provider[];
    public login: any = {
        email: '',
        password: '',
        rememberMe: false
    };

    ngOnInit(): void {
        this.api.getProviders().subscribe(x => (this.providers = x));
        if (this.route.snapshot.queryParams['forgotCode']) {
            this.hasForgot = true;
            this.login.code = this.route.snapshot.queryParams['forgotCode'];
            this.login.email = this.route.snapshot.queryParams['email'];
        }
    }

    public head: MenuEntry = {
        icon: 'home',
        name: 'Home'
    };

    public signInWith(provider: string) {
        this.api.signInWith(provider);
    }

    public register(): void {
        this.api.register(this.login.email, this.login.password);
    }
    public reset(): void {
        this.api.reset(this.login.email, this.login.password, this.login.code);
    }

    public forgot(form: NgForm): void {
        if (isNullOrWhitespace(this.login.email)) {
            this.login.email = ' ';
            form.control.markAsTouched();
            form.control.markAsDirty();
            CustomErrorStateMatcher.errors['email'] = ['PleaseSetEmail'];
            return;
        }
        delete CustomErrorStateMatcher.errors['email'];
        form.control.markAsUntouched();
        form.control.markAsPristine();
        this.api.forgot(this.login.email);
        this.login = <any>{};
    }
    public signIn(): void {
        this.api.signIn(
            this.login.email,
            this.login.password,
            this.login.rememberMe
        );
    }
}
