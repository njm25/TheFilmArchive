interface GenericListReq {
	pageSize: number;
	pageNumber: number;
	searchText: string;
	orderingType: OrderingTypeEnum;
}

export enum OrderingTypeEnum {
	Ascending = 0,
	Descending = 1
}

// get users
export interface GetUsersReq extends GenericListReq {
	orderBy: OrderByUserEnum;
}
export enum OrderByUserEnum {
	Id = 1
}
export interface GetUsersRes {
    users: GetUsersResItem[];
}
export interface GetUsersResItem {
    id: string;
    userName: string;
    email: string;
    role: RoleEnum;
}

// get films
export interface GetFilmsReq extends GenericListReq {
	orderBy: OrderByFilmEnum;
}
export enum OrderByFilmEnum {
	YearReleased = 1
}
export interface GetFilmsResItem {
	filmId: number;
	title: string;
	yearReleased: number;
	description: string;
	posterPath: string;
}

export interface GetFilmsRes {
	films: GetFilmsResItem[];
}

// add film
export interface AddFilmReq {
	tmdbId: string;
}

// add source
export enum SourceTypeEnum {
    S3 = 1,
    ArchiveOrg = 2
}

export interface AddSourceReq {
	filmId: number;
	sourceType: SourceTypeEnum;
	sourceUrl: string;
}


// get film
export interface GetFilmResSource {
	sourceId: number;
	type: SourceTypeEnum;
}

export interface GetFilmRes {
	title: string;
	yearReleased: number;
	description: string;
	tagline: string;
	posterPath: string;
	sources: GetFilmResSource[];
	primarySourceTypeId: number;
    backdropPath: string;
    runtime: number;
}

// requst acct
export interface RequestAccountReq {
    email: string;
}

// register
export interface RegisterReq {
    userName: string,
    password: string,
    token: string
}

// login
export interface LoginReq {
    userNameOrEmail: string;
    password: string;
}

// me
export interface MeRes {
    id: number;
    userName: string;
    role: RoleEnum;
}

export enum RoleEnum {
    User = 0,
    Admin = 1,
    SysAdmin = 99
}

export interface GetAccountRequestsRes
{
    accountRequests: GetAccountRequestsResItem[];
}

export interface GetAccountRequestsResItem
{
    email: string;
    token: string;
}
