import { hexToRgb, makeid } from '../common/helper';

import { Injectable } from '@angular/core';
import { retry } from 'rxjs/operator/retry';
import { TranslateService } from '@ngx-translate/core';

export const Colors = {
    Indigo: '#5C6BC0',
    Blue: '#42A5F5',
    Light_Blue: '#29B6F6',
    Cyan: '#26c6da',
    Teal: '#26a69a',
    Green: '#66bb6a',
    Light_Green: '#9CCC65',
    Lime: '#D4E157',
    Yellow: '#FFEE58',
    Amber: '#FFCA28',
    Orange: '#FFA726',
    Deep_Orange: '#ff7043',
    Red: '#ef5350',
    Pink: '#EC407A',
    Purple: '#AB47BC',
    Deep_Purple: '#7E57C2',
    Brown: '#8D6E63',
    Grey: '#BDBDBD',
    Blue_Grey: '#78909C'
};

@Injectable()
export class ConfigurationService {
    constructor(private translateService: TranslateService) {}

    public get language(): string {
        let userLang =
            navigator.languages && navigator.languages.length > 0
                ? navigator.languages[0]
                : navigator.language || (<any>navigator).userLanguage;
        var lang = userLang.split('-')[0];
        return lang;
    }

    public loggedIn: boolean;
    public get hasRoles(): boolean {
        return this.roles && this.roles.length > 0;
    }

    public username: string;
    public roles: string[];
}
