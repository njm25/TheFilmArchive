import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FilmService } from '../../../services/film.service';
import { GetFilmsResItem, GetFilmsReq, GetFilmsRes, OrderingTypeEnum } from '../../../types/types';
import { CardListComponent } from '../../../components/card-list/card-list.component';

@Component({
    selector: 'tfa-home',
    imports: [CardListComponent, FormsModule],
    templateUrl: './home.component.html',
    styleUrl: './home.component.css'
})
export class HomeComponent {
    filmService = inject(FilmService);

    films = signal<GetFilmsResItem[]>([]);
    searchText = '';
    orderingType = signal<OrderingTypeEnum>(OrderingTypeEnum.Descending);
    pageNumber = signal(1);
    loading = signal(true);
    readonly pageSize = 24;

    isAscending = computed(() => this.orderingType() === OrderingTypeEnum.Ascending);
    hasMore = computed(() => this.films().length === this.pageSize);
    hasPrev = computed(() => this.pageNumber() > 1);

    ngOnInit() {
        this.fetchFilms();
    }

    fetchFilms() {
        this.loading.set(true);
        const req: GetFilmsReq = {
            pageNumber: this.pageNumber(),
            pageSize: this.pageSize,
            searchText: this.searchText,
            orderBy: 1,
            orderingType: this.orderingType()
        };

        this.filmService.getFilms(req).subscribe((r: GetFilmsRes) => {
            this.films.set(r.films);
            this.loading.set(false);
        });
    }

    onSearch() {
        this.pageNumber.set(1);
        this.fetchFilms();
    }

    toggleOrder() {
        this.orderingType.update(v =>
            v === OrderingTypeEnum.Ascending
                ? OrderingTypeEnum.Descending
                : OrderingTypeEnum.Ascending
        );
        this.pageNumber.set(1);
        this.fetchFilms();
    }

    prevPage() {
        if (this.hasPrev()) {
            this.pageNumber.update(v => v - 1);
            this.fetchFilms();
        }
    }

    nextPage() {
        if (this.hasMore()) {
            this.pageNumber.update(v => v + 1);
            this.fetchFilms();
        }
    }
}
