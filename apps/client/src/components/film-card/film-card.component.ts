import { Component, computed, inject, input } from '@angular/core';
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

    readonly TMDB_BASE_URL = "https://image.tmdb.org/t/p/w200";

    film = input<GetFilmsResItem>();
    posterSrc = computed(() => `${this.TMDB_BASE_URL}/${this.film()?.posterPath}`);

    filmSrc = computed(() => `film/${this.film()?.filmId}`);

}
