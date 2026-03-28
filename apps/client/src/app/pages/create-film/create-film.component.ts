import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FilmService } from '../../../services/film.service';
import { AddFilmReq } from '../../../types/types';
import { Router } from '@angular/router';

@Component({
  selector: 'tfa-create-film',
  standalone: true,
  imports: [FormsModule, CommonModule],
  templateUrl: './create-film.component.html',
  styleUrl: './create-film.component.css'
})
export class CreateFilmComponent {
    router = inject(Router);
    filmService = inject(FilmService);

    req = signal<AddFilmReq>({
        tmdbId: "",
    });

    submitRequest()
    {
        this.filmService.addFilm(this.req()).subscribe((r: any) => {
            console.log(r);
            this.router.navigate([`/film/${r}`]);
        });
    }

}
