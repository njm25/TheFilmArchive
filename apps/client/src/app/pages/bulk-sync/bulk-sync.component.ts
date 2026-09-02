import { Component, computed, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { AdminService } from '../../../services/admin.service';
import { BulkSyncState, BulkSyncStatus } from '../../../types/types';

@Component({
    selector: 'tfa-bulk-sync',
    imports: [],
    templateUrl: './bulk-sync.component.html',
    styleUrl: './bulk-sync.component.css'
})
export class BulkSyncComponent implements OnInit, OnDestroy {
    adminService = inject(AdminService);

    readonly BulkSyncState = BulkSyncState;

    status = signal<BulkSyncStatus | null>(null);
    starting = signal(false);

    private pollHandle: ReturnType<typeof setInterval> | null = null;

    isRunning = computed(() => this.status()?.state === BulkSyncState.Running);

    progressPercent = computed(() => {
        const s = this.status();
        if (!s || s.totalFilms === 0) return 0;
        return Math.round((s.processedFilms / s.totalFilms) * 100);
    });

    ngOnInit() {
        this.refreshStatus();
    }

    ngOnDestroy() {
        this.stopPolling();
    }

    startBulkSync() {
        const confirmed = confirm(
            'This will import any films from the seed list that are not yet in the database, ' +
            'then refresh TMDB metadata for every film already in the database. This can take a ' +
            'few minutes and makes many calls to TMDB. Continue?'
        );

        if (!confirmed) return;

        this.starting.set(true);

        this.adminService.startBulkSync().subscribe({
            next: () => {
                this.starting.set(false);
                this.refreshStatus();
            },
            error: () => {
                this.starting.set(false);
                this.refreshStatus();
            }
        });
    }

    private refreshStatus() {
        this.adminService.getBulkSyncStatus().subscribe((s) => {
            this.status.set(s);

            if (s.state === BulkSyncState.Running) {
                this.startPolling();
            } else {
                this.stopPolling();
            }
        });
    }

    private startPolling() {
        if (this.pollHandle) return;

        this.pollHandle = setInterval(() => {
            this.adminService.getBulkSyncStatus().subscribe((s) => {
                this.status.set(s);

                if (s.state !== BulkSyncState.Running) {
                    this.stopPolling();
                }
            });
        }, 2000);
    }

    private stopPolling() {
        if (this.pollHandle) {
            clearInterval(this.pollHandle);
            this.pollHandle = null;
        }
    }
}
