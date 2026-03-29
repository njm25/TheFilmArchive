
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../services/user.service';
import { LoginReq } from '../../../types/types';

@Component({
    selector: 'tfa-login',
    imports: [FormsModule],
    templateUrl: './login.component.html',
    styleUrl: './login.component.css'
})
export class LoginComponent {
    userService = inject(UserService);
    req = signal<LoginReq>({
        userNameOrEmail: "",
        password: "",
    });

    submitRequest()
    {
        this.userService.login(this.req());
    }
}
