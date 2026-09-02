import { Component, inject, signal } from '@angular/core';
import { GetUsersReq, GetUsersRes, GetUsersResItem, RoleEnum } from '../../../types/types';
import { UserService } from '../../../services/user.service';
import { DropdownComponent, DropdownOption } from '../../../components/dropdown/dropdown.component';

@Component({
	selector: 'tfa-users',
	imports: [DropdownComponent],
	templateUrl: './users.component.html',
	styleUrl: './users.component.css',
})
export class UsersComponent {
	userService = inject(UserService);

	users = signal<GetUsersResItem[]>([]);

	readonly roles: DropdownOption<RoleEnum>[] = [
		{ label: 'User', value: RoleEnum.User },
		{ label: 'Admin', value: RoleEnum.Admin },
		{ label: 'System Admin', value: RoleEnum.SysAdmin },
	];

	ngOnInit() {
		let req: GetUsersReq = {
			pageNumber: 1,
			pageSize: 10,
			searchText: '',
			orderBy: 1,
			orderingType: 0
		};

		this.userService.getUsers(req).subscribe((r: GetUsersRes) => {
			this.users.set(r.users);
		});
	}

	setRole(userId: string, role: RoleEnum) {
		this.userService.setRole(userId, role).subscribe(() => {});
	}

	onRoleChange(userId: string, role: RoleEnum) {
		this.setRole(userId, role);

		this.users.update(users =>
			users.map(user =>
				user.id === userId
					? { ...user, role: role }
					: user
			)
		);
	}
}