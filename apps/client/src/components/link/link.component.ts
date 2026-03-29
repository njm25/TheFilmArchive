import { Component, inject, input } from '@angular/core';
import { Router } from '@angular/router';

@Component({
    selector: 'tfa-link',
    imports: [],
    templateUrl: './link.component.html',
    styleUrl: './link.component.css'
})

export class LinkComponent {
    router = inject(Router);
    
    href = input<string>("/");
    styleClass = input<string>("");

    onClick(event: MouseEvent) {
        const url = this.href();

        if (event.ctrlKey || event.button === 1) {
            event.preventDefault();
            window.open(url, '_blank');
        } else {
            this.router.navigate([url]);
        }
    }
}
