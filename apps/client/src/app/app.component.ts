import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FilmService } from '../services/film-service/film.service';
import { GetFilmRes, GetFilmsReq, GetFilmsRes, GetFilmsResItem } from '../types/types';

@Component({
    selector: 'app-root',
    standalone: true,
    imports: [RouterOutlet],
    templateUrl: './app.component.html',
    styleUrl: './app.component.css'
})
export class AppComponent {
    title = 'client';
    readonly SOURCE_BASE_URL = "https://d1wwpf11v1dnfp.cloudfront.net";

    filmService = inject(FilmService);

    films = signal<GetFilmsResItem[]>([]); 
    firstFilm = signal<GetFilmRes | null>(null);
    firstSourceUrl = signal<string | null>(null);

    ngOnInit()
    {
        let req: GetFilmsReq = {
            pageNumber: 1,
            pageSize: 10,
            searchText: "",
            orderBy: 1,
            orderingType: 0
        }

        this.filmService.getFilms(req).subscribe((r: GetFilmsRes) => {
            if (r.films.length == 0)
                return;
            
            this.films.set(r.films);
            
            const firstFilmId = this.films()[0].filmId;
            this.filmService.getFilm(firstFilmId).subscribe((r: GetFilmRes) => {
                console.log("herheheherhe");
                this.firstFilm.set(r);
                if (r.sources.length == 0)
                    return
                const firstSourceId = this.firstFilm()?.sources[0].sourceId ?? 0;

                this.filmService.getFilmSource(firstSourceId).subscribe((r: string) => {
                    this.firstSourceUrl.set(`${this.SOURCE_BASE_URL}/${r}`);
                    console.log(this.firstSourceUrl());
                });
                
            });
        })
    }

}
