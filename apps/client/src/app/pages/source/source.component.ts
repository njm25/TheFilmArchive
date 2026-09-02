import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { FilmService } from '../../../services/film.service';
import { GetFilmSourceRes, SourceTypeEnum } from '../../../types/types';

@Component({
    selector: 'tfa-source',
    imports: [],
    templateUrl: './source.component.html',
    styleUrl: './source.component.css'
})
export class SourceComponent {
    route = inject(ActivatedRoute);
    filmService = inject(FilmService);
    sanitizer = inject(DomSanitizer);

    readonly SOURCE_BASE_URL = "https://d1wwpf11v1dnfp.cloudfront.net";
    readonly SourceTypeEnum = SourceTypeEnum;

    source = signal<GetFilmSourceRes | null>(null);

    isArchiveOrg = computed(() => this.source()?.type === SourceTypeEnum.ArchiveOrg);

    // S3 sources store a relative object key, so the CDN base has to be prepended.
    videoUrl = computed(() => {
        const s = this.source();
        if (!s || s.type !== SourceTypeEnum.S3) return null;
        return `${this.SOURCE_BASE_URL}/${s.url}`;
    });

    // archive.org's /details/<id> page is an HTML viewer, not a raw video file,
    // so it has to be played via archive.org's own embeddable player instead.
    embedUrl = computed<SafeResourceUrl | null>(() => {
        const s = this.source();
        if (!s || s.type !== SourceTypeEnum.ArchiveOrg) return null;

        const identifier = this.extractArchiveIdentifier(s.url);
        if (!identifier) return null;

        return this.sanitizer.bypassSecurityTrustResourceUrl(`https://archive.org/embed/${identifier}`);
    });

    ngOnInit() {
        const sourceId = parseInt(this.route.snapshot.paramMap.get("id")!);
        this.filmService.getFilmSource(sourceId).subscribe((r) => {
            this.source.set(r);
        });
    }

    private extractArchiveIdentifier(url: string): string | null {
        const match = url.match(/archive\.org\/(?:details|embed)\/([^/?#]+)/);
        return match ? match[1] : null;
    }
}
