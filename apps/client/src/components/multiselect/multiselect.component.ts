import { Component, ElementRef, HostListener, computed, inject, input, output, signal } from '@angular/core';

export interface MultiSelectOption<T = unknown> {
    label: string;
    value: T;
}

@Component({
    selector: 'tfa-multiselect',
    imports: [],
    templateUrl: './multiselect.component.html',
    styleUrl: './multiselect.component.css'
})
export class MultiSelectComponent<T = unknown> {
    private elementRef = inject(ElementRef);

    options = input<MultiSelectOption<T>[]>([]);
    values = input<T[]>([]);
    placeholder = input<string>('Select...');
    ariaLabel = input<string>('');
    size = input<'md' | 'sm'>('md');

    valuesChange = output<T[]>();

    open = signal(false);

    summaryLabel = computed(() => {
        const selected = this.options().filter(o => this.values().includes(o.value));
        return selected.length === 0 ? this.placeholder() : selected.map(o => o.label).join(', ');
    });

    hasValue = computed(() => this.values().length > 0);

    toggle() {
        this.open.update(v => !v);
    }

    isSelected(value: T): boolean {
        return this.values().includes(value);
    }

    toggleValue(value: T) {
        const current = this.values();
        const next = this.isSelected(value)
            ? current.filter(v => v !== value)
            : [...current, value];

        this.valuesChange.emit(next);
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
