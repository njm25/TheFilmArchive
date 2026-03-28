import { Component, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { FilmService } from '../../../services/film.service';

@Component({
  selector: 'tfa-source',
  standalone: true,
  imports: [],
  templateUrl: './source.component.html',
  styleUrl: './source.component.css'
})
export class SourceComponent {
    route = inject(ActivatedRoute);
    filmService = inject(FilmService);

    readonly SOURCE_BASE_URL = "https://d1wwpf11v1dnfp.cloudfront.net";

    sourcePath = signal<string | null>(null);
    sourceUrl = computed(() => this.sourcePath() ? `${this.SOURCE_BASE_URL}/${this.sourcePath()}` : null);

    ngOnInit(){
        const sourceId = parseInt(this.route.snapshot.paramMap.get("id")!);
        this.filmService.getFilmSource(sourceId!).subscribe((r) => {
            this.sourcePath.set(r);
        });    
    }

}
