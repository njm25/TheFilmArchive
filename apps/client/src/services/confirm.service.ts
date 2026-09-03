import { Injectable, signal } from '@angular/core';

export interface ConfirmOptions {
    title?: string;
    message: string;
    confirmLabel?: string;
    cancelLabel?: string;
    /** Styles the confirm button as a destructive action (e.g. deletes). */
    danger?: boolean;
}

interface ConfirmState {
    title: string;
    message: string;
    confirmLabel: string;
    cancelLabel: string;
    danger: boolean;
}

const DEFAULT_STATE: ConfirmState = {
    title: 'Are you sure?',
    message: '',
    confirmLabel: 'Confirm',
    cancelLabel: 'Cancel',
    danger: false
};

/**
 * Renders through the single <tfa-confirm> instance mounted in AppComponent -
 * call confirm() from anywhere and await the result instead of using the
 * browser's native confirm().
 */
@Injectable({
    providedIn: 'root'
})
export class ConfirmService {
    open = signal(false);
    state = signal<ConfirmState>(DEFAULT_STATE);

    private resolver: ((result: boolean) => void) | null = null;

    confirm(options: ConfirmOptions | string): Promise<boolean> {
        const opts: ConfirmOptions = typeof options === 'string' ? { message: options } : options;

        this.state.set({
            title: opts.title ?? DEFAULT_STATE.title,
            message: opts.message,
            confirmLabel: opts.confirmLabel ?? DEFAULT_STATE.confirmLabel,
            cancelLabel: opts.cancelLabel ?? DEFAULT_STATE.cancelLabel,
            danger: opts.danger ?? DEFAULT_STATE.danger
        });

        // A prior confirm() left dangling (component destroyed mid-dialog)
        // shouldn't silently hang forever - resolve it false before opening.
        this.resolver?.(false);
        this.open.set(true);

        return new Promise<boolean>(resolve => {
            this.resolver = resolve;
        });
    }

    respond(result: boolean) {
        this.open.set(false);
        this.resolver?.(result);
        this.resolver = null;
    }
}
