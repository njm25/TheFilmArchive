import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthModalMode, AuthModalService } from '../../../services/auth-modal.service';

// The auth forms live in tfa-auth-modal now, but the emailed links still need
// real URLs to point at (and /login is still in the welcome email), so those
// routes all land here. This lifts the mode off route data and the token off the
// URL, opens the matching modal, and replaces the history entry with the home
// page so the token isn't left sitting in the back button.
@Component({
    selector: 'tfa-auth-route',
    imports: [],
    template: ''
})
export class AuthRouteComponent implements OnInit {
    private route = inject(ActivatedRoute);
    private router = inject(Router);
    private authModal = inject(AuthModalService);

    ngOnInit() {
        const mode = this.route.snapshot.data['authMode'] as Exclude<AuthModalMode, null>;
        const token = this.route.snapshot.paramMap.get('token') || "";

        switch (mode) {
            case 'register':
                // No token means they came from a "Sign up" link rather than an
                // invite, so they get the ask-for-an-email form.
                token ? this.authModal.openRegisterWithToken(token) : this.authModal.openRegister();
                break;
            case 'forgotPassword':
                this.authModal.openForgotPassword();
                break;
            case 'resetPassword':
                this.authModal.openResetPasswordWithToken(token);
                break;
            default:
                this.authModal.openLogin();
                break;
        }

        this.router.navigate(['/'], { replaceUrl: true });
    }
}
