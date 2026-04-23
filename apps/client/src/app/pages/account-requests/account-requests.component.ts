import { Component, inject, signal } from '@angular/core';
import { GetAccountRequestsRes, GetAccountRequestsResItem } from '../../../types/types';
import { UserService } from '../../../services/user.service';

@Component({
  selector: 'tfa-account-requests',
  imports: [],
  templateUrl: './account-requests.component.html',
  styleUrl: './account-requests.component.css',
})
export class AccountRequestsComponent {
    userService = inject(UserService);

    accountRequests = signal<GetAccountRequestsResItem[]>([]);
    baseUrl = window.location.origin;

    ngOnInit() {
        this.userService.getAccountRequests().subscribe((r: GetAccountRequestsRes) => {
            this.accountRequests.set(r.accountRequests);
        });
    }

    copyLink(token: string) {
        const url = `${this.baseUrl}/register/${token}`;
        navigator.clipboard.writeText(url);
    }
}
