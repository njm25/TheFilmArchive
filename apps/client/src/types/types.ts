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
	YearReleased = 1,
	Rating = 2,
	Title = 3,
	CreatedAt = 4
}
export interface GetFilmsResItem {
	filmId: number;
	title: string;
	yearReleased: number;
	description: string;
	posterPath: string;
	genres: string[];
	voteAverage: number | null;
}

export interface GetFilmsRes {
	films: GetFilmsResItem[];
	totalCount: number;
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
	qualityHeight?: number | null;
}


// get film
export interface GetFilmResSource {
	sourceId: number;
	type: SourceTypeEnum;
	qualityHeight: number | null;
}

export interface GetFilmSourceRes {
	type: SourceTypeEnum;
	url: string;
}

export interface GetFilmResCastMember {
	name: string;
	character: string | null;
	profilePath: string | null;
}

export enum FilmStatusEnum {
	Rumored = 1,
	Planned = 2,
	InProduction = 3,
	PostProduction = 4,
	Released = 5,
	Canceled = 6
}

export interface GetFilmRes {
	title: string;
	yearReleased: number;
	description: string;
	tagline: string;
	posterPath: string | null;
	sources: GetFilmResSource[];
	primarySourceTypeId: number;
    backdropPath: string | null;
    runtime: number;
    imdbId: string | null;
    homepage: string | null;
    status: FilmStatusEnum | null;
    voteAverage: number | null;
    voteCount: number | null;
    collectionName: string | null;
    genres: string[];
    directors: string[];
    cast: GetFilmResCastMember[];
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

// bulk sync
export enum BulkSyncState {
    Idle = 0,
    Running = 1,
    Completed = 2,
    Failed = 3
}

export interface BulkSyncError {
    title: string;
    reason: string;
}

export interface BulkSyncStatus {
    state: BulkSyncState;
    phase: string;
    totalFilms: number;
    processedFilms: number;
    createdCount: number;
    refreshedCount: number;
    skippedCount: number;
    failedCount: number;
    currentFilmTitle: string | null;
    startedAt: string | null;
    completedAt: string | null;
    errors: BulkSyncError[];
}
