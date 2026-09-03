import { Component, HostListener, inject } from '@angular/core';
import { ConfirmService } from '../../services/confirm.service';

@Component({
    selector: 'tfa-confirm',
    imports: [],
    templateUrl: './confirm.component.html',
    styleUrl: './confirm.component.css'
})
export class ConfirmComponent {
    confirmService = inject(ConfirmService);

    confirm() {
        this.confirmService.respond(true);
    }

    cancel() {
        this.confirmService.respond(false);
    }

    @HostListener('document:keydown.escape')
    onEscape() {
        if (this.confirmService.open()) {
            this.cancel();
        }
    }
}
