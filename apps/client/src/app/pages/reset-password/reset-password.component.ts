import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { UserService } from '../../../services/user.service';
import { LinkComponent } from '../../../components/link/link.component';

@Component({
    selector: 'tfa-reset-password',
    imports: [FormsModule, LinkComponent],
    templateUrl: './reset-password.component.html',
    styleUrl: './reset-password.component.css'
})
export class ResetPasswordComponent {
    route = inject(ActivatedRoute);
    userService = inject(UserService);

    password = signal("");
    confirmPassword = signal("");

    reset = signal(false);
    loading = signal(false);

    // Caught here so a typo doesn't cost a round trip and burn the token; the
    // server still enforces the actual password rules.
    mismatch = computed(() =>
        this.confirmPassword().length > 0 && this.password() !== this.confirmPassword()
    );

    canSubmit = computed(() =>
        this.password().length > 0 && !this.mismatch() && !this.loading()
    );

    submitRequest() {
        if (!this.canSubmit())
            return;

        this.loading.set(true);

        const token = this.route.snapshot.paramMap.get('token') || "";

        this.userService.resetPassword({ token, password: this.password() })
            .pipe(finalize(() => this.loading.set(false)))
            .subscribe(() => {
                this.reset.set(true);
            });
    }
}
