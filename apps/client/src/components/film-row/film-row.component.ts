import { Component, input } from '@angular/core';
import { GetFilmsResItem } from '../../types/types';
import { FilmCardComponent } from '../film-card/film-card.component';

@Component({
    selector: 'tfa-film-row',
    imports: [FilmCardComponent],
    templateUrl: './film-row.component.html',
    styleUrl: './film-row.component.css'
})
export class FilmRowComponent {
    title = input.required<string>();
    films = input<GetFilmsResItem[]>([]);
}
