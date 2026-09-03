import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
    selector: 'tfa-link',
    imports: [RouterLink],
    templateUrl: './link.component.html',
    styleUrl: './link.component.css'
})

export class LinkComponent {
    href = input<string>("/");
    styleClass = input<string>("");

    // routerLink resolves a path with no leading slash against the *current*
    // route, where the router.navigate() this replaced resolved it against the
    // root. Normalising here keeps call sites that pass a bare "film/12"
    // pointing where they always did.
    link = computed(() => {
        const href = this.href();

        return href.startsWith("/") ? href : `/${href}`;
    });
}
