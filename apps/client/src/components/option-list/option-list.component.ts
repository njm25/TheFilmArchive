import { Component, input, output } from '@angular/core';

export interface OptionListItem<T = unknown> {
    label: string;
    value: T;
    sublabel?: string;
}

@Component({
    selector: 'tfa-option-list',
    imports: [],
    templateUrl: './option-list.component.html',
    styleUrl: './option-list.component.css'
})
export class OptionListComponent<T = unknown> {
    options = input<OptionListItem<T>[]>([]);
    value = input<T | null>(null);
    emptyLabel = input<string>('No options available.');

    valueChange = output<T>();

    select(option: OptionListItem<T>) {
        this.valueChange.emit(option.value);
    }
}
