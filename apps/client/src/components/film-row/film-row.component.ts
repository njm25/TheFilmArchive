import { Component, ElementRef, input, viewChild } from '@angular/core';
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

    scrollEl = viewChild<ElementRef<HTMLDivElement>>('scrollEl');

    scrollBy(direction: -1 | 1) {
        const el = this.scrollEl()?.nativeElement;
        if (!el) return;

        el.scrollBy({ left: direction * el.clientWidth * 0.9, behavior: 'smooth' });
    }
}
