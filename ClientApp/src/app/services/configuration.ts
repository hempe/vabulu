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
    constructor(private translateService: TranslateService) {
        this.resetColor();
    }
    public get language(): string {
        let userLang = navigator.language || (<any>navigator).userLanguage;
        return userLang.split('-')[0];
    }

    public loggedIn: boolean;
    public username: string;

    private _avatar: string = '/assets/img/avatars/noavatar.png';
    public set avatar(val: string) {
        if (val) {
            val = val.split('?id=')[0];
            this._avatar = `${val}?q=${makeid()}`;
        } else {
            this._avatar = '/assets/img/avatars/noavatar.png';
        }
    }

    public get avatar(): string {
        return this._avatar;
    }

    public get color() {
        return this._color;
    }

    private _color: string;

    private fallback = Colors.Cyan; //'#BAEC8E';
    public setCustomColor(color: string) {
        this.fallback = color;
        this.resetColor();
    }

    public setColor(color?: string) {
        color = Colors[color] || color;
        let addon = 60;
        try {
            this._color = `rgba(${hexToRgb(color)
                .map(x => (x + addon < 255 ? x + addon : 255))
                .join(',')}, 0.6)`;
        } catch (err) {
            return color;
        }
    }

    public resetColor() {
        this.setColor();
    }
}
