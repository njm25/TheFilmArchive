import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { UserService } from '../../../services/user.service';
import { RegisterReq } from '../../../types/types';

import { FormsModule } from '@angular/forms';
import { LinkComponent } from '../../../components/link/link.component';

@Component({
    selector: 'tfa-register',
    imports: [FormsModule, LinkComponent],
    templateUrl: './register.component.html',
    styleUrl: './register.component.css'
})
export class RegisterComponent {
    route = inject(ActivatedRoute);
    userService = inject(UserService);

    req = signal<RegisterReq>({
        userName: "",
        password: "",
        token: ""
    });

    registered = signal(false);

    submitRequest()
    {
        this.req().token = this.route.snapshot.paramMap.get('token') || "";
        this.userService.register(this.req()).subscribe(() => {
            this.registered.set(true);
        });
    }
}
