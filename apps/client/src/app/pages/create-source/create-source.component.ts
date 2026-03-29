import { Component, inject, signal } from '@angular/core';
import { AddSourceReq, SourceTypeEnum } from '../../../types/types';
import { FilmService } from '../../../services/film.service';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';


@Component({
    selector: 'tfa-create-source',
    imports: [FormsModule],
    templateUrl: './create-source.component.html',
    styleUrl: './create-source.component.css'
})
export class CreateSourceComponent {
    router = inject(Router);
    filmService = inject(FilmService);
    route = inject(ActivatedRoute);
    
    req = signal<AddSourceReq>({
        filmId: 0,
        sourceType: SourceTypeEnum.S3,
        sourceUrl: "",
    });

    sourceTypeOptions = [
        { value: SourceTypeEnum.S3, label: 'S3' },
        { value: SourceTypeEnum.ArchiveOrg, label: 'ArchiveOrg' }
    ];

    submitRequest()
    {
        this.req().filmId = parseInt(this.route.snapshot.paramMap.get("filmId")!);

        this.filmService.addSource(this.req()).subscribe((r: any) => {
            this.router.navigate([`/source/${r}`]);
        });
    }
}
