import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PersonService } from '../../../services/person.service';
import { GetFilmsResItem, GetPersonRes, GetPersonResFilm } from '../../../types/types';
import { CardListComponent } from '../../../components/card-list/card-list.component';

@Component({
    selector: 'tfa-person',
    imports: [CardListComponent],
    templateUrl: './person.component.html',
    styleUrl: './person.component.css'
})
export class PersonComponent {
    route = inject(ActivatedRoute);
    personService = inject(PersonService);

    readonly PROFILE_BASE_URL = "https://image.tmdb.org/t/p/w300";

    person = signal<GetPersonRes | null>(null);
    personId = signal<number>(0);
    notFound = signal(false);

    profileSrc = computed(() => {
        const profilePath = this.person()?.profilePath;

        return profilePath ? `${this.PROFILE_BASE_URL}${profilePath}` : null;
    });

    directed = computed(() => this.toCards(this.person()?.directed ?? []));
    acted = computed(() => this.toCards(this.person()?.acted ?? []));

    // A person who directed and starred in the same film appears in both lists,
    // so the count is over distinct films rather than credits.
    filmCount = computed(() => new Set(
        [...(this.person()?.directed ?? []), ...(this.person()?.acted ?? [])].map(f => f.filmId)
    ).size);

    // Roles as the archive actually knows them - what this person is credited
    // for on films that are here, rather than what TMDB says they're known for.
    roleSummary = computed(() => {
        const person = this.person();
        if (!person) return '';

        const roles: string[] = [];

        if (person.directed.length) roles.push('Director');
        if (person.acted.length) roles.push('Actor');

        const films = this.filmCount();
        const filmLabel = `${films} ${films === 1 ? 'film' : 'films'}`;

        return [...roles, filmLabel].join(' · ');
    });

    ngOnInit() {
        this.personId.set(parseInt(this.route.snapshot.paramMap.get("id")!));
        this.loadPerson();
    }

    loadPerson() {
        this.personService.getPerson(this.personId()).subscribe({
            next: (r) => this.person.set(r),
            error: () => this.notFound.set(true)
        });
    }

    // The character comes back on the credit, but the card only knows about
    // films - captioning the card is how the role gets onto the poster.
    private toCards(films: GetPersonResFilm[]): GetFilmsResItem[] {
        return films.map(f => ({ ...f, caption: f.character ? `as ${f.character}` : null }));
    }
}
