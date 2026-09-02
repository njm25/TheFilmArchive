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
    selector: 'tfa-home',
    imports: [CardListComponent, FormsModule, DropdownComponent],
    templateUrl: './home.component.html',
    styleUrl: './home.component.css'
})
export class HomeComponent {
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
    searchText = '';
    sortIndex = signal(0);
    pageNumber = signal(1);
    loading = signal(true);
    readonly pageSize = 24;

    hasMore = computed(() => this.films().length === this.pageSize);
    hasPrev = computed(() => this.pageNumber() > 1);

    ngOnInit() {
        this.fetchFilms();
    }

    fetchFilms() {
        this.loading.set(true);
        const sort = this.sortOptions[this.sortIndex()];

        const req: GetFilmsReq = {
            pageNumber: this.pageNumber(),
            pageSize: this.pageSize,
            searchText: this.searchText,
            orderBy: sort.orderBy,
            orderingType: sort.orderingType
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

    onSortChange(index: number) {
        this.sortIndex.set(index);
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
