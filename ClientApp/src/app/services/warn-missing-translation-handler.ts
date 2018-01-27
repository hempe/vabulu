import { Injectable } from '@angular/core';
import {
    TranslateService,
    MissingTranslationHandler,
    MissingTranslationHandlerParams
} from '@ngx-translate/core';

@Injectable()
export class WarnMissingTranslationHandler extends MissingTranslationHandler {
    handle(params: MissingTranslationHandlerParams) {
        return `##${params.key}##`;
    }
}
