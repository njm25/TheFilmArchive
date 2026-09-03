import { Component, ElementRef, HostListener, computed, inject, input, output, signal } from '@angular/core';

export interface DropdownOption<T = unknown> {
    label: string;
    value: T;
}

@Component({
    selector: 'tfa-dropdown',
    imports: [],
    templateUrl: './dropdown.component.html',
    styleUrl: './dropdown.component.css'
})
export class DropdownComponent<T = unknown> {
    private elementRef = inject(ElementRef);

    options = input<DropdownOption<T>[]>([]);
    value = input<T | null>(null);
    placeholder = input<string>('Select...');
    ariaLabel = input<string>('');
    /** 'field' = bordered form-style control (default, for toolbars/forms).
     *  'ghost' = plain text trigger matching nav-link styling (for use in nav bars/menus). */
    variant = input<'field' | 'ghost'>('field');
    /** 'md' = default sizing. 'sm' = compact, for dense toolbars. */
    size = input<'md' | 'sm'>('md');

    valueChange = output<T>();

    open = signal(false);

    selectedOption = computed(() =>
        this.options().find(o => o.value === this.value()) ?? null
    );

    hasValue = computed(() => this.selectedOption() != null);

    toggle() {
        this.open.update(v => !v);
    }

    select(option: DropdownOption<T>) {
        this.valueChange.emit(option.value);
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
