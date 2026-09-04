import { Component, DestroyRef, ElementRef, computed, effect, inject, input, output, signal, viewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { GetCaptionResItem } from '../../types/types';

export interface PlayerProgress {
    currentTime: number;
    duration: number;
}

interface ResolvedCaption {
    label: string;
    src: string;
}

@Component({
    selector: 'tfa-player',
    imports: [],
    templateUrl: './player.component.html',
    styleUrl: './player.component.css'
})
export class PlayerComponent {
    private sanitizer = inject(DomSanitizer);
    private http = inject(HttpClient);

    src = input.required<string>();
    isDirectVideo = input<boolean>(true);
    /** Other playable derivatives of the same source, in priority order - tried
     *  in sequence if a candidate turns out to have no decodable video track. */
    fallbackSources = input<string[]>([]);
    /** Seconds into the film to resume from, applied once when playback metadata loads. */
    startAtSeconds = input<number>(0);
    captions = input<GetCaptionResItem[]>([]);

    play = output<void>();
    timeUpdate = output<PlayerProgress>();
    paused = output<PlayerProgress>();
    /** Fires when playback runs off the end - the throttled timeUpdate can miss
     *  the last stretch, and reaching the end is exactly what marks a film watched. */
    ended = output<PlayerProgress>();

    videoEl = viewChild<ElementRef<HTMLVideoElement>>('videoEl');

    embedUrl = computed<SafeResourceUrl | null>(() =>
        this.isDirectVideo() ? null : this.sanitizer.bypassSecurityTrustResourceUrl(this.src())
    );

    // Some archive.org uploads' highest-resolution derivative has no
    // decodable video track in a given browser despite otherwise looking
    // fine (readyState reaches HAVE_ENOUGH_DATA, duration/audio are fine,
    // just videoWidth/videoHeight stay 0) - when that happens, silently
    // advance through the rest of fallbackSources instead of leaving the
    // viewer looking at a black box.
    private candidateIndex = signal(0);
    candidates = computed(() => [this.src(), ...this.fallbackSources()]);
    activeSrc = computed(() => this.candidates()[Math.min(this.candidateIndex(), this.candidates().length - 1)]);

    // The <video> element can't have a `crossorigin` attribute - that's
    // required for a cross-origin <track> to load, but archive.org's file
    // server doesn't send CORS headers, so setting it breaks playback of the
    // video itself. Instead, fetch each caption's text ourselves (a plain
    // HTTP request isn't subject to that restriction) and hand the <track>
    // a same-origin blob: URL, which loads with no CORS involved at all.
    resolvedCaptions = signal<ResolvedCaption[]>([]);
    private blobUrls: string[] = [];

    constructor() {
        effect(() => this.loadCaptionBlobs(this.captions()));
        effect(() => {
            this.src();
            this.candidateIndex.set(0);
        });
        inject(DestroyRef).onDestroy(() => this.revokeBlobUrls());
    }

    private loadCaptionBlobs(captions: GetCaptionResItem[]) {
        this.revokeBlobUrls();

        if (captions.length === 0) {
            this.resolvedCaptions.set([]);
            return;
        }

        for (const caption of captions) {
            this.http.get(caption.url, { responseType: 'text' }).subscribe(vtt => {
                const url = URL.createObjectURL(new Blob([vtt], { type: 'text/vtt' }));
                this.blobUrls.push(url);
                this.resolvedCaptions.update(existing => [...existing, { label: caption.label, src: url }]);
            });
        }
    }

    private revokeBlobUrls() {
        this.blobUrls.forEach(url => URL.revokeObjectURL(url));
        this.blobUrls = [];
        this.resolvedCaptions.set([]);
    }

    onLoadedMetadata() {
        const el = this.videoEl()?.nativeElement;
        if (!el) return;

        if (el.videoWidth === 0 || el.videoHeight === 0) {
            this.tryNextCandidate();
            return;
        }

        const resumeAt = this.startAtSeconds();

        if (resumeAt > 0 && resumeAt < el.duration) {
            el.currentTime = resumeAt;
        }
    }

    onVideoError() {
        this.tryNextCandidate();
    }

    private tryNextCandidate() {
        if (this.candidateIndex() < this.candidates().length - 1) {
            this.candidateIndex.update(i => i + 1);
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

    onEnded() {
        const el = this.videoEl()?.nativeElement;
        if (!el || !el.duration) return;

        this.ended.emit({ currentTime: el.duration, duration: el.duration });
    }

    onPause() {
        const el = this.videoEl()?.nativeElement;
        if (!el || !el.duration) return;

        this.paused.emit({ currentTime: el.currentTime, duration: el.duration });
    }
}
