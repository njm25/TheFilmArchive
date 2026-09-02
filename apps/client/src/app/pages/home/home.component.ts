import { Component, inject, signal } from '@angular/core';
import { FilmService } from '../../../services/film.service';
import { AuthService } from '../../../services/auth.service';
import { GetFilmsResItem, GetFilmsReq, GetFilmsRes, OrderByFilmEnum, OrderingTypeEnum } from '../../../types/types';
import { FilmRowComponent } from '../../../components/film-row/film-row.component';

@Component({
    selector: 'tfa-home',
    imports: [FilmRowComponent],
    templateUrl: './home.component.html',
    styleUrl: './home.component.css'
})
export class HomeComponent {
    filmService = inject(FilmService);
    authService = inject(AuthService);

    recentlyAdded = signal<GetFilmsResItem[]>([]);
    popular = signal<GetFilmsResItem[]>([]);
    continueWatching = signal<GetFilmsResItem[]>([]);
    suggested = signal<GetFilmsResItem[]>([]);

    isLoggedIn = () => this.authService.isLoggedIn();

    ngOnInit() {
        const req: GetFilmsReq = {
            pageNumber: 1,
            pageSize: 18,
            searchText: "",
            orderBy: OrderByFilmEnum.CreatedAt,
            orderingType: OrderingTypeEnum.Descending
        };

        this.filmService.getFilms(req).subscribe((r: GetFilmsRes) => {
            this.recentlyAdded.set(r.films);
        });

        this.filmService.getPopularFilms().subscribe((r: GetFilmsRes) => {
            this.popular.set(r.films);
        });

        if (this.authService.isLoggedIn()) {
            this.filmService.getContinueWatching().subscribe((r: GetFilmsRes) => {
                this.continueWatching.set(r.films);
            });

            this.filmService.getSuggestedFilms().subscribe((r: GetFilmsRes) => {
                this.suggested.set(r.films);
            });
        }
    }
}
