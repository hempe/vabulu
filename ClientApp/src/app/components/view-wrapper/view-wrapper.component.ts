import { Component, Input, OnInit } from '@angular/core';

import { ConfigurationService } from '../../services/configuration';
import { Router } from '@angular/router';

export interface MenuGroup extends MenuEntry {
    entries: MenuEntry[];
}

export interface MenuEntry {
    name?: string;
    icon?: string;
    action?: () => any;
}

@Component({
    selector: 'view-wrapper',
    templateUrl: 'view-wrapper.component.html',
    styleUrls: ['./view-wrapper.component.css']
})
export class ViewWrapperComponent implements OnInit {
    public get color() {
        return this.config.color;
    }
    constructor(private router: Router, private config: ConfigurationService) {}

    @Input() public button: MenuEntry;

    @Input() public head: MenuEntry;

    @Input() public groups: MenuGroup[];

    ngOnInit() {
        this.groups = [
            {
                entries: [
                    {
                        name: 'Home',
                        icon: 'home',
                        action: () => this.router.navigate(['/home'])
                    }
                ]
            },
            {
                name: 'Admin',
                entries: [
                    {
                        name: 'Admin',
                        icon: 'settings',
                        action: () => this.router.navigate(['admin'])
                    }
                ]
            }
        ];
    }

    public noAction() {}
}
