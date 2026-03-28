import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
	AddFilmReq,
	AddSourceReq,
	GetFilmRes,
	GetFilmsReq,
	GetFilmsRes
} from '../types/types';
import { environment } from '../environments/environment';

@Injectable({
	providedIn: 'root'
})
export class FilmService {
	private http = inject(HttpClient);
	private baseUrl = environment.apiUrl;

	getFilms(req: GetFilmsReq): Observable<GetFilmsRes> {
		return this.http.get<GetFilmsRes>(`${this.baseUrl}/Film`, {
			params: {
				pageSize: req.pageSize,
				pageNumber: req.pageNumber,
				searchText: req.searchText,
				orderBy: req.orderBy,
				orderingType: req.orderingType
			}
		});
	}

	getFilm(id: number): Observable<GetFilmRes> {
		return this.http.get<GetFilmRes>(`${this.baseUrl}/Film/${id}`);
	}

	getFilmSource(sourceId: number): Observable<string> {
		return this.http.get(`${this.baseUrl}/Film/sources/${sourceId}`, {
			responseType: 'text'
		});
	}

	addFilm(req: AddFilmReq): Observable<number> {
		return this.http.post<number>(`${this.baseUrl}/Film/addFilm`, req);
	}

	addSource(req: AddSourceReq): Observable<number> {
		return this.http.post<number>(`${this.baseUrl}/Film/addSource`, req);
	}
}