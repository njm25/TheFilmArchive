import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  	const auth = inject(AuthService);
	const router = inject(Router);

	// ensure state is initialized from localStorage
	auth.checkLogin();

	if (auth.isLoggedIn()) {
		return true;
	}

	// optionally preserve return url
	return router.createUrlTree(['/login']);
};
