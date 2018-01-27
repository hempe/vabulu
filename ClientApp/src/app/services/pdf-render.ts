import { Injectable } from '@angular/core';

@Injectable()
export class PdfRenderService {
    constructor() {}
    public render(url: string) {
        var iframe = document.createElement('iframe');
        iframe.id = 'printing';
        iframe.name = 'printing';
        iframe.src = url;
        iframe.style.display = 'none';
        document.body.appendChild(iframe);
        setTimeout(function() {
            window.frames['printing'].focus();
            window.frames['printing'].print();
        }, 0);
    }
}
