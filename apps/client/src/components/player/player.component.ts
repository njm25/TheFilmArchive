import { Component, ElementRef, computed, inject, input, output, viewChild } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';

export interface PlayerProgress {
    currentTime: number;
    duration: number;
}

@Component({
    selector: 'tfa-player',
    imports: [],
    templateUrl: './player.component.html',
    styleUrl: './player.component.css'
})
export class PlayerComponent {
    private sanitizer = inject(DomSanitizer);

    src = input.required<string>();
    isDirectVideo = input<boolean>(true);
    /** Seconds into the film to resume from, applied once when playback metadata loads. */
    startAtSeconds = input<number>(0);

    play = output<void>();
    timeUpdate = output<PlayerProgress>();
    paused = output<PlayerProgress>();

    videoEl = viewChild<ElementRef<HTMLVideoElement>>('videoEl');

    embedUrl = computed<SafeResourceUrl | null>(() =>
        this.isDirectVideo() ? null : this.sanitizer.bypassSecurityTrustResourceUrl(this.src())
    );

    onLoadedMetadata() {
        const el = this.videoEl()?.nativeElement;
        const resumeAt = this.startAtSeconds();

        if (el && resumeAt > 0 && resumeAt < el.duration) {
            el.currentTime = resumeAt;
        }
    }

    onPlay() {
        this.play.emit();
    }

    onTimeUpdate() {
        const el = this.videoEl()?.nativeElement;
        if (!el || !el.duration) return;

        this.timeUpdate.emit({ currentTime: el.currentTime, duration: el.duration });
    }

    onPause() {
        const el = this.videoEl()?.nativeElement;
        if (!el || !el.duration) return;

        this.paused.emit({ currentTime: el.currentTime, duration: el.duration });
    }
}
