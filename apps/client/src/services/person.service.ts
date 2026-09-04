import { inject, Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GetPersonRes } from '../types/types';
import { environment } from '../environments/environment';

@Injectable({
	providedIn: 'root'
})
export class PersonService {
	private http = inject(HttpClient);
	private baseUrl = environment.apiUrl;

	getPerson(id: number): Observable<GetPersonRes> {
		return this.http.get<GetPersonRes>(`${this.baseUrl}/Person/${id}`);
	}
}
