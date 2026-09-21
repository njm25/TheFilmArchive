import { Component, computed, effect, HostListener, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { finalize } from 'rxjs';
import { AuthModalMode, AuthModalService } from '../../services/auth-modal.service';
import { UserService } from '../../services/user.service';
import { ForgotPasswordReq, LoginReq, RegisterReq, RequestAccountReq } from '../../types/types';

const CLOSE_ANIMATION_MS = 180;

@Component({
    selector: 'tfa-auth-modal',
    imports: [FormsModule],
    templateUrl: './auth-modal.component.html',
    styleUrl: './auth-modal.component.css'
})
export class AuthModalComponent {
    authModal = inject(AuthModalService);
    userService = inject(UserService);
    private toastr = inject(ToastrService);

    loginReq = signal<LoginReq>({ userNameOrEmail: "", password: "" });
    requestAccountReq = signal<RequestAccountReq>({ email: "" });
    createAccountReq = signal<Omit<RegisterReq, 'token'>>({ userName: "", password: "" });
    forgotPasswordReq = signal<ForgotPasswordReq>({ email: "" });
    resetPassword = signal("");
    resetConfirmPassword = signal("");

    // One flag for the whole modal: only a single form is on screen at a time,
    // so a signal per form would never be independently true.
    loading = signal(false);

    visible = signal(false);
    closing = signal(false);
    activeMode = signal<Exclude<AuthModalMode, null>>('login');

    // Caught here so a typo doesn't cost a round trip and burn the token; the
    // server still enforces the actual password rules.
    resetMismatch = computed(() =>
        this.resetConfirmPassword().length > 0 && this.resetPassword() !== this.resetConfirmPassword()
    );

    canSubmitReset = computed(() =>
        this.resetPassword().length > 0 && !this.resetMismatch() && !this.loading()
    );

    private isVisible = false;
    private closeTimer?: ReturnType<typeof setTimeout>;

    constructor() {
        effect(() => {
            const mode = this.authModal.mode();

            if (mode) {
                clearTimeout(this.closeTimer);
                this.activeMode.set(mode);
                this.visible.set(true);
                this.closing.set(false);
                // A request that errored while the modal was closing must not
                // leave the next open with a dead, disabled button.
                this.loading.set(false);
                this.isVisible = true;
            } else if (this.isVisible) {
                // Held on screen for the length of the close animation, then
                // actually removed.
                this.closing.set(true);
                this.closeTimer = setTimeout(() => {
                    this.visible.set(false);
                    this.closing.set(false);
                    this.isVisible = false;
                }, CLOSE_ANIMATION_MS);
            }
        });
    }

    @HostListener('document:keydown.escape')
    onEscape() {
        if (this.authModal.mode()) {
            this.close();
        }
    }

    close() {
        this.authModal.close();
    }

    switchTo(mode: 'login' | 'register' | 'forgotPassword') {
        this.loading.set(false);
        this.authModal.registerToken.set(null);
        this.authModal.resetToken.set(null);
        this.authModal.mode.set(mode);
    }

    submitLogin() {
        // The button is disabled while in flight; this also covers a repeated
        // Enter keypress, which submits the form without going through it.
        if (this.loading())
            return;

        this.loading.set(true);

        this.userService.login(this.loginReq())
            .pipe(finalize(() => this.loading.set(false)))
            .subscribe(() => {
                this.loginReq.set({ userNameOrEmail: "", password: "" });
            });
    }

    submitRequestAccount() {
        // Every extra submit here is another email, so it matters.
        if (this.loading())
            return;

        this.loading.set(true);

        this.userService.requestAccount(this.requestAccountReq())
            .pipe(finalize(() => this.loading.set(false)))
            .subscribe(() => {
                // Worded so it reveals nothing either way. The API answers
                // identically whether or not the address is already registered,
                // and this message has to match that or it hands back the
                // enumeration signal the API just stopped giving.
                this.toastr.success("Check your email for a link to finish signing up.", "Verification sent");
                this.requestAccountReq.set({ email: "" });
                this.close();
            });
    }

    submitForgotPassword() {
        if (this.loading())
            return;

        this.loading.set(true);

        this.userService.forgotPassword(this.forgotPasswordReq())
            .pipe(finalize(() => this.loading.set(false)))
            .subscribe(() => {
                // Same reasoning as submitRequestAccount: deliberately says "if".
                this.toastr.success("If that address has an account, a reset link is on its way.", "Check your email");
                this.forgotPasswordReq.set({ email: "" });
                this.close();
            });
    }

    submitCreateAccount() {
        const token = this.authModal.registerToken();

        if (!token || this.loading())
            return;

        this.loading.set(true);

        const req: RegisterReq = { ...this.createAccountReq(), token };

        this.userService.register(req)
            .pipe(finalize(() => this.loading.set(false)))
            .subscribe(() => {
                // Registering doesn't sign them in - the API hands back a
                // message, not a token - so this drops them on the sign-in form
                // with the account they just made.
                this.toastr.success("You can now sign in with your new credentials.", "Account created");
                this.createAccountReq.set({ userName: "", password: "" });
                this.switchTo('login');
            });
    }

    submitResetPassword() {
        const token = this.authModal.resetToken();

        if (!token || !this.canSubmitReset())
            return;

        this.loading.set(true);

        this.userService.resetPassword({ token, password: this.resetPassword() })
            .pipe(finalize(() => this.loading.set(false)))
            .subscribe(() => {
                this.toastr.success("You can now sign in with your new password.", "Password updated");
                this.resetPassword.set("");
                this.resetConfirmPassword.set("");
                this.switchTo('login');
            });
    }
}
