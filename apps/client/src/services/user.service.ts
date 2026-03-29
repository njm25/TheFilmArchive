import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { LoginReq, MeRes, RegisterReq, RequestAccountReq, RoleEnum } from '../types/types';
import { AuthService } from './auth.service';
import { Router } from '@angular/router';
import { environment } from '../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class UserService {

    private http = inject(HttpClient);
    private auth = inject(AuthService);
    private router = inject(Router);
    private baseUrl = environment.apiUrl;
    me = signal<MeRes | null>(null);

    isAdmin = () => this.isRoleOrHigher(RoleEnum.Admin);
    isSysAdmin = () => this.isRoleOrHigher(RoleEnum.SysAdmin);

    isRoleOrHigher = (role: RoleEnum) => {
        const user = this.me();
        return user ? user.role >= role : false;
    }

    requestAccount(req: RequestAccountReq) {
        return this.http.post(`${this.baseUrl}/User/requestAccount`, req);
    }

    register(req: RegisterReq) {
        return this.http.post(`${this.baseUrl}/User/register`, req);
    }

    login(req: LoginReq) {
        this.http.post(`${this.baseUrl}/User/login`, req).subscribe((r: any) => {
            this.auth.setToken(r.token);
            this.refreshMe();
            this.router.navigate(['/']);
        });
    }

    getMe()
    {
        return this.http.get<MeRes>(`${this.baseUrl}/User/me`);
    }

    refreshMe() {
        this.auth.checkLogin();        
        if (this.auth.isLoggedIn()) {
            this.getMe().subscribe({
                next: (res) => {
                    this.auth.isLoggedIn.set(true);
                    this.me.set(res);
                },
            });
        }
    }

}
