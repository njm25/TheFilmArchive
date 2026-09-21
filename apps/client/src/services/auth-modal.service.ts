import { Injectable, signal } from '@angular/core';

// 'register' covers both halves of signing up: with no token it asks for an
// email address, with one it's the create-account form the emailed link lands on.
export type AuthModalMode = 'login' | 'register' | 'forgotPassword' | 'resetPassword' | null;

@Injectable({
  providedIn: 'root'
})
export class AuthModalService {
    mode = signal<AuthModalMode>(null);
    registerToken = signal<string | null>(null);
    // Packed userId|token blob lifted off the /resetPassword/:token URL.
    resetToken = signal<string | null>(null);

    openLogin() {
        this.registerToken.set(null);
        this.resetToken.set(null);
        this.mode.set('login');
    }

    openRegister() {
        this.registerToken.set(null);
        this.resetToken.set(null);
        this.mode.set('register');
    }

    openForgotPassword() {
        this.registerToken.set(null);
        this.resetToken.set(null);
        this.mode.set('forgotPassword');
    }

    openRegisterWithToken(token: string) {
        this.resetToken.set(null);
        this.registerToken.set(token);
        this.mode.set('register');
    }

    openResetPasswordWithToken(token: string) {
        this.registerToken.set(null);
        this.resetToken.set(token);
        this.mode.set('resetPassword');
    }

    close() {
        this.mode.set(null);
        this.registerToken.set(null);
        this.resetToken.set(null);
    }
}
