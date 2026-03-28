import { inject, Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
    router = inject(Router);

    isLoggedIn = signal(false);

    setToken(token: string) {
        localStorage.setItem('jwt', token);
        this.isLoggedIn.set(true);
    }

    getToken() {
        return localStorage.getItem('jwt');
    }

    logout() {
        localStorage.removeItem('jwt');
        this.isLoggedIn.set(false);
        this.router.navigate(['/']);
    }

    checkLogin() {
        const token = this.getToken();
        if (token) {
            this.isLoggedIn.set(true);
        } else {
            this.isLoggedIn.set(false);
        }
    }
}
