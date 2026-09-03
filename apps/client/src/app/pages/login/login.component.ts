
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { UserService } from '../../../services/user.service';
import { LoginReq } from '../../../types/types';
import { finalize } from 'rxjs';

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

    loading = signal(false);

    submitRequest()
    {
        // The button is disabled while in flight; this also covers a repeated
        // Enter keypress, which submits the form without going through it.
        if (this.loading())
            return;

        this.loading.set(true);

        this.userService.login(this.req())
            .pipe(finalize(() => this.loading.set(false)))
            .subscribe();
    }
}
