export interface GetFilmsReq {
	pageSize: number;
	pageNumber: number;
	searchText: string;
	orderBy: OrderByEnum;
	orderingType: OrderingTypeEnum;
}

export enum OrderByEnum {
	YearReleased = 1
}

export enum OrderingTypeEnum {
	Ascending = 0,
	Descending = 1
}

export interface GetFilmsResItem {
	filmId: number;
	title: string;
	yearReleased: number;
	description: string;
	posterUrl: string;
}

export interface GetFilmsRes {
	films: GetFilmsResItem[];
}

export interface AddFilmReq {
	tmdbId: string;
}

export enum SourceTypeEnum {
    S3 = 1,
    ArchiveOrg = 2
}

export interface AddSourceReq {
	filmId: number;
	sourceType: SourceTypeEnum;
	sourceUrl: string;
}

export interface GetFilmResSource {
	sourceId: number;
	type: SourceTypeEnum;
}

export interface GetFilmRes {
	title: string;
	yearReleased: number;
	description: string;
	posterUrl: string;
	sources: GetFilmResSource[];
	primarySourceTypeId: number;
}

export interface RequestAccountReq {
    email: string;
}

export interface RegisterReq {
    userName: string,
    password: string,
    token: string
}

export interface LoginReq {
    userNameOrEmail: string;
    password: string;
}
