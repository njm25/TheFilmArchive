import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { UserService } from '../../services/user.service';
import { AuthModalService } from '../../services/auth-modal.service';

export const authGuard: CanActivateFn = (route, state) => {
  	const auth = inject(AuthService);
	const router = inject(Router);
	const authModal = inject(AuthModalService);

	// ensure state is initialized from localStorage
	auth.checkLogin();

	if (auth.isLoggedIn()) {
		return true;
	}

	// There's no sign-in page to send them to any more, so they land on the
	// home page with the sign-in form open over it.
	authModal.openLogin();

	return router.createUrlTree(['/']);
};

export const adminGuard: CanActivateFn = (route, state) => {
  	const user = inject(UserService);
	const router = inject(Router);

	if (user.isAdmin()) {
		return true;
	}

	return router.createUrlTree(['/']);
};

export const sysAdminGuard: CanActivateFn = (route, state) => {
  	const user = inject(UserService);
	const router = inject(Router);

	if (user.isSysAdmin()) {
		return true;
	}

	return router.createUrlTree(['/']);
};
