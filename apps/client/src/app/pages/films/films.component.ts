import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { FilmService } from '../../../services/film.service';
import { GetFilmsResItem, GetFilmsReq, GetFilmsRes, GetGenreResItem, GetLanguageResItem, OrderByFilmEnum, OrderingTypeEnum } from '../../../types/types';
import { CardListComponent } from '../../../components/card-list/card-list.component';
import { DropdownComponent, DropdownOption } from '../../../components/dropdown/dropdown.component';
import { MultiSelectComponent, MultiSelectOption } from '../../../components/multiselect/multiselect.component';
import { PopoverComponent } from '../../../components/popover/popover.component';

interface SortOption {
    label: string;
    orderBy: OrderByFilmEnum;
    orderingType: OrderingTypeEnum;
}

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

    films = signal<GetFilmsResItem[]>([]);
    totalCount = signal(0);
    searchText = '';
    sortIndex = signal(0);
    minRating = signal<number | null>(null);
    maxRating = signal<number | null>(null);
    minYear = signal<number | null>(null);
    maxYear = signal<number | null>(null);
    minRuntime = signal<number | null>(null);
    maxRuntime = signal<number | null>(null);
    selectedGenreIds = signal<number[]>([]);
    selectedLanguages = signal<string[]>([]);
    genreOptions = signal<MultiSelectOption<number>[]>([]);
    languageOptions = signal<MultiSelectOption<string>[]>([]);
    pageNumber = signal(1);

    // Reset (search/filter) loads and load-more (infinite scroll) loads get
    // separate flags since they render completely different UI - a skeleton
    // grid for the former, a small inline spinner for the latter.
    isResetLoading = signal(true);
    isLoadingMore = signal(false);

    // Flips on the instant a search/filter edit happens (not when the debounced
    // request actually fires) and stays on through the debounce + request, so
    // there's immediate feedback but only one HTTP call once things settle.
    showSkeleton = signal(false);

    readonly pageSize = 24;
    readonly skeletonItems = Array.from({ length: 12 });

    busy = computed(() => this.isResetLoading() || this.isLoadingMore());
    hasMore = computed(() => this.films().length < this.totalCount());
    activeFilterCount = computed(() =>
        (this.minRating() != null || this.maxRating() != null ? 1 : 0) +
        (this.minYear() != null || this.maxYear() != null ? 1 : 0) +
        (this.minRuntime() != null || this.maxRuntime() != null ? 1 : 0) +
        (this.selectedGenreIds().length > 0 ? 1 : 0) +
        (this.selectedLanguages().length > 0 ? 1 : 0)
    );

    private filterDebounce?: ReturnType<typeof setTimeout>;

    ngOnInit() {
        this.fetchFilms(true);

        this.filmService.getGenres().subscribe(r => {
            this.genreOptions.set(r.genres.map((g: GetGenreResItem) => ({ label: g.name, value: g.genreId })));
        });

        this.filmService.getLanguages().subscribe(r => {
            this.languageOptions.set(r.languages.map((l: GetLanguageResItem) => ({ label: l.name, value: l.code })));
        });
    }

    onSearchChange() {
        this.scheduleFetch();
    }

    onSortChange(index: number) {
        this.sortIndex.set(index);
        this.fetchFilms(true);
    }

    onMinRatingChange(event: Event) {
        this.minRating.set(this.parseNumber(event));
        this.scheduleFetch();
    }

    onMaxRatingChange(event: Event) {
        this.maxRating.set(this.parseNumber(event));
        this.scheduleFetch();
    }

    onMinYearChange(event: Event) {
        this.minYear.set(this.parseNumber(event));
        this.scheduleFetch();
    }

    onMaxYearChange(event: Event) {
        this.maxYear.set(this.parseNumber(event));
        this.scheduleFetch();
    }

    onMinRuntimeChange(event: Event) {
        this.minRuntime.set(this.parseNumber(event));
        this.scheduleFetch();
    }

    onMaxRuntimeChange(event: Event) {
        this.maxRuntime.set(this.parseNumber(event));
        this.scheduleFetch();
    }

    onGenresChange(genreIds: number[]) {
        this.selectedGenreIds.set(genreIds);
        this.scheduleFetch();
    }

    onLanguagesChange(languages: string[]) {
        this.selectedLanguages.set(languages);
        this.scheduleFetch();
    }

    clearFilters() {
        this.minRating.set(null);
        this.maxRating.set(null);
        this.minYear.set(null);
        this.maxYear.set(null);
        this.minRuntime.set(null);
        this.maxRuntime.set(null);
        this.selectedGenreIds.set([]);
        this.selectedLanguages.set([]);
        this.scheduleFetch();
    }

    private parseNumber(event: Event): number | null {
        const value = (event.target as HTMLInputElement).value;
        return value === '' ? null : Number(value);
    }

    private scheduleFetch(delay = 300) {
        this.showSkeleton.set(true);
        clearTimeout(this.filterDebounce);
        this.filterDebounce = setTimeout(() => this.fetchFilms(true), delay);
    }

    loadMore() {
        if (this.busy() || !this.hasMore())
            return;

        this.fetchFilms(false);
    }

    private fetchFilms(reset: boolean) {
        const pageNumber = reset ? 1 : this.pageNumber() + 1;

        if (reset) {
            this.isResetLoading.set(true);
            this.showSkeleton.set(true);
        } else {
            this.isLoadingMore.set(true);
        }

        const sort = this.sortOptions[this.sortIndex()];

        const req: GetFilmsReq = {
            pageNumber,
            pageSize: this.pageSize,
            searchText: this.searchText,
            orderBy: sort.orderBy,
            orderingType: sort.orderingType,
            genreIds: this.selectedGenreIds(),
            minRating: this.minRating(),
            maxRating: this.maxRating(),
            minYear: this.minYear(),
            maxYear: this.maxYear(),
            minRuntime: this.minRuntime(),
            maxRuntime: this.maxRuntime(),
            languages: this.selectedLanguages()
        };

        this.filmService.getFilms(req).subscribe((r: GetFilmsRes) => {
            this.films.set(reset ? r.films : [...this.films(), ...r.films]);
            this.totalCount.set(r.totalCount);
            this.pageNumber.set(pageNumber);

            if (reset) {
                this.isResetLoading.set(false);
                this.showSkeleton.set(false);
            } else {
                this.isLoadingMore.set(false);
            }
        });
    }
}
