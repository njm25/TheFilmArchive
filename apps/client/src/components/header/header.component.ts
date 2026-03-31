import { Component, computed, inject } from '@angular/core';
import { LinkComponent } from '../link/link.component';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';

@Component({
    selector: 'tfa-header',
    imports: [LinkComponent],
    templateUrl: './header.component.html',
    styleUrl: './header.component.css'
})
export class HeaderComponent {

    auth = inject(AuthService)
    userService = inject(UserService);

    isLoggedIn = computed(() => this.auth.isLoggedIn());
    isSysAdmin = computed(() => this.userService.isSysAdmin());
    isAdmin = computed(() => this.userService.isAdmin());

    logout = () => this.userService.logout();

}
