import { Component, ElementRef, HostListener, inject, input, signal } from '@angular/core';

@Component({
    selector: 'tfa-popover',
    imports: [],
    templateUrl: './popover.component.html',
    styleUrl: './popover.component.css'
})
export class PopoverComponent {
    private elementRef = inject(ElementRef);

    /** Which edge of the trigger the panel hangs from. */
    align = input<'left' | 'right'>('left');

    open = signal(false);

    toggle() {
        this.open.update(v => !v);
    }

    close() {
        this.open.set(false);
    }

    @HostListener('document:click', ['$event'])
    onDocumentClick(event: MouseEvent) {
        if (!this.elementRef.nativeElement.contains(event.target)) {
            this.open.set(false);
        }
    }

    @HostListener('document:keydown.escape')
    onEscape() {
        this.open.set(false);
    }
}
