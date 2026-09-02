import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FilmService } from '../../../services/film.service';
import { GetFilmsResItem, GetFilmsReq, GetFilmsRes, GetGenreResItem, OrderByFilmEnum, OrderingTypeEnum } from '../../../types/types';
import { CardListComponent } from '../../../components/card-list/card-list.component';
import { DropdownComponent, DropdownOption } from '../../../components/dropdown/dropdown.component';
import { MultiSelectComponent, MultiSelectOption } from '../../../components/multiselect/multiselect.component';
import { PopoverComponent } from '../../../components/popover/popover.component';

interface SortOption {
    label: string;
    orderBy: OrderByFilmEnum;
    orderingType: OrderingTypeEnum;
}

const RATING_OPTIONS: DropdownOption<number | null>[] = [
    { label: 'Any rating', value: null },
    { label: '9+', value: 9 },
    { label: '8+', value: 8 },
    { label: '7+', value: 7 },
    { label: '6+', value: 6 },
    { label: '5+', value: 5 }
];

@Component({
    selector: 'tfa-films',
    imports: [CardListComponent, FormsModule, DropdownComponent, MultiSelectComponent, PopoverComponent],
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

    readonly ratingOptions = RATING_OPTIONS;

    films = signal<GetFilmsResItem[]>([]);
    totalCount = signal(0);
    searchText = '';
    sortIndex = signal(0);
    minRating = signal<number | null>(null);
    selectedGenreIds = signal<number[]>([]);
    genreOptions = signal<MultiSelectOption<number>[]>([]);
    pageNumber = signal(1);
    loading = signal(true);
    readonly pageSize = 24;

    hasMore = computed(() => this.films().length < this.totalCount());
    activeFilterCount = computed(() =>
        (this.minRating() != null ? 1 : 0) + (this.selectedGenreIds().length > 0 ? 1 : 0)
    );

    private searchDebounce?: ReturnType<typeof setTimeout>;

    ngOnInit() {
        this.fetchFilms(true);

        this.filmService.getGenres().subscribe(r => {
            this.genreOptions.set(r.genres.map((g: GetGenreResItem) => ({ label: g.name, value: g.genreId })));
        });
    }

    onSearchChange() {
        clearTimeout(this.searchDebounce);
        this.searchDebounce = setTimeout(() => this.fetchFilms(true), 300);
    }

    onSortChange(index: number) {
        this.sortIndex.set(index);
        this.fetchFilms(true);
    }

    onMinRatingChange(rating: number | null) {
        this.minRating.set(rating);
        this.fetchFilms(true);
    }

    onGenresChange(genreIds: number[]) {
        this.selectedGenreIds.set(genreIds);
        this.fetchFilms(true);
    }

    clearFilters() {
        this.minRating.set(null);
        this.selectedGenreIds.set([]);
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
            orderingType: sort.orderingType,
            genreIds: this.selectedGenreIds(),
            minRating: this.minRating()
        };

        this.filmService.getFilms(req).subscribe((r: GetFilmsRes) => {
            this.films.set(reset ? r.films : [...this.films(), ...r.films]);
            this.totalCount.set(r.totalCount);
            this.pageNumber.set(pageNumber);
            this.loading.set(false);
        });
    }
}
