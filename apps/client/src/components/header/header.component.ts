import { Component, computed, inject, signal } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { LinkComponent } from '../link/link.component';
import { DropdownComponent, DropdownOption } from '../dropdown/dropdown.component';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { AuthModalService } from '../../services/auth-modal.service';

@Component({
    selector: 'tfa-header',
    imports: [LinkComponent, DropdownComponent],
    templateUrl: './header.component.html',
    styleUrl: './header.component.css'
})
export class HeaderComponent {
    auth = inject(AuthService);
    userService = inject(UserService);
    authModal = inject(AuthModalService);
    router = inject(Router);

    isLoggedIn = computed(() => this.auth.isLoggedIn());
    isSysAdmin = computed(() => this.userService.isSysAdmin());
    isAdmin = computed(() => this.userService.isAdmin());
    sidenavOpen = signal(false);

    readonly adminMenuOptions: DropdownOption<string>[] = [
        { label: 'Add Film', value: '/createFilm' }
    ];

    readonly sysAdminMenuOptions: DropdownOption<string>[] = [
        { label: 'Users', value: '/users' },
        { label: 'Bulk Sync', value: '/admin/bulkSync' }
    ];

    constructor() {
        this.router.events.pipe(
            filter(e => e instanceof NavigationEnd),
            takeUntilDestroyed()
        ).subscribe(() => this.sidenavOpen.set(false));
    }

    toggleSidenav() {
        this.sidenavOpen.update(v => !v);
    }

    closeSidenav() {
        this.sidenavOpen.set(false);
    }

    logout() {
        this.userService.logout();
        this.sidenavOpen.set(false);
    }

    // Both close the sidenav first: on mobile it's what the button was tapped
    // in, and it sits at the same stacking level as the modal.
    openLogin() {
        this.sidenavOpen.set(false);
        this.authModal.openLogin();
    }

    openRegister() {
        this.sidenavOpen.set(false);
        this.authModal.openRegister();
    }

    onMenuSelect(path: string) {
        this.router.navigate([path]);
    }
}
