import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
	const toastr = inject(ToastrService);

	return next(req).pipe(
		catchError((error) => {
			// Handle based on status
			if (error.status !== 200) {
				let message =
					error?.error?.message ||
					error?.statusText ||
					'An unexpected error occurred';
                
                if (error.status === 403) {
                    message = 'Unauthorized';
                }

				toastr.error(message, `Error ${error.status}`);
			}

			return throwError(() => error);
		})
	);
};