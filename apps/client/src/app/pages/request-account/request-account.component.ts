import { Component, inject, signal } from '@angular/core';
import { RequestAccountReq } from '../../../types/types';
import { UserService } from '../../../services/user.service';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'tfa-request-account',
    imports: [FormsModule],
    templateUrl: './request-account.component.html',
    styleUrl: './request-account.component.css'
})
export class RequestAccountComponent {
    userService = inject(UserService);

    req = signal<RequestAccountReq>({
        email: ""
    });

    requestSent = signal(false);

    submitRequest() {
        this.userService.requestAccount(this.req()).subscribe(() => {
            this.requestSent.set(true);
        });
    }
}
