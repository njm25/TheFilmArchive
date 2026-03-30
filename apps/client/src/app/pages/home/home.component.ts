import { Component, inject, signal } from '@angular/core';
import { FilmService } from '../../../services/film.service';
import { GetFilmsResItem, GetFilmsReq, GetFilmsRes } from '../../../types/types';
import { FilmCardComponent } from '../../../components/film-card/film-card.component';

@Component({
    selector: 'tfa-home',
    imports: [FilmCardComponent],
    templateUrl: './home.component.html',
    styleUrl: './home.component.css'
})
export class HomeComponent {
    filmService = inject(FilmService);

    films = signal<GetFilmsResItem[]>([]); 

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
        })
    }
}
