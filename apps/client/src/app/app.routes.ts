import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { FilmComponent } from './pages/film/film.component';
import { SourceComponent } from './pages/source/source.component';
import { RequestAccountComponent } from './pages/request-account/request-account.component';
import { RegisterComponent } from './pages/register/register.component';
import { LoginComponent } from './pages/login/login.component';
import { CreateFilmComponent } from './pages/create-film/create-film.component';
import { CreateSourceComponent } from './pages/create-source/create-source.component';
import { authGuard, sysAdminGuard } from './guards/auth.guard';
import { AccountRequestsComponent } from './pages/account-requests/account-requests.component';
import { UsersComponent } from './pages/users/users.component';

export const routes: Routes = [
    {
        path: "",
        component: HomeComponent
    },
    {
        path: "film/:id",
        component: FilmComponent
    },
    {
        path: "source/:id",
        component: SourceComponent
    },
    {
        path: "requestAccount",
        component: RequestAccountComponent
    },
    {
        path: "register/:token",
        component: RegisterComponent
    },
    {
        path: "login",
        component: LoginComponent
    },
    {
        path: "createFilm",
        component: CreateFilmComponent,
        canActivate: [sysAdminGuard]
    },
    {
        path: "createSource/:filmId",
        component: CreateSourceComponent,
        canActivate: [sysAdminGuard]
    },
    {
        path: "accountRequests",
        component: AccountRequestsComponent,
        canActivate: [sysAdminGuard]
    },
    {
        path: "users",
        component: UsersComponent,
        canActivate: [sysAdminGuard]
    }

];
