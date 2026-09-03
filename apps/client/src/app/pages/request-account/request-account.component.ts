import { Component, inject, signal } from '@angular/core';
import { RequestAccountReq } from '../../../types/types';
import { UserService } from '../../../services/user.service';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

@Component({
    selector: 'tfa-request-account',
    imports: [FormsModule],
    templateUrl: './request-account.component.html',
    styleUrl: './request-account.component.css'
})
export class RequestAccountComponent {
    userService = inject(UserService);

    req = signal<RequestAccountReq>({
        email: ""
    });

    requestSent = signal(false);

    // Snapshotted so the confirmation keeps showing the address we actually
    // sent to, independent of whatever the form field holds afterwards.
    submittedEmail = signal("");

    loading = signal(false);

    submitRequest() {
        // The button is disabled while in flight; this also covers a repeated
        // Enter keypress, which submits the form without going through it.
        // Every extra submit here sends another email, so it matters.
        if (this.loading())
            return;

        this.loading.set(true);

        const email = this.req().email;

        this.userService.requestAccount(this.req())
            .pipe(finalize(() => this.loading.set(false)))
            .subscribe(() => {
                this.submittedEmail.set(email);
                this.requestSent.set(true);
            });
    }
}
