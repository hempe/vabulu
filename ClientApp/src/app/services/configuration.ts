import { hexToRgb, makeid } from '../common/helper';

import { Injectable } from '@angular/core';
import { retry } from 'rxjs/operator/retry';
import { TranslateService } from '@ngx-translate/core';

export const Colors = {
    Amber: '#FFCA28',
    Blue: '#42A5F5',
    Blue_Grey: '#78909C',
    Brown: '#8D6E63',
    Cyan: '#26c6da',
    Deep_Orange: '#ff7043',
    Deep_Purple: '#7E57C2',
    Green: '#66bb6a',
    Grey: '#BDBDBD',
    Indigo: '#5C6BC0',
    Light_Blue: '#29B6F6',
    Light_Green: '#9CCC65',
    Lime: '#D4E157',
    Orange: '#FFA726',
    Pink: '#EC407A',
    Purple: '#AB47BC',
    Red: '#ef5350',
    Teal: '#26a69a',
    Yellow: '#FFEE58'
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
    public username: string;
    public roles: string[];
}
