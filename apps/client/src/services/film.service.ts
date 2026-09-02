import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
	AddFilmReq,
	AddSourceReq,
	GetFilmRes,
	GetFilmsReq,
	GetFilmsRes,
	GetFilmSourceRes,
	GetGenresRes,
	GetWatchProgressRes,
	WatchProgressReq
} from '../types/types';
import { environment } from '../environments/environment';

@Injectable({
	providedIn: 'root'
})
export class FilmService {
	private http = inject(HttpClient);
	private baseUrl = environment.apiUrl;

	getFilms(req: GetFilmsReq): Observable<GetFilmsRes> {
		const params: Record<string, string | number | number[]> = {
			pageSize: req.pageSize,
			pageNumber: req.pageNumber,
			searchText: req.searchText,
			orderBy: req.orderBy,
			orderingType: req.orderingType
		};

		if (req.genreIds && req.genreIds.length > 0) {
			params['genreIds'] = req.genreIds;
		}

		if (req.minRating != null) {
			params['minRating'] = req.minRating;
		}

		return this.http.get<GetFilmsRes>(`${this.baseUrl}/Film`, { params });
	}

	getGenres(): Observable<GetGenresRes> {
		return this.http.get<GetGenresRes>(`${this.baseUrl}/Film/genres`);
	}

	getFilm(id: number): Observable<GetFilmRes> {
		return this.http.get<GetFilmRes>(`${this.baseUrl}/Film/${id}`);
	}

	getFilmSource(sourceId: number): Observable<GetFilmSourceRes> {
		return this.http.get<GetFilmSourceRes>(`${this.baseUrl}/Film/sources/${sourceId}`);
	}

	addFilm(req: AddFilmReq): Observable<number> {
		return this.http.post<number>(`${this.baseUrl}/Film/addFilm`, req);
	}

	addSource(req: AddSourceReq): Observable<number> {
		return this.http.post<number>(`${this.baseUrl}/Film/addSource`, req);
	}

    refreshMetadata(filmId: number): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/Film/refreshMetadata/${filmId.toString()}`, { });
    }

    deleteFilm(filmId: number): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/Film/${filmId}`);
    }

    deleteSource(sourceId: number): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/Film/sources/${sourceId}`);
    }

    logFilmView(filmId: number): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/Film/${filmId}/logView`, {});
    }

    getPopularFilms(take: number = 12): Observable<GetFilmsRes> {
        return this.http.get<GetFilmsRes>(`${this.baseUrl}/Film/popular`, {
            params: { take }
        });
    }

    getContinueWatching(take: number = 12): Observable<GetFilmsRes> {
        return this.http.get<GetFilmsRes>(`${this.baseUrl}/Film/continueWatching`, {
            params: { take }
        });
    }

    getSuggestedFilms(take: number = 12): Observable<GetFilmsRes> {
        return this.http.get<GetFilmsRes>(`${this.baseUrl}/Film/suggested`, {
            params: { take }
        });
    }

    saveWatchProgress(req: WatchProgressReq): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/Film/watchProgress`, req);
    }

    getWatchProgress(filmId: number): Observable<GetWatchProgressRes> {
        return this.http.get<GetWatchProgressRes>(`${this.baseUrl}/Film/${filmId}/watchProgress`);
    }
}