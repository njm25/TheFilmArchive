import { Component, computed, inject, input, signal } from '@angular/core';
import { GetFilmsResItem } from '../../types/types';
import { Router } from '@angular/router';
import { LinkComponent } from "../link/link.component";

@Component({
    selector: 'tfa-film-card',
    imports: [LinkComponent],
    templateUrl: './film-card.component.html',
    styleUrl: './film-card.component.css'
})
export class FilmCardComponent {

    router = inject(Router);

    readonly TMDB_BASE_URL = "https://image.tmdb.org/t/p/w500";

    film = input<GetFilmsResItem>();
    posterSrc = computed(() => `${this.TMDB_BASE_URL}${this.film()?.posterPath}`);

    filmSrc = computed(() => `film/${this.film()?.filmId}`);

    posterLoaded = signal(false);

    // null when this card isn't in a Continue Watching row, or the film has been
    // opened but never actually played - either way there is no bar to draw.
    watchedPercent = computed(() => {
        const film = this.film();
        const progress = film?.progressSeconds ?? 0;
        const duration = film?.durationSeconds ?? 0;

        if (duration <= 0 || progress <= 0)
            return null;

        return Math.min(100, (progress / duration) * 100);
    });

    watchedPercentLabel = computed(() => Math.round(this.watchedPercent() ?? 0));

}
