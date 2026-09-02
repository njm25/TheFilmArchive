import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FilmService } from '../../../services/film.service';
import { GetFilmsResItem, GetFilmsReq, GetFilmsRes, OrderByFilmEnum, OrderingTypeEnum } from '../../../types/types';
import { CardListComponent } from '../../../components/card-list/card-list.component';
import { DropdownComponent, DropdownOption } from '../../../components/dropdown/dropdown.component';

interface SortOption {
    label: string;
    orderBy: OrderByFilmEnum;
    orderingType: OrderingTypeEnum;
}

@Component({
    selector: 'tfa-films',
    imports: [CardListComponent, FormsModule, DropdownComponent],
    templateUrl: './films.component.html',
    styleUrl: './films.component.css'
})
export class FilmsComponent {
    filmService = inject(FilmService);

    // index 0 is the default sort applied on first load
    readonly sortOptions: SortOption[] = [
        { label: 'Highest Rated', orderBy: OrderByFilmEnum.Rating, orderingType: OrderingTypeEnum.Descending },
        { label: 'Lowest Rated', orderBy: OrderByFilmEnum.Rating, orderingType: OrderingTypeEnum.Ascending },
        { label: 'Newest First', orderBy: OrderByFilmEnum.YearReleased, orderingType: OrderingTypeEnum.Descending },
        { label: 'Oldest First', orderBy: OrderByFilmEnum.YearReleased, orderingType: OrderingTypeEnum.Ascending },
        { label: 'Title (A-Z)', orderBy: OrderByFilmEnum.Title, orderingType: OrderingTypeEnum.Ascending },
        { label: 'Title (Z-A)', orderBy: OrderByFilmEnum.Title, orderingType: OrderingTypeEnum.Descending }
    ];

    readonly sortDropdownOptions: DropdownOption<number>[] = this.sortOptions.map((o, i) => ({
        label: o.label,
        value: i
    }));

    films = signal<GetFilmsResItem[]>([]);
    totalCount = signal(0);
    searchText = '';
    sortIndex = signal(0);
    pageNumber = signal(1);
    loading = signal(true);
    readonly pageSize = 24;

    hasMore = computed(() => this.films().length < this.totalCount());

    ngOnInit() {
        this.fetchFilms(true);
    }

    onSearch() {
        this.fetchFilms(true);
    }

    onSortChange(index: number) {
        this.sortIndex.set(index);
        this.fetchFilms(true);
    }

    loadMore() {
        if (this.loading() || !this.hasMore())
            return;

        this.fetchFilms(false);
    }

    private fetchFilms(reset: boolean) {
        this.loading.set(true);
        const sort = this.sortOptions[this.sortIndex()];
        const pageNumber = reset ? 1 : this.pageNumber() + 1;

        const req: GetFilmsReq = {
            pageNumber,
            pageSize: this.pageSize,
            searchText: this.searchText,
            orderBy: sort.orderBy,
            orderingType: sort.orderingType
        };

        this.filmService.getFilms(req).subscribe((r: GetFilmsRes) => {
            this.films.set(reset ? r.films : [...this.films(), ...r.films]);
            this.totalCount.set(r.totalCount);
            this.pageNumber.set(pageNumber);
            this.loading.set(false);
        });
    }
}
