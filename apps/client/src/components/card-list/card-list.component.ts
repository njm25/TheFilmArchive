import { Component, ElementRef, OnDestroy, effect, input, output, viewChild } from '@angular/core';
import { GetFilmsResItem } from '../../types/types';
import { FilmCardComponent } from '../film-card/film-card.component';

@Component({
  selector: 'tfa-card-list',
  imports: [FilmCardComponent],
  templateUrl: './card-list.component.html',
  styleUrl: './card-list.component.css',
})
export class CardListComponent implements OnDestroy {
    films = input<GetFilmsResItem[]>();

    loadMore = output<void>();

    sentinel = viewChild<ElementRef<HTMLDivElement>>('sentinel');

    private observer?: IntersectionObserver;

    constructor() {
        effect(() => {
            const target = this.sentinel()?.nativeElement;

            if (!target)
                return;

            this.observer?.disconnect();

            this.observer = new IntersectionObserver((entries) => {
                if (entries[0].isIntersecting)
                    this.loadMore.emit();
            });

            this.observer.observe(target);
        });
    }

    ngOnDestroy() {
        this.observer?.disconnect();
    }
}
