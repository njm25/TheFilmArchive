import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FilmService } from '../../../services/film-service/film.service';
import { GetFilmRes } from '../../../types/types';

@Component({
  selector: 'tfa-film',
  standalone: true,
  imports: [],
  templateUrl: './film.component.html',
  styleUrl: './film.component.css'
})
export class FilmComponent {
    route = inject(ActivatedRoute);
    router = inject(Router);
    filmService = inject(FilmService);

    film = signal<GetFilmRes | null>(null);

    goToSource = (sourceId: number) => this.router.navigate([`/source/${sourceId}`]);
    
    ngOnInit()
    {
        const filmId = parseInt(this.route.snapshot.paramMap.get("id")!);

        this.filmService.getFilm(filmId!).subscribe((r) => {
            this.film.set(r);
        });
    }

}
