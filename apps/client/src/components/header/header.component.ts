import { Component, computed, inject } from '@angular/core';
import { LinkComponent } from '../link/link.component';
import { AuthService } from '../../services/auth.service';

@Component({
    selector: 'tfa-header',
    imports: [LinkComponent],
    templateUrl: './header.component.html',
    styleUrl: './header.component.css'
})
export class HeaderComponent {

    auth = inject(AuthService)

    isLoggedIn = computed(() => this.auth.isLoggedIn());

    logout = () => this.auth.logout();

}
