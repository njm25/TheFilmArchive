import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { UserService } from '../../../services/user.service';
import { ForgotPasswordReq } from '../../../types/types';
import { LinkComponent } from '../../../components/link/link.component';

@Component({
    selector: 'tfa-forgot-password',
    imports: [FormsModule, LinkComponent],
    templateUrl: './forgot-password.component.html',
    styleUrl: './forgot-password.component.css'
})
export class ForgotPasswordComponent {
    userService = inject(UserService);

    req = signal<ForgotPasswordReq>({
        email: ""
    });

    sent = signal(false);
    loading = signal(false);

    // Snapshotted so the confirmation keeps showing the address we sent to.
    submittedEmail = signal("");

    submitRequest() {
        // Every extra submit is another email, so a repeated Enter keypress
        // shouldn't get past the disabled button.
        if (this.loading())
            return;

        this.loading.set(true);

        const email = this.req().email;

        this.userService.forgotPassword(this.req())
            .pipe(finalize(() => this.loading.set(false)))
            .subscribe(() => {
                this.submittedEmail.set(email);
                this.sent.set(true);
            });
    }
}
