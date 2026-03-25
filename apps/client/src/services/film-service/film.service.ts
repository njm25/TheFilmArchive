import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface GetFilmReq {
	pageSize: number;
	pageNumber: number;
	searchText: string;
	orderBy: number;
	orderingType: number;
}

export interface GetFilmResItem {
	filmId: number;
	title: string;
	yearReleased: number;
	description: string;
	posterUrl: string;
}

export interface GetFilmRes {
	films: GetFilmResItem[];
}

@Injectable({
	providedIn: 'root'
})
export class FilmService {
	private http = inject(HttpClient);
	private baseUrl = 'https://localhost:7156';

	getFilms(req: GetFilmReq): Observable<GetFilmRes> {
	return this.http.get<GetFilmRes>(`${this.baseUrl}/Film`, {
		params: {
			pageSize: req.pageSize,
			pageNumber: req.pageNumber,
			searchText: req.searchText,
			orderBy: req.orderBy,
			orderingType: req.orderingType
		}
	});
}
}