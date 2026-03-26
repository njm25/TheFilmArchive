import { Component, computed, inject, input } from '@angular/core';
import { GetFilmsResItem } from '../../types/types';
import { Router } from '@angular/router';

@Component({
  selector: 'tfa-film-card',
  standalone: true,
  imports: [],
  templateUrl: './film-card.component.html',
  styleUrl: './film-card.component.css'
})
export class FilmCardComponent {

    router = inject(Router);

    readonly TMDB_BASE_URL = "https://image.tmdb.org/t/p/w200";

    film = input<GetFilmsResItem>();
    posterSrc = computed(() => `${this.TMDB_BASE_URL}/${this.film()?.posterUrl}`);

    goToFilm = () => this.router.navigate([`/film/${this.film()?.filmId}`]);

}
