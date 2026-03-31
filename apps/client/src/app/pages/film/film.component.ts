import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FilmService } from '../../../services/film.service';
import { GetFilmRes } from '../../../types/types';
import { LinkComponent } from '../../../components/link/link.component';
import { AuthService } from '../../../services/auth.service';
import { UserService } from '../../../services/user.service';

@Component({
    selector: 'tfa-film',
    imports: [LinkComponent],
    templateUrl: './film.component.html',
    styleUrl: './film.component.css'
})
export class FilmComponent {
    route = inject(ActivatedRoute);
    router = inject(Router);
    filmService = inject(FilmService);
    authService = inject(AuthService);
    userService = inject(UserService);

    film = signal<GetFilmRes | null>(null);
    filmId = signal<number>(0);

    isLoggedIn = computed(() => this.authService.isLoggedIn());
    isAdmin = computed(() => this.userService.isAdmin());

    readonly TMDB_BASE_URL = "https://image.tmdb.org/t/p/w780";
    backdropSrc = computed(() => this.film()?.backdropPath ? `${this.TMDB_BASE_URL}${this.film()?.backdropPath}` : null);

    goToSource = (sourceId: number) => this.router.navigate([`/source/${sourceId}`]);
    
    ngOnInit()
    {
        this.filmId.set(parseInt(this.route.snapshot.paramMap.get("id")!));

        this.filmService.getFilm(this.filmId()).subscribe((r) => {
            this.film.set(r);
        });
    }

    refreshMetadata()
    {
        console.log("Refreshing metadata for film id " + this.filmId());

        this.filmService.refreshMetadata(this.filmId()).subscribe(() => {
            this.filmService.getFilm(this.filmId()).subscribe((r) => {
                this.film.set(r);
            });
        });
    }

}
