import { HttpClient } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { FilmService, GetFilmReq, GetFilmRes, GetFilmResItem } from '../services/film-service/film.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'client';

  filmService = inject(FilmService);

  films = signal<GetFilmResItem[]>([]); 

  ngOnInit()
  {
    let req: GetFilmReq = {
      pageNumber: 1,
      pageSize: 10,
      searchText: "2001",
      orderBy: 1,
      orderingType: 0
    }

    this.filmService.getFilms(req).subscribe((r: GetFilmRes) => {
      this.films.set(r.films);
    })
  }

}
