import { Component, OnDestroy, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FilmService } from '../../../services/film.service';
import { GetFilmRes, GetFilmResSource, GetFilmSourceRes, SourceTypeEnum } from '../../../types/types';
import { LinkComponent } from '../../../components/link/link.component';
import { OptionListComponent, OptionListItem } from '../../../components/option-list/option-list.component';
import { PlayerComponent, PlayerProgress } from '../../../components/player/player.component';
import { AuthService } from '../../../services/auth.service';
import { UserService } from '../../../services/user.service';

@Component({
    selector: 'tfa-film',
    imports: [LinkComponent, OptionListComponent, PlayerComponent],
    templateUrl: './film.component.html',
    styleUrl: './film.component.css'
})
export class FilmComponent implements OnDestroy {
    route = inject(ActivatedRoute);
    router = inject(Router);
    filmService = inject(FilmService);
    authService = inject(AuthService);
    userService = inject(UserService);

    film = signal<GetFilmRes | null>(null);
    filmId = signal<number>(0);

    isLoggedIn = computed(() => this.authService.isLoggedIn());
    isAdmin = computed(() => this.userService.isAdmin());
    isSysAdmin = computed(() => this.userService.isSysAdmin());

    readonly BACKDROP_BASE_URL = "https://image.tmdb.org/t/p/w1280";
    readonly POSTER_BASE_URL = "https://image.tmdb.org/t/p/w780";
    readonly SOURCE_BASE_URL = "https://d1wwpf11v1dnfp.cloudfront.net";
    readonly PROGRESS_INTERVAL_MS = 15000;

    backdropSrc = computed(() => this.film()?.backdropPath ? `${this.BACKDROP_BASE_URL}${this.film()?.backdropPath}` : null);
    posterSrc = computed(() => this.film()?.posterPath ? `${this.POSTER_BASE_URL}${this.film()?.posterPath}` : null);
    topCastNames = computed(() => this.film()?.cast?.slice(0, 6).map(c => c.name).join(', ') ?? '');

    sourceOptions = computed<OptionListItem<number>[]>(() =>
        (this.film()?.sources ?? []).map((s, i) => {
            const typeLabel = s.type === SourceTypeEnum.ArchiveOrg ? 'Archive.org' : 'S3';
            const qualityLabel = s.qualityHeight ? ` · ${s.qualityHeight}p` : '';

            return {
                label: `Source ${i + 1}`,
                value: s.sourceId,
                sublabel: `${typeLabel}${qualityLabel}`
            };
        })
    );

    selectedSourceId = signal<number | null>(null);
    activeSource = signal<GetFilmSourceRes | null>(null);
    resumeAtSeconds = signal(0);

    // S3 sources store a relative object key, so the CDN base has to be prepended.
    // The API resolves archive.org sources to a direct file URL already, so those
    // are used as-is.
    playerSrc = computed(() => {
        const s = this.activeSource();
        if (!s) return null;
        return s.type === SourceTypeEnum.S3 ? `${this.SOURCE_BASE_URL}/${s.url}` : s.url;
    });

    private viewLogged = false;
    private lastProgressSentAt = 0;
    private lastKnownProgress: PlayerProgress | null = null;

    ngOnInit() {
        this.filmId.set(parseInt(this.route.snapshot.paramMap.get("id")!));
        this.loadFilm();
    }

    loadFilm() {
        this.filmService.getFilm(this.filmId()).subscribe((r) => {
            this.film.set(r);

            const defaultSourceId = this.pickDefaultSourceId(r.sources, r.primarySourceTypeId);

            if (defaultSourceId !== undefined) {
                this.selectSource(defaultSourceId);
            } else {
                this.selectedSourceId.set(null);
                this.activeSource.set(null);
            }
        });

        if (this.authService.isLoggedIn()) {
            this.filmService.getWatchProgress(this.filmId()).subscribe((r) => {
                this.resumeAtSeconds.set(r.progressSeconds);
            });
        }
    }

    // Prefers the highest known-quality source; falls back to the film's
    // primary source flag, then just the first source, when quality is
    // unknown for everything (e.g. S3 sources added without a manual tag).
    private pickDefaultSourceId(sources: GetFilmResSource[], primarySourceId?: number): number | undefined {
        const bestByQuality = [...sources]
            .filter(s => s.qualityHeight != null)
            .sort((a, b) => (b.qualityHeight ?? 0) - (a.qualityHeight ?? 0))[0];

        if (bestByQuality) return bestByQuality.sourceId;

        if (primarySourceId != null && primarySourceId >= 0) {
            const primary = sources.find(s => s.sourceId === primarySourceId);
            if (primary) return primary.sourceId;
        }

        return sources[0]?.sourceId;
    }

    selectSource(sourceId: number) {
        this.selectedSourceId.set(sourceId);

        this.filmService.getFilmSource(sourceId).subscribe((r) => {
            this.activeSource.set(r);
            this.logView();
        });
    }

    onPlayerPlay() {
        this.logView();
    }

    onPlayerTimeUpdate(progress: PlayerProgress) {
        this.lastKnownProgress = progress;

        const now = Date.now();

        if (now - this.lastProgressSentAt < this.PROGRESS_INTERVAL_MS)
            return;

        this.lastProgressSentAt = now;
        this.saveProgress(progress);
    }

    onPlayerPaused(progress: PlayerProgress) {
        this.lastKnownProgress = progress;
        this.saveProgress(progress);
    }

    // Route navigation away from the film page destroys this component
    // (unlike a full page unload), so this is the reliable place to flush
    // whatever progress hasn't been sent by the throttled interval yet.
    ngOnDestroy() {
        if (this.lastKnownProgress) {
            this.saveProgress(this.lastKnownProgress);
        }
    }

    private saveProgress(progress: PlayerProgress) {
        if (!this.authService.isLoggedIn())
            return;

        const sourceId = this.selectedSourceId();
        if (sourceId == null)
            return;

        this.filmService.saveWatchProgress({
            filmId: this.filmId(),
            sourceId,
            progressSeconds: Math.floor(progress.currentTime),
            durationSeconds: Math.floor(progress.duration)
        }).subscribe();
    }

    private logView() {
        if (this.viewLogged)
            return;

        this.viewLogged = true;
        this.filmService.logFilmView(this.filmId()).subscribe();
    }

    refreshMetadata() {
        this.filmService.refreshMetadata(this.filmId()).subscribe(() => {
            this.loadFilm();
        });
    }

    deleteFilm() {
        const title = this.film()?.title ?? 'this film';

        if (!confirm(`Delete "${title}"? This will permanently remove it and all of its sources.`)) return;

        this.filmService.deleteFilm(this.filmId()).subscribe(() => {
            this.router.navigate(['/']);
        });
    }

    deleteSelectedSource() {
        const sourceId = this.selectedSourceId();
        if (sourceId == null) return;

        if (!confirm('Delete this source? This cannot be undone.')) return;

        this.filmService.deleteSource(sourceId).subscribe(() => {
            const remaining = (this.film()?.sources ?? []).filter(s => s.sourceId !== sourceId);
            this.film.update(f => f ? { ...f, sources: remaining } : f);

            const nextSourceId = this.pickDefaultSourceId(remaining);

            if (nextSourceId !== undefined) {
                this.selectSource(nextSourceId);
            } else {
                this.selectedSourceId.set(null);
                this.activeSource.set(null);
            }
        });
    }
}
