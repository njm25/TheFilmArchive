import { Component, input } from '@angular/core';
import { GetFilmsResItem } from '../../types/types';
import { FilmCardComponent } from '../film-card/film-card.component';

@Component({
  selector: 'tfa-card-list',
  imports: [FilmCardComponent],
  templateUrl: './card-list.component.html',
  styleUrl: './card-list.component.css',
})
export class CardListComponent {
    films = input<GetFilmsResItem[]>();
}
