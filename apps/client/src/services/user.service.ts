import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { LoginReq, RegisterReq, RequestAccountReq } from '../types/types';
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

    requestAccount(req: RequestAccountReq) {
        return this.http.post(`${this.baseUrl}/User/requestAccount`, req);
    }

    register(req: RegisterReq) {
        return this.http.post(`${this.baseUrl}/User/register`, req);
    }

    login(req: LoginReq) {
        this.http.post(`${this.baseUrl}/User/login`, req).subscribe((r: any) => {
            this.auth.setToken(r.token);
            this.router.navigate(['/']);
        });
    }

}
