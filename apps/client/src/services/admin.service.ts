import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BulkSyncStatus } from '../types/types';
import { environment } from '../environments/environment';

@Injectable({
    providedIn: 'root'
})
export class AdminService {
    private http = inject(HttpClient);
    private baseUrl = environment.apiUrl;

    startBulkSync(): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/Admin/bulkSync/start`, {});
    }

    getBulkSyncStatus(): Observable<BulkSyncStatus> {
        return this.http.get<BulkSyncStatus>(`${this.baseUrl}/Admin/bulkSync/status`);
    }
}
